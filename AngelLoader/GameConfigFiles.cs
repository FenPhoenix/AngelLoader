using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class GameConfigFiles
    {
        /// <summary>
        /// Remove our footprints from any config files we may have temporarily stomped on.
        /// If it fails, oh well. It's no worse than before, we just end up with ourselves as the loader,
        /// and the user will get a message about that if they start the game later.
        /// </summary>
        /// <param name="perGameGoFlags">
        /// An array of bools, of length <see cref="SupportedGameCount"/>. Each bool says whether to go ahead and
        /// do the reset work for that game. If false, we skip it.
        /// </param>
        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static void ResetGameConfigTempChanges(bool[]? perGameGoFlags = null)
        {
            AssertR(perGameGoFlags == null || perGameGoFlags.Length == SupportedGameCount,
                nameof(perGameGoFlags) + " length does not match " + nameof(SupportedGameCount));

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                string gameExe = Config.GetGameExe(gameIndex);
                try
                {
                    if ((perGameGoFlags == null || perGameGoFlags[i]) &&
                        // Only try to un-stomp the configs for the game if the game was actually specified
                        !gameExe.IsWhiteSpace())
                    {
                        // For Dark, we need to know if the game exe itself actually exists.
                        if (GameIsDark(gameIndex) && File.Exists(gameExe))
                        {
                            SetCamCfgLanguage(Config.GetGamePath(gameIndex), "");
                            SetDarkFMSelector(gameIndex, Config.GetGamePath(gameIndex), resetSelector: true);
                        }
                        else
                        {
                            // For Thief 3, we actually just want to know if SneakyOptions.ini exists. The game
                            // itself existing is not technically a requirement.
                            string soIni = Paths.GetSneakyOptionsIni();
                            if (!soIni.IsEmpty() && File.Exists(soIni))
                            {
                                SetT3FMSelector(resetSelector: true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(nameof(ResetGameConfigTempChanges) + ": Exception trying to unset temp config values\r\n" +
                        "GameIndex: " + gameIndex + "\r\n" +
                        "GameExe: " + gameExe,
                        ex);
                }
            }
        }

        // TODO: Pop up actual dialogs here if we fail, because we do NOT want scraps of the wrong language left
        internal static void SetCamCfgLanguage(string gamePath, string lang)
        {
            const string fmLanguage = "fm_language";
            const string fmLanguageForced = "fm_language_forced";

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamCfg, out string camCfg))
            {
                Log(Paths.CamCfg + " not found for " + gamePath, stackTrace: true);
                return;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(camCfg).ToList();
            }
            catch (Exception ex)
            {
                Log("Exception reading " + Paths.CamModIni + " for " + gamePath, ex);
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.StartsWithI(fmLanguage) || lineT.StartsWithI(fmLanguageForced))
                {
                    lines.RemoveAt(i);
                    i--;
                }
            }

            if (!lang.IsEmpty())
            {
                lines.Add(fmLanguage + " " + lang);
                lines.Add(fmLanguageForced + " 1");
            }

            try
            {
                File.WriteAllLines(camCfg, lines);
            }
            catch (Exception ex)
            {
                Log("Exception writing " + Paths.CamModIni + " for " + gamePath, ex);
                return;
            }
        }

        #region Set selectors

        // 2019-10-16: We also now force the loader to start in the config files rather than just on the command
        // line. This is to support Steam launching, because Steam can't take game-specific command line arguments.

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static bool SetDarkFMSelector(GameIndex game, string gamePath, bool resetSelector = false)
        {
            #region Local functions

            static string FindPreviousSelector(List<string> lines, string fmSelectorKey, string stubPath,
            string gamePath)
            {
                // Handle relative paths
                static string GetFullPath(string _gamePath, string path)
                {
                    if (PathIsRelative(path))
                    {
                        try
                        {
                            return RelativeToAbsolute(_gamePath, path);
                        }
                        catch (Exception)
                        {
                            return "";
                        }
                    }

                    return path;
                }

                string TryGetOtherSelectorSpecifier(string line)
                {
                    string selectorFileName;
                    // try-catch cause of Path.Combine() maybe trying to combine invalid-for-path strings
                    // In .NET Core, we could use Path.Join() to avoid throwing
                    try
                    {
                        return line.StartsWithI(fmSelectorKey) && line.Length > fmSelectorKey.Length &&
                               char.IsWhiteSpace(line[fmSelectorKey.Length]) &&
                               (selectorFileName = line.Substring(fmSelectorKey.Length + 1)).EndsWithI(".dll") &&
                               !selectorFileName.PathEqualsI(stubPath) &&
                               !GetFullPath(gamePath, selectorFileName).IsEmpty() &&
                               File.Exists(Path.Combine(gamePath, selectorFileName))
                            ? selectorFileName
                            : "";
                    }
                    catch
                    {
                        return "";
                    }
                }

                var selectorsList = new List<string>();
                var commentedSelectorsList = new List<string>();

                for (int i = 0; i < lines.Count; i++)
                {
                    string lt = lines[i].Trim();

                    string selectorFileName;
                    if (lt.Length > 0 && lt[0] == ';')
                    {
                        do { lt = lt.TrimStart(CA_Semicolon).Trim(); } while (lt.Length > 0 && lt[0] == ';');

                        if (!(selectorFileName = TryGetOtherSelectorSpecifier(lt)).IsEmpty())
                        {
                            if (!commentedSelectorsList.PathContainsI(selectorFileName)) commentedSelectorsList.Add(selectorFileName);
                        }
                    }
                    else if (!(selectorFileName = TryGetOtherSelectorSpecifier(lt)).IsEmpty())
                    {
                        if (!selectorsList.PathContainsI(selectorFileName)) selectorsList.Add(selectorFileName);
                    }
                }

                return
                    selectorsList.Count > 0 ? selectorsList[selectorsList.Count - 1] :
                    commentedSelectorsList.Count > 0 ? commentedSelectorsList[commentedSelectorsList.Count - 1] :
                    Paths.FMSelDll;
            }

            #endregion

            const string fmSelectorKey = "fm_selector";
            const string fmCommentLine = "always start the FM Selector (if one is present)";

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
            {
                Log(Paths.CamModIni + " not found for " + Config.GetGameExe(game), stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(camModIni).ToList();
            }
            catch (Exception ex)
            {
                Log("Exception reading " + Paths.CamModIni + " for " + Config.GetGameExe(game), ex);
                return false;
            }

            string stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            bool changeLoaderIfResetting = true;

            if (resetSelector)
            {
                // NOTE: We're reading cam_mod.ini right here to grab these new values, that's why we're not
                // calling the regular cam_mod.ini reader method. Don't panic.

                // If the loader is now something other than us, then leave it be and don't change anything
                var tempSelectorsList = new List<string>();
                for (int i = 0; i < lines.Count; i++)
                {
                    string lt = lines[i].Trim();

                    string selectorFileName;
                    if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                        char.IsWhiteSpace(lt[fmSelectorKey.Length]) &&
                        (selectorFileName = lt.Substring(fmSelectorKey.Length + 1)).EndsWithI(".dll"))
                    {
                        if (!tempSelectorsList.PathContainsI(selectorFileName)) tempSelectorsList.Add(selectorFileName);
                    }
                }

                if (tempSelectorsList.Count > 0 &&
                   !tempSelectorsList[tempSelectorsList.Count - 1].PathEqualsI(stubPath))
                {
                    changeLoaderIfResetting = false;
                }
            }

            // Confirmed NewDark can read fm_selector values with both forward and backward slashes

            // The loader is us, so use our saved previous loader or lacking that, make a best-effort guess
            var startupFMSelectorLines = Config.GetStartupFMSelectorLines(game);
            string selectorPath = resetSelector
                ? FindPreviousSelector(startupFMSelectorLines.Count > 0 ? startupFMSelectorLines : lines,
                    fmSelectorKey, stubPath, gamePath)
                : stubPath;

            bool prevAlwaysLoadSelector = Config.GetStartupAlwaysStartSelector(game);

            /*
             Conforms to the way NewDark reads it:
             - Zero or more whitespace characters allowed at the start of the line (before the key)
             - The key-value separator is one or more whitespace characters
             - Keys are case-insensitive
             - If duplicate keys exist, later ones replace earlier ones
             - Comment lines start with ;
             - No section headers
            */
            int lastSelKeyIndex = -1;
            int fmLineLastIndex = -1;
            int fmCommentLineIndex = -1;
            bool loaderIsAlreadyUs = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string lt = lines[i].TrimStart();

                do { lt = lt.TrimStart(CA_Semicolon).Trim(); } while (lt.Length > 0 && lt[0] == ';');

                // Steam robustness: get rid of any fan mission specifiers in here
                // line is "fm BrokenTriad_1_0" for example
                if (lt.StartsWithI("fm") && lt.Length > 2 && char.IsWhiteSpace(lt[2]) &&
                    lt.Substring(2).Trim().Length > 0)
                {
                    if (lines[i].TrimStart()[0] != ';') lines[i] = ";" + lines[i];
                }

                if (fmCommentLineIndex == -1 && lt.EqualsI(fmCommentLine)) fmCommentLineIndex = i;

                if (fmLineLastIndex == -1 && lt.EqualsI("fm"))
                {
                    if (!resetSelector)
                    {
                        if (lines[i].TrimStart()[0] == ';') lines[i] = "fm";
                    }
                    else
                    {
                        if (prevAlwaysLoadSelector)
                        {
                            if (lines[i].TrimStart()[0] == ';') lines[i] = "fm";
                        }
                        else
                        {
                            if (lines[i].TrimStart()[0] != ';') lines[i] = ";fm";
                        }
                    }
                    fmLineLastIndex = i;
                }
                if (!resetSelector || changeLoaderIfResetting)
                {
                    if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                        char.IsWhiteSpace(lt[fmSelectorKey.Length]) && lt
                            .Substring(fmSelectorKey.Length + 1).TrimStart()
                            .PathEqualsI(selectorPath))
                    {
                        if (loaderIsAlreadyUs)
                        {
                            lines.RemoveAt(i);
                            i--;
                            lastSelKeyIndex = (lastSelKeyIndex - 1).Clamp(-1, int.MaxValue);
                        }
                        else
                        {
                            lines[i] = fmSelectorKey + " " + selectorPath;
                            loaderIsAlreadyUs = true;
                        }
                        continue;
                    }

                    if (lt.EqualsI(fmSelectorKey) ||
                        (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                        char.IsWhiteSpace(lt[fmSelectorKey.Length])))
                    {
                        if (lines[i].TrimStart()[0] != ';') lines[i] = ";" + lines[i];
                        lastSelKeyIndex = i;
                    }
                }
            }

            if (!resetSelector || changeLoaderIfResetting)
            {
                if (!loaderIsAlreadyUs)
                {
                    if (lastSelKeyIndex == -1 || lastSelKeyIndex == lines.Count - 1)
                    {
                        lines.Add(fmSelectorKey + " " + selectorPath);
                    }
                    else
                    {
                        lines.Insert(lastSelKeyIndex + 1, fmSelectorKey + " " + selectorPath);
                    }
                }
            }

            if (fmLineLastIndex == -1)
            {
                if (fmCommentLineIndex == -1 || fmCommentLineIndex == lines.Count - 1)
                {
                    lines.Add("");
                    lines.Add("; " + fmCommentLine);
                    if (!resetSelector)
                    {
                        lines.Add("fm");
                    }
                    else
                    {
                        lines.Add(prevAlwaysLoadSelector ? "fm" : ";fm");
                    }
                }
                else
                {
                    if (!resetSelector)
                    {
                        lines.Insert(fmCommentLineIndex + 1, "fm");
                    }
                    else
                    {
                        lines.Insert(fmCommentLineIndex + 1, prevAlwaysLoadSelector ? "fm" : ";fm");
                    }
                }
            }

            try
            {
                File.WriteAllLines(camModIni, lines);
            }
            catch (Exception ex)
            {
                Log("Exception writing " + Paths.CamModIni + " for " + Config.GetGameExe(game), ex);
                return false;
            }

            return true;
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static bool SetT3FMSelector(bool resetSelector = false)
        {
            const string externSelectorKey = "ExternSelector=";
            const string alwaysShowKey = "AlwaysShow=";
            const string fanMissionKey = "FanMission=";
            bool existingExternSelectorKeyOverwritten = false;
            bool existingAlwaysShowKeyOverwritten = false;
            int insertLineIndex = -1;

            string soIni = Paths.GetSneakyOptionsIni();
            if (soIni.IsEmpty())
            {
                Log("Couldn't set us as the loader for Thief: Deadly Shadows because SneakyOptions.ini could not be found", stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(soIni).ToList();
            }
            catch (Exception ex)
            {
                Log("Problem reading SneakyOptions.ini", ex);
                return false;
            }

            // Confirmed SU can read the selector value with both forward and backward slashes

            string stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            string selectorPath;

            bool changeLoaderIfResetting = true;

            #region Reset loader

            // TODO: @CourteousBehavior: Save and restore the "always start with FM" line(s)
            // Probably nobody uses this feature, but maybe we should do it for completeness?
            if (resetSelector)
            {
                var startupFMSelectorLines = Config.GetStartupFMSelectorLines(GameIndex.Thief3);
                string prevFMSelectorValue = "";

                #region Read the previous loader value

                for (int i = 0; i < lines.Count; i++)
                {
                    if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                    while (i < lines.Count - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (lt.StartsWithI(externSelectorKey))
                        {
                            prevFMSelectorValue = lt.Substring(lt.IndexOf('=') + 1);
                            break;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }

                        i++;
                    }
                    break;
                }

                #endregion

                // If loader is not us, leave it be
                if (!prevFMSelectorValue.PathEqualsI(stubPath) &&
                    !(startupFMSelectorLines.Count > 0 &&
                     startupFMSelectorLines[0].PathEqualsI(Paths.StubFileName)))
                {
                    selectorPath = "";
                    changeLoaderIfResetting = false;
                }
                else if (startupFMSelectorLines.Count > 0 &&
                    startupFMSelectorLines[0].PathEqualsI(Paths.StubFileName) ||
                    prevFMSelectorValue.IsEmpty())
                {
                    selectorPath = Paths.FMSelDll;
                }
                else
                {
                    selectorPath = startupFMSelectorLines.Count == 0 ||
                                   !startupFMSelectorLines[0].PathEqualsI(stubPath)
                        ? startupFMSelectorLines[0]
                        : Paths.FMSelDll;
                }
            }
            else
            {
                selectorPath = stubPath;
            }

            #endregion

            bool prevAlwaysShowValue = Config.GetStartupAlwaysStartSelector(GameIndex.Thief3);

            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                insertLineIndex = i + 1;
                while (i < lines.Count - 1)
                {
                    string lt = lines[i + 1].Trim();
                    if ((!resetSelector || changeLoaderIfResetting) &&
                        !existingExternSelectorKeyOverwritten &&
                        lt.StartsWithI(externSelectorKey))
                    {
                        lines[i + 1] = externSelectorKey + selectorPath;
                        existingExternSelectorKeyOverwritten = true;
                    }
                    else if (!existingAlwaysShowKeyOverwritten &&
                        lt.StartsWithI(alwaysShowKey))
                    {
                        lines[i + 1] = alwaysShowKey + (resetSelector && !prevAlwaysShowValue ? "false" : "true");
                        existingAlwaysShowKeyOverwritten = true;
                    }
                    // Steam robustness: get rid of any fan mission specifiers in here
                    else if (lt.StartsWithI(fanMissionKey))
                    {
                        lines[i + 1] = fanMissionKey;
                    }

                    if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']') break;

                    i++;
                }
                break;
            }

            if ((!resetSelector || changeLoaderIfResetting) &&
                !existingExternSelectorKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, externSelectorKey + selectorPath);
            }

            if (!existingAlwaysShowKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, alwaysShowKey + "true");
            }

            try
            {
                File.WriteAllLines(soIni, lines);
            }
            catch (Exception ex)
            {
                Log("Problem writing SneakyOptions.ini", ex);
                return false;
            }

            return true;
        }

        #endregion
    }
}

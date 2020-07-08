using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class GameConfigFiles
    {
        #region Read

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static (string FMsPath, string FMLanguage, bool FMLanguageForced,
                         List<string> FMSelectorLines, bool AlwaysShowLoader)
        GetInfoFromCamModIni(string gamePath, out Error error, bool langOnly = false)
        {
            string CreateAndReturnFMsPath()
            {
                string fmsPath = Path.Combine(gamePath, "FMs");
                try
                {
                    Directory.CreateDirectory(fmsPath);
                }
                catch (Exception ex)
                {
                    Log("Exception creating FM installed base dir", ex);
                }

                return fmsPath;
            }

            var fmSelectorLines = new List<string>();
            bool alwaysShowLoader = false;

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
            {
                //error = Error.CamModIniNotFound;
                error = Error.None;
                return (!langOnly ? CreateAndReturnFMsPath() : "", "", false, fmSelectorLines, false);
            }

            string path = "";
            string fm_language = "";
            bool fm_language_forced = false;

            const string key_fm_selector = "fm_selector";
            //const int key_fm_selector_len = 11; // unused currently
            const string key_fm = "fm";
            //const int key_fm_len = 2; // unused currently
            const string key_fm_path = "fm_path";
            const int key_fm_path_len = 7;
            const string key_fm_language = "fm_language";
            const int key_fm_language_len = 11;
            const string key_fm_language_forced = "fm_language_forced";
            const int key_fm_language_forced_len = 18;

            using (var sr = new StreamReader(camModIni))
            {
                /*
                 Conforms to the way NewDark reads it:
                 - Zero or more whitespace characters allowed at the start of the line (before the key)
                 - The key-value separator is one or more whitespace characters
                 - Keys are case-insensitive
                 - If duplicate keys exist, later ones replace earlier ones
                 - Comment lines start with ;
                 - No section headers
                */
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.IsEmpty()) continue;

                    line = line.TrimStart();

                    // Quick check; these lines will be checked more thoroughly when we go to use them
                    if (!langOnly && line.ContainsI(key_fm_selector)) fmSelectorLines.Add(line);
                    if (!langOnly && line.Trim().EqualsI(key_fm)) alwaysShowLoader = true;

                    if (line.IsEmpty() || line[0] == ';') continue;

                    if (!langOnly && line.StartsWithIPlusWhiteSpace(key_fm_path))
                    {
                        path = line.Substring(key_fm_path_len).Trim();
                    }
                    else if (line.StartsWithIPlusWhiteSpace(key_fm_language))
                    {
                        fm_language = line.Substring(key_fm_language_len).Trim();
                    }
                    else if (line.StartsWithI(key_fm_language_forced))
                    {
                        if (line.Trim().Length == key_fm_language_forced_len)
                        {
                            fm_language_forced = true;
                        }
                        else if (char.IsWhiteSpace(line[key_fm_language_forced_len]))
                        {
                            fm_language_forced = line.Substring(key_fm_language_forced_len).Trim() != "0";
                        }
                    }
                }
            }

            if (langOnly)
            {
                error = Error.None;
                return ("", fm_language, fm_language_forced, new List<string>(), false);
            }

            if (PathIsRelative(path))
            {
                try
                {
                    path = RelativeToAbsolute(gamePath, path);
                }
                catch (Exception)
                {
                    error = Error.None;
                    return (CreateAndReturnFMsPath(), fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader);
                }
            }

            error = Error.None;
            return (Directory.Exists(path) ? path : CreateAndReturnFMsPath(),
                fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader);
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static (Error Error, bool UseCentralSaves, string FMInstallPath,
                        string PrevFMSelectorValue, bool AlwaysShowLoader)
        GetInfoFromSneakyOptionsIni()
        {
            string soIni = Paths.GetSneakyOptionsIni();
            Error soError = soIni.IsEmpty() ? Error.SneakyOptionsNoRegKey : !File.Exists(soIni) ? Error.SneakyOptionsNotFound : Error.None;
            if (soError != Error.None)
            {
                // Has to be MessageBox (not View.ShowAlert()) because the view may not have been created yet
                MessageBox.Show(LText.AlertMessages.Misc_SneakyOptionsIniNotFound, LText.AlertMessages.Alert, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (soError, false, "", "", false);
            }

            bool ignoreSavesKeyFound = false;
            bool ignoreSavesKey = true;

            bool fmInstPathFound = false;
            string fmInstPath = "";

            bool externSelectorFound = false;
            string prevFMSelectorValue = "";

            bool alwaysShowLoaderFound = false;
            bool alwaysShowLoader = false;

            string[] lines = File.ReadAllLines(soIni);
            for (int i = 0; i < lines.Length; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.EqualsI("[Loader]"))
                {
                    /*
                     Conforms to the way Sneaky Upgrade reads it:
                     - Whitespace allowed on both sides of section headers (but not within brackets)
                     - Section headers and keys are case-insensitive
                     - Key-value separator is '='
                     - Whitespace allowed on left side of key (but not right side before '=')
                     - Case-insensitive "true" is true, anything else is false
                     - If duplicate keys exist, the earliest one is used
                    */
                    while (i < lines.Length - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (!ignoreSavesKeyFound &&
                            !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("IgnoreSavesKey="))
                        {
                            ignoreSavesKey = lt.Substring(lt.IndexOf('=') + 1).EqualsTrue();
                            ignoreSavesKeyFound = true;
                        }
                        else if (!fmInstPathFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("InstallPath="))
                        {
                            fmInstPath = lt.Substring(lt.IndexOf('=') + 1).Trim();
                            fmInstPathFound = true;
                        }
                        else if (!externSelectorFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("ExternSelector="))
                        {
                            prevFMSelectorValue = lt.Substring(lt.IndexOf('=') + 1).Trim();
                            externSelectorFound = true;
                        }
                        else if (!alwaysShowLoaderFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("AlwaysShow="))
                        {
                            alwaysShowLoader = lt.Substring(lt.IndexOf('=') + 1).Trim().EqualsTrue();
                            alwaysShowLoaderFound = true;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }

                        // TODO: @Robustness: Easy to forget to add stuff here, and I don't think we need this really
                        // as long as we're only in the Loader section, it doesn't really give a speedup I don't
                        // think
                        if (ignoreSavesKeyFound &&
                            fmInstPathFound &&
                            externSelectorFound &&
                            alwaysShowLoaderFound)
                        {
                            break;
                        }

                        i++;
                    }
                    break;
                }
            }

            return fmInstPathFound
                ? (Error.None, !ignoreSavesKey, fmInstPath, prevFMSelectorValue, alwaysShowLoader)
                : (Error.T3FMInstPathNotFound, false, "", prevFMSelectorValue, alwaysShowLoader);
        }

        #endregion

        #region Write

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
        // @CAN_RUN_BEFORE_VIEW_INIT
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
                        return line.StartsWithIPlusWhiteSpace(fmSelectorKey) &&
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
                    if (lt.StartsWithIPlusWhiteSpace(fmSelectorKey) &&
                        (selectorFileName = lt.Substring(fmSelectorKey.Length + 1)).EndsWithI(".dll"))
                    {
                        if (!tempSelectorsList.PathContainsI(selectorFileName)) tempSelectorsList.Add(selectorFileName);
                    }
                }

                if (tempSelectorsList.Count > 0 &&
                   !tempSelectorsList[tempSelectorsList.Count - 1].PathEqualsI(Paths.StubPath))
                {
                    changeLoaderIfResetting = false;
                }
            }

            // Confirmed NewDark can read fm_selector values with both forward and backward slashes

            // The loader is us, so use our saved previous loader or lacking that, make a best-effort guess
            var startupFMSelectorLines = Config.GetStartupFMSelectorLines(game);
            string selectorPath = resetSelector
                ? FindPreviousSelector(startupFMSelectorLines.Count > 0 ? startupFMSelectorLines : lines,
                    fmSelectorKey, Paths.StubPath, gamePath)
                : Paths.StubPath;

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
                if (lt.StartsWithIPlusWhiteSpace("fm") &&
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
                    if (lt.StartsWithIPlusWhiteSpace(fmSelectorKey) &&
                        lt.Substring(fmSelectorKey.Length + 1).TrimStart().PathEqualsI(selectorPath))
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

                    if (lt.EqualsI(fmSelectorKey) || lt.StartsWithIPlusWhiteSpace(fmSelectorKey))
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
                if (!prevFMSelectorValue.PathEqualsI(Paths.StubPath) &&
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
                                   !startupFMSelectorLines[0].PathEqualsI(Paths.StubPath)
                        ? startupFMSelectorLines[0]
                        : Paths.FMSelDll;
                }
            }
            else
            {
                selectorPath = Paths.StubPath;
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

        #endregion
    }
}

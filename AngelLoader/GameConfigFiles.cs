using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader
{
    // @BetterErrors(GameConfigFiles):
    // Go through this whole thing and check what we want to do for each error. Some of these should maybe remain
    // silent because they have fallbacks, like if we can't set ourselves as the loader then the user will get a
    // dialog about it if they try to start the game executable by itself, and maybe we don't want to bother the
    // user for that? Then again, it's an unexpected situation that shouldn't happen often if at all, so maybe we
    // should alert the user.

    internal static class GameConfigFiles
    {
        #region Constants

        // cam_mod.ini
        private const string key_fm_selector = "fm_selector";
        private const int key_fm_selector_len = 11;
        private const string key_fm = "fm";
        private const string key_fm_path = "fm_path";
        private const int key_fm_path_len = 7;
        private const string mod_path = "mod_path";
        private const string uber_mod_path = "uber_mod_path";
        private const string mp_mod_path = "mp_mod_path";
        private const string mp_u_mod_path = "mp_u_mod_path";

        // cam_mod.ini, cam.cfg
        private const string key_fm_language = "fm_language";
        private const int key_fm_language_len = 11;
        private const string key_fm_language_forced = "fm_language_forced";
        private const int key_fm_language_forced_len = 18;

        // cam.cfg
        private const string key_character_detail = "character_detail";

        // SneakyOptions.ini
        private const string key_ExternSelector = "ExternSelector=";
        private const string key_AlwaysShow = "AlwaysShow=";
        private const string key_FanMission = "FanMission=";
        private const string key_InstallPath = "InstallPath=";
        private const string key_IgnoreSavesKey = "IgnoreSavesKey=";

#if !ReleaseBeta && !ReleasePublic

        // user.cfg
        private const string key_inv_status_height = "inv_status_height";
        private const string key_game_screen_size = "game_screen_size";

#endif

        #endregion

        #region Read

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static (string FMsPath, string FMLanguage, bool FMLanguageForced,
                         List<string> FMSelectorLines, bool AlwaysShowLoader)
        GetInfoFromCamModIni(string gamePath, bool langOnly = false)
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
                    // @BetterErrors(GetInfoFromCamModIni()/CreateAndReturnFMsPath())
                    Log(ErrorText.ExCreate + "FM installed base dir", ex);
                }

                return fmsPath;
            }

            var fmSelectorLines = new List<string>();
            bool alwaysShowLoader = false;

            // @BetterErrors: Throw up dialog if not found, cause that means we're OldDark or broken.
            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
            {
                return (!langOnly ? CreateAndReturnFMsPath() : "", "", false, fmSelectorLines, false);
            }

            string path = "";
            string fm_language = "";
            bool fm_language_forced = false;

            /*
            @vNext: Convert this to ReadAllLines in advance style like everything else
            NOTE: The issue is return value: if we can't read cam_mod.ini, do we want to create an FMs dir?
            We don't have an error return value. I think we don't want to create an FMs dir if we can't read
            cam_mod.ini because there could be a different one in there and so we would create the default
            one when we shouldn't in that case.
            If we return "" for fms path, we'll have it set blank in Config even though we're NewDark and it
            should never be blank. Normally if it's blank that means we didn't find cam_mod.ini at all, so
            we're OldDark, and the user is on their own, unsupported scenario.
            So it's actually quite tricky to figure out what we should do if we fail reading cam_mod.ini.
            We could throw up an error dialog, but we're still in a weird state after. We currently just let
            it crash (we have no exception catching for this!).
            */
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
                while (sr.ReadLine() is { } line)
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
                return ("", fm_language, fm_language_forced, new List<string>(), false);
            }

            if (PathIsRelative(path))
            {
                try
                {
                    path = RelativeToAbsolute(gamePath, path);
                }
                catch
                {
                    return (CreateAndReturnFMsPath(), fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader);
                }
            }

            return (Directory.Exists(path) ? path : CreateAndReturnFMsPath(),
                fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader);
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static (bool Success, bool UseCentralSaves, string FMInstallPath,
                        string PrevFMSelectorValue, bool AlwaysShowLoader)
        GetInfoFromSneakyOptionsIni()
        {
            string soIni = Paths.GetSneakyOptionsIni();
            if (soIni.IsEmpty())
            {
                Core.Dialogs.ShowAlert(LText.AlertMessages.Misc_SneakyOptionsIniNotFound, LText.AlertMessages.Alert);
                return (false, false, "", "", false);
            }

            bool ignoreSavesKeyFound = false;
            bool ignoreSavesKey = true;

            bool fmInstPathFound = false;
            string fmInstPath = "";

            bool externSelectorFound = false;
            string prevFMSelectorValue = "";

            bool alwaysShowLoaderFound = false;
            bool alwaysShowLoader = false;

            if (!TryReadAllLines(soIni, out var lines))
            {
                return (false, false, "", "", false);
            }

            for (int i = 0; i < lines.Count; i++)
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
                    while (i < lines.Count - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (!ignoreSavesKeyFound &&
                            !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI(key_IgnoreSavesKey))
                        {
                            ignoreSavesKey = lt.Substring(lt.IndexOf('=') + 1).EqualsTrue();
                            ignoreSavesKeyFound = true;
                        }
                        else if (!fmInstPathFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI(key_InstallPath))
                        {
                            fmInstPath = lt.Substring(lt.IndexOf('=') + 1).Trim();
                            fmInstPathFound = true;
                        }
                        else if (!externSelectorFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI(key_ExternSelector))
                        {
                            prevFMSelectorValue = lt.Substring(lt.IndexOf('=') + 1).Trim();
                            externSelectorFound = true;
                        }
                        else if (!alwaysShowLoaderFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI(key_AlwaysShow))
                        {
                            alwaysShowLoader = lt.Substring(lt.IndexOf('=') + 1).Trim().EqualsTrue();
                            alwaysShowLoaderFound = true;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }

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
                ? (true, !ignoreSavesKey, fmInstPath, prevFMSelectorValue, alwaysShowLoader)
                : (false, false, "", prevFMSelectorValue, alwaysShowLoader);
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
                            string gamePath = Config.GetGamePath(gameIndex);
                            SetCamCfgLanguage(gamePath, "");
                            SetDarkFMSelector(gameIndex, gamePath, resetSelector: true);
                            FixCharacterDetailLine(gameIndex);
                        }
                        else
                        {
                            // For Thief 3, we actually just need SneakyOptions.ini. The game itself existing
                            // is not technically a requirement.
                            SetT3FMSelector(resetSelector: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ErrorText.ExTry + "unset temp config values\r\n" +
                        "GameIndex: " + gameIndex + "\r\n" +
                        "GameExe: " + gameExe,
                        ex);

                    Core.Dialogs.ShowError("Error attempting to restore previous game config file settings.\r\n" +
                                           "Game: " + gameIndex + "\r\n" +
                                           "Game exe: " + gameExe);
                }
            }
        }

        // @BetterErrors(FixCharacterDetailLineInCamCfg)
        // The bug:
        // Ascend the Dim Valley has "character_detail 0" in its fm.cfg file. This file is supposed to contain
        // values that ONLY apply to that specific FM. But, NewDark writes the character_detail value back out
        // to the global cam.cfg after reading it from the FM-specific fm.cfg, causing the value to persist
        // permanently for other FMs, which causes broken texture UV on many models. The fix is to just force
        // character_detail back to 1 in cam.cfg.
        // We also force character_detail 1 in all other config files from which that value can be read and applied.
        // Note: this option is written and read from Config.ini, but has no UI way to change it. If a user wants
        // to change it they can change it in Config.ini and it will be honored.
        // We don't want to allow UI changing because the option shouldn't be disabled pretty much ever under
        // normal circumstances.
        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static void FixCharacterDetailLine(GameIndex gameIndex)
        {
            if (gameIndex is not GameIndex.Thief1 and not GameIndex.Thief2) return;

            if (!Config.EnableCharacterDetailFix) return;

            string gamePath = Config.GetGamePath(gameIndex);

            if (gamePath.IsEmpty()) return;

            static void Run(string gamePath, string fileName, bool removeAll)
            {
                if (!TryCombineFilePathAndCheckExistence(gamePath, fileName, out string cfgFile))
                {
                    return;
                }

                if (!TryReadAllLines(cfgFile, out var lines))
                {
                    return;
                }

                bool atLeastOneCharacterDetailOneLineFound = false;
                bool linesModified = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    string lt = lines[i].Trim();
                    if (lt.StartsWithI(key_character_detail))
                    {
                        string val = lt.Substring(key_character_detail.Length).Trim();
                        if (removeAll || val == "0")
                        {
                            lines.RemoveAt(i);
                            i--;
                            linesModified = true;
                        }
                        else if (val == "1")
                        {
                            atLeastOneCharacterDetailOneLineFound = true;
                        }
                    }
                }

                if (!removeAll && !atLeastOneCharacterDetailOneLineFound)
                {
                    lines.Add(key_character_detail + " 1");
                    linesModified = true;
                }

                if (!linesModified) return;

                if (!TryWriteAllLines(cfgFile, lines))
                {
                    // ReSharper disable once RedundantJumpStatement
                    return; // Explicit for clarity of intent
                }
            }

            Run(gamePath, Paths.CamCfg, removeAll: false);
            Run(gamePath, Paths.CamExtCfg, removeAll: true);
            Run(gamePath, Paths.CamModIni, removeAll: true);
        }

        // @BetterErrors(SetCamCfgLanguage): Pop up actual dialogs here if we fail, because we do NOT want scraps of the wrong language left
        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static void SetCamCfgLanguage(string gamePath, string lang)
        {
            if (gamePath.IsEmpty()) return;

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamCfg, out string camCfg))
            {
                return;
            }

            if (!TryReadAllLines(camCfg, out var lines))
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.StartsWithI(key_fm_language) || lineT.StartsWithI(key_fm_language_forced))
                {
                    lines.RemoveAt(i);
                    i--;
                }
            }

            if (!lang.IsEmpty())
            {
                lines.Add(key_fm_language + " " + lang);
                lines.Add(key_fm_language_forced + " 1");
            }

            if (!TryWriteAllLines(camCfg, lines))
            {
                // ReSharper disable once RedundantJumpStatement
                return; // Explicit for clarity of intent
            }
        }

        #region Set selectors

        // 2019-10-16: We also now force the loader to start in the config files rather than just on the command
        // line. This is to support Steam launching, because Steam can't take game-specific command line arguments.

        // @CAN_RUN_BEFORE_VIEW_INIT
        // @vNext(SetDarkFMSelector): Rewrite this mess into something readable!
        internal static bool SetDarkFMSelector(GameIndex gameIndex, string gamePath, bool resetSelector = false)
        {
            if (!GameIsDark(gameIndex)) return false;

            if (gamePath.IsEmpty()) return false;

            #region Local functions

            static string FindPreviousSelector(List<string> lines, string stubPath, string gamePath)
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
                        catch
                        {
                            return "";
                        }
                    }

                    return path;
                }

                string TryGetOtherSelectorSpecifier(string line)
                {
                    // try-catch cause of Path.Combine() maybe trying to combine invalid-for-path strings
                    // In .NET Core, we could use Path.Join() to avoid throwing
                    try
                    {
                        string selectorFileName;
                        return line.StartsWithIPlusWhiteSpace(key_fm_selector) &&
                               (selectorFileName = line.Substring(key_fm_selector_len + 1)).EndsWithI(".dll") &&
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
                        lt = RemoveLeadingSemicolons(lt);

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

            const string fmCommentLine = "always start the FM Selector (if one is present)";

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
            {
                // @BetterErrors: Notify if cam_mod.ini not found / couldn't read
                return false;
            }

            if (!TryReadAllLines(camModIni, out var lines))
            {
                return false;
            }

            bool changeLoaderIfResetting = true;

            if (resetSelector)
            {
                // We're reading cam_mod.ini right here to grab these new values, that's why we're not calling
                // the regular cam_mod.ini reader method. Don't panic.

                // If the loader is now something other than us, then leave it be and don't change anything
                var tempSelectorsList = new List<string>();
                for (int i = 0; i < lines.Count; i++)
                {
                    string lt = lines[i].Trim();

                    string selectorFileName;
                    if (lt.StartsWithIPlusWhiteSpace(key_fm_selector) &&
                        (selectorFileName = lt.Substring(key_fm_selector_len + 1)).EndsWithI(".dll"))
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
            var startupFMSelectorLines = Config.GetStartupFMSelectorLines(gameIndex);
            string selectorPath = resetSelector
                ? FindPreviousSelector(startupFMSelectorLines.Count > 0 ? startupFMSelectorLines : lines,
                    Paths.StubPath, gamePath)
                : Paths.StubPath;

            bool prevAlwaysLoadSelector = Config.GetStartupAlwaysStartSelector(gameIndex);

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

                lt = RemoveLeadingSemicolons(lt);

                // Steam robustness: get rid of any fan mission specifiers in here
                // line is "fm BrokenTriad_1_0" for example
                if (lt.StartsWithIPlusWhiteSpace(key_fm) &&
                    lt.Substring(2).Trim().Length > 0)
                {
                    if (lines[i].TrimStart()[0] != ';') lines[i] = ";" + lines[i];
                }

                if (fmCommentLineIndex == -1 && lt.EqualsI(fmCommentLine)) fmCommentLineIndex = i;

                if (fmLineLastIndex == -1 && lt.EqualsI(key_fm))
                {
                    if (!resetSelector)
                    {
                        if (lines[i].TrimStart()[0] == ';') lines[i] = key_fm;
                    }
                    else
                    {
                        if (prevAlwaysLoadSelector)
                        {
                            if (lines[i].TrimStart()[0] == ';') lines[i] = key_fm;
                        }
                        else
                        {
                            if (lines[i].TrimStart()[0] != ';') lines[i] = ";" + key_fm;
                        }
                    }
                    fmLineLastIndex = i;
                }
                if (!resetSelector || changeLoaderIfResetting)
                {
                    if (lt.StartsWithIPlusWhiteSpace(key_fm_selector) &&
                        lt.Substring(key_fm_selector_len + 1).TrimStart().PathEqualsI(selectorPath))
                    {
                        if (loaderIsAlreadyUs)
                        {
                            lines.RemoveAt(i);
                            i--;
                            lastSelKeyIndex = (lastSelKeyIndex - 1).ClampToMin(-1);
                        }
                        else
                        {
                            lines[i] = key_fm_selector + " " + selectorPath;
                            loaderIsAlreadyUs = true;
                        }
                        continue;
                    }

                    if (lt.EqualsI(key_fm_selector) || lt.StartsWithIPlusWhiteSpace(key_fm_selector))
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
                    if (lastSelKeyIndex == -1) lastSelKeyIndex = lines.Count - 1;
                    lines.Insert(lastSelKeyIndex + 1, key_fm_selector + " " + selectorPath);
                }
            }

            if (fmLineLastIndex == -1)
            {
                string fmLine = resetSelector && !prevAlwaysLoadSelector ? ";" + key_fm : key_fm;

                if (fmCommentLineIndex == -1)
                {
                    lines.Add("");
                    lines.Add("; " + fmCommentLine);
                    fmCommentLineIndex = lines.Count - 1;
                }

                lines.Insert(fmCommentLineIndex + 1, fmLine);
            }

            return TryWriteAllLines(camModIni, lines);
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static bool SetT3FMSelector(bool resetSelector = false)
        {
            bool existingExternSelectorKeyOverwritten = false;
            bool existingAlwaysShowKeyOverwritten = false;
            int insertLineIndex = -1;

            string soIni = Paths.GetSneakyOptionsIni();
            if (soIni.IsEmpty())
            {
                return false;
            }

            if (!TryReadAllLines(soIni, out var lines))
            {
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
                        if (lt.StartsWithI(key_ExternSelector))
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
                else if ((startupFMSelectorLines.Count > 0 &&
                    startupFMSelectorLines[0].PathEqualsI(Paths.StubFileName)) ||
                    prevFMSelectorValue.IsEmpty())
                {
                    selectorPath = Paths.FMSelDll;
                }
                else
                {
                    selectorPath = startupFMSelectorLines.Count > 0 &&
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
                        lt.StartsWithI(key_ExternSelector))
                    {
                        lines[i + 1] = key_ExternSelector + selectorPath;
                        existingExternSelectorKeyOverwritten = true;
                    }
                    else if (!existingAlwaysShowKeyOverwritten &&
                        lt.StartsWithI(key_AlwaysShow))
                    {
                        lines[i + 1] = key_AlwaysShow + (resetSelector && !prevAlwaysShowValue ? "false" : "true");
                        existingAlwaysShowKeyOverwritten = true;
                    }
                    // Steam robustness: get rid of any fan mission specifiers in here
                    else if (lt.StartsWithI(key_FanMission))
                    {
                        lines[i + 1] = key_FanMission;
                    }

                    if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']') break;

                    i++;
                }
                break;
            }

            if ((!resetSelector || changeLoaderIfResetting) &&
                !existingExternSelectorKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, key_ExternSelector + selectorPath);
            }

            if (!existingAlwaysShowKeyOverwritten && insertLineIndex > -1)
            {
                lines.Insert(insertLineIndex, key_AlwaysShow + "true");
            }

            return TryWriteAllLines(soIni, lines);
        }

        #endregion

        #endregion

#if !ReleaseBeta && !ReleasePublic

        private static void RemoveKeyLine(string key, List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                string ltNS = RemoveLeadingSemicolons(lines[i].Trim());
                if (ltNS.StartsWithIPlusWhiteSpace(key))
                {
                    lines.RemoveAt(i);
                    i--;
                }
            }
        }

        private static void RemoveConsecutiveWhiteSpace(List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].IsWhiteSpace())
                {
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        if (lines[j].IsWhiteSpace())
                        {
                            lines.RemoveAt(j);
                            j--;
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        private static bool TryGetGameDirFilePathIfExists(GameIndex gameIndex, string fileName, out string result)
        {
            result = "";
            string gamePath = Config.GetGamePath(gameIndex);
            if (gamePath.IsEmpty()) return false;
            try
            {
                string fullPath = Path.Combine(gamePath, fileName);
                if (File.Exists(fullPath))
                {
                    result = fullPath;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private static readonly char[] _ca_Space_Tab_Semicolon = { ' ', '\t', ';' };
        internal static bool? GetScreenShotMode(GameIndex gameIndex)
        {
            if (!GameIsDark(gameIndex)) return null;

            if (!TryGetGameDirFilePathIfExists(gameIndex, Paths.UserCfg, out string userCfgFile)) return null;

            if (!TryReadAllLines(userCfgFile, out var lines)) return null;

            bool ret = false;

            for (int i = 0; i < lines.Count; i++)
            {
                string lt = lines[i].Trim();
                string ltNS = RemoveLeadingSemicolons(lt);
                if (ltNS.StartsWithIPlusWhiteSpace(key_inv_status_height))
                {
                    string[] fields = ltNS.Split(_ca_Space_Tab_Semicolon, StringSplitOptions.RemoveEmptyEntries);
                    ret = fields.Length >= 2 &&
                          int.TryParse(fields[1], out int result) &&
                          result == 0 &&
                          lt[0] != ';';
                }
            }

            return ret;
        }

        internal static void SetScreenShotMode(GameIndex gameIndex, bool enabled)
        {
            if (!GameIsDark(gameIndex)) return;

            if (!TryGetGameDirFilePathIfExists(gameIndex, Paths.UserCfg, out string userCfgFile)) return;

            if (!TryReadAllLines(userCfgFile, out var lines)) return;

            RemoveKeyLine(key_inv_status_height, lines);

            lines.Insert(0, (enabled ? "" : ";") + key_inv_status_height + " 0 ; Added by AngelLoader: uncommented = screenshot mode enabled (no hud)");
            lines.Insert(1, "");

            RemoveConsecutiveWhiteSpace(lines);

            TryWriteAllLines(userCfgFile, lines);
        }

        internal static void SetGlobalDarkGameValues(GameIndex gameIndex)
        {
            if (!GameIsDark(gameIndex)) return;
            SetResolution(gameIndex);
        }

        internal static void SetPerFMDarkGameValues(FanMission fm, GameIndex gameIndex)
        {
            if (!GameIsDark(gameIndex)) return;
            SetResolution(gameIndex);
            // @FM_CFG: Call final SetPerFMValues() here
        }

        private static void SetResolution(GameIndex gameIndex)
        {
            if (!Config.ForceGameResToMainMonitorRes) return;

            if (!GameIsDark(gameIndex)) return;

            if (!TryGetGameDirFilePathIfExists(gameIndex, Paths.CamCfg, out string camCfgFile)) return;

            if (!TryReadAllLines(camCfgFile, out var lines)) return;

            RemoveKeyLine(key_game_screen_size, lines);

            var res = Screen.PrimaryScreen.Bounds;
            lines.Add(key_game_screen_size + " " + res.Width + " " + res.Height);

            RemoveConsecutiveWhiteSpace(lines);

            TryWriteAllLines(camCfgFile, lines);
        }

        internal static bool TryGetSmallestUsedMisFile(FanMission fm, out string misFile)
        {
            misFile = "";

            if (fm.Game is not Game.Thief1 and not Game.Thief2) return false;

            if (!FMIsReallyInstalled(fm)) return false;

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            string fmDir = Path.Combine(Config.GetFMInstallPath(gameIndex), fm.InstalledDir);

            var misFiles = new DirectoryInfo(fmDir).GetFiles("*.mis", SearchOption.TopDirectoryOnly);

            if (misFiles.Length == 0) return false;

            var usedMisFiles = new List<FileInfo>(misFiles.Length);

            string? missFlag = null;

            string loc1 = Path.Combine(fmDir, "strings", "missflag.str");
            string loc2 = Path.Combine(fmDir, "strings", "english", "missflag.str");

            if (File.Exists(loc1))
            {
                missFlag = loc1;
            }
            else if (File.Exists(loc2))
            {
                missFlag = loc2;
            }
            else
            {
                string[] files = Directory.GetFiles(Path.Combine(fmDir, "strings"), "missflag.str", SearchOption.AllDirectories);
                if (files.Length > 0) missFlag = files[0];
            }

            if (missFlag == null) return false;

            if (!TryReadAllLines(missFlag, out var mfLines)) return false;

            for (int mfI = 0; mfI < misFiles.Length; mfI++)
            {
                FileInfo mf = misFiles[mfI];

                // @FM_CFG: This is copied from the scanner where perf matters, but we should rewrite this to be clearer and simpler
                // Obtuse nonsense to avoid string allocations (perf)
                if (mf.Name.StartsWithI("miss") && mf.Name[4] != '.')
                {
                    // Since only files ending in .mis are in the misFiles list, we're guaranteed to find a .
                    // character and not get a -1 index. And since we know our file starts with "miss", the
                    // -4 is guaranteed not to take us negative either.
                    int count = mf.Name.IndexOf('.') - 4;
                    for (int mflI = 0; mflI < mfLines.Count; mflI++)
                    {
                        string line = mfLines[mflI];
                        if (line.StartsWithI("miss_") && line.Length > 5 + count && line[5 + count] == ':')
                        {
                            bool numsMatch = true;
                            for (int li = 4; li < 4 + count; li++)
                            {
                                if (line[li + 1] != mf.Name[li])
                                {
                                    numsMatch = false;
                                    break;
                                }
                            }
                            int qIndex;
                            if (numsMatch && (qIndex = line.IndexOf('\"')) > -1)
                            {
                                if (!(line.Length > qIndex + 5 &&
                                      // I don't think any files actually have "skip" in anything other than
                                      // lowercase, but I'm supporting any case anyway. You never know.
                                      (line[qIndex + 1] == 's' || line[qIndex + 1] == 'S') &&
                                      (line[qIndex + 2] == 'k' || line[qIndex + 2] == 'K') &&
                                      (line[qIndex + 3] == 'i' || line[qIndex + 3] == 'I') &&
                                      (line[qIndex + 4] == 'p' || line[qIndex + 4] == 'P') &&
                                      line[qIndex + 5] == '\"'))
                                {
                                    usedMisFiles.Add(mf);
                                }
                            }
                        }
                    }
                }
            }

            if (usedMisFiles.Count == 0) usedMisFiles.AddRange(misFiles);

            usedMisFiles = usedMisFiles.OrderBy(x => x.Length).ToList();

            misFile = usedMisFiles[0].FullName;

            return true;
        }

        public enum FMDarkVersion
        {
            NotDetected,
            OldDark,
            NewDark,
            NotApplicable
        }

        // @FM_CFG: Test this for identicality with the scanner!
        // Prolly just go through all FMs' archives and temp extract them to folders, do the operation, then
        // write out the results to a txt file, then do the same for the scanner, then diff them.
        internal static FMDarkVersion MissionIsOldDark(FanMission fm)
        {
            if (fm.Game is not Game.Thief1 and not Game.Thief2) return FMDarkVersion.NotApplicable;

            // Return "not detected" because if we're T1/T2 we're always supposed to succeed in finding at least
            // one .mis file, so we don't want to set it to "don't try again"
            if (!TryGetSmallestUsedMisFile(fm, out string misFile)) return FMDarkVersion.NotDetected;

            try
            {
                const int bufferSize = 81_920;

                byte[] SKYOBJVAR = Encoding.ASCII.GetBytes("SKYOBJVAR");

                using var br = new BinaryReader(File.OpenRead(misFile), Encoding.ASCII, leaveOpen: false);

                static bool TryReadFromLocation(BinaryReader br, int location, int count, out byte[] result)
                {
                    if (location + count > br.BaseStream.Length)
                    {
                        result = Array.Empty<byte>();
                        return false;
                    }

                    br.BaseStream.Position = location;
                    result = br.ReadBytes(count);

                    return true;
                }

                if (TryReadFromLocation(br, 772, 9, out byte[] buffer) &&
                    buffer.SequenceEqual(SKYOBJVAR))
                {
                    // SKYOBJVAR at byte 772: OldDark Thief 2
                    return FMDarkVersion.OldDark;
                }

                br.BaseStream.Position = 0;
                byte[] chunk = new byte[bufferSize + SKYOBJVAR.Length];
                if (!StreamContainsIdentString(br.BaseStream, SKYOBJVAR, chunk, bufferSize))
                {
                    // No SKYOBJVAR at all: OldDark Thief 1/Gold
                    return FMDarkVersion.OldDark;
                }

                // A very small number of T2 missions have SKYOBJVAR in a weird place, so we can't just say that
                // if we DID find that phrase that we're definitely NewDark. So check another byte location for
                // another string for robustness.
                return TryReadFromLocation(br, 580, 7, out buffer) &&
                       buffer.SequenceEqual(Encoding.ASCII.GetBytes("MAPISRC"))
                    ? FMDarkVersion.NewDark
                    : FMDarkVersion.OldDark;
            }
            catch (Exception ex)
            {
                Log(ex: ex);
                return FMDarkVersion.NotDetected;
            }
        }

        internal enum FMValueEnabled
        {
            Enabled,
            Disabled,
            Default
        };

        /*
        @FM_CFG: We only need 2 values currently
        ... and they're both on/off. So get rid of the generalization attempt and just do it simple until such
        time as we need anything more complex.
        @FM_CFG(Automatic value set) notes:
        -We can detect if a mission is OldDark and have the option "disable new mantle on all OldDark missions"
         (with manual override option per-FM), also we can detect if a mission needs the palette fix by checking
         OldDark and then checking pal\*pal.pcx existence (not confirmed this is reliable, just a working theory!)
        -To detect OldDark, we can use this heuristic/fingerprinting:
        -Find MAPISRC at byte 580 in first used .mis file. If present there, the mission should be NewDark.
         Otherwise OldDark.
        -To be more robust, we can also check if SKYOBJVAR is at 772 or is not in the file at all (both of these
         cases signify OldDark). Normally we wouldn't do this during a scan because we would have to search the
         entire .mis file, but here, we can just defer until the user goes to play an FM, and then:
        -If FM has no value in the NewDark field (true, false, n/a (T3)) then do the .mis scan from installed
         dir. Even for large .mis files, this won't take too much time to do once, and especially when playing
         a game since that's a "slow" operation anyway. Then, we store the NewDark value in the FM so we don't
         have to detect again.
        -Note! MAPISRC check only works for T1/T2. SS2 doesn't have it at all. We could try to use MAPPARAM or
         SKYOBJVAR position for SS2 FMs, or we could just not support this feature for them for now.
        */
        public sealed class FMKeyValue
        {
            public readonly string Key;
            public readonly FMValueEnabled Value;

            public FMKeyValue(string key, FMValueEnabled value)
            {
                Key = key;
                Value = value;
            }
        }

        internal static void SetPerFMValues(FanMission fm, params FMKeyValue[] keys)
        {
            if (!GameIsDark(fm.Game) || !FMIsReallyInstalled(fm)) return;

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            string fmCfgFile = Path.Combine(
                Config.GetFMInstallPath(gameIndex),
                fm.InstalledDir,
                Paths.FMCfg
            );

            List<string>? lines;

            if (File.Exists(fmCfgFile))
            {
                if (!TryReadAllLines(fmCfgFile, out lines))
                {
                    return;
                }
            }
            else
            {
                lines = new List<string>();
            }

            /*
            @FM_CFG: Temp, store actual valid-and-supported key-values later
            We're going to try storing the per-FM values in the fm.cfg file itself to save space in the db.
            This might turn out not to be the right decision but we're just gonna try and see how it works.
            Use a dict so as to de-dupe multiple keys and use only the latest in the file (that's how NewDark
            takes the value).
            -After further thought: Storing the value in fm.cfg only really works if we're on backup-all-files
             mode. Otherwise we lose it if we uninstall and reinstall. So we should definitely store in the db.
            -BUT! We still need to read from the file to get back any stored values that ARE there.
             And I guess we need to keep the two places in sync... Bleh...
             Otherwise we try and make the fm.cfg change temporary but then that's a whole other can of worms
             with tracking the game process exit, changing on proc exit/app exit/dir change, trying not to
             modify a file being used by someone else, etc...
            -UNLESS we just always update the fm.cfg file with our db-stored values, overwriting even if we just
             got the file from a backup restore. Because we're only overwriting the AL-specific section at the
             end of the file, that should be fine.
            */
            var oldLines = new List<string>();

            const string alSectionHeader = ";[AngelLoader]";

            /*
            -We should also use this as an opportunity to implement a robust failsafe system for file writes.
             Use a temp file and copy and check for fail and the whole deal.
            */

            for (int i = 0; i < lines.Count; i++)
            {
                string lt = lines[i].Trim();
                if (lt.Length > 0 && lt[0] == ';' && (";" + RemoveLeadingSemicolons(lt)) == alSectionHeader)
                {
                    // Remove actual header line (so we don't add it to the old lines list)
                    lines.RemoveAt(i);
                    i--;

                    for (int j = i; j < lines.Count; j++)
                    {
                        string lt2 = RemoveLeadingSemicolons(lines[i].Trim());
                        if (!lt2.IsEmpty()) oldLines.Add(lines[j]);
                        lines.RemoveAt(j);
                        j--;
                        i--;
                    }
                }
            }

            lines.Add("");
            lines.Add(alSectionHeader);

            foreach (var item in keys)
            {
                if (item.Value != FMValueEnabled.Default)
                {
                    lines.Add(item.Key + " " + (item.Value == FMValueEnabled.Enabled ? "1" : "0"));
                }
            }

            TryWriteAllLines(fmCfgFile, lines);
        }

#endif

        internal static (bool Success, List<Mod>)
        GetGameMods(GameIndex gameIndex)
        {
            var list = new List<Mod>();

            if (!GameSupportsMods(gameIndex)) return (false, list);

            string gamePath = Config.GetGamePath(gameIndex);

            if (gamePath.IsEmpty()) return (false, list);

            if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
            {
                return (false, list);
            }

            if (!TryReadAllLines(camModIni, out var lines))
            {
                // @BetterErrors(GetGameMods): Should we show the dialog?
                //Dialogs.ShowError(nameof(GetGameMods) + "():" +
                //                  "Couldn't read " + camModIni + "\r\n" +
                //                  "Game: " + gameIndex);
                return (false, list);
            }

            int modPathLastIndex = -1;
            int uberModPathLastIndex = -1;
            int mpModPathLastIndex = -1;
            int mpUberModPathLastIndex = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                string lineT = lines[i].Trim();

                if (lineT.IsEmpty() || lineT[0] == ';') continue;

                if (lineT.StartsWithI(mod_path))
                {
                    modPathLastIndex = i;
                }
                else if (lineT.StartsWithI(uber_mod_path))
                {
                    uberModPathLastIndex = i;
                }
                else if (lineT.StartsWithI(mp_mod_path))
                {
                    mpModPathLastIndex = i;
                }
                else if (lineT.StartsWithI(mp_u_mod_path))
                {
                    mpUberModPathLastIndex = i;
                }
            }

            static List<string>
            GetModPaths(List<string> lines, int lastIndex, string pathKey)
            {
                return lastIndex > -1
                    ? lines[lastIndex].Substring(pathKey.Length).Trim()
                        .Split(CA_Plus, StringSplitOptions.RemoveEmptyEntries)
                        .Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                    : new List<string>();
            }

            // Keeps the item in the hash set and removes it from the list if there are duplicates
            static void DeDupe(HashSetI hashSet, List<string> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (hashSet.Contains(list[i]))
                    {
                        list.RemoveAt(i);
                        i--;
                    }
                }
            }

            var modPaths = GetModPaths(lines, modPathLastIndex, mod_path);
            var uberModPaths = GetModPaths(lines, uberModPathLastIndex, uber_mod_path);
            var mpModPaths = GetModPaths(lines, mpModPathLastIndex, mp_mod_path);
            var mpUberModPaths = GetModPaths(lines, mpUberModPathLastIndex, mp_u_mod_path);

            var modPathsHash = modPaths.ToHashSetI();
            var uberModPathsHash = uberModPaths.ToHashSetI();

            DeDupe(uberModPathsHash, mpUberModPaths);
            DeDupe(uberModPathsHash, modPaths);
            DeDupe(uberModPathsHash, mpModPaths);
            DeDupe(modPathsHash, mpModPaths);

            foreach (var modPath in modPaths) list.Add(new Mod(modPath, ModType.ModPath));
            foreach (var modPath in uberModPaths) list.Add(new Mod(modPath, ModType.UberModPath));
            foreach (var modPath in mpModPaths) list.Add(new Mod(modPath, ModType.MPModPath));
            foreach (var modPath in mpUberModPaths) list.Add(new Mod(modPath, ModType.MPUberModPath));

            return (true, list);
        }

        internal static bool GameHasDarkLoaderFMInstalled(GameIndex gameIndex)
        {
            // DarkLoader only supports T1/T2/SS2
            if (!GameIsDark(gameIndex)) return false;

            string gamePath = Config.GetGamePath(gameIndex);
            if (!gamePath.IsEmpty() &&
                TryCombineFilePathAndCheckExistence(gamePath, Paths.DarkLoaderDotCurrent, out string dlFile))
            {
                try
                {
                    using var sr = new StreamReader(dlFile);
                    string? line1 = sr.ReadLine();
                    string? line2 = sr.ReadLine();
                    string? line3 = sr.ReadLine();
                    if (line1 != null &&
                        line2 != null &&
                        line1.Trim() != "Original" &&
                        line2.Trim() != "-1" &&
                        line3 != null)
                    {
                        return true;
                    }
                }
                catch
                {
                    // ignore
                    return false;
                }
            }

            return false;
        }

        #region Helpers

        private static string RemoveLeadingSemicolons(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == ';' || char.IsWhiteSpace(c)) continue;
                return line.Substring(i);
            }

            return line;
        }

        #endregion
    }
}

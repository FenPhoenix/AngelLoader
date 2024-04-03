using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

/*
@BetterErrors(GameConfigFiles):
Go through this whole thing and check what we want to do for each error. Some of these should maybe remain
silent because they have fallbacks, like if we can't set ourselves as the loader then the user will get a
dialog about it if they try to start the game executable by itself, and maybe we don't want to bother the
user for that? Then again, it's an unexpected situation that shouldn't happen often if at all, so maybe we
should alert the user.
*/

internal static class GameConfigFiles
{
    #region Constants

    // cam_mod.ini
    private const string key_fm_selector = "fm_selector";
    private const string key_fm = "fm";
    private const string key_fm_path = "fm_path";
    private const string mod_path = "mod_path";
    private const string uber_mod_path = "uber_mod_path";
    private const string mp_mod_path = "mp_mod_path";
    private const string mp_u_mod_path = "mp_u_mod_path";

    // cam_mod.ini, cam.cfg
    private const string key_fm_language = "fm_language";
    private const string key_fm_language_forced = "fm_language_forced";

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

    #region Read info from game configs

    // @CAN_RUN_BEFORE_VIEW_INIT
    internal static (string FMsPath, string FMLanguage, bool FMLanguageForced,
                     List<string> FMSelectorLines, bool AlwaysShowLoader, List<string>? AllLines)
    GetInfoFromCamModIni(string gamePath, bool langOnly, bool returnAllLines)
    {
        static string CreateAndReturnFMsPath(string gamePath)
        {
            string fmsPath = Path.Combine(gamePath, "FMs");
            try
            {
                Directory.CreateDirectory(fmsPath);
            }
            catch (Exception ex)
            {
                // @BetterErrors(GetInfoFromCamModIni()/CreateAndReturnFMsPath())
                // But return an error code, don't put up a dialog here! (thread safety)
                Log(ErrorText.ExCreate + "FM installed base dir", ex);
            }

            return fmsPath;
        }

        var fmSelectorLines = new List<string>();
        bool alwaysShowLoader = false;

        // @BetterErrors: Throw up dialog if not found, cause that means we're OldDark or broken.
        if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
        {
            return (!langOnly ? CreateAndReturnFMsPath(gamePath) : "", "", false, fmSelectorLines, false, null);
        }

        string path = "";
        string fm_language = "";
        bool fm_language_forced = false;

        List<string>? retLines = returnAllLines ? new List<string>() : null;

        /*
        TODO: Convert this to ReadAllLines in advance style like everything else
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
        using (var sr = new StreamReaderCustom.SRC_Wrapper(File.OpenRead(camModIni), new StreamReaderCustom()))
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
            while (sr.Reader.ReadLine() is { } line)
            {
                retLines?.Add(line);

                if (line.IsEmpty()) continue;

                string lineTS = line.TrimStart();

                // Quick check; these lines will be checked more thoroughly when we go to use them
                if (!langOnly && lineTS.ContainsI(key_fm_selector)) fmSelectorLines.Add(lineTS);
                if (!langOnly && lineTS.Trim().EqualsI(key_fm)) alwaysShowLoader = true;

                if (lineTS.IsEmpty() || lineTS[0] == ';') continue;

                if (!langOnly && lineTS.StartsWithIPlusWhiteSpace(key_fm_path))
                {
                    path = lineTS.Substring(key_fm_path.Length).Trim();
                }
                else if (lineTS.StartsWithIPlusWhiteSpace(key_fm_language))
                {
                    fm_language = lineTS.Substring(key_fm_language.Length).Trim();
                }
                else if (lineTS.StartsWithI(key_fm_language_forced))
                {
                    if (lineTS.Trim().Length == key_fm_language_forced.Length)
                    {
                        fm_language_forced = true;
                    }
                    else if (char.IsWhiteSpace(lineTS[key_fm_language_forced.Length]))
                    {
                        fm_language_forced = lineTS.Substring(key_fm_language_forced.Length).Trim() != "0";
                    }
                }
            }
        }

        if (langOnly)
        {
            return ("", fm_language, fm_language_forced, new List<string>(), false, retLines);
        }

        if (PathIsRelative(path))
        {
            try
            {
                path = RelativeToAbsolute(gamePath, path);
            }
            catch
            {
                return (CreateAndReturnFMsPath(gamePath), fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader, retLines);
            }
        }

        return (Directory.Exists(path) ? path : CreateAndReturnFMsPath(gamePath),
            fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader, retLines);
    }

    // @CAN_RUN_BEFORE_VIEW_INIT
    internal static (Error Error, bool UseCentralSaves, string FMInstallPath,
                     string PrevFMSelectorValue, bool AlwaysShowLoader, bool GamePathNeedsWriteCheck)
    GetInfoFromSneakyOptionsIni()
    {
        (string soIni, bool isPortable) = Paths.GetSneakyOptionsIni();
        if (soIni.IsEmpty())
        {
            return (Error.SneakyOptionsNotFound, false, "", "", false, isPortable);
        }

        bool ignoreSavesKeyFound = false;
        bool ignoreSavesKey = true;

        bool fmInstPathFound = false;
        string fmInstPath = "";

        bool externSelectorFound = false;
        string prevFMSelectorValue = "";

        bool alwaysShowLoaderFound = false;
        bool alwaysShowLoader = false;

        if (!TryReadAllLines(soIni, out List<string>? lines))
        {
            return (Error.GeneralSneakyOptionsIniError, false, "", "", false, isPortable);
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
                    else if (lt.IsIniHeader())
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

        if (isPortable && PathIsRelative(fmInstPath))
        {
            try
            {
                string? soIniDir = Path.GetDirectoryName(soIni);
                if (soIniDir != null)
                {
                    fmInstPath = RelativeToAbsolute(soIniDir, fmInstPath);
                    Directory.CreateDirectory(fmInstPath);
                }
            }
            catch (Exception ex)
            {
                Log("Unable to resolve the relative path " + fmInstPath + " with the absolute path where " + soIni + " is contained. Returning blank path.", ex);
                fmInstPath = "";
                fmInstPathFound = false;
            }
        }

        return fmInstPathFound
            ? (Error.None, !ignoreSavesKey, fmInstPath, prevFMSelectorValue, alwaysShowLoader, isPortable)
            : (Error.GeneralSneakyOptionsIniError, false, "", prevFMSelectorValue, alwaysShowLoader, isPortable);
    }

    #endregion

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
    internal static void ResetGameConfigTempChanges(PerGameGoFlags perGameGoFlags)
    {
        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            string gameExe = Config.GetGameExe(gameIndex);
            try
            {
                // @GENGAMES(Reset configs): Make sure the logic is correct here!
                // Twice now we've had the Thief 3 path running multiple times due to logic bugs or forgetting
                // about this spot.
                if (perGameGoFlags[i] &&
                    // Only try to un-stomp the configs for the game if the game was actually specified
                    !gameExe.IsWhiteSpace())
                {
                    if (GameIsDark(gameIndex))
                    {
                        // For Dark, we need to know if the game exe itself actually exists.
                        if (File.Exists(gameExe))
                        {
                            string gamePath = Config.GetGamePath(gameIndex);
                            SetCamCfgLanguage(gamePath, "");
                            SetDarkFMSelector(gameIndex, gamePath, resetSelector: true);
                            FixCharacterDetailLine(gameIndex);
                        }
                    }
                    else if (gameIndex == GameIndex.Thief3)
                    {
                        // For Thief 3, we actually just need SneakyOptions.ini. The game itself existing
                        // is not technically a requirement.
                        SetT3FMSelector(out _, resetSelector: true);
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

    /*
    @BetterErrors(FixCharacterDetailLineInCamCfg)
    The bug:
    Ascend the Dim Valley has "character_detail 0" in its fm.cfg file. This file is supposed to contain
    values that ONLY apply to that specific FM. But, NewDark writes the character_detail value back out
    to the global cam.cfg after reading it from the FM-specific fm.cfg, causing the value to persist
    permanently for other FMs, which causes broken texture UV on many models. The fix is to just force
    character_detail back to 1 in cam.cfg.
    We also force character_detail 1 in all other config files from which that value can be read and applied.
    Note: this option is written and read from Config.ini, but has no UI way to change it. If a user wants
    to change it they can change it in Config.ini and it will be honored.
    We don't want to allow UI changing because the option shouldn't be disabled pretty much ever under
    normal circumstances.
    @CAN_RUN_BEFORE_VIEW_INIT
    */
    internal static void FixCharacterDetailLine(GameIndex gameIndex, List<string>? camModIniLines = null)
    {
        if (gameIndex is not GameIndex.Thief1 and not GameIndex.Thief2) return;

        if (!Config.EnableCharacterDetailFix) return;

        string gamePath = Config.GetGamePath(gameIndex);

        if (gamePath.IsEmpty()) return;

        Run(gamePath, Paths.CamCfg, removeAll: false, useDefaultEncoding: false);
        Run(gamePath, Paths.CamExtCfg, removeAll: true, useDefaultEncoding: false);
        Run(gamePath, Paths.CamModIni, removeAll: true, useDefaultEncoding: true, camModIniLines);

        return;

        static void Run(
            string gamePath,
            string fileName,
            bool removeAll,
            bool useDefaultEncoding,
            List<string>? fileLines = null)
        {
            if (!TryCombineFilePathAndCheckExistence(gamePath, fileName, out string cfgFile))
            {
                return;
            }

            List<string>? lines;
            if (fileLines == null)
            {
                if (!(useDefaultEncoding
                    ? TryReadAllLines_DefaultEncoding(cfgFile, out lines)
                    : TryReadAllLines(cfgFile, out lines)))
                {
                    return;
                }
            }
            else
            {
                // We're modifying the list, so deep copy it
                lines = new List<string>(fileLines.Count);
                lines.AddRange(fileLines);
            }

            bool atLeastOneCharacterDetailOneLineFound = false;
            bool linesModified = false;

            for (int i = 0; i < lines.Count; i++)
            {
                ReadOnlySpan<char> lineTS = lines[i].AsSpan().TrimStart();
                if (lineTS.StartsWithI(key_character_detail))
                {
                    ReadOnlySpan<char> val = lineTS[key_character_detail.Length..].Trim();
                    // IMPORTANT: is instead of == because of span comparison shenanigans ('is "0"' is the same as 'SequenceEqual("0")')
                    // https://steven-giesel.com/blogPost/969cc5e7-da27-4742-ae9a-ab7a66715ff6
                    if (removeAll || val is "0")
                    {
                        lines.RemoveAt(i);
                        i--;
                        linesModified = true;
                    }
                    else if (val is "1")
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

            bool writeSuccess = useDefaultEncoding
                ? TryWriteAllLines_DefaultEncoding(cfgFile, lines, out _)
                : TryWriteAllLines(cfgFile, lines, out _);

            if (!writeSuccess)
            {
                // ReSharper disable once RedundantJumpStatement
                return; // Explicit for clarity of intent
            }
        }
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

        if (!TryReadAllLines(camCfg, out List<string>? lines))
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

        if (!TryWriteAllLines(camCfg, lines, out _))
        {
            // ReSharper disable once RedundantJumpStatement
            return; // Explicit for clarity of intent
        }
    }

    #region Set selectors

    // @CAN_RUN_BEFORE_VIEW_INIT
    // TODO(SetDarkFMSelector): Rewrite this mess into something readable!
    internal static (bool Success, Exception? Ex)
    SetDarkFMSelector(GameIndex gameIndex, string gamePath, bool resetSelector = false)
    {
        (bool, Exception?) failNoEx = (false, null);

        if (!GameIsDark(gameIndex)) return failNoEx;

        if (gamePath.IsEmpty()) return failNoEx;

        #region Local functions

        static string FindPreviousSelector(List<string> lines, string stubPath, string gamePath)
        {
            static string GetFullPath(string gamePath, string path)
            {
                if (PathIsRelative(path))
                {
                    try
                    {
                        return RelativeToAbsolute(gamePath, path);
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
                           (selectorFileName = line.Substring(key_fm_selector.Length + 1)).EndsWithI(".dll") &&
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
                selectorsList.Count > 0 ? selectorsList[^1] :
                commentedSelectorsList.Count > 0 ? commentedSelectorsList[^1] :
                Paths.FMSelDll;
        }

        #endregion

        const string fmCommentLine = "always start the FM Selector (if one is present)";

        if (!TryCombineFilePathAndCheckExistence(gamePath, Paths.CamModIni, out string camModIni))
        {
            // The above try-combine thing won't log any exceptions, so let's log this one ourselves.
            Log(Paths.CamModIni + " not found, or game path not found, or invalid game path.\r\n" +
                "Game path: " + gamePath + "\r\n" +
                "Game: " + gameIndex,
                stackTrace: true
            );
            return failNoEx;
        }

        if (!TryReadAllLines_DefaultEncoding(camModIni, out List<string>? lines))
        {
            return failNoEx;
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
                    (selectorFileName = lt.Substring(key_fm_selector.Length + 1)).EndsWithI(".dll"))
                {
                    if (!tempSelectorsList.PathContainsI(selectorFileName)) tempSelectorsList.Add(selectorFileName);
                }
            }

            if (tempSelectorsList.Count > 0 &&
                !tempSelectorsList[^1].PathEqualsI(Paths.StubPath))
            {
                changeLoaderIfResetting = false;
            }
        }

        // Confirmed NewDark can read fm_selector values with both forward and backward slashes

        // The loader is us, so use our saved previous loader or lacking that, make a best-effort guess
        List<string> startupFMSelectorLines = Config.GetStartupFMSelectorLines(gameIndex);
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

            bool lineIsCommented = lt.Length > 0 && lt[0] == ';';

            lt = RemoveLeadingSemicolons(lt);

            // Steam robustness: get rid of any fan mission specifiers in here
            // line is "fm BrokenTriad_1_0" for example
            if (lt.StartsWithIPlusWhiteSpace(key_fm) &&
                lt.Substring(2).Trim().Length > 0)
            {
                if (!lineIsCommented) lines[i] = ";" + lines[i];
            }

            if (fmCommentLineIndex == -1 && lt.EqualsI(fmCommentLine)) fmCommentLineIndex = i;

            if (fmLineLastIndex == -1 && lt.EqualsI(key_fm))
            {
                if (!resetSelector)
                {
                    if (lineIsCommented) lines[i] = key_fm;
                }
                else
                {
                    if (prevAlwaysLoadSelector)
                    {
                        if (lineIsCommented) lines[i] = key_fm;
                    }
                    else
                    {
                        if (!lineIsCommented) lines[i] = ";" + key_fm;
                    }
                }
                fmLineLastIndex = i;
            }
            if (!resetSelector || changeLoaderIfResetting)
            {
                if (lt.StartsWithIPlusWhiteSpace(key_fm_selector) &&
                    lt.Substring(key_fm_selector.Length + 1).TrimStart().PathEqualsI(selectorPath))
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
                    if (!lineIsCommented) lines[i] = ";" + lines[i];
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

        bool success = TryWriteAllLines_DefaultEncoding(camModIni, lines, out Exception? ex);
        return (success, ex);
    }

    // @CAN_RUN_BEFORE_VIEW_INIT
    internal static (bool Success, Exception? Ex)
    SetT3FMSelector(out bool suIsPortable, bool resetSelector = false)
    {
        (bool, Exception?) failNoEx = (false, null);

        bool existingExternSelectorKeyOverwritten = false;
        bool existingAlwaysShowKeyOverwritten = false;
        int insertLineIndex = -1;

        (string soIni, suIsPortable) = Paths.GetSneakyOptionsIni();
        if (soIni.IsEmpty())
        {
            return failNoEx;
        }

        if (!TryReadAllLines(soIni, out List<string>? lines))
        {
            return failNoEx;
        }

        // Confirmed SU can read the selector value with both forward and backward slashes

        string selectorPath;

        bool changeLoaderIfResetting = true;

        #region Reset loader

        // TODO: @CourteousBehavior: Save and restore the "always start with FM" line(s)
        // Probably nobody uses this feature, but maybe we should do it for completeness?
        if (resetSelector)
        {
            List<string> startupFMSelectorLines = Config.GetStartupFMSelectorLines(GameIndex.Thief3);
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
                    else if (lt.IsIniHeader())
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

                if (lt.IsIniHeader()) break;

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

        bool success = TryWriteAllLines(soIni, lines, out Exception? ex);
        return (success, ex);
    }

    #endregion

    #region Mods

    internal static (bool Success, List<Mod> Mods)
    GetGameMods(List<string> lines)
    {
        var list = new List<Mod>();

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

        List<string> modPaths = GetModPaths(lines, modPathLastIndex, mod_path);
        List<string> uberModPaths = GetModPaths(lines, uberModPathLastIndex, uber_mod_path);
        List<string> mpModPaths = GetModPaths(lines, mpModPathLastIndex, mp_mod_path);
        List<string> mpUberModPaths = GetModPaths(lines, mpUberModPathLastIndex, mp_u_mod_path);

        HashSetI modPathsHash = modPaths.ToHashSetI();
        HashSetI uberModPathsHash = uberModPaths.ToHashSetI();

        DeDupe(uberModPathsHash, mpUberModPaths);
        DeDupe(uberModPathsHash, modPaths);
        DeDupe(uberModPathsHash, mpModPaths);
        DeDupe(modPathsHash, mpModPaths);

        foreach (string modPath in modPaths)
        {
            list.Add(new Mod(modPath, ModType.ModPath));
        }
        foreach (string modPath in uberModPaths)
        {
            list.Add(new Mod(modPath, ModType.UberModPath));
        }
        foreach (string modPath in mpModPaths)
        {
            list.Add(new Mod(modPath, ModType.MPModPath));
        }
        foreach (string modPath in mpUberModPaths)
        {
            list.Add(new Mod(modPath, ModType.MPUberModPath));
        }

        return (true, list);

        static List<string> GetModPaths(List<string> lines, int lastIndex, string pathKey)
        {
            return lastIndex > -1
                ? lines[lastIndex].Substring(pathKey.Length).Trim()
                    .Split('+', StringSplitOptions.RemoveEmptyEntries)
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
    }

    private static bool ModDirExists(string fullPath)
    {
        if (Directory.Exists(fullPath)) return true;

        var paths = new List<string>();

        // Paranoid fallback exit condition
        int dirSepCount = fullPath.Rel_CountDirSeps() + 5;
        for (int i = 0; i < dirSepCount; i++)
        {
            paths.Insert(0, fullPath.TrimEnd(CA_BS_FS));

            try
            {
                string? cutPath = Path.GetDirectoryName(fullPath);
                if (cutPath.IsEmpty() || cutPath.EqualsI(paths[0]))
                {
                    break;
                }
                fullPath = cutPath;
            }
            catch
            {
                break;
            }
        }

        if (paths.Count > 0)
        {
            paths.RemoveAt(0);
        }

        if (paths.Count < 2) return false;

        for (int i = 1; i < paths.Count; i++)
        {
            string p = paths[i];
            /*
            Confirmed, the game only reads mods from .zip and .crf files (at least no other extensions I tried worked)

            Also, don't read the zip file entries or anything, just assume it contains the path, as that's the
            most probable case... (should we change this later?)
            */
            if (File.Exists(p + ".zip") ||
                File.Exists(p + ".crf"))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool ModExistsOnDisk(string gamePath, string modName)
    {
        /*
        @Mods: Notes for supporting directory-in-zip mods:
        -The game only supports zip files for mods, not 7z, so no need to deal with that.
        -The game supports zipped mods within zipped mods, so we'll have to be able to recursively open
         zip files within zip files... Maybe put a cap on it to prevent malicious zips. And/or put up a
         message box saying "Hey, you're doing something silly that will make startup slow".
        -We should abstract the zip query so we can treat it just like an on-disk query.
        -But we could cache the entries list(s) in case we need to query it/them more than once.
        */

        static string PathCombineRelativeSupport(string path1, string path2)
        {
            return PathIsRelative(path2) ? RelativeToAbsolute(path1, path2) : Path.Combine(path1, path2);
        }

        string fullPath;
        try
        {
            fullPath = PathCombineRelativeSupport(gamePath, modName);
        }
        catch
        {
            return false;
        }

        if (ModDirExists(fullPath))
        {
            return true;
        }

        string modContainingDir;
        try
        {
            modContainingDir = Path.GetDirectoryName(fullPath) ?? gamePath;
        }
        catch
        {
            return false;
        }

        try
        {
            if (Directory.Exists(modContainingDir))
            {
                string modFileNameOnly = modName.GetFileNameFast();
                if (modFileNameOnly.IsEmpty()) return false;
                List<string> modFiles = FastIO.GetFilesTopOnly(modContainingDir, modFileNameOnly + ".*");
                return modFiles.Count > 0;
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

    #endregion

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
                return false;
            }
        }

        return false;
    }

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

    private static bool GetFileAndLines(GameIndex gameIndex, string file, out string filePath, [NotNullWhen(true)] out List<string>? lines)
    {
        filePath = "";
        lines = null;

        return GameIsDark(gameIndex) &&
               TryGetGameDirFilePathIfExists(gameIndex, file, out filePath) &&
               TryReadAllLines(filePath, out lines);
    }

    private static readonly char[] _ca_Space_Tab_Semicolon = { ' ', '\t', ';' };
    internal static bool? GetScreenShotMode(GameIndex gameIndex)
    {
        if (!GetFileAndLines(gameIndex, Paths.UserCfg, out _, out List<string>? lines))
        {
            return null;
        }

        bool ret = false;

        for (int i = 0; i < lines.Count; i++)
        {
            string lt = lines[i].Trim();
            if (lt.StartsWithIPlusWhiteSpace(key_inv_status_height))
            {
                string[] fields = lt.Split(_ca_Space_Tab_Semicolon, StringSplitOptions.RemoveEmptyEntries);
                ret = fields.Length >= 2 &&
                      Int_TryParseInv(fields[1], out int result) &&
                      result == 0;
            }
        }

        return ret;
    }

    internal static void SetScreenShotMode(GameIndex gameIndex, bool enabled)
    {
        if (!GetFileAndLines(gameIndex, Paths.UserCfg, out string userCfgFile, out List<string>? lines))
        {
            return;
        }

        RemoveKeyLine(key_inv_status_height, lines);

        lines.Insert(0, (enabled ? "" : ";") + key_inv_status_height + " 0 ; Added by AngelLoader: uncommented = screenshot mode enabled (no hud)");
        lines.Insert(1, "");

        RemoveConsecutiveWhiteSpace(lines);

        TryWriteAllLines(userCfgFile, lines, out _);
    }

    internal static bool? GetTitaniumMode(GameIndex gameIndex)
    {
        if (!GetFileAndLines(gameIndex, Paths.UserBnd, out _, out List<string>? lines))
        {
            return null;
        }

        bool quickLoadEnabled = false;
        bool quickSaveEnabled = false;
        bool unstickPlayerEnabled = false;

        for (int i = 0; i < lines.Count; i++)
        {
            string lineT = lines[i].Trim();
            if (lineT.Length > 0 && lineT[0] != ';')
            {
                if (lineT.ContainsI(" quick_load"))
                {
                    quickLoadEnabled = true;
                }
                else if (lineT.ContainsI(" quick_save"))
                {
                    quickSaveEnabled = true;
                }
                else if (lineT.ContainsI(" unstick_player"))
                {
                    unstickPlayerEnabled = true;
                }
            }
        }

        return !quickLoadEnabled && !quickSaveEnabled && !unstickPlayerEnabled;
    }

    internal static void SetTitaniumMode(GameIndex gameIndex, bool enabled)
    {
        if (!GetFileAndLines(gameIndex, Paths.UserBnd, out string userBndFile, out List<string>? lines))
        {
            return;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            string lineT = lines[i].Trim();
            if (lineT.ContainsI(" quick_load") ||
                lineT.ContainsI(" quick_save") ||
                lineT.ContainsI(" unstick_player"))
            {
                lines[i] = (enabled ? ";" : "") + RemoveLeadingSemicolons(lineT);
            }
        }

        TryWriteAllLines(userBndFile, lines, out _);
    }

    internal static void SetGlobalDarkGameValues(GameIndex gameIndex)
    {
        if (!GameIsDark(gameIndex)) return;
        SetResolution(gameIndex);
    }

    private static void SetResolution(GameIndex gameIndex)
    {
        if (!Config.ForceGameResToMainMonitorRes) return;
        if (!GetFileAndLines(gameIndex, Paths.CamCfg, out string camCfgFile, out List<string>? lines))
        {
            return;
        }

        RemoveKeyLine(key_game_screen_size, lines);

        System.Windows.Forms.Screen? screen = System.Windows.Forms.Screen.PrimaryScreen;
        if (screen != null)
        {
            System.Drawing.Rectangle res = screen.Bounds;
            lines.Add(key_game_screen_size + " " + res.Width.ToStrInv() + " " + res.Height.ToStrInv());

            RemoveConsecutiveWhiteSpace(lines);

            TryWriteAllLines(camCfgFile, lines, out _);
        }
    }

#endif

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

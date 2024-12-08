//#define TIMING_TEST

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using AL_Common.FastZipReader;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers.Rar;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

internal static partial class FMInstallAndPlay
{
#if TIMING_TEST
    private static readonly Stopwatch _timingTestStopWatch = new();

    private static void StartTiming()
    {
        _timingTestStopWatch.Restart();
    }

    private static void StopTimingAndPrintResult(string msg)
    {
        _timingTestStopWatch.Stop();
        Trace.WriteLine(msg + ": " + _timingTestStopWatch.Elapsed);
    }
#endif

    #region Private fields

    private enum InstallResultType
    {
        InstallSucceeded,
        InstallFailed,
        RollbackSucceeded,
        RollbackFailed,
    }

    private enum UninstallResultType
    {
        UninstallSucceeded,
        UninstallFailed,
    }

    private enum ArchiveType
    {
        Zip,
        Rar,
        SevenZip,
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct FMInstallResult
    {
        internal readonly FMData FMData;
        internal readonly InstallResultType ResultType;
        internal readonly ArchiveType ArchiveType;
        internal readonly string ErrorMessage;
        internal readonly Exception? Exception;

        public FMInstallResult(FMData fmData, InstallResultType resultType)
        {
            FMData = fmData;
            ResultType = resultType;
            ErrorMessage = "";
        }

        public FMInstallResult(FMData fmData, InstallResultType resultType, ArchiveType archiveType, string errorMessage)
        {
            FMData = fmData;
            ResultType = resultType;
            ArchiveType = archiveType;
            ErrorMessage = errorMessage;
        }

        public FMInstallResult(FMData fmData, InstallResultType resultType, ArchiveType archiveType, string errorMessage, Exception? exception)
        {
            FMData = fmData;
            ResultType = resultType;
            ArchiveType = archiveType;
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct FMUninstallResult
    {
        internal readonly UninstallResultType ResultType;
        internal readonly string ErrorMessage;
        internal readonly Exception? Exception;

        public FMUninstallResult(UninstallResultType resultType)
        {
            ResultType = resultType;
            ErrorMessage = "";
        }

        public FMUninstallResult(UninstallResultType resultType, string errorMessage, Exception? exception)
        {
            ResultType = resultType;
            ErrorMessage = errorMessage;
            Exception = exception;
        }
    }

    private enum PlaySource
    {
        OriginalGame,
        Editor,
        FM,
    }

    private static readonly byte[] _DARKMISS_Bytes = "DARKMISS"u8.ToArray();

    // Immediately static init for thread safety
    private static readonly Encoding UTF8NoBOM = new UTF8Encoding(false, true);

    private static CancellationTokenSource _installCts = new();

    private static void CancelInstallToken()
    {
        _installCts.CancelIfNotDisposed();
        Core.View.Invoke(ShowCancelingInstallMessage);
    }

    private static void ShowCancelingInstallMessage()
    {
        if (Core.View.ProgressBoxVisible())
        {
            Core.View.SetProgressBoxState(mainMessage1: LText.ProgressBox.CancelingInstall);
        }
    }

    private static CancellationTokenSource _uninstallCts = new();
    private static void CancelUninstallToken() => _uninstallCts.CancelIfNotDisposed();

    #endregion

    internal static async Task InstallOrUninstall(FanMission[] fms)
    {
        using var dsw = new DisableScreenshotWatchers();

        AssertR(fms.Length > 0, nameof(fms) + ".Length == 0");
        FanMission firstFM = fms[0];

        if (firstFM.Game == Game.TDM)
        {
            SelectTdmFM(firstFM, deselect: firstFM.Installed);
        }
        else if (firstFM.Installed)
        {
            await Uninstall(fms);
            Core.View.SetAvailableAndFinishedFMCount();
        }
        else
        {
            await Install(fms);
        }
    }

    private static bool SelectTdmFM(FanMission? fm, bool deselect = false)
    {
        try
        {
            string gameExe = Config.GetGameExe(GameIndex.TDM);
            if (gameExe.IsEmpty()) return false;
            if (GameIsRunning(gameExe))
            {
                Core.Dialogs.ShowAlert(LText.AlertMessages.SelectFM_DarkMod_GameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            string gamePath = Config.GetGamePath(GameIndex.TDM);
            if (gamePath.IsEmpty()) return false;

            string currentFMFile = Path.Combine(gamePath, Paths.TDMCurrentFMFile);
            if (deselect && fm != null) fm.Installed = false;
            using var sw = new StreamWriter(currentFMFile);
            // TDM doesn't write a newline, so let's match it
            sw.Write(!deselect && fm != null ? fm.TDMInstalledDir : "");
            Core.View.RefreshAllSelectedFMs_UpdateInstallState();
        }
        catch
        {
            Core.Dialogs.ShowAlert(LText.AlertMessages.SelectFM_DarkMod_UnableToSelect, LText.AlertMessages.Alert);
            return false;
        }

        return true;
    }

    internal static async Task InstallIfNeededAndPlay(FanMission fm, bool askConfIfRequired = false, bool playMP = false)
    {
        using var dsw = new DisableScreenshotWatchers();

        if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
        {
            fm.LogInfo(ErrorText.FMGameU, stackTrace: true);
            Core.Dialogs.ShowError(fm.GetId() + $"{NL}" + ErrorText.FMGameU);
            return;
        }

        if (playMP && gameIndex != GameIndex.Thief2)
        {
            fm.LogInfo("playMP was true, but fm.Game was not Thief 2.", stackTrace: true);
            Core.Dialogs.ShowError(ErrorText.MPForNonT2);
            return;
        }

        bool askingConfirmation = askConfIfRequired && Config.ConfirmPlayOnDCOrEnter;
        if (askingConfirmation)
        {
            string message = fm.Installed || fm.Game == Game.TDM
                ? LText.AlertMessages.Play_ConfirmMessage
                : LText.AlertMessages.Play_InstallAndPlayConfirmMessage;

            if (Core.View.GetMainSelectedFMOrNull() != fm)
            {
                message += $"{NL}{NL}" +
                           fm.DisplayArchive + $"{NL}" +
                           fm.Title + $"{NL}" +
                           fm.Author + $"{NL}";
            }

            (MBoxButton result, bool dontAskAgain) = Core.Dialogs.ShowMultiChoiceDialog(
                message: message,
                title: LText.AlertMessages.Confirm,
                icon: MBoxIcon.None,
                yes: LText.Global.PlayFM,
                no: LText.Global.Cancel,
                checkBoxText: LText.AlertMessages.DontAskAgain);

            if (result == MBoxButton.No) return;

            Config.ConfirmPlayOnDCOrEnter = !dontAskAgain;
        }

        if (fm.Installed ||
            (fm.Game == Game.TDM && SelectTdmFM(fm)) ||
            (fm.Game != Game.TDM && await InstallInternal(fromPlay: true, suppressConfirmation: askingConfirmation, fm)))
        {
            if (playMP && gameIndex == GameIndex.Thief2 && Core.GetT2MultiplayerExe_FromDisk().IsEmpty())
            {
                Log($"Thief2MP.exe not found in Thief 2 game directory.{NL}" +
                    "Thief 2 game directory: " + Config.GetGamePath(GameIndex.Thief2));
                Core.Dialogs.ShowError(LText.AlertMessages.Thief2_Multiplayer_ExecutableNotFound);
                return;
            }

            try
            {
                Core.View.SetWaitCursor(true);

                if (await PlayFM(fm, gameIndex, playMP))
                {
                    fm.LastPlayed.DateTime = DateTime.Now;
                    Core.View.RefreshFM(fm);
                    Ini.WriteFullFMDataIni();
                }
            }
            finally
            {
                Core.View.SetWaitCursor(false);
            }
        }
    }

    #region Play / open

    internal static bool PlayOriginalGame(GameIndex gameIndex, bool playMP = false)
    {
        using var dsw = new DisableScreenshotWatchers();

        try
        {
            Core.View.SetWaitCursor(true);

            (bool success, string gameExe, string gamePath) =
                CheckAndReturnFinalGameExeAndGamePath(gameIndex, playingOriginalGame: true, playMP);
            if (!success) return false;

            Paths.CreateOrClearTempPath(TempPaths.StubComm);

            GameConfigFiles.FixCharacterDetailLine(gameIndex);
#if !ReleaseBeta && !ReleasePublic
            GameConfigFiles.SetGlobalDarkGameValues(gameIndex);
#endif
            if (!SetUsAsSelector(gameIndex, gamePath, PlaySource.OriginalGame)) return false;

#if !ReleaseBeta && !ReleasePublic
            string args = Config.ForceWindowed ? "force_windowed=1" : "";
#else
            string args = "";
#endif
            string workingPath = Config.GetGamePath(gameIndex);
            var sv = GetSteamValues(gameIndex, playMP);
            if (sv.Success) (_, gameExe, workingPath, args) = sv;

            if (GameIsDark(gameIndex))
            {
                switch (Config.GetNewMantling(gameIndex))
                {
                    case true:
                        args += " new_mantle=1";
                        break;
                    case false:
                        args += " new_mantle=0";
                        break;
                }
            }

            if (!WriteStubCommFileForOriginalGame(
                    gamePath: gamePath,
                    gameIndex: gameIndex,
                    origDisabledMods: Config.GetDisabledMods(gameIndex)))
            {
                return false;
            }

            if (gameIndex == GameIndex.TDM)
            {
                SelectTdmFM(null, deselect: true);
            }

            if (!StartExeForPlayOriginalGame(
                    gameIndex,
                    gameExe,
                    workingPath,
                    args))
            {
                return false;
            }

            return true;
        }
        finally
        {
            Core.View.SetWaitCursor(false);
        }
    }

    private static async Task<bool> PlayFM(FanMission fm, GameIndex gameIndex, bool playMP = false)
    {
        using var dsw = new DisableScreenshotWatchers();

        (bool success, string gameExe, string gamePath) =
            CheckAndReturnFinalGameExeAndGamePath(gameIndex, playingOriginalGame: false, playMP);
        if (!success) return false;

        Paths.CreateOrClearTempPath(TempPaths.StubComm);

        GameConfigFiles.FixCharacterDetailLine(gameIndex);
#if !ReleaseBeta && !ReleasePublic
        GameConfigFiles.SetGlobalDarkGameValues(gameIndex);
#endif
        if (!SetUsAsSelector(gameIndex, gamePath, PlaySource.FM)) return false;

        string steamArgs = "";
        string steamExe = "";
        string workingPath = Config.GetGamePath(gameIndex);
        var sv = GetSteamValues(gameIndex, playMP);
        if (sv.Success) (_, steamExe, workingPath, steamArgs) = sv;

        /*
        BUG: Possible stub comm file not being deleted in the following scenario:
        You launch a game through Steam, but the game doesn't actually launch (because you don't have
        it in your Steam library or any other situation in which it gets cancelled). Because the game
        never runs, it never deletes the stub comm file. The next time the game runs, it finds the stub
        file and loads up whatever FM was specified. This won't happen if you launch an FM or original
        game from AngelLoader, as we delete or overwrite the stub file ourselves before playing anything,
        but if you were to run the game manually, it would load whatever FM was specified in the stub
        once, and then delete it, so if you ran it again it would properly start the original game and
        everything would be fine again.
        I could solve it if there was a way to detect if we were being launched through Steam. I don't
        know if there is, but then I could just specify a Steam=True line in the stub file, and then
        if we're being launched through Steam we read and act on it as usual, but if we're not, then
        we just delete it and ignore.
        I'll have to buy the games on Steam to test this. Or just buy one so I can have one game that
        works and one that doesn't, so I can test both paths.
        */

        string args =
            /*
            We set it in currentfm.txt beforehand. The reason we DON'T pass it as an arg is because then if you
            deselect the FM in-game, when it restarts, the FM will be selected again. Probably it passes the
            arguments it was started with to the next instance. This is probably not what the user wants, so
            let's avoid it.

            @TDM_NOTE: Once in a while the game starts with no FM even though there's one in currentfm.txt.
            I haven't been able to reliably reproduce this. Presumably it can't read the file for some reason.
            Passing args would bypass this file and guarantee it loads our FM, but then we get the above-described
            problem. If anyone else reports this we could go back to the arg passing, but eh...
            */
            gameIndex == GameIndex.TDM ? "" :
            !steamArgs.IsEmpty() ? steamArgs :
            "-fm";

        if (GameIsDark(fm.Game))
        {
            MissFlagError missFlagError = GenerateMissFlagFileIfRequired(fm, errorOnCantPlay: true);
            if (missFlagError != MissFlagError.None) return false;

#if !ReleaseBeta && !ReleasePublic
            if (Config.ForceWindowed)
            {
                args += " force_windowed=1";
            }
#endif

            // Do this AFTER generating missflag.str! Otherwise it will fail to correctly detect the first used
            // .mis file when detecting OldDark (if there is no missflag.str)!
            bool fmIsOldDark = FMIsOldDark(fm);

            if (fmIsOldDark && FMRequiresPaletteFix(fm, checkForOldDark: false))
            {
                args += " legacy_32bit_txtpal=1";
            }

            /*
            We can say +new_mantle to enable and -new_mantle to disable, HOWEVER, if we use that syntax, the
            following quirk occurs:

            If new_mantle is ENABLED in an FM's fm.cfg, then passing the game "-new_mantle" has no effect.
            If new_mantle is specified but DISABLED (new_mantle 0) in fm.cfg, passing the game "+new_mantle"
            DOES have the desired effect.

            So instead we use new_mantle=1 and new_mantle=0, which always override the fm.cfg value.

            Phew!
            */
            if (fm.NewMantle == true)
            {
                args += " new_mantle=1";
            }
            else if (fm.NewMantle == false)
            {
                args += " new_mantle=0";
            }
            else if (fmIsOldDark && Config.UseOldMantlingForOldDarkFMs)
            {
                args += " new_mantle=0";
            }

            if (fm.PostProc == true)
            {
                args += " postprocess=1";
            }
            else if (fm.PostProc == false)
            {
                args += " postprocess=0";
            }

            if (fm.NDSubs == true)
            {
                args += " enable_subtitles=1";
            }
            else if (fm.NDSubs == false)
            {
                args += " enable_subtitles=0";
            }
        }

        if (!RunThiefBuddyIfRequired(fm)) return false;

        if (!WriteStubCommFileForFM(fm, gamePath)) return false;

        if (!await StartExeForPlayFM(
                fm: fm,
                gameIndex: gameIndex,
                exe: sv.Success ? steamExe : gameExe,
                gameExe: gameExe,
                workingPath: workingPath,
                args: args,
                steam: sv.Success))
        {
            return false;
        }

        return true;
    }

    internal static bool OpenFMInEditor(FanMission fm)
    {
        using var dsw = new DisableScreenshotWatchers();

        try
        {
            Core.View.SetWaitCursor(true);

            #region Checks (specific to DromEd)

            // This should never happen because our menu item is supposed to be hidden for Thief 3 FMs.
            if (!fm.Game.ConvertsToDark(out GameIndex gameIndex))
            {
                fm.LogInfo(ErrorText.FMGameNotDark, stackTrace: true);
                Core.Dialogs.ShowError(ErrorText.FMGameNotDark);
                return false;
            }

            string gamePath = Config.GetGamePath(gameIndex);
            if (gamePath.IsEmpty())
            {
                Log(ErrorText.GamePathEmpty + $"{NL}" + gameIndex, stackTrace: true);
                Core.Dialogs.ShowError(gameIndex + $":{NL}" + ErrorText.GamePathEmpty);
                return false;
            }

            string editorExe = Core.GetEditorExe_FromDisk(gameIndex);
            if (editorExe.IsEmpty())
            {
                fm.LogInfo(
                    $"Editor executable not found.{NL}" +
                    "Editor executable: " + editorExe);
                Core.Dialogs.ShowError(fm.Game == Game.SS2
                    ? LText.AlertMessages.ShockEd_ExecutableNotFound
                    : LText.AlertMessages.DromEd_ExecutableNotFound);
                return false;
            }

            #endregion

            // Just in case, and for consistency
            Paths.CreateOrClearTempPath(TempPaths.StubComm);

            GameConfigFiles.FixCharacterDetailLine(gameIndex);

            // We don't need to do this here, right?
            // In testing, it seems that with our current logic as of 2022-07-24, we don't need to call this
            // in practice. Leave it in, but don't fail if it fails.
            _ = SetUsAsSelector(gameIndex, gamePath, PlaySource.Editor);

            // Since we don't use the stub for the editor currently, set this here
            // NOTE: DromEd game mode doesn't even work for me anymore. Black screen no matter what. So I can't test if we need languages.
            GameConfigFiles.SetCamCfgLanguage(gamePath, "");

            // Why not
            GenerateMissFlagFileIfRequired(fm);

            // We don't need the stub for the editor, cause we don't need to pass anything except the fm folder
            if (!StartExeForOpenFMInEditor(
                    editorExe,
                    gamePath, "-fm=\"" + fm.InstalledDir + "\""))
            {
                return false;
            }

            return true;
        }
        finally
        {
            Core.View.SetWaitCursor(false);
        }
    }

    #endregion

    #region Helpers

    [MustUseReturnValue]
    private static bool SetUsAsSelector(GameIndex gameIndex, string gamePath, PlaySource playSource)
    {
        if (gameIndex == GameIndex.TDM) return true;

        bool suIsPortable = false;

        (bool success, Exception? ex) = GameIsDark(gameIndex)
            ? GameConfigFiles.SetDarkFMSelector(gameIndex, gamePath)
            : GameConfigFiles.SetT3FMSelector(out suIsPortable);

        if (success) return true;

        string protectedDir = ex is UnauthorizedAccessException
            ? $"{NL}Tried to write to a protected directory.{NL}Game path: " + gamePath
            : "";

        Log("Unable to set us as the selector for " + Config.GetGameExe(gameIndex) + " (" +
            (GameIsDark(gameIndex)
                ? nameof(GameConfigFiles.SetDarkFMSelector)
                : nameof(GameConfigFiles.SetT3FMSelector)) +
            $" returned false){NL}" +
            "Source: " + playSource +
            protectedDir,
            stackTrace: true);

        if (playSource == PlaySource.Editor) return false;

        if (ex is UnauthorizedAccessException)
        {
            // If SU is not portable, we should be accessing somewhere in the Documents folder, which should not
            // be write-protected and also the alert messages say "game directory" so that would be misleading.
            // @GENGAMES(Manual game-dir-needs-write-access check): Efficiency, don't do the portable check again
            if (gameIndex != GameIndex.Thief3 || suIsPortable)
            {
                Core.Dialogs.ShowError(
                    GetLocalizedGameNameColon(gameIndex) + $"{NL}" +
                    LText.AlertMessages.NoWriteAccessToGameDir_Play + $"{NL}{NL}" +
                    LText.AlertMessages.GameDirInsideProgramFiles_Explanation + $"{NL}{NL}" +
                    gamePath,
                    icon: MBoxIcon.Warning
                );
            }
        }
        else
        {
            Core.Dialogs.ShowError(
                $"Failed to start the game.{NL}{NL}" +
                $"Reason: Failed to set AngelLoader as the FM selector.{NL}{NL}" +
                "Game: " + gameIndex + $"{NL}" +
                "Game exe: " + Config.GetGameExe(gameIndex) + $"{NL}" +
                "Source: " + playSource + $"{NL}" +
                "");
        }

        return false;
    }

    [MustUseReturnValue]
    private static bool WriteStubCommFileForOriginalGame(string gamePath, GameIndex gameIndex, string origDisabledMods)
    {
        if (gameIndex == GameIndex.TDM) return true;

        if (GameIsDark(gameIndex)) GameConfigFiles.SetCamCfgLanguage(gamePath, "");

        try
        {
            // IMPORTANT (Stub comm file encoding):
            // Encoding MUST be UTF8 with no byte order mark (BOM) or the C++ stub won't read it.
            using var sw = new StreamWriter(Paths.StubCommFilePath, append: false, UTF8NoBOM);
            sw.WriteLine("PlayOriginalGame=True");
            if (GameIsDark(gameIndex) && !origDisabledMods.IsEmpty())
            {
                sw.WriteLine("DisabledMods=" + origDisabledMods);
            }

            return true;
        }
        catch (Exception ex)
        {
            Paths.CreateOrClearTempPath(TempPaths.StubComm);

            string topMsg = ErrorText.ExWrite + "stub file '" + Paths.StubCommFilePath + "'";

            Log(topMsg + $"{NL}" +
                "Game path: " + gamePath,
                ex);

            Core.Dialogs.ShowError(
                $"Failed to start the game.{NL}{NL}" +
                $"Reason: Unable to write the stub comm file.{NL}{NL}" +
                "Without a valid stub comm file, AngelLoader cannot start the game in no-FM mode correctly.");

            return false;
        }
    }

    [MustUseReturnValue]
    private static bool WriteStubCommFileForFM(FanMission fm, string gamePath)
    {
        if (fm.Game == Game.TDM) return true;

        string sLanguage = "";
        bool? bForceLanguage = null;

        if (fm.Game.ConvertsToDark(out GameIndex gameIndex))
        {
            string camCfgLang;
            if (!fm.SelectedLang.ConvertsToKnown(out LanguageIndex langIndex))
            {
                // For Dark, we have to do this semi-manual stuff.
                (sLanguage, bForceLanguage) = FMLanguages.GetDarkFMLanguage(gameIndex, fm.Archive, fm.InstalledDir);
                camCfgLang = "";
            }
            else
            {
                sLanguage = GetLanguageString(langIndex);
                bForceLanguage = true;
                camCfgLang = sLanguage;
            }

            GameConfigFiles.SetCamCfgLanguage(gamePath, camCfgLang);
        }

        // For Thief 3, Sneaky Upgrade does the entire language thing for me, Builder bless snobel once again.
        // I just can't tell you how much I appreciate how much work SU does for me, even right down to handling
        // the "All The World's a Stage" custom sound extract thing.
        // So, I don't have to do anything whatsoever here, just pass blank and carry on. Awesome!

        try
        {
            // IMPORTANT (Stub comm file encoding):
            // Encoding MUST be UTF8 with no byte order mark (BOM) or the C++ stub won't read it.
            using var sw = new StreamWriter(Paths.StubCommFilePath, append: false, UTF8NoBOM);
            sw.WriteLine("PlayOriginalGame=False");
            sw.WriteLine("SelectedFMName=" + fm.InstalledDir);
            if (GameIsDark(fm.Game))
            {
                if (!fm.DisabledMods.IsEmpty()) sw.WriteLine("DisabledMods=" + fm.DisabledMods);
                if (!sLanguage.IsEmpty()) sw.WriteLine("Language=" + sLanguage);
                if (bForceLanguage != null) sw.WriteLine("ForceLanguage=" + (bool)bForceLanguage);
            }

            return true;
        }
        catch (Exception ex)
        {
            Paths.CreateOrClearTempPath(TempPaths.StubComm);

            string topMsg = ErrorText.ExWrite + "stub file '" + Paths.StubCommFilePath + "'";

            fm.LogInfo(topMsg, ex);

            Core.Dialogs.ShowError(
                $"Failed to start the game.{NL}{NL}" +
                $"Reason: Unable to write the stub comm file.{NL}{NL}" +
                "Without a valid stub comm file, the FM '" + fm.GetId() +
                "' cannot be passed to the game and therefore cannot be played.");

            return false;
        }
    }

    #region Start Exe

    [MustUseReturnValue]
    private static bool StartExeForOpenFMInEditor(
        string exe,
        string workingPath,
        string args)
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exe,
                WorkingDirectory = workingPath,
                Arguments = !args.IsEmpty() ? args : "",
            };

            ProcessStart_UseShellExecute(startInfo);

            return true;
        }
        catch (Exception ex)
        {
            HandleStartExeFailure(exe, workingPath, args, ex);
            return false;
        }
    }

    [MustUseReturnValue]
    private static bool StartExeForPlayOriginalGame(
        GameIndex gameIndex,
        string exe,
        string workingPath,
        string args)
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exe,
                WorkingDirectory = workingPath,
                Arguments = !args.IsEmpty() ? args : "",
            };

            if (gameIndex == GameIndex.TDM)
            {
                PlayTimeTracking.GetTimeTrackingProcess(gameIndex).StartTdmWithNoFM(startInfo);
            }
            else
            {
                ProcessStart_UseShellExecute(startInfo);
            }

            return true;
        }
        catch (Exception ex)
        {
            HandleStartExeFailure(exe, workingPath, args, ex);
            return false;
        }
    }

    [MustUseReturnValue]
    private static async Task<bool> StartExeForPlayFM(
        FanMission fm,
        GameIndex gameIndex,
        string exe,
        string gameExe,
        string workingPath,
        string args,
        bool steam)
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exe,
                WorkingDirectory = workingPath,
                Arguments = !args.IsEmpty() ? args : "",
            };

            bool success = await PlayTimeTracking.GetTimeTrackingProcess(gameIndex).Start(startInfo, fm, steam, gameExe);
            return success;
        }
        catch (Exception ex)
        {
            HandleStartExeFailure(exe, workingPath, args, ex);
            return false;
        }
    }

    #endregion

    private static void HandleStartExeFailure(string exe, string workingPath, string args, Exception ex)
    {
        string msg = ErrorText.Un + "start '" + exe + "'.";
        Log(msg + $"{NL}" +
            nameof(workingPath) + ": " + workingPath + $"{NL}" +
            nameof(args) + ": " + args, ex);
        Core.Dialogs.ShowError(msg);
    }

    private static (bool Success, string GameExe, string GamePath)
    CheckAndReturnFinalGameExeAndGamePath(GameIndex gameIndex, bool playingOriginalGame, bool playMP)
    {
        var failed = (Success: false, GameExe: "", GamePath: "");

        string gameName = GetLocalizedGameName(gameIndex);

        string gameExe = Config.GetGameExe(gameIndex);

        #region Fail if game path is blank

        string gamePath = Config.GetGamePath(gameIndex);
        if (gamePath.IsEmpty())
        {
            Log(ErrorText.GamePathEmpty + $"{NL}" + gameIndex, stackTrace: true);
            Core.Dialogs.ShowError(gameName + $":{NL}" + ErrorText.GamePathEmpty);
            return failed;
        }

        #endregion

        if (playMP) gameExe = Path.Combine(gamePath, Paths.T2MPExe);

        #region Exe: Fail if blank or not found

        if (GameDirNeedsWriteAccess(gameIndex))
        {
            if (!DirectoryHasWritePermission(gamePath))
            {
                Log(gameName + $": No write permission for game directory.{NL}" +
                    "Game path: " + gamePath);

                Core.Dialogs.ShowError(
                    GetLocalizedGameNameColon(gameIndex) + $"{NL}" +
                    LText.AlertMessages.NoWriteAccessToGameDir_Play + $"{NL}{NL}" +
                    LText.AlertMessages.GameDirInsideProgramFiles_Explanation + $"{NL}{NL}" +
                    gamePath,
                    icon: MBoxIcon.Warning
                );

                return failed;
            }
        }

        if (gameExe.IsEmpty() || !File.Exists(gameExe))
        {
            string exeNotFoundMessage = playingOriginalGame
                ? LText.AlertMessages.Play_ExecutableNotFound
                : LText.AlertMessages.Play_ExecutableNotFoundFM;

            if (gameExe.IsEmpty())
            {
                Log(gameName + ": Game executable not specified.");
            }
            else
            {
                Log(gameName + ": Game executable not found: " + gameExe);
            }

            string finalMessage = gameName + $":{NL}" + exeNotFoundMessage;
            if (!gameExe.IsEmpty()) finalMessage += $"{NL}{NL}" + gameExe;

            Core.Dialogs.ShowError(finalMessage);
            return failed;
        }

        #endregion

        #region Exe: Fail if already running

        if (GameIsRunning(gameExe, checkAllGames: true))
        {
            Core.Dialogs.ShowAlert(LText.AlertMessages.Play_AnyGameIsRunning, LText.AlertMessages.Alert);
            return failed;
        }

        #endregion

        // Put this AFTER running check, because the user might go into DarkLoader and have it mess with the
        // files in the game dir, and we don't want the game running if they do that.
        #region Fail on DarkLoader FM installed

        if (GameConfigFiles.GameHasDarkLoaderFMInstalled(gameIndex))
        {
            string dlExe = AutodetectDarkLoaderFile(Paths.DarkLoaderExe);

            (MBoxButton result, _) = Core.Dialogs.ShowMultiChoiceDialog(
                message: GetLocalizedGameNameColon(gameIndex) + $"{NL}" +
                         LText.AlertMessages.DarkLoader_InstalledFMFound,
                title: LText.AlertMessages.Alert,
                icon: MBoxIcon.Warning,
                yes: !dlExe.IsEmpty() ? LText.AlertMessages.DarkLoader_OpenNow : null,
                no: LText.Global.ContinueAnyway,
                cancel: LText.Global.Cancel,
                defaultButton: !dlExe.IsEmpty() ? MBoxButton.Yes : MBoxButton.Cancel
            );
            if (result == MBoxButton.Yes && !dlExe.IsEmpty())
            {
                try
                {
                    ProcessStart_UseShellExecute(dlExe);
                }
                catch (Exception ex)
                {
                    string msg = ErrorText.Un + "open DarkLoader.";
                    Log(msg, ex);
                    Core.Dialogs.ShowError(msg);
                }
                return failed;
            }
            else if (result == MBoxButton.Cancel)
            {
                return failed;
            }
        }

        #endregion

        return (true, gameExe, gamePath);
    }

    private static (bool Success, string SteamExe, string SteamWorkingPath, string Args)
    GetSteamValues(GameIndex game, bool playMP)
    {
        // Multiplayer means starting Thief2MP.exe, so we can't really run it through Steam because Steam
        // will start Thief2.exe
        if (!playMP &&
            !GetGameSteamId(game).IsEmpty() && Config.GetUseSteamSwitch(game) &&
            Config.LaunchGamesWithSteam && !Config.SteamExe.IsEmpty() && File.Exists(Config.SteamExe))
        {
            string? workingPath = Path.GetDirectoryName(Config.SteamExe);

            if (workingPath.IsEmpty()) return (false, "", "", "");

            string args = "-applaunch " + GetGameSteamId(game);

            return (true, Config.SteamExe, workingPath, args);
        }
        else
        {
            return (false, "", "", "");
        }
    }

    private static bool RunThiefBuddyIfRequired(FanMission fm)
    {
        if (!fm.Game.ConvertsToDarkThief(out GameIndex gameIndex) ||
            Config.RunThiefBuddyOnFMPlay == RunThiefBuddyOnFMPlay.Never)
        {
            return true;
        }

        string thiefBuddyExe = Paths.GetThiefBuddyExePath();

        if (thiefBuddyExe.IsWhiteSpace() || !File.Exists(thiefBuddyExe))
        {
            return true;
        }

        bool runThiefBuddy = true;

        if (Config.RunThiefBuddyOnFMPlay == RunThiefBuddyOnFMPlay.Ask)
        {
            (MBoxButton result, bool dontAskAgain) = Core.Dialogs.ShowMultiChoiceDialog(
                message: LText.ThiefBuddy.AskToRunThiefBuddy,
                title: LText.AlertMessages.Confirm,
                icon: MBoxIcon.None,
                yes: LText.ThiefBuddy.RunThiefBuddy,
                no: LText.ThiefBuddy.DontRunThiefBuddy,
                cancel: LText.Global.Cancel,
                checkBoxText: LText.AlertMessages.DontAskAgain
            );

            if (result == MBoxButton.Cancel) return false;

            Config.RunThiefBuddyOnFMPlay = dontAskAgain
                ? result == MBoxButton.Yes ? RunThiefBuddyOnFMPlay.Always : RunThiefBuddyOnFMPlay.Never
                : RunThiefBuddyOnFMPlay.Ask;

            runThiefBuddy = result == MBoxButton.Yes;
        }

        if (runThiefBuddy)
        {
            try
            {
                string fmInstalledPath = Path.Combine(Config.GetFMInstallPath(gameIndex), fm.InstalledDir);
                ProcessStart_UseShellExecute(new ProcessStartInfo(thiefBuddyExe, "\"" + fmInstalledPath + "\" -startwatch"));
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExTry + "run Thief Buddy", ex);
                Core.Dialogs.ShowError(LText.ThiefBuddy.ErrorRunning);
            }
        }

        return true;
    }

    #endregion

    #region Per-FM fixes/patches etc.

    private static MissFlagError GenerateMissFlagFileIfRequired(FanMission fm, bool errorOnCantPlay = false)
    {
        // Only T1 and T2 have/require missflag.str
        if (!fm.Game.ConvertsToDarkThief(out GameIndex gameIndex)) return MissFlagError.None;

        try
        {
            string instFMsBasePath = Config.GetFMInstallPath(gameIndex);
            string fmInstalledPath = Path.Combine(instFMsBasePath, fm.InstalledDir);

            if (!Directory.Exists(fmInstalledPath)) return MissFlagError.None;

            string stringsPath = Path.Combine(fmInstalledPath, "strings");
            string missFlagFile = Path.Combine(stringsPath, Paths.MissFlagStr);

            bool MissFlagFilesExist()
            {
                if (!Directory.Exists(stringsPath)) return false;
                // Missflag.str could be in a subdirectory too! Don't make a new one in that case!
                string[] missFlag = Directory.GetFiles(stringsPath, Paths.MissFlagStr, SearchOption.AllDirectories);
                return missFlag.Length > 0;
            }

            if (MissFlagFilesExist()) return MissFlagError.None;

            List<string> misFiles = FastIO.GetFilesTopOnly(fmInstalledPath, "miss*.mis");
            var misNums = new List<int>(misFiles.Count);
            foreach (string mf in misFiles)
            {
                Match m = Regex.Match(mf, "miss(?<Num>[0-9]+).mis", IgnoreCaseInvariant);
                if (m.Success && Int_TryParseInv(m.Groups["Num"].Value, out int result))
                {
                    misNums.Add(result);
                }
            }

            if (misNums.Count == 0)
            {
                if (errorOnCantPlay)
                {
                    string msg =
                        $"This FM is not correctly structured, so the game will not be able to load it.{NL}{NL}" +
                        $"This FM may be a demo, an unfinished mission, or may just be broken. It may be necessary to open it in DromEd to play it, or it may not be playable at all.{NL}{NL}" +
                        $"Details:{NL}" +
                        "No missflag.str found. Tried to generate missflag.str, but failed because there were no correctly named .mis files. " +
                        "Thief 1 and Thief 2 FMs are required to have at least one .mis file named in the format 'missN.mis', where N is a number. For example, 'miss20.mis' would be a valid name.";

                    string logMsg = msg;

                    try
                    {
                        string misFileNames = "";
                        List<string> allMisFiles = FastIO.GetFilesTopOnly(fmInstalledPath, "*.mis");
                        if (allMisFiles.Count == 0)
                        {
                            logMsg += $"{NL}{NL}No .mis files were found in FM directory.{NL}";
                        }
                        else
                        {
                            foreach (string fn in allMisFiles)
                            {
                                misFileNames += $"{NL}" + Path.GetFileName(fn);
                            }
                            misFileNames += $"{NL}";
                            logMsg += $"{NL}{NL}.mis files in FM directory:" + misFileNames;
                        }
                    }
                    catch (Exception ex)
                    {
                        logMsg += $"Exception getting .mis file names in FM directory.{NL}EXCEPTION: " + ex;
                    }

                    fm.LogInfo(logMsg);
                    Core.Dialogs.ShowError(
                        message: msg,
                        title: LText.AlertMessages.Alert,
                        icon: MBoxIcon.Warning
                    );

                    return MissFlagError.NoValidlyNamedMisFiles;
                }
                else
                {
                    return MissFlagError.None;
                }
            }

            try
            {
                misNums.Sort();

                Directory.CreateDirectory(stringsPath);

                int lastMisNum = misNums[^1];

                var missFlagLines = new List<string>();
                for (int i = 1; i <= lastMisNum; i++)
                {
                    string curLine = "miss_" + i.ToStrInv() + ": ";
                    if (misNums.Contains(i))
                    {
                        curLine += "\"no_briefing, no_loadout";
                        if (i == lastMisNum) curLine += ", end";
                        curLine += "\"";
                    }
                    else
                    {
                        curLine += "\"skip\"";
                    }

                    missFlagLines.Add(curLine);
                }

                File.WriteAllLines(missFlagFile, missFlagLines, UTF8NoBOM);
            }
            catch (Exception ex)
            {
                fm.LogInfo(ErrorText.ExTry + "generate missflag.str file for an FM that needs it", ex);
                // IMPORTANT: Do NOT put up this dialog during threaded install!
                // Don't bother returning the exception or anything; we'll try to generate again on play and show
                // the dialog then if it still fails.
                if (errorOnCantPlay)
                {
                    Core.Dialogs.ShowError(
                        $"Failed trying to generate a missflag.str file for the following FM:{NL}{NL}" +
                        fm.GetId() + $"{NL}{NL}" +
                        "The FM will probably not be able to play its mission(s).");
                }
            }
        }
        catch (Exception ex)
        {
            fm.LogInfo(ErrorText.ExTry + "generate missflag.str file", ex);
            // ReSharper disable once RedundantJumpStatement
            return MissFlagError.None; // Explicit for clarity of intent
        }

        return MissFlagError.None;
    }

    private static bool TryGetSmallestUsedMisFile(FanMission fm, out string smallestUsedMisFile, out List<string> usedMisFiles)
    {
        smallestUsedMisFile = "";
        usedMisFiles = new List<string>();

        if (!GameIsDark(fm.Game)) return false;

        if (!FMIsReallyInstalled(fm, out string fmDir)) return false;

        List<FileInfo> misFileInfos;
        try
        {
            misFileInfos = new DirectoryInfo(fmDir).GetFiles("*.mis", SearchOption.TopDirectoryOnly).ToList();
        }
        catch (Exception ex)
        {
            string msg = "Error trying to get .mis files in FM installed directory.";
            fm.LogInfo(msg + " " + ErrorText.RetF, ex);
            Core.Dialogs.ShowError(msg + $"{NL}{NL}" + ErrorText.OldDarkDependentFeaturesWillFail);
            return false;
        }

        // Workaround https://fenphoenix.github.io/AngelLoader/file_ext_note.html
        for (int i = 0; i < misFileInfos.Count; i++)
        {
            if (!misFileInfos[i].Name.EndsWithI(".mis"))
            {
                misFileInfos.RemoveAt(i);
                i--;
            }
        }

        if (misFileInfos.Count == 0)
        {
            string msg = "Error detecting OldDark: could not find any .mis files in FM installed directory.";
            fm.LogInfo(msg + " " + ErrorText.RetF);
            Core.Dialogs.ShowError(msg + $"{NL}{NL}" + ErrorText.OldDarkDependentFeaturesWillFail);
            return false;
        }

        var usedMisFileInfos = new List<FileInfo>(misFileInfos.Count);

        if (fm.Game != Game.SS2)
        {
            string? missFlag = null;

            string stringsPath = Path.Combine(fmDir, "strings");

            if (!Directory.Exists(stringsPath)) return false;

            string loc1 = Path.Combine(stringsPath, Paths.MissFlagStr);
            string loc2 = Path.Combine(stringsPath, "english", Paths.MissFlagStr);

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
                try
                {
                    string[] files = Directory.GetFiles(stringsPath, Paths.MissFlagStr, SearchOption.AllDirectories);
                    if (files.Length > 0) missFlag = files[0];
                }
                catch (Exception ex)
                {
                    string msg = "Error trying to get " + Paths.MissFlagStr + " files in " + stringsPath + " or any subdirectory.";
                    fm.LogInfo(msg + " " + ErrorText.RetF, ex);
                    Core.Dialogs.ShowError(msg + $"{NL}{NL}" + ErrorText.OldDarkDependentFeaturesWillFail);
                    return false;
                }
            }

            if (missFlag == null)
            {
                string msg = "Expected to find " + Paths.MissFlagStr +
                             " for this FM, but it could not be found or the search failed. " +
                             "If it didn't exist, it should have been generated.";
                fm.LogInfo(msg + " " + ErrorText.RetF);
                Core.Dialogs.ShowError(msg + $"{NL}{NL}" + ErrorText.OldDarkDependentFeaturesWillFail);
                return false;
            }

            if (!TryReadAllLines(missFlag, out List<string>? mfLines))
            {
                Core.Dialogs.ShowError("Error trying to read '" + missFlag + $"'.{NL}{NL}" + ErrorText.OldDarkDependentFeaturesWillFail);
                return false;
            }

            /*
            Copied from Scanner except mis file type is different so yeah... that's why we can't just make
            it a method. But until this actually needs to be changed - and I don't see why it ever would -
            it's not actually a problem.
            */
            for (int mfI = 0; mfI < misFileInfos.Count; mfI++)
            {
                FileInfo mf = misFileInfos[mfI];

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
                                    usedMisFileInfos.Add(mf);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (usedMisFileInfos.Count == 0) usedMisFileInfos.AddRange(misFileInfos);

        usedMisFileInfos = usedMisFileInfos.OrderBy(static x => x.Length).ToList();

        smallestUsedMisFile = usedMisFileInfos[0].FullName;

        foreach (FileInfo fi in usedMisFileInfos)
        {
            usedMisFiles.Add(fi.FullName);
        }

        return true;
    }

    /*
    SS2 OldDark detect notes:
    -SS2_Zygo_Arena_ND
     This one has earth.mis and Arena.mis, with earth.mis being OldDark and Arena.mis being NewDark.
    -UNN Sayarath
     Same thing as above, this one has cloaking_turret.mis (OldDark) and sayarath.mis (NewDark).
    */
    private static bool FMIsOldDark(FanMission fm)
    {
        if (!GameIsDark(fm.Game)) return false;

        if (!TryGetSmallestUsedMisFile(fm, out string smallestUsedMisFile, out List<string> usedMisFiles))
        {
            return false;
        }

        try
        {
            const int MAPPARAM_OldDarkLocation = 916;
            const int MAPPARAM_NewDarkLocation = 696;

            const int DARKMISS_NewDarkLocation = 612;

            byte[] buffer = new byte[Math.Max(MAPPARAM.Length, _DARKMISS_Bytes.Length)];

            if (fm.Game == Game.SS2)
            {
                bool atLeastOneOldDarkMissionFound = false;

                // A couple of SS2 FMs have a mixture of OldDark and NewDark .mis files, so just return the
                // highest dark version found in the mission set.
                foreach (string misFile in usedMisFiles)
                {
                    using FileStream fs = File.OpenRead(misFile);

                    long streamLength = fs.Length;

                    if (streamLength > MAPPARAM_NewDarkLocation + MAPPARAM.Length)
                    {
                        fs.Position = MAPPARAM_NewDarkLocation;
                        int bytesRead = fs.ReadAll(buffer.Cleared(), 0, MAPPARAM.Length);
                        if (bytesRead == MAPPARAM.Length && buffer.StartsWith(MAPPARAM))
                        {
                            return false;
                        }
                    }

                    // Robustness - don't assume there's a MAPPARAM at the OldDark location just because
                    // there wasn't one at the NewDark location.
                    if (!atLeastOneOldDarkMissionFound &&
                        streamLength > MAPPARAM_OldDarkLocation + MAPPARAM.Length)
                    {
                        fs.Position = MAPPARAM_OldDarkLocation;
                        int bytesRead = fs.ReadAll(buffer.Cleared(), 0, MAPPARAM.Length);
                        if (bytesRead == MAPPARAM.Length && buffer.StartsWith(MAPPARAM))
                        {
                            atLeastOneOldDarkMissionFound = true;
                        }
                    }
                }

                return atLeastOneOldDarkMissionFound;
            }
            else
            {
                using FileStream fs = File.OpenRead(smallestUsedMisFile);

                if (DARKMISS_NewDarkLocation + _DARKMISS_Bytes.Length > fs.Length)
                {
                    return false;
                }

                fs.Position = DARKMISS_NewDarkLocation;
                int bytesRead = fs.ReadAll(buffer.Cleared(), 0, _DARKMISS_Bytes.Length);

                return !(bytesRead == _DARKMISS_Bytes.Length && buffer.StartsWith(_DARKMISS_Bytes));
            }
        }
        catch (Exception ex)
        {
            string msg = "Error trying to detect if FM is OldDark.";
            fm.LogInfo(msg + " " + ErrorText.RetF, ex);
            Core.Dialogs.ShowError(msg + $"{NL}{NL}" + ErrorText.OldDarkDependentFeaturesWillFail);
            return false;
        }
    }

    /*
    We're not going to try the palette fix for SS2 because I don't see any pal\ dirs in any SS2 FM I have
    so I'm just going to assume it's not supported to do the palette thing for SS2.
    */
    private static bool FMRequiresPaletteFix(FanMission fm, bool checkForOldDark = true)
    {
        #region Local functions

        static string GetDefaultPalName(string file)
        {
            if (!File.Exists(file)) return "";

            const string key_default_game_palette = "default_game_palette";

            using var sr = new StreamReader(file);
            while (sr.ReadLine() is { } line)
            {
                string lineT = line.Trim();
                if (lineT.StartsWithIPlusWhiteSpace(key_default_game_palette))
                {
                    string defaultPal = lineT.Substring(key_default_game_palette.Length).Trim();
                    if (!defaultPal.IsEmpty()) return defaultPal;
                }
            }

            return "";
        }

        #endregion

        if (!fm.Game.ConvertsToDarkThief(out GameIndex gameIndex)) return false;
        if (!fm.Installed) return false;
        if (checkForOldDark && !FMIsOldDark(fm)) return false;

        try
        {
            string gamePath = Config.GetGamePath(gameIndex);
            string fmInstallPath = Config.GetFMInstallPath(gameIndex);

            if (gamePath.IsEmpty() || fmInstallPath.IsEmpty()) return false;

            string fmDir = Path.Combine(gamePath, fmInstallPath, fm.InstalledDir);

            if (!Directory.Exists(fmDir)) return false;

            string palDir = Path.Combine(fmDir, "pal");

            if (!Directory.Exists(palDir)) return false;

            string defaultPal = GetDefaultPalName(Path.Combine(fmDir, Paths.DarkCfg));
            if (defaultPal.IsEmpty())
            {
                defaultPal = GetDefaultPalName(Path.Combine(gamePath, Paths.DarkCfg));
                if (defaultPal.IsEmpty()) return false;
            }

            return File.Exists(Path.Combine(palDir, defaultPal + ".pcx"));
        }
        catch (Exception ex)
        {
            string msg = "Error trying to detect if this OldDark FM requires a palette fix.";
            fm.LogInfo(msg + " " + ErrorText.RetF, ex);
            Core.Dialogs.ShowError(
                msg + $"{NL}{NL}" +
                "If the FM requires a palette fix (rare), the fix will not be applied on this run. " +
                "This may cause black-and-white OldDark missions to have some objects in color.");
            return false;
        }
    }

    #endregion

    #region Install/Uninstall

    [MustUseReturnValue]
    private static bool DoPreChecks(
        FanMission[] fms,
        List<FMData> fmDataList,
        bool install,
        [NotNullWhen(true)] out List<string>? archivePaths,
        [NotNullWhen(true)] out List<ThreadablePath>? threadablePaths)
    {
        threadablePaths = null;
        try
        {
            bool single = fms.Length == 1;

            bool[] gamesChecked = new bool[SupportedGameCount];

            archivePaths = FMArchives.GetFMArchivePaths();

            HashSetI usedArchivePaths = new(Config.FMArchivePaths.Count);
            for (int i = 0; i < fms.Length; i++)
            {
                FanMission fm = fms[i];

                AssertR(install ? !fm.Installed : fm.Installed, "fm.Installed == " + fm.Installed);

                if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
                {
                    fm.LogInfo(ErrorText.FMGameU, stackTrace: true);
                    Core.Dialogs.ShowError(fm.GetId() + $"{NL}" + ErrorText.FMGameU);
                    return false;
                }

                int intGameIndex = (int)gameIndex;

                if (Canceled(install)) return false;

                string fmArchivePath = FMArchives.FindFirstMatch(fm.Archive, archivePaths, out string archiveDirectoryFullPath);
                if (!archiveDirectoryFullPath.IsEmpty())
                {
                    usedArchivePaths.Add(archiveDirectoryFullPath);
                }

                if (Canceled(install)) return false;

                string gameExe = Config.GetGameExe(gameIndex);
                string gameName = GetLocalizedGameName(gameIndex);
                string instBasePath = Config.GetFMInstallPath(gameIndex);

                if (install)
                {
                    bool fmArchivePathIsEmpty = fmArchivePath.IsEmpty();
                    bool fmIsMarkedUnavailable = fm.MarkedUnavailable;

                    if (fmArchivePathIsEmpty && !fmIsMarkedUnavailable)
                    {
                        fm.LogInfo("FM archive field was empty; this means an archive was not found for it on the last search.");
                        Core.Dialogs.ShowError(fm.GetId() + $"{NL}" +
                                               LText.AlertMessages.Install_ArchiveNotFound);

                        return false;
                    }
                    else if (fmArchivePathIsEmpty || fmIsMarkedUnavailable)
                    {
                        continue;
                    }
                }

                fmDataList.Add(new FMData
                (
                    fm,
                    fmArchivePath,
                    archiveDirectoryFullPath,
                    instBasePath,
                    gameIndex
                ));

                if (!gamesChecked[intGameIndex])
                {
                    if (install)
                    {
                        if (!File.Exists(gameExe))
                        {
                            fm.LogInfo($"Game executable not found.{NL}Game executable: " + gameExe);
                            Core.Dialogs.ShowError(gameName + $":{NL}" +
                                                   fm.GetId() + $"{NL}" +
                                                   LText.AlertMessages.Install_ExecutableNotFound);

                            return false;
                        }

                        if (Canceled(install)) return false;

                        if (!Directory.Exists(instBasePath))
                        {
                            fm.LogInfo("FM install path not found.");

                            Core.Dialogs.ShowError(gameName + $":{NL}" +
                                                   fm.GetId() + $"{NL}" +
                                                   LText.AlertMessages.Install_FMInstallPathNotFound);

                            return false;
                        }
                    }

                    if (!DirectoryHasWritePermission(instBasePath))
                    {
                        Log(gameName + $": No write permission for installed FMs directory.{NL}" +
                            "Installed FMs directory: " + instBasePath);

                        Core.Dialogs.ShowError(
                            GetLocalizedGameNameColon(gameIndex) + $"{NL}" +
                            LText.AlertMessages.NoWriteAccessToInstalledFMsDir + $"{NL}{NL}" +
                            LText.AlertMessages.GameDirInsideProgramFiles_Explanation + $"{NL}{NL}" +
                            instBasePath,
                            icon: MBoxIcon.Warning
                        );

                        return false;
                    }

                    if (Canceled(install)) return false;

                    if (GameIsRunning(gameExe))
                    {
                        Core.Dialogs.ShowAlert(
                            !single
                                ? LText.AlertMessages.OneOrMoreGamesAreRunning
                                : gameName + $":{NL}" + (install
                                    ? LText.AlertMessages.Install_GameIsRunning
                                    : LText.AlertMessages.Uninstall_GameIsRunning),
                            LText.AlertMessages.Alert);

                        return false;
                    }

                    if (Canceled(install)) return false;

                    gamesChecked[intGameIndex] = true;
                }
            }

            threadablePaths = GetInstallUninstallRelevantPaths(usedArchivePaths, gamesChecked);
        }
        catch (Exception ex)
        {
            Log(ex: ex);
            Core.Dialogs.ShowError("Exception occurred in " + nameof(FMInstallAndPlay) + "." + nameof(DoPreChecks) + "(). See log for details.");

            archivePaths = new List<string>();
            return false;
        }

        return true;

        static bool Canceled(bool install) => install && _installCts.IsCancellationRequested;
    }

    #region Install

    internal sealed class FMData
    {
        internal readonly FanMission FM;
        internal readonly string ArchiveFilePath;
        internal readonly string ArchiveDirectoryPath;
        internal readonly string InstBasePath;
        internal readonly string InstalledPath;
        internal readonly GameIndex GameIndex;

        internal bool Uninstall_MarkFMAsUnavailable;
        internal bool Uninstall_SkipUninstallingThisFM;

        private string? _fmArchiveNoExtension;
        internal string FMArchiveNoExtension => _fmArchiveNoExtension ??= FM.Archive.RemoveExtension();

        private string? _archiveNoExtensionWhitespaceTrimmed;
        internal string ArchiveNoExtensionWhitespaceTrimmed => _archiveNoExtensionWhitespaceTrimmed ??= FMArchiveNoExtension.Trim();

        private string? _bakFile;
        internal string BakFile => _bakFile ??= Path.Combine(Config.FMsBackupPath,
            (!FM.Archive.IsEmpty() ? FMArchiveNoExtension : FM.InstalledDir) +
            Paths.FMBackupSuffix);

        public FMData(FanMission fm, string archiveFilePath, string archiveDirectoryPath, string instBasePath, GameIndex gameIndex)
        {
            FM = fm;
            ArchiveFilePath = archiveFilePath;
            ArchiveDirectoryPath = archiveDirectoryPath;
            InstBasePath = instBasePath;
            InstalledPath = Path.Combine(instBasePath, fm.InstalledDir);
            GameIndex = gameIndex;
        }

        public override string ToString() =>
            "FM id: " + FM.GetId() + $"{NL}" +
            "Archive file path: " + ArchiveFilePath + $"{NL}" +
            "Archive directory path: " + ArchiveDirectoryPath + $"{NL}" +
            "Installed base path: " + InstBasePath + $"{NL}" +
            "Installed path: " + InstalledPath + $"{NL}" +
            "Game index: " + GameIndex;
    }

    internal static async Task<bool> Install(params FanMission[] fms)
    {
        using var dsw = new DisableScreenshotWatchers();
        return await InstallInternal(false, false, fms);
    }

    private static async Task<bool> InstallInternal(bool fromPlay, bool suppressConfirmation, params FanMission[] fms)
    {
        var fmDataList = new List<FMData>(fms.Length);

        bool single = fms.Length == 1;

        if (!suppressConfirmation &&
            (Config.ConfirmBeforeInstall == ConfirmBeforeInstall.Always ||
             (!single && Config.ConfirmBeforeInstall == ConfirmBeforeInstall.OnlyForMultiple)))
        {
            (MBoxButton result, bool dontAskAgain) = Core.Dialogs.ShowMultiChoiceDialog(
                message: single
                    ? fromPlay
                        ? LText.AlertMessages.Play_InstallAndPlayConfirmMessage
                        : LText.AlertMessages.Install_ConfirmSingular
                    : LText.AlertMessages.Install_ConfirmPlural_BeforeNumber +
                      fms.Length.ToStrCur() +
                      LText.AlertMessages.Install_ConfirmPlural_AfterNumber,
                title: LText.AlertMessages.Alert,
                icon: MBoxIcon.None,
                yes: single ? fromPlay ? LText.Global.PlayFM : LText.Global.InstallFM : LText.Global.InstallFMs,
                no: LText.Global.Cancel,
                checkBoxText: LText.AlertMessages.DontAskAgain,
                defaultButton: MBoxButton.No);
            if (result == MBoxButton.No) return false;

            if (dontAskAgain) Config.ConfirmBeforeInstall = ConfirmBeforeInstall.Never;
        }

        _installCts = _installCts.Recreate();

        Core.View.ShowProgressBox_Single(
            message1: LText.ProgressBox.PreparingToInstall,
            progressType: ProgressType.Indeterminate,
            cancelAction: CancelInstallToken
        );

        return await Task.Run(() =>
        {
            try
            {
                if (!DoPreChecks(
                        fms,
                        fmDataList,
                        install: true,
                        out List<string>? archivePaths,
                        out List<ThreadablePath>? threadablePaths))
                {
                    return false;
                }

                if (fmDataList.Count == 0) return false;

                Core.View.SetProgressBoxState(
                    size: single ? ProgressSizeMode.Single : ProgressSizeMode.Double,
                    mainMessage1: single ? LText.ProgressBox.InstallingFM : LText.ProgressBox.InstallingFMs,
                    mainMessage2: "",
                    mainPercent: 0,
                    mainProgressType: ProgressType.Determinate,
                    subMessage: "",
                    subPercent: 0,
                    subProgressType: ProgressType.Determinate,
                    cancelMessage: LText.Global.Cancel
                );

                List<FMInstallResult> results = new(fmDataList.Count);

#if TIMING_TEST
                StartTiming();
#endif
                int fmDataIndex = 0;
                try
                {
                    DarkLoaderBackupContext ctx = new();

                    ZipContext_Pool zipCtxPool = new();
                    ZipContext_Threaded_Pool zipCtxThreadedPool = new();
                    IOBufferPools ioBufferPools = new();

                    // Entry-parallel/archive-sequential is nearly as fast as entry-parallel/archive-parallel,
                    // but much simpler.
                    int fmDataListCount = fmDataList.Count;
                    for (; fmDataIndex < fmDataListCount; fmDataIndex++)
                    {
                        FMData fmData = fmDataList[fmDataIndex];

                        int mainPercent = GetPercentFromValue_Int(fmDataIndex, fmDataListCount);

                        FMInstallResult fmInstallResult;
                        if (fmData.ArchiveFilePath.ExtIsZip())
                        {
                            List<ThreadablePath> zipInstallRelevantPaths = threadablePaths.FilterToZipFMInstallRelevant(fmData);
                            if (IsArchivePathAtLeastReadAndInstallPathAtLeastReadWrite(zipInstallRelevantPaths))
                            {
                                ThreadingData installThreadingData = GetLowestCommonThreadingData(zipInstallRelevantPaths);
                                fmInstallResult = InstallFMZip_ThreadedPerEntry(
                                    fmData,
                                    mainPercent,
                                    fmDataListCount,
                                    zipCtxPool,
                                    zipCtxThreadedPool,
                                    ioBufferPools,
                                    installThreadingData.Threads);
                            }
                            else
                            {
                                fmInstallResult = InstallFMZip(
                                    fmData,
                                    mainPercent,
                                    fmDataListCount,
                                    ioBufferPools,
                                    zipCtxPool);
                            }
                        }
                        else
                        {
                            fmInstallResult = fmData.ArchiveFilePath.ExtIsRar()
                                ? InstallFMRar(
                                    fmData,
                                    mainPercent,
                                    fmDataListCount,
                                    ioBufferPools)
                                : InstallFMSevenZip(
                                    fmData,
                                    mainPercent,
                                    fmDataListCount);
                        }

                        if (fmInstallResult.ResultType != InstallResultType.InstallSucceeded)
                        {
                            FMInstallResult result = RollBackSingleFM(fmData, fmDataListCount == 1);
                            results.Add(result);
                            continue;
                        }

                        results.Add(fmInstallResult);

                        _installCts.Token.ThrowIfCancellationRequested();

                        // @MT_TASK_NOTE: Set this before post-install work because it gets checked!
                        // This will again cause the UI to update the installed status if it's refreshed.
                        // If we wanted to prevent that we could get really fancy about it later, but keep
                        // this for now.
                        fmData.FM.Installed = true;

                        DoPostInstallWork(
                            fmData,
                            ioBufferPools,
                            _installCts.Token,
                            ctx,
                            archivePaths,
                            fmDataList.Count,
                            threadablePaths);
                    }
                }
                catch (OperationCanceledException)
                {
                    // @MT_TASK_NOTE: We get a brief UI thread block when run from within Visual Studio.
                    // Apparently because it has to spew out all those exception messages in the output console.
                    // Everything's fine outside of VS. So just ignore this during dev.

                    List<FMInstallResult> rollbackErrorResults = RollBackMultipleFMs(fmDataList, fmDataIndex);

                    if (rollbackErrorResults.Count > 0)
                    {
                        Log($"--- Rollback errors ---{NL}" +
                            GetResultErrorsLogText(rollbackErrorResults) +
                            "--- End Rollback errors ---");

                        Core.Dialogs.ShowError(
                            LText.AlertMessages.InstallRollback_FMInstallFolderDeleteFail,
                            LText.AlertMessages.Alert,
                            icon: MBoxIcon.Warning);
                    }

                    return false;
                }

                List<FMInstallResult> errorResults = new();

                foreach (FMInstallResult result in results)
                {
                    if (result.ResultType != InstallResultType.InstallSucceeded)
                    {
                        if (result.ResultType == InstallResultType.RollbackFailed)
                        {
                            // Rollbacks failing should be the rare case, so it's okay to take a disk hit here
                            result.FMData.FM.Installed = Directory.Exists(result.FMData.InstalledPath);
                        }
                        errorResults.Add(result);
                    }
                }

                if (errorResults.Count > 0)
                {
                    Log($"--- Install errors ---{NL}" +
                        GetResultErrorsLogText(errorResults) +
                        "--- End Install errors ---");

                    Core.Dialogs.ShowError(
                        LText.AlertMessages.Install_OneOrMoreFMsFailedToInstall,
                        LText.AlertMessages.Alert,
                        icon: MBoxIcon.Warning);
                }

                Core.View.Invoke(Core.View.RefreshAllSelectedFMs_UpdateInstallState);

                return true;
            }
            finally
            {
#if TIMING_TEST
                StopTimingAndPrintResult(nameof(InstallInternal));
#endif

                Ini.WriteFullFMDataIni();
                Core.View.HideProgressBox();

                _installCts.Dispose();
            }
        });

        static string GetResultErrorsLogText(List<FMInstallResult> results)
        {
            string logText = "";

            for (int i = 0; i < results.Count; i++)
            {
                FMInstallResult result = results[i];

                if (!logText.IsEmpty())
                {
                    logText += $"{NL}---{NL}";
                }

                logText +=
                    $"{NL}" +
                    "Archive type: " + result.ArchiveType + $"{NL}" +
                    "Result type: " + result.ResultType + $"{NL}" +
                    "Exception: " + (result.Exception?.ToString() ?? "none") + $"{NL}" +
                    "Error message: " + (result.ErrorMessage.IsEmpty() ? "none" : $"{NL}" + result.ErrorMessage) + $"{NL}" +
                    "FMData:" + $"{NL}" + result.FMData + $"{NL}";
            }

            logText += $"{NL}";

            return logText;
        }

        static FMInstallResult RollBackSingleFM(FMData fm, bool singleFMInList)
        {
            try
            {
                if (singleFMInList)
                {
                    Core.View.SetProgressBoxState_Single(
                        message1: LText.ProgressBox.CleaningUpFailedInstall,
                        progressType: ProgressType.Indeterminate,
                        cancelAction: NullAction
                    );
                }
                else
                {
                    Core.View.SetProgressBoxState_Double(
                        subMessage: LText.ProgressBox.CleaningUpFailedInstall,
                        subProgressType: ProgressType.Indeterminate
                    );
                }

                return RemoveFMFromDisk(fm);
            }
            finally
            {
                if (!singleFMInList)
                {
                    Core.View.SetProgressBoxState_Double(subProgressType: ProgressType.Determinate);
                }
            }
        }

        static List<FMInstallResult> RollBackMultipleFMs(List<FMData> fmDataList, int lastInstalledFMIndex)
        {
            List<FMInstallResult> results = new();

#if TIMING_TEST
            var sw = Stopwatch.StartNew();
#endif

            Core.View.SetProgressBoxState_Single(
                message1: LText.ProgressBox.CancelingInstall,
                percent: 100,
                progressType: ProgressType.Determinate,
                cancelAction: NullAction
            );

            for (int j = lastInstalledFMIndex; j >= 0; j--)
            {
                FMData fmData = fmDataList[j];

                Core.View.SetProgressBoxState_Single(
                    message2: fmData.FM.GetId(),
                    percent: GetPercentFromValue_Int(j + 1, lastInstalledFMIndex));

                FMInstallResult result = RemoveFMFromDisk(fmData);
                if (result.ResultType == InstallResultType.RollbackFailed)
                {
                    results.Add(result);
                }
            }

#if TIMING_TEST
            sw.Stop();
            Trace.WriteLine("Rollback: " + sw.Elapsed);
#endif

            return results;
        }

        static FMInstallResult RemoveFMFromDisk(FMData fmData)
        {
            if (DeleteFMInstalledDirectory(fmData.InstalledPath, fmData).ResultType != UninstallResultType.UninstallSucceeded)
            {
                // Don't log it here because the deleter method will already have logged it

                ArchiveType type =
                    fmData.FM.Archive.ExtIsZip() ? ArchiveType.Zip :
                    fmData.FM.Archive.ExtIs7z() ? ArchiveType.SevenZip :
                    ArchiveType.Rar;

                return new FMInstallResult(
                    fmData,
                    InstallResultType.RollbackFailed,
                    type,
                    $"Failed to delete the following directory:{NL}{NL}" + fmData.InstalledPath);
            }
            // This is going to get set based on this anyway at the next load from disk, might as well do it now
            fmData.FM.Installed = Directory.Exists(fmData.InstalledPath);

            return new FMInstallResult(fmData, InstallResultType.RollbackSucceeded);
        }
    }

    private static void DoPostInstallWork(
        FMData fmData,
        IOBufferPools ioBufferPools,
        CancellationToken cancellationToken,
        DarkLoaderBackupContext ctx,
        List<string> archivePaths,
        int fmDataListCount,
        List<ThreadablePath> threadablePaths)
    {
        bool single = fmDataListCount == 1;

        byte[] fmSelInfFileStreamBuffer = ioBufferPools.FileStream.Rent();
        try
        {
            string fileName = Path.Combine(fmData.InstalledPath, Paths.FMSelInf);
            using FileStream fs = GetWriteModeFileStreamWithCachedBuffer(fileName, overwrite: true, fmSelInfFileStreamBuffer);
            using var sw = new StreamWriter(fs);
            sw.WriteLine("Name=" + fmData.FM.InstalledDir);
            sw.WriteLine("Archive=" + fmData.FM.Archive);
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExCreate + Paths.FMSelInf + " in " + fmData.InstalledPath, ex);
        }
        finally
        {
            ioBufferPools.FileStream.Return(fmSelInfFileStreamBuffer);
        }

        // Only Dark engine games need audio conversion
        if (ValidAudioConvertibleFM.TryCreateFrom(fmData.FM, out ValidAudioConvertibleFM validAudioConvertibleFM))
        {
            try
            {
                if (single)
                {
                    Core.View.SetProgressBoxState_Single(
                        message1: LText.ProgressBox.ConvertingAudioFiles,
                        message2: "",
                        progressType: ProgressType.Indeterminate
                    );
                }
                else
                {
                    Core.View.SetProgressBoxState_Double(
                        subMessage: LText.ProgressBox.ConvertingAudioFiles,
                        subPercent: 100
                    );
                }

                ThreadingData audioConversionThreadingData =
                    GetLowestCommonThreadingData(threadablePaths.FilterToPostInstallWorkRelevant(fmData));

#if TIMING_TEST
                var audioConvertSW = Stopwatch.StartNew();
#endif

                // Dark engine games can't play MP3s, so they must be converted in all cases.
                // This one won't be called anywhere except during install, because it always runs during
                // install so there's no need to make it optional elsewhere. So we don't need to have a
                // check bool or anything.
                FMAudio.ConvertAsPartOfInstall(
                    validAudioConvertibleFM,
                    AudioConvert.MP3ToWAV,
                    audioConversionThreadingData,
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (Config.ConvertOGGsToWAVsOnInstall)
                {
                    FMAudio.ConvertAsPartOfInstall(
                        validAudioConvertibleFM,
                        AudioConvert.OGGToWAV,
                        audioConversionThreadingData,
                        cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

                if (Config.ConvertWAVsTo16BitOnInstall)
                {
                    FMAudio.ConvertAsPartOfInstall(
                        validAudioConvertibleFM,
                        AudioConvert.WAVToWAV16,
                        audioConversionThreadingData,
                        cancellationToken);
                }

#if TIMING_TEST
                audioConvertSW.Stop();
                Trace.WriteLine("CA: " + audioConvertSW.Elapsed + " (" + fmData.FM.GetId() + ")");
#endif

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                validAudioConvertibleFM.LogInfo(ErrorText.Ex + "in audio conversion", ex);
            }
        }

        // Don't be lazy about this; there can be no harm and only benefits by doing it right away
        GenerateMissFlagFileIfRequired(fmData.FM);

        if (single)
        {
            Core.View.SetProgressBoxState_Single(
                message1: LText.ProgressBox.RestoringBackup,
                message2: "",
                progressType: ProgressType.Indeterminate
            );
        }
        else
        {
            Core.View.SetProgressBoxState_Double(
                subMessage: LText.ProgressBox.RestoringBackup,
                subPercent: 100
            );
        }

        try
        {
            RestoreFM(
                ctx,
                fmData,
                archivePaths,
                ioBufferPools,
                cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log(ex: ex);
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static FMInstallResult InstallFMZip_ThreadedPerEntry(
        FMData fmData,
        int mainPercent,
        int fmCount,
        ZipContext_Pool zipCtxPool,
        ZipContext_Threaded_Pool zipCtxThreadedPool,
        IOBufferPools ioBufferPools,
        int threads)
    {
        bool single = fmCount == 1;

        string fmInstalledPath = fmData.InstalledPath.TrimEnd(CA_BS_FS) + "\\";

#if TIMING_TEST
        var overallSW = Stopwatch.StartNew();
#endif

        try
        {
            using ZipContextRentScope zipCtxRentScope = new(zipCtxPool);

            string fmDataArchivePath = fmData.ArchiveFilePath;

            Directory.CreateDirectory(fmInstalledPath);

            _installCts.Token.ThrowIfCancellationRequested();

#if TIMING_TEST
            var sw0 = Stopwatch.StartNew();
#endif

            ListFast<ZipArchiveFastEntry> entries =
                ZipArchiveFast.GetThreadableEntries(
                    fmDataArchivePath,
                    zipCtxRentScope.Ctx,
                    ioBufferPools.FileStream);

            _installCts.Token.ThrowIfCancellationRequested();

            // Create the entire directory tree beforehand, to avoid the possibility of race conditions during
            // file extraction. Normally we'd create each entry's directory tree on extract, which could easily
            // contain some or all of the same folders as other entries.
            ExtractableEntries entriesToExtract =
                Zip_CreateDirsAndGetExtractableEntries(entries, fmInstalledPath);

            int totalEntriesCount = entriesToExtract.TotalCount;
            int nonDuplicateEntriesCount = entriesToExtract.NonDuplicateEntries.Count;
            int duplicateEntriesCount = entriesToExtract.DuplicateEntries.Count;

#if TIMING_TEST
            Trace.WriteLine("Total entries count: " + totalEntriesCount);
            Trace.WriteLine("Threadable (non-duplicate) entries count: " + nonDuplicateEntriesCount);
            Trace.WriteLine("Duplicate entries count: " + duplicateEntriesCount);

            sw0.Stop();
            Trace.WriteLine("sw0: " + sw0.Elapsed);
#endif

            int entryNumber = 0;

            var uiThrottleSW = Stopwatch.StartNew();

#if TIMING_TEST
            var sw = Stopwatch.StartNew();
#endif

            int threadCount =
                nonDuplicateEntriesCount > 0
                    ? GetThreadCountForParallelOperation(nonDuplicateEntriesCount, threads)
                    : 1;

            if (nonDuplicateEntriesCount > 0)
            {
#if TIMING_TEST
                Trace.WriteLine(nameof(InstallFMZip_ThreadedPerEntry) + " thread count: " + threadCount);
#endif

                if (!TryGetParallelForData(threadCount, entriesToExtract.NonDuplicateEntries, _installCts.Token, out var pd))
                {
                    return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
                }

                Parallel.For(0, threadCount, pd.PO, _ =>
                {
                    byte[] fileStreamReadBuffer = ioBufferPools.FileStream.Rent();
                    byte[] fileStreamWriteBuffer = ioBufferPools.FileStream.Rent();

                    try
                    {
                        using FileStream fs = GetReadModeFileStreamWithCachedBuffer(fmDataArchivePath, fileStreamReadBuffer);
                        using ZipContextThreadedRentScope zipCtxThreadedRentScope = new(zipCtxThreadedPool, fs, fs.Length);

                        pd.PO.CancellationToken.ThrowIfCancellationRequested();

                        while (pd.CQ.TryDequeue(out ExtractableEntry entry))
                        {
                            ZipArchiveFast_Threaded.ExtractToFile_Fast(
                                entry: entry.Entry,
                                fileName: entry.ExtractedName,
                                overwrite: true,
                                unSetReadOnly: true,
                                context: zipCtxThreadedRentScope.Ctx,
                                fileStreamWriteBuffer: fileStreamWriteBuffer);

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();

                            int percent = GetPercentFromValue_Int(Interlocked.Increment(ref entryNumber), totalEntriesCount);
                            int newMainPercent = mainPercent + (percent / fmCount).ClampToZero();

                            if (uiThrottleSW.Elapsed.TotalMilliseconds > 4)
                            {
                                ReportExtractProgress(percent, newMainPercent, fmData, single);
                                uiThrottleSW.Restart();
                            }

                            pd.PO.CancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                    finally
                    {
                        ioBufferPools.FileStream.Return(fileStreamWriteBuffer);
                        ioBufferPools.FileStream.Return(fileStreamReadBuffer);
                    }
                });
            }

            /*
            Prevent "modified in outer scope" BS
            @MT_TASK: Does Parallel.For have a situation where its threads can still be running when the loop
             call finishes?! Because why else tf does ReSharper keep piping up about modifying captured variables
             after the damn loop is already finished?! WHO CARES, THE LOOP IS FINISHED! Argh!
            */
            int currentEntryNumber = entryNumber;

            if (duplicateEntriesCount > 0)
            {
                byte[] fileStreamReadBuffer = ioBufferPools.FileStream.Rent();
                byte[] fileStreamWriteBuffer = ioBufferPools.FileStream.Rent();

                using FileStream fs = GetReadModeFileStreamWithCachedBuffer(fmDataArchivePath, fileStreamReadBuffer);
                using ZipContextThreadedRentScope zipCtxThreadedRentScope = new(zipCtxThreadedPool, fs, fs.Length);

                _installCts.Token.ThrowIfCancellationRequested();

                List<ExtractableEntry> duplicateEntries = entriesToExtract.DuplicateEntries;
                for (int i = 0; i < duplicateEntries.Count; i++)
                {
                    ExtractableEntry entry = duplicateEntries[i];

                    ZipArchiveFast_Threaded.ExtractToFile_Fast(
                        entry: entry.Entry,
                        fileName: entry.ExtractedName,
                        overwrite: true,
                        unSetReadOnly: true,
                        context: zipCtxThreadedRentScope.Ctx,
                        fileStreamWriteBuffer: fileStreamWriteBuffer);

                    _installCts.Token.ThrowIfCancellationRequested();

                    int percent =
                        GetPercentFromValue_Int(Interlocked.Increment(ref currentEntryNumber), totalEntriesCount);
                    int newMainPercent = mainPercent + (percent / fmCount).ClampToZero();

                    if (uiThrottleSW.Elapsed.TotalMilliseconds > 4)
                    {
                        ReportExtractProgress(percent, newMainPercent, fmData, single);
                        uiThrottleSW.Restart();
                    }

                    _installCts.Token.ThrowIfCancellationRequested();
                }
            }

#if TIMING_TEST
            sw.Stop();
            Trace.WriteLine($"Zip extract threaded:{NL}" +
                            "    FM: " + fmData.FM.Archive + $"{NL}" +
                            "    Thread count: " + threadCount + $"{NL}" +
                            "    Total entries count: " + totalEntriesCount + $"{NL}" +
                            "    Threadable (non-duplicate) entries count: " + nonDuplicateEntriesCount + $"{NL}" +
                            "    Duplicate entries count: " + duplicateEntriesCount + $"{NL}" +
                            "    Initial read: " + sw0.Elapsed + $"{NL}" +
                            "    Full archive threaded extract: " + sw.Elapsed);
#endif

            return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log(ErrorText.Ex + "while installing zip " + fmData.ArchiveFilePath + " to " + fmInstalledPath, ex);
            return new FMInstallResult(
                fmData,
                InstallResultType.InstallFailed,
                ArchiveType.Zip,
                LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially,
                ex);
        }
#if TIMING_TEST
        finally
        {
            Trace.WriteLine(nameof(InstallFMZip_ThreadedPerEntry) + " (" + fmData.FM.Archive + "): " + overallSW.Elapsed);
        }
#endif
    }

    private static FMInstallResult
    InstallFMZip(
        FMData fmData,
        int mainPercent,
        int fmCount,
        IOBufferPools ioBufferPools,
        ZipContext_Pool zipCtxPool)
    {
        bool single = fmCount == 1;

        string fmInstalledPath = fmData.InstalledPath.TrimEnd(CA_BS_FS) + "\\";

#if TIMING_TEST
        var sw = Stopwatch.StartNew();
#endif
        try
        {
            Directory.CreateDirectory(fmInstalledPath);

            using ZipContextRentScope zipCtxRentScope = new(zipCtxPool);

            byte[] fileStreamReadBuffer = ioBufferPools.FileStream.Rent();
            byte[] fileStreamWriteBuffer = ioBufferPools.FileStream.Rent();
            try
            {
                using ZipArchiveFast archive =
                    GetReadModeZipArchiveCharEnc_Fast(
                        fmData.ArchiveFilePath,
                        fileStreamReadBuffer,
                        zipCtxRentScope.Ctx);

                /*
                We don't need to pre-run this on this codepath, but it doesn't hurt anything and reduces code
                duplication, plus we get the benefit of the large reduction in Directory.CreateDirectory() calls.
                */
                ExtractableEntries entriesToExtract =
                    Zip_CreateDirsAndGetExtractableEntries(archive.Entries, fmInstalledPath);

                int entriesTotalCount = entriesToExtract.TotalCount;
                for (int i = 0; i < entriesTotalCount; i++)
                {
                    ExtractableEntry entry = entriesToExtract[i];

                    archive.ExtractToFile_Fast(
                        entry: entry.Entry,
                        fileName: entry.ExtractedName,
                        overwrite: true,
                        unSetReadOnly: true,
                        fileStreamWriteBuffer: fileStreamWriteBuffer);

                    _installCts.Token.ThrowIfCancellationRequested();

                    int percent = GetPercentFromValue_Int(i + 1, entriesTotalCount);
                    int newMainPercent = mainPercent + (percent / fmCount).ClampToZero();

                    ReportExtractProgress(percent, newMainPercent, fmData, single);

                    _installCts.Token.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                ioBufferPools.FileStream.Return(fileStreamWriteBuffer);
                ioBufferPools.FileStream.Return(fileStreamReadBuffer);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log(ErrorText.Ex + "while installing zip " + fmData.ArchiveFilePath + " to " + fmInstalledPath, ex);
            return new FMInstallResult(
                fmData,
                InstallResultType.InstallFailed,
                ArchiveType.Zip,
                LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially,
                ex);
        }
#if TIMING_TEST
        finally
        {
            Trace.WriteLine(nameof(InstallFMZip) + " (" + fmData.FM.Archive + "): " + sw.Elapsed);
        }
#endif

        return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
    }

    private readonly struct ExtractableEntry
    {
        internal readonly ZipArchiveFastEntry Entry;
        internal readonly string ExtractedName;

        public ExtractableEntry(ZipArchiveFastEntry entry, string extractedName)
        {
            Entry = entry;
            ExtractedName = extractedName;
        }
    }

    private readonly ref struct ExtractableEntries
    {
        internal readonly List<ExtractableEntry> NonDuplicateEntries;
        internal readonly List<ExtractableEntry> DuplicateEntries;

        public ExtractableEntry this[int index]
        {
            get
            {
                int nonDuplicateEntriesCount = NonDuplicateEntries.Count;
                return index > nonDuplicateEntriesCount - 1
                    ? DuplicateEntries[index - nonDuplicateEntriesCount]
                    : NonDuplicateEntries[index];
            }
        }

        internal int TotalCount => NonDuplicateEntries.Count + DuplicateEntries.Count;

        public ExtractableEntries(List<ExtractableEntry> nonDuplicateEntries, List<ExtractableEntry> duplicateEntries)
        {
            NonDuplicateEntries = nonDuplicateEntries;
            DuplicateEntries = duplicateEntries;
        }
    }

    private static ExtractableEntries
    Zip_CreateDirsAndGetExtractableEntries(
        ListFast<ZipArchiveFastEntry> entries,
        string fmInstalledPath)
    {
#if TIMING_TEST
        var sw = Stopwatch.StartNew();
#endif
        // Expect the common case of no duplicate entries
        List<ExtractableEntry> nonDuplicateEntries = new(entries.Count);
        List<ExtractableEntry> duplicateEntries = new(0);

        List<string> dirEntries = new(entries.Count);

        HashSetPathI extractedNamesHash = new(entries.Count);

        foreach (ZipArchiveFastEntry entry in entries)
        {
            string fileName = entry.FullName;

            if (fileName.IsEmpty()) continue;

            string extractedName = GetExtractedNameOrThrowIfMalicious(fmInstalledPath, fileName);

            if (fileName.EndsWithDirSep())
            {
                dirEntries.Add(fileName.TrimEnd(CA_BS_FS));
            }
            else
            {
                if (fileName.Rel_ContainsDirSep())
                {
                    string dir = fileName.Substring(0, fileName.Rel_LastIndexOfDirSep());
                    dirEntries.Add(dir);
                }

                if (extractedNamesHash.Add(extractedName))
                {
                    nonDuplicateEntries.Add(new ExtractableEntry(entry, extractedName));
                }
                else
                {
                    duplicateEntries.Add(new ExtractableEntry(entry, extractedName));
                }
            }
        }

        // This still results in some amount of duplicate disk hitting, but far, far less than before where
        // we'd do it for every single entry that wasn't in the base dir.
        // For TROTB2, it's 231 vs. ~7000 Directory.Create() calls.
        IEnumerable<string> dirEntriesDistinct = dirEntries.Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (string dir in dirEntriesDistinct)
        {
            Directory.CreateDirectory(Path.Combine(fmInstalledPath, dir));
        }

#if TIMING_TEST
        sw.Stop();
        Trace.WriteLine(nameof(Zip_CreateDirsAndGetExtractableEntries) + ": " + sw.Elapsed);
#endif

        return new ExtractableEntries(nonDuplicateEntries, duplicateEntries);
    }

    private static FMInstallResult
    InstallFMRar(
        FMData fmData,
        int mainPercent,
        int fmCount,
        IOBufferPools ioBufferPools)
    {
        bool single = fmCount == 1;

        string fmInstalledPath = fmData.InstalledPath.TrimEnd(CA_BS_FS) + "\\";

        byte[] fileStreamWriteBuffer = ioBufferPools.FileStream.Rent();
        try
        {
            Directory.CreateDirectory(fmInstalledPath);

            using var fs = File_OpenReadFast(fmData.ArchiveFilePath);
            int entriesCount;
            using (var archive = RarArchive.Open(fs))
            {
                entriesCount = archive.Entries.Count;
                fs.Position = 0;
            }

            using var reader = RarReader.Open(fs);

            int i = -1;
            while (reader.MoveToNextEntry())
            {
                i++;
                RarReaderEntry entry = reader.Entry;
                string fileName = entry.Key;

                if (!fileName.IsEmpty())
                {
                    string extractedName = GetExtractedNameOrThrowIfMalicious(fmInstalledPath, fileName);

                    if (entry.IsDirectory)
                    {
                        Directory.CreateDirectory(extractedName);
                    }
                    else
                    {
                        if (fileName.Rel_ContainsDirSep())
                        {
                            Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                fileName.Substring(0, fileName.Rel_LastIndexOfDirSep())));
                        }

                        reader.ExtractToFile_Fast(extractedName, overwrite: true, ioBufferPools);

                        File_UnSetReadOnly(extractedName);
                    }

                    _installCts.Token.ThrowIfCancellationRequested();
                }

                int percentOfEntries = GetPercentFromValue_Int(i, entriesCount).Clamp(0, 100);
                int newMainPercent = mainPercent + (percentOfEntries / fmCount).ClampToZero();

                if (!_installCts.IsCancellationRequested)
                {
                    ReportExtractProgress(percentOfEntries, newMainPercent, fmData, single);
                }
            }

            _installCts.Token.ThrowIfCancellationRequested();

            return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log("Error extracting rar " + fmData.ArchiveFilePath + " to " + fmInstalledPath + $"{NL}", ex);
            return new FMInstallResult(
                fmData,
                InstallResultType.InstallFailed,
                ArchiveType.Rar,
                LText.AlertMessages.Extract_RarExtractFailedFullyOrPartially,
                ex);
        }
        finally
        {
            ioBufferPools.FileStream.Return(fileStreamWriteBuffer);
        }
    }

    private static FMInstallResult
    InstallFMSevenZip(
        FMData fmData,
        int mainPercent,
        int fmCount)
    {
        bool single = fmCount == 1;

        string fmInstalledPath = fmData.InstalledPath;

        try
        {
            Directory.CreateDirectory(fmInstalledPath);

            int entriesCount;

            using (var fs = File_OpenReadFast(fmData.ArchiveFilePath))
            {
                var extractor = new SevenZipArchive(fs);
                entriesCount = extractor.GetEntryCountOnly();
            }

            void ReportProgress(Fen7z.ProgressReport pr)
            {
                int newMainPercent = mainPercent + (pr.PercentOfEntries / fmCount).ClampToZero();

                if (!pr.Canceling)
                {
                    ReportExtractProgress(pr.PercentOfEntries, newMainPercent, fmData, single);
                }
            }

            var progress = new Progress<Fen7z.ProgressReport>(ReportProgress);

            Fen7z.Result result = Fen7z.Extract(
                Paths.SevenZipPath,
                Paths.SevenZipExe,
                fmData.ArchiveFilePath,
                fmInstalledPath,
                cancellationToken: _installCts.Token,
                entriesCount,
                progress: progress
            );

            if (result.ErrorOccurred)
            {
                Log("Error extracting 7z " + fmData.ArchiveFilePath + " to " + fmInstalledPath + $"{NL}" + result);

                return new FMInstallResult(
                    fmData,
                    InstallResultType.InstallFailed,
                    ArchiveType.SevenZip,
                    LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially + $"{NL}" +
                    result.ErrorText,
                    result.Exception);
            }

            if (!result.Canceled)
            {
                foreach (string file in Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                {
                    File_UnSetReadOnly(file);
                }
            }

            if (result.Canceled)
            {
                // MUST pass the token or else the exception doesn't get caught and the whole app crashes.
                // Best guess is the Parallel.For needs the token in order to throw the exception outside the
                // threading or however the hell it works. Whatever man, fixed, moving on.
                throw new OperationCanceledException(_installCts.Token);
            }
            else
            {
                return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log("Error extracting 7z " + fmData.ArchiveFilePath + " to " + fmInstalledPath + $"{NL}", ex);
            return new FMInstallResult(
                fmData,
                InstallResultType.InstallFailed,
                ArchiveType.SevenZip,
                LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially,
                ex);
        }
    }

    private static void ReportExtractProgress(int percent, int newMainPercent, FMData fmData, bool single)
    {
        if (single)
        {
            Core.View.SetProgressPercent(percent);
        }
        else
        {
            Core.View.SetProgressBoxState_Double(
                mainPercent: newMainPercent,
                subMessage: fmData.FM.Archive,
                subPercent: percent
            );
        }
    }

    #endregion

    #region Uninstall

    /*
    If the user clicks "Stop", we may be in the middle of several delete operations and they're all going to
    finish before the operation stops. Whereas if we're single-threaded, we'll stop immediately after the current
    FM. This delayed-cancel behavior is fine for cancel-semantics (revertible) operations, but stop-semantics
    (non-revertible) operations don't play as well with it. This is also a convenient excuse to sidestep the
    entire set of difficulties we were having with multithreading this thing. Shame to lose performance, but eh...
    */
    internal static async Task<(bool Success, bool AtLeastOneFMMarkedUnavailable)>
    Uninstall(FanMission[] fms, bool doEndTasks = true)
    {
        using var dsw = new DisableScreenshotWatchers();

        var fail = (false, false);

        var fmDataList = new List<FMData>(fms.Length);

        bool single = fms.Length == 1;

        bool doBackup;

        // Do checks first before progress box so it's not just annoyingly there while in confirmation dialogs
        #region Checks

        try
        {
            Core.View.SetWaitCursor(true);

            if (!DoPreChecks(
                    fms,
                    fmDataList,
                    install: false,
                    out _,
                    out _))
            {
                return fail;
            }
        }
        finally
        {
            Core.View.SetWaitCursor(false);
        }

        #endregion

        #region Confirm uninstall

        if (Config.ConfirmUninstall)
        {
            (MBoxButton result, bool dontAskAgain) = Core.Dialogs.ShowMultiChoiceDialog(
                message: single
                    ? LText.AlertMessages.Uninstall_Confirm
                    : LText.AlertMessages.Uninstall_Confirm_Multiple,
                title: LText.AlertMessages.Confirm,
                icon: MBoxIcon.Warning,
                yes: LText.AlertMessages.Uninstall,
                no: LText.Global.Cancel,
                checkBoxText: LText.AlertMessages.DontAskAgain);

            if (result == MBoxButton.No) return fail;

            Config.ConfirmUninstall = !dontAskAgain;
        }

        #endregion

        #region Confirm backup

        if (Config.BackupAlwaysAsk)
        {
            string message = Config.BackupFMData == BackupFMData.SavesAndScreensOnly
                ? LText.AlertMessages.Uninstall_BackupSavesAndScreenshots
                : LText.AlertMessages.Uninstall_BackupAllData;
            (MBoxButton result, bool dontAskAgain) =
                Core.Dialogs.ShowMultiChoiceDialog(
                    message: message + $"{NL}{NL}" + LText.AlertMessages.Uninstall_BackupChooseNoNote,
                    title: LText.AlertMessages.Confirm,
                    icon: MBoxIcon.None,
                    yes: LText.AlertMessages.BackUp,
                    no: LText.AlertMessages.DontBackUp,
                    cancel: LText.Global.Cancel,
                    checkBoxText: LText.AlertMessages.DontAskAgain
                );

            if (result == MBoxButton.Cancel) return fail;

            Config.BackupAlwaysAsk = !dontAskAgain;
            doBackup = result == MBoxButton.Yes;
        }
        else
        {
            doBackup = true;
        }

        #endregion

        bool atLeastOneFMMarkedUnavailable = false;
        try
        {
            return await Task.Run(() =>
            {
                _uninstallCts = _uninstallCts.Recreate();

                if (single)
                {
                    Core.View.SetProgressBoxState_Single(
                        visible: true,
                        message1: LText.ProgressBox.UninstallingFM,
                        progressType: ProgressType.Indeterminate
                    );
                }
                else
                {
                    Core.View.SetProgressBoxState_Single(
                        visible: true,
                        message1: LText.ProgressBox.UninstallingFMs,
                        progressType: ProgressType.Determinate,
                        cancelMessage: LText.Global.Stop,
                        cancelAction: CancelUninstallToken
                    );
                }

                bool skipUninstallWithNoArchiveWarning = false;

                #region Pre-check for missing archive and ask the user what to do

                for (int i = 0; i < fmDataList.Count; i++)
                {
                    FMData fmData = fmDataList[i];

                    if (fmData.ArchiveFilePath.IsEmpty())
                    {
                        // Match previous behavior: any not-really-installed FM gets rejected silently first.
                        // Put it inside the empty-archive check so as to minimize the number of times we hit the
                        // disk sequentially (since we'll be parallelizing below).
                        if (!Directory.Exists(fmData.InstalledPath))
                        {
                            fmData.FM.Installed = false;
                            fmData.Uninstall_SkipUninstallingThisFM = true;
                            continue;
                        }

                        if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);

                        if (doEndTasks && !skipUninstallWithNoArchiveWarning)
                        {
                            (MBoxButton result, _) = Core.Dialogs.ShowMultiChoiceDialog(
                                message: fmData.FM.GetId() + $"{NL}{NL}" +
                                         LText.AlertMessages.Uninstall_ArchiveNotFound,
                                title: LText.AlertMessages.Warning,
                                icon: MBoxIcon.Warning,
                                yes: single ? LText.AlertMessages.Uninstall : LText.AlertMessages.UninstallAll,
                                no: LText.Global.Skip,
                                cancel: LText.Global.Cancel,
                                defaultButton: MBoxButton.No);

                            if (result == MBoxButton.Cancel) return (false, atLeastOneFMMarkedUnavailable);
                            if (result == MBoxButton.No)
                            {
                                fmData.Uninstall_SkipUninstallingThisFM = true;
                                continue;
                            }

                            if (!single) skipUninstallWithNoArchiveWarning = true;
                        }
                        fmData.Uninstall_MarkFMAsUnavailable = true;
                        atLeastOneFMMarkedUnavailable = true;
                    }
                }

                #endregion

                byte[]? fileStreamBuffer = null;

                DarkLoaderBackupContext ctx = new();

#if TIMING_TEST
                var totalDeleteSW = Stopwatch.StartNew();
#endif

                for (int i = 0; i < fmDataList.Count; i++)
                {
                    if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);

                    FMData fmData = fmDataList[i];

                    if (fmData.Uninstall_SkipUninstallingThisFM) continue;

                    FanMission fm = fmData.FM;

                    if (!Directory.Exists(fmData.InstalledPath))
                    {
                        fm.Installed = false;
                        continue;
                    }

                    if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);

                    /*
                    If fm.Archive is blank, then fm.InstalledDir will be used for the backup file name instead.
                    This file will be included in the search when restoring, and the newest will be taken as
                    usual.

                    fm.Archive can be blank at this point when all of the following conditions are true:
                    -fm is installed
                    -fm does not have fmsel.inf in its installed folder (or its fmsel.inf is blank or invalid)
                    -fm was not in the database on startup
                    -the folder where the FM's archive is located is not in Config.FMArchivePaths (or its sub-
                     folders if that option is enabled)

                    It's not particularly likely, but it could happen if the user had NDL-installed FMs (which
                    don't have fmsel.inf), started AngelLoader for the first time, didn't specify the right
                    archive folder on initial setup, and hasn't imported from NDL by this point.
                    */

                    if (doBackup)
                    {
                        try
                        {
                            BackupFM(ctx, fmData, fileStreamBuffer ??= new byte[FileStreamBufferSize]);
                        }
                        catch (Exception ex)
                        {
                            fmData.FM.LogInfo(ErrorText.ExTry + "back up FM", ex);
                            (MBoxButton buttonPressed, _) = Core.Dialogs.ShowMultiChoiceDialog(
                                message:
                                fm.InstalledDir + $":{NL}{NL}" +
                                LText.AlertMessages.Uninstall_BackupError,
                                title: LText.AlertMessages.Alert,
                                icon: MBoxIcon.Warning,
                                yes: LText.AlertMessages.Uninstall_BackupError_KeepInstalled,
                                no: LText.AlertMessages.Uninstall_BackupError_UninstallWithoutBackup,
                                cancel: LText.Global.Cancel,
                                noIsDangerous: true,
                                defaultButton: MBoxButton.Yes,
                                viewLogButtonVisible: true);

                            if (buttonPressed == MBoxButton.Yes)
                            {
                                continue;
                            }
                            else if (buttonPressed == MBoxButton.Cancel)
                            {
                                return (false, atLeastOneFMMarkedUnavailable);
                            }
                        }
                    }

                    if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);

                    // TODO: Give the user the option to retry or something, if it's cause they have a file open
                    // Make option to open the folder in Explorer and delete it manually?
                    FMUninstallResult result = DeleteFMInstalledDirectory(fmData.InstalledPath, fmData);
                    if (result.ResultType != UninstallResultType.UninstallSucceeded)
                    {
                        fm.LogInfo(ErrorText.Un + "delete FM installed directory." + $"{NL}" +
                                   "Error message: " + result.ErrorMessage + $"{NL}" +
                                   "Result type: " + result.ResultType, ex: result.Exception);
                        Core.Dialogs.ShowError(
                            LText.AlertMessages.Uninstall_FailedFullyOrPartially + $"{NL}{NL}" +
                            "FM: " + fm.GetId());
                    }

                    fm.Installed = false;
                    if (fmData.Uninstall_MarkFMAsUnavailable) fm.MarkedUnavailable = true;

                    if (!single)
                    {
                        Core.View.SetProgressBoxState_Single(
                            message2: fm.GetId(),
                            percent: GetPercentFromValue_Int(i + 1, fmDataList.Count)
                        );
                    }

                    if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);
                }

#if TIMING_TEST
                totalDeleteSW.Stop();
                Trace.WriteLine("Total delete time: " + totalDeleteSW.Elapsed);
#endif

                return (true, atLeastOneFMMarkedUnavailable);
            });
        }
        catch (Exception ex)
        {
            Log(ErrorText.Ex + " during uninstall.", ex);
            Core.Dialogs.ShowError(
                LText.AlertMessages.Uninstall_Error,
                LText.AlertMessages.Alert,
                icon: MBoxIcon.Warning);
            return (false, atLeastOneFMMarkedUnavailable);
        }
        finally
        {
            Ini.WriteFullFMDataIni();
            if (doEndTasks)
            {
                Core.View.HideProgressBox();

                await DoUninstallEndTasks(atLeastOneFMMarkedUnavailable);
            }

            _uninstallCts.Dispose();
        }
    }

    internal static Task DoUninstallEndTasks(bool atLeastOneFMMarkedUnavailable)
    {
        // If any FMs are gone, refresh the list to remove them. Otherwise, don't refresh the list because
        // then the FMs might move in the list if we're sorting by installed state.
        if (atLeastOneFMMarkedUnavailable && !Core.View.GetShowUnavailableFMsFilter())
        {
            return Core.View.SortAndSetFilter(keepSelection: true);
        }
        else
        {
            Core.View.RefreshAllSelectedFMs_UpdateInstallState();
            return Task.CompletedTask;
        }
    }

    private static FMUninstallResult DeleteFMInstalledDirectory(string path, FMData fmData)
    {
        if (!Directory.Exists(path))
        {
            return new FMUninstallResult(UninstallResultType.UninstallSucceeded);
        }

        List<ThreadablePath> paths = GetDeleteInstalledDirRelevantPaths(path, fmData.GameIndex);
        ThreadingData threadingData = GetLowestCommonThreadingData(paths);
        try
        {
#if TIMING_TEST
            var sw = Stopwatch.StartNew();
#endif

            Delete_Threaded.Delete(path, recursive: true, threadingData.Threads);

#if TIMING_TEST
            sw.Stop();
            Trace.WriteLine("**** " + nameof(DeleteFMInstalledDirectory) + " Delete: " + sw.Elapsed);
#endif
            return new FMUninstallResult(UninstallResultType.UninstallSucceeded);
        }
        catch (Exception mainEx)
        {
            Log(ErrorText.FTDel + "FM path '" + path + "', attempting to remove readonly attributes and trying again...", mainEx);
            try
            {
                // FMs installed by us will not have any readonly attributes set, so we work on the assumption
                // that this is the rarer case and only do this extra work if we need to.
                DirAndFileTree_UnSetReadOnly(path, throwException: true);
            }
            catch (Exception removeReadOnlyAttributesEx)
            {
                Log(ErrorText.FT + "remove readonly attributes.", removeReadOnlyAttributesEx);
            }

            try
            {
                Delete_Threaded.Delete(path, recursive: true, threadingData.Threads);
                Log("Delete of '" + path + "' succeeded after removing readonly attributes.");
                return new FMUninstallResult(UninstallResultType.UninstallSucceeded);
            }
            catch (Exception retryDeleteEx)
            {
                string msg = ErrorText.FTDel + "FM path '" + path + "' twice, giving up...";
                Log(msg, retryDeleteEx);
                return new FMUninstallResult(UninstallResultType.UninstallFailed, msg, retryDeleteEx);
            }
        }
    }

    #endregion

    #endregion
}

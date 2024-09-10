// @MT_TASK: Comment this out for final release
#define TIMING_TEST

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
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
#pragma warning disable IDE0001
#pragma warning disable IDE0002
    // ReSharper disable RedundantNameQualifier
    private static readonly System.Diagnostics.Stopwatch _timingTestStopWatch = new();

    private static void StartTiming()
    {
        _timingTestStopWatch.Restart();
    }

    private static void StopTimingAndPrintResult()
    {
        _timingTestStopWatch.Stop();
        System.Diagnostics.Trace.WriteLine(_timingTestStopWatch.Elapsed);
    }
    // ReSharper restore RedundantNameQualifier
#pragma warning restore IDE0002
#pragma warning restore IDE0001
#endif

    #region Private fields

    private enum InstallResultType
    {
        InstallSucceeded,
        InstallFailed,
        RollbackSucceeded,
        RollbackFailed,
    }

    private enum ArchiveType
    {
        Zip,
        Rar,
        SevenZip,
    }

    // @MT_TASK: We may want other things in here like fm archive path/install path etc.
    private readonly struct FMInstallResult
    {
        internal readonly FMData FMData;
        internal readonly InstallResultType ResultType;
        internal readonly ArchiveType ArchiveType;
        internal readonly string ErrorMessage;

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
    }

    private enum PlaySource
    {
        OriginalGame,
        Editor,
        FM,
    }

    private static readonly byte[] _DARKMISS_Bytes = "DARKMISS"u8.ToArray();

    private static Encoding? _utf8NoBOM;
    private static Encoding UTF8NoBOM => _utf8NoBOM ??= new UTF8Encoding(false, true);

    private static CancellationTokenSource _installCts = new();

    private static void CancelInstallToken()
    {
        _installCts.CancelIfNotDisposed();
        Core.View.Invoke(ShowCancelingInstallMessage);
    }

    private static void ShowCancelingInstallMessage()
    {
        if (Core.View.MultiItemProgress_Visible())
        {
            Core.View.MultiItemProgress_SetState(
                message1: LText.ProgressBox.CancelingInstall,
                mainProgressMessage: "");
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
        if we're being launched through steam we read and act on it as usual, but if we're not, then
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
                    using FileStream fs = File_OpenReadFast(misFile);

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
                using FileStream fs = File_OpenReadFast(smallestUsedMisFile);

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
    private static (bool Success, List<string> ArchivePaths)
    DoPreChecks(FanMission[] fms, List<FMData> fmDataList, bool install)
    {
        var fail = (false, new List<string>());

        bool single = fms.Length == 1;

        bool[] gamesChecked = new bool[SupportedGameCount];

        List<string> archivePaths = FMArchives.GetFMArchivePaths();

        for (int i = 0; i < fms.Length; i++)
        {
            FanMission fm = fms[i];

            AssertR(install ? !fm.Installed : fm.Installed, "fm.Installed == " + fm.Installed);

            if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
            {
                fm.LogInfo(ErrorText.FMGameU, stackTrace: true);
                Core.Dialogs.ShowError(fm.GetId() + $"{NL}" + ErrorText.FMGameU);
                return fail;
            }

            int intGameIndex = (int)gameIndex;

            if (Canceled(install)) return fail;

            string fmArchivePath = FMArchives.FindFirstMatch(fm.Archive, archivePaths);

            if (Canceled(install)) return fail;

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

                    return fail;
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

                        return fail;
                    }

                    if (Canceled(install)) return fail;

                    if (!Directory.Exists(instBasePath))
                    {
                        fm.LogInfo("FM install path not found.");

                        Core.Dialogs.ShowError(gameName + $":{NL}" +
                                               fm.GetId() + $"{NL}" +
                                               LText.AlertMessages.Install_FMInstallPathNotFound);

                        return fail;
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

                    return fail;
                }

                if (Canceled(install)) return fail;

                if (GameIsRunning(gameExe))
                {
                    Core.Dialogs.ShowAlert(
                        !single
                            ? LText.AlertMessages.OneOrMoreGamesAreRunning
                            : gameName + $":{NL}" + (install
                                ? LText.AlertMessages.Install_GameIsRunning
                                : LText.AlertMessages.Uninstall_GameIsRunning),
                        LText.AlertMessages.Alert);

                    return fail;
                }

                if (Canceled(install)) return fail;

                gamesChecked[intGameIndex] = true;
            }
        }

        return (true, archivePaths);

        static bool Canceled(bool install) => install && _installCts.IsCancellationRequested;
    }

    #region Install

    private sealed class FMData
    {
        internal readonly FanMission FM;
        internal readonly string ArchivePath;
        internal readonly string InstBasePath;
        internal readonly string InstalledPath;
        internal readonly GameIndex GameIndex;
        internal int ViewItemIndex;
        internal bool InstallStarted;

        public FMData(FanMission fm, string archivePath, string instBasePath, GameIndex gameIndex)
        {
            FM = fm;
            ArchivePath = archivePath;
            InstBasePath = instBasePath;
            InstalledPath = Path.Combine(instBasePath, fm.InstalledDir);
            GameIndex = gameIndex;
        }
    }

    private sealed class Buffers
    {
        private byte[]? _extractBuffer;
        private byte[]? _fileStreamBuffer;

        internal byte[] ExtractTempBuffer => _extractBuffer ??= new byte[StreamCopyBufferSize];
        internal byte[] FileStreamBuffer => _fileStreamBuffer ??= new byte[FileStreamBufferSize];
    }

    internal static async Task<bool> Install(params FanMission[] fms)
    {
        using var dsw = new DisableScreenshotWatchers();
        return await InstallInternal(false, false, fms);
    }

    private sealed class ProgressReport_Install
    {
        internal int ViewItemIndex;
        internal string Text = "";
        internal int Percent;
    }

    // @MT_TASK(InstallInternal): Multithread this
    /*
    @MT_TASK: We're getting very poor scaling here.
    Speedup ranges from none to 2x for 12 threads, and this is SSD to other SSD.

    We should do some test code (not in this class) that just extracts archives in parallel and look at the perf
    there, devoid of all the mess in here so we can see the best case clearly.

    However, it might just be that extracting a ton of small files is just not going to get good perf or parallelization
    even from an SSD. It's possible an NVME SSD might handle it better, but I don't have the equipment to test
    that at the moment.
    */
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

        var reportThrottleSW = new Stopwatch();
        reportThrottleSW.Start();

        var progress = new Progress<ProgressReport_Install>(ReportProgress_Install);

        _installCts = _installCts.Recreate();

        //Core.View.ShowProgressBox_Single(
        //    message1: LText.ProgressBox.PreparingToInstall,
        //    progressType: ProgressType.Indeterminate,
        //    cancelAction: CancelInstallToken
        //);

        Core.View.MultiItemProgress_Show(
            message1: LText.ProgressBox.PreparingToInstall,
            cancelMessage: LText.Global.Cancel,
            cancelAction: CancelInstallToken);

        return await Task.Run(async () =>
        {
            try
            {
                (bool success, List<string> archivePaths) = DoPreChecks(fms, fmDataList, install: true);

                if (!success) return false;
                if (fmDataList.Count == 0) return false;

                //Core.View.SetProgressBoxState(
                //    size: single ? ProgressSizeMode.Single : ProgressSizeMode.Double,
                //    mainMessage1: single ? LText.ProgressBox.InstallingFM : LText.ProgressBox.InstallingFMs,
                //    mainMessage2: "",
                //    mainPercent: 0,
                //    mainProgressType: ProgressType.Determinate,
                //    subMessage: "",
                //    subPercent: 0,
                //    subProgressType: ProgressType.Determinate,
                //    cancelMessage: LText.Global.Cancel
                //);

                (string Line1, string Line2)[] fmInstallInitialItems = new (string Line1, string Line2)[fmDataList.Count];

                for (int i = 0; i < fmDataList.Count; i++)
                {
                    fmDataList[i].ViewItemIndex = i;
                    fmInstallInitialItems[i].Line1 = fmDataList[i].FM.Archive;
                    fmInstallInitialItems[i].Line2 = LText.ProgressBox.Queued;
                }

                int fmsFinishedCount = 0;

                Core.View.MultiItemProgress_SetState(
                    initialRowTexts: fmInstallInitialItems,
                    message1: fmDataList.Count == 1 ? LText.ProgressBox.InstallingFM : LText.ProgressBox.InstallingFMs,
                    mainProgressMessage: fmsFinishedCount.ToStrCur() + " / " + fmDataList.Count,
                    cancelMessage: LText.Global.Cancel,
                    cancelAction: CancelInstallToken);

                ConcurrentQueue<FMData> cq = new(fmDataList);
                ConcurrentBag<FMInstallResult> results = new();

                int threadCount =
                    //1;
                    GetThreadCountForParallelOperation(fmDataList.Count);

                // @MT_TASK: Remove for final release
                Trace.WriteLine(threadCount);

                ParallelOptions po = new()
                {
                    CancellationToken = _installCts.Token,
                    MaxDegreeOfParallelism = threadCount,
                };

#if TIMING_TEST
                StartTiming();
#endif

                try
                {
                    Parallel.For(0, threadCount, po, i =>
                    {
                        Buffers buffer = new();
                        BinaryBuffer binaryBuffer = new();

                        while (cq.TryDequeue(out FMData fmData))
                        {
                            int viewItemIndex = fmData.ViewItemIndex;
                            fmData.InstallStarted = true;

                            // Framework zip extracting is much faster, so use it if possible
                            // 2022-07-25: This may or may not be the case anymore now that we use 7z.exe
                            // But we don't want to parse out stupid console output for error detection and junk
                            // if we don't have to so whatever.

                            FMInstallResult fmInstallResult =
                                fmData.ArchivePath.ExtIsZip() ? InstallFMZip(
                                    viewItemIndex,
                                    progress,
                                    fmData,
                                    buffer.ExtractTempBuffer,
                                    buffer.FileStreamBuffer) :
                                fmData.ArchivePath.ExtIsRar() ? InstallFMRar(
                                    viewItemIndex,
                                    progress,
                                    fmData,
                                    buffer.ExtractTempBuffer) :
                                InstallFMSevenZip(
                                    viewItemIndex,
                                    progress,
                                    fmData);

                            // @MT_TASK: Rolling back needs re-architecting for multithreading
                            if (fmInstallResult.ResultType != InstallResultType.InstallSucceeded)
                            {
                                FMInstallResult result = RollBackSingleFM(fmData);
                                results.Add(result);
                                continue;
                            }

                            results.Add(fmInstallResult);

                            po.CancellationToken.ThrowIfCancellationRequested();

                            try
                            {
                                using var sw = new StreamWriter(Path.Combine(fmData.InstalledPath, Paths.FMSelInf));
                                sw.WriteLine("Name=" + fmData.FM.InstalledDir);
                                sw.WriteLine("Archive=" + fmData.FM.Archive);
                            }
                            catch (Exception ex)
                            {
                                Log(ErrorText.ExCreate + Paths.FMSelInf + " in " + fmData.InstalledPath, ex);
                            }

                            // Only Dark engine games need audio conversion
                            if (ValidAudioConvertibleFM.TryCreateFrom(fmData.FM, out ValidAudioConvertibleFM validAudioConvertibleFM))
                            {
                                try
                                {
                                    //if (single)
                                    //{
                                    //    Core.View.SetProgressBoxState_Single(
                                    //        message1: LText.ProgressBox.ConvertingAudioFiles,
                                    //        message2: "",
                                    //        progressType: ProgressType.Indeterminate
                                    //    );
                                    //}
                                    //else
                                    //{
                                    //    Core.View.SetProgressBoxState_Double(
                                    //        subMessage: LText.ProgressBox.ConvertingAudioFiles,
                                    //        subPercent: 100
                                    //    );
                                    //}

                                    Core.View.MultiItemProgress_SetItemData(
                                        index: viewItemIndex,
                                        line2: LText.ProgressBox.ConvertingAudioFiles,
                                        percent: 100,
                                        progressType: ProgressType.Indeterminate);

#if TIMING_TEST
#pragma warning disable IDE0001
#pragma warning disable IDE0002
                                    // ReSharper disable RedundantNameQualifier
                                    var audioConvertSW = new System.Diagnostics.Stopwatch();
                                    // ReSharper restore RedundantNameQualifier
#pragma warning restore IDE0002
#pragma warning restore IDE0001
                                    audioConvertSW.Start();
#endif

                                    // Dark engine games can't play MP3s, so they must be converted in all cases.
                                    // This one won't be called anywhere except during install, because it always runs during
                                    // install so there's no need to make it optional elsewhere. So we don't need to have a
                                    // check bool or anything.
                                    FMAudio.ConvertAsPartOfInstall(
                                        validAudioConvertibleFM,
                                        AudioConvert.MP3ToWAV,
                                        binaryBuffer,
                                        buffer.FileStreamBuffer,
                                        po.CancellationToken);

                                    po.CancellationToken.ThrowIfCancellationRequested();

                                    if (Config.ConvertOGGsToWAVsOnInstall)
                                    {
                                        FMAudio.ConvertAsPartOfInstall(
                                            validAudioConvertibleFM,
                                            AudioConvert.OGGToWAV,
                                            binaryBuffer,
                                            buffer.FileStreamBuffer,
                                            po.CancellationToken);
                                    }

                                    po.CancellationToken.ThrowIfCancellationRequested();

                                    if (Config.ConvertWAVsTo16BitOnInstall)
                                    {
                                        FMAudio.ConvertAsPartOfInstall(
                                            validAudioConvertibleFM,
                                            AudioConvert.WAVToWAV16,
                                            binaryBuffer,
                                            buffer.FileStreamBuffer,
                                            po.CancellationToken);
                                    }

#if TIMING_TEST
                                    audioConvertSW.Stop();
#pragma warning disable IDE0001
#pragma warning disable IDE0002
                                    // ReSharper disable RedundantNameQualifier
                                    System.Diagnostics.Trace.WriteLine("CA: " + audioConvertSW.Elapsed);
                                    // ReSharper restore RedundantNameQualifier
#pragma warning restore IDE0002
#pragma warning restore IDE0001
#endif

                                    po.CancellationToken.ThrowIfCancellationRequested();
                                }
                                catch (Exception ex) when (ex is not OperationCanceledException)
                                {
                                    validAudioConvertibleFM.LogInfo(ErrorText.Ex + "in audio conversion", ex);
                                }
                            }

                            // Don't be lazy about this; there can be no harm and only benefits by doing it right away
                            GenerateMissFlagFileIfRequired(fmData.FM);

                            Core.View.MultiItemProgress_SetItemData(
                                index: viewItemIndex,
                                line2: LText.ProgressBox.RestoringBackup,
                                percent: 100,
                                progressType: ProgressType.Indeterminate);

                            try
                            {
                                RestoreFM(
                                    fmData.FM,
                                    archivePaths,
                                    buffer.ExtractTempBuffer,
                                    buffer.FileStreamBuffer,
                                    po.CancellationToken);
                            }
                            catch (Exception ex) when (ex is not OperationCanceledException)
                            {
                                Log(ex: ex);
                            }

                            po.CancellationToken.ThrowIfCancellationRequested();

                            Interlocked.Increment(ref fmsFinishedCount);

                            Core.View.MultiItemProgress_SetItemData(
                                index: viewItemIndex,
                                line2: LText.ProgressBox.InstallComplete,
                                percent: 100,
                                progressType: ProgressType.Determinate);

                            Core.View.MultiItemProgress_SetState(
                                mainProgressMessage: fmsFinishedCount.ToStrCur() + " / " + fmDataList.Count
                            );
                        }
                    });
                }
                catch (OperationCanceledException)
                {
                    // @MT_TASK: We get a brief UI thread block when run from within Visual Studio.
                    // Apparently because it has to spew out all those exception messages in the output console.
                    // Everything's fine outside of VS. So just ignore this during dev.
                    await RollBackMultipleFMs(fmDataList);
                    return false;
                }
                finally
                {
                    Core.View.MultiItemProgress_Hide();
                }

                // @MT_TASK: If any errors, show dialog with them here
                foreach (FMInstallResult result in results)
                {
                    // Set this after the operation is finished, to prevent the UI from prematurely updating
                    // with the new installed statuses if it gets redrawn
                    if (result.ResultType == InstallResultType.InstallSucceeded)
                    {
                        result.FMData.FM.Installed = true;
                    }
                    else if (result.ResultType == InstallResultType.RollbackFailed)
                    {
                        // Rollbacks failing should be the rare case, so it's okay to take a disk hit here
                        result.FMData.FM.Installed = Directory.Exists(result.FMData.InstalledPath);
                    }
                }

                Core.View.Invoke(Core.View.RefreshAllSelectedFMs_UpdateInstallState);

                return true;
            }
            finally
            {
#if TIMING_TEST
                StopTimingAndPrintResult();
#endif

                Ini.WriteFullFMDataIni();
                Core.View.HideProgressBox();

                _installCts.Dispose();
            }
        });

        void ReportProgress_Install(ProgressReport_Install report)
        {
            if (_installCts.Token.IsCancellationRequested)
            {
                ShowCancelingInstallMessage();
                return;
            }

            // @MT_TASK: Fixes the not-displayed audio convert text, but makes UI update chunky at times
            // Don't really know how to solve this yet, but look into it
            if (reportThrottleSW.ElapsedMilliseconds > 1)
            {
                Core.View.MultiItemProgress_SetItemData(
                    index: report.ViewItemIndex,
                    line1: report.Text,
                    line2: LText.ProgressBox.InstallingFM,
                    percent: report.Percent,
                    progressType: ProgressType.Determinate);
                reportThrottleSW.Restart();
            }
        }

        static FMInstallResult RollBackSingleFM(FMData fm)
        {
            try
            {
                Core.View.MultiItemProgress_SetItemData(
                    index: fm.ViewItemIndex,
                    line2: LText.ProgressBox.CleaningUpFailedInstall,
                    percent: 100,
                    progressType: ProgressType.Indeterminate);

                return RemoveFMFromDisk(fm);
            }
            finally
            {
                Core.View.MultiItemProgress_SetItemData(
                    index: fm.ViewItemIndex,
                    line2: LText.ProgressBox.InstallFailed,
                    percent: 100,
                    progressType: ProgressType.Determinate);
            }
        }

        // @MT_TASK(RollBackInstalls): Handle multithreading here too
        static async Task RollBackMultipleFMs(List<FMData> fmDataList)
        {
            await Task.Run(() =>
            {
                try
                {
                    //Core.View.SetProgressBoxState_Single(
                    //    message1: LText.ProgressBox.CancelingInstall,
                    //    percent: 100,
                    //    progressType: ProgressType.Determinate,
                    //    cancelAction: NullAction
                    //);

                    // @MT_TASK: Figure out a way to move the progress bar backward like before
                    for (int i = 0; i < fmDataList.Count; i++)
                    {
                        FMData fmData = fmDataList[i];
                        if (fmData.InstallStarted)
                        {
                            // @MT_TASK: Return a list of error objects - or somehow add data to the existing one?
                            RemoveFMFromDisk(fmData);
                        }
                    }

                    //for (int j = lastInstalledFMIndex; j >= 0; j--)
                    //{
                    //    FMData fmData = fmDataList[j];

                    //    //Core.View.SetProgressBoxState_Single(
                    //    //    message2: fmData.FM.GetId(),
                    //    //    percent: GetPercentFromValue_Int(j + 1, lastInstalledFMIndex));

                    //    RemoveFMFromDisk(fmData);
                    //}
                }
                finally
                {
                    Ini.WriteFullFMDataIni();
                    Core.View.HideProgressBox();
                }
            });
        }

        static FMInstallResult RemoveFMFromDisk(FMData fmData)
        {
            string fmInstalledPath = Path.Combine(fmData.InstBasePath, fmData.FM.InstalledDir);
            if (!DeleteFMInstalledDirectory(fmInstalledPath))
            {
                // Don't log it here because the deleter method will already have logged it
                // @MT_TASK: Dialog in multithreading area
                //Core.Dialogs.ShowError(
                //    message: LText.AlertMessages.InstallRollback_FMInstallFolderDeleteFail + $"{NL}{NL}" +
                //             fmInstalledPath);

                ArchiveType type =
                    fmData.FM.Archive.ExtIsZip() ? ArchiveType.Zip :
                    fmData.FM.Archive.ExtIs7z() ? ArchiveType.SevenZip :
                    ArchiveType.Rar;

                return new FMInstallResult(
                    fmData,
                    InstallResultType.RollbackFailed,
                    type,
                    LText.AlertMessages.InstallRollback_FMInstallFolderDeleteFail + $"{NL}{NL}" + fmInstalledPath);
            }
            // This is going to get set based on this anyway at the next load from disk, might as well do it now
            fmData.FM.Installed = Directory.Exists(fmInstalledPath);

            return new FMInstallResult(fmData, InstallResultType.RollbackSucceeded);
        }
    }

    private static FMInstallResult
    InstallFMZip(
        int viewItemIndex,
        IProgress<ProgressReport_Install> progress,
        FMData fmData,
        byte[] tempBuffer,
        byte[] fileStreamBuffer)
    {
        string fmInstalledPath = fmData.InstalledPath.TrimEnd(CA_BS_FS) + "\\";

        try
        {
            var report = new ProgressReport_Install();

            Directory.CreateDirectory(fmInstalledPath);

            using ZipArchive archive = GetReadModeZipArchiveCharEnc(fmData.ArchivePath, fileStreamBuffer);

            int filesCount = archive.Entries.Count;
            for (int i = 0; i < filesCount; i++)
            {
                ZipArchiveEntry entry = archive.Entries[i];

                string fileName = entry.FullName;

                if (fileName[^1].IsDirSep()) continue;

                string extractedName = GetExtractedNameOrThrowIfMalicious(fmInstalledPath, fileName);

                if (fileName.Rel_ContainsDirSep())
                {
                    Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                        fileName.Substring(0, fileName.Rel_LastIndexOfDirSep())));
                }

                entry.ExtractToFile_Fast(extractedName, overwrite: true, tempBuffer);

                File_UnSetReadOnly(extractedName);

                int percent = GetPercentFromValue_Int(i + 1, filesCount);


                report.ViewItemIndex = viewItemIndex;
                report.Text = fmData.FM.Archive;
                report.Percent = percent;
                progress.Report(report);

                _installCts.Token.ThrowIfCancellationRequested();
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log(ErrorText.Ex + "while installing zip " + fmData.ArchivePath + " to " + fmInstalledPath, ex);
            // @MT_TASK: Dialog in multithreading area
            //Core.Dialogs.ShowError(LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially);
            return new FMInstallResult(fmData, InstallResultType.InstallFailed, ArchiveType.Zip, ex.Message);
        }

        return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
    }

    private static FMInstallResult
    InstallFMRar(
        int viewItemIndex,
        IProgress<ProgressReport_Install> progress,
        FMData fmData,
        byte[] tempBuffer)
    {
        string fmInstalledPath = fmData.InstalledPath.TrimEnd(CA_BS_FS) + "\\";

        try
        {
            var report = new ProgressReport_Install();

            Directory.CreateDirectory(fmInstalledPath);

            using var fs = File_OpenReadFast(fmData.ArchivePath);
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

                if (!entry.IsDirectory && !fileName.IsEmpty() && !fileName[^1].IsDirSep())
                {
                    string extractedName = GetExtractedNameOrThrowIfMalicious(fmInstalledPath, fileName);

                    if (fileName.Rel_ContainsDirSep())
                    {
                        Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                            fileName.Substring(0, fileName.Rel_LastIndexOfDirSep())));
                    }

                    reader.ExtractToFile_Fast(extractedName, overwrite: true, tempBuffer);

                    File_UnSetReadOnly(extractedName);

                    _installCts.Token.ThrowIfCancellationRequested();
                }

                int percentOfEntries = GetPercentFromValue_Int(i, entriesCount).Clamp(0, 100);

                if (!_installCts.IsCancellationRequested)
                {
                    report.ViewItemIndex = viewItemIndex;
                    report.Text = fmData.FM.Archive;
                    report.Percent = percentOfEntries;
                    progress.Report(report);
                }
            }

            _installCts.Token.ThrowIfCancellationRequested();

            return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log("Error extracting rar " + fmData.ArchivePath + " to " + fmInstalledPath + $"{NL}", ex);
            // @MT_TASK: Dialog in multithreading area
            //Core.Dialogs.ShowError(LText.AlertMessages.Extract_RarExtractFailedFullyOrPartially);
            return new FMInstallResult(fmData, InstallResultType.InstallFailed, ArchiveType.Rar, ex.Message);
        }
    }

    private static FMInstallResult
    InstallFMSevenZip(
        int viewItemIndex,
        IProgress<ProgressReport_Install> progressInstall,
        FMData fmData)
    {
        string fmInstalledPath = fmData.InstalledPath;

        try
        {
            var report = new ProgressReport_Install();

            Directory.CreateDirectory(fmInstalledPath);

            int entriesCount;

            using (var fs = File_OpenReadFast(fmData.ArchivePath))
            {
                var extractor = new SevenZipArchive(fs);
                entriesCount = extractor.GetEntryCountOnly();
            }

            void ReportProgress(Fen7z.ProgressReport pr)
            {

                if (!pr.Canceling)
                {
                    report.ViewItemIndex = viewItemIndex;
                    report.Text = fmData.FM.Archive;
                    report.Percent = pr.PercentOfEntries;
                    progressInstall.Report(report);
                }
            }

            var progress = new Progress<Fen7z.ProgressReport>(ReportProgress);

            Fen7z.Result result = Fen7z.Extract(
                Paths.SevenZipPath,
                Paths.SevenZipExe,
                fmData.ArchivePath,
                fmInstalledPath,
                cancellationToken: _installCts.Token,
                entriesCount,
                progress: progress
            );

            if (result.ErrorOccurred)
            {
                Log("Error extracting 7z " + fmData.ArchivePath + " to " + fmInstalledPath + $"{NL}" + result);

                // @MT_TASK: Dialog in multithreading area
                //Core.Dialogs.ShowError(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially);

                return new FMInstallResult(fmData, InstallResultType.InstallFailed, ArchiveType.SevenZip, result.ErrorText);
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
                throw new OperationCanceledException();
            }
            else
            {
                return new FMInstallResult(fmData, InstallResultType.InstallSucceeded);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Log("Error extracting 7z " + fmData.ArchivePath + " to " + fmInstalledPath + $"{NL}", ex);
            // @MT_TASK: Dialog in multithreading area
            //Core.Dialogs.ShowError(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially);
            return new FMInstallResult(fmData, InstallResultType.InstallFailed, ArchiveType.SevenZip, ex.Message);
        }
    }

    #endregion

    #region Uninstall

    // @MT_TASK(Uninstall): Multithread this (remember that FM deletion code calls this too!)
    internal static async Task<(bool Success, bool AtLeastOneFMMarkedUnavailable)>
    Uninstall(FanMission[] fms, bool doEndTasks = true)
    {
        using var dsw = new DisableScreenshotWatchers();

        var fail = (false, false);

        var fmDataList = new List<FMData>(fms.Length);

        bool single = fmDataList.Count == 1;

        bool doBackup;

        // Do checks first before progress box so it's not just annoyingly there while in confirmation dialogs
        #region Checks

        List<string> archivePaths;
        try
        {
            Core.View.SetWaitCursor(true);

            (bool success, archivePaths) = DoPreChecks(fms, fmDataList, install: false);

            if (!success) return fail;
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

            byte[]? fileStreamBuffer = null;

            for (int i = 0; i < fmDataList.Count; i++)
            {
                if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);

                FMData fmData = fmDataList[i];

                FanMission fm = fmData.FM;

                string fmInstalledPath = Path.Combine(Config.GetFMInstallPath(fmData.GameIndex), fm.InstalledDir);

                #region Check for already uninstalled

                bool fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    fm.Installed = false;
                    continue;
                }

                #endregion

                if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);

                bool markFMAsUnavailable = false;

                if (fmData.ArchivePath.IsEmpty())
                {
                    if (doEndTasks && !skipUninstallWithNoArchiveWarning)
                    {
                        (MBoxButton result, _) = Core.Dialogs.ShowMultiChoiceDialog(
                            message: LText.AlertMessages.Uninstall_ArchiveNotFound,
                            title: LText.AlertMessages.Warning,
                            icon: MBoxIcon.Warning,
                            yes: single ? LText.AlertMessages.Uninstall : LText.AlertMessages.UninstallAll,
                            no: LText.Global.Skip,
                            cancel: LText.Global.Cancel,
                            defaultButton: MBoxButton.No);

                        if (result == MBoxButton.Cancel) return (false, atLeastOneFMMarkedUnavailable);
                        if (result == MBoxButton.No) continue;

                        if (!single) skipUninstallWithNoArchiveWarning = true;
                    }
                    markFMAsUnavailable = true;
                    atLeastOneFMMarkedUnavailable = true;
                }

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

                if (doBackup) await BackupFM(fm, fmInstalledPath, fmData.ArchivePath, archivePaths, fileStreamBuffer ??= new byte[FileStreamBufferSize]);

                if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);

                // TODO: Give the user the option to retry or something, if it's cause they have a file open
                // Make option to open the folder in Explorer and delete it manually?
                if (!await Task.Run(() => DeleteFMInstalledDirectory(fmInstalledPath)))
                {
                    fm.LogInfo(ErrorText.Un + "delete FM installed directory.");
                    Core.Dialogs.ShowError(
                        LText.AlertMessages.Uninstall_FailedFullyOrPartially + $"{NL}{NL}" +
                        "FM: " + fm.GetId());
                }

                fm.Installed = false;
                if (markFMAsUnavailable) fm.MarkedUnavailable = true;

                if (!single)
                {
                    Core.View.SetProgressBoxState_Single(
                        message2: fm.GetId(),
                        percent: GetPercentFromValue_Int(i + 1, fmDataList.Count)
                    );
                }

                if (_uninstallCts.IsCancellationRequested) return (false, atLeastOneFMMarkedUnavailable);
            }
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

        return (true, atLeastOneFMMarkedUnavailable);
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
            return VoidTask;
        }
    }

    private static bool DeleteFMInstalledDirectory(string path)
    {
        if (!Directory.Exists(path)) return true;

        bool triedReadOnlyRemove = false;

        // Failsafe cause this is nasty
        for (int i = 0; i < 2; i++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return true;
            }
            catch (Exception ex1)
            {
                Log(ErrorText.FTDel + "FM path '" + path + "', attempting to remove readonly attributes and trying again...", ex1);
                try
                {
                    if (triedReadOnlyRemove)
                    {
                        Log(ErrorText.FTDel + "FM path '" + path + "' twice, giving up...");
                        return false;
                    }

                    // FMs installed by us will not have any readonly attributes set, so we work on the
                    // assumption that this is the rarer case and only do this extra work if we need to.
                    DirAndFileTree_UnSetReadOnly(path, throwException: true);

                    triedReadOnlyRemove = true;
                }
                catch (Exception ex2)
                {
                    Log(ErrorText.FT + "remove readonly attributes, giving up...", ex2);
                    return false;
                }
            }
        }

        return false;
    }

    #endregion

    #endregion
}

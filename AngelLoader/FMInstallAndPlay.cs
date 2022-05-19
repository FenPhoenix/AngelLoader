using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using FMScanner.FastZipReader;
using JetBrains.Annotations;
using SevenZip;
using static AL_Common.Common;
using static AngelLoader.FMBackupAndRestore;
using static AngelLoader.GameSupport;
using static AngelLoader.LanguageSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    /*
    @BetterErrors(FMInstallAndPlay):
    -We should have an error for if there's not enough disk space
    -Can we know that for sure? We need to know if there's also enough space for the backup restore, and the
     audio file conversions, and then if we uninstall we also need to know if there's enough space for the
     backup archive...
    -If we can't write the stub file or set ourselves as the selector, maybe we should just cancel the play operation?
    */

    // @vNext: Make "ask on install" be an actual radio button set on the dialog

    internal static class FMInstallAndPlay
    {
        private enum PlaySource
        {
            OriginalGame,
            Editor,
            FM
        }

        private static CancellationTokenSource _installCts = new();
        private static void CancelInstallToken() => _installCts.CancelIfNotDisposed();

        private static CancellationTokenSource _uninstallCts = new();
        private static void CancelUninstallToken() => _uninstallCts.CancelIfNotDisposed();

        internal static Task InstallOrUninstall(params FanMission[] fms)
        {
            AssertR(fms.Length > 0, nameof(fms) + ".Length == 0");
            return fms[0].Installed ? Uninstall(fms) : Install(fms);
        }

        internal static async Task InstallIfNeededAndPlay(FanMission fm, bool askConfIfRequired = false, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is unknown or unsupported for FM " + GetFMId(fm) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Core.Dialogs.ShowError(ErrorText.FMGameTypeUnknownOrUnsupported);
                return;
            }

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            if (playMP && gameIndex != GameIndex.Thief2)
            {
                Log("playMP was true, but fm.Game was not Thief 2.\r\n" +
                    "fm: " + GetFMId(fm) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Core.Dialogs.ShowError(ErrorText.MultiplayerForNonThief2);
                return;
            }

            bool askingConfirmation = askConfIfRequired && Config.ConfirmPlayOnDCOrEnter;
            if (askingConfirmation)
            {
                string message = fm.Installed
                    ? LText.AlertMessages.Play_ConfirmMessage
                    : LText.AlertMessages.Play_InstallAndPlayConfirmMessage;

                if (Core.View.GetMainSelectedFMOrNull() != fm)
                {
                    message += "\r\n\r\n" +
                               fm.Archive + "\r\n" +
                               fm.Title + "\r\n" +
                               fm.Author + "\r\n";
                }

                (bool cancel, bool dontAskAgain) = Core.Dialogs.AskToContinueYesNoCustomStrings(
                    message: message,
                    title: LText.AlertMessages.Confirm,
                    icon: MBoxIcon.None,
                    showDontAskAgain: true,
                    yes: LText.Global.PlayFM,
                    no: LText.Global.Cancel);

                if (cancel) return;

                Config.ConfirmPlayOnDCOrEnter = !dontAskAgain;
            }

            if (fm.Installed || await InstallInternal(fromPlay: true, suppressConfirmation: askingConfirmation, fm))
            {
                if (playMP && gameIndex == GameIndex.Thief2 && Config.GetT2MultiplayerExe_FromDisk().IsEmpty())
                {
                    Log("Thief2MP.exe not found in Thief 2 game directory.\r\n" +
                        "Thief 2 game directory: " + Config.GetGamePath(GameIndex.Thief2));
                    Core.Dialogs.ShowError(LText.AlertMessages.Thief2_Multiplayer_ExecutableNotFound);
                    return;
                }

                try
                {
                    Core.View.SetWaitCursor(true);

                    if (PlayFM(fm, playMP))
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

        internal static bool PlayOriginalGame(GameIndex game, bool playMP = false)
        {
            try
            {
                Core.View.SetWaitCursor(true);

                (bool success, string gameExe, string gamePath) =
                    CheckAndReturnFinalGameExeAndGamePath(game, playingOriginalGame: true, playMP);
                if (!success) return false;
                Paths.CreateOrClearTempPath(Paths.StubCommTemp);

                if (game is GameIndex.Thief1 or GameIndex.Thief2) GameConfigFiles.FixCharacterDetailLine(gamePath);
                SetUsAsSelector(game, gamePath, PlaySource.OriginalGame);

#if !ReleaseBeta && !ReleasePublic
                string args = Config.ForceWindowed ? "+force_windowed" : "";
#else
            string args = "";
#endif
                string workingPath = Config.GetGamePath(game);
                var sv = GetSteamValues(game, playMP);
                if (sv.Success) (_, gameExe, workingPath, args) = sv;

                WriteStubCommFile(null, gamePath, originalT3: game == GameIndex.Thief3);

                StartExe(gameExe, workingPath, args);

                return true;
            }
            finally
            {
                Core.View.SetWaitCursor(false);
            }
        }

        private static bool PlayFM(FanMission fm, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is unknown or unsupported for FM " + GetFMId(fm) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Core.Dialogs.ShowError(ErrorText.FMGameTypeUnknownOrUnsupported);
                return false;
            }

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            (bool success, string gameExe, string gamePath) =
                CheckAndReturnFinalGameExeAndGamePath(gameIndex, playingOriginalGame: false, playMP);
            if (!success) return false;

            // Always do this for robustness, see below
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            if (gameIndex is GameIndex.Thief1 or GameIndex.Thief2) GameConfigFiles.FixCharacterDetailLine(gamePath);
            SetUsAsSelector(gameIndex, gamePath, PlaySource.FM);

            string steamArgs = "";
            string workingPath = Config.GetGamePath(gameIndex);
            var sv = GetSteamValues(gameIndex, playMP);
            if (sv.Success) (_, gameExe, workingPath, steamArgs) = sv;

            // 2019-10-31: Always use the stub now, in prep for matching FMSel's language stuff

            // BUG: Possible stub comm file not being deleted in the following scenario:
            // You launch a game through Steam, but the game doesn't actually launch (because you don't have
            // it in your Steam library or any other situation in which it gets cancelled). Because the game
            // never runs, it never deletes the stub comm file. The next time the game runs, it finds the stub
            // file and loads up whatever FM was specified. This won't happen if you launch an FM or original
            // game from AngelLoader, as we delete or overwrite the stub file ourselves before playing anything,
            // but if you were to run the game manually, it would load whatever FM was specified in the stub
            // once, and then delete it, so if you ran it again it would properly start the original game and
            // everything would be fine again.
            // I could solve it if there was a way to detect if we were being launched through Steam. I don't
            // know if there is, but then I could just specify a Steam=True line in the stub file, and then
            // if we're being launched through steam we read and act on it as usual, but if we're not, then
            // we just delete it and ignore.
            // I'll have to buy the games on Steam to test this. Or just buy one so I can have one game that
            // works and one that doesn't, so I can test both paths.

#if !ReleaseBeta && !ReleasePublic
            string args = !steamArgs.IsEmpty() ? steamArgs : Config.ForceWindowed ? "+force_windowed -fm" : "-fm";
#else
            string args = !steamArgs.IsEmpty() ? steamArgs : "-fm";
#endif

            GenerateMissFlagFileIfRequired(fm);

            WriteStubCommFile(fm, gamePath);

            StartExe(gameExe, workingPath, args);

            // Don't clear the temp folder here, because the stub program will need to read from it. It will
            // delete the temp file itself after it's done with it.

            return true;
        }

        internal static bool OpenFMInEditor(FanMission fm)
        {
            try
            {
                Core.View.SetWaitCursor(true);

                #region Checks (specific to DromEd)

                // This should never happen because our menu item is supposed to be hidden for Thief 3 FMs.
                if (!GameIsDark(fm.Game))
                {
                    Log("FM game type is not a Dark Engine game.\r\n" +
                        "FM: " + GetFMId(fm) + "\r\n" +
                        "fm.Game was: " + fm.Game, stackTrace: true);
                    Core.Dialogs.ShowError(ErrorText.FMGameTypeIsNotDark);
                    return false;
                }

                GameIndex gameIndex = GameToGameIndex(fm.Game);

                string gamePath = Config.GetGamePath(gameIndex);
                if (gamePath.IsEmpty())
                {
                    Log("Game path is empty for " + gameIndex, stackTrace: true);
                    Core.Dialogs.ShowError(gameIndex + ":\r\n" + ErrorText.GamePathEmpty);
                    return false;
                }

                string editorExe = Config.GetEditorExe_FromDisk(gameIndex);
                if (editorExe.IsEmpty())
                {
                    Log("Editor executable not found.\r\n" +
                        "FM: " + GetFMId(fm) + "\r\n" +
                        "Editor executable: " + editorExe);
                    Core.Dialogs.ShowError(fm.Game == Game.SS2
                        ? LText.AlertMessages.ShockEd_ExecutableNotFound
                        : LText.AlertMessages.DromEd_ExecutableNotFound);
                    return false;
                }

                #endregion

                // Just in case, and for consistency
                Paths.CreateOrClearTempPath(Paths.StubCommTemp);

                if (gameIndex is GameIndex.Thief1 or GameIndex.Thief2) GameConfigFiles.FixCharacterDetailLine(gamePath);
                // We don't need to do this here, right?
                SetUsAsSelector(gameIndex, gamePath, PlaySource.Editor);

                // Since we don't use the stub currently, set this here
                // NOTE: DromEd game mode doesn't even work for me anymore. Black screen no matter what. So I can't test if we need languages.
                GameConfigFiles.SetCamCfgLanguage(gamePath, "");

                // Why not
                GenerateMissFlagFileIfRequired(fm);

                // We don't need the stub for DromEd, cause we don't need to pass anything except the fm folder
                StartExe(editorExe, gamePath, "-fm=\"" + fm.InstalledDir + "\"");

                return true;
            }
            finally
            {
                Core.View.SetWaitCursor(false);
            }
        }

        #endregion

        #region Helpers

        private static void SetUsAsSelector(GameIndex game, string gamePath, PlaySource playSource)
        {
            bool success = GameIsDark(game)
                ? GameConfigFiles.SetDarkFMSelector(game, gamePath)
                : GameConfigFiles.SetT3FMSelector();
            if (!success)
            {
                Log("Unable to set us as the selector for " + Config.GetGameExe(game) + " (" +
                    (GameIsDark(game) ? nameof(GameConfigFiles.SetDarkFMSelector) : nameof(GameConfigFiles.SetT3FMSelector)) +
                    " returned false)\r\n" +
                    "Source: " + playSource,
                    stackTrace: true);

                Core.Dialogs.ShowError(
                    "Failed to set AngelLoader as the FM selector.\r\n\r\n" +
                    "Game: " + game + "\r\n" +
                    "Game exe: " + Config.GetGameExe(game) + "\r\n" +
                    "Source: " + playSource + "\r\n" +
                    "");
            }
        }

        private static void WriteStubCommFile(FanMission? fm, string gamePath, bool originalT3 = false)
        {
            string sLanguage = "";
            bool? bForceLanguage = null;

            if (fm == null)
            {
                if (!originalT3) GameConfigFiles.SetCamCfgLanguage(gamePath, "");
            }
            else if (GameIsDark(fm.Game))
            {
                bool langIsDefault = fm.SelectedLang == Language.Default;
                if (langIsDefault)
                {
                    // For Dark, we have to do this semi-manual stuff.
                    (sLanguage, bForceLanguage) = FMLanguages.GetDarkFMLanguage(GameToGameIndex(fm.Game), fm.Archive, fm.InstalledDir);
                }
                else
                {
                    sLanguage = GetLanguageStringUnsafe(fm.SelectedLang);
                    bForceLanguage = true;
                }

                GameConfigFiles.SetCamCfgLanguage(gamePath, langIsDefault ? "" : GetLanguageStringUnsafe(fm.SelectedLang));
            }

            // For Thief 3, Sneaky Upgrade does the entire language thing for me, Builder bless snobel once again.
            // I just can't tell you how much I appreciate how much work SU does for me, even right down to handling
            // the "All The World's a Stage" custom sound extract thing.
            // So, I don't have to do anything whatsoever here, just pass blank and carry on. Awesome!

            try
            {
                // IMPORTANT (Stub comm file encoding):
                // Encoding MUST be "new UTF8Encoding(false, true)" or the C++ stub won't read it (it doesn't
                // handle the byte order mark).
                using var sw = new StreamWriter(Paths.StubCommFilePath, append: false,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
                sw.WriteLine("PlayOriginalGame=" + (fm == null));
                if (fm != null)
                {
                    sw.WriteLine("SelectedFMName=" + fm.InstalledDir);
                    sw.WriteLine("DisabledMods=" + fm.DisabledMods);
                    // Pass blank if we have nothing, so the stub will leave whatever was in there before
                    if (!sLanguage.IsEmpty()) sw.WriteLine("Language=" + sLanguage);
                    if (bForceLanguage != null) sw.WriteLine("ForceLanguage=" + (bool)bForceLanguage);
                }
            }
            catch (Exception ex)
            {
                Log("Exception writing stub file '" + Paths.StubFileName + "'\r\n" +
                    (fm == null ? "Original game" : "FM"), ex);
                Core.Dialogs.ShowError("Unable to write stub comm file. " +
                                       (fm == null
                                           ? "Game may not start correctly."
                                           : "FM cannot be passed to the game and therefore cannot be played."));
            }
        }

        private static void StartExe(string exe, string workingPath, string args)
        {
            try
            {
                ProcessStart_UseShellExecute(new ProcessStartInfo
                {
                    FileName = exe,
                    WorkingDirectory = workingPath,
                    Arguments = !args.IsEmpty() ? args : ""
                });
            }
            catch (Exception ex)
            {
                // @BetterErrors(FMInstallAndPlay/StartExe()):
                // Use more specific messages depending on the exception
                Log("Exception starting " + exe + "\r\n" +
                    "workingPath: " + workingPath + "\r\n" +
                    "args: " + args, ex);
                Core.Dialogs.ShowError(ErrorText.UnableToStartExecutable + "\r\n\r\n" + exe);
            }
        }

        private static (bool Success, string GameExe, string GamePath)
        CheckAndReturnFinalGameExeAndGamePath(GameIndex gameIndex, bool playingOriginalGame, bool playMP = false)
        {
            var failed = (Success: false, GameExe: "", GamePath: "");

            string gameName = GetLocalizedGameName(gameIndex);

            string gameExe = Config.GetGameExe(gameIndex);

            #region Fail if game path is blank

            string gamePath = Config.GetGamePath(gameIndex);
            if (gamePath.IsEmpty())
            {
                Log("Game path is empty for " + gameIndex, stackTrace: true);
                Core.Dialogs.ShowError(gameName + ":\r\n" + ErrorText.GamePathEmpty);
                return failed;
            }

            #endregion

            if (playMP) gameExe = Path.Combine(gamePath, Paths.T2MPExe);

            #region Exe: Fail if blank or not found

            if (gameExe.IsEmpty() || !File.Exists(gameExe))
            {
                string exeNotFoundMessage = playingOriginalGame
                    ? LText.AlertMessages.Play_ExecutableNotFound
                    : LText.AlertMessages.Play_ExecutableNotFoundFM;
                Core.Dialogs.ShowError(gameName + ":\r\n" + exeNotFoundMessage);
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

        private static void GenerateMissFlagFileIfRequired(FanMission fm)
        {
            // Only T1 and T2 have/require missflag.str
            if (fm.Game is not Game.Thief1 and not Game.Thief2) return;

            try
            {
                string instFMsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
                string fmInstalledPath = Path.Combine(instFMsBasePath, fm.InstalledDir);

                if (!Directory.Exists(fmInstalledPath)) return;

                string stringsPath = Path.Combine(fmInstalledPath, "strings");
                string missFlagFile = Path.Combine(stringsPath, "missflag.str");

                bool MissFlagFilesExist()
                {
                    if (!Directory.Exists(stringsPath)) return false;
                    // Missflag.str could be in a subdirectory too! Don't make a new one in that case!
                    string[] missFlag = Directory.GetFiles(stringsPath, "missflag.str", SearchOption.AllDirectories);
                    return missFlag.Length > 0;
                }

                if (MissFlagFilesExist()) return;

                string[] misFiles = Directory.GetFiles(fmInstalledPath, "miss*.mis", SearchOption.TopDirectoryOnly);
                var misNums = new List<int>();
                foreach (string mf in misFiles)
                {
                    Match m = Regex.Match(mf, @"miss(?<Num>\d+).mis", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    if (m.Success && int.TryParse(m.Groups["Num"].Value, out int result))
                    {
                        misNums.Add(result);
                    }
                }

                if (misNums.Count == 0) return;

                try
                {
                    misNums.Sort();

                    Directory.CreateDirectory(stringsPath);

                    int lastMisNum = misNums[misNums.Count - 1];

                    var missFlagLines = new List<string>();
                    for (int i = 1; i <= lastMisNum; i++)
                    {
                        string curLine = "miss_" + i + ": ";
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

                    File.WriteAllLines(missFlagFile, missFlagLines, new UTF8Encoding(false, true));
                }
                catch (Exception ex)
                {
                    Log("Exception trying to generate missflag.str file for an FM that needs it", ex);
                    Core.Dialogs.ShowError("Failed trying to generate a missflag.str file for the following FM:\r\n\r\n" +
                                           GetFMId(fm) + "\r\n\r\n" +
                                           "The FM will probably not be able to play its mission(s).");
                }
            }
            catch (Exception ex)
            {
                Log("Exception trying to generate missflag.str file", ex);
                // ReSharper disable once RedundantJumpStatement
                return; // Explicit for clarity of intent
            }
        }

        [MustUseReturnValue]
        private static bool DoPreChecks(FanMission[] fms, FMData[] fmDataList, bool install, [NotNullWhen(true)] out List<string>? fmArchivePaths)
        {
            static bool Canceled(bool install) => install && _installCts.IsCancellationRequested;

            bool single = fms.Length == 1;

            fmArchivePaths = null;

            bool[] gamesChecked = new bool[SupportedGameCount];

            for (int i = 0; i < fms.Length; i++)
            {
                FanMission fm = fms[i];

                AssertR(install ? !fm.Installed : fm.Installed, "fm.Installed == " + fm.Installed);

                if (!GameIsKnownAndSupported(fm.Game))
                {
                    Log("FM game type is unknown or unsupported.\r\n" +
                        "FM: " + GetFMId(fm) + "\r\n" +
                        "FM game was: " + fm.Game);
                    Core.Dialogs.ShowError(GetFMId(fm) + "\r\n" +
                                           ErrorText.FMGameTypeUnknownOrUnsupported);
                    return false;
                }

                GameIndex gameIndex = GameToGameIndex(fm.Game);
                int intGameIndex = (int)gameIndex;
                fmArchivePaths ??= FMArchives.GetFMArchivePaths();

                if (Canceled(install)) return false;

                string fmArchivePath = FMArchives.FindFirstMatch(fm.Archive, fmArchivePaths);

                if (Canceled(install)) return false;

                string gameExe = Config.GetGameExe(gameIndex);
                string gameName = GetLocalizedGameName(gameIndex);
                string instBasePath = Config.GetFMInstallPath(gameIndex);

                fmDataList[i] = new FMData
                (
                    fm,
                    fmArchivePath,
                    gameExe,
                    gameName,
                    instBasePath
                );

                if (install)
                {
                    if (fmArchivePath.IsEmpty() && !fm.MarkedUnavailable)
                    {
                        Log("FM archive field was empty; this means an archive was not found for it on the last search.\r\n" +
                            "FM: " + GetFMId(fm) + "\r\n" +
                            "FM game was: " + fm.Game);
                        Core.Dialogs.ShowError(GetFMId(fm) + "\r\n" +
                                               LText.AlertMessages.Install_ArchiveNotFound);

                        return false;
                    }
                }

                if (!gamesChecked[intGameIndex])
                {
                    if (install)
                    {
                        if (!File.Exists(gameExe))
                        {
                            Log("Game executable not found.\r\n" +
                                "Game executable: " + gameExe);
                            Core.Dialogs.ShowError(gameName + ":\r\n" +
                                                   GetFMId(fm) + "\r\n" +
                                                   LText.AlertMessages.Install_ExecutableNotFound);

                            return false;
                        }

                        if (Canceled(install)) return false;

                        if (!Directory.Exists(instBasePath))
                        {
                            Log("FM install path not found.\r\n" +
                                "FM: " + GetFMId(fm) + "\r\n" +
                                "FM game was: " + fm.Game + "\r\n" +
                                "FM install path: " + instBasePath
                            );
                            Core.Dialogs.ShowError(gameName + ":\r\n" +
                                                   GetFMId(fm) + "\r\n" +
                                                   LText.AlertMessages.Install_FMInstallPathNotFound);

                            return false;
                        }
                    }

                    if (Canceled(install)) return false;

                    if (GameIsRunning(gameExe))
                    {
                        Core.Dialogs.ShowAlert(
                            !single
                                ? LText.AlertMessages.OneOrMoreGamesAreRunning
                                : gameName + ":\r\n" + (install
                                    ? LText.AlertMessages.Install_GameIsRunning
                                    : LText.AlertMessages.Uninstall_GameIsRunning),
                            LText.AlertMessages.Alert);

                        return false;
                    }

                    if (Canceled(install)) return false;

                    gamesChecked[intGameIndex] = true;
                }
            }

            // Shouldn't ever be null here, but apparently the static analyzer thinks it can, even if I put a
            // guard check above the loop like "if (fms.Length == 0) return false;" in case the loop doesn't ever
            // run. Whatever man.
            fmArchivePaths ??= new List<string>();

            return true;
        }

        #endregion

        #region Install

        private sealed class FMData
        {
            internal readonly FanMission FM;
            internal readonly string ArchivePath;
            internal readonly string GameExe;
            internal readonly string GameName;
            internal readonly string InstBasePath;
            internal BackupFile? BackupFile;

            public FMData(FanMission fm, string archivePath, string gameExe, string gameName, string instBasePath)
            {
                FM = fm;
                ArchivePath = archivePath;
                GameExe = gameExe;
                GameName = gameName;
                InstBasePath = instBasePath;
            }
        }

        private sealed class DriveData
        {
            internal readonly DriveInfo DriveInfo;
            internal bool TotalExtractedSizeCalcSuccessful = true;
            internal long TotalExtractedSizeOfAllFMsForThisDisk;

            internal DriveData(DriveInfo driveInfo) => DriveInfo = driveInfo;
        }

        private static string GetPathRootSafe(string path)
        {
            try
            {
                return Path.GetPathRoot(path);
            }
            catch
            {
                return "";
            }
        }

        internal static Task<bool> Install(params FanMission[] fms) => InstallInternal(false, false, fms);

        private static async Task<bool> InstallInternal(bool fromPlay, bool suppressConfirmation, params FanMission[] fms)
        {
            #region Local functions

            static bool GetDriveDataDict(FanMission[] fms, out List<string> errorPaths, out Dictionary<string, DriveData> result)
            {
                result = new Dictionary<string, DriveData>(SupportedGameCount);
                errorPaths = new List<string>(SupportedGameCount);
                bool success = true;

                Game games = Game.Null;
                for (int i = 0; i < fms.Length; i++)
                {
                    games |= fms[i].Game;
                }

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    GameIndex gameIndex = (GameIndex)i;
                    if (games.HasFlagFast(GameIndexToGame(gameIndex)))
                    {
                        string fmInstallPath = Config.GetFMInstallPath(gameIndex);
                        if (!fmInstallPath.IsEmpty() && !result.TryGetValue(fmInstallPath, out DriveData driveData))
                        {
                            try
                            {
                                var driveInfo = new DriveInfo(fmInstallPath);
                                driveData = new DriveData(driveInfo);
                                result[driveInfo.Name] = driveData;
                            }
                            catch
                            {
                                errorPaths.Add(fmInstallPath);
                                success = false;
                            }
                        }
                    }
                }

                return success;
            }

            static long? GetFreeDiskSpaceForPath(Dictionary<string, DriveData> driveDataDict, string driveName)
            {
                try
                {
                    return driveDataDict.TryGetValue(driveName, out DriveData driveData)
                        ? driveData.DriveInfo.AvailableFreeSpace
                        : null;
                }
                catch
                {
                    return null;
                }
            }

            [MustUseReturnValue]
            static bool AddArchiveExtractedSize(string archivePath, DriveData driveData, ZipReusableBundle bundle)
            {
                long fmExtractedSize = 0;
                try
                {
                    if (archivePath.ExtIsZip())
                    {
#if false
                        using var archive = new ZipArchive(File.OpenRead(archivePath), ZipArchiveMode.Read, leaveOpen: false, Encoding.UTF8);

                        if (_extractCts.IsCancellationRequested) return false;

                        var entries = archive.Entries;

                        if (_extractCts.IsCancellationRequested) return false;

                        for (int entryI = 0; entryI < entries.Count; entryI++)
                        {
                            fmExtractedSize += entries[entryI].Length;
                        }
                        driveData.TotalExtractedSizeOfAllFMsForThisDisk += fmExtractedSize;
#else
                        if (_installCts.IsCancellationRequested) return false;

                        driveData.TotalExtractedSizeOfAllFMsForThisDisk += ZipSize.GetTotalUncompressedSize(File.OpenRead(archivePath), bundle);

                        if (_installCts.IsCancellationRequested) return false;
#endif
                    }
                    else
                    {
#if false
                        using var archive = new SevenZipExtractor(archivePath);

                        if (_extractCts.IsCancellationRequested) return false;

                        // UnpackedSize doesn't work unless you call Check() first, but that takes forever.
                        // Iterating the entries is fast(ish). Size property is uncompressed size, tested and
                        // confirmed.
                        var entries = archive.ArchiveFileData;

                        if (_extractCts.IsCancellationRequested) return false;

                        for (int entryI = 0; entryI < entries.Count; entryI++)
                        {
                            fmExtractedSize += (long)entries[entryI].Size;
                        }
                        driveData.TotalExtractedSizeOfAllFMsForThisDisk += fmExtractedSize;
#else
                        if (_installCts.IsCancellationRequested) return false;

                        var result = Fen7z.Fen7z.Extract(
                            sevenZipWorkingPath: Paths.SevenZipPath,
                            sevenZipPathAndExe: Paths.SevenZipExe,
                            archivePath: archivePath,
                            outputPath: "",
                            entriesCount: 0,
                            listFile: "",
                            fileNamesList: null,
                            cancellationToken: _installCts.Token,
                            progress: null,
                            justReturnUncompressedSize: true
                        );

                        if (_installCts.IsCancellationRequested) return false;

                        if (result.ErrorOccurred)
                        {
                            driveData.TotalExtractedSizeCalcSuccessful = false;
                            driveData.TotalExtractedSizeOfAllFMsForThisDisk = 0;
                        }
                        else
                        {
                            driveData.TotalExtractedSizeOfAllFMsForThisDisk += (long)result.UncompressedSize;
                        }
#endif
                    }
                }
                catch
                {
                    driveData.TotalExtractedSizeCalcSuccessful = false;
                    driveData.TotalExtractedSizeOfAllFMsForThisDisk = 0;
                }

                return true;
            }

            static async Task RollBackInstalls(FMData[] fmDataList, int lastInstalledFMIndex, bool rollBackCurrentOnly = false)
            {
                bool single = fmDataList.Length == 1;

                await Task.Run(() =>
                {
                    static void RemoveFMFromDisk(FMData fmData)
                    {
                        string fmInstalledPath = Path.Combine(fmData.InstBasePath, fmData.FM.InstalledDir);
                        if (!DeleteFMInstalledDirectory(fmInstalledPath))
                        {
                            // Don't log it here because the deleter method will already have logged it
                            Core.Dialogs.ShowError(
                                message: LText.AlertMessages.InstallRollback_FMInstallFolderDeleteFail + "\r\n\r\n" +
                                         fmInstalledPath);
                        }
                        // This is going to get set based on this anyway at the next load from disk, might as well
                        // do it now
                        fmData.FM.Installed = Directory.Exists(fmInstalledPath);
                    }

                    if (rollBackCurrentOnly)
                    {
                        try
                        {
                            if (single)
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

                            RemoveFMFromDisk(fmDataList[lastInstalledFMIndex]);
                        }
                        finally
                        {
                            if (!single)
                            {
                                Core.View.SetProgressBoxState_Double(subProgressType: ProgressType.Determinate);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            Core.View.SetProgressBoxState_Single(
                                    message1: LText.ProgressBox.CancelingInstall,
                                    percent: 100,
                                    progressType: ProgressType.Determinate,
                                    cancelAction: NullAction
                                );

                            for (int j = lastInstalledFMIndex; j >= 0; j--)
                            {
                                var fmData = fmDataList[j];

                                Core.View.SetProgressBoxState_Single(
                                        message2: GetFMId(fmData.FM),
                                        percent: GetPercentFromValue_Int(j + 1, lastInstalledFMIndex));

                                RemoveFMFromDisk(fmData);
                            }
                        }
                        finally
                        {
                            Ini.WriteFullFMDataIni();
                            Core.View.HideProgressBox();
                        }
                    }
                });
            }

            static bool ShowDiskSpaceErrorDialog(string message)
            {
                (bool cancel, _) = Core.Dialogs.AskToContinueYesNoCustomStrings(
                    message: message,
                    title: LText.AlertMessages.Alert,
                    icon: MBoxIcon.Warning,
                    showDontAskAgain: false,
                    yes: LText.Global.Continue,
                    no: LText.Global.Cancel,
                    defaultButton: MBoxButton.No);
                return !cancel;
            }

            static string AppendErrorText(string errorText, HashSetI lines, bool useCalcFailedLines)
            {
                string header = useCalcFailedLines
                    ? LText.AlertMessages.Install_DiskSpaceCalculationFailed + "\r\n\r\n"
                    : LText.AlertMessages.Install_NotEnoughFreeDiskSpace + "\r\n\r\n";

                string[] linesArray = lines.ToArray();
                Array.Sort(linesArray);

                errorText += header;
                errorText += string.Join("\r\n", linesArray);
                errorText += "\r\n\r\n";

                return errorText;
            }

            #endregion

            var fmDataList = new FMData[fms.Length];

            bool single = fmDataList.Length == 1;

            if (!suppressConfirmation &&
                (Config.ConfirmBeforeInstall == ConfirmBeforeInstall.Always ||
                (!single && Config.ConfirmBeforeInstall == ConfirmBeforeInstall.OnlyForMultiple)))
            {
                (bool cancel, bool dontAskAgain) = Core.Dialogs.AskToContinueYesNoCustomStrings(
                    message: single
                        ? fromPlay
                            ? LText.AlertMessages.Play_InstallAndPlayConfirmMessage
                            : LText.AlertMessages.Install_ConfirmSingular
                        : LText.AlertMessages.Install_ConfirmPlural_BeforeNumber +
                          fmDataList.Length.ToString(CultureInfo.CurrentCulture) +
                          LText.AlertMessages.Install_ConfirmPlural_AfterNumber,
                    title: LText.AlertMessages.Alert,
                    icon: MBoxIcon.None,
                    showDontAskAgain: true,
                    yes: single ? fromPlay ? LText.Global.PlayFM : LText.Global.InstallFM : LText.Global.InstallFMs,
                    no: LText.Global.Cancel,
                    defaultButton: MBoxButton.No);
                if (cancel) return false;

                if (dontAskAgain) Config.ConfirmBeforeInstall = ConfirmBeforeInstall.Never;
            }

            try
            {
                bool success = await Task.Run(() =>
                {
                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.PreparingToInstall,
                        progressType: ProgressType.Indeterminate,
                        cancelAction: CancelInstallToken
                    );

                    _installCts = _installCts.Recreate();

                    if (!DoPreChecks(fms, fmDataList, install: true, out var fmArchivePaths)) return false;

                    if (Config.CheckFreeDiskSpaceOnInstall)
                    {
                        Core.View.SetProgressBoxState_Single(
                            showCheckBox: true,
                            checkBoxMessage: LText.ProgressBox.DontCheckFreeDiskSpaceNextTime,
                            checkBoxAction: value => Config.CheckFreeDiskSpaceOnInstall = !value,
                        message1: LText.ProgressBox.CheckingFreeSpace);
                    }

                    #region Free space checks

                    bool success = GetDriveDataDict(fms, out var errorPaths, out var driveDataDict);

                    if (_installCts.IsCancellationRequested) return false;

                    if (!success)
                    {
                        var hash = new HashSetI(errorPaths.Count);
                        for (int i = 0; i < errorPaths.Count; i++)
                        {
                            string errorPath = errorPaths[i];
                            string root = GetPathRootSafe(errorPath);
                            hash.Add(!root.IsEmpty() ? root : errorPath);
                        }

                        string errorText = "";
                        errorText = AppendErrorText(errorText, hash, useCalcFailedLines: true);

                        bool cancel = !ShowDiskSpaceErrorDialog(
                            errorText +
                            LText.AlertMessages.Install_ContinueAfterErrorWarning);
                        if (cancel) return false;
                    }

                    using (var bundle = new ZipReusableBundle())
                    {
                        FileNameBoth? darkLoaderArchiveFiles = null;
                        for (int i = 0; i < fmDataList.Length; i++)
                        {
                            var fmData = fmDataList[i];

                            if (fmData.ArchivePath.IsEmpty() || fmData.FM.MarkedUnavailable) continue;

                            if (!driveDataDict.TryGetValue(GetPathRootSafe(fmData.InstBasePath), out DriveData driveData))
                            {
                                continue;
                            }

                            if (!AddArchiveExtractedSize(fmData.ArchivePath, driveData, bundle)) return false;

                            if (_installCts.IsCancellationRequested) return false;

                            var backupFile = GetBackupFile(
                                fmData.FM,
                                cachedDarkLoaderFiles: darkLoaderArchiveFiles,
                                cachedFMArchivePaths: fmArchivePaths);

                            if (_installCts.IsCancellationRequested) return false;

                            darkLoaderArchiveFiles = backupFile.Cached_DarkLoaderBackups;
                            fmArchivePaths = backupFile.Cached_NewBackups;

                            if (backupFile.Found)
                            {
                                if (!AddArchiveExtractedSize(backupFile.Name, driveData, bundle)) return false;

                                if (_installCts.IsCancellationRequested) return false;

                                fmData.BackupFile = backupFile;
                            }
                        }
                    }

#if false
                    // Debug - when testing custom zip size getter for identicality/performance
                    foreach (var item in driveDataDict)
                    {
                        Trace.WriteLine(item.Value.TotalExtractedSizeOfAllFMsForThisDisk);
                    }

                    return false;
#endif

                    var freeSpaceCalcFailedLines = new HashSetI(SupportedGameCount);
                    var notEnoughFreeSpaceLines = new HashSetI(SupportedGameCount);
                    for (int i = 0; i < SupportedGameCount; i++)
                    {
                        GameIndex gameIndex = (GameIndex)i;
                        string driveName = GetPathRootSafe(Config.GetFMInstallPath(gameIndex));
                        if (driveDataDict.TryGetValue(driveName, out DriveData driveData))
                        {
                            long? freeSpace = GetFreeDiskSpaceForPath(driveDataDict, driveName);
                            if (!driveData.TotalExtractedSizeCalcSuccessful || freeSpace == null)
                            {
                                freeSpaceCalcFailedLines.Add(driveName);
                            }
                            // @MULTISEL(Install/disk space check): Replace with smarter estimation here
                            else if (driveData.TotalExtractedSizeOfAllFMsForThisDisk >= freeSpace)
                            {
                                notEnoughFreeSpaceLines.Add(driveName);
                            }
                        }

                        if (_installCts.IsCancellationRequested) return false;
                    }

                    if (freeSpaceCalcFailedLines.Count > 0 || notEnoughFreeSpaceLines.Count > 0)
                    {
                        string finalErrorText = "";
                        if (notEnoughFreeSpaceLines.Count > 0)
                        {
                            finalErrorText = AppendErrorText(finalErrorText, notEnoughFreeSpaceLines, useCalcFailedLines: false);
                        }
                        if (freeSpaceCalcFailedLines.Count > 0)
                        {
                            finalErrorText = AppendErrorText(finalErrorText, freeSpaceCalcFailedLines, useCalcFailedLines: true);
                        }

                        bool cancel = !ShowDiskSpaceErrorDialog(
                            finalErrorText +
                            LText.AlertMessages.Install_ContinueAfterErrorWarning);
                        if (cancel) return false;
                    }

                    #endregion

                    return true;
                });
                if (!success) return false;

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

                for (int i = 0; i < fmDataList.Length; i++)
                {
                    var fmData = fmDataList[i];

                    if (fmData.ArchivePath.IsEmpty() || fmData.FM.MarkedUnavailable) continue;

                    string fmInstalledPath = Path.Combine(fmData.InstBasePath, fmData.FM.InstalledDir);

                    int mainPercent = GetPercentFromValue_Int(i, fmDataList.Length);

                    // Framework zip extracting is much faster, so use it if possible
                    (bool canceled, bool installFailed) = await (fmData.ArchivePath.ExtIsZip()
                        ? Task.Run(() => InstallFMZip(fmData.ArchivePath, fmInstalledPath, fmData.FM.Archive, mainPercent, fmDataList.Length))
                        : Task.Run(() => InstallFMSevenZip(fmData.ArchivePath, fmInstalledPath, fmData.FM.Archive, mainPercent, fmDataList.Length)));

                    if (installFailed)
                    {
                        await RollBackInstalls(fmDataList, i, rollBackCurrentOnly: true);
                        continue;
                    }

                    if (canceled)
                    {
                        await RollBackInstalls(fmDataList, i);
                        return false;
                    }

                    fmData.FM.Installed = true;

                    try
                    {
                        using var sw = new StreamWriter(Path.Combine(fmInstalledPath, Paths.FMSelInf), append: false);
                        await sw.WriteLineAsync("Name=" + fmData.FM.InstalledDir);
                        await sw.WriteLineAsync("Archive=" + fmData.FM.Archive);
                    }
                    catch (Exception ex)
                    {
                        Log("Couldn't create " + Paths.FMSelInf + " in " + fmInstalledPath, ex);
                    }

                    // Only Dark engine games need audio conversion
                    if (GameIsDark(fmData.FM.Game))
                    {
                        try
                        {
                            if (single)
                            {
                                Core.View.SetProgressBoxState_Single(
                                    message1: LText.ProgressBox.ConvertingFiles,
                                    message2: "",
                                    progressType: ProgressType.Indeterminate
                                );
                            }
                            else
                            {
                                // @MULTISEL(Install/convert files message): Maybe have indeterminate progress bar here?
                                Core.View.SetProgressBoxState_Double(
                                    subMessage: LText.ProgressBox.ConvertingFiles,
                                    subPercent: 100
                                );
                            }

                            // Dark engine games can't play MP3s, so they must be converted in all cases.
                            // This one won't be called anywhere except during install, because it always runs during
                            // install so there's no need to make it optional elsewhere. So we don't need to have a
                            // check bool or anything.
                            await FMAudio.ConvertToWAVs(fmData.FM, AudioConvert.MP3ToWAV, _installCts.Token);

                            if (_installCts.IsCancellationRequested)
                            {
                                await RollBackInstalls(fmDataList, i);
                                return false;
                            }

                            if (Config.ConvertOGGsToWAVsOnInstall) await FMAudio.ConvertToWAVs(fmData.FM, AudioConvert.OGGToWAV, _installCts.Token);

                            if (_installCts.IsCancellationRequested)
                            {
                                await RollBackInstalls(fmDataList, i);
                                return false;
                            }

                            if (Config.ConvertWAVsTo16BitOnInstall) await FMAudio.ConvertToWAVs(fmData.FM, AudioConvert.WAVToWAV16, _installCts.Token);

                            if (_installCts.IsCancellationRequested)
                            {
                                await RollBackInstalls(fmDataList, i);
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("Exception in audio conversion", ex);
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
                        await RestoreFM(fm: fmData.FM, backupFile: fmData.BackupFile, ct: _installCts.Token);
                    }
                    catch (Exception ex)
                    {
                        Log(ex: ex);
                    }

                    if (_installCts.IsCancellationRequested)
                    {
                        await RollBackInstalls(fmDataList, i);
                        return false;
                    }
                }
            }
            finally
            {
                Ini.WriteFullFMDataIni();
                Core.View.HideProgressBox();
            }

            Core.View.RefreshAllSelectedFMRows(refreshInstalledStateOfCurrentRow: true);

            return true;
        }

        private static (bool Canceled, bool InstallFailed)
        InstallFMZip(string fmArchivePath, string fmInstalledPath, string fmArchive, int mainPercent, int fmCount)
        {
            bool single = fmCount == 1;

            try
            {
                Directory.CreateDirectory(fmInstalledPath);

                using var archive = GetZipArchiveCharEnc(fmArchivePath);

                int filesCount = archive.Entries.Count;
                for (int i = 0; i < filesCount; i++)
                {
                    var entry = archive.Entries[i];

                    string fileName = entry.FullName;

                    if (fileName[fileName.Length - 1].IsDirSep()) continue;

                    if (fileName.ContainsDirSep())
                    {
                        Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                            fileName.Substring(0, fileName.LastIndexOfDirSep())));
                    }

                    string extractedName = Path.Combine(fmInstalledPath, fileName);
                    entry.ExtractToFile(extractedName, overwrite: true);

                    File_UnSetReadOnly(Path.Combine(fmInstalledPath, extractedName));

                    int percent = GetPercentFromValue_Int(i + 1, filesCount);

                    int newMainPercent = mainPercent + (percent / fmCount).ClampToZero();

                    if (single)
                    {
                        Core.View.SetProgressPercent(percent);
                    }
                    else
                    {
                        Core.View.SetProgressBoxState_Double(
                            mainPercent: newMainPercent,
                            subPercent: percent,
                            subMessage: fmArchive
                        );
                    }

                    if (_installCts.Token.IsCancellationRequested)
                    {
                        return (true, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception while installing zip " + fmArchivePath + " to " + fmInstalledPath, ex);
                Core.Dialogs.ShowError(LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially);
                return (false, true);
            }

            return (false, false);
        }

        private static (bool Canceled, bool InstallFailed)
        InstallFMSevenZip(string fmArchivePath, string fmInstalledPath, string fmArchive, int mainPercent, int fmCount)
        {
            bool single = fmCount == 1;

            try
            {
                Directory.CreateDirectory(fmInstalledPath);
                Paths.CreateOrClearTempPath(Paths.SevenZipListTemp);

                int entriesCount;

                using (var extractor = new SevenZipExtractor(fmArchivePath))
                {
                    entriesCount = extractor.ArchiveFileData.Count;
                }

                void ReportProgress(Fen7z.Fen7z.ProgressReport pr)
                {
                    int newMainPercent = mainPercent + (pr.PercentOfEntries / fmCount).ClampToZero();

                    if (!pr.Canceling)
                    {
                        if (single)
                        {
                            Core.View.SetProgressPercent(pr.PercentOfEntries);
                        }
                        else
                        {
                            Core.View.SetProgressBoxState_Double(
                                mainPercent: newMainPercent,
                                subPercent: pr.PercentOfEntries,
                                subMessage: fmArchive
                            );
                        }
                    }
                }

                var progress = new Progress<Fen7z.Fen7z.ProgressReport>(ReportProgress);

                var result = Fen7z.Fen7z.Extract(
                    Paths.SevenZipPath,
                    Paths.SevenZipExe,
                    fmArchivePath,
                    fmInstalledPath,
                    entriesCount,
                    listFile: "",
                    new List<string>(),
                    _installCts.Token,
                    progress
                );

                if (result.ErrorOccurred)
                {
                    Log("Error extracting 7z " + fmArchivePath + " to " + fmInstalledPath + "\r\n"
                        + result.ErrorText + "\r\n"
                        + (result.Exception?.ToString() ?? "") + "\r\n"
                        + "ExitCode: " + result.ExitCode + "\r\n"
                        + "ExitCodeInt: " + (result.ExitCodeInt?.ToString() ?? ""));

                    Core.Dialogs.ShowError(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially);

                    return (result.Canceled, true);
                }

                if (!result.Canceled)
                {
                    foreach (string file in Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        // TODO: Unset readonly for directories too
                        File_UnSetReadOnly(file);
                    }
                }

                return (result.Canceled, false);
            }
            catch (Exception ex)
            {
                Log("Error extracting 7z " + fmArchivePath + " to " + fmInstalledPath + "\r\n", ex);
                Core.Dialogs.ShowError(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially);
                return (false, true);
            }
        }

        #endregion

        #region Uninstall

        internal static async Task<bool> Uninstall(params FanMission[] fms)
        {
            var fmDataList = new FMData[fms.Length];

            bool single = fmDataList.Length == 1;

            bool doBackup;

            // Do checks first before progress box so it's not just annoyingly there while in confirmation dialogs
            #region Checks

            try
            {
                Core.View.SetWaitCursor(true);

                bool success = await Task.Run(() => DoPreChecks(fms, fmDataList, install: false, out _));
                if (!success) return false;
            }
            finally
            {
                Core.View.SetWaitCursor(false);
            }

            #endregion

            #region Confirm uninstall

            if (Config.ConfirmUninstall)
            {
                (bool cancel, bool dontAskAgain) = Core.Dialogs.AskToContinueYesNoCustomStrings(
                    message: single
                        ? LText.AlertMessages.Uninstall_Confirm
                        : LText.AlertMessages.Uninstall_Confirm_Multiple,
                    title: LText.AlertMessages.Confirm,
                    icon: MBoxIcon.Warning,
                    showDontAskAgain: true,
                    yes: LText.AlertMessages.Uninstall,
                    no: LText.Global.Cancel);

                if (cancel) return false;

                Config.ConfirmUninstall = !dontAskAgain;
            }

            #endregion

            #region Confirm backup

            if (Config.BackupAlwaysAsk)
            {
                string message = Config.BackupFMData == BackupFMData.SavesAndScreensOnly
                    ? LText.AlertMessages.Uninstall_BackupSavesAndScreenshots
                    : LText.AlertMessages.Uninstall_BackupAllData;
                (bool cancel, bool cont, bool dontAskAgain) =
                    Core.Dialogs.AskToContinueWithCancelCustomStrings(
                        message: message + "\r\n\r\n" + LText.AlertMessages.Uninstall_BackupChooseNoNote,
                        title: LText.AlertMessages.Confirm,
                        icon: MBoxIcon.None,
                        showDontAskAgain: true,
                        yes: LText.AlertMessages.BackUp,
                        no: LText.AlertMessages.DontBackUp,
                        cancel: LText.Global.Cancel);

                if (cancel) return false;

                Config.BackupAlwaysAsk = !dontAskAgain;
                doBackup = cont;
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

                for (int i = 0; i < fmDataList.Length; i++)
                {
                    if (_uninstallCts.IsCancellationRequested) return false;

                    var fmData = fmDataList[i];

                    FanMission fm = fmData.FM;

                    GameIndex gameIndex = GameToGameIndex(fm.Game);

                    string fmInstalledPath = Path.Combine(Config.GetFMInstallPath(gameIndex), fm.InstalledDir);

                    #region Check for already uninstalled

                    bool fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                    if (!fmDirExists)
                    {
                        fm.Installed = false;
                        continue;
                    }

                    #endregion

                    if (_uninstallCts.IsCancellationRequested) return false;

                    bool markFMAsUnavailable = false;

                    if (fmData.ArchivePath.IsEmpty())
                    {
                        (bool cancel, bool cont, _) = Core.Dialogs.AskToContinueWithCancelCustomStrings(
                            message: LText.AlertMessages.Uninstall_ArchiveNotFound,
                            title: LText.AlertMessages.Warning,
                            icon: MBoxIcon.Warning,
                            showDontAskAgain: false,
                            yes: LText.AlertMessages.Uninstall,
                            no: LText.Global.Skip,
                            cancel: LText.Global.Cancel,
                            defaultButton: MBoxButton.No);

                        if (cancel) return false;
                        if (!cont) continue;
                        markFMAsUnavailable = true;
                        atLeastOneFMMarkedUnavailable = true;
                    }

                    // If fm.Archive is blank, then fm.InstalledDir will be used for the backup file name instead.
                    // This file will be included in the search when restoring, and the newest will be taken as
                    // usual.

                    // fm.Archive can be blank at this point when all of the following conditions are true:
                    // -fm is installed
                    // -fm does not have fmsel.inf in its installed folder (or its fmsel.inf is blank or invalid)
                    // -fm was not in the database on startup
                    // -the folder where the FM's archive is located is not in Config.FMArchivePaths (or its sub-
                    //  folders if that option is enabled)

                    // It's not particularly likely, but it could happen if the user had NDL-installed FMs (which
                    // don't have fmsel.inf), started AngelLoader for the first time, didn't specify the right
                    // archive folder on initial setup, and hasn't imported from NDL by this point.

                    if (doBackup) await BackupFM(fm, fmInstalledPath, fmData.ArchivePath);

                    if (_uninstallCts.IsCancellationRequested) return false;

                    // TODO: Give the user the option to retry or something, if it's cause they have a file open
                    // Make option to open the folder in Explorer and delete it manually?
                    if (!await Task.Run(() => DeleteFMInstalledDirectory(fmInstalledPath)))
                    {
                        Log("Could not delete FM installed directory.\r\n" +
                            "FM: " + GetFMId(fm) + "\r\n" +
                            "FM installed path: " + fmInstalledPath);
                        Core.Dialogs.ShowError(LText.AlertMessages.Uninstall_FailedFullyOrPartially + "\r\n\r\n" +
                                               "FM: " + GetFMId(fm));
                    }

                    fm.Installed = false;
                    if (markFMAsUnavailable) fm.MarkedUnavailable = true;

                    // NewDarkLoader still truncates its Thief 3 install names, but the "official" way is not to
                    // do it for Thief 3. If the user already has FMs that were installed with NewDarkLoader, we
                    // just read in the truncated names and treat them as normal for compatibility purposes. But
                    // if we've just uninstalled the mission, then we can safely convert InstalledDir back to full
                    // un-truncated form for future use.
                    // NOTE: 2022-05-17: This was a dumb glib decision that will probably just cause confusion
                    // for the importer code and anyone else who looks at the behavior.
                    if (gameIndex == GameIndex.Thief3 && !fm.Archive.IsEmpty())
                    {
                        fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(truncate: false);
                    }

                    if (!single)
                    {
                        Core.View.SetProgressBoxState_Single(
                            percent: GetPercentFromValue_Int(i + 1, fmDataList.Length),
                            message2: GetFMId(fm)
                        );
                    }

                    if (_uninstallCts.IsCancellationRequested) return false;
                }
            }
            finally
            {
                Ini.WriteFullFMDataIni();
                Core.View.HideProgressBox();

                // If any FMs are gone, refresh the list to remove them. Otherwise, don't refresh the list because
                // then the FMs might move in the list if we're sorting by installed state.
                if (atLeastOneFMMarkedUnavailable && !Core.View.GetShowUnavailableFMsFilter())
                {
                    await Core.View.SortAndSetFilter(keepSelection: true);
                }
                else
                {
                    Core.View.RefreshAllSelectedFMRows(refreshInstalledStateOfCurrentRow: true);
                }
            }

            return true;
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
                    Log("Failed to delete FM path '" + path + "', attempting to remove readonly attributes and trying again...", ex1);
                    try
                    {
                        if (triedReadOnlyRemove)
                        {
                            Log("Failed to delete FM path '" + path + "' twice, giving up...");
                            return false;
                        }

                        // FMs installed by us will not have any readonly attributes set, so we work on the
                        // assumption that this is the rarer case and only do this extra work if we need to.
                        foreach (string f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                        {
                            new FileInfo(f).IsReadOnly = false;
                        }

                        foreach (string d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                        {
                            Dir_UnSetReadOnly(d);
                        }

                        triedReadOnlyRemove = true;
                    }
                    catch (Exception ex2)
                    {
                        Log("Failed to remove readonly attributes, giving up...", ex2);
                        return false;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}

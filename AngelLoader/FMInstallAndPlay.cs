using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using SevenZip;
using static AL_Common.Common;
using static AngelLoader.FMBackupAndRestore;
using static AngelLoader.GameSupport;
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

    internal static class FMInstallAndPlay
    {
        private enum PlaySource
        {
            OriginalGame,
            Editor,
            FM
        }

        private static CancellationTokenSource _extractCts = new CancellationTokenSource();

        internal static Task InstallOrUninstall(FanMission fm) => fm.Installed ? UninstallFM(fm) : InstallFM(fm);

        internal static async Task InstallIfNeededAndPlay(FanMission fm, bool askConfIfRequired = false, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is unknown or unsupported for FM " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Dialogs.ShowError(ErrorText.FMGameTypeUnknownOrUnsupported);
                return;
            }

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            if (playMP && gameIndex != GameIndex.Thief2)
            {
                Log("playMP was true, but fm.Game was not Thief 2.\r\n" +
                    "fm: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Dialogs.ShowError(ErrorText.MultiplayerForNonThief2);
                return;
            }

            if (askConfIfRequired && Config.ConfirmPlayOnDCOrEnter)
            {
                string message = fm.Installed
                    ? LText.AlertMessages.Play_ConfirmMessage
                    : LText.AlertMessages.Play_InstallAndPlayConfirmMessage;

                if (Core.View.GetSelectedFMOrNull() != fm)
                {
                    message += "\r\n\r\n" +
                               fm.Archive + "\r\n" +
                               fm.Title + "\r\n" +
                               fm.Author + "\r\n";
                }

                (bool cancel, bool dontAskAgain) = Dialogs.AskToContinueYesNoCustomStrings(
                    message: message,
                    title: LText.AlertMessages.Confirm,
                    icon: MessageBoxIcon.None,
                    showDontAskAgain: true,
                    yes: LText.Global.Yes,
                    no: LText.Global.No);

                if (cancel) return;

                Config.ConfirmPlayOnDCOrEnter = !dontAskAgain;
            }

            if (!fm.Installed && !await InstallFM(fm)) return;

            if (playMP && gameIndex == GameIndex.Thief2 && Config.GetT2MultiplayerExe_FromDisk().IsEmpty())
            {
                Log("Thief2MP.exe not found in Thief 2 game directory.\r\n" +
                    "Thief 2 game directory: " + Config.GetGamePath(GameIndex.Thief2));
                Dialogs.ShowError(LText.AlertMessages.Thief2_Multiplayer_ExecutableNotFound);
                return;
            }

            if (PlayFM(fm, playMP))
            {
                fm.LastPlayed.DateTime = DateTime.Now;
                Core.View.RefreshFM(fm);
                Ini.WriteFullFMDataIni();
            }
        }

        #region Play / open

        internal static bool PlayOriginalGame(GameIndex game, bool playMP = false)
        {
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

        private static bool PlayFM(FanMission fm, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is unknown or unsupported for FM " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Dialogs.ShowError(ErrorText.FMGameTypeUnknownOrUnsupported);
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
            #region Checks (specific to DromEd)

            // This should never happen because our menu item is supposed to be hidden for Thief 3 FMs.
            if (!GameIsDark(fm.Game))
            {
                Log("FM game type is not a Dark Engine game.\r\n" +
                    "FM: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Dialogs.ShowError(ErrorText.FMGameTypeIsNotDark);
                return false;
            }

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            string gamePath = Config.GetGamePath(gameIndex);
            if (gamePath.IsEmpty())
            {
                Log("Game path is empty for " + gameIndex, stackTrace: true);
                Dialogs.ShowError(gameIndex + ":\r\n" + ErrorText.GamePathEmpty);
                return false;
            }

            string editorExe = Config.GetEditorExe_FromDisk(gameIndex);
            if (editorExe.IsEmpty())
            {
                Log("Editor executable not found.\r\n" +
                    "FM:" + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "Editor executable: " + editorExe);
                Dialogs.ShowError(fm.Game == Game.SS2
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

                Dialogs.ShowError(
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
                bool langIsDefault = fm.SelectedLang.EqualsI(FMLanguages.DefaultLangKey);
                if (langIsDefault)
                {
                    // For Dark, we have to do this semi-manual stuff.
                    (sLanguage, bForceLanguage) = FMLanguages.GetDarkFMLanguage(GameToGameIndex(fm.Game), fm.Archive, fm.InstalledDir);
                }
                else
                {
                    sLanguage = fm.SelectedLang;
                    bForceLanguage = true;
                }

                GameConfigFiles.SetCamCfgLanguage(gamePath, langIsDefault ? "" : fm.SelectedLang);
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
                Dialogs.ShowError("Unable to write stub comm file. " +
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
                Dialogs.ShowError(ErrorText.UnableToStartExecutable + "\r\n\r\n" + exe);
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
                Dialogs.ShowError(gameName + ":\r\n" + ErrorText.GamePathEmpty);
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
                Dialogs.ShowError(gameName + ":\r\n" + exeNotFoundMessage);
                return failed;
            }

            #endregion

            #region Exe: Fail if already running

            if (GameIsRunning(gameExe, checkAllGames: true))
            {
                Dialogs.ShowAlert(LText.AlertMessages.Play_AnyGameIsRunning, LText.AlertMessages.Alert);
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
                // @BetterErrors(GenerateMissFlagFile())
                Log("Exception trying to write missflag.str file", ex);
                // ReSharper disable once RedundantJumpStatement
                return; // Explicit for clarity of intent
            }
        }

        #endregion

        #region Install

        internal static async Task<bool> InstallFM(FanMission fm)
        {
            #region Checks

            AssertR(!fm.Installed, "fm.Installed == false");

            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("FM game type is unknown or unsupported.\r\n" +
                    "FM: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "FM game was: " + fm.Game);
                Dialogs.ShowError(ErrorText.FMGameTypeUnknownOrUnsupported);
                return false;
            }

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            string fmArchivePath = FMArchives.FindFirstMatch(fm.Archive);

            if (fmArchivePath.IsEmpty())
            {
                Log("FM archive field was empty; this means an archive was not found for it on the last search.\r\n" +
                    "FM: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "FM game was: " + fm.Game);
                Dialogs.ShowError(LText.AlertMessages.Install_ArchiveNotFound);
                return false;
            }

            string gameExe = Config.GetGameExe(gameIndex);
            string gameName = GetLocalizedGameName(gameIndex);
            if (!File.Exists(gameExe))
            {
                Log("Game executable not found.\r\n" +
                    "Game executable: " + gameExe);
                Dialogs.ShowError(gameName + ":\r\n" +
                                  LText.AlertMessages.Install_ExecutableNotFound);
                return false;
            }

            string instBasePath = Config.GetFMInstallPath(gameIndex);

            if (!Directory.Exists(instBasePath))
            {
                Log("FM install path not found.\r\n" +
                    "FM: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "FM game was: " + fm.Game + "\r\n" +
                    "FM install path: " + instBasePath
                    );
                Dialogs.ShowError(gameName + ":\r\n" +
                                  LText.AlertMessages.Install_FMInstallPathNotFound);
                return false;
            }

            if (GameIsRunning(gameExe))
            {
                Dialogs.ShowAlert(gameName + ":\r\n" +
                                  LText.AlertMessages.Install_GameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            string fmInstalledPath = Path.Combine(instBasePath, fm.InstalledDir);

            _extractCts = _extractCts.Recreate();

            Core.View.ShowProgressBox(ProgressTask.InstallFM);

            // Framework zip extracting is much faster, so use it if possible
            bool canceled = !await (fmArchivePath.ExtIsZip()
                ? Task.Run(() => InstallFMZip(fmArchivePath, fmInstalledPath))
                : Task.Run(() => InstallFMSevenZip(fmArchivePath, fmInstalledPath)));

            if (canceled)
            {
                Core.View.SetCancelingFMInstall();
                await Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete(fmInstalledPath, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        // @BetterErrors(InstallFM() - install cancellation (folder deletion) failed)
                        Log("Unable to delete FM installed directory " + fmInstalledPath, ex);
                    }
                });
                Core.View.HideProgressBox();
                return false;
            }

            fm.Installed = true;

            Ini.WriteFullFMDataIni();

            try
            {
                using var sw = new StreamWriter(Path.Combine(fmInstalledPath, Paths.FMSelInf), append: false);
                await sw.WriteLineAsync("Name=" + fm.InstalledDir);
                await sw.WriteLineAsync("Archive=" + fm.Archive);
            }
            catch (Exception ex)
            {
                Log("Couldn't create " + Paths.FMSelInf + " in " + fmInstalledPath, ex);
            }

            // Only Dark engine games need audio conversion
            if (GameIsDark(gameIndex))
            {
                try
                {
                    Core.View.ShowProgressBox(ProgressTask.ConvertFiles);

                    // Dark engine games can't play MP3s, so they must be converted in all cases.
                    // This one won't be called anywhere except during install, because it always runs during
                    // install so there's no need to make it optional elsewhere. So we don't need to have a
                    // check bool or anything.
                    await FMAudio.ConvertToWAVs(fm, AudioConvert.MP3ToWAV, false);
                    if (Config.ConvertOGGsToWAVsOnInstall) await FMAudio.ConvertToWAVs(fm, AudioConvert.OGGToWAV, false);
                    if (Config.ConvertWAVsTo16BitOnInstall) await FMAudio.ConvertToWAVs(fm, AudioConvert.WAVToWAV16, false);
                }
                catch (Exception ex)
                {
                    Log("Exception in audio conversion", ex);
                }
            }

            // Don't be lazy about this; there can be no harm and only benefits by doing it right away
            GenerateMissFlagFileIfRequired(fm);

            // TODO: Put up a "Restoring saves and screenshots" box here to avoid the "converting files" one lasting beyond its time?
            try
            {
                await RestoreFM(fm);
            }
            catch (Exception ex)
            {
                Log(ex: ex);
            }
            finally
            {
                Core.View.HideProgressBox();
            }

            // Not doing RefreshFM(rowOnly: true) because that wouldn't update the install/uninstall buttons
            Core.View.RefreshFM(fm);

            return true;
        }

        private static bool InstallFMZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

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

                    Core.View.InvokeSync(new Action(() => Core.View.ReportFMExtractProgress(percent)));

                    if (_extractCts.Token.IsCancellationRequested)
                    {
                        canceled = true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception while installing zip " + fmArchivePath + " to " + fmInstalledPath, ex);
                Core.View.InvokeSync(new Action(() =>
                    Dialogs.ShowError(LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially)));
            }

            return !canceled;
        }

        private static bool InstallFMSevenZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;
            try
            {
                Directory.CreateDirectory(fmInstalledPath);
                Paths.CreateOrClearTempPath(Paths.SevenZipListTemp);

                int entriesCount;

                using (var extractor = new SevenZipExtractor(fmArchivePath))
                {
                    entriesCount = extractor.ArchiveFileData.Count;
                }

                static void ReportProgress(Fen7z.Fen7z.ProgressReport pr) =>
                    Core.View.InvokeSync(pr.Canceling
                        ? new Action(Core.View.SetCancelingFMInstall)
                        : new Action(() => Core.View.ReportFMExtractProgress(pr.PercentOfEntries)));

                var progress = new Progress<Fen7z.Fen7z.ProgressReport>(ReportProgress);

                var result = Fen7z.Fen7z.Extract(
                    Paths.SevenZipPath,
                    Paths.SevenZipExe,
                    fmArchivePath,
                    fmInstalledPath,
                    entriesCount,
                    listFile: "",
                    new List<string>(),
                    _extractCts.Token,
                    progress
                );

                canceled = result.Canceled;

                if (result.ErrorOccurred)
                {
                    Log("Error extracting 7z " + fmArchivePath + " to " + fmInstalledPath + "\r\n"
                        + result.ErrorText + "\r\n"
                        + (result.Exception?.ToString() ?? "") + "\r\n"
                        + "ExitCode: " + result.ExitCode + "\r\n"
                        + "ExitCodeInt: " + (result.ExitCodeInt?.ToString() ?? ""));

                    Core.View.InvokeSync(new Action(() =>
                        Dialogs.ShowError(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially)));

                    return !result.Canceled;
                }

                if (!result.Canceled)
                {
                    foreach (string file in Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        // TODO: Unset readonly for directories too
                        File_UnSetReadOnly(file);
                    }
                }

                return !result.Canceled;
            }
            catch (Exception ex)
            {
                Log("Error extracting 7z " + fmArchivePath + " to " + fmInstalledPath + "\r\n", ex);

                Core.View.InvokeSync(new Action(() =>
                    Dialogs.ShowError(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially)));

                return !canceled;
            }
        }

        internal static void CancelInstallFM() => _extractCts.CancelIfNotDisposed();

        #endregion

        #region Uninstall

        internal static async Task<bool> UninstallFM(FanMission fm)
        {
            if (!fm.Installed || !GameIsKnownAndSupported(fm.Game)) return false;

            GameIndex gameIndex = GameToGameIndex(fm.Game);

            if (Config.ConfirmUninstall)
            {
                (bool cancel, bool dontAskAgain) = Dialogs.AskToContinueYesNoCustomStrings(
                        message: LText.AlertMessages.Uninstall_Confirm,
                        title: LText.AlertMessages.Confirm,
                        icon: MessageBoxIcon.Warning,
                        showDontAskAgain: true,
                        yes: LText.AlertMessages.Uninstall,
                        no: LText.Global.Cancel);

                if (cancel) return false;

                Config.ConfirmUninstall = !dontAskAgain;
            }

            string gameExe = Config.GetGameExe(gameIndex);
            string gameName = GetLocalizedGameName(gameIndex);
            if (GameIsRunning(gameExe))
            {
                Dialogs.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.Uninstall_GameIsRunning,
                    LText.AlertMessages.Alert);
                return false;
            }

            Core.View.ShowProgressBox(ProgressTask.UninstallFM);

            try
            {
                string fmInstalledPath = Path.Combine(Config.GetFMInstallPath(gameIndex), fm.InstalledDir);

                bool fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    bool yes = Dialogs.AskToContinue(LText.AlertMessages.Uninstall_FMAlreadyUninstalled,
                        LText.AlertMessages.Alert);
                    if (yes)
                    {
                        fm.Installed = false;
                        Core.View.RefreshFM(fm);
                    }
                    return true;
                }

                string fmArchivePath = await Task.Run(() => FMArchives.FindFirstMatch(fm.Archive));

                bool markFMAsUnavailable = false;

                if (fmArchivePath.IsEmpty())
                {
                    (bool cancel, _) = Dialogs.AskToContinueYesNoCustomStrings(
                        message: LText.AlertMessages.Uninstall_ArchiveNotFound,
                        title: LText.AlertMessages.Warning,
                        icon: MessageBoxIcon.Warning,
                        showDontAskAgain: false,
                        yes: LText.AlertMessages.Uninstall,
                        no: LText.Global.Cancel);

                    if (cancel) return false;
                    markFMAsUnavailable = true;
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

                #region Backup

                bool doBackup;

                if (Config.BackupAlwaysAsk)
                {
                    string message = Config.BackupFMData == BackupFMData.SavesAndScreensOnly
                        ? LText.AlertMessages.Uninstall_BackupSavesAndScreenshots
                        : LText.AlertMessages.Uninstall_BackupAllData;
                    (bool cancel, bool cont, bool dontAskAgain) =
                        Dialogs.AskToContinueWithCancelCustomStrings(
                            message: message + "\r\n\r\n" + LText.AlertMessages.Uninstall_BackupChooseNoNote,
                            title: LText.AlertMessages.Confirm,
                            icon: MessageBoxIcon.None,
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

                if (doBackup) await BackupFM(fm, fmInstalledPath, fmArchivePath);

                #endregion

                // TODO: Give the user the option to retry or something, if it's cause they have a file open
                // Make option to open the folder in Explorer and delete it manually?
                if (!await Task.Run(() => DeleteFMInstalledDirectory(fmInstalledPath)))
                {
                    Log("Could not delete FM installed directory.\r\n" +
                        "FM: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                        "FM installed path: " + fmInstalledPath);
                    Dialogs.ShowError(LText.AlertMessages.Uninstall_UninstallNotCompleted);
                }

                fm.Installed = false;
                if (markFMAsUnavailable) fm.MarkedUnavailable = true;

                // NewDarkLoader still truncates its Thief 3 install names, but the "official" way is not to
                // do it for Thief 3. If the user already has FMs that were installed with NewDarkLoader, we
                // just read in the truncated names and treat them as normal for compatibility purposes. But
                // if we've just uninstalled the mission, then we can safely convert InstalledDir back to full
                // un-truncated form for future use.
                if (gameIndex == GameIndex.Thief3 && !fm.Archive.IsEmpty())
                {
                    fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(truncate: false);
                }

                Ini.WriteFullFMDataIni();

                // If the FM is gone, refresh the list to remove it. Otherwise, don't refresh the list because
                // then the FM might move in the list if we're sorting by installed state.
                if (fm.MarkedUnavailable && !Core.View.GetShowUnavailableFMsFilter())
                {
                    await Core.View.SortAndSetFilter(keepSelection: true);
                }
                else
                {
                    Core.View.RefreshFM(fm);
                }
            }
            catch (Exception ex)
            {
                Log("Exception uninstalling FM " + fm.Archive + ", " + fm.InstalledDir, ex);
                Dialogs.ShowError(LText.AlertMessages.Uninstall_FailedFullyOrPartially);
            }
            finally
            {
                Core.View.HideProgressBox();
            }

            return true;
        }

        private static bool DeleteFMInstalledDirectory(string path)
        {
            bool triedReadOnlyRemove = false;

            // Failsafe cause this is nasty
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    Directory.Delete(path, recursive: true);
                    return true;
                }
                catch
                {
                    try
                    {
                        if (triedReadOnlyRemove) return false;

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
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}

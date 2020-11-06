using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI.Ookii.Dialogs;
using SevenZip;
using static AngelLoader.FMBackupAndRestore;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMInstallAndPlay
    {
        private static CancellationTokenSource _extractCts = new CancellationTokenSource();

        internal static Task InstallOrUninstall(FanMission fm) => fm.Installed ? UninstallFM(fm) : InstallFM(fm);

        internal static async Task InstallIfNeededAndPlay(FanMission fm, bool askConfIfRequired = false, bool playMP = false)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is unknown or unsupported for FM " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                return;
            }

            if (playMP && fm.Game != Game.Thief2)
            {
                Log("playMP was true, but fm.Game was not Thief 2.\r\n" +
                    "fm: " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                return;
            }

            if (askConfIfRequired && Config.ConfirmPlayOnDCOrEnter)
            {
                (bool cancel, bool dontAskAgain) = Core.View.AskToContinueYesNoCustomStrings(
                    LText.AlertMessages.Play_ConfirmMessage,
                    LText.AlertMessages.Confirm,
                    icon: null,
                    showDontAskAgain: true,
                    yes: null,
                    no: null);

                if (cancel) return;

                Config.ConfirmPlayOnDCOrEnter = !dontAskAgain;
            }

            if (!fm.Installed && !await InstallFM(fm)) return;

            if (playMP && fm.Game == Game.Thief2 && Config.GetT2MultiplayerExe_FromDisk().IsEmpty())
            {
                Core.View.ShowAlert(
                    LText.AlertMessages.Thief2_Multiplayer_ExecutableNotFound,
                    LText.AlertMessages.Alert);
                return;
            }

            if (PlayFM(fm, playMP))
            {
                fm.LastPlayed.DateTime = DateTime.Now;
                Core.View.RefreshSelectedFM();
            }
        }

        #region Play / open

        internal static bool PlayOriginalGame(GameIndex game, bool playMP = false)
        {
            (bool success, string gameExe, string gamePath) =
                CheckAndReturnFinalGameExeAndGamePath(game, playingOriginalGame: true, playMP);
            if (!success) return false;
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            SetUsAsSelector(game, gamePath);

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
                Core.View.ShowAlert(LText.AlertMessages.Play_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var game = GameToGameIndex(fm.Game);

            (bool success, string gameExe, string gamePath) =
                CheckAndReturnFinalGameExeAndGamePath(game, playingOriginalGame: false, playMP);
            if (!success) return false;

            // Always do this for robustness, see below
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            SetUsAsSelector(game, gamePath);

            string steamArgs = "";
            string workingPath = Config.GetGamePath(game);
            var sv = GetSteamValues(game, playMP);
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

            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game is not Dark, is unknown, or is unsupported for FM " + (!fm.Archive.IsEmpty() ? fm.Archive : fm.InstalledDir) + "\r\n" +
                    "fm.Game was: " + fm.Game, stackTrace: true);
                Core.View.ShowAlert(LText.AlertMessages.DromEd_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            // This is different from the above: The above is just checking if the game is known, while this is
            // checking if it's Dark specifically, because we don't support Thief 3 for editor opening.
            // This should never happen because our menu item is supposed to be hidden for Thief 3 FMs.
            if (!GameIsDark(fm.Game)) return false;

            var game = GameToGameIndex(fm.Game);

            string gamePath = Config.GetGamePath(game);
            if (gamePath.IsEmpty())
            {
                Log("Game path is empty for " + fm.Game, stackTrace: true);
                return false;
            }

            string editorExe = Config.GetEditorExe_FromDisk(game);
            if (editorExe.IsEmpty())
            {
                Core.View.ShowAlert(fm.Game == Game.SS2
                    ? LText.AlertMessages.ShockEd_ExecutableNotFound
                    : LText.AlertMessages.DromEd_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            // Just in case, and for consistency
            Paths.CreateOrClearTempPath(Paths.StubCommTemp);

            // TODO: We don't need to do this here, right?
            SetUsAsSelector(game, gamePath);

            // Since we don't use the stub currently, set this here
            // TODO: DromEd game mode doesn't even work for me anymore. Black screen no matter what. So I can't test if we need languages.
            GameConfigFiles.SetCamCfgLanguage(gamePath, "");

            // Why not
            GenerateMissFlagFileIfRequired(fm);

            // We don't need the stub for DromEd, cause we don't need to pass anything except the fm folder
            StartExe(editorExe, gamePath, "-fm=\"" + fm.InstalledDir + "\"");

            return true;
        }

        #endregion

        #region Helpers

        private static void SetUsAsSelector(GameIndex game, string gamePath)
        {
            bool success = GameIsDark(game)
                ? GameConfigFiles.SetDarkFMSelector(game, gamePath)
                : GameConfigFiles.SetT3FMSelector();
            if (!success)
            {
                Log("Unable to set us as the selector for " + Config.GetGameExe(game) + " (" +
                    (GameIsDark(game) ? nameof(GameConfigFiles.SetDarkFMSelector) : nameof(GameConfigFiles.SetT3FMSelector)) +
                    " returned false)", stackTrace: true);
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
                    sw.WriteLine("DisabledMods=" + (fm.DisableAllMods ? "*" : fm.DisabledMods));
                    // Pass blank if we have nothing, so the stub will leave whatever was in there before
                    if (!sLanguage.IsEmpty()) sw.WriteLine("Language=" + sLanguage);
                    if (bForceLanguage != null) sw.WriteLine("ForceLanguage=" + (bool)bForceLanguage);
                }
            }
            catch (Exception ex)
            {
                Log("Exception writing stub file " + Paths.StubFileName, ex);
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
                Log("Exception starting " + exe + "\r\n" +
                    "workingPath: " + workingPath + "\r\n" +
                    "args: " + args, ex);
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
                Core.View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_GamePathNotFound,
                    LText.AlertMessages.Alert);
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
                Core.View.ShowAlert(gameName + ":\r\n" + exeNotFoundMessage, LText.AlertMessages.Alert);
                return failed;
            }

            #endregion

            #region Exe: Fail if already running

            if (GameIsRunning(gameExe, checkAllGames: true))
            {
                Core.View.ShowAlert(LText.AlertMessages.Play_AnyGameIsRunning, LText.AlertMessages.Alert);
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
            if (fm.Game != Game.Thief1 && fm.Game != Game.Thief2) return;

            string instFMsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);

            string fmInstalledPath;
            try
            {
                fmInstalledPath = Path.Combine(instFMsBasePath, fm.InstalledDir);

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

            if (fm.Game == Game.Null)
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            if (fm.Game == Game.Unsupported)
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_UnsupportedGameType, LText.AlertMessages.Alert);
                return false;
            }

            string fmArchivePath = FMArchives.FindFirstMatch(fm.Archive);

            if (fmArchivePath.IsEmpty())
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_ArchiveNotFound, LText.AlertMessages.Alert);
                return false;
            }

            string gameExe = Config.GetGameExeUnsafe(fm.Game);
            string gameName = GetLocalizedGameName(fm.Game);
            if (!File.Exists(gameExe))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            string instBasePath = Config.GetFMInstallPathUnsafe(fm.Game);

            if (!Directory.Exists(instBasePath))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                    LText.AlertMessages.Install_FMInstallPathNotFound, LText.AlertMessages.Alert);
                return false;
            }

            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_GameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            string fmInstalledPath = Path.Combine(instBasePath, fm.InstalledDir);

            _extractCts = new CancellationTokenSource();

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
            if (GameIsDark(fm.Game))
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

            // Not doing RefreshSelectedFM(rowOnly: true) because that wouldn't update the install/uninstall buttons
            Core.View.RefreshSelectedFM();

            return true;
        }

        private static bool InstallFMZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            try
            {
                Directory.CreateDirectory(fmInstalledPath);

                using var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read, leaveOpen: false);

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

                    int percent = GetPercentFromValue(i + 1, filesCount);

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
                    Core.View.ShowAlert(LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially,
                        LText.AlertMessages.Alert)));
            }

            return !canceled;
        }

        private static bool InstallFMSevenZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            try
            {
                Directory.CreateDirectory(fmInstalledPath);

                using var extractor = new SevenZipExtractor(fmArchivePath);

                extractor.Extracting += (sender, e) =>
                {
                    if (!canceled && _extractCts.Token.IsCancellationRequested)
                    {
                        canceled = true;
                    }
                    if (canceled)
                    {
                        Core.View.InvokeSync(new Action(Core.View.SetCancelingFMInstall));
                        return;
                    }
                    Core.View.InvokeSync(new Action(() => Core.View.ReportFMExtractProgress(e.PercentDone)));
                };

                extractor.FileExtractionFinished += (sender, e) =>
                {
                    // We're extracting all the files, so we don't need to do an index check here.
                    if (!e.FileInfo.IsDirectory)
                    {
                        // We don't need to set timestamps because we're using ExtractArchive(), but we
                        // call this to remove the ReadOnly attribute
                        // TODO: Unset readonly for directories too
                        SetFileAttributesFromSevenZipEntry(e.FileInfo, Path.Combine(fmInstalledPath, e.FileInfo.FileName));
                    }

                    if (_extractCts.Token.IsCancellationRequested)
                    {
                        Core.View.InvokeSync(new Action(Core.View.SetCancelingFMInstall));
                        canceled = true;
                        e.Cancel = true;
                    }
                };

                try
                {
                    extractor.ExtractArchive(fmInstalledPath);
                }
                catch (Exception ex)
                {
                    // Throws a weird exception even if everything's fine
                    Log("extractor.ExtractArchive(fmInstalledPath) exception (probably ignorable)", ex);
                }
            }
            catch (Exception ex)
            {
                Log("Exception extracting 7z " + fmArchivePath + " to " + fmInstalledPath, ex);
                Core.View.InvokeSync(new Action(() =>
                   Core.View.ShowAlert(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially,
                       LText.AlertMessages.Alert)));
            }

            return !canceled;
        }

        internal static void CancelInstallFM() => _extractCts.CancelIfNotDisposed();

        #endregion

        #region Uninstall

        private static async Task UninstallFM(FanMission fm)
        {
            if (!fm.Installed || !GameIsKnownAndSupported(fm.Game)) return;

            if (Config.ConfirmUninstall)
            {
                (bool cancel, bool dontAskAgain) = Core.View.AskToContinueYesNoCustomStrings(
                        LText.AlertMessages.Uninstall_Confirm,
                        LText.AlertMessages.Confirm,
                        TaskDialogIcon.Warning,
                        showDontAskAgain: true,
                        LText.AlertMessages.Uninstall,
                        LText.Global.Cancel);

                if (cancel) return;

                Config.ConfirmUninstall = !dontAskAgain;
            }

            string gameExe = Config.GetGameExeUnsafe(fm.Game);
            string gameName = GetLocalizedGameName(fm.Game);
            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.Uninstall_GameIsRunning,
                    LText.AlertMessages.Alert);
                return;
            }

            Core.View.ShowProgressBox(ProgressTask.UninstallFM);

            try
            {
                string fmInstalledPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);

                bool fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    bool yes = Core.View.AskToContinue(LText.AlertMessages.Uninstall_FMAlreadyUninstalled,
                        LText.AlertMessages.Alert);
                    if (yes)
                    {
                        fm.Installed = false;
                        Core.View.RefreshSelectedFM();
                    }
                    return;
                }

                string fmArchivePath = await Task.Run(() => FMArchives.FindFirstMatch(fm.Archive));

                if (fmArchivePath!.IsEmpty())
                {
                    (bool cancel, _) = Core.View.AskToContinueYesNoCustomStrings(
                        LText.AlertMessages.Uninstall_ArchiveNotFound,
                        LText.AlertMessages.Warning,
                        TaskDialogIcon.Warning,
                        showDontAskAgain: false,
                        LText.AlertMessages.Uninstall,
                        LText.Global.Cancel);

                    if (cancel) return;
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
                        Core.View.AskToContinueWithCancelCustomStrings(
                            message + "\r\n\r\n" + LText.AlertMessages.Uninstall_BackupChooseNoNote,
                            LText.AlertMessages.Confirm,
                            null,
                            showDontAskAgain: true,
                            LText.AlertMessages.BackUp,
                            LText.AlertMessages.DontBackUp,
                            LText.Global.Cancel);

                    if (cancel) return;

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
                if (!await Task.Run(() => DeleteFMInstalledDirectory(fmInstalledPath)))
                {
                    // TODO: Make option to open the folder in Explorer and delete it manually?
                    Core.View.ShowAlert(LText.AlertMessages.Uninstall_UninstallNotCompleted, LText.AlertMessages.Alert);
                }

                fm.Installed = false;

                // NewDarkLoader still truncates its Thief 3 install names, but the "official" way is not to
                // do it for Thief 3. If the user already has FMs that were installed with NewDarkLoader, we
                // just read in the truncated names and treat them as normal for compatibility purposes. But
                // if we've just uninstalled the mission, then we can safely convert InstalledDir back to full
                // un-truncated form for future use.
                if (fm.Game == Game.Thief3 && !fm.Archive.IsEmpty())
                {
                    fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(truncate: false);
                }

                Ini.WriteFullFMDataIni();
                Core.View.RefreshSelectedFM();
            }
            catch (Exception ex)
            {
                Log("Exception uninstalling FM " + fm.Archive + ", " + fm.InstalledDir, ex);
                Core.View.ShowAlert(LText.AlertMessages.Uninstall_FailedFullyOrPartially, LText.AlertMessages.Alert);
            }
            finally
            {
                Core.View.HideProgressBox();
            }
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using Ookii.Dialogs.WinForms;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.CustomControls.ProgressPanel;
using static AngelLoader.FMBackupAndRestore;

namespace AngelLoader
{
    internal static class InstallAndPlay
    {
        private static CancellationTokenSource ExtractCts;

        internal static async Task InstallOrUninstall(FanMission fm) => await (fm.Installed ? UninstallFM(fm) : InstallFM(fm));

        internal static async Task InstallIfNeededAndPlay(FanMission fm, bool askConfIfRequired = false, bool playMP = false)
        {
            Debug.Assert(!playMP || fm.Game == Game.Thief2, nameof(playMP) + " is true and fm.Game is not Thief 2");

            if (askConfIfRequired && Config.ConfirmPlayOnDCOrEnter)
            {
                var (cancel, dontAskAgain) = Core.View.AskToContinueYesNoCustomStrings(LText.AlertMessages.Play_ConfirmMessage,
                    LText.AlertMessages.Confirm, icon: null, showDontAskAgain: true, yes: null, no: null);

                if (cancel) return;

                Config.ConfirmPlayOnDCOrEnter = !dontAskAgain;
            }

            if (!fm.Installed && !await InstallFM(fm)) return;

            if (playMP && fm.Game == Game.Thief2 && GetT2MultiplayerExe().IsEmpty())
            {
                Core.View.ShowAlert(LText.AlertMessages.Thief2_Multiplayer_ExecutableNotFound, LText.AlertMessages.Alert);
                return;
            }

            if (PlayFM(fm, playMP))
            {
                fm.LastPlayed = DateTime.Now;
                await Core.View.RefreshSelectedFM(refreshReadme: false);
            }
        }

        #region Play / open

        internal static bool PlayOriginalGame(Game game, bool playMP = false)
        {
            var (success, gameExe, gamePath) = GetGameExeAndPath(game, LText.AlertMessages.Play_ExecutableNotFound);
            if (!success) return false;

            // Even though we're not actually loading an FM, we still want to set us as the selector so that our
            // stub can explicitly tell Thief to play without an FM. Otherwise, if another selector was specified,
            // then that selector would start upon running of the game exe, which would be bad.
            SetUsAsSelector(game, gameExe, gamePath);

            // When the stub finds nothing in the stub comm folder, it will just start the game with no FM
            Paths.PrepareTempPath(Paths.StubCommTemp);

            if (playMP) gameExe = Path.Combine(gamePath, Paths.T2MPExe);

            StartExe(gameExe, gamePath, null);

            return true;
        }

        private static bool PlayFM(FanMission fm, bool playMP = false)
        {
            if (fm.Game == Game.Null)
            {
                Core.View.ShowAlert(LText.AlertMessages.Play_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var (success, gameExe, gamePath) =
                GetGameExeAndPath(fm.Game, LText.AlertMessages.Play_ExecutableNotFoundFM, playMP);
            if (!success) return false;

            SetUsAsSelector(fm.Game, gameExe, gamePath);

            // Only use the stub if we need to pass something we can't pass on the command line
            // Add quotes around it in case there are spaces in the dir name. Will only happen if you put an FM
            // dir in there manually. Which if you do, you're on your own mate.
            var args = "-fm=\"" + fm.InstalledDir + "\"";
            if (!fm.DisabledMods.IsWhiteSpace() || fm.DisableAllMods)
            {
                args = "-fm";
                Paths.PrepareTempPath(Paths.StubCommTemp);

                try
                {
                    using (var sw = new StreamWriter(Paths.StubCommFilePath, false, Encoding.UTF8))
                    {
                        sw.WriteLine("SelectedFMName=" + fm.InstalledDir);
                        sw.WriteLine("DisabledMods=" + (fm.DisableAllMods ? "*" : fm.DisabledMods));
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception writing stub file " + Paths.StubFileName, ex);
                }
            }

            StartExe(gameExe, gamePath, args);

            // Don't clear the temp folder here, because the stub program will need to read from it. It will
            // delete the temp file itself after it's done with it.

            return true;
        }

        internal static bool OpenFMInDromEd(FanMission fm)
        {
            #region Checks (specific to DromEd)

            if (!GameIsDark(fm)) return false;

            if (fm.Game == Game.Null)
            {
                Core.View.ShowAlert(LText.AlertMessages.DromEd_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var gameExe = GetGameExeFromGameType(fm.Game);
            if (gameExe.IsEmpty())
            {
                Log("gameExe is empty for " + fm.Game, stackTrace: true);
                return false;
            }

            var dromedExe = GetDromEdExe(fm.Game);
            if (dromedExe.IsEmpty())
            {
                Core.View.ShowAlert(LText.AlertMessages.DromEd_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty()) return false;

            #endregion

            SetUsAsSelector(fm.Game, gameExe, gamePath);

            // We don't need the stub for DromEd, cause we don't need to pass anything except the fm folder
            StartExe(dromedExe, gamePath, "-fm=\"" + fm.InstalledDir + "\"");

            return true;
        }

        #endregion

        #region Helpers

        private static void StartExe(string exe, string workingPath, string args)
        {
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = exe;
                if (!args.IsEmpty()) proc.StartInfo.Arguments = args;
                proc.StartInfo.WorkingDirectory = workingPath;
                try
                {
                    proc.Start();
                }
                catch (Exception ex)
                {
                    Log("Exception starting " + exe, ex);
                }
            }
        }

        private static (bool Success, string gameExe, string gamePath)
        GetGameExeAndPath(Game game, string exeNotFoundMessage, bool playMP = false)
        {
            (bool, string, string) failed = (false, null, null);

            if (game == Game.Null) return failed;

            var gameExe = GetGameExeFromGameType(game);

            #region Exe: Fail if blank or not found

            var gameName = GetGameNameFromGameType(game);

            if (gameExe.IsEmpty() || !File.Exists(gameExe))
            {
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

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty())
            {
                Core.View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_GamePathNotFound,
                    LText.AlertMessages.Alert);
                return failed;
            }

            if (playMP) gameExe = Path.Combine(gamePath, Paths.T2MPExe);

            return (true, gameExe, gamePath);
        }

        #endregion

        #region Set us as selector

        private static void SetUsAsSelector(Game game, string gameExe, string gamePath)
        {
            Debug.Assert(GameIsKnownAndSupported(game), "!GameIsKnownAndSupported(game)");

            bool success = GameIsDark(game) ? SetUsAsDarkFMSelector(gameExe, gamePath) : SetUsAsT3FMSelector();
            if (!success)
            {
                Log("Unable to set us as the selector for " + gameExe + " (" +
                    (GameIsDark(game) ? nameof(SetUsAsDarkFMSelector) : nameof(SetUsAsT3FMSelector)) +
                    " returned false)", stackTrace: true);
            }
        }

        private static bool SetUsAsDarkFMSelector(string gameExe, string gamePath)
        {
            const string fmSelectorKey = "fm_selector";

            var camModIni = Path.Combine(gamePath, "cam_mod.ini");
            if (!File.Exists(camModIni))
            {
                Log("cam_mod.ini not found for " + gameExe, stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(camModIni).ToList();
            }
            catch (Exception ex)
            {
                Log("Exception reading cam_mod.ini for " + gameExe, ex);
                return false;
            }

            // Confirmed ND T1/T2 can read this with both forward and backward slashes
            var stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

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
            bool loaderIsAlreadyUs = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var lt = lines[i].TrimStart();

                do
                {
                    lt = lt.TrimStart(';').Trim();
                } while (lt.Length > 0 && lt[0] == ';');

                if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length && lt
                        .Substring(fmSelectorKey.Length + 1).TrimStart().ToBackSlashes()
                        .EqualsI(stubPath.ToBackSlashes()))
                {
                    if (loaderIsAlreadyUs)
                    {
                        lines.RemoveAt(i);
                        i--;
                        lastSelKeyIndex = (lastSelKeyIndex - 1).Clamp(-1, int.MaxValue);
                    }
                    else
                    {
                        lines[i] = fmSelectorKey + " " + stubPath;
                        loaderIsAlreadyUs = true;
                    }
                    continue;
                }

                if (lt.EqualsI(fmSelectorKey) ||
                    (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                    (lt[fmSelectorKey.Length] == ' ' || lt[fmSelectorKey.Length] == '\t')))
                {
                    if (!lines[i].TrimStart().StartsWith(";")) lines[i] = ";" + lines[i];
                    lastSelKeyIndex = i;
                }
            }

            if (!loaderIsAlreadyUs)
            {
                if (lastSelKeyIndex == -1 || lastSelKeyIndex == lines.Count - 1)
                {
                    lines.Add(fmSelectorKey + " " + stubPath);
                }
                else
                {
                    lines.Insert(lastSelKeyIndex + 1, fmSelectorKey + " " + stubPath);
                }
            }

            try
            {
                File.WriteAllLines(camModIni, lines);
            }
            catch (Exception ex)
            {
                Log("Exception writing cam_mod.ini for " + gameExe, ex);
                return false;
            }

            return true;
        }

        // If only you could do this with a command-line switch. You can say -fm to always start with the loader,
        // and you can say -fm=name to always start with the named FM, but you can't specify WHICH loader to use
        // on the command line. Only way to do it is through a file. Meh.
        private static bool SetUsAsT3FMSelector()
        {
            const string externSelectorKey = "ExternSelector=";
            bool existingKeyOverwritten = false;
            int insertLineIndex = -1;

            var ini = Paths.GetSneakyOptionsIni();
            if (ini.IsEmpty())
            {
                Log("Couldn't set us as the loader for Thief: Deadly Shadows because SneakyOptions.ini could not be found", stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(ini, Encoding.Default).ToList();
            }
            catch (Exception ex)
            {
                Log("Problem reading SneakyOptions.ini", ex);
                return false;
            }

            // Confirmed SU can read this with both forward and backward slashes
            var stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            for (var i = 0; i < lines.Count; i++)
            {
                if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                insertLineIndex = i + 1;
                while (i < lines.Count - 1)
                {
                    var lt = lines[i + 1].Trim();
                    if (lt.StartsWithI(externSelectorKey))
                    {
                        lines[i + 1] = externSelectorKey + stubPath;
                        existingKeyOverwritten = true;
                        break;
                    }

                    if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']') break;

                    i++;
                }
                break;
            }

            if (!existingKeyOverwritten)
            {
                if (insertLineIndex < 0) return false;
                lines.Insert(insertLineIndex, externSelectorKey + stubPath);
            }

            try
            {
                File.WriteAllLines(ini, lines, Encoding.Default);
            }
            catch (Exception ex)
            {
                Log("Problem writing SneakyOptions.ini", ex);
                return false;
            }

            return true;
        }

        #endregion

        #region Install / uninstall

        internal static async Task<bool> InstallFM(FanMission fm)
        {
            Debug.Assert(!fm.Installed, "!fm.Installed");

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

            var fmArchivePath = FindFMArchive(fm);

            if (fmArchivePath.IsEmpty())
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_ArchiveNotFound, LText.AlertMessages.Alert);
                return false;
            }

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var gameExe = GetGameExeFromGameType(fm.Game);
            var gameName = GetGameNameFromGameType(fm.Game);
            if (!File.Exists(gameExe))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var instBasePath = GetFMInstallsBasePath(fm.Game);

            if (!Directory.Exists(instBasePath))
            {
                Core.View.ShowAlert(LText.AlertMessages.Install_FMInstallPathNotFound, LText.AlertMessages.Alert);
                return false;
            }

            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_GameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            var fmInstalledPath = Path.Combine(instBasePath, fm.InstalledDir);

            ExtractCts = new CancellationTokenSource();

            Core.View.ShowProgressBox(ProgressTasks.InstallFM);

            // Framework zip extracting is much faster, so use it if possible
            bool canceled = fmArchivePath.ExtIsZip()
                ? !await InstallFMZip(fmArchivePath, fmInstalledPath)
                : !await InstallFMSevenZip(fmArchivePath, fmInstalledPath);

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

            Core.WriteFullFMDataIni();

            try
            {
                using (var sw = new StreamWriter(Path.Combine(fmInstalledPath, Paths.FMSelInf), append: false))
                {
                    await sw.WriteLineAsync("Name=" + fm.InstalledDir);
                    await sw.WriteLineAsync("Archive=" + fm.Archive);
                }
            }
            catch (Exception ex)
            {
                Log("Couldn't create " + Paths.FMSelInf + " in " + fmInstalledPath, ex);
            }

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm.Game));
            try
            {
                Core.View.ShowProgressBox(ProgressTasks.ConvertFiles);
                await ac.ConvertMP3sToWAVs();

                if (Config.ConvertOGGsToWAVsOnInstall)
                {
                    await ac.ConvertOGGsToWAVs();
                }
                else if (Config.ConvertWAVsTo16BitOnInstall)
                {
                    await ac.ConvertWAVsTo16Bit();
                }
            }
            catch (Exception ex)
            {
                Log("Exception in audio conversion", ex);
            }

            try
            {
                await RestoreSavesAndScreenshots(fm);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(RestoreSavesAndScreenshots), ex);
            }
            finally
            {
                Core.View.HideProgressBox();
            }

            // Not doing RefreshSelectedFMRowOnly() because that wouldn't update the install/uninstall buttons
            await Core.View.RefreshSelectedFM(refreshReadme: false);

            return true;
        }

        private static async Task<bool> InstallFMZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(fmInstalledPath);

                    var fs = new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read);
                    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false))
                    {
                        int filesCount = archive.Entries.Count;
                        for (var i = 0; i < filesCount; i++)
                        {
                            var entry = archive.Entries[i];

                            var fileName = entry.FullName.Replace('/', '\\');

                            if (fileName[fileName.Length - 1] == '\\') continue;

                            if (fileName.Contains('\\'))
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                    fileName.Substring(0, fileName.LastIndexOf('\\'))));
                            }

                            var extractedName = Path.Combine(fmInstalledPath, fileName);
                            entry.ExtractToFile(extractedName, overwrite: true);

                            UnSetReadOnly(Path.Combine(fmInstalledPath, extractedName));

                            int percent = (100 * (i + 1)) / filesCount;

                            Core.View.InvokeAsync(new Action(() => Core.View.ReportFMExtractProgress(percent)));

                            if (ExtractCts.Token.IsCancellationRequested)
                            {
                                canceled = true;
                                return;
                            }
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
            });

            return !canceled;
        }

        private static async Task<bool> InstallFMSevenZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(fmInstalledPath);

                    using (var extractor = new SevenZipExtractor(fmArchivePath))
                    {
                        extractor.Extracting += (sender, e) =>
                        {
                            if (!canceled && ExtractCts.Token.IsCancellationRequested)
                            {
                                canceled = true;
                            }
                            if (canceled)
                            {
                                Core.View.InvokeAsync(new Action(Core.View.SetCancelingFMInstall));
                                return;
                            }
                            Core.View.InvokeAsync(new Action(() => Core.View.ReportFMExtractProgress(e.PercentDone)));
                        };

                        extractor.FileExtractionFinished += (sender, e) =>
                        {
                            SetFileAttributesFromSevenZipEntry(e.FileInfo,
                                Path.Combine(fmInstalledPath, e.FileInfo.FileName));

                            if (ExtractCts.Token.IsCancellationRequested)
                            {
                                Core.View.InvokeAsync(new Action(Core.View.SetCancelingFMInstall));
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
                            Log("extractor.ExtractArchive(fmInstalledPath) exception (probably ignorable)",
                                ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception extracting 7z " + fmArchivePath + " to " + fmInstalledPath, ex);
                    Core.View.InvokeSync(new Action(() =>
                       Core.View.ShowAlert(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially,
                           LText.AlertMessages.Alert)));
                }
            });

            return !canceled;
        }

        internal static void CancelInstallFM()
        {
            try
            {
                ExtractCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static async Task UninstallFM(FanMission fm)
        {
            if (!fm.Installed || !GameIsKnownAndSupported(fm)) return;

            Debug.Assert(fm.Game != Game.Null, "fm.Game is Game.Null");

            if (Config.ConfirmUninstall)
            {
                var (cancel, dontAskAgain) =
                    Core.View.AskToContinueYesNoCustomStrings(LText.AlertMessages.Uninstall_Confirm,
                        LText.AlertMessages.Confirm, TaskDialogIcon.Warning, showDontAskAgain: true,
                        LText.AlertMessages.Uninstall, LText.Global.Cancel);

                if (cancel) return;

                Config.ConfirmUninstall = !dontAskAgain;
            }

            var gameExe = GetGameExeFromGameType(fm.Game);
            var gameName = GetGameNameFromGameType(fm.Game);
            if (GameIsRunning(gameExe))
            {
                Core.View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.Uninstall_GameIsRunning, LText.AlertMessages.Alert);
                return;
            }

            Core.View.ShowProgressBox(ProgressTasks.UninstallFM);

            try
            {
                var fmInstalledPath = Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir);

                var fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    var yes = Core.View.AskToContinue(LText.AlertMessages.Uninstall_FMAlreadyUninstalled,
                        LText.AlertMessages.Alert);
                    if (yes)
                    {
                        fm.Installed = false;
                        await Core.View.RefreshSelectedFM(refreshReadme: false);
                    }
                    return;
                }

                var fmArchivePath = await Task.Run(() => FindFMArchive(fm));

                if (fmArchivePath.IsEmpty())
                {
                    var (cancel, _) = Core.View.AskToContinueYesNoCustomStrings(
                        LText.AlertMessages.Uninstall_ArchiveNotFound, LText.AlertMessages.Warning,
                        TaskDialogIcon.Warning, showDontAskAgain: false, LText.AlertMessages.Uninstall,
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

                if (Config.BackupAlwaysAsk)
                {
                    var message = Config.BackupFMData == BackupFMData.SavesAndScreensOnly
                        ? LText.AlertMessages.Uninstall_BackupSavesAndScreenshots
                        : LText.AlertMessages.Uninstall_BackupAllData;
                    var (cancel, cont, dontAskAgain) =
                        Core.View.AskToContinueWithCancelCustomStrings(
                            message + "\r\n\r\n" + LText.AlertMessages.Uninstall_BackupChooseNoNote,
                            LText.AlertMessages.Confirm, null, showDontAskAgain: true,
                            LText.AlertMessages.BackUp, LText.AlertMessages.DontBackUp, LText.Global.Cancel);

                    if (cancel) return;

                    Config.BackupAlwaysAsk = !dontAskAgain;
                    if (cont) await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }
                else
                {
                    await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }

                // TODO: Give the user the option to retry or something, if it's cause they have a file open
                if (!await DeleteFMInstalledDirectory(fmInstalledPath))
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

                Core.WriteFullFMDataIni();
                await Core.View.RefreshSelectedFM(refreshReadme: false);
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

        private static async Task<bool> DeleteFMInstalledDirectory(string path)
        {
            return await Task.Run(() =>
            {
                var triedReadOnlyRemove = false;

                // Failsafe cause this is nasty
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        Directory.Delete(path, recursive: true);
                        return true;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            if (triedReadOnlyRemove) return false;

                            // FMs installed by us will not have any readonly attributes set, so we work on the
                            // assumption that this is the rarer case and only do this extra work if we need to.
                            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                            {
                                new FileInfo(f).IsReadOnly = false;
                            }

                            foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                            {
                                new DirectoryInfo(d).Attributes = FileAttributes.Normal;
                            }

                            triedReadOnlyRemove = true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }

                return false;
            });
        }

        #endregion
    }
}

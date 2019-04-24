using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.CustomControls;
using AngelLoader.Importing;
using FMScanner;
using Ookii.Dialogs.WinForms;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.FMBackupAndRestore;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal static class FMInstallAndPlay
    {
        private static CancellationTokenSource ExtractCts;

        internal static async Task<bool> InstallFM(FanMission fm)
        {
            Debug.Assert(!fm.Installed, "!fm.Installed");

            if (fm.Game == null)
            {
                Model.View.ShowAlert(LText.AlertMessages.Install_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            if (fm.Game == Game.Unsupported)
            {
                Model.View.ShowAlert(LText.AlertMessages.Install_UnsupportedGameType, LText.AlertMessages.Alert);
                return false;
            }

            var fmArchivePath = FindFMArchive(fm);

            if (fmArchivePath.IsEmpty())
            {
                Model.View.ShowAlert(LText.AlertMessages.Install_ArchiveNotFound, LText.AlertMessages.Alert);
                return false;
            }

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
            if (!File.Exists(gameExe))
            {
                Model.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var instBasePath = GetFMInstallsBasePath(fm.Game);

            if (!Directory.Exists(instBasePath))
            {
                Model.View.ShowAlert(LText.AlertMessages.Install_FMInstallPathNotFound, LText.AlertMessages.Alert);
                return false;
            }

            if (GameIsRunning(gameExe))
            {
                Model.View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_GameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            var fmInstalledPath = Path.Combine(instBasePath, fm.InstalledDir);

            ExtractCts = new CancellationTokenSource();

            Model.ProgressBox.ShowInstallingFM();

            // Framework zip extracting is much faster, so use it if possible
            bool canceled = fmArchivePath.ExtIsZip()
                ? !await InstallFMZip(fmArchivePath, fmInstalledPath)
                : !await InstallFMSevenZip(fmArchivePath, fmInstalledPath);

            if (canceled)
            {
                Model.ProgressBox.SetCancelingFMInstall();
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
                Model.ProgressBox.HideThis();
                return false;
            }

            fm.Installed = true;

            WriteFullFMDataIni();

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
                Model.ProgressBox.ShowConvertingFiles();
                await ac.ConvertMP3sToWAVs();

                if (Config.ConvertOGGsToWAVsOnInstall)
                {
                    await ac.ConvertOGGsToWAVsInternal();
                }
                else if (Config.ConvertWAVsTo16BitOnInstall)
                {
                    await ac.ConvertWAVsTo16BitInternal();
                }
            }
            finally
            {
                Model.ProgressBox.HideThis();
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
                Model.ProgressBox.HideThis();
            }

            // Not doing RefreshSelectedFMRowOnly() because that wouldn't update the install/uninstall buttons
            await Model.View.RefreshSelectedFM(refreshReadme: false);

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

                            Model.View.InvokeSync(new Action(() => Model.ProgressBox.ReportFMExtractProgress(percent)));

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
                    Model.View.InvokeSync(new Action(() =>
                        Model.View.ShowAlert(LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially,
                            LText.AlertMessages.Alert)));
                }
                finally
                {
                    Model.View.InvokeSync(new Action(() => Model.ProgressBox.HideThis()));
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
                                Model.ProgressBox.BeginInvoke(new Action(Model.ProgressBox.SetCancelingFMInstall));
                                return;
                            }
                            Model.ProgressBox.BeginInvoke(new Action(() =>
                                Model.ProgressBox.ReportFMExtractProgress(e.PercentDone)));
                        };

                        extractor.FileExtractionFinished += (sender, e) =>
                        {
                            SetFileAttributesFromSevenZipEntry(e.FileInfo,
                                Path.Combine(fmInstalledPath, e.FileInfo.FileName));

                            if (ExtractCts.Token.IsCancellationRequested)
                            {
                                Model.ProgressBox.BeginInvoke(new Action(Model.ProgressBox.SetCancelingFMInstall));
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
                    Model.View.InvokeSync(new Action(() =>
                       Model.View.ShowAlert(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially,
                           LText.AlertMessages.Alert)));
                }
                finally
                {
                    Model.View.InvokeSync(new Action(() => Model.ProgressBox.HideThis()));
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

        private static async Task<bool> DeleteFMInstalledDirectory(string path)
        {
            bool result = await Task.Run(() =>
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

            return result;
        }

        internal static async Task UninstallFM(FanMission fm)
        {
            if (!fm.Installed || !GameIsKnownAndSupported(fm)) return;

            Debug.Assert(fm.Game != null, "fm.Game != null");

            if (Config.ConfirmUninstall)
            {
                var (cancel, dontAskAgain) =
                    Model.View.AskToContinueYesNoCustomStrings(LText.AlertMessages.Uninstall_Confirm,
                        LText.AlertMessages.Confirm, TaskDialogIcon.Warning, showDontAskAgain: true,
                        LText.AlertMessages.Uninstall, LText.Global.Cancel);
                Config.ConfirmUninstall = !dontAskAgain;
                if (cancel) return;
            }

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
            if (GameIsRunning(gameExe))
            {
                Model.View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.Uninstall_GameIsRunning, LText.AlertMessages.Alert);
                return;
            }

            Model.ProgressBox.ShowUninstallingFM();

            try
            {
                var fmInstalledPath = Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir);

                var fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    var yes = Model.View.AskToContinue(LText.AlertMessages.Uninstall_FMAlreadyUninstalled,
                        LText.AlertMessages.Alert);
                    if (yes)
                    {
                        fm.Installed = false;
                        await Model.View.RefreshSelectedFM(refreshReadme: false);
                    }
                    return;
                }

                var fmArchivePath = await Task.Run(() => FindFMArchive(fm));

                if (fmArchivePath.IsEmpty())
                {
                    var (cancel, _) = Model.View.AskToContinueYesNoCustomStrings(
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
                        Model.View.AskToContinueWithCancelCustomStrings(
                            message + "\r\n\r\n" + LText.AlertMessages.Uninstall_BackupChooseNoNote,
                            LText.AlertMessages.Confirm, null, showDontAskAgain: true,
                            LText.AlertMessages.BackUp, LText.AlertMessages.DontBackUp, LText.Global.Cancel);
                    Config.BackupAlwaysAsk = !dontAskAgain;
                    if (cancel) return;
                    if (cont) await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }
                else
                {
                    await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }

                // --- DEBUG
                //return;

                // TODO: Give the user the option to retry or something, if it's cause they have a file open
                if (!await DeleteFMInstalledDirectory(fmInstalledPath))
                {
                    // TODO: Make option to open the folder in Explorer and delete it manually?
                    Model.View.ShowAlert(LText.AlertMessages.Uninstall_UninstallNotCompleted,
                        LText.AlertMessages.Alert);
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

                WriteFullFMDataIni();
                await Model.View.RefreshSelectedFM(refreshReadme: false);
            }
            catch (Exception ex)
            {
                Log("Exception uninstalling FM " + fm.Archive + ", " + fm.InstalledDir, ex);
                Model.View.InvokeSync(new Action(() =>
                    Model.View.ShowAlert(LText.AlertMessages.Uninstall_FailedFullyOrPartially,
                        LText.AlertMessages.Alert)));
            }
            finally
            {
                Model.ProgressBox.HideThis();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using JetBrains.Annotations;
using Microsoft.VisualBasic.FileIO; // the import of shame
using static AngelLoader.Logger;
using static AngelLoader.Misc;
using SearchOption = System.IO.SearchOption;

namespace AngelLoader
{
    [PublicAPI]
    internal static class FMArchives
    {
        /// <summary>
        /// Returns the list of FM archive paths, returning subfolders as well if that option is enabled.
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetFMArchivePaths()
        {
            // Always return a COPY of the paths list, so the caller can modify it safely if it wants
            var paths = new List<string>(Config.FMArchivePaths.Count);
            foreach (string path in Config.FMArchivePaths)
            {
                paths.Add(path);
                if (Config.FMArchivePathsIncludeSubfolders)
                {
                    try
                    {
                        string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                        for (int di = 0; di < dirs.Length; di++)
                        {
                            string dir = dirs[di];
                            if (!dir.GetDirNameFast().EqualsI(".fix") &&
                                // @DIRSEP: '/' conversion due to string.ContainsI()
                                !dir.ToForwardSlashes().ContainsI("/.fix/"))
                            {
                                paths.Add(dir);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // @BetterErrors(GetFMArchivePaths() where IncludeSubfolders == true)
                        Log("subfolders=true, Exception in GetDirectories", ex);
                    }
                }
            }

            return paths;
        }

        /// <summary>
        /// Returns the full path of the first matching FM archive file, or the empty string if no matches were found.
        /// </summary>
        /// <param name="fmArchive"></param>
        /// <param name="archivePaths"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        internal static string FindFirstMatch(string fmArchive, List<string>? archivePaths = null, CancellationToken? ct = null)
        {
            if (fmArchive.IsEmpty()) return "";

            var paths = archivePaths?.Count > 0 ? archivePaths : GetFMArchivePaths();
            foreach (string path in paths)
            {
                if (ct != null && ((CancellationToken)ct).IsCancellationRequested)
                {
                    return "";
                }

                if (TryCombineFilePathAndCheckExistence(path, fmArchive, out string f))
                {
                    return f;
                }
            }

            return "";
        }

        /// <summary>
        /// Returns a list of all matching FM archive files, or an empty list if no matches were found.
        /// </summary>
        /// <param name="fmArchive"></param>
        /// <param name="archivePaths"></param>
        /// <returns></returns>
        internal static List<string> FindAllMatches(string fmArchive, List<string>? archivePaths = null)
        {
            if (fmArchive.IsEmpty()) return new List<string>();

            var paths = archivePaths?.Count > 0 ? archivePaths : GetFMArchivePaths();

            var list = new List<string>(paths.Count);

            foreach (string path in paths)
            {
                if (TryCombineFilePathAndCheckExistence(path, fmArchive, out string f))
                {
                    list.Add(f);
                }
            }

            return list;
        }

        /// <summary>
        /// Deletes <paramref name="fm"/>'s archive from disk, asking the user for confirmation first.
        /// </summary>
        /// <param name="fm"></param>
        /// <param name="multiple"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        internal static async Task Delete(FanMission fm, bool multiple = false, int percent = -1)
        {
            if (!multiple && fm.MarkedUnavailable)
            {
                // @MULTISEL: Localize and finalize
                Dialogs.ShowAlert(
                    "This FM is not available on disk, and so can't be deleted.",
                    LText.AlertMessages.Alert,
                    MessageBoxIcon.None);
                return;
            }

            // Use wait cursor in blocking thread, rather than putting this on its own thread.
            // The archive find operation _probably_ won't take long enough to warrant a progress box,
            // and if it's quick then the progress box looks like an annoying flicker.
            List<string> archives;
            try
            {
                Core.View.Cursor = Cursors.WaitCursor;

                archives = FindAllMatches(fm.Archive);
            }
            finally
            {
                Core.View.Cursor = Cursors.Default;
            }

            if (archives.Count == 0)
            {
                Dialogs.ShowAlert(LText.FMDeletion.ArchiveNotFound, LText.AlertMessages.DeleteFMArchive, MessageBoxIcon.Error);
                return;
            }

            bool singleArchive = archives.Count == 1;

            var finalArchives = new List<string>();

            if (!multiple || !singleArchive)
            {
                using (var f = new MessageBoxCustomForm(
                    messageTop: singleArchive
                        ? LText.FMDeletion.AboutToDelete + "\r\n\r\n" + archives[0]
                        : LText.FMDeletion.DuplicateArchivesFound,
                    messageBottom: "",
                    title: LText.AlertMessages.DeleteFMArchive,
                    icon: MessageBoxIcon.Warning,
                    okText: singleArchive ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs,
                    cancelText: LText.Global.Cancel,
                    okIsDangerous: true,
                    choiceStrings: singleArchive ? null : archives.ToArray()))
                {
                    if (f.ShowDialogDark() != DialogResult.OK) return;

                    finalArchives.AddRange(singleArchive ? archives : f.SelectedItems);
                }
            }
            else
            {
                finalArchives.AddRange(archives);
            }

            if (!multiple && fm.Installed)
            {
                (bool cancel, bool cont, _) = Dialogs.AskToContinueWithCancelCustomStrings(
                    message: LText.FMDeletion.AskToUninstallFMFirst,
                    title: LText.AlertMessages.DeleteFMArchive,
                    icon: MessageBoxIcon.Warning,
                    showDontAskAgain: false,
                    yes: LText.AlertMessages.Uninstall,
                    no: LText.AlertMessages.LeaveInstalled,
                    cancel: LText.Global.Cancel
                );

                if (cancel) return;

                if (cont)
                {
                    bool canceled = !await FMInstallAndPlay.Uninstall(fm);
                    if (canceled) return;
                }
            }

            try
            {
                if (multiple)
                {
                    Core.View.SetProgressBoxState_Single(
                        percent: percent > -1 ? percent : 0,
                        message2: GetFMId(fm)
                    );
                }
                else
                {
                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.DeletingFMArchive,
                        progressType: ProgressType.Indeterminate
                    );
                }

                await Task.Run(() =>
                {
                    foreach (string archive in finalArchives)
                    {
                        try
                        {
                            FileSystem.DeleteFile(archive, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                        }
                        catch (Exception ex)
                        {
                            Log("Exception deleting archive '" + archive + "'", ex);
                            Dialogs.ShowError(LText.AlertMessages.DeleteFM_UnableToDelete + "\r\n\r\n" + archive);
                        }
                    }
                });
            }
            finally
            {
                var newArchives = await Task.Run(() => FindAllMatches(fm.Archive));

                if (!multiple)
                {
                    Core.View.HideProgressBox();
                }

                if (newArchives.Count == 0 && !fm.Installed)
                {
                    fm.MarkedUnavailable = true;
                    if (!multiple)
                    {
                        await Core.View.SortAndSetFilter(keepSelection: true);
                    }
                }
            }
        }

        internal static async Task Delete(List<FanMission> fms)
        {
            for (int i = 0; i < fms.Count; i++)
            {
                if (fms[i].MarkedUnavailable)
                {
                    fms.RemoveAt(i);
                    i--;
                }
            }

            if (fms.Count == 0) return;

            var archivePaths = GetFMArchivePaths();
            int installedWithArchiveCount = 0;
            int installedNoArchiveCount = 0;
            int notInstalledNoArchiveCount = 0;
            int installedCount = 0;
            for (int i = 0; i < fms.Count; i++)
            {
                FanMission fm = fms[i];
                var matches = FindAllMatches(fm.Archive, archivePaths);
                if (matches.Count > 0)
                {
                    if (fm.Installed)
                    {
                        installedCount++;
                        installedWithArchiveCount++;
                    }
                }
                else
                {
                    if (fm.Installed)
                    {
                        installedCount++;
                        installedNoArchiveCount++;
                    }
                    else
                    {
                        notInstalledNoArchiveCount++;
                    }
                }
            }

            using (var f = new MessageBoxCustomForm(
                       messageTop: LText.FMDeletion.AboutToDelete_Multiple_BeforeNumber + fms.Count +
                                   LText.FMDeletion.AboutToDelete_Multiple_AfterNumber,
                       messageBottom: "",
                       title: LText.AlertMessages.DeleteFMArchives,
                       icon: MessageBoxIcon.Warning,
                       okText: LText.FMDeletion.DeleteFMs_CertainMultiple,
                       cancelText: LText.Global.Cancel,
                       okIsDangerous: true))
            {
                if (f.ShowDialogDark() != DialogResult.OK) return;
            }

            // @MULTISEL: Multi-selection Delete() method in-progress code
            // Since multiple archives with the same name should be the rare case (nobody should be doing it),
            // we'll just ask the user per-FM if we find any as we go. Sorry to stop your batch, but yeah.

            // In the iteration up there, look for any FMs that are installed, and then note we have to ask the
            // user if they want to uninstall them and if so, ask the usual backup options for all, and then if
            // some have no archive, also inform the user of the proper information in that case.
            // Also if all of them have no archive found and are NOT installed, throw up a dialog and return.

            // This thing just tells you to uninstall the FMs to delete them, so it's correct functionality
            if (installedNoArchiveCount == fms.Count)
            {
                Dialogs.ShowAlert(LText.FMDeletion.ArchiveNotFound_All, LText.AlertMessages.DeleteFMArchives, MessageBoxIcon.Error);
                return;
            }

            if (installedCount > 0)
            {
                (bool cancel, bool cont, _) = Dialogs.AskToContinueWithCancelCustomStrings(
                    message: LText.FMDeletion.AskToUninstallFMFirst_Multiple,
                    title: LText.AlertMessages.DeleteFMArchives,
                    icon: MessageBoxIcon.Warning,
                    showDontAskAgain: false,
                    yes: LText.AlertMessages.Uninstall,
                    no: LText.AlertMessages.LeaveInstalled,
                    cancel: LText.Global.Cancel
                );

                if (cancel) return;
                if (cont)
                {
                    FanMission[] installedFMs = new FanMission[installedCount];
                    for (int i = 0, i2 = 0; i < fms.Count; i++)
                    {
                        FanMission fm = fms[i];
                        if (fm.Installed)
                        {
                            installedFMs[i2] = fm;
                            i2++;
                        }
                    }
                    if (!await FMInstallAndPlay.Uninstall(installedFMs))
                    {
                        return;
                    }
                }

                // Even though we culled out the unavailable FMs already, Uninstall() could have marked some more
                // of them unavailable if they didn't have an archive after being uninstalled.
                for (int i = 0; i < fms.Count; i++)
                {
                    if (fms[i].MarkedUnavailable)
                    {
                        fms.RemoveAt(i);
                        i--;
                    }
                }

                if (fms.Count == 0)
                {
                    await Core.View.SortAndSetFilter(keepSelection: true);
                    return;
                }
            }

            try
            {
                Core.View.ShowProgressBox_Single(
                    message1: LText.ProgressBox.DeletingFMArchives,
                    progressType: ProgressType.Determinate,
                    cancelType: ProgressCancelType.Stop,
                    // @MULTISEL: Should be an actual action in final version
                    cancelAction: null
                );

                for (int i = 0; i < fms.Count; i++)
                {
                    await Delete(fms[i], multiple: true, Common.GetPercentFromValue_Int(i + 1, fms.Count));
                }
            }
            finally
            {
                await Core.View.SortAndSetFilter(keepSelection: true);
                Core.View.HideProgressBox();
            }
        }

        internal static async Task<bool> Add(IWin32Window owner, List<string> droppedItemsList)
        {
            if (Config.FMArchivePaths.Count == 0) return false;

            // Drag-and-drop operations block not only the app thread, but also the thread of the Explorer window
            // from which you dragged the files. Good lord. So shove the entire thing into another thread so our
            // drag-and-drop operation can finish in the UI thread and unblock Explorer.
            bool success = await Task.Run(() =>
            {
                string archivesLines = "";
                bool archivesLinesTruncated = false;
                const int maxArchivesLines = 15;
                for (int i = 0, archiveLineCount = 0; i < droppedItemsList.Count; i++)
                {
                    string di = droppedItemsList[i];
                    if (di.IsEmpty() || !di.ExtIsArchive() ||
                        /*
                        Reject tomfoolery where a directory could be named "whatever.zip" etc.
                        Don't do this in the drag-over handler, because we don't want to potentially take a long
                        wait to hit the disk there. It does mean that for directories that are named like archive
                        files we'll have a "you can do this drop operation" icon and then do nothing once we get
                        here, but that should be a rare case anyway.
                        */
                        Directory.Exists(di))
                    {
                        droppedItemsList.RemoveAt(i);
                        i--;
                        continue;
                    }
                    else if (PathContainsUnsupportedProgramFilesFolder(di, out string progFilesPath))
                    {
                        string message = "This path contains '" + progFilesPath +
                                         "' which is an unsupported path for 32-bit apps.\r\n\r\n" +
                                         "The passed path was:\r\n\r\n" +
                                         di + "\r\n\r\n";
                        Log(message, stackTrace: true);
                        Dialogs.ShowError(message, owner);
                        return false;
                    }

                    if (!archivesLinesTruncated)
                    {
                        if (!archivesLines.IsEmpty()) archivesLines += "\r\n";
                        if (archiveLineCount < maxArchivesLines)
                        {
                            archivesLines += di;
                            archiveLineCount++;
                        }
                        else if (archiveLineCount == maxArchivesLines)
                        {
                            archivesLines += "[...]";
                            archivesLinesTruncated = true;
                        }
                    }
                }

                if (droppedItemsList.Count == 0)
                {
                    return false;
                }

                bool singleArchive = droppedItemsList.Count == 1;
                bool singleArchivePath = Config.FMArchivePaths.Count == 1;

                string destDir = "";

                if (!singleArchivePath)
                {
                    DialogResult result = (DialogResult)Core.View.Invoke(new Func<DialogResult>(() =>
                    {
                        using var f = new MessageBoxCustomForm(
                            messageTop:
                            (singleArchive
                                ? LText.AddFMsToSet.AddFM_Dialog_AskMessage
                                : LText.AddFMsToSet.AddFMs_Dialog_AskMessage) + "\r\n\r\n" + archivesLines + "\r\n\r\n" +
                            (singleArchive
                                ? LText.AddFMsToSet.AddFM_Dialog_ChooseArchiveDir
                                : LText.AddFMsToSet.AddFMs_Dialog_ChooseArchiveDir),
                            messageBottom: "",
                            title: singleArchive
                                ? LText.AddFMsToSet.AddFM_DialogTitle
                                : LText.AddFMsToSet.AddFMs_DialogTitle,
                            icon: MessageBoxIcon.None,
                            okText: LText.AddFMsToSet.AddFM_Add,
                            cancelText: LText.Global.Cancel,
                            okIsDangerous: false,
                            choiceStrings: Config.FMArchivePaths.ToArray(),
                            multiSelectionAllowed: false);

                        // Show with explicit owner because otherwise we get in a "halfway" state where the dialog is
                        // modal, but it can be made to be underneath the main window and then you can never get back
                        // to it again and have to kill the app through Task Manager.
                        DialogResult result = f.ShowDialogDark(owner);

                        if (result != DialogResult.OK) return DialogResult.Cancel;

                        destDir = f.SelectedItems[0];

                        return DialogResult.OK;
                    }));

                    if (result != DialogResult.OK) return false;
                }
                else
                {
                    destDir = Config.FMArchivePaths[0];
                }

                int successfulFilesCopiedCount = 0;

                foreach (string file in droppedItemsList)
                {
                    string destFile = Path.Combine(destDir, Path.GetFileName(file));
                    try
                    {
                        FileSystem.CopyFile(file, destFile, UIOption.AllDialogs, UICancelOption.DoNothing);
                        successfulFilesCopiedCount++;
                    }
                    catch (Exception ex)
                    {
                        Log("Exception copying archive '" + file + "' to '" + destDir, ex);
                        Dialogs.ShowError(LText.AlertMessages.AddFM_UnableToCopy +
                                          "\r\n\r\nSource FM archive file: " + file +
                                          "\r\n\r\nDestination directory: " + destDir);
                    }
                }

                return successfulFilesCopiedCount > 0;
            });

            if (!success) return false;

            await Core.RefreshFMsListFromDisk();

            return true;
        }
    }
}

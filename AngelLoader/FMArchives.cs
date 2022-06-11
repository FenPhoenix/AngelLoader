using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using Microsoft.VisualBasic.FileIO; // the import of shame
using static AL_Common.Common;
using static AngelLoader.Logger;
using static AngelLoader.Misc;
using SearchOption = System.IO.SearchOption;

namespace AngelLoader
{
    [PublicAPI]
    internal static class FMArchives
    {
        private static CancellationTokenSource _deleteCts = new();
        private static void CancelToken() => _deleteCts.CancelIfNotDisposed();

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

        private static (bool Cancel, bool Cont) UninstallFirstDialog()
        {
            (bool cancel, bool cont, _) = Core.Dialogs.AskToContinueWithCancelCustomStrings(
                message: LText.FMDeletion.AskToUninstallFMFirst,
                title: LText.AlertMessages.DeleteFMArchive,
                icon: MBoxIcon.Warning,
                showDontAskAgain: false,
                yes: LText.AlertMessages.Uninstall,
                no: LText.AlertMessages.LeaveInstalled,
                cancel: LText.Global.Cancel
            );

            return (cancel, cont);
        }

        internal static (bool Success, List<string> FinalArchives)
        GetFinalArchives(List<string> archives, bool singleCall)
        {
            var retFail = (false, new List<string>());

            if (archives.Count == 0)
            {
                Core.Dialogs.ShowAlert(LText.FMDeletion.ArchiveNotFound, LText.AlertMessages.DeleteFMArchive);
                return retFail;
            }

            bool singleArchive = archives.Count == 1;
            var finalArchives = new List<string>();
            if (singleCall || !singleArchive)
            {
                (bool accepted, List<string> selectedItems) = Core.View.ShowCustomDialog(
                    messageTop: singleArchive
                        ? LText.FMDeletion.AboutToDelete + "\r\n\r\n" + archives[0]
                        : LText.FMDeletion.DuplicateArchivesFound,
                    messageBottom: "",
                    title: LText.AlertMessages.DeleteFMArchive,
                    icon: MBoxIcon.Warning,
                    okText: singleArchive ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs,
                    cancelText: LText.Global.Cancel,
                    okIsDangerous: true,
                    choiceStrings: singleArchive ? null : archives.ToArray());

                if (!accepted) return retFail;

                finalArchives.AddRange(selectedItems);
            }
            else
            {
                finalArchives.AddRange(archives);
            }

            return (true, finalArchives);
        }

        private static async Task DoDeleteOperation(List<string> finalArchives)
        {
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
                        Core.Dialogs.ShowError(LText.AlertMessages.DeleteFM_UnableToDelete + "\r\n\r\n" + archive);
                    }
                }
            });
        }

        // @DB: When deleting FMs, allow user to also delete them from the database
        internal static async Task DeleteFMsFromDB(List<FanMission> fmsToDelete, bool singleCall = true)
        {
            if (fmsToDelete.Count == 0) return;
            foreach (FanMission fm in fmsToDelete)
            {
                if (!fm.MarkedUnavailable) return;
            }

            bool single = fmsToDelete.Count == 1;

            if (singleCall)
            {
                (bool cont, _) =
                    Core.View.ShowCustomDialog(
                        messageTop:
                        (single
                            ? LText.FMDeletion.DeleteFromDB_AlertMessage1_Single
                            : LText.FMDeletion.DeleteFromDB_AlertMessage1_Multiple) +
                        "\r\n\r\n" +
                        (single
                            ? LText.FMDeletion.DeleteFromDB_AlertMessage2_Single
                            : LText.FMDeletion.DeleteFromDB_AlertMessage2_Multiple),
                        messageBottom: "",
                        title: LText.AlertMessages.Alert,
                        icon: MBoxIcon.Warning,
                        okText: LText.FMDeletion.DeleteFromDB_OKMessage,
                        cancelText: LText.Global.Cancel,
                        okIsDangerous: true);
                if (!cont) return;
            }

            var iniDict = new DictionaryI<List<FanMission>>(FMDataIniList.Count);
            for (int i = 0; i < FMDataIniList.Count; i++)
            {
                FanMission fm = FMDataIniList[i];
                if (!fm.Archive.IsEmpty())
                {
                    if (iniDict.TryGetValue(fm.Archive, out var list))
                    {
                        list.Add(fm);
                    }
                    else
                    {
                        iniDict.Add(fm.Archive, new List<FanMission> { fm });
                    }
                }
            }

            foreach (var fmToDelete in fmsToDelete)
            {
                if (!fmToDelete.Archive.IsEmpty() &&
                    iniDict.TryGetValue(fmToDelete.Archive, out var fmToDeleteIniCopies))
                {
                    foreach (var fm in fmToDeleteIniCopies)
                    {
                        FMDataIniList.Remove(fm);
                        FMCache.ClearCacheDir(fmToDelete, deleteCacheDirItself: true);
                    }
                }
                else
                {
                    FMDataIniList.Remove(fmToDelete);
                    FMCache.ClearCacheDir(fmToDelete, deleteCacheDirItself: true);
                }
            }

            if (singleCall) await DeleteFromDBRefresh();
        }

        private static async Task DeleteFromDBRefresh()
        {
            Ini.WriteFullFMDataIni(makeBackup: true);
            SelectedFM? selFM = Core.FindNearestUnselectedFM(Core.View.GetMainSelectedRowIndex(), Core.View.GetRowCount());
            await Core.RefreshFMsListFromDisk(selFM);
        }

        internal static async Task DeleteSingle(FanMission fm)
        {
            if (fm.MarkedUnavailable)
            {
                const string message = "DeleteSingle(): " + nameof(fm) + " unavailable.";
                Log(message, stackTrace: true);
                Core.Dialogs.ShowError(message);
                return;
            }

            List<string> archives;
            try
            {
                Core.View.SetWaitCursor(true);

                archives = FindAllMatches(fm.Archive);
            }
            finally
            {
                Core.View.SetWaitCursor(false);
            }

            (bool success, List<string> finalArchives) = GetFinalArchives(archives, singleCall: true);
            if (!success) return;

            if (fm.Installed)
            {
                (bool cancel, bool cont) = UninstallFirstDialog();

                if (cancel) return;

                if (cont)
                {
                    if (!await FMInstallAndPlay.Uninstall(fm))
                    {
                        return;
                    }
                }
            }

            try
            {
                Core.View.ShowProgressBox_Single(
                    message1: LText.ProgressBox.DeletingFMArchive,
                    progressType: ProgressType.Indeterminate
                );

                await DoDeleteOperation(finalArchives);
            }
            finally
            {
                bool markedUnavailable = await MarkUnavailableIfNeeded(fm);

                Core.View.HideProgressBox();

                if (markedUnavailable)
                {
                    await Core.View.SortAndSetFilter(keepSelection: true);
                }
            }
        }

        private static async Task<bool> MarkUnavailableIfNeeded(FanMission fm)
        {
            var newArchives = await Task.Run(() => FindAllMatches(fm.Archive));
            if (newArchives.Count == 0 && !fm.Installed)
            {
                fm.MarkedUnavailable = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes <paramref name="fm"/>'s archive from disk, asking the user for confirmation first.
        /// </summary>
        /// <param name="fm"></param>
        /// <param name="singleCall"></param>
        /// <param name="percent"></param>
        /// <returns></returns>
        private static async Task DeleteInternal(FanMission fm, bool singleCall = true, int percent = -1)
        {
            if (fm.MarkedUnavailable)
            {
                string message = "Delete(" + nameof(singleCall) + " == " + singleCall + "): " + nameof(fm) + " unavailable.";
                Log(message, stackTrace: true);
                Core.Dialogs.ShowError(message);
                return;
            }

            // Use wait cursor in blocking thread, rather than putting this on its own thread.
            // The archive find operation _probably_ won't take long enough to warrant a progress box,
            // and if it's quick then the progress box looks like an annoying flicker.
            List<string> archives;
            try
            {
                if (singleCall) Core.View.SetWaitCursor(true);

                archives = FindAllMatches(fm.Archive);
            }
            finally
            {
                if (singleCall) Core.View.SetWaitCursor(false);
            }

            if (archives.Count == 0)
            {
                Core.Dialogs.ShowAlert(LText.FMDeletion.ArchiveNotFound, LText.AlertMessages.DeleteFMArchive);
                return;
            }

            bool singleArchive = archives.Count == 1;

            var finalArchives = new List<string>();

            if (singleCall || !singleArchive)
            {
                (bool accepted, List<string> selectedItems) = Core.View.ShowCustomDialog(
                    messageTop: singleArchive
                        ? LText.FMDeletion.AboutToDelete + "\r\n\r\n" + archives[0]
                        : LText.FMDeletion.DuplicateArchivesFound,
                    messageBottom: "",
                    title: LText.AlertMessages.DeleteFMArchive,
                    icon: MBoxIcon.Warning,
                    okText: singleArchive ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs,
                    cancelText: LText.Global.Cancel,
                    okIsDangerous: true,
                    choiceStrings: singleArchive ? null : archives.ToArray());

                if (!accepted) return;

                finalArchives.AddRange(singleArchive ? archives : selectedItems);
            }
            else
            {
                finalArchives.AddRange(archives);
            }

            if (singleCall && fm.Installed)
            {
                (bool cancel, bool cont, _) = Core.Dialogs.AskToContinueWithCancelCustomStrings(
                    message: LText.FMDeletion.AskToUninstallFMFirst,
                    title: LText.AlertMessages.DeleteFMArchive,
                    icon: MBoxIcon.Warning,
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
                if (singleCall)
                {
                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.DeletingFMArchive,
                        progressType: ProgressType.Indeterminate
                    );
                }
                else
                {
                    Core.View.SetProgressBoxState_Single(
                        percent: percent > -1 ? percent : 0,
                        message2: GetFMId(fm)
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
                            Core.Dialogs.ShowError(LText.AlertMessages.DeleteFM_UnableToDelete + "\r\n\r\n" + archive);
                        }
                    }
                });
            }
            finally
            {
                var newArchives = await Task.Run(() => FindAllMatches(fm.Archive));

                if (singleCall)
                {
                    Core.View.HideProgressBox();
                }

                if (newArchives.Count == 0 && !fm.Installed)
                {
                    fm.MarkedUnavailable = true;
                    if (singleCall)
                    {
                        await Core.View.SortAndSetFilter(keepSelection: true);
                    }
                }
            }
        }

        internal static async Task DeleteMultiple(List<FanMission> fms)
        {
            int origCount = fms.Count;

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
                const string message = "Delete(List<FanMission>): " + nameof(fms) + ".Count is 0 (meaning all are unavailable).";
                Log(message, stackTrace: true);
                Core.Dialogs.ShowError(message);
                return;
            }

            int installedNoArchiveCount = 0;
            int installedCount = 0;
            try
            {
                Core.View.SetWaitCursor(true);

                var archivePaths = GetFMArchivePaths();
                for (int i = 0; i < fms.Count; i++)
                {
                    FanMission fm = fms[i];
                    // PERF_TODO(Delete FindAllMatches): Delete(singular) re-finds the matches for each FM
                    // We could almost just cache this set and pass it, except that if we have to run the
                    // uninstaller, we could end up with fewer FMs in the list from removing archive-less ones
                    // and then the matches list wouldn't match up anymore. We could get really clever and account
                    // for that still, if we felt like the perf increase would be worth it, but for now, meh.
                    var matches = FindAllMatches(fm.Archive, archivePaths);
                    if (matches.Count > 0)
                    {
                        if (fm.Installed)
                        {
                            installedCount++;
                        }
                    }
                    else
                    {
                        if (fm.Installed)
                        {
                            installedCount++;
                            installedNoArchiveCount++;
                        }
                    }
                }
            }
            finally
            {
                Core.View.SetWaitCursor(false);
            }

            (bool accepted, _) = Core.View.ShowCustomDialog(
                messageTop: LText.FMDeletion.AboutToDelete_Multiple_BeforeNumber + origCount +
                            LText.FMDeletion.AboutToDelete_Multiple_AfterNumber,
                messageBottom: "",
                title: LText.AlertMessages.DeleteFMArchives,
                icon: MBoxIcon.Warning,
                okText: LText.FMDeletion.DeleteFMs_CertainMultiple,
                cancelText: LText.Global.Cancel,
                okIsDangerous: true);

            if (!accepted) return;

            // Since multiple archives with the same name should be the rare case (nobody should be doing it),
            // we'll just ask the user per-FM if we find any as we go. Sorry to stop your batch, but yeah.

            // This thing just tells you to uninstall the FMs to delete them, so it's correct functionality
            if (installedNoArchiveCount == fms.Count)
            {
                Core.Dialogs.ShowAlert(LText.FMDeletion.ArchiveNotFound_All, LText.AlertMessages.DeleteFMArchives);
                return;
            }

            if (installedCount > 0)
            {
                (bool cancel, bool cont) = UninstallFirstDialog();

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
                _deleteCts = _deleteCts.Recreate();
                Core.View.ShowProgressBox_Single(
                    message1: LText.ProgressBox.DeletingFMArchives,
                    progressType: ProgressType.Determinate,
                    cancelMessage: LText.Global.Stop,
                    cancelAction: CancelToken
                );

                for (int i = 0; i < fms.Count; i++)
                {
                    FanMission fm = fms[i];

                    var archives = FindAllMatches(fm.Archive);

                    (bool success, List<string> finalArchives) = GetFinalArchives(archives, singleCall: false);
                    if (!success) return;

                    try
                    {
                        Core.View.SetProgressBoxState_Single(
                            percent: Common.GetPercentFromValue_Int(i + 1, fms.Count),
                            message2: GetFMId(fm)
                        );

                        await DoDeleteOperation(finalArchives);
                    }
                    finally
                    {
                        bool markedUnavailable = await MarkUnavailableIfNeeded(fm);
                        if (markedUnavailable)
                        {

                        }
                    }

                    if (_deleteCts.IsCancellationRequested) return;
                }
            }
            finally
            {
                await Core.View.SortAndSetFilter(keepSelection: true);
                Core.View.HideProgressBox();
            }
        }

        internal static async Task<bool> Add(List<string> droppedItemsList)
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
                        Core.Dialogs.ShowError_ViewOwned(message);
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
                    bool result = (bool)Core.View.Invoke(new Func<bool>(() =>
                    {
                        // We need to show with explicit owner because otherwise we get in a "halfway" state where
                        // the dialog is modal, but it can be made to be underneath the main window and then you
                        // can never get back to it again and have to kill the app through Task Manager.
                        (bool accepted, List<string> selectedItems) = Core.View.ShowCustomDialog(
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
                            icon: MBoxIcon.None,
                            okText: LText.AddFMsToSet.AddFM_Add,
                            cancelText: LText.Global.Cancel,
                            okIsDangerous: false,
                            choiceStrings: Config.FMArchivePaths.ToArray(),
                            multiSelectionAllowed: false);

                        if (!accepted) return false;

                        destDir = selectedItems[0];

                        return true;
                    }));

                    if (!result) return false;
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
                        Core.Dialogs.ShowError(
                            LText.AlertMessages.AddFM_UnableToCopyFMArchive + "\r\n\r\n" +
                            LText.AlertMessages.AddFM_FMArchiveFile + file + "\r\n\r\n" +
                            LText.AlertMessages.AddFM_DestinationDir + destDir);
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

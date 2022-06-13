﻿//#define TEST_NO_DELETE_CACHE_DIR

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using Microsoft.VisualBasic.FileIO;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMDelete
    {
        #region Fields

        private static CancellationTokenSource _deleteCts = new();
        private static void CancelToken() => _deleteCts.CancelIfNotDisposed();

        #endregion

        #region Delete from database

        internal static async Task DeleteFMsFromDB(List<FanMission> fmsToDelete)
        {
            if (fmsToDelete.Count == 0) return;
            foreach (FanMission fm in fmsToDelete)
            {
                if (!fm.MarkedUnavailable) return;
            }

            bool single = fmsToDelete.Count == 1;

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

            DeleteFMsFromDB_Internal(fmsToDelete);
            await DeleteFromDBRefresh();
        }

        private static void DeleteFMsFromDB_Internal(List<FanMission> fmsToDelete)
        {
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
#if !TEST_NO_DELETE_CACHE_DIR
                        FMCache.ClearCacheDir(fmToDelete, deleteCacheDirItself: true);
#endif
                    }
                }
                else
                {
                    FMDataIniList.Remove(fmToDelete);
#if !TEST_NO_DELETE_CACHE_DIR
                    FMCache.ClearCacheDir(fmToDelete, deleteCacheDirItself: true);
#endif
                }
            }
        }

        private static async Task DeleteFromDBRefresh()
        {
            Ini.WriteFullFMDataIni(makeBackup: true);
            SelectedFM? selFM = Core.FindNearestUnselectedFM(Core.View.GetMainSelectedRowIndex(), Core.View.GetRowCount());
            await Core.RefreshFMsListFromDisk(selFM);
        }

        #endregion

        #region Delete from disk

        private static (bool Success, List<string> FinalArchives)
        GetFinalArchives(List<string> archives, bool single)
        {
            var retFail = (false, new List<string>());

            if (archives.Count == 0)
            {
                if (single)
                {
                    Core.Dialogs.ShowAlert(LText.FMDeletion.ArchiveNotFound, LText.AlertMessages.DeleteFMArchive);
                }
                return retFail;
            }

            var finalArchives = new List<string>();
            if (archives.Count > 1)
            {
                (bool accepted, List<string> selectedItems) = Core.View.ShowCustomDialog(
                    messageTop: LText.FMDeletion.DuplicateArchivesFound,
                    messageBottom: "",
                    title: LText.AlertMessages.DeleteFMArchive,
                    icon: MBoxIcon.Warning,
                    okText: single ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs,
                    cancelText: LText.Global.Cancel,
                    okIsDangerous: true,
                    choiceStrings: archives.ToArray());

                if (!accepted) return retFail;

                finalArchives.AddRange(selectedItems);
            }
            else
            {
                finalArchives.AddRange(archives);
            }

            return (true, finalArchives);
        }

        /*
        @DB: Deal with if all are installed and have no archive available?
        
        @DB(Refresh method deciding):
        -If uninstall state is the only thing changing, we need to match the Uninstall behavior, ie. only refresh
         the rows & installed-state relevant controls, and do NOT re-filter the list!
        -Also, we should NOT refresh if nothing whatsoever has changed.

        -Pass a bool to Uninstall that tells it not to run its own refreshes and not to close its own progress
         box. BUT we have to make sure to close it ourselves it all cases!
        
        if we've removed any FM from the DB
            Refresh from disk
        else if we've caused any FM to change state such that it would be filtered out
            Refresh list & keep selection
        else if we've only changed install state
            Refresh row & installed controls only
        else if we've canceled before we've modified any FM's state, either in-memory or on-disk
            return without refreshing

        And make it so it doesn't jump to the middle of the list (will probably be handled by uninstall behavior
        matching?)
        */
        internal static async Task DeleteFMsFromDisk(List<FanMission> fms)
        {
            int origCount = fms.Count;

            bool single = origCount == 1;

            var unavailableFMs = new List<FanMission>(fms.Count);

            void MoveUnavailableFMsFromMainListToUnavailableList()
            {
                for (int i = 0; i < fms.Count; i++)
                {
                    FanMission fm = fms[i];
                    if (fm.MarkedUnavailable)
                    {
                        unavailableFMs.Add(fm);
                        fms.RemoveAt(i);
                        i--;
                    }
                }
            }

            MoveUnavailableFMsFromMainListToUnavailableList();

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

                var archivePaths = FMArchives.GetFMArchivePaths();
                for (int i = 0; i < fms.Count; i++)
                {
                    FanMission fm = fms[i];
                    // PERF_TODO(Delete FindAllMatches): The delete loop re-finds the matches for each FM
                    // We could almost just cache this set, except that if we have to run the uninstaller, we
                    // could end up with fewer FMs in the list from removing archive-less ones and then the
                    // matches list wouldn't match up anymore. We could get really clever and account for that
                    // still, if we felt like the perf increase would be worth it, but for now, meh.
                    var matches = FMArchives.FindAllMatches(fm.Archive, archivePaths);
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

            (bool cancel, bool deleteFromDB) = Core.Dialogs.AskToContinueYesNoCustomStrings(
                message: single
                    ? LText.FMDeletion.AboutToDelete + "\r\n\r\n" + fms[0].Archive
                    : LText.FMDeletion.AboutToDelete_Multiple_BeforeNumber + origCount +
                      LText.FMDeletion.AboutToDelete_Multiple_AfterNumber,
                title: single ? LText.AlertMessages.DeleteFMArchive : LText.AlertMessages.DeleteFMArchives,
                icon: MBoxIcon.Warning,
                yes: single ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs_CertainMultiple,
                no: LText.Global.Cancel,
                checkBoxText: single
                    ? LText.FMDeletion.DeleteFMs_AlsoDeleteFromDB_Single
                    : LText.FMDeletion.DeleteFMs_AlsoDeleteFromDB_Multiple,
                yesIsDangerous: true);

            if (cancel) return;

            // Since multiple archives with the same name should be the rare case (nobody should be doing it),
            // we'll just ask the user per-FM if we find any as we go. Sorry to stop your batch, but yeah.

            // This thing just tells you to uninstall the FMs to delete them, so it's correct functionality
            // @DB: Should we just uninstall-as-delete all in this case? Seems more convenient
            if (installedNoArchiveCount == fms.Count)
            {
                Core.Dialogs.ShowAlert(
                    single ? LText.FMDeletion.ArchiveNotFound : LText.FMDeletion.ArchiveNotFound_All,
                    single ? LText.AlertMessages.DeleteFMArchive : LText.AlertMessages.DeleteFMArchives);
                return;
            }

            bool uninstMarkedAnFMUnavailable = false;

            if (installedCount > 0)
            {
                (cancel, bool cont, _) = Core.Dialogs.AskToContinueWithCancelCustomStrings(
                    message: single
                        ? LText.FMDeletion.AskToUninstallFMFirst
                        : LText.FMDeletion.AskToUninstallFMFirst_Multiple,
                    title: single
                        ? LText.AlertMessages.DeleteFMArchive
                        : LText.AlertMessages.DeleteFMArchives,
                    icon: MBoxIcon.Warning,
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
                    (bool success, uninstMarkedAnFMUnavailable) = await FMInstallAndPlay.Uninstall(installedFMs);
                    if (!success)
                    {
                        Core.View.HideProgressBox();
                        await FMInstallAndPlay.DoUninstallEndTasks(uninstMarkedAnFMUnavailable);
                        return;
                    }
                }

                // Even though we culled out the unavailable FMs already, Uninstall() could have marked some more
                // of them unavailable if they didn't have an archive after being uninstalled.
                MoveUnavailableFMsFromMainListToUnavailableList();

                if (fms.Count == 0 && !deleteFromDB)
                {
                    Core.View.HideProgressBox();
                    await Core.View.SortAndSetFilter(keepSelection: true);
                    return;
                }
            }

            bool dbDeleteRefreshRequired = false;
            bool deletedAtLeastOneFromDisk = false;

            try
            {
                _deleteCts = _deleteCts.Recreate();
                if (single)
                {
                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.DeletingFMArchive,
                        progressType: ProgressType.Indeterminate
                    );
                }
                else
                {
                    Core.View.ShowProgressBox_Single(
                        message1: LText.ProgressBox.DeletingFMArchives,
                        progressType: ProgressType.Determinate,
                        cancelMessage: LText.Global.Stop,
                        cancelAction: CancelToken
                    );
                }

                for (int i = 0; i < fms.Count; i++)
                {
                    FanMission fm = fms[i];

                    var archives = FMArchives.FindAllMatches(fm.Archive);

                    (bool success, List<string> finalArchives) = GetFinalArchives(archives, single);
                    if (!success)
                    {
                        if (single)
                        {
                            return;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    try
                    {
                        if (!single)
                        {
                            Core.View.SetProgressBoxState_Single(
                                percent: GetPercentFromValue_Int(i + 1, fms.Count),
                                message2: GetFMId(fm)
                            );
                        }

                        await Task.Run(() =>
                        {
                            foreach (string archive in finalArchives)
                            {
                                try
                                {
                                    deletedAtLeastOneFromDisk = true;
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
                        var newArchives = await Task.Run(() => FMArchives.FindAllMatches(fm.Archive));
                        if (newArchives.Count == 0 && !fm.Installed)
                        {
                            fm.MarkedUnavailable = true;
                            unavailableFMs.Add(fm);
                        }
                    }

                    if (!single && _deleteCts.IsCancellationRequested) return;
                }

                if (deleteFromDB && unavailableFMs.Count > 0)
                {
                    DeleteFMsFromDB_Internal(unavailableFMs);
                    dbDeleteRefreshRequired = true;
                }
            }
            finally
            {
                Core.View.HideProgressBox();
                if (dbDeleteRefreshRequired)
                {
                    await DeleteFromDBRefresh();
                }
                else if (uninstMarkedAnFMUnavailable && !deletedAtLeastOneFromDisk)
                {
                    Core.View.RefreshAllSelectedFMs_UpdateInstallState();
                }
                else if (deletedAtLeastOneFromDisk)
                {
                    var selFM = Core.FindNearestUnselectedFM(Core.View.GetMainSelectedRowIndex(), Core.View.GetRowCount());
                    await Core.View.SortAndSetFilter(
                        keepSelection: false,
                        selectedFM: selFM
                    );
                }
            }
        }

        #endregion
    }
}

//#define TEST_NO_DELETE_CACHE_DIR

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using Microsoft.VisualBasic.FileIO;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

namespace AngelLoader;

/*
@TDM: Things to be done for FM deletion:
-Don't uninstall, or mention anything about uninstalling
-When deleting an FM from disk, we have to delete its folder AND its entry in missions.tdminfo (presumably?!)
-We disable disk deletion entirely for now (TDM also has no way to do it, surprisingly).
 We could allow disk deletion and be heroes, but meh to it for now.
*/

internal static class FMDelete
{
    #region Fields

    private static CancellationTokenSource _deleteCts = new();
    private static void CancelToken() => _deleteCts.CancelIfNotDisposed();

    #endregion

    internal static Task HandleDelete()
    {
        List<FanMission> fms = Core.View.GetSelectedFMs_InOrder_List();

        if (fms.Count == 0) return Task.CompletedTask;

        bool allAreUnavailable = true;
        bool anyAreTDM = false;
        for (int i = 0; i < fms.Count; i++)
        {
            FanMission fm = fms[i];
            if (!fm.MarkedUnavailable)
            {
                allAreUnavailable = false;
            }
            if (fm.Game == Game.TDM)
            {
                anyAreTDM = true;
            }
        }

        if (!allAreUnavailable && anyAreTDM)
        {
            return Task.CompletedTask;
        }

        return allAreUnavailable ? DeleteFMsFromDB(fms) : DeleteFMsFromDisk(fms);
    }

    #region Delete from database

    internal static Task DeleteFMsFromDB(List<FanMission> fmsToDelete)
    {
        if (fmsToDelete.Count == 0) return Task.CompletedTask;
        foreach (FanMission fm in fmsToDelete)
        {
            if (!fm.MarkedUnavailable) return Task.CompletedTask;
        }

        bool single = fmsToDelete.Count == 1;

        (MBoxButton result, _) =
            Core.Dialogs.ShowMultiChoiceDialog(
                message:
                (single
                    ? LText.FMDeletion.DeleteFromDB_AlertMessage1_Single
                    : LText.FMDeletion.DeleteFromDB_AlertMessage1_Multiple) +
                $"{NL}{NL}" +
                (single
                    ? LText.FMDeletion.DeleteFromDB_AlertMessage2_Single
                    : LText.FMDeletion.DeleteFromDB_AlertMessage2_Multiple),
                title: LText.AlertMessages.Alert,
                icon: MBoxIcon.Warning,
                yes: LText.FMDeletion.DeleteFromDB_OKMessage,
                no: LText.Global.Cancel,
                yesIsDangerous: true,
                defaultButton: MBoxButton.No);
        if (result == MBoxButton.No) return Task.CompletedTask;

        DeleteFMsFromDB_Internal(fmsToDelete);
        return DeleteFromDBRefresh();
    }

    private static void DeleteFMsFromDB_Internal(List<FanMission> fmsToDelete)
    {
        var iniDict = new DictionaryI<List<FanMission>>(FMDataIniList.Count);
        for (int i = 0; i < FMDataIniList.Count; i++)
        {
            FanMission fm = FMDataIniList[i];
            if (!fm.Archive.IsEmpty())
            {
                if (iniDict.TryGetValue(fm.Archive, out List<FanMission>? list))
                {
                    list.Add(fm);
                }
                else
                {
                    iniDict.Add(fm.Archive, new List<FanMission> { fm });
                }
            }
        }

        foreach (FanMission fmToDelete in fmsToDelete)
        {
            if (!fmToDelete.Archive.IsEmpty() &&
                iniDict.TryGetValue(fmToDelete.Archive, out List<FanMission>? fmToDeleteIniCopies))
            {
                foreach (FanMission fm in fmToDeleteIniCopies)
                {
                    if (fm.Game == Game.TDM)
                    {
                        FMDataIniListTDM.Remove(fm);
                    }
                    else
                    {
                        FMDataIniList.Remove(fm);
                    }
#if !TEST_NO_DELETE_CACHE_DIR
                    FMCache.ClearCacheDir(fmToDelete, deleteCacheDirItself: true);
#endif
                }
            }
            else
            {
                if (fmToDelete.Game == Game.TDM)
                {
                    FMDataIniListTDM.Remove(fmToDelete);
                }
                else
                {
                    FMDataIniList.Remove(fmToDelete);
                }
#if !TEST_NO_DELETE_CACHE_DIR
                FMCache.ClearCacheDir(fmToDelete, deleteCacheDirItself: true);
#endif
            }
        }
    }

    private static Task DeleteFromDBRefresh()
    {
        Ini.WriteFullFMDataIni(makeBackup: true);
        SelectedFM? selFM = Core.FindNearestUnselectedFM(Core.View.GetMainSelectedRowIndex(), Core.View.GetRowCount());
        return Core.RefreshFMsListFromDisk(selFM);
    }

    #endregion

    #region Delete from disk

    private static (bool Success, List<string> FinalArchives)
    GetFinalArchives(List<string> archives, bool single)
    {
        var retFail = (false, new List<string>());

        var finalArchives = new List<string>(archives.Count);
        if (archives.Count > 1)
        {
            (bool accepted, List<string> selectedItems) = Core.Dialogs.ShowListDialog(
                messageTop: LText.FMDeletion.DuplicateArchivesFound,
                messageBottom: "",
                title: LText.AlertMessages.DeleteFMArchive,
                icon: MBoxIcon.Warning,
                okText: single ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs,
                cancelText: LText.Global.Cancel,
                okIsDangerous: true,
                choiceStrings: archives.ToArray(),
                multiSelectionAllowed: true);

            if (!accepted) return retFail;

            finalArchives.AddRange(selectedItems);
        }
        else
        {
            finalArchives.AddRange(archives);
        }

        return (true, finalArchives);
    }

    // * NIGHTMARE REALM *
    internal static async Task DeleteFMsFromDisk(List<FanMission> fms)
    {
        using var dsw = new DisableScreenshotWatchers();

        int origCount = fms.Count;

        bool single = origCount == 1;

        var unavailableFMs = new List<FanMission>(fms.Count);

        MoveUnavailableFMsFromMainListToUnavailableList(fms, unavailableFMs);

        if (fms.Count == 0)
        {
            string message = nameof(fms) + ".Count is 0 (meaning all are unavailable).";
            Log(message, stackTrace: true);
            Core.Dialogs.ShowError(message);
            return;
        }

        int installedCount = 0;
        for (int i = 0; i < fms.Count; i++)
        {
            if (fms[i].Installed) installedCount++;
        }

        (MBoxButton result, bool deleteFromDB) = Core.Dialogs.ShowMultiChoiceDialog(
            message: single
                ? LText.FMDeletion.AboutToDelete + $"{NL}{NL}" + fms[0].GetId()
                : LText.FMDeletion.AboutToDelete_Multiple_BeforeNumber + origCount.ToStrCur() +
                  LText.FMDeletion.AboutToDelete_Multiple_AfterNumber,
            title: single ? LText.AlertMessages.DeleteFMArchive : LText.AlertMessages.DeleteFMArchives,
            icon: MBoxIcon.Warning,
            yes: single ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs_CertainMultiple,
            no: LText.Global.Cancel,
            yesIsDangerous: true,
            checkBoxText: single
                ? LText.FMDeletion.DeleteFMs_AlsoDeleteFromDB_Single
                : LText.FMDeletion.DeleteFMs_AlsoDeleteFromDB_Multiple,
            defaultButton: MBoxButton.No);

        if (result == MBoxButton.No) return;

        // Since multiple archives with the same name should be the rare case (nobody should be doing it),
        // we'll just ask the user per-FM if we find any as we go. Sorry to stop your batch, but yeah.

        bool refreshRequired = false;
        bool leaveAllInstalled = false;
        bool deletedAtLeastOneFromDisk = false;

        if (installedCount > 0)
        {
            (result, _) = Core.Dialogs.ShowMultiChoiceDialog(
                message: single
                    ? LText.FMDeletion.AskToUninstallFMFirst
                    : LText.FMDeletion.AskToUninstallFMFirst_Multiple,
                title: single
                    ? LText.AlertMessages.DeleteFMArchive
                    : LText.AlertMessages.DeleteFMArchives,
                icon: MBoxIcon.Warning,
                yes: LText.AlertMessages.Uninstall,
                no: deleteFromDB ? null : LText.AlertMessages.LeaveInstalled,
                cancel: LText.Global.Cancel,
                defaultButton: MBoxButton.Cancel
            );

            if (result == MBoxButton.Cancel) return;
            if (result == MBoxButton.Yes)
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
                try
                {
                    (bool success, bool uninstMarkedAnFMUnavailable) =
                        await FMInstallAndPlay.Uninstall(installedFMs, doEndTasks: false);
                    if (!success)
                    {
                        /*
                        Uninstall() does hide it in the finally block BUT only if it's running end tasks!
                        We've told it not to here, so that's why we have to hide it ourselves.
                        This is to do with keeping one progress box up for the whole operation without closing
                        and reopening flicker I guess.
                        */
                        Core.View.HideProgressBox();
                        await FMInstallAndPlay.DoUninstallEndTasks(uninstMarkedAnFMUnavailable);
                        return;
                    }
                }
                catch
                {
                    // Uninstall() shouldn't be throwing now, but just in case...
                    Core.View.HideProgressBox();
                    // We don't know if any FMs were marked unavailable so let's be conservative and say that
                    // some were, and do the more forceful refresh or whatever else. Probably better than under-
                    // refreshing? Probably?
                    await FMInstallAndPlay.DoUninstallEndTasks(true);
                    return;
                }
                refreshRequired = true;
            }
            else
            {
                leaveAllInstalled = true;
            }

            // Even though we culled out the unavailable FMs already, Uninstall() could have marked some more
            // of them unavailable if they didn't have an archive after being uninstalled.
            MoveUnavailableFMsFromMainListToUnavailableList(fms, unavailableFMs);

            if (fms.Count == 0 && !deleteFromDB)
            {
                Core.View.HideProgressBox();
                await Core.View.SortAndSetFilter(keepSelection: true);
                return;
            }
        }

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

            List<string> archivePaths = FMArchives.GetFMArchivePaths();

            for (int i = 0; i < fms.Count; i++)
            {
                FanMission fm = fms[i];

                List<string> archives = FMArchives.FindAllMatches(fm.Archive, archivePaths);

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
                            message2: fm.GetId(),
                            percent: GetPercentFromValue_Int(i + 1, fms.Count));
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
                                Log(ErrorText.Ex + "deleting archive '" + archive + "'", ex);
                                Core.Dialogs.ShowError(LText.AlertMessages.DeleteFM_UnableToDelete + $"{NL}{NL}" + archive);
                            }
                        }
                    });
                }
                finally
                {
                    // Do this even if we had no archives, because we're still going to set it unavailable
                    // so we need to refresh to remove it from the filtered list
                    deletedAtLeastOneFromDisk = true;
                    List<string> newArchives = await Task.Run(() => FMArchives.FindAllMatches(fm.Archive, archivePaths));
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
                refreshRequired = true;
            }
        }
        finally
        {
            Core.View.HideProgressBox();
            _deleteCts.Dispose();
            if (refreshRequired || (deletedAtLeastOneFromDisk && !leaveAllInstalled))
            {
                // Just always do this because trying to determine the right "lighter" thing to do is
                // COMPLEXITY HELL. This always does what we want, idgaf if it involves a reload from disk.
                await DeleteFromDBRefresh();
            }
        }

        return;

        static void MoveUnavailableFMsFromMainListToUnavailableList(List<FanMission> fms, List<FanMission> unavailableFMs)
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
    }

    #endregion
}

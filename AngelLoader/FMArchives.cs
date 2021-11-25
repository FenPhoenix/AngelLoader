﻿using System;
using System.Collections.Generic;
using System.IO;
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
            var paths = new List<string>();
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
        /// <returns></returns>
        internal static string FindFirstMatch(string fmArchive, List<string>? archivePaths = null)
        {
            if (fmArchive.IsEmpty()) return "";

            var paths = archivePaths?.Count > 0 ? archivePaths : GetFMArchivePaths();
            foreach (string path in paths)
            {
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
        /// <returns></returns>
        internal static List<string> FindAllMatches(string fmArchive)
        {
            if (fmArchive.IsEmpty()) return new List<string>();

            var list = new List<string>();

            foreach (string path in GetFMArchivePaths())
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
        /// <returns></returns>
        internal static async Task Delete(FanMission fm)
        {
            if (fm.MarkedUnavailable) return;

            var archives = FindAllMatches(fm.Archive);
            if (archives.Count == 0)
            {
                Dialogs.ShowAlert(LText.FMDeletion.ArchiveNotFound, LText.AlertMessages.DeleteFMArchive, MessageBoxIcon.Error);
                return;
            }

            bool singleArchive = archives.Count == 1;

            var finalArchives = new List<string>();

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

            if (fm.Installed)
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
                    bool canceled = !await FMInstallAndPlay.UninstallFM(fm);
                    if (canceled) return;
                }
            }

            try
            {
                Core.View.ShowProgressBox(ProgressTask.DeleteFMArchive);
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
                            Core.View.InvokeSync(new Action(() => Dialogs.ShowError(LText.AlertMessages.DeleteFM_UnableToDelete + "\r\n\r\n" + archive)));
                        }
                    }
                });
            }
            finally
            {
                var newArchives = await Task.Run(() => FindAllMatches(fm.Archive));

                Core.View.HideProgressBox();

                if (newArchives.Count == 0 && !fm.Installed)
                {
                    fm.MarkedUnavailable = true;
                    await Core.View.SortAndSetFilter(keepSelection: true);
                }
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
                for (int i = 0; i < droppedItemsList.Count; i++)
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

                    if (!archivesLines.IsEmpty()) archivesLines += "\r\n";
                    archivesLines += di;
                }

                if (droppedItemsList.Count == 0)
                {
                    return false;
                }

                bool singleArchive = droppedItemsList.Count == 1;
                bool singleArchivePath = Config.FMArchivePaths.Count == 1;

                string destDir = "";

                DialogResult result = (DialogResult)Core.View.InvokeSync(new Func<DialogResult>(() =>
                {
                    using var f = new MessageBoxCustomForm(
                        messageTop:
                        (singleArchive
                            ? LText.AddFMsToSet.AddFM_Dialog_AskMessage
                            : LText.AddFMsToSet.AddFMs_Dialog_AskMessage) + "\r\n\r\n" + archivesLines + "\r\n\r\n\r\n" +
                        (singleArchivePath
                            ? ""
                            : singleArchive
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
                        choiceStrings: singleArchivePath ? null : Config.FMArchivePaths.ToArray(),
                        multiSelectionAllowed: false);

                    DialogResult result = f.ShowDialogDark();

                    if (result != DialogResult.OK) return DialogResult.Cancel;

                    destDir = singleArchivePath ? Config.FMArchivePaths[0] : f.SelectedItems[0];

                    return DialogResult.OK;
                }));

                if (result != DialogResult.OK) return false;

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
                        Core.View.InvokeSync(new Action(() =>
                            Dialogs.ShowError(LText.AlertMessages.AddFM_UnableToCopy +
                                              "\r\n\r\nSource FM archive file: " + file +
                                              "\r\n\r\nDestination directory: " + destDir)));
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

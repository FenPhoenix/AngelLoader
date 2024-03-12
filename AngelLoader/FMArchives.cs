using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using Microsoft.VisualBasic.FileIO;
using static AL_Common.Logger;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;
using SearchOption = System.IO.SearchOption;

namespace AngelLoader;

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
                    foreach (string dir in dirs)
                    {
                        if (!dir.GetDirNameFast().EqualsI(".fix") &&
                            // @DIRSEP: '/' conversion due to string.ContainsI()
                            !dir.ToForwardSlashes_Net().ContainsI("/.fix/"))
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
    [PublicAPI]
    internal static string FindFirstMatch(string fmArchive, List<string> archivePaths)
    {
        if (fmArchive.IsEmpty()) return "";

        foreach (string path in archivePaths)
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
    /// <param name="archivePaths"></param>
    /// <returns></returns>
    [PublicAPI]
    internal static List<string> FindAllMatches(string fmArchive, List<string> archivePaths)
    {
        if (fmArchive.IsEmpty()) return new List<string>();

        var list = new List<string>(archivePaths.Count);

        foreach (string path in archivePaths)
        {
            if (TryCombineFilePathAndCheckExistence(path, fmArchive, out string f))
            {
                list.Add(f);
            }
        }

        return list;
    }

    internal static async Task Add(List<string> droppedItemsList)
    {
        if (!Core.View.UIEnabled) return;

        if (Config.FMArchivePaths.Count == 0) return;

        try
        {
            Core.View.UIEnabled = false;

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

                string destDir;

                if (!singleArchivePath)
                {
                    (bool accepted, List<string> selectedItems) = Core.Dialogs.ShowListDialog(
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
                        Log(ErrorText.ExCopy + "archive '" + file + "' to '" + destDir + "'", ex);
                        Core.Dialogs.ShowError(
                            LText.AlertMessages.AddFM_UnableToCopyFMArchive + "\r\n\r\n" +
                            LText.AlertMessages.AddFM_FMArchiveFile + file + "\r\n\r\n" +
                            LText.AlertMessages.AddFM_DestinationDir + destDir);
                    }
                }

                return successfulFilesCopiedCount > 0;
            });

            if (!success) return;

            await Core.RefreshFMsListFromDisk();
        }
        finally
        {
            Core.View.UIEnabled = true;
        }
    }
}

using System;
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
                        Log(nameof(GetFMArchivePaths) + " : subfolders=true, Exception in GetDirectories", ex);
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

        // TODO: @ShowUnavailableFMs: We should bite the bullet and switch to refreshing from disk here
        // Instead of using the hack bool. This is so we know we'll be consistent if we delete and have the
        // "show unavailable FMs" option turned on.
        // We know there's a stumbling block to doing this, something to do with losing the FM selection position
        // due to the way we're architected. We should just power through it and make it possible whatever it
        // takes. It would make us more consistent and remove the iffy hack bool.

        /// <summary>
        /// Deletes <paramref name="fm"/>'s archive from disk, asking the user for confirmation first.
        /// </summary>
        /// <param name="fm"></param>
        /// <returns></returns>
        internal static async Task Delete(FanMission fm)
        {
            var archives = FindAllMatches(fm.Archive);
            if (archives.Count == 0)
            {
                ControlUtils.ShowAlert(LText.FMDeletion.ArchiveNotFound, LText.AlertMessages.DeleteFMArchive);
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
                if (f.ShowDialog() != DialogResult.OK) return;

                finalArchives.AddRange(singleArchive ? archives : f.SelectedItems);
            }

            // TODO: This is a first pass at the "uninstall first?" for FM deletion - it needs quite a bit more work
            if (fm.Installed)
            {
                using var f = new MessageBoxCustomForm(
                    messageTop: "This FM is installed. Uninstall it first?",
                    messageBottom: "",
                    title: LText.AlertMessages.DeleteFMArchive,
                    icon: MessageBoxIcon.Warning,
                    okText: LText.AlertMessages.Uninstall,
                    cancelText: "Leave installed",
                    okIsDangerous: true);

                if (f.ShowDialog() == DialogResult.OK)
                {
                    await FMInstallAndPlay.InstallOrUninstall(fm);
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
                            Core.View.InvokeSync(new Action(() =>
                            {
                                ControlUtils.ShowAlert(
                                     LText.AlertMessages.DeleteFM_UnableToDelete + "\r\n\r\n" +
                                     archive,
                                     LText.AlertMessages.Error);
                            }));
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
    }
}

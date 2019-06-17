using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.WinAPI;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal static class FindFMs
    {
        internal static void Find(List<FanMission> fmDataIniList, bool startup = false)
        {
            // Make sure we don't lose anything when we re-find!
            if (!startup) Core.WriteFullFMDataIni();

            // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
            // list when it's in an indeterminate state (which can cause a selection change (bad) and/or a visible
            // change of the list (not really bad but unprofessional looking))
            Core.View.SetRowCount(0);

            // Init or reinit - must be deep-copied or changes propagate back because reference types
            PresetTags.DeepCopyTo(GlobalTags);

            #region Back up lists and read FM data file

            // Copy FMs to backup lists before clearing, in case we can't read the ini file. We don't want to end
            // up with a blank or incomplete list and then glibly save it out later.
            var backupList = new List<FanMission>();
            foreach (var fm in fmDataIniList) backupList.Add(fm);

            var viewBackupList = new List<FanMission>();
            foreach (var fm in Core.FMsViewList) viewBackupList.Add(fm);

            fmDataIniList.Clear();
            Core.FMsViewList.Clear();

            var fmDataIniExists = File.Exists(Paths.FMDataIni);

            if (fmDataIniExists)
            {
                try
                {
                    ReadFMDataIni(Paths.FMDataIni, fmDataIniList);
                }
                catch (Exception ex)
                {
                    Log("Exception reading FM data ini", ex);
                    if (startup)
                    {
                        MessageBox.Show("Exception reading FM data ini. Exiting. Please check " + Paths.LogFile,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(1);
                    }
                    else
                    {
                        fmDataIniList.ClearAndAdd(backupList);
                        Core.FMsViewList.ClearAndAdd(viewBackupList);
                        return;
                    }
                }
            }

            #endregion

            #region Get installed dirs from disk

            // Could check inside the folder for a .mis file to confirm it's really an FM folder, but that's
            // horrendously expensive. Talking like eight seconds vs. < 4ms for the 1098 set. Weird.
            var t1InstalledFMDirs = new List<string>();
            var t2InstalledFMDirs = new List<string>();
            var t3InstalledFMDirs = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                var instFMDirs = i == 0 ? t1InstalledFMDirs : i == 1 ? t2InstalledFMDirs : t3InstalledFMDirs;
                var instPath = i == 0 ? Config.T1FMInstallPath : i == 1 ? Config.T2FMInstallPath : Config.T3FMInstallPath;

                if (Directory.Exists(instPath))
                {
                    try
                    {
                        foreach (var d in Directory.GetDirectories(instPath, "*", SearchOption.TopDirectoryOnly))
                        {
                            var dirName = d.GetTopmostDirName();
                            if (!dirName.EqualsI(".fmsel.cache")) instFMDirs.Add(dirName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Exception getting directories in " + instPath, ex);
                    }
                }
            }

            #endregion

            #region Get archives from disk

            var fmArchives = new List<string>();

            foreach (var path in GetFMArchivePaths())
            {
                try
                {
                    var files = FastIO.GetFilesTopOnly(path, "*");
                    foreach (var f in files)
                    {
                        if (!fmArchives.ContainsI(f.GetFileNameFast()) && f.ExtIsArchive() && !f.ContainsI(Paths.FMSelBak))
                        {
                            fmArchives.Add(f.GetFileNameFast());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception getting files in " + path, ex);
                }
            }

            #endregion

            #region Build FanMission objects from installed dirs

            var t1List = new List<FanMission>();
            var t2List = new List<FanMission>();
            var t3List = new List<FanMission>();

            for (int i = 0; i < 3; i++)
            {
                var instFMDirs = i == 0 ? t1InstalledFMDirs : i == 1 ? t2InstalledFMDirs : t3InstalledFMDirs;
                var list = i == 0 ? t1List : i == 1 ? t2List : t3List;
                var game = i == 0 ? Game.Thief1 : i == 1 ? Game.Thief2 : Game.Thief3;

                foreach (var item in instFMDirs)
                {
                    list.Add(new FanMission { InstalledDir = item, Game = game, Installed = true });
                }
            }

            #endregion

            MergeNewArchiveFMs(fmArchives, fmDataIniList);

            int instInitCount = fmDataIniList.Count;
            if (t1List.Count > 0) MergeNewInstalledFMs(t1List, fmDataIniList, instInitCount);
            if (t2List.Count > 0) MergeNewInstalledFMs(t2List, fmDataIniList, instInitCount);
            if (t3List.Count > 0) MergeNewInstalledFMs(t3List, fmDataIniList, instInitCount);

            SetArchiveNames(fmArchives, fmDataIniList);

            SetInstalledNames(fmDataIniList);

            BuildViewList(fmArchives, fmDataIniList, t1InstalledFMDirs, t2InstalledFMDirs, t3InstalledFMDirs, startup);
        }

        private static void SetArchiveNames(List<string> fmArchives, List<FanMission> fmDataIniList)
        {
            // Attempt to set archive names for newly found installed FMs (best effort search)
            for (var i = 0; i < fmDataIniList.Count; i++)
            {
                var fm = fmDataIniList[i];

                if (fm.Archive.IsEmpty())
                {
                    if (fm.InstalledDir.IsEmpty())
                    {
                        fmDataIniList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    var archiveName = GetArchiveNameFromInstalledDir(fmDataIniList, fm, fmArchives);
                    if (archiveName.IsEmpty()) continue;

                    // Exponential (slow) stuff, but we only do it once to correct the list and then never again
                    // NOTE: I guess this removes duplicates, which is why it has to do the search?
                    var existingFM = fmDataIniList.FirstOrDefault(x => x.Archive.EqualsI(archiveName));
                    if (existingFM != null)
                    {
                        existingFM.InstalledDir = fm.InstalledDir;
                        existingFM.Installed = true;
                        existingFM.Game = fm.Game;
                        fmDataIniList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        fm.Archive = archiveName;
                    }
                }
            }
        }

        private static void SetInstalledNames(List<FanMission> fmDataIniList)
        {
            // Fill in empty installed dir names, making sure to check for and handle truncated name collisions
            foreach (var fm in fmDataIniList)
            {
                if (fm.InstalledDir.IsEmpty())
                {
                    var truncate = fm.Game != Game.Thief3;
                    var instDir = fm.Archive.ToInstDirNameFMSel(truncate);
                    int i = 0;

                    // Again, an exponential search, but again, we only do it once to correct the list and then
                    // never again
                    while (fmDataIniList.Any(x => x.InstalledDir.EqualsI(instDir)))
                    {
                        // Yeah, this'll never happen, but hey
                        if (i > 999) break;

                        // Conform to FMSel's numbering format
                        var numStr = (i + 2).ToString();
                        instDir = instDir.Substring(0, (instDir.Length - 2) - numStr.Length) + "(" + numStr + ")";

                        Debug.Assert(truncate && instDir.Length == 30,
                            nameof(instDir) + "should have been truncated but its length is not 30");

                        i++;
                    }

                    // If it overflowed, oh well. You get what you deserve in that case.
                    fm.InstalledDir = instDir;
                }
            }
        }

        private static void MergeNewArchiveFMs(List<string> fmArchives, List<FanMission> fmDataIniList)
        {
            // Attempt at a perf optimization: we don't need to search anything we've added onto the end.
            int initCount = fmDataIniList.Count;

            var checkedList = new List<FanMission>();

            for (int ai = 0; ai < fmArchives.Count; ai++)
            {
                var archive = fmArchives[ai];

                // perf perf blah
                string aRemoveExt = null;
                string aFMSel = null;
                string aFMSelTrunc = null;
                string aNDL = null;

                bool existingFound = false;
                for (int i = 0; i < initCount; i++)
                {
                    var fm = fmDataIniList[i];

                    // This weird syntax is to memoize stuff for perf, but it seems if there's more than three of
                    // these comparisons(?!) then it either screws up or ReSharper just thinks it will screw up,
                    // because it then starts complaining that the iterator is never changed in the loop(?!?!)
                    // But only if more than three comparisons are done(?!?!?!?!?!) AND ONLY THEN IF IT'S NOT IN
                    // ITS OWN METHOD(?!?!?!?!?!?!?!?!?!) Argh!
                    if (!fm.Checked &&
                        fm.Archive.IsEmpty() &&
                        (fm.InstalledDir.EqualsI(aRemoveExt ?? (aRemoveExt = archive.RemoveExtension())) ||
                         fm.InstalledDir.EqualsI(aFMSel ?? (aFMSel = archive.ToInstDirNameFMSel(false))) ||
                         fm.InstalledDir.EqualsI(aFMSelTrunc ?? (aFMSelTrunc = archive.ToInstDirNameFMSel(true))) ||
                         fm.InstalledDir.EqualsI(aNDL ?? (aNDL = archive.ToInstDirNameNDL()))))
                    {
                        fm.Archive = archive;

                        fm.Checked = true;
                        checkedList.Add(fm);
                        existingFound = true;
                        break;
                    }
                    else if (!fm.Checked &&
                             !fm.Archive.IsEmpty() && fm.Archive.EqualsI(archive))
                    {
                        fm.Checked = true;
                        checkedList.Add(fm);
                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    fmDataIniList.Add(new FanMission { Archive = archive });
                }
            }

            // Reset temp bool
            for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;
        }

        // This takes an explicit initCount because we call this once per game, and we don't want to grow our
        // initCount with every call (we can keep it the initial size and still have this work, so it's faster)
        private static void MergeNewInstalledFMs(List<FanMission> installedList, List<FanMission> fmDataIniList, int initCount)
        {
            var checkedList = new List<FanMission>();

            for (int gFMi = 0; gFMi < installedList.Count; gFMi++)
            {
                var gFM = installedList[gFMi];

                // bool check seems to be faster than a null check
                bool isEmpty = gFM.InstalledDir.IsEmpty();

                bool existingFound = false;
                for (int i = 0; i < initCount; i++)
                {
                    var fm = fmDataIniList[i];

                    if (!isEmpty &&
                        // Early-out bool - much faster than checking EqualsI()
                        !fm.Checked &&
                        !fm.InstalledDir.IsEmpty() &&
                        fm.InstalledDir.EqualsI(gFM.InstalledDir))
                    {
                        fm.Game = gFM.Game;
                        fm.Installed = true;
                        fm.Checked = true;
                        // So we only loop through checked FMs when we reset them
                        checkedList.Add(fm);
                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    fmDataIniList.Add(new FanMission
                    {
                        InstalledDir = gFM.InstalledDir,
                        Game = gFM.Game,
                        Installed = true
                    });
                }
            }

            // Reset temp bool
            for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;
        }

        private static string GetArchiveNameFromInstalledDir(List<FanMission> fmDataIniList, FanMission fm, List<string> archives)
        {
            // The game type is supposed to be inferred from the installed location, so it should always be known
            Debug.Assert(fm.Game != null, "fm.Game == null: Game type is blank for an installed FM?!");

            var gamePath =
                fm.Game == Game.Thief1 ? Config.T1FMInstallPath :
                fm.Game == Game.Thief2 ? Config.T2FMInstallPath :
                // TODO: If SU's FMSel mangles install names in a different way, I need to account for it here
                fm.Game == Game.Thief3 ? Config.T3FMInstallPath :
                null;

            if (gamePath.IsEmpty()) return null;

            var fmDir = Path.Combine(gamePath, fm.InstalledDir);
            var fmselInf = Path.Combine(fmDir, Paths.FMSelInf);

            string FixUp(bool createFmselInf)
            {
                // Make a best-effort attempt to find what this FM's archive name should be
                // PERF: 5ms to run it once on the ~1500 set with no hits, but the time taken is all in the
                // ToInstDirName* calls. So, it doesn't really scale if the user has a bunch of installed FMs
                // with no matching archives, but... whatcha gonna do? We need this automatic linkup thing.
                // TODO: If you come up with any brilliant ideas for the archive-linkup search...
                bool truncate = fm.Game != Game.Thief3;
                var tryArchive =
                    archives.FirstOrDefault(x => x.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir)) ??
                    archives.FirstOrDefault(x => x.ToInstDirNameNDL().EqualsI(fm.InstalledDir)) ??
                    archives.FirstOrDefault(x => x.EqualsI(fm.InstalledDir)) ??
                    fmDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir))?.Archive ??
                    fmDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameNDL().EqualsI(fm.InstalledDir))?.Archive ??
                    fmDataIniList.FirstOrDefault(x => x.InstalledDir.EqualsI(fm.InstalledDir))?.Archive;

                if (tryArchive.IsEmpty()) return null;

                if (!createFmselInf) return tryArchive;

                try
                {
                    using (var sw = new StreamWriter(fmselInf, append: false))
                    {
                        sw.WriteLine("Name=" + fm.InstalledDir);
                        sw.WriteLine("Archive=" + tryArchive);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception in creating or overwriting" + fmselInf, ex);
                }

                return tryArchive;
            }

            if (!File.Exists(fmselInf)) return FixUp(true);

            string[] lines;
            try
            {
                lines = File.ReadAllLines(fmselInf);
            }
            catch (Exception ex)
            {
                Log("Exception reading " + fmselInf, ex);
                return null;
            }

            if (lines.Length < 2 || !lines[0].StartsWithI("Name=") || !lines[1].StartsWithI("Archive="))
            {
                return FixUp(true);
            }

            var installedName = lines[0].Substring(lines[0].IndexOf('=') + 1).Trim();
            if (!installedName.EqualsI(fm.InstalledDir))
            {
                return FixUp(true);
            }

            var archiveName = lines[1].Substring(lines[1].IndexOf('=') + 1).Trim();
            if (archiveName.IsEmpty())
            {
                return FixUp(true);
            }

            return archiveName;
        }

        private static void BuildViewList(List<string> fmArchives, List<FanMission> fmDataIniList,
            List<string> t1InstalledFMDirs, List<string> t2InstalledFMDirs, List<string> t3InstalledFMDirs,
            bool startup)
        {
            Core.ViewListGamesNull.Clear();
            for (var i = 0; i < fmDataIniList.Count; i++)
            {
                var item = fmDataIniList[i];

                #region Checks

                // Attempt to avoid re-searching lists
                bool? notInT1Dirs = null;
                bool? notInT2Dirs = null;
                bool? notInT3Dirs = null;

                if (item.Installed &&
                    ((item.Game == Game.Thief1 && (bool)(notInT1Dirs = !t1InstalledFMDirs.ContainsI(item.InstalledDir))) ||
                     (item.Game == Game.Thief2 && (bool)(notInT2Dirs = !t2InstalledFMDirs.ContainsI(item.InstalledDir))) ||
                     (item.Game == Game.Thief3 && (bool)(notInT3Dirs = !t3InstalledFMDirs.ContainsI(item.InstalledDir)))))
                {
                    item.Installed = false;
                }

                // NOTE: Old data
                // FMDataIniList: Thief1(personal)+Thief2(personal)+All(1098 set)
                // Archive dirs: Thief1(personal)+Thief2(personal)
                // Total time taken running this for all FMs in FMDataIniList: 3~7ms
                // Good enough?
                if ((!item.Installed ||
                     (item.Game == Game.Thief1 && (notInT1Dirs ?? !t1InstalledFMDirs.ContainsI(item.InstalledDir))) ||
                     (item.Game == Game.Thief2 && (notInT2Dirs ?? !t2InstalledFMDirs.ContainsI(item.InstalledDir))) ||
                     (item.Game == Game.Thief3 && (notInT3Dirs ?? !t3InstalledFMDirs.ContainsI(item.InstalledDir)))) &&
                    // Shrink the list as we get matches so we can reduce our search time as we go
                    !fmArchives.ContainsIRemoveFirstHit(item.Archive))
                {
                    continue;
                }

                #endregion

                // Perf so we don't have to iterate the list again later
                if (item.Game == null) Core.ViewListGamesNull.Add(i);

                item.Title =
                    !item.Title.IsEmpty() ? item.Title :
                    !item.Archive.IsEmpty() ? item.Archive.RemoveExtension() :
                    item.InstalledDir;
                // SizeString gets set on UI localize so don't set it here on startup, or it's duplicate work
                if (!startup) item.SizeString = ((long?)item.SizeBytes).ConvertSize();
                item.CommentSingleLine = item.Comment.FromEscapes().ToSingleLineComment(100);
                AddTagsToFMAndGlobalList(item.TagsString, item.Tags);

                Core.FMsViewList.Add(item);
            }
        }
    }
}

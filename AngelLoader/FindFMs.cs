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
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal static class FindFMs
    {
        // MT: On startup only, this is run in parallel with MainForm.ctor and .Init()
        // So don't touch anything the other touches: anything affecting the view.
        internal static void Find(FMInstallPaths fmInstPaths, List<FanMission> fmDataIniList, bool startup = false)
        {
            if (!startup)
            {
                // Make sure we don't lose anything when we re-find!
                Core.WriteFullFMDataIni();

                // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from
                // the list when it's in an indeterminate state (which can cause a selection change (bad) and/or
                // a visible change of the list (not really bad but unprofessional looking)).
                // MT: Don't do this on startup because we're running in parallel with the form new/init in that case.
                Core.View.SetRowCount(0);
            }

            // Init or reinit - must be deep-copied or changes propagate back because reference types
            // MT: This is thread-safe, the view ctor and Init() doesn't touch it.
            Common.Common.PresetTags.DeepCopyTo(Common.Common.GlobalTags);

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
                        // Language will be loaded by this point
                        MessageBox.Show(LText.AlertMessages.FindFMs_ExceptionReadingFMDataIni,
                            LText.AlertMessages.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                var instPath = i == 0 ? fmInstPaths.T1 : i == 1 ? fmInstPaths.T2 : fmInstPaths.T3;

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

            MergeNewArchiveFMs(fmArchives, fmDataIniList, fmInstPaths);

            int instInitCount = fmDataIniList.Count;
            if (t1List.Count > 0) MergeNewInstalledFMs(t1List, fmDataIniList, instInitCount);
            if (t2List.Count > 0) MergeNewInstalledFMs(t2List, fmDataIniList, instInitCount);
            if (t3List.Count > 0) MergeNewInstalledFMs(t3List, fmDataIniList, instInitCount);

            SetArchiveNames(fmInstPaths, fmArchives, fmDataIniList);

            SetInstalledNames(fmDataIniList);

            BuildViewList(fmArchives, fmDataIniList, t1InstalledFMDirs, t2InstalledFMDirs, t3InstalledFMDirs);

            /*
             TODO: There's an extreme corner case where duplicate FMs can appear in the list
             It's so unlikely it's almost not worth worrying about, but here's the scenario:
             -The FM is installed by hand and not truncated
             -The FM is not in the list
             -A matching archive exists for the FM
             In this scenario, the FM is added twice to the list, once with the full installed folder name and
             NoArchive set to true, and once with a truncated installed dir name, the correct archive name, and
             NoArchive not present (false).
             The code in here is so crazy-go-nuts I can't even find where this is happening. But putting this
             note down for the future.
            */
        }

        #region Set names

        private static void SetArchiveNames(FMInstallPaths fmInstPaths, List<string> fmArchives, List<FanMission> fmDataIniList)
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

                    string archiveName = null;
                    // Skip the expensive archive name search if we're marked as having no archive
                    if (!fm.NoArchive)
                    {
                        archiveName = GetArchiveNameFromInstalledDir(fmInstPaths, fmDataIniList, fm, fmArchives);
                    }
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
                        if (fm.NoArchive)
                        {
                            var fmselInf = GetFMSelInfPath(fm, fmInstPaths);
                            if (!fmselInf.IsEmpty()) WriteFMSelInf(fm, fmselInf, archiveName);
                        }
                        fm.NoArchive = false;
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

        #endregion

        #region Merge

        private static void MergeNewArchiveFMs(List<string> fmArchives, List<FanMission> fmDataIniList,
            FMInstallPaths fmInstPaths)
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
                        if (fm.NoArchive)
                        {
                            var fmselInf = GetFMSelInfPath(fm, fmInstPaths);
                            if (!fmselInf.IsEmpty()) WriteFMSelInf(fm, fmselInf, archive);
                        }
                        fm.NoArchive = false;

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
                    fmDataIniList.Add(new FanMission { Archive = archive, NoArchive = false });
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

        #endregion

        private static string GetArchiveNameFromInstalledDir(FMInstallPaths fmInstPaths, List<FanMission> fmDataIniList, FanMission fm, List<string> archives)
        {
            // The game type is supposed to be inferred from the installed location, but it could be unknown in
            // the following scenario:
            // -An FM is in the ini list which has an installed folder but no archive name, the game type has been
            // removed from the FM, and the FM no longer exists in any game's installed folder (so its game type
            // can't be inferred). Very rare and unlikely, but I came across it recently, so removing the assert
            // and just returning null here.
            // Note: It looks like this is handled below with a return on empty anyway, so in release mode there's
            // no bug. But we're more explicit now.

            var fmselInf = GetFMSelInfPath(fm, fmInstPaths);

            string FixUp()
            {
                // Make a best-effort attempt to find what this FM's archive name should be
                // PERF: 5ms to run it once on the ~1500 set with no hits, but the time taken is all in the
                // ToInstDirName* calls. So, it doesn't really scale if the user has a bunch of installed FMs
                // with no matching archives, but... whatcha gonna do? We need this automatic linkup thing.
                // PERF: NoArchive property caches this value so this only gets run once per archive-less FM and
                // then never again, rather than once per startup always.
                // PERF_TODO: Does this actually even need to be run?
                // Now that I know the NoArchive value can be set back in MergeNewArchiveFMs, I wonder if this is
                // wholly or at least partially unnecessary. If we don't have an archive name by this point, do
                // we therefore already know this is not going to find anything?
                bool truncate = fm.Game != Game.Thief3;
                var tryArchive =
                    archives.FirstOrDefault(x => x.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir)) ??
                    archives.FirstOrDefault(x => x.ToInstDirNameNDL().EqualsI(fm.InstalledDir)) ??
                    archives.FirstOrDefault(x => x.EqualsI(fm.InstalledDir)) ??
                    fmDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir))?.Archive ??
                    fmDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameNDL().EqualsI(fm.InstalledDir))?.Archive ??
                    fmDataIniList.FirstOrDefault(x => x.InstalledDir.EqualsI(fm.InstalledDir))?.Archive;

                if (tryArchive.IsEmpty())
                {
                    fm.NoArchive = true;
                    return null;
                }

                fm.NoArchive = false;

                WriteFMSelInf(fm, fmselInf, tryArchive);

                return tryArchive;
            }

            if (!File.Exists(fmselInf)) return FixUp();

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
                return FixUp();
            }

            var installedName = lines[0].Substring(lines[0].IndexOf('=') + 1).Trim();
            if (!installedName.EqualsI(fm.InstalledDir))
            {
                return FixUp();
            }

            var archiveName = lines[1].Substring(lines[1].IndexOf('=') + 1).Trim();
            if (archiveName.IsEmpty())
            {
                return FixUp();
            }

            return archiveName;
        }

        private static string GetFMSelInfPath(FanMission fm, FMInstallPaths fmInstPaths)
        {
            if (fm.Game == Game.Null) return null;

            var gamePath =
                fm.Game == Game.Thief1 ? fmInstPaths.T1 :
                fm.Game == Game.Thief2 ? fmInstPaths.T2 :
                // TODO: If SU's FMSel mangles install names in a different way, I need to account for it here
                fm.Game == Game.Thief3 ? fmInstPaths.T3 :
                null;

            if (gamePath.IsEmpty()) return null;

            var fmDir = Path.Combine(gamePath, fm.InstalledDir);
            return Path.Combine(fmDir, Paths.FMSelInf);
        }

        private static void WriteFMSelInf(FanMission fm, string path, string archiveName)
        {
            try
            {
                using (var sw = new StreamWriter(path, append: false))
                {
                    sw.WriteLine("Name=" + fm.InstalledDir);
                    sw.WriteLine("Archive=" + archiveName);
                }
            }
            catch (Exception ex)
            {
                Log("Exception in creating or overwriting" + path, ex);
            }
        }

        private static void BuildViewList(List<string> fmArchives, List<FanMission> fmDataIniList,
            List<string> t1InstalledFMDirs, List<string> t2InstalledFMDirs, List<string> t3InstalledFMDirs)
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
                if (item.Game == Game.Null) Core.ViewListGamesNull.Add(i);

                item.Title =
                    !item.Title.IsEmpty() ? item.Title :
                    !item.Archive.IsEmpty() ? item.Archive.RemoveExtension() :
                    item.InstalledDir;
                item.CommentSingleLine = item.Comment.FromRNEscapes().ToSingleLineComment(100);
                AddTagsToFMAndGlobalList(item.TagsString, item.Tags);

                Core.FMsViewList.Add(item);
            }
        }
    }
}

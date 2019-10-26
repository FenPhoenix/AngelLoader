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
using static AngelLoader.Common.GameSupport;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal static class FindFMs
    {
        // MT: On startup only, this is run in parallel with MainForm.ctor and .InitThreadable()
        // So don't touch anything the other touches: anything affecting the view.
        internal static void Find(string[] fmInstPaths, bool startup = false)
        {
            int gameCount = fmInstPaths.Length;

            if (!startup)
            {
                // Make sure we don't lose anything when we re-find!
                Ini.Ini.WriteFullFMDataIni();

                // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from
                // the list when it's in an indeterminate state (which can cause a selection change (bad) and/or
                // a visible change of the list (not really bad but unprofessional looking)).
                // MT: Don't do this on startup because we're running in parallel with the form new/init in that case.
                Core.View.SetRowCount(0);
            }

            // Init or reinit - must be deep-copied or changes propagate back because reference types
            // MT: This is thread-safe, the view ctor and InitThreadable() doesn't touch it.
            PresetTags.DeepCopyTo(GlobalTags);

            #region Back up lists and read FM data file

            // Copy FMs to backup lists before clearing, in case we can't read the ini file. We don't want to end
            // up with a blank or incomplete list and then glibly save it out later.
            var backupList = new List<FanMission>();
            foreach (var fm in FMDataIniList) backupList.Add(fm);

            var viewBackupList = new List<FanMission>();
            foreach (var fm in FMsViewList) viewBackupList.Add(fm);

            FMDataIniList.Clear();
            FMsViewList.Clear();

            var fmDataIniExists = File.Exists(Paths.FMDataIni);

            if (fmDataIniExists)
            {
                try
                {
                    ReadFMDataIni(Paths.FMDataIni, FMDataIniList);
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
                        FMDataIniList.ClearAndAdd(backupList);
                        FMsViewList.ClearAndAdd(viewBackupList);
                        return;
                    }
                }
            }

            #endregion

            #region Get installed dirs from disk

            // Could check inside the folder for a .mis file to confirm it's really an FM folder, but that's
            // horrendously expensive. Talking like eight seconds vs. < 4ms for the 1098 set. Weird.
            var perGameInstFMDirsList = new List<List<string>>(gameCount);

            for (int i = 0; i < gameCount; i++)
            {
                // NOTE! Make sure this list ends up with gameCount items in it. Just in case I change the loop
                // or something.
                perGameInstFMDirsList.Add(new List<string>());

                var instPath = fmInstPaths[i];

                if (Directory.Exists(instPath))
                {
                    try
                    {
                        foreach (var d in Directory.GetDirectories(instPath, "*", SearchOption.TopDirectoryOnly))
                        {
                            var dirName = d.GetTopmostDirName();
                            if (!dirName.EqualsI(".fmsel.cache")) perGameInstFMDirsList[i].Add(dirName);
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

            var perGameFMsList = new List<List<FanMission>>(gameCount);

            for (int i = 0; i < gameCount; i++)
            {
                // NOTE! List must have gameCount items in it
                perGameFMsList.Add(new List<FanMission>());

                foreach (var item in perGameInstFMDirsList[i])
                {
                    perGameFMsList[i].Add(new FanMission { InstalledDir = item, Game = GameIndexToGame((GameIndex)i), Installed = true });
                }
            }

            #endregion

            MergeNewArchiveFMs(fmArchives, fmInstPaths);

            int instInitCount = FMDataIniList.Count;
            for (int i = 0; i < gameCount; i++)
            {
                var curGameInstFMsList = perGameFMsList[i];
                if (curGameInstFMsList.Count > 0) MergeNewInstalledFMs(curGameInstFMsList, instInitCount);
            }

            SetArchiveNames(fmInstPaths, fmArchives);

            SetInstalledNames();

            BuildViewList(fmArchives, perGameInstFMDirsList, gameCount);

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

        private static void SetArchiveNames(string[] fmInstPaths, List<string> fmArchives)
        {
            // Attempt to set archive names for newly found installed FMs (best effort search)
            for (var i = 0; i < FMDataIniList.Count; i++)
            {
                var fm = FMDataIniList[i];

                if (fm.Archive.IsEmpty())
                {
                    if (fm.InstalledDir.IsEmpty())
                    {
                        FMDataIniList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    string archiveName = null;
                    // Skip the expensive archive name search if we're marked as having no archive
                    if (!fm.NoArchive)
                    {
                        archiveName = GetArchiveNameFromInstalledDir(fmInstPaths, fm, fmArchives);
                    }
                    if (archiveName.IsEmpty()) continue;

                    // Exponential (slow) stuff, but we only do it once to correct the list and then never again
                    // NOTE: I guess this removes duplicates, which is why it has to do the search?
                    var existingFM = FMDataIniList.FirstOrDefault(x => x.Archive.EqualsI(archiveName));
                    if (existingFM != null)
                    {
                        existingFM.InstalledDir = fm.InstalledDir;
                        existingFM.Installed = true;
                        existingFM.Game = fm.Game;
                        FMDataIniList.RemoveAt(i);
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

        private static void SetInstalledNames()
        {
            // Fill in empty installed dir names, making sure to check for and handle truncated name collisions
            foreach (var fm in FMDataIniList)
            {
                if (fm.InstalledDir.IsEmpty())
                {
                    var truncate = fm.Game != Game.Thief3;
                    var instDir = fm.Archive.ToInstDirNameFMSel(truncate);
                    int i = 0;

                    // Again, an exponential search, but again, we only do it once to correct the list and then
                    // never again
                    while (FMDataIniList.Any(x => x.InstalledDir.EqualsI(instDir)))
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

        private static void MergeNewArchiveFMs(List<string> fmArchives, string[] fmInstPaths)
        {
            // Attempt at a perf optimization: we don't need to search anything we've added onto the end.
            int initCount = FMDataIniList.Count;
            bool[] checkedArray = new bool[initCount];

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
                    var fm = FMDataIniList[i];

                    // This weird syntax is to memoize stuff for perf, but it seems if there's more than three of
                    // these comparisons(?!) then it either screws up or ReSharper just thinks it will screw up,
                    // because it then starts complaining that the iterator is never changed in the loop(?!?!)
                    // But only if more than three comparisons are done(?!?!?!?!?!) AND ONLY THEN IF IT'S NOT IN
                    // ITS OWN METHOD(?!?!?!?!?!?!?!?!?!) Argh!
                    if (!checkedArray[i] &&
                        fm.Archive.IsEmpty() &&
                        (fm.InstalledDir.EqualsI(aRemoveExt ??= archive.RemoveExtension()) ||
                         fm.InstalledDir.EqualsI(aFMSel ??= archive.ToInstDirNameFMSel(false)) ||
                         fm.InstalledDir.EqualsI(aFMSelTrunc ??= archive.ToInstDirNameFMSel(true)) ||
                         fm.InstalledDir.EqualsI(aNDL ??= archive.ToInstDirNameNDL())))
                    {
                        fm.Archive = archive;
                        if (fm.NoArchive)
                        {
                            var fmselInf = GetFMSelInfPath(fm, fmInstPaths);
                            if (!fmselInf.IsEmpty()) WriteFMSelInf(fm, fmselInf, archive);
                        }
                        fm.NoArchive = false;

                        checkedArray[i] = true;
                        existingFound = true;
                        break;
                    }
                    else if (!checkedArray[i] &&
                             !fm.Archive.IsEmpty() && fm.Archive.EqualsI(archive))
                    {
                        checkedArray[i] = true;
                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    FMDataIniList.Add(new FanMission { Archive = archive, NoArchive = false });
                }
            }
        }

        // This takes an explicit initCount because we call this once per game, and we don't want to grow our
        // initCount with every call (we can keep it the initial size and still have this work, so it's faster)
        private static void MergeNewInstalledFMs(List<FanMission> installedList, int initCount)
        {
            bool[] checkedArray = new bool[initCount];

            for (int gFMi = 0; gFMi < installedList.Count; gFMi++)
            {
                var gFM = installedList[gFMi];

                // bool check seems to be faster than a null check
                bool isEmpty = gFM.InstalledDir.IsEmpty();

                bool existingFound = false;
                for (int i = 0; i < initCount; i++)
                {
                    var fm = FMDataIniList[i];

                    if (!isEmpty &&
                        // Early-out bool - much faster than checking EqualsI()
                        !checkedArray[i] &&
                        !fm.InstalledDir.IsEmpty() &&
                        fm.InstalledDir.EqualsI(gFM.InstalledDir))
                    {
                        fm.Game = gFM.Game;
                        fm.Installed = true;
                        checkedArray[i] = true;
                        existingFound = true;
                        break;
                    }
                }
                if (!existingFound)
                {
                    FMDataIniList.Add(new FanMission
                    {
                        InstalledDir = gFM.InstalledDir,
                        Game = gFM.Game,
                        Installed = true
                    });
                }
            }
        }

        #endregion

        private static string GetArchiveNameFromInstalledDir(string[] fmInstPaths, FanMission fm, List<string> archives)
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
                    FMDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir))?.Archive ??
                    FMDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameNDL().EqualsI(fm.InstalledDir))?.Archive ??
                    FMDataIniList.FirstOrDefault(x => x.InstalledDir.EqualsI(fm.InstalledDir))?.Archive;

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

        private static string GetFMSelInfPath(FanMission fm, string[] fmInstPaths)
        {
            if (!GameIsKnownAndSupported(fm.Game)) return null;

            // TODO: If SU's FMSel mangles install names in a different way, I need to account for it here
            var gamePath = fmInstPaths[(int)GameToGameIndex(fm.Game)];

            return gamePath.IsEmpty() ? null : Path.Combine(gamePath, fm.InstalledDir, Paths.FMSelInf);
        }

        private static void WriteFMSelInf(FanMission fm, string path, string archiveName)
        {
            try
            {
                using var sw = new StreamWriter(path, append: false);
                sw.WriteLine("Name=" + fm.InstalledDir);
                sw.WriteLine("Archive=" + archiveName);
            }
            catch (Exception ex)
            {
                Log("Exception in creating or overwriting" + path, ex);
            }
        }

        private static void BuildViewList(List<string> fmArchives, List<List<string>> perGameInstalledFMDirsList,
            int gameCount)
        {
            ViewListGamesNull.Clear();

            var boolsList = new List<bool?>(gameCount);
            for (int i = 0; i < gameCount; i++) boolsList.Add(null);

            static bool NotInPerGameList(int gCount, FanMission fm, List<bool?> notInList, List<List<string>> list, bool useBool)
            {
                if (!GameIsKnownAndSupported(fm.Game)) return false;
                int intGame = (int)GameToGameIndex(fm.Game);

                if (!useBool)
                {
                    for (int i = 0; i < gCount; i++)
                    {
                        if (intGame == i &&
                            (bool)(notInList[i] = !list[i].ContainsI(fm.InstalledDir)))
                        {
                            return true;
                        }
                    }

                    return false;
                }
                else
                {
                    for (int i = 0; i < gCount; i++)
                    {
                        if (intGame == i &&
                            (notInList[i] ?? !list[i].ContainsI(fm.InstalledDir)))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            for (var i = 0; i < FMDataIniList.Count; i++)
            {
                var item = FMDataIniList[i];

                #region Checks

                // Attempt to avoid re-searching lists
                for (int ti = 0; ti < boolsList.Count; ti++) boolsList[ti] = null;

                if (item.Installed &&
                    NotInPerGameList(gameCount, item, boolsList, perGameInstalledFMDirsList, useBool: false))
                {
                    item.Installed = false;
                }

                // NOTE: Old data
                // FMDataIniList: Thief1(personal)+Thief2(personal)+All(1098 set)
                // Archive dirs: Thief1(personal)+Thief2(personal)
                // Total time taken running this for all FMs in FMDataIniList: 3~7ms
                // Good enough?
                if ((!item.Installed ||
                     NotInPerGameList(gameCount, item, boolsList, perGameInstalledFMDirsList, useBool: true)) &&
                    // Shrink the list as we get matches so we can reduce our search time as we go
                    !fmArchives.ContainsIRemoveFirstHit(item.Archive))
                {
                    continue;
                }

                #endregion

                // Perf so we don't have to iterate the list again later
                if (item.Game == Game.Null) ViewListGamesNull.Add(i);

                item.Title =
                    !item.Title.IsEmpty() ? item.Title :
                    !item.Archive.IsEmpty() ? item.Archive.RemoveExtension() :
                    item.InstalledDir;
                item.CommentSingleLine = item.Comment.FromRNEscapes().ToSingleLineComment(100);
                FMTags.AddTagsToFMAndGlobalList(item.TagsString, item.Tags);

                FMsViewList.Add(item);
            }
        }
    }
}

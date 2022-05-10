﻿//#define ENABLE_NEW_FMS_CHECK

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    // I finally randomly just tried yet again to convert the horrific quadratic searches to dictionary lookups,
    // and it just worked this time. Behavior appears to be the same as before, no removing entries it shouldn't
    // or anything. I'm... kind of speechless.
    // But okay then! YES! We're ready to handle tens of thousands of FMs! Gorge yourselves!
    internal static class FindFMs
    {
        /// <returns>A list of FMs that are part of the view list and that require scanning. Empty if none.</returns>
        internal static List<int> Find_Startup(SplashScreen splashScreen)
        {
            var ret = FindInternal(startup: true);
            splashScreen.SetCheckAtStoredMessageWidth();
            return ret;
        }

        /// <returns>A list of FMs that are part of the view list and that require scanning. Empty if none.</returns>
        internal static List<int> Find() => FindInternal(startup: false);

        // @THREADING: On startup only, this is run in parallel with MainForm.ctor and .InitThreadable()
        // So don't touch anything the other touches: anything affecting the view.
        // @CAN_RUN_BEFORE_VIEW_INIT
        private static List<int> FindInternal(bool startup)
        {
#if ENABLE_NEW_FMS_CHECK
            // @MEM/@PERF_TODO(Find): These bools can help avoid unnecessary work, BUT!
            // We need to still make sure that if the corresponding FM does NOT have a DateAdded set, that we
            // still add it even if we don't do the rest of the linkup work!
            // Now that I randomly seemed to have figured out how to hashtable the thing after years of trying
            // and failing, we don't really need these that badly, but we could turn them on later for a small
            // perf boost if we have no new FMs to add.
            bool newInstalledDirs = false;
            bool newArchives = false;

            // New check functionality disabled for now.
            bool NewInstalledDirs() => true;
            bool NewArchives() => true;
#endif

            // @MEM/@PERF_TODO(Find): We probably have more hashtables in here than we need
            // So we could probably cut a couple out, but I'm so relieved at not being quadratic anymore that
            // I kinda don't even care right now.

            if (!startup)
            {
                // Make sure we don't lose anything when we re-find!
                // NOTE: This also writes out TagsStrings and then reads them back in and syncs them with Tags.
                // Critical that that gets done.
                Ini.WriteFullFMDataIni();

                // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from
                // the list when it's in an indeterminate state (which can cause a selection change (bad) and/or
                // a visible change of the list (not really bad but unprofessional looking)).
                // @THREADING: Don't do this on startup because we're running in parallel with the form new/init in that case.
                Core.View.SetRowCount(0);
            }

            // Init or reinit - must be deep-copied or changes propagate back because reference types
            // @THREADING: This is thread-safe, the view ctor and InitThreadable() doesn't touch it.
            PresetTags.DeepCopyTo(GlobalTags);

            #region Back up lists and read FM data file

            // Copy FMs to backup lists before clearing, in case we can't read the ini file. We don't want to end
            // up with a blank or incomplete list and then glibly save it out later.
            FanMission[] backupList = new FanMission[FMDataIniList.Count];
            FMDataIniList.CopyTo(backupList);

            FanMission[] viewBackupList = new FanMission[FMsViewList.Count];
            FMsViewList.CopyTo(viewBackupList);

            FMDataIniList.Clear();
            FMsViewList.Clear();

            bool fmDataIniExists = File.Exists(Paths.FMDataIni);

            if (fmDataIniExists)
            {
                try
                {
                    Ini.ReadFMDataIni(Paths.FMDataIni, FMDataIniList);
                }
                catch (Exception ex)
                {
                    Log("Exception reading FM data ini", ex);
                    if (startup)
                    {
                        // Language will be loaded by this point
                        // ... but the view will be initializing in parallel! So we can't call a method that will
                        // check if the view is there to invoke to it!
                        // @vNext(FindFMs error dialog): We could invoke this to the splash screen thread...
                        Dialogs.ShowErrorNoInvoke(LText.AlertMessages.FindFMs_ExceptionReadingFMDataIni);
                        Core.EnvironmentExitDoShutdownTasks(1);
                    }
                    else
                    {
                        FMDataIniList.ClearAndAdd(backupList);
                        FMsViewList.ClearAndAdd(viewBackupList);
                        return new List<int>();
                    }
                }
            }

            #endregion

            #region Get installed dirs from disk

#if ENABLE_NEW_FMS_CHECK
            var fmDataIniInstalledDirsHash = new HashSetI(FMDataIniList.Count);
            for (int i = 0; i < FMDataIniList.Count; i++)
            {
                fmDataIniInstalledDirsHash.Add(FMDataIniList[i].InstalledDir);
            }
#endif

            // Could check inside the folder for a .mis file to confirm it's really an FM folder, but that's
            // horrendously expensive. Talking like eight seconds vs. < 4ms for the 1098 set. Weird.
            var perGameInstFMDirsList = new List<List<string>>(SupportedGameCount);
            var perGameInstFMDirsDatesList = new List<List<DateTime>>(SupportedGameCount);

            for (int gi = 0; gi < SupportedGameCount; gi++)
            {
                // NOTE! Make sure this list ends up with SupportedGameCount items in it. Just in case I change
                // the loop or something.
                perGameInstFMDirsList.Add(new List<string>());
                perGameInstFMDirsDatesList.Add(new List<DateTime>());

                string instPath = Config.GetFMInstallPath((GameIndex)gi);
                if (Directory.Exists(instPath))
                {
                    try
                    {
                        var dirs = FastIO.GetDirsTopOnly_FMs(instPath, "*", out List<DateTime> dateTimes);
                        for (int di = 0; di < dirs.Count; di++)
                        {
                            string d = dirs[di];
                            if (!d.EqualsI(Paths.FMSelCache))
                            {
                                perGameInstFMDirsList[gi].Add(d);
                                perGameInstFMDirsDatesList[gi].Add(dateTimes[di]);
#if ENABLE_NEW_FMS_CHECK
                                if (!fmDataIniInstalledDirsHash.Contains(d))
                                {
                                    newInstalledDirs = true;
                                }
#endif
                            }
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

            var fmArchivesAndDatesDict = new DictionaryI<DateTime>();

            var archivePaths = FMArchives.GetFMArchivePaths();
            bool onlyOnePath = archivePaths.Count == 1;

#if ENABLE_NEW_FMS_CHECK
            var fmDataIniArchivesHash = new HashSetI(FMDataIniList.Count);
            for (int i = 0; i < FMDataIniList.Count; i++)
            {
                fmDataIniArchivesHash.Add(FMDataIniList[i].Archive);
            }
#endif

            for (int ai = 0; ai < archivePaths.Count; ai++)
            {
                try
                {
                    // Returns filenames only (not full paths)
                    var files = FastIO.GetFilesTopOnly_FMs(archivePaths[ai], "*", out List<DateTime> dateTimes);
                    for (int fi = 0; fi < files.Count; fi++)
                    {
                        string f = files[fi];
                        // Do this first because it should be faster than a dictionary lookup
                        if (!f.ExtIsArchive()) continue;
                        // NOTE: We do a ContainsKey check to keep behavior the same as previously. When we use
                        // dict[key] = value, it _replaces_ the value with the new one every time. What we want
                        // is for it to just not touch it at all if the key is already in there. This check does
                        // technically slow it down some, but the actual perf degradation is negligible. And we
                        // still avoid the n-squared 1.6-million-call nightmare we get with ~1600 FMs in the list.
                        // Nevertheless, we can avoid even this small extra cost if we only have one FM archive
                        // path, so no harm in keeping the only-one-path check.
                        if ((onlyOnePath || !fmArchivesAndDatesDict.ContainsKey(f)) &&
                            // @DIRSEP: These are filename only, no need for PathContainsI()
                            !f.ContainsI(Paths.FMSelBak))
                        {
                            fmArchivesAndDatesDict[f] = dateTimes[fi];
#if ENABLE_NEW_FMS_CHECK
                            if (!fmDataIniArchivesHash.Contains(f))
                            {
                                newArchives = true;
                            }
#endif
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception getting files in " + archivePaths[ai], ex);
                }
            }

            int fmArchivesAndDatesDictLen = fmArchivesAndDatesDict.Count;
            // PERF_TODO: May want to keep these as dicts later or change other vars to dicts
            string[] fmArchives = new string[fmArchivesAndDatesDictLen];
            DateTime[] fmArchivesDates = new DateTime[fmArchivesAndDatesDictLen];
            {
                int i = 0;
                foreach (var item in fmArchivesAndDatesDict)
                {
                    fmArchives[i] = item.Key;
                    fmArchivesDates[i] = item.Value;
                    i++;
                }
            }

            #endregion

            #region Build FanMission objects from installed dirs

            var perGameFMsList = new List<List<FanMission>>(SupportedGameCount);

#if ENABLE_NEW_FMS_CHECK
            if (NewInstalledDirs())
#endif
            {
                for (int gi = 0; gi < SupportedGameCount; gi++)
                {
                    // NOTE! List must have SupportedGameCount items in it
                    perGameFMsList.Add(new List<FanMission>());

                    for (int di = 0; di < perGameInstFMDirsList[gi].Count; di++)
                    {
                        perGameFMsList[gi].Add(new FanMission
                        {
                            InstalledDir = perGameInstFMDirsList[gi][di],
                            Game = GameIndexToGame((GameIndex)gi),
                            Installed = true
                        });
                    }
                }
            }

            #endregion

#if ENABLE_NEW_FMS_CHECK
            if (NewArchives())
#endif
            {
                MergeNewArchiveFMs(fmArchives, fmArchivesDates);
            }

#if ENABLE_NEW_FMS_CHECK
            if (NewInstalledDirs())
#endif
            {
                int instInitCount = FMDataIniList.Count;

                var fmDataIniInstDirDict = new DictionaryI<FanMission>(instInitCount);
                for (int i = 0; i < instInitCount; i++)
                {
                    var fm = FMDataIniList[i];
                    if (!fm.InstalledDir.IsEmpty() && !fmDataIniInstDirDict.ContainsKey(fm.InstalledDir))
                    {
                        fmDataIniInstDirDict.Add(fm.InstalledDir, fm);
                    }
                }

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    var curGameInstFMsList = perGameFMsList[i];
                    if (curGameInstFMsList.Count > 0)
                    {
                        MergeNewInstalledFMs(
                            curGameInstFMsList,
                            perGameInstFMDirsDatesList[i],
                            fmDataIniInstDirDict);
                    }
                }
            }

#if ENABLE_NEW_FMS_CHECK
            if (NewArchives())
#endif
            {
                SetArchiveNames(fmArchives);
            }

#if ENABLE_NEW_FMS_CHECK
            if (NewInstalledDirs())
#endif
            {
                SetInstalledNames();
            }

            // Super quick-n-cheap hack for perf: So we don't have to iterate the whole list looking for unscanned
            // FMs. This will contain indexes into FMDataIniList (not FMsViewList!)
            var fmsViewListUnscanned = new List<int>(FMDataIniList.Count);

            BuildViewList(fmArchives, perGameInstFMDirsList, fmsViewListUnscanned);

            return fmsViewListUnscanned;

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

        // @BigO(FindFMs.SetArchiveNames())
        private static void SetArchiveNames(string[] fmArchives)
        {
            // Attempt to set archive names for newly found installed FMs (best effort search)
            for (int i = 0; i < FMDataIniList.Count; i++)
            {
                FanMission fm = FMDataIniList[i];

                if (fm.Archive.IsEmpty())
                {
                    if (fm.InstalledDir.IsEmpty())
                    {
                        FMDataIniList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    // PERF_TODO: Should we keep null here because it's faster? Is it faster? (tight loop)
                    string? archiveName = null;
                    // Skip the expensive archive name search if we're marked as having no archive
                    if (!fm.NoArchive)
                    {
                        // @BigO(FindFMs.SetArchiveNames()/GetArchiveNamesFromInstalledDir():
                        // This one potentially does multiple searches through large lists for archive / inst /
                        // inst-truncated / etc. and we're in a loop here already.
                        archiveName = GetArchiveNameFromInstalledDir(fm, fmArchives);
                    }
                    if (archiveName.IsEmpty()) continue;

                    // Exponential (slow) stuff, but we only do it once to correct the list and then never again
                    // NOTE: I guess this removes duplicates, which is why it has to do the search?
                    FanMission? existingFM = FMDataIniList.Find(x => x.Archive.EqualsI(archiveName));
                    if (existingFM != null)
                    {
                        existingFM.InstalledDir = fm.InstalledDir;
                        existingFM.Installed = true;
                        existingFM.Game = fm.Game;
                        existingFM.DateAdded ??= fm.DateAdded;
                        FMDataIniList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        fm.Archive = archiveName;
                        if (fm.NoArchive)
                        {
                            string? fmselInf = GetFMSelInfPath(fm);
                            if (!fmselInf.IsEmpty()) WriteFMSelInf(fm, fmselInf, archiveName);
                        }
                        fm.NoArchive = false;
                    }
                }
            }
        }

        // @BigO(FindFMs.SetInstalledNames())
        private static void SetInstalledNames()
        {
            // Fill in empty installed dir names, making sure to check for and handle truncated name collisions
            foreach (FanMission fm in FMDataIniList)
            {
                if (fm.InstalledDir.IsEmpty())
                {
                    bool truncate = fm.Game != Game.Thief3;
                    string instDir = fm.Archive.ToInstDirNameFMSel(truncate);
                    int i = 0;

                    // Again, an exponential search, but again, we only do it once to correct the list and then
                    // never again
                    while (FMDataIniList.Any(x => x.InstalledDir.EqualsI(instDir)))
                    {
                        // Yeah, this'll never happen, but hey
                        if (i > 999) break;

                        // Conform to FMSel's numbering format
                        string append = "(" + (i + 2).ToString() + ")";

                        if (truncate && instDir.Length + append.Length > 30)
                        {
                            instDir = instDir.Substring(0, 30 - append.Length);
                        }
                        instDir += append;

                        i++;
                    }

                    // If it overflowed, oh well. You get what you deserve in that case.
                    fm.InstalledDir = instDir;
                }
            }
        }

        #endregion

        #region Merge

        private static void MergeNewArchiveFMs(string[] fmArchives, DateTime[] dateTimes)
        {
            // Attempt at a perf optimization: we don't need to search anything we've added onto the end.
            // 2022-05-10 post-hash-tabling: Dunno if we need this anymore but it keeps the same behavior
            // so not touching it!
            int initCount = FMDataIniList.Count;

            var fmDataIniInstDirDict = new DictionaryI<FanMission>(initCount);
            for (int i = 0; i < initCount; i++)
            {
                var fm = FMDataIniList[i];
                if (fm.Archive.IsEmpty() && !fmDataIniInstDirDict.ContainsKey(fm.InstalledDir))
                {
                    fmDataIniInstDirDict.Add(fm.InstalledDir, fm);
                }
            }

            var fmDataIniArchiveDict = new DictionaryI<FanMission>(initCount);
            for (int i = 0; i < initCount; i++)
            {
                var fm = FMDataIniList[i];
                if (!fm.Archive.IsEmpty() && !fmDataIniArchiveDict.ContainsKey(fm.Archive))
                {
                    fmDataIniArchiveDict.Add(fm.Archive, fm);
                }
            }

            for (int ai = 0; ai < fmArchives.Length; ai++)
            {
                string archive = fmArchives[ai];

                if (fmDataIniInstDirDict.TryGetValue(archive.RemoveExtension(), out FanMission fm) ||
                    fmDataIniInstDirDict.TryGetValue(archive.ToInstDirNameFMSel(false), out fm) ||
                    fmDataIniInstDirDict.TryGetValue(archive.ToInstDirNameFMSel(true), out fm) ||
                    fmDataIniInstDirDict.TryGetValue(archive.ToInstDirNameNDL(), out fm))
                {
                    fm.Archive = archive;
                    if (fm.NoArchive)
                    {
                        string? fmselInf = GetFMSelInfPath(fm);
                        if (!fmselInf.IsEmpty()) WriteFMSelInf(fm, fmselInf, archive);
                    }
                    fm.NoArchive = false;

                    fm.DateAdded ??= dateTimes[ai];
                }
                else if (fmDataIniArchiveDict.TryGetValue(archive, out fm))
                {
                    fm.DateAdded ??= dateTimes[ai];
                }
                else
                {
                    FMDataIniList.Add(new FanMission
                    {
                        Archive = archive,
                        NoArchive = false,
                        DateAdded = dateTimes[ai]
                    });
                }
            }
        }

        // This takes an explicit initCount because we call this once per game, and we don't want to grow our
        // initCount with every call (we can keep it the initial size and still have this work, so it's faster)
        // 2022-05-10 post-hash-tabling: Same, dunno if we need it but not touching it.
        private static void MergeNewInstalledFMs(
            List<FanMission> installedList,
            List<DateTime> dateTimes,
            DictionaryI<FanMission> fmDataIniInstDirDict)
        {
            for (int gFMi = 0; gFMi < installedList.Count; gFMi++)
            {
                var gFM = installedList[gFMi];

                if (fmDataIniInstDirDict.TryGetValue(gFM.InstalledDir, out FanMission fm))
                {
                    fm.Game = gFM.Game;
                    fm.Installed = true;
                    fm.DateAdded ??= dateTimes[gFMi];
                }
                else
                {
                    FMDataIniList.Add(new FanMission
                    {
                        InstalledDir = gFM.InstalledDir,
                        Game = gFM.Game,
                        Installed = true,
                        DateAdded = dateTimes[gFMi]
                    });
                }
            }
        }

        #endregion

        // PERF_TODO: Keep returning null here for speed? Or even switch to a string/bool combo...?
        private static string? GetArchiveNameFromInstalledDir(FanMission fm, string[] archives)
        {
            // The game type is supposed to be inferred from the installed location, but it could be unknown in
            // the following scenario:
            // -An FM is in the ini list which has an installed folder but no archive name, the game type has been
            // removed from the FM, and the FM no longer exists in any game's installed folder (so its game type
            // can't be inferred). Very rare and unlikely, but I came across it recently, so removing the assert
            // and just returning null here.
            // Note: It looks like this is handled below with a return on empty anyway, so in release mode there's
            // no bug. But we're more explicit now.

            string? fmselInf = GetFMSelInfPath(fm);

            string? FixUp()
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
                // @BigO(FindFMs.GetArchiveNameFromInstalledDir()), but we're getting there!
                bool truncate = fm.Game != Game.Thief3;
                string? tryArchive =
                    Array.Find(archives, x => x.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir)) ??
                    Array.Find(archives, x => x.ToInstDirNameNDL().EqualsI(fm.InstalledDir)) ??
                    Array.Find(archives, x => x.EqualsI(fm.InstalledDir)) ??
                    FMDataIniList.Find(x => x.Archive.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir))?.Archive ??
                    FMDataIniList.Find(x => x.Archive.ToInstDirNameNDL().EqualsI(fm.InstalledDir))?.Archive ??
                    FMDataIniList.Find(x => x.InstalledDir.EqualsI(fm.InstalledDir))?.Archive;

                if (tryArchive.IsEmpty())
                {
                    fm.NoArchive = true;
                    return null;
                }

                fm.NoArchive = false;

                if (!fmselInf.IsEmpty()) WriteFMSelInf(fm, fmselInf, tryArchive);

                return tryArchive;
            }

            if (!File.Exists(fmselInf!)) return FixUp();

            List<string> lines;
            try
            {
                lines = File_ReadAllLines_List(fmselInf!);
            }
            catch (Exception ex)
            {
                Log("Exception reading " + fmselInf, ex);
                return null;
            }

            if (lines.Count < 2 || !lines[0].StartsWithI("Name=") || !lines[1].StartsWithI("Archive="))
            {
                return FixUp();
            }

            string installedName = lines[0].Substring(lines[0].IndexOf('=') + 1).Trim();
            if (!installedName.EqualsI(fm.InstalledDir))
            {
                return FixUp();
            }

            string archiveName = lines[1].Substring(lines[1].IndexOf('=') + 1).Trim();
            if (archiveName.IsEmpty())
            {
                return FixUp();
            }

            return archiveName;
        }

        private static string? GetFMSelInfPath(FanMission fm)
        {
            if (!GameIsKnownAndSupported(fm.Game)) return null;

            string fmInstPath = Config.GetFMInstallPathUnsafe(fm.Game);

            return fmInstPath.IsEmpty() ? null : Path.Combine(fmInstPath, fm.InstalledDir, Paths.FMSelInf);
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

        private static void BuildViewList(
            string[] fmArchives,
            List<List<string>> perGameInstalledFMDirsList,
            List<int> fmsViewListUnscanned)
        {
            var fmArchivesDict = new DictionaryI<bool>(fmArchives.Length);
            for (int i = 0; i < fmArchives.Length; i++)
            {
                fmArchivesDict.Add(fmArchives[i], false);
            }

            bool?[] boolsList = new bool?[SupportedGameCount];

            static bool NotInPerGameList(int gCount, FanMission fm, bool?[] notInList, List<List<string>> list, bool useBool)
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

            for (int i = 0; i < FMDataIniList.Count; i++)
            {
                FanMission fm = FMDataIniList[i];

                #region Checks

                // Attempt to avoid re-searching lists
                for (int ti = 0; ti < boolsList.Length; ti++) boolsList[ti] = null;

                if (fm.Installed &&
                    NotInPerGameList(SupportedGameCount, fm, boolsList, perGameInstalledFMDirsList, useBool: false))
                {
                    fm.Installed = false;
                }

                if (!fm.Installed ||
                    NotInPerGameList(SupportedGameCount, fm, boolsList, perGameInstalledFMDirsList, useBool: true))
                {
                    // Fix: we can have duplicate archive names if the installed dir is different, so cull them
                    // out of the view list at least.
                    // (This used to get done as an accidental side effect of the ContainsIRemoveFirst() call)
                    // TODO: We shouldn't have duplicate archives, but importing might add different installed dirs...
                    bool success = fmArchivesDict.TryGetValue(fm.Archive, out bool checkedThis);
                    if (success)
                    {
                        if (checkedThis)
                        {
                            continue;
                        }
                        else
                        {
                            fmArchivesDict[fm.Archive] = true;
                        }
                    }
                    else
                    {
                        fm.MarkedUnavailable = true;
                    }
                }

                #endregion

                // Perf so we don't have to iterate the list again later
                if (FMNeedsScan(fm)) fmsViewListUnscanned.Add(i);

                fm.Title =
                    !fm.Title.IsEmpty() ? fm.Title :
                    !fm.Archive.IsEmpty() ? fm.Archive.RemoveExtension() :
                    fm.InstalledDir;
                fm.CommentSingleLine = fm.Comment.FromRNEscapes().ToSingleLineComment(100);
                FMTags.AddTagsToFMAndGlobalList(fm.TagsString, fm.Tags);

                FMsViewList.Add(fm);
            }

            FMsViewList.TrimExcess();
        }
    }
}

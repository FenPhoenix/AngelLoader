//#define ENABLE_NEW_FMS_CHECK

using System;
using System.Collections.Generic;
using System.IO;
using AngelLoader.DataClasses;
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
        private sealed class LastResortLinkupBundle
        {
            private DictionaryI<string>? _archivesToInstDirNameFMSelTruncated;
            private DictionaryI<string>? _archivesToInstDirNameFMSelNotTruncated;
            private DictionaryI<string>? _archivesToInstDirNameNDLTruncated;
            private DictionaryI<string>? _archives;
            private DictionaryI<string>? _archivesToInstDirNameFMSelTruncated_FromFMDataIniList;
            private DictionaryI<string>? _archivesToInstDirNameFMSelNotTruncated_FromFMDataIniList;
            private DictionaryI<string>? _archivesToInstDirNameNDLTruncated_FromFMDataIniList;
            private DictionaryI<string>? _installedDirs_FromFMDataIniList;

            internal DictionaryI<string> GetInstDirFMSelToArchives(DictionaryI<ArchiveValueData> archives, bool truncate)
            {
                if (truncate)
                {
                    if (_archivesToInstDirNameFMSelTruncated == null)
                    {
                        _archivesToInstDirNameFMSelTruncated = new DictionaryI<string>(archives.Count);
                        foreach (var item in archives)
                        {
                            string value = item.Key;
                            string key = value.ToInstDirNameFMSel(true);
                            if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameFMSelTruncated.ContainsKey(key))
                            {
                                _archivesToInstDirNameFMSelTruncated.Add(key, value);
                            }
                        }
                    }
                    return _archivesToInstDirNameFMSelTruncated;
                }
                else
                {
                    if (_archivesToInstDirNameFMSelNotTruncated == null)
                    {
                        _archivesToInstDirNameFMSelNotTruncated = new DictionaryI<string>(archives.Count);
                        foreach (var item in archives)
                        {
                            string value = item.Key;
                            string key = value.ToInstDirNameFMSel(false);
                            if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameFMSelNotTruncated.ContainsKey(key))
                            {
                                _archivesToInstDirNameFMSelNotTruncated.Add(key, value);
                            }
                        }
                    }
                    return _archivesToInstDirNameFMSelNotTruncated;
                }
            }

            internal DictionaryI<string> GetInstDirNDLTruncatedToArchives(DictionaryI<ArchiveValueData> archives)
            {
                if (_archivesToInstDirNameNDLTruncated == null)
                {
                    _archivesToInstDirNameNDLTruncated = new DictionaryI<string>(archives.Count);
                    foreach (var item in archives)
                    {
                        string value = item.Key;
                        string key = value.ToInstDirNameNDL(truncate: true);
                        if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameNDLTruncated.ContainsKey(key))
                        {
                            _archivesToInstDirNameNDLTruncated.Add(key, value);
                        }
                    }
                }
                return _archivesToInstDirNameNDLTruncated;
            }

            internal DictionaryI<string> GetInstDirNameToArchives(DictionaryI<ArchiveValueData> archives)
            {
                if (_archives == null)
                {
                    _archives = new DictionaryI<string>(archives.Count);
                    foreach (var item in archives)
                    {
                        string key = item.Key;
                        string value = key;
                        if (!key.IsEmpty() && !value.IsEmpty() && !_archives.ContainsKey(key))
                        {
                            _archives.Add(key, value);
                        }
                    }
                }
                return _archives;
            }

            internal DictionaryI<string> GetInstDirNameFMSelToArchives_FromFMDataIni(bool truncate)
            {
                if (truncate)
                {
                    if (_archivesToInstDirNameFMSelTruncated_FromFMDataIniList == null)
                    {
                        _archivesToInstDirNameFMSelTruncated_FromFMDataIniList = new DictionaryI<string>(FMDataIniList.Count);
                        for (int i = 0; i < FMDataIniList.Count; i++)
                        {
                            string value = FMDataIniList[i].Archive;
                            string key = value.ToInstDirNameFMSel(truncate: true);
                            if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameFMSelTruncated_FromFMDataIniList.ContainsKey(key))
                            {
                                _archivesToInstDirNameFMSelTruncated_FromFMDataIniList.Add(key, value);
                            }
                        }
                    }
                    return _archivesToInstDirNameFMSelTruncated_FromFMDataIniList;
                }
                else
                {
                    if (_archivesToInstDirNameFMSelNotTruncated_FromFMDataIniList == null)
                    {
                        _archivesToInstDirNameFMSelNotTruncated_FromFMDataIniList = new DictionaryI<string>(FMDataIniList.Count);
                        for (int i = 0; i < FMDataIniList.Count; i++)
                        {
                            string value = FMDataIniList[i].Archive;
                            string key = value.ToInstDirNameFMSel(truncate: false);
                            if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameFMSelNotTruncated_FromFMDataIniList.ContainsKey(key))
                            {
                                _archivesToInstDirNameFMSelNotTruncated_FromFMDataIniList.Add(key, value);
                            }
                        }
                    }
                    return _archivesToInstDirNameFMSelNotTruncated_FromFMDataIniList;
                }
            }

            internal DictionaryI<string> GetInstDirNDLTruncatedToArchives_FromFMDataIni()
            {
                if (_archivesToInstDirNameNDLTruncated_FromFMDataIniList == null)
                {
                    _archivesToInstDirNameNDLTruncated_FromFMDataIniList = new DictionaryI<string>(FMDataIniList.Count);
                    for (int i = 0; i < FMDataIniList.Count; i++)
                    {
                        string value = FMDataIniList[i].Archive;
                        string key = value.ToInstDirNameNDL(truncate: true);
                        if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameNDLTruncated_FromFMDataIniList.ContainsKey(key))
                        {
                            _archivesToInstDirNameNDLTruncated_FromFMDataIniList.Add(key, value);
                        }
                    }
                }
                return _archivesToInstDirNameNDLTruncated_FromFMDataIniList;
            }

            internal DictionaryI<string> GetInstDirToArchives_FromFMDataIni()
            {
                if (_installedDirs_FromFMDataIniList == null)
                {
                    _installedDirs_FromFMDataIniList = new DictionaryI<string>(FMDataIniList.Count);
                    for (int i = 0; i < FMDataIniList.Count; i++)
                    {
                        string value = FMDataIniList[i].Archive;
                        string key = FMDataIniList[i].InstalledDir;
                        if (!key.IsEmpty() && !value.IsEmpty() && !_installedDirs_FromFMDataIniList.ContainsKey(key))
                        {
                            _installedDirs_FromFMDataIniList.Add(key, value);
                        }
                    }
                }
                return _installedDirs_FromFMDataIniList;
            }
        }

        private sealed class ArchiveValueData
        {
            internal readonly DateTime DateTime;
            // This bool is for BuildViewList; we don't use it until then but it's just so we can pass the dictionary
            // all the way through without creating a second one.
            internal bool Checked;

            internal ArchiveValueData(DateTime dateTime) => DateTime = dateTime;
        }

        private sealed class InstDirValueData
        {
            internal readonly FanMission FM;
            internal readonly DateTime DateTime;

            internal InstDirValueData(FanMission fm, DateTime dateTime)
            {
                FM = fm;
                DateTime = dateTime;
            }
        }

        /// <summary>
        /// Finds and merges new FMs (archives and installed) into the set. Call only on startup during parallel
        /// load.
        /// </summary>
        /// <param name="splashScreen">The splash screen for it to update with a checkmark when it's done.</param>
        /// <returns>A list of FMs that are part of the view list and that require scanning. Empty if none.</returns>
        internal static (List<int> FMsViewListUnscanned, Exception? Ex) Find_Startup(SplashScreen splashScreen)
        {
            // This will run in a thread, so we don't want to try throwing up any dialogs or running the shutdown
            // tasks or anything here... just return an exception and handle it on the main thread...
            try
            {
                var ret = FindInternal(startup: true);
                splashScreen.SetCheckAtStoredMessageWidth();
                return (ret, null);
            }
            catch (Exception ex)
            {
                return (new List<int>(), ex);
            }
        }

        /// <summary>
        /// Finds and merges new FMs into the set.
        /// </summary>
        /// <returns>A list of FMs that are part of the view list and that require scanning. Empty if none.</returns>
        internal static List<int> Find() => FindInternal(startup: false);

        // @THREADING: On startup only, this is run in parallel with MainForm.ctor and .InitThreadable()
        // So don't touch anything the other touches: anything affecting the view.
        // @CAN_RUN_BEFORE_VIEW_INIT
        private static List<int> FindInternal(bool startup)
        {
#if ENABLE_NEW_FMS_CHECK
            // NOTE(Find): These bools can help avoid unnecessary work, BUT!
            // We need to still make sure that if the corresponding FM does NOT have a DateAdded set, that we
            // still add it even if we don't do the rest of the linkup work!
            // Now that I randomly seemed to have figured out how to hashtable the thing after years of trying
            // and failing, we don't really need these that badly, but we could turn them on later for a small
            // perf boost if we have no new FMs to add.
            // Also, checking requires creating and populating two new hashsets and then doing a full set of
            // lookups into each. That's just adding even more work that probably won't end up really saving us
            // any time.
            bool newInstalledDirs = false;
            bool newArchives = false;

            // New check functionality disabled for now.
            bool NewInstalledDirs() => true;
            bool NewArchives() => true;
#endif

            // @PERF_TODO(Find): Number of hashtable recreations
            // We recreate several hashtables anew after potentially modifying the FM data ini list, because the
            // modification may necessitate the hashtable to be rebuilt from the updated FM list.
            // But, we should check if this is actually needed! We might be able to get rid of the rebuilds.

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
                        throw;
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
            var perGameInstFMDirsItems = new DictionaryI<InstDirValueData>[SupportedGameCount];

            for (int gi = 0; gi < SupportedGameCount; gi++)
            {
                perGameInstFMDirsItems[gi] = new DictionaryI<InstDirValueData>();

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
                                var fm = new FanMission
                                {
                                    InstalledDir = d,
                                    Game = GameIndexToGame((GameIndex)gi),
                                    Installed = true
                                };
                                perGameInstFMDirsItems[gi][d] = new InstDirValueData(fm, dateTimes[di]);
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

            var fmArchivesAndDatesDict = new DictionaryI<ArchiveValueData>();

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
                            fmArchivesAndDatesDict[f] = new ArchiveValueData(dateTimes[fi]);
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

            #endregion

#if ENABLE_NEW_FMS_CHECK
            if (NewArchives())
#endif
            {
                MergeNewArchiveFMs(fmArchivesAndDatesDict);
            }

#if ENABLE_NEW_FMS_CHECK
            if (NewInstalledDirs())
#endif
            {
                int fmDataIniListCount = FMDataIniList.Count;
                var fmDataIniInstDirDict = new DictionaryI<FanMission>(fmDataIniListCount);
                for (int i = 0; i < fmDataIniListCount; i++)
                {
                    var fm = FMDataIniList[i];
                    if (!fm.InstalledDir.IsEmpty() && !fmDataIniInstDirDict.ContainsKey(fm.InstalledDir))
                    {
                        fmDataIniInstDirDict.Add(fm.InstalledDir, fm);
                    }
                }

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    var curGameInstFMsList = perGameInstFMDirsItems[i];
                    if (curGameInstFMsList.Count > 0)
                    {
                        MergeNewInstalledFMs(
                            curGameInstFMsList,
                            fmDataIniInstDirDict);
                    }
                }
            }

#if ENABLE_NEW_FMS_CHECK
            if (NewArchives())
#endif
            {
                SetArchiveNames(fmArchivesAndDatesDict);
            }

#if ENABLE_NEW_FMS_CHECK
            if (NewInstalledDirs())
#endif
            {
                EnsureUniqueInstalledNames();
            }

            // Super quick-n-cheap hack for perf: So we don't have to iterate the whole list looking for unscanned
            // FMs. This will contain indexes into FMDataIniList (not FMsViewList!)
            var fmsViewListUnscanned = new List<int>(FMDataIniList.Count);

            BuildViewList(fmArchivesAndDatesDict, perGameInstFMDirsItems, fmsViewListUnscanned);

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

        private static void SetArchiveNames(DictionaryI<ArchiveValueData> fmArchives)
        {
            DictionaryI<FanMission>? archivesDict = null;
            DictionaryI<FanMission> GetArchivesDict()
            {
                if (archivesDict == null)
                {
                    archivesDict = new DictionaryI<FanMission>(FMDataIniList.Count);
                    for (int i = 0; i < FMDataIniList.Count; i++)
                    {
                        var fm = FMDataIniList[i];
                        if (!fm.Archive.IsEmpty() && !archivesDict.ContainsKey(fm.Archive))
                        {
                            archivesDict.Add(fm.Archive, fm);
                        }
                    }
                }
                return archivesDict;
            }

            var lastResortLinkupBundle = new LastResortLinkupBundle();

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
                        archiveName = GetArchiveNameFromInstalledDir(fm, fmArchives, lastResortLinkupBundle);
                    }
                    if (archiveName.IsEmpty()) continue;

                    // NOTE: I guess this removes duplicates, which is why it has to do the search?
                    if (GetArchivesDict().TryGetValue(archiveName, out FanMission existingFM))
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

        private static void EnsureUniqueInstalledNames()
        {
            // Check for and handle truncated name collisions
            var hash = new HashSetI(FMDataIniList.Count);
            for (int i = 0; i < FMDataIniList.Count; i++)
            {
                var fm = FMDataIniList[i];
                if (!hash.Contains(fm.InstalledDir))
                {
                    hash.Add(fm.InstalledDir);
                }
                else
                {
                    bool truncate = fm.Game != Game.Thief3;
                    for (int j = 0; ; j++)
                    {
                        // Yeah, this'll never happen, but hey
                        // If it overflowed, oh well. You get what you deserve in that case.
                        if (j > 999) return;

                        // Conform to FMSel's numbering format
                        string append = "(" + (j + 2) + ")";

                        if (truncate && fm.InstalledDir.Length + append.Length > 30)
                        {
                            fm.InstalledDir = fm.InstalledDir.Substring(0, 30 - append.Length);
                        }
                        fm.InstalledDir += append;

                        if (!hash.Contains(fm.InstalledDir)) break;
                    }

                    hash.Add(fm.InstalledDir);
                }
            }
        }

        #endregion

        #region Merge

        private static void MergeNewArchiveFMs(DictionaryI<ArchiveValueData> fmArchives)
        {
            int fmDataIniListCount = FMDataIniList.Count;
            var fmDataIniInstDirDict = new DictionaryI<FanMission>(fmDataIniListCount);
            var fmDataIniArchiveDict = new DictionaryI<FanMission>(fmDataIniListCount);
            for (int i = 0; i < fmDataIniListCount; i++)
            {
                var fm = FMDataIniList[i];
                if (fm.Archive.IsEmpty() && !fm.InstalledDir.IsEmpty() && !fmDataIniInstDirDict.ContainsKey(fm.InstalledDir))
                {
                    fmDataIniInstDirDict.Add(fm.InstalledDir, fm);
                }
                if (!fm.Archive.IsEmpty() && !fmDataIniArchiveDict.ContainsKey(fm.Archive))
                {
                    fmDataIniArchiveDict.Add(fm.Archive, fm);
                }
            }

            foreach (var item in fmArchives)
            {
                string archive = item.Key;

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

                    if (fm.InstalledDir.IsEmpty())
                    {
                        bool truncate = fm.Game != Game.Thief3;
                        fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(truncate);
                    }

                    fm.DateAdded ??= item.Value.DateTime;
                }
                else if (fmDataIniArchiveDict.TryGetValue(archive, out fm))
                {
                    fm.DateAdded ??= item.Value.DateTime;

                    if (fm.InstalledDir.IsEmpty())
                    {
                        bool truncate = fm.Game != Game.Thief3;
                        fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(truncate);
                    }
                }
                else
                {
                    FMDataIniList.Add(new FanMission
                    {
                        Archive = archive,
                        InstalledDir = archive.ToInstDirNameFMSel(true),
                        NoArchive = false,
                        DateAdded = item.Value.DateTime
                    });
                }
            }
        }

        private static void MergeNewInstalledFMs(
            DictionaryI<InstDirValueData> installedList,
            DictionaryI<FanMission> fmDataIniInstDirDict)
        {
            foreach (var item in installedList)
            {
                var gFM = item.Value.FM;

                if (fmDataIniInstDirDict.TryGetValue(gFM.InstalledDir, out FanMission fm))
                {
                    fm.Game = gFM.Game;
                    fm.Installed = true;
                    fm.DateAdded ??= item.Value.DateTime;
                }
                else
                {
                    FMDataIniList.Add(new FanMission
                    {
                        InstalledDir = gFM.InstalledDir,
                        Game = gFM.Game,
                        Installed = true,
                        DateAdded = item.Value.DateTime
                    });
                }
            }
        }

        #endregion

        // PERF_TODO: Keep returning null here for speed? Or even switch to a string/bool combo...?
        private static string? GetArchiveNameFromInstalledDir(FanMission fm, DictionaryI<ArchiveValueData> archives, LastResortLinkupBundle bundle)
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
                // PERF: NoArchive property caches this value so this only gets run once per archive-less FM and
                // then never again, rather than once per startup always.
                // PERF_TODO: Does this actually even need to be run?
                // Now that I know the NoArchive value can be set back in MergeNewArchiveFMs, I wonder if this is
                // wholly or at least partially unnecessary. If we don't have an archive name by this point, do
                // we therefore already know this is not going to find anything?
                bool truncate = fm.Game != Game.Thief3;

                if (!bundle.GetInstDirFMSelToArchives(archives, truncate).TryGetValue(fm.InstalledDir, out string? tryArchive) &&
                    !bundle.GetInstDirNDLTruncatedToArchives(archives).TryGetValue(fm.InstalledDir, out tryArchive) &&
                    !bundle.GetInstDirNameToArchives(archives).TryGetValue(fm.InstalledDir, out tryArchive) &&
                    !bundle.GetInstDirNameFMSelToArchives_FromFMDataIni(truncate).TryGetValue(fm.InstalledDir, out tryArchive) &&
                    !bundle.GetInstDirNDLTruncatedToArchives_FromFMDataIni().TryGetValue(fm.InstalledDir, out tryArchive) &&
                    !bundle.GetInstDirToArchives_FromFMDataIni().TryGetValue(fm.InstalledDir, out tryArchive))
                {
                    fm.NoArchive = true;
                    return null;
                }

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
            DictionaryI<ArchiveValueData> fmArchivesDict,
            DictionaryI<InstDirValueData>[] perGameInstalledFMDirsItems,
            List<int> fmsViewListUnscanned)
        {
            bool?[] boolsList = new bool?[SupportedGameCount];

            static bool NotInPerGameList(FanMission fm, bool?[] notInList, DictionaryI<InstDirValueData>[] list, bool useBool)
            {
                if (!GameIsKnownAndSupported(fm.Game)) return false;
                int intGame = (int)GameToGameIndex(fm.Game);

                if (!useBool)
                {
                    return (bool)(notInList[intGame] = !list[intGame].ContainsKey(fm.InstalledDir));
                }
                else
                {
                    return notInList[intGame] ?? !list[intGame].ContainsKey(fm.InstalledDir);
                }
            }

            for (int i = 0; i < FMDataIniList.Count; i++)
            {
                FanMission fm = FMDataIniList[i];

                #region Checks

                // Now that we're using hashtables, we don't really need these I guess, but if they save a lookup
                // then I guess why not
                for (int ti = 0; ti < boolsList.Length; ti++) boolsList[ti] = null;

                if (fm.Installed &&
                    NotInPerGameList(fm, boolsList, perGameInstalledFMDirsItems, useBool: false))
                {
                    fm.Installed = false;
                }

                if (!fm.Installed ||
                    NotInPerGameList(fm, boolsList, perGameInstalledFMDirsItems, useBool: true))
                {
                    if (!fmArchivesDict.ContainsKey(fm.Archive))
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

            // Fix: we can have duplicate archive names if the installed dir is different, so cull them
            // out of the view list at least.
            // (This used to get done as an accidental side effect of the ContainsIRemoveFirst() call)
            // TODO: We shouldn't have duplicate archives, but importing might add different installed dirs...
            var viewListHash = new HashSetI(FMsViewList.Count);
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                var fm = FMsViewList[i];
                if (fm.Archive.IsEmpty()) continue;

                if (!viewListHash.Contains(fm.Archive))
                {
                    viewListHash.Add(fm.Archive);
                }
                else
                {
                    FMsViewList.RemoveAt(i);
                    i--;
                }
            }

            FMsViewList.TrimExcess();
        }
    }
}

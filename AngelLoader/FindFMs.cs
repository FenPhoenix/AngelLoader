//#define ENABLE_TDM_SMART_NAME_APPEND

using System;
using System.Collections.Generic;
using System.IO;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader;

/*
@BetterErrors(FindFMs - All disk hits)
This is tricky. Should we throw on all I/O fails? If we're on startup, we currently just exit the app if we
throw. This hasn't been a problem yet I guess, but maybe we actually shouldn't do that? The user then doesn't
have the chance to go into Settings and maybe be able to fix the paths or something.
What we want to do here is to restore the backed up lists if we fail, so that the old lists are guaranteed
to either be unchanged, or new and valid.
But we should decide what's a hard enough fail to warrant that. Failing to write fmsel.inf definitely isn't
a hard enough fail, since it's really just redundancy and the user probably won't understand what the file
is and why it matters.
*/
internal static class FindFMs
{
    private sealed class LastResortLinkupContext
    {
        private readonly InstDirNameContext _instDirNameContext;

        private DictionaryI<string>? _archivesToInstDirNameFMSelTruncated;
        private DictionaryI<string>? _archivesToInstDirNameFMSelNotTruncated;
        private DictionaryI<string>? _archivesToInstDirNameNDLTruncated;
        private DictionaryI<string>? _archives;
        private DictionaryI<string>? _archivesToInstDirNameFMSelTruncated_FromFMDataIniList;
        private DictionaryI<string>? _archivesToInstDirNameFMSelNotTruncated_FromFMDataIniList;
        private DictionaryI<string>? _archivesToInstDirNameNDLTruncated_FromFMDataIniList;
        private DictionaryI<string>? _installedDirs_FromFMDataIniList;

        public LastResortLinkupContext(InstDirNameContext instDirNameContext) => _instDirNameContext = instDirNameContext;

        internal DictionaryI<string> GetInstDirFMSelToArchives(DictionaryI<ExpandableDate_FromTicks> archives, bool truncate)
        {
            if (truncate)
            {
                if (_archivesToInstDirNameFMSelTruncated == null)
                {
                    _archivesToInstDirNameFMSelTruncated = new DictionaryI<string>(archives.Count);
                    foreach (var item in archives)
                    {
                        string value = item.Key;
                        string key = value.ToInstDirNameFMSel(_instDirNameContext, true);
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
                        string key = value.ToInstDirNameFMSel(_instDirNameContext, false);
                        if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameFMSelNotTruncated.ContainsKey(key))
                        {
                            _archivesToInstDirNameFMSelNotTruncated.Add(key, value);
                        }
                    }
                }
                return _archivesToInstDirNameFMSelNotTruncated;
            }
        }

        internal DictionaryI<string> GetInstDirNDLTruncatedToArchives(DictionaryI<ExpandableDate_FromTicks> archives)
        {
            if (_archivesToInstDirNameNDLTruncated == null)
            {
                _archivesToInstDirNameNDLTruncated = new DictionaryI<string>(archives.Count);
                foreach (var item in archives)
                {
                    string value = item.Key;
                    string key = value.ToInstDirNameNDL(_instDirNameContext, truncate: true);
                    if (!key.IsEmpty() && !value.IsEmpty() && !_archivesToInstDirNameNDLTruncated.ContainsKey(key))
                    {
                        _archivesToInstDirNameNDLTruncated.Add(key, value);
                    }
                }
            }
            return _archivesToInstDirNameNDLTruncated;
        }

        internal DictionaryI<string> GetInstDirNameToArchives(DictionaryI<ExpandableDate_FromTicks> archives)
        {
            if (_archives == null)
            {
                _archives = new DictionaryI<string>(archives.Count);
                foreach (var item in archives)
                {
                    string key = item.Key;
                    // ReSharper disable once InlineTemporaryVariable
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
                        string key = value.ToInstDirNameFMSel(_instDirNameContext, truncate: true);
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
                        string key = value.ToInstDirNameFMSel(_instDirNameContext, truncate: false);
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
                    string key = value.ToInstDirNameNDL(_instDirNameContext, truncate: true);
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

    private sealed class InstDirValueData
    {
        internal readonly FanMission FM;
        internal readonly ExpandableDate_FromTicks DateTime;

        internal InstDirValueData(FanMission fm, ExpandableDate_FromTicks dateTime)
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
    internal static (List<FanMission> FMsViewListUnscanned, Exception? Ex)
    Find_Startup(SplashScreen splashScreen)
    {
        // This will run in a thread, so we don't want to try throwing up any dialogs or running the shutdown
        // tasks or anything here... just return an exception and handle it on the main thread...
        try
        {
            List<FanMission> fmsViewListUnscanned = FindInternal(startup: true);
            splashScreen.SetCheckAtStoredMessageWidth();
            return (fmsViewListUnscanned, null);
        }
        catch (Exception ex)
        {
            return (new List<FanMission>(), ex);
        }
    }

    /// <summary>
    /// Finds and merges new FMs (archives and installed) into the set.
    /// </summary>
    /// <returns>A list of FMs that are part of the view list and that require scanning. Empty if none.</returns>
    internal static List<FanMission> Find()
    {
        AssertR(Core.View != null!, "View was null during FindFMs.Find() call");

        List<FanMission> fmsViewListUnscanned = FindInternal(startup: false);
        Core.View!.SetAvailableAndFinishedFMCount();
        return fmsViewListUnscanned;
    }

    // @THREADING: On startup only, this is run in parallel with MainForm.ctor and .InitThreadable()
    // So don't touch anything the other touches: anything affecting the view.
    // @CAN_RUN_BEFORE_VIEW_INIT
    private static List<FanMission> FindInternal(bool startup)
    {
        InstDirNameContext instDirNameContext = new();

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
            Core.View.DisableFMsListDisplay();
        }

        // Init or reinit - must be deep-copied or changes propagate back because reference types
        // @THREADING: This is thread-safe, the view ctor and InitThreadable() doesn't touch it.
        PresetTags.DeepCopyTo(GlobalTags);

        #region Back up lists and read FM data file

        // Copy FMs to backup lists before clearing, in case we can't read the ini file. We don't want to end
        // up with a blank or incomplete list and then glibly save it out later.
        FanMission[] backupList = new FanMission[FMDataIniList.Count];
        FanMission[] backupListTDM = new FanMission[FMDataIniListTDM.Count];
        FMDataIniList.CopyTo(backupList);
        FMDataIniListTDM.CopyTo(backupListTDM);

        FanMission[] viewBackupList = new FanMission[FMsViewList.Count];
        FMsViewList.CopyTo(viewBackupList);

        FMDataIniList.Clear();
        FMDataIniListTDM.Clear();
        FMsViewList.Clear();

        bool fmDataIniExists = File.Exists(Paths.FMDataIni);

        if (fmDataIniExists)
        {
            try
            {
                Ini.ReadFMDataIni(Paths.FMDataIni, FMDataIniList, FMDataIniListTDM);
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExRead + Paths.FMDataIni, ex);
                if (startup)
                {
                    throw;
                }
                else
                {
                    FMDataIniList.ClearAndAdd_Large(backupList);
                    FMDataIniListTDM.ClearAndAdd_Large(backupListTDM);
                    FMsViewList.ClearAndAdd_Large(viewBackupList);
                    return new List<FanMission>();
                }
            }
        }

        #endregion

        // PERF: We can't know how many files we're going to find, so make the initial list capacity large
        // enough that we're unlikely to have it bump its size up repeatedly. Shaves some time off.
        var files = new List<string>(2000);
        var dateTimes = new List<ExpandableDate_FromTicks>(2000);

        #region Get installed dirs from disk

        // Could check inside the folder for a .mis file to confirm it's really an FM folder, but that's
        // horrendously expensive. Talking like eight seconds vs. < 4ms for the 1098 set. Weird.
        var perGameInstFMDirsItems = new DictionaryI<InstDirValueData>[SupportedGameCount];

        for (int gi = 0; gi < SupportedGameCount; gi++)
        {
            GameIndex gameIndex = (GameIndex)gi;
            if (gameIndex == GameIndex.TDM) continue;

            perGameInstFMDirsItems[gi] = new DictionaryI<InstDirValueData>();

            string instPath = Config.GetFMInstallPath(gameIndex);
            if (Directory.Exists(instPath))
            {
                try
                {
                    FastIO.GetDirsTopOnly_FMs(instPath, "*", files, dateTimes);
                    for (int di = 0; di < files.Count; di++)
                    {
                        string d = files[di];
                        if (!d.EqualsI(Paths.FMSelCache))
                        {
                            var fm = new FanMission
                            {
                                InstalledDir = d,
                                Game = GameIndexToGame(gameIndex),
                                Installed = true
                            };
                            perGameInstFMDirsItems[gi][d] = new InstDirValueData(fm, dateTimes[di]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ErrorText.ExGet + "directories in " + instPath, ex);
                }
            }
        }

        #endregion

        #region Get archives from disk

        var fmArchivesAndDatesDict = new DictionaryI<ExpandableDate_FromTicks>();

        List<string> archivePaths = FMArchives.GetFMArchivePaths();
        bool onlyOnePath = archivePaths.Count == 1;

        for (int ai = 0; ai < archivePaths.Count; ai++)
        {
            try
            {
                // Returns filenames only (not full paths)
                FastIO.GetFilesTopOnly_FMs(archivePaths[ai], "*", files, dateTimes);
                for (int fi = 0; fi < files.Count; fi++)
                {
                    string f = files[fi];
                    // Do this first because it should be faster than a dictionary lookup
                    if (!f.ExtIsArchive()) continue;
                    // We do a ContainsKey check to keep behavior the same as previously. When we use
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
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExGet + "files in " + archivePaths[ai], ex);
            }
        }

        #endregion

        MergeNewArchiveFMs(fmArchivesAndDatesDict, instDirNameContext);

        int fmDataIniListCount = FMDataIniList.Count;
        var fmDataIniInstDirDict = new DictionaryI<FanMission>(fmDataIniListCount);
        for (int i = 0; i < fmDataIniListCount; i++)
        {
            FanMission fm = FMDataIniList[i];
            if (!fm.InstalledDir.IsEmpty() && !fmDataIniInstDirDict.ContainsKey(fm.InstalledDir))
            {
                fmDataIniInstDirDict.Add(fm.InstalledDir, fm);
            }
        }

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            if (gameIndex == GameIndex.TDM) continue;

            var curGameInstFMsList = perGameInstFMDirsItems[i];
            if (curGameInstFMsList.Count > 0)
            {
                MergeNewInstalledFMs(
                    curGameInstFMsList,
                    fmDataIniInstDirDict);
            }
        }

        SetArchiveNames(fmArchivesAndDatesDict, instDirNameContext);

        EnsureUniqueInstalledNames();

        // Super quick-n-cheap hack for perf: So we don't have to iterate the whole list looking for unscanned FMs.
        var fmsViewListUnscanned = new List<FanMission>(FMDataIniList.Count);

        AddTdmFMs(files, dateTimes);

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
        2022-07-25: The code is way less crazy-go-nuts now, maybe we should try to find this thing again.
        */
    }

    private static void AddTdmFMs(List<string> files, List<ExpandableDate_FromTicks> dateTimes)
    {
        string fmsPath = Config.GetFMInstallPath(GameIndex.TDM);
        if (fmsPath.IsEmpty() && FMDataIniListTDM.Count == 0) return;

        /*
        @TDM_CASE(FindFMs): We're using DictionaryI for installed dirs here at the moment.
        This is the right thing on Windows, but we need to think about what this would mean on Linux.
        Is it even possible to have both scenarios play nice with the list? Probably not...
        On Windows if we renamed the dir "iris" to "iriS" we'd still get a match and not add "iriS" to the list
        if "iris" already existed, but on Linux "iriS" could exist as an actual separate dir, so we would have
        to add it. But then our FMs list is not portable between Linux and Windows - if you took it back and forth,
        it would change/get screwed up.
        */
        var fmDataIniListTDM_Dict = new DictionaryI<FanMission>(FMDataIniListTDM.Count);
        foreach (FanMission fm in FMDataIniListTDM)
        {
            fmDataIniListTDM_Dict[fm.TDMInstalledDir] = fm;
        }

        if (Directory.Exists(fmsPath))
        {
            try
            {
                List<string> dirs = files;

                var filesPK4 = new List<string>();
                var filesZip = new List<string>();
                var dateTimesPK4 = new List<ExpandableDate_FromTicks>();
                var dateTimesZip = new List<ExpandableDate_FromTicks>();

                FastIO.GetDirsTopOnly_FMs(fmsPath, "*", dirs, dateTimes);
                FastIO.GetFilesTopOnly_FMs(fmsPath, "*.pk4", filesPK4, dateTimesPK4);
                FastIO.GetFilesTopOnly_FMs(fmsPath, "*.zip", filesZip, dateTimesZip);

                HashSetI dirsHash = dirs.ToHashSetI();

                var finalFilesList = new List<string>(dirs.Count + filesPK4.Count + filesZip.Count);
                var finalDateTimesList = new List<ExpandableDate_FromTicks>(dateTimes.Count + dateTimesPK4.Count + dateTimesZip.Count);

                finalFilesList.AddRange(dirs);
                finalDateTimesList.AddRange(dateTimes);

                for (int i = 0; i < filesPK4.Count; i++)
                {
                    string validTdmFMName = filesPK4[i].ConvertToValidTDMInternalName(".pk4");
                    if (dirsHash.Add(validTdmFMName))
                    {
                        finalFilesList.Add(validTdmFMName);
                        finalDateTimesList.Add(dateTimesPK4[i]);
                    }
                }

                for (int i = 0; i < filesZip.Count; i++)
                {
                    string validTdmFMName = filesZip[i].ConvertToValidTDMInternalName(".zip");
                    if (dirsHash.Add(validTdmFMName))
                    {
                        finalFilesList.Add(validTdmFMName);
                        finalDateTimesList.Add(dateTimesZip[i]);
                    }
                }

                for (int fileIndex = 0; fileIndex < finalFilesList.Count; fileIndex++)
                {
                    string fmDir = finalFilesList[fileIndex];
                    if (TDM.IsValidTdmFM(fmsPath, fmDir))
                    {
                        if (fmDataIniListTDM_Dict.TryGetValue(fmDir, out FanMission dictFM))
                        {
                            dictFM.Installed = false;
                            dictFM.DateAdded ??= finalDateTimesList[fileIndex].DateTime;
                        }
                        else
                        {
                            FMDataIniListTDM.Add(new FanMission
                            {
                                Game = Game.TDM,
                                InstalledDir = fmDir,
                                TDMInstalledDir = fmDir,
                                Installed = false,
                                DateAdded = finalDateTimesList[fileIndex].DateTime
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(ErrorText.ExGet + "directories in " + fmsPath, ex);
            }
        }

        #region Ensure unique installed dir

        // We use an FM's installed dir as a unique tag, so it must still be unique among all FMs for all games.
        // We use the TDM-specific installed dir for actual TDM FM installed dir stuff.
        // If I'd have known I'd do TDM, I'd have planned for it better...
        // This is really, really janky and easy to make a mistake with the wrong installed dir version, meh.

        var fmDataIniList_Hash = new HashSetI(FMDataIniList.Count);
        foreach (FanMission fm in FMDataIniList)
        {
            fmDataIniList_Hash.Add(fm.InstalledDir);
        }

        foreach (FanMission fm in FMDataIniListTDM)
        {
            if (fmDataIniList_Hash.Contains(fm.InstalledDir))
            {
                for (int j = 0; ; j++)
                {
                    // Yeah, this'll never happen, but hey
                    // If it overflowed, oh well. You get what you deserve in that case.
                    if (j > 999) return;

                    /*
                    This results in eg. "some_fm_name(2)(3)" if we need to append more than once.
                    Enable the below to properly increment the number. Note that this doesn't really matter,
                    since the only thing we use the name for is a unique id and we still end up with that.
                    And there's no length limit on it either. So for perf we can leave it as is.
                    */
#if ENABLE_TDM_SMART_NAME_APPEND
                    System.Text.RegularExpressions.Match match;
                    System.Text.RegularExpressions.Group numberGroup;
                    if (fm.InstalledDir.Length >= 3 &&
                        (match = System.Text.RegularExpressions.Regex.Match(fm.InstalledDir, @"\(<?(?<Number>[0123456789]+)\)$")).Success &&
                        UInt_TryParseInv((numberGroup = match.Groups["Number"]).Value, out uint result)
                       )
                    {
                        fm.InstalledDir = fm.InstalledDir.Substring(0, numberGroup.Index - 1) + "(" + (result + 1) + ")";
                    }
                    else
                    {
                        // Conform to FMSel's numbering format
                        string append = "(" + (j + 2).ToStrInv() + ")";
                        fm.InstalledDir += append;
                    }
#else
                    // Conform to FMSel's numbering format
                    string append = "(" + (j + 2).ToStrInv() + ")";
                    fm.InstalledDir += append;
#endif

                    if (!fmDataIniList_Hash.Contains(fm.InstalledDir)) break;
                }
            }
        }

        #endregion
    }

    #region Set names

    private static void SetArchiveNames(DictionaryI<ExpandableDate_FromTicks> fmArchives, InstDirNameContext instDirNameContext)
    {
        DictionaryI<FanMission>? archivesDict = null;
        DictionaryI<FanMission> GetArchivesDict()
        {
            if (archivesDict == null)
            {
                archivesDict = new DictionaryI<FanMission>(FMDataIniList.Count);
                for (int i = 0; i < FMDataIniList.Count; i++)
                {
                    FanMission fm = FMDataIniList[i];
                    if (!fm.Archive.IsEmpty() && !archivesDict.ContainsKey(fm.Archive))
                    {
                        archivesDict.Add(fm.Archive, fm);
                    }
                }
            }
            return archivesDict;
        }

        LastResortLinkupContext? lastResortLinkupContext = null;

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

                // @PERF_TODO: Should we keep null here because it's faster? Is it faster? (tight loop)
                string? archiveName = null;
                // Skip the expensive archive name search if we're marked as having no archive
                if (!fm.NoArchive)
                {
                    lastResortLinkupContext ??= new LastResortLinkupContext(instDirNameContext);
                    archiveName = GetArchiveNameFromInstalledDir(fm, fmArchives, lastResortLinkupContext);
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
            FanMission fm = FMDataIniList[i];
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
                    string append = "(" + (j + 2).ToStrInv() + ")";

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

    private static void MergeNewArchiveFMs(DictionaryI<ExpandableDate_FromTicks> fmArchives, InstDirNameContext instDirNameContext)
    {
        int fmDataIniListCount = FMDataIniList.Count;
        var fmDataIniInstDirDict = new DictionaryI<FanMission>(fmDataIniListCount);
        var fmDataIniArchiveDict = new DictionaryI<FanMission>(fmDataIniListCount);
        for (int i = 0; i < fmDataIniListCount; i++)
        {
            FanMission fm = FMDataIniList[i];
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

            // Do the check that doesn't require allocations first
            if (fmDataIniArchiveDict.TryGetValue(archive, out FanMission fm))
            {
                fm.Archive = archive;
                if (fm.NoArchive)
                {
                    string? fmselInf = GetFMSelInfPath(fm);
                    if (!fmselInf.IsEmpty()) WriteFMSelInf(fm, fmselInf, archive);
                }
                fm.NoArchive = false;

                fm.DateAdded ??= item.Value.DateTime;

                if (fm.InstalledDir.IsEmpty())
                {
                    bool truncate = fm.Game != Game.Thief3;
                    fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(instDirNameContext, truncate);
                }
            }
            else if (fmDataIniInstDirDict.TryGetValue(archive.RemoveExtension(), out fm) ||
                     fmDataIniInstDirDict.TryGetValue(archive.ToInstDirNameFMSel(instDirNameContext, false), out fm) ||
                     fmDataIniInstDirDict.TryGetValue(archive.ToInstDirNameFMSel(instDirNameContext, true), out fm) ||
                     fmDataIniInstDirDict.TryGetValue(archive.ToInstDirNameNDL(instDirNameContext, true), out fm))
            {
                fm.DateAdded ??= item.Value.DateTime;

                if (fm.InstalledDir.IsEmpty())
                {
                    bool truncate = fm.Game != Game.Thief3;
                    fm.InstalledDir = fm.Archive.ToInstDirNameFMSel(instDirNameContext, truncate);
                }
            }
            else
            {
                FMDataIniList.Add(new FanMission
                {
                    Archive = archive,
                    InstalledDir = archive.ToInstDirNameFMSel(instDirNameContext, true),
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
            FanMission gFM = item.Value.FM;

            if (fmDataIniInstDirDict.TryGetValue(gFM.InstalledDir, out FanMission fm))
            {
                fm.Game = gFM.Game;
                fm.Installed = true;
                fm.DateAdded ??= item.Value.DateTime.DateTime;
            }
            else
            {
                FMDataIniList.Add(new FanMission
                {
                    InstalledDir = gFM.InstalledDir,
                    Game = gFM.Game,
                    Installed = true,
                    DateAdded = item.Value.DateTime.DateTime
                });
            }
        }
    }

    #endregion

    // @PERF_TODO: Keep returning null here for speed? Or even switch to a string/bool combo...?
    private static string? GetArchiveNameFromInstalledDir(FanMission fm, DictionaryI<ExpandableDate_FromTicks> archives, LastResortLinkupContext context)
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
            // @PERF_TODO: Does this actually even need to be run?
            // Now that I know the NoArchive value can be set back in MergeNewArchiveFMs, I wonder if this is
            // wholly or at least partially unnecessary. If we don't have an archive name by this point, do
            // we therefore already know this is not going to find anything?
            bool truncate = fm.Game != Game.Thief3;

            if (!context.GetInstDirFMSelToArchives(archives, truncate).TryGetValue(fm.InstalledDir, out string? tryArchive) &&
                !context.GetInstDirNDLTruncatedToArchives(archives).TryGetValue(fm.InstalledDir, out tryArchive) &&
                !context.GetInstDirNameToArchives(archives).TryGetValue(fm.InstalledDir, out tryArchive) &&
                !context.GetInstDirNameFMSelToArchives_FromFMDataIni(truncate).TryGetValue(fm.InstalledDir, out tryArchive) &&
                !context.GetInstDirNDLTruncatedToArchives_FromFMDataIni().TryGetValue(fm.InstalledDir, out tryArchive) &&
                !context.GetInstDirToArchives_FromFMDataIni().TryGetValue(fm.InstalledDir, out tryArchive))
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

        if (!TryReadAllLines(fmselInf!, out var lines))
        {
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

    // TODO(FindFMs.GetFMSelInfPath()):
    // We should be using FMIsReallyInstalled() here, but that one swallows exceptions where this one doesn't.
    // Keeping behavior identicality by leaving it as-is for now, but we should look into this!
    private static string? GetFMSelInfPath(FanMission fm)
    {
        if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex)) return null;

        string fmInstPath = Config.GetFMInstallPath(gameIndex);

        return fmInstPath.IsEmpty() ? null : Path.Combine(fmInstPath, fm.InstalledDir, Paths.FMSelInf);
    }

    private static void WriteFMSelInf(FanMission fm, string path, string archiveName)
    {
        try
        {
            using var sw = new StreamWriter(path);
            sw.WriteLine("Name=" + fm.InstalledDir);
            sw.WriteLine("Archive=" + archiveName);
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExWrite + path, ex);
        }
    }

    private static void BuildViewList(
        DictionaryI<ExpandableDate_FromTicks> fmArchivesDict,
        DictionaryI<InstDirValueData>[] perGameInstalledFMDirsItems,
        List<FanMission> fmsViewListUnscanned)
    {
        FMsViewList.Capacity = FMDataIniList.Count + FMDataIniListTDM.Count;

        bool?[] boolsList = new bool?[SupportedGameCount];

        static bool NotInPerGameList(FanMission fm, bool?[] notInList, DictionaryI<InstDirValueData>[] list, bool useBool)
        {
            if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex)) return false;
            int intGame = (int)gameIndex;

            if (!useBool)
            {
                return (bool)(notInList[intGame] = !list[intGame].ContainsKey(fm.InstalledDir));
            }
            else
            {
                return notInList[intGame] ?? !list[intGame].ContainsKey(fm.InstalledDir);
            }
        }

        var viewListHash = new HashSetI(FMDataIniList.Count);
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

            // Fix: we can have duplicate archive names if the installed dir is different, so cull them
            // out of the view list at least.
            // (This used to get done as an accidental side effect of the ContainsIRemoveFirst() call)
            // We shouldn't have duplicate archives, but importing might add different installed dirs...
            if (!fm.Archive.IsEmpty())
            {
                if (!viewListHash.Contains(fm.Archive))
                {
                    viewListHash.Add(fm.Archive);
                }
                else
                {
                    continue;
                }
            }

            // Perf so we don't have to iterate the list again later
            if (fm.NeedsScan()) fmsViewListUnscanned.Add(fm);

            fm.Title =
                !fm.Title.IsEmpty() ? fm.Title :
                !fm.Archive.IsEmpty() ? fm.Archive.RemoveExtension() :
                fm.InstalledDir;
            fm.CommentSingleLine = fm.Comment.FromRNEscapes().ToSingleLineComment(100);

            FMTags.AddTagsToFMAndGlobalList(fm.TagsString, fm.Tags);

            FMsViewList.Add(fm);
        }

        string tdmFMsPath = Config.GetFMInstallPath(GameIndex.TDM);
        if (!tdmFMsPath.IsEmpty() || FMDataIniListTDM.Count > 0)
        {
            DictionaryI<string> pk4ConvertedNamesHash = TDM.GetTDMBaseFMsDirPK4sConverted();

            for (int i = 0; i < FMDataIniListTDM.Count; i++)
            {
                FanMission fm = FMDataIniListTDM[i];

                string fmInstDir = !tdmFMsPath.IsEmpty()
                    ? Path.Combine(tdmFMsPath, fm.TDMInstalledDir)
                    : "";

                if (fmInstDir.IsEmpty() ||
                    (!Directory.Exists(fmInstDir) &&
                     !pk4ConvertedNamesHash.ContainsKey(fm.TDMInstalledDir)))
                {
                    fm.MarkedUnavailable = true;
                }
                else if (fm.NeedsScan())
                {
                    fmsViewListUnscanned.Add(fm);
                }

                fm.Title =
                    !fm.Title.IsEmpty() ? fm.Title :
                    !fm.Archive.IsEmpty() ? fm.Archive.RemoveExtension() :
                    fm.InstalledDir;
                fm.CommentSingleLine = fm.Comment.FromRNEscapes().ToSingleLineComment(100);

                FMTags.AddTagsToFMAndGlobalList(fm.TagsString, fm.Tags);

                FMsViewList.Add(fm);
            }
        }

        FMDataIniList.TrimExcess();
        FMDataIniListTDM.TrimExcess();
        FMsViewList.TrimExcess();

#if DateAccTest
        Ini.ReadDateAccuracyFile();
#endif
    }
}

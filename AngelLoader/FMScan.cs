#define TIMING_TEST
//#define INDIVIDUAL_FM_TIMING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
#if INDIVIDUAL_FM_TIMING
using System.Linq;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AL_Common.FastZipReader;
using AngelLoader.DataClasses;
using FMScanner;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;
using Game = AngelLoader.GameSupport.Game;
using ScannerGame = FMScanner.Game;

namespace AngelLoader;

internal static class FMScan
{
#if TIMING_TEST
    private static readonly Stopwatch _timingTestStopWatch = new();

    private static void StartTiming()
    {
        _timingTestStopWatch.Restart();
    }

    private static void StopTimingAndPrintResult()
    {
        _timingTestStopWatch.Stop();
        // ReSharper disable RedundantNameQualifier
        Trace.WriteLine("Scan: " + _timingTestStopWatch.Elapsed);
        Core.Dialogs.ShowAlert("Scan: " + _timingTestStopWatch.Elapsed, "", MBoxIcon.None);
    }
#endif

    private static CancellationTokenSource _scanCts = new();
    private static void CancelToken()
    {
        // Multithreaded scans can in certain cases take a significant amount of time to cancel, so inform the
        // user that we're trying our best here.
        _scanCts.CancelIfNotDisposed();
        Core.View.Invoke(ShowCancelingScanMessage);
    }

    private static void ShowCancelingScanMessage()
    {
        if (Core.View.ProgressBoxVisible())
        {
            Core.View.SetProgressBoxState_Single(message1: LText.ProgressBox.CancelingScan);
        }
    }

    /// <summary>
    /// Scans a list of FMs using the specified scan options. Pass null for default scan options.
    /// <para>
    /// To scan a single FM, just pass a list with a single FM in it. ScanFM() has been removed because it
    /// just added an extra await for only a very tiny convenience.
    /// </para>
    /// </summary>
    /// <param name="fmsToScan"></param>
    /// <param name="scanOptions">Pass null for default scan options.</param>
    /// <param name="scanFullIfNew"></param>
    /// <param name="suppressSingleFMProgressBoxIfFast">
    /// Suppresses showing the progress box for a single-FM scan
    /// if the FM scan is of a type deemed "fast" (will complete reliably quickly).
    /// </param>
    /// <param name="setForceReCacheReadmes"></param>
    /// <param name="scanMessage"></param>
    /// <returns></returns>
    internal static async Task<bool> ScanFMs(
        NonEmptyList<FanMission> fmsToScan,
        ScanOptions? scanOptions = null,
        bool scanFullIfNew = false,
        bool suppressSingleFMProgressBoxIfFast = false,
        bool setForceReCacheReadmes = false,
        string? scanMessage = null)
    {
        scanOptions ??= GetDefaultScanOptions();

        bool scanningOne = fmsToScan.Single;

        // Cache once globally, not once per-thread (allocation reduction)
        string reportCachedString = "";

        Stopwatch reportThrottleSW = new();

        // IMPORTANT(Multithreaded scan):
        // The progress object MUST be constructed here on the UI thread! This is what allows it to report smoothly
        // and without the endless and unsolvable issues we get when we merely invoke to the UI thread from the
        // report function.
        var progress = new Progress<ProgressReport>(ReportProgress);

        // Show on UI thread to prevent a small gap between when the thread starts (freeing the UI thread) and
        // when we show the progress box (blocking refreshes). Theoretically a refresh could sneak in through
        // the gap.
        #region Show progress box or block UI thread

        static void ShowProgressBox(bool suppressShow)
        {
            Core.View.SetProgressBoxState_Single(
                visible: !suppressShow,
                message1: LText.ProgressBox.Scanning,
                message2: LText.ProgressBox.PreparingToScanFMs,
                percent: 0,
                progressType: ProgressType.Determinate,
                cancelAction: CancelToken
            );
        }

        if (suppressSingleFMProgressBoxIfFast && scanningOne)
        {
            // Just use a cheap check and throw up the progress box for "slow to scan" files, otherwise not.
            // Not as nice as the timer method, but that can cause race conditions I don't know how to fix,
            // so whatever.
            if (!fmsToScan[0].IsFastToScan())
            {
                ShowProgressBox(suppressShow: false);
            }
            else
            {
                // Block user input to the form to mimic the UI thread being blocked, because we're async
                // here
                Core.View.Block(true);
                // Doesn't actually show the box, but shows the meter on the taskbar I guess?
                ShowProgressBox(suppressShow: true);
            }
        }
        else
        {
            ShowProgressBox(suppressShow: false);
        }

        #endregion

        try
        {
            // Shove the whole thing into a thread, otherwise the progress box will be half-blocked still somehow
            bool result = await Task.Run(async () =>
            {
                try
                {
                    #region Init

                    _scanCts = _scanCts.Recreate();

                    var fms = new List<FMToScan>(fmsToScan.Count);

                    // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                    // cause then it will hit the actual disk rather than just going through a list of paths in
                    // memory
                    List<string> archivePaths = FMArchives.GetFMArchivePaths();

                    if (_scanCts.IsCancellationRequested) return false;

                    #endregion

                    #region Filter out invalid FMs from scan list

                    // Safety net to guarantee that the in and out lists will have the same count and order
                    var fmsToScanFiltered = new List<FanMission>(fmsToScan.Count);

                    bool tdmDataRequired = false;

                    bool[] fmInstalledDirsRequired = new bool[SupportedGameCount];
                    bool atLeastOneSolidArchiveInSet = false;
                    HashSetI usedArchivePaths = new(Config.FMArchivePaths.Count);

                    for (int i = 0; i < fmsToScan.Count; i++)
                    {
                        FanMission fm = fmsToScan[i];

                        if (fm.MarkedUnavailable) continue;

                        string fmArchivePath;

                        if (_scanCts.IsCancellationRequested) return false;

                        if (!fm.Archive.IsEmpty() &&
                            !(fmArchivePath = FMArchives.FindFirstMatch(fm.Archive, archivePaths, out string archiveDirectoryFullPath)).IsEmpty())
                        {
                            if (!archiveDirectoryFullPath.IsEmpty())
                            {
                                usedArchivePaths.Add(archiveDirectoryFullPath);
                            }
                            bool needsReadmesCachedDuringScan = fm.NeedsReadmesCachedDuringScan();
                            fmsToScanFiltered.Add(fm);
                            fms.Add(new FMToScan
                            (
                                path: fmArchivePath,
                                forceFullScan: scanFullIfNew && !fm.MarkedScanned,
                                cachePath: needsReadmesCachedDuringScan
                                    ? Path.Combine(Paths.FMsCache, fm.RealInstalledDir)
                                    : "",
                                // TDM is folder-only
                                isTDM: false,
                                displayName: fm.Archive,
                                isArchive: true,
                                originalIndex: fms.Count
                            ));
                            if (needsReadmesCachedDuringScan)
                            {
                                atLeastOneSolidArchiveInSet = true;
                            }
                        }
                        else if (fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
                        {
                            string fmInstalledPath = Config.GetFMInstallPath(gameIndex);
                            if (!fmInstalledPath.IsEmpty())
                            {
                                fmsToScanFiltered.Add(fm);
                                fms.Add(new FMToScan
                                (
                                    path: Path.Combine(fmInstalledPath, fm.RealInstalledDir),
                                    forceFullScan: scanFullIfNew && !fm.MarkedScanned,
                                    isTDM: fm.Game == Game.TDM,
                                    displayName: fm.RealInstalledDir,
                                    isArchive: false,
                                    originalIndex: fms.Count
                                ));
                                if (fm.Game == Game.TDM) tdmDataRequired = true;
                                fmInstalledDirsRequired[(int)gameIndex] = true;
                            }
                        }

                        if (_scanCts.IsCancellationRequested) return false;
                    }

                    if (fmsToScanFiltered.Count == 0) return false;

                    #endregion

                    #region Run scanner

                    List<ScannedFMDataAndError> fmDataList;
                    try
                    {
                        Paths.CreateOrClearTempPath(TempPaths.FMScanner);
                        Paths.CreateOrClearTempPath(TempPaths.SevenZipList);

                        if (_scanCts.IsCancellationRequested) return false;

                        ScannerTDMContext tdmContext;
                        if (tdmDataRequired)
                        {
                            Core.View.SetProgressBoxState_Single(message2: LText.ProgressBox.RetrievingFMDataFromTDMServer);
                            tdmContext = await TDM.GetScannerTDMContext(_scanCts.Token);
                        }
                        else
                        {
                            tdmContext = new ScannerTDMContext(Config.GetFMInstallPath(GameIndex.TDM));
                        }

                        reportThrottleSW.Start();

#if TIMING_TEST
                        StartTiming();
#endif
                        reportCachedString =
                            LText.ProgressBox.ReportScanningBetweenNumAndTotal +
                            fms.Count.ToStrCur() +
                            LText.ProgressBox.ReportScanningLast;

                        // Don't take the substantial parallel loop overhead if it's just one FMs
                        if (fms.Count == 1)
                        {
                            using Scanner scanner = new(
                                sevenZipWorkingPath: Paths.SevenZipPath,
                                sevenZipExePath: Paths.SevenZipExe,
                                fullScanOptions: GetDefaultScanOptions(),
                                readOnlyDataContext: new ReadOnlyDataContext(),
                                tdmContext: tdmContext);

                            fmDataList = await scanner.ScanAsync(
                                fms: fms,
                                tempPath: Paths.FMScannerTemp,
                                scanOptions: scanOptions,
                                progress: progress,
                                cancellationToken: _scanCts.Token);
                        }
                        else
                        {
                            ThreadingData threadingData = GetLowestCommonThreadingData(
                                GetScanRelevantPaths(usedArchivePaths, fmInstalledDirsRequired, atLeastOneSolidArchiveInSet)
                            );

                            fmDataList = Scanner.ScanThreaded(
                                sevenZipWorkingPath: Paths.SevenZipPath,
                                sevenZipExePath: Paths.SevenZipExe,
                                fullScanOptions: GetDefaultScanOptions(),
                                tdmContext: tdmContext,
                                threadCount: GetThreadCountForParallelOperation(fms.Count, threadingData.Threads),
                                fms: fms,
                                tempPath: Paths.FMScannerTemp,
                                scanOptions: scanOptions,
                                progress: progress,
                                cancellationToken: _scanCts.Token);
                        }

#if INDIVIDUAL_FM_TIMING
                        List<Scanner.TimingData> timingDataList = new();
                        timingDataList.AddRange(Scanner.TimingDataList);

                        timingDataList = timingDataList.OrderBy(static x => Path.GetFileName(x.Path)).ToList();

                        using (var sw = new StreamWriter(@"C:\al_7z_scan_timings.txt"))
                        {
                            foreach (var item in timingDataList)
                            {
                                sw.WriteLine(item.Time + "=" + Path.GetFileName(item.Path));
                            }
                        }
#endif

                        Core.View.SetProgressPercent(100);
                    }
                    catch (OperationCanceledException)
                    {
                        CleanupAfterCancel();
                        return false;
                    }

                    if (fmDataList.Count > 0)
                    {
                        // @MEM(Scanner/unsupported compression errors):
                        // We're looping through the whole thing always just to see if there are errors!
                        // The scanner should just return a list and we can skip this if it's empty.
                        bool errors = false;
                        bool otherErrors = false;
                        var unsupportedCompressionErrors = new List<(FMToScan FM, ScannedFMDataAndError ScannedFMDataAndError)>();

                        for (int i = 0; i < fmsToScanFiltered.Count; i++)
                        {
                            ScannedFMDataAndError item = fmDataList[i];
                            if (item.Fen7zResult != null ||
                                item.Exception != null ||
                                !item.ErrorInfo.IsEmpty())
                            {
                                if (item.Exception is ZipCompressionMethodException)
                                {
                                    unsupportedCompressionErrors.Add((fms[i], item));
                                }
                                else
                                {
                                    otherErrors = true;
                                }

                                errors = true;
                            }
                        }

                        if (errors)
                        {
                            // @BetterErrors(FMScan): We should maybe have an option to cancel the scan.
                            // So that we don't set the data on the FMs if it's going to be corrupt or wrong.
                            if (unsupportedCompressionErrors.Count > 0)
                            {
                                if (unsupportedCompressionErrors.Count == 1)
                                {
                                    Core.Dialogs.ShowError(
                                        "The zip archive '"
                                        + unsupportedCompressionErrors[0].FM.Path +
                                        "' contains one or more files compressed with an unsupported compression method. " +
                                        "Only the DEFLATE method is supported. Try manually extracting and re-creating the zip archive.");
                                }
                                else
                                {
                                    string msg =
                                        "One or more zip archives contain files compressed with unsupported compression methods. " +
                                        $"Only the DEFLATE method is supported. Try manually extracting and re-creating the zip archives.{NL}{NL}" +
                                        $"The following zip archives produced this error:{NL}{NL}";

                                    for (int i = 0; i < Math.Min(unsupportedCompressionErrors.Count, 10); i++)
                                    {
                                        msg += unsupportedCompressionErrors[i].FM.Path + $"{NL}";
                                    }

                                    if (unsupportedCompressionErrors.Count > 10)
                                    {
                                        msg += "[See the log for the rest]";
                                    }

                                    if (otherErrors)
                                    {
                                        msg += $"{NL}{NL}In addition, one or more other errors occurred. See the log for details.";
                                    }

                                    Core.Dialogs.ShowError(msg);
                                }
                            }
                            else
                            {
                                Core.Dialogs.ShowError(
                                    "One or more errors occurred while scanning. See the log for details.");
                            }
                        }
                    }

                    #endregion

                    #region Copy scanned data to FMs

                    StringBuilder tagsToStringSB = new(FMTags.TagsToStringSBInitialCapacity);

                    for (int i = 0; i < fmsToScanFiltered.Count; i++)
                    {
                        ScannedFMDataAndError scannedFMDataAndError = fmDataList[i];
                        ScannedFMData? scannedFM = scannedFMDataAndError.ScannedFMData;

                        #region Checks

                        if (scannedFM == null)
                        {
                            // We need to return fail for scanning one, else we get into an infinite loop because of
                            // a refresh that gets called in that case
                            if (scanningOne)
                            {
                                Log($"(one) scanned FM was null. FM was:{NL}" +
                                    "Archive: " + fmsToScanFiltered[0].DisplayArchive + $"{NL}" +
                                    "InstalledDir: " + fmsToScanFiltered[0].InstalledDir + $"{NL}" +
                                    "TDMInstalledDir (if applicable): " + fmsToScanFiltered[0].TDMInstalledDir);
                                return false;
                            }
                            continue;
                        }

                        #endregion

                        FanMission fm = fmsToScanFiltered[i];

                        #region Set FM fields

                        if (scannedFMDataAndError.NeedsHtmlRefExtract)
                        {
                            fm.ForceReadmeReCacheAlways = true;
                        }

                        bool gameSup = scannedFM.Game != ScannerGame.Unsupported;

                        if (fms[i].ForceFullScan || scanOptions.ScanTitle)
                        {
                            fm.Title =
                                !scannedFM.Title.IsEmpty() ? scannedFM.Title
                                : scannedFM.ArchiveName.ExtIsArchive() ? scannedFM.ArchiveName.RemoveExtension()
                                : scannedFM.ArchiveName;

                            if (gameSup)
                            {
                                fm.AltTitles.ClearAndAddTitleAndAltTitles(fm.Title, scannedFM.AlternateTitles);
                            }
                            else
                            {
                                fm.AltTitles.Clear();
                            }
                        }

                        if (fms[i].ForceFullScan || scanOptions.ScanSize)
                        {
                            fm.SizeBytes = gameSup ? scannedFM.Size ?? 0 : 0;
                        }
                        if (fms[i].ForceFullScan || scanOptions.ScanReleaseDate)
                        {
                            fm.ReleaseDate.DateTime = gameSup ? scannedFM.LastUpdateDate : null;
                        }
                        if (fms[i].ForceFullScan || scanOptions.ScanCustomResources)
                        {
                            #region This exact setup is needed to get identical results to the old method. Don't change.
                            // We don't scan custom resources for Thief 3, so they should never be set in that case.
                            if (gameSup &&
                                scannedFM.Game != ScannerGame.Thief3 &&
                                scannedFM.Game != ScannerGame.TDM)
                            {
                                fm.SetResource(CustomResources.Map, scannedFM.HasMap == true);
                                fm.SetResource(CustomResources.Automap, scannedFM.HasAutomap == true);
                                fm.SetResource(CustomResources.Scripts, scannedFM.HasCustomScripts == true);
                                fm.SetResource(CustomResources.Textures, scannedFM.HasCustomTextures == true);
                                fm.SetResource(CustomResources.Sounds, scannedFM.HasCustomSounds == true);
                                fm.SetResource(CustomResources.Objects, scannedFM.HasCustomObjects == true);
                                fm.SetResource(CustomResources.Creatures, scannedFM.HasCustomCreatures == true);
                                fm.SetResource(CustomResources.Motions, scannedFM.HasCustomMotions == true);
                                fm.SetResource(CustomResources.Movies, scannedFM.HasMovies == true);
                                fm.SetResource(CustomResources.Subtitles, scannedFM.HasCustomSubtitles == true);
                                fm.ResourcesScanned = true;
                            }
                            else
                            {
                                fm.ResourcesScanned = false;
                            }
                            #endregion
                        }

                        if (fms[i].ForceFullScan || scanOptions.ScanAuthor)
                        {
                            fm.Author = gameSup ? scannedFM.Author : "";
                        }

                        if (fms[i].ForceFullScan || scanOptions.ScanGameType)
                        {
                            fm.Game = ScannerGameToGame(scannedFM.Game);
                        }

                        if (fms[i].ForceFullScan || scanOptions.ScanTags)
                        {
                            string tagsString = gameSup ? scannedFM.TagsString : "";

                            // Don't clear the tags, because the user could have added a bunch and we should only
                            // add to those, not overwrite them
                            if (gameSup)
                            {
                                // Don't rebuild global tags for every FM; do it only once at the end
                                FMTags.AddTagsToFM(fm, tagsString, rebuildGlobalTags: false, tagsToStringSB);
                            }
                        }

                        if (fms[i].ForceFullScan || scanOptions.ScanMissionCount)
                        {
                            fm.MisCount = gameSup ? scannedFM.MissionCount ?? -1 : -1;

                            if (gameSup && scannedFM.MissionCount is > 1)
                            {
                                FMTags.AddTagsToFM(fm, "misc:campaign", rebuildGlobalTags: false);
                            }
                        }

                        fm.MarkedScanned = true;
                        if (setForceReCacheReadmes) fm.ForceReadmeReCache = true;

                        #endregion
                    }

                    #endregion

                    FMTags.RebuildGlobalTags();

                    Ini.WriteFullFMDataIni();
                }
                catch (Exception ex)
                {
                    Log(ex: ex);
                    string message = scanningOne
                        ? LText.AlertMessages.Scan_ExceptionInScanOne
                        : LText.AlertMessages.Scan_ExceptionInScanMultiple;
                    Core.Dialogs.ShowError(message);
                    return false;
                }

                return true;
            });

            return result;
        }
        finally
        {
            _scanCts.Dispose();
            Core.View.Block(false);
            Core.View.HideProgressBox();
#if TIMING_TEST
            StopTimingAndPrintResult();
#endif
        }

        #region Local functions

        static ScanOptions GetDefaultScanOptions() => ScanOptions.FalseDefault(
            scanTitle: true,
            scanAuthor: true,
            scanGameType: true,
            scanCustomResources: true,
            scanSize: true,
            scanReleaseDate: true,
            scanTags: true,
            scanMissionCount: true);

        void ReportProgress(ProgressReport pr)
        {
            if (_scanCts.IsCancellationRequested)
            {
                ShowCancelingScanMessage();
                return;
            }

            int percent = GetPercentFromValue_Int(scanningOne ? 0 : pr.FMNumber - 1, pr.FMsCount);

            /*
            @MT_TASK_NOTE: Necessary if we have multiple threads to allow UI time to catch up.
            Not necessary if we're on one thread, but does no harm either.
            @MT_TASK_NOTE(Scanner 7z reporting):
             >1 or >2 thread 7z scans don't report smoothly, probably due to the out-of-order nature of it.
             We could try the multi item progress box here, just to see?
             2024/11/18: We tried that and there are other problems with UI updating interval and whatever else.
             Even though for 7z the percentage display is suboptimal, this is overall the least worst thing for
             now...
            */
            if (scanningOne ||
                pr.FMNumber is 0 or 1 ||
                reportThrottleSW.ElapsedMilliseconds > 4)
            {
                Core.View.SetProgressBoxState_Single(
                    message1:
                    scanMessage ??
                    LText.ProgressBox.ReportScanningFirst +
                    pr.FMNumber.ToStrCur() +
                    reportCachedString,
                    message2:
                    pr.FMName,
                    percent: percent
                );

                reportThrottleSW.Restart();
            }
        }

        static void CleanupAfterCancel()
        {
#if TIMING_TEST
            Trace.WriteLine("Canceled");
#endif
            Paths.CreateOrClearTempPath(TempPaths.FMScanner);
            Paths.CreateOrClearTempPath(TempPaths.SevenZipList);
        }

        #endregion
    }

    internal static Task ScanNewFMs(NonEmptyList<FanMission> newFMs)
    {
        return ScanFMs(newFMs,
            ScanOptions.FalseDefault(scanGameType: true),
            scanFullIfNew: true);
    }

    private static ScanOptions? GetScanOptionsFromDialog(bool selected)
    {
        (bool accepted, ScanOptions scanOptions, bool noneSelected) =
            Core.View.ShowScanAllFMsWindow(selected);

        if (!accepted) return null;

        if (noneSelected)
        {
            Core.Dialogs.ShowAlert(LText.ScanAllFMsBox.NothingWasScanned, LText.AlertMessages.Alert);
            return null;
        }

        return scanOptions;
    }

    internal static async Task ScanAllFMs()
    {
        if (!NonEmptyList<FanMission>.TryCreateFrom_Ref(FMsViewList, out var fmsToScan))
        {
            return;
        }

        ScanOptions? scanOptions = GetScanOptionsFromDialog(selected: false);
        if (scanOptions == null) return;

        if (await ScanFMs(fmsToScan, scanOptions))
        {
            await Core.View.SortAndSetFilter(forceDisplayFM: true);
        }
    }

    internal static async Task ScanSelectedFMs()
    {
        if (!NonEmptyList<FanMission>.TryCreateFrom_Ref(Core.View.GetSelectedFMs_InOrder_List(), out var fmsToScan))
        {
            return;
        }

        if (fmsToScan.Single)
        {
            if (await ScanFMs(fmsToScan, suppressSingleFMProgressBoxIfFast: true, setForceReCacheReadmes: true))
            {
                Core.View.RefreshFM(fmsToScan[0]);
            }
        }
        else
        {
            ScanOptions? scanOptions = GetScanOptionsFromDialog(selected: true);
            if (scanOptions == null) return;

            if (await ScanFMs(fmsToScan, scanOptions, setForceReCacheReadmes: true))
            {
                // @MULTISEL(Scan selected FMs): Do we want to sort and set filter here too?
                // Because we might scan FMs and they end up being something that's filtered out?
                // We don't do it with one, I guess cause of the "don't refresh the list for a single change"
                // rule. But does this count?
                Core.View.RefreshAllSelectedFMs_Full();
            }
        }
    }
}

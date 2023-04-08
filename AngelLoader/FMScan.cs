using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AL_Common;
using AL_Common.FastZipReader;
using AngelLoader.DataClasses;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

internal static class FMScan
{
    private static CancellationTokenSource _scanCts = new();
    private static void CancelToken() => _scanCts.CancelIfNotDisposed();

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
    /// <param name="hideBoxIfZip"></param>
    /// <param name="setForceReCacheReadmes"></param>
    /// <param name="scanMessage"></param>
    /// <returns></returns>
    internal static Task<bool> ScanFMs(
        List<FanMission> fmsToScan,
        FMScanner.ScanOptions? scanOptions = null,
        bool scanFullIfNew = false,
        bool hideBoxIfZip = false,
        bool setForceReCacheReadmes = false,
        string? scanMessage = null)
    {
        // Shove the whole thing into a thread, otherwise the progress box will be half-blocked still somehow
        return Task.Run(async () =>
        {
            #region Local functions

            static FMScanner.ScanOptions GetDefaultScanOptions() => FMScanner.ScanOptions.FalseDefault(
                scanTitle: true,
                scanAuthor: true,
                scanGameType: true,
                scanCustomResources: true,
                scanSize: true,
                scanReleaseDate: true,
                scanTags: true,
                scanMissionCount: true);

            void ReportProgress(FMScanner.ProgressReport pr) => Core.View.SetProgressBoxState_Single(
                message1:
                scanMessage ??
                (LText.ProgressBox.ReportScanningFirst +
                 pr.FMNumber +
                 (pr.CachedString ??=
                     (LText.ProgressBox.ReportScanningBetweenNumAndTotal +
                      pr.FMsTotal +
                      LText.ProgressBox.ReportScanningLast))),
                message2:
                // @MEM: We could get really clever and not have to do this work if we cached stuff from earlier
                pr.FMName.ExtIsArchive()
                    ? pr.FMName.GetFileNameFast()
                    : pr.FMName.GetDirNameFast(),
                percent: pr.Percent
            );

            #endregion

            scanOptions ??= GetDefaultScanOptions();

            if (fmsToScan.Count == 0) return false;

            bool scanningOne = fmsToScan.Count == 1;

            try
            {
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

                if (hideBoxIfZip && scanningOne)
                {
                    // Just use a cheap check and throw up the progress box for .7z files, otherwise not. Not as
                    // nice as the timer method, but that can cause race conditions I don't know how to fix, so
                    // whatever.
                    if (fmsToScan[0].Archive.ExtIs7z())
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

                #region Init

                _scanCts = _scanCts.Recreate();

                var fms = new List<FMScanner.FMToScan>(fmsToScan.Count);

                // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                // cause then it will hit the actual disk rather than just going through a list of paths in
                // memory
                List<string> archivePaths = FMArchives.GetFMArchivePaths();

                if (_scanCts.IsCancellationRequested) return false;

                #endregion

                #region Filter out invalid FMs from scan list

                // Safety net to guarantee that the in and out lists will have the same count and order
                var fmsToScanFiltered = new List<FanMission>(fmsToScan.Count);

                for (int i = 0; i < fmsToScan.Count; i++)
                {
                    FanMission fm = fmsToScan[i];

                    if (fm.MarkedUnavailable) continue;

                    string fmArchivePath = FMArchives.FindFirstMatch(fm.Archive, archivePaths);

                    if (_scanCts.IsCancellationRequested) return false;

                    if (!fm.Archive.IsEmpty() && !fmArchivePath.IsEmpty())
                    {
                        fmsToScanFiltered.Add(fm);
                        fms.Add(new FMScanner.FMToScan
                        {
                            Path = fmArchivePath,
                            ForceFullScan = scanFullIfNew && !fm.MarkedScanned,
                            CachePath = fm.Archive.ExtIs7z()
                                ? Path.Combine(Paths.FMsCache, fm.InstalledDir)
                                : ""
                        });
                    }
                    else if (fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
                    {
                        string fmInstalledPath = Config.GetFMInstallPath(gameIndex);
                        if (!fmInstalledPath.IsEmpty())
                        {
                            fmsToScanFiltered.Add(fm);
                            fms.Add(new FMScanner.FMToScan
                            {
                                Path = Path.Combine(fmInstalledPath, fm.InstalledDir),
                                ForceFullScan = scanFullIfNew && !fm.MarkedScanned
                            });
                        }
                    }

                    if (_scanCts.IsCancellationRequested) return false;
                }

                if (fmsToScanFiltered.Count == 0) return false;

                #endregion

                #region Run scanner

                List<FMScanner.ScannedFMDataAndError>? fmDataList = null;
                try
                {
                    var progress = new Progress<FMScanner.ProgressReport>(ReportProgress);

                    Paths.CreateOrClearTempPath(Paths.FMScannerTemp);
                    Paths.CreateOrClearTempPath(Paths.SevenZipListTemp);

                    if (_scanCts.IsCancellationRequested) return false;

                    using var scanner = new FMScanner.Scanner(Paths.SevenZipExe)
                    {
                        FullScanOptions = GetDefaultScanOptions()
                    };
                    fmDataList = await scanner.ScanAsync(fms, Paths.FMScannerTemp, scanOptions, progress, _scanCts.Token);
                }
                catch (OperationCanceledException)
                {
                    Paths.CreateOrClearTempPath(Paths.FMScannerTemp);
                    Paths.CreateOrClearTempPath(Paths.SevenZipListTemp);
                    return false;
                }
                finally
                {
                    if (fmDataList != null)
                    {
                        // @MEM(Scanner/unsupported compression errors):
                        // We're looping through the whole thing always just to see if there are errors!
                        // The scanner should just return a list and we can skip this if it's empty.
                        bool errors = false;
                        bool otherErrors = false;
                        var unsupportedCompressionErrors = new List<(FMScanner.FMToScan FM, FMScanner.ScannedFMDataAndError ScannedFMDataAndError)>();

                        for (int i = 0; i < fmsToScanFiltered.Count; i++)
                        {
                            FMScanner.ScannedFMDataAndError item = fmDataList[i];
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
                                        "Only the DEFLATE method is supported. Try manually extracting and re-creating the zip archives.\r\n\r\n" +
                                        "The following zip archives produced this error:\r\n\r\n";

                                    for (int i = 0; i < Math.Min(unsupportedCompressionErrors.Count, 10); i++)
                                    {
                                        msg += unsupportedCompressionErrors[i].FM.Path + "\r\n";
                                    }

                                    if (unsupportedCompressionErrors.Count > 10)
                                    {
                                        msg += "[See the log for the rest]";
                                    }

                                    if (otherErrors)
                                    {
                                        msg += "\r\n\r\nIn addition, one or more other errors occurred. See the log for details.";
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
                }

                #endregion

                #region Copy scanned data to FMs

                for (int i = 0; i < fmsToScanFiltered.Count; i++)
                {
                    FMScanner.ScannedFMData? scannedFM = fmDataList[i].ScannedFMData;

                    #region Checks

                    if (scannedFM == null)
                    {
                        // We need to return fail for scanning one, else we get into an infinite loop because of
                        // a refresh that gets called in that case
                        if (scanningOne)
                        {
                            Log("(one) scanned FM was null. FM was:\r\n" +
                                "Archive: " + fmsToScanFiltered[0].Archive + "\r\n" +
                                "InstalledDir: " + fmsToScanFiltered[0].InstalledDir);
                            return false;
                        }
                        continue;
                    }

                    FanMission sel = fmsToScanFiltered[i];

                    #endregion

                    #region Set FM fields

                    bool gameSup = scannedFM.Game != FMScanner.Game.Unsupported;

                    if (fms[i].ForceFullScan || scanOptions.ScanTitle)
                    {
                        sel.Title =
                            !scannedFM.Title.IsEmpty() ? scannedFM.Title
                            : scannedFM.ArchiveName.ExtIsArchive() ? scannedFM.ArchiveName.RemoveExtension()
                            : scannedFM.ArchiveName;

                        if (gameSup)
                        {
                            sel.AltTitles.ClearAndAdd(sel.Title);
                            sel.AltTitles.AddRange(scannedFM.AlternateTitles);
                        }
                        else
                        {
                            sel.AltTitles.Clear();
                        }
                    }

                    if (fms[i].ForceFullScan || scanOptions.ScanSize)
                    {
                        sel.SizeBytes = gameSup ? scannedFM.Size ?? 0 : 0;
                    }
                    if (fms[i].ForceFullScan || scanOptions.ScanReleaseDate)
                    {
                        sel.ReleaseDate.DateTime = gameSup ? scannedFM.LastUpdateDate : null;
                    }
                    if (fms[i].ForceFullScan || scanOptions.ScanCustomResources)
                    {
                        #region This exact setup is needed to get identical results to the old method. Don't change.
                        // We don't scan custom resources for Thief 3, so they should never be set in that case.
                        if (gameSup && scannedFM.Game != FMScanner.Game.Thief3)
                        {
                            SetFMResource(sel, CustomResources.Map, scannedFM.HasMap == true);
                            SetFMResource(sel, CustomResources.Automap, scannedFM.HasAutomap == true);
                            SetFMResource(sel, CustomResources.Scripts, scannedFM.HasCustomScripts == true);
                            SetFMResource(sel, CustomResources.Textures, scannedFM.HasCustomTextures == true);
                            SetFMResource(sel, CustomResources.Sounds, scannedFM.HasCustomSounds == true);
                            SetFMResource(sel, CustomResources.Objects, scannedFM.HasCustomObjects == true);
                            SetFMResource(sel, CustomResources.Creatures, scannedFM.HasCustomCreatures == true);
                            SetFMResource(sel, CustomResources.Motions, scannedFM.HasCustomMotions == true);
                            SetFMResource(sel, CustomResources.Movies, scannedFM.HasMovies == true);
                            SetFMResource(sel, CustomResources.Subtitles, scannedFM.HasCustomSubtitles == true);
                            sel.ResourcesScanned = true;
                        }
                        else
                        {
                            sel.ResourcesScanned = false;
                        }
                        #endregion
                    }

                    if (fms[i].ForceFullScan || scanOptions.ScanAuthor)
                    {
                        sel.Author = gameSup ? scannedFM.Author : "";
                    }

                    if (fms[i].ForceFullScan || scanOptions.ScanGameType)
                    {
                        sel.Game = ScannerGameToGame(scannedFM.Game);
                    }

                    if (fms[i].ForceFullScan || scanOptions.ScanTags)
                    {
                        string tagsString = gameSup ? scannedFM.TagsString : "";

                        // Don't clear the tags, because the user could have added a bunch and we should only
                        // add to those, not overwrite them
                        if (gameSup)
                        {
                            // Don't rebuild global tags for every FM; do it only once at the end
                            FMTags.AddTagToFM(sel, tagsString, rebuildGlobalTags: false);
                        }
                    }

                    if (fms[i].ForceFullScan || scanOptions.ScanMissionCount)
                    {
                        sel.MisCount = gameSup ? scannedFM.MissionCount ?? -1 : -1;

                        if (gameSup && scannedFM.MissionCount is > 1)
                        {
                            FMTags.AddTagToFM(sel, "misc:campaign", rebuildGlobalTags: false);
                        }
                    }

                    sel.MarkedScanned = true;
                    if (setForceReCacheReadmes) sel.ForceReadmeReCache = true;

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
            finally
            {
                _scanCts.Dispose();
                Core.View.Block(false);
                Core.View.HideProgressBox();
            }

            return true;
        });
    }

    internal static Task ScanNewFMs(List<FanMission> fmsViewListUnscanned)
    {
        AssertR(fmsViewListUnscanned.Count > 0, nameof(fmsViewListUnscanned) + ".Count was 0");

        var fmsToScan = new List<FanMission>(fmsViewListUnscanned.Count);

        fmsToScan.AddRange(fmsViewListUnscanned);
        // Just in case
        fmsViewListUnscanned.Clear();

        return ScanFMs(fmsToScan,
            FMScanner.ScanOptions.FalseDefault(scanGameType: true),
            scanFullIfNew: true);
    }

    private static FMScanner.ScanOptions? GetScanOptionsFromDialog(bool selected)
    {
        (bool accepted, FMScanner.ScanOptions scanOptions, bool noneSelected) =
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
        if (FMsViewList.Count == 0) return;

        FMScanner.ScanOptions? scanOptions = GetScanOptionsFromDialog(selected: false);
        if (scanOptions == null) return;

        if (await ScanFMs(FMsViewList, scanOptions))
        {
            await Core.View.SortAndSetFilter(forceDisplayFM: true);
        }
    }

    internal static async Task ScanSelectedFMs()
    {
        List<FanMission> fms = Core.View.GetSelectedFMs_InOrder_List();
        if (fms.Count == 1)
        {
            if (await ScanFMs(fms, hideBoxIfZip: true, setForceReCacheReadmes: true))
            {
                Core.View.RefreshFM(fms[0]);
            }
        }
        else if (fms.Count > 1)
        {
            FMScanner.ScanOptions? scanOptions = GetScanOptionsFromDialog(selected: true);
            if (scanOptions == null) return;

            if (await ScanFMs(fms, scanOptions, setForceReCacheReadmes: true))
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

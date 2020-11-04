using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMScan
    {
        private static CancellationTokenSource _scanCts = new CancellationTokenSource();

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
        /// <returns></returns>
        internal static async Task<bool> ScanFMs(List<FanMission> fmsToScan, FMScanner.ScanOptions? scanOptions = null,
                                                 bool scanFullIfNew = false, bool hideBoxIfZip = false)
        {
            #region Local functions

            static FMScanner.ScanOptions GetDefaultScanOptions() => FMScanner.ScanOptions.FalseDefault(
                scanTitle: true,
                scanAuthor: true,
                scanGameType: true,
                scanCustomResources: true,
                scanSize: true,
                scanReleaseDate: true,
                scanTags: true);

            static void ReportProgress(FMScanner.ProgressReport pr) => Core.View.ReportScanProgress(
                pr.FMNumber,
                pr.FMsTotal,
                pr.Percent,
                pr.FMName.ExtIsArchive() ? pr.FMName.GetFileNameFast() : pr.FMName.GetDirNameFast());

            #endregion

            scanOptions ??= GetDefaultScanOptions();

            if (fmsToScan.Count == 0) return false;

            bool scanningOne = fmsToScan.Count == 1;

            try
            {
                #region Show progress box or block UI thread

                if (hideBoxIfZip && scanningOne)
                {
                    // Just use a cheap check and throw up the progress box for .7z files, otherwise not. Not as
                    // nice as the timer method, but that can cause race conditions I don't know how to fix, so
                    // whatever.
                    if (fmsToScan[0].Archive.ExtIs7z())
                    {
                        Core.View.ShowProgressBox(ProgressTask.FMScan);
                    }
                    else
                    {
                        // Block user input to the form to mimic the UI thread being blocked, because we're async
                        // here
                        Core.View.Block(true);
                        // Doesn't actually show the box, but shows the meter on the taskbar I guess?
                        Core.View.ShowProgressBox(ProgressTask.FMScan, suppressShow: true);
                    }
                }
                else
                {
                    Core.View.ShowProgressBox(ProgressTask.FMScan);
                }

                #endregion

                #region Init

                _scanCts = new CancellationTokenSource();

                var fms = new List<FMScanner.FMToScan>();

                // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                // cause then it will hit the actual disk rather than just going through a list of paths in
                // memory
                var archivePaths = await Task.Run(FMArchives.GetFMArchivePaths);

                #endregion

                #region Filter out invalid FMs from scan list

                // Safety net to guarantee that the in and out lists will have the same count and order
                var fmsToScanFiltered = new List<FanMission>();

                for (int i = 0; i < fmsToScan.Count; i++)
                {
                    FanMission fm = fmsToScan[i];

                    // Through luck we're actually bug-free here even without this check, but we should do it
                    // explicitly
                    if (fm.MarkedDeleted) continue;

                    string fmArchivePath = await Task.Run(() => FMArchives.FindFirstMatch(fm.Archive, archivePaths));
                    if (!fm.Archive.IsEmpty() && !fmArchivePath.IsEmpty())
                    {
                        fmsToScanFiltered.Add(fm);
                        fms.Add(new FMScanner.FMToScan
                        {
                            Path = fmArchivePath,
                            ForceFullScan = scanFullIfNew && !fm.MarkedScanned
                        });
                    }
                    else if (GameIsKnownAndSupported(fm.Game))
                    {
                        string fmInstalledPath = Config.GetFMInstallPathUnsafe(fm.Game);
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

                List<FMScanner.ScannedFMData?> fmDataList;
                try
                {
                    var progress = new Progress<FMScanner.ProgressReport>(ReportProgress);

                    await Task.Run(() => Paths.CreateOrClearTempPath(Paths.FMScannerTemp));

                    using var scanner = new FMScanner.Scanner
                    {
                        FullScanOptions = GetDefaultScanOptions(),
                        LogFile = Paths.ScannerLogFile
                    };
                    fmDataList = await scanner.ScanAsync(fms, Paths.FMScannerTemp, scanOptions, progress, _scanCts.Token);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }

                #endregion

                #region Copy scanned data to FMs

                for (int i = 0; i < fmsToScanFiltered.Count; i++)
                {
                    FMScanner.ScannedFMData? scannedFM = fmDataList[i];

                    #region Checks

                    if (scannedFM == null)
                    {
                        // We need to return fail for scanning one, else we get into an infinite loop because of
                        // a refresh that gets called in that case
                        if (scanningOne)
                        {
                            Log("(one) scanned FM was null. FM was:\r\n" +
                                "Archive: " + fmsToScanFiltered[0].Archive + "\r\n" +
                                "InstalledDir: " + fmsToScanFiltered[0].InstalledDir,
                                methodName: false);
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

                    sel.MarkedScanned = true;

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
                Core.View.ShowAlert(message, LText.AlertMessages.Error);
                return false;
            }
            finally
            {
                _scanCts?.Dispose();
                Core.View.Block(false);
                Core.View.HideProgressBox();
            }

            return true;
        }

        internal static void CancelScan() => _scanCts.CancelIfNotDisposed();

        internal static Task ScanNewFMs(List<int> fmsViewListUnscanned)
        {
            AssertR(fmsViewListUnscanned.Count > 0, nameof(fmsViewListUnscanned) + ".Count was 0");

            var fmsToScan = new List<FanMission>();

            // NOTE: We use FMDataIniList index because that's the list that the indexes are pulled from!
            // (not FMsViewList)
            foreach (int index in fmsViewListUnscanned) fmsToScan.Add(FMDataIniList[index]);
            // Just in case
            fmsViewListUnscanned.Clear();

            return ScanFMs(fmsToScan, FMScanner.ScanOptions.FalseDefault(scanGameType: true), scanFullIfNew: true);
        }
    }
}

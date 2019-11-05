using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        #region Scan

        private static CancellationTokenSource ScanCts = new CancellationTokenSource();

        internal static async Task ScanFMAndRefresh(FanMission fm, FMScanner.ScanOptions? scanOptions = null)
        {
            // NULL_TODO: We could avoid a nullable by having a different way to do the default thing
            if (scanOptions == null) scanOptions = GetDefaultScanOptions();
            bool success = await ScanFM(fm, scanOptions);
            if (success) await Core.View.RefreshSelectedFM(refreshReadme: false);
        }

        internal static Task<bool> ScanFM(FanMission fm, FMScanner.ScanOptions scanOptions) => ScanFMs(new List<FanMission> { fm }, scanOptions);

        internal static async Task<bool> ScanFMs(List<FanMission> fmsToScan, FMScanner.ScanOptions scanOptions, bool markAsScanned = true)
        {
            // NULL_TODO: Do we need this FM null check...?
            if (fmsToScan.Count == 0 || (fmsToScan.Count == 1 && fmsToScan[0] == null))
            {
                return false;
            }

            var scanningOne = fmsToScan.Count == 1;

            try
            {
                #region Show progress box or block UI thread

                if (scanningOne)
                {
                    Log(nameof(ScanFMs) + ": Scanning one", methodName: false);
                    // Just use a cheap check and throw up the progress box for .7z files, otherwise not. Not as
                    // nice as the timer method, but that can cause race conditions I don't know how to fix, so
                    // whatever.
                    if (fmsToScan[0].Archive.ExtIs7z())
                    {
                        Core.View.ShowProgressBox(ProgressTasks.ScanAllFMs);
                    }
                    else
                    {
                        // Block user input to the form to mimic the UI thread being blocked, because we're async
                        // here
                        Core.View.Block(true);
                        // Doesn't actually show the box, but shows the meter on the taskbar I guess?
                        Core.View.ShowProgressBox(ProgressTasks.ScanAllFMs, suppressShow: true);
                    }
                }
                else
                {
                    Core.View.ShowProgressBox(ProgressTasks.ScanAllFMs);
                }

                #endregion

                static void ReportProgress(FMScanner.ProgressReport pr)
                {
                    var fmIsZip = pr.FMName.ExtIsArchive();
                    var name = fmIsZip ? pr.FMName.GetFileNameFast() : pr.FMName.GetDirNameFast();
                    Core.View.ReportScanProgress(pr.FMNumber, pr.FMsTotal, pr.Percent, name);
                }

                #region Init

                ScanCts = new CancellationTokenSource();

                var fms = new List<string>();

                Log(nameof(ScanFMs) + ": about to call " + nameof(GetFMArchivePaths) + " with subfolders=" +
                    Config.FMArchivePathsIncludeSubfolders);

                // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                // cause then it will hit the actual disk rather than just going through a list of paths in
                // memory
                var archivePaths = await Task.Run(GetFMArchivePaths);

                #endregion

                #region Filter out invalid FMs from scan list

                // Safety net to guarantee that the in and out lists will have the same count and order
                var fmsToScanFiltered = new List<FanMission>();

                for (var i = 0; i < fmsToScan.Count; i++)
                {
                    var fm = fmsToScan[i];
                    var fmArchivePath = await Task.Run(() => FindFMArchive(fm.Archive, archivePaths));
                    if (!fm.Archive.IsEmpty() && !fmArchivePath.IsEmpty())
                    {
                        fmsToScanFiltered.Add(fm);
                        fms.Add(fmArchivePath);
                    }
                    else if (GameIsKnownAndSupported(fm.Game))
                    {
                        var fmInstalledPath = Config.GetFMInstallPathUnsafe(fm.Game);
                        if (!fmInstalledPath.IsEmpty())
                        {
                            fmsToScanFiltered.Add(fm);
                            fms.Add(Path.Combine(fmInstalledPath, fm.InstalledDir));
                        }
                    }

                    if (ScanCts.IsCancellationRequested) return false;
                }

                if (fmsToScanFiltered.Count == 0) return false;

                #endregion

                #region Run scanner

                List<FMScanner.ScannedFMData> fmDataList;
                try
                {
                    var progress = new Progress<FMScanner.ProgressReport>(ReportProgress);

                    await Task.Run(() => Paths.CreateOrClearTempPath(Paths.FMScannerTemp));

                    using var scanner = new FMScanner.Scanner { LogFile = Paths.ScannerLogFile, ZipEntryNameEncoding = Encoding.UTF8 };
                    fmDataList = await scanner.ScanAsync(fms, Paths.FMScannerTemp, scanOptions, progress, ScanCts.Token);
                }
                catch (OperationCanceledException)
                {
                    return false;
                }

                #endregion

                #region Copy scanned data to FMs

                for (var i = 0; i < fmsToScanFiltered.Count; i++)
                {
                    var scannedFM = fmDataList[i];

                    #region Checks

                    if (scannedFM == null)
                    {
                        // We need to return fail for scanning one, else we get into an infinite loop because of
                        // a refresh that gets called in that case
                        if (scanningOne)
                        {
                            Log(nameof(ScanFMs) + " (one) scanned FM was null. FM was:\r\n" +
                                "Archive: " + fmsToScanFiltered[0].Archive + "\r\n" +
                                "InstalledDir: " + fmsToScanFiltered[0].InstalledDir,
                                methodName: false);
                            return false;
                        }
                        continue;
                    }

                    var sel = fmsToScanFiltered[i];
                    // NULL_TODO: Yeah pretty sure we don't need this anymore
                    if (sel == null)
                    {
                        // Same as above (this should never happen now, but hey)
                        if (scanningOne) return false;
                        continue;
                    }

                    #endregion

                    #region Set FM fields

                    var gameSup = scannedFM.Game != FMScanner.Game.Unsupported;

                    if (scanOptions.ScanTitle)
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

                    if (scanOptions.ScanSize)
                    {
                        sel.SizeBytes = (ulong)(gameSup ? scannedFM.Size ?? 0 : 0);
                    }
                    if (scanOptions.ScanReleaseDate)
                    {
                        sel.ReleaseDate.DateTime = gameSup ? scannedFM.LastUpdateDate : null;
                    }
                    if (scanOptions.ScanCustomResources)
                    {
                        sel.HasMap = gameSup ? scannedFM.HasMap : null;
                        sel.HasAutomap = gameSup ? scannedFM.HasAutomap : null;
                        sel.HasScripts = gameSup ? scannedFM.HasCustomScripts : null;
                        sel.HasTextures = gameSup ? scannedFM.HasCustomTextures : null;
                        sel.HasSounds = gameSup ? scannedFM.HasCustomSounds : null;
                        sel.HasObjects = gameSup ? scannedFM.HasCustomObjects : null;
                        sel.HasCreatures = gameSup ? scannedFM.HasCustomCreatures : null;
                        sel.HasMotions = gameSup ? scannedFM.HasCustomMotions : null;
                        sel.HasMovies = gameSup ? scannedFM.HasMovies : null;
                        sel.HasSubtitles = gameSup ? scannedFM.HasCustomSubtitles : null;
                    }

                    if (scanOptions.ScanAuthor)
                    {
                        sel.Author = gameSup ? scannedFM.Author : "";
                    }

                    if (scanOptions.ScanGameType)
                    {
                        // @GENGAMES: Do a hard convert at the API boundary, even though these now match the ordering
                        sel.Game = scannedFM.Game switch
                        {
                            FMScanner.Game.Unsupported => Game.Unsupported,
                            FMScanner.Game.Thief1 => Game.Thief1,
                            FMScanner.Game.Thief2 => Game.Thief2,
                            FMScanner.Game.Thief3 => Game.Thief3,
                            FMScanner.Game.SS2 => Game.SS2,
                            _ => Game.Null
                        };
                    }

                    if (scanOptions.ScanLanguages)
                    {
                        // TODO: Uncomment if you start using this
                        //sel.Languages = gameSup ? scannedFM.Languages : new string[0];
                        sel.LanguagesString = gameSup
                            ? scannedFM.Languages != null ? string.Join(", ", scannedFM.Languages) : ""
                            : "";
                    }

                    if (scanOptions.ScanTags)
                    {
                        sel.TagsString = gameSup ? scannedFM.TagsString : "";

                        // Don't clear the tags, because the user could have added a bunch and we should only
                        // add to those, not overwrite them
                        if (gameSup) FMTags.AddTagsToFMAndGlobalList(sel.TagsString, sel.Tags);
                    }

                    sel.MarkedScanned = markAsScanned;

                    #endregion
                }

                #endregion

                Ini.Ini.WriteFullFMDataIni();
            }
            catch (Exception ex)
            {
                Log("Exception in ScanFMs", ex);
                var message = scanningOne
                    ? LText.AlertMessages.Scan_ExceptionInScanOne
                    : LText.AlertMessages.Scan_ExceptionInScanMultiple;
                Core.View.ShowAlert(message, LText.AlertMessages.Error);
                return false;
            }
            finally
            {
                ScanCts?.Dispose();
                Core.View.Block(false);
                Core.View.HideProgressBox();
            }

            return true;
        }

        internal static void CancelScan() => ScanCts.CancelIfNotDisposed();

        internal static async Task ScanNewFMsForGameType()
        {
            var fmsToScan = new List<FanMission>();

            try
            {
                // NOTE: We use FMDataIniList index because that's the list that the indexes are pulled from!
                // (not FMsViewList)
                foreach (var index in ViewListGamesNull) fmsToScan.Add(FMDataIniList[index]);
            }
            catch
            {
                // Cheap fallback in case something goes wrong, because what we're doing is a little iffy
                fmsToScan.Clear();
                // Since we're doing it manually here, we can pull from FMsViewList for perf (it'll be the same
                // size or smaller than FMDataIniList)
                foreach (var fm in FMsViewList) if (fm.Game == Game.Null) fmsToScan.Add(fm);
            }
            finally
            {
                // Critical that this gets cleared immediately after use!
                ViewListGamesNull.Clear();
            }

            if (fmsToScan.Count > 0)
            {
                try
                {
                    await ScanFMs(fmsToScan, FMScanner.ScanOptions.FalseDefault(scanGameType: true), markAsScanned: false);
                }
                catch (Exception ex)
                {
                    Log("Exception in ScanFMs", ex);
                }
            }
        }

        internal static async Task FindNewFMsAndScanForGameType()
        {
            FindFMs.Find(Config.FMInstallPaths);
            // This await call takes 15ms just to make the call alone(?!) so don't do it unless we have to
            if (ViewListGamesNull.Count > 0) await ScanNewFMsForGameType();
        }

        internal static async Task ScanAndFind(List<FanMission> fms, FMScanner.ScanOptions scanOptions)
        {
            if (fms.Count == 0) return;

            await ScanFMs(fms, scanOptions);
            // TODO: Why am I doing a find after a scan?!?!?! WTF use is this?
            // Note: I might be doing it to get rid of any duplicates or bad data that may have been imported?
            FindFMs.Find(Config.FMInstallPaths);
        }

        #endregion
    }
}

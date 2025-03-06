//#define ScanSynchronous
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader;
using FMScanner;
using YamlDotNet.Serialization;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal static class FMScan
{
    private static CancellationTokenSource _scannerCTS = new();

    #region Private methods

    private static void ClearTempDir()
    {
        string tempPath = Core.View.GetTempPath();

        if (tempPath.IsWhiteSpace()) return;

        try
        {
            // BUG: We're not unsetting readonly
            foreach (string d in Directory.GetDirectories(tempPath, "*", SearchOption.TopDirectoryOnly))
            {
                Directory.Delete(d, true);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    private static string GetYamlPath(bool zips) =>
        zips
            ? Config.FMsPath == Paths.SevenZipTestPath
                ? Paths.LocalDataSevenZipSharpVerPath
                : Config.FMsPath == Paths.T3ArchivePath
                    ? Paths.LocalDataT3Path
                    : Config.FMsPath == Paths.SS2ArchivePath
                        ? Paths.LocalDataSS2Path
                        : Paths.LocalDataPath
            : Paths.LocalDataFolderVerPath;

    private static void WriteYaml(bool zips, string path, List<ScannedFMDataAndError> scannedFMs, bool deleteSingle = false)
    {
        foreach (ScannedFMDataAndError fmData in scannedFMs)
        {
            if (fmData.ScannedFMData == null) continue;

            var builder = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
                .DisableAliases()
                .WithNamingConvention(new LowerCaseNamingConvention());

            ISerializer serializer = builder.Build();
            string yaml = serializer.Serialize(fmData.ScannedFMData);

            string yamlFile = Path.Combine(path,
                (zips
                    ? fmData.ScannedFMData.ArchiveName.FN_NoExt().Trim()
                    : Path.GetFileName(fmData.ScannedFMData.ArchiveName).Trim())
                + ".yaml");

            if (deleteSingle) File.Delete(yamlFile);

            using StreamWriter sw = new(yamlFile, append: false);
            sw.Write(yaml);
        }
    }

    private static (string YamlPath, List<FMToScan> FMsToScan, ScanOptions ScanOptions)
    GetScanData(bool zips)
    {
        List<FMToScan> fmsList = new();
        if (zips)
        {
            foreach (string f in Core.View.GetFMArchives())
            {
                fmsList.Add(new FMToScan(path: Path.Combine(Config.FMsPath, f), false, cachePath: "",
                    isTDM: false, displayName: f, isArchive: true, originalIndex: fmsList.Count));
            }
        }
        else
        {
            foreach (string f in Directory.GetDirectories(Paths.CurrentExtractedDir, "*", SearchOption.TopDirectoryOnly))
            {
                fmsList.Add(new FMToScan(path: f, forceFullScan: false, cachePath: "", isTDM: false,
                    displayName: f, isArchive: false, originalIndex: fmsList.Count));
            }
        }

        string yamlPath = GetYamlPath(zips);

        Directory.CreateDirectory(yamlPath);

        foreach (string f in Directory.GetFiles(yamlPath, "*.yaml", SearchOption.TopDirectoryOnly))
        {
            File.Delete(f);
        }

        return (yamlPath, fmsList, Core.View.GetSelectedScanOptions());
    }

    // Have to ifdef this whole method because the scanner's multi-FM synchronous Scan() is also ifdeffed on
    // DEBUG
#if DEBUG || ScanSynchronous
    private static void ScanAllFMs_Sync_Internal(bool zips)
    {
        (string yamlPath, List<FMToScan> fms, ScanOptions scanOptions) = GetScanData(zips);

        List<ScannedFMDataAndError> scannedFMs;

        Stopwatch t = new();
        t.Start();

        using (Scanner scanner = new(Paths.SevenZipExe))
        {
            scannedFMs = scanner.Scan(
                fms,
                @"J:\FM packs\FM pack\All\ExtractTemp",
                scanOptions,
                null!,
                CancellationToken.None);
        }

        Debug.WriteLine("Scan took: " + t.Elapsed);
        Core.View.SetDebugLabelText("Scan took: " + t.Elapsed);

        t.Stop();

        // Clear it again after scanning all FMs just in case. Don't include this in the time measurement.
        ClearTempDir();

        WriteYaml(zips, yamlPath, scannedFMs);
    }
#endif

    private static async Task ScanAllFMs_Async_Internal(bool zips)
    {
        (string yamlPath, List<FMToScan> fms, ScanOptions scanOptions) = GetScanData(zips);

        List<ScannedFMDataAndError> scannedFMs;

        Stopwatch t = new();

        try
        {
            _scannerCTS = _scannerCTS.Recreate();

            Progress<ProgressReport> progress = new(ReportProgress);

            t.Start();

            using (Scanner scanner = new(Paths.SevenZipExe))
            {
                scannedFMs = await scanner.ScanAsync(
                    fms,
                    @"J:\FM packs\FM pack\All\ExtractTemp",
                    scanOptions,
                    progress,
                    _scannerCTS.Token);
            }

            t.Stop();

            MessageBox.Show("Finished scan!");

            Debug.WriteLine("Scan took: " + t.Elapsed);
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Canceled.");
            Core.View.SetDebugLabelText("Canceled.");
            return;
        }
        finally
        {
            t.Stop();
            _scannerCTS.Dispose();
        }

        try
        {
            await Task.Run(() =>
            {
                Core.View.SetDebugLabelText("Finishing and writing to disk...");

                // Clear it again after scanning all FMs just in case. Don't include this in the time measurement.
                ClearTempDir();
                WriteYaml(zips, yamlPath, scannedFMs);
            });
        }
        finally
        {
            Core.View.SetDebugLabelText("Scan took: " + t.Elapsed);
            Core.View.SetFMScanProgressBarValue(0);
        }

        return;

        static void ReportProgress(ProgressReport pr)
        {
            int percent = GetPercentFromValue_Int(pr.FMNumber, pr.FMsCount);

            Core.View.SetDebugLabelText(
                "Scanned " + pr.FMNumber + "/" + pr.FMsCount + ", " +
                percent + "%\r\n" +
                pr.FMName);
            Core.View.SetFMScanProgressBarValue(percent);
        }
    }

    #endregion

    #region Public methods

    internal static void ScanFM(string item, bool zip, string? yamlPathOverride = null)
    {
        string yamlPath = yamlPathOverride ?? GetYamlPath(zip);

        Directory.CreateDirectory(yamlPath);

        ScanOptions scanOptions = Core.View.GetSelectedScanOptions();

        ScannedFMDataAndError fmData;
        using (Scanner scanner = new(Paths.SevenZipExe))
        {
            string fmPath = zip
                ? Path.Combine(Config.FMsPath, item)
                : Path.Combine(Paths.CurrentExtractedDir, item);
            fmData = scanner.Scan(fmPath, Core.View.GetTempPath(), scanOptions, false, item, zip);
        }

        if (fmData.ScannedFMData == null) return;

        WriteYaml(zip, yamlPath, new List<ScannedFMDataAndError> { fmData }, deleteSingle: true);
    }

#pragma warning disable 1998
    internal static async Task ScanAllFMs(bool zips)
    {
        try
        {
            Core.View.BeginScanMode();
#if DEBUG || ScanSynchronous
            {
                ScanAllFMs_Sync_Internal(zips);
            }
#else
            {
                await ScanAllFMs_Async_Internal(zips);
            }
#endif
        }
        finally
        {
            Core.View.EndScanMode();
        }
    }
#pragma warning restore 1998

    internal static void CancelScan() => _scannerCTS.CancelIfNotDisposed();

    #endregion
}

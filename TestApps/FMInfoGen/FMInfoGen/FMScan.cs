//#define ScanSynchronous
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FMScanner;
using YamlDotNet.Serialization;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal static class FMScan
{
    private static CancellationTokenSource? _scannerCTS;

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
        foreach (var fmData in scannedFMs)
        {
            if (fmData.ScannedFMData == null) continue;

            var builder = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.Preserve)
                .DisableAliases()
                .WithNamingConvention(new LowerCaseNamingConvention());

            var serializer = builder.Build();
            var yaml = serializer.Serialize(fmData.ScannedFMData);

            string yamlFile = Path.Combine(path,
                (zips
                    ? fmData.ScannedFMData.ArchiveName.FN_NoExt().Trim()
                    : Path.GetFileName(fmData.ScannedFMData.ArchiveName).Trim())
                + ".yaml");

            if (deleteSingle) File.Delete(yamlFile);

            using var sw = new StreamWriter(yamlFile, append: false);
            sw.Write(yaml);
        }
    }

    private static (string YamlPath, List<FMToScan> FMsToScan, ScanOptions ScanOptions)
    GetScanData(bool zips)
    {
        var fmsList = (zips
                ? Core.View.GetFMArchives()
                    .Select(static f => new FMToScan(path: Path.Combine(Config.FMsPath, f), false, cachePath: "", isTDM: false, displayName: f))
                : Directory.GetDirectories(Paths.CurrentExtractedDir, "*", SearchOption.TopDirectoryOnly)
                    .Select(static f => new FMToScan(path: f, forceFullScan: false, cachePath: "", isTDM: false, displayName: f)))
            .ToList();

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

            var t = new Stopwatch();
            t.Start();

            using (var scanner = new Scanner(Paths.SevenZipExe))
            {
                scannedFMs = scanner.Scan(
                    fms,
                    @"F:\FM pack\All\ExtractTemp",
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
        static void ReportProgress(ProgressReport pr)
        {
            Core.View.SetDebugLabelText(
                "Scanned " + pr.FMNumber + "/" + pr.FMsTotal + ", " +
                pr.Percent + "%\r\n" +
                pr.FMName);
            Core.View.SetFMScanProgressBarValue(pr.Percent);
        }

        (string yamlPath, List<FMToScan> fms, ScanOptions scanOptions) = GetScanData(zips);

        List<ScannedFMDataAndError> scannedFMs;

        var t = new Stopwatch();

        try
        {
            _scannerCTS = new CancellationTokenSource();

            var progress = new Progress<ProgressReport>(ReportProgress);

            t.Start();

            using (var scanner = new Scanner(Paths.SevenZipExe))
            {
                scannedFMs = await scanner.ScanAsync(
                    fms,
                    @"F:\FM pack\All\ExtractTemp",
                    scanOptions,
                    progress,
                    _scannerCTS.Token);
            }

            t.Stop();

            MessageBox.Show("Finished scan!");

            await Task.Delay(500);
            Debug.WriteLine("Scan took: " + t.Elapsed);
            Core.View.SetDebugLabelText("Scan took: " + t.Elapsed);
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
            _scannerCTS?.Dispose();
        }

        // Clear it again after scanning all FMs just in case. Don't include this in the time measurement.
        ClearTempDir();

        WriteYaml(zips, yamlPath, scannedFMs);
    }

    #endregion

    #region Public methods

    internal static void ScanFM(string item, bool zip, string? yamlPathOverride = null)
    {
        string yamlPath = yamlPathOverride ?? GetYamlPath(zip);

        Directory.CreateDirectory(yamlPath);

        var scanOptions = Core.View.GetSelectedScanOptions();

        ScannedFMDataAndError fmData;
        using (var scanner = new Scanner(Paths.SevenZipExe))
        {
            string fmPath = zip
                ? Path.Combine(Config.FMsPath, item)
                : Path.Combine(Paths.CurrentExtractedDir, item);
            fmData = scanner.Scan(fmPath, Core.View.GetTempPath(), scanOptions, false, item);
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

    internal static void CancelScan() => _scannerCTS?.CancelIfNotDisposed();

    #endregion
}

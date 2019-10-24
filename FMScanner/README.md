# FMScanner

A fast, thorough, accurate scanner for Thief 1, Thief 2, and Thief 3 fan missions.

Detects the following:
- Title (along with a list of alternate titles if more than one is detected and they don't all match)
- Titles of campaign missions if it's a campaign
- Author
- Description (if specified in fm.ini)
- Game (Thief 1, Thief 2, or Thief: Deadly Shadows)
- Languages (work in progress)
- Version (if specified; work in progress)
- Whether the mission requires NewDark
- The minimum required NewDark version (if specified; work in progress)
- Last updated date (if specified explicitly; otherwise best guess)
- Size of the FM (compressed size for zips; uncompressed size for folders)
- Whether the mission has any of the following:
  - Map
  - Automap
  - Custom textures
  - Custom objects
  - Custom AIs (creatures, guards, etc.)
  - Custom sounds
  - Custom movies
  - Custom motions
  - Custom scripts
  - Custom subtitles (NewDark-style only)
  
## Usage

```csharp
private async void FMScannerUsageExample()
{
    // Set these to what you want. The fewer things that are scanned for, the faster the scan will be.
    // This is optional. If you call Scan() without providing a ScanOptions object, all options will default
    // to true.
    var scanOptions = new FMScanner.ScanOptions
    {
        ScanTitle = true,
        ScanCampaignMissionNames = true,
        ScanAuthor = true,
        ScanVersion = true,
        ScanLanguages = true,
        ScanGameType = true,
        ScanNewDarkRequired = true,
        ScanNewDarkMinimumVersion = true,
        ScanCustomResources = true,
        ScanSize = true
    };

    // In most cases archives will be scanned without requiring an extract to disk, but when that's not the
    // case, they will be temporarily extracted to this directory.
    var tempPath = "C:\\MyTempDir\\FmScanTemp";

    // Single-FM scans are synchronous (which is probably what you want for FM loaders).
    ScanSingleFM(scanOptions, tempPath);
    
    // Multi-FM scans are asynchronous.
    await ScanMultipleFMs(scanOptions, tempPath);
    await ScanMultipleFMsWithProgressReport(scanOptions, tempPath);
}
```
### Scan a single FM
```csharp
private void ScanSingleFM(FMScanner.ScanOptions scanOptions, string tempPath)
{
    // This can be either an archive (.zip, .7z) or a directory. The scanner detects based on extension.
    var fm = "C:\\FMs\\Rocksbourg3.zip";

    FMScanner.ScannedFMData fmData;
    using (var scanner = new FMScanner.Scanner())
    {
        fmData = scanner.Scan(fm, tempPath, scanOptions);
    }

    // do something with fmData here
}
```

### Scan multiple FMs
```csharp
private async Task ScanMultipleFMs(FMScanner.ScanOptions scanOptions, string tempPath)
{
    // The list can contain both archives (.zip, .7z) and directories. The scanner detects based on
    // extension.
    var fms = new List<string>
    {
        "C:\\FMs\\BrokenTriad.zip",
        "C:\\FMs\\Racket.7z",
        "C:\\Thief2\\FMs\\SevenSisters_The"
    };
    
    List<FMScanner.ScannedFMData> fmDataList;
    using (var scanner = new FMScanner.Scanner())
    {
        fmDataList = await scanner.ScanAsync(fms, tempPath, scanOptions);
    }
    
    // do something with fmDataList here
}
```

### Scan multiple FMs with progress reporting and cancellation
```csharp
private System.Threading.CancellationTokenSource cts;
private async Task ScanMultipleFMsWithProgressReport(FMScanner.ScanOptions scanOptions, string tempPath)
{
    // The list can contain both archives (.zip, .7z) and directories. The scanner detects based on
    // extension.
    var fms = new List<string>
    {
        "C:\\FMs\\BrokenTriad.zip",
        "C:\\FMs\\Racket.7z",
        "C:\\Thief2\\FMs\\SevenSisters_The"
    };
    
    void ReportProgress(FMScanner.ProgressReport progressReport)
    {
        Console.WriteLine("FM name: " + progressReport.FMName);     // string
        Console.WriteLine("FM number: " + progressReport.FMNumber); // int
        Console.WriteLine("FMs total: " + progressReport.FMsTotal); // int
        Console.WriteLine("Percent: " + progressReport.Percent);    // int
        Console.WriteLine("Finished: " + progressReport.Finished);  // bool
    }
    
    cts = new System.Threading.CancellationTokenSource();

    List<FMScanner.ScannedFMData> fmDataList;
    try
    {
        var progress = new Progress<FMScanner.ProgressReport>(ReportProgress);
        using (var scanner = new FMScanner.Scanner())
        {
            // You can pass CancellationToken.None if you don't want the ability to cancel.
            fmDataList = await scanner.ScanAsync(fms, tempPath, scanOptions, progress, cts.Token);
        }
    }
    catch (OperationCanceledException)
    {
        // The user cancelled the scan; act accordingly here
    }
    finally
    {
        cts?.Dispose();
    }
    
    // do something with fmDataList here
}

// You can call this from a cancel button or what have you.
private void CancelFMScan()
{
    try
    {
        cts?.Cancel();
    }
    catch (ObjectDisposedException)
    {
    }
}
```

## License

FMScanner is licensed under the CC0 license, but contains portions that are licensed differently (because they're based on others' work). Each differently-licensed component is cleanly separated into its own folder with a LICENSE file in it. The license for each component is also specified at the top of each source file belonging to it. I apologize that legalities can be a pain, but I've done what I can to make it clear. Hope it helps.

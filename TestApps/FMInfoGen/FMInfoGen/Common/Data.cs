using System.IO;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using static FMInfoGen.Misc;

namespace FMInfoGen;

internal static class Paths
{
    private static readonly string _appPath = Application.StartupPath;
    private const string _extractedDir = "ExtractPermanent";
    private const string _accuracyDataDir = "accuracy-data";

    internal const string T1T2ArchivePath = @"J:\FM packs\FM pack\All";
    internal const string SS2ArchivePath = @"J:\FM packs\SS2 FM Pack";
    internal const string T3ArchivePath = @"J:\FM packs\FM pack T3";
    internal const string SevenZipTestPath = @"J:\FM packs\7z_FMs";

    internal const string LocalTestPath = @"C:\fm-info-test";

    internal static readonly string ConfigFile = Path.Combine(_appPath, "config.ini");

    internal static readonly string LocalDataPath = Path.Combine(LocalTestPath, "localdata");
    internal static readonly string LocalDataFolderVerPath = Path.Combine(LocalTestPath, "localdata-folder-ver");
    internal static readonly string LocalDataSevenZipSharpVerPath = Path.Combine(LocalTestPath, "localdata-sevenzipsharp");
    internal static readonly string LocalDataT3Path = Path.Combine(LocalTestPath, "localdata-thief3");
    internal static readonly string LocalDataSS2Path = Path.Combine(LocalTestPath, "localdata-ss2");

    internal static readonly string LocalDetectionLogsPath = Path.Combine(LocalTestPath, "detection-problems");
    internal static readonly string AccuracyDataPath = Path.Combine(LocalTestPath, _accuracyDataDir);

    internal const string SevenZipExe = @"C:\AngelLoader\7z64\7z.exe";

    internal static string CurrentExtractedDir => Path.Combine(Config.FMsPath, _extractedDir);
}

internal sealed class LowerCaseNamingConvention : INamingConvention
{
    public string Apply(string value) => value.ToLowerInvariant();
}

internal sealed class ConfigData
{
    internal string TempPath { get; set; } = "";
    internal string FMsPath { get; set; } = "";
}

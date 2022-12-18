using System;
using System.Globalization;
using System.IO;
using static FenGen.Misc;

namespace FenGen;

internal static class BuildDateGen
{
    internal static void Generate(string destFile, bool remove = false)
    {
        var w = GetWriterForClass(destFile, GenAttributes.FenGenBuildDateDestClass);

        string date = remove ? "" : DateTime.UtcNow.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
        w.WL("internal const string BuildDate = \"" + date + "\";");
        w.CloseClassAndNamespace();

        File.WriteAllText(destFile, w.ToString());
    }
}
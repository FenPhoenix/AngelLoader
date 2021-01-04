using System;
using System.Globalization;
using System.IO;
using System.Text;
using static FenGen.Misc;

namespace FenGen
{
    internal static class BuildDateGen
    {
        internal static void Generate(string destFile, bool remove = false)
        {
            string codeBlock = GetCodeBlock(destFile, GenAttributes.FenGenBuildDateDestClass);

            var w = new CodeWriters.IndentingWriter(startingIndent: 1);

            w.AppendRawString(codeBlock);
            w.WL("{");
            string date = remove ? "" : DateTime.UtcNow.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
            w.WL("internal const string BuildDate = \"" + date + "\";");
            w.WL("}");
            w.WL("}");

            File.WriteAllText(destFile, w.ToString());
        }
    }
}

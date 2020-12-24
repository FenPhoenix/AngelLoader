using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen
{
    internal static class BuildDateGen
    {
        internal static void Generate(string destFile, bool remove = false)
        {
            string codeBlock = GetCodeBlock(destFile, GenAttributes.FenGenBuildDateDestClass);

            var sb = new StringBuilder();
            var w = new Generators.IndentingWriter(sb, startingIndent: 1);

            sb.Append(codeBlock);
            w.WL("{");
            string date = remove ? "" : DateTime.UtcNow.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
            w.WL("internal const string BuildDate = \"" + date + "\";");
            w.WL("}");
            w.WL("}");

            File.WriteAllText(destFile, sb.ToString());
        }
    }
}

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
        internal static void Generate(string destFile)
        {
            #region Find the class we're going to write to

            string code = File.ReadAllText(destFile);
            SyntaxTree tree = ParseTextFast(code);

            var (member, _) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, GenAttributes.FenGenBuildDateDestClass);
            var classToUse = (ClassDeclarationSyntax)member;

            string classString = classToUse.ToString();
            string classDeclLine = classString.Substring(0, classString.IndexOf('{'));

            string codeBlock = code
                .Substring(0, classToUse.GetLocation().SourceSpan.Start + classDeclLine.Length)
                .TrimEnd() + "\r\n";

            #endregion

            var sb = new StringBuilder();

            var w = new Generators.IndentingWriter(sb, startingIndent: 1);

            sb.Append(codeBlock);
            w.WL("{");
            var date = DateTime.UtcNow.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo);
            w.WL("internal const string BuildDate = \"" + date + "\";");
            w.WL("}");
            w.WL("}");

            File.WriteAllText(destFile, sb.ToString());
        }
    }
}

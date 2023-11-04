using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen;

internal static class RtfDupeCodeGen
{
    internal static void Generate(string sourceFile)
    {
        string dupeText = ReadSourceFile(sourceFile);
        WriteDestFiles(dupeText);
    }

    private static string ReadSourceFile(string file)
    {
        string code = File.ReadAllText(file);
        SyntaxTree tree = ParseTextFast(code);

        (MemberDeclarationSyntax sourceClassMember, _) = GetAttrMarkedItem(
            tree,
            SyntaxKind.ClassDeclaration,
            GenAttributes.FenGenRtfDuplicateSourceClass);

        ClassDeclarationSyntax sourceClass = (ClassDeclarationSyntax)sourceClassMember;

        string ret = "";

        foreach (var member in sourceClass.Members)
        {
            ret += member.GetText();
        }

        return ret;
    }

    private static void WriteDestFiles(string dupeText)
    {
        foreach (string file in Cache.RtfDupeDestFiles)
        {
            var w = GetWriterForClass(file, GenAttributes.FenGenRtfDuplicateDestClass);
            w.WL(dupeText);
            w.CloseClassAndNamespace();
            File.WriteAllText(file, w.ToString());
        }
    }
}

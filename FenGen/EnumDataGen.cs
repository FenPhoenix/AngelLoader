using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen;
internal static class EnumDataGen
{
    internal static void Generate(string destFile)
    {
        var enumDataList = ReadSourceFiles();
        WriteDestFile(enumDataList, destFile);
    }

    private sealed class EnumData
    {
        internal readonly string Name;
        internal readonly List<string> ItemNames = new();
        internal bool GenerateCount;
        internal int CountPlusOrMinus;
        internal bool GenerateNames;

        public EnumData(string name) => Name = name;
    }

    private static List<EnumData> ReadSourceFiles()
    {
        var ret = new List<EnumData>();

        List<string> sourceFiles = Cache.TypeSourceFiles;

        foreach (string sourceFile in sourceFiles)
        {
            string code = File.ReadAllText(sourceFile);
            SyntaxTree tree = ParseTextFast(code);

            List<AttrMarkedItem> attrMarkedItems = GetAttrMarkedItems(
                tree,
                SyntaxKind.EnumDeclaration,
                GenAttributes.FenGenEnumCount,
                GenAttributes.FenGenEnumNames);

            foreach (AttrMarkedItem attrMarkedItem in attrMarkedItems)
            {
                var enumMember = (EnumDeclarationSyntax)attrMarkedItem.Member;
                var enumData = new EnumData(enumMember.Identifier.ToString());

                foreach (var enumSyntaxMember in enumMember.Members)
                {
                    enumData.ItemNames.Add(enumSyntaxMember.Identifier.ToString());
                }

                foreach (AttributeSyntax attr in attrMarkedItem.Attributes)
                {
                    if (GetAttributeName(attr.Name.ToString(), GenAttributes.FenGenEnumCount))
                    {
                        enumData.GenerateCount = true;
                        if (attr.ArgumentList is { Arguments.Count: 1 })
                        {
                            string tempStr = attr.ArgumentList.Arguments[0].ToString();
                            if (Int_TryParseInv(tempStr, out int result))
                            {
                                enumData.CountPlusOrMinus = result;
                            }
                        }
                    }
                    else if (GetAttributeName(attr.Name.ToString(), GenAttributes.FenGenEnumNames))
                    {
                        enumData.GenerateNames = true;
                    }
                }

                ret.Add(enumData);
            }
        }

        return ret;
    }

    private static void WriteDestFile(List<EnumData> enumDataList, string destFile)
    {
        var w = GetWriterForClass(destFile, GenAttributes.FenGenEnumDataDestClass);

        foreach (var enumData in enumDataList)
        {
            if (enumData.GenerateCount)
            {
                w.WL("public const int " + enumData.Name + "Count = " + (enumData.ItemNames.Count + enumData.CountPlusOrMinus).ToStrInv() + ";");
            }
            if (enumData.GenerateNames)
            {
                w.WL("public static readonly string[] " + enumData.Name + "Names =");
                w.WL("{");
                for (int itemNameI = 0; itemNameI < enumData.ItemNames.Count; itemNameI++)
                {
                    string itemName = enumData.ItemNames[itemNameI];
                    w.WL("\"" + itemName + "\",");
                }
                w.WL("};");
            }
        }

        w.CloseClassAndNamespace();

        File.WriteAllText(destFile, w.ToString());
    }
}

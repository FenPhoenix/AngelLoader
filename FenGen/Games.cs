using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen
{
    internal static class Games
    {
        internal static GameSourceEnum FillGamesEnum(string file) => ReadGameSourceEnum(file);

        internal static void Generate()
        {
            // not implemented
        }

        private static GameSourceEnum ReadGameSourceEnum(string file)
        {
            const string FenGenGameSourceEnumAttribute = "FenGenGameSourceEnum";

            var ret = new GameSourceEnum();

            string code = File.ReadAllText(file);
            var tree = ParseTextFast(code);

            var attrMarkedEnums = new List<EnumDeclarationSyntax>();

            var nodes = tree.GetCompilationUnitRoot().DescendantNodesAndSelf();
            foreach (SyntaxNode n in nodes)
            {
                if (!n.IsKind(SyntaxKind.EnumDeclaration)) continue;

                var enumItem = (EnumDeclarationSyntax)n;
                if (enumItem.AttributeLists.Count > 0 && enumItem.AttributeLists[0].Attributes.Count > 0)
                {
                    foreach (var attr in enumItem.AttributeLists[0].Attributes)
                    {
                        if (GetAttributeName(attr.Name.ToString(), FenGenGameSourceEnumAttribute))
                        {
                            attrMarkedEnums.Add(enumItem);
                        }
                    }
                }
            }

            if (attrMarkedEnums.Count > 1)
            {
                const string multipleUsesError = "ERROR: Multiple uses of attribute '" + FenGenGameSourceEnumAttribute + "'.";
                ThrowErrorAndTerminate(multipleUsesError);
            }
            else if (attrMarkedEnums.Count == 0)
            {
                const string noneFoundError = "ERROR: No uses of attribute '" + FenGenGameSourceEnumAttribute +
                                              "' (No marked game source enum found)";
                ThrowErrorAndTerminate(noneFoundError);
            }

            var gameSourceEnum = attrMarkedEnums[0];

            ret.Name = gameSourceEnum.Identifier.ToString().Trim();

            foreach (var member in gameSourceEnum.Members)
            {
                ret.Items.Add(member.Identifier.ToString());
            }

            return ret;
        }
    }
}

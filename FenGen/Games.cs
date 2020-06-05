using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            var ret = new GameSourceEnum();

            string code = File.ReadAllText(file);
            SyntaxTree tree = ParseTextFast(code);

            var gameEnum = (EnumDeclarationSyntax)
                GetAttrMarkedItem(tree, SyntaxKind.EnumDeclaration, GenAttributes.FenGenGameEnum);

            //var gamePrefixes = GetAttrMarkedItem(tree, SyntaxKind.EnumDeclaration, GenAttributes.FenGenGamePrefixes);

            //ArrayTypeSyntax prefixesArray;
            //foreach (var n in gamePrefixes.DescendantNodesAndSelf())
            //{
            //    if (n.IsKind(SyntaxKind.VariableDeclaration))
            //    {
            //        //Trace.WriteLine("var");
            //        var subNode0 = n.ChildNodes().First();
            //        //Trace.WriteLine(n.ChildNodes().First().Kind());
            //        if (subNode0.IsKind(SyntaxKind.ArrayType))
            //        {
            //            var arrayItem = (ArrayTypeSyntax)subNode0;

            //            //Trace.WriteLine(arrayItem.ElementType.ToString());

            //            if (arrayItem.ElementType.ToString() == "string" ||
            //                arrayItem.ElementType.ToString() == "String")
            //            {
            //                prefixesArray = arrayItem;
            //            }
            //            else
            //            {
            //                ThrowErrorAndTerminate("ERROR: Game prefix array is not of type string");
            //            }
            //        }
            //    }
            //}

            //Trace.WriteLine("");

            ret.Name = gameEnum.Identifier.ToString().Trim();
            for (int i = 0; i < gameEnum.Members.Count; i++)
            {
                var member = gameEnum.Members[i];
                string memberName = member.Identifier.ToString();
                ret.GameEnumNames.Add(memberName);
                if (!HasAttribute(member, GenAttributes.FenGenNotAGameType))
                {
                    ret.GameIndexEnumNames.Add(memberName);
                }
            }

            return ret;
        }
    }
}

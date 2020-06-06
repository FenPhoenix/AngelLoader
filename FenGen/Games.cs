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

            var d = GetAttrMarkedItem(tree, SyntaxKind.EnumDeclaration, GenAttributes.FenGenGameEnum);
            var gameEnum = (EnumDeclarationSyntax)d.Member;
            AttributeSyntax gameEnumAttr = d.Attribute;

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

            var argsList = gameEnumAttr.ArgumentList;
            if (argsList == null || argsList.Arguments.Count == 0)
            {
                ThrowErrorAndTerminate(GenAttributes.FenGenGameEnum + " was expected to have an argument but doesn't.");
            }

            string attrVal = argsList!.Arguments[0].ToString();
            string[] prefixes = attrVal.Split(',');
            if (prefixes.Length != ret.GameIndexEnumNames.Count)
            {
                ThrowErrorAndTerminate("Prefix list in " + GenAttributes.FenGenGameEnum + " doesn't match count of games");
            }

            ret.GamePrefixes.AddRange(prefixes);

            return ret;
        }
    }
}

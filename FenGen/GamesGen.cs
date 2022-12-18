﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen;

internal static class Games
{
    internal static GameSourceEnum FillGamesEnum(string file) => ReadGameSourceEnum(file);

    internal static void Generate(string mainGenDestFile) => WriteGameSupportMainGen(mainGenDestFile);

    private static GameSourceEnum ReadGameSourceEnum(string file)
    {
        var ret = new GameSourceEnum();

        string code = File.ReadAllText(file);
        SyntaxTree tree = ParseTextFast(code);

        var d = GetAttrMarkedItem(tree, SyntaxKind.EnumDeclaration, GenAttributes.FenGenGameEnum);
        var gameEnum = (EnumDeclarationSyntax)d.Member;
        AttributeSyntax gameEnumAttr = d.Attribute;
        if (gameEnumAttr.ArgumentList == null || gameEnumAttr.ArgumentList.Arguments.Count == 0)
        {
            ThrowErrorAndTerminate(nameof(GenAttributes.FenGenGameEnum) + " had 0 args");
        }

        ret.GameIndexName = ((LiteralExpressionSyntax)gameEnumAttr.ArgumentList!.Arguments[0].Expression).Token.ValueText;

        ret.Name = gameEnum.Identifier.ToString().Trim();
        for (int i = 0; i < gameEnum.Members.Count; i++)
        {
            var member = gameEnum.Members[i];
            string memberName = member.Identifier.ToString();
            ret.GameEnumNames.Add(memberName);
            if (!HasAttribute(member, GenAttributes.FenGenIgnore))
            {
                ret.GameIndexEnumNames.Add(memberName);

                AttributeSyntax? gameAttr = member
                    .AttributeLists[0]
                    .Attributes
                    .FirstOrDefault(static x => x.Name.ToString() == GenAttributes.FenGenGame);

                if (gameAttr != null)
                {
                    const int reqArgCount = 3;

                    if (gameAttr.ArgumentList is not { Arguments.Count: reqArgCount })
                    {
                        ThrowErrorAndTerminate(nameof(GenAttributes.FenGenGame) + " had other than " + reqArgCount + " args");
                    }

                    string prefixArg =
                        ((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[0].Expression).Token
                        .ValueText;
                    string steamIdArg =
                        ((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[1].Expression).Token
                        .ValueText;
                    string editorNameArg =
                        ((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[2].Expression).Token
                        .ValueText;

                    ret.GamePrefixes.Add(prefixArg);
                    ret.SteamIds.Add(steamIdArg);
                    ret.EditorNames.Add(editorNameArg);
                }
            }
        }

        return ret;
    }

    private static void WriteGameSupportMainGen(string destFile)
    {
        #region Local functions

        static void WriteArrayAndGetter(
            CodeWriters.IndentingWriter w,
            string arrayName,
            string getterName,
            List<string> items,
            string gameIndexName,
            bool addQuotes = true)
        {
            w.WL("private static readonly string[] " + arrayName + " =");
            WriteListBody(w, items, addQuotes: addQuotes);
            w.WL("public static string " + getterName + "(" + gameIndexName + " index) => " + arrayName + "[(int)index];");
            w.WL();
        }

        #endregion

        var w = GetWriterForClass(destFile, GenAttributes.FenGenGameSupportMainGenDestClass);

        var gameNames = Cache.GamesEnum.GameIndexEnumNames;

        string gameName = Cache.GamesEnum.Name;
        string gameNameVarCase = gameName.ToVarCase();
        string gameIndexName = Cache.GamesEnum.GameIndexName;
        string gameIndexNameVarCase = gameIndexName.ToVarCase();

        w.WL("#region Autogenerated game support code");
        w.WL();

        w.WL("public enum " + gameIndexName + " : uint");
        WriteListBody(w, gameNames, isEnum: true);

        w.WL("#region Per-game constants");
        w.WL();

        WriteArrayAndGetter(w, "_gamePrefixes", "GetGamePrefix", Cache.GamesEnum.GamePrefixes, gameIndexName);
        WriteArrayAndGetter(w, "_steamAppIds", "GetGameSteamId", Cache.GamesEnum.SteamIds, gameIndexName);
        WriteArrayAndGetter(w, "_gameEditorNames", "GetGameEditorName", Cache.GamesEnum.EditorNames, gameIndexName);

        w.WL("#endregion");
        w.WL();

        #region GameToGameIndex

        w.WL("/// <summary>");
        w.WL("/// Converts a " + gameName + " to a " + gameIndexName + ". *Narrowing conversion, so make sure the game has been checked for convertibility first!");
        w.WL("/// </summary>");
        w.WL("/// <param name=\"" + gameNameVarCase + "\"></param>");
        w.WL("public static " + gameIndexName + " " + gameName + "To" + gameIndexName + "(" + gameName + " " + gameNameVarCase + ")");
        w.WL("{");
        w.WL("AssertR(GameIsKnownAndSupported(" + gameNameVarCase + "), nameof(" + gameNameVarCase + ") + \" was out of range: \" + " + gameNameVarCase + ");");
        w.WL();
        w.WL("return game switch");
        w.WL("{");
        for (int i = 0; i < gameNames.Count; i++)
        {
            string prefix = i < gameNames.Count - 1 ? gameName + "." + gameNames[i] : "_";
            string suffix = i < gameNames.Count - 1 ? "," : "";
            w.WL(prefix + " => " + gameIndexName + "." + gameNames[i] + suffix);
        }
        w.WL("};");
        w.WL("}");
        w.WL();

        #endregion

        #region GameIndexToGame

        w.WL("/// <summary>");
        w.WL("/// Converts a " + gameIndexName + " to a " + gameName + ". Widening conversion, so it will always succeed.");
        w.WL("/// </summary>");
        w.WL("/// <param name=\"" + gameIndexNameVarCase + "\"></param>");
        w.WL("public static " + gameName + " " + gameIndexName + "To" + gameName + "(" + gameIndexName + " " + gameIndexNameVarCase + ") => " + gameIndexNameVarCase + " switch");
        w.WL("{");
        for (int i = 0; i < gameNames.Count; i++)
        {
            string prefix = i < gameNames.Count - 1 ? gameIndexName + "." + gameNames[i] : "_";
            string suffix = i < gameNames.Count - 1 ? "," : "";
            w.WL(prefix + " => " + gameName + "." + gameNames[i] + suffix);
        }
        w.WL("};");
        w.WL();

        #endregion

        w.WL("#endregion");

        w.CloseClassAndNamespace();

        File.WriteAllText(destFile, w.ToString());
    }
}
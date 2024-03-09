using System.Collections.Generic;
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
        ret.EnumType = gameEnum.BaseList?.Types.Count > 0
            ? gameEnum.BaseList.Types[0].Type.ToString()
            : "int";
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
                    const int reqArgCount = 9;

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
                    bool isDarkEngine =
                        (bool)((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[3].Expression).Token
                        .Value!;
                    bool supportsMods =
                        (bool)((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[4].Expression).Token
                        .Value!;
                    bool supportsImport =
                        (bool)((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[5].Expression).Token
                        .Value!;
                    bool supportsLanguages =
                        (bool)((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[6].Expression).Token
                        .Value!;
                    bool supportsResourceDetection =
                        (bool)((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[7].Expression).Token
                        .Value!;
                    bool requiresBackupPath =
                        (bool)((LiteralExpressionSyntax)gameAttr.ArgumentList!.Arguments[8].Expression).Token
                        .Value!;

                    ret.GamePrefixes.Add(prefixArg);
                    ret.SteamIds.Add(steamIdArg);
                    ret.EditorNames.Add(editorNameArg);
                    ret.IsDarkEngine.Add(isDarkEngine);
                    ret.SupportsMods.Add(supportsMods);
                    ret.SupportsImport.Add(supportsImport);
                    ret.SupportsLanguages.Add(supportsLanguages);
                    ret.SupportsResourceDetection.Add(supportsResourceDetection);
                    ret.RequiresBackupPath.Add(requiresBackupPath);
                }
            }
            else
            {
                ret.NotKnownAndSupportedEnumNames.Add(memberName);
            }
        }

        return ret;
    }

    private static void WriteGameSupportMainGen(string destFile)
    {
        const string gameIsKnownAndSupportedFuncName = "GameIsKnownAndSupported";
        const string convertsToKnownAndSupportedFuncName = "ConvertsToKnownAndSupported";

        var w = GetWriterForClass(destFile, GenAttributes.FenGenGameSupportMainGenDestClass);

        var gameNames = Cache.GamesEnum.GameIndexEnumNames;

        string gameName = Cache.GamesEnum.Name;
        string gameNameVarCase = gameName.ToVarCase();
        string gameIndexName = Cache.GamesEnum.GameIndexName;
        string gameIndexNameVarCase = gameIndexName.ToVarCase();

        string gameToGameIndexFuncName = gameName + "To" + gameIndexName;

        w.WL("#region Autogenerated game support code");
        w.WL();

        w.WL("public const int SupportedGameCount = " + Cache.GamesEnum.GameIndexEnumNames.Count + ";");
        w.WL("public const int DarkGameCount = " + Cache.GamesEnum.IsDarkEngine.Count(static x => x) + ";");
        w.WL("public const int ModSupportingGameCount = " + Cache.GamesEnum.SupportsMods.Count(static x => x) + ";");
        w.WL("public const int ImportSupportingGameCount = " + Cache.GamesEnum.SupportsImport.Count(static x => x) + ";");
        w.WL("public const int LanguageSupportingGameCount = " + Cache.GamesEnum.SupportsLanguages.Count(static x => x) + ";");
        w.WL("public const int ResourceDetectionSupportingGameCount = " + Cache.GamesEnum.SupportsResourceDetection.Count(static x => x) + ";");
        w.WL("public const int BackupRequiringGameCount = " + Cache.GamesEnum.RequiresBackupPath.Count(static x => x) + ";");
        w.WL();

        w.WL("public enum " + gameIndexName + " : " + Cache.GamesEnum.EnumType);
        WriteListBody(w, gameNames, isEnum: true);

        w.WL("#region General");
        w.WL();

        #region GameToGameIndex

        w.WL("/// <summary>");
        w.WL("/// Converts a " + gameName + " to a " + gameIndexName + ". *Narrowing conversion, so make sure the game has been checked for convertibility first!");
        w.WL("/// </summary>");
        w.WL("/// <param name=\"" + gameNameVarCase + "\"></param>");
        w.WL("public static " + gameIndexName + " " + gameToGameIndexFuncName + "(" + gameName + " " + gameNameVarCase + ")");
        w.WL("{");
        w.WL("AssertR(" + gameIsKnownAndSupportedFuncName + "(" + gameNameVarCase + "), nameof(" + gameNameVarCase + ") + \" was out of range: \" + " + gameNameVarCase + ");");
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

        w.WL("public static bool " + gameIsKnownAndSupportedFuncName + "(" + gameName + " " + gameNameVarCase + ") =>");
        w.IncrementIndent();
        w.WL(gameNameVarCase);
        w.IncrementIndent();
        for (int i = 0; i < Cache.GamesEnum.NotKnownAndSupportedEnumNames.Count; i++)
        {
            string prefix = i == 0 ? "is" : "and";
            string suffix = i == Cache.GamesEnum.NotKnownAndSupportedEnumNames.Count - 1 ? ";" : "";
            w.WL(prefix + " not " + gameName + "." + Cache.GamesEnum.NotKnownAndSupportedEnumNames[i] + suffix);
        }
        w.DecrementIndent();
        w.DecrementIndent();
        w.WL();

        WriteConvertsToFunction("KnownAndSupported", gameIsKnownAndSupportedFuncName);

        w.WL("#endregion");
        w.WL();

        WriteArrayAndGetter("_gamePrefixes", "GetGamePrefix", Cache.GamesEnum.GamePrefixes);
        WriteArrayAndGetter("_steamAppIds", "GetGameSteamId", Cache.GamesEnum.SteamIds);
        WriteArrayAndGetter("_gameEditorNames", "GetGameEditorName", Cache.GamesEnum.EditorNames);

        WriteBoolSection("_isDark", "GameIsDark", "Dark", Cache.GamesEnum.IsDarkEngine);
        WriteBoolSection("_supportsMods", "GameSupportsMods", "ModSupporting", Cache.GamesEnum.SupportsMods);
        WriteBoolSection("_supportsImport", "GameSupportsImport", "ImportSupporting", Cache.GamesEnum.SupportsImport);
        WriteBoolSection("_supportsLanguages", "GameSupportsLanguages", "LanguageSupporting", Cache.GamesEnum.SupportsLanguages);
        WriteBoolSection("_supportsResourceDetection", "GameSupportsResourceDetection", "ResourceDetectionSupporting", Cache.GamesEnum.SupportsResourceDetection);
        WriteBoolSection("_requiresBackupPath", "GameRequiresBackupPath", "BackupPathRequiring", Cache.GamesEnum.RequiresBackupPath);

        w.WL("#endregion");

        w.CloseClassAndNamespace();

        File.WriteAllText(destFile, w.ToString());

        return;

        #region Local functions

        void WriteArrayAndGetter<T>(
            string arrayName,
            string getterName,
            List<T> items,
            bool addQuotes = true,
            bool addRegion = true)
        {
            string typeName = items[0].GetTypeName();
            if (addRegion)
            {
                w.WL("#region " + getterName);
                w.WL();
            }
            w.WL("private static readonly " + typeName + "[] " + arrayName + " =");
            WriteListBody(w, items, addQuotes: addQuotes);
            w.WL("public static " + typeName + " " + getterName + "(" + gameIndexName + " index) => " + arrayName + "[(" + Cache.GamesEnum.EnumType + ")index];");
            w.WL();
            if (addRegion)
            {
                w.WL("#endregion");
                w.WL();
            }
        }

        void WriteGetterForGameNonIndex(string getterName)
        {
            w.WL("public static bool " + getterName + "(" + gameName + " " + gameNameVarCase + ") =>");
            w.IncrementIndent();
            w.WL(gameNameVarCase + "." + convertsToKnownAndSupportedFuncName + "(out " + gameIndexName + " " + gameIndexNameVarCase + ") &&");
            w.WL(getterName + "(" + gameIndexNameVarCase + ");");
            w.DecrementIndent();
            w.WL();
        }

        void WriteConvertsToFunction(
            string convertsToSuffix,
            string getterName)
        {
            w.WL("public static bool ConvertsTo" + convertsToSuffix + "(this " + gameName + " " + gameNameVarCase + ", out " + gameIndexName + " " + gameIndexNameVarCase + ")");
            w.WL("{");
            w.WL("if (" + getterName + "(" + gameNameVarCase + "))");
            w.WL("{");
            w.WL(gameIndexNameVarCase + " = " + gameToGameIndexFuncName + "(" + gameNameVarCase + ");");
            w.WL("return true;");
            w.WL("}");
            w.WL("else");
            w.WL("{");
            w.WL(gameIndexNameVarCase + " = default;");
            w.WL("return true;");
            w.WL("}");
            w.WL("}");
            w.WL();
        }

        void WriteBoolSection(
            string arrayName,
            string getterName,
            string convertsToSuffix,
            List<bool> boolsList)
        {
            w.WL("#region " + getterName);
            w.WL();
            WriteArrayAndGetter(arrayName, getterName, boolsList, addQuotes: false, addRegion: false);
            WriteGetterForGameNonIndex(getterName);
            WriteConvertsToFunction(convertsToSuffix, getterName);
            w.WL("#endregion");
            w.WL();
        }

        #endregion
    }
}

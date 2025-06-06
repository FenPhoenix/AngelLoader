﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen;

internal static class Language
{
    private sealed class IniItem
    {
        internal bool DoNotWrite;
        internal string Key = "";
        internal string Value = "";
        internal bool IsComment;
        internal string PlaceAfterKey = "";
    }

    private sealed class IniSection : List<IniItem>
    {
        internal IniSection(string name) => Name = name;
        internal readonly string Name;
    }

    internal static void Generate(
        string mainSourceFile,
        string updaterSourceFile,
        string perGameLangGetterDestFile,
        string langIniFile,
        string testLangIniFile,
        string testLang2IniFile)
    {
        var (sections, perGameSets) = ReadSource(mainSourceFile);
        var (updaterSections, _) = ReadSource(updaterSourceFile);

        foreach (IniSection updaterSection in updaterSections)
        {
            IniSection? mainSection = sections.Find(x => x.Name == updaterSection.Name);
            if (mainSection != null)
            {
                foreach (IniItem updaterSectionItem in updaterSection)
                {
                    if (updaterSectionItem.Key.IsEmpty() || updaterSectionItem.IsComment)
                    {
                        mainSection.Add(updaterSectionItem);
                    }
                    else if (mainSection.Find(x => x.Key == updaterSectionItem.Key) == null)
                    {
                        IniItem? placeAfterKeyItem = updaterSectionItem.PlaceAfterKey.IsEmpty()
                            ? null
                            : mainSection.Find(x => x.Key == updaterSectionItem.PlaceAfterKey);

                        if (placeAfterKeyItem != null)
                        {
                            mainSection.Insert(mainSection.IndexOf(placeAfterKeyItem) + 1, updaterSectionItem);
                        }
                        else
                        {
                            mainSection.Add(updaterSectionItem);
                        }
                    }
                }
            }
            else
            {
                sections.Add(updaterSection);
            }
        }

        WritePerGameStringGetterFile(perGameLangGetterDestFile, perGameSets);
        WriteIniFile(langIniFile, sections);
        if (!testLangIniFile.IsEmpty())
        {
            Directory.CreateDirectory(Path.GetDirectoryName(testLangIniFile)!);
            WriteIniFile(testLangIniFile, sections, test: true);
            WriteIniFile(testLang2IniFile, sections, test2: true);
        }
    }

    private static (List<IniSection> Sections, Dictionary<string, (string Field, string Section)[]> PerGameSets)
    ReadSource(string file)
    {
        string code = File.ReadAllText(file);
        SyntaxTree tree = ParseTextFast(code);

        var (markedMember, _) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, GenAttributes.FenGenLocalizationSourceClass);
        var lTextClass = (ClassDeclarationSyntax)markedMember;

        SyntaxNode[] childNodes = lTextClass.ChildNodes().ToArray();
        var classInstanceDict = new Dictionary<string, string>(StringComparer.Ordinal);

        // Once through first to get the instance types and names
        for (int i = 0; i < childNodes.Length; i++)
        {
            if (childNodes[i] is FieldDeclarationSyntax field)
            {
                classInstanceDict.Add(field.Declaration.Type.ToString(), field.Declaration.Variables[0].Identifier.Text);
            }
        }

        var sections = new List<IniSection>();

        var perGameSets = new Dictionary<string, (string Field, string Section)[]>(StringComparer.Ordinal);

        // Now through again to get the language string names from the nested classes
        foreach (SyntaxNode cn in childNodes)
        {
            if (cn is not ClassDeclarationSyntax childClass) continue;

            SyntaxNode[] members = childClass.ChildNodes()
                .Where(static x => x.IsKind(SyntaxKind.FieldDeclaration) || x.IsKind(SyntaxKind.PropertyDeclaration))
                .ToArray();

            if (members.Length == 0) continue;

            string childClassIdentifier = childClass.Identifier.ToString();
            string sectionInstanceName = classInstanceDict[childClassIdentifier];
            var section = new IniSection(sectionInstanceName);

            int gameIndex = -1;
            string memberGameSetGetterName = "";

            foreach (SyntaxNode m in members)
            {
                bool doNotWriteThis = false;
                string placeAfterKey = "";

                var member = (MemberDeclarationSyntax)m;
                foreach (AttributeListSyntax attrList in member.AttributeLists)
                {
                    foreach (AttributeSyntax attr in attrList.Attributes)
                    {
                        switch (attr.Name.ToString())
                        {
                            case GenAttributes.FenGenPlaceAfterKey:
                            {
                                var argList = attr.ArgumentList;
                                if (argList != null)
                                {
                                    var arg = argList.Arguments[0];
                                    placeAfterKey = ((LiteralExpressionSyntax)arg.Expression).Token.ValueText;
                                }
                                break;
                            }
                            case GenAttributes.FenGenGameSet:
                            {
                                gameIndex = 0;
                                var argList = attr.ArgumentList;
                                if (argList != null)
                                {
                                    var arg = argList.Arguments[0];
                                    memberGameSetGetterName = ((LiteralExpressionSyntax)arg.Expression).Token.ValueText;
                                    perGameSets.Add(memberGameSetGetterName, new (string, string)[Cache.GamesEnum.GameIndexEnumNames.Count]);
                                }
                                break;
                            }
                            case GenAttributes.FenGenComment:
                            {
                                var argList = attr.ArgumentList;
                                if (argList != null)
                                {
                                    string fValue = "";
                                    var args = argList.Arguments;
                                    for (int i = 0; i < args.Count; i++)
                                    {
                                        if (i > 0) fValue += "\r\n";
                                        if (args[i].Expression is LiteralExpressionSyntax literal)
                                        {
                                            fValue += literal.Token.ValueText;
                                        }
                                        else
                                        {
                                            ThrowErrorAndTerminate(
                                                nameof(Language) + "." + nameof(ReadSource) +
                                                ":\r\n" + GenAttributes.FenGenComment +
                                                " arguments contained something other than an un-concatenated string literal.");
                                        }
                                    }

                                    section.Add(new IniItem { Value = fValue, IsComment = true });
                                }
                                break;
                            }
                            case GenAttributes.FenGenBlankLine:
                            {
                                var argList = attr.ArgumentList;
                                int blankLinesToAdd = argList?.Arguments.Count > 0
                                    ? (int)((LiteralExpressionSyntax)argList.Arguments[0].Expression).Token.Value!
                                    : 1;

                                for (int i = 0; i < blankLinesToAdd; i++)
                                {
                                    section.Add(new IniItem());
                                }
                                break;
                            }
                            case GenAttributes.FenGenDoNotWrite:
                                doNotWriteThis = true;
                                break;
                        }
                    }
                }

                string fName;
                EqualsValueClauseSyntax? initializer;
                if (m.IsKind(SyntaxKind.FieldDeclaration))
                {
                    var fds = (FieldDeclarationSyntax)m;

                    string type = fds.Declaration.Type.ToString();
                    if (!TypeIsString(type)) continue;

                    var vds = fds.Declaration.Variables[0];
                    fName = vds.Identifier.ToString();
                    initializer = vds.Initializer;
                }
                else
                {
                    var pds = (PropertyDeclarationSyntax)m;

                    string type = pds.Type.ToString();
                    if (!TypeIsString(type)) continue;

                    fName = pds.Identifier.ToString();
                    initializer = pds.Initializer;
                }

                if (initializer == null)
                {
                    string type = m.IsKind(SyntaxKind.FieldDeclaration) ? "field" : "property";
                    ThrowErrorAndTerminate(nameof(Language) + ":\r\nFound a " + type + " without an initializer in " + file);
                }

                if (gameIndex > -1)
                {
                    if (gameIndex >= Cache.GamesEnum.GameIndexEnumNames.Count)
                    {
                        gameIndex = -1;
                        memberGameSetGetterName = "";
                    }
                    else
                    {
                        perGameSets[memberGameSetGetterName][gameIndex] = (Field: fName, Section: section.Name);
                        gameIndex++;
                    }
                }

                section.Add(new IniItem
                {
                    Key = fName,
                    Value = ((LiteralExpressionSyntax)initializer.Value).Token.ValueText,
                    DoNotWrite = doNotWriteThis,
                    PlaceAfterKey = placeAfterKey,
                });
            }

            sections.Add(section);
        }

        return (sections, perGameSets);

        static bool TypeIsString(string type) => type is "string" or "String" or "System.String";
    }

    private static void WritePerGameStringGetterFile(
        string destFile,
        Dictionary<string, (string Field, string Section)[]> perGameSets)
    {
        var w = GetWriterForClass(destFile, GenAttributes.FenGenLocalizedGameNameGetterDestClass);

        if (perGameSets.Count != 0)
        {
            w.WL("#region Autogenerated per-game localized string getters");
            w.WL();

            int count = Cache.GamesEnum.GameIndexEnumNames.Count;
            string gameIndexName = Cache.GamesEnum.GameIndexName;
            string gameIndexNameVarCase = gameIndexName.ToVarCase();
            foreach (var gameSet in perGameSets)
            {
                w.WL("internal static string " + gameSet.Key + "(" + gameIndexName + " " + gameIndexNameVarCase + ") => " + gameIndexNameVarCase + " switch");
                w.WL("{");
                // Loop through GameIndex count rather than game set count, so if our count mismatches we crash and error
                for (int i = 0; i < count; i++)
                {
                    var game = gameSet.Value[i];
                    string prefix = i < count - 1
                        ? gameIndexName + "." + Cache.GamesEnum.GameIndexEnumNames[i] + " => "
                        : "_ => ";
                    string line = prefix + "LText." + game.Section + "." + game.Field + ",";
                    w.WL(line);
                }
                w.WL("};");
                w.WL();
            }

            w.WL("#endregion");
        }
        w.CloseClassAndNamespace();

        File.WriteAllText(destFile, w.ToString());
    }

    private static void WriteIniFile(string langIniFile, List<IniSection> sections, bool test = false, bool test2 = false)
    {
        var sb = new StringBuilder();
        sb.AppendLine("; This is an AngelLoader language file.");
        sb.AppendLine("; This file MUST be saved with UTF8 encoding in order to guarantee correct display of strings.");
        sb.AppendLine("; Lines beginning with ; are comments to assist the translator and are not used by AngelLoader itself,");
        sb.AppendLine("; so translation of these lines is not required.");
        sb.AppendLine();

        string[] linebreaks = { "\r\n", "\r", "\n" };

        string testPrefix = test || test2 ? "█" : "";
        string testSuffix = test2 ? " sd fdsf sd fdsf dsf dsf dsf dsf sd" : "";
        for (int i = 0; i < sections.Count; i++)
        {
            IniSection section = sections[i];

            sb.Append('[');
            sb.Append(section.Name);
            sb.AppendLine("]");
            foreach (IniItem item in section)
            {
                if (item.Key.IsEmpty() && item.Value.IsEmpty())
                {
                    sb.AppendLine();
                }
                else if (item.IsComment && !item.Value.IsEmpty())
                {
                    string[] comments = item.Value.Split(linebreaks, StringSplitOptions.None);
                    foreach (string c in comments)
                    {
                        sb.Append("; ");
                        sb.AppendLine(c);
                    }
                }
                else if (!item.DoNotWrite)
                {
                    string val =
                        test && item.Key == "TranslatedLanguageName" ? "TéstLang" :
                        test2 && item.Key == "TranslatedLanguageName" ? "TéstLangLong" :
                        testPrefix + item.Value + testSuffix;
                    sb.Append(item.Key);
                    sb.Append('=');
                    sb.AppendLine(val);
                }
            }
            if (i < sections.Count - 1) sb.AppendLine();
        }

        File.WriteAllText(langIniFile, sb.ToString(), Encoding.UTF8);
    }
}

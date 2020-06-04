﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen
{
    // PERF_TODO: Roslyn is still the big fat slug, with ParseText() taking hundreds of ms.
    // Revert to parsing everything manually for speed. This is ludicrously untenable, who would even accept a
    // multi-second tack-on to build time for code generation this simple?!
    internal static class LanguageGen
    {
        private class IniItem
        {
            // Keep these "" so they can be non-null defaults that don't have to be set
            internal string Key = "";
            internal string Value = "";
            internal bool IsComment;
        }

        private class NamedDictionary : List<IniItem>
        {
            public NamedDictionary(string name) => Name = name;
            internal readonly string Name;
        }

        internal static void
        Generate(string sourceFile, string destFile, string langIniFile, string testLangIniFile = "")
        {
            var (langClassName, dictList) = ReadSource(sourceFile);

            WriteDest(langClassName, dictList, destFile, langIniFile, testLangIniFile: testLangIniFile);
        }

        private static (string LangClassName, List<NamedDictionary> Dict)
        ReadSource(string file)
        {
            const string FenGenLocalizationClassAttribute = "FenGenLocalizationClassAttribute";

            var retDict = new List<NamedDictionary>();

            string code = File.ReadAllText(file);
            var tree = ParseTextFast(code);

            var attrMarkedClasses = new List<ClassDeclarationSyntax>();

            var nodes = tree.GetCompilationUnitRoot().DescendantNodesAndSelf();
            foreach (SyntaxNode n in nodes)
            {
                if (!n.IsKind(SyntaxKind.ClassDeclaration)) continue;

                var classItem = (ClassDeclarationSyntax)n;

                if (classItem.AttributeLists.Count > 0 && classItem.AttributeLists[0].Attributes.Count > 0)
                {
                    foreach (var attr in classItem.AttributeLists[0].Attributes)
                    {
                        if (GetAttributeName(attr.Name.ToString(), FenGenLocalizationClassAttribute))
                        {
                            attrMarkedClasses.Add(classItem);
                        }
                    }
                }
            }

            if (attrMarkedClasses.Count > 1)
            {
                const string multipleUsesError = "ERROR: Multiple uses of attribute '" + FenGenLocalizationClassAttribute + "'.";
                ThrowErrorAndTerminate(multipleUsesError);
            }
            else if (attrMarkedClasses.Count == 0)
            {
                const string noneFoundError = "ERROR: No uses of attribute '" + FenGenLocalizationClassAttribute +
                    "' (No marked localization class found)";
                ThrowErrorAndTerminate(noneFoundError);
            }

            var LTextClass = attrMarkedClasses[0];

            foreach (SyntaxNode item in LTextClass.DescendantNodes())
            {
                if (!item.IsKind(SyntaxKind.ClassDeclaration)) continue;

                var subClass = (ClassDeclarationSyntax)item;

                var fields = subClass.DescendantNodes()
                    .Where(x => x.IsKind(SyntaxKind.VariableDeclaration) ||
                                x.IsKind(SyntaxKind.PropertyDeclaration) ||
                                x.IsKind(SyntaxKind.Attribute))
                    .ToArray();

                if (fields.Length == 0) continue;

                var dict = new NamedDictionary(subClass.Identifier.ToString());
                foreach (SyntaxNode f in fields)
                {
                    string fName = "";
                    string fValue = "";
                    bool isComment = false;

                    int blankLinesToAdd = 0;

                    if (f is AttributeSyntax attr)
                    {
                        if (GetAttributeName(attr.Name.ToString(), "FenGenComment"))
                        {
                            var exp = (LiteralExpressionSyntax)attr.ArgumentList.Arguments[0].Expression;
                            fValue = exp.Token.ValueText;
                            isComment = true;
                        }
                        else if (GetAttributeName(attr.Name.ToString(), "FenGenBlankLine"))
                        {
                            var args = attr.ArgumentList;
                            if (args == null || args.Arguments.Count == 0)
                            {
                                blankLinesToAdd = 1;
                            }
                            else if (args.Arguments.Count == 1)
                            {
                                blankLinesToAdd = (int)((LiteralExpressionSyntax)args.Arguments[0].Expression).Token.Value;
                            }
                        }
                    }
                    else if (f is VariableDeclarationSyntax vds)
                    {
                        var v = vds.Variables[0];
                        fName = v.Identifier.ToString();
                        fValue = ((LiteralExpressionSyntax)v.Initializer.Value).Token.ValueText;
                    }
                    else if (f is PropertyDeclarationSyntax pds)
                    {
                        fName = pds.Identifier.ToString();
                        fValue = ((LiteralExpressionSyntax)pds.Initializer.Value).Token.ValueText;
                    }

                    if (blankLinesToAdd > 0)
                    {
                        for (int i = 0; i < blankLinesToAdd; i++)
                        {
                            dict.Add(new IniItem());
                        }
                    }
                    else
                    {
                        dict.Add(new IniItem { Key = fName, Value = fValue, IsComment = isComment });
                    }
                }

                retDict.Add(dict);
            }

            return (LTextClass.Identifier.ToString(), retDict);
        }

        private static void WriteDest(string langClassName, List<NamedDictionary> dictList, string destFile,
                                      string langIniFile, string testLangIniFile = "")
        {
            #region Find the class we're going to write to

            const string FenGenLocalizationReadClassAttribute = "FenGenLocalizationReadWriteClass";

            string code = File.ReadAllText(destFile);
            var tree = ParseTextFast(code);

            var attrMarkedClasses = new List<ClassDeclarationSyntax>();

            foreach (SyntaxNode item in tree.GetCompilationUnitRoot().DescendantNodes())
            {
                if (!item.IsKind(SyntaxKind.ClassDeclaration)) continue;

                var classItem = (ClassDeclarationSyntax)item;

                if (classItem.AttributeLists.Count > 0 && classItem.AttributeLists[0].Attributes.Count > 0)
                {
                    foreach (var attr in classItem.AttributeLists[0].Attributes)
                    {
                        if (GetAttributeName(attr.Name.ToString(), FenGenLocalizationReadClassAttribute))
                        {
                            attrMarkedClasses.Add(classItem);
                        }
                    }
                }
            }

            if (attrMarkedClasses.Count > 1)
            {
                const string multipleUsesError = "ERROR: Multiple uses of attribute '" + FenGenLocalizationReadClassAttribute + "'.";
                ThrowErrorAndTerminate(multipleUsesError);
            }
            else if (attrMarkedClasses.Count == 0)
            {
                const string noneFoundError = "ERROR: No uses of attribute '" + FenGenLocalizationReadClassAttribute +
                                              "' (No marked localization class found)";
                ThrowErrorAndTerminate(noneFoundError);
            }

            var iniClass = attrMarkedClasses[0];

            // Make the whole thing fail so I can get a fail message in AngelLoader on build
            if (iniClass == null) throw new ArgumentNullException();

            string iniClassString = iniClass.ToString();
            string classDeclLine = iniClassString.Substring(0, iniClassString.IndexOf('{'));

            string codeBlock =
                code.Substring(0, iniClass.GetLocation().SourceSpan.Start + classDeclLine.Length)
                    .TrimEnd() + "\r\n";

            #endregion

            // TODO: Roslyn-ize this
            // Requires figuring out the idiotic black magic of how to CONSTRUCT code with Roslyn, but hey...
            #region Write dest file

            // This may look janky and it may be error-prone to write, but at least I know HOW to write the damn
            // thing. You type out your code. Just right there. There it is. As any half-way sane system would
            // work. Roslyn? Forget it. Zero documentation and you have to build the Burj Khalifa just to get it
            // to do anything. And you bang your head against a wall for five hours every time you need to write
            // the next statement. Forget. It.
            using (var sw = new StreamWriter(destFile))
            {
                void swl(int indent, string str) => sw.WriteLine(Indent(indent) + str);

                sw.Write(codeBlock);
                swl(1, "{");
                swl(2, GenMessages.Method);
                swl(2, "internal static void ReadLocalizationIni(string file)");
                swl(2, "{");
                swl(3, "string[] lines = File.ReadAllLines(file, Encoding.UTF8);");
                swl(3, "for (int i = 0; i < lines.Length; i++)");
                swl(3, "{");
                swl(4, "string lineT = lines[i].Trim();");
                bool sectElseIf = false;
                foreach (var dict in dictList)
                {
                    swl(4, (sectElseIf ? "else " : "") + "if (lineT == \"[" + dict.Name + "]\")");
                    swl(4, "{");
                    swl(5, "while (i < lines.Length - 1)");
                    swl(5, "{");
                    swl(6, "string lt = lines[i + 1].TrimStart();");

                    bool keysElseIf = false;
                    foreach (IniItem item in dict)
                    {
                        if (item.Key.IsEmpty()) continue;
                        swl(6, (keysElseIf ? "else " : "") + "if (lt.StartsWithFast_NoNullChecks(\"" + item.Key + "=\"))");
                        swl(6, "{");
                        swl(7, langClassName + "." + dict.Name + "." + item.Key +
                               " = lt.Substring(" + (item.Key + "=").Length + ");");
                        swl(6, "}");
                        if (!keysElseIf) keysElseIf = true;
                    }
                    swl(6, "else if (lt.Length > 0 && lt[0] == '[' && lt[lt.Length - 1] == ']')");
                    swl(6, "{");
                    swl(7, "break;");
                    swl(6, "}");
                    swl(6, "i++;");
                    swl(5, "}");
                    swl(4, "}");
                    if (!sectElseIf) sectElseIf = true;
                }
                swl(3, "}");
                swl(2, "}");
                swl(1, "}");
                swl(0, "}");
            }

            #endregion

            WriteIniFile(langIniFile, dictList);
            if (!testLangIniFile.IsEmpty()) WriteIniFile(testLangIniFile, dictList, test: true);
        }

        private static void WriteIniFile(string langIniFile, List<NamedDictionary> dictList, bool test = false)
        {
            using var sw = new StreamWriter(langIniFile, append: false, Encoding.UTF8);

            string testPrefix = test ? "█" : "";

            sw.WriteLine("; This is an AngelLoader language file.");
            sw.WriteLine("; This file MUST be saved with UTF8 encoding in order to guarantee correct display of strings.");
            sw.WriteLine();

            for (int i = 0; i < dictList.Count; i++)
            {
                var dict = dictList[i];

                sw.WriteLine("[" + dict.Name + "]");
                foreach (IniItem item in dict)
                {
                    if (item.IsComment)
                    {
                        if (!item.Value.IsEmpty())
                        {
                            string[] comments = item.Value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            foreach (string c in comments) sw.WriteLine("; " + c);
                        }
                    }
                    else if (item.Key.IsEmpty() && item.Value.IsEmpty())
                    {
                        sw.WriteLine();
                    }
                    else
                    {
                        if (test && item.Key == "TranslatedLanguageName")
                        {
                            sw.WriteLine(item.Key + "=" + "TéstLang");
                        }
                        else
                        {
                            sw.WriteLine(item.Key + "=" + testPrefix + item.Value);
                        }
                    }
                }
                if (i < dictList.Count - 1) sw.WriteLine();
            }
        }
    }
}

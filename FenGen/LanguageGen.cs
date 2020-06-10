using System;
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
    internal static class Language
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
            var retDict = new List<NamedDictionary>();

            string code = File.ReadAllText(file);
            var tree = ParseTextFast(code);

            var (member, _) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, GenAttributes.FenGenLocalizationSourceClass);
            var LTextClass = (ClassDeclarationSyntax)member;

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

                // TODO: Un-hardcode this "remove _Class suffix" stuff
                string _name = subClass.Identifier.ToString();
                _name = _name.Substring(0, _name.IndexOf("_Class", StringComparison.InvariantCulture));
                var dict = new NamedDictionary(_name);
                foreach (SyntaxNode f in fields)
                {
                    string fName = "";
                    string fValue = "";
                    bool isComment = false;

                    int blankLinesToAdd = 0;

                    if (f is AttributeSyntax attr)
                    {
                        if (GetAttributeName(attr.Name.ToString(), GenAttributes.FenGenComment))
                        {
                            var argList = attr.ArgumentList;
                            if (argList != null)
                            {
                                for (int i = 0; i < argList.Arguments.Count; i++)
                                {
                                    if (i > 0) fValue += "\r\n";
                                    fValue += ((LiteralExpressionSyntax)argList.Arguments[i].Expression).Token.ValueText;
                                }
                                isComment = true;
                            }
                        }
                        else if (GetAttributeName(attr.Name.ToString(), GenAttributes.FenGenBlankLine))
                        {
                            var args = attr.ArgumentList;
                            if (args == null || args.Arguments.Count == 0)
                            {
                                blankLinesToAdd = 1;
                            }
                            else if (args.Arguments.Count == 1)
                            {
                                blankLinesToAdd = (int)((LiteralExpressionSyntax)args.Arguments[0].Expression).Token.Value!;
                            }
                        }
                    }
                    else if (f is VariableDeclarationSyntax vds)
                    {
                        var v = vds.Variables[0];

                        var initializer = v.Initializer;
                        if (initializer == null)
                        {
                            ThrowErrorAndTerminate(nameof(Language) + ":\r\n" +
                                                   "Found a variable without an initializer in " + file);
                        }

                        fName = v.Identifier.ToString();
                        fValue = ((LiteralExpressionSyntax)initializer!.Value).Token.ValueText;
                    }
                    else if (f is PropertyDeclarationSyntax pds)
                    {
                        var initializer = pds.Initializer;
                        if (initializer == null)
                        {
                            ThrowErrorAndTerminate(nameof(Language) + ":\r\n" +
                                                   "Found a property without an initializer in " + file);
                        }

                        fName = pds.Identifier.ToString();
                        fValue = ((LiteralExpressionSyntax)initializer!.Value).Token.ValueText;
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

            string lTextClassId = LTextClass.Identifier.ToString();
            lTextClassId = lTextClassId.Substring(0, lTextClassId.IndexOf("_Class", StringComparison.InvariantCulture));
            return (lTextClassId, retDict);
        }

        private static void WriteDest(string langClassName, List<NamedDictionary> dictList, string destFile,
                                      string langIniFile, string testLangIniFile = "")
        {
            #region Find the class we're going to write to

            string code = File.ReadAllText(destFile);
            var tree = ParseTextFast(code);

            var (member, _) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, GenAttributes.FenGenLocalizationDestClass);
            var iniClass = (ClassDeclarationSyntax)member;

            string iniClassString = iniClass.ToString();
            string classDeclLine = iniClassString.Substring(0, iniClassString.IndexOf('{'));

            string codeBlock = code
                .Substring(0, iniClass.GetLocation().SourceSpan.Start + classDeclLine.Length)
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
            var sb = new StringBuilder();

            var w = new Generators.IndentingWriter(sb, startingIndent: 1);

            sb.Append(codeBlock);
            w.WL("{");
            w.WL(GenMessages.Method);
            w.WL("internal static void ReadLocalizationIni(string file)");
            w.WL("{");
            w.WL("LText = new LText_Class();");
            w.WL("string[] lines = File.ReadAllLines(file, Encoding.UTF8);");
            w.WL("for (int i = 0; i < lines.Length; i++)");
            w.WL("{");
            w.WL("string lineT = lines[i].Trim();");
            bool sectElseIf = false;
            foreach (var dict in dictList)
            {
                w.WL((sectElseIf ? "else " : "") + "if (lineT == \"[" + dict.Name + "]\")");
                w.WL("{");
                w.WL("while (i < lines.Length - 1)");
                w.WL("{");
                w.WL("string lt = lines[i + 1].TrimStart();");

                bool keysElseIf = false;
                foreach (IniItem item in dict)
                {
                    if (item.Key.IsEmpty()) continue;
                    w.WL((keysElseIf ? "else " : "") + "if (lt.StartsWithFast_NoNullChecks(\"" + item.Key + "=\"))");
                    w.WL("{");
                    w.WL(langClassName + "." + dict.Name + "." + item.Key + " = lt.Substring(" + (item.Key + "=").Length + ");");
                    w.WL("}");
                    if (!keysElseIf) keysElseIf = true;
                }
                w.WL("else if (lt.Length > 0 && lt[0] == '[' && lt[lt.Length - 1] == ']')");
                w.WL("{");
                w.WL("break;");
                w.WL("}");
                w.WL("i++;");
                w.WL("}");
                w.WL("}");
                if (!sectElseIf) sectElseIf = true;
            }
            w.WL("}");
            w.WL("}");
            w.WL("}");
            w.WL("}");

            File.WriteAllText(destFile, sb.ToString());

            #endregion

            WriteIniFile(langIniFile, dictList);
            if (!testLangIniFile.IsEmpty()) WriteIniFile(testLangIniFile, dictList, test: true);
        }

        private static void WriteIniFile(string langIniFile, List<NamedDictionary> dictList, bool test = false)
        {
            var sb = new StringBuilder();

            string testPrefix = test ? "█" : "";

            sb.AppendLine("; This is an AngelLoader language file.");
            sb.AppendLine("; This file MUST be saved with UTF8 encoding in order to guarantee correct display of strings.");
            sb.AppendLine();

            for (int i = 0; i < dictList.Count; i++)
            {
                var dict = dictList[i];

                sb.AppendLine("[" + dict.Name + "]");
                foreach (IniItem item in dict)
                {
                    if (item.IsComment)
                    {
                        if (!item.Value.IsEmpty())
                        {
                            string[] comments = item.Value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                            foreach (string c in comments) sb.AppendLine("; " + c);
                        }
                    }
                    else if (item.Key.IsEmpty() && item.Value.IsEmpty())
                    {
                        sb.AppendLine();
                    }
                    else
                    {
                        if (test && item.Key == "TranslatedLanguageName")
                        {
                            sb.AppendLine(item.Key + "=" + "TéstLang");
                        }
                        else
                        {
                            sb.AppendLine(item.Key + "=" + testPrefix + item.Value);
                        }
                    }
                }
                if (i < dictList.Count - 1) sb.AppendLine();
            }

            File.WriteAllText(langIniFile, sb.ToString(), Encoding.UTF8);
        }
    }
}

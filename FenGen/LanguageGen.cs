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
    internal static class Language
    {
        private class IniItem
        {
            internal string Key = "";
            internal string Value = "";
            internal bool IsComment;
        }

        private class NamedDictionary : List<IniItem>
        {
            internal NamedDictionary(string name) => Name = name;
            internal readonly string Name;
        }

        internal static void
        Generate(string sourceFile, string destFile, string langIniFile, string testLangIniFile = "")
        {
            var (langClassName, sections) = ReadSource(sourceFile);

            WriteDest(langClassName, sections, destFile, langIniFile, testLangIniFile);
        }

        private static (string LangClassName, List<NamedDictionary> Dict)
        ReadSource(string file)
        {
            string code = File.ReadAllText(file);
            var tree = ParseTextFast(code);

            var (markedMember, _) = GetAttrMarkedItem(tree, SyntaxKind.ClassDeclaration, GenAttributes.FenGenLocalizationSourceClass);
            var lTextClass = (ClassDeclarationSyntax)markedMember;

            var childNodes = lTextClass.ChildNodes().ToArray();
            var classInstanceDict = new Dictionary<string, string>();

            // Once through first to get the instance types and names
            for (int i = 0; i < childNodes.Length; i++)
            {
                if (childNodes[i] is FieldDeclarationSyntax field)
                {
                    classInstanceDict.Add(field.Declaration.Type.ToString(), field.Declaration.Variables[0].Identifier.Text);
                }
            }

            var retDict = new List<NamedDictionary>();

            // Now through again to get the language string names from the nested classes
            foreach (SyntaxNode cn in childNodes)
            {
                if (!(cn is ClassDeclarationSyntax childClass)) continue;

                SyntaxNode[] members = childClass.ChildNodes()
                    .Where(x => x.IsKind(SyntaxKind.FieldDeclaration) || x.IsKind(SyntaxKind.PropertyDeclaration))
                    .ToArray();

                if (members.Length == 0) continue;

                string sectionInstanceName = classInstanceDict[childClass.Identifier.ToString()];
                var dict = new NamedDictionary(sectionInstanceName);

                foreach (SyntaxNode m in members)
                {
                    if (!m.IsKind(SyntaxKind.FieldDeclaration) && !m.IsKind(SyntaxKind.PropertyDeclaration))
                    {
                        continue;
                    }

                    var member = (MemberDeclarationSyntax)m;
                    foreach (AttributeListSyntax attrList in member.AttributeLists)
                    {
                        foreach (AttributeSyntax attr in attrList.Attributes)
                        {
                            switch (attr.Name.ToString())
                            {
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
                                            fValue += ((LiteralExpressionSyntax)args[i].Expression).Token.ValueText;
                                        }

                                        dict.Add(new IniItem { Value = fValue, IsComment = true });
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
                                        dict.Add(new IniItem());
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    string fName;
                    EqualsValueClauseSyntax? initializer;
                    if (m.IsKind(SyntaxKind.FieldDeclaration))
                    {
                        var vds = ((FieldDeclarationSyntax)m).Declaration.Variables[0];
                        fName = vds.Identifier.ToString();
                        initializer = vds.Initializer;
                    }
                    else
                    {
                        var pds = (PropertyDeclarationSyntax)m;
                        fName = pds.Identifier.ToString();
                        initializer = pds.Initializer;
                    }

                    if (initializer == null)
                    {
                        string type = m.IsKind(SyntaxKind.FieldDeclaration) ? "field" : "property";
                        ThrowErrorAndTerminate(nameof(Language) + ":\r\n" + "Found a " + type + " without an initializer in " + file);
                    }

                    dict.Add(new IniItem { Key = fName, Value = ((LiteralExpressionSyntax)initializer!.Value).Token.ValueText });
                }

                retDict.Add(dict);
            }

            string lTextClassId = lTextClass.Identifier.ToString();
            return (lTextClassId, retDict);
        }

        private static void WriteDest(string langClassName, List<NamedDictionary> sections, string destFile,
                                      string langIniFile, string testLangIniFile)
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
            w.WL("[MustUseReturnValue]");
            w.WL("internal static " + langClassName + " ReadLocalizationIni(string file)");
            w.WL("{");
            w.WL("var ret = new " + langClassName + "();");
            w.WL("string[] lines = File.ReadAllLines(file, Encoding.UTF8);");
            w.WL("for (int i = 0; i < lines.Length; i++)");
            w.WL("{");
            w.WL("string lineT = lines[i].Trim();");
            bool sectElseIf = false;
            foreach (var section in sections)
            {
                w.WL((sectElseIf ? "else " : "") + "if (lineT == \"[" + section.Name + "]\")");
                w.WL("{");
                w.WL("while (i < lines.Length - 1)");
                w.WL("{");
                w.WL("string lt = lines[i + 1].TrimStart();");

                bool keysElseIf = false;
                foreach (IniItem item in section)
                {
                    if (item.Key.IsEmpty()) continue;
                    w.WL((keysElseIf ? "else " : "") + "if (lt.StartsWithFast_NoNullChecks(\"" + item.Key + "=\"))");
                    w.WL("{");
                    w.WL("ret." + section.Name + "." + item.Key + " = lt.Substring(" + (item.Key + "=").Length + ");");
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
            w.WL();
            w.WL("return ret;");
            w.WL("}");
            w.WL("}");
            w.WL("}");

            File.WriteAllText(destFile, sb.ToString());

            #endregion

            WriteIniFile(langIniFile, sections);
            if (!testLangIniFile.IsEmpty()) WriteIniFile(testLangIniFile, sections, test: true);
        }

        private static void WriteIniFile(string langIniFile, List<NamedDictionary> sections, bool test = false)
        {
            var sb = new StringBuilder();
            sb.AppendLine("; This is an AngelLoader language file.");
            sb.AppendLine("; This file MUST be saved with UTF8 encoding in order to guarantee correct display of strings.");
            sb.AppendLine();

            string testPrefix = test ? "█" : "";
            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];

                sb.AppendLine("[" + section.Name + "]");
                foreach (IniItem item in section)
                {
                    if (item.Key.IsEmpty() && item.Value.IsEmpty())
                    {
                        sb.AppendLine();
                    }
                    else if (item.IsComment && !item.Value.IsEmpty())
                    {
                        string[] comments = item.Value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                        foreach (string c in comments) sb.AppendLine("; " + c);
                    }
                    else
                    {
                        string val = test && item.Key == "TranslatedLanguageName" ? "TéstLang" : testPrefix + item.Value;
                        sb.AppendLine(item.Key + "=" + val);
                    }
                }
                if (i < sections.Count - 1) sb.AppendLine();
            }

            File.WriteAllText(langIniFile, sb.ToString(), Encoding.UTF8);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.CommonStatic;
using static FenGen.Methods;

namespace FenGen
{
    internal class LanguageGen
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

        internal void Generate(string sourceFile, string destFile, string langIniFile)
        {
            var (langClassName, dictList) = ReadSource(sourceFile);

            WriteDest(langClassName, dictList, destFile, langIniFile);
        }

        private static (string LangClassName, List<NamedDictionary> Dict)
        ReadSource(string file)
        {
            var retDict = new List<NamedDictionary>();

            var code = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(code);

            ClassDeclarationSyntax LTextClass = null;
            foreach (var item in tree.GetCompilationUnitRoot().DescendantNodes())
            {
                if (!item.IsKind(SyntaxKind.ClassDeclaration)) continue;

                var classItem = (ClassDeclarationSyntax)item;

                if (classItem.AttributeLists.Count > 0 && classItem.AttributeLists[0].Attributes.Count > 0 &&
                    classItem.AttributeLists[0].Attributes.Any(x =>
                        GetAttributeName(x.Name.ToString(), "FenGenLocalizationClass")))
                {
                    LTextClass = classItem;
                    break;
                }
            }

            // Make the whole thing fail so I can get a fail message in AngelLoader on build
            if (LTextClass == null) throw new ArgumentNullException();

            foreach (var item in LTextClass.DescendantNodes())
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
                foreach (var f in fields)
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

        private static void WriteDest(string langClassName, List<NamedDictionary> dictList, string destFile, string langIniFile)
        {
            #region Read existing dest code

            var code = File.ReadAllText(destFile);
            var tree = CSharpSyntaxTree.ParseText(code);

            ClassDeclarationSyntax IniClass = null;
            foreach (var item in tree.GetCompilationUnitRoot().DescendantNodes())
            {
                if (!item.IsKind(SyntaxKind.ClassDeclaration)) continue;

                var classItem = (ClassDeclarationSyntax)item;

                if (classItem.AttributeLists.Count > 0 && classItem.AttributeLists[0].Attributes.Count > 0 &&
                    classItem.AttributeLists[0].Attributes.Any(x =>
                        GetAttributeName(x.Name.ToString(), "FenGenLocalizationReadWriteClass")))
                {
                    IniClass = classItem;
                    break;
                }
            }

            // Make the whole thing fail so I can get a fail message in AngelLoader on build
            if (IniClass == null) throw new ArgumentNullException();

            var iniClassString = IniClass.ToString();
            var classDeclLine = iniClassString.Substring(0, iniClassString.IndexOf('{'));

            var codeBlock =
                code.Substring(0, IniClass.GetLocation().SourceSpan.Start + classDeclLine.Length)
                    .TrimEnd() + "\r\n";

            #endregion

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
                swl(2, AutogeneratedMessage);
                swl(2, "internal static void ReadLocalizationIni(string file)");
                swl(2, "{");
                swl(3, "var lines = File.ReadAllLines(file, Encoding.UTF8);");
                swl(3, "for (int i = 0; i < lines.Length; i++)");
                swl(3, "{");
                swl(4, "var lineT = lines[i].Trim();");
                bool sectElseIf = false;
                foreach (var dict in dictList)
                {
                    swl(4, (sectElseIf ? "else " : "") + "if (lineT == \"[" + dict.Name + "]\")");
                    swl(4, "{");
                    swl(5, "while (i < lines.Length - 1)");
                    swl(5, "{");
                    swl(6, "var lt = lines[i + 1].TrimStart();");

                    bool keysElseIf = false;
                    foreach (var item in dict)
                    {
                        if (item.Key.IsEmpty()) continue;
                        swl(6, (keysElseIf ? "else " : "") + "if (lt.StartsWithFast_NoNullChecks(\"" + item.Key + "=\"))");
                        swl(6, "{");
                        swl(7, langClassName + "." + dict.Name + "." + item.Key +
                               " = lt.Substring(" + (item.Key + "=").Length + ");");
                        swl(6, "}");
                        if (!keysElseIf) keysElseIf = true;
                    }
                    swl(6, "else if (!string.IsNullOrEmpty(lt) && lt[0] == '[' && lt[lt.Length - 1] == ']')");
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
            if (StateVars.WriteTestLangFile) WriteIniFile("", dictList, test: true);
        }

        private static void WriteIniFile(string langIniFile, List<NamedDictionary> dictList, bool test = false)
        {
            if (test) langIniFile = StateVars.TestFile;

            using (var sw = new StreamWriter(langIniFile, append: false, Encoding.UTF8))
            {
                var testPrefix = test ? "█" : "";

                sw.WriteLine("; This is an AngelLoader language file.");
                sw.WriteLine("; This file MUST be saved with UTF8 encoding in order to guarantee correct display of strings.");
                sw.WriteLine();

                for (var i = 0; i < dictList.Count; i++)
                {
                    var dict = dictList[i];

                    sw.WriteLine("[" + dict.Name + "]");
                    foreach (var item in dict)
                    {
                        if (item.IsComment)
                        {
                            if (!item.Value.IsEmpty())
                            {
                                var comments = item.Value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                                foreach (var c in comments) sw.WriteLine("; " + c);
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
}

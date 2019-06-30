using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FenGen
{
    internal static class MainFormBacking
    {
        internal static void Generate(string sourceFile, string destFile)
        {
            var src = File.ReadAllLines(sourceFile).ToList();

            for (int i = 0; i < src.Count; i++)
            {
                if (src[i].Trim().StartsWith("#if") || src[i].Trim() == "#endif")
                {
                    src.RemoveAt(i);
                    i--;
                }
            }

            // This must go here, so it can get at the ifdef-stripped code
            var (ns, classDeclaration, lines) = ReadSource(src);

            for (var i = 0; i < src.Count; i++)
            {
                var sl = src[i];
                if (sl.Trim() != "private void InitializeComponent()") continue;

                for (int j = i; j > 0; j--)
                {
                    if (!src[j].Trim().StartsWith("#region ")) continue;

                    src.Insert(j, "#if DEBUG");
                    i++;
                    int stack = 0;
                    bool stackStarted = false;
                    for (int k = Math.Min(i + 1, src.Count); k < src.Count; k++)
                    {
                        if (src[k].Trim().StartsWith("//")) continue;

                        if (src[k].CountChars('{') > 0) stackStarted = true;
                        stack += src[k].CountChars('{');
                        stack -= src[k].CountChars('}');
                        if (!stackStarted || stack != 0) continue;

                        for (; k < src.Count; k++)
                        {
                            if (src[k].Trim() != "#endregion") continue;
                            if (k == src.Count - 1)
                            {
                                src.Add("#endif");
                            }
                            else
                            {
                                src.Insert(k + 1, "#endif");
                            }
                        }
                        File.WriteAllLines(sourceFile, src, Encoding.UTF8);
                        goto breakout;
                    }

                    break;
                }
            }

            breakout:
            using (var sw = new StreamWriter(destFile, append: false, Encoding.UTF8))
            {
                sw.WriteLine("namespace " + ns + "\r\n{");
                sw.WriteLine("    partial class " + classDeclaration + "\r\n    {");
                sw.WriteLine("        private void InitComponentFast()");

                foreach (var line in lines)
                {
                    if (!line.IsWhiteSpace()) sw.WriteLine(line);
                }

                sw.WriteLine("    }\r\n}");
            }
        }

        private static (string Namespace, string ClassDeclaration, List<string> Lines)
        ReadSource(List<string> sourceLines)
        {
            (string Namespace, string ClassDeclaration, List<string> Lines) ret =
                (null, null, new List<string>());

            var code = string.Join("\r\n", sourceLines.ToArray());
            var tree = CSharpSyntaxTree.ParseText(code);

            foreach (var item in tree.GetCompilationUnitRoot().DescendantNodesAndSelf())
            {
                if (!item.IsKind(SyntaxKind.NamespaceDeclaration)) continue;

                var ns = (NamespaceDeclarationSyntax)item;

                ret.Namespace = ns.Name.ToString();
                break;
            }

            if (ret.Namespace == null) throw new ArgumentNullException();

            ClassDeclarationSyntax MainFormClass = null;
            foreach (var item in tree.GetCompilationUnitRoot().DescendantNodes())
            {
                if (!item.IsKind(SyntaxKind.ClassDeclaration)) continue;

                // There's only one class in the Designer file, so use it
                MainFormClass = (ClassDeclarationSyntax)item;
                break;
            }

            // Make the whole thing fail so I can get a fail message in AngelLoader on build
            if (MainFormClass == null) throw new ArgumentNullException();

            ret.ClassDeclaration = MainFormClass.Identifier.ToString();

            MethodDeclarationSyntax initComponent = null;

            foreach (var item in MainFormClass.DescendantNodes())
            {
                if (item.IsKind(SyntaxKind.MethodDeclaration) &&
                    ((MethodDeclarationSyntax)item).Identifier.ToString() == "InitializeComponent")
                {
                    initComponent = (MethodDeclarationSyntax)item;
                    break;
                }
            }

            if (initComponent == null) throw new ArgumentException();

            var stuff = initComponent.DescendantNodes();

            foreach (var node in stuff)
            {
                if (!node.IsKind(SyntaxKind.Block)) continue;

                var block = (BlockSyntax)node;
                foreach (var line in block.ToFullString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                {
                    if (!Regex.Match(line, @"[^\.]+\.Text\s*=\s*"".*"";$").Success)
                    {
                        ret.Lines.Add(line);
                    }
                }
            }

            return ret;
        }
    }
}

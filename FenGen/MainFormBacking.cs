using System;
using System.Collections.Generic;
using System.IO;
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
            var src = ReadSource(sourceFile);
            using (var sw = new StreamWriter(destFile, append: false))
            {
                sw.WriteLine("namespace " + src.Namespace + "\r\n{");
                sw.WriteLine("    partial class " + src.ClassDeclaration + "\r\n    {");
                sw.WriteLine("        private void InitComponentFast()");

                foreach (var line in src.Lines)
                {
                    if (!line.IsWhiteSpace()) sw.WriteLine(line);
                }

                sw.WriteLine("    }\r\n}");
            }
        }

        private static (string Namespace, string ClassDeclaration, List<string> Lines)
        ReadSource(string sourceFile)
        {
            (string Namespace, string ClassDeclaration, List<string> Lines) ret =
                (null, null, new List<string>());

            var code = File.ReadAllText(sourceFile);
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

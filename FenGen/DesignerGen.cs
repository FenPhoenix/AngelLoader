using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen
{
    internal static class DesignerGen
    {
        internal static void Generate()
        {
            var dfs = Cache.DesignerCSFiles;
            foreach (string df in dfs)
            {
                CreateInitManualFile(df);
                //break;
            }
        }

        private static void CreateInitManualFile(string designerFile)
        {
            var origLines = File.ReadAllLines(designerFile);
            var workingLines = new List<string>();

            workingLines.AddRange(origLines);

            for (int i = 0; i < workingLines.Count; i++)
            {
                string lineT = workingLines[i].Trim();
                if (lineT.EqualsOrStartsWithPlusWhiteSpaceI("#if") ||
                    lineT.EqualsOrStartsWithPlusWhiteSpaceI("#elif") ||
                    lineT.EqualsOrStartsWithPlusWhiteSpaceI("#else") ||
                    lineT.EqualsOrStartsWithPlusWhiteSpaceI("#endif"))
                {
                    workingLines.RemoveAt(i);
                    i--;
                }
            }

            string dfInitManualFile = designerFile.RemoveExtension() + "_InitManual.cs";

            string code = string.Join("\r\n", workingLines);
            var tree = ParseTextFast(code);
            var nodes = tree.GetCompilationUnitRoot().DescendantNodesAndSelf();
            ClassDeclarationSyntax? formClass = null;
            foreach (var node in nodes)
            {
                if (node.IsKind(SyntaxKind.ClassDeclaration))
                {
                    formClass = (ClassDeclarationSyntax)node;
                    break;
                }
            }

            if (formClass == null) return;

            var childNodes = formClass.DescendantTrivia();

            bool mainForm = Path.GetFileName(designerFile).EqualsI("MainForm.Designer.cs");
            /*
            if (mainForm)
            {
                //Debugger.Break();
                Trace.WriteLine("*********** MainForm start");

                var sb = new StringBuilder(code);

                int charsMinus = 0;

                foreach (var node in childNodes)
                {
                    //Trace.WriteLine(node.ToString());

                    if (node.IsKind(SyntaxKind.IfDirectiveTrivia) ||
                        node.IsKind(SyntaxKind.ElifDirectiveTrivia) ||
                        node.IsKind(SyntaxKind.ElseDirectiveTrivia) ||
                        node.IsKind(SyntaxKind.EndIfDirectiveTrivia))
                    {
                        //Trace.WriteLine(code.Substring(node.SpanStart, node.Span.End - node.SpanStart));
                        var len = node.Span.End - node.SpanStart;
                        sb.Remove(node.SpanStart - charsMinus, len);
                        charsMinus += len;
                    }

                    //if (node.IsKind(SyntaxKind.IfDirectiveTrivia))
                    //{
                    //    Trace.WriteLine("if directive");

                    //    //foreach (var dt in node.GetDiagnostics())
                    //    //{
                    //    //    Trace.WriteLine(dt.Descriptor);
                    //    //}
                    //}
                    //else if (node.IsKind(SyntaxKind.EndIfDirectiveTrivia))
                    //{
                    //    Trace.WriteLine("endif directive");
                    //}

                    //if (node.IsKind(SyntaxKind.IfDirectiveTrivia))
                    //{
                    //    Trace.WriteLine("if directive");
                    //}

                    //Trace.WriteLine(node.ToString());
                    //if (node is MethodDeclarationSyntax mds)
                    //{
                    //    Trace.WriteLine(mds.Identifier.Text);
                    //}
                }

                File.WriteAllText(@"C:\designer_gen_test.txt", sb.ToString());

                Trace.WriteLine("*********** MainForm end");
            }
            */
            }
        }
    }

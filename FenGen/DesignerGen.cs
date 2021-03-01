using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using JetBrains.Annotations;
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
            File.Delete(@"C:\formFileName.txt");
            foreach (var designerFile in Cache.DesignerCSFiles)
            {
                GenerateDesignerFile(designerFile);
            }
        }

        private static void GenerateDesignerFile(string designerFile)
        {
            //const string sourceFile = @"C:\Users\Brian\source\repos\AngelLoader\AngelLoader\Forms\CustomControls\SettingsPages\FMDisplayPage.Designer.cs";
            string formFileName = Path.GetFileName(designerFile);
            //formFileName = formFileName.Substring(0, (formFileName.Length - "Designer.cs".Length) - 1) + ".cs";
            //Trace.WriteLine(formFileName);
            //File.WriteAllLines(@"C:\formFileName.txt", new[] { formFileName });
            string destFile = Path.Combine(Path.GetDirectoryName(designerFile)!, formFileName.Substring(0, (formFileName.Length - "Designer.cs".Length) - 1) + "_InitManual.cs");
            //using (var sw = new StreamWriter(@"C:\formFileName.txt", append: true))
            //{
            //    sw.WriteLine(Path.GetFileName(designerFile));
            //    sw.WriteLine(destFile);
            //}
            //return;

            string code = File.ReadAllText(designerFile);

            List<string> sourceLines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

            // Hack: Remove existing ifdefs because otherwise the parser just ignores all code inside them...
            for (int i = 0; i < sourceLines.Count; i++)
            {
                string lineT = sourceLines[i].Trim();
                if (lineT.StartsWith("#if DEBUG") || lineT.StartsWith("#endif"))
                {
                    sourceLines.RemoveAt(i);
                    i--;
                }
            }

            code = string.Join("\r\n", sourceLines);

            SyntaxTree tree = ParseTextFast(code);

            var descendantNodes = tree.GetRoot().DescendantNodes();

            ClassDeclarationSyntax? formClass = null;

            foreach (SyntaxNode node in descendantNodes)
            {
                if (node is ClassDeclarationSyntax cds)
                {
                    formClass = cds;
                    break;
                }
            }

            if (formClass == null) return;

            var formClassLineSpan = formClass.GetLocation().GetLineSpan();
            int formClassStartLine = formClassLineSpan.StartLinePosition.Line;
            int formClassEndLine = formClassLineSpan.EndLinePosition.Line;

            MethodDeclarationSyntax? initializeComponentMethod = null;

            string nameSpace = "";

            foreach (SyntaxNode node in formClass.DescendantNodes())
            {
                if (nameSpace.IsEmpty() && node is NamespaceDeclarationSyntax nds)
                {
                    nameSpace = nds.Name.ToString();
                }
                else if (node is MethodDeclarationSyntax mds &&
                    mds.Identifier.Value?.ToString() == "InitializeComponent")
                {
                    initializeComponentMethod = mds;
                    break;
                }
            }

            if (initializeComponentMethod == null) return;

            var initComponentLineSpan = initializeComponentMethod.GetLocation().GetLineSpan();
            int initComponentStartLine = initComponentLineSpan.StartLinePosition.Line;
            int initComponentEndLine = initComponentLineSpan.EndLinePosition.Line;

            SyntaxNode? block = initializeComponentMethod.Body;

            if (block == null) return;

            var destLines = new List<string>();

            foreach (SyntaxNode node in block.ChildNodes())
            {
                bool nodeExcluded = false;

                if (node is ExpressionStatementSyntax exp)
                {
                    foreach (var cn in exp.DescendantNodes())
                    {
                        if (cn is AssignmentExpressionSyntax { Left: MemberAccessExpressionSyntax mae })
                        {
                            string name = mae.Name.ToString();
                            if (name == "Name" || name == "Text")
                            {
                                nodeExcluded = true;
                                break;
                            }
                        }
                    }
                }

                if (!nodeExcluded)
                {
                    destLines.Add(node.ToFullString().TrimEnd('\r', '\n'));
                }
            }

            // TODO: Auto-gen the ifdeffed method call switch

            var finalDestLines = new List<string>();

            for (int i = 0; i <= formClassStartLine; i++)
            {
                finalDestLines.Add(sourceLines[i]);
            }
            finalDestLines.Add("    {");

            finalDestLines.Add("        private void InitComponentManual()");
            finalDestLines.Add("        {");
            finalDestLines.AddRange(destLines);
            finalDestLines.Add("        }");
            finalDestLines.Add("    }");
            finalDestLines.Add("}");

            File.WriteAllLines(destFile, finalDestLines);

            bool foundIfDEBUG = false;
            bool foundEndIfDEBUG = false;

            for (int i = initComponentStartLine; i > formClassStartLine; i--)
            {
                if (sourceLines[i].Trim() == "#if DEBUG")
                {
                    foundIfDEBUG = true;
                }
            }

            if (foundIfDEBUG)
            {
                for (int i = initComponentEndLine; i < formClassEndLine; i++)
                {
                    if (sourceLines[i].Trim() == "#endif")
                    {
                        foundEndIfDEBUG = true;
                    }
                }
            }

            if (!foundIfDEBUG && !foundEndIfDEBUG)
            {
                sourceLines.Insert(initComponentStartLine, "#if DEBUG");
                sourceLines.Insert(initComponentEndLine + 2, "#endif");
            }

            while (sourceLines[sourceLines.Count - 1].IsWhiteSpace())
            {
                sourceLines.RemoveAt(sourceLines.Count - 1);
            }

            // Designer.cs files want UTF8 with BOM I guess...
            File.WriteAllLines(designerFile, sourceLines, Encoding.UTF8);
        }
    }
}

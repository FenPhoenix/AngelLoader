﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static FenGen.Misc;

namespace FenGen
{
    internal static class DesignerGen
    {
        private enum ControlProperty
        {
            Anchor,
            AutoSize,
            Checked,
            CheckState,
            Dock,
            Location,
            MinimumSize,
            Size
        }

        private sealed class CProp
        {
            internal bool IsFormProperty;
            internal AnchorStyles? AnchorStyles;
            internal bool? AutoSize;
            internal bool? Checked;
            internal CheckState? CheckState;
            internal DockStyle? Dock;
            internal Size? MinimumSize;
            internal Size? Size;
            internal Point? Location;
            internal bool HasName;
            internal bool HasText;
        }

        private static CProp Prop(this Dictionary<string, CProp> dict, string key)
        {
            if (!dict.ContainsKey(key)) dict.Add(key, new CProp());
            return dict[key];
        }

        private sealed class ControlInfo
        {
            internal Dictionary<ControlProperty, object> Properties = new();
        }

        internal static void Generate()
        {
            foreach (var designerFile in Cache.DesignerCSFiles)
            {
                GenerateDesignerFile(designerFile);
            }
        }
        /*
        TODO(FenGen/DesignerGen):
        -If Anchor == Top | Left, remove Anchor.
        -If AutoSize == true OR Dock == Fill, remove Size and Location.
        -If there exists a Checked and a CheckedState whose values match, remove CheckedState.
        -If Dock == Fill, remove Location.
        -If Size is the same as MinimumSize, remove Size.
        -Remove Location = new Point(0, 0).
        -If this.Text exists, don't remove it but instead just say Text = " "; (Settings form perf hack).
        -Somehow deal with icons/images wanting to load from Resources but we want them from Images etc.
        -Have some way to manually specify things, like specify lines not to overwrite because they're manually
         set up.
        */
        private static void GenerateDesignerFile(string designerFile)
        {
            var controlProperties = new Dictionary<string, CProp>();
            var destNodes = new List<(string? PropName, SyntaxNode Node)>();

            string formFileName = Path.GetFileName(designerFile);
            string destFile = Path.Combine(Path.GetDirectoryName(designerFile)!, formFileName.Substring(0, (formFileName.Length - "Designer.cs".Length) - 1) + "_InitManual.cs");

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

            NamespaceDeclarationSyntax? namespaceDeclaration = null;

            foreach (SyntaxNode node in descendantNodes)
            {
                if (node is NamespaceDeclarationSyntax nds)
                {
                    namespaceDeclaration = nds;
                }
                else if (node is ClassDeclarationSyntax cds)
                {
                    formClass = cds;
                    break;
                }
            }

            if (namespaceDeclaration == null) return;

            var namespaceDeclarationLineSpan = namespaceDeclaration.GetLocation().GetLineSpan();
            int namespaceDeclarationStartLine = namespaceDeclarationLineSpan.StartLinePosition.Line;

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

            bool pastConstructorSection = false;
            int pastConstructorStartLine = -1;

            foreach (SyntaxTrivia tn in block.DescendantTrivia())
            {
                if (tn.IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    if (tn.ToString().Trim() == "//")
                    {
                        pastConstructorStartLine = tn.GetLocation().GetLineSpan().StartLinePosition.Line;
                        break;
                    }
                }
            }

            if (pastConstructorStartLine == -1) return;

            foreach (SyntaxNode node in block.ChildNodes())
            {
                (string? PropName, SyntaxNode Node) curNode = new();

                //bool nodeExcluded = false;

                if (!pastConstructorSection && node.GetLocation().GetLineSpan().StartLinePosition.Line >=
                    pastConstructorStartLine)
                {
                    pastConstructorSection = true;
                }

                if (pastConstructorSection && node is ExpressionStatementSyntax exp)
                {
                    foreach (var cn in exp.DescendantNodes())
                    {
                        if (cn is AssignmentExpressionSyntax aes)
                        {
                            bool isFormProperty = false;

                            if (aes.Left is MemberAccessExpressionSyntax left)
                            {
                                string name = left.Name.ToString();
                                if (aes.Right != null)
                                {
                                    switch (name)
                                    {
                                        case "AutoSize":
                                        case "Checked":
                                        {
                                            if (aes.Right is LiteralExpressionSyntax les)
                                            {
                                                string value = les.ToString();

                                                CProp prop = controlProperties.Prop(name);

                                                bool? boolVal = value switch
                                                {
                                                    "true" => true,
                                                    "false" => false,
                                                    _ => null
                                                };

                                                switch (name)
                                                {
                                                    case "AutoSize":
                                                        prop.AutoSize = boolVal;
                                                        break;
                                                    case "Checked":
                                                        prop.Checked = boolVal;
                                                        break;
                                                }
                                            }
                                            break;
                                        }
                                        case "Size":
                                        case "MinimumSize":
                                        {
                                            if (aes.Right is ObjectCreationExpressionSyntax oce &&
                                                oce.Type.ToString() == "System.Drawing.Size" &&
                                                oce.ArgumentList != null &&
                                                oce.ArgumentList.Arguments.Count == 2 &&
                                                int.TryParse(oce.ArgumentList.Arguments[0].ToString(),
                                                    out int width) &&
                                                int.TryParse(oce.ArgumentList.Arguments[1].ToString(),
                                                    out int height))
                                            {
                                                CProp prop = controlProperties.Prop(name);

                                                switch (name)
                                                {
                                                    case "Size":
                                                        prop.Size = new Size(width, height);
                                                        break;
                                                    case "MinimumSize":
                                                        prop.MinimumSize = new Size(width, height);
                                                        break;
                                                }
                                            }
                                            break;
                                        }
                                        case "Location":
                                        {
                                            if (aes.Right is ObjectCreationExpressionSyntax oce &&
                                                oce.Type.ToString() == "System.Drawing.Point" &&
                                                oce.ArgumentList != null && oce.ArgumentList.Arguments.Count == 2 &&
                                                int.TryParse(oce.ArgumentList.Arguments[0].ToString(), out int x) &&
                                                int.TryParse(oce.ArgumentList.Arguments[1].ToString(), out int y))
                                            {
                                                CProp prop = controlProperties.Prop(name);
                                                prop.Location = new Point(x, y);
                                            }
                                            break;
                                        }
                                        case "Name":
                                        case "Text":
                                        {
                                            //Trace.WriteLine(aes.Right.Kind());
                                            if (aes.Right is LiteralExpressionSyntax)
                                            {
                                                CProp prop = controlProperties.Prop(name);
                                                switch (name)
                                                {
                                                    case "Name":
                                                        prop.HasName = true;
                                                        break;
                                                    case "Text":
                                                        prop.HasText = true;
                                                        break;
                                                }
                                            }
                                            break;
                                        }
                                        case "AnchorStyles":
                                        case "CheckState":
                                        case "DockStyle":
                                        {
                                            // TODO: Implement
                                            break;
                                        }
                                    }
                                }

                                foreach (var n in left.DescendantNodes())
                                {
                                    if ((n is ThisExpressionSyntax && left.DescendantNodes().Count() == 2) ||
                                        (n is not ThisExpressionSyntax && left.DescendantNodes().Count() == 1))
                                    {
                                        isFormProperty = true;
                                        CProp prop = controlProperties.Prop(name);
                                        prop.IsFormProperty = true;
                                    }

                                    if (n is IdentifierNameSyntax ins)
                                    {
                                        //Trace.WriteLine(cn.ToString());
                                        //Trace.WriteLine(left.ToString());
                                        //Trace.WriteLine(ins.ToString());
                                        //Trace.WriteLine(left.Name.ToString());
                                        //Trace.WriteLine(left.DescendantNodes().Count());
                                        //Trace.WriteLine("");
                                        break;
                                    }
                                }

                                if (name == "Name" || name == "Text")
                                {
                                    if (isFormProperty && name == "Text")
                                    {
                                        // TODO: Implement setting the value to " "
                                        curNode.Node = node;
                                    }
                                    else
                                    {
                                        //nodeExcluded = true;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

                //if (!nodeExcluded)
                //{
                //    destLines.Add(node.ToFullString().TrimEnd('\r', '\n'));
                //}

                destNodes.Add(curNode);
            }

            // TODO: Auto-gen the ifdeffed method call switch

            var finalDestLines = new List<string>();

            // By starting at the namespace declaration, we skip copying the #define FenGen_* thing which would
            // throw us an exception for not being in a .Designer.cs file
            for (int i = namespaceDeclarationStartLine; i <= formClassStartLine; i++)
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

            // UTF8 with BOM or else Visual Studio complains about "different" encoding
            File.WriteAllLines(destFile, finalDestLines, Encoding.UTF8);

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

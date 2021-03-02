using System;
using System.Collections.Generic;
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
        private sealed class CProps
        {
            internal bool IsFormProperty;
            internal bool HasDefaultAnchor;
            internal bool? AutoSize;
            internal bool? Checked;
            internal CheckState? CheckState;
            internal bool DockIsFill;
            internal Size? MinimumSize;
            internal Size? Size;
            internal Point? Location;
            internal bool HasName;
            internal bool HasText;
        }

        private sealed class NodeCustom
        {
            internal string ControlName = "";
            internal string PropName = "";
            internal SyntaxNode? Node;
            internal string OverrideLine = "";
        }

        private static CProps GetOrAddProps(this Dictionary<string, CProps> dict, string key)
        {
            if (!dict.ContainsKey(key)) dict.Add(key, new CProps());
            return dict[key];
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
        -Somehow deal with icons/images wanting to load from Resources but we want them from Images etc.
        -Have some way to manually specify things, like specify lines not to overwrite because they're manually
         set up.
        */
        private static void GenerateDesignerFile(string designerFile)
        {
            var controlProperties = new Dictionary<string, CProps>();
            var destNodes = new List<NodeCustom>();

            string formFileName = Path.GetFileName(designerFile);
            string destFile = Path.Combine(Path.GetDirectoryName(designerFile)!, formFileName.Substring(0, (formFileName.Length - ".Designer.cs".Length)) + "_InitManual.cs");

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

            bool pastConstructorSection = false;
            int pastConstructorStartLine = -1;

            // Cheap way to know we're past the construction portion - we get the first header comment:
            // 
            // ControlName
            // 
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

            var array = block.ChildNodes().ToArray();
            foreach (SyntaxNode node in array)
            {
                if (!pastConstructorSection && node.GetLocation().GetLineSpan().StartLinePosition.Line >=
                    pastConstructorStartLine)
                {
                    pastConstructorSection = true;
                }

                var curNode = new NodeCustom();

                if (pastConstructorSection && node is ExpressionStatementSyntax exp)
                {
                    foreach (var cn in exp.DescendantNodes())
                    {
                        if (cn is not AssignmentExpressionSyntax aes) continue;

                        if (aes.Left is not MemberAccessExpressionSyntax left) continue;
                        string controlName = left.DescendantNodes().First(x => x is IdentifierNameSyntax).ToString();
                        string propName = left.Name.ToString();

                        if (aes.Right != null)
                        {
                            switch (propName)
                            {
                                case "AutoSize":
                                case "Checked":
                                {
                                    if (aes.Right is LiteralExpressionSyntax les)
                                    {
                                        string value = les.ToString();

                                        CProps props = controlProperties.GetOrAddProps(controlName);

                                        bool? boolVal = value switch
                                        {
                                            "true" => true,
                                            "false" => false,
                                            _ => null
                                        };

                                        switch (propName)
                                        {
                                            case "AutoSize":
                                                props.AutoSize = boolVal;
                                                break;
                                            case "Checked":
                                                props.Checked = boolVal;
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
                                        CProps props = controlProperties.GetOrAddProps(controlName);

                                        switch (propName)
                                        {
                                            case "Size":
                                                props.Size = new Size(width, height);
                                                break;
                                            case "MinimumSize":
                                                props.MinimumSize = new Size(width, height);
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
                                        CProps props = controlProperties.GetOrAddProps(controlName);
                                        props.Location = new Point(x, y);
                                    }
                                    break;
                                }
                                case "Name":
                                case "Text":
                                {
                                    if (aes.Right is LiteralExpressionSyntax)
                                    {
                                        CProps props = controlProperties.GetOrAddProps(controlName);
                                        switch (propName)
                                        {
                                            case "Name":
                                                props.HasName = true;
                                                break;
                                            case "Text":
                                                props.HasText = true;
                                                break;
                                        }
                                    }
                                    break;
                                }
                                case "Anchor":
                                {
                                    var ors = aes.Right.DescendantNodesAndSelf()
                                        .Where(x =>
                                            x is MemberAccessExpressionSyntax &&
                                            x.ToString().StartsWith("System.Windows.Forms.AnchorStyles."))
                                        .ToArray();

                                    if (ors.Length == 2
                                        && ors.FirstOrDefault(
                                            x => x.ToString() ==
                                                 "System.Windows.Forms.AnchorStyles.Top") != null
                                        && ors.FirstOrDefault(
                                            x => x.ToString() ==
                                                 "System.Windows.Forms.AnchorStyles.Left") != null)
                                    {
                                        CProps props = controlProperties.GetOrAddProps(controlName);
                                        props.HasDefaultAnchor = true;
                                    }
                                    break;
                                }
                                case "Dock":
                                {
                                    if (aes.Right is MemberAccessExpressionSyntax mae)
                                    {
                                        CProps props = controlProperties.GetOrAddProps(controlName);
                                        props.DockIsFill = mae.ToString() == "System.Windows.Forms.DockStyle.Fill";
                                    }
                                    break;
                                }
                                case "CheckState":
                                {
                                    if (aes.Right is MemberAccessExpressionSyntax les)
                                    {
                                        CProps props = controlProperties.GetOrAddProps(controlName);

                                        props.CheckState = les.ToString() switch
                                        {
                                            "System.Windows.Forms.CheckState.Checked" => CheckState.Checked,
                                            "System.Windows.Forms.CheckState.Unchecked" => CheckState.Unchecked,
                                            "System.Windows.Forms.CheckState.Indeterminate" => CheckState.Indeterminate,
                                            _ => null
                                        };
                                    }
                                    break;
                                }
                            }
                        }
                        foreach (var n in left.DescendantNodes())
                        {
                            if ((n is ThisExpressionSyntax && left.DescendantNodes().Count() == 2) ||
                                (n is not ThisExpressionSyntax && left.DescendantNodes().Count() == 1))
                            {
                                CProps props = controlProperties.GetOrAddProps(controlName);
                                props.IsFormProperty = true;
                            }
                        }
                        curNode.ControlName = controlName;
                        curNode.PropName = propName;
                    }
                }
                curNode.Node = node;

                destNodes.Add(curNode);
            }

            for (int i = 0; i < destNodes.Count; i++)
            {
                var destNode = destNodes[i];

                if (destNode.ControlName.IsEmpty() || destNode.Node == null) continue;
                if (!controlProperties.TryGetValue(destNode.ControlName, out CProps props)) continue;

                if (destNode.PropName == "Name" && props.HasName)
                {
                    destNodes.RemoveAt(i);
                    i--;
                }
                else if (destNode.PropName == "Text" && props.HasText)
                {
                    if (props.IsFormProperty)
                    {
                        // Hack to avoid knowing the black art of how to construct code with Roslyn...
                        destNode.OverrideLine = "            this.Text = \" \"";
                    }
                    else
                    {
                        destNodes.RemoveAt(i);
                        i--;
                    }
                }
                else if (destNode.PropName == "Anchor" && props.HasDefaultAnchor)
                {
                    destNodes.RemoveAt(i);
                    i--;
                }
                else if (destNode.PropName == "Location" &&
                         ((props.Location?.X == 0 && props.Location?.Y == 0) ||
                          props.DockIsFill))
                {
                    destNodes.RemoveAt(i);
                    i--;
                }
                else if (destNode.PropName == "Size" &&
                         ((props.Size != null && props.MinimumSize != null && props.Size == props.MinimumSize) ||
                         (props.Size != null && props.AutoSize == true) ||
                         props.AutoSize == true ||
                         props.DockIsFill))
                {
                    destNodes.RemoveAt(i);
                    i--;
                }
                else if (destNode.PropName == "CheckState" &&
                         ((props.Checked == true && props.CheckState == CheckState.Checked) ||
                          (props.Checked == false && props.CheckState == CheckState.Unchecked)))
                {
                    destNodes.RemoveAt(i);
                    i--;
                }
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
            foreach (NodeCustom node in destNodes)
            {
                if (!node.OverrideLine.IsEmpty())
                {
                    finalDestLines.Add(node.OverrideLine);
                    continue;
                }

                if (node.Node == null) continue;
                finalDestLines.Add(node.Node.ToFullString().TrimEnd('\r', '\n'));
            }
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

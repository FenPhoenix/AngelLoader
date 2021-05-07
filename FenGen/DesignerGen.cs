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
            internal bool? HasDefaultAnchor;
            internal bool? AutoSize;
            internal bool? Checked;
            internal CheckState? CheckState;
            internal bool DockIsFill;
            internal Size? MinimumSize;
            internal Size? Size;
            internal Point? Location;
            internal bool HasName;
            internal bool HasText;
            internal bool ExplicitAppIcon;
        }

        private sealed class NodeCustom
        {
            internal bool IgnoreExceptForComments;
            internal string ControlName = "";
            internal string PropName = "";
            internal string OverrideLine = "";
            internal readonly SyntaxNode Node;
            internal readonly List<string> Comments = new List<string>();

            internal NodeCustom(SyntaxNode node) => Node = node;
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
        -If a control is being added to a FlowLayoutPanel, remove Location.
        */
        private static void GenerateDesignerFile(string designerFile)
        {
            var controlTypes = new Dictionary<string, string>();
            var controlAttributes = new Dictionary<string, string>();
            var controlsInFlowLayoutPanels = new HashSet<string>();
            var controlProperties = new Dictionary<string, CProps>();
            var destNodes = new List<NodeCustom>();

            string formFileName = Path.GetFileName(designerFile);
            string destFile = Path.Combine(Path.GetDirectoryName(designerFile)!, formFileName.Substring(0, formFileName.Length - ".Designer.cs".Length) + "_InitSlim.Generated.cs");

            string code = File.ReadAllText(designerFile);

            List<string> sourceLines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

            #region Remove existing ifdefs

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

            #endregion

            SyntaxTree tree = ParseTextFast(code);

            NamespaceDeclarationSyntax? namespaceDeclaration = null;
            ClassDeclarationSyntax? formClass = null;

            #region Find namespace and form class declarations

            foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
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

            if (namespaceDeclaration == null)
            {
                ThrowErrorAndTerminate("Namespace declaration not found in:\r\n" + designerFile);
                return;
            }

            var namespaceDeclarationLineSpan = namespaceDeclaration.GetLocation().GetLineSpan();
            int namespaceDeclarationStartLine = namespaceDeclarationLineSpan.StartLinePosition.Line;

            if (formClass == null)
            {
                ThrowErrorAndTerminate("Form class declaration not found in:\r\n" + designerFile);
                return;
            }

            #endregion

            #region Find start/end line indexes of form class and InitializeComponent() method

            var formClassLineSpan = formClass.GetLocation().GetLineSpan();
            int formClassStartLine = formClassLineSpan.StartLinePosition.Line;
            int formClassEndLine = formClassLineSpan.EndLinePosition.Line;

            var initializeComponentMethod = (MethodDeclarationSyntax?)formClass.DescendantNodes()
                .FirstOrDefault(
                    x => x is MethodDeclarationSyntax mds &&
                         mds.Identifier.Value?.ToString() == "InitializeComponent");

            if (initializeComponentMethod == null)
            {
                ThrowErrorAndTerminate("InitializeComponent() method not found in:\r\n" + designerFile);
                return;
            }

            var initComponentLineSpan = initializeComponentMethod.GetLocation().GetLineSpan();
            int initComponentStartLine = initComponentLineSpan.StartLinePosition.Line;
            int initComponentEndLine = initComponentLineSpan.EndLinePosition.Line;

            #endregion

            SyntaxNode? block = initializeComponentMethod.Body;

            if (block == null)
            {
                ThrowErrorAndTerminate("Body of InitializeComponent() method not found found in:\r\n" + designerFile);
                return;
            }

            foreach (SyntaxNode node in formClass.ChildNodes())
            {
                if (node is FieldDeclarationSyntax fds)
                {
                    string name = fds.Declaration.Variables.First().Identifier.ToString();
                    string type = fds.Declaration.Type.ToString();
                    int lastIndexOfDot = type.LastIndexOf('.');
                    string typeShort = lastIndexOfDot > -1 ? type.Substring(lastIndexOfDot + 1) : type;

                    if (typeShort != "IContainer") controlTypes[name] = typeShort;

                    if (HasAttribute(fds, GenAttributes.FenGenDoNotRemoveTextAttribute))
                    {
                        controlAttributes[name] = GenAttributes.FenGenDoNotRemoveTextAttribute;
                    }
                }
            }

            #region Find start line index of property-set section

            bool pastConstructorSection = false;
            int pastConstructorStartLine = -1;

            // Cheap way to know we're past the construction portion - we get the first header comment:
            // 
            // ControlName
            // 
            foreach (SyntaxTrivia tn in block.DescendantTrivia())
            {
                if (tn.IsKind(SyntaxKind.SingleLineCommentTrivia) && tn.ToString().Trim() == "//")
                {
                    pastConstructorStartLine = tn.GetLocation().GetLineSpan().StartLinePosition.Line;
                    break;
                }
            }

            if (pastConstructorStartLine == -1)
            {
                ThrowErrorAndTerminate("Post-construction-section control comment header not found in:\r\n" + designerFile);
                return;
            }

            #endregion

            #region Process nodes

            foreach (SyntaxNode node in block.ChildNodes())
            {
                if (!pastConstructorSection && node.GetLocation().GetLineSpan().StartLinePosition.Line >=
                    pastConstructorStartLine)
                {
                    pastConstructorSection = true;
                }

                var curNode = new NodeCustom(node);
                destNodes.Add(curNode);

                if (!pastConstructorSection || node is not ExpressionStatementSyntax exp)
                {
                    continue;
                }

                var aes = (AssignmentExpressionSyntax?)exp.DescendantNodes().FirstOrDefault(x => x is AssignmentExpressionSyntax);

                var ies = (InvocationExpressionSyntax?)exp.DescendantNodes().FirstOrDefault(x => x is InvocationExpressionSyntax);

                if (ies != null)
                {
                    foreach (SyntaxNode mesN in ies.DescendantNodes())
                    {
                        if (mesN is MemberAccessExpressionSyntax mes && mes.Name.ToString() == "Add")
                        {
                            string nodeStr = mesN.ToString().Trim();
                            if (nodeStr.StartsWith("this.")) nodeStr = nodeStr.Substring(5);

                            string nodeControlName = nodeStr.Substring(0, nodeStr.IndexOf('.'));

                            if (nodeStr == nodeControlName + ".Controls.Add" &&
                                controlTypes.TryGetValue(nodeControlName, out string nodeType) &&
                                nodeType == "FlowLayoutPanel" &&
                                ies.ArgumentList.Arguments.Count == 1)
                            {
                                var arg = ies.ArgumentList.Arguments[0];
                                string argStr = arg.ToString().Trim();
                                if (argStr.StartsWith("this.")) argStr = argStr.Substring(5);
                                controlsInFlowLayoutPanels.Add(argStr);
                                break;
                            }
                        }
                    }
                }

                if (aes?.Left is not MemberAccessExpressionSyntax left) continue;

                curNode.ControlName = left.DescendantNodes().First(x => x is IdentifierNameSyntax).ToString();
                curNode.PropName = left.Name.ToString();

                if (left.DescendantNodes().Any(
                    n => (n is ThisExpressionSyntax && left.DescendantNodes().Count() == 2) ||
                         (n is not ThisExpressionSyntax && left.DescendantNodes().Count() == 1)))
                {
                    CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                    props.IsFormProperty = true;
                }

                #region Set control property

                switch (curNode.PropName)
                {
                    case "AutoSize":
                    case "Checked":
                    {
                        if (aes.Right is LiteralExpressionSyntax les)
                        {
                            string value = les.ToString();

                            CProps props = controlProperties.GetOrAddProps(curNode.ControlName);

                            bool? boolVal = value switch
                            {
                                "true" => true,
                                "false" => false,
                                _ => null
                            };

                            switch (curNode.PropName)
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
                            oce.ArgumentList?.Arguments.Count == 2 &&
                            int.TryParse(oce.ArgumentList.Arguments[0].ToString(), out int width) &&
                            int.TryParse(oce.ArgumentList.Arguments[1].ToString(), out int height))
                        {
                            CProps props = controlProperties.GetOrAddProps(curNode.ControlName);

                            switch (curNode.PropName)
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
                    case "Icon":
                    {
                        if (aes.Right is MemberAccessExpressionSyntax maes)
                        {
                            string val = maes.ToString();
                            if (val.TrimEnd(';').EndsWith(".Resources.AngelLoader"))
                            {
                                CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                                props.ExplicitAppIcon = true;
                            }
                        }
                        break;
                    }
                    case "Location":
                    {
                        if (aes.Right is ObjectCreationExpressionSyntax oce &&
                            oce.Type.ToString() == "System.Drawing.Point" &&
                            oce.ArgumentList?.Arguments.Count == 2 &&
                            int.TryParse(oce.ArgumentList.Arguments[0].ToString(), out int x) &&
                            int.TryParse(oce.ArgumentList.Arguments[1].ToString(), out int y))
                        {
                            CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                            props.Location = new Point(x, y);
                        }
                        break;
                    }
                    case "Name":
                    case "Text":
                    {
                        if (aes.Right is LiteralExpressionSyntax)
                        {
                            CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                            switch (curNode.PropName)
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

                        CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                        if (ors.Length == 2
                            && Array.Find(ors, x => x.ToString() == "System.Windows.Forms.AnchorStyles.Top") != null
                            && Array.Find(ors, x => x.ToString() == "System.Windows.Forms.AnchorStyles.Left") != null)
                        {
                            props.HasDefaultAnchor = true;
                        }
                        else
                        {
                            props.HasDefaultAnchor = false;
                        }
                        break;
                    }
                    case "Dock":
                    {
                        if (aes.Right is MemberAccessExpressionSyntax mae)
                        {
                            CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                            props.DockIsFill = mae.ToString() == "System.Windows.Forms.DockStyle.Fill";
                        }
                        break;
                    }
                    case "CheckState":
                    {
                        if (aes.Right is MemberAccessExpressionSyntax les)
                        {
                            CProps props = controlProperties.GetOrAddProps(curNode.ControlName);

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

                #endregion
            }

            #endregion

            #region Edit or elide property sets based on control's other properties

            for (int i = 0; i < destNodes.Count; i++)
            {
                var destNode = destNodes[i];

                if (destNode.ControlName.IsEmpty() ||
                    !controlProperties.TryGetValue(destNode.ControlName, out CProps props))
                {
                    continue;
                }

                foreach (SyntaxTrivia t in destNode.Node.DescendantTrivia())
                {
                    if (t.Kind() == SyntaxKind.SingleLineCommentTrivia)
                    {
                        destNode.Comments.Add(t.ToString());
                    }
                }

                if (destNode.PropName == "Name" && props.HasName)
                {
                    destNode.IgnoreExceptForComments = true;
                }
                else if (destNode.PropName == "Text" && props.HasText &&
                         (!controlAttributes.TryGetValue(destNode.ControlName, out string attr) ||
                         attr != GenAttributes.FenGenDoNotRemoveTextAttribute))
                {
                    if (props.IsFormProperty)
                    {
                        // Hack to avoid knowing the black art of how to construct code with Roslyn...
                        destNode.OverrideLine = "            this.Text = \" \";";
                    }
                    else
                    {
                        destNode.IgnoreExceptForComments = true;
                    }
                }
                else if (destNode.PropName == "Anchor" &&
                         (
                             props.HasDefaultAnchor == true ||
                             controlsInFlowLayoutPanels.Contains(destNode.ControlName)
                         )
                )
                {
                    destNode.IgnoreExceptForComments = true;
                }
                else if (destNode.PropName == "Icon" && props.ExplicitAppIcon && props.IsFormProperty)
                {
                    destNode.OverrideLine = "            this.Icon = AngelLoader.Forms.AL_Icon.AngelLoader;";
                }
                else if (destNode.PropName == "Location" &&
                         (
                             (props.Location?.X == 0 && props.Location?.Y == 0) ||
                             props.DockIsFill ||
                             controlsInFlowLayoutPanels.Contains(destNode.ControlName)
                         )
                )
                {
                    destNode.IgnoreExceptForComments = true;
                }
                else if (destNode.PropName == "Size" &&
                         // If anchor is anything other than top-left, we need to keep the size, for reasons I'm
                         // unable to think how to explain well at the moment but you can figure it out, it's like
                         // when we go to position it, it needs to know its size so it can properly position itself
                         // relative to its anchor... you know.
                         (props.HasDefaultAnchor != false) &&
                         (
                             (props.Size != null && props.MinimumSize != null && props.Size == props.MinimumSize) ||
                             (props.Size != null && props.AutoSize == true) ||
                             props.AutoSize == true
                         )
                )
                {
                    destNode.IgnoreExceptForComments = true;
                }
                else if (destNode.PropName == "CheckState" &&
                         ((props.Checked == true && props.CheckState == CheckState.Checked) ||
                          (props.Checked == false && props.CheckState == CheckState.Unchecked)))
                {
                    destNode.IgnoreExceptForComments = true;
                }
            }

            #endregion

            // TODO: Auto-gen the ifdeffed method call switch

            var finalDestLines = new List<string>();

            #region Create final destination file lines

            // By starting at the namespace declaration, we skip copying the #define FenGen_* thing which would
            // throw us an exception for not being in a .Designer.cs file
            for (int i = namespaceDeclarationStartLine; i <= formClassStartLine; i++)
            {
                finalDestLines.Add(sourceLines[i]);
            }
            finalDestLines.Add("    {");
            finalDestLines.Add("        /// <summary>");
            finalDestLines.Add("        /// Custom generated component initializer with cruft removed.");
            finalDestLines.Add("        /// </summary>");
            finalDestLines.Add("        private void InitializeComponentSlim()");
            finalDestLines.Add("        {");
            foreach (NodeCustom node in destNodes)
            {
                if (node.IgnoreExceptForComments)
                {
                    foreach (string line in node.Comments)
                    {
                        finalDestLines.Add("            " + line);
                    }
                }
                else
                {
                    finalDestLines.Add(!node.OverrideLine.IsEmpty()
                        ? node.OverrideLine
                        : node.Node.ToFullString().TrimEnd('\r', '\n'));
                }
            }
            finalDestLines.Add("        }");
            finalDestLines.Add("    }");
            finalDestLines.Add("}");

            #endregion

            // UTF8 with BOM or else Visual Studio complains about "different" encoding
            File.WriteAllLines(destFile, finalDestLines, Encoding.UTF8);

            #region Add ifdefs around original InitializeComponent() method

            bool foundIfDEBUG = false;
            bool foundEndIfDEBUG = false;

            for (int i = initComponentStartLine; i > formClassStartLine; i--)
            {
                if (sourceLines[i].Trim() == "#if DEBUG")
                {
                    foundIfDEBUG = true;
                    break;
                }
            }

            if (foundIfDEBUG)
            {
                for (int i = initComponentEndLine; i < formClassEndLine; i++)
                {
                    if (sourceLines[i].Trim() == "#endif")
                    {
                        foundEndIfDEBUG = true;
                        break;
                    }
                }
            }

            if (!foundIfDEBUG && !foundEndIfDEBUG)
            {
                sourceLines.Insert(initComponentStartLine - 4, "#if DEBUG");
                sourceLines.Insert(initComponentEndLine + 2, "#endif");
            }

            #endregion

            while (sourceLines[sourceLines.Count - 1].IsWhiteSpace())
            {
                sourceLines.RemoveAt(sourceLines.Count - 1);
            }

            // Designer.cs files want UTF8 with BOM I guess...
            File.WriteAllLines(designerFile, sourceLines, Encoding.UTF8);
        }
    }
}

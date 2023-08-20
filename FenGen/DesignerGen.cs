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

/*
@WPF(FenGen/DesignerGen): This whole thing is WinForms-specific, but:
It only generates into *.Designer.cs files marked with attributes, so it won't mess with WPF and if we removed
WinForms it would just be a no-op, so let's just leave it as is.

MainForm gen notes:
[ ] Remove tab control images

[ ] Handle Image properties (most if not all of these are set in code due to theming or vector drawing)

[ ] Properly ignore ifdef blocks outside the one around InitializeComponent() (we have some in MainForm.Designer.cs)

[ ] Decide how to handle the mess of DataGridViewCellStyle objects in the Designer file.
    In the manual version we just set attributes on the attached ones directly, rather than making whole other objects
    and then just assigning them to the attached ones which is bloated and pointless. But generating the removal of
    these might be tricky(?). See if we can though!

[ ] Filter bar scroll buttons have locations, but we place them manually in code, so make a way to tell it to remove
    location

[ ] ToolStrip AutoSize is true by default (thus not explicitly set if true), so we miss removing the Size property

[ ] BackColor = SystemColors.Control necessary always? (eg. Filter buttons - this prop is not set in InitComponentManual())

[ ] Game tab pages are "fake" tab pages (only the tab part is visible and matters) so we should remove everything
    except ImageIndex and TabIndex on these

[ ] Tab pages dock in their tab control, so "Location" for them is meaningless. We should remove this.
    But Size is needed for internal layout, so keep that.

[ ] Buttons in FlowLayoutPanels that are AutoSize=true and AutoSizeMode=GrowAndShrink - I think we can remove Size here?
    (Manual version does so)

[x] Panels: Size is removed when AutoSize=true, but should be kept for internal layout!

Things are getting combinatorial-explosive in here already, with all these different cases... It's not really
future-proof (will likely break badly if normal but unaccounted-for UI things are done).
We could just switch to a simple Text-and-Name remove, that'll cut down our bloat a lot and maybe we can just
settle for that. But we also want to handle Image properties there, but that's probably all we need...
*/

namespace FenGen;

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
        internal bool HasHeaderText;
        internal bool HasToolTipText;
        internal bool ExplicitAppIcon;
        internal bool HasSetToolTip;
        internal bool HasChildren;
    }

    private sealed class NodeCustom
    {
        internal bool IgnoreExceptForComments;
        internal string ControlName = "";
        internal string PropName = "";
        internal string OverrideLine = "";
        internal readonly SyntaxNode Node;
        internal readonly List<string> Comments = new();

        internal NodeCustom(SyntaxNode node) => Node = node;
    }

    private static CProps GetOrAddProps(this Dictionary<string, CProps> dict, string key)
    {
        if (!dict.ContainsKey(key)) dict.Add(key, new CProps());
        return dict[key];
    }

    internal static void Generate()
    {
        foreach (DesignerCSFile designerFile in Cache.DesignerCSFiles)
        {
            GenerateDesignerFile(designerFile);
        }
    }
    /*
    TODO(FenGen/DesignerGen):
    -Somehow deal with icons/images wanting to load from Resources but we want them from Images etc.
     (we now do this for AngelLoader app icon only as a hack, but we should generalize it)
    -Have some way to manually specify things, like specify lines not to overwrite because they're manually
     set up.
    */
    private static void GenerateDesignerFile(DesignerCSFile designerFile)
    {
        var controlTypes = new Dictionary<string, string>();
        var controlAttributes = new Dictionary<string, string>();
        var controlsInFlowLayoutPanels = new HashSet<string>();
        var controlProperties = new Dictionary<string, CProps>();
        var destNodes = new List<NodeCustom>();

        string formFileName = Path.GetFileName(designerFile.FileName);
        string destFile = Path.Combine(Path.GetDirectoryName(designerFile.FileName)!, formFileName.Substring(0, formFileName.Length - ".Designer.cs".Length) + "_InitSlim.Generated.cs");

        string code = File.ReadAllText(designerFile.FileName);

        List<string> sourceLines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();

        #region Remove existing ifdefs

        // Hack: Remove existing ifdefs because otherwise the parser just ignores all code inside them...
        for (int i = 0; i < sourceLines.Count; i++)
        {
            string lineT = sourceLines[i].Trim();
            if (lineT.StartsWithO("#if DEBUG") || lineT.StartsWithO("#endif"))
            {
                sourceLines.RemoveAt(i);
                i--;
            }
        }

        code = string.Join("\r\n", sourceLines);

        #endregion

        SyntaxTree tree = ParseTextFast(code);

        BaseNamespaceDeclarationSyntax? namespaceDeclaration = null;
        ClassDeclarationSyntax? formClass = null;

        bool fileScopedNamespace = false;

        #region Find namespace and form class declarations

        foreach (SyntaxNode node in tree.GetRoot().DescendantNodes())
        {
            if (node is BaseNamespaceDeclarationSyntax nds)
            {
                namespaceDeclaration = nds;
                fileScopedNamespace = node is FileScopedNamespaceDeclarationSyntax;
            }
            else if (node is ClassDeclarationSyntax cds)
            {
                formClass = cds;
                break;
            }
        }

        if (namespaceDeclaration == null)
        {
            ThrowErrorAndTerminate("Namespace declaration (either normal or file-scoped) not found in:\r\n" + designerFile);
        }

        var namespaceDeclarationLineSpan = namespaceDeclaration.GetLocation().GetLineSpan();
        int namespaceDeclarationStartLine = namespaceDeclarationLineSpan.StartLinePosition.Line;

        if (formClass == null)
        {
            ThrowErrorAndTerminate("Form class declaration not found in:\r\n" + designerFile);
        }

        #endregion

        #region Find start/end line indexes of form class and InitializeComponent() method

        var formClassLineSpan = formClass.GetLocation().GetLineSpan();
        int formClassStartLine = formClassLineSpan.StartLinePosition.Line;
        int formClassEndLine = formClassLineSpan.EndLinePosition.Line;

        var initializeComponentMethod = (MethodDeclarationSyntax?)formClass.DescendantNodes()
            .FirstOrDefault(
                static x => x is MethodDeclarationSyntax mds &&
                            mds.Identifier.Value?.ToString() == "InitializeComponent");

        if (initializeComponentMethod == null)
        {
            ThrowErrorAndTerminate("InitializeComponent() method not found in:\r\n" + designerFile);
        }

        var initComponentLineSpan = initializeComponentMethod.GetLocation().GetLineSpan();
        int initComponentStartLine = initComponentLineSpan.StartLinePosition.Line;
        int initComponentEndLine = initComponentLineSpan.EndLinePosition.Line;

        #endregion

        SyntaxNode? block = initializeComponentMethod.Body;

        if (block == null)
        {
            ThrowErrorAndTerminate("Body of InitializeComponent() method not found found in:\r\n" + designerFile);
        }

        #region Store control names, types, and attributes from field declarations

        foreach (SyntaxNode node in formClass.ChildNodes())
        {
            if (node is FieldDeclarationSyntax fds)
            {
                string name = fds.Declaration.Variables.First().Identifier.ToString();
                string type = fds.Declaration.Type.ToString();
                int lastIndexOfDot = type.LastIndexOf('.');
                string typeShort = lastIndexOfDot > -1 ? type.Substring(lastIndexOfDot + 1) : type;

                if (typeShort != "IContainer") controlTypes[name] = typeShort;

                // We should allow multiple attributes...
                if (HasAttribute(fds, GenAttributes.FenGenDoNotRemoveTextAttribute))
                {
                    controlAttributes[name] = GenAttributes.FenGenDoNotRemoveTextAttribute;
                }
                else if (HasAttribute(fds, GenAttributes.FenGenForceRemoveSizeAttribute))
                {
                    controlAttributes[name] = GenAttributes.FenGenForceRemoveSizeAttribute;
                }
            }
        }

        #endregion

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

            #region Determine if a control is being added to a FlowLayoutPanel

            // If it is, we can remove an additional couple of layout properties (because it will be auto-
            // laid-out inside the FlowLayoutPanel).

            var ies = (InvocationExpressionSyntax?)exp.DescendantNodes().FirstOrDefault(static x => x is InvocationExpressionSyntax);
            if (ies != null)
            {
                foreach (SyntaxNode mesN in ies.DescendantNodes())
                {
                    if (mesN is MemberAccessExpressionSyntax mes)
                    {
                        if (mes.Name.ToString() == "Add")
                        {
                            string nodeStr = mesN.ToString().Trim();
                            if (nodeStr.StartsWithO("this.")) nodeStr = nodeStr.Substring(5);

                            string nodeControlName = nodeStr.Substring(0, nodeStr.IndexOf('.'));

                            if (nodeStr == nodeControlName + ".Controls.Add")
                            {
                                if (controlTypes.TryGetValue(nodeControlName, out string nodeType) &&
                                    (nodeType is "FlowLayoutPanel" or "DarkFlowLayoutPanel") &&
                                    ies.ArgumentList.Arguments.Count == 1)
                                {
                                    var arg = ies.ArgumentList.Arguments[0];
                                    string argStr = arg.ToString().Trim();
                                    if (argStr.StartsWithO("this.")) argStr = argStr.Substring(5);
                                    controlsInFlowLayoutPanels.Add(argStr);
                                }

                                curNode.ControlName = ies.DescendantNodes().First(static x => x is IdentifierNameSyntax).ToString();
                                CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                                props.HasChildren = true;

                                break;
                            }
                        }
                        else if (mes.Name.ToString() == "SetToolTip")
                        {
                            string nodeStr = mesN.ToString().Trim();
                            if (nodeStr.StartsWithO("this.")) nodeStr = nodeStr.Substring(5);

                            string nodeControlName = nodeStr.Substring(0, nodeStr.IndexOf('.'));

                            if (nodeStr == nodeControlName + ".SetToolTip" &&
                                controlTypes.TryGetValue(nodeControlName, out string nodeType) &&
                                nodeType == "ToolTip" &&
                                ies.ArgumentList.Arguments.Count == 2)
                            {
                                curNode.ControlName = ies.DescendantNodes().First(static x => x is IdentifierNameSyntax).ToString();
                                curNode.PropName = "SetToolTip";
                                CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                                props.HasSetToolTip = true;
                                break;
                            }
                        }
                    }
                }
            }

            #endregion

            var aes = (AssignmentExpressionSyntax?)exp.DescendantNodes().FirstOrDefault(static x => x is AssignmentExpressionSyntax);
            if (aes?.Left is not MemberAccessExpressionSyntax left) continue;

            curNode.ControlName = left.DescendantNodes().First(static x => x is IdentifierNameSyntax).ToString();
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
                        if (val.TrimEnd(';').EndsWithO(".Resources.AngelLoader"))
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
                case "HeaderText":
                case "ToolTipText":
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
                        case "HeaderText":
                            props.HasHeaderText = true;
                            break;
                        case "ToolTipText":
                            props.HasToolTipText = true;
                            break;
                    }
                    break;
                }
                case "Anchor":
                {
                    SyntaxNode[] anchorStyles = aes.Right.DescendantNodesAndSelf()
                        .Where(static x =>
                            x is MemberAccessExpressionSyntax &&
                            x.ToString().StartsWithO("System.Windows.Forms.AnchorStyles."))
                        .ToArray();

                    CProps props = controlProperties.GetOrAddProps(curNode.ControlName);
                    props.HasDefaultAnchor =
                        anchorStyles.Length == 2 &&
                        Array.Find(anchorStyles, static x => x.ToString() == "System.Windows.Forms.AnchorStyles.Top") != null &&
                        Array.Find(anchorStyles, static x => x.ToString() == "System.Windows.Forms.AnchorStyles.Left") != null;
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
                if (t.IsKind(SyntaxKind.SingleLineCommentTrivia))
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
                    // Copied from SettingsForm old manual init:
                    // Un-obvious hack: If we DON'T set Text to something, anything, here, then first render (if paths tab
                    // is the startup tab) is really slow. We just set a one-char blank space to prevent that(?!) Probably
                    // something to do with this activating some kind of render routine beforehand... I guess... who knows...
                    destNode.OverrideLine = "// Hack to prevent slow first render on some forms if Text is blank\r\n" +
                                            "this.Text = \" \";";
                }
                else
                {
                    destNode.IgnoreExceptForComments = true;
                }
            }
            else if (destNode.PropName == "HeaderText" && props.HasHeaderText &&
                     (!controlAttributes.TryGetValue(destNode.ControlName, out attr) ||
                      attr != GenAttributes.FenGenDoNotRemoveHeaderTextAttribute))
            {
                destNode.IgnoreExceptForComments = true;
            }
            else if (destNode.PropName == "ToolTipText" && props.HasToolTipText &&
                     (!controlAttributes.TryGetValue(destNode.ControlName, out attr) ||
                      attr != GenAttributes.FenGenDoNotRemoveToolTipTextAttribute))
            {
                destNode.IgnoreExceptForComments = true;
            }
            else if (destNode.PropName == "SetToolTip" && props.HasSetToolTip &&
                     (!controlAttributes.TryGetValue(destNode.ControlName, out attr) ||
                      attr != GenAttributes.FenGenDoNotRemoveToolTipTextAttribute))
            {
                destNode.IgnoreExceptForComments = true;
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
                destNode.OverrideLine = "this.Icon = AngelLoader.Forms.AL_Icon.AngelLoader;";
            }
            else if (destNode.PropName == "Location" &&
                     (
                         props.Location is { X: 0, Y: 0 } ||
                         props.DockIsFill ||
                         controlsInFlowLayoutPanels.Contains(destNode.ControlName)
                     )
                    )
            {
                destNode.IgnoreExceptForComments = true;
            }
            else if (destNode.PropName == "Size" &&
                     ((props is { Size: not null, MinimumSize: not null } &&
                       props.Size == props.MinimumSize) ||

                      // Keep size if a control has subcontrols, because they might depend on it for layout
                      // (eg. if they're anchored right or bottom)
                      (!props.HasChildren &&
                       (
                           (controlAttributes.TryGetValue(destNode.ControlName, out attr) &&
                            attr == GenAttributes.FenGenForceRemoveSizeAttribute) ||
                           // If anchor is anything other than top-left, we need to keep the size, for reasons I'm
                           // unable to think how to explain well at the moment but you can figure it out, it's like
                           // when we go to position it, it needs to know its size so it can properly position itself
                           // relative to its anchor... you know.
                           ((props.HasDefaultAnchor != false) &&
                            (
                                props is { Size: not null, AutoSize: true } ||
                                props.AutoSize == true
                            ))
                       ))
                     ))
            {
                destNode.IgnoreExceptForComments = true;
            }
            else if (destNode.PropName == "CheckState" && props is
            { Checked: true, CheckState: CheckState.Checked } or
            { Checked: false, CheckState: CheckState.Unchecked })
            {
                destNode.IgnoreExceptForComments = true;
            }
            else if (destNode.PropName is "TabIndex" or "TabStop")
            {
                if (controlTypes.TryGetValue(destNode.ControlName, out string type))
                {
                    if (type.StartsWithO("Dark")) type = type.Substring(4);

                    if (!props.HasChildren &&
                        (type
                            is "Label"
                            or "ProgressBar"
                            or "HorizontalDivider"
                            or "PictureBox"
                            or "Control"
                            or "Panel"
                            or "DrawnPanel"
                            or "FlowLayoutPanel")
                        // TODO: Cheap hack temporarily
                        && destNode.ControlName != "PagePanel")
                    {
                        destNode.IgnoreExceptForComments = true;
                    }
                }
            }
        }

        #endregion

        // TODO: Auto-gen the ifdeffed method call switch

        var w = new CodeWriters.IndentingWriter(fileScopedNamespace ? 0 : 1, fileScopedNamespace);

        #region Create final destination file lines

        // By starting at the namespace declaration, we skip copying the #define FenGen_* thing which would
        // throw us an exception for not being in a .Designer.cs file
        for (int i = namespaceDeclarationStartLine; i <= formClassStartLine; i++)
        {
            w.AppendRawString(sourceLines[i] + "\r\n");
        }
        w.WL("{");
        w.WL("/// <summary>");
        w.WL("/// Custom generated component initializer with cruft removed.");
        w.WL("/// </summary>");
        w.WL("private void InitSlim()");
        w.WL("{");
        foreach (NodeCustom node in destNodes)
        {
            if (node.IgnoreExceptForComments)
            {
                foreach (string line in node.Comments)
                {
                    w.WL(line);
                }
            }
            else
            {
                // We might be multiple lines so just write each out individually, so we're affected by the
                // writer's formatting

                string finalLine = !node.OverrideLine.IsEmpty()
                    ? node.OverrideLine
                    : node.Node.ToFullString().TrimEnd('\r', '\n');

                if (designerFile.SplashScreen)
                {
                    string line = finalLine.Trim();

                    if (line.StartsWithO("this.")) line = line.Substring(5);
                    int index = line.IndexOf('(');
                    if (index > -1)
                    {
                        line = line.Substring(0, index);
                    }
                    if (line == "ResumeLayout")
                    {
                        continue;
                    }
                }

                string[] finalLineSplit = finalLine.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                for (int i = 0; i < finalLineSplit.Length; i++)
                {
                    w.WL(finalLineSplit[i]);
                }
            }
        }
        w.WL("}");
        w.CloseClassAndNamespace();

        #endregion

        // UTF8 with BOM or else Visual Studio complains about "different" encoding
        File.WriteAllText(destFile, w.ToString(), Encoding.UTF8);

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
        File.WriteAllLines(designerFile.FileName, sourceLines, Encoding.UTF8);
    }
}

using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class TreeViewCustom : TreeView
    {
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            bool nodeFocused = (e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused;

            Brush backColor;
            Color textColor;
            if (e.Node == SelectedNode)
            {
                backColor = Focused && !nodeFocused ? SystemBrushes.Window : SystemBrushes.Highlight;
                textColor = Focused && !nodeFocused ? ForeColor : SystemColors.HighlightText;
            }
            else
            {
                backColor = nodeFocused ? SystemBrushes.Highlight : SystemBrushes.Window;
                textColor = nodeFocused ? SystemColors.HighlightText : ForeColor;
            }

            e.Graphics.FillRectangle(backColor, e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, e.Bounds, textColor);

            base.OnDrawNode(e);
        }
    }
}

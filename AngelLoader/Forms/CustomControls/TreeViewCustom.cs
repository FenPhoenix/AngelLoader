using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms.CustomControls
{
    // TODO: @DarkMode(TreeViewCustom): Switch to DarkTreeView and subclass this from it, and ensure we draw with the right colors
    internal sealed class TreeViewCustom : TreeView
    {
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            bool nodeFocused = (e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused;

            bool darkMode = Misc.Config.VisualTheme == VisualTheme.Dark;

            // @DarkMode(TreeViewCustom): TEMP
            var bgBrush = darkMode ? DarkColors.Fen_ControlBackgroundBrush : SystemBrushes.Window;

            Brush backColorBrush;
            Color textColor;
            if (e.Node == SelectedNode)
            {
                //backColorBrush = Focused && !nodeFocused ? SystemBrushes.Window : SystemBrushes.Highlight;
                backColorBrush = Focused && !nodeFocused ? bgBrush : SystemBrushes.Highlight;
                textColor = Focused && !nodeFocused ? ForeColor : SystemColors.HighlightText;
            }
            else
            {
                //backColorBrush = nodeFocused ? SystemBrushes.Highlight : SystemBrushes.Window;
                backColorBrush = nodeFocused ? SystemBrushes.Highlight : bgBrush;
                textColor = nodeFocused ? SystemColors.HighlightText : ForeColor;
            }

            e.Graphics.FillRectangle(backColorBrush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, e.Bounds, textColor);

            base.OnDrawNode(e);
        }
    }
}

//#define DRAW_BORDER

#if DRAW_BORDER
using System;
#endif
using AngelLoader.DataClasses;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
#if DRAW_BORDER
using AngelLoader.Forms.WinFormsNative;
#endif
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

internal sealed class DarkTreeView : TreeView, IDarkable, IUpdateRegion
{
    [DefaultValue(false)]
    public bool AlwaysDrawNodesFocused { get; set; }

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            BackColor = _darkModeEnabled ? DarkColors.LightBackground : SystemColors.Window;
        }
    }

#if DEBUG

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new BorderStyle BorderStyle { get; set; }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new TreeViewDrawMode DrawMode { get; set; }

#endif

    public DarkTreeView()
    {
        base.BorderStyle = BorderStyle.FixedSingle;

        // @DarkModeNote(DarkTreeView - close/expand buttons note)
        // We currently draw the close/expand buttons in a theme renderer, which is quick and easy. If we
        // wanted to draw them here, we would have to use OwnerDrawAll mode, and then we would have to draw
        // everything - close/expand buttons, dotted lines, text, icons if there are any, etc.
        base.DrawMode = TreeViewDrawMode.OwnerDrawText;

        HideSelection = false;
    }

    #region Event overrides

    protected override void OnDrawNode(DrawTreeNodeEventArgs e)
    {
        bool nodeFocused = (e.State & TreeNodeStates.Focused) != 0;

        bool darkMode = _darkModeEnabled;

        Brush bgBrush_Normal = darkMode ? DarkColors.LightBackgroundBrush : SystemBrushes.Window;
        Brush bgBrush_Highlighted_Focused = darkMode ? DarkColors.BlueSelectionBrush : SystemBrushes.Highlight;
        Brush bgBrush_Highlighted_NotFocused = darkMode ? DarkColors.GreySelectionBrush : SystemBrushes.ControlLight;

        Color textColor_Normal = darkMode ? DarkColors.LightText : SystemColors.ControlText;
        Color textColor_Highlighted_Focused = darkMode ? DarkColors.Fen_HighlightText : SystemColors.HighlightText;
        Color textColor_Highlighted_NotFocused = darkMode ? DarkColors.Fen_HighlightText : SystemColors.ControlText;

        Brush backColorBrush;
        Color textColor;
        if (e.Node == SelectedNode)
        {
            if (AlwaysDrawNodesFocused)
            {
                backColorBrush = Focused && !nodeFocused ? bgBrush_Normal : bgBrush_Highlighted_Focused;
                textColor = Focused && !nodeFocused ? textColor_Normal : textColor_Highlighted_Focused;
            }
            else
            {
                if (Focused)
                {
                    backColorBrush = nodeFocused ? bgBrush_Highlighted_Focused : bgBrush_Normal;
                    textColor = nodeFocused ? textColor_Highlighted_Focused : textColor_Normal;
                }
                else
                {
                    backColorBrush = bgBrush_Highlighted_NotFocused;
                    textColor = textColor_Highlighted_NotFocused;
                }
            }
        }
        else
        {
            backColorBrush = nodeFocused ? bgBrush_Highlighted_Focused : bgBrush_Normal;
            textColor = nodeFocused ? textColor_Highlighted_Focused : textColor_Normal;
        }

        // IMPORTANT(TreeView node draw): DO NOT change any of the params "e.Node.Bounds" or "TextFormatFlags.NoPrefix"
        // They have to be just so or we get one or more of several different visual problems.
        // Don't use e.Bounds, keep e.Node.Bounds.
        if (e.Node != null)
        {
            e.Graphics.FillRectangle(backColorBrush, e.Node.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, e.Node.Bounds, textColor, TextFormatFlags.NoPrefix);
        }

        base.OnDrawNode(e);
    }

    #endregion

#if DRAW_BORDER
    private void DrawBorder(IntPtr hWnd)
    {
        // This draws a buggy extra border an item-height high at the bottom if we collapse an item.
        // Everything seems to look fine without this, so disabling for now.
        return;

        if (!_darkModeEnabled || base.BorderStyle == BorderStyle.None) return;

        using var gc = new Native.GraphicsContext(hWnd);
        gc.G.DrawRectangle(DarkColors.LighterBackgroundPen, new Rectangle(1, 1, Width - 3, Height - 3));
        gc.G.DrawRectangle(DarkColors.LightBorderPen, new Rectangle(0, 0, Width - 1, Height - 1));
    }
#endif

#if DRAW_BORDER
    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case Native.WM_PAINT:
                base.WndProc(ref m);
                if (_darkModeEnabled) DrawBorder(m.HWnd);
                break;
            case Native.WM_NCPAINT:
                if (_darkModeEnabled) DrawBorder(m.HWnd);
                base.WndProc(ref m);
                break;
            default:
                base.WndProc(ref m);
                break;
        }
    }
#endif
}

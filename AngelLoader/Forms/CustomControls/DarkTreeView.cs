﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class DarkTreeView : TreeView, IDarkable
    {
        [DefaultValue(false)]
        [PublicAPI]
        public bool AlwaysDrawNodesFocused { get; set; }

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color DarkModeBackColor { get; set; } = DarkColors.LightBackground;

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SolidBrush DarkModeBackColorBrush { get; set; } = DarkColors.LightBackgroundBrush;

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeViewDrawMode DrawMode => base.DrawMode;

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (_darkModeEnabled)
                {
                    BackColor = DarkModeBackColor;
                }
                else
                {
                    BackColor = SystemColors.Window;
                }
            }
        }

        public DarkTreeView()
        {
            DoubleBuffered = true;

            BorderStyle = BorderStyle.FixedSingle;

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
            bool nodeFocused = (e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused;

            bool darkMode = _darkModeEnabled;

            Brush bgBrush_Normal = darkMode ? DarkModeBackColorBrush : SystemBrushes.Window;
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

            e.Graphics.FillRectangle(backColorBrush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, e.Bounds, textColor);

            base.OnDrawNode(e);
        }

        #endregion

        private void DrawBorder(IntPtr hWnd)
        {
            // This draws a buggy extra border an item-height high at the bottom if we collapse an item.
            // Everything seems to look fine without this, so disabling for now.
            return;

            if (!_darkModeEnabled || BorderStyle == BorderStyle.None) return;

            using var gc = new Native.GraphicsContext(hWnd);
            gc.G.DrawRectangle(DarkColors.LighterBackgroundPen, new Rectangle(1, 1, Width - 3, Height - 3));
            gc.G.DrawRectangle(DarkColors.LightBorderPen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

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
    }
}

using System;
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
        public bool AlwaysDrawNodesFocused { get; set; }

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
                    BackColor = DarkColors.LightBackground;
                }
                else
                {
                    BackColor = SystemColors.Window;
                }
            }
        }

        public DarkTreeView()
        {
            BorderStyle = BorderStyle.FixedSingle;

            // @DarkModeNote(DarkTreeView): Our +/- buttons are not drawn dark, but they look okay for now.
            // Apparently we have to switch to OwnerDrawAll in order to draw them. We can do that in the future
            // if we feel like polishing it up.
            DrawMode = TreeViewDrawMode.OwnerDrawText;

            HideSelection = false;
        }

        #region Event overrides

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            bool nodeFocused = (e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused;

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

            e.Graphics.FillRectangle(backColorBrush, e.Bounds);
            TextRenderer.DrawText(e.Graphics, e.Node.Text, Font, e.Bounds, textColor);

            base.OnDrawNode(e);
        }

        #endregion

        private void DrawBorder(IntPtr hWnd)
        {
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

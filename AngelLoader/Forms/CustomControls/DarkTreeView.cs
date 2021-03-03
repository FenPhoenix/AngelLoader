using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class DarkTreeView : TreeView, IDarkableScrollableNative
    {
        [DefaultValue(false)]
        public bool AlwaysDrawNodesFocused { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Suspended { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native? VerticalVisualScrollBar { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native? HorizontalVisualScrollBar { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Corner? VisualScrollBarCorner { get; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? Scroll;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control? ClosestAddableParent => Parent;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? DarkModeChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? RefreshIfNeededForceCorner;

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (_darkModeEnabled)
                {
                    BackColor = DarkColors.Fen_ControlBackground;
                }
                else
                {
                    BackColor = SystemColors.Window;
                }

                DarkModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DarkTreeView()
        {
            BorderStyle = BorderStyle.FixedSingle;

            // TODO: @DarkMode(DarkTreeView): Switch to OwnerDrawAll so we can draw dark plus/minus buttons
            DrawMode = TreeViewDrawMode.OwnerDrawText;

            HideSelection = false;

            VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
            VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
        }

        #region Visible / Show / Hide overrides

        [PublicAPI]
        public new bool Visible
        {
            get => base.Visible;
            set
            {
                if (value)
                {
                    // Do this before setting the Visible value to avoid the classic-bar-flicker
                    VerticalVisualScrollBar?.ForceSetVisibleState(true);
                    HorizontalVisualScrollBar?.ForceSetVisibleState(true);
                    base.Visible = true;
                }
                else
                {
                    base.Visible = false;
                    VerticalVisualScrollBar?.ForceSetVisibleState(false);
                    HorizontalVisualScrollBar?.ForceSetVisibleState(false);
                }
            }
        }

        [PublicAPI]
        public new void Show()
        {
            VerticalVisualScrollBar?.ForceSetVisibleState(true);
            HorizontalVisualScrollBar?.ForceSetVisibleState(true);
            base.Show();
        }

        [PublicAPI]
        public new void Hide()
        {
            base.Hide();
            VerticalVisualScrollBar?.ForceSetVisibleState(false);
            HorizontalVisualScrollBar?.ForceSetVisibleState(false);
        }

        #endregion

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            bool nodeFocused = (e.State & TreeNodeStates.Focused) == TreeNodeStates.Focused;

            bool darkMode = _darkModeEnabled;

            Brush bgBrush_Normal = darkMode ? DarkColors.Fen_ControlBackgroundBrush : SystemBrushes.Window;
            Brush bgBrush_Highlighted_Focused = darkMode ? DarkColors.BlueSelectionBrush : SystemBrushes.Highlight;
            Brush bgBrush_Highlighted_NotFocused = darkMode ? DarkColors.GreySelectionBrush : SystemBrushes.ControlLight;

            Color textColor_Normal = darkMode ? DarkColors.LightText : SystemColors.ControlText;
            Color textColor_Highlighted_Focused = darkMode ? DarkColors.LightText : SystemColors.HighlightText;
            Color textColor_Highlighted_NotFocused = darkMode ? DarkColors.LightText : SystemColors.ControlText;

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
                        if (nodeFocused)
                        {
                            backColorBrush = bgBrush_Highlighted_Focused;
                            textColor = textColor_Highlighted_Focused;
                        }
                        else
                        {
                            backColorBrush = bgBrush_Normal;
                            textColor = textColor_Normal;
                        }
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

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        private void DrawBorder(IntPtr hWnd)
        {
            if (_darkModeEnabled && BorderStyle != BorderStyle.None)
            {
                using var dc = new Native.DeviceContext(hWnd);
                using Graphics g = Graphics.FromHdc(dc.DC);
                g.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, new Rectangle(1, 1, Width - 3, Height - 3));
                g.DrawRectangle(DarkColors.LightBorderPen, new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Native.WM_PAINT:
                case Native.WM_VSCROLL:
                case Native.WM_HSCROLL:
                    base.WndProc(ref m);
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                        if (m.Msg == Native.WM_PAINT) DrawBorder(m.HWnd);
                    }
                    break;
                case Native.WM_CTLCOLORSCROLLBAR:
                case Native.WM_NCPAINT:
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                        if (m.Msg == Native.WM_NCPAINT) DrawBorder(m.HWnd);
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}

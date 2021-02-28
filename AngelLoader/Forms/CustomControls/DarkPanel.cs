using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkPanel : Panel, IDarkableScrollableNative
    {
        private Color? _origForeColor;
        private Color? _origBackColor;

        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                if (_darkModeEnabled)
                {
                    _origForeColor ??= ForeColor;
                    _origBackColor ??= BackColor;

                    ForeColor = DarkColors.LightText;
                    BackColor = DarkColors.Fen_ControlBackground;
                }
                else
                {
                    if (_origForeColor != null) ForeColor = (Color)_origForeColor;
                    if (_origBackColor != null) BackColor = (Color)_origBackColor;
                }
                DarkModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DarkPanel()
        {
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
                    VerticalVisualScrollBar?.ForceSetVisibleState(true);
                    HorizontalVisualScrollBar?.ForceSetVisibleState(true);
                    base.Visible = value;
                }
                else
                {
                    base.Visible = value;
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

        #region Event overrides

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        [Browsable(false)]
        public bool Suspended { get; set; }
        [Browsable(false)]
        public ScrollBarVisualOnly_Native? VerticalVisualScrollBar { get; }
        [Browsable(false)]
        public ScrollBarVisualOnly_Native? HorizontalVisualScrollBar { get; }
        [Browsable(false)]
        public ScrollBarVisualOnly_Corner? VisualScrollBarCorner { get; }
        public new event EventHandler? Scroll;
        [Browsable(false)]
        public Control? ClosestAddableParent => Parent;
        [Browsable(false)]
        public event EventHandler? DarkModeChanged;
        [Browsable(false)]
        public event EventHandler? RefreshIfNeededForceCorner;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Native.WM_NCPAINT:
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                case Native.WM_PAINT:
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    }
                    base.WndProc(ref m);
                    break;
                case Native.WM_VSCROLL:
                case Native.WM_HSCROLL:
                    // Do this FIRST, otherwise we don't update on mousedown
                    base.WndProc(ref m);
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}

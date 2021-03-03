using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkTextBox : TextBox, IDarkableScrollableNative
    {
        private bool _origValuesStored;
        private Color? _origForeColor;
        private Color? _origBackColor;
        private Padding? _origPadding;
        private BorderStyle? _origBorderStyle;

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

                var sbi_v = new Native.SCROLLBARINFO { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
                int result_v = Native.GetScrollBarInfo(Handle, Native.OBJID_VSCROLL, ref sbi_v);

                bool vertScrollBarNeedsRepositioning =
                    result_v != 0 &&
                    (sbi_v.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE &&
                    (sbi_v.rgstate[0] & Native.STATE_SYSTEM_UNAVAILABLE) != Native.STATE_SYSTEM_UNAVAILABLE;

                var sbi_h = new Native.SCROLLBARINFO { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
                int result_h = Native.GetScrollBarInfo(Handle, Native.OBJID_HSCROLL, ref sbi_h);

                bool horzScrollBarNeedsRepositioning =
                    result_h != 0 &&
                    (sbi_h.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE &&
                    (sbi_h.rgstate[0] & Native.STATE_SYSTEM_UNAVAILABLE) != Native.STATE_SYSTEM_UNAVAILABLE;

                Native.SCROLLINFO? si_v = null, si_h = null;
                if (vertScrollBarNeedsRepositioning) si_v = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
                if (horzScrollBarNeedsRepositioning) si_h = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_HORZ);

                if (_darkModeEnabled)
                {
                    if (!_origValuesStored)
                    {
                        _origForeColor ??= ForeColor;
                        _origBackColor ??= BackColor;
                        _origPadding ??= Padding;
                        _origBorderStyle ??= BorderStyle;
                        _origValuesStored = true;
                    }

                    BackColor = DarkColors.LightBackground;
                    ForeColor = DarkColors.LightText;
                    Padding = new Padding(2, 2, 2, 2);
                    BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    if (_origValuesStored)
                    {
                        BackColor = (Color)_origBackColor!;
                        ForeColor = (Color)_origForeColor!;
                        Padding = (Padding)_origPadding!;
                        BorderStyle = (BorderStyle)_origBorderStyle!;
                    }
                }

                if (vertScrollBarNeedsRepositioning && si_v != null)
                {
                    ControlUtils.RepositionScroll(Handle, (Native.SCROLLINFO)si_v, Native.SB_VERT);
                }
                if (horzScrollBarNeedsRepositioning && si_h != null)
                {
                    ControlUtils.RepositionScroll(Handle, (Native.SCROLLINFO)si_h, Native.SB_HORZ);
                }

                DarkModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [PublicAPI]
        public new bool Multiline
        {
            get => base.Multiline;
            set
            {
                base.Multiline = value;
                if (value &&
                    VerticalVisualScrollBar == null &&
                    HorizontalVisualScrollBar == null &&
                    VisualScrollBarCorner == null)
                {
                    VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
                    HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
                    VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
                }
            }
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

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Suspended { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native? VerticalVisualScrollBar { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native? HorizontalVisualScrollBar { get; private set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Corner? VisualScrollBarCorner { get; private set; }

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

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Native.WM_PAINT:
                case Native.WM_VSCROLL:
                case Native.WM_HSCROLL:
                    base.WndProc(ref m);
                    if (_darkModeEnabled) RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    break;
                case Native.WM_CTLCOLORSCROLLBAR:
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
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkListBox : ListBox, IDarkableScrollableNative
    {
        // TODO: @DarkMode(DarkListBox): Horizontal scrollbars issue
        // We can't handle horizontal scrollbar for some reason. Even if we set the HorizontalExtent, the items
        // are drawn completely broken when scrolled horizontally. Probably doing something wrong in OnPaint(),
        // but consider just switching all ListBoxes to ListView or DataGridViews in virtual mode and turning off
        // all headers and everything so it's just a single line that looks just like a ListBox.
        // We know DataGridView scrolling works and we know how to use it, so we could just use that.

        private uint _origStyle;
        private uint _origExStyle;
        private bool _origIntegralHeight;

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
                SetUpTheme();
                DarkModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // ResizeRedraw doesn't do it. We have to do it manually...
        protected override void OnResize(EventArgs e)
        {
            if (_darkModeEnabled)
            {
                // TODO: @DarkMode(DarkListBox: OnResize()):
                // If we're in dark mode, then we're always IntegralHeight = false even if we would be true in
                // classic mode. Technically we should implement IntegralHeight manually here, but we don't ever
                // change the height of IntegralHeight ListBoxes currently. We probably won't either because it's
                // janky and looks terrible, but if we ever need to, then implement it here.

                Native.RedrawWindow(
                    Handle,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    Native.RedrawWindowFlags.Frame |
                    Native.RedrawWindowFlags.UpdateNow |
                    Native.RedrawWindowFlags.Invalidate);
            }
            base.OnResize(e);
        }

        public DarkListBox()
        {
            VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            HorizontalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: false, passMouseWheel: true);
            VisualScrollBarCorner = new ScrollBarVisualOnly_Corner(this);
        }

        private void SetUpTheme()
        {
            // Order matters here

            if (_darkModeEnabled)
            {
                _origIntegralHeight = IntegralHeight;
                IntegralHeight = false;

                SetStyle(
                    ControlStyles.Opaque |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, true);

                DrawMode = DrawMode.OwnerDrawVariable;
                BackColor = DarkColors.Fen_ControlBackground;
                ForeColor = DarkColors.LightText;

                uint style = Native.GetWindowLongPtr(Handle, Native.GWL_STYLE).ToUInt32();
                uint exStyle = Native.GetWindowLongPtr(Handle, Native.GWL_EXSTYLE).ToUInt32();

                _origStyle = style;
                _origExStyle = exStyle;

                style |= Native.WS_BORDER;
                exStyle &= ~Native.WS_EX_CLIENTEDGE;

                Native.SetWindowLongPtr(Handle, Native.GWL_STYLE, (UIntPtr)style);
                Native.SetWindowLongPtr(Handle, Native.GWL_EXSTYLE, (UIntPtr)exStyle);

                Native.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                    Native.SetWindowPosFlags.DoNotChangeOwnerZOrder
                    | Native.SetWindowPosFlags.IgnoreMove
                    | Native.SetWindowPosFlags.IgnoreResize
                    | Native.SetWindowPosFlags.DoNotActivate
                    | Native.SetWindowPosFlags.DrawFrame);
            }
            else
            {
                IntegralHeight = _origIntegralHeight;

                Native.SetWindowLongPtr(Handle, Native.GWL_STYLE, (UIntPtr)_origStyle);
                Native.SetWindowLongPtr(Handle, Native.GWL_EXSTYLE, (UIntPtr)_origExStyle);

                Native.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                    Native.SetWindowPosFlags.DoNotChangeOwnerZOrder
                    | Native.SetWindowPosFlags.IgnoreMove
                    | Native.SetWindowPosFlags.IgnoreResize
                    | Native.SetWindowPosFlags.DoNotActivate
                    | Native.SetWindowPosFlags.DrawFrame);

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint, true);
                SetStyle(
                    ControlStyles.Opaque |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer, false);

                BackColor = SystemColors.Window;
                ForeColor = SystemColors.WindowText;

                DrawMode = DrawMode.Normal;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnPaint(e);
                return;
            }

            var g = e.Graphics;

            var itemsToDraw = new List<DrawItemEventArgs>(Items.Count);

            if (Items.Count > 0)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var itemRect = GetItemRectangle(i);

                    if (itemRect.IntersectsWith(ClientRectangle))
                    {
                        DrawItemState state =
                            (this.SelectionMode == SelectionMode.One && SelectedIndex == i) ||
                            (this.SelectionMode != SelectionMode.None && SelectedIndices.Contains(i))
                                ? DrawItemState.Selected
                                : DrawItemState.Default;

                        itemsToDraw.Add(new DrawItemEventArgs(g, Font, itemRect, i, state, ForeColor, BackColor));
                    }
                }
            }

            // We have to draw the background in case there are no items or not enough items to fill out the size.
            // Very slightly wasteful because some of it may be painted over by the items later, but meh.
            g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, ClientRectangle);

            for (int i = 0; i < itemsToDraw.Count; i++)
            {
                OnDrawItem(itemsToDraw[i]);
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (!_darkModeEnabled)
            {
                base.OnDrawItem(e);
                return;
            }

            if (e.Index == -1) return;

            var itemRect = new Rectangle(
                e.Bounds.X + 1,
                e.Bounds.Y + 1,
                e.Bounds.Width - 2,
                e.Bounds.Height
            );

            e.Graphics.FillRectangle((e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? DarkColors.BlueSelectionBrush
                : DarkColors.Fen_ControlBackgroundBrush, itemRect);

            // No TextAlign property, so leave constant
            const TextFormatFlags textFormat =
                TextFormatFlags.Default |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoPadding |
                TextFormatFlags.NoClipping;

            var textRect = new Rectangle(e.Bounds.X + 3, e.Bounds.Y + 1, e.Bounds.Width - 3, e.Bounds.Height - 1);
            TextRenderer.DrawText(e.Graphics, Items[e.Index].ToString(), e.Font, textRect, e.ForeColor, textFormat);
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

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Suspended { get; set; }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScrollBarVisualOnly_Corner VisualScrollBarCorner { get; }
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control? ClosestAddableParent => Parent;
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? DarkModeChanged;
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? Scroll;
        
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public event EventHandler? RefreshIfNeededForceCorner;

        private void WmNcPaint(IntPtr hWnd)
        {
            if (_darkModeEnabled && BorderStyle != BorderStyle.None)
            {
                using var dc = new Native.DeviceContext(hWnd);
                using Graphics g = Graphics.FromHdc(dc.DC);
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
                case Native.WM_ERASEBKGND:
                    base.WndProc(ref m);
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case Native.WM_CTLCOLORSCROLLBAR:
                case Native.WM_NCPAINT:
                    if (_darkModeEnabled)
                    {
                        RefreshIfNeededForceCorner?.Invoke(this, EventArgs.Empty);
                        if (m.Msg == Native.WM_NCPAINT)
                        {
                            WmNcPaint(m.HWnd);
                        }
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

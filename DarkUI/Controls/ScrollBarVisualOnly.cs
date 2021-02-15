using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;
using Gma.System.MouseKeyHook;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly : ScrollBarVisualOnly_Base
    {
        #region Private fields

        private readonly ScrollBar _owner;

        #endregion

        #region Constructor / init

        public ScrollBarVisualOnly(ScrollBar owner) : base(owner is VScrollBar)
        {
            #region Set up self

            Visible = false;
            DoubleBuffered = true;
            ResizeRedraw = true;

            BackColor = Config.Colors.DarkBackground;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.CacheText,
                true);

            #endregion

            #region Setup involving owner

            _owner = owner;

            if (_owner.Parent != null)
            {
                _owner.Parent.Controls.Add(this);
                BringToFront();
                if (owner.Parent is IDarkableScrollable ids)
                {
                    _owner.Parent.Paint += (sender, e) => PaintDarkScrollBars(ids, e);
                }
            }

            _owner.VisibleChanged += (sender, e) => { if (_owner.Visible) BringToFront(); };
            _owner.Scroll += (sender, e) => { if (_owner.Visible || Visible) RefreshIfNeeded(); };

            #endregion

            #region Set up refresh timer

            _timer.Interval = 1;
            _timer.Tick += (sender, e) => RefreshIfNeeded();

            #endregion

            #region Set up scroll bar arrows

            _upArrowNormal.RotateFlip(RotateFlipType.Rotate180FlipNone);
            _upArrowHot.RotateFlip(RotateFlipType.Rotate180FlipNone);
            _upArrowPressed.RotateFlip(RotateFlipType.Rotate180FlipNone);

            _leftArrowNormal.RotateFlip(RotateFlipType.Rotate90FlipNone);
            _leftArrowHot.RotateFlip(RotateFlipType.Rotate90FlipNone);
            _leftArrowPressed.RotateFlip(RotateFlipType.Rotate90FlipNone);

            _rightArrowNormal.RotateFlip(RotateFlipType.Rotate270FlipNone);
            _rightArrowHot.RotateFlip(RotateFlipType.Rotate270FlipNone);
            _rightArrowPressed.RotateFlip(RotateFlipType.Rotate270FlipNone);

            #endregion

            #region Set up thumb colors

            _thumbNormalBrush = new SolidBrush(Config.Colors.GreySelection);
            _thumbHotBrush = new SolidBrush(Config.Colors.GreyHighlight);
            _thumbPressedBrush = new SolidBrush(Config.Colors.DarkGreySelection);

            #endregion

            #region Set up mouse hook

            if (Global.MouseHook == null) Global.MouseHook = Hook.AppEvents();

            Global.MouseHook.MouseDownExt += MouseDownExt_Handler;
            Global.MouseHook.MouseUpExt += MouseUpExt_Handler;
            Global.MouseHook.MouseMoveExt += MouseMoveExt_Handler;

            #endregion
        }

        #endregion

        #region Private methods

        internal override Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            var sbi = new Native.SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollBarInfo(_owner.Handle, Native.OBJID_CLIENT, ref sbi);
            }

            return sbi;
        }

        private void RefreshIfNeeded()
        {
            // Refresh only if our thumb's size/position is stale. Otherwise, we get unacceptable lag.
            var sbi = GetCurrentScrollBarInfo();
            if (_xyThumbTop == null)
            {
                _xyThumbTop = sbi.xyThumbTop;
                _xyThumbBottom = sbi.xyThumbBottom;
                Refresh();
            }
            else
            {
                if (sbi.xyThumbTop != _xyThumbTop || sbi.xyThumbBottom != _xyThumbBottom)
                {
                    Refresh();
                }
            }
        }

        private static void PaintDarkScrollBars(IDarkableScrollable control, PaintEventArgs e)
        {
            if (control.DarkModeEnabled)
            {
                for (int i = 0; i < 2; i++)
                {
                    var realScrollBar = i == 0 ? control.VerticalScrollBar : control.HorizontalScrollBar;
                    var visualScrollBar = i == 0 ? control.VerticalVisualScrollBar : control.HorizontalVisualScrollBar;

                    // PERF: Check if we need to set expensive properties before setting expensive properties
                    if (realScrollBar.Visible)
                    {
                        visualScrollBar.Location = realScrollBar.Location;
                        visualScrollBar.Size = realScrollBar.Size;
                        visualScrollBar.Anchor = realScrollBar.Anchor;
                    }

                    if (realScrollBar.Visible != visualScrollBar.Visible)
                    {
                        visualScrollBar.Visible = realScrollBar.Visible;
                    }
                }

                if (control.VerticalScrollBar.Visible && control.HorizontalScrollBar.Visible)
                {
                    // Draw the corner in between the two scroll bars
                    // TODO: @DarkMode: Also cache this brush
                    using (var b = new SolidBrush(control.VerticalVisualScrollBar.BackColor))
                    {
                        e.Graphics.FillRectangle(b, new Rectangle(
                            control.VerticalScrollBar.Location.X,
                            control.HorizontalScrollBar.Location.Y,
                            control.VerticalScrollBar.Width,
                            control.HorizontalScrollBar.Height));
                    }
                }
            }
            else
            {
                control.VerticalVisualScrollBar.Hide();
                control.HorizontalVisualScrollBar.Hide();
            }
        }

        #endregion

        #region Event overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            #region Arrows

            int w, h;
            if (_isVertical)
            {
                w = SystemInformation.VerticalScrollBarWidth;
                h = SystemInformation.VerticalScrollBarArrowHeight;
            }
            else
            {
                w = SystemInformation.HorizontalScrollBarHeight;
                h = SystemInformation.HorizontalScrollBarArrowWidth;
            }

            if (_isVertical)
            {
                Bitmap upArrow = _firstArrowState == State.Normal
                    ? _upArrowNormal
                    : _firstArrowState == State.Hot
                    ? _upArrowHot
                    : _upArrowPressed;

                Bitmap downArrow = _secondArrowState == State.Normal
                    ? _downArrowNormal
                    : _secondArrowState == State.Hot
                    ? _downArrowHot
                    : _downArrowPressed;

                g.DrawImageUnscaled(
                    upArrow,
                    (w / 2) - (_upArrowNormal.Width / 2),
                    (h / 2) - (_upArrowNormal.Height / 2));

                g.DrawImageUnscaled(
                    downArrow,
                    (w / 2) - (_downArrowNormal.Width / 2),
                    (Height - h) + ((h / 2) - (_downArrowNormal.Height / 2)));
            }
            else
            {
                Bitmap leftArrow = _firstArrowState == State.Normal
                    ? _leftArrowNormal
                    : _firstArrowState == State.Hot
                    ? _leftArrowHot
                    : _leftArrowPressed;

                Bitmap rightArrow = _secondArrowState == State.Normal
                    ? _rightArrowNormal
                    : _secondArrowState == State.Hot
                    ? _rightArrowHot
                    : _rightArrowPressed;

                g.DrawImageUnscaled(
                    leftArrow,
                    (w / 2) - (_leftArrowNormal.Width / 2),
                    (h / 2) - (_leftArrowNormal.Height / 2));

                g.DrawImageUnscaled(
                    rightArrow,
                    Width - w + (w / 2) - (_rightArrowNormal.Width / 2),
                    (h / 2) - (_rightArrowNormal.Height / 2));
            }

            #endregion

            #region Thumb

            if (_owner.IsHandleCreated)
            {
                var sbi = GetCurrentScrollBarInfo();

                _xyThumbTop = sbi.xyThumbTop;
                _xyThumbBottom = sbi.xyThumbBottom;

                SolidBrush thumbBrush = _thumbState == State.Normal
                    ? _thumbNormalBrush
                    : _thumbState == State.Hot
                    ? _thumbHotBrush
                    : _thumbPressedBrush;

                g.FillRectangle(thumbBrush, GetVisualThumbRect(ref sbi));
            }

            #endregion

            base.OnPaint(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            _timer.Enabled = Visible;
            base.OnVisibleChanged(e);
        }

        protected override void WndProc(ref Message m)
        {
            void SendToOwner(ref Message _m)
            {
                if (_owner.IsHandleCreated)
                {
                    Native.PostMessage(_owner.Handle, _m.Msg, _m.WParam, _m.LParam);
                    //_m.Result = IntPtr.Zero;
                }
            }

            if (m.Msg == Native.WM_LBUTTONDOWN || m.Msg == Native.WM_NCLBUTTONDOWN
                || m.Msg == Native.WM_LBUTTONUP || m.Msg == Native.WM_NCLBUTTONUP
                || m.Msg == Native.WM_LBUTTONDBLCLK || m.Msg == Native.WM_NCLBUTTONDBLCLK

                || m.Msg == Native.WM_MBUTTONDOWN || m.Msg == Native.WM_NCMBUTTONDOWN
                || m.Msg == Native.WM_MBUTTONUP || m.Msg == Native.WM_NCMBUTTONUP
                || m.Msg == Native.WM_MBUTTONDBLCLK || m.Msg == Native.WM_NCMBUTTONDBLCLK

                || m.Msg == Native.WM_RBUTTONDOWN || m.Msg == Native.WM_NCRBUTTONDOWN
                || m.Msg == Native.WM_RBUTTONUP || m.Msg == Native.WM_NCRBUTTONUP
                || m.Msg == Native.WM_RBUTTONDBLCLK || m.Msg == Native.WM_NCRBUTTONDBLCLK

                //|| m.Msg == Native.WM_MOUSEMOVE || m.Msg == Native.WM_NCMOUSEMOVE

                // Don't handle mouse wheel or mouse wheel tilt for now - mousewheel at least breaks on FMsDGV
                //|| m.Msg == Native.WM_MOUSEWHEEL || m.Msg == Native.WM_MOUSEHWHEEL
                // TODO: @DarkMode: Test wheel tilt with this system!
                // (do I still have that spare Logitech mouse that works?)
                )
            {
                SendToOwner(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        #endregion
    }
}

﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;
using Gma.System.MouseKeyHook;

namespace DarkUI.Controls
{
    // TODO: Get rid of this when we combine projects of course
    internal static class Extensions
    {
        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> =>
            value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
    }

    public sealed class ScrollBarVisualOnly : Control
    {
        // Only one copy of the hook
        private static IMouseEvents _mouseHook;

        #region Private fields

        private readonly ScrollBar _owner;

        private int? _xyThumbTop;
        private int? _xyThumbBottom;

        private readonly Pen _greySelectionPen = new Pen(Config.Colors.GreySelection);

        private SolidBrush _thumbCurrentBrush;
        private readonly SolidBrush _thumbNormalBrush;
        private readonly SolidBrush _thumbHighlightedBrush;
        private readonly SolidBrush _thumbPressedBrush;

        // We want them separate, not all pointing to the same reference
        private readonly Bitmap _upArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _downArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _leftArrow = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _rightArrow = ScrollIcons.scrollbar_arrow_small_standard;

        private readonly Timer _timer = new Timer();

        private readonly bool _isVertical;

        private Point _storedCursorPosition = Point.Empty;

        private const int WM_REFLECT = 0x2000;
        private const int WM_HSCROLL = 0x0114;
        private const int WM_VSCROLL = 0x0115;

        private const int SB_THUMBTRACK = 5;

        private const int WS_EX_NOACTIVATE = 0x8000000;

        #endregion

        private static double GetPercentFromValue(int current, int total) => (double)(100 * current) / total;
        private static int GetValueFromPercent(double percent, int total) => (int)((percent / 100) * total);
        private void SetOwnerValue(int value)
        {
            _owner.Value = value.Clamp(_owner.Minimum, _owner.Maximum);

            // We have to explicitly send this message, or else the scroll bar's parent control doesn't
            // actually update itself in accordance with the scroll bar thumb position (at least with the
            // DataGridView anyway).
            uint msg = (uint)(_isVertical ? WM_VSCROLL : WM_HSCROLL);
            Native.SendMessage(_owner.Handle, WM_REFLECT + msg, (IntPtr)SB_THUMBTRACK, IntPtr.Zero);
        }

        #region Menu item event handlers

        private void ScrollHereMenuItem_Click(object sender, EventArgs e)
        {
            var sbi = GetCurrentScrollBarInfo();
            int thumbSize = sbi.xyThumbBottom - sbi.xyThumbTop;

            int arrowMargin = _isVertical
                ? SystemInformation.VerticalScrollBarArrowHeight
                : SystemInformation.HorizontalScrollBarArrowWidth;

            var rect = _isVertical
                ? new Rectangle(0, arrowMargin, Width, (Height - (arrowMargin * 2)) - thumbSize)
                : new Rectangle(arrowMargin, 0, (Width - (arrowMargin * 2)) - thumbSize, Height);

            int posAlong = _isVertical ? _storedCursorPosition.Y : +_storedCursorPosition.X;

            posAlong -= arrowMargin;

            posAlong -= thumbSize / 2;

            double posPercent = GetPercentFromValue(posAlong, _isVertical ? rect.Height : rect.Width).Clamp(0, 100);
            // Important that we use this formula (nMax - max(nPage -1, 0)) or else our position is always
            // infuriatingly not-quite-right.
            int finalValue = GetValueFromPercent(posPercent, _owner.Maximum - Math.Max(_owner.LargeChange - 1, 0));

            SetOwnerValue(finalValue);
        }

        private void MinMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(0);

        private void MaxMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Maximum);

        private void PageBackMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value - _owner.LargeChange.Clamp(_owner.Minimum, _owner.Maximum));

        private void PageForwardMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value + _owner.LargeChange.Clamp(_owner.Minimum, _owner.Maximum));

        private void ScrollBackMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value - _owner.SmallChange.Clamp(_owner.Minimum, _owner.Maximum));

        private void ScrollForwardMenuItem_Click(object sender, EventArgs e) => SetOwnerValue(_owner.Value + _owner.SmallChange.Clamp(_owner.Minimum, _owner.Maximum));

        #endregion

        #region Constructor / init

        public ScrollBarVisualOnly(ScrollBar owner)
        {
            _owner = owner;

            _isVertical = owner is VScrollBar;

            DoubleBuffered = true;
            ResizeRedraw = true;

            #region Set up refresh timer

            _timer.Interval = 1;
            _timer.Tick += (sender, e) => RefreshIfNeeded();

            #endregion

            #region Set up scroll bar arrows

            _upArrow.RotateFlip(RotateFlipType.Rotate180FlipNone);
            _leftArrow.RotateFlip(RotateFlipType.Rotate90FlipNone);
            _rightArrow.RotateFlip(RotateFlipType.Rotate270FlipNone);

            #endregion

            //BackColor = Config.Colors.Fen_DarkBackground;
            //BackColor = Color.FromArgb(44, 44, 44);
            BackColor = Config.Colors.DarkBackground;

            _thumbNormalBrush = new SolidBrush(Config.Colors.GreySelection);
            _thumbHighlightedBrush = new SolidBrush(Config.Colors.GreyHighlight);
            _thumbPressedBrush = new SolidBrush(Config.Colors.DarkGreySelection);
            //_thumbPressedBrush = new SolidBrush(Color.Red);
            _thumbCurrentBrush = _thumbNormalBrush;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.CacheText,
                true);

            if (_mouseHook == null) _mouseHook = Hook.AppEvents();

            _mouseHook.MouseDownExt += MouseDownExt_Handler;
            _mouseHook.MouseUpExt += MouseUpExt_Handler;
            _mouseHook.MouseMoveExt += MouseMoveExt_Handler;
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        var cp = base.CreateParams;
        //        cp.ExStyle |= WS_EX_NOACTIVATE;
        //        return cp;
        //    }
        //}

        #endregion

        #region Private methods

        private Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            var sbi = new Native.SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollBarInfo(_owner.Handle, Native.OBJID_CLIENT, ref sbi);
            }

            return sbi;
        }

        #endregion

        #region Public methods

        public void RefreshIfNeeded()
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

        #endregion

        // MouseMove doesn't send the buttons, despite having a Button field. The field is just always empty.
        // So we just store the value here.
        private bool _leftButtonPressed;

        // TODO: @DarkMode(Scrollbars): Match all this up exactly with stock in terms of mouse state/colors
        // Like, scroll bar should stay pressed when we move off it unless we've released the left mouse button etc.
        private void MouseDownExt_Handler(object sender, MouseEventExtArgs e)
        {
            return;
            _leftButtonPressed = true;

            if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                var sbi = GetCurrentScrollBarInfo();
                var thumbRect = GetThumbRect(sbi);

                bool cursorOverThumb = thumbRect.Contains(PointToClient(e.Location));

                if (cursorOverThumb && e.Button == MouseButtons.Left)
                {
                    if (_thumbCurrentBrush != _thumbPressedBrush)
                    {
                        _thumbCurrentBrush = _thumbPressedBrush;
                        Refresh();
                    }
                }
            }
        }

        private void MouseUpExt_Handler(object sender, MouseEventExtArgs e)
        {
            return;
            _leftButtonPressed = false;
            //if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                var sbi = GetCurrentScrollBarInfo();
                var thumbRect = GetThumbRect(sbi);

                bool cursorOverThumb = thumbRect.Contains(PointToClient(e.Location));

                if (cursorOverThumb)
                {
                    if (_thumbCurrentBrush != _thumbHighlightedBrush)
                    {
                        _thumbCurrentBrush = _thumbHighlightedBrush;
                        Refresh();
                    }
                }
                else
                {
                    if (_thumbCurrentBrush != _thumbNormalBrush)
                    {
                        _thumbCurrentBrush = _thumbNormalBrush;
                        Refresh();
                    }
                }
            }
        }

        private void MouseMoveExt_Handler(object sender, MouseEventExtArgs e)
        {
            return;
            //if (ClientRectangle.Contains(PointToClient(Cursor.Position)))
            {
                var sbi = GetCurrentScrollBarInfo();
                var thumbRect = GetThumbRect(sbi);

                bool cursorOverThumb = thumbRect.Contains(PointToClient(e.Location));

                if (cursorOverThumb)
                {
                    if (_leftButtonPressed)
                    {
                        if (_thumbCurrentBrush != _thumbPressedBrush)
                        {
                            _thumbCurrentBrush = _thumbPressedBrush;
                            Refresh();
                        }
                    }
                    else
                    {
                        if (_thumbCurrentBrush != _thumbHighlightedBrush)
                        {
                            _thumbCurrentBrush = _thumbHighlightedBrush;
                            Refresh();
                        }
                    }
                }
                else
                {
                    if (_thumbCurrentBrush != _thumbNormalBrush)
                    {
                        _thumbCurrentBrush = _thumbNormalBrush;
                        Refresh();
                    }
                }
            }
        }

        #region Event overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            #region Arrows

            int w = SystemInformation.VerticalScrollBarWidth;
            int h = SystemInformation.VerticalScrollBarArrowHeight;

            if (_isVertical)
            {
                g.DrawImageUnscaled(
                    _upArrow,
                    (w / 2) - (_upArrow.Width / 2),
                    (h / 2) - (_upArrow.Height / 2));

                g.DrawImageUnscaled(
                    _downArrow,
                    (w / 2) - (_upArrow.Width / 2),
                    (Height - h) + ((h / 2) - (_upArrow.Height / 2)));
            }
            else
            {
                g.DrawImageUnscaled(
                    _leftArrow,
                    (w / 2) - (_leftArrow.Width / 2),
                    (h / 2) - (_leftArrow.Height / 2));

                g.DrawImageUnscaled(
                    _rightArrow,
                    Width - w + (w / 2) - (_leftArrow.Width / 2),
                    (h / 2) - (_leftArrow.Height / 2));
            }

            #endregion

            #region Thumb

            if (_owner.IsHandleCreated)
            {
                var sbi = GetCurrentScrollBarInfo();
                var rect = GetThumbRect(sbi);

                g.FillRectangle(_thumbCurrentBrush, rect);
            }

            #endregion

            base.OnPaint(e);
        }

        private Rectangle GetThumbRect(Native.SCROLLBARINFO sbi)
        {
            return _isVertical
                ? new Rectangle(0, sbi.xyThumbTop, Width, sbi.xyThumbBottom - sbi.xyThumbTop)
                : new Rectangle(sbi.xyThumbTop, 0, sbi.xyThumbBottom - sbi.xyThumbTop, Height);
        }

        private Rectangle GetArrowRect(bool second = false)
        {
            var vertArrowHeight = SystemInformation.VerticalScrollBarArrowHeight;
            var horzArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;

            return !second
                ? _isVertical
                    ? new Rectangle(0, 0, Width, vertArrowHeight)
                    : new Rectangle(0, 0, horzArrowWidth, Height)
                : _isVertical
                    ? new Rectangle(0, Height - vertArrowHeight, Width, vertArrowHeight)
                    : new Rectangle(Width - horzArrowWidth, 0, horzArrowWidth, Height);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            _timer.Enabled = Visible;
            base.OnVisibleChanged(e);
        }

        #endregion

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _upArrow.Dispose();
                _downArrow.Dispose();
                _leftArrow.Dispose();
                _rightArrow.Dispose();

                _greySelectionPen.Dispose();

                _thumbNormalBrush.Dispose();
                _thumbHighlightedBrush.Dispose();
                _thumbPressedBrush.Dispose();
                _thumbCurrentBrush.Dispose();

                _timer.Dispose();

                _mouseHook.MouseDownExt -= MouseDownExt_Handler;
                _mouseHook.MouseUpExt -= MouseUpExt_Handler;
                _mouseHook.MouseMoveExt -= MouseMoveExt_Handler;
            }

            base.Dispose(disposing);
        }
    }
}

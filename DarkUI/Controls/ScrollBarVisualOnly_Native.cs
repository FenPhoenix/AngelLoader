using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;
using static AL_Common.CommonUtils;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly_Native : ScrollBarVisualOnly_Base
    {
        // TODO: @DarkMode(ScrollBarVisualOnly_Native):
        // For textboxes, when the vertical scroll is available, it's always visible (but disabled when unnecessary).
        // But the horizontal one is only visible when necessary and invisible otherwise.
        // TODO: @DarkMode(ScrollBarVisualOnly_Native):
        // The scroll bar is still not right. It bumps upward when pressed, and at the very bottom we have a
        // one-line change when going between light and dark modes...

        #region Private fields

        private readonly IDarkableScrollableNative _owner;

        private readonly Dictionary<int, int> _WM_ClientToNonClient = new Dictionary<int, int>
        {
            { Native.WM_LBUTTONDOWN, Native.WM_NCLBUTTONDOWN },
            { Native.WM_LBUTTONUP, Native.WM_NCLBUTTONUP },
            { Native.WM_LBUTTONDBLCLK, Native.WM_NCLBUTTONDBLCLK },
            { Native.WM_MBUTTONDOWN, Native.WM_NCMBUTTONDOWN },
            { Native.WM_MBUTTONUP, Native.WM_NCMBUTTONUP },
            { Native.WM_MBUTTONDBLCLK, Native.WM_NCMBUTTONDBLCLK },
            { Native.WM_RBUTTONDOWN, Native.WM_NCRBUTTONDOWN },
            { Native.WM_RBUTTONUP, Native.WM_NCRBUTTONUP },
            { Native.WM_RBUTTONDBLCLK, Native.WM_NCRBUTTONDBLCLK }
        };

        #region Stored state

        private bool _addedToControls;

        private Size? _size;
        private Rectangle? _thumbLoc;
        private int _trackPos;

        private Size _ownerClientSize;

        #endregion

        #endregion

        #region Constructor / init

        public ScrollBarVisualOnly_Native(IDarkableScrollableNative owner, bool isVertical, bool passMouseWheel)
            : base(isVertical, passMouseWheel)
        {
            SetUpSelf();

            // DON'T anchor it, or we get visual glitches and chaos

            #region Setup involving owner

            _owner = owner;

            _size = Size;

            _owner.Scroll += (sender, e) => RefreshIfNeeded();
            _owner.VisibilityChanged += (sender, e) => RefreshIfNeeded(forceRefreshCorner: true);
            _owner.DarkModeChanged += (sender, e) => RefreshIfNeeded(forceRefreshCorner: true);

            #endregion

            SetUpAfterOwner();
        }

        #endregion

        #region Public methods

        public void AddToParent()
        {
            if (!_addedToControls && _owner.ClosestAddableParent != null)
            {
                _owner.ClosestAddableParent.Controls.Add(this);
                BringToFront();
                _addedToControls = true;
            }
        }

        public void ForceSetVisibleState(bool? visible = null)
        {
            if (_owner != null && _owner.IsHandleCreated && _owner.DarkModeEnabled)
            {
                var sbi = GetCurrentScrollBarInfo();
                var parentBarVisible = (sbi.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE;
                Visible = visible == null ? parentBarVisible : visible == true && parentBarVisible;
            }
        }

        #endregion

        #region Private methods

        private protected override Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            var sbi = new Native.SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollBarInfo(_owner.Handle, _isVertical ? Native.OBJID_VSCROLL : Native.OBJID_HSCROLL, ref sbi);
            }

            return sbi;
        }

        private Native.SCROLLINFO GetScrollInfo(uint mask)
        {
            var si = new Native.SCROLLINFO();
            si.cbSize = Marshal.SizeOf(si);
            si.fMask = mask;

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollInfo(_owner.Handle, _isVertical ? Native.SB_VERT : Native.SB_HORZ, ref si);
            }

            return si;
        }

        private protected override void RefreshIfNeeded(bool forceRefreshCorner = false)
        {
            if (_owner.ClosestAddableParent == null) return;
            if (!_owner.IsHandleCreated) return;

            if (!_owner.DarkModeEnabled)
            {
                if (Visible) Visible = false;
                if (_owner.VisualScrollBarCorner?.Visible == true)
                {
                    _owner.VisualScrollBarCorner.Visible = false;
                }
                return;
            }

            var sbi = GetCurrentScrollBarInfo();

            if (_owner.Suspended && _xyThumbTop != null && _xyThumbBottom != null)
            {
                return;
            }

            var parentBarVisible = (sbi.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE;

            var newVisible = !_owner.Suspended && _owner.Visible && _owner.DarkModeEnabled && parentBarVisible;

            if (newVisible != Visible) Visible = newVisible;

            var otherScrollBar = _isVertical
                ? _owner.HorizontalVisualScrollBar
                : _owner.VerticalVisualScrollBar;

            bool dontShowCorner = false;

            if ((!Visible || (otherScrollBar == null || !otherScrollBar.Visible)) &&
                _owner.VisualScrollBarCorner != null)
            {
                _owner.VisualScrollBarCorner.Visible = false;
                dontShowCorner = true;
            }

            if (Visible)
            {
                var topLeft = _owner.Parent.PointToClient(new Point(sbi.rcScrollBar.left, sbi.rcScrollBar.top));
                var bottomRight = _owner.Parent.PointToClient(new Point(sbi.rcScrollBar.right, sbi.rcScrollBar.bottom));

                var loc = new Rectangle(
                    topLeft.X,
                    topLeft.Y,
                    bottomRight.X - topLeft.X,
                    bottomRight.Y - topLeft.Y
                );

                var size = new Size(loc.Width, loc.Height);

                //var si = GetScrollInfo(Native.SIF_TRACKPOS);

                bool refresh = false;

                // Only refresh when we need to
                if ((_leftButtonPressedOnThumb /*&& si.nTrackPos != _trackPos*/) ||
                    _size != size || _thumbLoc != loc ||
                    _xyThumbTop != sbi.xyThumbTop ||
                    _xyThumbBottom != sbi.xyThumbBottom)
                {
                    Location = new Point(loc.X, loc.Y);

                    Size = size;
                    _size = size;
                    _thumbLoc = loc;
                    //_trackPos = si.nTrackPos;

                    _xyThumbTop = sbi.xyThumbTop;
                    _xyThumbBottom = sbi.xyThumbBottom;

                    refresh = true;
                }

                // Only refresh when we need to
                if (!dontShowCorner &&
                    _owner.VisualScrollBarCorner != null &&
                    _owner.VerticalVisualScrollBar != null &&
                    _owner.HorizontalVisualScrollBar != null &&
                    (forceRefreshCorner || _owner.ClientSize != _ownerClientSize))
                {
                    _ownerClientSize = _owner.ClientSize;

                    if (!_owner.VisualScrollBarCorner.Visible) _owner.VisualScrollBarCorner.Visible = true;

                    _owner.VisualScrollBarCorner.Location = new Point(
                        _owner.VerticalVisualScrollBar.Visible
                            ? _owner.VerticalVisualScrollBar.Left
                            : (_owner.HorizontalVisualScrollBar.Left + _owner.HorizontalVisualScrollBar.Width) - SystemInformation.HorizontalScrollBarArrowWidth,
                        _owner.HorizontalVisualScrollBar.Visible
                            ? _owner.HorizontalVisualScrollBar.Top
                            : (_owner.VerticalVisualScrollBar.Top + _owner.VerticalVisualScrollBar.Height) - SystemInformation.VerticalScrollBarArrowHeight
                    );
                }

                if (refresh) Refresh();
            }
        }

        #endregion

        #region Event overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_owner.IsHandleCreated)
            {
                var sbi = GetCurrentScrollBarInfo();
                bool enabled = (sbi.rgstate[0] & Native.STATE_SYSTEM_UNAVAILABLE) != Native.STATE_SYSTEM_UNAVAILABLE;

                var g = e.Graphics;

                //if (_isVertical && _owner.HorizontalVisualScrollBar == null)
                //{
                //    var si0 = GetScrollInfo(Native.SIF_ALL);
                //    Trace.WriteLine("nPage: " + si0.nPage);
                //}

                PaintArrows(g, enabled);

                #region Thumb

                if (enabled && _owner.Enabled)
                {
                    if (_leftButtonPressedOnThumb)
                    {
                        // TODO: @DarkMode(ScrollBarVisualOnly_Native.OnPaint()): This whole thing is an approximation.
                        // It works okay, but if we can figure out how we're SUPPOSED to translate the "units"
                        // into pixels, we should switch to that immediately.
                        // The WinForms ScrollBars still report the xyThumbTop/xyThumbBottom values correctly
                        // even when tracking. Looks like it may do something with WndProc()/WmReflectScroll().
                        // See if we can replicate it!

                        // TODO: @DarkMode(ScrollBarVisualOnly_Native.OnPaint()):
                        // If we set nPos from nTrackPos manually, we work exactly like the non-native version
                        // with no further hacks necessary. YES! But we're still not being clamped to the minimum
                        // thumb size. Figure that out and we're golden.
                        var si = GetScrollInfo(Native.SIF_ALL);
                        si.nPos = si.nTrackPos;
                        //si.nMin = 0;
                        //si.nMax = 100;
                        //si.nPage = (uint)Math.Min(si.nPage, si.nMax - si.nMin + 1);
                        si.nTrackPos = 0;
                        Native.SetScrollInfo(_owner.Handle,
                            (int)(_isVertical ? Native.SB_VERT : Native.SB_HORZ),
                            ref si, true);

                        sbi = GetCurrentScrollBarInfo();

                        _xyThumbTop = sbi.xyThumbTop;
                        _xyThumbBottom = sbi.xyThumbBottom;

                        //Trace.WriteLine("Native xyThumbTop: " + sbi.xyThumbTop);

                        g.FillRectangle(CurrentThumbBrush, GetVisualThumbRect(ref sbi, clampToMin: true));

                        return;

                        int thumbTop = si.nTrackPos;

                        double percentAlong = GetPercentFromValue_Double(
                            thumbTop,
                            (int)(si.nMax - Math.Max(si.nPage - 1, 0)));

                        if (percentAlong >= 99.5) percentAlong = 100;

                        int thisExtent = _isVertical ? Height : Width;

                        int scrollMarginPX = _isVertical
                            ? SystemInformation.VerticalScrollBarArrowHeight
                            : SystemInformation.HorizontalScrollBarArrowWidth;

                        int minThumbLength = _isVertical
                            ? SystemInformation.VerticalScrollBarThumbHeight
                            : SystemInformation.HorizontalScrollBarThumbWidth;

                        int thumbLength = (sbi.xyThumbBottom - sbi.xyThumbTop).Clamp(minThumbLength, int.MaxValue);

                        int thumbTopPixels = GetValueFromPercent_Rounded(
                            percentAlong,
                            (thisExtent - (scrollMarginPX * 2)) - thumbLength);

                        var rect = _isVertical
                            ? new Rectangle(1, thumbTopPixels + scrollMarginPX, Width - 2, thumbLength)
                            : new Rectangle(thumbTopPixels + scrollMarginPX, 1, thumbLength, Height - 2);

                        g.FillRectangle(_thumbPressedBrush, rect);
                    }
                    else
                    {
                        g.FillRectangle(CurrentThumbBrush, GetVisualThumbRect(ref sbi, clampToMin: true));
                    }
                }
            }

            #endregion

            base.OnPaint(e);
        }

        protected override void WndProc(ref Message m)
        {
            void SendToOwner(ref Message _m)
            {
                if (!_owner.IsHandleCreated) return;
                // We have to take our client messages and convert them to non-client messages to pass to our
                // underlying scroll bar.
                if (_WM_ClientToNonClient.TryGetValue(_m.Msg, out int ncMsg))
                {
                    // TODO: @DarkMode(ScrollBarVisualOnly_Native):
                    // Test to make sure these params transfer their signedness! Use multiple monitors and see
                    // if the value goes negative as it should, and works as it should.
                    int wParam;
                    int x = Global.SignedLOWORD(_m.LParam);
                    int y = Global.SignedHIWORD(_m.LParam);
                    Point ownerScreenLoc = _owner.Parent.PointToScreen(_owner.Location);

                    if (_isVertical)
                    {
                        int sbWidthOrHeight = SystemInformation.VerticalScrollBarWidth;
                        x += ownerScreenLoc.X + (_owner.Size.Width - sbWidthOrHeight);
                        y += ownerScreenLoc.Y;
                        wParam = Native.HTVSCROLL;
                    }
                    else
                    {
                        int sbWidthOrHeight = SystemInformation.HorizontalScrollBarHeight;
                        x += ownerScreenLoc.X;
                        y += ownerScreenLoc.Y + (_owner.Size.Height - sbWidthOrHeight);
                        wParam = Native.HTHSCROLL;
                    }

                    Native.POINTS points = new Native.POINTS((short)x, (short)y);

                    Native.PostMessage(_owner.Handle, ncMsg, (IntPtr)wParam, points);
                }
                else if (_m.Msg == Native.WM_MOUSEWHEEL || _m.Msg == Native.WM_MOUSEHWHEEL)
                {
                    Native.SendMessage(_owner.Handle, _m.Msg, _m.WParam, _m.LParam);
                }
                else if (_m.Msg == Native.WM_MOUSELEAVE || _m.Msg == Native.WM_NCMOUSELEAVE)
                {
                    // Prevents underlying scrollbar from remaining highlighted until re-moused-over when switched
                    // to classic mode
                    // TODO: @DarkMode: It can still happen in this case:
                    // You mouseover in classic mode, you switch to dark mode, you switch back to light mode (all
                    // while keeping the mouse over and only moving it away afterwards).
                    Native.SendMessage(_owner.Handle, _m.Msg, _m.WParam, _m.LParam);
                }
            }

            if (ShouldSendToOwner(m.Msg))
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

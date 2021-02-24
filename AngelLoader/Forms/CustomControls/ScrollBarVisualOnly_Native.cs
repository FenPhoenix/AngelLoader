using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using JetBrains.Annotations;
using static AL_Common.CommonUtils;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ScrollBarVisualOnly_Native : ScrollBarVisualOnly_Base
    {
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

        private Size _ownerClientSize;

        #endregion

        #endregion

        #region Constructor / init

        public ScrollBarVisualOnly_Native(IDarkableScrollableNative owner, bool isVertical, bool passMouseWheel)
            : base(isVertical, passMouseWheel)
        {
            // DON'T anchor it, or we get visual glitches and chaos

            _owner = owner;

            _size = Size;

            _owner.Scroll += (_, _) => RefreshIfNeeded();
            _owner.RefreshIfNeededForceCorner += (_, _) => RefreshIfNeeded(forceRefreshCorner: true);
            _owner.DarkModeChanged += (_, _) => RefreshIfNeeded(forceRefreshCorner: true);
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
            if (_owner.IsHandleCreated && _owner.DarkModeEnabled)
            {
                var sbi = GetCurrentScrollBarInfo();
                var parentBarVisible = (sbi.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE;
                Visible = visible == null ? parentBarVisible : visible == true && parentBarVisible;
            }
        }

        [PublicAPI]
        public void RefreshScrollBar(bool force = false) => RefreshIfNeeded(forceRefreshAll: force);

        #endregion

        #region Private methods

        private protected override bool GetOwnerEnabled() => _owner.Enabled;

        private protected override Native.SCROLLBARINFO
        GetCurrentScrollBarInfo()
        {
            var sbi = new Native.SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollBarInfo(_owner.Handle, _isVertical ? Native.OBJID_VSCROLL : Native.OBJID_HSCROLL, ref sbi);

                // When the thumb is smaller than the minimum size, it visually clamps (in classic mode) to min,
                // but our thumb top and bottom area continues shrinking down past it, causing us to be desynced
                // with the real thumb. This whole garbage is just to clamp our size. It's not 100% perfect, but
                // it works well enough for what is probably an uncommon-ish situation.
                // Note that the WinForms ScrollBar control - and by extension any control (like DataGridView)
                // that uses it - somehow avoids this and keeps the thumb top/bottom values correct even when
                // clamping its visual thumb size. I've dug into the code and messed around and can't figure out
                // how it does it. I'm tired of this crap, so have a 99% perfect one instead.
                (int minThumbLengthPX, int arrowMarginPX, _, int innerExtentPX)
                    = GetMeasurements();

                int thumbLengthPX = sbi.xyThumbBottom - sbi.xyThumbTop;

                if (thumbLengthPX < minThumbLengthPX)
                {
                    // These count from the top of the arrow, but we want them to count from the top of the inner area
                    int xyThumbTop = sbi.xyThumbTop - arrowMarginPX;
                    int xyThumbBottom = sbi.xyThumbBottom - arrowMarginPX;

                    #region Plus up

                    double p1 = GetPercentFromValue_Double(
                        xyThumbTop,
                        innerExtentPX - thumbLengthPX);

                    double plusUpDouble = GetValueFromPercent_Double(
                        p1,
                        minThumbLengthPX - thumbLengthPX);

                    double plusUp_IntPortion = Math.Truncate(plusUpDouble);
                    double plusUp_FractionalPortion = plusUpDouble - plusUp_IntPortion;

                    int plusUp = plusUp_FractionalPortion >= 0.5
                        ? (int)plusUp_IntPortion + 1
                        : (int)plusUp_IntPortion;

                    #endregion

                    #region Plus down

                    double p2 = GetPercentFromValue_Double(
                        innerExtentPX - xyThumbBottom,
                        innerExtentPX - thumbLengthPX);

                    double plusDownDouble = GetValueFromPercent_Double(
                        p2,
                        minThumbLengthPX - thumbLengthPX);

                    double plusDown_IntPortion = Math.Truncate(plusDownDouble);
                    double plusDown_FractionalPortion = plusUpDouble - plusDown_IntPortion;

                    int plusDown = plusDown_FractionalPortion >= 0.5
                        ? (int)plusDown_IntPortion + 1
                        : (int)plusDown_IntPortion;

                    #endregion

                    xyThumbTop -= plusUp;
                    xyThumbBottom += plusDown;

                    // We still get one-pixel-off positioning sometimes, but at least we stay the same size.
                    // Good enough...
                    int newThumbLength = xyThumbBottom - xyThumbTop;
                    if (newThumbLength > arrowMarginPX)
                    {
                        xyThumbBottom--;
                    }
                    else if (newThumbLength < arrowMarginPX)
                    {
                        xyThumbBottom++;
                    }

                    // Add the arrow margin back on now to make the final value correct
                    sbi.xyThumbTop = xyThumbTop + arrowMarginPX;
                    sbi.xyThumbBottom = xyThumbBottom + arrowMarginPX;
                }
            }

            return sbi;
        }

        private Native.SCROLLINFO GetScrollInfo(uint mask)
        {
            var si = new Native.SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = mask;

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollInfo(_owner.Handle, _isVertical ? Native.SB_VERT : Native.SB_HORZ, ref si);
            }

            return si;
        }

        private void RefreshIfNeeded(bool forceRefreshAll = false, bool forceRefreshCorner = false)
        {
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

            if (_owner.Suspended && _xyThumbTop != null && _xyThumbBottom != null)
            {
                return;
            }

            var sbi = GetCurrentScrollBarInfo();
            var parentBarVisible = (sbi.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE;

            var newVisible = !_owner.Suspended && _owner.Visible && _owner.DarkModeEnabled && parentBarVisible;

            if (newVisible != Visible) Visible = newVisible;

            var otherScrollBar = _isVertical
                ? _owner.HorizontalVisualScrollBar
                : _owner.VerticalVisualScrollBar;

            bool dontShowCorner = false;

            if ((!Visible || otherScrollBar == null || !otherScrollBar.Visible) &&
                _owner.VisualScrollBarCorner != null)
            {
                _owner.VisualScrollBarCorner.Visible = false;
                dontShowCorner = true;
            }

            if (Visible && _owner.Parent != null)
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

                bool refresh = false;

                // Only refresh when we need to
                if (forceRefreshAll || _leftButtonPressedOnThumb ||
                    _size != size || _thumbLoc != loc ||
                    _xyThumbTop != sbi.xyThumbTop ||
                    _xyThumbBottom != sbi.xyThumbBottom)
                {
                    Location = new Point(loc.X, loc.Y);

                    // Even though we already set this in AddParent(), some controls need it set here too...
                    BringToFront();

                    Size = size;
                    _size = size;
                    _thumbLoc = loc;

                    _xyThumbTop = sbi.xyThumbTop;
                    _xyThumbBottom = sbi.xyThumbBottom;

                    refresh = true;
                }

                // Only refresh when we need to
                if (!dontShowCorner &&
                    _owner.VisualScrollBarCorner != null &&
                    _owner.VerticalVisualScrollBar != null &&
                    _owner.HorizontalVisualScrollBar != null &&
                    (forceRefreshAll || forceRefreshCorner || _owner.ClientSize != _ownerClientSize))
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

                PaintArrows(g, enabled);

                #region Thumb

                if (enabled && _owner.Enabled)
                {
                    if (_leftButtonPressedOnThumb)
                    {
                        // If we set nPos from nTrackPos manually, we work exactly like the non-native version
                        // except that we still don't visually clamp our thumb length. But we handle that elsewhere.
                        var si = GetScrollInfo(Native.SIF_ALL);
                        si.nPos = si.nTrackPos;
                        si.nTrackPos = 0;

                        Native.SetScrollInfo(_owner.Handle, (int)(_isVertical ? Native.SB_VERT : Native.SB_HORZ), ref si, true);

                        sbi = GetCurrentScrollBarInfo();

                        _xyThumbTop = sbi.xyThumbTop;
                        _xyThumbBottom = sbi.xyThumbBottom;

                    }

                    (int minThumbLengthPX, _, _, int innerExtentPX) = GetMeasurements();

                    if (innerExtentPX >= minThumbLengthPX)
                    {
                        g.FillRectangle(CurrentThumbBrush, GetVisualThumbRect(ref sbi));
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
                if (_owner.Parent != null && _WM_ClientToNonClient.TryGetValue(_m.Msg, out int ncMsg))
                {
                    // TODO: @DarkMode(ScrollBarVisualOnly_Native):
                    // Test to make sure these params transfer their signedness! Use multiple monitors and see
                    // if the value goes negative as it should, and works as it should.
                    int wParam;
                    int x = Native.SignedLOWORD(_m.LParam);
                    int y = Native.SignedHIWORD(_m.LParam);
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

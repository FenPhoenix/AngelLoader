using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DarkUI.Win32;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly_Native : ScrollBarVisualOnly_Base
    {
        // TODO: @DarkMode(ScrollBarVisualOnly_Native): Paint the corner between vert/horz scrollbars

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

        private Size? _size;
        private Rectangle? _thumbLoc;
        private int _trackPos;

        private bool _addedToControls;

        #endregion

        #endregion

        #region Constructor / init

        public ScrollBarVisualOnly_Native(IDarkableScrollableNative owner, bool isVertical, bool passMouseWheel)
            : base(isVertical, passMouseWheel)
        {
            SetUpSelf();

            Anchor = _isVertical
                ? AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom
                : AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            #region Setup involving owner

            _owner = owner;

            _size = Size;

            BringThisToFront();

            _owner.Scroll += (sender, e) => RefreshIfNeeded();
            _owner.VisibilityChanged += (sender, e) => RefreshIfNeeded();
            _owner.DarkModeChanged += (sender, e) => RefreshIfNeeded();

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
                _addedToControls = true;
            }
        }

        public void ForceSetVisibleState()
        {
            if (_owner != null && _owner.IsHandleCreated)
            {
                var sbi = GetCurrentScrollBarInfo();
                var parentBarVisible = (sbi.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE;
                Visible = _owner.DarkModeEnabled && parentBarVisible;
            }
        }

        #endregion

        #region Private methods

        private void BringThisToFront()
        {
            AddToParent();
            if (_addedToControls) BringToFront();
        }

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

        private protected override void RefreshIfNeeded()
        {
            if (_owner.ClosestAddableParent == null) return;
            if (!_owner.IsHandleCreated) return;

            if (!_owner.DarkModeEnabled)
            {
                Visible = false;
                return;
            }

            var sbi = GetCurrentScrollBarInfo();

            if (_owner.Suspended && _xyThumbTop != null && _xyThumbBottom != null)
            {
                return;
            }

            var parentBarVisible = (sbi.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE;

            bool oldVisible = Visible;

            Visible = !_owner.Suspended && _owner.Visible && _owner.DarkModeEnabled && parentBarVisible;

            if (!Visible)
            {
                var otherScrollBar = _isVertical
                    ? _owner.HorizontalVisualScrollBar
                    : _owner.VerticalVisualScrollBar;

                if ((otherScrollBar == null || !otherScrollBar.Visible) && _owner.VisualScrollBarCorner != null)
                {
                    _owner.VisualScrollBarCorner.Visible = false;
                }
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

                var si = GetScrollInfo(Native.SIF_TRACKPOS);

                // Only refresh when we need to
                if ((_leftButtonPressedOnThumb && si.nTrackPos != _trackPos) ||
                    _size != size || _thumbLoc != loc ||
                    _xyThumbTop != sbi.xyThumbTop ||
                    _xyThumbBottom != sbi.xyThumbBottom)
                {
                    BringThisToFront();
                    Location = new Point(loc.X, loc.Y);

                    Size = size;
                    _size = size;
                    _thumbLoc = loc;
                    _trackPos = si.nTrackPos;

                    _xyThumbTop = sbi.xyThumbTop;
                    _xyThumbBottom = sbi.xyThumbBottom;

                    Refresh();
                }

                // Only refresh when we need to
                if (oldVisible != Visible && _owner.VisualScrollBarCorner != null &&
                    _owner.VerticalVisualScrollBar != null &&
                    _owner.HorizontalVisualScrollBar != null)
                {
                    //Trace.WriteLine(_random.Next() + " Setting corner visible");

                    _owner.VisualScrollBarCorner.Visible = true;

                    _owner.VisualScrollBarCorner.Location = new Point(
                        _owner.VerticalVisualScrollBar.Left,
                        _owner.HorizontalVisualScrollBar.Top
                    );

                    var parentControls = _owner.ClosestAddableParent.Controls;
                    if (parentControls.GetChildIndex(_owner.VisualScrollBarCorner) != parentControls.GetChildIndex(this) - 1)
                    {
                        //Trace.WriteLine(_random.Next() + " Refreshing corner");

                        parentControls.SetChildIndex(
                            _owner.VisualScrollBarCorner,
                            (parentControls.GetChildIndex(this) - 1).Clamp(0, parentControls.Count - 1));

                        Refresh();
                    }
                }
            }
        }

        //private Random _random = new Random();

        #endregion

        #region Event overrides

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            PaintArrows(g);

            #region Thumb

            if (_owner.IsHandleCreated)
            {
                var sbi = GetCurrentScrollBarInfo();

                if (_leftButtonPressedOnThumb)
                {
                    var si = GetScrollInfo(Native.SIF_TRACKPOS | Native.SIF_PAGE | Native.SIF_RANGE);

                    int thumbTop = si.nTrackPos + (_isVertical
                        ? SystemInformation.VerticalScrollBarArrowHeight
                        : SystemInformation.HorizontalScrollBarArrowWidth);

                    int thumbLength = sbi.xyThumbBottom - sbi.xyThumbTop;

                    int scrollMargin = _isVertical
                        ? SystemInformation.VerticalScrollBarArrowHeight
                        : SystemInformation.HorizontalScrollBarArrowWidth;

                    int thisExtent = _isVertical ? Height : Width;

                    // Important that we use this formula (nMax - max(nPage -1, 0)) or else our position is always
                    // infuriatingly not-quite-right.
                    double percentAlong = Global.GetPercentFromValue(thumbTop - scrollMargin, (int)(si.nMax - Math.Max(si.nPage - 1, 0)) - 0);
                    int thumbTopPixels = Global.GetValueFromPercent(percentAlong, thisExtent - (scrollMargin * 2) - (thumbLength));

                    var rect = _isVertical
                        ? new Rectangle(1, thumbTopPixels + scrollMargin, Width - 2, thumbLength)
                        : new Rectangle(thumbTopPixels + scrollMargin, 1, thumbLength, Height - 2);

                    g.FillRectangle(_thumbPressedBrush, rect);
                }
                else
                {
                    g.FillRectangle(CurrentThumbBrush, GetVisualThumbRect(ref sbi));
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
                if (_WM_ClientToNonClient.ContainsKey(_m.Msg))
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

                    Native.PostMessage(_owner.Handle, _WM_ClientToNonClient[_m.Msg], (IntPtr)wParam, points);
                }
                else if (_m.Msg == Native.WM_MOUSEWHEEL || _m.Msg == Native.WM_MOUSEHWHEEL)
                {
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

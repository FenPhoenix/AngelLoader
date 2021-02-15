using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using DarkUI.Win32;
using Gma.System.MouseKeyHook;

namespace DarkUI.Controls
{
    public sealed class ScrollBarVisualOnly_Native : Control
    {
        #region Enums

        private enum State
        {
            Normal,
            Hot,
            Pressed
        }

        #endregion

        #region Private fields

        private readonly IDarkableScrollableNative _owner;

        private readonly bool _isVertical;

        #region Stored state

        private int? _xyThumbTop;
        private int? _xyThumbBottom;

        // MouseMove doesn't send the buttons, despite having a Button field. The field is just always empty.
        // So we just store the values here.
        private bool _leftButtonPressedOnThumb;
        private bool _leftButtonPressedOnFirstArrow;
        private bool _leftButtonPressedOnSecondArrow;

        private State _thumbState;
        private State _firstArrowState;
        private State _secondArrowState;

        #endregion

        #region Disposables

        private readonly Timer _timer = new Timer();

        private readonly Pen _greySelectionPen = new Pen(Config.Colors.GreySelection);

        private readonly SolidBrush _thumbNormalBrush;
        private readonly SolidBrush _thumbHotBrush;
        private readonly SolidBrush _thumbPressedBrush;

        // We want them separate, not all pointing to the same reference
        private readonly Bitmap _upArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _upArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        private readonly Bitmap _upArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        private readonly Bitmap _downArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _downArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        private readonly Bitmap _downArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        private readonly Bitmap _leftArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _leftArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        private readonly Bitmap _leftArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        private readonly Bitmap _rightArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        private readonly Bitmap _rightArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        private readonly Bitmap _rightArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        #endregion

        #endregion

        #region Constructor / init

        private bool _addedToControls;

        public ScrollBarVisualOnly_Native(IDarkableScrollableNative owner, bool isVertical)
        {
            #region Set up self

            Visible = false;
            DoubleBuffered = true;
            ResizeRedraw = true;

            //BackColor = Config.Colors.Fen_DarkBackground;
            //BackColor = Color.FromArgb(44, 44, 44);
            BackColor = Config.Colors.DarkBackground;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.CacheText,
                true);

            #endregion

            #region Setup involving owner

            _owner = owner;

            _isVertical = isVertical;

            //_owner.AddToControls(this);
            BringThisToFront();
            //_owner.Paint += (sender, e) => PaintDarkScrollBars(_owner, e);

            //_owner.VisibleChanged += (sender, e) => { if (_owner.Visible) BringThisToFront(); };
            //_owner.Scroll += (sender, e) => { if (_owner.Visible || Visible) RefreshIfNeeded(); };

            _owner.VScroll += (sender, e) =>
            {
                if (_owner.VScrollVisible || _owner.HScrollVisible || Visible) RefreshIfNeeded();
            };

            _owner.DarkModeChanged += (sender, e) =>
            {
                if (_owner.IsHandleCreated && _owner.Parent != null)
                {
                    Visible = e.DarkModeEnabled;
                }
                RefreshIfNeeded();
            };

            #endregion

            #region Set up refresh timer

            _timer.Interval = 1;
            _timer.Tick += (sender, e) =>
            {
                RefreshIfNeeded();
            };
            _timer.Enabled = true;

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

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                const int WS_EX_NOACTIVATE = 0x8000000;
                // This doesn't seem to change anything one way or the other, but meh
                cp.ExStyle |= WS_EX_NOACTIVATE;
                return cp;
            }
        }

        #endregion

        #region Private methods

        private void BringThisToFront()
        {
            if (!_addedToControls && _owner.Parent != null)
            {
                _owner.Parent.Controls.Add(this);
                _addedToControls = true;
            }
            if (_addedToControls) BringToFront();
        }

        private Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            var sbi = new Native.SCROLLBARINFO();
            sbi.cbSize = Marshal.SizeOf(sbi);

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollBarInfo(_owner.Handle, _isVertical ? Native.OBJID_VSCROLL : Native.OBJID_HSCROLL, ref sbi);
            }

            return sbi;
        }

        private int GetTrackingThumbTopPosition()
        {
            var si = new Native.SCROLLINFO();
            si.cbSize = Marshal.SizeOf(si);
            si.fMask = Native.SIF_TRACKPOS;

            if (_owner.IsHandleCreated)
            {
                Native.GetScrollInfo(_owner.Handle, _isVertical ? Native.SB_VERT : Native.SB_HORZ, ref si);
            }

            return si.nTrackPos +
                   (_isVertical
                   ? SystemInformation.VerticalScrollBarArrowHeight
                   : SystemInformation.HorizontalScrollBarArrowWidth);
        }

        private Rectangle GetThumbRect(ref Native.SCROLLBARINFO sbi)
        {
            return _isVertical
                ? new Rectangle(1, sbi.xyThumbTop, Width - 2, sbi.xyThumbBottom - sbi.xyThumbTop)
                : new Rectangle(sbi.xyThumbTop, 1, sbi.xyThumbBottom - sbi.xyThumbTop, Height - 2);
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

        private static bool ChangeStateAndAskIfRefreshRequired(ref State state1, State state2)
        {
            if (state1 == state2) return false;

            state1 = state2;
            return true;
        }

        private bool _parentBarVisible;

        private bool _clientSizeHookedUp;

        private void RefreshIfNeeded()
        {
            if (_owner.Parent == null) return;
            if (!_owner.IsHandleCreated) return;

            if (!_clientSizeHookedUp)
            {
                _owner.ClientSizeChanged += (sender, e) =>
                {
                    RefreshIfNeeded();
                    _clientSizeHookedUp = true;
                };
            }

            if (!_owner.DarkModeEnabled)
            {
                Visible = false;
                return;
            }

            // Refresh only if our thumb's size/position is stale. Otherwise, we get unacceptable lag.
            var sbi = GetCurrentScrollBarInfo();

            bool oldParentBarVisible = _parentBarVisible;
            _parentBarVisible = (sbi.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) != Native.STATE_SYSTEM_INVISIBLE;

            //Trace.WriteLine(_rnd.Next() + " " + nameof(_parentBarVisible) + "=" + _parentBarVisible);

            if (oldParentBarVisible != _parentBarVisible)
            {
                if (_parentBarVisible)
                {
                    BringThisToFront();

                    var topLeft = _owner.Parent.PointToClient(new Point(sbi.rcScrollBar.left, sbi.rcScrollBar.top));
                    var bottomRight = _owner.Parent.PointToClient(new Point(sbi.rcScrollBar.right, sbi.rcScrollBar.bottom));

                    var loc = new Rectangle(
                        topLeft.X,
                        topLeft.Y,
                        bottomRight.X - topLeft.X,
                        bottomRight.Y - topLeft.Y
                    );

                    Location = new Point(loc.X, loc.Y);
                    Size = new Size(loc.Width, loc.Height);
                    // TODO: @DarkMode: Support right-to-left modes(?)
                    Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

                    //Trace.WriteLine("RefreshIfNeeded() Visible=true");
                    Visible = true;

                    //if (realScrollBar.Visible != visualScrollBar.Visible)
                    //{
                    //    visualScrollBar.Visible = realScrollBar.Visible;
                    //}
                }
                else
                {
                    //Trace.WriteLine("RefreshIfNeeded() Visible=false");
                    Visible = false;
                }

                //Refresh();
            }
            else if (_xyThumbTop == null)
            {
                _xyThumbTop = sbi.xyThumbTop;
                _xyThumbBottom = sbi.xyThumbBottom;
                Refresh();
            }
            else
            {
                if (_leftButtonPressedOnThumb ||
                    sbi.xyThumbTop != _xyThumbTop || sbi.xyThumbBottom != _xyThumbBottom)
                {
                    Refresh();
                }
            }
        }

        /*
        private static void PaintDarkScrollBars(IDarkableScrollableNative control)
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

                //if (control.VerticalScrollBar.Visible && control.HorizontalScrollBar.Visible)
                //{
                //    // Draw the corner in between the two scroll bars
                //    // TODO: @DarkMode: Also cache this brush
                //    using (var b = new SolidBrush(control.VerticalVisualScrollBar.BackColor))
                //    {
                //        e.Graphics.FillRectangle(b, new Rectangle(
                //            control.VerticalScrollBar.Location.X,
                //            control.HorizontalScrollBar.Location.Y,
                //            control.VerticalScrollBar.Width,
                //            control.HorizontalScrollBar.Height));
                //    }
                //}
            }
            else
            {
                control.VerticalVisualScrollBar.Hide();
                control.HorizontalVisualScrollBar.Hide();
            }
        }
        */

        #endregion

        #region Mouse hook handlers

        private void MouseDownExt_Handler(object sender, MouseEventExtArgs e)
        {
            if (!Visible || !Enabled) return;

            if (e.Button != MouseButtons.Left) return;

            Point cursorPos = PointToClient(e.Location);

            var sbi = GetCurrentScrollBarInfo();
            var thumbRect = GetThumbRect(ref sbi);

            if (thumbRect.Contains(cursorPos))
            {
                _leftButtonPressedOnThumb = true;
                if (ChangeStateAndAskIfRefreshRequired(ref _thumbState, State.Pressed))
                {
                    Refresh();
                }
            }
            else if (GetArrowRect().Contains(cursorPos))
            {
                _leftButtonPressedOnFirstArrow = true;
                if (ChangeStateAndAskIfRefreshRequired(ref _firstArrowState, State.Pressed))
                {
                    Refresh();
                }
            }
            else if (GetArrowRect(second: true).Contains(cursorPos))
            {
                _leftButtonPressedOnSecondArrow = true;
                if (ChangeStateAndAskIfRefreshRequired(ref _secondArrowState, State.Pressed))
                {
                    Refresh();
                }
            }
        }

        private void MouseUpExt_Handler(object sender, MouseEventExtArgs e)
        {
            if (!Visible || !Enabled) return;

            if (e.Button != MouseButtons.Left) return;

            bool refresh = false;

            var sbi = GetCurrentScrollBarInfo();
            var thumbRect = GetThumbRect(ref sbi);
            var firstArrowRect = GetArrowRect();
            var secondArrowRect = GetArrowRect(second: true);

            Point cursorPos = PointToClient(e.Location);

            if (thumbRect.Contains(cursorPos))
            {
                _leftButtonPressedOnThumb = false;
                _leftButtonPressedOnFirstArrow = false;
                _leftButtonPressedOnSecondArrow = false;

                if (ChangeStateAndAskIfRefreshRequired(ref _thumbState, State.Hot))
                {
                    refresh = true;
                }
                if (ChangeStateAndAskIfRefreshRequired(ref _firstArrowState, State.Normal))
                {
                    refresh = true;
                }
                if (ChangeStateAndAskIfRefreshRequired(ref _secondArrowState, State.Normal))
                {
                    refresh = true;
                }
            }
            else
            {
                if (ChangeStateAndAskIfRefreshRequired(ref _thumbState, State.Normal))
                {
                    refresh = true;
                }

                if (_leftButtonPressedOnFirstArrow || _leftButtonPressedOnThumb)
                {
                    _leftButtonPressedOnFirstArrow = false;

                    _firstArrowState = firstArrowRect.Contains(cursorPos) ? State.Hot : State.Normal;
                    refresh = true;
                }
                if (_leftButtonPressedOnSecondArrow || _leftButtonPressedOnThumb)
                {
                    _leftButtonPressedOnSecondArrow = false;
                    _secondArrowState = secondArrowRect.Contains(cursorPos) ? State.Hot : State.Normal;
                    refresh = true;
                }

                _leftButtonPressedOnThumb = false;
            }

            if (refresh) Refresh();
        }

        private void MouseMoveExt_Handler(object sender, MouseEventExtArgs e)
        {
            if (!Visible || !Enabled) return;

            var sbi = GetCurrentScrollBarInfo();
            var thumbRect = GetThumbRect(ref sbi);
            var leftArrowRect = GetArrowRect();
            var rightArrowRect = GetArrowRect(second: true);

            Point cursorPos = PointToClient(e.Location);

            bool cursorOverThumb = thumbRect.Contains(cursorPos);

            var cursorOverFirstArrow = leftArrowRect.Contains(cursorPos);
            var cursorOverSecondArrow = rightArrowRect.Contains(cursorPos);

            bool refresh = false;

            if (cursorOverThumb)
            {
                State stateToChangeTo = _leftButtonPressedOnThumb ? State.Pressed : State.Hot;
                if (ChangeStateAndAskIfRefreshRequired(ref _thumbState, stateToChangeTo))
                {
                    refresh = true;
                }
                if (ChangeStateAndAskIfRefreshRequired(ref _firstArrowState, State.Normal))
                {
                    refresh = true;
                }
                if (ChangeStateAndAskIfRefreshRequired(ref _secondArrowState, State.Normal))
                {
                    refresh = true;
                }
            }
            else
            {
                if (!_leftButtonPressedOnThumb)
                {
                    if (ChangeStateAndAskIfRefreshRequired(ref _thumbState, State.Normal))
                    {
                        refresh = true;
                    }
                }

                if (!cursorOverFirstArrow)
                {
                    if (ChangeStateAndAskIfRefreshRequired(ref _firstArrowState, State.Normal))
                    {
                        refresh = true;
                    }
                }

                if (!cursorOverSecondArrow)
                {
                    if (ChangeStateAndAskIfRefreshRequired(ref _secondArrowState, State.Normal))
                    {
                        refresh = true;
                    }
                }

                if (!_leftButtonPressedOnThumb)
                {
                    if (cursorOverFirstArrow)
                    {
                        var firstArrowState = _leftButtonPressedOnFirstArrow ? State.Pressed : State.Hot;
                        if (ChangeStateAndAskIfRefreshRequired(ref _firstArrowState, firstArrowState))
                        {
                            refresh = true;
                        }
                    }
                    else if (cursorOverSecondArrow)
                    {
                        var secondArrowState = _leftButtonPressedOnSecondArrow ? State.Pressed : State.Hot;
                        if (ChangeStateAndAskIfRefreshRequired(ref _secondArrowState, secondArrowState))
                        {
                            refresh = true;
                        }
                    }
                }
            }

            if (refresh) Refresh();
        }

        #endregion


        public static IntPtr FromLowHigh(int low, int high) => (IntPtr)ToInt(low, high);

        //public static unsafe IntPtr FromLowHighUnsigned(int low, int high)
        //    // Convert the int to an uint before converting it to a pointer type,
        //    // which ensures the high dword being zero for 64-bit pointers.
        //    // This corresponds to the logic of the MAKELPARAM/MAKEWPARAM/MAKELRESULT
        //    // macros.
        //    // TODO: Use nint (with 'unchecked') instead of void* when it is available.
        //    => (IntPtr)(void*)unchecked((uint)ToInt(low, high));

        public static int ToInt(int low, int high) => (high << 16) | (low & 0xffff);

        public static int SignedHIWORD(int n) => (int)(short)HIWORD(n);

        public static int SignedLOWORD(int n) => (int)(short)LOWORD(n);

        public static int SignedHIWORD(IntPtr n) => SignedHIWORD(unchecked((int)(long)n));

        public static int SignedLOWORD(IntPtr n) => SignedLOWORD(unchecked((int)(long)n));

        public static int HIWORD(int n) => (n >> 16) & 0xffff;

        public static int LOWORD(int n) => n & 0xffff;

        public static int LOWORD(IntPtr n) => LOWORD(unchecked((int)(long)n));

        public static int HIWORD(IntPtr n) => HIWORD(unchecked((int)(long)n));

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        private readonly Random _rnd = new Random();

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

                if (_leftButtonPressedOnThumb)
                {
                    var si = new Native.SCROLLINFO();
                    si.cbSize = Marshal.SizeOf(si);
                    si.fMask = Native.SIF_TRACKPOS | Native.SIF_PAGE | Native.SIF_RANGE;

                    Native.GetScrollInfo(_owner.Handle, _isVertical ? Native.SB_VERT : Native.SB_HORZ, ref si);

                    int thumbTop = si.nTrackPos + (_isVertical
                        ? SystemInformation.VerticalScrollBarArrowHeight
                        : SystemInformation.HorizontalScrollBarArrowWidth);

                    int thumbLength = sbi.xyThumbBottom - sbi.xyThumbTop;

                    int scrollMargin = _isVertical
                        ? SystemInformation.VerticalScrollBarArrowHeight
                        : SystemInformation.HorizontalScrollBarArrowWidth;

                    int thisExtent = _isVertical ? Height : Width;

                    double percentAlong = GetPercentFromValue(thumbTop - scrollMargin, (int)(si.nMax - Math.Max(si.nPage - 1, 0)) - 0);

                    int thumbTopPixels = GetValueFromPercent(percentAlong, thisExtent - (scrollMargin * 2) - (thumbLength));

                    var rect = _isVertical
                        ? new Rectangle(1, thumbTopPixels + scrollMargin, Width - 2, thumbLength)
                        : new Rectangle(thumbTopPixels + scrollMargin, 1, thumbLength, Height - 2);

                    g.FillRectangle(_thumbPressedBrush, rect);
                }
                else
                {
                    SolidBrush thumbBrush = _thumbState == State.Normal
                        ? _thumbNormalBrush
                        : _thumbState == State.Hot
                            ? _thumbHotBrush
                            : _thumbPressedBrush;

                    g.FillRectangle(thumbBrush, GetThumbRect(ref sbi));
                }
            }

            #endregion

            base.OnPaint(e);
        }

        private static double GetPercentFromValue(int current, int total) => (double)(100 * current) / total;
        private static int GetValueFromPercent(double percent, int total) => (int)((percent / 100) * total);

        //private void ScrollHereMenuItem_Click(object sender, EventArgs e)
        //{
        //    var sbi = GetCurrentScrollBarInfo();
        //    int thumbSize = sbi.xyThumbBottom - sbi.xyThumbTop;

        //    int arrowMargin = _isVertical
        //        ? SystemInformation.VerticalScrollBarArrowHeight
        //        : SystemInformation.HorizontalScrollBarArrowWidth;

        //    var rect = _isVertical
        //        ? new Rectangle(0, arrowMargin, Width, (Height - (arrowMargin * 2)) - thumbSize)
        //        : new Rectangle(arrowMargin, 0, (Width - (arrowMargin * 2)) - thumbSize, Height);

        //    int posAlong = _isVertical ? _storedCursorPosition.Y : +_storedCursorPosition.X;

        //    posAlong -= arrowMargin;

        //    posAlong -= thumbSize / 2;

        //    double posPercent = GetPercentFromValue(posAlong, _isVertical ? rect.Height : rect.Width).Clamp(0, 100);
        //    // Important that we use this formula (nMax - max(nPage -1, 0)) or else our position is always
        //    // infuriatingly not-quite-right.
        //    int finalValue = GetValueFromPercent(posPercent, _owner.Maximum - Math.Max(_owner.LargeChange - 1, 0));

        //    SetOwnerValue(finalValue);
        //}

        protected override void OnVisibleChanged(EventArgs e)
        {
            _timer.Enabled = Visible;
            base.OnVisibleChanged(e);
        }

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

        protected override void WndProc(ref Message m)
        {
            void SendToOwner(ref Message _m)
            {
                if (!_owner.IsHandleCreated) return;
                if (!_WM_ClientToNonClient.ContainsKey(_m.Msg)) return;

                int wParam;
                int x = SignedLOWORD(_m.LParam);
                int y = SignedHIWORD(_m.LParam);
                Point ownerScreenLoc = _owner.PointToScreen(_owner.Location);

                if (_isVertical)
                {
                    int sbWidthOrHeight = SystemInformation.VerticalScrollBarWidth;
                    x += (ownerScreenLoc.X + (_owner.Size.Width - sbWidthOrHeight)) - _owner.Parent.Padding.Left;
                    y += ownerScreenLoc.Y - _owner.Parent.Padding.Top;
                    wParam = Native.HTVSCROLL;
                }
                else
                {
                    int sbWidthOrHeight = SystemInformation.HorizontalScrollBarHeight;
                    x += ownerScreenLoc.X - _owner.Parent.Padding.Left;
                    y += (ownerScreenLoc.Y + (_owner.Size.Height - sbWidthOrHeight)) - _owner.Parent.Padding.Top;
                    wParam = Native.HTHSCROLL;
                }

                Native.POINTS points = new Native.POINTS((short)x, (short)y);

                Native.PostMessage(_owner.Handle, _WM_ClientToNonClient[_m.Msg], (IntPtr)wParam, points);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Dispose();

                Global.MouseHook.MouseDownExt -= MouseDownExt_Handler;
                Global.MouseHook.MouseUpExt -= MouseUpExt_Handler;
                Global.MouseHook.MouseMoveExt -= MouseMoveExt_Handler;

                _greySelectionPen.Dispose();

                _thumbNormalBrush.Dispose();
                _thumbHotBrush.Dispose();
                _thumbPressedBrush.Dispose();

                _upArrowNormal.Dispose();
                _upArrowHot.Dispose();
                _upArrowPressed.Dispose();

                _downArrowNormal.Dispose();
                _downArrowHot.Dispose();
                _downArrowPressed.Dispose();

                _leftArrowNormal.Dispose();
                _leftArrowHot.Dispose();
                _leftArrowPressed.Dispose();

                _rightArrowNormal.Dispose();
                _rightArrowHot.Dispose();
                _rightArrowPressed.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

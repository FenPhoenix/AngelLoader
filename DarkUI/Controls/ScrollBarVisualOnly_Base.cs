using System;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Win32;
using Gma.System.MouseKeyHook;

namespace DarkUI.Controls
{
    public class ScrollBarVisualOnly_Base : Control
    {
        // TODO: @DarkMode(Scroll bars):
        // See if we can recycle the structs we pass to the P/Invokes (GC avoidance on a huge number of calls)

        #region Enums

        private protected enum State
        {
            Normal,
            Hot,
            Pressed
        }

        #endregion

        #region  Private / protected fields

        private protected readonly bool _isVertical;
        private readonly bool _passMouseWheel;

        #endregion

        #region Stored state

        private protected int? _xyThumbTop;
        private protected int? _xyThumbBottom;

        // MouseMove doesn't send the buttons, despite having a Button field. The field is just always empty.
        // So we just store the values here.
        private protected bool _leftButtonPressedOnThumb;
        private bool _leftButtonPressedOnFirstArrow;
        private bool _leftButtonPressedOnSecondArrow;

        private protected State _thumbState;
        private protected State _firstArrowState;
        private protected State _secondArrowState;

        protected SolidBrush CurrentThumbBrush => _thumbState == State.Normal
            ? _thumbNormalBrush
            : _thumbState == State.Hot
            ? _thumbHotBrush
            : _thumbPressedBrush;

        #endregion

        #region Disposables

        private readonly Timer _timer = new Timer();

        private readonly Pen _greySelectionPen = new Pen(Config.Colors.GreySelection);

        private SolidBrush _thumbNormalBrush;
        private SolidBrush _thumbHotBrush;
        protected SolidBrush _thumbPressedBrush;

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

        #region Constructor / init

        private protected ScrollBarVisualOnly_Base(bool isVertical, bool passMouseWheel)
        {
            _isVertical = isVertical;
            _passMouseWheel = passMouseWheel;
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

        private protected void SetUpSelf()
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
        }

        private protected void SetUpAfterOwner()
        {
            #region Set up refresh timer

            if ((this is ScrollBarVisualOnly))
            {
                // TODO: @DarkMode(ScrollBarVisualOnly):
                // See if we can hook up any event at all that will let us get rid of this timer for non-native.
                _timer.Interval = 1;
                _timer.Tick += (sender, e) => RefreshIfNeeded();
            }
            else
            {
                // @DarkMode(ScrollBarVisualOnly_Native):
                // Timer not used at the moment as it's not apparently needed anymore.
                _timer.Enabled = false;
            }

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

        #region Methods

        private protected virtual void RefreshIfNeeded(bool forceRefreshCorner = false) { }

        private protected virtual Native.SCROLLBARINFO GetCurrentScrollBarInfo() => throw new NotImplementedException();

        private protected Rectangle GetVisualThumbRect(ref Native.SCROLLBARINFO sbi)
        {
            return _isVertical
                ? new Rectangle(1, sbi.xyThumbTop, Width - 2, sbi.xyThumbBottom - sbi.xyThumbTop)
                : new Rectangle(sbi.xyThumbTop, 1, sbi.xyThumbBottom - sbi.xyThumbTop, Height - 2);
        }

        private Rectangle GetThumbRect(ref Native.SCROLLBARINFO sbi)
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

        private protected static bool ChangeStateAndAskIfRefreshRequired(ref State state1, State state2)
        {
            if (state1 == state2) return false;

            state1 = state2;
            return true;
        }

        private protected bool ShouldSendToOwner(int msg)
        {
            return
                (msg == Native.WM_LBUTTONDOWN || msg == Native.WM_NCLBUTTONDOWN
                || msg == Native.WM_LBUTTONUP || msg == Native.WM_NCLBUTTONUP
                || msg == Native.WM_LBUTTONDBLCLK || msg == Native.WM_NCLBUTTONDBLCLK

                || msg == Native.WM_MBUTTONDOWN || msg == Native.WM_NCMBUTTONDOWN
                || msg == Native.WM_MBUTTONUP || msg == Native.WM_NCMBUTTONUP
                || msg == Native.WM_MBUTTONDBLCLK || msg == Native.WM_NCMBUTTONDBLCLK

                || msg == Native.WM_RBUTTONDOWN || msg == Native.WM_NCRBUTTONDOWN
                || msg == Native.WM_RBUTTONUP || msg == Native.WM_NCRBUTTONUP
                || msg == Native.WM_RBUTTONDBLCLK || msg == Native.WM_NCRBUTTONDBLCLK

                //|| Msg == Native.WM_MOUSEMOVE || Msg == Native.WM_NCMOUSEMOVE

                // TODO: @DarkMode: Test wheel tilt with this system!
                // (do I still have that spare Logitech mouse that works?)
                || (_passMouseWheel && (msg == Native.WM_MOUSEWHEEL || msg == Native.WM_MOUSEHWHEEL))
                );
        }

        private protected void PaintArrows(Graphics g)
        {
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
        }

        #endregion

        #region Event overrides

        protected override void OnVisibleChanged(EventArgs e)
        {
            if (this is ScrollBarVisualOnly)
            {
                _timer.Enabled = Visible;
            }
            base.OnVisibleChanged(e);
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

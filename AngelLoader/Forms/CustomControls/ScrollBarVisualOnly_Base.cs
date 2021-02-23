using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.WinAPI;
using Gma.System.MouseKeyHook;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
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

        private readonly Point[] _arrowPolygon = new Point[3];

        private protected SolidBrush CurrentThumbBrush => _thumbState switch
        {
            State.Normal => DarkColors.GreySelectionBrush,
            State.Hot => DarkColors.GreyHighlightBrush,
            _ => DarkColors.ActiveControlBrush
        };

        private static SolidBrush GetStateBrush(State state) => state switch
        {
            State.Normal => DarkColors.GreySelectionBrush,
            State.Hot => DarkColors.GreyHighlightBrush,
            _ => DarkColors.ActiveControlBrush
        };

        #endregion
        
        #region Constructor / init

        private protected ScrollBarVisualOnly_Base(bool isVertical, bool passMouseWheel)
        {
            _isVertical = isVertical;
            _passMouseWheel = passMouseWheel;

            #region Set up self

            Visible = false;
            base.DoubleBuffered = true;
            ResizeRedraw = true;

            base.BackColor = DarkColors.DarkBackground;

            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.CacheText,
                true);

            #endregion

            #region Set up mouse hook

            ControlUtils.MouseHook ??= Hook.AppEvents();

            ControlUtils.MouseHook.MouseDownExt += MouseDownExt_Handler;
            ControlUtils.MouseHook.MouseUpExt += MouseUpExt_Handler;
            ControlUtils.MouseHook.MouseMoveExt += MouseMoveExt_Handler;

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

        #region Mouse hook handlers

        private void MouseDownExt_Handler(object sender, MouseEventExtArgs e)
        {
            if (!Visible || !Enabled) return;

            if (e.Button != MouseButtons.Left) return;

            Point cursorPos = PointToClient(e.Location);

            Rectangle thumbRect = GetThumbRect();

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

            Rectangle thumbRect = GetThumbRect();
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

            Rectangle thumbRect = GetThumbRect();
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

        private protected virtual Native.SCROLLBARINFO GetCurrentScrollBarInfo() => throw new NotImplementedException();

        private protected Rectangle GetVisualThumbRect(ref Native.SCROLLBARINFO sbi)
        {
            int thumbLength = sbi.xyThumbBottom - sbi.xyThumbTop;
            return _isVertical
                ? new Rectangle(1, sbi.xyThumbTop, Width - 2, thumbLength)
                : new Rectangle(sbi.xyThumbTop, 1, thumbLength, Height - 2);
        }

        private Rectangle GetThumbRect()
        {
            if (_xyThumbBottom != null && _xyThumbTop != null)
            {
                int thumbLength = (int)_xyThumbBottom - (int)_xyThumbTop;
                return _isVertical
                    ? new Rectangle(0, (int)_xyThumbTop, Width, thumbLength)
                    : new Rectangle((int)_xyThumbTop, 0, thumbLength, Height);
            }
            else
            {
                var sbi = GetCurrentScrollBarInfo();
                int thumbLength = sbi.xyThumbBottom - sbi.xyThumbTop;
                return _isVertical
                    ? new Rectangle(0, sbi.xyThumbTop, Width, thumbLength)
                    : new Rectangle(sbi.xyThumbTop, 0, thumbLength, Height);
            }
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

        private protected (int MinThumbLengthPX, int ArrowMarginPX, int ExtentPX, int InnerExtentPX)
        GetMeasurements()
        {
            int minThumbLengthPX;
            int scrollMarginPX;
            int thisExtentPX;
            if (_isVertical)
            {
                minThumbLengthPX = SystemInformation.VerticalScrollBarThumbHeight;
                scrollMarginPX = SystemInformation.VerticalScrollBarArrowHeight;
                thisExtentPX = Height;
            }
            else
            {
                minThumbLengthPX = SystemInformation.HorizontalScrollBarThumbWidth;
                scrollMarginPX = SystemInformation.HorizontalScrollBarArrowWidth;
                thisExtentPX = Width;
            }
            int innerExtentPX = thisExtentPX - (scrollMarginPX * 2);

            return (minThumbLengthPX, scrollMarginPX, thisExtentPX, innerExtentPX);
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
                   msg == Native.WM_LBUTTONDOWN || msg == Native.WM_NCLBUTTONDOWN
                || msg == Native.WM_LBUTTONUP || msg == Native.WM_NCLBUTTONUP
                || msg == Native.WM_LBUTTONDBLCLK || msg == Native.WM_NCLBUTTONDBLCLK

                || msg == Native.WM_MBUTTONDOWN || msg == Native.WM_NCMBUTTONDOWN
                || msg == Native.WM_MBUTTONUP || msg == Native.WM_NCMBUTTONUP
                || msg == Native.WM_MBUTTONDBLCLK || msg == Native.WM_NCMBUTTONDBLCLK

                || msg == Native.WM_RBUTTONDOWN || msg == Native.WM_NCRBUTTONDOWN
                || msg == Native.WM_RBUTTONUP || msg == Native.WM_NCRBUTTONUP
                || msg == Native.WM_RBUTTONDBLCLK || msg == Native.WM_NCRBUTTONDBLCLK

                //|| msg == Native.WM_MOUSEMOVE || msg == Native.WM_NCMOUSEMOVE
                || msg == Native.WM_MOUSELEAVE || msg == Native.WM_NCMOUSELEAVE

                // TODO: @DarkMode: Test wheel tilt with this system!
                // (do I still have that spare Logitech mouse that works?)
                || (_passMouseWheel && (msg == Native.WM_MOUSEWHEEL || msg == Native.WM_MOUSEHWHEEL));
        }

        private protected virtual bool GetOwnerEnabled() => throw new NotImplementedException();

        private protected void PaintArrows(Graphics g, bool enabled)
        {
            int w, h, xOffset, yOffset;
            Direction firstDirection, secondDirection;
            if (_isVertical)
            {
                w = SystemInformation.VerticalScrollBarWidth;
                h = SystemInformation.VerticalScrollBarArrowHeight;
                xOffset = 0;
                yOffset = Height - h;
                firstDirection = Direction.Up;
                secondDirection = Direction.Down;
            }
            else
            {
                w = SystemInformation.HorizontalScrollBarHeight;
                h = SystemInformation.HorizontalScrollBarArrowWidth;
                xOffset = Width - w;
                yOffset = 0;
                firstDirection = Direction.Left;
                secondDirection = Direction.Right;
            }

            var firstArrowBrush = GetStateBrush(_firstArrowState);
            var secondArrowBrush = GetStateBrush(_secondArrowState);

            ControlPainter.PaintArrow(
                g,
                _arrowPolygon,
                firstDirection,
                w,
                h,
                GetOwnerEnabled(),
                brush: firstArrowBrush);

            ControlPainter.PaintArrow(
                g,
                _arrowPolygon,
                secondDirection,
                w,
                h,
                GetOwnerEnabled(),
                brush: secondArrowBrush,
                xOffset: xOffset,
                yOffset: yOffset);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ControlUtils.MouseHook != null)
                {
                    ControlUtils.MouseHook.MouseDownExt -= MouseDownExt_Handler;
                    ControlUtils.MouseHook.MouseUpExt -= MouseUpExt_Handler;
                    ControlUtils.MouseHook.MouseMoveExt -= MouseMoveExt_Handler;
                }
            }

            base.Dispose(disposing);
        }
    }
}

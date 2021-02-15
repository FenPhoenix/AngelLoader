using System;
using System.Drawing;
using System.Windows.Forms;
using DarkUI.Win32;
using Gma.System.MouseKeyHook;

namespace DarkUI.Controls
{
    public class ScrollBarVisualOnly_Base : Control
    {
        #region Enums

        protected enum State
        {
            Normal,
            Hot,
            Pressed
        }

        #endregion

        protected readonly bool _isVertical;

        #region Stored state

        protected int? _xyThumbTop;
        protected int? _xyThumbBottom;

        // MouseMove doesn't send the buttons, despite having a Button field. The field is just always empty.
        // So we just store the values here.
        protected bool _leftButtonPressedOnThumb;
        protected bool _leftButtonPressedOnFirstArrow;
        protected bool _leftButtonPressedOnSecondArrow;

        protected State _thumbState;
        protected State _firstArrowState;
        protected State _secondArrowState;

        #endregion

        #region Disposables

        protected readonly Timer _timer = new Timer();

        protected readonly Pen _greySelectionPen = new Pen(Config.Colors.GreySelection);

        protected SolidBrush _thumbNormalBrush;
        protected SolidBrush _thumbHotBrush;
        protected SolidBrush _thumbPressedBrush;

        // We want them separate, not all pointing to the same reference
        protected readonly Bitmap _upArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        protected readonly Bitmap _upArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        protected readonly Bitmap _upArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        protected readonly Bitmap _downArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        protected readonly Bitmap _downArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        protected readonly Bitmap _downArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        protected readonly Bitmap _leftArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        protected readonly Bitmap _leftArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        protected readonly Bitmap _leftArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        protected readonly Bitmap _rightArrowNormal = ScrollIcons.scrollbar_arrow_small_standard;
        protected readonly Bitmap _rightArrowHot = ScrollIcons.scrollbar_arrow_small_hot;
        protected readonly Bitmap _rightArrowPressed = ScrollIcons.scrollbar_arrow_small_clicked;

        #endregion

        protected ScrollBarVisualOnly_Base(bool isVertical)
        {
            _isVertical = isVertical;
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

        internal virtual Native.SCROLLBARINFO GetCurrentScrollBarInfo()
        {
            throw new NotImplementedException();
        }

        #region Mouse hook handlers

        protected void MouseDownExt_Handler(object sender, MouseEventExtArgs e)
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

        protected void MouseUpExt_Handler(object sender, MouseEventExtArgs e)
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

        protected void MouseMoveExt_Handler(object sender, MouseEventExtArgs e)
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

        internal Rectangle GetVisualThumbRect(ref Native.SCROLLBARINFO sbi)
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

        protected static bool ChangeStateAndAskIfRefreshRequired(ref State state1, State state2)
        {
            if (state1 == state2) return false;

            state1 = state2;
            return true;
        }

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

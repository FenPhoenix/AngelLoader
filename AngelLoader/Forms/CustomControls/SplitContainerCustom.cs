using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class SplitContainerCustom : SplitContainer, IDarkable
    {
        // Insightful Note:
        // Horizontal and Vertical are the opposite of what you expect them to mean...
        // "Horizontal" means vertically stacked and you resize it in a vertical direction. The split line
        // itself is horizontal, but that's not exactly a compelling argument for calling it "horizontal".
        // Oh well.

        #region Private fields

        private int _storedCollapsiblePanelMinSize;
        private float _storedSplitterPercent;
        private float SplitterPercent
        {
            get => SplitterDistance / (float)(IsMain() ? Height : Width);
            set => SplitterDistance = (int)Math.Round(value * (IsMain() ? Height : Width));
        }

        // This is so you can drag both directions by grabbing the corner between the two. One SplitContainer can
        // control both its own SplitterDistance and that of its orthogonally-oriented sibling at the same time.
        private SplitContainerCustom? _sibling;

        // This realtime-draw resize stuff still flickers a bit, but it's better than no redraw at all.
        private int _originalDistance;
        private bool _mouseOverCrossSection;
        [Browsable(false)]
        private bool MouseOverCrossSection => _sibling != null && _mouseOverCrossSection;

        private Color? _origBackColor;
        private Color? _origPanel1BackColor;
        private Color? _origPanel2BackColor;

        #endregion

        private bool _darkModeEnabled;

        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetUpTheme();
            }
        }

        private void SetUpTheme()
        {
            if (_darkModeEnabled)
            {
                _origBackColor ??= BackColor;
                _origPanel1BackColor ??= Panel1.BackColor;
                _origPanel2BackColor ??= Panel2.BackColor;

                BackColor = DarkModeColors.GreySelection;
                Panel1.BackColor = Panel1DarkBackColor;
                Panel2.BackColor = Panel2DarkBackColor;
            }
            else
            {
                if (_origBackColor != null) BackColor = (Color)_origBackColor;
                if (_origPanel1BackColor != null) Panel1.BackColor = (Color)_origPanel1BackColor;
                if (_origPanel2BackColor != null) Panel2.BackColor = (Color)_origPanel2BackColor;
            }
        }

        #region Public fields

        internal float SplitterPercentReal => FullScreen ? _storedSplitterPercent : SplitterPercent;

        internal bool FullScreen { get; private set; }
        internal int CollapsedSize = 0;

        internal Color Panel1DarkBackColor { get; set; } = DarkModeColors.Fen_ControlBackground;
        internal Color Panel2DarkBackColor { get; set; } = DarkModeColors.Fen_ControlBackground;

        #endregion

        // @R#_FALSE_POSITIVE note (SplitContainerCustom):
        // The Pure attribute is used so that calls to this method don't cause ReSharper to invalidate null checks
        // and assume the checked-for members might have been set null again by this method.
        // 2020-07-12:
        // I think what's happening is that when fields are non-readonly because they have to be initialized
        // somewhere other than the constructor (because of lazy-loading), then when R# sees that a method is
        // called between their initialization and them being accessed, it can't know for sure that that method
        // doesn't change the member back to null (halting problem?).
        // But, the null warning goes away if you use a property rather than a method call, even though they're
        // the same under the hood. R# might be scanning properties because it assumes they won't do anything
        // heavy, but doesn't make that assumption about methods?
        // (The field in question here is the nullable, non-readonly sibling SplitContainer)
        [Pure]
        private bool IsMain() => Orientation == Orientation.Horizontal;

        public SplitContainerCustom()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            _storedCollapsiblePanelMinSize = IsMain() ? Panel1MinSize : Panel2MinSize;
        }

        #region Public methods

        internal void InjectSibling(SplitContainerCustom sibling) => _sibling = sibling;

        internal void ToggleFullScreen() => SetFullScreen(!FullScreen);

        internal void SetFullScreen(bool enabled, bool suspendResume = true)
        {
            if (IsMain())
            {
                if (enabled)
                {
                    if (suspendResume) this.SuspendDrawing();
                    IsSplitterFixed = true;
                    _storedCollapsiblePanelMinSize = Panel1MinSize;
                    _storedSplitterPercent = SplitterPercent;
                    Panel1MinSize = CollapsedSize;
                    _sibling!.Hide();
                    SplitterDistance = CollapsedSize;
                    FullScreen = true;
                    if (suspendResume) this.ResumeDrawing();
                }
                else
                {
                    if (suspendResume) this.SuspendDrawing();
                    SplitterPercent = _storedSplitterPercent;
                    Panel1MinSize = _storedCollapsiblePanelMinSize;
                    _sibling!.Show();
                    FullScreen = false;
                    IsSplitterFixed = false;
                    if (suspendResume) this.ResumeDrawing();
                }
            }
            else
            {
                if (enabled)
                {
                    if (suspendResume) this.SuspendDrawing();
                    IsSplitterFixed = true;
                    _storedCollapsiblePanelMinSize = Panel2MinSize;
                    _storedSplitterPercent = SplitterPercent;
                    Panel2MinSize = CollapsedSize;
                    SplitterDistance = Width - CollapsedSize;
                    FullScreen = true;
                    // Colossal hack (hiding dark tab control prevents white line on side)
                    foreach (Control control in Panel2.Controls)
                    {
                        if (control is DarkTabControl) control.Hide();
                    }
                    if (suspendResume) this.ResumeDrawing();
                }
                else
                {
                    if (suspendResume) this.SuspendDrawing();
                    SplitterPercent = _storedSplitterPercent;
                    Panel2MinSize = _storedCollapsiblePanelMinSize;
                    FullScreen = false;
                    IsSplitterFixed = false;
                    // Colossal hack (hiding dark tab control prevents white line on side)
                    foreach (Control control in Panel2.Controls)
                    {
                        if (control is DarkTabControl) control.Show();
                    }
                    if (suspendResume) this.ResumeDrawing();
                }
            }
        }

        internal void SetSplitterPercent(float percent, bool suspendResume = true)
        {
            if (FullScreen && !IsMain())
            {
                // Don't un-collapse top-right panel
                _storedSplitterPercent = percent;
                return;
            }

            try
            {
                if (suspendResume) this.SuspendDrawing();
                SplitterPercent = percent;
            }
            catch
            {
                // Leave it at the default
            }
            finally
            {
                if (suspendResume) this.ResumeDrawing();
            }
        }

        internal void ResetSplitterPercent()
        {
            if (!IsMain() && FullScreen)
            {
                _storedSplitterPercent = Defaults.TopSplitterPercent;
            }
            else
            {
                SplitterPercent = IsMain() ? Defaults.MainSplitterPercent : Defaults.TopSplitterPercent;
            }
        }

        internal void CancelResize()
        {
            if (!IsSplitterFixed || FullScreen) return;

            IsSplitterFixed = false;
            SplitterDistance = _originalDistance;
            if (MouseOverCrossSection) _sibling!.SplitterDistance = _sibling._originalDistance;
            _mouseOverCrossSection = false;
        }

        #endregion

        #region Event overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (FullScreen) return;

            if (e.Button == MouseButtons.Left &&
                (Cursor.Current == Cursors.SizeAll ||
                 (IsMain() && Cursor.Current == Cursors.HSplit) ||
                 (!IsMain() && Cursor.Current == Cursors.VSplit)))
            {
                _originalDistance = SplitterDistance;
                if (MouseOverCrossSection) _sibling!._originalDistance = _sibling.SplitterDistance;
                IsSplitterFixed = true;
            }
            else
            {
                if (IsSplitterFixed) CancelResize();
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (FullScreen) return;

            _mouseOverCrossSection = false;
            IsSplitterFixed = false;

            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (FullScreen) return;

            if (!IsSplitterFixed && _sibling != null)
            {
                int sibCursorPos = IsMain()
                    ? _sibling.Panel1.PointToClient(new Point(Cursor.Position.X, 0)).X
                    : _sibling.Panel1.PointToClient(new Point(0, Cursor.Position.Y)).Y;

                int sibSplitterPos = IsMain()
                    ? _sibling.Panel1.Width
                    : _sibling.Panel1.Height;

                // Don't do the both-directional-drag if the top-right panel is collapsed
                if (!_sibling.FullScreen &&
                    sibCursorPos >= sibSplitterPos - 7 &&
                    sibCursorPos <= sibSplitterPos + _sibling.SplitterWidth + 6)
                {
                    Cursor.Current = Cursors.SizeAll;
                    _mouseOverCrossSection = true;
                }
                else
                {
                    _mouseOverCrossSection = false;
                }
            }
            if (IsSplitterFixed)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (MouseOverCrossSection) Cursor.Current = Cursors.SizeAll;

                    // SuspendDrawing() / ResumeDrawing() reduces visual artifacts
                    int axis = IsMain() ? e.Y : e.X;

                    // Things need to happen in different orders depending on who we are, in order to avoid
                    // flickering. We could also Suspend/Resume them one at a time, but that's perceptibly
                    // laggier.
                    if (MouseOverCrossSection)
                    {
                        if (IsMain())
                        {
                            _sibling!.SuspendDrawing();
                            this.SuspendDrawing();
                        }
                        else
                        {
                            this.SuspendDrawing();
                            _sibling!.SuspendDrawing();
                        }

                        _sibling!.SplitterDistance = (axis == e.X ? e.Y : e.X).ClampToZero();
                    }
                    else
                    {
                        this.SuspendDrawing();
                    }

                    SplitterDistance = axis.ClampToZero();

                    if (MouseOverCrossSection)
                    {
                        if (IsMain())
                        {
                            _sibling!.ResumeDrawing();
                            this.ResumeDrawing();
                        }
                        else
                        {
                            this.ResumeDrawing();
                            _sibling!.ResumeDrawing();
                        }
                    }
                    else
                    {
                        this.ResumeDrawing();
                    }
                }
                else
                {
                    IsSplitterFixed = false;
                }
            }

            base.OnMouseMove(e);
        }

        #endregion
    }
}

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Common.Utility;

namespace AngelLoader.CustomControls
{
    public sealed class SplitContainerCustom : SplitContainer
    {
        // Insightful Note:
        // Horizontal and Vertical are the opposite of what you expect them to mean...
        // "Horizontal" means vertically stacked and you resize it in a vertical direction. The split line
        // itself is horizontal, but that's not exactly a compelling argument for calling it "horizontal".
        // Oh well.

        private int _storedCollapsiblePanelMinSize;
        private int _storedSplitterDistance;
        internal bool FullScreen { get; private set; }
        internal int CollapsedSize = 0;

        internal int SplitterDistanceReal => FullScreen ? _storedSplitterDistance : SplitterDistance;

        internal void ToggleFullScreen() => SetFullScreen(!FullScreen);

        internal void SetFullScreen(bool enabled, bool suspendResume = true)
        {
            if (Orientation == Orientation.Vertical)
            {
                if (enabled)
                {
                    if (suspendResume) this.SuspendDrawing();
                    IsSplitterFixed = true;
                    _storedCollapsiblePanelMinSize = Panel2MinSize;
                    _storedSplitterDistance = SplitterDistance;
                    Panel2MinSize = CollapsedSize;
                    SplitterDistance = Width - CollapsedSize;
                    FullScreen = true;
                    if (suspendResume) this.ResumeDrawing();
                }
                else
                {
                    if (suspendResume) this.SuspendDrawing();
                    SplitterDistance = _storedSplitterDistance;
                    Panel2MinSize = _storedCollapsiblePanelMinSize;
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
                    _storedCollapsiblePanelMinSize = Panel1MinSize;
                    _storedSplitterDistance = SplitterDistance;
                    Panel1MinSize = CollapsedSize;
                    Sibling.Hide();
                    SplitterDistance = CollapsedSize;
                    FullScreen = true;
                    if (suspendResume) this.ResumeDrawing();
                }
                else
                {
                    if (suspendResume) this.SuspendDrawing();
                    SplitterDistance = _storedSplitterDistance;
                    Panel1MinSize = _storedCollapsiblePanelMinSize;
                    Sibling.Show();
                    FullScreen = false;
                    IsSplitterFixed = false;
                    if (suspendResume) this.ResumeDrawing();
                }
            }
        }

        internal float SplitterDistancePercent = -1;

        // This realtime-draw resize stuff still flickers a bit, but it's better than no redraw at all.
        public int OriginalDistance;

        private bool _mouseOverCrossSection;
        public bool MouseOverCrossSection
        {
            get => Sibling != null && _mouseOverCrossSection;
            set => _mouseOverCrossSection = value;
        }

        public SplitContainerCustom()
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;
            _storedCollapsiblePanelMinSize = Panel1MinSize;
        }

        // This is so you can drag both directions by grabbing the corner between the two. One SplitContainer can
        // control both its own SplitterDistance and that of its orthogonally-oriented sibling at the same time.
        private SplitContainerCustom Sibling;
        public void InjectSibling(SplitContainerCustom sibling) => Sibling = sibling;

        /// <summary>
        /// If <paramref name="distance"/> is valid, sets the splitter distance. Otherwise, leaves it alone.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="refresh"></param>
        public void SetSplitterDistance(int distance, bool refresh = true)
        {
            if (FullScreen && Orientation == Orientation.Vertical)
            {
                // Don't un-collapse top-right panel
                _storedSplitterDistance = distance;
                return;
            }

            try
            {
                if (refresh) this.SuspendDrawing();
                SplitterDistance = distance;
            }
            catch (Exception)
            {
                // Leave it at the default
            }
            finally
            {
                if (refresh) this.ResumeDrawing();
            }
        }

        public void ResetSplitterDistance()
        {
            var percent = SplitterDistancePercent;

            Debug.Assert(percent > -1, "percent is -1 (default value)");

            var dist = Orientation == Orientation.Horizontal
                ? (int)Math.Round((percent / 100) * Height)
                : (int)Math.Round((percent / 100) * Width);

            SetSplitterDistance(dist);
        }

        public void CancelResize()
        {
            if (!IsSplitterFixed || (Orientation == Orientation.Vertical && FullScreen))
            {
                return;
            }

            IsSplitterFixed = false;
            SplitterDistance = OriginalDistance;
            if (MouseOverCrossSection) Sibling.SplitterDistance = Sibling.OriginalDistance;
            _mouseOverCrossSection = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (FullScreen) return;

            if (e.Button == MouseButtons.Left &&
                (Cursor.Current == Cursors.SizeAll ||
                 (Orientation == Orientation.Horizontal && Cursor.Current == Cursors.HSplit) ||
                 (Orientation == Orientation.Vertical && Cursor.Current == Cursors.VSplit)))
            {
                OriginalDistance = SplitterDistance;
                if (MouseOverCrossSection) Sibling.OriginalDistance = Sibling.SplitterDistance;
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

            if (!IsSplitterFixed && Sibling != null)
            {
                int sibCursorPos = Orientation == Orientation.Horizontal
                    ? Sibling.Panel1.PointToClient(new Point(Cursor.Position.X, 0)).X
                    : Sibling.Panel1.PointToClient(new Point(0, Cursor.Position.Y)).Y;

                int sibSplitterPos = Orientation == Orientation.Horizontal
                    ? Sibling.Panel1.Width
                    : Sibling.Panel1.Height;

                // Don't do the both-directional-drag if the top-right panel is collapsed
                if (!Sibling.FullScreen &&
                    sibCursorPos >= sibSplitterPos - 7 &&
                    sibCursorPos <= sibSplitterPos + Sibling.SplitterWidth + 6)
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
                    var axis = Orientation == Orientation.Vertical ? e.X : e.Y;

                    // Things need to happen in different orders depending on who we are, in order to avoid
                    // flickering. We could also Suspend/Resume them one at a time, but that's perceptibly
                    // laggier.
                    if (MouseOverCrossSection)
                    {
                        if (Orientation == Orientation.Horizontal)
                        {
                            Sibling.SuspendDrawing();
                            this.SuspendDrawing();
                        }
                        else
                        {
                            this.SuspendDrawing();
                            Sibling.SuspendDrawing();
                        }

                        Sibling.SplitterDistance = (axis == e.X ? e.Y : e.X).ClampToZero();
                    }
                    else
                    {
                        this.SuspendDrawing();
                    }

                    SplitterDistance = axis.ClampToZero();

                    if (MouseOverCrossSection)
                    {
                        if (Orientation == Orientation.Horizontal)
                        {
                            Sibling.ResumeDrawing();
                            this.ResumeDrawing();
                        }
                        else
                        {
                            this.ResumeDrawing();
                            Sibling.ResumeDrawing();
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
    }
}

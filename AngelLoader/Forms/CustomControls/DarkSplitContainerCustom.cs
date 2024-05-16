using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkSplitContainerCustom : SplitContainer, IDarkable
{
    // Insightful Note:
    // Horizontal and Vertical are the opposite of what you expect them to mean...
    // "Horizontal" means vertically stacked and you resize it in a vertical direction. The split line
    // itself is horizontal, but that's not exactly a compelling argument for calling it "horizontal".
    // Oh well.

    #region Private fields

    private sealed class SiblingData
    {
        internal DarkSplitContainerCustom? SplitContainer;
        private bool _mouseOverCrossSection;

        [MemberNotNullWhen(true, nameof(SplitContainer))]
        internal bool MouseOverCrossSection
        {
            get => SplitContainer != null && _mouseOverCrossSection;
            set => _mouseOverCrossSection = value;
        }
    }

    private bool IsStacked => Orientation == Orientation.Horizontal;
    private int CrossLength => IsStacked ? Height : Width;

    private int _storedCollapsiblePanelMinSize;
    private float _storedSplitterPercent;
    private float SplitterPercent
    {
        get => SplitterDistance / (float)CrossLength;
        set => SplitterDistance = (int)Math.Round(value * CrossLength);
    }

    // This is so you can drag both directions by grabbing the corner between the two. One SplitContainer can
    // control both its own SplitterDistance and that of its orthogonally-oriented sibling at the same time.
    private readonly SiblingData _sibling1 = new();
    private readonly SiblingData _sibling2 = new();

    // This realtime-draw resize stuff still flickers a bit, but it's better than no redraw at all.
    private int _originalDistance;
    private Color? _origBackColor;
    private Color? _origPanel1BackColor;
    private Color? _origPanel2BackColor;

    private bool _resizing;
    /// <summary>
    /// True if the user is in the middle of dragging the splitter.
    /// </summary>
    internal bool Resizing
    {
        get => _resizing;
        private set
        {
            IsSplitterFixed = value;
            _resizing = value;
        }
    }

    #endregion

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        get => _darkModeEnabled;
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            if (_darkModeEnabled)
            {
                _origBackColor ??= BackColor;
                _origPanel1BackColor ??= Panel1.BackColor;
                _origPanel2BackColor ??= Panel2.BackColor;

                BackColor = DarkColors.GreySelection;
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
    }

    #region Public fields

    internal float SplitterPercentReal => FullScreen ? _storedSplitterPercent : SplitterPercent;

    internal int SplitterDistanceLogical =>
        FullScreen
            ? (int)Math.Round(_storedSplitterPercent * CrossLength)
            : SplitterDistance;

    internal bool FullScreen { get; private set; }
    internal int CollapsedSize;

    internal Color Panel1DarkBackColor = DarkColors.Fen_ControlBackground;
    internal Color Panel2DarkBackColor = DarkColors.Fen_ControlBackground;

    #endregion

    [PublicAPI]
    public enum Panel
    {
        Panel1,
        Panel2,
    }

    [Browsable(true)]
    [DefaultValue(Panel.Panel1)]
    [PublicAPI]
    public Panel FullScreenCollapsePanel { get; set; }

    [Browsable(true)]
    [DefaultValue(false)]
    [PublicAPI]
    public bool RefreshSiblingFirst { get; set; }

    public DarkSplitContainerCustom()
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        DoubleBuffered = true;
    }

    public event EventHandler? FullScreenBeforeChanged;
    public event EventHandler? FullScreenChanged;

    #region Public methods

    internal void SetSibling1(DarkSplitContainerCustom sibling) => _sibling1.SplitContainer = sibling;
    internal void SetSibling2(DarkSplitContainerCustom sibling) => _sibling2.SplitContainer = sibling;

    internal void ToggleFullScreen() => SetFullScreen(!FullScreen);

    internal void SetFullScreen(bool enabled, bool suspendResume = true)
    {
        try
        {
            if (suspendResume) this.SuspendDrawing();

            FullScreenBeforeChanged?.Invoke(this, EventArgs.Empty);

            bool isPanel1 = FullScreenCollapsePanel == Panel.Panel1;

            if (enabled)
            {
                IsSplitterFixed = true;
                _storedCollapsiblePanelMinSize = isPanel1 ? Panel1MinSize : Panel2MinSize;
                _storedSplitterPercent = SplitterPercent;
                SetPanelMinSize(this, FullScreenCollapsePanel, CollapsedSize);
                SplitterDistance = isPanel1 ? CollapsedSize : CrossLength - CollapsedSize;
                FullScreen = true;
            }
            else
            {
                SplitterPercent = _storedSplitterPercent;
                SetPanelMinSize(this, FullScreenCollapsePanel, _storedCollapsiblePanelMinSize);
                FullScreen = false;
                IsSplitterFixed = false;
            }
        }
        finally
        {
            FullScreenChanged?.Invoke(this, EventArgs.Empty);
            if (suspendResume) this.ResumeDrawing();
        }

        return;

        static void SetPanelMinSize(DarkSplitContainerCustom @this, Panel panel, int size)
        {
            if (panel == Panel.Panel1)
            {
                @this.Panel1MinSize = size;
            }
            else
            {
                @this.Panel2MinSize = size;
            }
        }
    }

    internal void SetSplitterPercent(float percent, bool setIfFullScreen, bool suspendResume = true)
    {
        if (!setIfFullScreen && FullScreen)
        {
            _storedSplitterPercent = percent;
        }
        else
        {
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
    }

    internal void CancelResize()
    {
        if (!Resizing || FullScreen) return;

        Resizing = false;
        SplitterDistance = _originalDistance;
        if (_sibling1.MouseOverCrossSection)
        {
            _sibling1.SplitContainer.SplitterDistance = _sibling1.SplitContainer._originalDistance;
        }
        if (_sibling2.MouseOverCrossSection)
        {
            _sibling2.SplitContainer.SplitterDistance = _sibling2.SplitContainer._originalDistance;
        }
        _sibling1.MouseOverCrossSection = false;
        _sibling2.MouseOverCrossSection = false;
    }

    #endregion

    #region Event overrides

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        DoSizeFix();
    }

    private bool _parentShownOnce;
    protected override void OnParentVisibleChanged(EventArgs e)
    {
        base.OnParentVisibleChanged(e);

        if (Parent?.Visible == true && !_parentShownOnce)
        {
            DoSizeFix();
            _parentShownOnce = true;
        }
    }

    private void DoSizeFix()
    {
        if (FullScreen)
        {
            SplitterDistance = FullScreenCollapsePanel == Panel.Panel1 ? CollapsedSize : CrossLength - CollapsedSize;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (DesignMode)
        {
            base.OnMouseDown(e);
            return;
        }

        if (FullScreen) return;

        if (e.Button == MouseButtons.Left &&
            (Cursor.Current == Cursors.SizeAll ||
             (IsStacked && Cursor.Current == Cursors.HSplit) ||
             (!IsStacked && Cursor.Current == Cursors.VSplit)))
        {
            _originalDistance = SplitterDistance;
            if (_sibling1.MouseOverCrossSection)
            {
                _sibling1.SplitContainer._originalDistance = _sibling1.SplitContainer.SplitterDistance;
            }
            if (_sibling2.MouseOverCrossSection)
            {
                _sibling2.SplitContainer._originalDistance = _sibling2.SplitContainer.SplitterDistance;
            }
            Resizing = true;
        }
        else
        {
            CancelResize();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (DesignMode)
        {
            base.OnMouseUp(e);
            return;
        }

        if (FullScreen) return;

        _sibling1.MouseOverCrossSection = false;
        _sibling2.MouseOverCrossSection = false;
        Resizing = false;

        base.OnMouseUp(e);
    }

    private void SetMouseOverCrossSection(SiblingData sibling)
    {
        if (!Resizing && sibling.SplitContainer != null)
        {
            int sibCursorPos = IsStacked
                ? sibling.SplitContainer.Panel1.ClientCursorPos().X
                : sibling.SplitContainer.Panel1.ClientCursorPos().Y;

            int sibSplitterPos = IsStacked
                ? sibling.SplitContainer.Panel1.Width
                : sibling.SplitContainer.Panel1.Height;

            // Don't do the both-directional-drag if the sibling has a panel collapsed
            if (!sibling.SplitContainer.FullScreen &&
                sibCursorPos >= sibSplitterPos - 7 &&
                sibCursorPos <= sibSplitterPos + sibling.SplitContainer.SplitterWidth + 6)
            {
                Cursor.Current = Cursors.SizeAll;
                sibling.MouseOverCrossSection = true;
            }
            else
            {
                sibling.MouseOverCrossSection = false;
            }
        }
    }

    private bool SuspendDrawingAndMoveSiblingSplitters(SiblingData sibling1, SiblingData sibling2, int axis, int eX)
    {
        // Things need to happen in different orders depending on who we are, in order to avoid
        // flickering. We could also Suspend/Resume them one at a time, but that's perceptibly
        // laggier.
        if (sibling1.MouseOverCrossSection)
        {
            if (RefreshSiblingFirst)
            {
                sibling1.SplitContainer.SuspendDrawing();
                if (sibling2.MouseOverCrossSection) sibling2.SplitContainer.SuspendDrawing();
                this.SuspendDrawing();
            }
            else
            {
                this.SuspendDrawing();
                if (sibling2.MouseOverCrossSection) sibling2.SplitContainer.SuspendDrawing();
                sibling1.SplitContainer.SuspendDrawing();
            }

            Point pt1 = sibling1.SplitContainer.ClientCursorPos();
            sibling1.SplitContainer.SplitterDistance = (axis == eX ? pt1.Y : pt1.X).ClampToZero();
            if (sibling2.MouseOverCrossSection)
            {
                Point pt2 = sibling2.SplitContainer.ClientCursorPos();
                sibling2.SplitContainer.SplitterDistance = (axis == eX ? pt2.Y : pt2.X).ClampToZero();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    private bool ResumeDrawing(SiblingData sibling1, SiblingData sibling2)
    {
        if (sibling1.MouseOverCrossSection)
        {
            if (RefreshSiblingFirst)
            {
                if (sibling2.MouseOverCrossSection) sibling2.SplitContainer.ResumeDrawing();
                sibling1.SplitContainer.ResumeDrawing();
                this.ResumeDrawing();
            }
            else
            {
                this.ResumeDrawing();
                sibling1.SplitContainer.ResumeDrawing();
                if (sibling2.MouseOverCrossSection) sibling2.SplitContainer.ResumeDrawing();
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (DesignMode)
        {
            base.OnMouseMove(e);
            return;
        }

        if (FullScreen) return;

        SetMouseOverCrossSection(_sibling1);
        SetMouseOverCrossSection(_sibling2);

        if (Resizing)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_sibling1.MouseOverCrossSection || _sibling2.MouseOverCrossSection)
                {
                    Cursor.Current = Cursors.SizeAll;
                }

                // SuspendDrawing() / ResumeDrawing() reduces visual artifacts
                int axis = IsStacked ? e.Y : e.X;

                if (!SuspendDrawingAndMoveSiblingSplitters(_sibling1, _sibling2, axis, e.X) &&
                    !SuspendDrawingAndMoveSiblingSplitters(_sibling2, _sibling1, axis, e.X))
                {
                    this.SuspendDrawing();
                }

                SplitterDistance = axis.ClampToZero();

                if (!ResumeDrawing(_sibling1, _sibling2) &&
                    !ResumeDrawing(_sibling2, _sibling1))
                {
                    this.ResumeDrawing();
                }
            }
            else
            {
                Resizing = false;
            }
        }

        base.OnMouseMove(e);
    }

    #endregion
}

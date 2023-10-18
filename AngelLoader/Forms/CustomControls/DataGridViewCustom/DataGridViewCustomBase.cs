using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls;

public partial class DataGridViewCustomBase : DataGridView, IDarkable
{
    private bool _mouseHere;
    private int _mouseDownOnHeader = -1;

    private protected MainForm _owner = null!;

    private protected readonly Color DefaultRowBackColor = SystemColors.Window;

    internal bool SuppressSelectionEvent;

    public DataGridViewCustomBase() => base.DoubleBuffered = true;

    #region Sort

    internal Column CurrentSortedColumn;
    internal SortDirection CurrentSortDirection = SortDirection.Ascending;

    #endregion

    private protected bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            if (_darkModeEnabled)
            {
                BackgroundColor = DarkColors.Fen_DarkBackground;
                RowsDefaultCellStyle.ForeColor = DarkColors.Fen_DarkForeground;
                RowsDefaultCellStyle.BackColor = DarkColors.Fen_DarkBackground;
            }
            else
            {
                BackgroundColor = SystemColors.ControlDark;
                RowsDefaultCellStyle.ForeColor = SystemColors.ControlText;
                RowsDefaultCellStyle.BackColor = SystemColors.Window;
            }
        }
    }

    internal void SetOwner(MainForm owner) => _owner = owner;

    /// <summary>
    /// Returns true if any row is selected, false if no rows exist or none are selected.
    /// </summary>
    /// <returns></returns>
    internal bool RowSelected() => GetRowSelectedCount() > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int GetRowSelectedCount()
    {
        // Since we're full-row select, one row will be ColumnsCount cells selected. Avoids having to make a
        // heavy SelectedRows call.
        return GetCellCount(DataGridViewElementStates.Selected) / ColumnCount;
    }

    internal bool MultipleFMsSelected() => GetRowSelectedCount() > 1;

    internal void SelectSingle(int index, bool suppressSelectionChangedEvent = false)
    {
        try
        {
            if (suppressSelectionChangedEvent) SuppressSelectionEvent = true;

            // Stops the no-FM-selected code from being run (would clear the top-right area etc.) causing flicker.
            // Because clearing the selection is just some stupid crap we have to do to make one be selected,
            // so it shouldn't count as actually having none selected.
            using (new DisableZeroSelectCode(_owner))
            {
                ClearSelection();
                Rows[index].Selected = true;
            }
        }
        finally
        {
            if (suppressSelectionChangedEvent) SuppressSelectionEvent = false;
        }
    }


    /// <summary>
    /// If you don't have an actual cell selected (indicated by its header being blue) and you try to move
    /// with the keyboard, it pops back to the top item. This fixes that, and is called wherever appropriate.
    /// </summary>
    internal void SelectProperly(bool suspendResume = true)
    {
        if (Rows.Count == 0 || Columns.Count == 0 || !RowSelected())
        {
            return;
        }

        // Crappy mitigation for losing horizontal scroll position, not perfect but better than nothing
        int origHSO = HorizontalScrollingOffset;

        try
        {
            // Note: we need to do this null check here, otherwise we get an exception that doesn't get caught(!!!)
            SelectedRows[0].Cells[FirstDisplayedCell?.ColumnIndex ?? 0].Selected = true;
        }
        catch
        {
            // It can't be selected for whatever reason. Oh well.
        }

        try
        {
            if (suspendResume) this.SuspendDrawing();
            if (HorizontalScrollBar.Visible && HorizontalScrollingOffset != origHSO)
            {
                HorizontalScrollingOffset = origHSO;
            }
        }
        catch
        {
            // ignore
        }
        finally
        {
            if (suspendResume) this.ResumeDrawing();
        }
    }

    internal void SendKeyDown(KeyEventArgs e) => OnKeyDown(e);

    public event EventHandler? MainSelectedRowChanged;

    private void SetMainSelectedRow()
    {
        if (GetRowSelectedCount() == 0)
        {
            MainSelectedRow = null;
        }
        else
        {
            if (MainSelectedRow == null || GetRowSelectedCount() == 1)
            {
                // Need to make a SelectedRows call here, can't set CurrentRow for example because that ends
                // up causing bad behavior when right-clicking to open the menu.
                // In theory this should only ever be 1 long anyway.
                MainSelectedRow = SelectedRows[0];
            }
            MainSelectedRowChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal void SetRowSelected(int index, bool selected, bool suppressEvent)
    {
        try
        {
            if (suppressEvent) SuppressSelectionEvent = true;
            Rows[index].Selected = selected;
        }
        finally
        {
            if (suppressEvent) SuppressSelectionEvent = false;
        }
    }

    protected override void OnSelectionChanged(EventArgs e)
    {
        if (!_owner.AboutToClose)
        {
            _selectedRowsCached = null;
            if (!SuppressSelectionEvent) SetMainSelectedRow();
        }
        base.OnSelectionChanged(e);
    }

    protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
    {
        _selectedRowsCached = null;
        base.OnRowsAdded(e);
    }

    protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
    {
        _selectedRowsCached = null;
        base.OnRowsRemoved(e);
    }

    protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
    {
        // Allows the highlight-removal hack to work. See the custom header cell class for details.
        if (!DesignMode &&
            e.Column.HeaderCell is not DataGridViewColumnHeaderCellCustom)
        {
            e.Column.HeaderCell = new DataGridViewColumnHeaderCellCustom();
        }

        base.OnColumnAdded(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (!_darkModeEnabled) return;

        if (BorderStyle == BorderStyle.FixedSingle)
        {
            e.Graphics.DrawRectangle(DarkColors.GreySelectionPen, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
        }

        if (VerticalScrollBar.Visible && HorizontalScrollBar.Visible)
        {
            int vertScrollBarWidth = SystemInformation.VerticalScrollBarWidth;
            int horzScrollBarHeight = SystemInformation.HorizontalScrollBarHeight;
            e.Graphics.FillRectangle(DarkColors.DarkBackgroundBrush,
                VerticalScrollBar.Left,
                HorizontalScrollBar.Top,
                vertScrollBarWidth,
                horzScrollBarHeight);
        }
    }

    private protected void DrawRows(
        DataGridViewCellPaintingEventArgs e,
        bool isSelected,
        SolidBrush bgBrush,
        Pen borderPen,
        Pen bottomBorderPen)
    {
        #region Paint cell background

        e.Graphics.FillRectangle(bgBrush, e.CellBounds);

        #endregion

        #region Draw content

        e.CellStyle.ForeColor = isSelected
            ? DarkColors.Fen_HighlightText
            : DarkColors.Fen_DarkForeground;

        e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);

        #endregion

        #region Draw cell borders
        // Bottom
        e.Graphics.DrawLine(
            bottomBorderPen,
            e.CellBounds.X,
            e.CellBounds.Y + (e.CellBounds.Height - 1),
            e.CellBounds.X + (e.CellBounds.Width - 2),
            e.CellBounds.Y + (e.CellBounds.Height - 1)
        );

        // Right
        e.Graphics.DrawLine(
            borderPen,
            e.CellBounds.X + (e.CellBounds.Width - 1),
            e.CellBounds.Y,
            e.CellBounds.X + (e.CellBounds.Width - 1),
            e.CellBounds.Y + (e.CellBounds.Height - 1)
        );

        #endregion

        e.Handled = true;
    }

    private protected void DrawColumnHeaders(DataGridViewCellPaintingEventArgs e)
    {
        /*
        @DarkModeNote(DGV headers):
        -Header painting appears to happen in DataGridViewColumnHeaderCell.PaintPrivate() - look here for
        precise text bounds calculations etc.
        */
        int displayIndex = Columns[e.ColumnIndex].DisplayIndex;

        bool mouseOver = e.CellBounds.Contains(this.ClientCursorPos());

        // If we wanted to match classic mode, this is what we would use to start with
        /*
        var selectionRect = new Rectangle(
            e.CellBounds.X + 2,
            e.CellBounds.Y + 2,
            e.CellBounds.Width - 2,
            e.CellBounds.Height - 3);
        */

        // For now, we're just simplifying and not drawing all the fussy borders of the classic mode.
        // This way looks perfectly fine in dark mode and saves work.
        Rectangle selectionRect = e.CellBounds with { Height = e.CellBounds.Height - 1 };

        e.Graphics.FillRectangle(DarkColors.GreyBackgroundBrush, e.CellBounds);

        if (!mouseOver)
        {
            e.Graphics.DrawLine(
                DarkColors.GreySelectionPen,
                e.CellBounds.X + e.CellBounds.Width - 1,
                0,
                e.CellBounds.X + e.CellBounds.Width - 1,
                e.CellBounds.Y + e.CellBounds.Height - 1);
        }
        e.Graphics.DrawLine(
            DarkColors.GreySelectionPen,
            e.CellBounds.X,
            e.CellBounds.Y + e.CellBounds.Height - 1,
            e.CellBounds.X + e.CellBounds.Width - 1,
            e.CellBounds.Y + e.CellBounds.Height - 1);

        if (_mouseHere && mouseOver)
        {
            SolidBrush b = _mouseDownOnHeader == e.ColumnIndex
                ? DarkColors.Fen_DGVColumnHeaderPressedBrush
                : DarkColors.Fen_DGVColumnHeaderHighlightBrush;
            e.Graphics.FillRectangle(b, selectionRect);
        }

        // The classic-themed bounds are complicated and difficult to discern, so we're just using
        // measured constants here, which match classic mode in our particular case.
        Rectangle textRect = e.CellBounds with
        {
            X = e.CellBounds.X + (displayIndex == 0 ? 6 : 4),
            Width = e.CellBounds.Width - (displayIndex == 0 ? 10 : 6)
        };

        if (e.Value is string headerText)
        {
            TextFormatFlags textFormatFlags =
                ControlUtils.GetTextAlignmentFlags(e.CellStyle.Alignment) |
                TextFormatFlags.NoPrefix |
                TextFormatFlags.NoClipping |
                TextFormatFlags.EndEllipsis;

            TextRenderer.DrawText(
                e.Graphics,
                headerText,
                e.CellStyle.Font,
                textRect,
                DarkColors.Fen_DarkForeground,
                textFormatFlags);

            int textLength = TextRenderer.MeasureText(
                e.Graphics,
                headerText,
                e.CellStyle.Font,
                new Size(int.MaxValue, int.MaxValue),
                textFormatFlags
            ).Width;

            if (e.ColumnIndex == (int)CurrentSortedColumn &&
                textLength < e.CellBounds.Width - 24)
            {
                Direction direction = CurrentSortDirection == SortDirection.Ascending
                    ? Direction.Up
                    : Direction.Down;

                Images.PaintArrow9x5(
                    g: e.Graphics,
                    direction: direction,
                    area: new Rectangle(e.CellBounds.Right - 17, 2, 17, e.CellBounds.Height),
                    controlEnabled: Enabled
                );
            }
        }

        e.Handled = true;
    }

    protected override void OnCellMouseDown(DataGridViewCellMouseEventArgs e)
    {
        // Prevent the last selected row from being de-selected - that would put us into an undefined state!
        Keys modifierKeys = ModifierKeys;
        if (modifierKeys is Keys.Control or (Keys.Control | Keys.Shift) &&
            RowCount > 0 &&
            GetRowSelectedCount() == 1 &&
            Rows[e.RowIndex].Selected)
        {
            return;
        }
        base.OnCellMouseDown(e);
    }

    // Stupid bloody SelectedRows rebuilds itself EVERY. TIME. YOU. CALL. IT.
    // So cache the frigging thing so we don't do a full rebuild if we haven't changed.
    private protected DataGridViewSelectedRowCollection? _selectedRowsCached;
    [Browsable(false)]
    public new DataGridViewSelectedRowCollection SelectedRows => _selectedRowsCached ??= base.SelectedRows;

    internal DataGridViewRow? MainSelectedRow;

    // Better, faster (especially with a large selection set) way of doing the hack that prevents the glitched
    // last row on first jump-to-end when the end is scrolled offscreen.
    internal bool RefreshOnScrollHack;
    private bool _fmsListOneTimeHackRefreshDone;
    ///<inheritdoc cref="DataGridView.FirstDisplayedScrollingRowIndex"/>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new int FirstDisplayedScrollingRowIndex
    {
        get => base.FirstDisplayedScrollingRowIndex;
        set
        {
            bool needsResume = false;
            try
            {
                // The hack only works when owner form is visible, and it's only done once, so don't waste it.
                if (!_fmsListOneTimeHackRefreshDone && _owner.Visible)
                {
                    needsResume = true;
                    this.SuspendDrawing();

                    try { base.FirstDisplayedScrollingRowIndex++; } catch {/* ignore */}
                    try { base.FirstDisplayedScrollingRowIndex--; } catch {/* ignore */}
                    _fmsListOneTimeHackRefreshDone = true;
                }
                base.FirstDisplayedScrollingRowIndex = value;
            }
            finally
            {
                // Don't refresh cause we'll refresh automatically as a result of the scrolling, and calling
                // Refresh() just causes visual issues and/or is slow
                // But ALSO still invalidate if not refreshing because otherwise the middle-click centering
                // breaks!
                if (needsResume) this.ResumeDrawing(invalidateInsteadOfRefresh: !RefreshOnScrollHack);
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            HitTestInfo ht = HitTest(e.X, e.Y);
            if (ht.Type == DataGridViewHitTestType.ColumnHeader)
            {
                _mouseDownOnHeader = ht.ColumnIndex;
            }
        }

        if (!StartColumnResize(e)) return;
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) _mouseDownOnHeader = -1;

        if (!EndColumnResize(e)) return;
        base.OnMouseUp(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _mouseHere = true;
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _mouseHere = false;
        base.OnMouseLeave(e);
    }

    protected override void OnDoubleClick(EventArgs e)
    {
        // This particular order of calls allows the double-click-to-autosize feature to work again. I don't
        // know why it does really, but I'm willing to take what works at this point.
        CancelColumnResize();
        base.OnMouseDown((MouseEventArgs)e);
        base.OnDoubleClick(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!DoColumnResize(e)) return;
        base.OnMouseMove(e);
    }

    // Cancel column resize operations if we lose focus. Otherwise, we end up still in drag mode with the
    // mouse up and another window in the foreground, which looks annoying to me, so yeah.
    protected override void OnLeave(EventArgs e)
    {
        CancelColumnResize();
        base.OnLeave(e);
    }

    internal void MakeColumnVisible(DataGridViewColumn column, bool visible)
    {
        column.Visible = visible;
        // Fix for zero-height glitch when Rating column gets swapped out when all columns are hidden
        try
        {
            column.Width++;
            column.Width--;
        }
        // stupid OCD check in case adding 1 would take us over 65536
        catch (ArgumentOutOfRangeException)
        {
            column.Width--;
            column.Width++;
        }
    }
}

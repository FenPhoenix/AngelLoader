using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class DataGridViewCustom : DataGridView, IDarkable
{
    #region Private fields

    private MainForm _owner = null!;

    private readonly Color DefaultRowBackColor = SystemColors.Window;

    private bool _mouseHere;
    private int _mouseDownOnHeader = -1;

    internal bool SuppressSelectionEvent;

    #endregion

    #region Public fields

    #region Sort

    internal Column CurrentSortedColumn;
    internal SortDirection CurrentSortDirection = SortDirection.Ascending;

    #endregion

    internal readonly SelectedFM CurrentSelFM = new();

    internal readonly GameTabsState GameTabsState = new();

    #region Filter

    internal readonly Filter Filter = new();
    internal readonly List<int> FilterShownIndexList = new();

    #endregion

    #endregion

    private bool _darkModeEnabled;
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

    public DataGridViewCustom() => DoubleBuffered = true;

    #region Public methods

    #region Init

    internal void SetOwner(MainForm owner) => _owner = owner;

    #endregion

    #region Get FM / FM data

    /// <summary>
    /// Gets the FM at <paramref name="index"/>, taking the currently set filters into account.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    internal FanMission GetFMFromIndex(int index) => FMsViewList[FilterShownIndexList[index]];

    /// <summary>
    /// Gets the currently selected FM, taking the currently set filters into account.
    /// </summary>
    /// <returns></returns>
    internal FanMission GetMainSelectedFM()
    {
        AssertR(GetRowSelectedCount() > 0, nameof(GetMainSelectedFM) + ": no rows selected!");
        AssertR(MainSelectedRow != null, nameof(MainSelectedRow) + " is null when it shouldn't be");

        return GetFMFromIndex(MainSelectedRow!.Index);
    }

    /// <summary>
    /// Order is not guaranteed. Seems to be in reverse order currently but who knows. Use <see cref="GetSelectedFMs_InOrder"/>
    /// if you need them in visual order.
    /// </summary>
    /// <returns></returns>
    internal FanMission[] GetSelectedFMs()
    {
        var selRows = SelectedRows;
        var ret = new FanMission[selRows.Count];
        for (int i = 0; i < selRows.Count; i++)
        {
            ret[i] = GetFMFromIndex(selRows[i].Index);
        }

        return ret;
    }

    private DataGridViewRow[] GetOrderedRowsArray()
    {
        // Why. Why would Microsoft have all these infuriatingly stupid custom list types.
        // DataGridViewSelectedRowCollection?! Not even a row collection, but a _SELECTED_ row collection.
        // Jesus christ is that really necessary? And it has no sort methods either because it's custom, so
        // it's copy with Cast and then copy with OrderBy and then copy to Array and then copy that to another
        // array of actual FM objects. Hooray. We've got a garbage collector so who cares right?
        return SelectedRows.Cast<DataGridViewRow>().OrderBy(static x => x.Index).ToArray();
    }

    /// <summary>
    /// Use this if you need the FMs in visual order, but take a (probably minor-ish) perf/mem hit.
    /// </summary>
    /// <returns></returns>
    internal FanMission[] GetSelectedFMs_InOrder()
    {
        var selRows = GetOrderedRowsArray();

        var ret = new FanMission[selRows.Length];
        for (int i = 0; i < selRows.Length; i++)
        {
            ret[i] = GetFMFromIndex(selRows[i].Index);
        }

        return ret;
    }

    /// <summary>
    /// Use this if you need the FMs in visual order, but take a (probably minor-ish) perf/mem hit.
    /// </summary>
    /// <returns></returns>
    internal List<FanMission> GetSelectedFMs_InOrder_List()
    {
        DataGridViewRow[] selRows = GetOrderedRowsArray();

        var ret = new List<FanMission>(selRows.Length);
        foreach (DataGridViewRow selRow in selRows)
        {
            ret.Add(GetFMFromIndex(selRow.Index));
        }

        return ret;
    }

    internal int GetIndexFromInstalledName(string installedName, bool findNearest, int defaultValue = 0)
    {
        if (installedName.IsEmpty()) return defaultValue;

        for (int i = 0; i < FilterShownIndexList.Count; i++)
        {
            if (GetFMFromIndex(i).InstalledDir.EqualsI(installedName)) return i;
        }

        if (findNearest)
        {
            for (int i = 0; i < FMsViewList.Count; i++)
            {
                if (FMsViewList[i].InstalledDir.EqualsI(installedName))
                {
                    for (int j = i; j < FMsViewList.Count; j++)
                    {
                        int index = FilterShownIndexList.IndexOf(j);
                        if (index > -1) return index;
                    }
                    for (int j = i; j > 0; j--)
                    {
                        int index = FilterShownIndexList.IndexOf(j);
                        if (index > -1) return index;
                    }
                    break;
                }
            }
        }

        return defaultValue;
    }

    internal SelectedFM GetMainSelectedFMPosInfo() =>
        !RowSelected()
            ? new SelectedFM { InstalledName = "", IndexFromTop = 0 }
            : GetFMPosInfoFromIndex(index: MainSelectedRow!.Index);

    internal SelectedFM GetFMPosInfoFromIndex(int index)
    {
        var ret = new SelectedFM { InstalledName = "", IndexFromTop = 0 };

        // TODO/BUG: GetFMPosInfoFromIndex(): This is a leftover from when this just got the selection, instead of an explicit index.
        // I'm pretty sure this never gets hit in practice, but the Delete code ends up calling this and
        // I haven't checked that code thoroughly. All other callsites appear to not be able to hit this.
        // If we can determine this never gets hit, we should remove it because it's unintended logic (but
        // we just want to know we don't accidentally depend on it).
        // But we should replace it with a RowCount > 0 check, because this also implicitly guards against
        // that, and we do want that guard check.
        if (!RowSelected()) return ret;

        bool readmeMaximized = _owner.MainSplitContainer.FullScreen;

        int firstDisplayed = readmeMaximized
            ? _owner._storedFMsDGVFirstDisplayedScrollingRowIndex
            : FirstDisplayedScrollingRowIndex;
        int lastDisplayed = firstDisplayed +
                            (readmeMaximized
                                ? _owner._storedFMsDGVDisplayedRowCountFalse
                                : DisplayedRowCount(false));

        int indexFromTop = index >= firstDisplayed && index <= lastDisplayed
            ? index - firstDisplayed
            : (readmeMaximized ? _owner._storedFMsDGVDisplayedRowCountTrue : DisplayedRowCount(true)) / 2;

        ret.InstalledName = GetFMFromIndex(index).InstalledDir;
        ret.IndexFromTop = indexFromTop;
        return ret;
    }

    #endregion

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

            // Stops the no-FM-selected code from being run (would clear the FM tabs area(s) etc.) causing flicker.
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

    #region Get and set columns

    internal ColumnDataArray GetColumnData()
    {
        ColumnDataArray columns = new();

        for (int i = 0; i < Columns.Count; i++)
        {
            DataGridViewColumn dgvColumn = Columns[i];
            ColumnData column = columns[i];
            column.Id = (Column)dgvColumn.Index;
            column.DisplayIndex = dgvColumn.DisplayIndex;
            column.Visible = dgvColumn.Visible;
            column.Width = dgvColumn.Width;
        }

        return columns.OrderById();
    }

    internal void SetColumnData(FMsDGV_ColumnHeaderLLMenu menu, ColumnDataArray columnData)
    {
        retry:
        #region Important

        // IMPORTANT (SetColumnData):
        // Do not remove! DataGridViewColumn.DisplayIndex changes the DisplayIndex of other columns when you
        // set it. That means we can't step through columns in order and set their DisplayIndex to whatever
        // it should be; instead we have to step through DisplayIndexes in order and assign them to whatever
        // column they should be assigned to.

        // Wrong:
        // Column[0].DisplayIndex = 4; Column[1].DisplayIndex = 12; etc.

        // Right:
        // Column[10].DisplayIndex = 0; Column[3].DisplayIndex = 1; etc.

        ColumnDataArray columnDataSorted = columnData.OrderByDisplayIndex();

        #endregion

        try
        {
            foreach (ColumnData colData in columnDataSorted)
            {
                DataGridViewColumn col = Columns[(int)colData.Id];

                col.DisplayIndex = colData.DisplayIndex;
                if (col.Resizable == DataGridViewTriState.True) col.Width = colData.Width;
                MakeColumnVisible(col, colData.Visible);

                menu.SetColumnChecked((int)colData.Id, colData.Visible);
            }
        }
        // Last line of defense fallback in case against all odds an invalid state has made it this far
        catch
        {
            ResetColumnDisplayIndexes(columnData);
            goto retry;
        }
    }

    #endregion

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

    internal void SendKeyDown(KeyEventArgs e) => OnKeyDown(e);

    #endregion

    #region Event overrides

    // Stupid bloody SelectedRows rebuilds itself EVERY. TIME. YOU. CALL. IT.
    // So cache the frigging thing so we don't do a full rebuild if we haven't changed.
    private DataGridViewSelectedRowCollection? _selectedRowsCached;
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
            _owner.UpdateUIControlsForMultiSelectState(GetMainSelectedFM());
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

    #region Mouse

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

    #endregion

    #region Paint

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

    protected override void OnRowPrePaint(DataGridViewRowPrePaintEventArgs e)
    {
        base.OnRowPrePaint(e);

        if (_darkModeEnabled) return;

        // Coloring the recent rows here because if we do it in _CellValueNeeded, we get a brief flash of the
        // default white-background cell color before it changes.
        if (!_owner.CellValueNeededDisabled && FilterShownIndexList.Count > 0)
        {
            FanMission fm = GetFMFromIndex(e.RowIndex);

            Rows[e.RowIndex].DefaultCellStyle.BackColor =
                fm.MarkedUnavailable
                    ? DarkColors.DGV_UnavailableColorLight
                    : fm.Pinned
                        ? DarkColors.DGV_PinnedBackgroundLight
                        : fm.MarkedRecent
                            ? DarkColors.DGV_RecentHighlightColorLight
                            : DefaultRowBackColor;
        }
    }

    protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
    {
        base.OnCellPainting(e);

        if (!_darkModeEnabled) return;

        // This is for having different colored grid lines in recent-highlighted rows.
        // That way, we can get a good, visible separator color for all cases by just having two.

        if (e.RowIndex > -1)
        {
            FanMission fm = GetFMFromIndex(e.RowIndex);

            bool isSelected = (e.State & DataGridViewElementStates.Selected) != 0;

            #region Paint cell background

            SolidBrush bgBrush = isSelected
                ? DarkColors.BlueSelectionBrush
                : fm.MarkedUnavailable
                    ? DarkColors.Fen_RedHighlightBrush
                    : fm.Pinned
                        ? DarkColors.DGV_PinnedBackgroundDarkBrush
                        : fm.MarkedRecent
                            ? DarkColors.Fen_DGVCellBordersBrush
                            : DarkColors.Fen_DarkBackgroundBrush;

            e.Graphics.FillRectangle(bgBrush, e.CellBounds);

            #endregion

            #region Draw content

            e.CellStyle.ForeColor = isSelected
                ? DarkColors.Fen_HighlightText
                : DarkColors.Fen_DarkForeground;

            e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);

            #endregion

            #region Draw cell borders

            bool needsDarkBorder = fm.MarkedUnavailable || fm.MarkedRecent;

            Pen borderPen = needsDarkBorder
                ? DarkColors.Fen_DarkBackgroundPen
                : DarkColors.Fen_DGVCellBordersPen;

            FanMission nextFM;
            Pen bottomBorderPen =
                needsDarkBorder &&
                e.RowIndex < RowCount - 1 &&
                !(nextFM = GetFMFromIndex(e.RowIndex + 1)).MarkedUnavailable &&
                !nextFM.MarkedRecent
                    ? DarkColors.Fen_DGVCellBordersPen
                    : borderPen;

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

        #region Draw column headers

        /*
        @DarkModeNote(DGV headers):
        -Header painting appears to happen in DataGridViewColumnHeaderCell.PaintPrivate() - look here for
        precise text bounds calculations etc.
        */
        if (e.RowIndex == -1)
        {
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

        #endregion
    }

    #endregion

    // Cancel column resize operations if we lose focus. Otherwise, we end up still in drag mode with the
    // mouse up and another window in the foreground, which looks annoying to me, so yeah.
    protected override void OnLeave(EventArgs e)
    {
        CancelColumnResize();
        base.OnLeave(e);
    }

    #endregion
}

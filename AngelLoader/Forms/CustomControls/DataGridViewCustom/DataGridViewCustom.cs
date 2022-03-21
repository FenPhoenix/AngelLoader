﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.LazyLoaded;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom : DataGridView, IDarkable
    {
        #region Private fields

        private MainForm _owner = null!;

        private readonly Color DefaultRowBackColor = SystemColors.Window;
        private readonly Color RecentHighlightColor = Color.LightGoldenrodYellow;
        private readonly Color UnavailableColor = Color.MistyRose;
        private readonly Color PinnedColor = Color.FromArgb(203, 226, 206);

        private bool _mouseHere;
        private int _mouseDownOnHeader = -1;

        #endregion

        #region Public fields

        #region Sort

        internal Column CurrentSortedColumn;
        internal SortDirection CurrentSortDirection = SortDirection.Ascending;

        #endregion

        internal readonly SelectedFM CurrentSelFM = new SelectedFM();

        // Only used if game tabs are enabled. It's used to save and restore per-tab selected FM, filters etc.
        internal readonly GameTabsState GameTabsState = new GameTabsState();

        #region Filter

        internal readonly Filter Filter = new Filter();
        internal readonly List<int> FilterShownIndexList = new List<int>();

        #endregion

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

                // Optimization for dark mode: We can avoid some reflection by having the custom header cells
                // check their dark mode bools before painting.
                foreach (DataGridViewColumn column in Columns)
                {
                    if (column.HeaderCell is DataGridViewColumnHeaderCellCustom cellCustom)
                    {
                        cellCustom.DarkModeEnabled = _darkModeEnabled;
                    }
                }

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

        internal void InjectOwner(MainForm owner) => _owner = owner;

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
            AssertR(SelectedRows.Count > 0, nameof(GetMainSelectedFM) + ": no rows selected!");
            AssertR(MainSelectedRow != null, nameof(MainSelectedRow) + " is null when it shouldn't be");

            return GetFMFromIndex(MainSelectedRow!.Index);
        }

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

        internal int GetIndexFromFM(FanMission fm)
        {
            for (int i = 0; i < FilterShownIndexList.Count; i++)
            {
                if (GetFMFromIndex(i) == fm) return i;
            }
            return -1;
        }

        internal int GetIndexFromInstalledName(string installedName, bool findNearest)
        {
            // Graceful default if a value is missing
            if (installedName.IsEmpty()) return 0;

            for (int i = 0; i < FilterShownIndexList.Count; i++)
            {
                if (GetFMFromIndex(i).InstalledDir.EqualsI(installedName)) return i;
            }

            // If a refresh has caused our selected FM to be filtered out, find the next closest one
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

            return 0;
        }

        internal SelectedFM GetMainSelectedFMPosInfo() =>
            SelectedRows.Count == 0
                ? new SelectedFM { InstalledName = "", IndexFromTop = 0 }
                : GetFMPosInfoFromIndex(index: MainSelectedRow!.Index);

        internal SelectedFM GetFMPosInfoFromIndex(int index)
        {
            var ret = new SelectedFM { InstalledName = "", IndexFromTop = 0 };

            if (SelectedRows.Count == 0) return ret;

            int firstDisplayed = FirstDisplayedScrollingRowIndex;
            int lastDisplayed = firstDisplayed + DisplayedRowCount(false);

            int indexFromTop = index >= firstDisplayed && index <= lastDisplayed
                ? index - firstDisplayed
                : DisplayedRowCount(true) / 2;

            ret.InstalledName = GetFMFromIndex(index).InstalledDir;
            ret.IndexFromTop = indexFromTop;
            return ret;
        }

        #endregion

        /// <summary>
        /// Returns true if any row is selected, false if no rows exist or none are selected.
        /// </summary>
        /// <returns></returns>
        internal bool RowSelected() => MainSelectedRow != null;

        internal bool MultipleFMsSelected() => SelectedRows.Count > 1;

        internal void SelectSingle(int index)
        {
            ClearSelection();
            Rows[index].Selected = true;
        }

        #region Get and set columns

        internal ColumnData[] GetColumnData()
        {
            var columns = new ColumnData[Columns.Count];

            for (int i = 0; i < Columns.Count; i++)
            {
                DataGridViewColumn col = Columns[i];
                columns[i] = new ColumnData
                {
                    Id = (Column)col.Index,
                    DisplayIndex = col.DisplayIndex,
                    Visible = col.Visible,
                    Width = col.Width
                };
            }

            return columns.OrderBy(x => x.Id).ToArray();
        }

        internal void SetColumnData(FMsDGV_ColumnHeaderLLMenu menu, ColumnData[] columnDataList)
        {
            if (columnDataList.Length == 0) return;

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

            var columnDataListSorted = columnDataList.OrderBy(x => x.DisplayIndex).ToArray();

            #endregion

            foreach (ColumnData colData in columnDataListSorted)
            {
                DataGridViewColumn col = Columns[(int)colData.Id];

                col.DisplayIndex = colData.DisplayIndex;
                if (col.Resizable == DataGridViewTriState.True) col.Width = colData.Width;
                MakeColumnVisible(col, colData.Visible);

                menu.SetColumnChecked((int)colData.Id, colData.Visible);
            }
        }

        #endregion

        /// <summary>
        /// If you don't have an actual cell selected (indicated by its header being blue) and you try to move
        /// with the keyboard, it pops back to the top item. This fixes that, and is called wherever appropriate.
        /// </summary>
        internal void SelectProperly(bool suspendResume = true)
        {
            DataGridViewSelectedRowCollection selRows;
            if (Rows.Count == 0 || Columns.Count == 0 || (selRows = SelectedRows).Count == 0)
            {
                return;
            }

            // Crappy mitigation for losing horizontal scroll position, not perfect but better than nothing
            int origHSO = HorizontalScrollingOffset;

            try
            {
                // Note: we need to do this null check here, otherwise we get an exception that doesn't get caught(!!!)
                selRows[0].Cells[FirstDisplayedCell?.ColumnIndex ?? 0].Selected = true;
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

        internal DataGridViewRow? MainSelectedRow = null;

        private void SetMainSelectedRow()
        {
            var selRows = SelectedRows;
            if (selRows.Count == 0)
            {
                MainSelectedRow = null;
                //System.Diagnostics.Trace.WriteLine("selection cleared");
            }
            else
            {
                if (MainSelectedRow == null || selRows.Count == 1)
                {
                    MainSelectedRow = selRows[0];
                }
                //System.Diagnostics.Trace.WriteLine(GetFMFromIndex(MainSelectedRow.Index).Archive);
                _owner.UpdateUIControlsForMultiSelectState(GetMainSelectedFM());
            }
        }

        protected override void OnSelectionChanged(EventArgs e)
        {
            SetMainSelectedRow();
            base.OnSelectionChanged(e);
        }

        // @MULTISEL(FMsDGV on rows added/removed): I suspect we don't need to set main sel on these
        // Because the FM refresh method changes the selection explicitly (for rows) or implicitly (for no rows)
        protected override void OnRowsAdded(DataGridViewRowsAddedEventArgs e)
        {
            SetMainSelectedRow();
            base.OnRowsAdded(e);
        }

        protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
        {
            SetMainSelectedRow();
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
                var fm = GetFMFromIndex(e.RowIndex);

                Rows[e.RowIndex].DefaultCellStyle.BackColor =
                    fm.MarkedUnavailable
                    ? UnavailableColor
                    : fm.Pinned
                    ? PinnedColor
                    : fm.MarkedRecent
                    ? RecentHighlightColor
                    : DefaultRowBackColor;
            }
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            base.OnCellPainting(e);

            if (!_darkModeEnabled) return;

            // This is for having different colored grid lines in recent-highlighted rows.
            // That way, we can get a good, visible separator color for all cases by just having two.

            static bool IsSelected(DataGridViewElementStates state) =>
                (state & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected;

            if (e.RowIndex > -1)
            {
                FanMission fm = GetFMFromIndex(e.RowIndex);

                #region Paint cell background

                SolidBrush bgBrush = IsSelected(e.State)
                    ? DarkColors.BlueSelectionBrush
                    : fm.MarkedUnavailable
                    ? DarkColors.Fen_RedHighlightBrush
                    : fm.Pinned
                    ? DarkColors.PinnedBackgroundDarkBrush
                    : fm.MarkedRecent
                    ? DarkColors.Fen_DGVCellBordersBrush
                    : DarkColors.Fen_DarkBackgroundBrush;

                e.Graphics.FillRectangle(bgBrush, e.CellBounds);

                #endregion

                #region Draw content

                e.CellStyle.ForeColor = IsSelected(e.State)
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

                bool mouseOver = e.CellBounds.Contains(PointToClient(Cursor.Position));

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
                var selectionRect = new Rectangle(
                    e.CellBounds.X,
                    e.CellBounds.Y,
                    e.CellBounds.Width,
                    e.CellBounds.Height - 1);

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
                    var b = _mouseDownOnHeader == e.ColumnIndex
                        ? DarkColors.Fen_DGVColumnHeaderPressedBrush
                        : DarkColors.Fen_DGVColumnHeaderHighlightBrush;
                    e.Graphics.FillRectangle(b, selectionRect);
                }

                // The classic-themed bounds are complicated and difficult to discern, so we're just using
                // measured constants here, which match classic mode in our particular case.
                var textRect = new Rectangle(
                    e.CellBounds.X + (displayIndex == 0 ? 6 : 4),
                    e.CellBounds.Y,
                    e.CellBounds.Width - (displayIndex == 0 ? 10 : 6),
                    e.CellBounds.Height
                );

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
                        RowsDefaultCellStyle.ForeColor,
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
                        var direction = CurrentSortDirection == SortDirection.Ascending
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
}

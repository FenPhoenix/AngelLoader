using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.Static_LazyLoaded;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom : DarkDataGridView
    {
        #region Private fields

        private MainForm _owner = null!;

        private readonly Color DefaultRowBackColor = SystemColors.Window;
        private readonly Color RecentHighlightColor = Color.LightGoldenrodYellow;
        private readonly Color UnavailableColor = Color.MistyRose;

        private bool _mouseHere;
        private int _mouseDownOnHeader = -1;

        #endregion

        #region Public fields

        #region Sort

        internal Column CurrentSortedColumn;
        internal SortOrder CurrentSortDirection = SortOrder.None;

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
        public override bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;

                base.DarkModeEnabled = value;

                BackgroundColor = _darkModeEnabled ? DarkColors.Fen_DarkBackground : SystemColors.ControlDark;
            }
        }

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
        internal FanMission GetSelectedFM()
        {
            AssertR(SelectedRows.Count > 0, "GetSelectedFM: no rows selected!");

            return GetFMFromIndex(SelectedRows[0].Index);
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

        internal SelectedFM GetSelectedFMPosInfo()
        {
            var ret = new SelectedFM { InstalledName = "", IndexFromTop = 0 };

            if (SelectedRows.Count == 0) return ret;

            int sel = SelectedRows[0].Index;
            int firstDisplayed = FirstDisplayedScrollingRowIndex;
            int lastDisplayed = firstDisplayed + DisplayedRowCount(false);

            int indexFromTop = sel >= firstDisplayed && sel <= lastDisplayed
                ? sel - firstDisplayed
                : DisplayedRowCount(true) / 2;

            ret.InstalledName = GetFMFromIndex(sel).InstalledDir;
            ret.IndexFromTop = indexFromTop;
            return ret;
        }

        #endregion

        /// <summary>
        /// Returns true if any row is selected, false if no rows exist or none are selected.
        /// </summary>
        /// <returns></returns>
        internal bool RowSelected() => SelectedRows.Count > 0;

        #region Set context menu

        internal void SetContextMenuToNone() => ContextMenuStrip = null;

        internal void SetContextMenuToColumnHeader()
        {
            FMsDGV_ColumnHeaderLLMenu.Construct(_owner);
            ContextMenuStrip = FMsDGV_ColumnHeaderLLMenu.GetContextMenu();
        }

        internal void SetContextMenuToFM()
        {
            FMsDGV_FM_LLMenu.Construct(_owner);
            ContextMenuStrip = FMsDGV_FM_LLMenu.FMContextMenu;
        }

        #endregion

        #region Get and set columns

        internal List<ColumnData> GetColumnData()
        {
            var columns = new List<ColumnData>(Columns.Count);

            foreach (DataGridViewColumn col in Columns)
            {
                columns.Add(new ColumnData
                {
                    Id = (Column)col.Index,
                    DisplayIndex = col.DisplayIndex,
                    Visible = col.Visible,
                    Width = col.Width
                });
            }

            return columns.OrderBy(x => x.Id).ToList();
        }

        internal void SetColumnData(List<ColumnData> columnDataList)
        {
            if (columnDataList.Count == 0) return;

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

            columnDataList = columnDataList.OrderBy(x => x.DisplayIndex).ToList();

            #endregion

            foreach (ColumnData colData in columnDataList)
            {
                DataGridViewColumn col = Columns[(int)colData.Id];

                col.DisplayIndex = colData.DisplayIndex;
                if (col.Resizable == DataGridViewTriState.True) col.Width = colData.Width;
                ControlUtils.MakeColumnVisible(col, colData.Visible);

                FMsDGV_ColumnHeaderLLMenu.SetColumnChecked((int)colData.Id, colData.Visible);
            }
        }

        #endregion

        #endregion

        #region Event overrides

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

            int selectedIndex = SelectedRows.Count == 0 ? -1 : SelectedRows[0].Index;
            if (e.RowIndex > -1)
            {
                FanMission fm = GetFMFromIndex(e.RowIndex);

                #region Paint cell background

                SolidBrush bgBrush = selectedIndex == e.RowIndex
                    ? DarkColors.BlueSelectionBrush
                    : fm.MarkedUnavailable
                    ? DarkColors.Fen_RedHighlightBrush
                    : fm.MarkedRecent
                    ? DarkColors.Fen_DGVCellBordersBrush
                    : DarkColors.Fen_DarkBackgroundBrush;

                e.Graphics.FillRectangle(bgBrush, e.CellBounds);

                #endregion

                e.Paint(e.CellBounds, DataGridViewPaintParts.ContentForeground);

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
            TODO: @DarkMode(DGV headers):
            -TODO: @DarkMode: Draw sort glyph
            -Header painting appears to happen in DataGridViewColumnHeaderCell.PaintPrivate() - look here for
             precise text bounds calculations etc.
            -We're not painting the vestigial selected-column header color. We should see if we can owner-paint
             the classic-mode one too, in a way that's indistinguishable from the original except that it has no
             selected-column header blue color.
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
                }

                e.Handled = true;
            }

            #endregion
        }

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

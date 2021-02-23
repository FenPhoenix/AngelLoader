using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.Static_LazyLoaded;
using AngelLoader.WinAPI;
using DarkUI.Controls;
using static AngelLoader.Forms.ControlUtils;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom : DataGridView, IDarkableScrollable
    {
        #region Private fields

        private MainForm _owner = null!;

        private Color DefaultRowBackColor = SystemColors.Window;
        private Color RecentHighlightColor = Color.LightGoldenrodYellow;
        private Color UnavailableColor = Color.MistyRose;

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
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;

                if (_darkModeEnabled)
                {
                    RowsDefaultCellStyle.ForeColor = DarkUI.Config.Colors.Fen_DarkForeground;
                    GridColor = Color.FromArgb(64, 64, 64);
                    //FMsDGV.RowsDefaultCellStyle.BackColor = DarkUI.Config.Colors.Fen_DarkBackground;
                    RecentHighlightColor = Color.FromArgb(64, 64, 72);
                    UnavailableColor = Color.FromArgb(64, 24, 24);
                    DefaultRowBackColor = DarkUI.Config.Colors.Fen_DarkBackground;

                    // Custom refresh routine, because we don't want to run SelectProperly() as that will pop us
                    // back up and put our selection in view, but we don't want to do anything at all other than
                    // change the look, leaving everything exactly as it was functionally.
                    int selectedRow = RowCount > 0 ? SelectedRows[0].Index : -1;
                }
                else
                {
                    RowsDefaultCellStyle.ForeColor = SystemColors.ControlText;
                    GridColor = SystemColors.ControlDark;
                    RecentHighlightColor = Color.LightGoldenrodYellow;
                    UnavailableColor = Color.MistyRose;
                    DefaultRowBackColor = SystemColors.Window;
                    //FMsDGV.RowsDefaultCellStyle.BackColor = SystemColors.Window;
                }
            }
        }

        public DataGridViewCustom()
        {
            DoubleBuffered = true;

            VerticalVisualScrollBar = new ScrollBarVisualOnly(VerticalScrollBar);
            HorizontalVisualScrollBar = new ScrollBarVisualOnly(HorizontalScrollBar);

            /*
            TODO: @DarkMode(Scroll bars): The original plan:
            
            *** remove only after scroll bars are 100% complete and working to my satisfaction
            
            -Create a custom control that looks like a scroll bar and place it overtop of the real scroll bar(s),
             showing and hiding as the real scroll bars do. Dock and anchor so the size and position always matches
             the real scroll bar. Apply the style here https://stackoverflow.com/a/50245502 to make clicks fall
             through to the real scroll bar underneath.
            -On Paint of whatever control contains the scroll bars, call GetScrollBarInfo() to get the top and
             bottom of the scroll thumb, and paint our themed scroll bar to match.
            -On mouse events of the real scroll bar, detect cursor position (is it on the arrow, the thumb, etc.?)
             and paint ourselves appropriately to show arrow pressed states etc.

            Notes:
            MSDN:
            "Beginning with Windows 8, WS_EX_LAYERED can be used with child windows and top-level windows. Previous
            Windows versions support WS_EX_LAYERED only for top-level windows."
            Damn... so we can't do this on 7, which we otherwise support.
            -Handle NCHITTEST / return HTTRANSPARENT - might work instead?
            -Handle all mouse events and pass them along with PostMessage() - this works, we'll use it
            */
        }

        #region API methods

        #region Init

        public new ScrollBar VerticalScrollBar => base.VerticalScrollBar;
        public new ScrollBar HorizontalScrollBar => base.HorizontalScrollBar;

        public ScrollBarVisualOnly VerticalVisualScrollBar { get; }
        public ScrollBarVisualOnly HorizontalVisualScrollBar { get; }

        internal void InjectOwner(MainForm owner) => _owner = owner;

        #endregion

        // We keep this non-static so we can call it with an instance syntax like everything else for consistency.
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        internal bool ColumnHeaderMenuVisible => FMsDGV_ColumnHeaderLLMenu.Visible;

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
                MakeColumnVisible(col, colData.Visible);

                FMsDGV_ColumnHeaderLLMenu.SetColumnChecked((int)colData.Id, colData.Visible);
            }
        }

        #endregion

        /// <summary>
        /// If you don't have an actual cell selected (indicated by its header being blue) and you try to move
        /// with the keyboard, it pops back to the top item. This fixes that, and is called wherever appropriate.
        /// </summary>
        internal void SelectProperly(bool suspendResume = true)
        {
            if (Rows.Count == 0 || SelectedRows.Count == 0 || Columns.Count == 0) return;

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

            // Coloring the recent rows here because if we do it in _CellValueNeeded, we get a brief flash of the
            // default white-background cell color before it changes.
            if (!_owner.CellValueNeededDisabled && FilterShownIndexList.Count > 0)
            {
                var fm = GetFMFromIndex(e.RowIndex);

                Rows[e.RowIndex].DefaultCellStyle.BackColor = fm.MarkedUnavailable
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

            // TODO: @DarkMode: This is for having different colored grid lines in recent-highlighted rows
            // That way, we can get a good, visible separator color for all cases by just having two.
            // But we need to figure this out exactly because it doesn't work properly as is.
            // https://stackoverflow.com/a/32170212

#if false

            if (Config.VisualTheme == VisualTheme.Classic || _owner.CellValueNeededDisabled ||
                FilterShownIndexList.Count == 0 || !GetFMFromIndex(e.RowIndex).MarkedRecent)
            {
                e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
                e.Handled = true;
                return;
            }

            e.Paint(e.ClipBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.Border);
            e.Graphics.DrawRectangle(Pens.Black, new Rectangle(e.CellBounds.Left, e.CellBounds.Top, e.CellBounds.Width - 1, e.CellBounds.Height - 1));
            //e.Graphics.DrawLine(
            //    Pens.Black,
            //    e.CellBounds.Left,
            //    e.CellBounds.Top,
            //    e.CellBounds.Right,
            //    e.CellBounds.Top);
            e.Handled = true;

#endif
            /*
            TODO: @DarkMode(DGV headers):
            -Tune highlighting colors
            -Draw sort glyph
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

                using (var b = new SolidBrush(DarkUI.Config.Colors.GreyBackground))
                {
                    e.Graphics.FillRectangle(b, e.CellBounds);
                }

                using (var p = new Pen(DarkUI.Config.Colors.GreySelection))
                {
                    if (!mouseOver)
                    {
                        e.Graphics.DrawLine(
                            p,
                            e.CellBounds.X + e.CellBounds.Width - 1,
                            0,
                            e.CellBounds.X + e.CellBounds.Width - 1,
                            e.CellBounds.Y + e.CellBounds.Height - 1);
                    }
                    e.Graphics.DrawLine(
                        p,
                        e.CellBounds.X,
                        e.CellBounds.Y + e.CellBounds.Height - 1,
                        e.CellBounds.X + e.CellBounds.Width,
                        e.CellBounds.Y + e.CellBounds.Height - 1);
                }

                if (_mouseHere && mouseOver)
                {
                    using var b = new SolidBrush(_mouseDownOnHeader == e.ColumnIndex
                        ? DarkUI.Config.Colors.BlueHighlight
                        : DarkUI.Config.Colors.BlueSelection);
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
                    const TextFormatFlags textFormatFlags =
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.NoPrefix |
                        TextFormatFlags.Default |
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
        }

        // Cancel column resize operations if we lose focus. Otherwise, we end up still in drag mode with the
        // mouse up and another window in the foreground, which looks annoying to me, so yeah.
        protected override void OnLeave(EventArgs e)
        {
            CancelColumnResize();
            base.OnLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_darkModeEnabled)
            {
                if (BorderStyle == BorderStyle.FixedSingle)
                {
                    // TODO: @DarkMode: Extract this pen...
                    using var p = new Pen(DarkUI.Config.Colors.GreySelection);
                    e.Graphics.DrawRectangle(p, 0, 0, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
                }
            }
        }

        #endregion

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case Native.WM_CTLCOLORSCROLLBAR:
                    if (_darkModeEnabled)
                    {
                        // Needed for scrollbar thumbs to show up immediately without using a timer
                        VerticalVisualScrollBar.RefreshScrollBar();
                        HorizontalVisualScrollBar.RefreshScrollBar();
                    }
                    else
                    {
                        base.WndProc(ref m);
                    }
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;

namespace AngelLoader.CustomControls
{
    public sealed class DataGridViewCustom : DataGridView, ILocalizable
    {
        #region Fields / Properties

        #region Column header context menu

        private readonly ToolStripMenuItem[] ColumnHeaderCheckBoxMenuItems;

        private enum ColumnProperties
        {
            Visible,
            DisplayIndex,
            Width
        }

        internal ContextMenuStripCustom FMColumnHeaderRightClickMenu = new ContextMenuStripCustom();
        private readonly ToolStripMenuItem ResetColumnVisibilityMenuItem = new ToolStripMenuItem
        {
            Name = "ResetColumnVisibilityMenuItem"
        };
        private readonly ToolStripMenuItem ResetAllColumnWidthsMenuItem = new ToolStripMenuItem
        {
            Name = "ResetAllColumnWidthsMenuItem"
        };
        private readonly ToolStripMenuItem ResetColumnPositionsMenuItem = new ToolStripMenuItem
        {
            Name = "ResetColumnPositionsMenuItem"
        };
        private readonly ToolStripSeparator ColumnHeaderRightClickMenuSeparator1 = new ToolStripSeparator
        {
            Name = "ColumnHeaderRightClickMenuSeparator1"
        };
        private readonly ToolStripMenuItem ShowGameMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowGameMenuItem"
        };
        private readonly ToolStripMenuItem ShowInstalledMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowInstalledMenuItem"
        };
        private readonly ToolStripMenuItem ShowTitleMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowTitleMenuItem"
        };
        private readonly ToolStripMenuItem ShowArchiveMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowArchiveMenuItem"
        };
        private readonly ToolStripMenuItem ShowAuthorMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowAuthorMenuItem"
        };
        private readonly ToolStripMenuItem ShowSizeMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowSizeMenuItem"
        };
        private readonly ToolStripMenuItem ShowRatingMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowRatingMenuItem"
        };
        private readonly ToolStripMenuItem ShowFinishedMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowFinishedMenuItem"
        };
        private readonly ToolStripMenuItem ShowReleaseDateMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowReleaseDateMenuItem"
        };
        private readonly ToolStripMenuItem ShowLastPlayedMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowLastPlayedMenuItem"
        };
        private readonly ToolStripMenuItem ShowDisabledModsMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowDisabledModsMenuItem"
        };
        private readonly ToolStripMenuItem ShowCommentMenuItem = new ToolStripMenuItem
        {
            Checked = true,
            CheckOnClick = true,
            CheckState = CheckState.Checked,
            Name = "ShowCommentMenuItem"
        };

        #endregion

        #region Sort

        internal int CurrentSortedColumn = -1;
        internal SortOrder CurrentSortDirection = SortOrder.None;

        #endregion

        #region Column header resize

        private enum DataGridViewHitTestTypeInternal
        {
            None,
            Cell,
            ColumnHeader,
            RowHeader,
            ColumnResizeLeft,
            ColumnResizeRight,
            RowResizeTop,
            RowResizeBottom,
            FirstColumnHeaderLeft,
            TopLeftHeader,
            TopLeftHeaderResizeLeft,
            TopLeftHeaderResizeRight,
            TopLeftHeaderResizeTop,
            TopLeftHeaderResizeBottom,
            ColumnHeadersResizeBottom,
            ColumnHeadersResizeTop,
            RowHeadersResizeRight,
            RowHeadersResizeLeft,
            ColumnHeaderLeft,
            ColumnHeaderRight
        }

        private bool ColumnResizeInProgress;
        private int ColumnToResize;
        private int ColumnToResizeOriginalMouseX;
        private int ColumnToResizeOriginalWidth;

        private ContextMenuStrip OriginalRightClickMenu;

        #endregion

        internal readonly SelectedFM CurrentSelFM = new SelectedFM();

        // Only used if game tabs are enabled. It's used to save and restore per-tab selected FM, filters etc.
        internal GameTabsState GameTabsState = new GameTabsState();

        #region Filter

        internal Filter Filter { get; set; } = new Filter();

        // Slightly janky separate value here, but we need it because:
        // -We can't check the filtered index list for length > 0 because it could be empty when ALL FMs are being
        //  filtered as well as when none are
        // -We could check Filter.IsEmpty(), but that's slow to do in a loop (and we do sometimes need to do it
        //  in a loop)
        internal bool Filtered = false;

        internal List<int> FilterShownIndexList = new List<int>();

        #endregion

        #endregion

        public DataGridViewCustom()
        {
            DoubleBuffered = true;

            #region Init column header context menu

            FMColumnHeaderRightClickMenu.Name = "FMColumnHeaderRightClickMenu";

            ResetColumnVisibilityMenuItem.Click += ResetColumnVisibilityMenuItem_Click;
            ResetAllColumnWidthsMenuItem.Click += ResetAllColumnWidthsMenuItem_Click;
            ResetColumnPositionsMenuItem.Click += ResetColumnPositionsMenuItem_Click;

            ShowGameMenuItem.Click += CheckBoxMenuItem_Click;
            ShowInstalledMenuItem.Click += CheckBoxMenuItem_Click;
            ShowTitleMenuItem.Click += CheckBoxMenuItem_Click;
            ShowArchiveMenuItem.Click += CheckBoxMenuItem_Click;
            ShowAuthorMenuItem.Click += CheckBoxMenuItem_Click;
            ShowSizeMenuItem.Click += CheckBoxMenuItem_Click;
            ShowRatingMenuItem.Click += CheckBoxMenuItem_Click;
            ShowFinishedMenuItem.Click += CheckBoxMenuItem_Click;
            ShowReleaseDateMenuItem.Click += CheckBoxMenuItem_Click;
            ShowLastPlayedMenuItem.Click += CheckBoxMenuItem_Click;
            ShowDisabledModsMenuItem.Click += CheckBoxMenuItem_Click;
            ShowCommentMenuItem.Click += CheckBoxMenuItem_Click;

            FMColumnHeaderRightClickMenu.Items.AddRange(new ToolStripItem[]
            {
                ResetColumnVisibilityMenuItem,
                ResetAllColumnWidthsMenuItem,
                ResetColumnPositionsMenuItem,
                ColumnHeaderRightClickMenuSeparator1,
                ShowGameMenuItem,
                ShowInstalledMenuItem,
                ShowTitleMenuItem,
                ShowArchiveMenuItem,
                ShowAuthorMenuItem,
                ShowSizeMenuItem,
                ShowRatingMenuItem,
                ShowFinishedMenuItem,
                ShowReleaseDateMenuItem,
                ShowLastPlayedMenuItem,
                ShowDisabledModsMenuItem,
                ShowCommentMenuItem
            });

            ColumnHeaderCheckBoxMenuItems = new[]
            {
                ShowGameMenuItem,
                ShowInstalledMenuItem,
                ShowTitleMenuItem,
                ShowArchiveMenuItem,
                ShowAuthorMenuItem,
                ShowSizeMenuItem,
                ShowRatingMenuItem,
                ShowFinishedMenuItem,
                ShowReleaseDateMenuItem,
                ShowLastPlayedMenuItem,
                ShowDisabledModsMenuItem,
                ShowCommentMenuItem
            };

            FMColumnHeaderRightClickMenu.SetPreventCloseOnClickItems(ColumnHeaderCheckBoxMenuItems);

            ShowGameMenuItem.Tag = Column.Game;
            ShowInstalledMenuItem.Tag = Column.Installed;
            ShowTitleMenuItem.Tag = Column.Title;
            ShowArchiveMenuItem.Tag = Column.Archive;
            ShowAuthorMenuItem.Tag = Column.Author;
            ShowSizeMenuItem.Tag = Column.Size;
            ShowRatingMenuItem.Tag = Column.Rating;
            ShowFinishedMenuItem.Tag = Column.Finished;
            ShowReleaseDateMenuItem.Tag = Column.ReleaseDate;
            ShowLastPlayedMenuItem.Tag = Column.LastPlayed;
            ShowDisabledModsMenuItem.Tag = Column.DisabledMods;
            ShowCommentMenuItem.Tag = Column.Comment;

            #endregion
        }

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            ResetColumnVisibilityMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnsToVisible;
            ResetAllColumnWidthsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnWidths;
            ResetColumnPositionsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnPositions;

            ShowGameMenuItem.Text = LText.FMsList.GameColumn;
            ShowInstalledMenuItem.Text = LText.FMsList.InstalledColumn;
            ShowTitleMenuItem.Text = LText.FMsList.TitleColumn;
            ShowArchiveMenuItem.Text = LText.FMsList.ArchiveColumn;
            ShowAuthorMenuItem.Text = LText.FMsList.AuthorColumn;
            ShowSizeMenuItem.Text = LText.FMsList.SizeColumn;
            ShowRatingMenuItem.Text = LText.FMsList.RatingColumn;
            ShowFinishedMenuItem.Text = LText.FMsList.FinishedColumn;
            ShowReleaseDateMenuItem.Text = LText.FMsList.ReleaseDateColumn;
            ShowLastPlayedMenuItem.Text = LText.FMsList.LastPlayedColumn;
            ShowDisabledModsMenuItem.Text = LText.FMsList.DisabledModsColumn;
            ShowCommentMenuItem.Text = LText.FMsList.CommentColumn;
        }

        #region Main

        #region Column header resize

        internal void CancelColumnResize()
        {
            if (!ColumnResizeInProgress) return;

            ColumnResizeInProgress = false;
            // Prevents the right-click menu from popping up if the user right-clicked to cancel. The menu will
            // be set back to what it should be when the user right-clicks while a resize is not progress.
            ContextMenuStrip = null;
            Columns[ColumnToResize].Width = ColumnToResizeOriginalWidth;
        }

        private int FindColumnIndexByDisplayIndex(int displayIndex)
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].DisplayIndex == displayIndex) return Columns[i].Index;
            }

            return -1;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            #region Resize

            var ht = HitTest(e.X, e.Y);

            // Manual implementation of real-time column width resizing (the column changes size as you drag)
            // TODO: If you mousedown while a right-click menu is up, the cursor isn't a size cursor. Fix it for
            // TODO: "the dev thought of everything" points.
            if (Cursor.Current == Cursors.SizeWE && e.Button == MouseButtons.Left)
            {
                var typeInternal =
                    ht.GetType().GetField("typeInternal", BindingFlags.NonPublic | BindingFlags.Instance);

                // Something has changed internally, so fall back to the crappy resize-without-indication default
                if (typeInternal == null)
                {
                    ColumnResizeInProgress = false;
                    base.OnMouseDown(e);
                    return;
                }

                var hitTestType = (DataGridViewHitTestTypeInternal)typeInternal.GetValue(ht);

                // When we're dragging a column divider, we always want to resize the column to the left of it.
                // But the hit test will report the right-side column if our mouse is to the right of the divider
                // when we start dragging, so in that case we need to use the column that's one to the left of
                // the reported one.
                // NOTE: I think ColumnResizeLeft means the resizable divider on the left side of the current
                //       column. But if we're thinking of the divider itself, we're on the right side of it.
                //       Just so I don't get confused again if I look at this in a few months.
                ColumnToResize = hitTestType == DataGridViewHitTestTypeInternal.ColumnResizeLeft
                    ? FindColumnIndexByDisplayIndex(Columns[ht.ColumnIndex].DisplayIndex - 1)
                    : ht.ColumnIndex;

                ColumnToResizeOriginalMouseX = e.X;
                ColumnToResizeOriginalWidth = Columns[ColumnToResize].Width;
                ColumnResizeInProgress = true;
                OriginalRightClickMenu = ContextMenuStrip;
                return;
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (ColumnResizeInProgress)
                {
                    CancelColumnResize();
                    return;
                }
                else
                {
                    ContextMenuStrip = OriginalRightClickMenu;
                }
            }

            #endregion

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ColumnResizeInProgress)
            {
                // The move is complete
                ColumnResizeInProgress = false;
                return;
            }

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
            if (ColumnResizeInProgress)
            {
                Columns[ColumnToResize].Width = e.X > ColumnToResizeOriginalMouseX
                    ? ColumnToResizeOriginalWidth + (e.X - ColumnToResizeOriginalMouseX)
                    : ColumnToResizeOriginalWidth - (ColumnToResizeOriginalMouseX - e.X);
                return;
            }

            base.OnMouseMove(e);
        }

        // Cancel column resize operations if we lose focus. Otherwise, we end up still in drag mode with the
        // mouse up and another window in the foreground, which looks annoying to me, so yeah.
        protected override void OnLeave(EventArgs e)
        {
            CancelColumnResize();

            base.OnLeave(e);
        }

        #endregion

        private static void MakeVisible(DataGridViewColumn column, bool visible)
        {
            column.Visible = visible;
            // Fix for zero-height glitch when Rating column gets swapped out when all columns are hidden
            column.Width = column.Width + 1;
            column.Width = column.Width - 1;
        }

        #region Column header context menu

        private void ResetPropertyOnAllColumns(ColumnProperties property)
        {
            for (var i = 0; i < Columns.Count; i++)
            {
                DataGridViewColumn c = Columns[i];
                switch (property)
                {
                    case ColumnProperties.Visible:
                        MakeVisible(c, true);
                        break;
                    case ColumnProperties.DisplayIndex:
                        c.DisplayIndex = c.Index;
                        break;
                    case ColumnProperties.Width:
                        if (c.Resizable == DataGridViewTriState.True) c.Width = Defaults.ColumnWidth;
                        break;
                }

                ColumnHeaderCheckBoxMenuItems[c.Index].Checked = c.Visible;
            }
        }

        internal void ResetColumnVisibilityMenuItem_Click(object sender, EventArgs e)
        {
            ResetPropertyOnAllColumns(ColumnProperties.Visible);
        }

        internal void ResetColumnPositionsMenuItem_Click(object sender, EventArgs e)
        {
            ResetPropertyOnAllColumns(ColumnProperties.DisplayIndex);
        }

        internal void ResetAllColumnWidthsMenuItem_Click(object sender, EventArgs e)
        {
            ResetPropertyOnAllColumns(ColumnProperties.Width);
        }

        private void CheckBoxMenuItem_Click(object sender, EventArgs e)
        {
            var s = (ToolStripMenuItem)sender;

            MakeVisible(Columns[(int)s.Tag], s.Checked);
        }

        #endregion

        internal List<ColumnData> ColumnsToColumnData()
        {
            var columns = new List<ColumnData>();

            foreach (DataGridViewColumn col in Columns)
            {
                var cData = new ColumnData
                {
                    Id = (Column)col.Index,
                    DisplayIndex = col.DisplayIndex,
                    Visible = col.Visible,
                    Width = col.Width
                };

                columns.Add(cData);
            }

            columns = columns.OrderBy(x => x.Id).ToList();

            return columns;
        }

        internal void FillColumns(List<ColumnData> columnDataList)
        {
            if (columnDataList == null || columnDataList.Count == 0) return;

            #region Important

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

            foreach (var colData in columnDataList)
            {
                var col = Columns[(int)colData.Id];

                col.DisplayIndex = colData.DisplayIndex;
                if (col.Resizable == DataGridViewTriState.True) col.Width = colData.Width;
                MakeVisible(col, colData.Visible);

                ColumnHeaderCheckBoxMenuItems[(int)colData.Id].Checked = colData.Visible;
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                OriginalRightClickMenu?.Dispose();
                FMColumnHeaderRightClickMenu?.Dispose();
                ShowTitleMenuItem?.Dispose();
                ShowArchiveMenuItem?.Dispose();
                ShowSizeMenuItem?.Dispose();
                ShowRatingMenuItem?.Dispose();
                ShowReleaseDateMenuItem?.Dispose();
                ShowLastPlayedMenuItem?.Dispose();
                ShowCommentMenuItem?.Dispose();
                ShowDisabledModsMenuItem?.Dispose();
                ShowInstalledMenuItem?.Dispose();
                ShowGameMenuItem?.Dispose();
                ShowAuthorMenuItem?.Dispose();
                ResetColumnVisibilityMenuItem?.Dispose();
                ResetColumnPositionsMenuItem?.Dispose();
                ResetAllColumnWidthsMenuItem?.Dispose();
                ColumnHeaderRightClickMenuSeparator1?.Dispose();
                ShowFinishedMenuItem?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

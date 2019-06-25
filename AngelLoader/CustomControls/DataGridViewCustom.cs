using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.CustomControls
{
    public sealed partial class DataGridViewCustom : DataGridView, ILocalizable
    {
        #region Fields / Properties

        #region Column header context menu

        private enum ColumnProperties { Visible, DisplayIndex, Width }

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

        private ContextMenuStrip OriginalContextMenu;

        #endregion

        internal readonly SelectedFM CurrentSelFM = new SelectedFM();

        // Only used if game tabs are enabled. It's used to save and restore per-tab selected FM, filters etc.
        internal GameTabsState GameTabsState = new GameTabsState();

        #region Filter

        internal Filter Filter = new Filter();

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

            Debug.Assert(Enum.GetValues(typeof(Column)).Length == ColumnCheckedStates.Length,
                nameof(Column) + ".Length != " + nameof(ColumnCheckedStates) + ".Length");
        }

        #region Get FM selected/index etc.

        /// <summary>
        /// Gets the FM at <paramref name="index"/>, taking the currently set filters into account.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal FanMission GetFMFromIndex(int index) => Core.FMsViewList[Filtered ? FilterShownIndexList[index] : index];

        /// <summary>
        /// Gets the currently selected FM, taking the currently set filters into account.
        /// </summary>
        /// <returns></returns>
        internal FanMission GetSelectedFM()
        {
            Debug.Assert(SelectedRows.Count > 0, "GetSelectedFM: no rows selected!");

            return GetFMFromIndex(SelectedRows[0].Index);
        }

        internal int GetIndexFromInstalledName(string installedName)
        {
            // Graceful default if a value is missing
            if (installedName.IsEmpty()) return 0;

            for (int i = 0; i < (Filtered ? FilterShownIndexList.Count : Core.FMsViewList.Count); i++)
            {
                if (GetFMFromIndex(i).InstalledDir.EqualsI(installedName)) return i;
            }

            return 0;
        }

        internal SelectedFM GetSelectedFMPosInfo()
        {
            var ret = new SelectedFM { InstalledName = null, IndexFromTop = 0 };

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

        #region Localization

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            SetColumnHeaderMenuItemTextToLocalized();
        }

        private void SetColumnHeaderMenuItemTextToLocalized()
        {
            if (!_columnHeaderMenuCreated) return;

            ResetColumnVisibilityMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnsToVisible.EscapeAmpersands();
            ResetAllColumnWidthsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnWidths.EscapeAmpersands();
            ResetColumnPositionsMenuItem.Text = LText.FMsList.ColumnMenu_ResetAllColumnPositions.EscapeAmpersands();

            ShowGameMenuItem.Text = LText.FMsList.GameColumn.EscapeAmpersands();
            ShowInstalledMenuItem.Text = LText.FMsList.InstalledColumn.EscapeAmpersands();
            ShowTitleMenuItem.Text = LText.FMsList.TitleColumn.EscapeAmpersands();
            ShowArchiveMenuItem.Text = LText.FMsList.ArchiveColumn.EscapeAmpersands();
            ShowAuthorMenuItem.Text = LText.FMsList.AuthorColumn.EscapeAmpersands();
            ShowSizeMenuItem.Text = LText.FMsList.SizeColumn.EscapeAmpersands();
            ShowRatingMenuItem.Text = LText.FMsList.RatingColumn.EscapeAmpersands();
            ShowFinishedMenuItem.Text = LText.FMsList.FinishedColumn.EscapeAmpersands();
            ShowReleaseDateMenuItem.Text = LText.FMsList.ReleaseDateColumn.EscapeAmpersands();
            ShowLastPlayedMenuItem.Text = LText.FMsList.LastPlayedColumn.EscapeAmpersands();
            ShowDisabledModsMenuItem.Text = LText.FMsList.DisabledModsColumn.EscapeAmpersands();
            ShowCommentMenuItem.Text = LText.FMsList.CommentColumn.EscapeAmpersands();
        }

        private void SetFMMenuTextToLocalized()
        {
            if (!_fmMenuCreated) return;

            #region Get current FM info

            // Some menu items' text depends on FM state. Because this could be run after startup, we need to
            // make sure those items' text is set correctly.
            var selFM = SelectedRows.Count > 0 ? GetSelectedFM() : null;
            bool sayInstall = selFM == null || !selFM.Installed;

            #endregion

            #region Play

            PlayFMMenuItem.Text = LText.FMsList.FMMenu_PlayFM.EscapeAmpersands();
            PlayFMInMPMenuItem.Text = LText.FMsList.FMMenu_PlayFM_Multiplayer.EscapeAmpersands();

            //PlayFMAdvancedMenuItem.Text = LText.FMsList.FMMenu_PlayFMAdvanced.EscapeAmpersands();
            //Core.SetDefaultConfigVarNamesToLocalized();

            #endregion

            InstallUninstallMenuItem.Text = (sayInstall
                ? LText.FMsList.FMMenu_InstallFM
                : LText.FMsList.FMMenu_UninstallFM).EscapeAmpersands();

            OpenInDromEdMenuItem.Text = LText.FMsList.FMMenu_OpenInDromEd.EscapeAmpersands();

            ScanFMMenuItem.Text = LText.FMsList.FMMenu_ScanFM.EscapeAmpersands();

            #region Convert submenu

            ConvertAudioRCSubMenu.Text = LText.FMsList.FMMenu_ConvertAudio.EscapeAmpersands();
            ConvertWAVsTo16BitMenuItem.Text = LText.FMsList.ConvertAudioMenu_ConvertWAVsTo16Bit.EscapeAmpersands();
            ConvertOGGsToWAVsMenuItem.Text = LText.FMsList.ConvertAudioMenu_ConvertOGGsToWAVs.EscapeAmpersands();

            #endregion

            #region Rating submenu

            RatingRCSubMenu.Text = LText.FMsList.FMMenu_Rating.EscapeAmpersands();
            RatingRCMenuUnrated.Text = LText.Global.Unrated.EscapeAmpersands();

            #endregion

            #region Finished On submenu

            FinishedOnRCSubMenu.Text = LText.FMsList.FMMenu_FinishedOn.EscapeAmpersands();

            var fmIsT3 = selFM != null && selFM.Game == Game.Thief3;

            FinishedOnNormalMenuItem.Text = (fmIsT3 ? LText.Difficulties.Easy : LText.Difficulties.Normal).EscapeAmpersands();
            FinishedOnHardMenuItem.Text = (fmIsT3 ? LText.Difficulties.Normal : LText.Difficulties.Hard).EscapeAmpersands();
            FinishedOnExpertMenuItem.Text = (fmIsT3 ? LText.Difficulties.Hard : LText.Difficulties.Expert).EscapeAmpersands();
            FinishedOnExtremeMenuItem.Text = (fmIsT3 ? LText.Difficulties.Expert : LText.Difficulties.Extreme).EscapeAmpersands();
            FinishedOnUnknownMenuItem.Text = LText.Difficulties.Unknown.EscapeAmpersands();

            #endregion

            WebSearchMenuItem.Text = LText.FMsList.FMMenu_WebSearch.EscapeAmpersands();
        }

        internal void UpdateRatingList(bool fmSelStyle)
        {
            if (_fmMenuCreated)
            {
                for (int i = 0; i <= 10; i++)
                {
                    string num = (fmSelStyle ? i / 2.0 : i).ToString(CultureInfo.CurrentCulture);
                    RatingRCSubMenu.DropDownItems[i + 1].Text = num;
                }
            }
            else
            {
                // update backing
            }
        }

        #endregion

        #region Main

        #region Column header resize

        internal void CancelColumnResize()
        {
            if (!ColumnResizeInProgress) return;

            ColumnResizeInProgress = false;
            // Prevents the context menu from popping up if the user right-clicked to cancel. The menu will be
            // set back to what it should be when the user right-clicks while a resize is not progress.
            SetContextMenu(null);
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
            // TODO: If you mousedown while a context menu is up, the cursor isn't a size cursor. Fix it for
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
                OriginalContextMenu = ContextMenuStrip;
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
                    SetContextMenu(OriginalContextMenu);
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

        private static void MakeColumnVisible(DataGridViewColumn column, bool visible)
        {
            column.Visible = visible;
            // Fix for zero-height glitch when Rating column gets swapped out when all columns are hidden
            column.Width++;
            column.Width--;
        }

        #region Column header context menu

        internal void SetContextMenu(ContextMenuStrip menu)
        {
            if (!_columnHeaderMenuCreated && menu == FMColumnHeaderContextMenu)
            {
                InitColumnHeaderContextMenu();
            }

            ContextMenuStrip = menu;
        }

        private void ResetPropertyOnAllColumns(ColumnProperties property)
        {
            for (var i = 0; i < Columns.Count; i++)
            {
                DataGridViewColumn c = Columns[i];
                switch (property)
                {
                    case ColumnProperties.Visible:
                        MakeColumnVisible(c, true);
                        break;
                    case ColumnProperties.DisplayIndex:
                        c.DisplayIndex = c.Index;
                        break;
                    case ColumnProperties.Width:
                        if (c.Resizable == DataGridViewTriState.True) c.Width = Defaults.ColumnWidth;
                        break;
                }

                SetColumnChecked(c.Index, c.Visible);
            }
        }

        private void SetColumnChecked(int index, bool enabled)
        {
            if (_columnHeaderMenuCreated)
            {
                ColumnHeaderCheckBoxMenuItems[index].Checked = enabled;
            }
            else
            {
                ColumnCheckedStates[index] = enabled;
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

            MakeColumnVisible(Columns[(int)s.Tag], s.Checked);
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
                MakeColumnVisible(col, colData.Visible);

                SetColumnChecked((int)colData.Id, colData.Visible);
            }
        }

        #endregion
    }
}

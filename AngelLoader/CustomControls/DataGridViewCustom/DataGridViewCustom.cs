using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls
{
    public sealed partial class DataGridViewCustom : DataGridView, ILocalizable
    {
        #region Private fields

        private IView Owner;

        #endregion

        #region API fields

        #region Sort

        internal int CurrentSortedColumn = -1;
        internal SortOrder CurrentSortDirection = SortOrder.None;

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

        #region Private methods

        private static void MakeColumnVisible(DataGridViewColumn column, bool visible)
        {
            column.Visible = visible;
            // Fix for zero-height glitch when Rating column gets swapped out when all columns are hidden
            column.Width++;
            column.Width--;
        }

        private void SetConcreteInstallUninstallMenuItemText(bool sayInstall)
        {
            InstallUninstallMenuItem.Text = (sayInstall
                ? LText.FMsList.FMMenu_InstallFM
                : LText.FMsList.FMMenu_UninstallFM).EscapeAmpersands();
        }

        #endregion

        #region API methods

        #region Init

        public DataGridViewCustom()
        {
            DoubleBuffered = true;

            Debug.Assert(Enum.GetValues(typeof(Column)).Length == ColumnHeaderLLMenu.ColumnCheckedStates.Length,
                nameof(Column) + ".Length != " + nameof(ColumnHeaderLLMenu.ColumnCheckedStates) + ".Length");
        }

        internal void InjectOwner(IView owner) => Owner = owner;

        public void SetUITextToLocalized(bool suspendResume = true)
        {
            ColumnHeaderLLMenu.SetMenuItemTextToLocalized();
            SetFMMenuTextToLocalized();
        }

        #endregion

        #region Get FM / FM data

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

        #region Set context menu

        internal void SetContextMenuToNone() => ContextMenuStrip = null;

        internal void SetContextMenuToColumnHeader()
        {
            ColumnHeaderLLMenu.Init(this);
            ContextMenuStrip = ColumnHeaderLLMenu.GetContextMenu();
        }

        internal void SetContextMenuToFM()
        {
            InitFMContextMenu();
            ContextMenuStrip = FMContextMenu;
        }

        #endregion

        #region Get and set columns

        internal List<ColumnData> GetColumnData()
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

        internal void SetColumnData(List<ColumnData> columnDataList)
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

                ColumnHeaderLLMenu.SetColumnChecked((int)colData.Id, colData.Visible);
            }
        }

        #endregion

        /// <summary>
        /// If you don't have an actual cell selected (indicated by its header being blue) and you try to move
        /// with the keyboard, it pops back to the top item. This fixes that, and is called wherever appropriate.
        /// </summary>
        internal void SelectProperly()
        {
            if (Rows.Count == 0 || SelectedRows.Count == 0 || Columns.Count == 0) return;

            for (int i = 0; i < SelectedRows[0].Cells.Count; i++)
            {
                if (SelectedRows[0].Cells[i].Visible)
                {
                    try
                    {
                        SelectedRows[0].Cells[i].Selected = true;
                        break;
                    }
                    catch
                    {
                        // It can't be selected for whatever reason. Oh well.
                    }
                }
            }
        }

        #endregion

        #region Event overrides

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!StartColumnResize(e)) return;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
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

        // Cancel column resize operations if we lose focus. Otherwise, we end up still in drag mode with the
        // mouse up and another window in the foreground, which looks annoying to me, so yeah.
        protected override void OnLeave(EventArgs e)
        {
            CancelColumnResize();
            base.OnLeave(e);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ColumnHeaderLLMenu.Dispose();
                DisposeFMContextMenu();
            }
            base.Dispose(disposing);
        }
    }
}

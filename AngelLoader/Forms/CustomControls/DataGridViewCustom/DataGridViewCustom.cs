using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls.Static_LazyLoaded;
using static AngelLoader.Forms.ControlExtensions;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed partial class DataGridViewCustom : DataGridView
    {
        #region Private fields

        private MainForm _owner = null!;

        #endregion

        #region API fields

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

        #region API methods

        #region Init

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private const uint OBJID_HSCROLL = 0xFFFFFFFA;
        private const uint OBJID_VSCROLL = 0xFFFFFFFB;
        private const uint OBJID_CLIENT = 0xFFFFFFFC;

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        private static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref SCROLLBARINFO psbi);

        [StructLayout(LayoutKind.Sequential)]
        public struct SCROLLBARINFO
        {
            public int cbSize;
            public RECT rcScrollBar;
            public int dxyLineButton;
            public int xyThumbTop;
            public int xyThumbBottom;
            public int reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

        public DataGridViewCustom()
        {
            DoubleBuffered = true;

            /*
            TODO: @DarkMode(Scroll bars): The plan:
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
            */
            /*
            this.Paint += (sender, e) =>
            {
                //e.Graphics.FillRectangle(Brushes.Red, 0, 0, Size.Width, Size.Height);

                //Trace.WriteLine(_rnd.Next() + " dgv vscrolled");
                SCROLLBARINFO psbi = new SCROLLBARINFO();
                psbi.cbSize = Marshal.SizeOf(psbi);
                int result = GetScrollBarInfo(this.VerticalScrollBar.Handle, OBJID_CLIENT, ref psbi);
                if (result == 0)
                {
                    Trace.WriteLine("***ERROR: " + Marshal.GetLastWin32Error());
                }
                else
                {
                    Trace.WriteLine(nameof(psbi.dxyLineButton) + ": " + psbi.dxyLineButton);
                    Trace.WriteLine(nameof(psbi.xyThumbTop) + ": " + psbi.xyThumbTop);
                    Trace.WriteLine(nameof(psbi.xyThumbBottom) + ": " + psbi.xyThumbBottom);
                }
            };
            */
        }

        internal void InjectOwner(MainForm owner) => _owner = owner;

        internal void Localize()
        {
            FMsDGV_ColumnHeaderLLMenu.SetMenuItemTextToLocalized();
            FMsDGV_FM_LLMenu.SetFMMenuTextToLocalized();
        }

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
    }
}

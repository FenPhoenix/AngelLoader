using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    internal class TabControlCustom : TabControl
    {
        internal class BackingTab
        {
            internal TabPage? Tab;
            internal bool Visible = true;
        }

        private TabPage? DragTab;
        private readonly List<BackingTab> BackingTabList = new List<BackingTab>();

        public TabControlCustom()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        internal void AddTabsFull(IEnumerable<TabPage> tabPages)
        {
            BackingTabList.Clear();

            foreach (TabPage tabPage in tabPages)
            {
                TabPages.Add(tabPage);
                BackingTabList.Add(new BackingTab { Tab = tabPage });
            }
        }

        internal (int index, BackingTab? backingTab)
        FindBackingTab(TabPage tab, bool indexVisibleOnly = false)
        {
            for (int i = 0, vi = 0; i < BackingTabList.Count; i++)
            {
                var bt = BackingTabList[i];
                if (indexVisibleOnly && bt.Visible) vi++;
                if (bt?.Tab == tab) return (indexVisibleOnly ? vi : i, bt);
            }

            if (!DesignMode)
            {
                throw new Exception(nameof(FindBackingTab) + " couldn't find the specified tab page '" +
                                    // DOTNAME
                                    tab.Name +
                                    "'. That's not supposed to happen. All tab pages should always exist in the backing list.");
            }

            // To keep design mode tab selection happy
            return (-1, null);
        }

        internal void ShowTab(TabPage tab, bool show)
        {
            var (index, bt) = FindBackingTab(tab, indexVisibleOnly: true);
            if (index < 0 || bt == null) return;

            if (show)
            {
                bt.Visible = true;
                if (!TabPages.Contains(bt.Tab)) TabPages.Insert(Math.Min(index, TabCount), bt.Tab);
            }
            else
            {
                bt.Visible = false;
                if (TabPages.Contains(bt.Tab)) TabPages.Remove(bt.Tab);
            }
        }

        #region Tab reordering

        protected override void OnMouseDown(MouseEventArgs e)
        {
            (_, DragTab) = GetTabAtPoint(e.Location);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || DragTab == null || TabCount <= 1) return;

            int dragTabIndex = TabPages.IndexOf(DragTab);
            var (bDragTabIndex, _) = FindBackingTab(DragTab);

            var dragTabRect = GetTabRect(dragTabIndex);

            // Special-case for if there's 2 tabs. This is not the most readable thing, but hey... it works.
            var rightNotYet = dragTabIndex < TabCount - 1 &&
                              e.Location.X < dragTabRect.Left + GetTabRect(dragTabIndex + 1).Width;
            var leftNotYet = dragTabIndex > 0 &&
                             e.Location.X > GetTabRect(dragTabIndex - 1).Left + dragTabRect.Width;
            if ((TabCount == 2 && (rightNotYet || leftNotYet)) || (rightNotYet && leftNotYet))
            {
                return;
            }

            var (bNewTabIndex, newTab) = GetTabAtPoint(e.Location);
            if (bNewTabIndex == -1 || newTab == null || newTab == DragTab) return;

            int newTabIndex = TabPages.IndexOf(newTab);
            TabPages[dragTabIndex] = newTab;
            TabPages[newTabIndex] = DragTab;

            BackingTabList[bDragTabIndex].Tab = newTab;
            BackingTabList[bNewTabIndex].Tab = DragTab;

            SelectedTab = DragTab;

            base.OnMouseMove(e);
        }

        private (int backingTabIndex, TabPage? tab)
        GetTabAtPoint(Point position)
        {
            for (int i = 0; i < TabCount; i++)
            {
                if (GetTabRect(i).Contains(position))
                {
                    var tabPage = TabPages[i];
                    var (index, bTab) = FindBackingTab(tabPage);

                    if (index == -1 || bTab == null) return (-1, null);

                    return (index, tabPage);
                }
            }

            return (-1, null);
        }

        #endregion
    }
}

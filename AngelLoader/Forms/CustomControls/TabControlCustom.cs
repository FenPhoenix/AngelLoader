using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    internal class TabControlCustom : TabControl
    {
        internal class BackingTab
        {
            internal TabPage TabPage;
            internal bool Visible = true;
            internal BackingTab(TabPage tabPage) => TabPage = tabPage;
        }

        private TabPage? _dragTab;
        private readonly List<BackingTab> _backingTabList = new List<BackingTab>(Misc.TopRightTabsCount);

        public TabControlCustom()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        internal void AddTabsFull(List<TabPage> tabPages)
        {
            _backingTabList.Clear();

            foreach (TabPage tabPage in tabPages)
            {
                TabPages.Add(tabPage);
                _backingTabList.Add(new BackingTab(tabPage));
            }
        }

        internal (int Index, BackingTab BackingTab)
        FindBackingTab(TabPage tabPage, bool indexVisibleOnly = false)
        {
            for (int i = 0, vi = 0; i < _backingTabList.Count; i++)
            {
                BackingTab backingTab = _backingTabList[i];
                if (indexVisibleOnly && backingTab.Visible) vi++;
                if (backingTab.TabPage == tabPage) return (indexVisibleOnly ? vi : i, backingTab);
            }

            if (!DesignMode)
            {
                throw new Exception(nameof(FindBackingTab) + " couldn't find the specified tab page '" +
                                    // DOTNAME
                                    tabPage.Name +
                                    "'. That's not supposed to happen. All tab pages should always exist in the backing list.");
            }

            // To keep design mode tab selection happy
            return (-1, null)!;
        }

        internal void ShowTab(TabPage tabPage, bool show)
        {
            var (index, bt) = FindBackingTab(tabPage, indexVisibleOnly: true);
            if (index < 0 || bt == null) return;

            if (show)
            {
                bt.Visible = true;
                if (!TabPages.Contains(bt.TabPage)) TabPages.Insert(Math.Min(index, TabCount), bt.TabPage);
            }
            else
            {
                bt.Visible = false;
                if (TabPages.Contains(bt.TabPage)) TabPages.Remove(bt.TabPage);
            }
        }

        #region Tab reordering

        protected override void OnMouseDown(MouseEventArgs e)
        {
            (_, _dragTab) = GetTabAtPoint(e.Location);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || _dragTab == null || TabCount <= 1) return;

            int dragTabIndex = TabPages.IndexOf(_dragTab);
            var (bDragTabIndex, _) = FindBackingTab(_dragTab);

            Rectangle dragTabRect = GetTabRect(dragTabIndex);

            // Special-case for if there's 2 tabs. This is not the most readable thing, but hey... it works.
            bool rightNotYet = dragTabIndex < TabCount - 1 &&
                              e.Location.X < dragTabRect.Left + GetTabRect(dragTabIndex + 1).Width;
            bool leftNotYet = dragTabIndex > 0 &&
                             e.Location.X > GetTabRect(dragTabIndex - 1).Left + dragTabRect.Width;
            if ((TabCount == 2 && (rightNotYet || leftNotYet)) || (rightNotYet && leftNotYet))
            {
                return;
            }

            var (bNewTabIndex, newTab) = GetTabAtPoint(e.Location);
            if (bNewTabIndex == -1 || newTab == null || newTab == _dragTab) return;

            int newTabIndex = TabPages.IndexOf(newTab);
            TabPages[dragTabIndex] = newTab;
            TabPages[newTabIndex] = _dragTab;

            _backingTabList[bDragTabIndex].TabPage = newTab;
            _backingTabList[bNewTabIndex].TabPage = _dragTab!;

            SelectedTab = _dragTab;

            base.OnMouseMove(e);
        }

        private (int BackingTabIndex, TabPage? TabPage)
        GetTabAtPoint(Point position)
        {
            for (int i = 0; i < TabCount; i++)
            {
                if (GetTabRect(i).Contains(position))
                {
                    TabPage tabPage = TabPages[i];
                    var (index, backingTab) = FindBackingTab(tabPage);

                    return index == -1 || backingTab == null ? (-1, null) : (index, tabPage);
                }
            }

            return (-1, null);
        }

        #endregion
    }
}

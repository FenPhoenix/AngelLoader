using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class TabControlCustom : DarkTabControl
    {
        #region Private fields

        private sealed class BackingTab
        {
            internal TabPage TabPage;
            internal bool Visible = true;
            internal BackingTab(TabPage tabPage) => TabPage = tabPage;
        }

        private TabPage? _dragTab;
        // TODO: This is using a specific tab count number, but in theory this class is generic.
        // I'm only using it for the top-right tabs right now, but remove this capacity initializer if I use it
        // in another place.
        private readonly List<BackingTab> _backingTabList = new List<BackingTab>(DataClasses.TopRightTabsData.Count);

        #endregion

        [PublicAPI]
        public TabControlCustom() => SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

        #region API methods

        /// <summary>
        /// Removes all tabs and adds a set of new ones.
        /// </summary>
        /// <param name="tabPages"></param>
        [PublicAPI]
        public void SetTabsFull(List<TabPage> tabPages)
        {
            ClearTabsFull();

            foreach (TabPage tabPage in tabPages)
            {
                TabPages.Add(tabPage);
                _backingTabList.Add(new BackingTab(tabPage));
            }
        }

        [PublicAPI]
        public void ClearTabsFull()
        {
            if (TabCount > 0) TabPages.Clear();
            _backingTabList.Clear();
        }

        /// <summary>
        /// Shows or hides the specified <see cref="TabPage"/>.
        /// </summary>
        /// <param name="tabPage"></param>
        /// <param name="show"></param>
        [PublicAPI]
        public void ShowTab(TabPage tabPage, bool show)
        {
            var (index, bt) = FindBackingTab(tabPage, indexVisibleOnly: true);
            if (index < 0 || bt == null!) return;

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

        /// <summary>
        /// Returns the display index of the specified <see cref="TabPage"/>.
        /// </summary>
        /// <param name="tabPage"></param>
        /// <returns></returns>
        [PublicAPI]
        public int GetTabDisplayIndex(TabPage tabPage) => FindBackingTab(tabPage).Index;

        #endregion

        private (int Index, BackingTab BackingTab)
        FindBackingTab(TabPage tabPage, bool indexVisibleOnly = false)
        {
            for (int i = 0, vi = 0; i < _backingTabList.Count; i++)
            {
                BackingTab backingTab = _backingTabList[i];
                if (indexVisibleOnly && backingTab.Visible) vi++;
                if (backingTab.TabPage == tabPage) return (indexVisibleOnly ? vi : i, backingTab);
            }

#if DEBUG
            if (DesignMode) return (-1, null)!;
#endif

            // We should never get here! (unless we're in infernal-forsaken design mode...!)
            throw new InvalidOperationException("Can't find backing tab?!");
        }

        #region Tab reordering

        // TODO: BUG: We mis-order these in the following scenario:
        // If you grab a tab, move the mouse out of the tab area, then move it back onto the tabs area multiple
        // tabs away (ie, you drag the first tab straight to the end without going through the steps of bringing
        // it past each other tab in turn).
        // We could just move the tab if the mouse is ever moving horizontally, even if we're not vertically on
        // the tab bar.

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) (_, _dragTab) = GetTabAtPoint(e.Location);
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Fix: Ensure we don't start dragging a tab again after we've released the button.
            _dragTab = null;
            base.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Run the base event handler if we're not actually dragging a tab
            if (e.Button != MouseButtons.Left || _dragTab == null || TabCount <= 1)
            {
                base.OnMouseMove(e);
                return;
            }

            // If we are dragging a tab, don't run the handler, because we want to be "modal" and block so nothing
            // weird happens

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

            var (bNewTabIndex, newTab) = GetTabAtPoint(e.Location, xOnly: true);
            if (bNewTabIndex == -1 || newTab == null || newTab == _dragTab) return;

            int newTabIndex = TabPages.IndexOf(newTab);
            TabPages[dragTabIndex] = newTab;
            TabPages[newTabIndex] = _dragTab;

            _backingTabList[bDragTabIndex].TabPage = newTab;
            _backingTabList[bNewTabIndex].TabPage = _dragTab!;

            SelectedTab = _dragTab;
        }


        private (int BackingTabIndex, TabPage? TabPage)
        GetTabAtPoint(Point position, bool xOnly = false)
        {
            for (int i = 0; i < TabCount; i++)
            {
                var tabRect = GetTabRect(i);

                bool contains =
                    xOnly
                        ? position.X >= tabRect.X && position.X <= tabRect.Width + tabRect.X
                        : tabRect.Contains(position);

                if (contains)
                {
                    TabPage tabPage = TabPages[i];
                    var (index, backingTab) = FindBackingTab(tabPage);

                    return index == -1 || backingTab == null! ? (-1, null) : (index, tabPage);
                }
            }

            return (-1, null);
        }

        #endregion
    }
}

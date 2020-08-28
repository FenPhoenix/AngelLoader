using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed class TabControlCustom : TabControl
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

        /// <summary>
        /// <para>
        /// Unavailable. Don't try to use (you can't) or you will mess everything up (which is why you can't).
        /// </para>
        /// <para>
        /// Use <see cref="SetTabsFull"/> to set tabs, or <see cref="ClearTabsFull"/> to remove all tabs.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        [PublicAPI]
        public new object TabPages => throw new InvalidOperationException();

#if !DEBUG
        /// <summary>
        /// <para>
        /// Unavailable. Don't try to use (you can't) or you will mess everything up (which is why you can't).
        /// </para>
        /// <para>
        /// Use <see cref="SetTabsFull"/> to set tabs, or <see cref="ClearTabsFull"/> to remove all tabs.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        [PublicAPI]
        public new object Controls => throw new InvalidOperationException();
#endif

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
                base.TabPages.Add(tabPage);
                _backingTabList.Add(new BackingTab(tabPage));
            }
        }

        [PublicAPI]
        public void ClearTabsFull()
        {
            if (TabCount > 0) base.TabPages.Clear();
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
                if (!base.TabPages.Contains(bt.TabPage)) base.TabPages.Insert(Math.Min(index, TabCount), bt.TabPage);
            }
            else
            {
                bt.Visible = false;
                if (base.TabPages.Contains(bt.TabPage)) base.TabPages.Remove(bt.TabPage);
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

            int dragTabIndex = base.TabPages.IndexOf(_dragTab);
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

            int newTabIndex = base.TabPages.IndexOf(newTab);
            base.TabPages[dragTabIndex] = newTab;
            base.TabPages[newTabIndex] = _dragTab;

            _backingTabList[bDragTabIndex].TabPage = newTab;
            _backingTabList[bNewTabIndex].TabPage = _dragTab!;

            SelectedTab = _dragTab;
        }

        private (int BackingTabIndex, TabPage? TabPage)
        GetTabAtPoint(Point position)
        {
            for (int i = 0; i < TabCount; i++)
            {
                if (GetTabRect(i).Contains(position))
                {
                    TabPage tabPage = base.TabPages[i];
                    var (index, backingTab) = FindBackingTab(tabPage);

                    return index == -1 || backingTab == null! ? (-1, null) : (index, tabPage);
                }
            }

            return (-1, null);
        }

        #endregion
    }
}

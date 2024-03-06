using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

/*
TODO(DarkTabControlCustom - designability):
-Set it up so that adding tabs can be done normally:
In OnControlAdded()/OnControlRemoved(), add/remove from backing list, but have a bool guard check for that.
When we show/hide tabs, set the bool so that OnControlAdded()/OnControlRemoved() don't do anything while we
update the backing list ourselves.
*/
public sealed class DarkTabControl : TabControl, IDarkable, IOptionallyLazyTabControl
{
    #region Private fields

    internal TabPage? DragTab { get; private set; }

    private List<BackingTab> _backingTabList = new(0);

    #region Back up backing tabs

    /*
    The user can drag tabs horizontally in the same move as they're trying to drag them into the other tab
    control. In that case, if the user commits the move, we want to revert the horizontal movement they did
    and put the tab in the new control at the same relative position as it was before the user started the
    drag. The easiest way to do that is to simply save the backing tabs list before the move and then restore
    it if and only if the user commits the between-tab-control move.
    */

    private List<BackingTab>? _backedUpBackingTabs;
    private TabPage? _backedUpNearestTabPage;

    private void BackUpTempDragData()
    {
        _backedUpBackingTabs = new List<BackingTab>(_backingTabList.Count);
        foreach (BackingTab backingTab in _backingTabList)
        {
            _backedUpBackingTabs.Add(new BackingTab(backingTab.TabPage) { VisibleIn = backingTab.VisibleIn });
        }
        if (TabCount > 1 && SelectedIndex > 0)
        {
            _backedUpNearestTabPage = TabPages[GetNearestIndex_NoBoundsChecks()];
        }
    }

    internal void RestoreBackedUpBackingTabs()
    {
        if (_backedUpBackingTabs == null || _backedUpBackingTabs.Count != _backingTabList.Count)
        {
            return;
        }

        for (int i = 0; i < _backingTabList.Count; i++)
        {
            BackingTab backingTab = _backingTabList[i];
            BackingTab backedUpBackingTab = _backedUpBackingTabs[i];
            backingTab.TabPage = backedUpBackingTab.TabPage;
            backingTab.VisibleIn = backedUpBackingTab.VisibleIn;
        }
        _backedUpBackingTabs = null;
    }

    internal void ResetTempDragData()
    {
        _backedUpBackingTabs = null;
        _backedUpNearestTabPage = null;
    }

    #endregion

    /// <summary>
    /// Use this to use an external list rather than the internal one.
    /// Use when you want multiple controls to use the same list.
    /// </summary>
    /// <param name="list"></param>
    internal void SetBackingList(List<BackingTab> list) => _backingTabList = list;

    private WhichTabControl _whichTabControl = WhichTabControl.Top;
    internal void SetWhich(WhichTabControl value) => _whichTabControl = value;

    #endregion

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            SetStyle(ControlStyles.UserPaint, _darkModeEnabled);

            if (EnableScrollButtonsRefreshHack && Visible)
            {
                // Utterly repellent hack to force the scroll buttons to redraw, because they're one of those
                // "normal control but locked inside another and we even suppress its messages, ha-ha" things.
                Rectangle tabBarVisibleRect = GetTabBarRect();
                int totalTabsWidth = 0;
                for (int i = 0; i < TabCount; i++)
                {
                    totalTabsWidth += GetTabRect(i).Width;
                }

                // +1 because the scroll buttons' appearance trigger is not exact
                if (totalTabsWidth > tabBarVisibleRect.Width + 1)
                {
                    Hide();
                    Show();
                }
            }

            Refresh();
        }
    }

    public event MouseEventHandler? MouseDragCustom;

    [PublicAPI]
    [DefaultValue(false)]
    public bool AllowReordering { get; set; }

    [PublicAPI]
    [DefaultValue(false)]
    public bool EnableScrollButtonsRefreshHack { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Control[] BackingTabPagesAsControls
    {
        get
        {
            var ret = new Control[_backingTabList.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = _backingTabList[i].TabPage;
            }
            return ret;
        }
    }

    // Double-buffering prevents flickering when mouse is moved over in dark mode
    public DarkTabControl() => SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

    #region Private methods

    internal (int Index, BackingTab BackingTab)
    FindBackingTab(List<BackingTab> backingTabs, TabPage tabPage, bool indexVisibleOnly = false)
    {
        for (int i = 0, vi = 0; i < backingTabs.Count; i++)
        {
            BackingTab backingTab = backingTabs[i];
            if (indexVisibleOnly && VisibleInEqualsWhich(backingTab.VisibleIn, _whichTabControl)) vi++;
            if (backingTab.TabPage == tabPage) return (indexVisibleOnly ? vi : i, backingTab);
        }

        // We should never get here! (unless we're in infernal-forsaken design mode...!)
        throw new InvalidOperationException("Can't find backing tab?!");

        static bool VisibleInEqualsWhich(FMTabVisibleIn visibleIn, WhichTabControl which) =>
            (visibleIn == FMTabVisibleIn.Top && which == WhichTabControl.Top) ||
            (visibleIn == FMTabVisibleIn.Bottom && which == WhichTabControl.Bottom);
    }

    private (int BackingTabIndex, TabPage? TabPage)
    GetTabAtPoint(Point position, bool xOnly = false)
    {
        for (int i = 0; i < TabCount; i++)
        {
            Rectangle tabRect = GetTabRect(i);

            bool contains =
                xOnly
                    ? position.X >= tabRect.X && position.X <= tabRect.Width + tabRect.X
                    : tabRect.Contains(position);

            if (contains)
            {
                TabPage tabPage = TabPages[i];
                var (index, backingTab) = FindBackingTab(_backingTabList, tabPage);

                return index == -1 || backingTab == null! ? (-1, null) : (index, tabPage);
            }
        }

        return (-1, null);
    }

    #endregion

    #region Event overrides

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_darkModeEnabled)
        {
            Graphics g = e.Graphics;

            if (Parent != null)
            {
                // Fill background behind the control (shows up behind tabs)
                using var b = new SolidBrush(Parent.BackColor);
                g.FillRectangle(b, ClientRectangle);
            }

            if (TabPages.Count > 0)
            {
                Rectangle firstTabRect = GetTabRect(0);
                var pageRect = new Rectangle(
                    ClientRectangle.X,
                    ClientRectangle.Y + firstTabRect.Y + firstTabRect.Height,
                    (ClientRectangle.Width - firstTabRect.X) - 1,
                    (ClientRectangle.Height - (firstTabRect.Y + firstTabRect.Height + 1)) - 1);

                // Fill tab page background (shows up as a small border around the tab page)
                // (our actual area is slightly larger than gets filled by simply setting BackColor)
                g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, pageRect);

                // Draw tab page border
                g.DrawRectangle(DarkColors.LighterBackgroundPen, pageRect);

                TabPage? selectedTab = SelectedTab;

                // Paint tabs
                for (int i = 0; i < TabPages.Count; i++)
                {
                    TabPage tabPage = TabPages[i];
                    Rectangle tabRect = GetTabRect(i);

                    bool focused = selectedTab == tabPage;

                    if (focused)
                    {
                        tabRect = tabRect with { Y = tabRect.Y - 2, Height = tabRect.Height + 2 };
                    }

                    SolidBrush backColorBrush = focused
                        ? DarkColors.LightBackgroundBrush
                        : Enabled &&
                          // Prevent highlighting when the mouse is over the scroll arrows
                          Control.FromHandle(Native.WindowFromPoint(Native.GetCursorPosition_Fast())) != null &&
                          tabRect.Contains(this.ClientCursorPos())
                            ? DarkColors.Fen_HotTabBackgroundBrush
                            : DarkColors.Fen_DeselectedTabBackgroundBrush;

                    // Draw tab background
                    g.FillRectangle(backColorBrush, tabRect);

                    // Draw tab border
                    g.DrawRectangle(DarkColors.LighterBackgroundPen, tabRect);

                    bool thisTabHasImage = ImageList?.Images?.Empty == false &&
                                           tabPage.ImageIndex > -1;

                    #region Image

                    // Don't try to be clever and complicated and check for missing indexes etc.
                    // That would be a bug as far as I'm concerned, so just let it crash in that case.

                    if (thisTabHasImage)
                    {
                        int textWidth = TextRenderer.MeasureText(
                            g,
                            tabPage.Text,
                            Font
                        ).Width;

                        Image image = ImageList!.Images![tabPage.ImageIndex];

                        int leftMargin = tabRect.Width - textWidth;

                        var imgPoint = new Point(
                            tabRect.Left + 1 + ((leftMargin / 2) - (image.Width / 2)),
                            focused ? 2 : 4
                        );
                        g.DrawImage(image, imgPoint.X, imgPoint.Y);
                    }

                    #endregion

                    TextFormatFlags textHorzAlign = thisTabHasImage
                        ? TextFormatFlags.Right
                        : TextFormatFlags.HorizontalCenter;

                    // No TextAlign property, so leave constant
                    TextFormatFlags textFormat =
                        textHorzAlign |
                        TextFormatFlags.VerticalCenter |
                        TextFormatFlags.EndEllipsis |
                        /*
                        @DarkModeNote(DarkTabControl/Mnemonic ampersands):
                        In classic mode, putting a single ampersand into a tab's text will still display
                        it as a single ampersand, but will mess up the length-and-x-position slightly.
                        Putting a double-ampersand in will also display as a single ampersand (indicating
                        that mnemonics are active), but the length/x-position is still the same. Removing
                        the ampersand (replacing it with a different char like '+') fixes the length/x-
                        positioning.
                        I mean whatevs I guess, but note it for the future... maybe we turn off NoPrefix
                        here and just override Text and escape all ampersands before we set it, just to be
                        correct.
                        */
                        TextFormatFlags.NoPrefix |
                        TextFormatFlags.NoClipping;

                    var textRect =
                        thisTabHasImage
                            ? new Rectangle(
                                tabRect.X - 2,
                                tabRect.Y + (focused ? 0 : 1),
                                tabRect.Width,
                                tabRect.Height - 1
                            )
                            : new Rectangle(
                                tabRect.X + 1,
                                tabRect.Y + (focused ? 0 : 1),
                                tabRect.Width,
                                tabRect.Height - 1
                            );

                    Color textColor = Enabled ? DarkColors.LightText : DarkColors.DisabledText;

                    TextRenderer.DrawText(g, tabPage.Text, Font, textRect, textColor, textFormat);
                }
            }
        }

        base.OnPaint(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (DesignMode || !AllowReordering)
        {
            base.OnMouseDown(e);
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            (_, DragTab) = GetTabAtPoint(e.Location);
            if (DragTab != null) BackUpTempDragData();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (DesignMode || !AllowReordering)
        {
            base.OnMouseUp(e);
            return;
        }

        // Do this first so DragTab is still valid when we handle drag-and-drop between tab controls
        base.OnMouseUp(e);

        // Fix: Ensure we don't start dragging a tab again after we've released the button.
        DragTab = null;
        _backedUpBackingTabs = null;
    }

    private void InvokeMouseDragCustomIfNeeded(MouseEventArgs e)
    {
        Rectangle tabBarExpanded = GetTabBarRect();
        tabBarExpanded.Inflate(4, 16);
        if (!tabBarExpanded.Contains(e.Location))
        {
            MouseDragCustom?.Invoke(this, e);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (DesignMode || !AllowReordering)
        {
            base.OnMouseMove(e);
            return;
        }

        // Run the base event handler if we're not actually dragging a tab
        if (e.Button != MouseButtons.Left || DragTab == null)
        {
            base.OnMouseMove(e);
            return;
        }

        if (TabCount <= 1)
        {
            InvokeMouseDragCustomIfNeeded(e);
            base.OnMouseMove(e);
            return;
        }

        // If we are dragging a tab, don't run the normal handler, because we want to be "modal" and block so
        // nothing weird happens
        InvokeMouseDragCustomIfNeeded(e);

        int dragTabIndex = TabPages.IndexOf(DragTab);
        var (bDragTabIndex, _) = FindBackingTab(_backingTabList, DragTab);

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

        // If the user has moved the mouse off of the tab bar vertically, still stay in the move. This prevents
        // a mis-ordering bug if the user drags a tab off the bar then back onto the bar at a different position.
        var (bNewTabIndex, newTab) = GetTabAtPoint(e.Location, xOnly: true);
        if (bNewTabIndex == -1 || newTab == null || newTab == DragTab) return;

        int newTabIndex = TabPages.IndexOf(newTab);

        // Handle the case where the tab is moving more than one position in one go.
        // The easy way would be to insert/remove, but that results in terrible flickering for the TabPages
        // collection, and suspend/resume doesn't fix it either. So just move everything over manually.

        if (newTabIndex > dragTabIndex)
        {
            for (int i = dragTabIndex; i < newTabIndex; i++)
            {
                TabPages[i] = TabPages[i + 1];
            }
            for (int i = bDragTabIndex; i < bNewTabIndex; i++)
            {
                _backingTabList[i].TabPage = _backingTabList[i + 1].TabPage;
                _backingTabList[i].VisibleIn = _backingTabList[i + 1].VisibleIn;
            }
        }
        else
        {
            for (int i = dragTabIndex - 1; i >= newTabIndex; i--)
            {
                TabPages[i + 1] = TabPages[i];
            }
            for (int i = bDragTabIndex - 1; i >= bNewTabIndex; i--)
            {
                _backingTabList[i + 1].TabPage = _backingTabList[i].TabPage;
                _backingTabList[i + 1].VisibleIn = _backingTabList[i].VisibleIn;
            }
        }

        TabPages[newTabIndex] = DragTab;
        _backingTabList[bNewTabIndex].TabPage = DragTab;

        SelectedTab = DragTab;

        // Otherwise the first control within the tab page gets selected
        SelectedTab.Focus();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// <para>Clears images and adds a set of new ones.</para>
    /// This should always be called no matter what, because there's a ridiculous bug where if you add images
    /// through the designer, they'll just always be rendered in like 4-bit quality even though the color
    /// depth is set to 32 or whatever else.
    /// </summary>
    /// <param name="images"></param>
    [PublicAPI]
    public void SetImages(Image[] images)
    {
        if (ImageList == null) return;

        ImageList.Images.Clear();
        ImageList.Images.AddRange(images);
    }

    // @PERF_TODO(SetTabsFull/Show tab):
    // We could combine these to only add the tabs to TabPages that are going to be visible, rather than adding
    // them all and then potentially removing some again.

    private bool _doneTabFixHack;
    /// <summary>
    /// Removes all tabs and adds a set of new ones.
    /// </summary>
    /// <param name="tabPages"></param>
    [PublicAPI]
    public void SetTabsFull(TabPage[] tabPages)
    {
        /*
        Create handle before adding tabs to prevent the following:
        -You start the app in dark mode and with the FM tabs area(s) hidden
        -You show the FM tabs area(s)
        -The tabs are all the same width, and if you switch to light mode, they have a crappy bold font instead
         of the intended one
        */
        if (!_doneTabFixHack)
        {
            _ = Handle;
            _doneTabFixHack = true;
        }

        if (TabCount > 0) TabPages.Clear();
        _backingTabList.Clear();

        _backingTabList.Capacity = tabPages.Length;

        foreach (TabPage tabPage in tabPages)
        {
            TabPages.Add(tabPage);
            _backingTabList.Add(new BackingTab(tabPage));
        }
    }

    /// <summary>
    /// Shows or hides the specified <see cref="TabPage"/>.
    /// </summary>
    /// <param name="tabPage"></param>
    /// <param name="show"></param>
    [PublicAPI]
    public void ShowTab(TabPage tabPage, bool show)
    {
        var (index, bt) = FindBackingTab(_backingTabList, tabPage, indexVisibleOnly: true);
        if (index < 0 || bt == null!) return;

        if (show)
        {
            bt.VisibleIn = _whichTabControl == WhichTabControl.Bottom ? FMTabVisibleIn.Bottom : FMTabVisibleIn.Top;
            if (!TabPages.Contains(bt.TabPage)) TabPages.Insert(Math.Min(index, TabCount), bt.TabPage);
        }
        else
        {
            bt.VisibleIn = FMTabVisibleIn.None;
            if (TabPages.Contains(bt.TabPage))
            {
                Control? parent = Parent;
                if (_backedUpNearestTabPage != null && TabPages.Contains(_backedUpNearestTabPage))
                {
                    try
                    {
                        parent?.SuspendDrawing();
                        SelectedTab = _backedUpNearestTabPage;
                        TabPages.Remove(bt.TabPage);
                    }
                    finally
                    {
                        parent?.ResumeDrawing();
                    }
                }
                else if (TabPages.Count > 1 && SelectedIndex > 0 && SelectedTab == bt.TabPage)
                {
                    try
                    {
                        parent?.SuspendDrawing();
                        SelectedTab = TabPages[GetNearestIndex_NoBoundsChecks()];
                        TabPages.Remove(bt.TabPage);
                    }
                    finally
                    {
                        parent?.ResumeDrawing();
                    }
                }
                else
                {
                    TabPages.Remove(bt.TabPage);
                }
                _backedUpNearestTabPage = null;
                if (TabCount > 0) SelectedTab.Focus();
            }
        }
    }

    private int GetNearestIndex_NoBoundsChecks()
    {
        return SelectedIndex == TabCount - 1 ? SelectedIndex - 1 : SelectedIndex + 1;
    }

    public Rectangle GetTabBarRect() =>
        TabCount == 0
            ? Rectangle.Empty
            : new Rectangle(0, 0, Width, GetTabRect(0).Height);

    #endregion
}

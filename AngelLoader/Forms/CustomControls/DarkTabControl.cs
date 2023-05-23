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
public sealed class DarkTabControl : TabControl, IDarkable
{
    #region Private fields

    private sealed class BackingTab
    {
        internal TabPage TabPage;
        internal bool Visible = true;
        internal BackingTab(TabPage tabPage) => TabPage = tabPage;
    }

    private TabPage? _dragTab;

    private readonly List<BackingTab> _backingTabList = new(0);

    private Font? _originalFont;

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

            SetStyle(
                // Double-buffering prevents flickering when mouse is moved over in dark mode
                ControlStyles.UserPaint,
                _darkModeEnabled);

            /*
            @TabFont: This was probably from when we were having wrong-font issues in the tab control.
            I'm guessing this must have fixed it or at least was an attempt to fix it(?), but removing it doesn't
            seem to cause any problems now (on Windows 10 at least). But we had Win7 before when we had the
            problem, so I'm not 100% sure. Leaving it in for now.
            */
            if (_darkModeEnabled)
            {
                _originalFont ??= (Font)Font.Clone();
            }
            else
            {
                if (_originalFont != null) Font = (Font)_originalFont.Clone();
            }

            Refresh();
        }
    }

    [PublicAPI]
    [DefaultValue(false)]
    public bool AllowReordering { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Control[] BackingTabPages
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

    public DarkTabControl() => SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

    #region Private methods

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
        if (DesignMode) return (-1, null!);
#endif

        // We should never get here! (unless we're in infernal-forsaken design mode...!)
        throw new InvalidOperationException("Can't find backing tab?!");
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
                var (index, backingTab) = FindBackingTab(tabPage);

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
                        : Enabled && tabRect.Contains(this.ClientCursorPos())
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
                        // @DarkModeNote(DarkTabControl/Mnemonic ampersands):
                        // In classic mode, putting a single ampersand into a tab's text will still display
                        // it as a single ampersand, but will mess up the length-and-x-position slightly.
                        // Putting a double-ampersand in will also display as a single ampersand (indicating
                        // that mnemonics are active), but the length/x-position is still the same. Removing
                        // the ampersand (replacing it with a different char like '+') fixes the length/x-
                        // positioning.
                        // I mean whatevs I guess, but note it for the future... maybe we turn off NoPrefix
                        // here and just override Text and escape all ampersands before we set it, just to be
                        // correct.
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

        if (e.Button == MouseButtons.Left) (_, _dragTab) = GetTabAtPoint(e.Location);
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (DesignMode || !AllowReordering)
        {
            base.OnMouseUp(e);
            return;
        }

        // Fix: Ensure we don't start dragging a tab again after we've released the button.
        _dragTab = null;
        base.OnMouseUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (DesignMode || !AllowReordering)
        {
            base.OnMouseMove(e);
            return;
        }

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

        // If the user has moved the mouse off of the tab bar vertically, still stay in the move. This prevents
        // a mis-ordering bug if the user drags a tab off the bar then back onto the bar at a different position.
        var (bNewTabIndex, newTab) = GetTabAtPoint(e.Location, xOnly: true);
        if (bNewTabIndex == -1 || newTab == null || newTab == _dragTab) return;

        int newTabIndex = TabPages.IndexOf(newTab);
        TabPages[dragTabIndex] = newTab;
        TabPages[newTabIndex] = _dragTab;

        _backingTabList[bDragTabIndex].TabPage = newTab;
        _backingTabList[bNewTabIndex].TabPage = _dragTab;

        SelectedTab = _dragTab;

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

    /// <summary>
    /// Removes all tabs and adds a set of new ones.
    /// </summary>
    /// <param name="tabPages"></param>
    [PublicAPI]
    public void SetTabsFull(TabPage[] tabPages)
    {
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

    public Rectangle GetTabBarRect() => new(0, 0, Width, GetTabRect(0).Height);

    #endregion
}

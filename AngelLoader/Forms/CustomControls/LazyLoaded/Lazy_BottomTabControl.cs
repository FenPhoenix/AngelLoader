using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_BottomTabControl : IDarkable, IOptionallyLazyTabControl
{
    internal bool Constructed { get; private set; }

    private readonly MainForm _owner;

    private DarkTabControl _tabControl = null!;
    public DarkTabControl TabControl
    {
        get
        {
            Construct();
            return _tabControl;
        }
    }

    private bool _enabled = true;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!Constructed) return;

            TabControl.DarkModeEnabled = value;
        }
    }

    public event TabControlEventHandler? Selected;

    internal Lazy_BottomTabControl(MainForm owner) => _owner = owner;

    // NOTE: We construct if we have any tab controls assigned to us, even if we're hidden
    // That's not a huge deal because the tab control itself is relatively lightweight compared to the tab pages'
    // contents and those are still unloaded until they show.
    internal void Construct()
    {
        if (Constructed) return;

        var container = _owner.BottomSplitContainer.Panel2;

        _tabControl = new DarkTabControl
        {
            Tag = LoadType.Lazy,

            AllowReordering = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            EnableScrollButtonsRefreshHack = true,
            Size = new Size(container.Width - 16, container.Height + 1),
        };

        _tabControl.SetWhich(WhichTabControl.Bottom);
        _tabControl.SetBackingList(_owner._backingFMTabs);

        // Handle hack here instead, because it needs to be for whatever finicky goddamn reason
        _ = _tabControl.Handle;
        // Dark mode set MUST come AFTER the handle hack, otherwise it doesn't work!
        _tabControl.DarkModeEnabled = _darkModeEnabled;

        _tabControl.MouseClick += _owner.BottomFMTabsBar_MouseClick;
        container.MouseClick += _owner.BottomFMTabsBar_MouseClick;
        _owner.BottomFMTabsEmptyMessageLabel.MouseClick += _owner.BottomFMTabsBar_MouseClick;

        _tabControl.Selected += TabControl_Selected;
        _tabControl.MouseDragCustom += _owner.Lazy_BottomTabControl_MouseDragCustom;
        _tabControl.MouseUp += _owner.Lazy_BottomTabControl_MouseUp;
        _tabControl.VisibleChanged += TabControl_VisibleChanged;

        container.Controls.Add(_tabControl);

        _tabControl.Enabled = _enabled;

        Constructed = true;
    }

    public void ShowTab(TabPage tabPage, bool show)
    {
        if (show)
        {
            Construct();
            _tabControl.ShowTab(tabPage, true);
        }
        else if (Constructed)
        {
            _tabControl.ShowTab(tabPage, false);
        }
    }

    public bool Enabled
    {
        get => Constructed ? _tabControl.Enabled : _enabled;
        set
        {
            if (Constructed)
            {
                _tabControl.Enabled = value;
            }
            else
            {
                _enabled = value;
            }
        }
    }

    public int TabCount => Constructed ? _tabControl.TabCount : 0;

    public TabPage? SelectedTab
    {
        get => Constructed ? _tabControl.SelectedTab : null;
        set
        {
            if (!Constructed) return;
            _tabControl.SelectedTab = value;
        }
    }

    public bool Focused => Constructed && _tabControl.Focused;

    public bool TabPagesContains(TabPage tabPage) => Constructed && _tabControl.TabPages.Contains(tabPage);

    public Rectangle GetTabRect(int index) => Constructed ? _tabControl.GetTabRect(index) : Rectangle.Empty;

    public Rectangle GetTabBarRect() => Constructed ? _tabControl.GetTabBarRect() : Rectangle.Empty;

    public Rectangle ClientRectangle => Constructed ? _tabControl.ClientRectangle : Rectangle.Empty;

    public Point ClientCursorPos() => Constructed ? _tabControl.ClientCursorPos() : Point.Empty;

    public int Width => Constructed ? _tabControl.Width : 0;

    public int Height => Constructed ? _tabControl.Height : 0;

    public int SelectedIndex => Constructed ? _tabControl.SelectedIndex : -1;

    public bool Visible
    {
        get => Constructed && _tabControl.Visible;
        set
        {
            if (value)
            {
                Construct();
                _tabControl.Show();
            }
            else if (Constructed)
            {
                _tabControl.Hide();
            }
        }
    }

    public void DrawToBitmap(Bitmap bitmap, Rectangle targetBounds)
    {
        if (!Constructed) return;
        _tabControl.DrawToBitmap(bitmap, targetBounds);
    }

    public Point PointToClient_Fast(Point point) => Constructed ? _tabControl.PointToClient_Fast(point) : Point.Empty;

    public void RestoreBackedUpBackingTabs()
    {
        if (!Constructed) return;
        _tabControl.RestoreBackedUpBackingTabs();
    }

    public void ResetTempDragData()
    {
        if (!Constructed) return;
        _tabControl.ResetTempDragData();
    }

    public TabPage? DragTab => Constructed ? _tabControl.DragTab : null;

    private void TabControl_Selected(object sender, TabControlEventArgs e) => Selected?.Invoke(_tabControl, e);

    private void TabControl_VisibleChanged(object sender, System.EventArgs e)
    {
        if (_tabControl is { Visible: true, SelectedTab: Lazy_TabsBase lazyTab } &&
            !_owner.BottomSplitContainer.FullScreen)
        {
            lazyTab.ConstructWithSuspendResume();
        }
    }
}

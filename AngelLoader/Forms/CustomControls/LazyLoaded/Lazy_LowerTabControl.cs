using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_LowerTabControl : IDarkable
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

    internal DarkButton MenuButton = null!;
    internal DarkArrowButton CollapseButton = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!Constructed) return;

            MenuButton.DarkModeEnabled = value;
            CollapseButton.DarkModeEnabled = value;
            TabControl.DarkModeEnabled = value;
        }
    }

    public event TabControlEventHandler? Selected;

    public Lazy_LowerTabControl(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (Constructed) return;

        var container = _owner.LowerSplitContainer.Panel2;

        _owner.LowerSplitContainer.Panel2Collapsed = false;

        _tabControl = new DarkTabControl
        {
            Tag = LoadType.Lazy,

            AllowReordering = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            EnableScrollButtonsRefreshHack = true,
            Size = new Size(container.Width - 16, container.Height + 1)
        };

        _tabControl.SetWhich(WhichTabControl.Bottom);
        _tabControl.SetBackingList(_owner._backingFMTabs);

        // Handle hack here instead, because it needs to be for whatever finicky goddamn reason
        _ = _tabControl.Handle;
        // Dark mode set MUST come AFTER the handle hack, otherwise it doesn't work!
        _tabControl.DarkModeEnabled = _darkModeEnabled;

        _tabControl.MouseClick += _owner.LowerFMTabsBar_MouseClick;
        container.MouseClick += _owner.LowerFMTabsBar_MouseClick;

        _tabControl.Selected += TabControl_Selected;
        _tabControl.MouseDragCustom += _owner.Lazy_LowerTabControl_MouseDragCustom;
        _tabControl.MouseUp += _owner.Lazy_LowerTabControl_MouseUp;
        _tabControl.VisibleChanged += TabControl_VisibleChanged;

        MenuButton = new DarkButton
        {
            Tag = LoadType.Lazy,

            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatAppearance = { BorderSize = 0 },
            FlatStyle = FlatStyle.Flat,
            Location = new Point(container.Width - 18, 0),
            Size = new Size(18, 20),
            TabIndex = 1,

            DarkModeEnabled = _darkModeEnabled
        };
        MenuButton.Click += _owner.LowerFMTabsMenuButton_Click;
        MenuButton.PaintCustom += _owner.FMTabsMenuButton_Paint;

        CollapseButton = new DarkArrowButton
        {
            Tag = LoadType.Lazy,

            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
            ArrowDirection = Direction.Right,
            FlatAppearance = { BorderSize = 0 },
            FlatStyle = FlatStyle.Flat,
            Location = new Point(container.Width - 18, 20),
            Size = new Size(18, container.Height - 20),
            TabIndex = 2,

            DarkModeEnabled = _darkModeEnabled
        };
        CollapseButton.Click += _owner.LowerFMTabsCollapseButton_Click;

        container.Controls.Add(MenuButton);
        container.Controls.Add(CollapseButton);
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
        else
        {
            if (!Constructed) return;
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

    public TabPage? SelectedTab => Constructed ? _tabControl.SelectedTab : null;

    public TabPage? DragTab => Constructed ? _tabControl.DragTab : null;

    private void TabControl_Selected(object sender, TabControlEventArgs e) => Selected?.Invoke(_tabControl, e);

    private void TabControl_VisibleChanged(object sender, System.EventArgs e)
    {
        if (_tabControl is { Visible: true, SelectedTab: Lazy_TabsBase lazyTab })
        {
            lazyTab.ConstructWithSuspendResume();
        }
    }
}

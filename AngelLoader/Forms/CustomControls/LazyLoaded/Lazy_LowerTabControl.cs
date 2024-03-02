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

    public Lazy_LowerTabControl(MainForm owner) => _owner = owner;

    private void Construct()
    {
        if (Constructed) return;

        var container = _owner.LowerSplitContainer.Panel2;

        _tabControl = new DarkTabControl
        {
            Tag = LoadType.Lazy,

            AllowReordering = true,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            EnableScrollButtonsRefreshHack = true,
            Size = container.Size,
        };

        _tabControl.SetWhich(WhichTabControl.Bottom);

        // Handle hack here instead, because it needs to be for whatever finicky goddamn reason
        _ = _tabControl.Handle;
        _tabControl.DarkModeEnabled = _darkModeEnabled;

        _tabControl.MouseClick += _owner.LowerFMTabsBar_MouseClick;
        _owner.LowerSplitContainer.Panel2.MouseClick += _owner.LowerFMTabsBar_MouseClick;

        _tabControl.Selected += TabControl_Selected;
        _tabControl.MouseDragCustom += _owner.Lazy_LowerTabControl_MouseDragCustom;
        _tabControl.MouseUp += _owner.Lazy_LowerTabControl_MouseUp;
        _tabControl.VisibleChanged += TabControl_VisibleChanged;

        Constructed = true;
    }

    private void TabControl_Selected(object sender, TabControlEventArgs e) => Selected?.Invoke(_tabControl, e);

    public event TabControlEventHandler? Selected;

    private void TabControl_VisibleChanged(object sender, System.EventArgs e)
    {
        if (_tabControl is { Visible: true, SelectedTab: Lazy_TabsBase lazyTab })
        {
            lazyTab.ConstructWithSuspendResume();
        }
    }
}

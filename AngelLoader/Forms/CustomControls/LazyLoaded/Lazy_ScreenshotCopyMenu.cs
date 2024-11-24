using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class Lazy_ScreenshotCopyMenu : IDarkable
{
    private bool _constructed;

    private readonly ScreenshotsTabPage _owner;

    internal Lazy_ScreenshotCopyMenu(ScreenshotsTabPage owner) => _owner = owner;

    private DarkContextMenu _menu = null!;
    internal DarkContextMenu Menu
    {
        get
        {
            Construct();
            return _menu;
        }
    }

    private ToolStripMenuItemCustom CopyMenuItem = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            _menu.DarkModeEnabled = _darkModeEnabled;
        }
    }

    private void Construct()
    {
        if (_constructed) return;

        _menu = new DarkContextMenu(_owner);
        _menu.Items.AddRange(
            CopyMenuItem = new ToolStripMenuItemCustom { ShortcutKeys = Keys.Control | Keys.C }
        );

        _menu.Opening += _owner.CopyMenu_Opening;
        _menu.ItemClicked += _owner.CopyMenu_ItemClicked;

        _menu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;
        CopyMenuItem.Text = LText.Global.Copy;
    }
}

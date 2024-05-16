using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class MainLLMenu : IDarkable
{
    #region Backing fields

    private bool _constructed;
    private bool _scanAllFMsMenuItemEnabled;

    #endregion

    private readonly MainForm _owner;

    internal MainLLMenu(MainForm owner) => _owner = owner;

    #region Menu items

    private DarkContextMenu _menu = null!;
    internal DarkContextMenu Menu
    {
        get
        {
            Construct();
            return _menu;
        }
    }

    private ToolStripMenuItemCustom GameVersionsMenuItem = null!;
    private ToolStripMenuItemCustom ImportMenuItem = null!;
    internal ToolStripMenuItemCustom ImportFromDarkLoaderMenuItem = null!;
    internal ToolStripMenuItemCustom ImportFromFMSelMenuItem = null!;
    internal ToolStripMenuItemCustom ImportFromNewDarkLoaderMenuItem = null!;

    internal ToolStripMenuItemCustom ScanAllFMsMenuItem = null!;

    internal ToolStripMenuItemCustom SettingsMenuItem = null!;
    internal ToolStripMenuItemCustom CheckForUpdatesMenuItem = null!;

    private ToolStripMenuItemCustom ViewHelpFileMenuItem = null!;
    private ToolStripMenuItemCustom AboutMenuItem = null!;
    private ToolStripMenuItemCustom ExitMenuItem = null!;

    #endregion

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
        _menu.Items.AddRange(new ToolStripItem[]
        {
            GameVersionsMenuItem = new ToolStripMenuItemCustom(),
            new ToolStripSeparator(),
            ImportMenuItem = new ToolStripMenuItemCustom(),
            new ToolStripSeparator(),
            ScanAllFMsMenuItem = new ToolStripMenuItemCustom(),
            SettingsMenuItem = new ToolStripMenuItemCustom(),
            CheckForUpdatesMenuItem = new ToolStripMenuItemCustom(),
            new ToolStripSeparator(),
            ViewHelpFileMenuItem = new ToolStripMenuItemCustom { ShortcutKeys = Keys.F1 },
            AboutMenuItem = new ToolStripMenuItemCustom(),
            new ToolStripSeparator(),
            ExitMenuItem = new ToolStripMenuItemCustom { ShortcutKeys = Keys.Alt | Keys.F4 },
        });

        ImportMenuItem.DropDown.Items.AddRange(new ToolStripItem[]
        {
            ImportFromDarkLoaderMenuItem = new ToolStripMenuItemCustom(NonLocalizableText.DarkLoaderEllipses),
            ImportFromFMSelMenuItem = new ToolStripMenuItemCustom(NonLocalizableText.FMSelEllipses),
            ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItemCustom(NonLocalizableText.NewDarkLoaderEllipses),
        });

        ScanAllFMsMenuItem.Enabled = _scanAllFMsMenuItemEnabled;

        GameVersionsMenuItem.Click += _owner.MainMenu_GameVersionsMenuItem_Click;

        foreach (ToolStripMenuItemCustom item in ImportMenuItem.DropDown.Items)
        {
            item.Click += _owner.Async_EventHandler_Main;
        }
        ScanAllFMsMenuItem.Click += _owner.Async_EventHandler_Main;
        SettingsMenuItem.Click += _owner.Async_EventHandler_Main;
        CheckForUpdatesMenuItem.Click += _owner.Async_EventHandler_Main;
        ViewHelpFileMenuItem.Click += _owner.ViewHelpFileMenuItem_Click;
        AboutMenuItem.Click += _owner.AboutMenuItem_Click;
        ExitMenuItem.Click += _owner.Exit_Click;

        _menu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;

        GameVersionsMenuItem.Text = LText.MainMenu.GameVersions;
        ImportMenuItem.Text = LText.MainMenu.Import;
        ScanAllFMsMenuItem.Text = LText.MainMenu.ScanAllFMs;
        SettingsMenuItem.Text = LText.MainButtons.Settings;
        CheckForUpdatesMenuItem.Text = LText.MainMenu.CheckForUpdates;
        ViewHelpFileMenuItem.Text = LText.MainMenu.ViewHelpFile;
        AboutMenuItem.Text = LText.MainMenu.About;
        ExitMenuItem.Text = LText.Global.Exit;
    }

    internal bool Visible => _constructed && _menu.Visible;

    internal void SetScanAllFMsMenuItemEnabled(bool enabled)
    {
        if (_constructed)
        {
            ScanAllFMsMenuItem.Enabled = enabled;
        }
        else
        {
            _scanAllFMsMenuItemEnabled = enabled;
        }
    }
}

using System;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class MainLLMenu
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
#if false
        private  ToolStripMenuItemCustom GlobalFMStatsMenuItem = null!;
#endif
        private ToolStripMenuItemCustom ImportMenuItem = null!;
        private ToolStripMenuItemCustom ImportFromDarkLoaderMenuItem = null!;
        private ToolStripMenuItemCustom ImportFromFMSelMenuItem = null!;
        [UsedImplicitly]
        // It's an implicit "else" case, but let's keep it just for consistency
#pragma warning disable IDE0052 // Remove unread private members
        private ToolStripMenuItemCustom ImportFromNewDarkLoaderMenuItem = null!;
#pragma warning restore IDE0052 // Remove unread private members

        private ToolStripMenuItemCustom ScanAllFMsMenuItem = null!;

        private ToolStripMenuItemCustom SettingsMenuItem = null!;

        private ToolStripMenuItemCustom ViewHelpFileMenuItem = null!;
        private ToolStripMenuItemCustom AboutMenuItem = null!;
        private ToolStripMenuItemCustom ExitMenuItem = null!;

        #endregion

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
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

            _menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents());
            _menu.Items.AddRange(new ToolStripItem[]
            {
                GameVersionsMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy},
#if false
                GlobalFMStatsMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy}
#endif
                new ToolStripSeparator { Tag = LoadType.Lazy },
                ImportMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                ScanAllFMsMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy },
                SettingsMenuItem = new ToolStripMenuItemCustom{ Tag = LoadType.Lazy },
                new ToolStripSeparator { Tag = LoadType.Lazy },
                ViewHelpFileMenuItem = new ToolStripMenuItemCustom { ShortcutKeys = Keys.F1, Tag = LoadType.Lazy },
                AboutMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy},
                new ToolStripSeparator { Tag = LoadType.Lazy },
                ExitMenuItem = new ToolStripMenuItemCustom { ShortcutKeys = Keys.Alt | Keys.F4, Tag = LoadType.Lazy }
            });

            ImportMenuItem.DropDown.Items.AddRange(new ToolStripItem[]
            {
                // Not localized because they consist solely of proper names! Don't remove these!
                ImportFromDarkLoaderMenuItem = new ToolStripMenuItemCustom("DarkLoader...") { Tag = LoadType.Lazy },
                ImportFromFMSelMenuItem = new ToolStripMenuItemCustom("FMSel...") { Tag = LoadType.Lazy },
                ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItemCustom("NewDarkLoader...") { Tag = LoadType.Lazy }
            });

            ScanAllFMsMenuItem.Enabled = _scanAllFMsMenuItemEnabled;

            GameVersionsMenuItem.Click += MainMenu_GameVersionsMenuItem_Click;
#if false
            GlobalFMStatsMenuItem.Click += form.GlobalFMStatsMenuItem_Click;
#endif
            foreach (ToolStripMenuItemCustom item in ImportMenuItem.DropDown.Items)
            {
                item.Click += ImportMenuItems_Click;
            }
            ScanAllFMsMenuItem.Click += _owner.ScanAllFMsMenuItem_Click;
            SettingsMenuItem.Click += _owner.Settings_Click;
            ViewHelpFileMenuItem.Click += ViewHelpFileMenuItem_Click;
            AboutMenuItem.Click += AboutMenuItem_Click;
            ExitMenuItem.Click += (_, _) => _owner.Close();

            _constructed = true;

            Localize();
        }

        internal void Localize()
        {
            if (!_constructed) return;

            GameVersionsMenuItem.Text = LText.MainMenu.GameVersions;
#if false
            GlobalFMStatsMenuItem.Text = LText.MainMenu.GlobalFMStats;
#endif
            ImportMenuItem.Text = LText.MainMenu.Import;
            ScanAllFMsMenuItem.Text = LText.MainMenu.ScanAllFMs;
            SettingsMenuItem.Text = LText.MainButtons.Settings;
            ViewHelpFileMenuItem.Text = LText.MainMenu.ViewHelpFile;
            AboutMenuItem.Text = LText.MainMenu.About;
            ExitMenuItem.Text = LText.Global.Exit;
        }

        internal bool Visible => _constructed && _menu.Visible;

        internal void SetScanAllFMsMenuItemEnabled(bool enabled)
        {
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

        private void MainMenu_GameVersionsMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new GameVersionsForm();
            f.ShowDialogDark();
        }

        private async void ImportMenuItems_Click(object sender, EventArgs e)
        {
            ImportType importType =
                sender == ImportFromDarkLoaderMenuItem
                ? ImportType.DarkLoader
                : sender == ImportFromFMSelMenuItem
                ? ImportType.FMSel
                : ImportType.NewDarkLoader;

            await Import.ImportFrom(importType);
        }

#if false
        private void GlobalFMStatsMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new GlobalFMStatsForm();
            f.ShowDialogDark();
        }
#endif

        private void ViewHelpFileMenuItem_Click(object sender, EventArgs e) => Core.OpenHelpFile();

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new AboutForm();
            f.ShowDialogDark();
        }
    }
}

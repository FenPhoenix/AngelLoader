using System;
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class MainLLMenu
    {
        #region Backing fields

        private static bool _constructed;
        private static bool _scanAllFMsMenuItemEnabled;

        #endregion

        #region Menu items

        internal static DarkContextMenu Menu = null!;
        private static ToolStripMenuItemCustom GameVersionsMenuItem = null!;
#if false
        private static ToolStripMenuItemCustom GlobalFMStatsMenuItem = null!;
#endif
        private static ToolStripMenuItemCustom ImportMenuItem = null!;
        private static ToolStripMenuItemCustom ImportFromDarkLoaderMenuItem = null!;
        private static ToolStripMenuItemCustom ImportFromFMSelMenuItem = null!;
        [UsedImplicitly]
        // It's an implicit "else" case, but let's keep it just for consistency
#pragma warning disable IDE0052 // Remove unread private members
        private static ToolStripMenuItemCustom ImportFromNewDarkLoaderMenuItem = null!;
#pragma warning restore IDE0052 // Remove unread private members

        private static ToolStripMenuItemCustom ScanAllFMsMenuItem = null!;

        private static ToolStripMenuItemCustom SettingsMenuItem = null!;

        private static ToolStripMenuItemCustom ViewHelpFileMenuItem = null!;
        private static ToolStripMenuItemCustom AboutMenuItem = null!;
        private static ToolStripMenuItemCustom ExitMenuItem = null!;

        #endregion

        private static bool _darkModeEnabled;
        [PublicAPI]
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new DarkContextMenu(_darkModeEnabled, components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                GameVersionsMenuItem = new ToolStripMenuItemCustom { Tag = LoadType.Lazy},
#if false
                GlobalFMStatsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True}
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
            ScanAllFMsMenuItem.Click += form.ScanAllFMsMenuItem_Click;
            SettingsMenuItem.Click += form.Settings_Click;
            ViewHelpFileMenuItem.Click += ViewHelpFileMenuItemClick;
            AboutMenuItem.Click += AboutMenuItemClick;
            ExitMenuItem.Click += (_, _) => form.Close();

            _constructed = true;

            Localize();
        }

        internal static void Localize()
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

        internal static bool Visible => _constructed && Menu.Visible;

        internal static bool ScanAllFMsMenuItemEnabled
        {
            get => _constructed && ScanAllFMsMenuItem.Enabled;
            set
            {
                if (_constructed)
                {
                    ScanAllFMsMenuItem.Enabled = value;
                }
                else
                {
                    _scanAllFMsMenuItemEnabled = value;
                }
            }
        }

        private static void MainMenu_GameVersionsMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new GameVersionsForm();
            f.ShowDialogDark();
        }

        private static async void ImportMenuItems_Click(object sender, EventArgs e)
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
        private static void GlobalFMStatsMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new GlobalFMStatsForm();
            f.ShowDialogDark();
        }
#endif

        private static void ViewHelpFileMenuItemClick(object sender, EventArgs e) => Core.OpenHelpFile();

        private static void AboutMenuItemClick(object sender, EventArgs e)
        {
            using var f = new AboutForm();
            f.ShowDialogDark();
        }
    }
}

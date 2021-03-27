using System;
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class MainLLMenu
    {
        private static bool _constructed;

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

        private static ToolStripMenuItemCustom ViewHelpFileMenuItem = null!;
        private static ToolStripMenuItemCustom AboutMenuItem = null!;
        private static ToolStripMenuItemCustom ExitMenuItem = null!;

        private static bool _darkModeEnabled;
        public static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu!.DarkModeEnabled = _darkModeEnabled;
            }
        }

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new DarkContextMenu(_darkModeEnabled, components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                GameVersionsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True},
#if false
                GlobalFMStatsMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True}
#endif
                new ToolStripSeparator { Tag = LazyLoaded.True },
                ImportMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True },
                new ToolStripSeparator { Tag = LazyLoaded.True },
                ViewHelpFileMenuItem = new ToolStripMenuItemCustom { ShortcutKeys = Keys.F1, Tag = LazyLoaded.True },
                AboutMenuItem = new ToolStripMenuItemCustom { Tag = LazyLoaded.True},
                new ToolStripSeparator { Tag = LazyLoaded.True },
                ExitMenuItem = new ToolStripMenuItemCustom { ShortcutKeys = Keys.Alt | Keys.F4, Tag = LazyLoaded.True }
            });

            ImportMenuItem.DropDown.Items.AddRange(new ToolStripItem[]
            {
                // Not localized because they consist solely of proper names! Don't remove these!
                ImportFromDarkLoaderMenuItem = new ToolStripMenuItemCustom("DarkLoader...") { Tag = LazyLoaded.True },
                ImportFromFMSelMenuItem = new ToolStripMenuItemCustom("FMSel...") { Tag = LazyLoaded.True },
                ImportFromNewDarkLoaderMenuItem = new ToolStripMenuItemCustom("NewDarkLoader...") { Tag = LazyLoaded.True }
            });

            foreach (ToolStripMenuItemCustom item in ImportMenuItem.DropDown.Items)
            {
                item.Click += ImportMenuItems_Click;
            }

            GameVersionsMenuItem.Click += MainMenu_GameVersionsMenuItem_Click;
#if false
            GlobalFMStatsMenuItem.Click += form.GlobalFMStatsMenuItem_Click;
#endif
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
            ViewHelpFileMenuItem.Text = LText.MainMenu.ViewHelpFile;
            AboutMenuItem.Text = LText.MainMenu.About;
            ExitMenuItem.Text = LText.Global.Exit;
        }

        internal static bool Visible => _constructed && Menu.Visible;

        private static void MainMenu_GameVersionsMenuItem_Click(object sender, EventArgs e)
        {
            using var f = new GameVersionsForm();
            f.ShowDialog();
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
            f.ShowDialog();
        }
#endif

        private static void ViewHelpFileMenuItemClick(object sender, EventArgs e) => Core.OpenHelpFile();

        private static void AboutMenuItemClick(object sender, EventArgs e)
        {
            using var f = new AboutForm();
            f.ShowDialog();
        }
    }
}

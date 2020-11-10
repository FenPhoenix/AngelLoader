using System.ComponentModel;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class MainLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip Menu = null!;
        private static ToolStripMenuItemCustom GameVersionsMenuItem = null!;
#if false
        private static ToolStripMenuItemCustom GlobalFMStatsMenuItem = null!;
#endif
        private static ToolStripMenuItemCustom ViewHelpFileMenuItem = null!;
        private static ToolStripMenuItemCustom AboutMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                GameVersionsMenuItem = new ToolStripMenuItemCustom(),
#if false
                GlobalFMStatsMenuItem = new ToolStripMenuItemCustom()
#endif
                ViewHelpFileMenuItem = new ToolStripMenuItemCustom(),
                AboutMenuItem = new ToolStripMenuItemCustom()
            });

            GameVersionsMenuItem.Click += form.MainMenu_GameVersionsMenuItem_Click;
#if false
            GlobalFMStatsMenuItem.Click += form.GlobalFMStatsMenuItem_Click;
#endif
            ViewHelpFileMenuItem.Click += form.ViewHelpFileMenuItemClick;
            AboutMenuItem.Click += form.AboutMenuItemClick;

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
            ViewHelpFileMenuItem.Text = LText.MainMenu.ViewHelpFile;
            AboutMenuItem.Text = LText.MainMenu.About;
        }
    }
}

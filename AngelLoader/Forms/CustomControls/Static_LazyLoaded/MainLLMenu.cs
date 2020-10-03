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
        private static ToolStripMenuItemCustom GlobalFMStatsMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                GameVersionsMenuItem = new ToolStripMenuItemCustom(),
                GlobalFMStatsMenuItem = new ToolStripMenuItemCustom()
            });

            GameVersionsMenuItem.Click += form.MainMenu_GameVersionsMenuItem_Click;
            GlobalFMStatsMenuItem.Click += form.GlobalFMStatsMenuItem_Click;

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            GameVersionsMenuItem.Text = LText.MainMenu.GameVersions;
            GlobalFMStatsMenuItem.Text = LText.MainMenu.GlobalFMStats;
        }
    }
}

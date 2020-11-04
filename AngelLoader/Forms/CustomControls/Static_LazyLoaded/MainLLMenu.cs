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
            });

            GameVersionsMenuItem.Click += form.MainMenu_GameVersionsMenuItem_Click;
#if false
            GlobalFMStatsMenuItem.Click += form.GlobalFMStatsMenuItem_Click;
#endif

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
        }
    }
}

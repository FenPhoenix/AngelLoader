using System.ComponentModel;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class MainLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip Menu = null!;
        private static ToolStripMenuItem GameVersionsMenuItem = null!;
        private static ToolStripMenuItem GlobalFMStatsMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                GameVersionsMenuItem = new ToolStripMenuItem(),
                GlobalFMStatsMenuItem = new ToolStripMenuItem()
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

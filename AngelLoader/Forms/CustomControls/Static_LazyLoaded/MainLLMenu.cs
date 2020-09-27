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
        private static ToolStripMenuItem FMsListStatsMenuItem = null!;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);
            Menu.Items.AddRange(new ToolStripItem[]
            {
                GameVersionsMenuItem = new ToolStripMenuItem(),
                FMsListStatsMenuItem = new ToolStripMenuItem()
            });

            GameVersionsMenuItem.Click += form.MainMenu_GameVersionsMenuItem_Click;
            FMsListStatsMenuItem.Click += form.FMsListStatsMenuItem_Click;

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            GameVersionsMenuItem.Text = LText.MainMenu.GameVersions;
        }
    }
}

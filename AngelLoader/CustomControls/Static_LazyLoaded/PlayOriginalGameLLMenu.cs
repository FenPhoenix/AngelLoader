using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;

namespace AngelLoader.CustomControls.Static_LazyLoaded
{
    internal static class PlayOriginalGameLLMenu
    {
        private static bool _constructed;

        internal static ContextMenuStrip Menu;
        internal static ToolStripMenuItem Thief1MenuItem;
        internal static ToolStripMenuItem Thief2MenuItem;
        internal static ToolStripMenuItem Thief2MPMenuItem;
        internal static ToolStripMenuItem Thief3MenuItem;
        internal static ToolStripMenuItem SS2MenuItem;

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new ContextMenuStrip(components);

            Thief1MenuItem = new ToolStripMenuItem { Image = Images.Thief1_16 };
            Thief2MenuItem = new ToolStripMenuItem { Image = Images.Thief2_16 };
            Thief2MPMenuItem = new ToolStripMenuItem { Image = Images.Thief2_16 };
            Thief3MenuItem = new ToolStripMenuItem { Image = Images.Thief3_16 };
            SS2MenuItem = new ToolStripMenuItem { Image = Images.Shock2_16 };

            Menu.Items.AddRange(new ToolStripItem[]
            {
                Thief1MenuItem,
                Thief2MenuItem,
                Thief2MPMenuItem,
                Thief3MenuItem,
                SS2MenuItem
            });

            Thief1MenuItem.Click += form.PlayOriginalGameMenuItem_Click;
            Thief2MenuItem.Click += form.PlayOriginalGameMenuItem_Click;
            Thief2MPMenuItem.Click += form.PlayOriginalGameMenuItem_Click;
            Thief3MenuItem.Click += form.PlayOriginalGameMenuItem_Click;
            SS2MenuItem.Click += form.PlayOriginalGameMenuItem_Click;

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            Thief1MenuItem.Text = LText.Global.Thief1.EscapeAmpersands();
            Thief2MenuItem.Text = LText.Global.Thief2.EscapeAmpersands();
            Thief2MPMenuItem.Text = LText.PlayOriginalGameMenu.Thief2_Multiplayer.EscapeAmpersands();
            Thief3MenuItem.Text = LText.Global.Thief3.EscapeAmpersands();
            SS2MenuItem.Text = LText.Global.SystemShock2.EscapeAmpersands();
        }
    }
}

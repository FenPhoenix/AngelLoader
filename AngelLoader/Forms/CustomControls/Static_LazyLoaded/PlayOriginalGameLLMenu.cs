using System.ComponentModel;
using System.Windows.Forms;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    // @GENGAMES - Play original game menu
    internal static class PlayOriginalGameLLMenu
    {
        private static bool _constructed;

        private static ContextMenuStrip? _menu;
        internal static ContextMenuStrip Menu
        {
            get => _menu!;
            private set => _menu = value;
        }

        private static ToolStripMenuItem? _thief1MenuItem;
        internal static ToolStripMenuItem Thief1MenuItem
        {
            get => _thief1MenuItem!;
            private set => _thief1MenuItem = value;
        }

        private static ToolStripMenuItem? _thief2MenuItem;
        internal static ToolStripMenuItem Thief2MenuItem
        {
            get => _thief2MenuItem!;
            private set => _thief2MenuItem = value;
        }

        private static ToolStripMenuItem? _thief2MPMenuItem;
        internal static ToolStripMenuItem Thief2MPMenuItem
        {
            get => _thief2MPMenuItem!;
            private set => _thief2MPMenuItem = value;
        }

        private static ToolStripMenuItem? _thief3MenuItem;
        internal static ToolStripMenuItem Thief3MenuItem
        {
            get => _thief3MenuItem!;
            private set => _thief3MenuItem = value;
        }

        private static ToolStripMenuItem? _ss2MenuItem;
        internal static ToolStripMenuItem SS2MenuItem
        {
            get => _ss2MenuItem!;
            private set => _ss2MenuItem = value;
        }

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

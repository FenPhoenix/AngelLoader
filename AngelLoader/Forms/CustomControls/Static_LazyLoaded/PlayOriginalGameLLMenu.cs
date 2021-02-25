using System.ComponentModel;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    // @GENGAMES (Play original game menu): Begin
    internal static class PlayOriginalGameLLMenu
    {
        private static bool _constructed;

        internal static DarkContextMenu Menu = null!;

        internal static ToolStripMenuItemCustom Thief1MenuItem = null!;
        internal static ToolStripMenuItemCustom Thief2MenuItem = null!;
        internal static ToolStripMenuItemCustom Thief2MPMenuItem = null!;
        internal static ToolStripMenuItemCustom Thief3MenuItem = null!;
        internal static ToolStripMenuItemCustom SS2MenuItem = null!;

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
                Thief1MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief1_16 },
                Thief2MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief2_16 },
                Thief2MPMenuItem = new ToolStripMenuItemCustom { Image = Images.Thief2_16 },
                Thief3MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief3_16 },
                SS2MenuItem = new ToolStripMenuItemCustom { Image = Images.Shock2_16 }
            });

            foreach (ToolStripMenuItemCustom item in Menu.Items)
            {
                item.Click += form.PlayOriginalGameMenuItem_Click;
            }

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            Thief1MenuItem.Text = LText.Global.Thief1;
            Thief2MenuItem.Text = LText.Global.Thief2;
            Thief2MPMenuItem.Text = LText.PlayOriginalGameMenu.Thief2_Multiplayer;
            Thief3MenuItem.Text = LText.Global.Thief3;
            SS2MenuItem.Text = LText.Global.SystemShock2;
        }
    }
    // @GENGAMES (Play original game menu): End
}

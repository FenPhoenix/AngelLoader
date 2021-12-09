using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
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
        [PublicAPI]
        internal static bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Menu.DarkModeEnabled = _darkModeEnabled;

                Thief1MenuItem.Image = Images.Thief1_16;
                Thief2MenuItem.Image = Images.Thief2_16;
                Thief2MPMenuItem.Image = Images.Thief2_16;
                Thief3MenuItem.Image = Images.Thief3_16;
                SS2MenuItem.Image = Images.Shock2_16;
            }
        }

        internal static void Construct(MainForm form, IContainer components)
        {
            if (_constructed) return;

            Menu = new DarkContextMenu(_darkModeEnabled, components) { Tag = LoadType.Lazy };

            Menu.Items.AddRange(new ToolStripItem[]
            {
                Thief1MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief1_16, Tag = LoadType.Lazy },
                Thief2MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief2_16, Tag = LoadType.Lazy },
                Thief2MPMenuItem = new ToolStripMenuItemCustom { Image = Images.Thief2_16, Tag = LoadType.Lazy },
                Thief3MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief3_16, Tag = LoadType.Lazy },
                SS2MenuItem = new ToolStripMenuItemCustom { Image = Images.Shock2_16, Tag = LoadType.Lazy }
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

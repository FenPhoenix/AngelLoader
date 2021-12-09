using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    // @GENGAMES (Play original game menu): Begin
    internal sealed class PlayOriginalGameLLMenu
    {
        private bool _constructed;

        private readonly MainForm _owner;

        private DarkContextMenu _menu = null!;
        internal DarkContextMenu Menu
        {
            get
            {
                Construct();
                return _menu;
            }
        }

        internal PlayOriginalGameLLMenu(MainForm owner) => _owner = owner;

        internal ToolStripMenuItemCustom Thief1MenuItem = null!;
        internal ToolStripMenuItemCustom Thief2MenuItem = null!;
        internal ToolStripMenuItemCustom Thief2MPMenuItem = null!;
        internal ToolStripMenuItemCustom Thief3MenuItem = null!;
        internal ToolStripMenuItemCustom SS2MenuItem = null!;

        private bool _darkModeEnabled;
        [PublicAPI]
        internal bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _menu.DarkModeEnabled = _darkModeEnabled;

                Thief1MenuItem.Image = Images.Thief1_16;
                Thief2MenuItem.Image = Images.Thief2_16;
                Thief2MPMenuItem.Image = Images.Thief2_16;
                Thief3MenuItem.Image = Images.Thief3_16;
                SS2MenuItem.Image = Images.Shock2_16;
            }
        }

        internal void Construct()
        {
            if (_constructed) return;

            _menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };

            _menu.Items.AddRange(new ToolStripItem[]
            {
                Thief1MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief1_16, Tag = LoadType.Lazy },
                Thief2MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief2_16, Tag = LoadType.Lazy },
                Thief2MPMenuItem = new ToolStripMenuItemCustom { Image = Images.Thief2_16, Tag = LoadType.Lazy },
                Thief3MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief3_16, Tag = LoadType.Lazy },
                SS2MenuItem = new ToolStripMenuItemCustom { Image = Images.Shock2_16, Tag = LoadType.Lazy }
            });

            foreach (ToolStripMenuItemCustom item in _menu.Items)
            {
                item.Click += _owner.PlayOriginalGameMenuItem_Click;
            }

            _constructed = true;

            Localize();
        }

        internal void Localize()
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

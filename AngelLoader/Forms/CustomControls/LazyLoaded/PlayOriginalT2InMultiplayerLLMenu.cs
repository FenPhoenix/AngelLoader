using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal class PlayOriginalT2InMultiplayerLLMenu
    {
        private bool _constructed;

        private readonly MainForm _owner;

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _menu.DarkModeEnabled = _darkModeEnabled;
                MenuItem.Image = Images.Thief2_16;
            }
        }

        internal PlayOriginalT2InMultiplayerLLMenu(MainForm owner) => _owner = owner;

        private ToolStripMenuItemCustom MenuItem = null!;

        private DarkContextMenu _menu = null!;
        internal DarkContextMenu Menu
        {
            get
            {
                Construct();
                return _menu;
            }
        }

        private void Construct()
        {
            if (_constructed) return;

            _menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };
            MenuItem = new ToolStripMenuItemCustom { Image = Images.Thief2_16, Tag = LoadType.Lazy };
            MenuItem.Click += _owner.PlayT2InMultiplayerMenuItem_Click;
            _menu.Items.Add(MenuItem);

            _constructed = true;

            Localize();
        }

        internal void Localize()
        {
            if (!_constructed) return;

            MenuItem.Text = LText.PlayOriginalGameMenu.Thief2_Multiplayer;
        }
    }
}

using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
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

        internal ToolStripMenuItemCustom Thief2MPMenuItem = null!;

        internal ToolStripItem[] GameMenuItems = new ToolStripItem[SupportedGameCount];

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

                for (int i = 0; i < SupportedGameCount; i++)
                {
                    GameMenuItems[i].Image =
                        Images.GetPerGameImage(
                            (GameIndex)i,
                            Images.PerGameImageSize.Small,
                            Images.PerGameImageType.Primary);
                }
                Thief2MPMenuItem.Image = Images.GetPerGameImage(GameIndex.Thief2, Images.PerGameImageSize.Small, Images.PerGameImageType.Primary);
            }
        }

        internal void Construct()
        {
            if (_constructed) return;

            _menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameMenuItems[i] = new ToolStripMenuItemCustom
                {
                    GameIndex = (GameIndex)i,
                    Image =
                        Images.GetPerGameImage(
                            (GameIndex)i,
                            Images.PerGameImageSize.Small,
                            Images.PerGameImageType.Primary),
                    Tag = LoadType.Lazy
                };
            }

            _menu.Items.AddRange(GameMenuItems);

            Thief2MPMenuItem = new ToolStripMenuItemCustom
            {
                GameIndex = GameIndex.Thief2,
                Image = Images.GetPerGameImage(GameIndex.Thief2, Images.PerGameImageSize.Small, Images.PerGameImageType.Primary),
                Tag = LoadType.Lazy
            };
            _menu.Items.Insert(2, Thief2MPMenuItem);

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

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameMenuItems[i].Text = GameSupport.GetLocalizedGameName((GameIndex)i);
            }
            Thief2MPMenuItem.Text = LText.PlayOriginalGameMenu.Thief2_Multiplayer;
        }
    }
    // @GENGAMES (Play original game menu): End
}

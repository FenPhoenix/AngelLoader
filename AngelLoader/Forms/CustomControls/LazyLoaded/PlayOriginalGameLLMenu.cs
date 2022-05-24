using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    // @GENGAMES(T2MP) (Play original game menu): Begin
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

        internal ToolStripMenuItemCustom ModsSubMenu = null!;

        internal readonly ToolStripItem[] GameMenuItems = new ToolStripItem[SupportedGameCount];

        // @GENGAMES(T3 doesn't support mod management)
        internal readonly ToolStripItem[] ModMenuItems = new ToolStripItem[SupportedGameCount - 1];

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

                for (int i = 0, modI = 0; i < SupportedGameCount; i++)
                {
                    GameIndex gameIndex = (GameIndex)i;
                    GameMenuItems[i].Image = Images.GetPerGameImage(i).Primary.Small();
                    // @GENGAMES(T3 doesn't support mod management)
                    if (GameIsDark(gameIndex))
                    {
                        ModMenuItems[modI].Image = Images.GetPerGameImage(i).Primary.Small();
                        modI++;
                    }
                }
                Thief2MPMenuItem.Image = Images.GetPerGameImage(GameIndex.Thief2).Primary.Small();
                ModsSubMenu.Image = Images.Mods_16;
            }
        }

        internal void Construct()
        {
            if (_constructed) return;

            _menu = new DarkContextMenu(_darkModeEnabled, _owner.GetComponents()) { Tag = LoadType.Lazy };

            for (int i = 0, modI = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                GameMenuItems[i] = new ToolStripMenuItemCustom
                {
                    GameIndex = gameIndex,
                    Image = Images.GetPerGameImage(i).Primary.Small(),
                    Tag = LoadType.Lazy
                };
                // @GENGAMES(T3 doesn't support mod management)
                if (GameIsDark(gameIndex))
                {
                    ModMenuItems[modI] = new ToolStripMenuItemCustom
                    {
                        GameIndex = gameIndex,
                        Image = Images.GetPerGameImage(i).Primary.Small(),
                        Tag = LoadType.Lazy
                    };
                    modI++;
                }
            }

            _menu.Items.AddRange(GameMenuItems);

            Thief2MPMenuItem = new ToolStripMenuItemCustom
            {
                GameIndex = GameIndex.Thief2,
                Image = Images.GetPerGameImage(GameIndex.Thief2).Primary.Small(),
                Tag = LoadType.Lazy
            };
            _menu.Items.Insert(2, Thief2MPMenuItem);

            foreach (ToolStripMenuItemCustom item in _menu.Items)
            {
                item.Click += _owner.PlayOriginalGameMenuItem_Click;
            }

            ModsSubMenu = new ToolStripMenuItemCustom
            {
                Image = Images.Mods_16,
                Tag = LoadType.Lazy
            };

            _menu.Items.Add(new ToolStripSeparator());

            _menu.Items.Add(ModsSubMenu);

            ModsSubMenu.DropDownItems.AddRange(ModMenuItems);

            foreach (ToolStripMenuItemCustom item in ModsSubMenu.DropDownItems)
            {
                item.Click += _owner.PlayOriginalGameModMenuItem_Click;
            }

            _constructed = true;

            Localize();
        }

        internal void Localize()
        {
            if (!_constructed) return;

            for (int i = 0, modI = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                GameMenuItems[i].Text = GetLocalizedGameName(gameIndex);
                // @GENGAMES(T3 doesn't support mod management)
                if (GameIsDark(gameIndex))
                {
                    ModsSubMenu.DropDownItems[modI].Text = GetLocalizedGameName(gameIndex) + "...";
                    modI++;
                }
            }
            Thief2MPMenuItem.Text = LText.PlayOriginalGameMenu.Thief2_Multiplayer;
            ModsSubMenu.Text = LText.PlayOriginalGameMenu.Mods_SubMenu;
        }
    }
    // @GENGAMES(T2MP) (Play original game menu): End
}

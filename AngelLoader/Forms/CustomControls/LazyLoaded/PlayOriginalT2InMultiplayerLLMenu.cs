using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

internal sealed class PlayOriginalT2InMultiplayerLLMenu : IDarkable
{
    private bool _constructed;

    private readonly MainForm _owner;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            if (!_constructed) return;

            _menu.DarkModeEnabled = _darkModeEnabled;
            MenuItem.Image = Images.GetPerGameImage(GameIndex.Thief2).Primary.Small();
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

        _menu = new DarkContextMenu(_owner);
        MenuItem = new ToolStripMenuItemCustom
        {
            GameIndex = GameIndex.Thief2,
            Image = Images.GetPerGameImage(GameIndex.Thief2).Primary.Small(),
        };
        MenuItem.Click += _owner.PlayT2MPMenuItem_Click;
        _menu.Items.Add(MenuItem);

        _menu.DarkModeEnabled = _darkModeEnabled;

        _constructed = true;

        Localize();
    }

    internal void Localize()
    {
        if (!_constructed) return;

        MenuItem.Text = LText.PlayOriginalGameMenu.Thief2_Multiplayer;
    }
}

using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded;

// @GENGAMES(T2MP) (Play original game controls - lazy loaded)
internal sealed class Lazy_PlayOriginalControls : IDarkable
{
    private bool _constructedSingle;
    private bool _constructedMulti;

    private readonly MainForm _owner;

    internal DarkButton ButtonSingle = null!;

    private readonly DarkButton[] GameButtons = new DarkButton[SupportedGameCount];

    internal DarkArrowButton T2MPMenuButton = null!;

    private bool _darkModeEnabled;
    [PublicAPI]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            if (_constructedSingle)
            {
                ButtonSingle.DarkModeEnabled = value;
            }
            if (_constructedMulti)
            {
                foreach (DarkButton gameButton in GameButtons)
                {
                    gameButton.DarkModeEnabled = value;
                }
                T2MPMenuButton.DarkModeEnabled = value;
            }
        }
    }

    public Lazy_PlayOriginalControls(MainForm owner) => _owner = owner;

    private void ConstructSingle()
    {
        if (_constructedSingle) return;

        ButtonSingle = new DarkButton
        {
            Tag = LoadType.Lazy,

            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0),
            MinimumSize = new Size(0, 36),
            Padding = new Padding(33, 0, 6, 0),
            TabIndex = 0,

            DarkModeEnabled = _darkModeEnabled,
        };
        ButtonSingle.PaintCustom += _owner.PlayOriginalGameButton_Paint;
        ButtonSingle.Click += _owner.PlayOriginalGameButton_Click;
        _owner.PlayOriginalFLP.Controls.Add(ButtonSingle);
        _owner.PlayOriginalFLP.Controls.SetChildIndex(ButtonSingle, 0);

        _constructedSingle = true;

        LocalizeSingle();
    }

    private void ConstructMulti()
    {
        if (_constructedMulti) return;

        for (int i = 0, tabIndex = 1; i < SupportedGameCount; i++)
        {
            var gameButton = new DarkButton
            {
                Tag = LoadType.Lazy,
                GameIndex = (GameIndex)i,

                Margin = new Padding(0),
                MinimumSize = new Size(36, 36),
                Size = new Size(36, 36),
                TabIndex = tabIndex,

                DarkModeEnabled = _darkModeEnabled,
            };

            GameButtons[i] = gameButton;

            gameButton.PaintCustom += _owner.PlayOriginalGamesButtons_Paint;
            gameButton.Click += _owner.PlayOriginalGameButtons_Click;
            gameButton.MouseUp += _owner.PlayOriginalGameButtons_MouseUp;

            tabIndex += i == (int)GameIndex.Thief2 ? 2 : 1;
        }

        T2MPMenuButton = new DarkArrowButton
        {
            Tag = LoadType.Lazy,

            ArrowDirection = Direction.Up,
            Margin = new Padding(0),
            Size = new Size(16, 36),
            TabIndex = 3,

            DarkModeEnabled = _darkModeEnabled,
        };
        T2MPMenuButton.Click += _owner.PlayOriginalT2MPButton_Click;

        foreach (DarkButton gameButton in GameButtons)
        {
            _owner.PlayOriginalFLP.Controls.Add(gameButton);
        }
        _owner.PlayOriginalFLP.Controls.Add(T2MPMenuButton);
        _owner.PlayOriginalFLP.Controls.SetChildIndex(T2MPMenuButton, _owner.PlayOriginalFLP.Controls.GetChildIndex(GameButtons[(int)GameIndex.Thief2]) + 1);

        _constructedMulti = true;

        LocalizeMulti();
    }

    internal void LocalizeSingle()
    {
        if (!_constructedSingle) return;
        ButtonSingle.Text = LText.MainButtons.PlayOriginalGame;
    }

    internal void LocalizeMulti()
    {
        if (!_constructedMulti) return;

        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;
            string message = GetLocalizedGamePlayOriginalText(gameIndex);
            if (GameSupportsMods(gameIndex)) message += "\r\n" + LText.PlayOriginalGameMenu.Mods_ToolTipMessage;
            _owner.MainToolTip.SetToolTip(GameButtons[i], message);
        }
    }

    internal void SetMode(bool singleButton)
    {
        if (singleButton)
        {
            ConstructSingle();
            ButtonSingle.Show();

            if (_constructedMulti)
            {
                foreach (DarkButton gameButton in GameButtons)
                {
                    gameButton.Hide();
                }
                T2MPMenuButton.Hide();
            }
        }
        else
        {
            ConstructMulti();

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameButtons[i].Visible = !Config.GetGameExe((GameIndex)i).IsEmpty();
            }
            T2MPMenuButton.Visible = Config.T2MPDetected;

            if (_constructedSingle)
            {
                ButtonSingle.Hide();
            }
        }
    }
}

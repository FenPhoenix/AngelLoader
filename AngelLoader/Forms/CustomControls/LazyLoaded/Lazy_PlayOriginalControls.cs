using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    // @GENGAMES (Play original game controls - lazy loaded)
    internal sealed class Lazy_PlayOriginalControls
    {
        private bool _constructedSingle;
        private bool _constructedMulti;

        private readonly MainForm _owner;

        internal DarkButton ButtonSingle = null!;

        private readonly DarkButton[] GameButtons = new DarkButton[SupportedGameCount];

        private DarkButton GetGameButton(GameIndex gameIndex) => GameButtons[(int)gameIndex];

        internal DarkArrowButton T2MPMenuButton = null!;

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
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
                    for (int i = 0; i < SupportedGameCount; i++)
                    {
                        GameButtons[i].DarkModeEnabled = value;
                    }
                    T2MPMenuButton.DarkModeEnabled = value;
                }
            }
        }

        public Lazy_PlayOriginalControls(MainForm owner) => _owner = owner;

        private void ConstructSingle()
        {
            if (_constructedSingle) return;

            ButtonSingle = new DarkButton { Tag = LoadType.Lazy };

            ButtonSingle.AutoSize = true;
            ButtonSingle.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            ButtonSingle.Margin = new Padding(0);
            ButtonSingle.MinimumSize = new Size(0, 36);
            ButtonSingle.Padding = new Padding(33, 0, 6, 0);
            ButtonSingle.TabIndex = 0;
            ButtonSingle.UseVisualStyleBackColor = true;
            ButtonSingle.PaintCustom += _owner.PlayOriginalGameButton_Paint;
            ButtonSingle.Click += _owner.PlayOriginalGameButton_Click;
            ButtonSingle.DarkModeEnabled = _darkModeEnabled;
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
                    GameIndex = (GameIndex)i,
                    Margin = new Padding(0),
                    MinimumSize = new Size(36, 36),
                    Size = new Size(36, 36),
                    TabIndex = tabIndex,
                    Tag = LoadType.Lazy,
                    UseVisualStyleBackColor = true,
                    DarkModeEnabled = _darkModeEnabled
                };

                GameButtons[i] = gameButton;

                gameButton.PaintCustom += _owner.PlayOriginalGamesButtons_Paint;
                gameButton.Click += _owner.PlayOriginalGameButtons_Click;

                tabIndex += i == (int)GameIndex.Thief2 ? 2 : 1;
            }

            T2MPMenuButton = new DarkArrowButton { Tag = LoadType.Lazy };

            T2MPMenuButton.ArrowDirection = Direction.Up;
            T2MPMenuButton.Margin = new Padding(0);
            T2MPMenuButton.Size = new Size(16, 36);
            T2MPMenuButton.TabIndex = 3;
            T2MPMenuButton.UseVisualStyleBackColor = true;
            T2MPMenuButton.DarkModeEnabled = _darkModeEnabled;
            T2MPMenuButton.Click += _owner.PlayOriginalT2MPButton_Click;

            for (int i = 0; i < SupportedGameCount; i++)
            {
                _owner.PlayOriginalFLP.Controls.Add(GameButtons[i]);
            }
            _owner.PlayOriginalFLP.Controls.Add(T2MPMenuButton);
            _owner.PlayOriginalFLP.Controls.SetChildIndex(T2MPMenuButton, 2);

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
                _owner.MainToolTip.SetToolTip(GameButtons[i], Ini.GetGamePlayOriginalText((GameIndex)i));
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
                    for (int i = 0; i < SupportedGameCount; i++)
                    {
                        GameButtons[i].Hide();
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
}

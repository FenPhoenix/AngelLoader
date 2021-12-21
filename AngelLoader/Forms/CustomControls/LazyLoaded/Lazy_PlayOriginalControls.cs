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
        internal DarkButton T1Button = null!;
        internal DarkButton T2Button = null!;
        internal DarkArrowButton T2MPMenuButton = null!;
        internal DarkButton T3Button = null!;
        // Implicit in an if-else chain
        // ReSharper disable once MemberCanBePrivate.Global
        internal DarkButton SS2Button = null!;

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
                    T1Button.DarkModeEnabled = value;
                    T2Button.DarkModeEnabled = value;
                    T2MPMenuButton.DarkModeEnabled = value;
                    T3Button.DarkModeEnabled = value;
                    SS2Button.DarkModeEnabled = value;
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

            T1Button = new DarkButton { Tag = LoadType.Lazy };
            T2Button = new DarkButton { Tag = LoadType.Lazy };
            T2MPMenuButton = new DarkArrowButton { Tag = LoadType.Lazy };
            T3Button = new DarkButton { Tag = LoadType.Lazy };
            SS2Button = new DarkButton { Tag = LoadType.Lazy };

            T1Button.Margin = new Padding(0);
            T1Button.MinimumSize = new Size(36, 36);
            T1Button.Size = new Size(36, 36);
            T1Button.TabIndex = 1;
            T1Button.UseVisualStyleBackColor = true;
            T1Button.DarkModeEnabled = _darkModeEnabled;
            T1Button.PaintCustom += _owner.PlayOriginalGamesButtons_Paint;
            T1Button.Click += _owner.PlayOriginalGameButtons_Click;

            T2Button.Margin = new Padding(0);
            T2Button.MinimumSize = new Size(36, 36);
            T2Button.Size = new Size(36, 36);
            T2Button.TabIndex = 2;
            T2Button.UseVisualStyleBackColor = true;
            T2Button.DarkModeEnabled = _darkModeEnabled;
            T2Button.PaintCustom += _owner.PlayOriginalGamesButtons_Paint;
            T2Button.Click += _owner.PlayOriginalGameButtons_Click;

            T2MPMenuButton.ArrowDirection = Direction.Up;
            T2MPMenuButton.Margin = new Padding(0);
            T2MPMenuButton.Size = new Size(16, 36);
            T2MPMenuButton.TabIndex = 3;
            T2MPMenuButton.UseVisualStyleBackColor = true;
            T2MPMenuButton.DarkModeEnabled = _darkModeEnabled;
            T2MPMenuButton.Click += _owner.PlayOriginalT2MPButton_Click;

            T3Button.Margin = new Padding(0);
            T3Button.MinimumSize = new Size(36, 36);
            T3Button.Size = new Size(36, 36);
            T3Button.TabIndex = 4;
            T3Button.UseVisualStyleBackColor = true;
            T3Button.DarkModeEnabled = _darkModeEnabled;
            T3Button.PaintCustom += _owner.PlayOriginalGamesButtons_Paint;
            T3Button.Click += _owner.PlayOriginalGameButtons_Click;

            SS2Button.Margin = new Padding(0);
            SS2Button.MinimumSize = new Size(36, 36);
            SS2Button.Size = new Size(36, 36);
            SS2Button.TabIndex = 5;
            SS2Button.UseVisualStyleBackColor = true;
            SS2Button.DarkModeEnabled = _darkModeEnabled;
            SS2Button.PaintCustom += _owner.PlayOriginalGamesButtons_Paint;
            SS2Button.Click += _owner.PlayOriginalGameButtons_Click;

            _owner.PlayOriginalFLP.Controls.Add(T1Button);
            _owner.PlayOriginalFLP.Controls.Add(T2Button);
            _owner.PlayOriginalFLP.Controls.Add(T2MPMenuButton);
            _owner.PlayOriginalFLP.Controls.Add(T3Button);
            _owner.PlayOriginalFLP.Controls.Add(SS2Button);

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
            _owner.MainToolTip.SetToolTip(T1Button, LText.PlayOriginalGameMenu.Thief1_PlayOriginal);
            _owner.MainToolTip.SetToolTip(T2Button, LText.PlayOriginalGameMenu.Thief2_PlayOriginal);
            _owner.MainToolTip.SetToolTip(T3Button, LText.PlayOriginalGameMenu.Thief3_PlayOriginal);
            _owner.MainToolTip.SetToolTip(SS2Button, LText.PlayOriginalGameMenu.SS2_PlayOriginal);
        }

        internal void SetMode(bool singleButton)
        {
            if (singleButton)
            {
                ConstructSingle();
                ButtonSingle.Show();

                if (_constructedMulti)
                {
                    T1Button.Hide();
                    T2Button.Hide();
                    T2MPMenuButton.Hide();
                    T3Button.Hide();
                    SS2Button.Hide();
                }
            }
            else
            {
                ConstructMulti();
                T1Button.Visible = !Config.GetGameExe(GameIndex.Thief1).IsEmpty();
                T2Button.Visible = !Config.GetGameExe(GameIndex.Thief2).IsEmpty();
                T2MPMenuButton.Visible = Config.T2MPDetected;
                T3Button.Visible = !Config.GetGameExe(GameIndex.Thief3).IsEmpty();
                SS2Button.Visible = !Config.GetGameExe(GameIndex.SS2).IsEmpty();

                if (_constructedSingle)
                {
                    ButtonSingle.Hide();
                }
            }
        }
    }
}

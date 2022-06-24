using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class ExitLLButton
    {
        private bool _constructed;

        private readonly MainForm _owner;

        private DarkButton Button = null!;

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

                Button.DarkModeEnabled = value;
            }
        }

        internal ExitLLButton(MainForm owner) => _owner = owner;

        internal void Localize()
        {
            if (!_constructed) return;
            Button.Text = LText.Global.Exit;
        }

        internal void SetVisible(bool enabled)
        {
            if (enabled)
            {
                if (!_constructed)
                {
                    var container = _owner.BottomRightButtonsFLP;

                    Button = new DarkButton
                    {
                        Tag = LoadType.Lazy,

                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        MinimumSize = new Size(36, 36),
                        TabIndex = 63,
                        UseVisualStyleBackColor = true,

                        DarkModeEnabled = _darkModeEnabled,
                    };

                    Button.Click += _owner.Exit_Click;

                    container.Controls.Add(Button);
                    container.Controls.SetChildIndex(Button, 0);

                    _constructed = true;

                    Localize();
                }

                Button.Show();
            }
            else
            {
                if (_constructed) Button.Hide();
            }
        }
    }
}

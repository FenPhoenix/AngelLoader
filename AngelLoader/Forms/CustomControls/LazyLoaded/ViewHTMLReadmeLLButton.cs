using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class ViewHTMLReadmeLLButton
    {
        private bool _constructed;
        private DarkButton Button = null!;

        private readonly MainForm _owner;

        private bool _darkModeEnabled;
        [PublicAPI]
        internal bool DarkModeEnabled
        {
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                Button.DarkModeEnabled = value;
            }
        }

        internal ViewHTMLReadmeLLButton(MainForm owner) => _owner = owner;

        internal void Localize()
        {
            if (_constructed) Button.Text = LText.ReadmeArea.ViewHTMLReadme;
        }

        internal void Center(Control parent)
        {
            if (_constructed) Button.CenterHV(parent);
        }

        internal bool Visible => _constructed && Button.Visible;

        internal void Hide()
        {
            if (_constructed) Button.Hide();
        }

        internal void Show()
        {
            if (!_constructed)
            {
                var container = _owner.MainSplitContainer.Panel2;

                Button = new DarkButton
                {
                    Tag = LoadType.Lazy,

                    Anchor = AnchorStyles.None,
                    AutoSize = true,
                    // This thing gets centered later so no location is specified here
                    Padding = new Padding(6, 0, 6, 0),
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    MinimumSize = new Size(0, 23),
                    TabIndex = 49,
                    UseVisualStyleBackColor = true,
                    Visible = false,

                    DarkModeEnabled = _darkModeEnabled
                };

                container.Controls.Add(Button);
                Button.Click += _owner.ViewHTMLReadmeButton_Click;
                Button.MouseLeave += _owner.ReadmeArea_MouseLeave;

                _constructed = true;

                Localize();
                Button.CenterHV(container);
            }

            Button.Show();
        }
    }
}

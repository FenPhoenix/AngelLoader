using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class Lazy_LangDetectError : IDarkable
    {
        private bool _constructed;

        private readonly MainForm _owner;

        private PictureBox _pictureBox = null!;

        private bool _darkModeEnabled;
        [PublicAPI]
        public bool DarkModeEnabled
        {
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (!_constructed) return;

                _pictureBox.Image = Images.RedExclCircle;
            }
        }

        internal Lazy_LangDetectError(MainForm owner) => _owner = owner;

        private void Construct()
        {
            if (_constructed) return;

            DarkButton b = _owner.EditFMScanLanguagesButton;
            _pictureBox = new PictureBox
            {
                Image = Images.RedExclCircle,
                Location = new Point(b.Left + b.Width + 2, b.Top + 4),
                Size = new Size(14, 14),
                TabStop = false,
                Visible = false,
            };

            _pictureBox.Click += static (_, _) => Core.OpenLogFile();

            _owner.EditFMTabPage.Controls.Add(_pictureBox);

            _constructed = true;
        }

        internal void SetVisible(bool visible)
        {
            if (visible)
            {
                Construct();
                _pictureBox.Show();
                Localize();
            }
            else
            {
                if (_constructed) _pictureBox.Hide();
            }
        }

        internal void Localize()
        {
            if (!_constructed) return;

            _owner.MainToolTip.SetToolTip(_pictureBox, LText.EditFMTab.ErrorDetectingFMSupportedLanguages_ToolTip);
        }
    }
}

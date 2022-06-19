using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.LazyLoaded
{
    internal sealed class Lazy_FMsListZoomButtons
    {
        private readonly MainForm _owner;

        private bool _constructed;

        internal readonly ToolStripButtonCustom[] Buttons = new ToolStripButtonCustom[3];

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

                RegenerateButtonImages();
            }
        }

        internal Lazy_FMsListZoomButtons(MainForm owner) => _owner = owner;

        private void RegenerateButtonImages()
        {
            for (int i = 0; i < 3; i++)
            {
                Buttons[i].Image?.Dispose();
                Buttons[i].Image = Images.GetZoomImage(Buttons[i].ContentRectangle, (Zoom)i, regenerate: true);
            }
        }

        private void Construct()
        {
            if (_constructed) return;

            // Insert them in reverse order so we always insert at 0
            for (int i = 2; i >= 0; i--)
            {
                var button = new ToolStripButtonCustom();
                Buttons[i] = button;
                _owner.RefreshAreaToolStrip.Items.Insert(0, button);
                button.AutoSize = false;
                button.DisplayStyle = ToolStripItemDisplayStyle.Image;
                button.Margin = new Padding(0);
                button.Size = new Size(25, 25);
                button.Click += _owner.FMsListZoomButtons_Click;
            }

            RegenerateButtonImages();

            _constructed = true;

            Localize();
        }

        internal void Localize()
        {
            if (!_constructed) return;

            Buttons[0].ToolTipText = LText.Global.ZoomIn;
            Buttons[1].ToolTipText = LText.Global.ZoomOut;
            Buttons[2].ToolTipText = LText.Global.ResetZoom;
        }

        internal void SetVisible(bool enabled)
        {
            if (enabled)
            {
                Construct();

                for (int i = 0; i < 3; i++)
                {
                    Buttons[i].Visible = true;
                }
            }
            else
            {
                if (!_constructed) return;

                for (int i = 0; i < 3; i++)
                {
                    Buttons[i].Visible = false;
                }
            }
        }
    }
}

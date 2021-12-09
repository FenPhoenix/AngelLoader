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
        private ToolStripButtonCustom ZoomInButton = null!;
        private ToolStripButtonCustom ZoomOutButton = null!;
        private ToolStripButtonCustom ResetZoomButton = null!;

        private bool _darkModeEnabled;
        [PublicAPI]
        internal bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                if (_constructed)
                {
                    RegenerateButtonImages();
                }
            }
        }

        internal Lazy_FMsListZoomButtons(MainForm owner) => _owner = owner;

        private void RegenerateButtonImages()
        {
            ZoomInButton.Image?.Dispose();
            ZoomOutButton.Image?.Dispose();
            ResetZoomButton.Image?.Dispose();

            ZoomInButton.Image = Images.GetZoomImage(ZoomInButton.ContentRectangle, Zoom.In, regenerate: true);
            ZoomOutButton.Image = Images.GetZoomImage(ZoomOutButton.ContentRectangle, Zoom.Out, regenerate: true);
            ResetZoomButton.Image = Images.GetZoomImage(ResetZoomButton.ContentRectangle, Zoom.Reset, regenerate: true);
        }

        private void Construct()
        {
            if (_constructed) return;

            // Insert them in reverse order so we always insert at 0
            ResetZoomButton = new ToolStripButtonCustom { Tag = LoadType.Lazy };
            _owner.RefreshAreaToolStrip.Items.Insert(0, ResetZoomButton);
            ResetZoomButton.AutoSize = false;
            ResetZoomButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ResetZoomButton.Margin = new Padding(0);
            ResetZoomButton.Size = new Size(25, 25);
            ResetZoomButton.Click += _owner.FMsListResetZoomButton_Click;

            ZoomOutButton = new ToolStripButtonCustom { Tag = LoadType.Lazy };
            _owner.RefreshAreaToolStrip.Items.Insert(0, ZoomOutButton);
            ZoomOutButton.AutoSize = false;
            ZoomOutButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomOutButton.Margin = new Padding(0);
            ZoomOutButton.Size = new Size(25, 25);
            ZoomOutButton.Click += _owner.FMsListZoomOutButton_Click;

            ZoomInButton = new ToolStripButtonCustom { Tag = LoadType.Lazy };
            _owner.RefreshAreaToolStrip.Items.Insert(0, ZoomInButton);
            ZoomInButton.AutoSize = false;
            ZoomInButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomInButton.Margin = new Padding(2, 0, 0, 0);
            ZoomInButton.Size = new Size(25, 25);
            ZoomInButton.Click += _owner.FMsListZoomInButton_Click;

            RegenerateButtonImages();

            _constructed = true;

            Localize();
        }

        internal void Localize()
        {
            if (!_constructed) return;

            ZoomInButton.ToolTipText = LText.Global.ZoomIn;
            ZoomOutButton.ToolTipText = LText.Global.ZoomOut;
            ResetZoomButton.ToolTipText = LText.Global.ResetZoom;
        }

        internal void SetVisible(bool enabled)
        {
            if (enabled)
            {
                Construct();

                ZoomInButton.Visible = true;
                ZoomOutButton.Visible = true;
                ResetZoomButton.Visible = true;
            }
            else
            {
                if (!_constructed) return;

                ZoomInButton.Visible = false;
                ZoomOutButton.Visible = false;
                ResetZoomButton.Visible = false;
            }
        }
    }
}

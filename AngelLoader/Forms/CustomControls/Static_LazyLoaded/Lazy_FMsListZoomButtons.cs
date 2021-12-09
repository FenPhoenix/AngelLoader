using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class Lazy_FMsListZoomButtons
    {
        private static bool _constructed;
        private static ToolStripButtonCustom ZoomInButton = null!;
        private static ToolStripButtonCustom ZoomOutButton = null!;
        private static ToolStripButtonCustom ResetZoomButton = null!;

        private static bool _darkModeEnabled;
        [PublicAPI]
        internal static bool DarkModeEnabled
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

        private static void RegenerateButtonImages()
        {
            ZoomInButton.Image?.Dispose();
            ZoomOutButton.Image?.Dispose();
            ResetZoomButton.Image?.Dispose();

            ZoomInButton.Image = Images.GetZoomImage(ZoomInButton.ContentRectangle, Zoom.In, regenerate: true);
            ZoomOutButton.Image = Images.GetZoomImage(ZoomOutButton.ContentRectangle, Zoom.Out, regenerate: true);
            ResetZoomButton.Image = Images.GetZoomImage(ResetZoomButton.ContentRectangle, Zoom.Reset, regenerate: true);
        }

        private static void Construct(MainForm owner)
        {
            if (_constructed) return;

            // Insert them in reverse order so we always insert at 0
            ResetZoomButton = new ToolStripButtonCustom { Tag = LoadType.Lazy };
            owner.RefreshAreaToolStrip.Items.Insert(0, ResetZoomButton);
            ResetZoomButton.AutoSize = false;
            ResetZoomButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ResetZoomButton.Margin = new Padding(0);
            ResetZoomButton.Size = new Size(25, 25);
            ResetZoomButton.Click += owner.FMsListResetZoomButton_Click;

            ZoomOutButton = new ToolStripButtonCustom { Tag = LoadType.Lazy };
            owner.RefreshAreaToolStrip.Items.Insert(0, ZoomOutButton);
            ZoomOutButton.AutoSize = false;
            ZoomOutButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomOutButton.Margin = new Padding(0);
            ZoomOutButton.Size = new Size(25, 25);
            ZoomOutButton.Click += owner.FMsListZoomOutButton_Click;

            ZoomInButton = new ToolStripButtonCustom { Tag = LoadType.Lazy };
            owner.RefreshAreaToolStrip.Items.Insert(0, ZoomInButton);
            ZoomInButton.AutoSize = false;
            ZoomInButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomInButton.Margin = new Padding(2, 0, 0, 0);
            ZoomInButton.Size = new Size(25, 25);
            ZoomInButton.Click += owner.FMsListZoomInButton_Click;

            RegenerateButtonImages();

            _constructed = true;

            Localize();
        }

        internal static void Localize()
        {
            if (!_constructed) return;

            ZoomInButton.ToolTipText = LText.Global.ZoomIn;
            ZoomOutButton.ToolTipText = LText.Global.ZoomOut;
            ResetZoomButton.ToolTipText = LText.Global.ResetZoom;
        }

        internal static void SetVisible(MainForm owner, bool enabled)
        {
            if (enabled)
            {
                Construct(owner);

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

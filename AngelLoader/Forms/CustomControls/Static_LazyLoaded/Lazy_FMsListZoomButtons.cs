using System.Drawing;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls.Static_LazyLoaded
{
    internal static class Lazy_FMsListZoomButtons
    {
        private static bool _constructed;
        private static ToolStripButtonCustom ZoomInButton = null!;
        private static ToolStripButtonCustom ZoomOutButton = null!;
        private static ToolStripButtonCustom ResetZoomButton = null!;

        private static void Construct(MainForm owner)
        {
            if (_constructed) return;

            // Insert them in reverse order so we always insert at 0
            ResetZoomButton = new ToolStripButtonCustom();
            owner.RefreshAreaToolStrip.Items.Insert(0, ResetZoomButton);
            ResetZoomButton.AutoSize = false;
            ResetZoomButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ResetZoomButton.Margin = new Padding(0);
            ResetZoomButton.Size = new Size(25, 25);
            ResetZoomButton.Image = Images.GetZoomImage(ResetZoomButton.ContentRectangle.Width, ResetZoomButton.ContentRectangle.Height, Zoom.Reset);
            ResetZoomButton.Click += owner.FMsListResetZoomButton_Click;

            ZoomOutButton = new ToolStripButtonCustom();
            owner.RefreshAreaToolStrip.Items.Insert(0, ZoomOutButton);
            ZoomOutButton.AutoSize = false;
            ZoomOutButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomOutButton.Margin = new Padding(0);
            ZoomOutButton.Size = new Size(25, 25);
            ZoomOutButton.Image = Images.GetZoomImage(ZoomOutButton.ContentRectangle.Width, ZoomOutButton.ContentRectangle.Height, Zoom.Out);
            ZoomOutButton.Click += owner.FMsListZoomOutButton_Click;

            ZoomInButton = new ToolStripButtonCustom();
            owner.RefreshAreaToolStrip.Items.Insert(0, ZoomInButton);
            ZoomInButton.AutoSize = false;
            ZoomInButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            ZoomInButton.Margin = new Padding(2, 0, 0, 0);
            ZoomInButton.Size = new Size(25, 25);
            ZoomInButton.Image = Images.GetZoomImage(ZoomInButton.ContentRectangle.Width, ZoomInButton.ContentRectangle.Height, Zoom.In);
            ZoomInButton.Click += owner.FMsListZoomInButton_Click;

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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms
{
    public sealed partial class MainForm
    {
        // Anything in MainForm doesn't need to be disposed because close = app exit
#pragma warning disable IDE0069 // Disposable fields should be disposed

        // It's ridiculous to instantiate two controls (a ToolStrip and a ToolStripSeparator contained within it)
        // just to draw two one-pixel-wide lines. Especially when there's a ton of them on the UI. For startup
        // perf and lightness of weight, we just draw them ourselves.


        // The reason these are in with the MainForm class is that they need to access a bunch of other controls'
        // sizes and locations in order to know where to paint. So we can't just put them in a static class and
        // pass params, unless we wanted to pass params for all the controls each one needs, which would be awful.

        private void PaintFilterBarFLP(PaintEventArgs e)
        {
            Pen s1Pen = Application.RenderWithVisualStyles ? ControlPainter.Sep1Pen : ControlPainter.Sep1PenC;
            const int y1 = 5;
            const int y2 = 20;
            {
                int bx = FilterTitleLabel.Location.X;
                int sep1x = bx - 6;
                int sep2x = bx - 5;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(ControlPainter.Sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            {
                int bx = FilterAuthorLabel.Location.X;
                int sep1x = bx - 6;
                int sep2x = bx - 5;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(ControlPainter.Sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }

        private void PaintRefreshAreaToolStrip(PaintEventArgs e)
        {
            Pen s1Pen = Application.RenderWithVisualStyles ? ControlPainter.Sep1Pen : ControlPainter.Sep1PenC;
            const int y1 = 5;
            const int y2 = 20;

            {
                int bx = RefreshFromDiskButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(ControlPainter.Sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            // Right side
            {
                int bx = ClearFiltersButton.Bounds.Location.X;
                int bw = ClearFiltersButton.Bounds.Width;
                int sep1x = bx + bw + 6;
                int sep2x = bx + bw + 7;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(ControlPainter.Sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }
    }
}

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public partial class MainForm
    {
        // It's ridiculous to instantiate two controls (a ToolStrip and a ToolStripSeparator contained within it)
        // just to draw two one-pixel-wide lines. Especially when there's a ton of them on the UI. For startup
        // perf and lightness of weight, we just draw them ourselves.

        // TODO: Switch all separators to realtime-draw.

        private readonly Pen sep1Pen = new Pen(Color.FromArgb(255, 189, 189, 189));
        private readonly Pen sep1PenC = new Pen(Color.FromArgb(255, 166, 166, 166));
        private readonly Pen sep2Pen = new Pen(Color.FromArgb(255, 255, 255, 255));

        private void PaintBottomLeftButtonsFLP(PaintEventArgs e)
        {
            // Visual style check takes 0.0000040ms, so my performance concerns were unfounded. Awesome.
            // This is to mimic standard separator behavior of slightly changing the darker color depending on
            // whether we're in classic mode or not. We're almost certainly still not 100% matching the standard
            // behavior, which I'm sure must be algorithmic (ie, background color + something?) because its colors
            // are always slightly off from the closest framework-defined color. But close enough is close enough.
            // Most people probably wouldn't even test or care about classic mode in the first place, so hey.
            var s1Pen = Application.RenderWithVisualStyles ? sep1Pen : sep1PenC;
            {
                int bx = ScanAllFMsButton.Location.X;
                int by = ScanAllFMsButton.Location.Y;
                int h = ScanAllFMsButton.Height - 5;
                int sep1x = bx - 8;
                int sep2x = bx - 7;
                e.Graphics.DrawLine(s1Pen, sep1x, by + 2, sep1x, by + 2 + h);
                e.Graphics.DrawLine(sep2Pen, sep2x, by + 3, sep2x, by + 3 + h);
            }

            {
                int bx = WebSearchButton.Location.X;
                int by = WebSearchButton.Location.Y;
                int h = WebSearchButton.Height - 5;
                int sep1x = bx - 8;
                int sep2x = bx - 7;
                e.Graphics.DrawLine(s1Pen, sep1x, by + 2, sep1x, by + 2 + h);
                e.Graphics.DrawLine(sep2Pen, sep2x, by + 3, sep2x, by + 3 + h);
            }
        }

        private void PaintFilterBarFLP(PaintEventArgs e)
        {
            var s1Pen = Application.RenderWithVisualStyles ? sep1Pen : sep1PenC;
            const int y1 = 5;
            const int y2 = 20;
            {
                int bx = FilterTitleLabel.Location.X;
                int sep1x = bx - 6;
                int sep2x = bx - 5;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            {
                int bx = FilterAuthorLabel.Location.X;
                int sep1x = bx - 6;
                int sep2x = bx - 5;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }

        private void PaintFilterIconButtonsToolStrip(PaintEventArgs e)
        {
            var s1Pen = Application.RenderWithVisualStyles ? sep1Pen : sep1PenC;
            const int y1 = 5;
            const int y2 = 20;

            {
                int bx = FilterByReleaseDateButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            {
                int bx = FilterByLastPlayedButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            {
                int bx = FilterByTagsButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            {
                int bx = FilterByFinishedButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            {
                int bx = FilterByRatingButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            {
                int bx = FilterShowUnsupportedButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }

        private void PaintRefreshAreaToolStrip(PaintEventArgs e)
        {
            var s1Pen = Application.RenderWithVisualStyles ? sep1Pen : sep1PenC;
            const int y1 = 5;
            const int y2 = 20;

            {
                int bx = RefreshFromDiskButton.Bounds.Location.X;
                int sep1x = bx - 3;
                int sep2x = bx - 2;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }

            // Right side
            {
                int bx = ClearFiltersButton.Bounds.Location.X;
                int bw = ClearFiltersButton.Bounds.Width;
                int sep1x = bx + bw + 6;
                int sep2x = bx + bw + 7;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }
    }
}

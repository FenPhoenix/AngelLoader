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

        private void BottomLeftButtonsFLP_Paint(object sender, PaintEventArgs e)
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
    }
}

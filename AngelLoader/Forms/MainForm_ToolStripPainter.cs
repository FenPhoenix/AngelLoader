using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    public partial class MainForm
    {
        private readonly Pen sep1Pen = new Pen(Color.FromArgb(255, 189, 189, 189));
        private readonly Pen sep2Pen = new Pen(Color.FromArgb(255, 255, 255, 255));

        private void BottomLeftButtonsFLP_Paint(object sender, PaintEventArgs e)
        {
            {
                int bx = ScanAllFMsButton.Location.X;
                int by = ScanAllFMsButton.Location.Y;
                int h = ScanAllFMsButton.Height - 5;
                int sep1x = bx - 8;
                int sep2x = bx - 7;
                e.Graphics.DrawLine(sep1Pen, sep1x, by + 2, sep1x, by + 2 + h);
                e.Graphics.DrawLine(sep2Pen, sep2x, by + 3, sep2x, by + 3 + h);
            }

            {
                int bx = WebSearchButton.Location.X;
                int by = WebSearchButton.Location.Y;
                int h = WebSearchButton.Height - 5;
                int sep1x = bx - 8;
                int sep2x = bx - 7;
                e.Graphics.DrawLine(sep1Pen, sep1x, by + 2, sep1x, by + 2 + h);
                e.Graphics.DrawLine(sep2Pen, sep2x, by + 3, sep2x, by + 3 + h);
            }
        }
    }
}

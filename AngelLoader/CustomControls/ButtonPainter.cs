using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    internal static class ButtonPainter
    {
        private static readonly SolidBrush playArrowBrush = new SolidBrush(Color.FromArgb(255, 45, 154, 47));
        private static readonly Point[] playArrowPoints = { new Point(15, 5), new Point(29, 17), new Point(15, 29) };

        internal static void PaintPlayFMButton(PaintEventArgs e)
        {
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            e.Graphics.FillPolygon(playArrowBrush, playArrowPoints);
        }

        internal static void PaintPlusButton(PaintEventArgs e)
        {
            var hRect = new Rectangle((e.ClipRectangle.Width / 2) - 4, (e.ClipRectangle.Height / 2), 10, 2);
            var vRect = new Rectangle((e.ClipRectangle.Width / 2), (e.ClipRectangle.Height / 2) - 4, 2, 10);
            e.Graphics.FillRectangles(Brushes.Black, new[] { hRect, vRect });
        }

        internal static void PaintMinusButton(PaintEventArgs e)
        {
            var hRect = new Rectangle((e.ClipRectangle.Width / 2) - 4, (e.ClipRectangle.Height / 2), 10, 2);
            e.Graphics.FillRectangle(Brushes.Black, hRect);
        }

        internal static void PaintExButton(PaintEventArgs e)
        {
            var wh = e.ClipRectangle.Width / 2;
            var hh = e.ClipRectangle.Height / 2;
            var ps = new[]
            {
                // top
                new Point(wh - 3, hh - 4),
                new Point(wh, hh - 1),
                new Point(wh + 3, hh - 4),

                // right
                new Point(wh + 4, hh - 3),
                new Point(wh + 1, hh),
                new Point(wh + 4, hh + 3),

                // bottom
                new Point(wh + 3, hh + 4),
                new Point(wh, hh + 1),
                new Point(wh - 3, hh + 4),

                // left
                new Point(wh - 4, hh + 3),
                new Point(wh - 1, hh),
                new Point(wh - 4, hh - 3),
            };

            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            e.Graphics.FillPolygon(Brushes.Black, ps);
        }

        private static readonly Rectangle ham1 = new Rectangle(1, 1, 14, 2);
        private static readonly Rectangle ham2 = new Rectangle(1, 7, 14, 2);
        private static readonly Rectangle ham3 = new Rectangle(1, 13, 14, 2);

        internal static void PaintTopRightMenuButton(PaintEventArgs e)
        {
            e.Graphics.FillRectangles(Brushes.Black, new[] { ham1, ham2, ham3 });
        }
    }
}

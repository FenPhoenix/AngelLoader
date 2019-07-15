using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    internal static class ButtonPainter
    {
        private static readonly SolidBrush playArrowBrush = new SolidBrush(Color.FromArgb(45, 154, 47));
        private static readonly Point[] playArrowPoints = { new Point(15, 5), new Point(29, 17), new Point(15, 29) };
        internal static void PaintPlayFMButton(bool enabled, PaintEventArgs e)
        {
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            e.Graphics.FillPolygon(enabled ? playArrowBrush : SystemBrushes.ControlDark, playArrowPoints);
        }

        internal static void PaintPlusButton(bool enabled, PaintEventArgs e)
        {
            var hRect = new Rectangle((e.ClipRectangle.Width / 2) - 4, (e.ClipRectangle.Height / 2), 10, 2);
            var vRect = new Rectangle((e.ClipRectangle.Width / 2), (e.ClipRectangle.Height / 2) - 4, 2, 10);
            e.Graphics.FillRectangles(enabled ? Brushes.Black : SystemBrushes.ControlDark, new[] { hRect, vRect });
        }

        internal static void PaintMinusButton(bool enabled, PaintEventArgs e)
        {
            var hRect = new Rectangle((e.ClipRectangle.Width / 2) - 4, (e.ClipRectangle.Height / 2), 10, 2);
            e.Graphics.FillRectangle(enabled ? Brushes.Black : SystemBrushes.ControlDark, hRect);
        }

        internal static void PaintExButton(bool enabled, PaintEventArgs e)
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
            e.Graphics.FillPolygon(enabled ? Brushes.Black : SystemBrushes.ControlDark, ps);
        }

        private static readonly Rectangle ham1 = new Rectangle(1, 1, 14, 2);
        private static readonly Rectangle ham2 = new Rectangle(1, 7, 14, 2);
        private static readonly Rectangle ham3 = new Rectangle(1, 13, 14, 2);
        internal static void PaintTopRightMenuButton(bool enabled, PaintEventArgs e)
        {
            e.Graphics.FillRectangles(enabled ? Brushes.Black : SystemBrushes.ControlDark, new[] { ham1, ham2, ham3 });
        }

        private static readonly Pen webSearchCirclePen = new Pen(Color.FromArgb(4, 125, 202), 2);
        private static readonly Pen webSearchCircleDisabledPen = new Pen(SystemColors.ControlDark, 2);
        private static readonly Rectangle webSearchRect1 = new Rectangle(12, 11, 19, 2);
        private static readonly RectangleF webSearchRect2 = new RectangleF(10, 16.5f, 23, 2);
        private static readonly Rectangle webSearchRect3 = new Rectangle(12, 22, 19, 2);
        internal static void PaintWebSearchButton(bool enabled, PaintEventArgs e)
        {
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            var pen = enabled ? webSearchCirclePen : webSearchCircleDisabledPen;

            e.Graphics.DrawEllipse(pen, 10, 6, 23, 23);
            e.Graphics.DrawEllipse(pen, 17, 6, 9, 23);
            e.Graphics.FillRectangles(pen.Brush, new[] { webSearchRect1, webSearchRect2, webSearchRect3 });
        }

        private static readonly Point[] readmeFullScreenTopLeft =
        {
            new Point(0, 0),
            new Point(7, 0),
            new Point(7, 3),
            new Point(3, 3),
            new Point(3, 7),
            new Point(0, 7)
        };
        private static readonly Point[] readmeFullScreenTopRight =
        {
            new Point(13, 0),
            new Point(20, 0),
            new Point(20, 7),
            new Point(17, 7),
            new Point(17, 3),
            new Point(13, 3)
        };
        private static readonly Point[] readmeFullScreenBottomLeft =
        {
            new Point(0, 13),
            new Point(3, 13),
            new Point(3, 17),
            new Point(7, 17),
            new Point(7, 20),
            new Point(0, 20)
        };
        private static readonly Point[] readmeFullScreenBottomRight =
        {
            new Point(17, 13),
            new Point(20, 13),
            new Point(20, 20),
            new Point(13, 20),
            new Point(13, 17),
            new Point(17, 17)
        };
        internal static void PaintReadmeFullScreenButton(bool enabled, PaintEventArgs e)
        {
            var brush = enabled ? Brushes.Black : SystemBrushes.ControlDark;

            e.Graphics.FillPolygon(brush, readmeFullScreenTopLeft);
            e.Graphics.FillPolygon(brush, readmeFullScreenTopRight);
            e.Graphics.FillPolygon(brush, readmeFullScreenBottomLeft);
            e.Graphics.FillPolygon(brush, readmeFullScreenBottomRight);
        }

        private static readonly Pen resetLayoutPen = new Pen(Color.FromArgb(123, 123, 123), 2);
        private static readonly Pen resetLayoutPenDisabled = new Pen(SystemColors.ControlDark, 2);
        internal static void PaintResetLayoutButton(bool enabled, PaintEventArgs e)
        {
            var pen = enabled ? resetLayoutPen : resetLayoutPenDisabled;
            e.Graphics.DrawRectangle(pen, 3, 3, 16, 16);
            e.Graphics.DrawLine(pen, 13, 2, 13, 10);
            e.Graphics.DrawLine(pen, 2, 11, 18, 11);
        }
    }
}

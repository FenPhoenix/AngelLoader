using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AngelLoader.Forms
{
    // Normally you would use images pulled from Resources for this. But to avoid bloating up our executable and
    // bogging down startup time, we just draw images ourselves where it's reasonable to do so.
    internal static class ControlPainter
    {
        internal static readonly Pen Sep1Pen = new Pen(Color.FromArgb(189, 189, 189));
        internal static readonly Pen Sep1PenC = new Pen(Color.FromArgb(166, 166, 166));
        internal static readonly Pen Sep2Pen = new Pen(Color.FromArgb(255, 255, 255));

        #region Global

        private static readonly Color _al_LightBlue = Color.FromArgb(4, 125, 202);

        #endregion

        #region Play arrow

        private static readonly SolidBrush _playArrowBrush = new SolidBrush(Color.FromArgb(45, 154, 47));
        private static readonly Point[] _playArrowPoints = { new Point(15, 5), new Point(29, 17), new Point(15, 29) };

        #endregion

        #region Plus

        private static readonly Rectangle[] _plusRects = new Rectangle[2];

        #endregion

        #region Hamburger

        private static readonly Rectangle[] _hamRects =
        {
            new Rectangle(1, 1, 14, 2),
            new Rectangle(1, 7, 14, 2),
            new Rectangle(1, 13, 14, 2)
        };

        #endregion

        #region Web search

        private static readonly Pen _webSearchCirclePen = new Pen(_al_LightBlue, 2);
        private static readonly Pen _webSearchCircleDisabledPen = new Pen(SystemColors.ControlDark, 2);
        private static readonly RectangleF[] _webSearchRects =
        {
            new Rectangle(12, 11, 19, 2),
            new RectangleF(10, 16.5f, 23, 2),
            new Rectangle(12, 22, 19, 2)
        };

        #endregion

        #region Readme fullscreen

        private static readonly Point[] _readmeFullScreenTopLeft =
        {
            new Point(0, 0),
            new Point(7, 0),
            new Point(7, 3),
            new Point(3, 3),
            new Point(3, 7),
            new Point(0, 7)
        };
        private static readonly Point[] _readmeFullScreenTopRight =
        {
            new Point(13, 0),
            new Point(20, 0),
            new Point(20, 7),
            new Point(17, 7),
            new Point(17, 3),
            new Point(13, 3)
        };
        private static readonly Point[] _readmeFullScreenBottomLeft =
        {
            new Point(0, 13),
            new Point(3, 13),
            new Point(3, 17),
            new Point(7, 17),
            new Point(7, 20),
            new Point(0, 20)
        };
        private static readonly Point[] _readmeFullScreenBottomRight =
        {
            new Point(17, 13),
            new Point(20, 13),
            new Point(20, 20),
            new Point(13, 20),
            new Point(13, 17),
            new Point(17, 17)
        };

        #endregion

        #region Reset layout

        private static readonly Pen _resetLayoutPen = new Pen(Color.FromArgb(123, 123, 123), 2);
        private static readonly Pen _resetLayoutPenDisabled = new Pen(SystemColors.ControlDark, 2);

        #endregion

        #region Scan

        private static readonly Pen _scanPen = new Pen(_al_LightBlue, 3);
        private static readonly Pen _scanDisabledPen = new Pen(SystemColors.ControlDark, 3);
        private static readonly Point[] _scanPoints =
        {
            new Point(26, 25),
            new Point(29, 28),
            new Point(32, 25),
            new Point(28, 21)
        };

        private static readonly Pen _scanSmallCirclePen = new Pen(_al_LightBlue, 1.8f);
        private static readonly Pen _scanSmallCircleDisabledPen = new Pen(SystemColors.ControlDark, 1.8f);
        private static readonly Pen _scanSmallHandlePen = new Pen(_al_LightBlue, 2.6f);
        private static readonly Pen _scanSmallHandleDisabledPen = new Pen(SystemColors.ControlDark, 2.6f);
        private static readonly RectangleF _scanSmallHandleRect = new RectangleF(15, 15, 2.4f, 2.4f);

        #endregion

        #region Buttons

        internal static void PaintPlayFMButton(Button button, PaintEventArgs e)
        {
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            e.Graphics.FillPolygon(button.Enabled ? _playArrowBrush : SystemBrushes.ControlDark, _playArrowPoints);
        }

        internal static void PaintPlusButton(Button button, PaintEventArgs e)
        {
            var hRect = new Rectangle((button.ClientRectangle.Width / 2) - 4, button.ClientRectangle.Height / 2, 10, 2);
            var vRect = new Rectangle(button.ClientRectangle.Width / 2, (button.ClientRectangle.Height / 2) - 4, 2, 10);
            (_plusRects[0], _plusRects[1]) = (hRect, vRect);
            e.Graphics.FillRectangles(button.Enabled ? Brushes.Black : SystemBrushes.ControlDark, _plusRects);
        }

        internal static void PaintMinusButton(Button button, PaintEventArgs e)
        {
            var hRect = new Rectangle((button.ClientRectangle.Width / 2) - 4, button.ClientRectangle.Height / 2, 10, 2);
            e.Graphics.FillRectangle(button.Enabled ? Brushes.Black : SystemBrushes.ControlDark, hRect);
        }

        internal static void PaintExButton(Button button, PaintEventArgs e)
        {
            int wh = button.ClientRectangle.Width / 2;
            int hh = button.ClientRectangle.Height / 2;
            Point[] ps =
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
                new Point(wh - 4, hh - 3)
            };

            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            e.Graphics.FillPolygon(button.Enabled ? Brushes.Black : SystemBrushes.ControlDark, ps);
        }

        internal static void PaintTopRightMenuButton(Button button, PaintEventArgs e)
        {
            e.Graphics.FillRectangles(button.Enabled ? Brushes.Black : SystemBrushes.ControlDark, _hamRects);
        }

        internal static void PaintWebSearchButton(Button button, PaintEventArgs e)
        {
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            Pen pen = button.Enabled ? _webSearchCirclePen : _webSearchCircleDisabledPen;

            e.Graphics.DrawEllipse(pen, 10, 6, 23, 23);
            e.Graphics.DrawEllipse(pen, 17, 6, 9, 23);
            e.Graphics.FillRectangles(pen.Brush, _webSearchRects);
        }

        internal static void PaintReadmeFullScreenButton(Button button, PaintEventArgs e)
        {
            Brush brush = button.Enabled ? Brushes.Black : SystemBrushes.ControlDark;

            e.Graphics.FillPolygon(brush, _readmeFullScreenTopLeft);
            e.Graphics.FillPolygon(brush, _readmeFullScreenTopRight);
            e.Graphics.FillPolygon(brush, _readmeFullScreenBottomLeft);
            e.Graphics.FillPolygon(brush, _readmeFullScreenBottomRight);
        }

        internal static void PaintResetLayoutButton(Button button, PaintEventArgs e)
        {
            Pen pen = button.Enabled ? _resetLayoutPen : _resetLayoutPenDisabled;
            e.Graphics.DrawRectangle(pen, 3, 3, 16, 16);
            e.Graphics.DrawLine(pen, 13, 2, 13, 10);
            e.Graphics.DrawLine(pen, 2, 11, 18, 11);
        }

        internal static void PaintScanAllFMsButton(Button button, PaintEventArgs e)
        {
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            Pen pen = button.Enabled ? _scanPen : _scanDisabledPen;

            e.Graphics.DrawEllipse(pen, 11, 7, 18, 18);
            e.Graphics.FillPolygon(pen.Brush, _scanPoints);
            e.Graphics.FillEllipse(pen.Brush, new RectangleF(29, 25, 4.5f, 4.5f));
        }

        internal static void PaintScanSmallButtons(Button button, PaintEventArgs e)
        {
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            Pen pen = button.Enabled ? _scanSmallCirclePen : _scanSmallCircleDisabledPen;
            Pen hPen = button.Enabled ? _scanSmallHandlePen : _scanSmallHandleDisabledPen;

            e.Graphics.DrawEllipse(pen, 4.25f, 4.25f, 10.6f, 10.6f);

            e.Graphics.DrawLine(hPen, 13, 13, 16.5f, 16.5f);
            e.Graphics.FillEllipse(pen.Brush, _scanSmallHandleRect);
        }

        #endregion

        #region Separators

        internal static void PaintToolStripSeparators(PaintEventArgs e, int pixelsFromVerticalEdges,
            params ToolStripItem[] items)
        {
            Pen s1Pen = Application.RenderWithVisualStyles ? Sep1Pen : Sep1PenC;

            int pfe = pixelsFromVerticalEdges;

            Rectangle sizeBounds = items[0].Bounds;

            int y1 = sizeBounds.Top + pfe;
            int y2 = sizeBounds.Bottom - pfe;

            for (int i = 0; i < items.Length; i++)
            {
                int bx = items[i].Bounds.Location.X;
                int l1s = (items[i].Margin.Left + (i > 0 ? items[i - 1].Margin.Right : 0)) / 2;
                int l2s = l1s - 1;
                int sep1x = bx - l1s;
                int sep2x = bx - l2s;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(Sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }

        internal static void PaintControlSeparators(PaintEventArgs e, int pixelsFromVerticalEdges,
            params Control[] items)
        {
            Pen s1Pen = Application.RenderWithVisualStyles ? Sep1Pen : Sep1PenC;

            int pfe = pixelsFromVerticalEdges;

            Rectangle sizeBounds = items[0].Bounds;

            int y1 = sizeBounds.Top + pfe;
            int y2 = (sizeBounds.Bottom - pfe) - 1;

            for (int i = 0; i < items.Length; i++)
            {
                int bx = items[i].Bounds.Location.X;
                int l1s = (int)Math.Ceiling((double)items[i].Margin.Left / 2);
                int l2s = l1s - 1;
                int sep1x = bx - l1s;
                int sep2x = bx - l2s;
                e.Graphics.DrawLine(s1Pen, sep1x, y1, sep1x, y2);
                e.Graphics.DrawLine(Sep2Pen, sep2x, y1 + 1, sep2x, y2 + 1);
            }
        }

        #endregion
    }
}

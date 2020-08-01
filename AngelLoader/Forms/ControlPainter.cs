using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    // Normally you would use images pulled from Resources for this. But to avoid bloating up our executable and
    // bogging down startup time, we just draw images ourselves where it's reasonable to do so.
    internal static class ControlPainter
    {
        #region Path points and types

        #region Magnifying glass

        // These could be deflate-compressed to save space, or I could scrap the points and just draw a few shapes
        // on the GraphicsPath if I could figure out how to union them together (rather than one cutting a piece
        // out of the other like it does currently) and that would save the most space. Wouldn't necessarily work
        // for every possible image, but some of them at least would be amenable to that.
        // Regardless, with this empty magnifying glass path, I can get 13 images worth of mileage out of it by
        // itself or in combination with +, -, and reset-zoom symbols. So I get my space's worth out of this one
        // for sure. It'll be the same with the finished-on icons when I come to those.
        // (this array init code was generated)
        private static readonly float[] _magnifierEmptyPoints =
        {
            59.19173f, 0f, 26.60027f, -2.65E-06f, 0f, 26.60027f, 0f, 59.19173f, 0f, 91.7832f, 26.60027f,
            118.383f, 59.19173f, 118.3829f, 70.74734f, 118.3829f, 81.54756f, 115.036f, 90.67818f, 109.2667f,
            113.498f, 132.0865f, 118.3931f, 136.9816f, 126.2747f, 136.9816f, 131.1698f, 132.0865f, 131.6421f,
            131.6142f, 136.5372f, 126.7191f, 136.5372f, 118.8375f, 131.6421f, 113.9424f, 108.921f, 91.2213f,
            114.9029f, 81.97736f, 118.3829f, 70.97697f, 118.3829f, 59.19173f, 118.3829f, 26.60027f, 91.7832f, 0f,
            59.19173f, 0f, 59.19173f, 16.70131f, 82.75785f, 16.70131f, 101.6816f, 35.62561f, 101.6816f,
            59.19174f, 101.6816f, 82.75784f, 82.75786f, 101.6837f, 59.19173f, 101.6837f, 35.62562f, 101.6837f,
            16.69924f, 82.75786f, 16.69924f, 59.19174f, 16.69924f, 35.62562f, 35.62562f, 16.70131f, 59.19173f,
            16.70131f, 59.19173f, 16.70131f
        };

        private static readonly byte[] _magnifierEmptyTypes =
        {
            0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 3, 3, 131, 0, 3, 3, 3, 3, 3, 3, 3,
            3, 3, 3, 3, 3, 129
        };

        private static GraphicsPath? _magnifierEmptyGPath;
        private static GraphicsPath MagnifierEmptyGPath => _magnifierEmptyGPath ??= MakeGraphicsPath(_magnifierEmptyPoints, _magnifierEmptyTypes);

        #endregion

        #region Zoom symbols

        private static readonly float[][] _zoomTypePoints =
        {
            // Zoom in (plus)
            new[]
            {
                51.71002f, 34.41495f, 51.71002f, 51.30281f, 34.82216f, 51.30281f, 34.82216f, 65.74844f,
                51.71002f, 65.74844f, 51.71002f, 82.63629f, 66.15565f, 82.63629f, 66.15565f, 65.74844f, 83.0435f,
                65.74844f, 83.0435f, 51.30281f, 66.15565f, 51.30281f, 66.15565f, 34.41495f, 51.71002f, 34.41495f
            },
            // Zoom out (minus)
            new[]
            {
                83.04366f, 65.74826f, 83.04366f, 51.30262f, 34.8222f, 51.30262f, 34.8222f, 65.74826f, 83.04366f,
                65.74826f
            },
            // Zoom reset
            new[]
            {
                35.39526f, 35.31619f, 35.39526f, 47.97226f, 35.39526f, 63.18219f, 48.05133f, 63.18219f,
                48.05133f, 47.97226f, 63.26177f, 47.97226f, 63.26177f, 35.31619f, 48.05133f, 35.31619f,
                35.39526f, 35.31619f, 69.87067f, 55.11757f, 69.87067f, 70.32749f, 54.66074f, 70.32749f,
                54.66074f, 82.98357f, 69.87067f, 82.98357f, 82.52675f, 82.98357f, 82.52675f, 70.32749f,
                82.52675f, 55.11757f, 69.87067f, 55.11757f
            }
        };

        private static readonly byte[][] _zoomTypeTypes =
        {
            // Zoom in (plus)
            new byte[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129 },
            // Zoom out (minus)
            new byte[] { 0, 1, 1, 1, 129 },
            // Zoom reset
            new byte[] { 0, 1, 1, 1, 1, 1, 1, 1, 129, 0, 1, 1, 1, 1, 1, 1, 1, 129 }
        };

        private static readonly GraphicsPath?[] _zoomImagesComplete = new GraphicsPath[ZoomTypesCount];

        public static GraphicsPath GetZoomImageComplete(Zoom zoomType)
        {
            int index = (int)zoomType;
            if (_zoomImagesComplete[index] == null)
            {
                var gp = new GraphicsPath();
                gp.AddPath(MagnifierEmptyGPath, true);
                gp.AddPath(MakeGraphicsPath(_zoomTypePoints[index], _zoomTypeTypes[index]), true);

                _zoomImagesComplete[index] = gp;
            }
            return _zoomImagesComplete[index]!;
        }

        #endregion

        #region Finished checkmarks

        internal static readonly Brush NormalCheckOutlineBrush = new SolidBrush(Color.FromArgb(3, 100, 1));
        internal static readonly Brush NormalCheckFillBrush = new SolidBrush(Color.FromArgb(0, 170, 0));

        internal static readonly Brush HardCheckOutlineBrush = new SolidBrush(Color.FromArgb(196, 157, 2));
        internal static readonly Brush HardCheckFillBrush = new SolidBrush(Color.FromArgb(255, 210, 0));

        internal static readonly Brush ExpertCheckOutlineBrush = new SolidBrush(Color.FromArgb(135, 2, 2));
        internal static readonly Brush ExpertCheckFillBrush = new SolidBrush(Color.FromArgb(216, 0, 0));

        internal static readonly Brush ExtremeCheckOutlineBrush = new SolidBrush(Color.FromArgb(19, 1, 100));
        internal static readonly Brush ExtremeCheckFillBrush = new SolidBrush(Color.FromArgb(0, 53, 226));

        internal static readonly Brush UnknownCheckOutlineBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
        internal static readonly Brush UnknownCheckFillBrush = new SolidBrush(Color.FromArgb(170, 170, 170));

        internal static readonly Brush FinishedOnFilterOutlineBrush = new SolidBrush(Color.FromArgb(14, 101, 139));
        internal static readonly Brush FinishedOnFilterFillBrush = new SolidBrush(Color.FromArgb(89, 159, 203));

        // Inner path starts at index 14
        private static readonly float[] _finishedCheckPoints =
        {
            65.75464f, 3.180167f, 29.22405f, 47.79398f, 10.8f, 30.83739f, -0.1080037f, 42.47338f, 30.86943f,
            71.04063f, 78.09653f, 13.29531f, 65.75464f, 3.180167f, 66.03886f, 7.544f, 73.736f, 13.58057f,
            30.66118f, 66.582f, 4.278f, 42.36847f, 10.90528f, 35.199f, 29.43231f, 52.25f, 66.03886f, 7.544f
        };

        // Inner path starts at index 7
        private static readonly byte[] _finishedCheckTypes =
        {
            0, 1, 1, 1, 1, 1, 129, 0, 1, 1, 1, 1, 1, 129
        };

        internal static readonly PointF[] FinishedCheckInnerPoints = new PointF[7];
        internal static readonly byte[] FinishedCheckInnerTypes = new byte[7];

        private static GraphicsPath? _finishedCheckGPath;
        internal static GraphicsPath FinishedCheckOutlineGPath => _finishedCheckGPath ??= MakeGraphicsPath(_finishedCheckPoints, _finishedCheckTypes);

        #endregion

        #region Stars

        private static readonly float[] _starEmptyPoints = new float[66]
        {
            /* outer */
            50f, 0f, 36f, 33f, 0f, 36f, 27f, 60f, 19f, 95f, 50f, 76f, 81f, 95f, 73f, 60f, 100f, 36f, 64f, 33f,
            50f, 0f,
            /* middle */
            50f, 9f, 61.5f, 36.5f, 91f, 39f, 69f, 58.5f, 75.5f, 87.5f, 50f, 72f, 24.5f, 87.5f, 31f, 58.5f, 9f,
            39f, 38.5f, 36.5f, 50f, 9f,
            /* inner */
            50f, 22f, 42f, 41f, 21f, 43f, 36.5f, 56.5f, 32f, 77f, 50f, 66f, 68f, 77f, 63.5f, 56.5f, 79f, 43f,
            58f, 41f, 50f, 22f
        };

        private static readonly byte[] _starEmptyTypes = new byte[33]
        {
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129,
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129,
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129
        };

        private static readonly float[] _starRightEmptyPoints = new float[]
        {
            /* outer */
            50f, 0f, 36f, 33f, 0f, 36f, 27f, 60f, 19f, 95f, 50f, 76f, 81f, 95f, 73f, 60f, 100f, 36f, 64f, 33f,
            50f, 0f,
            /* middle */
            50f, 9f, 61.5f, 36.5f, 91f, 39f, 69f, 58.5f, 75.5f, 87.5f, 50f, 72f, 24.5f, 87.5f, 31f, 58.5f, 9f,
            39f, 38.5f, 36.5f, 50f, 9f,
            /* inner */
            50f, 66f, 68f, 77f, 63.5f, 56.5f, 79f, 43f, 58f, 41f, 50f, 22f, 50f, 66f
        };

        private static readonly byte[] _starRightEmptyTypes = new byte[]
        {
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129,
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129,
            0, 1, 1, 1, 1, 1, 129
        };

        private static readonly float[] _starFullPoints = new float[44]
        {
            /* outer */
            50f, 0f, 36f, 33f, 0f, 36f, 27f, 60f, 19f, 95f, 50f, 76f, 81f, 95f, 73f, 60f, 100f, 36f, 64f, 33f,
            50f, 0f,
            /* middle */
            50f, 9f, 61.5f, 36.5f, 91f, 39f, 69f, 58.5f, 75.5f, 87.5f, 50f, 72f, 24.5f, 87.5f, 31f, 58.5f, 9f,
            39f, 38.5f, 36.5f, 50f, 9f
        };

        private static readonly byte[] _starFullTypes = new byte[22]
        {
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129,
            0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129
        };

        private static GraphicsPath? _starEmptyGPath;
        internal static GraphicsPath StarEmptyGPath => _starEmptyGPath ??= MakeGraphicsPath(_starEmptyPoints, _starEmptyTypes);

        private static GraphicsPath? _starRightEmptyGPath;
        internal static GraphicsPath StarRightEmptyGPath => _starRightEmptyGPath ??= MakeGraphicsPath(_starRightEmptyPoints, _starRightEmptyTypes);

        private static GraphicsPath? _starFullGPath;
        internal static GraphicsPath StarFullGPath => _starFullGPath ??= MakeGraphicsPath(_starFullPoints, _starFullTypes);

        internal static readonly Brush StarOutlineBrush = new SolidBrush(Color.FromArgb(192, 113, 0));
        internal static readonly Brush StarFillBrush = new SolidBrush(Color.FromArgb(255, 180, 0));

        #endregion

        #endregion

        #region Vector helpers

        private static GraphicsPath MakeGraphicsPath(float[] points, byte[] types)
        {
            int pointsCount = points.Length;
            var rawPoints = new PointF[pointsCount / 2];
            for (int i = 0, j = 0; i < pointsCount; i += 2, j++)
            {
                var x = points[i];
                var y = points[i + 1];
                rawPoints[j] = new PointF(x, y);
            }
            return new GraphicsPath(rawPoints, types);
        }

        internal static void FitRectInBounds(Graphics g, RectangleF drawRect, RectangleF boundsRect)
        {
            if (boundsRect.Width < 1 || boundsRect.Height < 1) return;

            g.ResetTransform();

            // Set scale origin
            float drawRectCenterX = drawRect.Left + (drawRect.Width / 2);
            float drawRectCenterY = drawRect.Top + (drawRect.Height / 2);
            // Er, yeah, I don't actually know why these have to be negated... but it works, so oh well...?
            g.TranslateTransform(-drawRectCenterX, -drawRectCenterY, MatrixOrder.Append);

            // Scale graphic
            float scaleBothAxes = Math.Min(boundsRect.Width / drawRect.Width, boundsRect.Height / drawRect.Height);
            g.ScaleTransform(scaleBothAxes, scaleBothAxes, MatrixOrder.Append);

            // Center graphic in bounding rectangle
            float boundsRectCenterX = boundsRect.Left + (boundsRect.Width / 2);
            float boundsRectCenterY = boundsRect.Top + (boundsRect.Height / 2);
            g.TranslateTransform(boundsRectCenterX, boundsRectCenterY, MatrixOrder.Append);
        }

        private static void SetSmoothingMode(PaintEventArgs e, SmoothingMode mode)
        {
            if (e.Graphics.SmoothingMode != mode) e.Graphics.SmoothingMode = mode;
        }

        #endregion

        #region Global

        internal static readonly Pen Sep1Pen = new Pen(Color.FromArgb(189, 189, 189));
        internal static readonly Pen Sep1PenC = new Pen(Color.FromArgb(166, 166, 166));
        internal static readonly Pen Sep2Pen = new Pen(Color.FromArgb(255, 255, 255));

        private static readonly Color _al_LightBlue = Color.FromArgb(4, 125, 202);
        private static readonly Brush _al_LightBlueBrush = new SolidBrush(_al_LightBlue);

        internal static Pen GetSeparatorPenForCurrentVisualStyleMode() => Application.RenderWithVisualStyles ? Sep1Pen : Sep1PenC;

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

        /*
        private static readonly Pen _scanPen = new Pen(_al_LightBlue, 3);
        private static readonly Pen _scanDisabledPen = new Pen(SystemColors.ControlDark, 3);
        private static readonly Point[] _scanPoints =
        {
            new Point(26, 25),
            new Point(29, 28),
            new Point(32, 25),
            new Point(28, 21)
        };
        */

        private static readonly Pen _scanSmallCirclePen = new Pen(_al_LightBlue, 1.8f);
        private static readonly Pen _scanSmallCircleDisabledPen = new Pen(SystemColors.ControlDark, 1.8f);
        private static readonly Pen _scanSmallHandlePen = new Pen(_al_LightBlue, 2.6f);
        private static readonly Pen _scanSmallHandleDisabledPen = new Pen(SystemColors.ControlDark, 2.6f);
        private static readonly RectangleF _scanSmallHandleRect = new RectangleF(15, 15, 2.4f, 2.4f);

        #endregion

        #region Buttons

        internal static void PaintZoomButtons(Button button, PaintEventArgs e, Zoom zoomType)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

            Brush brush = button.Enabled ? Brushes.Black : SystemBrushes.ControlDark;

            var gPath = GetZoomImageComplete(zoomType);
            FitRectInBounds(e.Graphics, gPath.GetBounds(), button.ClientRectangle);
            e.Graphics.FillPath(brush, gPath);
        }

        internal static void PaintPlayFMButton(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);
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

            SetSmoothingMode(e, SmoothingMode.AntiAlias);
            e.Graphics.FillPolygon(button.Enabled ? Brushes.Black : SystemBrushes.ControlDark, ps);
        }

        internal static void PaintTopRightMenuButton(Button button, PaintEventArgs e)
        {
            e.Graphics.FillRectangles(button.Enabled ? Brushes.Black : SystemBrushes.ControlDark, _hamRects);
        }

        internal static void PaintWebSearchButton(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

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
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

            /*
            Pen pen = button.Enabled ? _scanPen : _scanDisabledPen;

            e.Graphics.DrawEllipse(pen, 11, 7, 18, 18);
            e.Graphics.FillPolygon(pen.Brush, _scanPoints);
            e.Graphics.FillEllipse(pen.Brush, new RectangleF(29, 25, 4.5f, 4.5f));
            */

            Brush brush = button.Enabled ? _al_LightBlueBrush : SystemBrushes.ControlDark;

            var cr = button.ClientRectangle;

            FitRectInBounds(
                e.Graphics,
                MagnifierEmptyGPath.GetBounds(),
                new RectangleF(
                    (cr.X + button.Padding.Left) - (cr.Height - 12),
                    cr.Y + 6,
                    cr.Height - 12,
                    cr.Height - 12));
            e.Graphics.FillPath(brush, MagnifierEmptyGPath);
        }

        internal static void PaintScanSmallButtons(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

            /*
            Pen pen = button.Enabled ? _scanSmallCirclePen : _scanSmallCircleDisabledPen;
            Pen hPen = button.Enabled ? _scanSmallHandlePen : _scanSmallHandleDisabledPen;

            e.Graphics.DrawEllipse(pen, 4.25f, 4.25f, 10.6f, 10.6f);

            e.Graphics.DrawLine(hPen, 13, 13, 16.5f, 16.5f);
            e.Graphics.FillEllipse(pen.Brush, _scanSmallHandleRect);
            */

            Brush brush = button.Enabled ? _al_LightBlueBrush : SystemBrushes.ControlDark;

            var cr = button.ClientRectangle;

            FitRectInBounds(
                e.Graphics,
                MagnifierEmptyGPath.GetBounds(),
                new RectangleF(
                    (cr.X + 3f),//- (cr.Height - 4),
                    cr.Y + 3f,
                    cr.Height - 7,
                    cr.Height - 7));
            e.Graphics.FillPath(brush, MagnifierEmptyGPath);
        }

        #endregion

        #region Separators

        // It's ridiculous to instantiate two controls (a ToolStrip and a ToolStripSeparator contained within it)
        // just to draw two one-pixel-wide lines. Especially when there's a ton of them on the UI. For startup
        // perf and lightness of weight, we just draw them ourselves.

        internal static void PaintToolStripSeparators(PaintEventArgs e, int pixelsFromVerticalEdges,
            params ToolStripItem[] items)
        {
            Pen s1Pen = GetSeparatorPenForCurrentVisualStyleMode();

            Rectangle sizeBounds = items[0].Bounds;

            int y1 = sizeBounds.Top + pixelsFromVerticalEdges;
            int y2 = sizeBounds.Bottom - pixelsFromVerticalEdges;

            for (int i = 0; i < items.Length; i++)
            {
                int l1s = (int)Math.Ceiling((double)items[i].Margin.Left / 2);
                DrawSeparator(e, s1Pen, l1s, y1, y2, items[i].Bounds.Location.X);
            }
        }

        internal static void PaintControlSeparators(
            PaintEventArgs e,
            int pixelsFromVerticalEdges,
            int topOverride = -1,
            int bottomOverride = -1,
            params Control[] items)
        {
            Pen s1Pen = GetSeparatorPenForCurrentVisualStyleMode();

            Rectangle sizeBounds = items[0].Bounds;

            int y1 = topOverride > -1 ? topOverride : sizeBounds.Top + pixelsFromVerticalEdges;
            int y2 = bottomOverride > -1 ? bottomOverride : (sizeBounds.Bottom - pixelsFromVerticalEdges) - 1;

            for (int i = 0; i < items.Length; i++)
            {
                int l1s = (int)Math.Ceiling((double)items[i].Margin.Left / 2);
                DrawSeparator(e, s1Pen, l1s, y1, y2, items[i].Bounds.Location.X);
            }
        }

        internal static void DrawSeparator(
            PaintEventArgs e,
            Pen line1Pen,
            int line1DistanceBackFromLoc,
            int line1Top,
            int line1Bottom,
            int x)
        {
            int sep1x = x - line1DistanceBackFromLoc;
            int sep2x = (x - line1DistanceBackFromLoc) + 1;
            e.Graphics.DrawLine(line1Pen, sep1x, line1Top, sep1x, line1Bottom);
            e.Graphics.DrawLine(Sep2Pen, sep2x, line1Top + 1, sep2x, line1Bottom + 1);
        }

        #endregion
    }
}

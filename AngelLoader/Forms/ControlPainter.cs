﻿using System;
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
        private static readonly float[] _magnifying_glass_empty_exported_points_raw = new float[78]
        {
            223.7168f, 0f, 100.5365f, -1.001575E-05f, 0f, 100.5364f, 0f, 223.7168f, 0f, 346.8972f, 100.5365f,
            447.4317f, 223.7168f, 447.4316f, 267.3915f, 447.4316f, 308.2113f, 434.7819f, 342.7207f, 412.9766f,
            428.9688f, 499.2246f, 447.4698f, 517.7256f, 477.2587f, 517.7257f, 495.7598f, 499.2246f, 497.5449f,
            497.4395f, 516.046f, 478.9384f, 516.046f, 449.1495f, 497.5449f, 430.6484f, 411.6699f, 344.7734f,
            434.2787f, 309.8357f, 447.4316f, 268.2595f, 447.4316f, 223.7168f, 447.4316f, 100.5364f, 346.8972f,
            1.894781E-14f, 223.7168f, 0f, 223.7168f, 63.12305f, 312.7856f, 63.12305f, 384.3086f, 134.648f,
            384.3086f, 223.7168f, 384.3086f, 312.7856f, 312.7856f, 384.3164f, 223.7168f, 384.3164f, 134.648f,
            384.3164f, 63.11523f, 312.7856f, 63.11523f, 223.7168f, 63.11523f, 134.648f, 134.648f, 63.12305f,
            223.7168f, 63.12305f, 223.7168f, 63.12305f
        };

        private static readonly byte[] _magnifying_glass_empty_exported_types_raw = new byte[39]
        {
            0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 3, 3, 131, 0, 3, 3, 3, 3, 3, 3, 3,
            3, 3, 3, 3, 3, 129
        };

        #endregion

        #region Zoom minus

        private static readonly float[] _zoom_minus_symbol_exported_points_raw =
        {
            223.7168f, 0f, 100.5365f, -7.559055E-06f, -3.789561E-14f, 100.5364f, 0f, 223.7168f, 0f, 346.8972f,
            100.5365f, 447.4317f, 223.7168f, 447.4316f, 267.3916f, 447.4316f, 308.2113f, 434.7819f, 342.7207f,
            412.9766f, 428.9688f, 499.2246f, 447.4698f, 517.7256f, 477.2587f, 517.7257f, 495.7598f, 499.2246f,
            497.5449f, 497.4395f, 516.046f, 478.9384f, 516.046f, 449.1495f, 497.5449f, 430.6484f, 411.6699f,
            344.7734f, 434.2787f, 309.8357f, 447.4316f, 268.2595f, 447.4316f, 223.7168f, 447.4316f, 100.5364f,
            346.8972f, 0f, 223.7168f, 0f, 223.7168f, 63.12305f, 312.7856f, 63.12305f, 384.3086f, 134.648f,
            384.3086f, 223.7168f, 384.3086f, 312.7856f, 312.7856f, 384.3164f, 223.7168f, 384.3164f, 134.648f,
            384.3164f, 63.11523f, 312.7856f, 63.11523f, 223.7168f, 63.11523f, 134.648f, 134.648f, 63.12305f,
            223.7168f, 63.12305f, 223.7168f, 63.12305f, 111.1855f, 188.3711f, 111.1855f, 254.0723f, 334.8066f,
            254.0723f, 334.8066f, 188.3711f, 111.1855f, 188.3711f, 111.1855f, 188.3711f
        };

        private static readonly byte[] _zoom_minus_symbol_exported_types_raw =
        {
            0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 3, 3, 131, 0, 3, 3, 3, 3, 3, 3, 3,
            3, 3, 3, 3, 3, 129, 0, 1, 1, 1, 1, 129
        };

        #endregion

        #region Zoom plus

        private static readonly float[] _zoom_plus_symbol_exported_points_raw =
        {
            223.7168f, 0f, 100.5365f, -1.001575E-05f, 0f, 100.5364f, 0f, 223.7168f, 0f, 346.8972f, 100.5365f,
            447.4317f, 223.7168f, 447.4316f, 267.3915f, 447.4316f, 308.2113f, 434.7819f, 342.7207f, 412.9766f,
            428.9688f, 499.2246f, 447.4698f, 517.7256f, 477.2587f, 517.7257f, 495.7598f, 499.2246f, 497.5449f,
            497.4395f, 516.046f, 478.9384f, 516.046f, 449.1495f, 497.5449f, 430.6484f, 411.6699f, 344.7734f,
            434.2787f, 309.8357f, 447.4316f, 268.2595f, 447.4316f, 223.7168f, 447.4316f, 100.5364f, 346.8972f,
            1.894781E-14f, 223.7168f, 0f, 223.7168f, 63.12305f, 312.7856f, 63.12305f, 384.3086f, 134.648f,
            384.3086f, 223.7168f, 384.3086f, 312.7856f, 312.7856f, 384.3164f, 223.7168f, 384.3164f, 134.648f,
            384.3164f, 63.11523f, 312.7856f, 63.11523f, 223.7168f, 63.11523f, 134.648f, 134.648f, 63.12305f,
            223.7168f, 63.12305f, 223.7168f, 63.12305f, 190.1445f, 109.4141f, 190.1445f, 188.3711f, 111.1855f,
            188.3711f, 111.1855f, 254.0723f, 190.1445f, 254.0723f, 190.1445f, 333.0293f, 255.8457f, 333.0293f,
            255.8457f, 254.0723f, 334.8066f, 254.0723f, 334.8066f, 188.3711f, 255.8457f, 188.3711f, 255.8457f,
            109.4141f, 190.1445f, 109.4141f, 190.1445f, 109.4141f
        };

        private static readonly byte[] _zoom_plus_symbol_exported_types_raw =
        {
            0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 3, 3, 131, 0, 3, 3, 3, 3, 3, 3, 3,
            3, 3, 3, 3, 3, 129, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 129
        };

        #endregion

        #region Zoom reset

        private static readonly float[] _zoom_reset_symbol_exported_points_raw =
        {
            223.7168f, 0f, 100.5365f, -1.001575E-05f, 0f, 100.5364f, 0f, 223.7168f, 0f, 346.8972f, 100.5365f,
            447.4317f, 223.7168f, 447.4316f, 267.3915f, 447.4316f, 308.2113f, 434.7819f, 342.7207f, 412.9766f,
            428.9688f, 499.2246f, 447.4698f, 517.7256f, 477.2587f, 517.7257f, 495.7598f, 499.2246f, 497.5449f,
            497.4395f, 516.046f, 478.9384f, 516.046f, 449.1495f, 497.5449f, 430.6484f, 411.6699f, 344.7734f,
            434.2787f, 309.8357f, 447.4316f, 268.2595f, 447.4316f, 223.7168f, 447.4316f, 100.5364f, 346.8972f,
            1.894781E-14f, 223.7168f, 0f, 223.7168f, 63.12305f, 312.7856f, 63.12305f, 384.3086f, 134.648f,
            384.3086f, 223.7168f, 384.3086f, 312.7856f, 312.7856f, 384.3164f, 223.7168f, 384.3164f, 134.648f,
            384.3164f, 63.11523f, 312.7856f, 63.11523f, 223.7168f, 63.11523f, 134.648f, 134.648f, 63.12305f,
            223.7168f, 63.12305f, 223.7168f, 63.12305f, 133.7773f, 133.4785f, 133.7773f, 181.3125f, 133.7773f,
            238.7988f, 181.6113f, 238.7988f, 181.6113f, 181.3125f, 239.0996f, 181.3125f, 239.0996f, 133.4785f,
            181.6113f, 133.4785f, 133.7773f, 133.4785f, 133.7773f, 133.4785f, 264.0781f, 208.3184f, 264.0781f,
            265.8047f, 206.5918f, 265.8047f, 206.5918f, 313.6387f, 264.0781f, 313.6387f, 311.9121f, 313.6387f,
            311.9121f, 265.8047f, 311.9121f, 208.3184f, 264.0781f, 208.3184f, 264.0781f, 208.3184f
        };

        private static readonly byte[] _zoom_reset_symbol_exported_types_raw =
        {
            0, 3, 3, 3, 3, 3, 3, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 1, 3, 3, 3, 3, 3, 131, 0, 3, 3, 3, 3, 3, 3, 3,
            3, 3, 3, 3, 3, 129, 0, 1, 1, 1, 1, 1, 1, 1, 1, 129, 0, 1, 1, 1, 1, 1, 1, 1, 1, 129
        };

        #endregion

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

        private static GraphicsPath? _magnifierEmptyGPath;
        private static GraphicsPath MagnifierEmptyGPath => _magnifierEmptyGPath ??=
            MakeGraphicsPath(_magnifying_glass_empty_exported_points_raw, _magnifying_glass_empty_exported_types_raw);

        private static GraphicsPath? _plusGPath;
        private static GraphicsPath PlusGPath => _plusGPath ??=
            MakeGraphicsPath(_zoom_plus_symbol_exported_points_raw, _zoom_plus_symbol_exported_types_raw);

        private static GraphicsPath? _minusGPath;
        private static GraphicsPath MinusGPath => _minusGPath ??=
            MakeGraphicsPath(_zoom_minus_symbol_exported_points_raw, _zoom_minus_symbol_exported_types_raw);

        private static GraphicsPath? _zoomResetGPath;
        private static GraphicsPath ZoomResetGPath => _zoomResetGPath ??=
            MakeGraphicsPath(_zoom_reset_symbol_exported_points_raw, _zoom_reset_symbol_exported_types_raw);

        private static GraphicsPath ZoomInComplete =>
            MakeGraphicsPath(_zoom_minus_symbol_exported_points_raw, _zoom_minus_symbol_exported_types_raw);
        private static GraphicsPath ZoomOutComplete =>
            MakeGraphicsPath(_zoom_plus_symbol_exported_points_raw, _zoom_plus_symbol_exported_types_raw);
        private static GraphicsPath ZoomResetComplete =>
            MakeGraphicsPath(_zoom_reset_symbol_exported_points_raw, _zoom_reset_symbol_exported_types_raw);

        //private static GraphicsPath? _zoomInComplete;
        //private static GraphicsPath ZoomInComplete
        //{
        //    get
        //    {
        //        if (_zoomInComplete == null)
        //        {
        //            _zoomInComplete = new GraphicsPath();
        //            _zoomInComplete.AddPath(MagnifierEmptyGPath, true);
        //            _zoomInComplete.AddPath(PlusGPath, true);
        //            _zoomInComplete.Flatten();
        //        }
        //        return _zoomInComplete;
        //    }
        //}

        //private static GraphicsPath? _zoomOutComplete;
        //private static GraphicsPath ZoomOutComplete
        //{
        //    get
        //    {
        //        if (_zoomOutComplete == null)
        //        {
        //            _zoomOutComplete = new GraphicsPath();
        //            _zoomOutComplete.AddPath(MagnifierEmptyGPath, false);
        //            _zoomOutComplete.AddPath(MinusGPath, true);
        //            ZoomOutComplete.Flatten();
        //        }
        //        return _zoomOutComplete;
        //    }
        //}

        //private static GraphicsPath? _zoomResetComplete;
        //private static GraphicsPath ZoomResetComplete
        //{
        //    get
        //    {
        //        if (_zoomResetComplete == null)
        //        {
        //            _zoomResetComplete = new GraphicsPath();
        //            _zoomResetComplete.AddPath(MagnifierEmptyGPath, true);
        //            _zoomResetComplete.AddPath(ZoomResetGPath, true);
        //            _zoomResetComplete.Flatten();
        //        }
        //        return _zoomResetComplete;
        //    }
        //}

        #endregion

        private static void FitRectInBounds(Graphics g, RectangleF drawRect, RectangleF boundsRect)
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

        internal static readonly Pen Sep1Pen = new Pen(Color.FromArgb(189, 189, 189));
        internal static readonly Pen Sep1PenC = new Pen(Color.FromArgb(166, 166, 166));
        internal static readonly Pen Sep2Pen = new Pen(Color.FromArgb(255, 255, 255));

        internal static Pen GetSeparatorPenForCurrentVisualStyleMode() => Application.RenderWithVisualStyles ? Sep1Pen : Sep1PenC;

        #region Global

        private static readonly Color _al_LightBlue = Color.FromArgb(4, 125, 202);

        private static readonly Brush _al_LightBlueBrush = new SolidBrush(_al_LightBlue);

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
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            Brush brush = button.Enabled ? Brushes.Black : SystemBrushes.ControlDark;

            var cr = button.ClientRectangle;

            var gPath = zoomType switch
            {
                Zoom.In => ZoomInComplete,
                Zoom.Out => ZoomOutComplete,
                _ => ZoomResetComplete
            };

            FitRectInBounds(
                e.Graphics,
                gPath.GetBounds(),
                new RectangleF(cr.X, cr.Y, cr.Width, cr.Height)
                //new RectangleF(
                //    (cr.X + 3f),
                //    cr.Y + 3f,
                //    cr.Height - 7,
                //    cr.Height - 7)
                );
            e.Graphics.FillPath(brush, gPath);
        }

        internal static void PaintZoomToolStripButtons(ToolStripButton button, PaintEventArgs e, Zoom zoomType)
        {
            return;
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

            Brush brush = button.Enabled ? Brushes.Black : SystemBrushes.ControlDark;

            var cr = button.Bounds;

            var gPath = zoomType switch
            {
                Zoom.In => ZoomInComplete,
                Zoom.Out => ZoomOutComplete,
                _ => ZoomResetComplete
            };

            FitRectInBounds(
                e.Graphics,
                gPath.GetBounds(),
                new RectangleF(
                    (cr.X + 3f),
                    cr.Y + 3f,
                    cr.Height - 7,
                    cr.Height - 7));
            e.Graphics.FillPath(brush, MagnifierEmptyGPath);
        }

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
            if (e.Graphics.SmoothingMode != SmoothingMode.AntiAlias)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }

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

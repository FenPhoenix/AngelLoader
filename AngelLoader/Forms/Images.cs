﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Properties;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    // Pulling an image from Resources is an expensive operation, so if we're going to load an image multiple times,
    // we want to cache it in a Bitmap object. Images that are loaded only once are not present here, as they would
    // derive no performance benefit.
    // NOTE: This class and everything accessible inside it needs to be public for the designers to recognize it.

    // Performance hack for splash screen, so we don't cause a static ctor cascade, and we still only load the
    // icon once.
    public static class AL_Icon
    {
        private static Icon? _AngelLoader;
        public static Icon AngelLoader => _AngelLoader ??= Resources.AngelLoader;
    }

    public static class DarkModeImageConversion
    {
        private static ColorMatrix MultiplyColorMatrix(float[][] matrix1, float[][] matrix2)
        {
            const int length = 5;
            float[][] newColorMatrix = new float[length][];
            for (int index = 0; index < length; ++index)
            {
                newColorMatrix[index] = new float[length];
            }

            float[] numArray1 = new float[length];
            for (int index1 = 0; index1 < length; ++index1)
            {
                for (int index2 = 0; index2 < length; ++index2)
                {
                    numArray1[index2] = matrix1[index2][index1];
                }

                for (int index3 = 0; index3 < length; ++index3)
                {
                    float[] numArray2 = matrix2[index3];
                    float num = 0.0f;
                    for (int index4 = 0; index4 < length; ++index4)
                    {
                        num += numArray2[index4] * numArray1[index4];
                    }

                    newColorMatrix[index3][index1] = num;
                }
            }

            return new ColorMatrix(newColorMatrix);
        }

        private static ColorMatrix? _darkModeMultiplyColorMatrix;
        private static ColorMatrix DarkModeMultiplyColorMatrix =>
            _darkModeMultiplyColorMatrix ??= MultiplyColorMatrix(new float[5][]
            {
                #region Original ToolStripRenderer.CreateDisabledImage version for reference

                //var disabledImageColorMatrix = MultiplyColorMatrix(new float[5][]
                //{
                //    new float[5] { 1f, 0.0f, 0.0f, 0.0f, 0.0f },
                //    new float[5] { 0.0f, 1f, 0.0f, 0.0f, 0.0f },
                //    new float[5] { 0.0f, 0.0f, 1f, 0.0f, 0.0f },
                //    new float[5] { 0.0f, 0.0f, 0.0f, 0.7f, 0.0f },
                //    new float[5]
                //}, new float[5][]
                //{
                //    new float[5] { 0.2125f, 0.2125f, 0.2125f, 0.0f, 0.0f },
                //    new float[5] { 0.2577f, 0.2577f, 0.2577f, 0.0f, 0.0f },
                //    new float[5] { 0.0361f, 0.0361f, 0.0361f, 0.0f, 0.0f },
                //    new float[5] { 0.0f, 0.0f, 0.0f, 1f, 0.0f },
                //    new float[5] { 0.38f, 0.38f, 0.38f, 0.0f, 1f }
                //});

                #endregion

                new float[5] { 1f, 0.0f, 0.0f, 0.0f, 0.0f },
                new float[5] { 0.0f, 1f, 0.0f, 0.0f, 0.0f },
                new float[5] { 0.0f, 0.0f, 1f, 0.0f, 0.0f },
                new float[5] { 0.0f, 0.0f, 0.0f, 0.8425f, 0.0f },
                new float[5]
            }, new float[5][]
            {
                new float[5] { 0.2125f, 0.2125f, 0.2125f, 0.0f, 0.0f },
                new float[5] { 0.2577f, 0.2577f, 0.2577f, 0.0f, 0.0f },
                new float[5] { 0.0361f, 0.0361f, 0.0361f, 0.0f, 0.0f },
                new float[5] { 0.0f, 0.0f, 0.0f, 1f, 0.0f },
                new float[5] { 0.99f, 0.99f, 0.99f, 0.0f, 1f }
            });

        public static Bitmap CreateDarkModeVersion(Bitmap normalImage, bool disabled = false)
        {
            using var imgAttrib = new ImageAttributes();

            imgAttrib.ClearColorKey();

            DarkModeMultiplyColorMatrix.Matrix33 = disabled ? 0.273f : 0.8425f;

            imgAttrib.SetColorMatrix(DarkModeMultiplyColorMatrix);

            Size size = normalImage.Size;

            var darkModeImage = new Bitmap(size.Width, size.Height);

            using Graphics graphics = Graphics.FromImage(darkModeImage);
            graphics.DrawImage(
                normalImage,
                new Rectangle(0, 0, size.Width, size.Height),
                0,
                0,
                size.Width,
                size.Height,
                GraphicsUnit.Pixel,
                imgAttrib);

            return darkModeImage;
        }
    }

    public static class Images
    {
        public static bool DarkModeEnabled;

        #region Path points and types

        #region Magnifying glass

        // These could be deflate-compressed to save space, or I could scrap the points and just draw a few shapes
        // on the GraphicsPath if I could figure out how to union them together (rather than one cutting a piece
        // out of the other like it does currently) and that would save the most space. Wouldn't necessarily work
        // for every possible image, but some of them at least would be amenable to that.
        // Regardless, with this empty magnifying glass path, I can get 13 images worth of mileage out of it by
        // itself or in combination with +, -, and reset-zoom symbols. So I get my space's worth out of this one
        // for sure.
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

        private static readonly byte[] _magnifierEmptyTypes = MakeTypeArray(
            (3, 9, 0, 1),
            (3, 3, -1, 1),
            (3, 3, -1, 1),
            (3, 5, -1, 131),
            (3, 12, 0, 129));

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
            MakeTypeArray((1, 11, 0, 129)),
            // Zoom out (minus)
            MakeTypeArray((1, 3, 0, 129)),
            // Zoom reset
            MakeTypeArray((1, 7, 0, 129), (1, 7, 0, 129))
        };

        private static readonly GraphicsPath?[] _zoomImageGraphicsPaths = new GraphicsPath[ZoomTypesCount];

        #endregion

        #region Finished checkmarks

        // Inner path starts at index 14
        private static readonly float[] _finishedCheckPoints =
        {
            65.75464f, 3.180167f, 29.22405f, 47.79398f, 10.8f, 30.83739f, -0.1080037f, 42.47338f, 30.86943f,
            71.04063f, 78.09653f, 13.29531f, 65.75464f, 3.180167f, 66.03886f, 7.544f, 73.736f, 13.58057f,
            30.66118f, 66.582f, 4.278f, 42.36847f, 10.90528f, 35.199f, 29.43231f, 52.25f, 66.03886f, 7.544f
        };

        // Inner path starts at index 7
        private static readonly byte[] _finishedCheckTypes = MakeTypeArray((1, 5, 0, 129), (1, 5, 0, 129));

        private static readonly PointF[] FinishedCheckInnerPoints = new PointF[7];
        private static readonly byte[] FinishedCheckInnerTypes = new byte[7];

        private static GraphicsPath? _finishedCheckGPath;
        private static GraphicsPath FinishedCheckOutlineGPath => _finishedCheckGPath ??= MakeGraphicsPath(_finishedCheckPoints, _finishedCheckTypes);

        #endregion

        #region Circle checkmark

        private static readonly float[] _circleCheckPoints =
        {
            40, 80, 86, 128, 215, 0, 253, 38, 86, 204, 0, 118, 40, 80
        };

        private static readonly byte[] _circleCheckTypes = MakeTypeArray((1, 5, 0, 129));

        private static GraphicsPath? _circleCheckGPath;
        private static GraphicsPath CircleCheckGPath => _circleCheckGPath ??= MakeGraphicsPath(_circleCheckPoints, _circleCheckTypes);

        #endregion

        #region X symbol

        private static readonly float[] _xPoints =
        {
            8, 32,
            32, 8,
            80, 58,
            128, 8,
            152, 32,
            102, 80,
            152, 128,
            128, 152,
            80, 102,
            32, 152,
            8, 128,
            58, 80,
            8, 32
        };

        private static readonly byte[] _xTypes = MakeTypeArray((1, 11, 0, 129));

        private static GraphicsPath? _xGPath;
        private static GraphicsPath XGPath => _xGPath ??= MakeGraphicsPath(_xPoints, _xTypes);

        #endregion

        #region Stars

        private static readonly float[] _starOuterPoints =
        {
            50f, 0f, 36f, 33f, 0f, 36f, 27f, 60f, 19f, 95f, 50f, 76f, 81f, 95f, 73f, 60f, 100f, 36f, 64f, 33f,
            50f, 0f
        };

        private static readonly float[] _starMiddlePoints =
        {
            50f, 9f, 61.5f, 36.5f, 91f, 39f, 69f, 58.5f, 75.5f, 87.5f, 50f, 72f, 24.5f, 87.5f, 31f, 58.5f, 9f,
            39f, 38.5f, 36.5f, 50f, 9f
        };

        private static readonly float[] _starInnerFullPoints =
        {
            50f, 22f, 42f, 41f, 21f, 43f, 36.5f, 56.5f, 32f, 77f, 50f, 66f, 68f, 77f, 63.5f, 56.5f, 79f, 43f,
            58f, 41f, 50f, 22f
        };

        private static readonly float[] _starInnerRightHalfPoints =
        {
            50f, 66f, 68f, 77f, 63.5f, 56.5f, 79f, 43f, 58f, 41f, 50f, 22f, 50f, 66f
        };

        private static readonly byte[] _starMainTypes = MakeTypeArray((1, 9, 0, 129));

        private static readonly byte[] _starInnerRightHalfTypes = MakeTypeArray((1, 5, 0, 129));

        private static GraphicsPath? _starEmptyGPath;
        private static GraphicsPath StarEmptyGPath
        {
            get
            {
                if (_starEmptyGPath == null)
                {
                    float[] points = CombineArrays(_starOuterPoints, _starMiddlePoints, _starInnerFullPoints);
                    byte[] types = CombineArrays(_starMainTypes, _starMainTypes, _starMainTypes);
                    _starEmptyGPath = MakeGraphicsPath(points, types);
                }
                return _starEmptyGPath;
            }
        }

        private static GraphicsPath? _starRightEmptyGPath;
        private static GraphicsPath StarRightEmptyGPath
        {
            get
            {
                if (_starRightEmptyGPath == null)
                {
                    float[] points = CombineArrays(_starOuterPoints, _starMiddlePoints, _starInnerRightHalfPoints);
                    byte[] types = CombineArrays(_starMainTypes, _starMainTypes, _starInnerRightHalfTypes);
                    _starRightEmptyGPath = MakeGraphicsPath(points, types);
                }
                return _starRightEmptyGPath;
            }
        }

        private static GraphicsPath? _starFullGPath;
        private static GraphicsPath StarFullGPath
        {
            get
            {
                if (_starFullGPath == null)
                {
                    float[] points = CombineArrays(_starOuterPoints, _starMiddlePoints);
                    byte[] types = CombineArrays(_starMainTypes, _starMainTypes);
                    _starFullGPath = MakeGraphicsPath(points, types);
                }
                return _starFullGPath;
            }
        }

        #endregion

        #region Vector points

        #region Play arrow

        private static readonly Point[] _playArrowPoints =
        {
            new Point(15, 5),
            new Point(29, 17),
            new Point(15, 29)
        };

        private static readonly PointF[] _playOriginalArrowPoints =
        {
            new PointF(17.5f, 7.5f),
            new PointF(28.5f, 17),
            new PointF(17.5f, 26.5f)
        };

        #endregion

        #region Plus / ex

        private static readonly Rectangle[] _plusRects = new Rectangle[2];

        #endregion

        #region Hamburger

        private static readonly Rectangle[] _hamRects_TopRightMenu =
        {
            new Rectangle(2, 3, 14, 2),
            new Rectangle(2, 9, 14, 2),
            new Rectangle(2, 15, 14, 2)
        };

        private static readonly Rectangle[] _hamRects24 =
        {
            new Rectangle(5, 5, 14, 2),
            new Rectangle(5, 11, 14, 2),
            new Rectangle(5, 17, 14, 2)
        };

        #endregion

        #region Web search

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
            new Point(14, 0),
            new Point(21, 0),
            new Point(21, 7),
            new Point(18, 7),
            new Point(18, 3),
            new Point(14, 3)
        };
        private static readonly Point[] _readmeFullScreenBottomLeft =
        {
            new Point(0, 14),
            new Point(3, 14),
            new Point(3, 18),
            new Point(7, 18),
            new Point(7, 21),
            new Point(0, 21)
        };
        private static readonly Point[] _readmeFullScreenBottomRight =
        {
            new Point(18, 14),
            new Point(21, 14),
            new Point(21, 21),
            new Point(14, 21),
            new Point(14, 18),
            new Point(18, 18)
        };

        #endregion

        #endregion

        #endregion

        #region Colors / brushes / pens

        private static readonly Brush _greenCircleBrushDark = new SolidBrush(Color.FromArgb(68, 178, 68));
        private static readonly Brush _greenCircleBrush = new SolidBrush(Color.FromArgb(65, 173, 73));
        private static Brush GreenCircleBrush => DarkModeEnabled ? _greenCircleBrushDark : _greenCircleBrush;

        private static readonly Brush _deleteFromDBBrushDark = new SolidBrush(Color.FromArgb(209, 70, 70));
        private static readonly Brush _deleteFromDBBrush = new SolidBrush(Color.FromArgb(135, 0, 0));
        private static Brush DeleteFromDBBrush => DarkModeEnabled ? _deleteFromDBBrushDark : _deleteFromDBBrush;

        #region Finished states

        private static readonly Brush _normalCheckOutlineBrushDark = new SolidBrush(Color.FromArgb(3, 100, 1));
        private static readonly Brush _normalCheckOutlineBrush = new SolidBrush(Color.FromArgb(3, 100, 1));
        private static Brush NormalCheckOutlineBrush => DarkModeEnabled ? _normalCheckOutlineBrushDark : _normalCheckOutlineBrush;

        private static readonly Brush _normalCheckFillBrushDark = new SolidBrush(Color.FromArgb(68, 178, 68));
        private static readonly Brush _normalCheckFillBrush = new SolidBrush(Color.FromArgb(0, 170, 0));
        private static Brush NormalCheckFillBrush => DarkModeEnabled ? _normalCheckFillBrushDark : _normalCheckFillBrush;

        private static readonly Brush _hardCheckOutlineBrushDark = new SolidBrush(Color.FromArgb(139, 111, 0));
        private static readonly Brush _hardCheckOutlineBrush = new SolidBrush(Color.FromArgb(196, 157, 2));
        private static Brush HardCheckOutlineBrush => DarkModeEnabled ? _hardCheckOutlineBrushDark : _hardCheckOutlineBrush;

        private static readonly Brush _hardCheckFillBrushDark = new SolidBrush(Color.FromArgb(212, 187, 73));
        private static readonly Brush _hardCheckFillBrush = new SolidBrush(Color.FromArgb(255, 210, 0));
        private static Brush HardCheckFillBrush => DarkModeEnabled ? _hardCheckFillBrushDark : _hardCheckFillBrush;

        private static readonly Brush _expertCheckOutlineBrushDark = new SolidBrush(Color.FromArgb(118, 14, 14));
        private static readonly Brush _expertCheckOutlineBrush = new SolidBrush(Color.FromArgb(135, 2, 2));
        private static Brush ExpertCheckOutlineBrush => DarkModeEnabled ? _expertCheckOutlineBrushDark : _expertCheckOutlineBrush;

        private static readonly Brush _expertCheckFillBrushDark = new SolidBrush(Color.FromArgb(209, 70, 70));
        private static readonly Brush _expertCheckFillBrush = new SolidBrush(Color.FromArgb(216, 0, 0));
        private static Brush ExpertCheckFillBrush => DarkModeEnabled ? _expertCheckFillBrushDark : _expertCheckFillBrush;

        private static readonly Brush _extremeCheckOutlineBrushDark = new SolidBrush(Color.FromArgb(28, 76, 153));
        private static readonly Brush _extremeCheckOutlineBrush = new SolidBrush(Color.FromArgb(19, 1, 100));
        private static Brush ExtremeCheckOutlineBrush => DarkModeEnabled ? _extremeCheckOutlineBrushDark : _extremeCheckOutlineBrush;

        private static readonly Brush _extremeCheckFillBrushDark = new SolidBrush(Color.FromArgb(34, 148, 228));
        private static readonly Brush _extremeCheckFillBrush = new SolidBrush(Color.FromArgb(0, 53, 226));
        private static Brush ExtremeCheckFillBrush => DarkModeEnabled ? _extremeCheckFillBrushDark : _extremeCheckFillBrush;

        private static readonly Brush UnknownCheckOutlineBrush = new SolidBrush(Color.FromArgb(100, 100, 100));
        private static readonly Brush UnknownCheckFillBrush = new SolidBrush(Color.FromArgb(170, 170, 170));

        private static readonly Brush _finishedOnFilterOutlineBrushDark = new SolidBrush(Color.FromArgb(89, 159, 203));
        private static readonly Brush _finishedOnFilterOutlineOnlyBrushDark = new SolidBrush(Color.FromArgb(132, 206, 252));
        private static readonly Brush _finishedOnFilterOutlineBrush = new SolidBrush(Color.FromArgb(14, 101, 139));
        private static Brush FinishedOnFilterOutlineBrush => DarkModeEnabled ? _finishedOnFilterOutlineBrushDark : _finishedOnFilterOutlineBrush;

        private static readonly Brush _finishedOnFilterFillBrushDark = new SolidBrush(Color.FromArgb(89, 159, 203));
        private static readonly Brush _finishedOnFilterFillBrush = new SolidBrush(Color.FromArgb(89, 159, 203));
        private static Brush FinishedOnFilterFillBrush => DarkModeEnabled ? _finishedOnFilterFillBrushDark : _finishedOnFilterFillBrush;

        #endregion

        #region Calendars

        private static readonly Brush _calendarBackgroundBrushDark = new SolidBrush(Color.FromArgb(220, 220, 220));
        private static readonly Brush _calendarBackgroundBrush = Brushes.White;

        private static readonly Pen _calendarBackgroundPenDark = new Pen(Color.FromArgb(220, 220, 220));
        private static readonly Pen _calendarBackgroundPen = Pens.White;

        #region Release date

        private static readonly Color _releaseDateForegroundDark = Color.FromArgb(0, 150, 255);
        private static readonly Color _releaseDateForeground = Color.FromArgb(28, 132, 204);

        private static readonly Brush _releaseDateForegroundBrushDark = new SolidBrush(_releaseDateForegroundDark);
        private static readonly Brush _releaseDateForegroundBrush = new SolidBrush(_releaseDateForeground);

        private static readonly Pen _releaseDateForegroundPenDark = new Pen(_releaseDateForegroundDark);
        private static readonly Pen _releaseDateForegroundPen = new Pen(_releaseDateForeground);

        #endregion

        #region Last played

        private static readonly Color _lastPlayedForegroundDark = Color.FromArgb(19, 172, 48);
        private static readonly Color _lastPlayedForeground = Color.FromArgb(0, 163, 0);

        private static readonly Brush _lastPlayedForegroundBrushDark = new SolidBrush(_lastPlayedForegroundDark);
        private static readonly Brush _lastPlayedForegroundBrush = new SolidBrush(_lastPlayedForeground);

        private static readonly Pen _lastPlayedForegroundPenDark = new Pen(_lastPlayedForegroundDark);
        private static readonly Pen _lastPlayedForegroundPen = new Pen(_lastPlayedForeground);

        #endregion

        #endregion

        #region Stars

        private static readonly Brush _starOutlineBrushDark = new SolidBrush(Color.FromArgb(200, 128, 26));
        private static readonly Brush _starOutlineBrush = new SolidBrush(Color.FromArgb(192, 113, 0));
        private static Brush StarOutlineBrush => DarkModeEnabled ? _starOutlineBrushDark : _starOutlineBrush;

        private static readonly Brush _starFillBrushDark = new SolidBrush(Color.FromArgb(228, 185, 82));
        private static readonly Brush _starFillBrush = new SolidBrush(Color.FromArgb(255, 180, 0));
        private static Brush StarFillBrush => DarkModeEnabled ? _starFillBrushDark : _starFillBrush;

        private static readonly Brush _starEmptyBrushDark = new SolidBrush(DarkColors.Fen_DarkBackground);
        private static readonly Brush _starEmptyBrush = Brushes.White;
        private static Brush StarEmptyBrush => DarkModeEnabled ? _starEmptyBrushDark : _starEmptyBrush;

        #endregion

        #region Separators

        private static readonly Pen _sep1Pen = new Pen(Color.FromArgb(189, 189, 189));
        private static readonly Pen _sep1PenC = new Pen(Color.FromArgb(166, 166, 166));
        internal static readonly Pen Sep2Pen = new Pen(Color.FromArgb(255, 255, 255));

        internal static Pen Sep1Pen =>
            DarkModeEnabled
                ? DarkColors.GreySelectionPen
            : Application.RenderWithVisualStyles
                ? _sep1Pen
                : _sep1PenC;

        #endregion

        #region AL Blue

        private static readonly Color _al_LightBlue = Color.FromArgb(4, 125, 202);
        private static readonly Color _al_LightBlueDark = Color.FromArgb(54, 146, 204);

        private static readonly SolidBrush _al_LightBlueBrush = new SolidBrush(_al_LightBlue);
        private static readonly SolidBrush _al_LightBlueBrushDark = new SolidBrush(_al_LightBlueDark);
        private static SolidBrush AL_LightBlueBrush => DarkModeEnabled ? _al_LightBlueBrushDark : _al_LightBlueBrush;

        #endregion

        #region Web search

        private static readonly Pen _webSearchCirclePen = new Pen(_al_LightBlue, 2);
        private static readonly Pen _webSearchCirclePenDark = new Pen(_al_LightBlueDark, 2);
        private static Pen WebSearchCirclePen => DarkModeEnabled ? _webSearchCirclePenDark : _webSearchCirclePen;

        private static readonly Pen _webSearchCircleDisabledPen = new Pen(SystemColors.ControlDark, 2);

        #endregion

        #region Play arrow

        private static readonly Color _playArrowColor = Color.FromArgb(45, 154, 47);
        private static readonly Color _playArrowColor_Dark = Color.FromArgb(91, 176, 93);
        private static readonly SolidBrush _playArrowBrush = new SolidBrush(_playArrowColor);
        private static readonly SolidBrush _playArrowBrush_Dark = new SolidBrush(_playArrowColor_Dark);
        private static readonly Pen _playArrowPen = new Pen(_playArrowColor, 2.5f);
        private static readonly Pen _playArrowPen_Dark = new Pen(_playArrowColor_Dark, 2.5f);

        private static SolidBrush PlayArrowBrush => DarkModeEnabled ? _playArrowBrush_Dark : _playArrowBrush;
        private static Pen PlayArrowPen => DarkModeEnabled ? _playArrowPen_Dark : _playArrowPen;
        // Explicit pen for this because we need to set the width
        private static readonly Pen PlayArrowDisabledPen = new Pen(SystemColors.ControlDark, 2.5f);

        #endregion

        #region Reset layout

        private static readonly Pen _resetLayoutPen = new Pen(Color.FromArgb(123, 123, 123), 2);
        private static readonly Pen _resetLayoutPenDisabled = new Pen(SystemColors.ControlDark, 2);

        #endregion

        private static Brush BlackForegroundBrush => DarkModeEnabled ? DarkColors.Fen_DarkForegroundBrush : Brushes.Black;
        private static Pen BlackForegroundPen => DarkModeEnabled ? DarkColors.Fen_DarkForegroundPen : Pens.Black;

        #region Arrows

        private static Pen ArrowButtonEnabledPen => DarkModeEnabled ? DarkColors.ArrowEnabledPen : SystemPens.ControlText;

        #endregion

        #endregion

        #region Raster

        #region Image arrays

        // Load this only once, as it's transparent and so doesn't have to change with the theme
        internal static readonly Bitmap Blank = new Bitmap(1, 1, PixelFormat.Format32bppPArgb);

        // We need to grab these images every time a cell is shown on the DataGridView, and pulling them from
        // Resources every time is enormously expensive, causing laggy scrolling and just generally wasting good
        // cycles. So we copy them only once to these local bitmaps, and voila, instant scrolling performance.
        // 2021-12-22:
        // On Win10, DataGridView scrolling performance is laggy af regardless.
        // Also, we could just get these images in realtime using the per-game getters now, but I guess we put
        // the actual bitmaps into an array (updating it only on theme change) and pull from there because it's
        // the fastest thing to do? Like, getting them in realtime has a lot of checks. Probably doesn't matter.
        // But anyway.
        internal static readonly Image?[] FMsList_GameIcons = new Image?[SupportedGameCount];

        // 0-10, and we don't count -1 (no rating) because that's handled elsewhere
        private const int _numRatings = 11;
        internal static readonly Bitmap?[] FMsList_StarIcons = new Bitmap?[_numRatings];
        internal static readonly Bitmap?[] FMsList_FinishedOnIcons = new Bitmap?[16];

        private static readonly Bitmap?[] _zoomImages = new Bitmap?[ZoomTypesCount];

        #endregion

        #region Image properties

        private static readonly Dictionary<Image, Image> _disabledImages = new(25);
        internal static Image GetDisabledImage(Image image)
        {
            if (_disabledImages.TryGetValue(image, out Image result))
            {
                return result;
            }
            else
            {
                Image disabledImage = ToolStripRenderer.CreateDisabledImage(image);
                _disabledImages[image] = disabledImage;
                return disabledImage;
            }
        }

        #region Games

        #region Per-game image getters

        [PublicAPI]
        public sealed class PerGameSizedImage
        {
            private readonly Func<Image> _large;
            private readonly Func<Image> _small;
            public Image Large(bool enabled = true) => enabled ? _large.Invoke() : GetDisabledImage(_large.Invoke());
            public Image Small(bool enabled = true) => enabled ? _small.Invoke() : GetDisabledImage(_small.Invoke());

            public PerGameSizedImage(Func<Bitmap> large, Func<Bitmap> small)
            {
                _large = large;
                _small = small;
            }
        }

        [PublicAPI]
        public sealed class PerGameImage
        {
            public readonly PerGameSizedImage Primary;
            public readonly PerGameSizedImage Alternate;

            public PerGameImage(PerGameSizedImage primary, PerGameSizedImage alternate)
            {
                Primary = primary;
                Alternate = alternate;
            }
        }

        private static readonly PerGameImage[] PerGameImageGetters = InitPerGameImageGetters();

        private static PerGameImage[] InitPerGameImageGetters()
        {
            var ret = new PerGameImage[SupportedGameCount];

            // @GENGAMES (Images.InitPerGameImageGetters()) - Begin
            var t1Primary = new PerGameSizedImage(large: Thief1_21, small: Thief1_16);
            var t1Secondary = new PerGameSizedImage(large: Thief1_21_DGV, small: Thief1_16);

            var t2 = new PerGameSizedImage(large: Thief2_21, small: Thief2_16);
            var t3 = new PerGameSizedImage(large: Thief3_21, small: Thief3_16);
            var ss2 = new PerGameSizedImage(large: Shock2_21, small: Shock2_16);

            ret[(int)GameIndex.Thief1] = new PerGameImage(t1Primary, t1Secondary);
            ret[(int)GameIndex.Thief2] = new PerGameImage(t2, t2);
            ret[(int)GameIndex.Thief3] = new PerGameImage(t3, t3);
            ret[(int)GameIndex.SS2] = new PerGameImage(ss2, ss2);
            // @GENGAMES (Images.InitPerGameImageGetters()) - End

            return ret;
        }

        public static PerGameImage GetPerGameImage(int gameIndex) => PerGameImageGetters[gameIndex];
        public static PerGameImage GetPerGameImage(GameIndex gameIndex) => PerGameImageGetters[(int)gameIndex];

        #endregion

        #region Game image properties

        // @GENGAMES (Images/game image properties): Begin

        private static Bitmap? _thief1_16;
        private static Bitmap? _thief1_16_Dark;
        private static Bitmap Thief1_16() =>
            DarkModeEnabled
                ? _thief1_16_Dark ??= Resources.Thief1_16_Dark
                : _thief1_16 ??= Resources.Thief1_16;

        private static Bitmap? _thief2_16;
        private static Bitmap Thief2_16() => _thief2_16 ??= Resources.Thief2_16;

        private static Bitmap? _thief3_16;
        private static Bitmap Thief3_16() => _thief3_16 ??= Resources.Thief3_16;

        private static Bitmap? _shock2_16;
        private static Bitmap? _shock2_16_Dark;
        private static Bitmap Shock2_16() =>
            DarkModeEnabled
                ? _shock2_16_Dark ??= Resources.Shock2_16_Dark
                : _shock2_16 ??= Resources.Shock2_16;

        private static Bitmap? _thief1_21;
        private static Bitmap? _thief1_21_Dark;
        private static Bitmap Thief1_21() =>
            DarkModeEnabled
                ? _thief1_21_Dark ??= Resources.Thief1_21_Dark
                : _thief1_21 ??= Resources.Thief1_21;

        private static Bitmap? _thief1_21_dark_DarkBG;
        private static Bitmap Thief1_21_DGV() =>
            DarkModeEnabled
                ? _thief1_21_dark_DarkBG ??= Resources.Thief1_21_Dark_DarkBG
                : _thief1_21 ??= Resources.Thief1_21;

        private static Bitmap? _thief2_21;
        private static Bitmap Thief2_21() => _thief2_21 ??= Resources.Thief2_21;

        private static Bitmap? _thief3_21;
        private static Bitmap Thief3_21() => _thief3_21 ??= Resources.Thief3_21;

        private static Bitmap? _shock2_21;
        private static Bitmap? _shock2_21_Dark;
        private static Bitmap Shock2_21() =>
            DarkModeEnabled
                ? _shock2_21_Dark ??= Resources.Shock2_21_Dark
                : _shock2_21 ??= Resources.Shock2_21;

        // @GENGAMES (Images/game image properties): End

        #endregion

        #endregion

        #region Filter bar

        private static Bitmap? _filterByReleaseDate;
        private static Bitmap? _filterByReleaseDate_Dark;
        public static Bitmap FilterByReleaseDate =>
            DarkModeEnabled
                ? _filterByReleaseDate_Dark ??= CreateCalendarImage(lastPlayed: false, darkMode: true)
                : _filterByReleaseDate ??= CreateCalendarImage(lastPlayed: false, darkMode: false);

        private static Bitmap? _filterByLastPlayed;
        private static Bitmap? _filterByLastPlayed_Dark;
        public static Bitmap FilterByLastPlayed =>
            DarkModeEnabled
                ? _filterByLastPlayed_Dark ??= CreateCalendarImage(lastPlayed: true, darkMode: true)
                : _filterByLastPlayed ??= CreateCalendarImage(lastPlayed: true, darkMode: false);

        private static Bitmap? _filterByTags;
        private static Bitmap? _filterByTags_Dark;
        public static Bitmap FilterByTags =>
            DarkModeEnabled
                ? _filterByTags_Dark ??= Resources.FilterByTags_Dark
                : _filterByTags ??= Resources.FilterByTags;

        public static Bitmap FilterByFinished => CreateFinishedOnBitmap(Difficulty.None, filterFinished: true);
        public static Bitmap FilterByUnfinished => CreateFinishedOnBitmap(Difficulty.None, filterUnfinished: true);

        public static Bitmap FilterByRating => CreateStarImage(StarFullGPath, 24);

        private static Bitmap? _showRecentAtTop;
        private static Bitmap? _showRecentAtTop_Dark;
        public static Bitmap FilterShowRecentAtTop =>
            DarkModeEnabled
                ? _showRecentAtTop_Dark ??= Resources.FilterShowRecentAtTop_Dark
                : _showRecentAtTop ??= Resources.FilterShowRecentAtTop;

        #endregion

        #region Filter bar right side

        private static Bitmap? _refreshFilters;
        private static Bitmap? _refreshFilters_Dark;
        public static Bitmap RefreshFilters =>
            DarkModeEnabled
                ? _refreshFilters_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.RefreshFilters)
                : _refreshFilters ??= Resources.RefreshFilters;

        private static Bitmap? _clearFilters;
        private static Bitmap? _clearFilters_Dark;
        public static Bitmap ClearFilters =>
            DarkModeEnabled
                ? _clearFilters_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.ClearFilters)
                : _clearFilters ??= Resources.ClearFilters;

        #endregion

        #region Character encoding

        private static Image? _charEncLetter;
        private static Image? _charEncLetter_Dark;
        private static Image CharEncLetter =>
            DarkModeEnabled
                ? _charEncLetter_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.CharacterEncodingLetter)
                : _charEncLetter ??= Resources.CharacterEncodingLetter;

        // Don't use the auto-disabled-colored-image dictionary for this, because we want a custom disabled color
        // to go against the dark background of the readme
        private static Image? _charEncLetter_Disabled;
        private static Image? _charEncLetter_Disabled_Dark;
        private static Image CharEncLetter_Disabled =>
            DarkModeEnabled
                ? _charEncLetter_Disabled_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.CharacterEncodingLetter, true)
                : _charEncLetter_Disabled ??= ToolStripRenderer.CreateDisabledImage(Resources.CharacterEncodingLetter);

        #endregion

        #region Install / uninstall

        private static Image? _install_24;
        public static Image Install_24 => _install_24 ??= Resources.Install_24;

        private static Image? _uninstall_24;
        public static Image Uninstall_24 => _uninstall_24 ??= Resources.Uninstall_24;

        #endregion

        #region Settings

        private static Image? _settings;
        public static Image Settings => _settings ??= Resources.Settings_24;

        #endregion

        #region Zoom

        // We can't use the Paint event to paint the image on ToolStrip crap, as it's tool strip crap, you know.
        // It just bugs out in various different ways. So we just paint on an image and set their image to that.
        public static Bitmap GetZoomImage(Rectangle rect, Zoom zoomType, bool regenerate = false)
        {
            int index = (int)zoomType;
            if (regenerate || _zoomImages[index] == null)
            {
                _zoomImages[index]?.Dispose();
                _zoomImages[index] = new Bitmap(rect.Width, rect.Height);
                using var g = Graphics.FromImage(_zoomImages[index]!);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                FitRectInBounds(
                    g,
                    GetZoomImageGraphicsPath(zoomType).GetBounds(),
                    new RectangleF(0, 0, rect.Width, rect.Height));
                g.FillPath(BlackForegroundBrush, GetZoomImageGraphicsPath(zoomType));
            }
            return _zoomImages[index]!;
        }

        #endregion

        #region Rating example

        private static Bitmap? _ratingExample_NDL;
        private static Bitmap? _ratingExample_NDL_Dark;
        public static Bitmap RatingExample_NDL =>
            DarkModeEnabled
                ? _ratingExample_NDL_Dark ??= CreateRatingExample_Number(outOfFive: false, darkMode: true)
                : _ratingExample_NDL ??= CreateRatingExample_Number(outOfFive: false, darkMode: false);

        private static Bitmap? _ratingExample_FMSel_Stars;
        private static Bitmap? _ratingExample_FMSel_Stars_Dark;
        public static Bitmap RatingExample_FMSel_Stars =>
            DarkModeEnabled
                ? _ratingExample_FMSel_Stars_Dark ??= CreateRatingExample_FMSel_Stars(darkMode: true)
                : _ratingExample_FMSel_Stars ??= CreateRatingExample_FMSel_Stars(darkMode: false);

        private static Bitmap? _ratingExample_FMSel_Number;
        private static Bitmap? _ratingExample_FMSel_Number_Dark;
        public static Bitmap RatingExample_FMSel_Number =>
            DarkModeEnabled
                ? _ratingExample_FMSel_Number_Dark ??= CreateRatingExample_Number(outOfFive: true, darkMode: true)
                : _ratingExample_FMSel_Number ??= CreateRatingExample_Number(outOfFive: true, darkMode: false);

        #endregion

        #region FMs list only

        private static Bitmap? _greenCheckCircle;
        private static Bitmap? _greenCheckCircle_Dark;
        public static Bitmap GreenCheckCircle =>
            DarkModeEnabled
                ? _greenCheckCircle_Dark ??= CreateGreenCheckCircleImage()
                : _greenCheckCircle ??= CreateGreenCheckCircleImage();

        private static Bitmap? _redQuestionMarkCircle;
        private static Bitmap? _redQuestionMarkCircle_Dark;
        public static Bitmap RedQuestionMarkCircle =>
            DarkModeEnabled
                ? _redQuestionMarkCircle_Dark ??= Resources.RedQuestionMarkCircle_Dark
                : _redQuestionMarkCircle ??= Resources.RedQuestionMarkCircle;

        private static Bitmap? _finishedOnUnknown;
        public static Bitmap FinishedOnUnknown => _finishedOnUnknown ??= CreateFinishedOnBitmap(Difficulty.None);

        #endregion

        #region FM context menu

        private static Bitmap? _pin_16;
        private static Bitmap? _pin_16_Dark;
        public static Bitmap Pin_16 =>
            DarkModeEnabled
                ? _pin_16_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.Pin_16)
                : _pin_16 ??= Resources.Pin_16;

        private static Bitmap? _unpin_16;
        private static Bitmap? _unpin_16_Dark;
        public static Bitmap Unpin_16 =>
            DarkModeEnabled
                ? _unpin_16_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.Unpin_16)
                : _unpin_16 ??= Resources.Unpin_16;

        private static Bitmap? _trash_16;
        private static Bitmap? _trash_16_Dark;
        public static Bitmap Trash_16 =>
            DarkModeEnabled
                ? _trash_16_Dark ??= Resources.Trash_16_Dark
                : _trash_16 ??= Resources.Trash_16;

        private static Bitmap? _deleteFromDB;
        private static Bitmap? _deleteFromDB_Dark;
        public static Bitmap DeleteFromDB =>
            DarkModeEnabled
                ? _deleteFromDB_Dark ??= CreateDeleteFromDBImage()
                : _deleteFromDB ??= CreateDeleteFromDBImage();

        #endregion

        #region Misc

        private static Bitmap? _mods_16;
        private static Bitmap? _mods_16_Dark;
        public static Bitmap Mods_16 =>
            DarkModeEnabled
                ? _mods_16_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.Mods_16)
                : _mods_16 ??= Resources.Mods_16;

        private static Bitmap? _redExclamationMarkCircle;
        private static Bitmap? _redExclamationMarkCircle_Dark;
        public static Bitmap RedExclamationMarkCircle =>
            DarkModeEnabled
                ? _redExclamationMarkCircle_Dark ??= Resources.RedExclamationMarkCircle_Dark
                : _redExclamationMarkCircle ??= Resources.RedExclamationMarkCircle;

        private static Bitmap? _refresh;
        private static Bitmap? _refresh_Dark;
        public static Bitmap Refresh =>
            DarkModeEnabled
                ? _refresh_Dark ??= DarkModeImageConversion.CreateDarkModeVersion(Resources.Refresh)
                : _refresh ??= Resources.Refresh;

        #endregion

        #endregion

        #region Methods

        internal static void ReloadImageArrays()
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                FMsList_GameIcons[i] = GetPerGameImage(i).Alternate.Large();
            }

            LoadRatingImages();
            LoadFinishedOnImages();
        }

        private static void LoadFinishedOnImages()
        {
            AssertR(FMsList_FinishedOnIcons.Length == 16, "bitmaps.Length != 16");

            FMsList_FinishedOnIcons.DisposeRange(1, FMsList_FinishedOnIcons.Length);
            FMsList_FinishedOnIcons[0] = Blank;

            Bitmap? _finishedOnNormal_single = null;
            Bitmap? _finishedOnHard_single = null;
            Bitmap? _finishedOnExpert_single = null;
            Bitmap? _finishedOnExtreme_single = null;
            try
            {
                #region Image getters

                Bitmap GetFinishedOnNormal_Single() => _finishedOnNormal_single ??= CreateFinishedOnBitmap(Difficulty.Normal);
                Bitmap GetFinishedOnHard_Single() => _finishedOnHard_single ??= CreateFinishedOnBitmap(Difficulty.Hard);
                Bitmap GetFinishedOnExpert_Single() => _finishedOnExpert_single ??= CreateFinishedOnBitmap(Difficulty.Expert);
                Bitmap GetFinishedOnExtreme_Single() => _finishedOnExtreme_single ??= CreateFinishedOnBitmap(Difficulty.Extreme);

                #endregion

                var list = new List<Bitmap>(4);

                for (int ai = 1; ai < FMsList_FinishedOnIcons.Length; ai++)
                {
                    var canvas = new Bitmap(138, 32, PixelFormat.Format32bppPArgb);
                    Difficulty difficulty = (Difficulty)ai;

                    list.Clear();

                    if (difficulty.HasFlagFast(Difficulty.Normal)) list.Add(GetFinishedOnNormal_Single());
                    if (difficulty.HasFlagFast(Difficulty.Hard)) list.Add(GetFinishedOnHard_Single());
                    if (difficulty.HasFlagFast(Difficulty.Expert)) list.Add(GetFinishedOnExpert_Single());
                    if (difficulty.HasFlagFast(Difficulty.Extreme)) list.Add(GetFinishedOnExtreme_Single());

                    int totalWidth = 0;
                    // Some of these images are +-1px width from each other, but they all add up to the full 138px
                    // width of the canvas, which we really don't want to change as other things depend on it. So
                    // that's why we get the width of each individual image, rather than keeping a constant.
                    for (int i = 0; i < list.Count; i++) totalWidth += list[i].Width;

                    int x = (138 / 2) - (totalWidth / 2);

                    using var g = Graphics.FromImage(canvas);
                    for (int i = 0; i < list.Count; i++)
                    {
                        Bitmap subImage = list[i];
                        g.DrawImage(subImage, x, 0);
                        x += subImage.Width;
                    }

                    FMsList_FinishedOnIcons[ai] = canvas;
                }
            }
            finally
            {
                _finishedOnNormal_single?.Dispose();
                _finishedOnHard_single?.Dispose();
                _finishedOnExpert_single?.Dispose();
                _finishedOnExtreme_single?.Dispose();
            }
        }

        private static void LoadRatingImages()
        {
            FMsList_StarIcons.DisposeAll();

            bool[] bits = new bool[_numRatings];

            Bitmap? _starEmpty = null;
            Bitmap? _starRightEmpty = null;
            Bitmap? _starFull = null;
            try
            {
                #region Image getters

                Bitmap GetStarEmpty() => _starEmpty ??= CreateStarImage(StarEmptyGPath, 22);
                Bitmap GetStarRightEmpty() => _starRightEmpty ??= CreateStarImage(StarRightEmptyGPath, 22);
                Bitmap GetStarFull() => _starFull ??= CreateStarImage(StarFullGPath, 22);

                #endregion

                for (int bi = 0; bi < _numRatings; bi++)
                {
                    var canvas = new Bitmap(110, 32, PixelFormat.Format32bppPArgb);

                    using var g = Graphics.FromImage(canvas);

                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    for (int i = 0; i < bits.Length - 1; i += 2)
                    {
                        g.DrawImage(bits[i] ? bits[i + 1] ? GetStarFull() : GetStarRightEmpty() : GetStarEmpty(), (i / 2) * 22, 5);
                    }

                    bits[bi] = true;
                    FMsList_StarIcons[bi] = canvas;
                }
            }
            finally
            {
                _starEmpty?.Dispose();
                _starRightEmpty?.Dispose();
                _starFull?.Dispose();
            }
        }

        private static Bitmap CreateFinishedOnBitmap(Difficulty difficulty, bool filterFinished = false, bool filterUnfinished = false)
        {
            int width, height;
            Brush outlineBrush, fillBrush;
            if (filterFinished || filterUnfinished)
            {
                width = 24;
                height = 24;
                outlineBrush = FinishedOnFilterOutlineBrush;
                fillBrush = FinishedOnFilterFillBrush;
            }
            else
            {
                height = 32;
                // Variations in image widths (34,35,36) are intentional to keep the same dimensions as the old
                // raster images used to have, so that the new vector ones are drop-in replacements.
                (width, outlineBrush, fillBrush) = difficulty switch
                {
                    Difficulty.Normal => (34, NormalCheckOutlineBrush, NormalCheckFillBrush),
                    Difficulty.Hard => (35, HardCheckOutlineBrush, HardCheckFillBrush),
                    Difficulty.Expert => (35, ExpertCheckOutlineBrush, ExpertCheckFillBrush),
                    Difficulty.Extreme => (34, ExtremeCheckOutlineBrush, ExtremeCheckFillBrush),
                    _ => (36, UnknownCheckOutlineBrush, UnknownCheckFillBrush)
                };
            }

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var gp = FinishedCheckOutlineGPath;

            FitRectInBounds(
                g,
                gp.GetBounds(),
                new RectangleF(0, 0, width, height));

            g.FillPath(
                DarkModeEnabled && filterUnfinished ? _finishedOnFilterOutlineOnlyBrushDark : outlineBrush,
                gp);

            if (!filterUnfinished)
            {
                const int pointsCount = 14;
                for (int i = 0, j = 7; j < pointsCount; i++, j++)
                {
                    FinishedCheckInnerPoints[i] = gp.PathPoints[j];
                    FinishedCheckInnerTypes[i] = gp.PathTypes[j];
                }

                using var innerGP = new GraphicsPath(
                    FinishedCheckInnerPoints,
                    FinishedCheckInnerTypes);

                g.FillPath(fillBrush, innerGP);
            }

            return bmp;
        }

        private static void FillCircle21(Graphics g, Brush brush)
        {
            g.FillEllipse(
                brush,
                new RectangleF(-0.5f, -0.5f, 20.8f, 20.8f)
            );
        }

        private static Bitmap CreateGreenCheckCircleImage()
        {
            var ret = new Bitmap(21, 21, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(ret);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            FillCircle21(g, GreenCircleBrush);

            var gp = CircleCheckGPath;

            FitRectInBounds(
                g,
                gp.GetBounds(),
                new RectangleF(1.7f, 1.7f, 16.5f, 16.5f));

            g.FillPath(
                DarkModeEnabled ? DarkColors.Fen_DarkBackgroundBrush : Brushes.White,
                gp
            );

            return ret;
        }

        private static Bitmap CreateDeleteFromDBImage()
        {
            var ret = new Bitmap(16, 16, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(ret);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var gp = XGPath;

            FitRectInBounds(
                g,
                gp.GetBounds(),
                new RectangleF(0, 0, 14.75f, 14.75f));

            g.FillPath(
                DeleteFromDBBrush,
                gp
            );

            return ret;
        }

        private static Bitmap CreateCalendarImage(bool lastPlayed, bool darkMode)
        {
            var ret = new Bitmap(21, 21, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(ret);

            g.SmoothingMode = SmoothingMode.None;

            (Brush bgBrush, Pen bgPen) =
                darkMode
                    ? (_calendarBackgroundBrushDark, _calendarBackgroundPenDark)
                    : (_calendarBackgroundBrush, _calendarBackgroundPen);

            (Brush fgBrush, Pen fgPen) =
                lastPlayed
                    ? darkMode
                        ? (_lastPlayedForegroundBrushDark, _lastPlayedForegroundPenDark)
                        : (_lastPlayedForegroundBrush, _lastPlayedForegroundPen)
                    : darkMode
                        ? (_releaseDateForegroundBrushDark, _releaseDateForegroundPenDark)
                        : (_releaseDateForegroundBrush, _releaseDateForegroundPen);

            // Top bar
            g.FillRectangle(fgBrush, 0, 2, 21, 4);

            // Main section background
            g.FillRectangle(bgBrush, 1, 8, 19, 11);

            // Main section outline
            g.DrawRectangle(fgPen, 0, 7, 20, 12);

            // Vertical line 1
            g.DrawLine(fgPen, 5, 8, 5, 18);
            // Vertical line 2
            g.DrawLine(fgPen, 10, 8, 10, 18);
            // Vertical line 3
            g.DrawLine(fgPen, 15, 8, 15, 18);

            // Horizontal line 1
            g.DrawLine(fgPen, 1, 11, 19, 11);
            // Horizontal line 2
            g.DrawLine(fgPen, 1, 15, 19, 15);

            if (lastPlayed)
            {
                // Erase middle of horizontal lines on right half
                g.DrawLine(bgPen, 12, 11, 17, 11);
                g.DrawLine(bgPen, 12, 15, 17, 15);

                // Erase middle of vertical line on right half
                g.DrawLine(bgPen, 15, 9, 15, 17);

                PaintArrow9x5(
                    g: g,
                    direction: Direction.Right,
                    area: new Rectangle(13, 9, 5, 9),
                    pen: fgPen);
            }

            return ret;
        }

        #region Rating example

        private static Bitmap CreateRatingExampleRectangle(bool darkMode)
        {
            var ret = new Bitmap(79, 23, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(ret);
            g.FillRectangle(darkMode ? DarkColors.Fen_DarkBackgroundBrush : SystemBrushes.Window, 1, 1, 77, 21);

            var borderRect = new Rectangle(0, 0, 78, 22);

            Pen pen = darkMode ? DarkColors.Fen_DGVCellBordersPen : SystemPens.ControlDark;
            g.DrawRectangle(pen, borderRect);

            return ret;
        }

        private static Bitmap CreateRatingExample_Number(bool outOfFive, bool darkMode)
        {
            Bitmap ret = CreateRatingExampleRectangle(darkMode);

            using var g = Graphics.FromImage(ret);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using Font font = ControlUtils.GetMicrosoftSansSerifDefault();
            TextRenderer.DrawText(
                dc: g,
                text: outOfFive ? "3.5" : "7",
                font: font,
                pt: new Point(1, 5),
                foreColor: darkMode ? DarkColors.Fen_DarkForeground : SystemColors.ControlText,
                backColor: darkMode ? DarkColors.Fen_DarkBackground : SystemColors.Window);

            return ret;
        }

        private static Bitmap CreateRatingExample_FMSel_Stars(bool darkMode)
        {
            Bitmap ret;
            Bitmap? _starEmpty = null;
            Bitmap? _starRightEmpty = null;
            Bitmap? _starFull = null;
            try
            {
                const int px = 14;

                Bitmap GetStarEmpty() => _starEmpty = CreateStarImage(StarEmptyGPath, px);
                Bitmap GetStarRightEmpty() => _starRightEmpty = CreateStarImage(StarRightEmptyGPath, px);
                Bitmap GetStarFull() => _starFull ??= CreateStarImage(StarFullGPath, px);

                ret = CreateRatingExampleRectangle(darkMode);

                using var g = Graphics.FromImage(ret);

                g.SmoothingMode = SmoothingMode.AntiAlias;

                float x = 4;
                const float y = 3.5f;
                for (int i = 0; i < 3; i++, x += px) g.DrawImage(GetStarFull(), x, y);
                g.DrawImage(GetStarRightEmpty(), x, y);
                g.DrawImage(GetStarEmpty(), x + px, y);
            }
            finally
            {
                _starEmpty?.Dispose();
                _starRightEmpty?.Dispose();
                _starFull?.Dispose();
            }
            return ret;
        }

        #endregion

        private static Bitmap CreateStarImage(GraphicsPath gp, int px)
        {
            PointF[] points = new PointF[11];
            byte[] types = new byte[11];

            var bmp = new Bitmap(px, px, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            FitRectInBounds(g, gp.GetBounds(), new RectangleF(0, 0, px, px));

            Brush[] brushes = { StarOutlineBrush, StarFillBrush, StarEmptyBrush };

            int elemCount = 11;

            for (int i = 0, pos = 0; i < 3; i++, pos += 11)
            {
                if (i == 2)
                {
                    if ((elemCount = gp.PointCount - 22) == 0) break;
                    Array.Resize(ref points, elemCount);
                    Array.Resize(ref types, elemCount);
                }

                Array.Copy(gp.PathPoints, pos, points, 0, elemCount);
                Array.Copy(gp.PathTypes, pos, types, 0, elemCount);

                using var individualGP = new GraphicsPath(points, types);
                g.FillPath(brushes[i], individualGP);
            }

            return bmp;
        }

        private static GraphicsPath GetZoomImageGraphicsPath(Zoom zoomType)
        {
            int index = (int)zoomType;
            if (_zoomImageGraphicsPaths[index] == null)
            {
                var gp = new GraphicsPath();
                gp.AddPath(MagnifierEmptyGPath, true);
                gp.AddPath(MakeGraphicsPath(_zoomTypePoints[index], _zoomTypeTypes[index]), true);

                _zoomImageGraphicsPaths[index] = gp;
            }
            return _zoomImageGraphicsPaths[index]!;
        }

        #region Vector helpers

        // Believe it or not, I actually save space by having this massive complicated method rather than a few
        // very small byte arrays. I guess byte arrays must take up more space than you might think, or something.
        private static byte[] MakeTypeArray(params (byte FillValue, int FillCount, int Prefix, int Suffix)[] sets)
        {
            int setsLen = sets.Length;

            int totalArrayLen = 0;
            for (int i = 0; i < setsLen; i++)
            {
                var (_, fillCount, prefix, suffix) = sets[i];
                totalArrayLen += fillCount + (prefix > -1 ? 1 : 0) + (suffix > -1 ? 1 : 0);
            }
            byte[] ret = new byte[totalArrayLen];

            int pos = 0;
            for (int i = 0; i < setsLen; i++)
            {
                var (fillValue, fillCount, prefix, suffix) = sets[i];

                if (prefix > -1) ret[pos++] = (byte)prefix;

                int j;
                for (j = pos; j < pos + fillCount; j++) ret[j] = fillValue;
                pos = j;

                if (suffix > -1) ret[pos++] = (byte)suffix;
            }

            return ret;
        }

        private static GraphicsPath MakeGraphicsPath(float[] points, byte[] types)
        {
            int pointsCount = points.Length;
            var rawPoints = new PointF[pointsCount / 2];
            for (int i = 0, j = 0; i < pointsCount; i += 2, j++)
            {
                rawPoints[j] = new PointF(points[i], points[i + 1]);
            }
            return new GraphicsPath(rawPoints, types);
        }

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

        private static void SetSmoothingMode(PaintEventArgs e, SmoothingMode mode)
        {
            if (e.Graphics.SmoothingMode != mode) e.Graphics.SmoothingMode = mode;
        }

        #endregion

        #endregion

        #endregion

        #region Vector

        // Normally you would use images pulled from Resources for this. But to avoid bloating up our executable
        // and bogging down startup time, we just draw images ourselves where it's reasonable to do so.

        #region Buttons

        internal static void PaintZoomButtons(Button button, PaintEventArgs e, Zoom zoomType)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

            Brush brush = button.Enabled ? BlackForegroundBrush : SystemBrushes.ControlDark;

            var gPath = GetZoomImageGraphicsPath(zoomType);
            FitRectInBounds(e.Graphics, gPath.GetBounds(), button.ClientRectangle);
            e.Graphics.FillPath(brush, gPath);
        }

        internal static void PaintPlayFMButton(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);
            e.Graphics.FillPolygon(button.Enabled ? PlayArrowBrush : SystemBrushes.ControlDark, _playArrowPoints);
        }

        internal static void PaintPlayOriginalButton(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);
            e.Graphics.DrawPolygon(button.Enabled ? PlayArrowPen : PlayArrowDisabledPen, _playOriginalArrowPoints);
        }

        internal static void PaintBitmapButton(
            Button button,
            PaintEventArgs e,
            Image img,
            int x = 0,
            int? y = null,
            RectangleF? scaledRect = null)
        {
            if (scaledRect != null)
            {
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.DrawImage(img, (RectangleF)scaledRect);
            }
            else
            {
                y ??= (button.Height - img.Height) / 2;
                e.Graphics.DrawImage(img, x, (int)y);
            }
        }

        internal static void PaintPlusButton(Button button, PaintEventArgs e)
        {
            var hRect = new Rectangle((button.ClientRectangle.Width / 2) - 4, button.ClientRectangle.Height / 2, 10, 2);
            var vRect = new Rectangle(button.ClientRectangle.Width / 2, (button.ClientRectangle.Height / 2) - 4, 2, 10);
            (_plusRects[0], _plusRects[1]) = (hRect, vRect);
            e.Graphics.FillRectangles(button.Enabled ? BlackForegroundBrush : SystemBrushes.ControlDark, _plusRects);
        }

        internal static void PaintMinusButton(Button button, PaintEventArgs e)
        {
            var hRect = new Rectangle((button.ClientRectangle.Width / 2) - 4, button.ClientRectangle.Height / 2, 10, 2);
            e.Graphics.FillRectangle(button.Enabled ? BlackForegroundBrush : SystemBrushes.ControlDark, hRect);
        }

        internal static void PaintExButton(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

            int minDimension = (Math.Min(button.Width, button.Height) - 1).ClampToZero();
            float leftAndTop = GetValueFromPercent_Float(30.43f, minDimension);
            float widthAndHeight = minDimension - (leftAndTop * 2);

            float x = leftAndTop, y = leftAndTop;

            if (button.Width > button.Height)
            {
                x = ((float)button.Width / 2) - (widthAndHeight / 2);
            }
            else if (button.Height > button.Width)
            {
                y = ((float)button.Height / 2) - (widthAndHeight / 2);
            }

            FitRectInBounds(e.Graphics, XGPath.GetBounds(), new RectangleF(x, y, widthAndHeight, widthAndHeight));

            e.Graphics.FillPath(button.Enabled ? BlackForegroundBrush : SystemBrushes.ControlDark, XGPath);
        }

        internal static void PaintHamburgerMenuButton_TopRight(Button button, PaintEventArgs e)
        {
            e.Graphics.FillRectangles(button.Enabled ? BlackForegroundBrush : SystemBrushes.ControlDark, _hamRects_TopRightMenu);
        }

        internal static void PaintHamburgerMenuButton24(Button button, PaintEventArgs e)
        {
            e.Graphics.FillRectangles(button.Enabled ? BlackForegroundBrush : SystemBrushes.ControlDark, _hamRects24);
        }

        internal static void PaintWebSearchButton(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

            Pen pen = button.Enabled ? WebSearchCirclePen : _webSearchCircleDisabledPen;

            e.Graphics.DrawEllipse(pen, 10, 6, 23, 23);
            e.Graphics.DrawEllipse(pen, 17, 6, 9, 23);
            e.Graphics.FillRectangles(pen.Brush, _webSearchRects);
        }

        internal static void PaintReadmeFullScreenButton(Button button, PaintEventArgs e)
        {
            Brush brush = button.Enabled ? BlackForegroundBrush : SystemBrushes.ControlDark;

            e.Graphics.FillPolygon(brush, _readmeFullScreenTopLeft);
            e.Graphics.FillPolygon(brush, _readmeFullScreenTopRight);
            e.Graphics.FillPolygon(brush, _readmeFullScreenBottomLeft);
            e.Graphics.FillPolygon(brush, _readmeFullScreenBottomRight);
        }

        internal static void PaintReadmeEncodingButton(Button button, PaintEventArgs e)
        {
            Pen pen = button.Enabled ? BlackForegroundPen : DarkModeEnabled ? DarkColors.GreySelectionPen : SystemPens.ControlDark;

            e.Graphics.DrawRectangle(pen, 0, 0, 20, 20);
            e.Graphics.DrawRectangle(pen, 1, 1, 18, 18);

            e.Graphics.DrawImage(button.Enabled ? CharEncLetter : CharEncLetter_Disabled, 3, 3);
        }

        internal static void PaintResetLayoutButton(Button button, PaintEventArgs e)
        {
            Pen pen = button.Enabled ? _resetLayoutPen : _resetLayoutPenDisabled;
            e.Graphics.DrawRectangle(pen, 3, 3, 16, 16);
            e.Graphics.DrawLine(pen, 13, 2, 13, 10);
            e.Graphics.DrawLine(pen, 2, 11, 18, 11);
        }

        internal static void PaintScanSmallButtons(Button button, PaintEventArgs e)
        {
            SetSmoothingMode(e, SmoothingMode.AntiAlias);

            Brush brush = button.Enabled ? AL_LightBlueBrush : SystemBrushes.ControlDark;

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

        internal static void PaintToolStripSeparators(
            PaintEventArgs e,
            int pixelsFromVerticalEdges,
            ToolStripItem[] items)
        {
            Rectangle sizeBounds = items[0].Bounds;

            int y1 = sizeBounds.Top + pixelsFromVerticalEdges;
            int y2 = sizeBounds.Bottom - pixelsFromVerticalEdges;

            for (int i = 0; i < items.Length; i++)
            {
                ToolStripItem item = items[i];
                if (!item.Visible) continue;
                int l1s = (int)Math.Ceiling((double)item.Margin.Left / 2);
                DrawSeparator(e, Sep1Pen, l1s, y1, y2, item.Bounds.Location.X);
            }
        }

        internal static void PaintControlSeparators(
            PaintEventArgs e,
            int pixelsFromVerticalEdges,
            Control[] items,
            int topOverride = -1,
            int bottomOverride = -1)
        {
            Rectangle sizeBounds = items[0].Bounds;

            int y1 = topOverride > -1 ? topOverride : sizeBounds.Top + pixelsFromVerticalEdges;
            int y2 = bottomOverride > -1 ? bottomOverride : (sizeBounds.Bottom - pixelsFromVerticalEdges) - 1;

            for (int i = 0; i < items.Length; i++)
            {
                Control item = items[i];
                if (!item.Visible) continue;
                int l1s = (int)Math.Ceiling((double)item.Margin.Left / 2);
                DrawSeparator(e, Sep1Pen, l1s, y1, y2, item.Bounds.Location.X);
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
            e.Graphics.DrawLine(line1Pen, sep1x, line1Top, sep1x, line1Bottom);
            if (!DarkModeEnabled)
            {
                int sep2x = (x - line1DistanceBackFromLoc) + 1;
                e.Graphics.DrawLine(Sep2Pen, sep2x, line1Top + 1, sep2x, line1Bottom + 1);
            }
        }

        #endregion

        #region Arrows

        internal static void PaintArrow7x4(
            Graphics g,
            Direction direction,
            Rectangle area,
            bool? controlEnabled = null,
            Pen? pen = null)
        {
            g.SmoothingMode = SmoothingMode.None;

            int x = area.X + (area.Width / 2);
            int y = area.Y + (area.Height / 2);

            pen ??= controlEnabled == true ? ArrowButtonEnabledPen : SystemPens.ControlDark;

            switch (direction)
            {
                case Direction.Left:
                    x -= 2;
                    y -= 3;

                    // Arrow tip
                    g.DrawLine(pen, x, y + 3, x + 1, y + 3);

                    g.DrawLine(pen, x + 1, y + 2, x + 1, y + 4);
                    g.DrawLine(pen, x + 2, y + 1, x + 2, y + 5);
                    g.DrawLine(pen, x + 3, y, x + 3, y + 6);

                    break;
                case Direction.Right:
                    x -= 1;
                    y -= 3;

                    g.DrawLine(pen, x, y, x, y + 6);
                    g.DrawLine(pen, x + 1, y + 1, x + 1, y + 5);
                    g.DrawLine(pen, x + 2, y + 2, x + 2, y + 4);

                    // Arrow tip
                    g.DrawLine(pen, x + 2, y + 3, x + 3, y + 3);

                    break;
                case Direction.Up:
                    x -= 3;
                    y -= 2;

                    // Arrow tip
                    g.DrawLine(pen, x + 3, y, x + 3, y + 1);

                    g.DrawLine(pen, x + 2, y + 1, x + 4, y + 1);
                    g.DrawLine(pen, x + 1, y + 2, x + 5, y + 2);
                    g.DrawLine(pen, x, y + 3, x + 6, y + 3);

                    break;
                case Direction.Down:
                default:
                    x -= 3;
                    y -= 1;

                    g.DrawLine(pen, x, y, x + 6, y);
                    g.DrawLine(pen, x + 1, y + 1, x + 5, y + 1);
                    g.DrawLine(pen, x + 2, y + 2, x + 4, y + 2);

                    // Arrow tip
                    g.DrawLine(pen, x + 3, y + 2, x + 3, y + 3);

                    break;
            }
        }

        internal static void PaintArrow9x5(
            Graphics g,
            Direction direction,
            Rectangle area,
            bool? controlEnabled = null,
            Pen? pen = null)
        {
            g.SmoothingMode = SmoothingMode.None;

            int x = area.X + (area.Width / 2);
            int y = area.Y + (area.Height / 2);

            pen ??= controlEnabled == true ? ArrowButtonEnabledPen : SystemPens.ControlDark;

            switch (direction)
            {
                case Direction.Left:
                    x -= 2;
                    y -= 4;

                    // Arrow tip
                    g.DrawLine(pen, x, y + 4, x + 1, y + 4);

                    g.DrawLine(pen, x + 1, y + 3, x + 1, y + 5);
                    g.DrawLine(pen, x + 2, y + 2, x + 2, y + 6);
                    g.DrawLine(pen, x + 3, y + 1, x + 3, y + 7);
                    g.DrawLine(pen, x + 4, y, x + 4, y + 8);

                    break;
                case Direction.Right:
                    x -= 2;
                    y -= 4;

                    g.DrawLine(pen, x, y, x, y + 8);
                    g.DrawLine(pen, x + 1, y + 1, x + 1, y + 7);
                    g.DrawLine(pen, x + 2, y + 2, x + 2, y + 6);
                    g.DrawLine(pen, x + 3, y + 3, x + 3, y + 5);

                    // Arrow tip
                    g.DrawLine(pen, x + 3, y + 4, x + 4, y + 4);

                    break;
                case Direction.Up:
                    x -= 4;
                    y -= 2;

                    // Arrow tip
                    g.DrawLine(pen, x + 4, y, x + 4, y + 1);

                    g.DrawLine(pen, x + 3, y + 1, x + 5, y + 1);
                    g.DrawLine(pen, x + 2, y + 2, x + 6, y + 2);
                    g.DrawLine(pen, x + 1, y + 3, x + 7, y + 3);
                    g.DrawLine(pen, x, y + 4, x + 8, y + 4);

                    break;
                case Direction.Down:
                default:
                    x -= 4;
                    y -= 2;

                    g.DrawLine(pen, x, y, x + 8, y);
                    g.DrawLine(pen, x + 1, y + 1, x + 7, y + 1);
                    g.DrawLine(pen, x + 2, y + 2, x + 6, y + 2);
                    g.DrawLine(pen, x + 3, y + 3, x + 5, y + 3);

                    // Arrow tip
                    g.DrawLine(pen, x + 4, y + 3, x + 4, y + 4);

                    break;
            }
        }

        #endregion

        #endregion
    }
}

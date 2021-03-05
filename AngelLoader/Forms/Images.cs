using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using AngelLoader.Properties;
using static AngelLoader.Misc;

namespace AngelLoader.Forms
{
    // Pulling an image from Resources is an expensive operation, so if we're going to load an image multiple times,
    // we want to cache it in a Bitmap object. Images that are loaded only once are not present here, as they would
    // derive no performance benefit.
    // NOTE: This class and everything accessible inside it needs to be public for the designers to recognize it.
    public static class Images
    {
        public static bool DarkModeEnabled;

        #region Games

        // @GENGAMES (Images): Begin
        // Putting these into an array doesn't really gain us anything and loses us robustness, so leave them.
        // We would have to say ResourceManager.GetObject(nameof(gameIndex) + "_16") etc. and that doesn't even
        // work due to SS2 -> Shock2_16 etc. naming. We'd have to remember to name our resources properly or it
        // would fail silently, so really it's best to just leave these as hard-converts even though we really
        // want to get rid of individually-specified games.
        private static Bitmap? _thief1_16;
        public static Bitmap Thief1_16 => _thief1_16 ??= Resources.Thief1_16;

        private static Bitmap? _thief2_16;
        public static Bitmap Thief2_16 => _thief2_16 ??= Resources.Thief2_16;

        private static Bitmap? _thief3_16;
        public static Bitmap Thief3_16 => _thief3_16 ??= Resources.Thief3_16;

        private static Bitmap? _shock2_16;
        public static Bitmap Shock2_16 => _shock2_16 ??= Resources.Shock2_16;

        private static Bitmap? _thief1_21;
        public static Bitmap Thief1_21 => _thief1_21 ??= Resources.Thief1_21;

        private static Bitmap? _thief2_21;
        public static Bitmap Thief2_21 => _thief2_21 ??= Resources.Thief2_21;

        private static Bitmap? _thief3_21;
        public static Bitmap Thief3_21 => _thief3_21 ??= Resources.Thief3_21;

        private static Bitmap? _shock2_21;
        public static Bitmap Shock2_21 => _shock2_21 ??= Resources.Shock2_21;
        // @GENGAMES (Images): End

        #endregion

        #region Finished on

        private static Bitmap? _finishedOnUnknown;

        public static Bitmap FinishedOnUnknown
        {
            get
            {
                if (_finishedOnUnknown == null)
                {
                    using Bitmap resBmp = FillFinishedOnBitmap(Difficulty.None);
                    _finishedOnUnknown = new Bitmap(138, 32, PixelFormat.Format32bppPArgb);
                    using var g = Graphics.FromImage(_finishedOnUnknown);
                    g.DrawImage(resBmp, (138 / 2) - (resBmp.Width / 2), 0);
                }
                return _finishedOnUnknown;
            }
        }

        // Designer file isn't able to use a method call. Whatever.
#if DEBUG
        public static Bitmap Debug_Finished => FillFinishedOnBitmap(Difficulty.None, filterFinished: true);
        public static Bitmap Debug_Unfinished => FillFinishedOnBitmap(Difficulty.None, filterUnfinished: true);
#endif

        public static Bitmap FillFinishedOnBitmap(Difficulty difficulty, bool filterFinished = false, bool filterUnfinished = false)
        {
            int width, height;
            Brush outlineBrush, fillBrush;
            if (filterFinished || filterUnfinished)
            {
                width = 24;
                height = 24;
                outlineBrush = ControlPainter.FinishedOnFilterOutlineBrush;
                fillBrush = ControlPainter.FinishedOnFilterFillBrush;
            }
            else
            {
                height = 32;
                // Variations in image widths (34,35,36) are intentional to keep the same dimensions as the old
                // raster images used to have, so that the new vector ones are drop-in replacements.
                (width, outlineBrush, fillBrush) = difficulty switch
                {
                    Difficulty.Normal => (34, ControlPainter.NormalCheckOutlineBrush, ControlPainter.NormalCheckFillBrush),
                    Difficulty.Hard => (35, ControlPainter.HardCheckOutlineBrush, ControlPainter.HardCheckFillBrush),
                    Difficulty.Expert => (35, ControlPainter.ExpertCheckOutlineBrush, ControlPainter.ExpertCheckFillBrush),
                    Difficulty.Extreme => (34, ControlPainter.ExtremeCheckOutlineBrush, ControlPainter.ExtremeCheckFillBrush),
                    _ => (36, ControlPainter.UnknownCheckOutlineBrush, ControlPainter.UnknownCheckFillBrush)
                };
            }

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

            using var g = Graphics.FromImage(bmp);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            var gp = ControlPainter.FinishedCheckOutlineGPath;

            ControlPainter.FitRectInBounds(
                g,
                gp.GetBounds(),
                new RectangleF(0, 0, width, height));

            g.FillPath(
                outlineBrush,
                gp);

            if (!filterUnfinished)
            {
                const int pointsCount = 14;
                for (int i = 0, j = 7; j < pointsCount; i++, j++)
                {
                    ControlPainter.FinishedCheckInnerPoints[i] = gp.PathPoints[j];
                    ControlPainter.FinishedCheckInnerTypes[i] = gp.PathTypes[j];
                }

                using var innerGP = new GraphicsPath(
                    ControlPainter.FinishedCheckInnerPoints,
                    ControlPainter.FinishedCheckInnerTypes);

                g.FillPath(fillBrush, innerGP);
            }

            return bmp;
        }

        /// <summary>
        /// We use positionZeroBitmap so we can be passed an already-constructed BlankIcon bitmap so that we
        /// don't have to have BlankIcon be in here and subject to a property call and null check every time
        /// it gets displayed, which is a lot.
        /// </summary>
        /// <param name="positionZeroBitmap">Silly hack, see description.</param>
        /// <returns></returns>
        public static Bitmap[] GetFinishedOnImages(Bitmap positionZeroBitmap)
        {
            var retArray = new Bitmap[16];

            Bitmap? _finishedOnNormal_single = null;
            Bitmap? _finishedOnHard_single = null;
            Bitmap? _finishedOnExpert_single = null;
            Bitmap? _finishedOnExtreme_single = null;
            try
            {
                #region Image getters

                Bitmap GetFinishedOnNormal_Single() => _finishedOnNormal_single ??= FillFinishedOnBitmap(Difficulty.Normal);
                Bitmap GetFinishedOnHard_Single() => _finishedOnHard_single ??= FillFinishedOnBitmap(Difficulty.Hard);
                Bitmap GetFinishedOnExpert_Single() => _finishedOnExpert_single ??= FillFinishedOnBitmap(Difficulty.Expert);
                Bitmap GetFinishedOnExtreme_Single() => _finishedOnExtreme_single ??= FillFinishedOnBitmap(Difficulty.Extreme);

                #endregion

                var list = new List<Bitmap>(4);

                retArray[0] = positionZeroBitmap;
                for (int ai = 1; ai < retArray.Length; ai++)
                {
                    Bitmap canvas = new Bitmap(138, 32, PixelFormat.Format32bppPArgb);
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

                    retArray[ai] = canvas;
                }
            }
            finally
            {
                _finishedOnNormal_single?.Dispose();
                _finishedOnHard_single?.Dispose();
                _finishedOnExpert_single?.Dispose();
                _finishedOnExtreme_single?.Dispose();
            }

            return retArray;
        }

        #endregion

        #region Stars

        private static Bitmap? _filterByRating;
        public static Bitmap FilterByRating => _filterByRating ??= FillStarImage(ControlPainter.StarFullGPath, 24);

        private static Bitmap FillStarImage(GraphicsPath gp, int px)
        {
            PointF[] points = new PointF[11];
            byte[] types = new byte[11];

            var bmp = new Bitmap(px, px, PixelFormat.Format32bppPArgb);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            ControlPainter.FitRectInBounds(g, gp.GetBounds(), new RectangleF(0, 0, px, px));

            Brush[] brushes = { ControlPainter.StarOutlineBrush, ControlPainter.StarFillBrush, Brushes.White };

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

        public static Bitmap[] GetRatingImages()
        {
            // 0-10, and we don't count -1 (no rating) because that's handled elsewhere
            const int numRatings = 11;
            Bitmap[] retArray = new Bitmap[numRatings];
            bool[] bits = new bool[numRatings];

            Bitmap? _starEmpty = null;
            Bitmap? _starRightEmpty = null;
            Bitmap? _starFull = null;
            try
            {
                #region Image getters

                Bitmap GetStarEmpty() => _starEmpty ??= FillStarImage(ControlPainter.StarEmptyGPath, 22);
                Bitmap GetStarRightEmpty() => _starRightEmpty ??= FillStarImage(ControlPainter.StarRightEmptyGPath, 22);
                Bitmap GetStarFull() => _starFull ??= FillStarImage(ControlPainter.StarFullGPath, 22);

                #endregion

                for (int bi = 0; bi < numRatings; bi++)
                {
                    var canvas = new Bitmap(110, 32, PixelFormat.Format32bppPArgb);

                    using var g = Graphics.FromImage(canvas);

                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    for (int i = 0; i < bits.Length - 1; i += 2)
                    {
                        g.DrawImage(bits[i] ? bits[i + 1] ? GetStarFull() : GetStarRightEmpty() : GetStarEmpty(), (i / 2) * 22, 5);
                    }

                    bits[bi] = true;
                    retArray[bi] = canvas;
                }
            }
            finally
            {
                _starEmpty?.Dispose();
                _starRightEmpty?.Dispose();
                _starFull?.Dispose();
            }

            return retArray;
        }

        #endregion

        private static Icon? _AngelLoader;
        public static Icon AngelLoader => _AngelLoader ??= Resources.AngelLoader;

        private static Image? _playOriginalGame;
        public static Image PlayOriginalGame => _playOriginalGame ??= Resources.Play_Original_24;

        private static Image? _playOriginalGame_Disabled;
        public static Image PlayOriginalGame_Disabled => _playOriginalGame_Disabled ??= ToolStripRenderer.CreateDisabledImage(PlayOriginalGame);

        #region Install / uninstall

        private static Image? _install_24;
        public static Image Install_24 => _install_24 ??= Resources.Install_24;

        private static Image? _install_24_Disabled;
        public static Image Install_24_Disabled => _install_24_Disabled ??= ToolStripRenderer.CreateDisabledImage(Install_24);

        private static Image? _uninstall_24;
        public static Image Uninstall_24 => _uninstall_24 ??= Resources.Uninstall_24;

        private static Image? _uninstall_24_Disabled;
        public static Image Uninstall_24_Disabled => _uninstall_24_Disabled ??= ToolStripRenderer.CreateDisabledImage(Uninstall_24);

        #endregion

        private static Image? _import;
        public static Image Import => _import ??= Resources.Import_24;

        private static Image? __import_Disabled;
        public static Image Import_Disabled => __import_Disabled ??= ToolStripRenderer.CreateDisabledImage(Import);

        private static Image? _settings;
        public static Image Settings => _settings ??= Resources.Settings_24;

        private static Image? _settings_Disabled;
        public static Image Settings_Disabled => _settings_Disabled ??= ToolStripRenderer.CreateDisabledImage(Settings);

        #region Zoom

        private static readonly Bitmap?[] _zoomImages = new Bitmap?[ZoomTypesCount];
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
                ControlPainter.FitRectInBounds(
                    g,
                    ControlPainter.GetZoomImageComplete(zoomType).GetBounds(),
                    new RectangleF(0, 0, rect.Width, rect.Height));
                g.FillPath(ControlPainter.BlackForegroundBrush, ControlPainter.GetZoomImageComplete(zoomType));
            }
            return _zoomImages[index]!;
        }

        #endregion

        private static Bitmap? _refresh_Classic;
        private static Bitmap? _refresh_Dark;
        public static Bitmap Refresh =>
            DarkModeEnabled
                ? _refresh_Dark ??= Resources.Refresh_DarkMode
                : _refresh_Classic ??= Resources.Refresh;

        #region Rating example

        // TODO: @DarkMode: Change star colors to be appropriate for dark mode

        private static Bitmap? _ratingExample_NDL_Classic;
        private static Bitmap? _ratingExample_NDL_Dark;
        public static Bitmap RatingExample_NDL =>
            DarkModeEnabled
                ? _ratingExample_NDL_Dark ??= Resources.RatingExample_NDL_Dark
                : _ratingExample_NDL_Classic ??= Resources.RatingExample_NDL;

        private static Bitmap? _ratingExample_FMSel_Stars_Classic;
        private static Bitmap? _ratingExample_FMSel_Stars_Dark;
        public static Bitmap RatingExample_FMSel_Stars =>
            DarkModeEnabled
                ? _ratingExample_FMSel_Stars_Dark ??= GetRatingExample_FMSel_Stars(darkMode: true)
                : _ratingExample_FMSel_Stars_Classic ??= GetRatingExample_FMSel_Stars(darkMode: false);

        private static Bitmap GetRatingExample_FMSel_Stars(bool darkMode)
        {
            Bitmap ret;
            Bitmap? _starEmpty = null;
            Bitmap? _starRightEmpty = null;
            Bitmap? _starFull = null;
            try
            {
                const int px = 14;

                Bitmap GetStarEmpty() => _starEmpty = FillStarImage(ControlPainter.StarEmptyGPath, px);
                Bitmap GetStarRightEmpty() => _starRightEmpty = FillStarImage(ControlPainter.StarRightEmptyGPath, px);
                Bitmap GetStarFull() => _starFull ??= FillStarImage(ControlPainter.StarFullGPath, px);

                ret = new Bitmap(79, 23, PixelFormat.Format32bppPArgb);
                using var g = Graphics.FromImage(ret);
                g.FillRectangle(darkMode ? DarkColors.Fen_DarkBackgroundBrush : Brushes.White, 1, 1, 77, 21);

                var borderRect = new Rectangle(0, 0, 78, 22);

                using var pen = new Pen(darkMode ? Color.FromArgb(64, 64, 64) : Color.FromArgb(160, 160, 160));
                g.DrawRectangle(pen, borderRect);

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

        private static Bitmap? _ratingExample_FMSel_Number_Classic;
        private static Bitmap? _ratingExample_FMSel_Number_Dark;
        public static Bitmap RatingExample_FMSel_Number =>
            DarkModeEnabled
                ? _ratingExample_FMSel_Number_Dark ??= Resources.RatingExample_FMSel_Number_Dark
                : _ratingExample_FMSel_Number_Classic ??= Resources.RatingExample_FMSel_Number;

        #endregion
    }
}

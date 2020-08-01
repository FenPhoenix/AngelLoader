using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using AngelLoader.DataClasses;
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
                width = 32;
                height = 32;
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
        /// <param name="regenerate"></param>
        /// <returns></returns>
        public static Bitmap[] GetFinishedOnImages(Bitmap positionZeroBitmap, bool regenerate = false)
        {
            var retArray = new Bitmap[16];

            Bitmap? _finishedOnNormal_single = null;
            Bitmap? _finishedOnHard_single = null;
            Bitmap? _finishedOnExpert_single = null;
            Bitmap? _finishedOnExtreme_single = null;
            try
            {
                #region Image getters

                Bitmap GetFinishedOnNormal_Single()
                {
                    if (regenerate || _finishedOnNormal_single == null)
                    {
                        _finishedOnNormal_single = FillFinishedOnBitmap(Difficulty.Normal);
                    }
                    return _finishedOnNormal_single;
                }

                Bitmap GetFinishedOnHard_Single()
                {
                    if (regenerate || _finishedOnHard_single == null)
                    {
                        _finishedOnHard_single = FillFinishedOnBitmap(Difficulty.Hard);
                    }
                    return _finishedOnHard_single;
                }

                Bitmap GetFinishedOnExpert_Single()
                {
                    if (regenerate || _finishedOnExpert_single == null)
                    {
                        _finishedOnExpert_single = FillFinishedOnBitmap(Difficulty.Expert);
                    }
                    return _finishedOnExpert_single;
                }

                Bitmap GetFinishedOnExtreme_Single()
                {
                    if (regenerate || _finishedOnExtreme_single == null)
                    {
                        _finishedOnExtreme_single = FillFinishedOnBitmap(Difficulty.Extreme);
                    }
                    return _finishedOnExtreme_single;
                }

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
                    // that's why we get get the width of each individual image , rather than keeping a constant.
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
        public static Bitmap FilterByRating
        {
            get
            {
                if (_filterByRating == null)
                {
                    //using var frb1 = Resources.FilterByRating_half;
                    //using var frb2 = (Bitmap)frb1.Clone();
                    //frb2.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    _filterByRating = new Bitmap(32, 32, PixelFormat.Format32bppPArgb);
                    //using var g = Graphics.FromImage(_filterByRating);
                    //g.DrawImage(frb1, 0, 0);
                    //g.DrawImage(frb2, 16, 0);
                }
                return _filterByRating;
            }
        }

        public static Bitmap[] GetRatingImages()
        {
            // Just coincidence that these numbers are the same; don't combine
            //const int halfStarWidth = 11;
            // 0-10, and we don't count -1 (no rating) because that's handled elsewhere
            const int numRatings = 11;

            var retArray = new Bitmap[numRatings];

            var eGP = ControlPainter.StarEmptyGPath;
            var reGP = ControlPainter.StarRightEmptyGPath;
            var fGP = ControlPainter.StarFullGPath;

            bool[] bits = new bool[numRatings];

            for (int ai = 0; ai < numRatings; ai++)
            {
                var canvas = new Bitmap(110, 32, PixelFormat.Format32bppPArgb);

                Array.Clear(bits, 0, numRatings);
                for (int i = 0; i < ai; i++) bits[i] = true;

                //int x = 0;
                using (var g = Graphics.FromImage(canvas))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    var drawBounds = eGP.GetBounds();
                    //for (int i = 0; i < bits.Length; i++)
                    //{
                    //    bool bitSet = bits[i];
                    //    bool left = i % 2 == 0;

                    //    if (bitSet)
                    //    {
                    //        g.DrawImage(left ? GetFilledLeft() : GetFilledRight(), x, 0);
                    //    }
                    //    else
                    //    {
                    //        g.DrawImage(left ? GetEmptyLeft() : GetEmptyRight(), x, 0);
                    //    }
                    //    x += halfStarWidth;
                    //}
                    for (int i2 = 0; i2 < bits.Length - 1; i2 += 2)
                    {
                        var fitBounds = new RectangleF((i2 / 2) * 22, 5, 22, 22);
                        ControlPainter.FitRectInBounds(g, drawBounds, fitBounds);

                        bool bit1Set = bits[i2];
                        bool bit2Set = bits[i2 + 1];

                        if (bit1Set)
                        {
                            if (bit2Set)
                            {
                                // draw full
                                PointF[] oGPPoints = new PointF[11];
                                byte[] oGPTypes = new byte[11];
                                Array.Copy(fGP.PathPoints, 0, oGPPoints, 0, 11);
                                Array.Copy(fGP.PathTypes, 0, oGPTypes, 0, 11);
                                var oGP = new GraphicsPath(oGPPoints, oGPTypes);

                                PointF[] mGPPoints = new PointF[11];
                                byte[] mGPTypes = new byte[11];
                                Array.Copy(fGP.PathPoints, 11, mGPPoints, 0, 11);
                                Array.Copy(fGP.PathTypes, 11, mGPTypes, 0, 11);
                                var mGP = new GraphicsPath(mGPPoints, mGPTypes);

                                g.FillPath(ControlPainter.StarOutlineBrush, oGP);
                                g.FillPath(ControlPainter.StarFillBrush, mGP);
                            }
                            else
                            {
                                // draw right empty
                                PointF[] oGPPoints = new PointF[11];
                                byte[] oGPTypes = new byte[11];
                                Array.Copy(reGP.PathPoints, 0, oGPPoints, 0, 11);
                                Array.Copy(reGP.PathTypes, 0, oGPTypes, 0, 11);
                                var oGP = new GraphicsPath(oGPPoints, oGPTypes);

                                PointF[] mGPPoints = new PointF[11];
                                byte[] mGPTypes = new byte[11];
                                Array.Copy(reGP.PathPoints, 11, mGPPoints, 0, 11);
                                Array.Copy(reGP.PathTypes, 11, mGPTypes, 0, 11);
                                var mGP = new GraphicsPath(mGPPoints, mGPTypes);

                                PointF[] iGPPoints = new PointF[7];
                                byte[] iGPTypes = new byte[7];
                                Array.Copy(reGP.PathPoints, 22, iGPPoints, 0, 7);
                                Array.Copy(reGP.PathTypes, 22, iGPTypes, 0, 7);
                                var iGP = new GraphicsPath(iGPPoints, iGPTypes);

                                g.FillPath(ControlPainter.StarOutlineBrush, oGP);
                                g.FillPath(ControlPainter.StarFillBrush, mGP);
                                g.FillPath(Brushes.White, iGP);
                            }
                        }
                        else
                        {
                            // draw empty
                            PointF[] oGPPoints = new PointF[11];
                            byte[] oGPTypes = new byte[11];
                            Array.Copy(eGP.PathPoints, 0, oGPPoints, 0, 11);
                            Array.Copy(eGP.PathTypes, 0, oGPTypes, 0, 11);
                            var oGP = new GraphicsPath(oGPPoints, oGPTypes);

                            PointF[] mGPPoints = new PointF[11];
                            byte[] mGPTypes = new byte[11];
                            Array.Copy(eGP.PathPoints, 11, mGPPoints, 0, 11);
                            Array.Copy(eGP.PathTypes, 11, mGPTypes, 0, 11);
                            var mGP = new GraphicsPath(mGPPoints, mGPTypes);

                            PointF[] iGPPoints = new PointF[11];
                            byte[] iGPTypes = new byte[11];
                            Array.Copy(eGP.PathPoints, 22, iGPPoints, 0, 11);
                            Array.Copy(eGP.PathTypes, 22, iGPTypes, 0, 11);
                            var iGP = new GraphicsPath(iGPPoints, iGPTypes);

                            g.FillPath(ControlPainter.StarOutlineBrush, oGP);
                            g.FillPath(ControlPainter.StarFillBrush, mGP);
                            g.FillPath(Brushes.White, iGP);
                        }
                    }
                }

                retArray[ai] = canvas;
            }

            return retArray;
        }

        #endregion

        private static Icon? _AngelLoader;
        public static Icon AngelLoader => _AngelLoader ??= Resources.AngelLoader;

        private static Bitmap? _install_24;
        public static Bitmap Install_24 => _install_24 ??= Resources.Install_24;

        private static Bitmap? _uninstall_24;
        public static Bitmap Uninstall_24 => _uninstall_24 ??= Resources.Uninstall_24;

        #region Zoom

        private static readonly Bitmap?[] _zoomImages = new Bitmap?[ZoomTypesCount];
        // We can't use the Paint even to paint the image on ToolStrip crap, as it's tool strip crap, you know.
        // It just bugs out in various different ways. So we just paint on an image and set their image to that.
        // Reconstruct param is for when we're changing size/scale and need to redraw it.
        public static Bitmap GetZoomImage(int width, int height, Zoom zoomType, bool reconstruct = false)
        {
            int index = (int)zoomType;
            if (reconstruct || _zoomImages[index] == null)
            {
                _zoomImages[index] = new Bitmap(width, height);
                using var g = Graphics.FromImage(_zoomImages[index]!);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                ControlPainter.FitRectInBounds(
                    g,
                    ControlPainter.GetZoomImageComplete(zoomType).GetBounds(),
                    new RectangleF(0, 0, width, height));
                g.FillPath(Brushes.Black, ControlPainter.GetZoomImageComplete(zoomType));
            }
            return _zoomImages[index]!;
        }

        #endregion

        private static Bitmap? _refresh;
        public static Bitmap Refresh => _refresh ??= Resources.Refresh;

        private static Bitmap? _ratingExample_NDL;
        public static Bitmap RatingExample_NDL => _ratingExample_NDL ??= Resources.RatingExample_NDL;

        private static Bitmap? _ratingExample_FMSel_Stars;
        public static Bitmap RatingExample_FMSel_Stars => _ratingExample_FMSel_Stars ??= Resources.RatingExample_FMSel_Stars;

        private static Bitmap? _ratingExample_FMSel_Number;
        public static Bitmap RatingExample_FMSel_Number => _ratingExample_FMSel_Number ??= Resources.RatingExample_FMSel_Number;
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using AngelLoader.DataClasses;
using AngelLoader.Properties;

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

                Bitmap GetFinishedOnNormal_Single() => _finishedOnNormal_single ??= Resources.Finished_Normal_Icon;

                Bitmap GetFinishedOnHard_Single() => _finishedOnHard_single ??= Resources.Finished_Hard_Icon;

                Bitmap GetFinishedOnExpert_Single() => _finishedOnExpert_single ??= Resources.Finished_Expert_Icon;

                Bitmap GetFinishedOnExtreme_Single() => _finishedOnExtreme_single ??= Resources.Finished_Extreme_Icon;

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

        public static Bitmap[] GetRatingImages()
        {
            const int halfStarWidth = 11;
            // 0-10, and we don't count -1 (no rating) because that's handled elsewhere
            const int numRatings = 11;

            var retArray = new Bitmap[numRatings];

            Bitmap? _star_Filled_Left_Half = null;
            Bitmap? _star_Filled_Right_Half = null;
            Bitmap? _star_Empty_Left_Half = null;
            Bitmap? _star_Empty_Right_Half = null;
            try
            {
                #region Image getters

                Bitmap GetFilledLeft() => _star_Filled_Left_Half ??= Resources.Star_Filled_Half;

                Bitmap GetFilledRight()
                {
                    if (_star_Filled_Right_Half == null)
                    {
                        _star_Filled_Right_Half = Resources.Star_Filled_Half;
                        _star_Filled_Right_Half.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    }
                    return _star_Filled_Right_Half;
                }

                Bitmap GetEmptyLeft() => _star_Empty_Left_Half ??= Resources.Star_Empty_Half;

                Bitmap GetEmptyRight()
                {
                    if (_star_Empty_Right_Half == null)
                    {
                        _star_Empty_Right_Half = Resources.Star_Empty_Half;
                        _star_Empty_Right_Half.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    }
                    return _star_Empty_Right_Half;
                }

                #endregion

                bool[] bits = new bool[numRatings];

                for (int ai = 0; ai < numRatings; ai++)
                {
                    var canvas = new Bitmap(110, 32, PixelFormat.Format32bppPArgb);

                    Array.Clear(bits, 0, numRatings);
                    for (int i = 0; i < ai; i++) bits[i] = true;

                    int x = 0;
                    using (var g = Graphics.FromImage(canvas))
                    {
                        for (int i = 0; i < bits.Length; i++)
                        {
                            bool bitSet = bits[i];
                            bool left = i % 2 == 0;

                            if (bitSet)
                            {
                                g.DrawImage(left ? GetFilledLeft() : GetFilledRight(), x, 0);
                            }
                            else
                            {
                                g.DrawImage(left ? GetEmptyLeft() : GetEmptyRight(), x, 0);
                            }
                            x += halfStarWidth;
                        }
                    }

                    retArray[ai] = canvas;
                }
            }
            finally
            {
                _star_Filled_Left_Half?.Dispose();
                _star_Filled_Right_Half?.Dispose();
                _star_Empty_Left_Half?.Dispose();
                _star_Empty_Right_Half?.Dispose();
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

        private static Bitmap? _zoomIn;
        public static Bitmap ZoomIn => _zoomIn ??= Resources.ZoomIn;

        private static Bitmap? _zoomOut;
        public static Bitmap ZoomOut => _zoomOut ??= Resources.ZoomOut;

        private static Bitmap? _zoomReset;
        public static Bitmap ZoomReset => _zoomReset ??= Resources.ZoomReset;

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

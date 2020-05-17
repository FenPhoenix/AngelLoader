using System.Drawing;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    // This class and everything accessible inside it needs to be public for the designers to recognize it
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

using System.Drawing;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    // This class and everything accessible inside it needs to be public for the designers to recognize it
    // @GENGAMES: There's game-related stuff in here
    public static class Images
    {
        // @GENGAMES: Putting these into an array doesn't really gain us anything and loses us robustness, so leave them
        // We would have to say ResourceManager.GetObject(nameof(gameIndex) + "_16") etc. and that doesn't even
        // work due to SS2 -> Shock2_16 etc. naming. We'd have to remember to name our resources properly or it
        // would fail silently, so really it's best to just leave these as hard-converts even though we really
        // want to get rid of individually-specified games.
        private static Bitmap? _Thief1_16;
        private static Bitmap? _Thief2_16;
        private static Bitmap? _Thief3_16;
        private static Bitmap? _Shock2_16;

        private static Bitmap? _Thief1_21;
        private static Bitmap? _Thief2_21;
        private static Bitmap? _Thief3_21;
        private static Bitmap? _Shock2_21;

        private static Icon? _AngelLoader;
        private static Bitmap? _Install_24;
        private static Bitmap? _Uninstall_24;
        private static Bitmap? _ZoomIn;
        private static Bitmap? _ZoomOut;
        private static Bitmap? _ZoomReset;
        private static Bitmap? _Refresh;
        private static Bitmap? _RatingExample_NDL;
        private static Bitmap? _RatingExample_FMSel_Stars;
        private static Bitmap? _RatingExample_FMSel_Number;

        public static Bitmap Thief1_16 => _Thief1_16 ??= Resources.Thief1_16;

        public static Bitmap Thief2_16 => _Thief2_16 ??= Resources.Thief2_16;

        public static Bitmap Thief3_16 => _Thief3_16 ??= Resources.Thief3_16;

        public static Bitmap Shock2_16 => _Shock2_16 ??= Resources.Shock2_16;

        public static Bitmap Thief1_21 => _Thief1_21 ??= Resources.Thief1_21;

        public static Bitmap Thief2_21 => _Thief2_21 ??= Resources.Thief2_21;

        public static Bitmap Thief3_21 => _Thief3_21 ??= Resources.Thief3_21;

        public static Bitmap Shock2_21 => _Shock2_21 ??= Resources.Shock2_21;

        public static Icon AngelLoader => _AngelLoader ??= Resources.AngelLoader;

        public static Bitmap Install_24 => _Install_24 ??= Resources.Install_24;

        public static Bitmap Uninstall_24 => _Uninstall_24 ??= Resources.Uninstall_24;

        public static Bitmap ZoomIn => _ZoomIn ??= Resources.ZoomIn;

        public static Bitmap ZoomOut => _ZoomOut ??= Resources.ZoomOut;

        public static Bitmap ZoomReset => _ZoomReset ??= Resources.ZoomReset;

        public static Bitmap Refresh => _Refresh ??= Resources.Refresh;

        public static Bitmap RatingExample_NDL => _RatingExample_NDL ??= Resources.RatingExample_NDL;

        public static Bitmap RatingExample_FMSel_Stars => _RatingExample_FMSel_Stars ??= Resources.RatingExample_FMSel_Stars;

        public static Bitmap RatingExample_FMSel_Number => _RatingExample_FMSel_Number ??= Resources.RatingExample_FMSel_Number;
    }
}

using System.Drawing;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    // This class and everything accessible inside it needs to be public for the designers to recognize it
    public static class Images
    {
        private static Bitmap _Thief1_16;
        private static Bitmap _Thief2_16;
        private static Bitmap _Thief3_16;
        private static Bitmap _Shock2_16;

        private static Bitmap _Thief1_21;
        private static Bitmap _Thief2_21;
        private static Bitmap _Thief3_21;
        private static Bitmap _Shock2_21;

        private static Icon _AngelLoader;
        private static Bitmap _Install_24;
        private static Bitmap _Uninstall_24;
        private static Bitmap _ZoomIn;
        private static Bitmap _ZoomOut;
        private static Bitmap _ZoomReset;
        private static Bitmap _Refresh;
        private static Bitmap _RatingExample_NDL;
        private static Bitmap _RatingExample_FMSel_Stars;
        private static Bitmap _RatingExample_FMSel_Number;

        private static bool c_Thief1_16;
        private static bool c_Thief2_16;
        private static bool c_Thief3_16;
        private static bool c_Shock2_16;

        private static bool c_Thief1_21;
        private static bool c_Thief2_21;
        private static bool c_Thief3_21;
        private static bool c_Shock2_21;

        private static bool c_AngelLoader;
        private static bool c_Install_24;
        private static bool c_Uninstall_24;
        private static bool c_ZoomIn;
        private static bool c_ZoomOut;
        private static bool c_ZoomReset;
        private static bool c_Refresh;
        private static bool c_RatingExample_NDL;
        private static bool c_RatingExample_FMSel_Stars;
        private static bool c_RatingExample_FMSel_Number;

        public static Bitmap Thief1_16
        {
            get
            {
                if (c_Thief1_16)
                {
                    return _Thief1_16;
                }
                else
                {
                    c_Thief1_16 = true;
                    return _Thief1_16 = Resources.Thief1_16;
                }
            }
        }

        public static Bitmap Thief2_16
        {
            get
            {
                if (c_Thief2_16)
                {
                    return _Thief2_16;
                }
                else
                {
                    c_Thief2_16 = true;
                    return _Thief2_16 = Resources.Thief2_16;
                }
            }
        }

        public static Bitmap Thief3_16
        {
            get
            {
                if (c_Thief3_16)
                {
                    return _Thief3_16;
                }
                else
                {
                    c_Thief3_16 = true;
                    return _Thief3_16 = Resources.Thief3_16;
                }
            }
        }

        public static Bitmap Shock2_16
        {
            get
            {
                if (c_Shock2_16)
                {
                    return _Shock2_16;
                }
                else
                {
                    c_Shock2_16 = true;
                    return _Shock2_16 = Resources.Shock2_16;
                }
            }
        }

        public static Bitmap Thief1_21
        {
            get
            {
                if (c_Thief1_21)
                {
                    return _Thief1_21;
                }
                else
                {
                    c_Thief1_21 = true;
                    return _Thief1_21 = Resources.Thief1_21;
                }
            }
        }

        public static Bitmap Thief2_21
        {
            get
            {
                if (c_Thief2_21)
                {
                    return _Thief2_21;
                }
                else
                {
                    c_Thief2_21 = true;
                    return _Thief2_21 = Resources.Thief2_21;
                }
            }
        }

        public static Bitmap Thief3_21
        {
            get
            {
                if (c_Thief3_21)
                {
                    return _Thief3_21;
                }
                else
                {
                    c_Thief3_21 = true;
                    return _Thief3_21 = Resources.Thief3_21;
                }
            }
        }

        public static Bitmap Shock2_21
        {
            get
            {
                if (c_Shock2_21)
                {
                    return _Shock2_21;
                }
                else
                {
                    c_Shock2_21 = true;
                    return _Shock2_21 = Resources.Shock2_21;
                }
            }
        }

        public static Icon AngelLoader
        {
            get
            {
                if (c_AngelLoader)
                {
                    return _AngelLoader;
                }
                else
                {
                    c_AngelLoader = true;
                    return _AngelLoader = Resources.AngelLoader;
                }
            }
        }

        public static Bitmap Install_24
        {
            get
            {
                if (c_Install_24)
                {
                    return _Install_24;
                }
                else
                {
                    c_Install_24 = true;
                    return _Install_24 = Resources.Install_24;
                }
            }
        }

        public static Bitmap Uninstall_24
        {
            get
            {
                if (c_Uninstall_24)
                {
                    return _Uninstall_24;
                }
                else
                {
                    c_Uninstall_24 = true;
                    return _Uninstall_24 = Resources.Uninstall_24;
                }
            }
        }

        public static Bitmap ZoomIn
        {
            get
            {
                if (c_ZoomIn)
                {
                    return _ZoomIn;
                }
                else
                {
                    c_ZoomIn = true;
                    return _ZoomIn = Resources.ZoomIn;
                }
            }
        }

        public static Bitmap ZoomOut
        {
            get
            {
                if (c_ZoomOut)
                {
                    return _ZoomOut;
                }
                else
                {
                    c_ZoomOut = true;
                    return _ZoomOut = Resources.ZoomOut;
                }
            }
        }

        public static Bitmap ZoomReset
        {
            get
            {
                if (c_ZoomReset)
                {
                    return _ZoomReset;
                }
                else
                {
                    c_ZoomReset = true;
                    return _ZoomReset = Resources.ZoomReset;
                }
            }
        }

        public static Bitmap Refresh
        {
            get
            {
                if (c_Refresh)
                {
                    return _Refresh;
                }
                else
                {
                    c_Refresh = true;
                    return _Refresh = Resources.Refresh;
                }
            }
        }

        public static Bitmap RatingExample_NDL
        {
            get
            {
                if (c_RatingExample_NDL)
                {
                    return _RatingExample_NDL;
                }
                else
                {
                    c_RatingExample_NDL = true;
                    return _RatingExample_NDL = Resources.RatingExample_NDL;
                }
            }
        }

        public static Bitmap RatingExample_FMSel_Stars
        {
            get
            {
                if (c_RatingExample_FMSel_Stars)
                {
                    return _RatingExample_FMSel_Stars;
                }
                else
                {
                    c_RatingExample_FMSel_Stars = true;
                    return _RatingExample_FMSel_Stars = Resources.RatingExample_FMSel_Stars;
                }
            }
        }

        public static Bitmap RatingExample_FMSel_Number
        {
            get
            {
                if (c_RatingExample_FMSel_Number)
                {
                    return _RatingExample_FMSel_Number;
                }
                else
                {
                    c_RatingExample_FMSel_Number = true;
                    return _RatingExample_FMSel_Number = Resources.RatingExample_FMSel_Number;
                }
            }
        }
    }
}

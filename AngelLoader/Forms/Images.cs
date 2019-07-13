using System.Drawing;
using AngelLoader.Properties;

namespace AngelLoader.Forms
{
    internal static class Images
    {
        private static Bitmap _Add;
        private static Bitmap _Minus;
        private static Bitmap _Thief1_16;
        private static Bitmap _Thief2_16;
        private static Bitmap _Thief3_16;
        private static Bitmap _Thief1_21;
        private static Bitmap _Thief2_21;
        private static Bitmap _Thief3_21;
        private static Icon _AngelLoader;
        private static Bitmap _ArrowLeftSmall;
        private static Bitmap _ArrowRightSmall;
        private static Bitmap _Remove;
        private static Bitmap _Install_24;
        private static Bitmap _Uninstall_24;
        private static Bitmap _ZoomIn;
        private static Bitmap _ZoomOut;
        private static Bitmap _ZoomReset;
        private static Bitmap _Refresh;
        private static Bitmap _ScanSmall;
        private static Bitmap _RatingExample_NDL;
        private static Bitmap _RatingExample_FMSel_Stars;
        private static Bitmap _RatingExample_FMSel_Number;

        private static bool c_Add;
        private static bool c_Minus;
        private static bool c_Thief1_16;
        private static bool c_Thief2_16;
        private static bool c_Thief3_16;
        private static bool c_Thief1_21;
        private static bool c_Thief2_21;
        private static bool c_Thief3_21;
        private static bool c_AngelLoader;
        private static bool c_ArrowLeftSmall;
        private static bool c_ArrowRightSmall;
        private static bool c_Remove;
        private static bool c_Install_24;
        private static bool c_Uninstall_24;
        private static bool c_ZoomIn;
        private static bool c_ZoomOut;
        private static bool c_ZoomReset;
        private static bool c_Refresh;
        private static bool c_ScanSmall;
        private static bool c_RatingExample_NDL;
        private static bool c_RatingExample_FMSel_Stars;
        private static bool c_RatingExample_FMSel_Number;

        internal static Bitmap Add
        {
            get
            {
                if (c_Add)
                {
                    return _Add;
                }
                else
                {
                    c_Add = true;
                    return _Add = Resources.Add;
                }
            }
        }

        internal static Bitmap Minus
        {
            get
            {
                if (c_Minus)
                {
                    return _Minus;
                }
                else
                {
                    c_Minus = true;
                    return _Minus = Resources.Minus;
                }
            }
        }

        internal static Bitmap Thief1_16
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

        internal static Bitmap Thief2_16
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

        internal static Bitmap Thief3_16
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

        internal static Bitmap Thief1_21
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

        internal static Bitmap Thief2_21
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

        internal static Bitmap Thief3_21
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

        internal static Icon AngelLoader
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

        internal static Bitmap ArrowLeftSmall
        {
            get
            {
                if (c_ArrowLeftSmall)
                {
                    return _ArrowLeftSmall;
                }
                else
                {
                    c_ArrowLeftSmall = true;
                    return _ArrowLeftSmall = Resources.ArrowLeftSmall;
                }
            }
        }

        internal static Bitmap ArrowRightSmall
        {
            get
            {
                if (c_ArrowRightSmall)
                {
                    return _ArrowRightSmall;
                }
                else
                {
                    c_ArrowRightSmall = true;
                    return _ArrowRightSmall = Resources.ArrowRightSmall;
                }
            }
        }

        internal static Bitmap Remove
        {
            get
            {
                if (c_Remove)
                {
                    return _Remove;
                }
                else
                {
                    c_Remove = true;
                    return _Remove = Resources.Remove;
                }
            }
        }

        internal static Bitmap Install_24
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

        internal static Bitmap Uninstall_24
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

        internal static Bitmap ZoomIn
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

        internal static Bitmap ZoomOut
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

        internal static Bitmap ZoomReset
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

        internal static Bitmap Refresh
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

        internal static Bitmap ScanSmall
        {
            get
            {
                if (c_ScanSmall)
                {
                    return _ScanSmall;
                }
                else
                {
                    c_ScanSmall = true;
                    return _ScanSmall = Resources.ScanSmall;
                }
            }
        }

        internal static Bitmap RatingExample_NDL
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

        internal static Bitmap RatingExample_FMSel_Stars
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

        internal static Bitmap RatingExample_FMSel_Number
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

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
        private static Bitmap _Scan;
        private static Bitmap _Settings_24;
        private static Bitmap _Import_24;
        private static Bitmap _PlayArrow_24;
        private static Bitmap _Play_Original_24;
        private static Bitmap _WebSearch_24;
        private static Bitmap _FilterByReleaseDate;
        private static Bitmap _FilterByLastPlayed;
        private static Bitmap _FilterByTags;
        private static Bitmap _FilterByFinished;
        private static Bitmap _FilterByUnfinished;
        private static Bitmap _FilterByRating;
        private static Bitmap _Show_Unsupported;
        private static Bitmap _ZoomIn;
        private static Bitmap _ZoomOut;
        private static Bitmap _ZoomReset;
        private static Bitmap _FindNewFMs_21;
        private static Bitmap _Refresh;
        private static Bitmap _ClearFilters;
        private static Bitmap _ResetLayout;
        private static Bitmap _Hamburger_16;
        private static Bitmap _ScanSmall;
        private static Bitmap _Fullscreen;
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
        private static bool c_Scan;
        private static bool c_Settings_24;
        private static bool c_Import_24;
        private static bool c_PlayArrow_24;
        private static bool c_Play_Original_24;
        private static bool c_WebSearch_24;
        private static bool c_FilterByReleaseDate;
        private static bool c_FilterByLastPlayed;
        private static bool c_FilterByTags;
        private static bool c_FilterByFinished;
        private static bool c_FilterByUnfinished;
        private static bool c_FilterByRating;
        private static bool c_Show_Unsupported;
        private static bool c_ZoomIn;
        private static bool c_ZoomOut;
        private static bool c_ZoomReset;
        private static bool c_FindNewFMs_21;
        private static bool c_Refresh;
        private static bool c_ClearFilters;
        private static bool c_ResetLayout;
        private static bool c_Hamburger_16;
        private static bool c_ScanSmall;
        private static bool c_Fullscreen;
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

        internal static Bitmap Scan
        {
            get
            {
                if (c_Scan)
                {
                    return _Scan;
                }
                else
                {
                    c_Scan = true;
                    return _Scan = Resources.Scan;
                }
            }
        }

        internal static Bitmap Settings_24
        {
            get
            {
                if (c_Settings_24)
                {
                    return _Settings_24;
                }
                else
                {
                    c_Settings_24 = true;
                    return _Settings_24 = Resources.Settings_24;
                }
            }
        }

        internal static Bitmap Import_24
        {
            get
            {
                if (c_Import_24)
                {
                    return _Import_24;
                }
                else
                {
                    c_Import_24 = true;
                    return _Import_24 = Resources.Import_24;
                }
            }
        }

        internal static Bitmap PlayArrow_24
        {
            get
            {
                if (c_PlayArrow_24)
                {
                    return _PlayArrow_24;
                }
                else
                {
                    c_PlayArrow_24 = true;
                    return _PlayArrow_24 = Resources.PlayArrow_24;
                }
            }
        }

        internal static Bitmap Play_Original_24
        {
            get
            {
                if (c_Play_Original_24)
                {
                    return _Play_Original_24;
                }
                else
                {
                    c_Play_Original_24 = true;
                    return _Play_Original_24 = Resources.Play_original_24;
                }
            }
        }

        internal static Bitmap WebSearch_24
        {
            get
            {
                if (c_WebSearch_24)
                {
                    return _WebSearch_24;
                }
                else
                {
                    c_WebSearch_24 = true;
                    return _WebSearch_24 = Resources.WebSearch_24;
                }
            }
        }

        internal static Bitmap FilterByReleaseDate
        {
            get
            {
                if (c_FilterByReleaseDate)
                {
                    return _FilterByReleaseDate;
                }
                else
                {
                    c_FilterByReleaseDate = true;
                    return _FilterByReleaseDate = Resources.FilterByReleaseDate;
                }
            }
        }

        internal static Bitmap FilterByLastPlayed
        {
            get
            {
                if (c_FilterByLastPlayed)
                {
                    return _FilterByLastPlayed;
                }
                else
                {
                    c_FilterByLastPlayed = true;
                    return _FilterByLastPlayed = Resources.FilterByLastPlayed;
                }
            }
        }

        internal static Bitmap FilterByTags
        {
            get
            {
                if (c_FilterByTags)
                {
                    return _FilterByTags;
                }
                else
                {
                    c_FilterByTags = true;
                    return _FilterByTags = Resources.FilterByTags;
                }
            }
        }

        internal static Bitmap FilterByFinished
        {
            get
            {
                if (c_FilterByFinished)
                {
                    return _FilterByFinished;
                }
                else
                {
                    c_FilterByFinished = true;
                    return _FilterByFinished = Resources.FilterByFinished;
                }
            }
        }

        internal static Bitmap FilterByUnfinished
        {
            get
            {
                if (c_FilterByUnfinished)
                {
                    return _FilterByUnfinished;
                }
                else
                {
                    c_FilterByUnfinished = true;
                    return _FilterByUnfinished = Resources.FilterByUnfinished;
                }
            }
        }

        internal static Bitmap FilterByRating
        {
            get
            {
                if (c_FilterByRating)
                {
                    return _FilterByRating;
                }
                else
                {
                    c_FilterByRating = true;
                    return _FilterByRating = Resources.FilterByRating;
                }
            }
        }

        internal static Bitmap Show_Unsupported
        {
            get
            {
                if (c_Show_Unsupported)
                {
                    return _Show_Unsupported;
                }
                else
                {
                    c_Show_Unsupported = true;
                    return _Show_Unsupported = Resources.Show_Unsupported;
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

        internal static Bitmap FindNewFMs_21
        {
            get
            {
                if (c_FindNewFMs_21)
                {
                    return _FindNewFMs_21;
                }
                else
                {
                    c_FindNewFMs_21 = true;
                    return _FindNewFMs_21 = Resources.FindNewFMs_21;
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

        internal static Bitmap ClearFilters
        {
            get
            {
                if (c_ClearFilters)
                {
                    return _ClearFilters;
                }
                else
                {
                    c_ClearFilters = true;
                    return _ClearFilters = Resources.ClearFilters;
                }
            }
        }

        internal static Bitmap ResetLayout
        {
            get
            {
                if (c_ResetLayout)
                {
                    return _ResetLayout;
                }
                else
                {
                    c_ResetLayout = true;
                    return _ResetLayout = Resources.ResetLayout;
                }
            }
        }

        internal static Bitmap Hamburger_16
        {
            get
            {
                if (c_Hamburger_16)
                {
                    return _Hamburger_16;
                }
                else
                {
                    c_Hamburger_16 = true;
                    return _Hamburger_16 = Resources.Hamburger_16;
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

        internal static Bitmap Fullscreen
        {
            get
            {
                if (c_Fullscreen)
                {
                    return _Fullscreen;
                }
                else
                {
                    c_Fullscreen = true;
                    return _Fullscreen = Resources.Fullscreen;
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

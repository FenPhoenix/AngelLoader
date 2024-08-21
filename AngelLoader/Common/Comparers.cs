using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using AL_Common;
using AngelLoader.DataClasses;
using FMScanner;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using Game = AngelLoader.GameSupport.Game;

namespace AngelLoader;

// TODO(Comparers): Make reverse-sort columns start with arrow down by default

internal static class Comparers
{
    #region Fields

    #region FM column comparers

    // It takes like 0.2ms to construct these and mere bytes of memory per, so don't bother with the bloat of lazy loading
    // ReSharper disable once RedundantExplicitArraySize
    internal static readonly IDirectionalSortFMComparer[] ColumnComparers = new IDirectionalSortFMComparer[ColumnCount]
    {
#if DateAccTest
        new FMReleaseDateComparer(),
#endif
        new FMGameComparer(),
        new FMInstalledComparer(),
        new FMMisCountComparer(),
        new FMTitleComparer(),
        new FMArchiveComparer(),
        new FMAuthorComparer(),
        new FMSizeComparer(),
        new FMRatingComparer(),
        new FMFinishedComparer(),
        new FMReleaseDateComparer(),
        new FMLastPlayedComparer(),
        new FMDateAddedComparer(),
        new FMPlayTimeComparer(),
        new FMDisabledModsComparer(),
        new FMCommentComparer(),
    };

    #endregion

    #region Misc comparers

    internal static readonly FileNameNoExtComparer FileNameNoExt = new();

    internal static readonly ScreenshotComparer Screenshot = new();

    internal static readonly FMScanOriginalIndexComparer FMScanOriginalIndex = new();

    #endregion

    #endregion

    #region Classes

    internal interface IDirectionalSortFMComparer : IComparer<FanMission>
    {
        SortDirection SortDirection { set; }
    }

    #region FM list sorting

    #region Static methods

    // From .NET Framework 4.8 source
    private static unsafe bool EqualsHelper(string strA_Str, string strB_Str, int length)
    {
        ReadOnlySpan<char> strA = strA_Str.AsSpan();
        ReadOnlySpan<char> strB = strB_Str.AsSpan();

        fixed (char* ap = &MemoryMarshal.GetReference(strA))
        fixed (char* bp = &MemoryMarshal.GetReference(strB))
        {
            char* a = ap;
            char* b = bp;

            // unroll the loop
#if X64
            // for AMD64 bit platform we unroll by 12 and
            // check 3 qword at a time. This is less code
            // than the 32 bit case and is shorter
            // pathlength

            while (length >= 12)
            {
                if (*(long*)a != *(long*)b) return false;
                if (*(long*)(a + 4) != *(long*)(b + 4)) return false;
                if (*(long*)(a + 8) != *(long*)(b + 8)) return false;
                a += 12; b += 12; length -= 12;
            }
#else
            while (length >= 10)
            {
                if (*(int*)a != *(int*)b) return false;
                if (*(int*)(a + 2) != *(int*)(b + 2)) return false;
                if (*(int*)(a + 4) != *(int*)(b + 4)) return false;
                if (*(int*)(a + 6) != *(int*)(b + 6)) return false;
                if (*(int*)(a + 8) != *(int*)(b + 8)) return false;
                a += 10; b += 10; length -= 10;
            }
#endif

            // This depends on the fact that the String objects are
            // always zero terminated and that the terminating zero is not included
            // in the length. For odd string sizes, the last compare will include
            // the zero terminator.
            while (length > 0)
            {
                if (*(int*)a != *(int*)b) break;
                a += 2; b += 2; length -= 2;
            }

            return length <= 0;
        }
    }

    private static bool EqualsFast(string str1, string str2)
    {
        int str1Length = str1.Length;
        return str1Length == str2.Length && EqualsHelper(str1, str2, str1Length);
    }

    // Static for perf - this gets called from most comparer classes and we don't want to be instantiating
    // new title-sort classes in a loop!
    private static int TitleCompare(FanMission x, FanMission y)
    {
        string title1 = x.Title;
        string title2 = y.Title;

        int title1Length = title1.Length;
        int title2Length = title2.Length;

        /*
        Domain knowledge:
        -The two strings are never the same reference
        -Neither string is null
        -The two strings are unlikely to be equal - a char-wise equality check is likely to quit early
        Elide the reference equality and null checks and use the copied-out equals helper, and we save a few dozen
        ms on the huge set.
        */
        if (title1Length == title2Length && EqualsHelper(title1, title2, title1Length))
        {
            int earlyRet = string.Compare(x.Archive, y.Archive, StringComparison.InvariantCultureIgnoreCase);
            return earlyRet != 0
                ? earlyRet
                : string.Compare(x.InstalledDir, y.InstalledDir, StringComparison.InvariantCultureIgnoreCase);
        }

        if (title1Length == 0) return -1;
        if (title2Length == 0) return 1;

        int ret;

        if (Config.EnableArticles)
        {
            int xStart = 0;
            int yStart = 0;

            bool xArticleSet = false;
            bool yArticleSet = false;

            for (int i = 0; i < Config.Articles.Count; i++)
            {
                string article = Config.Articles[i];
                int aLen = article.Length;

                if (!xArticleSet && title1.StartsWithIPlusWhiteSpace(article, aLen))
                {
                    xStart = aLen + 1;
                    xArticleSet = true;
                }
                if (!yArticleSet && title2.StartsWithIPlusWhiteSpace(article, aLen))
                {
                    yStart = aLen + 1;
                    yArticleSet = true;
                }
            }

            ret = (xStart | yStart) == 0
                ? string.Compare(title1, title2, StringComparison.InvariantCultureIgnoreCase)
                : string.Compare(title1, xStart, title2, yStart, Math.Max(title1Length, title2Length),
                    StringComparison.InvariantCultureIgnoreCase);
        }
        else
        {
            ret = string.Compare(title1, title2, StringComparison.InvariantCultureIgnoreCase);
        }

        if (ret != 0) return ret;

        ret = string.Compare(x.Archive, y.Archive, StringComparison.InvariantCultureIgnoreCase);

        return ret != 0
            ? ret
            : string.Compare(x.InstalledDir, y.InstalledDir, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    #region Column comparers

    // Having every comparer inherit this (instead of having its own implemented property) reduces file size
    private class ColumnComparer
    {
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    }

    private sealed class FMTitleComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret = TitleCompare(x, y);
            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

#if DateAccTest
    private sealed class FMDateAccuracyComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.DateAccuracy == y.DateAccuracy ? TitleCompare(x, y) :
                x.DateAccuracy == DateAccuracy.Null ? -1 :
                y.DateAccuracy == DateAccuracy.Null ? 1 :
                x.DateAccuracy < y.DateAccuracy ? -1 : 1;

            ret = -ret;

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }
#endif

    private sealed class FMGameComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.Game == y.Game ? TitleCompare(x, y) :
                x.Game == Game.Null ? -1 :
                y.Game == Game.Null ? 1 :
                x.Game < y.Game ? -1 : 1;

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMInstalledComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.Installed == y.Installed ? TitleCompare(x, y) :
                // Installed goes on top, non-installed (blank icon) goes on bottom
                x.Installed ? -1 : 1;

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMMisCountComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.MisCount == y.MisCount ? TitleCompare(x, y) :
                x.MisCount == -1 ? -1 :
                y.MisCount == -1 ? 1 :
                x.MisCount < y.MisCount ? -1 : 1;

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMArchiveComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            string xArchive = x.DisplayArchive;
            string yArchive = y.DisplayArchive;

            int ret =
                EqualsFast(xArchive, yArchive) ? TitleCompare(x, y) :
                    string.Compare(xArchive, yArchive, StringComparison.InvariantCultureIgnoreCase);

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMAuthorComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            string xAuthor = x.Author;
            string yAuthor = y.Author;

            int ret =
                EqualsFast(xAuthor, yAuthor) ? TitleCompare(x, y) :
                xAuthor.Length == 0 ? -1 :
                yAuthor.Length == 0 ? 1 :
                string.Compare(xAuthor, yAuthor, StringComparison.InvariantCultureIgnoreCase);

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMSizeComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.SizeBytes == y.SizeBytes ? TitleCompare(x, y) :
                x.SizeBytes == 0 ? -1 :
                y.SizeBytes == 0 ? 1 :
                x.SizeBytes < y.SizeBytes ? -1 : 1;

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMRatingComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            // Working
#if false
            if (false)
            {
                int one = SortDirection == SortDirection.Ascending ? 1 : -1;

                ret =
                    x.Rating == y.Rating ? TitleCompare(x, y) :
                    x.Rating == -1 && y.Rating > -1 ? one :
                    x.Rating > -1 && y.Rating == -1 ? -one :
                    x.Rating > -1 && y.Rating > -1 && x.Rating < y.Rating ? -1 : 1;
            }
            else
#endif
            {
                ret =
                    x.Rating == y.Rating ? TitleCompare(x, y) :
                    x.Rating == -1 ? -1 :
                    y.Rating == -1 ? 1 :
                    x.Rating < y.Rating ? -1 : 1;
            }

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMFinishedComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            // Working: will add new option for this when done
#if false
            if (false)
            {
                ret =
                    (x.FinishedOnUnknown && y.FinishedOnUnknown) || x.FinishedOn == y.FinishedOn
                        ? TitleCompare(x, y) :
                    !x.FinishedOnUnknown && y.FinishedOnUnknown ? -1 :
                    x.FinishedOnUnknown && !y.FinishedOnUnknown ? 1 :
                    x.FinishedOn == 0 && y.FinishedOn > 0 ? 1 :
                    x.FinishedOn > 0 && y.FinishedOn == 0 ? -1 :
                    x.FinishedOn > 0 && y.FinishedOn > 0 && x.FinishedOn < y.FinishedOn ? -1 : 1;
            }
            else
#endif
            {
                ret =
                    !x.FinishedOnUnknown && y.FinishedOnUnknown ? -1 :
                        x.FinishedOnUnknown && !y.FinishedOnUnknown ? 1 :
                            (x.FinishedOnUnknown && y.FinishedOnUnknown) || x.FinishedOn == y.FinishedOn
                                ? TitleCompare(x, y) :
                                x.FinishedOn < y.FinishedOn ? -1 : 1;
            }

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMReleaseDateComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            // Sort this one down to the day only, because the exact time may very well not be known, and
            // even if it is, it's not visible or editable anywhere and it'd be weird to have missions
            // sorted out of name order because of an invisible time difference.
            int ret;
            if (x.ReleaseDate.DateTime == null && y.ReleaseDate.DateTime == null)
            {
                ret = TitleCompare(x, y);
            }
            else if (x.ReleaseDate.DateTime == null)
            {
                ret = -1;
            }
            else if (y.ReleaseDate.DateTime == null)
            {
                ret = 1;
            }
            else
            {
                int cmp = ((DateTime)x.ReleaseDate.DateTime).Date.CompareTo(((DateTime)y.ReleaseDate.DateTime).Date);
                ret = cmp == 0 ? TitleCompare(x, y) : cmp;
            }

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMLastPlayedComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            if (x.LastPlayed.DateTime == null && y.LastPlayed.DateTime == null)
            {
                ret = TitleCompare(x, y);
            }
            else if (x.LastPlayed.DateTime == null)
            {
                ret = -1;
            }
            else if (y.LastPlayed.DateTime == null)
            {
                ret = 1;
            }
            else
            {
                // Sort this one by exact DateTime because the time is (indirectly) changeable down to the
                // second (you change it by playing it), and the user will expect precise sorting.
                int cmp = ((DateTime)x.LastPlayed.DateTime).CompareTo((DateTime)y.LastPlayed.DateTime);
                ret = cmp == 0 ? TitleCompare(x, y) : cmp;
            }

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMDateAddedComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            if (x.DateAdded == null && y.DateAdded == null)
            {
                ret = TitleCompare(x, y);
            }
            else if (x.DateAdded == null)
            {
                ret = -1;
            }
            else if (y.DateAdded == null)
            {
                ret = 1;
            }
            else
            {
                // Sorting this one by exact DateTime is the appropriate method here
                int cmp = ((DateTime)x.DateAdded).CompareTo((DateTime)y.DateAdded);
                ret = cmp == 0 ? TitleCompare(x, y) : cmp;
            }

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMPlayTimeComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            {
                int cmp = x.PlayTime.CompareTo(y.PlayTime);
                ret = cmp == 0 ? TitleCompare(x, y) : cmp;
            }

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMDisabledModsComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            if (x.DisableAllMods && !y.DisableAllMods)
            {
                ret = -1;
            }
            else
            {
                if (!x.DisableAllMods && y.DisableAllMods)
                {
                    ret = 1;
                }
                else
                {
                    string xDisabledMods = x.DisabledMods;
                    string yDisabledMods = y.DisabledMods;

                    ret = (x.DisableAllMods && y.DisableAllMods) || EqualsFast(xDisabledMods, yDisabledMods)
                        ? TitleCompare(x, y)
                        // Sort this column content-first for better UX
                        : xDisabledMods.Length == 0
                            ? 1
                            : yDisabledMods.Length == 0
                                ? -1
                                : string.Compare(xDisabledMods, yDisabledMods,
                                    StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    private sealed class FMCommentComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            string xCommentSingleLine = x.CommentSingleLine;
            string yCommentSingleLine = y.CommentSingleLine;

            int ret =
                EqualsFast(xCommentSingleLine, yCommentSingleLine) ? TitleCompare(x, y) :
                // Sort this column content-first for better UX
                xCommentSingleLine.Length == 0 ? 1 :
                yCommentSingleLine.Length == 0 ? -1 :
                string.Compare(xCommentSingleLine, yCommentSingleLine, StringComparison.InvariantCultureIgnoreCase);

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    #endregion

    #endregion

    internal sealed class FileNameNoExtComparer : IComparer<string>
    {
        public int Compare(string x, string y) =>
            x == y ? 0 :
            x.IsEmpty() ? -1 :
            y.IsEmpty() ? 1 :
            string.Compare(
                Path.GetFileNameWithoutExtension(x),
                Path.GetFileNameWithoutExtension(y),
                StringComparison.OrdinalIgnoreCase);
    }

    internal sealed class ScreenshotComparer : IComparer<FileInfo>
    {
        public SortDirection SortDirection = SortDirection.Ascending;

        public int Compare(FileInfo x, FileInfo y)
        {
            int cmp = x.LastWriteTime.CompareTo(y.LastWriteTime);
            int ret = cmp == 0
                // @TDM_CASE when file is a TDM screenshot
                ? string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
                : cmp;
            return SortDirection == SortDirection.Descending ? -ret : ret;
        }
    }

    internal sealed class FMScanOriginalIndexComparer : IComparer<ScannedFMDataAndError>
    {
        public int Compare(ScannedFMDataAndError x, ScannedFMDataAndError y)
        {
            return x.OriginalIndex.CompareTo(y.OriginalIndex);
        }
    }

#if SMART_NEW_COLUMN_INSERT
    internal sealed class ValidatorColumnComparer : IComparer<ColumnData>
    {
        public int Compare(ColumnData x, ColumnData y) =>
            x.DisplayIndex == y.DisplayIndex
                ? x.ExplicitlySet == y.ExplicitlySet ? 0 : x.ExplicitlySet ? 1 : -1
                : x.DisplayIndex < y.DisplayIndex
                    ? -1
                    : 1;
    }
#endif

    #endregion
}

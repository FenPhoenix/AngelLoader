using System;
using System.Collections.Generic;
using System.IO;
using AngelLoader.DataClasses;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

// This is a completely stupid .NET modern thing. We know the comparer objects won't be null. No need to add tons
// of ! operators or unnecessary null checks.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

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

    #endregion

    #endregion

    #region Classes

    internal interface IDirectionalSortFMComparer : IComparer<FanMission>
    {
        SortDirection SortDirection { set; }
    }

    #region FM list sorting

    #region Static methods

    // Static for perf - this gets called from most comparer classes and we don't want to be instantiating
    // new title-sort classes in a loop!
    private static int TitleCompare(FanMission x, FanMission y)
    {
        string title1 = x.Title;
        string title2 = y.Title;

        /*
        Domain knowledge:
        -The two strings are never the same reference
        -Neither string is null
        -The two strings are unlikely to be equal - a char-wise equality check is likely to quit early

        @NET5(TitleCompare):
        .NET 8's equality check is some crazy perf-optimized thing that's bonkers insane to read, so let's assume
        for now that it's at least as fast as Framework and probably much faster. But test with the huge set if
        we ever want to switch to .NET modern for public release.
        */
        if (title1.AsSpan().SequenceEqual(title2.AsSpan()))
        {
            int earlyRet = string.Compare(x.Archive, y.Archive, StringComparison.InvariantCultureIgnoreCase);
            return earlyRet != 0
                ? earlyRet
                : string.Compare(x.InstalledDir, y.InstalledDir, StringComparison.InvariantCultureIgnoreCase);
        }

        int title1Length = title1.Length;
        int title2Length = title2.Length;

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
                xArchive.AsSpan().SequenceEqual(yArchive.AsSpan()) ? TitleCompare(x, y) :
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
                xAuthor.AsSpan().SequenceEqual(yAuthor.AsSpan()) ? TitleCompare(x, y) :
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

                    ret = (x.DisableAllMods && y.DisableAllMods) || xDisabledMods.AsSpan().SequenceEqual(yDisabledMods.AsSpan())
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
                xCommentSingleLine.AsSpan().SequenceEqual(yCommentSingleLine.AsSpan()) ? TitleCompare(x, y) :
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
            Path.GetFileNameWithoutExtension(x.AsSpan()).CompareTo(
                Path.GetFileNameWithoutExtension(y.AsSpan()),
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

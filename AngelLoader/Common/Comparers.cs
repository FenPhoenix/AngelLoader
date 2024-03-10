using System;
using System.Collections.Generic;
using System.IO;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;

// This is a completely stupid .NET modern thing. We know the comparer objects won't be null. No need to add tons
// of ! operators or unnecessary null checks.
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

namespace AngelLoader;

// TODO(Comparers): Make reverse-sort columns start with arrow down by default

[PublicAPI]
internal static class Comparers
{
    #region Fields

    #region FM column comparers

    // It takes like 0.2ms to construct these and mere bytes of memory per, so don't bother with the bloat of lazy loading
    internal static readonly IDirectionalSortFMComparer[] ColumnComparers =
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
        new FMDisabledModsComparer(),
        new FMCommentComparer()
    };

    #endregion

    #region Misc comparers

    internal static FileNameNoExtComparer FileNameNoExt = new();

    internal static ScreenshotComparer Screenshot = new();

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
        static int TitleOrFallback(
            string title1,
            string title2,
            FanMission fm1,
            FanMission fm2,
            bool compareTitles = true,
            int xStart = 0,
            int yStart = 0)
        {
            int ret;
            if (compareTitles)
            {
                ret = string.Compare(title1, xStart, title2, yStart, Math.Max(title1.Length, title2.Length),
                    StringComparison.InvariantCultureIgnoreCase);
                if (ret != 0) return ret;
            }

            ret = string.Compare(fm1.Archive, fm2.Archive, StringComparison.InvariantCultureIgnoreCase);
            if (ret != 0) return ret;

            return string.Compare(fm1.InstalledDir, fm2.InstalledDir, StringComparison.InvariantCultureIgnoreCase);
        }

        if (x.Title == y.Title) return TitleOrFallback(x.Title, y.Title, x, y, compareTitles: false);
        if (x.Title.IsEmpty()) return -1;
        if (y.Title.IsEmpty()) return 1;

        int xStart = 0;
        int yStart = 0;

        if (Config.EnableArticles)
        {
            bool xArticleSet = false;
            bool yArticleSet = false;

            foreach (string article in Config.Articles)
            {
                int aLen = article.Length;

                if (!xArticleSet && x.Title.StartsWithIPlusWhiteSpace(article, aLen))
                {
                    xStart = aLen + 1;
                    xArticleSet = true;
                }
                if (!yArticleSet && y.Title.StartsWithIPlusWhiteSpace(article, aLen))
                {
                    yStart = aLen + 1;
                    yArticleSet = true;
                }
            }
        }

        return TitleOrFallback(x.Title, y.Title, x, y, xStart: xStart, yStart: yStart);
    }

    #endregion

    #region Column comparers

    // Having every comparer inherit this (instead of having its own implemented property) reduces file size
    internal class ColumnComparer
    {
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    }

    internal sealed class FMTitleComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret = TitleCompare(x, y);
            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

#if DateAccTest
    internal sealed class FMDateAccuracyComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMGameComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMInstalledComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.Installed == y.Installed ? TitleCompare(x, y) :
                // Installed goes on top, non-installed (blank icon) goes on bottom
                x.Installed && !y.Installed ? -1 : 1;

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMMisCountComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMArchiveComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            string xArchive = x.DisplayArchive;
            string yArchive = y.DisplayArchive;

            int ret =
                xArchive == yArchive ? TitleCompare(x, y) :
                    string.Compare(xArchive, yArchive, StringComparison.InvariantCultureIgnoreCase);

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMAuthorComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.Author == y.Author ? TitleCompare(x, y) :
                x.Author.IsEmpty() ? -1 :
                y.Author.IsEmpty() ? 1 :
                string.Compare(x.Author, y.Author, StringComparison.InvariantCultureIgnoreCase);

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMSizeComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMRatingComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMFinishedComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMReleaseDateComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMLastPlayedComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMDateAddedComparer : ColumnComparer, IDirectionalSortFMComparer
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

    internal sealed class FMDisabledModsComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.DisableAllMods && !y.DisableAllMods ? -1 :
                !x.DisableAllMods && y.DisableAllMods ? 1 :
                (x.DisableAllMods && y.DisableAllMods) || x.DisabledMods == y.DisabledMods ? TitleCompare(x, y) :
                // Sort this column content-first for better UX
                x.DisabledMods.IsEmpty() ? 1 :
                y.DisabledMods.IsEmpty() ? -1 :
                string.Compare(x.DisabledMods, y.DisabledMods, StringComparison.InvariantCultureIgnoreCase);

            return SortDirection == SortDirection.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMCommentComparer : ColumnComparer, IDirectionalSortFMComparer
    {
        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x.CommentSingleLine == y.CommentSingleLine ? TitleCompare(x, y) :
                // Sort this column content-first for better UX
                x.CommentSingleLine.IsEmpty() ? 1 :
                y.CommentSingleLine.IsEmpty() ? -1 :
                string.Compare(x.CommentSingleLine, y.CommentSingleLine, StringComparison.InvariantCultureIgnoreCase);

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

    #endregion
}

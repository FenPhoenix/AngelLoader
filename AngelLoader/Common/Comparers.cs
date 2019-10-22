using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.GameSupport;
using static AngelLoader.Common.SortStatic;

namespace AngelLoader.Common
{
    #region FM list sorting

    internal static class SortStatic
    {
        private static int TitleOrFallback(string t1, string t2, FanMission fm1, FanMission fm2, bool compareTitles = true)
        {
            int ret;
            if (compareTitles)
            {
                ret = string.Compare(t1, t2, StringComparison.InvariantCultureIgnoreCase);
                if (ret != 0) return ret;
            }

            ret = string.Compare(fm1.Archive, fm2.Archive, StringComparison.InvariantCultureIgnoreCase);
            if (ret != 0) return ret;

            return string.Compare(fm1.InstalledDir, fm2.InstalledDir, StringComparison.InvariantCultureIgnoreCase);
        }

        // Static for perf - this gets called from most comparer classes and we don't want to be instantiating
        // new title-sort classes in a loop!
        internal static int TitleCompare(FanMission x, FanMission y)
        {
            // Important: these get modified, so don't use the originals!
            var xTitle = x.Title;
            var yTitle = y.Title;

            // null for perf: don't create a new List<string> just to signify empty
            var articles = Config.EnableArticles ? Config.Articles : null;

            if (xTitle == yTitle) return TitleOrFallback(xTitle, yTitle, x, y, compareTitles: false);
            if (xTitle.IsEmpty()) return -1;
            if (yTitle.IsEmpty()) return 1;

            int xTitleLen = xTitle.Length;
            int yTitleLen = yTitle.Length;

            if (articles == null || articles.Count == 0) return TitleOrFallback(xTitle, yTitle, x, y);

            bool xArticleSet = false;
            bool yArticleSet = false;

            foreach (var a in articles)
            {
                int aLen = a.Length;

                // Avoid concats for perf
                if (!xArticleSet && xTitle.StartsWithI(a) && xTitleLen > aLen && (xTitle[aLen] == ' ' || xTitle[aLen] == '\t'))
                {
                    xTitle = xTitle.Substring(aLen + 1);
                    xArticleSet = true;
                }
                if (!yArticleSet && yTitle.StartsWithI(a) && yTitleLen > aLen && (yTitle[aLen] == ' ' || yTitle[aLen] == '\t'))
                {
                    yTitle = yTitle.Substring(aLen + 1);
                    yArticleSet = true;
                }
            }

            return TitleOrFallback(xTitle, yTitle, x, y);
        }
    }

    #region Column comparers

    // These are separated out for performance reasons, so we don't have to use LINQ OrderBy() etc.

    internal sealed class FMTitleComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMTitleComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret = x == null || y == null ? 0 : TitleCompare(x, y);
            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMGameComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMGameComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x == null || y == null ? 0 :
                x.Game == y.Game ? TitleCompare(x, y) :
                x.Game == Game.Null ? -1 :
                y.Game == Game.Null ? 1 :
                x.Game < y.Game ? -1 : 1;

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMInstalledComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMInstalledComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x == null || y == null ? 0 :
                x.Installed == y.Installed ? TitleCompare(x, y) :
                // Installed goes on top, non-installed (blank icon) goes on bottom
                x.Installed && !y.Installed ? -1 : 1;

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMArchiveComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMArchiveComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x == null || y == null ? 0 :
                x.Archive.IsEmpty() ? -1 :
                y.Archive.IsEmpty() ? 1 :
                string.Compare(x.Archive, y.Archive, StringComparison.InvariantCultureIgnoreCase);

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMAuthorComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMAuthorComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x == null || y == null ? 0 :
                x.Author == y.Author ? TitleCompare(x, y) :
                x.Author.IsEmpty() ? -1 :
                y.Author.IsEmpty() ? 1 :
                string.Compare(x.Author, y.Author, StringComparison.InvariantCultureIgnoreCase);

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMSizeComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMSizeComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x == null || y == null ? 0 :
                x.SizeBytes == y.SizeBytes ? TitleCompare(x, y) :
                x.SizeBytes == 0 ? -1 :
                y.SizeBytes == 0 ? 1 :
                x.SizeBytes < y.SizeBytes ? -1 : 1;

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMRatingComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMRatingComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            // Working
            if (false)
            {
                int one = _sortOrder == SortOrder.Ascending ? 1 : -1;

                ret =
                    x == null || y == null ? 0 :
                    x.Rating == y.Rating ? TitleCompare(x, y) :
                    x.Rating == -1 && y.Rating > -1 ? one :
                    x.Rating > -1 && y.Rating == -1 ? -one :
                    x.Rating > -1 && y.Rating > -1 && x.Rating < y.Rating ? -1 : 1;
            }
            else
            {
                ret =
                    x == null || y == null ? 0 :
                    x.Rating == y.Rating ? TitleCompare(x, y) :
                    x.Rating == 0 ? -1 :
                    y.Rating == 0 ? 1 :
                    x.Rating < y.Rating ? -1 : 1;
            }

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMFinishedComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMFinishedComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret;
            // Working: will add new option for this when done
            if (false)
            {
                ret =
                    x == null || y == null ? 0 :
                    (x.FinishedOnUnknown && y.FinishedOnUnknown) || x.FinishedOn == y.FinishedOn
                        ? TitleCompare(x, y) :
                    !x.FinishedOnUnknown && y.FinishedOnUnknown ? -1 :
                    x.FinishedOnUnknown && !y.FinishedOnUnknown ? 1 :
                    x.FinishedOn == 0 && y.FinishedOn > 0 ? 1 :
                    x.FinishedOn > 0 && y.FinishedOn == 0 ? -1 :
                    x.FinishedOn > 0 && y.FinishedOn > 0 && x.FinishedOn < y.FinishedOn ? -1 : 1;
            }
            else
            {
                ret =
                    x == null || y == null ? 0 :
                    !x.FinishedOnUnknown && y.FinishedOnUnknown ? -1 :
                    x.FinishedOnUnknown && !y.FinishedOnUnknown ? 1 :
                    (x.FinishedOnUnknown && y.FinishedOnUnknown) || x.FinishedOn == y.FinishedOn
                        ? TitleCompare(x, y) :
                    x.FinishedOn < y.FinishedOn ? -1 : 1;
            }


            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMReleaseDateComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMReleaseDateComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            // Sort this one down to the day only, because the exact time may very well not be known, and
            // even if it is, it's not visible or editable anywhere and it'd be weird to have missions
            // sorted out of name order because of an invisible time difference.
            int ret;
            if (x == null || y == null)
            {
                ret = 0;
            }
            else if (x.ReleaseDate == null && y.ReleaseDate == null)
            {
                ret = TitleCompare(x, y);
            }
            else if (x.ReleaseDate == null)
            {
                ret = -1;
            }
            else if (y.ReleaseDate == null)
            {
                ret = 1;
            }
            else
            {
                int cmp = ((DateTime)x.ReleaseDate).Date.CompareTo(((DateTime)y.ReleaseDate).Date);
                ret = cmp == 0 ? TitleCompare(x, y) : cmp;
            }

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMLastPlayedComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMLastPlayedComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            // Sort this one by exact DateTime because the time is (indirectly) changeable down to the
            // second (you change it by playing it), and the user will expect precise sorting.
            int ret;
            if (x == null || y == null)
            {
                ret = 0;
            }
            else if (x.LastPlayed == null && y.LastPlayed == null)
            {
                ret = TitleCompare(x, y);
            }
            else if (x.LastPlayed == null)
            {
                ret = -1;
            }
            else if (y.LastPlayed == null)
            {
                ret = 1;
            }
            else
            {
                int cmp = ((DateTime)x.LastPlayed).CompareTo(((DateTime)y.LastPlayed));
                ret = cmp == 0 ? TitleCompare(x, y) : cmp;
            }

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMDisabledModsComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMDisabledModsComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x == null || y == null ? 0 :
                x.DisableAllMods && !y.DisableAllMods ? -1 :
                !x.DisableAllMods && y.DisableAllMods ? 1 :
                (x.DisableAllMods && y.DisableAllMods) || x.DisabledMods == y.DisabledMods ? TitleCompare(x, y) :
                // Sort this column content-first for better UX
                x.DisabledMods.IsEmpty() ? 1 :
                y.DisabledMods.IsEmpty() ? -1 :
                string.Compare(x.DisabledMods, y.DisabledMods, StringComparison.InvariantCultureIgnoreCase);

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    internal sealed class FMCommentComparer : IComparer<FanMission>
    {
        private readonly SortOrder _sortOrder;

        public FMCommentComparer(SortOrder sortOrder) => _sortOrder = sortOrder;

        public int Compare(FanMission x, FanMission y)
        {
            int ret =
                x == null || y == null ? 0 :
                x.CommentSingleLine == y.CommentSingleLine ? TitleCompare(x, y) :
                // Sort this column content-first for better UX
                x.CommentSingleLine.IsEmpty() ? 1 :
                y.CommentSingleLine.IsEmpty() ? -1 :
                string.Compare(x.CommentSingleLine, y.CommentSingleLine, StringComparison.InvariantCultureIgnoreCase);

            return _sortOrder == SortOrder.Ascending ? ret : -ret;
        }
    }

    #endregion

    #endregion

    internal sealed class FileNameNoExtComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x.IsEmpty()) return -1;
            if (y.IsEmpty()) return 1;

            return string.Compare(Path.GetFileNameWithoutExtension(x), Path.GetFileNameWithoutExtension(y),
                StringComparison.OrdinalIgnoreCase);
        }
    }

    #region Category and tags

    internal sealed class CategoryComparer : IComparer<CatAndTags>
    {
        public int Compare(CatAndTags x, CatAndTags y)
        {
            Debug.Assert(x != null, "CategoryComparer: x is null");
            Debug.Assert(y != null, "CategoryComparer: y is null");

            Debug.Assert(!x.Category.IsEmpty(), "CategoryComparer: x.Name is null or empty");
            Debug.Assert(!y.Category.IsEmpty(), "CategoryComparer: y.Name is null or empty");

            Debug.Assert(x.Category.ToLowerInvariant() == x.Category, "CategoryComparer: x.Name is not lowercase");
            Debug.Assert(y.Category.ToLowerInvariant() == y.Category, "CategoryComparer: y.Name is not lowercase");

            x.Tags.Sort(StringComparer.OrdinalIgnoreCase);
            y.Tags.Sort(StringComparer.OrdinalIgnoreCase);

            // Category names are supposed to always be lowercase, so don't waste time checking
            return string.Compare(x.Category, y.Category, StringComparison.Ordinal);

        }
    }

    internal sealed class CategoryComparerGlobal : IComparer<GlobalCatAndTags>
    {
        public int Compare(GlobalCatAndTags x, GlobalCatAndTags y)
        {
            Debug.Assert(x != null, "CategoryComparer: x is null");
            Debug.Assert(y != null, "CategoryComparer: y is null");

            Debug.Assert(!x.Category.Name.IsEmpty(), "CategoryComparer: x.Name is null or empty");
            Debug.Assert(!y.Category.Name.IsEmpty(), "CategoryComparer: y.Name is null or empty");

            Debug.Assert(x.Category.Name.ToLowerInvariant() == x.Category.Name, "CategoryComparer: x.Name is not lowercase");
            Debug.Assert(y.Category.Name.ToLowerInvariant() == y.Category.Name, "CategoryComparer: y.Name is not lowercase");

            x.Tags.Sort(new CatItemComparer());
            y.Tags.Sort(new CatItemComparer());

            // Category names are supposed to always be lowercase, so don't waste time checking
            return string.Compare(x.Category.Name, y.Category.Name, StringComparison.Ordinal);

        }
    }

    internal sealed class CatItemComparer : IComparer<GlobalCatOrTag>
    {
        public int Compare(GlobalCatOrTag x, GlobalCatOrTag y)
        {
            Debug.Assert(x != null, "CatItemComparer: x is null");
            Debug.Assert(y != null, "CatItemComparer: y is null");

            return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }
    }

    #endregion
}

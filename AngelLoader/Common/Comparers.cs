using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader
{
    [PublicAPI]
    internal static class Comparers
    {
        #region Fields

        #region FM column comparers

        private static FMTitleComparer? _fmTitleComparer;
        internal static IDirectionalSortFMComparer FMTitle => _fmTitleComparer ??= new FMTitleComparer();

        private static FMGameComparer? _fmGameComparer;
        internal static IDirectionalSortFMComparer FMGame => _fmGameComparer ??= new FMGameComparer();

        private static FMInstalledComparer? _fmInstalledComparer;
        internal static IDirectionalSortFMComparer FMInstalled => _fmInstalledComparer ??= new FMInstalledComparer();

        private static FMArchiveComparer? _fmArchiveComparer;
        internal static IDirectionalSortFMComparer FMArchive => _fmArchiveComparer ??= new FMArchiveComparer();

        private static FMAuthorComparer? _fmAuthorComparer;
        internal static IDirectionalSortFMComparer FMAuthor => _fmAuthorComparer ??= new FMAuthorComparer();

        private static FMSizeComparer? _fmSizeComparer;
        internal static IDirectionalSortFMComparer FMSize => _fmSizeComparer ??= new FMSizeComparer();

        private static FMRatingComparer? _fmRatingComparer;
        internal static IDirectionalSortFMComparer FMRating => _fmRatingComparer ??= new FMRatingComparer();

        private static FMFinishedComparer? _fmFinishedComparer;
        internal static IDirectionalSortFMComparer FMFinished => _fmFinishedComparer ??= new FMFinishedComparer();

        private static FMReleaseDateComparer? _fmReleaseDateComparer;
        internal static IDirectionalSortFMComparer FMReleaseDate => _fmReleaseDateComparer ??= new FMReleaseDateComparer();

        private static FMLastPlayedComparer? _fmLastPlayedComparer;
        internal static IDirectionalSortFMComparer FMLastPlayed => _fmLastPlayedComparer ??= new FMLastPlayedComparer();

        private static FMDateAddedComparer? _fmDateAddedComparer;
        internal static IDirectionalSortFMComparer FMDateAdded => _fmDateAddedComparer ??= new FMDateAddedComparer();

        private static FMDisabledModsComparer? _fmDisabledModsComparer;
        internal static IDirectionalSortFMComparer FMDisabledMods => _fmDisabledModsComparer ??= new FMDisabledModsComparer();

        private static FMCommentComparer? _fmCommentComparer;
        internal static IDirectionalSortFMComparer FMComment => _fmCommentComparer ??= new FMCommentComparer();

        #endregion

        #region Category comparers

        private static CategoryComparer? _categoryComparer;
        internal static CategoryComparer Category => _categoryComparer ??= new CategoryComparer();

        private static CategoryComparerGlobal? _categoryComparerGlobal;
        internal static CategoryComparerGlobal CategoryGlobal => _categoryComparerGlobal ??= new CategoryComparerGlobal();

        private static CatItemComparer? _catItemComparer;
        internal static CatItemComparer CatItem => _catItemComparer ??= new CatItemComparer();

        #endregion

        #region Misc comparers

        private static FileNameNoExtComparer? _fileNameNoExtComparer;
        internal static FileNameNoExtComparer FileNameNoExt => _fileNameNoExtComparer ??= new FileNameNoExtComparer();

        #endregion

        #endregion

        #region Classes

        internal interface IDirectionalSortFMComparer : IComparer<FanMission>
        {
            SortOrder SortOrder { set; }
        }

        #region FM list sorting

        #region Static methods

        // Static for perf - this gets called from most comparer classes and we don't want to be instantiating
        // new title-sort classes in a loop!
        private static int TitleCompare(FanMission x, FanMission y)
        {
            static int TitleOrFallback(string title1, string title2, FanMission fm1, FanMission fm2, bool compareTitles = true)
            {
                int ret;
                if (compareTitles)
                {
                    ret = string.Compare(title1, title2, StringComparison.InvariantCultureIgnoreCase);
                    if (ret != 0) return ret;
                }

                ret = string.Compare(fm1.Archive, fm2.Archive, StringComparison.InvariantCultureIgnoreCase);
                if (ret != 0) return ret;

                return string.Compare(fm1.InstalledDir, fm2.InstalledDir, StringComparison.InvariantCultureIgnoreCase);
            }

            // IMPORTANT: these get modified, so don't use the originals!
            string xTitle = x.Title;
            string yTitle = y.Title;

            // null for perf: don't create a new List<string> just to signify empty
            var articles = Config.EnableArticles ? Config.Articles : null;

            if (xTitle == yTitle) return TitleOrFallback(xTitle, yTitle, x, y, compareTitles: false);
            if (xTitle.IsEmpty()) return -1;
            if (yTitle.IsEmpty()) return 1;

            if (articles?.Count > 0)
            {
                bool xArticleSet = false;
                bool yArticleSet = false;

                foreach (string a in articles)
                {
                    int aLen = a.Length;

                    // Avoid concats for perf
                    if (!xArticleSet && xTitle.StartsWithIPlusWhiteSpace(a, aLen))
                    {
                        xTitle = xTitle.Substring(aLen + 1);
                        xArticleSet = true;
                    }
                    if (!yArticleSet && yTitle.StartsWithIPlusWhiteSpace(a, aLen))
                    {
                        yTitle = yTitle.Substring(aLen + 1);
                        yArticleSet = true;
                    }
                }
            }

            return TitleOrFallback(xTitle, yTitle, x, y);
        }

        #endregion

        #region Column comparers

        // These are separated out for performance reasons, so we don't have to use LINQ OrderBy() etc.

        internal sealed class FMTitleComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret = TitleCompare(x, y);
                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMGameComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret =
                    x.Game == y.Game ? TitleCompare(x, y) :
                    x.Game == Game.Null ? -1 :
                    y.Game == Game.Null ? 1 :
                    x.Game < y.Game ? -1 : 1;

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMInstalledComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret =
                    x.Installed == y.Installed ? TitleCompare(x, y) :
                    // Installed goes on top, non-installed (blank icon) goes on bottom
                    x.Installed && !y.Installed ? -1 : 1;

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMArchiveComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret =
                    x.Archive.IsEmpty() ? -1 :
                    y.Archive.IsEmpty() ? 1 :
                    string.Compare(x.Archive, y.Archive, StringComparison.InvariantCultureIgnoreCase);

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMAuthorComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret =
                    x.Author == y.Author ? TitleCompare(x, y) :
                    x.Author.IsEmpty() ? -1 :
                    y.Author.IsEmpty() ? 1 :
                    string.Compare(x.Author, y.Author, StringComparison.InvariantCultureIgnoreCase);

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMSizeComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret =
                    x.SizeBytes == y.SizeBytes ? TitleCompare(x, y) :
                    x.SizeBytes == 0 ? -1 :
                    y.SizeBytes == 0 ? 1 :
                    x.SizeBytes < y.SizeBytes ? -1 : 1;

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMRatingComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret;
                // Working
#if false
            if (false)
            {
                int one = _sortOrder == SortOrder.Ascending ? 1 : -1;

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
                        x.Rating == 0 ? -1 :
                        y.Rating == 0 ? 1 :
                        x.Rating < y.Rating ? -1 : 1;
                }

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMFinishedComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

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

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMReleaseDateComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

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

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMLastPlayedComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

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

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMDateAddedComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

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

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMDisabledModsComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

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

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        internal sealed class FMCommentComparer : IDirectionalSortFMComparer
        {
            public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

            public int Compare(FanMission x, FanMission y)
            {
                int ret =
                    x.CommentSingleLine == y.CommentSingleLine ? TitleCompare(x, y) :
                    // Sort this column content-first for better UX
                    x.CommentSingleLine.IsEmpty() ? 1 :
                    y.CommentSingleLine.IsEmpty() ? -1 :
                    string.Compare(x.CommentSingleLine, y.CommentSingleLine, StringComparison.InvariantCultureIgnoreCase);

                return SortOrder == SortOrder.Ascending ? ret : -ret;
            }
        }

        #endregion

        #endregion

        #region Category and tags

        internal sealed class CategoryComparer : IComparer<CatAndTags>
        {
            public int Compare(CatAndTags x, CatAndTags y)
            {
                AssertR(!x.Category.IsEmpty(), "CategoryComparer: x.Name is null or empty");
                AssertR(!y.Category.IsEmpty(), "CategoryComparer: y.Name is null or empty");

                AssertR(x.Category.ToLowerInvariant() == x.Category, "CategoryComparer: x.Name is not lowercase");
                AssertR(y.Category.ToLowerInvariant() == y.Category, "CategoryComparer: y.Name is not lowercase");

                // Category names are supposed to always be lowercase, so don't waste time checking
                return string.CompareOrdinal(x.Category, y.Category);
            }
        }

        internal sealed class CategoryComparerGlobal : IComparer<GlobalCatAndTags>
        {
            public int Compare(GlobalCatAndTags x, GlobalCatAndTags y)
            {
                AssertR(!x.Category.Name.IsEmpty(), "CategoryComparer: x.Name is null or empty");
                AssertR(!y.Category.Name.IsEmpty(), "CategoryComparer: y.Name is null or empty");

                AssertR(x.Category.Name.ToLowerInvariant() == x.Category.Name, "CategoryComparer: x.Name is not lowercase");
                AssertR(y.Category.Name.ToLowerInvariant() == y.Category.Name, "CategoryComparer: y.Name is not lowercase");

                // Category names are supposed to always be lowercase, so don't waste time checking
                return string.CompareOrdinal(x.Category.Name, y.Category.Name);
            }
        }

        internal sealed class CatItemComparer : IComparer<GlobalCatOrTag>
        {
            public int Compare(GlobalCatOrTag x, GlobalCatOrTag y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        internal sealed class FileNameNoExtComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x.IsEmpty()) return -1;
                if (y.IsEmpty()) return 1;

                return string.Compare(
                    Path.GetFileNameWithoutExtension(x),
                    Path.GetFileNameWithoutExtension(y),
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;

namespace AngelLoader.Common
{
    internal sealed class FMArchiveNameComparer : IEqualityComparer<FanMission>
    {
        public bool Equals(FanMission x, FanMission y)
        {
            Trace.Assert(x != null && y != null, "x != null && y != null");

            return x.Archive.EqualsI(y.Archive);
        }

        public int GetHashCode(FanMission obj)
        {
            return obj.Archive.GetHashCode();
        }
    }

    internal sealed class FMInstalledNameComparer : IEqualityComparer<FanMission>
    {
        public bool Equals(FanMission x, FanMission y)
        {
            Trace.Assert(x != null && y != null, "x != null && y != null");

            return /*!string.IsNullOrEmpty(x.ArchiveName) &&*/
                !x.InstalledDir.IsEmpty() &&
                !y.InstalledDir.IsEmpty() &&
                x.InstalledDir.EqualsI(y.InstalledDir);
        }

        public int GetHashCode(FanMission obj)
        {
            return obj.InstalledDir.GetHashCode();
        }
    }

    internal sealed class FMTitleComparer : IComparer<string>
    {
        private readonly List<string> Articles;

        public FMTitleComparer(List<string> articles) => Articles = articles;

        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x.IsEmpty()) return -1;
            if (y.IsEmpty()) return 1;

            if (Articles == null || Articles.Count == 0)
            {
                return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
            }

            bool xArticleSet = false;
            bool yArticleSet = false;

            foreach (var a in Articles)
            {
                if (!xArticleSet && x.StartsWithI(a + " "))
                {
                    x = x.Substring(a.Length + 1);
                    xArticleSet = true;
                }
                if (!yArticleSet && y.StartsWithI(a + " "))
                {
                    y = y.Substring(a.Length + 1);
                    yArticleSet = true;
                }
            }

            return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
        }
    }

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
}

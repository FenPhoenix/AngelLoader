using System;
using System.Collections.Generic;

namespace AngelLoader.DataClasses
{
    internal sealed class CatAndTags
    {
        internal string Category = "";
        internal readonly List<string> Tags = new List<string>();
    }

    internal sealed class GlobalCatAndTags
    {
        internal GlobalCatOrTag Category = new GlobalCatOrTag();
        internal List<GlobalCatOrTag> Tags = new List<GlobalCatOrTag>();
    }

    internal class CatAndTagsList : List<CatAndTags>
    {
        internal void DeepCopyTo(CatAndTagsList dest)
        {
            dest.Clear();

            if (Count == 0) return;

            for (int i = 0; i < Count; i++)
            {
                var item = new CatAndTags { Category = this[i].Category };
                for (int j = 0; j < this[i].Tags.Count; j++) item.Tags.Add(this[i].Tags[j]);
                dest.Add(item);
            }
        }

        internal void SortAndMoveMiscToEnd()
        {
            if (Count == 0) return;

            Sort(Comparers.CategoryComparer);
            foreach (var item in this) item.Tags.Sort(StringComparer.OrdinalIgnoreCase);

            if (this[Count - 1].Category == "misc") return;

            for (int i = 0; i < Count; i++)
            {
                CatAndTags item = this[i];
                if (this[i].Category == "misc")
                {
                    Remove(item);
                    Add(item);
                    return;
                }
            }
        }
    }

    internal class GlobalCatAndTagsList : List<GlobalCatAndTags>
    {
        public GlobalCatAndTagsList(int capacity) : base(capacity) { }

        internal void DeepCopyTo(GlobalCatAndTagsList dest)
        {
            dest.Clear();

            if (Count == 0) return;

            for (int i = 0; i < Count; i++)
            {
                var item = new GlobalCatAndTags
                {
                    Category = new GlobalCatOrTag
                    {
                        Name = this[i].Category.Name,
                        IsPreset = this[i].Category.IsPreset,
                        UsedCount = this[i].Category.UsedCount
                    }
                };
                for (int j = 0; j < this[i].Tags.Count; j++)
                {
                    item.Tags.Add(new GlobalCatOrTag
                    {
                        Name = this[i].Tags[j].Name,
                        IsPreset = this[i].Tags[j].IsPreset,
                        UsedCount = this[i].Tags[j].UsedCount
                    });
                }

                dest.Add(item);
            }
        }

        internal void SortAndMoveMiscToEnd()
        {
            if (Count == 0) return;

            Sort(Comparers.CategoryComparerGlobal);
            foreach (var item in this) item.Tags.Sort(Comparers.CatItemComparer);

            if (this[Count - 1].Category.Name == "misc") return;

            for (int i = 0; i < Count; i++)
            {
                GlobalCatAndTags item = this[i];
                if (this[i].Category.Name == "misc")
                {
                    Remove(item);
                    Add(item);
                    return;
                }
            }
        }
    }

    internal sealed class GlobalCatOrTag
    {
        internal string Name = "";

        /// <summary>
        /// If true, the tag will never be removed from the global list even if no FMs are using it.
        /// </summary>
        internal bool IsPreset;

        /// <summary>
        /// Keeps track of the number of FMs that are using this tag. If a tag is removed from an FM and its
        /// <see cref="UsedCount"/> in the global list is greater than 0, then its <see cref="UsedCount"/> will
        /// be decremented by one and it will not be removed from the global list. This is for performance: it's
        /// much faster to simply keep track of what needs removing than to rebuild the list every time a tag is
        /// removed.
        /// </summary>
        internal int UsedCount;
    }
}

using System;
using System.Collections.Generic;

namespace AngelLoader.DataClasses
{
    internal sealed class CatAndTags
    {
        internal string Category = "";
        internal readonly List<string> Tags = new List<string>();
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

    internal sealed class GlobalCatAndTags
    {
        internal GlobalCatOrTag Category = new GlobalCatOrTag();
        internal List<GlobalCatOrTag> Tags = new List<GlobalCatOrTag>();
    }

    internal sealed class CatAndTagsList : List<CatAndTags>
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

            Sort(Comparers.Category);
            foreach (CatAndTags item in this) item.Tags.Sort(StringComparer.OrdinalIgnoreCase);

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

    // We lock the preset tags in a private array inside a static class whose only public method is a deep-copier.
    // That way we have a strong guarantee of immutability of the original set. These things will not be messed
    // with, ever.
    internal static class PresetTags
    {
        #region Preset tags array

        // These are the FMSel preset tags. Conforming to standards here.
        private static readonly GlobalCatAndTags[] _presetTags =
        {
            new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = "author", IsPreset = true } },
            new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = "contest", IsPreset = true } },
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag { Name = "genre", IsPreset = true },
                Tags = new List<GlobalCatOrTag>(5)
                {
                    new GlobalCatOrTag { Name = "action", IsPreset = true },
                    new GlobalCatOrTag { Name = "crime", IsPreset = true },
                    new GlobalCatOrTag { Name = "horror", IsPreset = true },
                    new GlobalCatOrTag { Name = "mystery", IsPreset = true },
                    new GlobalCatOrTag { Name = "puzzle", IsPreset = true }
                }
            },
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag { Name = "language", IsPreset = true },
                Tags = new List<GlobalCatOrTag>(11)
                {
                    new GlobalCatOrTag { Name = "English", IsPreset = true },
                    new GlobalCatOrTag { Name = "Czech", IsPreset = true },
                    new GlobalCatOrTag { Name = "Dutch", IsPreset = true },
                    new GlobalCatOrTag { Name = "French", IsPreset = true },
                    new GlobalCatOrTag { Name = "German", IsPreset = true },
                    new GlobalCatOrTag { Name = "Hungarian", IsPreset = true },
                    new GlobalCatOrTag { Name = "Italian", IsPreset = true },
                    new GlobalCatOrTag { Name = "Japanese", IsPreset = true },
                    new GlobalCatOrTag { Name = "Polish", IsPreset = true },
                    new GlobalCatOrTag { Name = "Russian", IsPreset = true },
                    new GlobalCatOrTag { Name = "Spanish", IsPreset = true }
                }
            },
            new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = "series", IsPreset = true } },
            new GlobalCatAndTags
            {
                Category = new GlobalCatOrTag { Name = "misc", IsPreset = true },
                Tags = new List<GlobalCatOrTag>(6)
                {
                    new GlobalCatOrTag { Name = "campaign", IsPreset = true },
                    new GlobalCatOrTag { Name = "demo", IsPreset = true },
                    new GlobalCatOrTag { Name = "long", IsPreset = true },
                    new GlobalCatOrTag { Name = "other protagonist", IsPreset = true },
                    new GlobalCatOrTag { Name = "short", IsPreset = true },
                    new GlobalCatOrTag { Name = "unknown author", IsPreset = true }
                }
            }
        };

        #endregion

        internal static readonly int Count = _presetTags.Length;

        /// <summary>
        /// Deep-copies the set of preset tags to a <see cref="GlobalCatAndTagsList"/>.
        /// </summary>
        /// <param name="dest">The <see cref="GlobalCatAndTagsList"/> to copy the preset tags to.</param>
        internal static void DeepCopyTo(GlobalCatAndTagsList dest)
        {
            dest.Clear();

            for (int i = 0; i < Count; i++)
            {
                var item = new GlobalCatAndTags
                {
                    Category = new GlobalCatOrTag
                    {
                        Name = _presetTags[i].Category.Name,
                        IsPreset = _presetTags[i].Category.IsPreset,
                        UsedCount = _presetTags[i].Category.UsedCount
                    }
                };
                for (int j = 0; j < _presetTags[i].Tags.Count; j++)
                {
                    item.Tags.Add(new GlobalCatOrTag
                    {
                        Name = _presetTags[i].Tags[j].Name,
                        IsPreset = _presetTags[i].Tags[j].IsPreset,
                        UsedCount = _presetTags[i].Tags[j].UsedCount
                    });
                }

                dest.Add(item);
            }
        }
    }

    internal sealed class GlobalCatAndTagsList : List<GlobalCatAndTags>
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

            Sort(Comparers.CategoryGlobal);
            foreach (GlobalCatAndTags item in this) item.Tags.Sort(Comparers.CatItem);

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
}

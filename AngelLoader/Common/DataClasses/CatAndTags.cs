using System;
using System.Collections.Generic;

namespace AngelLoader.DataClasses
{
    internal sealed class CatAndTags
    {
        internal readonly string Category;
        internal readonly List<string> Tags;

        internal CatAndTags(string category)
        {
            Category = category;
            Tags = new List<string>();
        }

        internal CatAndTags(string category, int tagsCapacity)
        {
            Category = category;
            Tags = new List<string>(tagsCapacity);
        }
    }

    internal sealed class CatAndTagsList : List<CatAndTags>
    {
        internal CatAndTagsList() { }

        internal CatAndTagsList(int capacity) : base(capacity) { }

        internal void DeepCopyTo(CatAndTagsList dest)
        {
            dest.Clear();

            if (Count == 0) return;

            for (int i = 0; i < Count; i++)
            {
                CatAndTags thisI = this[i];
                var item = new CatAndTags(thisI.Category, thisI.Tags.Count);
                for (int j = 0; j < thisI.Tags.Count; j++) item.Tags.Add(thisI.Tags[j]);
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
        private static readonly KeyValuePair<string, string[]>[]
        _presetTags =
        {
            new("author", Array.Empty<string>()),
            new("contest", Array.Empty<string>()),
            new("genre", new[]
            {
                "action",
                "crime",
                "horror",
                "mystery",
                "puzzle"
            }),
            new("language", new[]
            {
                "English",
                "Czech",
                "Dutch",
                "French",
                "German",
                "Hungarian",
                "Italian",
                "Japanese",
                "Polish",
                "Russian",
                "Spanish"
            }),
            new("series", Array.Empty<string>()),
            new("misc", new[]
            {
                "campaign",
                "demo",
                "long",
                "other protagonist",
                "short",
                "unknown author"
            })
        };

        #endregion

        internal static readonly int Count = _presetTags.Length;

        /// <summary>
        /// Deep-copies the set of preset tags to a <see cref="CatAndTagsList"/>.
        /// </summary>
        /// <param name="dest">The <see cref="CatAndTagsList"/> to copy the preset tags to.</param>
        internal static void DeepCopyTo(CatAndTagsList dest)
        {
            dest.Clear();

            for (int i = 0; i < Count; i++)
            {
                var pt = _presetTags[i];
                var item = new CatAndTags(pt.Key, pt.Value.Length);
                for (int j = 0; j < pt.Value.Length; j++)
                {
                    item.Tags.Add(pt.Value[j]);
                }

                dest.Add(item);
            }
        }
    }
}

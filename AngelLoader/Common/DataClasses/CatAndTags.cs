using System;
using System.Collections.Generic;

namespace AngelLoader.DataClasses
{
    internal sealed class CatAndTags
    {
        internal string Category = "";
        internal readonly List<string> Tags;

        internal CatAndTags() => Tags = new List<string>();
        internal CatAndTags(int tagsCapacity) => Tags = new List<string>(tagsCapacity);
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
                var item = new CatAndTags(thisI.Tags.Count) { Category = thisI.Category };
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
            new KeyValuePair<string, string[]>("author", Array.Empty<string>()),
            new KeyValuePair<string, string[]>("contest", Array.Empty<string>()),
            new KeyValuePair<string, string[]>("genre", new[]
            {
                "action",
                "crime",
                "horror",
                "mystery",
                "puzzle"
            }),
            new KeyValuePair<string, string[]>("language", new[]
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
            new KeyValuePair<string, string[]>("series", Array.Empty<string>()),
            new KeyValuePair<string, string[]>("misc", new[]
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
                var item = new CatAndTags(pt.Value.Length) { Category = pt.Key };
                for (int j = 0; j < pt.Value.Length; j++)
                {
                    item.Tags.Add(pt.Value[j]);
                }

                dest.Add(item);
            }
        }
    }
}

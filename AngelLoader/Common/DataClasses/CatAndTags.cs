using System;
using System.Collections.Generic;

namespace AngelLoader.DataClasses
{
    public sealed class FMTagsCollection : HashSet<string>
    {
        public readonly List<string> List;

        public FMTagsCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
            List = new List<string>();
        }

        public FMTagsCollection(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
        {
            List = new List<string>(capacity);
        }

        public new void Add(string tag)
        {
            if (base.Add(tag))
            {
                List.Add(tag);
            }
        }

        public new void Remove(string category)
        {
            base.Remove(category);
            List.Remove(category);
        }

        public void RemoveAt(int index)
        {
            string item = List[index];
            base.Remove(item);
        }

        public void SortCaseInsensitive() => List.Sort(StringComparer.OrdinalIgnoreCase);
    }

    public sealed class FMCategoriesCollection : Dictionary<string, FMTagsCollection>
    {
        public readonly List<string> List;

        public FMCategoriesCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
            List = new List<string>();
        }

        public FMCategoriesCollection(int capacity) : base(capacity, StringComparer.OrdinalIgnoreCase)
        {
            List = new List<string>(capacity);
        }

        public new void Add(string category, FMTagsCollection tags)
        {
            if (!base.ContainsKey(category))
            {
                base[category] = tags;
                List.Add(category);
            }
        }

        public new bool Remove(string category)
        {
            List.Remove(category);
            return base.Remove(category);
        }

        public bool RemoveAt(int index)
        {
            string item = List[index];
            return base.Remove(item);
        }

        public new void Clear()
        {
            base.Clear();
            List.Clear();
        }

        public void DeepCopyTo(FMCategoriesCollection dest)
        {
            dest.Clear();
            for (int i = 0; i < List.Count; i++)
            {
                string category = List[i];
                FMTagsCollection srcTags = base[category];
                var destTags = new FMTagsCollection(srcTags.Count);
                for (int j = 0; j < srcTags.Count; j++)
                {
                    destTags.Add(srcTags.List[j]);
                }
                dest.Add(category, destTags);
            }
        }

        internal void SortAndMoveMiscToEnd()
        {
            if (List.Count == 0) return;

            List.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (var item in this)
            {
                item.Value.SortCaseInsensitive();
            }

            if (List[List.Count - 1] == "misc") return;

            for (int i = 0; i < List.Count; i++)
            {
                string item = List[i];
                if (List[i] == "misc")
                {
                    List.Remove(item);
                    List.Add(item);
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
        /// Deep-copies the set of preset tags to a <see cref="FMCategoriesCollection"/>.
        /// </summary>
        /// <param name="dest">The <see cref="FMCategoriesCollection"/> to copy the preset tags to.</param>
        internal static void DeepCopyTo(FMCategoriesCollection dest)
        {
            dest.Clear();

            for (int i = 0; i < Count; i++)
            {
                var pt = _presetTags[i];
                string category = pt.Key;
                var tags = new FMTagsCollection(pt.Value.Length);
                for (int j = 0; j < pt.Value.Length; j++)
                {
                    tags.Add(pt.Value[j]);
                }

                dest.Add(category, tags);
            }
        }
    }
}

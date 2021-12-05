using System;
using System.Collections;
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

    public readonly struct CatAndTagsList
    {
        public readonly string Category;
        public readonly FMTagsCollection Tags;

        public CatAndTagsList(string category, FMTagsCollection tags)
        {
            Category = category;
            Tags = tags;
        }
    }

    public sealed class FMCategoriesCollection : IEnumerable<CatAndTagsList>
    {
        private readonly Dictionary<string, FMTagsCollection> _dict;
        private readonly List<string> _list;

        public FMCategoriesCollection()
        {
            _list = new List<string>();
            _dict = new Dictionary<string, FMTagsCollection>(StringComparer.OrdinalIgnoreCase);
        }

        public FMCategoriesCollection(int capacity)
        {
            _list = new List<string>(capacity);
            _dict = new Dictionary<string, FMTagsCollection>(capacity, StringComparer.OrdinalIgnoreCase);
        }

        public int Count => _list.Count;

        public void Add(string category, FMTagsCollection tags)
        {
            if (!_dict.ContainsKey(category))
            {
                _dict[category] = tags;
                _list.Add(category);
            }
        }

        public bool TryGetValue(string key, out FMTagsCollection value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public bool Remove(string category)
        {
            _list.Remove(category);
            return _dict.Remove(category);
        }

        public bool RemoveAt(int index)
        {
            string item = _list[index];
            _list.RemoveAt(index);
            return _dict.Remove(item);
        }

        public void Clear()
        {
            _dict.Clear();
            _list.Clear();
        }

        public void DeepCopyTo(FMCategoriesCollection dest)
        {
            dest.Clear();
            for (int i = 0; i < _list.Count; i++)
            {
                string category = _list[i];
                FMTagsCollection srcTags = _dict[category];
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
            if (_list.Count == 0) return;

            _list.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (var item in _dict)
            {
                item.Value.SortCaseInsensitive();
            }

            if (_list[_list.Count - 1] == "misc") return;

            for (int i = 0; i < _list.Count; i++)
            {
                string item = _list[i];
                if (_list[i] == "misc")
                {
                    _list.Remove(item);
                    _list.Add(item);
                    return;
                }
            }
        }

        public CatAndTagsList this[int index] => new CatAndTagsList(_list[index], _dict[_list[index]]);

        public IEnumerator<CatAndTagsList> GetEnumerator()
        {
            foreach (string item in _list)
            {
                yield return new CatAndTagsList(item, _dict[item]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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

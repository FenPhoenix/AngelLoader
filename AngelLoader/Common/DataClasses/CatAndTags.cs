using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static AL_Common.Common;

namespace AngelLoader.DataClasses;

public sealed class FMTagsCollection : IEnumerable<string>
{
    private readonly List<string> _list;
    private readonly HashSetI _hashSet;

    public FMTagsCollection()
    {
        _list = new List<string>();
        _hashSet = new HashSetI();
    }

    public FMTagsCollection(int capacity)
    {
        _list = new List<string>(capacity);
        _hashSet = new HashSetI(capacity);
    }

    public int Count => _list.Count;

    public void Clear()
    {
        _hashSet.Clear();
        _list.Clear();
    }

    public bool Contains(string item) => _hashSet.Contains(item);

    public void Add(string tag)
    {
        if (_hashSet.Add(tag))
        {
            _list.Add(tag);
        }
    }

    public void Remove(string category)
    {
        _hashSet.Remove(category);
        _list.Remove(category);
    }

#if false
    public void RemoveAt(int index)
    {
        string item = _list[index];
        _list.RemoveAt(index);
        _hashSet.Remove(item);
    }
#endif

    public string this[int index] => _list[index];

    public void SortCaseInsensitive() => _list.Sort(StringComparer.OrdinalIgnoreCase);

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string item in _list)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
    private readonly DictionaryI<FMTagsCollection> _dict;
    private readonly List<string> _list;

    public FMCategoriesCollection()
    {
        _list = new List<string>();
        _dict = new DictionaryI<FMTagsCollection>();
    }

    public FMCategoriesCollection(int capacity)
    {
        _list = new List<string>(capacity);
        _dict = new DictionaryI<FMTagsCollection>(capacity);
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

    public bool TryGetValue(string key, [NotNullWhen(true)] out FMTagsCollection? value)
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

    public CatAndTagsList this[int index] => new(_list[index], _dict[_list[index]]);

#if false
    public bool RemoveAt(int index)
    {
        string item = _list[index];
        _list.RemoveAt(index);
        return _dict.Remove(item);
    }
#endif

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
                destTags.Add(srcTags[j]);
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

        if (_list[^1] == PresetTags.MiscCategory) return;

        for (int i = 0; i < _list.Count; i++)
        {
            string item = _list[i];
            if (_list[i] == PresetTags.MiscCategory)
            {
                _list.Remove(item);
                _list.Add(item);
                return;
            }
        }
    }

#if false
    public CatAndTagsList this[int index] => new CatAndTagsList(_list[index], _dict[_list[index]]);
#endif

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
    internal const string MiscCategory = "misc";

    #region Preset tags array

    private static readonly KeyValuePair<string, string[]>[]
    _fmSelPresetTags =
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
        new(MiscCategory, new[]
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

    internal static readonly int Count = _fmSelPresetTags.Length;

    /// <summary>
    /// Deep-copies the set of preset tags to a <see cref="FMCategoriesCollection"/>.
    /// </summary>
    /// <param name="dest">The <see cref="FMCategoriesCollection"/> to copy the preset tags to.</param>
    internal static void DeepCopyTo(FMCategoriesCollection dest)
    {
        dest.Clear();

        for (int i = 0; i < Count; i++)
        {
            var pt = _fmSelPresetTags[i];
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

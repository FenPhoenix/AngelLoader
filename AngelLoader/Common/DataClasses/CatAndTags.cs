using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AL_Common.Common;

namespace AngelLoader.DataClasses;

/*
@NET5/@MEM(CatAndTags "frugal object" pattern (https://prodotnetmemory.com/slides/PerformancePatternsLong/#79))
@NET5(CatAndTags): Port this back to AL Framework when we've made this as frugal as we can.
There's often zero or one tag in here, so it would reduce memory usage.

2023-12-10:
Some very basic frugal object stuff is now in here, but we get only a modest reduction. Many FMs still have at
least "language: English", which is one category and one tag (two Lists, one HashSet, and one Dictionary).

Ideas:
-If we only have one item, we don't need any of the enumerables.
 List = one string, HashSet = one string, Dictionary = one FMTagsCollection
-Technically we could elide the FMTagsCollection entirely in that case, but callers are expecting an object of
 that type. We'd need a wrapper object with just the one nullable FMTagsCollection field to elide the heavier
 FMTagsCollection object. This would of course add more and more layers of indirection.
*/
public sealed class FMTagsCollection : IEnumerable<string>
{
    private readonly int _initialCapacity;
    private List<string>? _list;
    private HashSetI? _hashSet;

    private bool FieldsInitialized
    {
        [MemberNotNullWhen(true, nameof(_hashSet), nameof(_list))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    [MemberNotNull(nameof(_hashSet), nameof(_list))]
    private void InitializeFields()
    {
        _list ??= new List<string>(_initialCapacity);
        _hashSet ??= new HashSetI(_initialCapacity);
        FieldsInitialized = true;
    }

    public FMTagsCollection() => _initialCapacity = 1;

    public FMTagsCollection(int capacity) => _initialCapacity = capacity;

    public int Count => _list?.Count ?? 0;

    public void Clear()
    {
        _hashSet?.Clear();
        _list?.Clear();
    }

    public bool Contains(string item) => _hashSet?.Contains(item) ?? false;

    public void Add(string tag)
    {
        InitializeFields();

        if (_hashSet.Add(tag))
        {
            _list.Add(tag);
        }
    }

    public void Remove(string category)
    {
        if (!FieldsInitialized) return;

        _hashSet.Remove(category);
        _list.Remove(category);
    }

    public string this[int index] => _list![index];

    public void SortCaseInsensitive() => _list?.Sort(StringComparer.OrdinalIgnoreCase);

    public IEnumerator<string> GetEnumerator()
    {
        if (FieldsInitialized)
        {
            foreach (string item in _list)
            {
                yield return item;
            }
        }
        else
        {
            foreach (string item in Array.Empty<string>())
            {
                yield return item;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[StructLayout(LayoutKind.Auto)]
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
    private readonly int _initialCapacity;
    private List<string>? _list;
    private DictionaryI<FMTagsCollection>? _dict;

    private bool FieldsInitialized
    {
        [MemberNotNullWhen(true, nameof(_dict), nameof(_list))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    [MemberNotNull(nameof(_dict), nameof(_list))]
    private void InitializeFields()
    {
        _dict ??= new DictionaryI<FMTagsCollection>(_initialCapacity);
        _list ??= new List<string>(_initialCapacity);
        FieldsInitialized = true;
    }

    public FMCategoriesCollection() => _initialCapacity = 1;

    public FMCategoriesCollection(int capacity) => _initialCapacity = capacity;

    public int Count => _list?.Count ?? 0;

    public void Add(string category, FMTagsCollection tags)
    {
        InitializeFields();

        if (_dict.TryAdd(category, tags))
        {
            _list.Add(category);
        }
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out FMTagsCollection? value)
    {
        if (FieldsInitialized)
        {
            return _dict.TryGetValue(key, out value);
        }
        else
        {
            value = null;
            return false;
        }
    }

    public bool ContainsKey(string key) => _dict?.ContainsKey(key) ?? false;

    public bool Remove(string category)
    {
        if (!FieldsInitialized) return false;

        _list.Remove(category);
        return _dict.Remove(category);
    }

    public CatAndTagsList this[int index] => new(_list![index], _dict![_list![index]]);

    public void Clear()
    {
        _dict?.Clear();
        _list?.Clear();
    }

    public void DeepCopyTo(FMCategoriesCollection dest)
    {
        dest.Clear();
        if (!FieldsInitialized) return;
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
        if (!FieldsInitialized) return;

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
    public CatAndTagsList this[int index] => new CatAndTagsList(_list![index], _dict![_list![index]]);
#endif

    public IEnumerator<CatAndTagsList> GetEnumerator()
    {
        if (FieldsInitialized)
        {
            foreach (string item in _list)
            {
                yield return new CatAndTagsList(item, _dict[item]);
            }
        }
        else
        {
            foreach (string item in Array.Empty<string>())
            {
                yield return new CatAndTagsList(item, new FMTagsCollection());
            }
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

        foreach (KeyValuePair<string, string[]> presetTag in _fmSelPresetTags)
        {
            string category = presetTag.Key;
            FMTagsCollection tags = new(presetTag.Value.Length);
            foreach (string value in presetTag.Value)
            {
                tags.Add(value);
            }

            dest.Add(category, tags);
        }
    }
}

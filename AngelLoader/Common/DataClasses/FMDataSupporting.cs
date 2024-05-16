#define FenGen_TypeSource

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using AL_Common;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.FenGenAttributes;
using static AngelLoader.Utils;

namespace AngelLoader.DataClasses;

internal sealed class ExpandableDate
{
    private bool _expanded;
    private DateTime? _dateTime;

    internal string UnixDateString = "";

    internal DateTime? DateTime
    {
        get
        {
            if (_expanded)
            {
                return _dateTime;
            }
            else
            {
                _dateTime = ConvertHexUnixDateToDateTime(UnixDateString.AsSpan());
                _expanded = true;
                return _dateTime;
            }
        }
        set
        {
            _dateTime = value;
            UnixDateString = value != null
                ? new DateTimeOffset((DateTime)value).ToUnixTimeSeconds().ToString("X", CultureInfo.InvariantCulture)
                : "";
            _expanded = true;
        }
    }
}

internal sealed class ExpandableDate_FromTicks
{
    private readonly long _ticks;
    private DateTime? _dateTime;

    public ExpandableDate_FromTicks(long ticks) => _ticks = ticks;

    internal DateTime DateTime
    {
        get
        {
            _dateTime ??= DateTime.FromFileTimeUtc(_ticks).ToLocalTime();
            return (DateTime)_dateTime;
        }
    }
}

public sealed class AltTitlesList
{
    // ~65% of known FMs as of 2024-02-16 have <2 alternate titles
    private object? _values;

    public int Count;

    [MemberNotNull(nameof(_values))]
    private void InitValuesToList()
    {
        if (_values is not List<string>)
        {
            _values = new List<string>();
        }
    }

    public void Add(string value)
    {
        if (Count == 0)
        {
            _values = value;
        }
        else
        {
            string? prevSingle = _values as string;
            InitValuesToList();
            List<string> list = Unsafe.As<List<string>>(_values);
            if (prevSingle != null)
            {
                list.Add(prevSingle);
            }
            list.Add(value);
        }
        ++Count;
    }

    public void ClearAndAddTitleAndAltTitles(string title, string[] altTitles)
    {
        _values = title;
        Count = 1;

        if (altTitles.Length == 0) return;

        string? prevSingle = _values as string;
        InitValuesToList();
        List<string> list = Unsafe.As<List<string>>(_values);
        if (prevSingle != null)
        {
            list.Add(prevSingle);
        }
        list.AddRange(altTitles);
        Count += altTitles.Length;
    }

    public void Clear()
    {
        if (_values is string)
        {
            _values = "";
        }
        else if (_values != null)
        {
            Unsafe.As<List<string>>(_values).Clear();
        }
        Count = 0;
    }

    public string this[int index]
    {
        get
        {
            if (index == 0 && _values is string str)
            {
                return str;
            }
            else if (_values != null)
            {
                return Unsafe.As<List<string>>(_values)[index];
            }
            else
            {
                ThrowHelper.IndexOutOfRange();
                return null;
            }
        }
    }
}

/*
@MEM(Readme code pages collection):
A List is half the size of a Dictionary (40 bytes vs. 80 bytes). If we wanted to be maximally compact even in the
worst case, we could use a List. That means linear searches, but the most readmes any known FM has is 8, and it's
almost unimaginable that any non-troll FM would have enough for a linear search to become a problem.
*/
public sealed class ReadmeCodePagesCollection
{
    // ~77% of known FMs as of 2024-02-16 have <2 readmes
    private object? _values;

    #region Massive disgusting hack

    // Because of enumerators being allocated 8 trillion times, let's do this horrible thing where the generated
    // FMData.ini writer code does the type switch itself.

    public bool TryGetDictionary([NotNullWhen(true)] out DictionaryI<int>? result)
    {
        if (_values is DictionaryI<int> dict)
        {
            result = dict;
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }

    private static readonly KeyValuePair<string, int> _blankSingle = new();

    public bool TryGetSingle(out KeyValuePair<string, int> result)
    {
        if (_values is KeyValuePair<string, int> single)
        {
            result = single;
            return true;
        }
        else
        {
            result = _blankSingle;
            return false;
        }
    }

    #endregion

    [MemberNotNull(nameof(_values))]
    private void InitValuesToDict()
    {
        if (_values is not DictionaryI<int>)
        {
            _values = new DictionaryI<int>();
        }
    }

    public bool TryGetValue(string key, out int value)
    {
        if (_values != null)
        {
            if (_values is KeyValuePair<string, int> single)
            {
                if (key == single.Key)
                {
                    value = single.Value;
                    return true;
                }
            }
            else if (_values is DictionaryI<int> dict)
            {
                return dict.TryGetValue(key, out value);
            }
        }

        value = 0;
        return false;
    }

    public int this[string key]
    {
        set
        {
            if (_values == null)
            {
                _values = new KeyValuePair<string, int>(key, value);
            }
            else if (_values is KeyValuePair<string, int> single)
            {
                if (key.EqualsI(single.Key))
                {
                    _values = new KeyValuePair<string, int>(key, value);
                }
                else
                {
                    InitValuesToDict();
                    DictionaryI<int> dict = Unsafe.As<DictionaryI<int>>(_values);
                    dict[single.Key] = single.Value;
                    dict[key] = value;
                }
            }
            else
            {
                DictionaryI<int> dict = Unsafe.As<DictionaryI<int>>(_values);
                dict[key] = value;
            }
        }
    }
}

// IMPORTANT: Do not rename elements or compatibility will break!
[Flags]
[FenGenEnumNames]
[FenGenEnumCount]
internal enum CustomResources : ushort
{
    None = 0,
    Map = 1,
    Automap = 2,
    Scripts = 4,
    Textures = 8,
    Sounds = 16,
    Objects = 32,
    Creatures = 64,
    Motions = 128,
    Movies = 256,
    Subtitles = 512,
}

[Flags]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[FenGenEnumCount(-1)]
public enum Difficulty : uint
{
    None = 0,
    Normal = 1,
    Hard = 2,
    Expert = 4,
    Extreme = 8,
}

#if DateAccTest
public enum DateAccuracy
{
    Null,
    Red,
    Yellow,
    Green
}
#endif

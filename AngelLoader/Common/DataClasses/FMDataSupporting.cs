#define FenGen_TypeSource

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using AL_Common;
using JetBrains.Annotations;
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

public sealed class AltTitlesList : IEnumerable<string>
{
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

    public void ClearAndAdd_Single(string str)
    {
        if (_values is null or string)
        {
            _values = str;
        }
        else
        {
            InitValuesToList();
            Unsafe.As<List<string>>(_values).ClearAndAdd_Single(str);
        }
    }

    public void AddRange(string[] collection)
    {
        if (collection.Length == 1)
        {
            if (_values is null or string)
            {
                _values = collection[0];
            }
            else
            {
                List<string> list = Unsafe.As<List<string>>(_values);
                list.Add(collection[0]);
            }
        }
        else if (collection.Length > 1)
        {
            string? prevSingle = _values as string;
            InitValuesToList();
            List<string> list = Unsafe.As<List<string>>(_values);
            if (prevSingle != null)
            {
                list.Add(prevSingle);
            }
            list.AddRange(collection);
        }
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

    public IEnumerator<string> GetEnumerator()
    {
        if (_values is string str)
        {
            yield return str;
        }
        else if (_values != null)
        {
            foreach (string item in Unsafe.As<List<string>>(_values))
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

// IMPORTANT: Do not rename elements or compatibility will break!
[Flags]
[FenGenEnumNames]
[FenGenEnumCount]
internal enum CustomResources
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
    Subtitles = 512
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
    Extreme = 8
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

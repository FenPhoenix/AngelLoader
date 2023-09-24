﻿#define FenGen_TypeSource

using System;
using System.Globalization;
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
                _dateTime = ConvertHexUnixDateToDateTime(UnixDateString);
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

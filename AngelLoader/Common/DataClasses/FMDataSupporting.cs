using System;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.DataClasses
{
    #region FM data

    // Startup perf: we don't need to convert them on ini read, we can lazy-load their heavy DateTime? objects
    // later when we go to display them
    internal sealed class ExpandableDate
    {
        private bool _expanded;
        private DateTime? _dateTime;

        internal string UnixDateString { get; set; } = "";

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
                    ? new DateTimeOffset((DateTime)value).ToUnixTimeSeconds().ToString("X")
                    : "";
                _expanded = true;
            }
        }
    }

    // IMPORTANT: Do not rename elements or compatibility will break!
    [Flags]
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

    [Flags, PublicAPI]
    internal enum Difficulty : uint { None = 0, Normal = 1, Hard = 2, Expert = 4, Extreme = 8 }

    #endregion
}

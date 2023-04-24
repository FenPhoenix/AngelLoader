using System.Collections.Generic;
using AngelLoader.DataClasses;

namespace AngelLoader;

internal static class Global
{
    internal static readonly ConfigData Config = new();

    // This one is sort of quasi-immutable: its fields are readonly (they're loaded by reflection) but the
    // object itself is not readonly, so that the reader can start with a fresh instance with default values
    // for all the fields it doesn't find a new value for.
    internal static LText_Class LText = new();

    // Preset tags will be deep copied to this list later
    internal static readonly FMCategoriesCollection GlobalTags = new(PresetTags.Count);

    #region FM lists

    // Init to 0 capacity so they don't allocate a 4-byte backing array or whatever, cause we're going to
    // reallocate them right away anyway.
    internal static readonly List<FanMission> FMDataIniList = new(0);
    internal static readonly List<FanMission> FMsViewList = new(0);

    #endregion
}
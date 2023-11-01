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

    #region FM lists

#if !RT_HeavyTests
    // Preset tags will be deep copied to this list later
    internal static readonly FMCategoriesCollection GlobalTags = new(PresetTags.Count);

    // Init to 0 capacity so they don't allocate a 4-byte backing array or whatever, cause we're going to
    // reallocate them right away anyway.
    internal static readonly List<FanMission> FMDataIniList = new(0);
    internal static readonly List<FanMission> FMDataIniListTDM = new(0);
    internal static readonly List<FanMission> FMsViewList = new(0);
#else
    internal static bool ThreadLocked;

    private static readonly FMCategoriesCollection _globalTags = new(PresetTags.Count);
    private static readonly List<FanMission> _fmDataIniList = new(0);
    private static readonly List<FanMission> _fmDataIniListTDM = new(0);
    private static readonly List<FanMission> _fmsViewList = new(0);

    internal static FMCategoriesCollection GlobalTags
    {
        get
        {
            if (ThreadLocked) CheckThread(nameof(GlobalTags));
            return _globalTags;
        }
    }

    internal static List<FanMission> FMDataIniList
    {
        get
        {
            if (ThreadLocked) CheckThread(nameof(FMDataIniList));
            return _fmDataIniList;
        }
    }

    internal static List<FanMission> FMDataIniListTDM
    {
        get
        {
            if (ThreadLocked) CheckThread(nameof(FMDataIniListTDM));
            return _fmDataIniListTDM;
        }
    }

    internal static List<FanMission> FMsViewList
    {
        get
        {
            if (ThreadLocked) CheckThread(nameof(FMsViewList));
            return _fmsViewList;
        }
    }

    private static void CheckThread(string fieldName)
    {
        Utils.AssertR(Core.CoreThread != System.Threading.Thread.CurrentThread, "Startup cross-thread access of " + fieldName);
    }
#endif

    #endregion
}

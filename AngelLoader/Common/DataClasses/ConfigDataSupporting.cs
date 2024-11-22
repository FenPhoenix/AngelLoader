#define FenGen_TypeSource

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AL_Common;
using static AL_Common.Common;
using static AL_Common.FenGenAttributes;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.DataClasses;

/// <summary>
/// Cheesy hack to make sure exceptions are thrown in the same places as before - on access of the path properties.
/// And because we're not actually modifying the global config object anymore, we're thread-safe now too.
/// </summary>
public sealed class DarkLoaderBackupContext
{
    private readonly Exception? DarkLoaderBackupPathException;
    private readonly Exception? DarkLoaderOriginalBackupPathException;

    public DarkLoaderBackupContext()
    {
        try
        {
            _darkLoaderBackupPath = Path.Combine(Global.Config.FMsBackupPath, Paths.DarkLoaderSaveBakDir);
            DarkLoaderBackupPathException = null;
        }
        catch (Exception ex)
        {
            _darkLoaderBackupPath = "";
            DarkLoaderBackupPathException = ex;
        }

        try
        {
            _darkLoaderOriginalBackupPath = Path.Combine(Global.Config.FMsBackupPath, Paths.DarkLoaderSaveOrigBakDir);
            DarkLoaderOriginalBackupPathException = null;
        }
        catch (Exception ex)
        {
            _darkLoaderOriginalBackupPath = "";
            DarkLoaderOriginalBackupPathException = ex;
        }
    }

    private readonly string _darkLoaderBackupPath;
    public string DarkLoaderBackupPath =>
        DarkLoaderBackupPathException == null
            ? _darkLoaderBackupPath
            : throw DarkLoaderBackupPathException;

    private readonly string _darkLoaderOriginalBackupPath;
    public string DarkLoaderOriginalBackupPath =>
        DarkLoaderOriginalBackupPathException == null
            ? _darkLoaderOriginalBackupPath
            : throw DarkLoaderOriginalBackupPathException;
}

#region Columns

public sealed class ColumnData
{
    public Column Id;

#if SMART_NEW_COLUMN_INSERT
    // Don't clamp this anymore because it messes with our validator; let the validator handle it
    internal int DisplayIndex;
#else
    private int _displayIndex;
    public int DisplayIndex { get => _displayIndex; set => _displayIndex = value.Clamp(0, ColumnCount - 1); }
#endif

    private int _width = Defaults.ColumnWidth;
    public int Width { get => _width; set => _width = value.Clamp(Defaults.MinColumnWidth, 65536); }

    public bool Visible = true;

#if SMART_NEW_COLUMN_INSERT
    // Needed for the validator to sort new columns with the same column index as old ones in the intended way
    public bool ExplicitlySet;
#endif
}

public sealed class ColumnDataArray
{
    private readonly ColumnData[] _columns;

    public ColumnDataArray()
    {
        _columns = new ColumnData[ColumnCount];
        // Must set the display indexes, otherwise we crash!
        for (int i = 0; i < ColumnCount; i++)
        {
            _columns[i] = new ColumnData { Id = (Column)i, DisplayIndex = i };
        }
    }

    public ColumnData this[int index] => _columns[index];

    public IEnumerator<ColumnData> GetEnumerator()
    {
        foreach (ColumnData column in _columns)
        {
            yield return column;
        }
    }

    public ColumnDataArray OrderById()
    {
        ColumnData[] columnsArray = _columns.OrderBy(static x => x.Id).ToArray();
        ColumnDataArray ret = new();
        for (int i = 0; i < ColumnCount; i++)
        {
            ret._columns[i] = columnsArray[i];
        }
        return ret;
    }

    public ColumnDataArray OrderByDisplayIndex()
    {
        ColumnData[] columnsArray = _columns.OrderBy(static x => x.DisplayIndex).ToArray();
        ColumnDataArray ret = new();
        for (int i = 0; i < ColumnCount; i++)
        {
            ret._columns[i] = columnsArray[i];
        }
        return ret;
    }

#if SMART_NEW_COLUMN_INSERT
    public ColumnDataArray Sorted(IComparer<ColumnData> comparer)
    {
        ColumnDataArray ret = new();
        CopyTo(ret);
        Array.Sort(ret._columns, comparer);
        return ret;
    }
#endif

    public void CopyTo(ColumnDataArray dest) => Array.Copy(_columns, dest._columns, ColumnCount);
}

[FenGenEnumCount]
public enum Column
{
#if DateAccTest
    DateAccuracy,
#endif
    Game,
    Installed,
    MissionCount,
    Title,
    Archive,
    Author,
    Size,
    Rating,
    Finished,
    ReleaseDate,
    LastPlayed,
    DateAdded,
    PlayTime,
    DisabledMods,
    Comment,
}

#endregion

[FenGenEnumCount]
public enum HideableFilterControls
{
    Title,
    Author,
    ReleaseDate,
    LastPlayed,
    Tags,
    FinishedState,
    Rating,
    ShowUnsupported,
    ShowUnavailable,
    ShowRecentAtTop,
}

internal enum GameOrganization { ByTab, OneList }

public enum RatingDisplayStyle { NewDarkLoader, FMSel }

internal enum DateFormat { CurrentCultureShort, CurrentCultureLong, Custom }

[Flags]
internal enum FinishedState : uint { Null = 0, Finished = 1, Unfinished = 2 }

internal enum BackupFMData { SavesAndScreensOnly, AllChangedFiles }

internal enum ConfirmBeforeInstall { Always, OnlyForMultiple, Never }

internal enum RunThiefBuddyOnFMPlay
{
    Always,
    Ask,
    Never,
}

public enum VisualTheme { Classic, Dark }

public enum CheckForUpdates
{
    FirstTimeAsk,
    True,
    False,
}

public enum IOThreadsMode
{
    Auto,
    Custom,
}

public enum IOThreadingLevel
{
    Normal,
    Aggressive,
}

public enum DriveMultithreadingLevel
{
    None,
    Read,
    ReadWrite,
    Auto,
}

public enum ThreadablePathType
{
    FMInstallPath,
    BackupPath,
    ArchivePath,
    TempPath,
    FMCachePath,
}

#region FM tabs

// IMPORTANT(FM tabs enum): Do not rename members, they're used in the config file
[FenGenEnumCount]
internal enum FMTab
{
    Statistics,
    EditFM,
    Comment,
    Tags,
    Patch,
    Mods,
    Screenshots,
}

public enum FMTabVisibleIn
{
    None,
    Top,
    Bottom,
}

internal sealed class FMTabData
{
    private int _displayIndex;
    internal int DisplayIndex { get => _displayIndex; set => _displayIndex = value.Clamp(0, FMTabCount - 1); }

    internal FMTabVisibleIn Visible = FMTabVisibleIn.Top;
}

internal sealed class FMTabsData
{
    internal readonly FMTabData[] Tabs = InitializedArray<FMTabData>(FMTabCount);

    internal FMTab SelectedTab = FMTab.Statistics;
    internal FMTab SelectedTab2 = FMTab.Statistics;

    internal FMTabsData() => ResetAllDisplayIndexes();

    internal FMTabData GetTab(FMTab tab) => Tabs[(int)tab];

    internal void EnsureValidity()
    {
        #region Fallback if multiple tabs have the same display index

        var displayIndexesSet = new HashSet<int>();
        foreach (FMTabData tab in Tabs)
        {
            if (!displayIndexesSet.Add(tab.DisplayIndex))
            {
                ResetAllDisplayIndexes();
                break;
            }
        }

        #endregion

        if (NoneVisible()) SetAllVisible(FMTabVisibleIn.Top);

        // Fallback if selected tab is not marked as visible
        if (GetTab(SelectedTab).Visible != FMTabVisibleIn.Top)
        {
            for (int i = 0; i < FMTabCount; i++)
            {
                if (Tabs[i].Visible == FMTabVisibleIn.Top)
                {
                    SelectedTab = (FMTab)i;
                    break;
                }
            }
        }
        if (GetTab(SelectedTab2).Visible != FMTabVisibleIn.Bottom)
        {
            for (int i = 0; i < FMTabCount; i++)
            {
                if (Tabs[i].Visible == FMTabVisibleIn.Bottom)
                {
                    SelectedTab2 = (FMTab)i;
                    break;
                }
            }
        }
    }

    private bool NoneVisible()
    {
        foreach (FMTabData tab in Tabs)
        {
            if (tab.Visible != FMTabVisibleIn.None) return false;
        }
        return true;
    }

    private void SetAllVisible(FMTabVisibleIn visible)
    {
        foreach (FMTabData tab in Tabs)
        {
            tab.Visible = visible;
        }
    }

    private void ResetAllDisplayIndexes()
    {
        for (int i = 0; i < Tabs.Length; i++)
        {
            Tabs[i].DisplayIndex = i;
        }
    }
}

#endregion

#region Filter

public sealed class Filter
{
    internal void ClearAll(bool clearGames = true)
    {
        ClearTitle();
        ClearAuthor();
        if (clearGames) ClearGames();
        ClearTags();
        ClearRating();
        ClearReleaseDate();
        ClearLastPlayed();
        ClearFinished();
    }

    internal void ClearHideableFilter(HideableFilterControls filter)
    {
        switch (filter)
        {
            case HideableFilterControls.Title:
                ClearTitle();
                break;
            case HideableFilterControls.Author:
                ClearAuthor();
                break;
            case HideableFilterControls.ReleaseDate:
                ClearReleaseDate();
                break;
            case HideableFilterControls.LastPlayed:
                ClearLastPlayed();
                break;
            case HideableFilterControls.Tags:
                ClearTags();
                break;
            case HideableFilterControls.FinishedState:
                ClearFinished();
                break;
            case HideableFilterControls.Rating:
                ClearRating();
                break;
        }
    }

    private void ClearTitle() => Title = "";

    private void ClearAuthor() => Author = "";

    private void ClearGames() => Games = Game.Null;

    private void ClearTags() => Tags.Clear();

    private void ClearRating()
    {
        _ratingFrom = -1;
        _ratingTo = 10;
    }

    private void ClearReleaseDate()
    {
        ReleaseDateFrom = null;
        ReleaseDateTo = null;
    }

    private void ClearLastPlayed()
    {
        LastPlayedFrom = null;
        LastPlayedTo = null;
    }

    private void ClearFinished() => Finished = FinishedState.Null;

    internal string Title = "";
    internal string Author = "";
    internal Game Games = Game.Null;
    internal readonly TagsFilter Tags = new();

    #region Rating

    internal void SetRatingFromAndTo(int from, int to)
    {
        RatingFrom = Math.Min(from, to);
        RatingTo = Math.Max(from, to);
    }

    private int _ratingFrom = -1;
    internal int RatingFrom { get => _ratingFrom; set => _ratingFrom = value.SetRatingClamped(); }

    private int _ratingTo = 10;
    internal int RatingTo { get => _ratingTo; set => _ratingTo = value.SetRatingClamped(); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool RatingIsSet() => this is not { _ratingFrom: -1, _ratingTo: 10 };

    #endregion

    internal void SetDateFromAndTo(bool lastPlayed, DateTime? from, DateTime? to)
    {
        if (from != null && to != null && from.Value.CompareTo((DateTime)to) > 0)
        {
            if (lastPlayed)
            {
                LastPlayedFrom = to;
                LastPlayedTo = from;
            }
            else
            {
                ReleaseDateFrom = to;
                ReleaseDateTo = from;
            }
        }
        else
        {
            if (lastPlayed)
            {
                LastPlayedFrom = from;
                LastPlayedTo = to;
            }
            else
            {
                ReleaseDateFrom = from;
                ReleaseDateTo = to;
            }
        }
    }

    #region Release date

    internal DateTime? ReleaseDateFrom;
    internal DateTime? ReleaseDateTo;

    #endregion

    #region Last played

    internal DateTime? LastPlayedFrom;
    internal DateTime? LastPlayedTo;

    #endregion

    internal FinishedState Finished = FinishedState.Null;

    internal void DeepCopyTo(Filter dest)
    {
        dest.ClearAll();
        dest.Title = Title;
        dest.Author = Author;
        dest.SetRatingFromAndTo(RatingFrom, RatingTo);

        dest.SetDateFromAndTo(lastPlayed: false, ReleaseDateFrom, ReleaseDateTo);
        dest.SetDateFromAndTo(lastPlayed: true, LastPlayedFrom, LastPlayedTo);

        dest.Finished = Finished;
        dest.Games = Games;
        Tags.DeepCopyTo(dest.Tags);
    }
}

internal sealed class TagsFilter
{
    internal readonly FMCategoriesCollection AndTags = new();
    internal readonly FMCategoriesCollection OrTags = new();
    internal readonly FMCategoriesCollection NotTags = new();

    internal void Clear()
    {
        AndTags.Clear();
        OrTags.Clear();
        NotTags.Clear();
    }

    internal bool IsEmpty() => AndTags.Count == 0 &&
                               OrTags.Count == 0 &&
                               NotTags.Count == 0;

    internal void DeepCopyTo(TagsFilter dest)
    {
        AndTags.DeepCopyTo(dest.AndTags);
        OrTags.DeepCopyTo(dest.OrTags);
        NotTags.DeepCopyTo(dest.NotTags);
    }
}

#endregion

// IMPORTANT: SettingsTab enum used as indexes; don't reorder
[FenGenEnumCount]
internal enum SettingsTab
{
    Paths,
    Appearance,
    Other,
    ThiefBuddy,
    Update,
    IOThreading,
}

/*
TODO: This name is confusing, it sounds like it refers to an entire FanMission object or something
Naming this is brutally difficult. If we call it SelectedFM, it sounds like it's encapsulating an entire
FM object, and var names ("selectedFM" / "selFM") sound like they're of type FanMission. FMSelectionData
or FMSelectionInfo is likely to be shortened to fmSelData/fmSelInfo, and then it sounds like we're talking
about FMSel the application. Calling it FMInstNameAndScrollIndex is clear and descriptive but annoyingly
long and shortening it is impossible (fmINASI?!). Also, we have multiple of them (one per game tab and then
one global), so that would now be FMInstNameAndScrollIndexes which is an awkward half-plural.
SelectionMetadata? SelectedFMTag/Handle? Maybe we put an underscore like fm_SelData?
Maybe FMInstNameAndScrollIndex really is the least worst name here.
*/
public sealed class SelectedFM
{
    internal void DeepCopyTo(SelectedFM dest)
    {
        dest.IndexFromTop = IndexFromTop;
        dest.InstalledName = InstalledName;
    }

    internal void Clear()
    {
        IndexFromTop = 0;
        InstalledName = "";
    }

    internal string InstalledName = "";

    private int _indexFromTop;
    /// <summary>
    /// The index relative to the first displayed item (not the first item period) in the list.
    /// </summary>
    internal int IndexFromTop { get => _indexFromTop; set => _indexFromTop = value.ClampToZero(); }
}

internal sealed class GameTabsState
{
    internal GameTabsState() => InitializeArrays(SupportedGameCount, out _selectedFMs, out _filters);

    #region Selected FMs

    private readonly SelectedFM[] _selectedFMs;

    internal SelectedFM GetSelectedFM(GameIndex index) => _selectedFMs[(uint)index];

    #endregion

    #region Filters

    private readonly Filter[] _filters;

    internal Filter GetFilter(GameIndex index) => _filters[(uint)index];

    #endregion

    // TODO: Add sorted column / sort order as a per-tab thing

    internal void DeepCopyTo(GameTabsState dest)
    {
        for (int i = 0; i < SupportedGameCount; i++)
        {
            _selectedFMs[i].DeepCopyTo(dest._selectedFMs[i]);
            _filters[i].DeepCopyTo(dest._filters[i]);
        }
    }

    internal void ClearAllSelectedFMs()
    {
        foreach (SelectedFM selectedFM in _selectedFMs)
        {
            selectedFM.Clear();
        }
    }

    internal void ClearAllFilters()
    {
        foreach (Filter filter in _filters)
        {
            filter.ClearAll();
        }
    }
}

internal enum ModType
{
    ModPath,
    UberModPath,
    MPModPath,
    MPUberModPath,
}

[StructLayout(LayoutKind.Auto)]
internal readonly struct Mod
{
    internal readonly string InternalName;
    internal readonly bool IsUber;

    internal Mod(string internalName, ModType type)
    {
        InternalName = internalName;
        IsUber = type is ModType.UberModPath or ModType.MPUberModPath;
    }
}

#define FenGen_ConfigSource

using System;
using System.Collections.Generic;
using System.Drawing;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.DataClasses;

public sealed class ConfigData
{
    internal ConfigData()
    {
        #region To be left at defaults

        _newMantling = new bool?[SupportedGameCount];
        _gameEditorDetected = new bool[SupportedGameCount];
        _startupAlwaysStartSelector = new bool[SupportedGameCount];

        _mods = new List<Mod>?[SupportedGameCount];

        #endregion

        #region To be initialized in loop

        _disabledMods = new string[SupportedGameCount];

        _gameExes = new string[SupportedGameCount];
        _gamePaths = new string[SupportedGameCount];
        _fmInstallPaths = new string[SupportedGameCount];

        _useSteamSwitches = new bool[SupportedGameCount];
        _startupFMSelectorLines = new List<string>[SupportedGameCount];

        GameFilterControlVisibilities = new bool[SupportedGameCount];

        #endregion

        FilterControlVisibilities = InitializedArray(HideableFilterControlsCount, true);

        for (int i = 0; i < SupportedGameCount; i++)
        {
            // We want them empty strings, not null, for safety
            _disabledMods[i] = "";

            _gameExes[i] = "";
            _gamePaths[i] = "";
            _fmInstallPaths[i] = "";

            _useSteamSwitches[i] = true;
            _startupFMSelectorLines[i] = new List<string>();

            GameFilterControlVisibilities[i] = true;
        }
    }

    //internal int Version = 1;

    #region Saved-on-startup loader config values

    #region fm_selector lines

    private readonly List<string>[] _startupFMSelectorLines;

    internal List<string> GetStartupFMSelectorLines(GameIndex index) => _startupFMSelectorLines[(uint)index];

    internal void SetStartupFMSelectorLines(GameIndex index, List<string> value) => _startupFMSelectorLines[(uint)index] = value;

    #endregion

    #region "Always start selector" values

    private readonly bool[] _startupAlwaysStartSelector;

    internal bool GetStartupAlwaysStartSelector(GameIndex index) => _startupAlwaysStartSelector[(uint)index];

    internal void SetStartupAlwaysStartSelector(GameIndex index, bool value) => _startupAlwaysStartSelector[(uint)index] = value;

    #endregion

    #endregion

    #region New mantling

    private readonly bool?[] _newMantling;

    internal bool? GetNewMantling(GameIndex index) => GameIsDark(index) ? _newMantling[(int)index] : null;

    internal void SetNewMantling(GameIndex index, bool? value) => _newMantling[(int)index] = GameIsDark(index) ? value : null;

    #endregion

    #region Mod dirs

    // Hack that's only here for perf in the mods control, so it can check only a single bool to know if it needs
    // to recreate its checkbox list. Complete leak of view implementation details into here, but meh...
    internal bool ModsChanged;

    private readonly List<Mod>?[] _mods;

    internal List<Mod> GetMods(GameIndex gameIndex) => _mods[(int)gameIndex] ??= new List<Mod>();

    internal void SetMods(GameIndex gameIndex, List<Mod> value)
    {
        _mods[(int)gameIndex] = value;
        ModsChanged = true;
    }

    #endregion

    #region Disabled mods

    private readonly string[] _disabledMods;

    internal string GetDisabledMods(GameIndex index) => GameSupportsMods(index) ? _disabledMods[(int)index] : "";

    internal void SetDisabledMods(GameIndex index, string value) => _disabledMods[(int)index] = GameSupportsMods(index) ? value : "";

    #endregion

    #region Paths

    internal readonly List<string> FMArchivePaths = new();
    internal bool FMArchivePathsIncludeSubfolders;

    /*
    NOTE: Backup path is currently required. Notes on potentially making it optional:
    -We would need to add a guard check before attempting to either back up or restore an FM. We'd put up
     a dialog telling the user they need to specify a backup path first, and let them click a button to go
     to the Settings window backup path field.
    -DarkLoader import needs to know the backup path (when importing saves). We'd have to ask the user here
     too. (2023-10-24: We do this now.)
    -Due to backup/restore FM being a dead-common operation and one of the main purposes of AngelLoader
     even, allowing the backup path to be optional seems of questionable utility. However, we do allow
     all other fields to be optional even though leaving them ALL blank is basically nonsensical, and
     having backup path be the lone required field feels arbitrary. But then, allowing the user to not
     worry about it but then slapping them with a "hey, go set this!" message as soon as they try to
     install or uninstall something might be the more annoying thing.

    2023/10/11:
    Backup path is now only required if you've specified one or more games that require it (TDM doesn't).
    */
    internal string FMsBackupPath = "";

    #region Game exes

    private readonly string[] _gameExes;

    internal string GetGameExe(GameIndex index) => _gameExes[(uint)index];

    internal void SetGameExe(GameIndex index, string value) => _gameExes[(uint)index] = value;

    #endregion

    #region Game exe paths

    private readonly string[] _gamePaths;

    internal string GetGamePath(GameIndex index) => _gamePaths[(uint)index];

    internal void SetGamePath(GameIndex index, string value) => _gamePaths[(uint)index] = value;

    #endregion

    #region FM install paths

    private readonly string[] _fmInstallPaths;

    internal string GetFMInstallPath(GameIndex index) => _fmInstallPaths[(uint)index];

    internal void SetFMInstallPath(GameIndex index, string value) => _fmInstallPaths[(uint)index] = value;

    #endregion

    #region Game editor detected

    // Session-only; don't write these out

    private readonly bool[] _gameEditorDetected;

    internal bool GetGameEditorDetected(GameIndex gameIndex) => _gameEditorDetected[(uint)gameIndex];

    internal void SetGameEditorDetected(GameIndex index, bool value) => _gameEditorDetected[(uint)index] = value;

    #endregion

    #region Steam

    internal bool LaunchGamesWithSteam = true;

    internal string SteamExe = "";

    private readonly bool[] _useSteamSwitches;

    internal bool GetUseSteamSwitch(GameIndex index) => _useSteamSwitches[(uint)index];

    internal void SetUseSteamSwitch(GameIndex index, bool value) => _useSteamSwitches[(uint)index] = value;

    #endregion

    // @GENGAMES (ConfigData - Miscellaneous game-specific stuff): Begin

    // New for T2 NewDark 1.27: Multiplayer support (beta, and T2 only)
    internal bool T2MPDetected;

    internal bool T3UseCentralSaves;

    // @GENGAMES (ConfigData - Miscellaneous game-specific stuff): End

    #endregion

    #region Selected FMs

    internal void ClearAllSelectedFMs()
    {
        SelFM.Clear();
        GameTabsState.ClearAllSelectedFMs();
    }

    internal void ClearAllFilters()
    {
        Filter.ClearAll();
        GameTabsState.ClearAllFilters();
    }

    internal GameOrganization GameOrganization = GameOrganization.OneList;
    internal GameIndex GameTab = GameIndex.Thief1;

    internal readonly SelectedFM SelFM = new();

    internal readonly GameTabsState GameTabsState = new();

    #endregion

    #region Filtering

    internal readonly Filter Filter = new();

    internal readonly bool[] FilterControlVisibilities;

    internal readonly bool[] GameFilterControlVisibilities;

    #endregion

    #region Columns and sorting

    internal readonly ColumnDataArray Columns = new();
    internal Column SortedColumn = Column.Title;
    internal SortDirection SortDirection = SortDirection.Ascending;

    private float _fmsListFontSizeInPoints = Defaults.FMsListFontSizeInPoints;
    internal float FMsListFontSizeInPoints
    {
        get => _fmsListFontSizeInPoints;
        set
        {
            float val = value;

            // @NET5: Enable this when we want to start converting old default font size to new
#if false
            if (Version == 1 && Math.Abs(val - 8.25f) < 0.001)
            {
                val = 9.0f;
            }
#endif

            _fmsListFontSizeInPoints = val.ClampToFMsDGVFontSizeMinMax();
        }
    }

    internal bool EnableArticles = true;
    internal readonly List<string> Articles = new() { "a", "an", "the" };
    internal bool MoveArticlesToEnd = true;

    #endregion

    #region Language

    internal string Language = "English";

    // Session-only; don't write out
    internal readonly DictionaryI<string> LanguageNames = new();

    #endregion

    #region Date format

    internal DateFormat DateFormat = DateFormat.CurrentCultureShort;
    // Clunky, but removes the need for parsing
    internal string DateCustomFormat1 = Defaults.DateCustomFormat1;
    internal string DateCustomSeparator1 = Defaults.DateCustomSeparator1;
    internal string DateCustomFormat2 = Defaults.DateCustomFormat2;
    internal string DateCustomSeparator2 = Defaults.DateCustomSeparator2;
    internal string DateCustomFormat3 = Defaults.DateCustomFormat3;
    internal string DateCustomSeparator3 = Defaults.DateCustomSeparator3;
    internal string DateCustomFormat4 = Defaults.DateCustomFormat4;
    // Session-only; don't write out
    internal string DateCustomFormatString = Defaults.DateCustomFormat1 +
                                             Defaults.DateCustomSeparator1 +
                                             Defaults.DateCustomFormat2 +
                                             Defaults.DateCustomSeparator2 +
                                             Defaults.DateCustomFormat3 +
                                             Defaults.DateCustomSeparator3 +
                                             Defaults.DateCustomFormat4;

    #endregion

    #region Main window state

    private WindowState _mainWindowState = WindowState.Maximized;
    internal WindowState MainWindowState { get => _mainWindowState; set => _mainWindowState = value == WindowState.Minimized ? WindowState.Maximized : value; }
    internal Size MainWindowSize = Defaults.MainWindowSize;
    internal Point MainWindowLocation = Defaults.MainWindowLocation;

    private float _mainSplitterPercent = Defaults.MainSplitterPercent;
    internal float MainSplitterPercent { get => _mainSplitterPercent; set => _mainSplitterPercent = value.ClampZeroToOne(); }

    private float _topSplitterPercent = Defaults.TopSplitterPercent;
    internal float TopSplitterPercent { get => _topSplitterPercent; set => _topSplitterPercent = value.ClampZeroToOne(); }

    private float _bottomSplitterPercent = Defaults.BottomSplitterPercent;
    internal float BottomSplitterPercent { get => _bottomSplitterPercent; set => _bottomSplitterPercent = value.ClampZeroToOne(); }

    internal bool TopFMTabsPanelCollapsed;
    internal bool BottomFMTabsPanelCollapsed = true;

    internal readonly FMTabsData FMTabsData = new();

    #endregion

    #region Settings window state

    internal SettingsTab SettingsTab = SettingsTab.Paths;
    internal Size SettingsWindowSize = Defaults.SettingsWindowSize;
    internal int SettingsWindowSplitterDistance = Defaults.SettingsWindowSplitterDistance;

    private readonly int[] _settingsVScrollPositions = new int[SettingsTabCount];
    internal void SetSettingsTabVScrollPos(SettingsTab tab, int value) => _settingsVScrollPositions[(int)tab] = value;
    internal int GetSettingsTabVScrollPos(SettingsTab tab) => _settingsVScrollPositions[(int)tab];

    #endregion

    #region Readme box

    private float _readmeZoomFactor = 1;
    internal float ReadmeZoomFactor
    {
        get => _readmeZoomFactor;
        set
        {
            value = (float)Math.Round(value, 1, MidpointRounding.AwayFromZero);
            _readmeZoomFactor = value.ClampToRichTextBoxZoomMinMax();
        }
    }

    internal bool ReadmeUseFixedWidthFont = true;

    #endregion

    #region Rating display style

    internal RatingDisplayStyle RatingDisplayStyle = RatingDisplayStyle.FMSel;
    internal bool RatingUseStars = true;

    #endregion

    #region FM settings

    internal bool ConvertWAVsTo16BitOnInstall = true;
    internal bool ConvertOGGsToWAVsOnInstall;

    internal bool UseOldMantlingForOldDarkFMs;

    #endregion

    #region Uninstall

    internal bool ConfirmUninstall = true;

    internal BackupFMData BackupFMData = BackupFMData.AllChangedFiles;
    internal bool BackupAlwaysAsk = true;

    #endregion

    private static string[] InitWebSearchUrls()
    {
        string[] ret = new string[SupportedGameCount];
        Defaults.WebSearchUrls.CopyTo(ret, 0);
        return ret;
    }
    internal readonly string[] WebSearchUrls = InitWebSearchUrls();
    internal string GetWebSearchUrl(GameIndex index) => WebSearchUrls[(int)index];
    internal void SetWebSearchUrl(GameIndex index, string value) => WebSearchUrls[(int)index] = value;

    internal bool ConfirmPlayOnDCOrEnter = true;

    #region Show/hide UI elements

    internal bool HideUninstallButton;
    internal bool HideFMListZoomButtons;
    internal bool HideExitButton = true;
    internal bool HideWebSearchButton;

    #endregion

    internal bool UseShortGameTabNames;

    #region Recent FMs

    private uint _daysRecent = Defaults.DaysRecent;
    internal uint DaysRecent { get => _daysRecent; set => _daysRecent = value.Clamp((uint)0, Defaults.MaxDaysRecent); }
    internal bool ShowRecentAtTop;

    #endregion

    internal bool DarkMode => VisualTheme == VisualTheme.Dark;

    internal VisualTheme VisualTheme = VisualTheme.Classic;

    internal bool FollowSystemTheme = true;

    internal bool ShowUnsupported;
    internal bool ShowUnavailableFMs;

    internal bool EnableCharacterDetailFix = true;

    internal bool PlayOriginalSeparateButtons;

    internal ConfirmBeforeInstall ConfirmBeforeInstall = ConfirmBeforeInstall.OnlyForMultiple;

    internal bool AskedToScanForMisCounts;

    internal bool EnableFuzzySearch;

    #region Thief Buddy

    internal RunThiefBuddyOnFMPlay RunThiefBuddyOnFMPlay = RunThiefBuddyOnFMPlay.Ask;

    #endregion

    internal CheckForUpdates CheckForUpdates = CheckForUpdates.FirstTimeAsk;

    private int _screenshotGammaPercent = 50;
    internal int ScreenshotGammaPercent
    {
        get => _screenshotGammaPercent;
        set => _screenshotGammaPercent = value.Clamp(0, 100);
    }

    internal IOThreadsMode IOThreadsMode = IOThreadsMode.Auto;

    private int _customIOThreadCount = CoreCount;
    internal int CustomIOThreadCount
    {
        get => _customIOThreadCount;
        set => _customIOThreadCount = value.ClampToMin(1);
    }

    internal readonly DriveLetterDictionary DriveLettersAndTypes = new(26);

    internal static DriveMultithreadingLevel GetDriveThreadability(DriveLetterDictionary dict, string letter)
    {
        if (letter.Length == 0)
        {
            return DriveMultithreadingLevel.Auto;
        }

        char letterChar = letter[0];
        if (dict.TryGetValue(letterChar, out DriveMultithreadingLevel result))
        {
            return result;
        }
        else
        {
            dict[letterChar] = DriveMultithreadingLevel.Auto;
            return DriveMultithreadingLevel.Auto;
        }
    }

#if !ReleaseBeta && !ReleasePublic
    // Quick-n-dirty session-only var for now
    internal bool ForceWindowed;
    // @FM_CFG: Make this properly customizable if we want to add it to the public release
    internal bool ForceGameResToMainMonitorRes = true;
    //internal bool CheckForUpdatesOnStartup = true;
#endif
}

#define FenGen_ConfigSource

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.DataClasses
{
    // TODO: @FenGen: Get rid of WinForms type and other unsavory type stuff in here
    internal sealed class ConfigData
    {
        internal ConfigData()
        {
            // Automatically set the correct length based on our actual supported game count
            GameExes = new string[SupportedGameCount];
            GamePaths = new string[SupportedGameCount];
            FMInstallPaths = new string[SupportedGameCount];

            GameEditorDetected = new bool[SupportedGameCount];
            UseSteamSwitches = new bool[SupportedGameCount];
            StartupFMSelectorLines = new List<string>[SupportedGameCount];
            StartupAlwaysStartSelector = new bool[SupportedGameCount];

            // We want them empty strings, not null, for safety
            for (int i = 0; i < SupportedGameCount; i++)
            {
                // bool[]s are initialized to false by default, so in that case we don't need to do anything here

                GameExes[i] = "";
                GamePaths[i] = "";
                FMInstallPaths[i] = "";

                UseSteamSwitches[i] = true;
                StartupFMSelectorLines[i] = new List<string>();
            }
        }

        #region Saved-on-startup loader config values

        #region fm_selector lines

        private readonly List<string>[] StartupFMSelectorLines;

        internal List<string> GetStartupFMSelectorLines(GameIndex index) => StartupFMSelectorLines[(uint)index];

        internal void SetStartupFMSelectorLines(GameIndex index, List<string> value) => StartupFMSelectorLines[(uint)index] = value;

        #endregion

        #region "Always start selector" values

        private readonly bool[] StartupAlwaysStartSelector;

        internal bool GetStartupAlwaysStartSelector(GameIndex index) => StartupAlwaysStartSelector[(uint)index];

        internal void SetStartupAlwaysStartSelector(GameIndex index, bool value) => StartupAlwaysStartSelector[(uint)index] = value;

        #endregion

        #endregion

        #region Paths

        internal readonly List<string> FMArchivePaths = new List<string>();
        internal bool FMArchivePathsIncludeSubfolders = false;

        // TODO: Backup path is currently required. Notes on potentially making it optional:
        // -We would need to add a guard check before attempting to either back up or restore an FM. We'd put up
        //  a dialog telling the user they need to specify a backup path first, and let them click a button to go
        //  to the Settings window backup path field.
        // -DarkLoader import needs to know the backup path (when importing saves). We'd have to ask the user here
        //  too.
        // -Due to backup/restore FM being a dead-common operation and one of the main purposes of AngelLoader
        //  even, allowing the backup path to be optional seems of questionable utility. However, we do allow
        //  all other fields to be optional even though leaving them ALL blank is basically nonsensical, and
        //  having backup path be the lone required field feels arbitrary. But then, allowing the user to not
        //  worry about it but then slapping them with a "hey, go set this!" message as soon as they try to
        //  install or uninstall something might be the more annoying thing.
        internal string FMsBackupPath = "";

        #region Game exes

        internal readonly string[] GameExes;

        internal string GetGameExe(GameIndex index) => GameExes[(uint)index];

        /// <summary>
        /// This may throw if <paramref name="game"/> can't convert to a <see cref="GameIndex"/>. Do a guard check first!
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal string GetGameExeUnsafe(Game game) => GameExes[(uint)GameToGameIndex(game)];

        internal void SetGameExe(GameIndex index, string value) => GameExes[(uint)index] = value;

        #endregion

        #region Game exe paths

        private readonly string[] GamePaths;

        internal string GetGamePath(GameIndex index) => GamePaths[(uint)index];

        internal void SetGamePath(GameIndex index, string value) => GamePaths[(uint)index] = value;

        #endregion

        #region FM install paths

        private readonly string[] FMInstallPaths;

        internal string GetFMInstallPath(GameIndex index) => FMInstallPaths[(uint)index];

        /// <summary>
        /// This may throw if <paramref name="game"/> can't convert to a <see cref="GameIndex"/>. Do a guard check first!
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal string GetFMInstallPathUnsafe(Game game) => FMInstallPaths[(uint)GameToGameIndex(game)];

        internal void SetFMInstallPath(GameIndex index, string value) => FMInstallPaths[(uint)index] = value;

        #endregion

        #region Game editor detected

        // Session-only; don't write these out

        private readonly bool[] GameEditorDetected;

        /// <summary>
        /// This may throw if <paramref name="game"/> can't convert to a <see cref="GameIndex"/>. Do a guard check first!
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal bool GetGameEditorDetectedUnsafe(Game game) => GameEditorDetected[(uint)GameToGameIndex(game)];

        internal void SetGameEditorDetected(GameIndex index, bool value) => GameEditorDetected[(uint)index] = value;

        #endregion

        // If a Steam exe is specified, that is
        internal bool LaunchGamesWithSteam = true;

        #region Use Steam switches

        private readonly bool[] UseSteamSwitches;

        internal bool GetUseSteamSwitch(GameIndex index) => UseSteamSwitches[(uint)index];

        internal void SetUseSteamSwitch(GameIndex index, bool value) => UseSteamSwitches[(uint)index] = value;

        #endregion

        internal string SteamExe = "";

        // @GENGAMES (ConfigData - Miscellaneous game-specific stuff): Begin

        // New for T2 NewDark 1.27: Multiplayer support (beta, and T2 only)
        internal bool T2MPDetected = false;

        internal bool T3UseCentralSaves = false;

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
            Filter.Clear();
            GameTabsState.ClearAllFilters();
        }

        internal GameOrganization GameOrganization = GameOrganization.OneList;
        internal GameIndex GameTab = GameIndex.Thief1;

        internal readonly SelectedFM SelFM = new SelectedFM();

        internal readonly GameTabsState GameTabsState = new GameTabsState();

        #endregion

        #region Filtering

        internal readonly Filter Filter = new Filter();

        #endregion

        #region Columns and sorting

        internal readonly List<ColumnData> Columns = new List<ColumnData>(13);
        internal Column SortedColumn = Column.Title;
        internal SortOrder SortDirection = SortOrder.Ascending;

        private float _fMsListFontSizeInPoints = Defaults.FMsListFontSizeInPoints;
        internal float FMsListFontSizeInPoints
        {
            get => _fMsListFontSizeInPoints;
            set
            {
                float val = value;
                if (val < Math.Round(1.00f, 2)) val = 1.00f;
                if (val > Math.Round(41.25f, 2)) val = 41.25f;
                val = (float)Math.Round(val, 2);
                _fMsListFontSizeInPoints = val;
            }
        }

        internal bool EnableArticles = true;
        internal readonly List<string> Articles = new List<string> { "a", "an", "the" };
        internal bool MoveArticlesToEnd = true;

        #endregion

        internal string Language = "English";

        // Session-only; don't write out
        internal readonly Dictionary<string, string> LanguageNames = new Dictionary<string, string>();

        #region Date format

        internal DateFormat DateFormat = DateFormat.CurrentCultureShort;
        // Clunky, but removes the need for parsing
        internal string DateCustomFormat1 = "";
        internal string DateCustomSeparator1 = "";
        internal string DateCustomFormat2 = "";
        internal string DateCustomSeparator2 = "";
        internal string DateCustomFormat3 = "";
        internal string DateCustomSeparator3 = "";
        internal string DateCustomFormat4 = "";
        // Session-only; don't write out
        internal string DateCustomFormatString = "";

        #endregion

        #region Main window state

        private FormWindowState _mainWindowState = FormWindowState.Maximized;
        internal FormWindowState MainWindowState { get => _mainWindowState; set => _mainWindowState = value == FormWindowState.Minimized ? FormWindowState.Maximized : value; }
        internal Size MainWindowSize = new Size(Defaults.MainWindowWidth, Defaults.MainWindowHeight);
        internal Point MainWindowLocation = new Point(Defaults.MainWindowX, Defaults.MainWindowY);

        private float _mainSplitterPercent = Defaults.MainSplitterPercent;
        internal float MainSplitterPercent { get => _mainSplitterPercent; set => _mainSplitterPercent = value.Clamp(0, 1.0f); }

        private float _topSplitterPercent = Defaults.TopSplitterPercent;
        internal float TopSplitterPercent { get => _topSplitterPercent; set => _topSplitterPercent = value.Clamp(0, 1.0f); }

        internal bool TopRightPanelCollapsed = false;

        internal readonly TopRightTabsData TopRightTabsData = new TopRightTabsData();

        #endregion

        #region Settings window state

        internal SettingsTab SettingsTab = SettingsTab.Paths;
        internal Size SettingsWindowSize = new Size(Defaults.SettingsWindowWidth, Defaults.SettingsWindowHeight);
        internal int SettingsWindowSplitterDistance = Defaults.SettingsWindowSplitterDistance;
        internal int SettingsPathsVScrollPos = 0;
        internal int SettingsFMDisplayVScrollPos = 0;
        internal int SettingsOtherVScrollPos = 0;

        #endregion

        #region Readme box

        private float _readmeZoomFactor = 1;
        internal float ReadmeZoomFactor { get => _readmeZoomFactor; set => _readmeZoomFactor = value.Clamp(0.1f, 5.0f); }
        internal bool ReadmeUseFixedWidthFont = true;

        #endregion

        #region Rating display style

        internal RatingDisplayStyle RatingDisplayStyle = RatingDisplayStyle.FMSel;
        internal bool RatingUseStars = true;

        #endregion

        #region Audio conversion

        internal bool ConvertWAVsTo16BitOnInstall = true;
        internal bool ConvertOGGsToWAVsOnInstall = false;

        #endregion

        #region Uninstall

        internal bool ConfirmUninstall = true;

        internal BackupFMData BackupFMData = BackupFMData.AllChangedFiles;
        internal bool BackupAlwaysAsk = true;

        #endregion

        internal string WebSearchUrl = Defaults.WebSearchUrl;

        internal bool ConfirmPlayOnDCOrEnter = true;

        #region Show/hide UI elements

        internal bool HideUninstallButton = false;
        internal bool HideFMListZoomButtons = false;

        #endregion

        internal bool UseShortGameTabNames = false;

        private uint _daysRecent = Defaults.DaysRecent;
        internal uint DaysRecent { get => _daysRecent; set => _daysRecent = value.Clamp((uint)0, Defaults.MaxDaysRecent); }
        internal bool ShowRecentAtTop = false;

        //internal readonly List<ConfigVar> CustomConfigVars = new List<ConfigVar>();

#if !ReleaseBeta && !ReleasePublic
        // TODO: Quick-n-dirty session-only var for now
        internal bool ForceWindowed = false;
        internal bool CheckForUpdatesOnStartup = true;
#endif
    }
}

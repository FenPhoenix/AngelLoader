using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.GameSupport;

namespace AngelLoader.Common.DataClasses
{
    internal sealed class ConfigData
    {
        internal ConfigData()
        {
            // Automatically set the correct length based on our actual supported game count
            GameExes = new string[SupportedGameCount];
            FMInstallPaths = new string[SupportedGameCount];

            // We want them empty strings, not null, for safety
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameExes[i] = "";
                FMInstallPaths[i] = "";
            }
        }

        #region Paths

        internal readonly List<string> FMArchivePaths = new List<string>();
        internal bool FMArchivePathsIncludeSubfolders = false;
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

        #region FM install paths

        internal readonly string[] FMInstallPaths;

        internal string GetFMInstallPath(GameIndex index) => FMInstallPaths[(uint)index];

        /// <summary>
        /// This may throw if <paramref name="game"/> can't convert to a <see cref="GameIndex"/>. Do a guard check first!
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal string GetFMInstallPathUnsafe(Game game) => FMInstallPaths[(uint)GameToGameIndex(game)];

        internal void SetFMInstallPath(GameIndex index, string value) => FMInstallPaths[(uint)index] = value;

        #endregion

        // If a Steam exe is specified, that is
        internal bool LaunchGamesWithSteam = true;
        internal bool T1UseSteam = true;
        internal bool T2UseSteam = true;
        internal bool T3UseSteam = true;
        internal bool SS2UseSteam = true;

        internal string SteamExe = "";

        // Session-only; don't write these out
        internal bool T1DromEdDetected;
        internal bool T2DromEdDetected;
        internal bool SS2ShockEdDetected;
        // New for T2 NewDark 1.27: Multiplayer support (beta, and T2 only)
        internal bool T2MPDetected;

        internal bool T3UseCentralSaves = false;

        #endregion

        #region Selected FMs

        internal void ClearAllSelectedFMs()
        {
            SelFM.Clear();
            GameTabsState.ClearSelectedFMs();
        }

        internal void ClearAllFilters()
        {
            Filter.Clear();
            GameTabsState.T1Filter.Clear();
            GameTabsState.T2Filter.Clear();
            GameTabsState.T3Filter.Clear();
            GameTabsState.SS2Filter.Clear();
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

        internal readonly List<ColumnData> Columns = new List<ColumnData>();
        internal Column SortedColumn = Column.Title;
        internal SortOrder SortDirection = SortOrder.Ascending;

        private float _fMsListFontSizeInPoints = 8.25f;
        internal float FMsListFontSizeInPoints
        {
            get => _fMsListFontSizeInPoints;
            set
            {
                var val = value;
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

        #region Settings window state

        internal SettingsTab SettingsTab = SettingsTab.Paths;
        internal Size SettingsWindowSize = new Size(710, 708);
        internal int SettingsWindowSplitterDistance = 155;
        internal int SettingsPathsVScrollPos = 0;
        internal int SettingsFMDisplayVScrollPos = 0;
        internal int SettingsOtherVScrollPos = 0;

        #endregion

        #region Date format

        internal DateFormat DateFormat = DateFormat.CurrentCultureShort;
        // Clunky, but removes the need for parsing
        internal string DateCustomFormat1;
        internal string DateCustomSeparator1;
        internal string DateCustomFormat2;
        internal string DateCustomSeparator2;
        internal string DateCustomFormat3;
        internal string DateCustomSeparator3;
        internal string DateCustomFormat4;
        // Session-only; don't write out
        internal string DateCustomFormatString;

        #endregion

        #region Main window state

        internal FormWindowState MainWindowState = FormWindowState.Maximized;
        internal Size MainWindowSize = new Size(1280, 720);
        internal Point MainWindowLocation = new Point(100, 100);

        private float _mainSplitterPercent = Defaults.MainSplitterPercent;
        internal float MainSplitterPercent { get => _mainSplitterPercent; set => _mainSplitterPercent = value.Clamp(0, 1.0f); }

        private float _topSplitterPercent = Defaults.TopSplitterPercent;
        internal float TopSplitterPercent { get => _topSplitterPercent; set => _topSplitterPercent = value.Clamp(0, 1.0f); }

        internal bool TopRightPanelCollapsed = false;

        internal readonly TopRightTabsData TopRightTabsData = new TopRightTabsData();

        #endregion

        #region Readme box

        private float _readmeZoomFactor = 1;
        internal float ReadmeZoomFactor { get => _readmeZoomFactor; set => _readmeZoomFactor = value.Clamp(0.1f, 5.0f); }
        internal bool ReadmeUseFixedWidthFont = false;

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

        //internal readonly List<ConfigVar> CustomConfigVars = new List<ConfigVar>();
    }
}

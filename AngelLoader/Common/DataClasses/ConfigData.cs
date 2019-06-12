using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Common.Utility;

namespace AngelLoader.Common.DataClasses
{
    internal sealed class ConfigData
    {
        #region Paths

        internal readonly List<string> FMArchivePaths = new List<string>();
        internal bool FMArchivePathsIncludeSubfolders = false;
        internal string FMsBackupPath = null;

        internal string T1Exe = null;
        internal string T2Exe = null;
        internal string T3Exe = null;

        // Session-only; don't write these out
        internal string T1FMInstallPath = null;
        internal string T2FMInstallPath = null;
        internal string T3FMInstallPath = null;
        internal bool T1DromEdDetected;
        internal bool T2DromEdDetected;

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
        }

        internal GameOrganization GameOrganization = GameOrganization.OneList;
        internal Game GameTab = Game.Thief1;

        internal SelectedFM SelFM = new SelectedFM();

        internal GameTabsState GameTabsState = new GameTabsState();

        #endregion

        #region Filtering

        internal Filter Filter = new Filter();

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

        internal TopRightTabsData TopRightTabsData = new TopRightTabsData();

        internal float _readmeZoomFactor = 1;
        internal float ReadmeZoomFactor { get => _readmeZoomFactor; set => _readmeZoomFactor = value.Clamp(0.1f, 5.0f); }

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

        internal readonly List<ConfigVar> CustomConfigVars = new List<ConfigVar>();
    }
}

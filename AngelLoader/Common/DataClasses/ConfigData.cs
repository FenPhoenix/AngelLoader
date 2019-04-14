using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Logger;

namespace AngelLoader.Common.DataClasses
{
    internal sealed class ConfigData
    {
        #region Paths

        internal List<string> FMArchivePaths = new List<string>();
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

        internal List<ColumnData> Columns = new List<ColumnData>();
        internal Column SortedColumn = Column.Title;
        internal SortOrder SortDirection = SortOrder.Ascending;
        internal bool EnableArticles = true;
        internal readonly List<string> Articles = new List<string> { "a", "an", "the" };
        internal bool MoveArticlesToEnd = true;

        #endregion

        internal string Language = "English";

        // Session-only; don't write out
        internal Dictionary<string, string> LanguageNames = new Dictionary<string, string>();

        internal SettingsTab SettingsTab = SettingsTab.Paths;

        internal TopRightTab TopRightTab = TopRightTab.Statistics;

        internal TopRightTabOrder TopRightTabOrder = new TopRightTabOrder();

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

        internal FormWindowState MainWindowState = FormWindowState.Maximized;
        internal Size MainWindowSize = new Size(1280, 720);
        internal Point MainWindowLocation = new Point(100, 100);

        private float _mainSplitterPercent = Defaults.MainSplitterPercent;
        internal float MainSplitterPercent
        {
            get => _mainSplitterPercent;
            set => _mainSplitterPercent = value.Clamp(0, 1.0f);
        }

        private float _topSplitterPercent = Defaults.TopSplitterPercent;
        internal float TopSplitterPercent
        {
            get => _topSplitterPercent;
            set => _topSplitterPercent = value.Clamp(0, 1.0f);
        }

        internal bool TopRightPanelCollapsed = false;

        internal RatingDisplayStyle RatingDisplayStyle = RatingDisplayStyle.FMSel;
        // Only relevant if we're using FMSel-style rating display
        internal bool RatingUseStars = true;

        internal bool ConvertWAVsTo16BitOnInstall = true;
        internal bool ConvertOGGsToWAVsOnInstall = false;

        internal BackupFMData BackupFMData = BackupFMData.AllChangedFiles;
        internal bool BackupAlwaysAsk = true;

        internal string WebSearchUrl = Defaults.WebSearchUrl;

        internal bool ConfirmPlayOnDCOrEnter = true;

        internal float _readmeZoomFactor = 1;
        internal float ReadmeZoomFactor
        {
            get
            {
                Log("Config.ReadmeZoomFactor.get: " + _readmeZoomFactor, methodName: false);
                return _readmeZoomFactor;
            }
            set
            {
                Log("Config.ReadmeZoomFactor.set in: " + value, methodName: false);
                _readmeZoomFactor = value.Clamp(0.1f, 5.0f);
                Log("Config.ReadmeZoomFactor.set final: " + _readmeZoomFactor, methodName: false);
            }
        }
    }
}

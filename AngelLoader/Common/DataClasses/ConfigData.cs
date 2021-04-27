#define FenGen_ConfigSource

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using static AL_Common.CommonUtils;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;

namespace AngelLoader.DataClasses
{
    internal sealed class ConfigData
    {
        internal ConfigData()
        {
            GameExes = new string[SupportedGameCount];
            GamePaths = new string[SupportedGameCount];
            FMInstallPaths = new string[SupportedGameCount];

            // Leave all false
            GameEditorDetected = new bool[SupportedGameCount];

            UseSteamSwitches = new bool[SupportedGameCount];
            StartupFMSelectorLines = new List<string>[SupportedGameCount];

            // Leave all false
            StartupAlwaysStartSelector = new bool[SupportedGameCount];

            FilterControlVisibilities = InitializedArray(HideableFilterControlsCount, true);

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

        internal int Version = 1;

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

        #region Get special exes

        /*
         I'm not completely comfortable putting these in here... They're sort of in a no-man's land between
         belonging here or belonging outside.
         Arguments for being in here:
         -They depend on the game paths which are in here
         -They conceptually go with the other Get*Exe() methods
         Arguments for being outside:
         -The actual filenames at the end of their paths are constants from the static paths class
         -They access the file system, which feels a bit weird for a config object to do. I feel like the methods
          in here should all be "instant" and operate on memory only and not do weird things you don't expect.
          (that's the main reason for my discomfort really)

         I'm just going to append "_FromDisk" to their names, document them, and call it serviceable.
        */

        /// <summary>
        /// Returns the full path of the editor for <paramref name="game"/> if and only if it exists on disk.
        /// Otherwise, returns the empty string. It will also return the empty string if <paramref name="game"/>
        /// is not Dark.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal string GetEditorExe_FromDisk(GameIndex game)
        {
            string gamePath;
            if (!GameIsDark(game) || (gamePath = GetGamePath(game)).IsEmpty()) return "";

            string exe = game == GameIndex.SS2 ? Paths.ShockEdExe : Paths.DromEdExe;
            return TryCombineFilePathAndCheckExistence(gamePath, exe, out string fullPathExe)
                ? fullPathExe
                : "";
        }

        /// <summary>
        /// Returns the full path of Thief2MP.exe if any only if it exists on disk in the same directory as the
        /// specified Thief 2 executable. Otherwise, returns the empty string.
        /// </summary>
        /// <returns></returns>
        internal string GetT2MultiplayerExe_FromDisk()
        {
            string gamePath = GetGamePath(GameIndex.Thief2);
            return !gamePath.IsEmpty() && TryCombineFilePathAndCheckExistence(gamePath, Paths.T2MPExe, out string fullPathExe)
                ? fullPathExe
                : "";
        }

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

        #region Steam

        // If a Steam exe is specified, that is
        internal bool LaunchGamesWithSteam = true;

        internal string SteamExe = "";

        private readonly bool[] UseSteamSwitches;

        internal bool GetUseSteamSwitch(GameIndex index) => UseSteamSwitches[(uint)index];

        internal void SetUseSteamSwitch(GameIndex index, bool value) => UseSteamSwitches[(uint)index] = value;

        #endregion

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
            Filter.ClearAll();
            GameTabsState.ClearAllFilters();
        }

        internal GameOrganization GameOrganization = GameOrganization.OneList;
        internal GameIndex GameTab = GameIndex.Thief1;

        internal readonly SelectedFM SelFM = new SelectedFM();

        internal readonly GameTabsState GameTabsState = new GameTabsState();

        #endregion

        #region Filtering

        internal readonly Filter Filter = new Filter();

        internal readonly bool[] FilterControlVisibilities;

        #endregion

        #region Columns and sorting

        internal readonly List<ColumnData> Columns = new List<ColumnData>(ColumnsCount);
        internal Column SortedColumn = Column.Title;
        internal SortOrder SortDirection = SortOrder.Ascending;

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
        internal readonly List<string> Articles = new List<string> { "a", "an", "the" };
        internal bool MoveArticlesToEnd = true;

        #endregion

        #region Language

        internal string Language = "English";

        // Session-only; don't write out
        internal readonly Dictionary<string, string> LanguageNames = new Dictionary<string, string>();

        #endregion

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
        internal float MainSplitterPercent { get => _mainSplitterPercent; set => _mainSplitterPercent = value.ClampZeroToOne(); }

        private float _topSplitterPercent = Defaults.TopSplitterPercent;
        internal float TopSplitterPercent { get => _topSplitterPercent; set => _topSplitterPercent = value.ClampZeroToOne(); }

        internal bool TopRightPanelCollapsed = false;

        internal readonly TopRightTabsData TopRightTabsData = new TopRightTabsData();

        #endregion

        #region Settings window state

        internal SettingsTab SettingsTab = SettingsTab.Paths;
        internal Size SettingsWindowSize = new Size(Defaults.SettingsWindowWidth, Defaults.SettingsWindowHeight);
        internal int SettingsWindowSplitterDistance = Defaults.SettingsWindowSplitterDistance;
        internal int SettingsPathsVScrollPos = 0;
        internal int SettingsAppearanceVScrollPos = 0;
        internal int SettingsFMsListVScrollPos = 0;
        internal int SettingsOtherVScrollPos = 0;

        #endregion

        #region Readme box

        private float _readmeZoomFactor = 1;
        internal float ReadmeZoomFactor { get => _readmeZoomFactor; set => _readmeZoomFactor = value.ClampToRichTextBoxZoomMinMax(); }
        internal bool ReadmeUseFixedWidthFont = true;
        internal RTFColorStyle RTFThemedColorStyle = RTFColorStyle.Auto;

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
        internal bool HideExitButton = true;

        #endregion

        internal bool UseShortGameTabNames = false;

        #region Recent FMs

        private uint _daysRecent = Defaults.DaysRecent;
        internal uint DaysRecent { get => _daysRecent; set => _daysRecent = value.Clamp((uint)0, Defaults.MaxDaysRecent); }
        internal bool ShowRecentAtTop = false;

        #endregion

        internal bool DarkMode => VisualTheme == VisualTheme.Dark;

        internal VisualTheme VisualTheme = VisualTheme.Classic;

        internal bool ShowUnsupported = false;
        internal bool ShowUnavailableFMs = false;

#if !ReleaseBeta && !ReleasePublic
        // Quick-n-dirty session-only var for now
        internal bool ForceWindowed = false;
        //internal bool CheckForUpdatesOnStartup = true;
#endif
    }
}

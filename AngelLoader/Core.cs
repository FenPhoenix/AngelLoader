/* NOTE: Core ideas:
 -We could have the stub be called back on game exit and use that to track game lifetime, for temp config var changes
  But note we may have to handle no_unload_fmsel option - make sure we don't have stale values on SelectFM call?
 -@IO_SAFETY: Make a system where files get temp-copied and then if writes fail, we copy the old file back (FMSel does this)
  For FMData.ini this will be more complicated because we rewrite it a lot (whenever values change on the UI) so
  if we want to keep multiple backups (and we probably should) then we want to avoid blowing out our backup cache
  every time we write
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using AngelLoader.WinAPI;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class Core
    {
        // TODO: Core: View = null!; note
        // Remove this null-handwave and get null notification on this so we don't accidentally access it when
        // it's null. But if we check it from another thread there'll be a race condition. Figure something out?
        internal static IView View = null!;

        internal static async void Init(Task configTask)
        {
            bool openSettings = false;
            // This is if we have no config file; in that case we assume we're starting for the first time ever
            bool cleanStart = false;

            List<int>? fmsViewListUnscanned = null;

            try
            {
                #region Create required directories

                try
                {
                    Directory.CreateDirectory(Paths.Data);
                    Directory.CreateDirectory(Paths.Languages);
                }
                catch (Exception ex)
                {
                    // ReSharper disable once ConvertToConstant.Local
                    string message = "Failed to create required application directories on startup.";
                    Log(message, ex);
                    // We're not even close to having a theme at this point, so just use regular MessageBox
                    MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                #endregion

                #region Read config file if it exists

                if (File.Exists(Paths.ConfigIni))
                {
                    try
                    {
                        Ini.ReadConfigIni(Paths.ConfigIni, Config);
                    }
                    catch (Exception ex)
                    {
                        string message = Paths.ConfigIni + " exists but there was an error while reading it.";
                        Log(message, ex);
                        openSettings = true;
                    }
                }
                else
                {
                    openSettings = true;
                    // We're starting for the first time ever (assumed)
                    cleanStart = true;
                }

                #endregion
            }
            finally
            {
                configTask.Wait();
                configTask.Dispose();
            }

            // We can't show the splash screen until we know our theme, which we have to get from the config
            // file, so we can't show it any earlier than this.
            using var splashScreen = new SplashScreen();

            static void ReadLanguages(SplashScreen splashScreen)
            {
                // We can't show a message until we've read the config file (to know which language to use) and
                // the current language file (to get the translated message strings). So just show language dir/
                // language file names, so it's as clear as possible what we're doing without actually having to
                // display a translated string.
                splashScreen.SetMessage(Paths.Languages);

                // Have to read langs here because which language to use will be stored in the config file.
                // Gather all lang files in preparation to read their LanguageName= value so we can get the lang's
                // name in its own language
                var langFiles = FastIO.GetFilesTopOnly(Paths.Languages, "*.ini");
                bool selFound = false;

                // Do it ONCE here, not every loop!
                Config.LanguageNames.Clear();

                for (int i = 0; i < langFiles.Count; i++)
                {
                    string f = langFiles[i];
                    string fn = f.GetFileNameFast().RemoveExtension();

                    splashScreen.SetMessage(f);

                    if (!selFound && fn.EqualsI(Config.Language))
                    {
                        try
                        {
                            LText = Ini.ReadLocalizationIni(f);
                            selFound = true;
                        }
                        catch (Exception ex)
                        {
                            Log("Error while reading " + f + ".", ex);
                        }
                    }
                    Ini.AddLanguageFromFile(f, Config.LanguageNames);
                }
            }

            if (!openSettings)
            {
                splashScreen.Show(Config.VisualTheme);

                ReadLanguages(splashScreen);

                splashScreen.SetMessage(LText.SplashScreen.CheckingRequiredSettingsFields);

                openSettings = !Directory.Exists(Config.FMsBackupPath);
            }
            // Don't show the splash screen on first start, because it looks awkward
            else
            {
                ReadLanguages(splashScreen);
            }

            splashScreen.SetMessage(LText.SplashScreen.ReadingGameConfigFiles);

            #region Set game data

            bool[] gameExeExists = new bool[SupportedGameCount];
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                // Existence checks on startup are merely a perf optimization: values start blank so just don't
                // set them if we don't have a game exe
                string gameExe = Config.GetGameExe(gameIndex);
                gameExeExists[i] = !gameExe.IsEmpty() && File.Exists(gameExe);
                if (gameExeExists[i])
                {
                    SetGameDataFromDisk(gameIndex, storeConfigInfo: true);
                    if (gameIndex is Thief1 or Thief2) GameConfigFiles.FixCharacterDetailLine(Config.GetGamePath(gameIndex));
                }
            }

            #endregion

            Task DoParallelLoad()
            {
                splashScreen.SetMessage(LText.SplashScreen.SearchingForNewFMs + Environment.NewLine +
                                        LText.SplashScreen.LoadingMainApp);
                // Set beforehand to avoid the cross-thread font access problem
                splashScreen.SetCheckMessageWidth(LText.SplashScreen.SearchingForNewFMs);
                // IMPORTANT: Begin no-splash-screen-call zone
                // The FM finder will update the splash screen from another thread (accessing only the graphics
                // context, so no cross-thread Control access exceptions), so any calls in here are potential
                // race conditions.
                using (Task findFMsTask = Task.Run(() => fmsViewListUnscanned = FindFMs.Find_Startup(splashScreen)))
                {
                    // Construct and init the view both right here, because they're both heavy operations and
                    // we want them both to run in parallel with Find() to the greatest extent possible.
                    View = new MainForm();
                    View.InitThreadable();

                    findFMsTask.Wait();
                }
                // IMPORTANT: End no-splash-screen-call zone

                return View.FinishInitAndShow(fmsViewListUnscanned!);
            }

            if (!openSettings)
            {
                await DoParallelLoad();
            }
            else
            {
                splashScreen.Hide();
                if (!OpenSettings(startup: true, cleanStart: cleanStart).Canceled)
                {
                    splashScreen.Show(Config.VisualTheme);
                    await DoParallelLoad();
                }
            }
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        // We return bools signifying which awaitable tasks to run, so we can avoid this method having to be async
        public static (bool Canceled, List<int>? FMsViewListUnscanned, bool SortAndSetFilter, bool KeepSel)
        OpenSettings(bool startup = false, bool cleanStart = false)
        {
            DialogResult result;
            ConfigData outConfig;
            using (var sf = new SettingsForm(View, Config, startup, cleanStart))
            {
                result = sf.ShowDialogDark();
                outConfig = sf.OutConfig;
            }

            // This needs to be separate so the below "always-save" stuff can work

            #region Save window state

            // Special case: these are meta, so they should always be set even if the user clicked Cancel
            Config.SettingsTab = outConfig.SettingsTab;
            Config.SettingsWindowSize = outConfig.SettingsWindowSize;
            Config.SettingsWindowSplitterDistance = outConfig.SettingsWindowSplitterDistance;

            Config.SettingsPathsVScrollPos = outConfig.SettingsPathsVScrollPos;
            Config.SettingsAppearanceVScrollPos = outConfig.SettingsAppearanceVScrollPos;
            Config.SettingsOtherVScrollPos = outConfig.SettingsOtherVScrollPos;

            #endregion

            if (result != DialogResult.OK)
            {
                // Since nothing of consequence has yet happened, it's okay to do the brutal quit
                // We know the game paths by now, so we can do this
                if (startup) EnvironmentExitDoShutdownTasks(0);
                return (true, null, false, false);
            }

            #region Set changed bools

            bool archivePathsChanged =
                !startup &&
                (!Config.FMArchivePaths.PathSequenceEqualI_Dir(outConfig.FMArchivePaths) ||
                 Config.FMArchivePathsIncludeSubfolders != outConfig.FMArchivePathsIncludeSubfolders);

            bool gamePathsChanged =
                !startup &&
                !Config.GameExes.PathSequenceEqualI(outConfig.GameExes);

            // We need these in order to decide which, if any, startup config infos to re-read
            bool[] individualGamePathsChanged = new bool[SupportedGameCount];

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                individualGamePathsChanged[i] =
                    !startup &&
                    !Config.GetGameExe(gameIndex).PathEqualsI(outConfig.GetGameExe(gameIndex));
            }

            bool gameOrganizationChanged =
                !startup && Config.GameOrganization != outConfig.GameOrganization;

            bool useShortGameTabNamesChanged =
                !startup && Config.UseShortGameTabNames != outConfig.UseShortGameTabNames;

            bool articlesChanged =
                !startup &&
                (Config.EnableArticles != outConfig.EnableArticles ||
                 !Config.Articles.SequenceEqual(outConfig.Articles, StringComparer.InvariantCultureIgnoreCase) ||
                 Config.MoveArticlesToEnd != outConfig.MoveArticlesToEnd);

            bool ratingDisplayStyleChanged =
                !startup &&
                (Config.RatingDisplayStyle != outConfig.RatingDisplayStyle ||
                 Config.RatingUseStars != outConfig.RatingUseStars);

            bool dateFormatChanged =
                !startup &&
                (Config.DateFormat != outConfig.DateFormat ||
                 Config.DateCustomFormatString != outConfig.DateCustomFormatString);

            bool daysRecentChanged =
                !startup && Config.DaysRecent != outConfig.DaysRecent;

            bool languageChanged =
                !startup && !Config.Language.EqualsI(outConfig.Language);

            bool useFixedFontChanged =
                !startup && Config.ReadmeUseFixedWidthFont != outConfig.ReadmeUseFixedWidthFont;

            #endregion

            #region Set config data

            // Set values individually (rather than deep-copying) so that non-Settings values don't get overwritten.

            #region Paths tab

            #region Game exes

            // Do this BEFORE copying game exes to Config, because we need the Config game exes to still point to
            // the old ones.
            if (gamePathsChanged) GameConfigFiles.ResetGameConfigTempChanges(individualGamePathsChanged);

            // Game paths should have been checked and verified before OK was clicked, so assume they're good here
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;

                // This must be done first!
                Config.SetGameExe(gameIndex, outConfig.GetGameExe(gameIndex));

                // Set it regardless of game existing, because we want to blank the data
                SetGameDataFromDisk(gameIndex, storeConfigInfo: startup || individualGamePathsChanged[i]);

                Config.SetUseSteamSwitch(gameIndex, outConfig.GetUseSteamSwitch(gameIndex));
            }

            #endregion

            Config.SteamExe = outConfig.SteamExe;
            Config.LaunchGamesWithSteam = outConfig.LaunchGamesWithSteam;

            Config.FMsBackupPath = outConfig.FMsBackupPath;

            Config.FMArchivePaths.ClearAndAdd(outConfig.FMArchivePaths);

            Config.FMArchivePathsIncludeSubfolders = outConfig.FMArchivePathsIncludeSubfolders;

            #endregion

            List<int>? fmsViewListUnscanned = null;

            if (startup)
            {
                Config.Language = outConfig.Language;

                // We don't need to set the paths again, because we've already done so above

                Ini.WriteConfigIni();

                return (false, null, false, false);
            }

            // From this point on, we're not in startup mode.

            // For clarity, don't copy the other tabs' data on startup, because their tabs won't be shown and so
            // they won't have been changed

            #region FM Display tab

            Config.VisualTheme = outConfig.VisualTheme;

            Config.GameOrganization = outConfig.GameOrganization;
            Config.UseShortGameTabNames = outConfig.UseShortGameTabNames;

            Config.EnableArticles = outConfig.EnableArticles;
            Config.Articles.ClearAndAdd(outConfig.Articles);

            Config.MoveArticlesToEnd = outConfig.MoveArticlesToEnd;

            Config.RatingDisplayStyle = outConfig.RatingDisplayStyle;
            Config.RatingUseStars = outConfig.RatingUseStars;

            Config.DateFormat = outConfig.DateFormat;
            Config.DateCustomFormat1 = outConfig.DateCustomFormat1;
            Config.DateCustomSeparator1 = outConfig.DateCustomSeparator1;
            Config.DateCustomFormat2 = outConfig.DateCustomFormat2;
            Config.DateCustomSeparator2 = outConfig.DateCustomSeparator2;
            Config.DateCustomFormat3 = outConfig.DateCustomFormat3;
            Config.DateCustomSeparator3 = outConfig.DateCustomSeparator3;
            Config.DateCustomFormat4 = outConfig.DateCustomFormat4;
            Config.DateCustomFormatString = outConfig.DateCustomFormatString;

            Config.DaysRecent = outConfig.DaysRecent;

            #endregion

            #region Other tab

            Config.ConvertWAVsTo16BitOnInstall = outConfig.ConvertWAVsTo16BitOnInstall;
            Config.ConvertOGGsToWAVsOnInstall = outConfig.ConvertOGGsToWAVsOnInstall;

            Config.ConfirmUninstall = outConfig.ConfirmUninstall;

            Config.BackupFMData = outConfig.BackupFMData;
            Config.BackupAlwaysAsk = outConfig.BackupAlwaysAsk;

            Config.Language = outConfig.Language;

            Config.WebSearchUrl = outConfig.WebSearchUrl;

            Config.ConfirmPlayOnDCOrEnter = outConfig.ConfirmPlayOnDCOrEnter;

            Config.HideUninstallButton = outConfig.HideUninstallButton;
            Config.HideFMListZoomButtons = outConfig.HideFMListZoomButtons;
            Config.HideExitButton = outConfig.HideExitButton;

            Config.ReadmeUseFixedWidthFont = outConfig.ReadmeUseFixedWidthFont;

            #endregion

            // These ones MUST NOT be set on startup, because the source values won't be valid
            Config.SortedColumn = View.GetCurrentSortedColumnIndex();
            Config.SortDirection = View.GetCurrentSortDirection();

            #endregion

            #region Change-specific actions (pre-refresh)

            View.ShowInstallUninstallButton(!Config.HideUninstallButton);
            View.ShowFMsListZoomButtons(!Config.HideFMListZoomButtons);
            View.ShowExitButton(!Config.HideExitButton);

            if (archivePathsChanged || gamePathsChanged)
            {
                fmsViewListUnscanned = FindFMs.Find();
            }
            if (gameOrganizationChanged)
            {
                // Clear everything to defaults so we don't have any leftover state screwing things all up
                Config.ClearAllSelectedFMs();
                Config.ClearAllFilters();
                Config.GameTab = Thief1;
                View.ClearUIAndCurrentInternalFilter();
                if (Config.GameOrganization == GameOrganization.ByTab) Config.Filter.Games = Game.Thief1;
            }
            if (gamePathsChanged || gameOrganizationChanged)
            {
                View.ChangeGameOrganization();
            }
            if (useShortGameTabNamesChanged)
            {
                View.ChangeGameTabNameShortness(Config.UseShortGameTabNames, refreshFilterBarPositionIfNeeded: true);
            }
            if (ratingDisplayStyleChanged)
            {
                View.UpdateRatingDisplayStyle(Config.RatingDisplayStyle, startup: false);
            }
            if (useFixedFontChanged)
            {
                View.ChangeReadmeBoxFont(Config.ReadmeUseFixedWidthFont);
            }

            #endregion

            #region Return bools for appropriate refresh method (if applicable)

            var ret = (Canceled: false, FMsViewListUnscanned: (List<int>?)null, SortAndSetFilter: false, KeepSel: false);

            if (archivePathsChanged || gamePathsChanged || gameOrganizationChanged || articlesChanged ||
                daysRecentChanged)
            {
                if (archivePathsChanged || gamePathsChanged)
                {
                    if (fmsViewListUnscanned!.Count > 0)
                    {
                        ret.FMsViewListUnscanned = fmsViewListUnscanned;
                    }

#if !ReleaseBeta && !ReleasePublic
                    if (gamePathsChanged) View.UpdateGameScreenShotModes();
#endif
                }

                ret.KeepSel = !gameOrganizationChanged;
                ret.SortAndSetFilter = true;
            }
            else if (dateFormatChanged || languageChanged)
            {
                View.RefreshFMsListKeepSelection();
            }

            #endregion

            return ret;
        }

        /// <summary>
        /// Sets the per-game config data that we pull directly from the game folders on disk. Game paths,
        /// FM install paths, editors detected, pre-modification cam_mod.ini lines, etc.
        /// </summary>
        /// <param name="gameIndex"></param>
        /// <param name="storeConfigInfo"></param>
        private static void SetGameDataFromDisk(GameIndex gameIndex, bool storeConfigInfo)
        {
            string gameExe = Config.GetGameExe(gameIndex);
            bool gameExeSpecified = !gameExe.IsWhiteSpace();

            string gamePath = "";
            if (gameExeSpecified)
            {
                try
                {
                    gamePath = Path.GetDirectoryName(gameExe) ?? "";
                }
                catch
                {
                    // ignore for now
                }
            }

            // This must come first, so below methods can use it
            Config.SetGamePath(gameIndex, gamePath);
            if (GameIsDark(gameIndex))
            {
                var data = gameExeSpecified
                    ? GameConfigFiles.GetInfoFromCamModIni(gamePath, out Error _)
                    : (
                        FMsPath: "",
                        FMLanguage: "",
                        FMLanguageForced: false,
                        FMSelectorLines: new List<string>(),
                        AlwaysShowLoader: false
                    );

                Config.SetFMInstallPath(gameIndex, data.FMsPath);
                Config.SetGameEditorDetected(gameIndex, gameExeSpecified && !Config.GetEditorExe_FromDisk(gameIndex).IsEmpty());

                if (storeConfigInfo)
                {
                    if (gameExeSpecified)
                    {
                        Config.SetStartupFMSelectorLines(gameIndex, data.FMSelectorLines);
                        Config.SetStartupAlwaysStartSelector(gameIndex, data.AlwaysShowLoader);
                    }
                    else
                    {
                        Config.GetStartupFMSelectorLines(gameIndex).Clear();
                        Config.SetStartupAlwaysStartSelector(gameIndex, false);
                    }
                }

                if (gameIndex == Thief2) Config.T2MPDetected = gameExeSpecified && !Config.GetT2MultiplayerExe_FromDisk().IsEmpty();
            }
            else
            {
                if (gameExeSpecified)
                {
                    var t3Data = GameConfigFiles.GetInfoFromSneakyOptionsIni();
                    if (t3Data.Error == Error.None)
                    {
                        Config.SetFMInstallPath(Thief3, t3Data.FMInstallPath);
                        Config.T3UseCentralSaves = t3Data.UseCentralSaves;
                    }
                    else
                    {
                        Config.SetFMInstallPath(Thief3, "");
                    }
                    // Do this even if there was an error, because we could still have a valid selector line
                    if (storeConfigInfo)
                    {
                        Config.GetStartupFMSelectorLines(Thief3).Clear();
                        if (!t3Data.PrevFMSelectorValue.IsEmpty())
                        {
                            Config.GetStartupFMSelectorLines(Thief3).Add(t3Data.PrevFMSelectorValue);
                        }
                        Config.SetStartupAlwaysStartSelector(Thief3, t3Data.AlwaysShowLoader);
                    }
                }
                else
                {
                    Config.SetFMInstallPath(Thief3, "");
                    if (storeConfigInfo)
                    {
                        Config.GetStartupFMSelectorLines(Thief3).Clear();
                        Config.SetStartupAlwaysStartSelector(Thief3, false);
                    }
                    Config.T3UseCentralSaves = false;
                }
            }
        }

        internal static void SortFMsViewList(Column column, SortOrder sortDirection)
        {
            var comparer = column switch
            {
                Column.Game => Comparers.FMGame,
                Column.Installed => Comparers.FMInstalled,
                Column.Title => Comparers.FMTitle,
                Column.Archive => Comparers.FMArchive,
                Column.Author => Comparers.FMAuthor,
                Column.Size => Comparers.FMSize,
                Column.Rating => Comparers.FMRating,
                Column.Finished => Comparers.FMFinished,
                Column.ReleaseDate => Comparers.FMReleaseDate,
                Column.LastPlayed => Comparers.FMLastPlayed,
                Column.DateAdded => Comparers.FMDateAdded,
                Column.DisabledMods => Comparers.FMDisabledMods,
                Column.Comment => Comparers.FMComment,
                _ => null
            };

            AssertR(comparer != null, nameof(comparer) + "==null: column not being handled");

            comparer!.SortOrder = sortDirection;

            FMsViewList.Sort(comparer);

            var tempFMs = new List<FanMission>();

            if (View.GetShowRecentAtTop())
            {
                // Store it so it doesn't change
                var dtNow = DateTime.Now;

                for (int i = 0; i < FMsViewList.Count; i++)
                {
                    var fm = FMsViewList[i];
                    fm.MarkedRecent = false;

                    if (
                        // Don't mess with the sort order of pinned FMs, because they should be in the same sort
                        // order as the main list but just placed at the top. Whereas the recent FMs will always
                        // be displayed in order of date added.
                        !fm.Pinned &&
                        fm.DateAdded != null &&
                        ((DateTime)fm.DateAdded).ToLocalTime().CompareTo(dtNow) <= 0 &&
                        (dtNow - ((DateTime)fm.DateAdded).ToLocalTime()).TotalDays <= Config.DaysRecent)
                    {
                        tempFMs.Add(fm);
                    }
                }

                Comparers.FMDateAdded.SortOrder = SortOrder.Ascending;
                tempFMs.Sort(Comparers.FMDateAdded);

                for (int i = 0; i < tempFMs.Count; i++)
                {
                    var fm = tempFMs[i];
                    fm.MarkedRecent = true;
                    FMsViewList.Remove(fm);
                    FMsViewList.Insert(0, fm);
                }
            }
            else
            {
                for (int i = 0; i < FMsViewList.Count; i++) FMsViewList[i].MarkedRecent = false;
            }

            #region Pinned

            tempFMs.Clear();

            for (int i = 0; i < FMsViewList.Count; i++)
            {
                var fm = FMsViewList[i];
                if (fm.Pinned) tempFMs.Add(fm);
            }

            tempFMs.Reverse();

            for (int i = 0; i < tempFMs.Count; i++)
            {
                var fm = tempFMs[i];
                FMsViewList.Remove(fm);
                FMsViewList.Insert(0, fm);
            }

            #endregion
        }

        public static async Task RefreshFMsListFromDisk()
        {
            SelectedFM? selFM = View.GetSelectedFMPosInfo();
            using (new DisableEvents(View))
            {
                var fmsViewListUnscanned = FindFMs.Find();
                if (fmsViewListUnscanned.Count > 0) await FMScan.ScanNewFMs(fmsViewListUnscanned);
            }
            await View.SortAndSetFilter(selFM, forceDisplayFM: true);
        }

        // PERF: 0.7~2.2ms with every filter set (including a bunch of tag filters), over 1098 set. But note that
        //       the majority had no tags for this test.
        //       This was tested with the Release_Testing (optimized) profile.
        //       All in all, I'd say performance is looking really good. Certainly better than I was expecting,
        //       given this is a reasonably naive implementation with no real attempt to be clever.
        internal static void SetFilter()
        {
#if DEBUG || (Release_Testing && !RT_StartupOnly)
            View.SetDebug2Text(int.TryParse(View.GetDebug2Text(), out int result) ? (result + 1).ToString() : "1");
#endif

            Filter viewFilter = View.GetFilter();

            #region Set filters that are stored in control state

            viewFilter.Title = View.GetTitleFilter();
            viewFilter.Author = View.GetAuthorFilter();

            bool[] gameFiltersChecked = View.GetGameFiltersEnabledStates();
            viewFilter.Games = Game.Null;
            for (int i = 0; i < SupportedGameCount; i++)
            {
                if (gameFiltersChecked[i]) viewFilter.Games |= GameIndexToGame((GameIndex)i);
            }

            viewFilter.Finished = FinishedState.Null;
            if (View.GetFinishedFilter()) viewFilter.Finished |= FinishedState.Finished;
            if (View.GetUnfinishedFilter()) viewFilter.Finished |= FinishedState.Unfinished;

            #endregion

            var filterShownIndexList = View.GetFilterShownIndexList();

            filterShownIndexList.Clear();

            // These are checked in a loop, so cache them. Others are only checked once, so leave them be.
            bool titleIsWhitespace = viewFilter.Title.IsWhiteSpace();
            string titleTrimmed = viewFilter.Title.Trim();

            // Note: we used to have an early-out here if all filter options were off, but since the filter
            // requires ShowUnsupported to be active to be considered "off", in practice, the early-out would
            // almost never be run. For this reason, and also because it required a janky bool to tell the
            // difference between "filtered index list is empty because all FMs are filtered or because none are",
            // we just always indirect our indexes through the filtered list now even in the rare case where we
            // don't need to.

            #region Title / initial

            for (int i = 0; i < FMsViewList.Count; i++)
            {
                var fm = FMsViewList[i];

                if (fm.MarkedRecent ||
                    fm.Pinned ||
                    titleIsWhitespace ||
                    fm.Title.ContainsI(viewFilter.Title) ||
                    (fm.Archive.ExtIsArchive()
                        ? titleTrimmed.EqualsI(".zip") || titleTrimmed.EqualsI(".7z")
                            ? fm.Archive.EndsWithI(titleTrimmed)
                            : titleTrimmed.EqualsI(fm.Archive) ||
                              fm.Archive.IndexOf(viewFilter.Title, 0, fm.Archive.LastIndexOf('.'), StringComparison.OrdinalIgnoreCase) > -1
                        : fm.Archive.ContainsI(viewFilter.Title)))
                {
                    filterShownIndexList.Add(i);
                }
            }

            #endregion

            #region Author

            if (!viewFilter.Author.IsWhiteSpace())
            {
                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (!fm.MarkedRecent &&
                        !fm.Pinned &&
                        !fm.Author.ContainsI(viewFilter.Author))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Show unsupported

            if (!View.GetShowUnsupportedFilter())
            {
                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (fm.Game == Game.Unsupported)
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Games

            if (viewFilter.Games > Game.Null)
            {
                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (GameIsKnownAndSupported(fm.Game) &&
                        (Config.GameOrganization == GameOrganization.ByTab || !fm.MarkedRecent) &&
                        !viewFilter.Games.HasFlagFast(fm.Game))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Tags

            if (viewFilter.Tags.AndTags.Count > 0 ||
                viewFilter.Tags.OrTags.Count > 0 ||
                viewFilter.Tags.NotTags.Count > 0)
            {
                CatAndTagsList andTags = viewFilter.Tags.AndTags;
                CatAndTagsList orTags = viewFilter.Tags.OrTags;
                CatAndTagsList notTags = viewFilter.Tags.NotTags;

                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];

                    if (fm.MarkedRecent || fm.Pinned) continue;

                    if (fm.Tags.Count == 0 && notTags.Count == 0)
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    // I don't ever want to see these damn things again

                    #region And

                    if (andTags.Count > 0)
                    {
                        bool andPass = true;
                        foreach (CatAndTags andTag in andTags)
                        {
                            CatAndTags? match = fm.Tags.Find(x => x.Category == andTag.Category);
                            if (match == null)
                            {
                                andPass = false;
                                break;
                            }

                            if (andTag.Tags.Count > 0)
                            {
                                foreach (string andTagTag in andTag.Tags)
                                {
                                    if (match.Tags.Find(x => x == andTagTag) == null)
                                    {
                                        andPass = false;
                                        break;
                                    }
                                }

                                if (!andPass) break;
                            }
                        }

                        if (!andPass)
                        {
                            filterShownIndexList.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    #endregion

                    #region Or

                    if (orTags.Count > 0)
                    {
                        bool orPass = false;
                        foreach (CatAndTags orTag in orTags)
                        {
                            CatAndTags? match = fm.Tags.Find(x => x.Category == orTag.Category);
                            if (match == null) continue;

                            if (orTag.Tags.Count > 0)
                            {
                                foreach (string orTagTag in orTag.Tags)
                                {
                                    if (match.Tags.Find(x => x == orTagTag) != null)
                                    {
                                        orPass = true;
                                        break;
                                    }
                                }

                                if (orPass) break;
                            }
                            else
                            {
                                orPass = true;
                            }
                        }

                        if (!orPass)
                        {
                            filterShownIndexList.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    #endregion

                    #region Not

                    if (notTags.Count > 0)
                    {
                        bool notPass = true;
                        foreach (CatAndTags notTag in notTags)
                        {
                            CatAndTags? match = fm.Tags.Find(x => x.Category == notTag.Category);
                            if (match == null) continue;

                            if (notTag.Tags.Count == 0)
                            {
                                notPass = false;
                                continue;
                            }

                            if (notTag.Tags.Count > 0)
                            {
                                foreach (string notTagTag in notTag.Tags)
                                {
                                    if (match.Tags.Find(x => x == notTagTag) != null)
                                    {
                                        notPass = false;
                                        break;
                                    }
                                }

                                if (!notPass) break;
                            }
                        }

                        if (!notPass)
                        {
                            filterShownIndexList.RemoveAt(i);
                            i--;
                            // Explicit continue for safety in case the order of these gets changed or another
                            // gets added
                            // ReSharper disable once RedundantJumpStatement
                            continue;
                        }
                    }

                    #endregion
                }
            }

            #endregion

            #region Rating

            if (!(viewFilter.RatingFrom == -1 && viewFilter.RatingTo == 10))
            {
                int rf = viewFilter.RatingFrom;
                int rt = viewFilter.RatingTo;

                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (!fm.MarkedRecent &&
                        !fm.Pinned &&
                        (fm.Rating < rf || fm.Rating > rt))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Release date

            if (viewFilter.ReleaseDateFrom != null || viewFilter.ReleaseDateTo != null)
            {
                DateTime? rdf = viewFilter.ReleaseDateFrom;
                DateTime? rdt = viewFilter.ReleaseDateTo;

                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (!fm.MarkedRecent &&
                        !fm.Pinned &&
                        (fm.ReleaseDate.DateTime == null ||
                        (rdf != null &&
                         fm.ReleaseDate.DateTime.Value.Date.CompareTo(rdf.Value.Date) < 0) ||
                        (rdt != null &&
                         fm.ReleaseDate.DateTime.Value.Date.CompareTo(rdt.Value.Date) > 0)))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Last played

            if (viewFilter.LastPlayedFrom != null || viewFilter.LastPlayedTo != null)
            {
                DateTime? lpdf = viewFilter.LastPlayedFrom;
                DateTime? lpdt = viewFilter.LastPlayedTo;

                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (!fm.MarkedRecent &&
                        !fm.Pinned &&
                        (fm.LastPlayed.DateTime == null ||
                        (lpdf != null &&
                         fm.LastPlayed.DateTime.Value.Date.CompareTo(lpdf.Value.Date) < 0) ||
                        (lpdt != null &&
                         fm.LastPlayed.DateTime.Value.Date.CompareTo(lpdt.Value.Date) > 0)))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Finished

            if (viewFilter.Finished > FinishedState.Null)
            {
                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    uint fmFinished = fm.FinishedOn;
                    bool fmFinishedOnUnknown = fm.FinishedOnUnknown;

                    if (!fm.MarkedRecent &&
                        !fm.Pinned &&
                        (((fmFinished > 0 || fmFinishedOnUnknown) &&
                          !viewFilter.Finished.HasFlagFast(FinishedState.Finished)) ||
                         (fmFinished == 0 && !fmFinishedOnUnknown &&
                         !viewFilter.Finished.HasFlagFast(FinishedState.Unfinished))))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Marked unavailable

            if (!View.GetShowUnavailableFMsFilter())
            {
                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (fm.MarkedUnavailable)
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion
        }

        #region DML

        internal static bool AddDML(FanMission fm, string sourceDMLPath)
        {
            AssertR(GameIsDark(fm.Game), nameof(AddDML) + ": " + nameof(fm) + " is not Dark");

            if (!FMIsReallyInstalled(fm))
            {
                LogFMInstDirError(fm, "FM install directory not found.");
                Dialogs.ShowError(LText.AlertMessages.Patch_AddDML_InstallDirNotFound);
                return false;
            }

            string installedFMPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);
            try
            {
                string dmlFile = Path.GetFileName(sourceDMLPath);
                if (dmlFile.IsEmpty()) return false;
                File.Copy(sourceDMLPath, Path.Combine(installedFMPath, dmlFile), overwrite: true);
            }
            catch (Exception ex)
            {
                LogFMInstDirError(fm, "Unable to add .dml to installed folder.", ex);
                Dialogs.ShowError(LText.AlertMessages.Patch_AddDML_UnableToAdd);
                return false;
            }

            return true;
        }

        internal static bool RemoveDML(FanMission fm, string dmlFile)
        {
            AssertR(GameIsDark(fm.Game), nameof(RemoveDML) + ": " + nameof(fm) + " is not Dark");

            if (!FMIsReallyInstalled(fm))
            {
                LogFMInstDirError(fm, "FM install directory not found.");
                Dialogs.ShowError(LText.AlertMessages.Patch_RemoveDML_InstallDirNotFound);
                return false;
            }

            string installedFMPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);
            try
            {
                File.Delete(Path.Combine(installedFMPath, dmlFile));
            }
            catch (Exception ex)
            {
                LogFMInstDirError(fm, "Unable to remove .dml from installed folder.", ex);
                Dialogs.ShowError(LText.AlertMessages.Patch_RemoveDML_UnableToRemove);
                return false;
            }

            return true;
        }

        internal static (bool Success, List<string> DMLFiles)
        GetDMLFiles(FanMission fm)
        {
            AssertR(GameIsDark(fm.Game), nameof(GetDMLFiles) + ": " + nameof(fm) + " is not Dark");

            try
            {
                var dmlFiles = FastIO.GetFilesTopOnly(
                    Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir), "*.dml",
                    returnFullPaths: false);
                return (true, dmlFiles);
            }
            catch (Exception ex)
            {
                Log("Exception getting DML files for " + fm.InstalledDir + ", game: " + fm.Game, ex);
                return (false, new List<string>());
            }
        }

        #endregion

        #region Readme

        private static string GetReadmeFileFullPath(FanMission fm) =>
            FMIsReallyInstalled(fm)
                ? Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir, fm.SelectedReadme)
                : Path.Combine(Paths.FMsCache, fm.InstalledDir, fm.SelectedReadme);

        internal static (string ReadmePath, ReadmeType ReadmeType)
        GetReadmeFileAndType(FanMission fm)
        {
            string readmeOnDisk = GetReadmeFileFullPath(fm);

            if (fm.SelectedReadme.ExtIsHtml()) return (readmeOnDisk, ReadmeType.HTML);
            if (fm.SelectedReadme.ExtIsGlml()) return (readmeOnDisk, ReadmeType.GLML);

            int headerLen = RTFHeaderBytes.Length;

            byte[] buffer = new byte[headerLen];

            // This might throw, but all calls to this method are supposed to be wrapped in a try-catch block
            using (var fs = File.OpenRead(readmeOnDisk))
            {
                // Fix: In theory, the readme could be less than headerLen bytes long and then we would throw and
                // end up with an "unable to load readme" error.
                if (fs.Length >= headerLen)
                {
                    using var br = new BinaryReader(fs, Encoding.ASCII);
                    buffer = br.ReadBytes(headerLen);
                }
            }

            var rType = buffer.SequenceEqual(RTFHeaderBytes) ? ReadmeType.RichText : ReadmeType.PlainText;

            return (readmeOnDisk, rType);
        }

        /// <summary>
        /// Given a list of readme filenames, attempts to find one that doesn't contain spoilers by "eyeballing"
        /// the list of names similarly to how a human would to determine the same thing.
        /// </summary>
        /// <param name="readmeFiles"></param>
        /// <param name="fmTitle"></param>
        /// <returns></returns>
        internal static string DetectSafeReadme(List<string> readmeFiles, string fmTitle)
        {
            // Since an FM's readmes are very few in number, we can afford to be all kinds of lazy and slow here

            #region Local functions

            static string StripPunctuation(string str) => str
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(";", "")
                .Replace("'", "");

            static string FirstByPreferredFormat(List<string> files)
            {
                // Don't use IsValidReadme(), because we want a specific search order
                foreach (string x in files) if (x.ExtIsGlml()) return x;
                foreach (string x in files) if (x.ExtIsRtf()) return x;
                foreach (string x in files) if (x.ExtIsTxt()) return x;
                foreach (string x in files) if (x.ExtIsWri()) return x;
                foreach (string x in files) if (x.ExtIsHtml()) return x;
                return "";
            }

            static bool ContainsUnsafePhrase(string str) =>
                str.ContainsI("loot") ||
                str.ContainsI("walkthrough") ||
                str.ContainsI("walkthru") ||
                str.ContainsI("secret") ||
                str.ContainsI("spoiler") ||
                str.ContainsI("tips") ||
                str.ContainsI("convo") ||
                str.ContainsI("conversation") ||
                str.ContainsI("cheat") ||
                str.ContainsI("notes");

            static bool ContainsUnsafeOrJunkPhrase(string str) =>
                ContainsUnsafePhrase(str) ||
                str.EqualsI("scripts") ||
                str.ContainsI("copyright") ||
                str.ContainsI("install") ||
                str.ContainsI("update") ||
                str.ContainsI("patch") ||
                str.ContainsI("nvscript") ||
                str.ContainsI("tnhscript") ||
                str.ContainsI("GayleSaver") ||
                str.ContainsI("changelog") ||
                str.ContainsI("changes") ||
                str.ContainsI("credits") ||
                str.ContainsI("objectives") ||
                str.ContainsI("hint");

            #endregion

            bool allEqual = true;
            for (int i = 0; i < readmeFiles.Count; i++)
            {
                if (i > 0 && !StripPunctuation(Path.GetFileNameWithoutExtension(readmeFiles[i]))
                    .EqualsI(StripPunctuation(Path.GetFileNameWithoutExtension(readmeFiles[i - 1]))))
                {
                    allEqual = false;
                    break;
                }
            }

            string safeReadme = "";
            if (allEqual)
            {
                safeReadme = FirstByPreferredFormat(readmeFiles);
            }
            else
            {
                // Because we allow arbitrary languages, it's theoretically possible to get one that doesn't have
                // a language code.
                bool langCodeExists = FMLanguages.LangCodes.TryGetValue(Config.Language, out string langCode);
                langCode ??= "";
                bool altLangCodeExists = FMLanguages.AltLangCodes.TryGetValue(langCode, out string altLangCode);
                altLangCode ??= "";

                var safeReadmes = new List<string>();
                foreach (string rf in readmeFiles)
                {
                    string fn_orig = Path.GetFileNameWithoutExtension(rf);
                    string fn = StripPunctuation(fn_orig);

                    // Original English-favoring section (keeping this in causes no harm)
                    if (fn.EqualsI("Readme") || fn.EqualsI("ReadmeEn") || fn.EqualsI("ReadmeEng") ||
                        fn.EqualsI("FMInfo") || fn.EqualsI("FMInfoEn") || fn.EqualsI("FMInfoEng") ||
                        fn.EqualsI("fm") || fn.EqualsI("fmEn") || fn.EqualsI("fmEng") ||
                        fn.EqualsI("GameInfo") || fn.EqualsI("GameInfoEn") || fn.EqualsI("GameInfoEng") ||
                        fn.EqualsI("Mission") || fn.EqualsI("MissionEn") || fn.EqualsI("MissionEng") ||
                        fn.EqualsI("MissionInfo") || fn.EqualsI("MissionInfoEn") || fn.EqualsI("MissionInfoEng") ||
                        fn.EqualsI("Info") || fn.EqualsI("InfoEn") || fn.EqualsI("InfoEng") ||
                        fn.EqualsI("Entry") || fn.EqualsI("EntryEn") || fn.EqualsI("EntryEng") ||
                        fn.EqualsI("English") ||
                        // end original English-favoring section
                        (langCodeExists &&
                         !ContainsUnsafeOrJunkPhrase(fn) &&
                         (fn_orig.EndsWithI("_" + langCode) ||
                          fn_orig.EndsWithI("-" + langCode) ||
                          (altLangCodeExists &&
                           (fn_orig.EndsWithI("_" + altLangCode) ||
                            fn_orig.EndsWithI("-" + altLangCode))))) ||
                        (fn.StartsWithI(StripPunctuation(fmTitle)) && !ContainsUnsafeOrJunkPhrase(fn)) ||
                        (fn.EndsWithI("Readme") && !ContainsUnsafePhrase(fn)))
                    {
                        safeReadmes.Add(rf);
                    }
                }

                if (safeReadmes.Count > 0)
                {
                    safeReadmes.Sort(Comparers.FileNameNoExt);

                    foreach (string item in new[] { "readme", "fminfo", "fm", "gameinfo", "mission", "missioninfo", "info", "entry" })
                    {
                        foreach (string sr in safeReadmes)
                        {
                            if (Path.GetFileNameWithoutExtension(sr).EqualsI(item))
                            {
                                safeReadmes.Remove(sr);
                                safeReadmes.Insert(0, sr);
                                break;
                            }
                        }
                    }
                    foreach (string sr in safeReadmes)
                    {
                        string srNoExt = Path.GetFileNameWithoutExtension(sr);
                        if (langCodeExists &&
                            (srNoExt.EndsWithI("_" + langCode) ||
                             srNoExt.EndsWithI("-" + langCode) ||
                             (altLangCodeExists &&
                              (srNoExt.EndsWithI("_" + altLangCode) ||
                               srNoExt.EndsWithI("-" + altLangCode)))))
                        {
                            safeReadmes.Remove(sr);
                            safeReadmes.Insert(0, sr);
                            break;
                        }
                    }
                    safeReadme = FirstByPreferredFormat(safeReadmes);
                }
            }

            if (safeReadme.IsEmpty())
            {
                int numSafe = 0;
                int safeIndex = -1;
                for (int i = 0; i < readmeFiles.Count; i++)
                {
                    string rf = readmeFiles[i];

                    string fn = StripPunctuation(Path.GetFileNameWithoutExtension(rf));
                    if (!ContainsUnsafeOrJunkPhrase(fn))
                    {
                        numSafe++;
                        safeIndex = i;
                    }
                }

                if (numSafe == 1 && safeIndex > -1) safeReadme = readmeFiles[safeIndex];
            }

            return safeReadme;
        }

        #endregion

        #region Open / run

        internal static void OpenFMFolder(FanMission fm)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("fm is not known or supported. FM game type: " + fm.Game, stackTrace: true);
                Dialogs.ShowError(ErrorText.UnableToOpenFMFolder);
                return;
            }

            string installsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
            string fmDir;
            if (installsBasePath.IsEmpty() || !Directory.Exists(fmDir = Path.Combine(installsBasePath, fm.InstalledDir)))
            {
                LogFMInstDirError(fm, "FM install directory not found.");
                Dialogs.ShowError(LText.AlertMessages.Patch_FMFolderNotFound);
                return;
            }

            try
            {
                ProcessStart_UseShellExecute(fmDir);
            }
            catch (Exception ex)
            {
                Log("Exception trying to open FM folder " + fmDir, ex);
                Dialogs.ShowError(ErrorText.UnableToOpenFMFolder);
            }
        }

        internal static void OpenWebSearchUrl(string fmTitle)
        {
            static bool CheckUrl(string url)
            {
                if (url.IsWhiteSpace())
                {
                    Log(nameof(url) + " consists only of whitespace.");
                    Dialogs.ShowError("Web search URL (as set in the Settings window) is empty or consists only of whitespace. Unable to create a valid link.");
                    return false;
                }

                if (url.Length > 32766)
                {
                    Log(nameof(url) + " is too long (>32766 chars).");
                    Dialogs.ShowError("Web search URL (as set in the Settings window) is too long. Unable to create a valid link.");
                    return false;
                }

                return true;
            }

            string url = Config.WebSearchUrl;

            if (!CheckUrl(url)) return;

            // Possible exceptions are:
            // ArgumentNullException (stringToEscape is null)
            // UriFormatException (The length of stringToEscape exceeds 32766 characters)
            // Those are both checked for above so we're good.
            url = Uri.EscapeUriString(url);

            if (!CheckUrl(url)) return;

            int index = url.IndexOf("$TITLE$", StringComparison.OrdinalIgnoreCase);

            string finalUrl;

            try
            {
                finalUrl = index == -1
                    ? url
                    : url.Substring(0, index) + Uri.EscapeDataString(fmTitle) + url.Substring(index + "$TITLE$".Length);
            }
            catch (Exception ex)
            {
                Log("Problem opening web search URL", ex);
                Dialogs.ShowError(LText.AlertMessages.WebSearchURL_ProblemOpening);
                return;
            }

            try
            {
                ProcessStart_UseShellExecute(finalUrl);
            }
            catch (FileNotFoundException ex)
            {
                Log("\"The PATH environment variable has a string containing quotes.\" (that's what MS docs says?!)", ex);
                Dialogs.ShowError(LText.AlertMessages.WebSearchURL_ProblemOpening);
            }
            catch (Win32Exception ex)
            {
                Log("Problem opening web search URL", ex);
                Dialogs.ShowError(LText.AlertMessages.WebSearchURL_ProblemOpening);
            }
        }

        internal static void ViewHTMLReadme(FanMission fm)
        {
            string path;
            try
            {
                path = GetReadmeFileFullPath(fm);
            }
            catch (Exception ex)
            {
                Log("Exception opening HTML readme " + fm.SelectedReadme, ex);
                Dialogs.ShowError(ErrorText.UnableToOpenHTMLReadme);
                return;
            }

            if (File.Exists(path))
            {
                try
                {
                    ProcessStart_UseShellExecute(path);
                }
                catch (Exception ex)
                {
                    Log("Exception opening HTML readme " + path, ex);
                    Dialogs.ShowError(ErrorText.UnableToOpenHTMLReadme);
                }
            }
            else
            {
                Log("File not found: " + path, stackTrace: true);
                Dialogs.ShowError(path + "\r\n\r\n" + ErrorText.HTMLReadmeNotFound);
            }
        }

        internal static void OpenHelpFile(string section = "")
        {
            /*
             We want to go directly to the relevant section of the manual, but Process.Start() won't let us open
             a file URL with an anchor tag stuck on the end. We could try to detect the user's default browser
             and start it directly with the passed file URL and that would work, but finding the default browser
             appears to be of dubious reliability and I wouldn't trust it to be future proof as far as I could
             throw it. So we just do this crappy hack where we make a temp file that just redirects to our anchor-
             postfixed URL and then open that with Process.Start(). We get auto-navigated to our section and there
             you go.
            */

            Paths.CreateOrClearTempPath(Paths.HelpTemp);

            if (!File.Exists(Paths.DocFile))
            {
                Log("Help file not found: " + Paths.DocFile);
                Dialogs.ShowError(LText.AlertMessages.Help_HelpFileNotFound + "\r\n\r\n" + Paths.DocFile);
                return;
            }

            string finalUri;
            if (section.IsEmpty())
            {
                finalUri = Paths.DocFile;
            }
            else
            {
                string helpFileUri = "file://" + Paths.DocFile;
                try
                {
                    File.WriteAllText(Paths.HelpRedirectFilePath, @"<meta http-equiv=""refresh"" content=""0; URL=" + helpFileUri + section + @""" />");
                    finalUri = Paths.HelpRedirectFilePath;
                }
                catch (Exception ex)
                {
                    // @BetterErrors(OpenHelpFile redirect file write failure): This one isn't important enough to put a dialog
                    Log("Exception writing temp help redirect file. Using un-anchored path (help file will be positioned at top, not at requested section)...", ex);
                    finalUri = helpFileUri;
                }
            }

            try
            {
                ProcessStart_UseShellExecute(finalUri);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(ProcessStart_UseShellExecute) + ". Couldn't open help file.", ex);
                Dialogs.ShowError(LText.AlertMessages.Help_UnableToOpenHelpFile);
            }
        }

        internal static void OpenLink(string link, bool fixUpEmailLinks = false)
        {
            try
            {
                // The RichTextBox may send a link that's supposed to be an email but without the "mailto:" prefix,
                // so use a crappy heuristic to add it if necessary.
                if (fixUpEmailLinks && !link.StartsWithI("mailto:") && link.CountCharsUpToAmount('@', 2) == 1)
                {
                    int atIndex = link.IndexOf('@');
                    if (link.IndexOf(':', 0, atIndex) == -1 &&
                        link.IndexOf('.', atIndex) > 0)
                    {
                        link = "mailto:" + link;
                    }
                }

                ProcessStart_UseShellExecute(link);
            }
            catch (Exception ex)
            {
                Log("Problem opening link '" + link + "'", ex);
                Dialogs.ShowError(ErrorText.UnableToOpenLink + "\r\n\r\n" + link);
            }
        }

        #endregion

        internal static (Error Error, string Version)
        GetGameVersion(GameIndex game)
        {
            string gameExe = Config.GetGameExe(game);
            if (gameExe.IsWhiteSpace()) return (Error.GameExeNotSpecified, "");
            if (!File.Exists(gameExe)) return (Error.GameExeNotFound, "");

            string exeToSearch;
            if (GameIsDark(game))
            {
                exeToSearch = gameExe;
            }
            else
            {
                // TODO: If Sneaky.dll not found, just use the version from specified exe and don't say "Sneaky Upgrade" before it
                if (!TryCombineFilePathAndCheckExistence(Config.GetGamePath(Thief3), Paths.SneakyDll, out exeToSearch))
                {
                    return (Error.SneakyDllNotFound, "");
                }
            }

            FileVersionInfo vi;
            try
            {
                vi = FileVersionInfo.GetVersionInfo(exeToSearch);
            }
            catch (FileNotFoundException)
            {
                return (GameIsDark(game) ? Error.GameExeNotFound : Error.SneakyDllNotFound, "");
            }

            return vi.ProductVersion.IsEmpty() ? (Error.GameVersionNotFound, "") : (Error.None, vi.ProductVersion);
        }

        internal static void UpdateFMReadmeCodePages(FanMission fm, int codePage)
        {
            fm.ReadmeCodePages[fm.SelectedReadme] = codePage;
            fm.ReadmeAndCodePageEntries.Clear();
            foreach (var item in fm.ReadmeCodePages)
            {
                fm.ReadmeAndCodePageEntries.Add(item.Key + "," + item.Value);
            }
        }

        #region Shutdown

        internal static void UpdateConfig(
            FormWindowState mainWindowState,
            Size mainWindowSize,
            Point mainWindowLocation,
            float mainSplitterPercent,
            float topSplitterPercent,
            List<ColumnData> columns,
            Column sortedColumn,
            SortOrder sortDirection,
            float fmsListFontSizeInPoints,
            Filter filter,
            bool[] gameFilterControlVisibilities,
            bool[] filterControlVisibilities,
            SelectedFM selectedFM,
            GameTabsState gameTabsState,
            GameIndex gameTab,
            TopRightTabsData topRightTabsData,
            bool topRightPanelCollapsed,
            float readmeZoomFactor)
        {
            #region Main window state

            Config.MainWindowState = mainWindowState;
            Config.MainWindowSize = mainWindowSize;
            Config.MainWindowLocation = mainWindowLocation;
            Config.MainSplitterPercent = mainSplitterPercent;
            Config.TopSplitterPercent = topSplitterPercent;

            #endregion

            #region FMs list

            Config.Columns.ClearAndAdd(columns);

            Config.SortedColumn = sortedColumn;
            Config.SortDirection = sortDirection;

            Config.ShowRecentAtTop = View.GetShowRecentAtTop();

            Config.FMsListFontSizeInPoints = fmsListFontSizeInPoints;

            #endregion

            Array.Copy(gameFilterControlVisibilities, Config.GameFilterControlVisibilities, SupportedGameCount);
            Array.Copy(filterControlVisibilities, Config.FilterControlVisibilities, HideableFilterControlsCount);

            filter.DeepCopyTo(Config.Filter);

            #region Top-right panel

            Config.TopRightTabsData.SelectedTab = topRightTabsData.SelectedTab;

            for (int i = 0; i < TopRightTabsData.Count; i++)
            {
                Config.TopRightTabsData.Tabs[i].DisplayIndex = topRightTabsData.Tabs[i].DisplayIndex;
                Config.TopRightTabsData.Tabs[i].Visible = topRightTabsData.Tabs[i].Visible;
            }

            Config.TopRightTabsData.EnsureValidity();

            Config.TopRightPanelCollapsed = topRightPanelCollapsed;

            #endregion

            #region Selected FM and game tab state

            switch (Config.GameOrganization)
            {
                case GameOrganization.OneList:
                    Config.ClearAllSelectedFMs();
                    selectedFM.DeepCopyTo(Config.SelFM);
                    Config.GameTab = Thief1;
                    break;

                case GameOrganization.ByTab:
                    Config.SelFM.Clear();
                    gameTabsState.DeepCopyTo(Config.GameTabsState);
                    Config.GameTab = gameTab;
                    break;
            }

            #endregion

            Config.ReadmeZoomFactor = readmeZoomFactor;
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        internal static void EnvironmentExitDoShutdownTasks(int exitCode)
        {
            DoShutdownTasks();
            Environment.Exit(exitCode);
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        private static void DoShutdownTasks()
        {
            // Currently just this, but we may want to add other things later
            GameConfigFiles.ResetGameConfigTempChanges();
        }

        internal static void Shutdown()
        {
            Ini.WriteConfigIni();
            Ini.WriteFullFMDataIni();

            DoShutdownTasks();

            Application.Exit();
        }

        #endregion
    }
}

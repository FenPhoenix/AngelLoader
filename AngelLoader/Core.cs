// TODO: Idea: We could have the stub be called back on game exit and use that to track game lifetime, for temp config var changes
// But note we may have to handle no_unload_fmsel option - make sure we don't have stale values on SelectFM call?
// TODO: @IO_SAFETY: Make a system where files get temp-copied and then if writes fail, we copy the old file back (FMSel does this)
// For FMData.ini this will be more complicated because we rewrite it a lot (whenever values change on the UI) so
// if we want to keep multiple backups (and we probably should) then we want to avoid blowing out our backup cache
// every time we write
// TODO: @Robustness: Move away from the "hide errors, fail silently, I'm scared to know" and towards failing clearly and fast with dialogs
// This will need a lot of extra localization strings. So also put more comments in the lang files. To do this,
// finish implementing the custom FenGen code that can read actual comments to make it easier for me to comment
// the fields.
// TODO: Maybe delete the stub comm file on exit, but:
// Don't do it for Steam because Steam could start without running a game and/or give the user time to maybe exit
// AngelLoader and then the FM wouldn't load. Also it may be too aggressive in general, but it's an idea.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using AngelLoader.WinAPI;
using Microsoft.VisualBasic.FileIO; // the import of shame
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class Core
    {
        // TODO: Remove these pragmas and get null notification on this so we don't accidentally access it when it's null.
        // TODO: But if we check it from another thread there'll be a race condition. Use a locking construct of some kind.
#pragma warning disable CS8618
        internal static IView View;
#pragma warning restore CS8618

        // Stupid hack for perf and nice UX when deleting FMs (we filter out deleted ones until the next find from
        // disk, when we remove them properly)
        internal static bool OneOrMoreFMsAreMarkedDeleted;

        internal static void Init(Task configTask)
        {
            bool openSettings;
            // This is if we have no config file; in that case we assume we're starting for the first time ever
            bool cleanStart = false;
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
                    const string message = "Failed to create required application directories on startup.";
                    Log(message, ex);
                    MessageBox.Show(message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
                }

                #endregion

                #region Read config file if it exists

                if (File.Exists(Paths.ConfigIni))
                {
                    try
                    {
                        Ini.ReadConfigIni(Paths.ConfigIni, Config);

                        #region Set paths

                        // PERF: 9ms, but it's mostly IO. Darn.
                        bool[] gameExeExists = new bool[SupportedGameCount];
                        for (int i = 0; i < SupportedGameCount; i++)
                        {
                            // Existence checks on startup are merely a perf optimization: values start blank so
                            // just don't set them if we don't have a game exe
                            string gameExe = Config.GetGameExe((GameIndex)i);
                            gameExeExists[i] = !gameExe.IsEmpty() && File.Exists(gameExe);
                            if (gameExeExists[i]) SetGameDataFromDisk((GameIndex)i, storeConfigInfo: true);
                        }

                        Error error =
                            // Must be first, otherwise other stuff overrides it and then we don't act on it
                            !Directory.Exists(Config.FMsBackupPath) ? Error.BackupPathNotSpecified :
                            gameExeExists.All(x => false) ? Error.NoGamesSpecified :
                            Error.None;

                        #endregion

                        openSettings = error == Error.BackupPathNotSpecified;
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

                #region Read languages

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
                    if (!selFound && fn.EqualsI(Config.Language))
                    {
                        try
                        {
                            LText = Ini.ReadLocalizationIni(f);
                            selFound = true;
                        }
                        catch (Exception ex)
                        {
                            Log("There was an error while reading " + f + ".", ex);
                        }
                    }
                    Ini.AddLanguageFromFile(f, Config.LanguageNames);

                    // These need to be set after language read. Slightly awkward but oh well.
                    //SetDefaultConfigVarNamesToLocalized();
                }

                #endregion

                if (!openSettings)
                {
                    #region Parallel load

                    using Task findFMsTask = Task.Run(() => FindFMs.Find(startup: true));

                    // It's safe to overlap this with Find(), but not with MainForm.ctor()
                    configTask.Wait();

                    // Construct and init the view both right here, because they're both heavy operations and we
                    // want them both to run in parallel with Find() to the greatest extent possible.
                    View = new MainForm();
                    View.InitThreadable();

                    findFMsTask.Wait();

                    #endregion
                }
                else
                {
                    // Don't forget to wait in this case too
                    configTask.Wait();
                }
            }
            finally
            {
                configTask.Dispose();
            }

            if (openSettings)
            {
                // Eliding await so that we don't have to be async and run the state machine and lose time. This
                // will be the last line run in this method and nothing does anything up the call stack, so it's
                // safe. Don't put this inside a try block, or it won't be safe. It has to really be the last
                // thing run in the method.
#pragma warning disable 4014
                OpenSettings(startup: true, cleanStart: cleanStart);
#pragma warning restore 4014
                // ReSharper disable once RedundantJumpStatement
                return; // return for clarity of intent
            }
            else
            {
                // async, but same as above
                // View won't be null here
                View.FinishInitAndShow();
            }
        }

        // @CAN_RUN_BEFORE_VIEW_INIT
        public static async Task OpenSettings(bool startup = false, bool cleanStart = false)
        {
            using var sf = new SettingsForm(View, Config, startup, cleanStart);

            // This needs to be separate so the below "always-save" stuff can work
            var result = sf.ShowDialog();

            #region Save window state

            // Special case: these are meta, so they should always be set even if the user clicked Cancel
            Config.SettingsTab = sf.OutConfig.SettingsTab;
            Config.SettingsWindowSize = sf.OutConfig.SettingsWindowSize;
            Config.SettingsWindowSplitterDistance = sf.OutConfig.SettingsWindowSplitterDistance;

            Config.SettingsPathsVScrollPos = sf.OutConfig.SettingsPathsVScrollPos;
            Config.SettingsFMDisplayVScrollPos = sf.OutConfig.SettingsFMDisplayVScrollPos;
            Config.SettingsOtherVScrollPos = sf.OutConfig.SettingsOtherVScrollPos;

            #endregion

            if (result != DialogResult.OK)
            {
                // Since nothing of consequence has yet happened, it's okay to do the brutal quit
                // We know the game paths by now, so we can do this
                if (startup) EnvironmentExitDoShutdownTasks(0);
                return;
            }

            #region Set changed bools

            bool archivePathsChanged =
                !startup &&
                (!Config.FMArchivePaths.PathSequenceEqualI_Dir(sf.OutConfig.FMArchivePaths) ||
                 Config.FMArchivePathsIncludeSubfolders != sf.OutConfig.FMArchivePathsIncludeSubfolders);

            bool gamePathsChanged =
                !startup &&
                !Config.GameExes.PathSequenceEqualI(sf.OutConfig.GameExes);

            // We need these in order to decide which, if any, startup config infos to re-read
            bool[] individualGamePathsChanged = new bool[SupportedGameCount];

            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                individualGamePathsChanged[i] =
                    !startup &&
                    !Config.GetGameExe(gameIndex).PathEqualsI(sf.OutConfig.GetGameExe(gameIndex));
            }

            bool gameOrganizationChanged =
                !startup && Config.GameOrganization != sf.OutConfig.GameOrganization;

            bool useShortGameTabNamesChanged =
                !startup && Config.UseShortGameTabNames != sf.OutConfig.UseShortGameTabNames;

            bool articlesChanged =
                !startup &&
                (Config.EnableArticles != sf.OutConfig.EnableArticles ||
                 !Config.Articles.SequenceEqual(sf.OutConfig.Articles, StringComparer.InvariantCultureIgnoreCase) ||
                 Config.MoveArticlesToEnd != sf.OutConfig.MoveArticlesToEnd);

            bool ratingDisplayStyleChanged =
                !startup &&
                (Config.RatingDisplayStyle != sf.OutConfig.RatingDisplayStyle ||
                 Config.RatingUseStars != sf.OutConfig.RatingUseStars);

            bool dateFormatChanged =
                !startup &&
                (Config.DateFormat != sf.OutConfig.DateFormat ||
                 Config.DateCustomFormatString != sf.OutConfig.DateCustomFormatString);

            bool daysRecentChanged =
                !startup && Config.DaysRecent != sf.OutConfig.DaysRecent;

            bool languageChanged =
                !startup && !Config.Language.EqualsI(sf.OutConfig.Language);

            bool useFixedFontChanged =
                !startup && Config.ReadmeUseFixedWidthFont != sf.OutConfig.ReadmeUseFixedWidthFont;

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
                Config.SetGameExe(gameIndex, sf.OutConfig.GetGameExe(gameIndex));

                // Set it regardless of game existing, because we want to blank the data
                SetGameDataFromDisk(gameIndex, storeConfigInfo: startup || individualGamePathsChanged[i]);

                Config.SetUseSteamSwitch(gameIndex, sf.OutConfig.GetUseSteamSwitch(gameIndex));
            }

            #endregion

            Config.SteamExe = sf.OutConfig.SteamExe;
            Config.LaunchGamesWithSteam = sf.OutConfig.LaunchGamesWithSteam;

            Config.FMsBackupPath = sf.OutConfig.FMsBackupPath;

            Config.FMArchivePaths.ClearAndAdd(sf.OutConfig.FMArchivePaths);

            Config.FMArchivePathsIncludeSubfolders = sf.OutConfig.FMArchivePathsIncludeSubfolders;

            #endregion

            if (startup)
            {
                Config.Language = sf.OutConfig.Language;

                // We don't need to set the paths again, because we've already done so above

                Ini.WriteConfigIni();

                // We have to do this here because we won't have before
                using (Task findFMsTask = Task.Run(() => FindFMs.Find(startup: true)))
                {
                    // Have to do the full View init sequence here, because we skipped them all before
                    View = new MainForm();
                    View.InitThreadable();

                    findFMsTask.Wait();
                }
                // Again, last line and nothing up the call stack, so call without await.
#pragma warning disable 4014
                View.FinishInitAndShow();
#pragma warning restore 4014

                return;
            }

            // From this point on, we're not in startup mode.

            // For clarity, don't copy the other tabs' data on startup, because their tabs won't be shown and so
            // they won't have been changed

            #region FM Display tab

            Config.GameOrganization = sf.OutConfig.GameOrganization;
            Config.UseShortGameTabNames = sf.OutConfig.UseShortGameTabNames;

            Config.EnableArticles = sf.OutConfig.EnableArticles;
            Config.Articles.ClearAndAdd(sf.OutConfig.Articles);

            Config.MoveArticlesToEnd = sf.OutConfig.MoveArticlesToEnd;

            Config.RatingDisplayStyle = sf.OutConfig.RatingDisplayStyle;
            Config.RatingUseStars = sf.OutConfig.RatingUseStars;

            Config.DateFormat = sf.OutConfig.DateFormat;
            Config.DateCustomFormat1 = sf.OutConfig.DateCustomFormat1;
            Config.DateCustomSeparator1 = sf.OutConfig.DateCustomSeparator1;
            Config.DateCustomFormat2 = sf.OutConfig.DateCustomFormat2;
            Config.DateCustomSeparator2 = sf.OutConfig.DateCustomSeparator2;
            Config.DateCustomFormat3 = sf.OutConfig.DateCustomFormat3;
            Config.DateCustomSeparator3 = sf.OutConfig.DateCustomSeparator3;
            Config.DateCustomFormat4 = sf.OutConfig.DateCustomFormat4;
            Config.DateCustomFormatString = sf.OutConfig.DateCustomFormatString;

            Config.DaysRecent = sf.OutConfig.DaysRecent;

            #endregion

            #region Other tab

            Config.ConvertWAVsTo16BitOnInstall = sf.OutConfig.ConvertWAVsTo16BitOnInstall;
            Config.ConvertOGGsToWAVsOnInstall = sf.OutConfig.ConvertOGGsToWAVsOnInstall;

            Config.ConfirmUninstall = sf.OutConfig.ConfirmUninstall;

            Config.BackupFMData = sf.OutConfig.BackupFMData;
            Config.BackupAlwaysAsk = sf.OutConfig.BackupAlwaysAsk;

            Config.Language = sf.OutConfig.Language;

            Config.WebSearchUrl = sf.OutConfig.WebSearchUrl;

            Config.ConfirmPlayOnDCOrEnter = sf.OutConfig.ConfirmPlayOnDCOrEnter;

            Config.HideUninstallButton = sf.OutConfig.HideUninstallButton;
            Config.HideFMListZoomButtons = sf.OutConfig.HideFMListZoomButtons;

            Config.ReadmeUseFixedWidthFont = sf.OutConfig.ReadmeUseFixedWidthFont;

            #endregion

            // These ones MUST NOT be set on startup, because the source values won't be valid
            Config.SortedColumn = View.CurrentSortedColumnIndex;
            Config.SortDirection = View.CurrentSortDirection;

            #endregion

            #region Change-specific actions (pre-refresh)

            View.ShowInstallUninstallButton(!Config.HideUninstallButton);
            View.ShowFMsListZoomButtons(!Config.HideFMListZoomButtons);

            if (archivePathsChanged || gamePathsChanged)
            {
                FindFMs.Find();
            }
            if (gameOrganizationChanged)
            {
                // Clear everything to defaults so we don't have any leftover state screwing things all up
                Config.ClearAllSelectedFMs();
                Config.ClearAllFilters();
                Config.GameTab = Thief1;
                View.ClearUIAndCurrentInternalFilter();
                if (Config.GameOrganization == GameOrganization.ByTab) Config.Filter.Games = Game.Thief1;
                View.ChangeGameOrganization();
            }
            if (useShortGameTabNamesChanged)
            {
                View.ChangeGameTabNameShortness(refreshFilterBarPositionIfNeeded: true);
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

            #region Call appropriate refresh method (if applicable)

            if (archivePathsChanged || gamePathsChanged || gameOrganizationChanged || articlesChanged ||
                daysRecentChanged)
            {
                if (archivePathsChanged || gamePathsChanged)
                {
                    if (FMsViewListUnscanned.Count > 0) await FMScan.ScanNewFMs();
                }

                bool keepSel = !gameOrganizationChanged;

                // TODO: forceDisplayFM is always true so that this always works, but it could be smarter
                // If I store the selected FM up above the Find(), I can make the FM not have to reload if
                // it's still selected
                await View.SortAndSetFilter(keepSelection: keepSel, forceDisplayFM: true);
            }
            else if (dateFormatChanged || languageChanged)
            {
                View.RefreshFMsListKeepSelection();
            }

            #endregion
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
                    : (FMsPath: "", FMLanguage: "", FMLanguageForced: false, FMSelectorLines: new List<string>(),
                        AlwaysShowLoader: false);

                Config.SetFMInstallPath(gameIndex, data.FMsPath);
                Config.SetGameEditorDetected(gameIndex, gameExeSpecified && !GetEditorExe(gameIndex).IsEmpty());
#if false
                Config.SetPerGameFMLanguage(game, data.FMLanguage);
                Config.SetPerGameFMForcedLanguage(game, data.FMLanguageForced);
#endif

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

                if (gameIndex == Thief2) Config.T2MPDetected = gameExeSpecified && !GetT2MultiplayerExe().IsEmpty();
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

        // Future use
        //internal static void SetDefaultConfigVarNamesToLocalized()
        //{
        //    Defaults.CV_ForceFullScreen.Name = LText.ConfigVars.ForceFullScreen;
        //    Defaults.CV_ForceWindowed.Name = LText.ConfigVars.ForceWindowed;
        //    Defaults.CV_ForceNewMantle.Name = LText.ConfigVars.ForceNewMantle;
        //    Defaults.CV_ForceOldMantle.Name = LText.ConfigVars.ForceOldMantle;
        //}

        internal static void SortFMsViewList(Column column, SortOrder sortDirection)
        {
            var comparer = column switch
            {
                Column.Game => Comparers.FMGameComparer,
                Column.Installed => Comparers.FMInstalledComparer,
                Column.Title => Comparers.FMTitleComparer,
                Column.Archive => Comparers.FMArchiveComparer,
                Column.Author => Comparers.FMAuthorComparer,
                Column.Size => Comparers.FMSizeComparer,
                Column.Rating => Comparers.FMRatingComparer,
                Column.Finished => Comparers.FMFinishedComparer,
                Column.ReleaseDate => Comparers.FMReleaseDateComparer,
                Column.LastPlayed => Comparers.FMLastPlayedComparer,
                Column.DateAdded => Comparers.FMDateAddedComparer,
                Column.DisabledMods => Comparers.FMDisabledModsComparer,
                Column.Comment => Comparers.FMCommentComparer,
                // NULL_TODO: Null only so I can run the assert below
                // For if I ever need to add something here and forget... not likely
                _ => null
            };

            AssertR(comparer != null, nameof(comparer) + "==null: column not being handled");

            comparer!.SortOrder = sortDirection;

            FMsViewList.Sort(comparer);

            if (View.ShowRecentAtTop)
            {
                // Store it so it doesn't change
                var dtNow = DateTime.Now;

                var tempFMs = new List<FanMission>();

                for (int i = 0; i < FMsViewList.Count; i++)
                {
                    var fm = FMsViewList[i];
                    fm.MarkedRecent = false;

                    if (fm.DateAdded != null &&
                        ((DateTime)fm.DateAdded).ToLocalTime().CompareTo(dtNow) <= 0 &&
                        (dtNow - ((DateTime)fm.DateAdded).ToLocalTime()).TotalDays <= Config.DaysRecent)
                    {
                        tempFMs.Add(fm);
                    }
                }

                Comparers.FMDateAddedComparer.SortOrder = SortOrder.Ascending;
                tempFMs.Sort(Comparers.FMDateAddedComparer);

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
        }

        public static async Task RefreshFMsListFromDisk()
        {
            SelectedFM? selFM = View.GetSelectedFMPosInfo();
            using (new DisableEvents(View)) await FMScan.FindNewFMsAndScanNew();
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

            viewFilter.ShowUnsupported = View.GetShowUnsupportedFilter();

            #endregion

            var filterShownIndexList = View.GetFilterShownIndexList();

            filterShownIndexList.Clear();

            // This one gets checked in a loop, so cache it. Others are only checked once, so leave them be.
            bool titleIsWhitespace = viewFilter.Title.IsWhiteSpace();

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
                    titleIsWhitespace ||
                    fm.Title.ContainsI(viewFilter.Title) ||
                    (fm.Archive.ExtIsArchive()
                        ? fm.Archive.IndexOf(viewFilter.Title, 0, fm.Archive.LastIndexOf('.'), StringComparison.OrdinalIgnoreCase) > -1
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
                        !fm.Author.ContainsI(viewFilter.Author))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Show unsupported

            if (!viewFilter.ShowUnsupported)
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
                        (viewFilter.Games & fm.Game) != fm.Game)
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

                    if (fm.MarkedRecent) continue;

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
                            CatAndTags? match = fm.Tags.FirstOrDefault(x => x.Category == andTag.Category);
                            if (match == null)
                            {
                                andPass = false;
                                break;
                            }

                            if (andTag.Tags.Count > 0)
                            {
                                foreach (string andTagTag in andTag.Tags)
                                {
                                    if (match.Tags.FirstOrDefault(x => x == andTagTag) == null)
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
                            CatAndTags? match = fm.Tags.FirstOrDefault(x => x.Category == orTag.Category);
                            if (match == null) continue;

                            if (orTag.Tags.Count > 0)
                            {
                                foreach (string orTagTag in orTag.Tags)
                                {
                                    if (match.Tags.FirstOrDefault(x => x == orTagTag) != null)
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
                            CatAndTags? match = fm.Tags.FirstOrDefault(x => x.Category == notTag.Category);
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
                                    if (match.Tags.FirstOrDefault(x => x == notTagTag) != null)
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
                        (((fmFinished > 0 || fmFinishedOnUnknown) && (viewFilter.Finished & FinishedState.Finished) != FinishedState.Finished) ||
                        (fmFinished == 0 && !fmFinishedOnUnknown && (viewFilter.Finished & FinishedState.Unfinished) != FinishedState.Unfinished)))
                    {
                        filterShownIndexList.RemoveAt(i);
                        i--;
                    }
                }
            }

            #endregion

            #region Marked deleted (special case)

            if (OneOrMoreFMsAreMarkedDeleted)
            {
                for (int i = 0; i < filterShownIndexList.Count; i++)
                {
                    var fm = FMsViewList[filterShownIndexList[i]];
                    if (fm.MarkedDeleted)
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
            if (!GameIsDark(fm.Game))
            {
                Log("AddDML: fm is not Dark", stackTrace: true);
                return false;
            }

            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_AddDML_InstallDirNotFound, LText.AlertMessages.Alert);
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
                Log("Unable to add .dml to installed folder " + fm.InstalledDir, ex);
                View.ShowAlert(LText.AlertMessages.Patch_AddDML_UnableToAdd, LText.AlertMessages.Alert);
                return false;
            }

            return true;
        }

        internal static bool RemoveDML(FanMission fm, string dmlFile)
        {
            if (!GameIsDark(fm.Game))
            {
                Log("RemoveDML: fm is not Dark", stackTrace: true);
                return false;
            }

            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_RemoveDML_InstallDirNotFound, LText.AlertMessages.Alert);
                return false;
            }

            string installedFMPath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);
            try
            {
                File.Delete(Path.Combine(installedFMPath, dmlFile));
            }
            catch (Exception ex)
            {
                Log("Unable to remove .dml from installed folder " + fm.InstalledDir, ex);
                View.ShowAlert(LText.AlertMessages.Patch_RemoveDML_UnableToRemove, LText.AlertMessages.Alert);
                return false;
            }

            return true;
        }

        internal static (bool Success, List<string> DMLFiles)
        GetDMLFiles(FanMission fm)
        {
            if (!GameIsDark(fm.Game))
            {
                Log("GetDMLFiles: fm is not Dark", stackTrace: true);
                return (false, new List<string>());
            }

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

        // TODO: Make this actually exception-safe (FMIsReallyInstalled doesn't throw, but Path.Combine() might)
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
            using (var fs = new FileStream(readmeOnDisk, FileMode.Open, FileAccess.Read))
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
                // PERF_TODO: Switch to TryGetValue
                // Because there's no built-in way to tell it to find a key case-insensitively, we just convert
                // to lowercase cause whatever. Perf doesn't really matter here.
                string langLower = Config.Language.ToLowerInvariant();
                // Because we allow arbitrary languages, it's theoretically possible to get one that doesn't have
                // a language code.
                bool langCodeExists = FMLanguages.LangCodes.ContainsKey(langLower);
                string langCode = langCodeExists ? FMLanguages.LangCodes[langLower] : "";
                bool altLangCodeExists = FMLanguages.AltLangCodes.ContainsKey(langCode);
                string altLangCode = altLangCodeExists ? FMLanguages.AltLangCodes[langCode] : "";

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
                    safeReadmes.Sort(Comparers.FileNameNoExtComparer);

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

            if (safeReadme!.IsEmpty())
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
                Log(nameof(OpenFMFolder) + ": fm is not known or supported", stackTrace: true);
                return;
            }

            string installsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
            string fmDir;
            if (installsBasePath.IsEmpty() || !Directory.Exists(fmDir = Path.Combine(installsBasePath, fm.InstalledDir)))
            {
                View.ShowAlert(LText.AlertMessages.Patch_FMFolderNotFound, LText.AlertMessages.Alert);
                return;
            }

            try
            {
                ProcessStart_UseShellExecute(fmDir);
            }
            catch (Exception ex)
            {
                Log("Exception trying to open FM folder " + fmDir, ex);
            }
        }

        internal static void OpenWebSearchUrl(string fmTitle)
        {
            string url = Config.WebSearchUrl;
            if (url.IsWhiteSpace() || url.Length > 32766) return;

            int index = url.IndexOf("$TITLE$", StringComparison.OrdinalIgnoreCase);

            // Possible exceptions are:
            // ArgumentNullException (stringToEscape is null)
            // UriFormatException (The length of stringToEscape exceeds 32766 characters)
            // Those are both checked for above so we're good.
            string finalUrl = Uri.EscapeUriString(index == -1
                ? url
                : url.Substring(0, index) + fmTitle + url.Substring(index + "$TITLE$".Length));

            try
            {
                ProcessStart_UseShellExecute(finalUrl);
            }
            catch (FileNotFoundException ex)
            {
                Log("\"The PATH environment variable has a string containing quotes.\" (that's what MS docs says?!)", ex);
            }
            catch (Win32Exception ex)
            {
                Log("Problem opening web search URL", ex);
                View.ShowAlert(LText.AlertMessages.WebSearchURL_ProblemOpening, LText.AlertMessages.Alert);
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
                Log("Exception in " + nameof(GetReadmeFileFullPath), ex);
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
                }
            }
            else
            {
                Log("File not found: " + path, stackTrace: true);
            }
        }

        internal static void OpenHelpFile(string section)
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

            // TODO: Un-hardcode this
            string helpFileBase = Path.Combine(Paths.Doc, "AngelLoader documentation.html");

            if (!File.Exists(helpFileBase))
            {
                View.ShowAlert(LText.AlertMessages.Help_HelpFileNotFound, LText.AlertMessages.Alert);
                return;
            }

            string helpFileUri = "file://" + helpFileBase;
            string finalUri;
            try
            {
                File.WriteAllText(Paths.HelpRedirectFilePath, @"<meta http-equiv=""refresh"" content=""0; URL=" + helpFileUri + section + @""" />");
                finalUri = Paths.HelpRedirectFilePath;
            }
            catch (Exception ex)
            {
                Log(nameof(OpenHelpFile) + ": Exception writing temp help redirect file. Using un-anchored path (help file will be positioned at top, not at requested section)...", ex);
                finalUri = helpFileUri;
            }

            try
            {
                ProcessStart_UseShellExecute(finalUri);
            }
            catch (Exception ex)
            {
                Log(nameof(OpenHelpFile) + ": Exception in " + nameof(ProcessStart_UseShellExecute) + ". Couldn't open help file.", ex);
                View.ShowAlert(LText.AlertMessages.Help_UnableToOpenHelpFile, LText.AlertMessages.Alert);
            }
        }

        internal static void OpenLink(string link)
        {
            try
            {
                ProcessStart_UseShellExecute(link);
            }
            catch (Exception ex)
            {
                Log("Problem opening clickable link from rtfbox", ex);
            }
        }

        #endregion

        internal static async Task DeleteFMArchive(FanMission fm)
        {
            var archives = FindFMArchive_Multiple(fm.Archive);
            if (archives.Count == 0)
            {
                View.ShowAlert(LText.FMDeletion.ArchiveNotFound, LText.AlertMessages.DeleteFMArchive);
                return;
            }

            bool singleArchive = archives.Count == 1;

            var finalArchives = new List<string>();

            using (var f = new MessageBoxCustomForm(
                messageTop: singleArchive
                    ? LText.FMDeletion.AboutToDelete + "\r\n\r\n" + archives[0]
                    : LText.FMDeletion.DuplicateArchivesFound,
                messageBottom: "",
                title: LText.AlertMessages.DeleteFMArchive,
                icon: MessageBoxIcon.Warning,
                okText: singleArchive ? LText.FMDeletion.DeleteFM : LText.FMDeletion.DeleteFMs,
                cancelText: LText.Global.Cancel,
                okIsDangerous: true,
                choiceStrings: singleArchive ? null : archives.ToArray()))
            {
                if (f.ShowDialog() != DialogResult.OK) return;

                finalArchives.AddRange(singleArchive ? archives : f.SelectedItems);
            }

            try
            {
                View.ShowProgressBox(ProgressTasks.DeleteFMArchive);
                await Task.Run(() =>
                {
                    foreach (string archive in finalArchives)
                    {
                        try
                        {
                            FileSystem.DeleteFile(archive, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                        }
                        catch (Exception ex)
                        {
                            Log(nameof(DeleteFMArchive) + ": Exception deleting file '" + archive + "'", ex);
                            View.InvokeSync(new Action(() =>
                            {
                                View.ShowAlert(
                                     LText.AlertMessages.DeleteFM_UnableToDelete + "\r\n\r\n" +
                                     archive,
                                     LText.AlertMessages.Error);
                            }));
                        }
                    }
                });
            }
            finally
            {
                var newArchives = await Task.Run(() => FindFMArchive_Multiple(fm.Archive));

                View.HideProgressBox();

                if (newArchives.Count == 0 && !fm.Installed)
                {
                    // Disgusting hack that results in a better user experience than the "proper" way of reloading
                    // the list from disk immediately
                    fm.MarkedDeleted = true;
                    OneOrMoreFMsAreMarkedDeleted = true;

                    await View.SortAndSetFilter(keepSelection: true);
                }
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

            Config.SortedColumn = (Column)sortedColumn;
            Config.SortDirection = sortDirection;

            Config.ShowRecentAtTop = View.ShowRecentAtTop;

            Config.FMsListFontSizeInPoints = fmsListFontSizeInPoints;

            #endregion

            filter.DeepCopyTo(Config.Filter);

            #region Top-right panel

            Config.TopRightTabsData.SelectedTab = topRightTabsData.SelectedTab;

            for (int i = 0; i < TopRightTabsCount; i++)
            {
                Config.TopRightTabsData.Tabs[i].Position = topRightTabsData.Tabs[i].Position;
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

//#define ENABLE_README_TESTS

/*
NOTE(Core notes):
-We could have the stub be called back on game exit and use that to track game lifetime, for temp config var changes
 But note we may have to handle no_unload_fmsel option - make sure we don't have stale values on SelectFM call?

-@IO_SAFETY: Make a system where files get temp-copied and then if writes fail, we copy the old file back (FMSel does this)
 For FMData.ini this will be more complicated because we rewrite it a lot (whenever values change on the UI) so
 if we want to keep multiple backups (and we probably should) then we want to avoid blowing out our backup cache
 every time we write

-@NET5: Process output/error reading, code pages rear their ugly heads again
 From the .NET 7 Process doc:
"In the .NET Framework, the Process class by default uses Console encodings, which are typically code page
encodings, for the input, output, and error streams. For example code, on systems whose culture is English
(United States), code page 437 is the default encoding for the Console class. However, .NET Core may make
only a limited subset of these encodings available. If this is the case, it uses Encoding.UTF8 as the default
encoding."

We're reading output from ffprobe.exe and 7z.exe. We're only pulling out ascii strings, so we're fine with all
the Windows and DOS codepages (125x, 437 etc.) but we should see if we can switch to UTF8 and have it still work.
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.LanguageSupport;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.SettingsWindowData;
using static AngelLoader.Utils;

namespace AngelLoader;

internal static class Core
{
    // TODO: Core: View = null!; note
    // Remove this null-handwave and get null notification on this so we don't accidentally access it when
    // it's null. But if we check it from another thread there'll be a race condition. Figure something out?
    internal static IView View = null!;
    private static IViewEnvironment ViewEnv = null!;
    internal static IDialogs Dialogs = null!;

    /*
    We can't put this locally in a using statement because of "Access to disposed closure" blah blah whatever,
    so just put it here and never dispose... meh.
    I mean it might/probably is safe because we wait for the task before moving on? But who cares for now,
    this is guaranteed safe at least...
    */
    private static readonly AutoResetEvent _configReadARE = new(false);

#if RT_HeavyTests
    internal static System.Threading.Thread CoreThread = null!;
#endif

    internal static async void Init(IViewEnvironment viewEnv)
    {
#if RT_HeavyTests
        CoreThread = System.Threading.Thread.CurrentThread;
#endif

        ViewEnv = viewEnv;
        Dialogs = ViewEnv.GetDialogs();

        TDMWatchers.Init();

        bool openSettings = false;
        SettingsWindowState settingsWindowState = SettingsWindowState.Startup;

        List<FanMission>? fmsViewListUnscanned = null;

        #region Create required directories

        try
        {
            Directory.CreateDirectory(Paths.Data);
            Directory.CreateDirectory(Paths.Languages);
        }
        catch (Exception ex)
        {
            // ReSharper disable once ConvertToConstant.Local
            string message = "Fatal error: Failed to create required application directories on startup.";
            Log(message, ex);
            // We're not even close to having a theme at this point, so just use regular MessageBox
            Dialogs.ShowAlert_Stock(message, "Error", MBoxButtons.OK, MBoxIcon.Error);
            Environment.Exit(1);
        }

        #endregion

        // Perf: The only thing the splash screen needs is the theme
        Config.VisualTheme = Ini.ReadThemeFromConfigIni(Paths.ConfigIni);

        Error[] gameDataErrors = InitializedArray(SupportedGameCount, Error.None);
        bool enableTDMWatchers = false;
        List<string>?[] perGameCamModIniLines = new List<string>?[SupportedGameCount];

        using Task startupWorkTask = Task.Run(() =>
        {
            try
            {
                #region Old FMScanner log file delete

                // We use just the one file now
                try
                {
                    File.Delete(Paths.ScannerLogFile_Old);
                }
                catch
                {
                    // ignore
                }

                #endregion

                #region Read config ini

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

                        // We need to run this to have correct/valid config settings, if we didn't get to where it
                        // runs in the config reader
                        Ini.FinalizeConfig(Config);

                        return;
                    }
                    finally
                    {
                        _configReadARE.Set();
                    }

                    // @BetterErrors(Set game data on startup)
                    // @THREADING(Config):
                    // Set game data here to ensure we're DONE filling out the config object before the form does
                    // anything (race condition possibility prevention)
                    for (int i = 0; i < SupportedGameCount; i++)
                    {
                        GameIndex gameIndex = (GameIndex)i;
                        // Existence checks on startup are merely a perf optimization: values start blank so just
                        // don't set them if we don't have a game exe
                        string gameExe = Config.GetGameExe(gameIndex);
                        if (!gameExe.IsEmpty() && File.Exists(gameExe))
                        {
                            (gameDataErrors[i], bool _enableTdmWatchers, perGameCamModIniLines[i]) = SetGameDataFromDisk(gameIndex, storeConfigInfo: true);
                            if (gameIndex == GameIndex.TDM) enableTDMWatchers = _enableTdmWatchers;
                        }
                    }
                }
                else
                {
                    openSettings = true;
                    settingsWindowState = SettingsWindowState.StartupClean;

                    // Ditto the above
                    try
                    {
                        Ini.FinalizeConfig(Config);
                    }
                    finally
                    {
                        _configReadARE.Set();
                    }
                }

                #endregion
            }
            finally
            {
                _configReadARE.Set();
            }
        });

        // We can't show the splash screen until we know our theme, which we have to get from the config
        // file, so we can't show it any earlier than this.
        using var splashScreen = new SplashScreen(ViewEnv.GetSplashScreen());

        splashScreen.Show(Config.VisualTheme);

        _configReadARE.WaitOne();

        #region Read languages

        static void ReadLanguages(SplashScreen splashScreen)
        {
            static void ResetLanguages()
            {
                LText = new LText_Class();
                Config.Language = "English";
                Config.LanguageNames.Clear();
            }

            // We can't show a message until we've read the config file (to know which language to use) and
            // the current language file (to get the translated message strings). So just show language dir/
            // language file names, so it's as clear as possible what we're doing without actually having to
            // display a translated string.
            splashScreen.SetMessage(Paths.Languages);

            // Have to read langs here because which language to use will be stored in the config file.
            // Gather all lang files in preparation to read their LanguageName= value so we can get the lang's
            // name in its own language
            List<string> langFiles;
            try
            {
                langFiles = FastIO.GetFilesTopOnly(Paths.Languages, "*.ini");
            }
            catch (Exception ex)
            {
                splashScreen.Hide();
                ResetLanguages();
                Log(ErrorText.ExTry + "get language .ini files from " + Paths.Languages, ex);
                Dialogs.ShowError(
                    "Couldn't get the language .ini files.\r\n\r\n" +
                    ErrorText.LangDefault + "\r\n\r\n" +
                    "Path: " + Paths.Languages);
                splashScreen.Show(Config.VisualTheme);
                return;
            }
            bool selFound = false;

            Config.LanguageNames.Clear();

            try
            {
                for (int i = 0; i < langFiles.Count; i++)
                {
                    string f = langFiles[i];
                    string fn = f.GetFileNameFast().RemoveExtension();

                    splashScreen.SetMessage(f);

                    if (!selFound && fn.EqualsI(Config.Language))
                    {
                        Ini.ReadLocalizationIni(f, LText);
                        selFound = true;
                    }

                    Ini.AddLanguageFromFile(f, fn, Config.LanguageNames);
                }
            }
            catch (Exception ex)
            {
                splashScreen.Hide();
                ResetLanguages();
                Log(ErrorText.Ex + "in language files read", ex);
                Dialogs.ShowError("An error occurred while trying to read language file(s). " + ErrorText.LangDefault);
                splashScreen.Show(Config.VisualTheme);
            }
        }

        ReadLanguages(splashScreen);

        #endregion

        if (!openSettings)
        {
            splashScreen.SetMessage(LText.SplashScreen.CheckingRequiredSettingsFields);

            bool backupPathInvalid = BackupPathInvalid(Config.FMsBackupPath);
            bool allArchivePathsExist = true;
            for (int i = 0; i < Config.FMArchivePaths.Count; i++)
            {
                if (!Directory.Exists(Config.FMArchivePaths[i]))
                {
                    allArchivePathsExist = false;
                    break;
                }
            }
            openSettings = backupPathInvalid || !allArchivePathsExist;
        }

        splashScreen.SetMessage(LText.SplashScreen.ReadingGameConfigFiles);

        startupWorkTask.Wait();

        Task DoParallelLoad()
        {
            splashScreen.SetMessage(LText.SplashScreen.SearchingForNewFMs + Environment.NewLine +
                                    LText.SplashScreen.LoadingMainApp);
            // We set this beforehand because we thought there was some cross-thread font access problem, but
            // actually it was just some issue with incorrect font disposal (which we fixed), but whatever, this
            // still works, so keeping it.
            splashScreen.SetCheckMessageWidth(LText.SplashScreen.SearchingForNewFMs);
            try
            {
                splashScreen.LockPainting(true);

#if RT_HeavyTests
#pragma warning disable IDE0002
                // ReSharper disable once ArrangeStaticMemberQualifier
                Global.ThreadLocked = true;
#pragma warning restore IDE0002
#endif

                Exception? ex = null;
                // IMPORTANT: Begin no-splash-screen-call zone
                // The FM finder will update the splash screen from another thread (accessing only the graphics
                // context, so no cross-thread Control access exceptions), so any calls in here are potential
                // race conditions.
                using Task findFMsTask = Task.Run(() =>
                {
                    for (int i = 0; i < SupportedGameCount; i++)
                    {
                        List<string>? camModIniLines = perGameCamModIniLines[i];
                        if (camModIniLines != null)
                        {
                            GameConfigFiles.FixCharacterDetailLine((GameIndex)i, camModIniLines);
                        }
                    }
                    (fmsViewListUnscanned, ex) = FindFMs.Find_Startup(splashScreen);
                    if (ex == null)
                    {
                        ViewEnv.PreprocessRTFReadme(Config, FMsViewList, fmsViewListUnscanned);
                    }

                    TDM.UpdateTDMDataFromDisk(refresh: false);
                });

                // Construct and init the view right here, because it's a heavy operation and we want it to run
                // in parallel with Find() to the greatest extent possible.
                View = ViewEnv.GetView();

                findFMsTask.Wait();

#if RT_HeavyTests
#pragma warning disable IDE0002
                // ReSharper disable once ArrangeStaticMemberQualifier
                Global.ThreadLocked = false;
#pragma warning restore IDE0002
#endif

                // IMPORTANT: End no-splash-screen-call zone

                if (ex != null)
                {
                    splashScreen.Hide();
                    Log(ErrorText.ExRead + Paths.FMDataIni, ex);
                    Dialogs.ShowError(LText.AlertMessages.FindFMs_ExceptionReadingFMDataIni);
                    EnvironmentExitDoShutdownTasks(1);
                }
            }
            finally
            {
                splashScreen.LockPainting(false);
            }

            return View.FinishInitAndShow(fmsViewListUnscanned!, splashScreen);
        }

        ThrowDialogIfSneakyOptionsIniNotFound(gameDataErrors);

        if (!openSettings)
        {
            await DoParallelLoad();
        }
        else
        {
            splashScreen.Hide();
            if (await OpenSettings(settingsWindowState))
            {
                splashScreen.Show(Config.VisualTheme);
                await DoParallelLoad();
            }
        }

        TDMWatchers.DeferredWatchersEnable(enableTDMWatchers);
    }

    private static void ThrowDialogIfSneakyOptionsIniNotFound(Error[] errors)
    {
        if (errors[(int)GameIndex.Thief3] == Error.SneakyOptionsNotFound)
        {
            Dialogs.ShowAlert(LText.AlertMessages.Misc_SneakyOptionsIniNotFound, LText.AlertMessages.Alert);
        }
    }

    // @CAN_RUN_BEFORE_VIEW_INIT
    /// <summary>
    /// Opens the Settings dialog and performs any required changes and updates.
    /// </summary>
    /// <param name="state"></param>
    /// <returns><see langword="true"/> if changes were accepted, <see langword="false"/> if canceled.</returns>
    public static async Task<bool>
    OpenSettings(SettingsWindowState state = SettingsWindowState.Normal)
    {
        bool startup = state.IsStartup();

        (bool accepted, ConfigData outConfig) =
            ViewEnv.ShowSettingsWindow(startup ? null : View, Config, state);

        #region Save window state

        // Special case: these are meta, so they should always be set even if the user clicked Cancel
        Config.SettingsTab = outConfig.SettingsTab;
        Config.SettingsWindowSize = outConfig.SettingsWindowSize;
        Config.SettingsWindowSplitterDistance = outConfig.SettingsWindowSplitterDistance;

        for (int i = 0; i < SettingsTabCount; i++)
        {
            SettingsTab tab = (SettingsTab)i;
            Config.SetSettingsTabVScrollPos(tab, outConfig.GetSettingsTabVScrollPos(tab));
        }

        #endregion

        if (!accepted)
        {
            // Since nothing of consequence has yet happened, it's okay to do the brutal quit
            // We know the game paths by now, so we can do this
            if (startup) EnvironmentExitDoShutdownTasks(0);
            return false;
        }

        #region Set changed bools

        bool archivePathsChanged =
            !startup &&
            (!Config.FMArchivePaths.PathSequenceEqualI_Dir(outConfig.FMArchivePaths) ||
             Config.FMArchivePathsIncludeSubfolders != outConfig.FMArchivePathsIncludeSubfolders);

        bool gamePathsChanged = false;
        // We need these in order to decide which, if any, startup config infos to re-read
        bool[] individualGamePathsChanged = new bool[SupportedGameCount];
        if (!startup)
        {
            for (int i = 0; i < SupportedGameCount; i++)
            {
                GameIndex gameIndex = (GameIndex)i;
                bool gamePathChanged = !Config.GetGameExe(gameIndex).PathEqualsI(outConfig.GetGameExe(gameIndex));
                if (gamePathChanged) gamePathsChanged = true;
                individualGamePathsChanged[i] = gamePathChanged;
            }
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

        bool playWithFMButtonStyleChanged =
            !startup && Config.PlayOriginalSeparateButtons != outConfig.PlayOriginalSeparateButtons;

        bool fuzzySearchChanged =
            !startup && Config.EnableFuzzySearch != outConfig.EnableFuzzySearch;

        #endregion

        #region Set config data

        // Set values individually (rather than deep-copying) so that non-Settings values don't get overwritten.

        #region Paths page

        #region Game exes

        // Do this BEFORE copying game exes to Config, because we need the Config game exes to still point to
        // the old ones.
        if (gamePathsChanged) GameConfigFiles.ResetGameConfigTempChanges(individualGamePathsChanged);

        // Game paths should have been checked and verified before OK was clicked, so assume they're good here
        Error[] setGameDataErrors = new Error[SupportedGameCount];
        bool enableTDMWatchers = false;
        for (int i = 0; i < SupportedGameCount; i++)
        {
            GameIndex gameIndex = (GameIndex)i;

            // This must be done first!
            Config.SetGameExe(gameIndex, outConfig.GetGameExe(gameIndex));

            // Set it regardless of game existing, because we want to blank the data
            (setGameDataErrors[i], bool _enableTDMWatchers, _) = SetGameDataFromDisk(gameIndex, storeConfigInfo: startup || individualGamePathsChanged[i]);
            if (gameIndex == GameIndex.TDM) enableTDMWatchers = _enableTDMWatchers;

            Config.SetUseSteamSwitch(gameIndex, outConfig.GetUseSteamSwitch(gameIndex));
        }

        ThrowDialogIfSneakyOptionsIniNotFound(setGameDataErrors);

        #endregion

        Config.SteamExe = outConfig.SteamExe;
        Config.LaunchGamesWithSteam = outConfig.LaunchGamesWithSteam;

        Config.RunThiefBuddyOnFMPlay = outConfig.RunThiefBuddyOnFMPlay;

        Config.FMsBackupPath = outConfig.FMsBackupPath;

        Config.FMArchivePaths.ClearAndAdd_Small(outConfig.FMArchivePaths);

        Config.FMArchivePathsIncludeSubfolders = outConfig.FMArchivePathsIncludeSubfolders;

        #endregion

        List<FanMission>? fmsViewListUnscanned = null;

        if (startup)
        {
            Config.Language = outConfig.Language;

            // We don't need to set the paths again, because we've already done so above

            Ini.WriteConfigIni();

            return true;
        }

        // From this point on, we're not in startup mode.

        // For clarity, don't copy the other tabs' data on startup, because their tabs won't be shown and so
        // they won't have been changed

        #region Appearance page

        Config.Language = outConfig.Language;

        Config.VisualTheme = outConfig.VisualTheme;

        Config.GameOrganization = outConfig.GameOrganization;
        Config.UseShortGameTabNames = outConfig.UseShortGameTabNames;

        Config.EnableArticles = outConfig.EnableArticles;
        Config.Articles.ClearAndAdd_Small(outConfig.Articles);
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

        Config.HideUninstallButton = outConfig.HideUninstallButton;
        Config.HideFMListZoomButtons = outConfig.HideFMListZoomButtons;
        Config.HideExitButton = outConfig.HideExitButton;
        Config.HideWebSearchButton = outConfig.HideWebSearchButton;

        Config.ReadmeUseFixedWidthFont = outConfig.ReadmeUseFixedWidthFont;

        Config.PlayOriginalSeparateButtons = outConfig.PlayOriginalSeparateButtons;

        #endregion

        #region Other page

        Config.ConvertWAVsTo16BitOnInstall = outConfig.ConvertWAVsTo16BitOnInstall;
        Config.ConvertOGGsToWAVsOnInstall = outConfig.ConvertOGGsToWAVsOnInstall;

        Config.UseOldMantlingForOldDarkFMs = outConfig.UseOldMantlingForOldDarkFMs;

        Config.ConfirmBeforeInstall = outConfig.ConfirmBeforeInstall;

        Config.ConfirmUninstall = outConfig.ConfirmUninstall;

        Config.BackupFMData = outConfig.BackupFMData;
        Config.BackupAlwaysAsk = outConfig.BackupAlwaysAsk;

        Array.Copy(outConfig.WebSearchUrls, Config.WebSearchUrls, SupportedGameCount);

        Config.ConfirmPlayOnDCOrEnter = outConfig.ConfirmPlayOnDCOrEnter;

        Config.EnableFuzzySearch = outConfig.EnableFuzzySearch;

        #endregion

        // This MUST NOT be set on startup, because the source values won't be valid
        View.UpdateConfig();

        #endregion

        #region Change-specific actions (pre-refresh)

        View.ShowInstallUninstallButton(!Config.HideUninstallButton);
        View.ShowFMsListZoomButtons(!Config.HideFMListZoomButtons);
        View.ShowExitButton(!Config.HideExitButton);
        View.ShowWebSearchButton(!Config.HideWebSearchButton);

        if (archivePathsChanged || gamePathsChanged)
        {
            fmsViewListUnscanned = FindFMs.Find();
        }
        if (gameOrganizationChanged)
        {
            // Clear everything to defaults so we don't have any leftover state screwing things all up
            Config.ClearAllSelectedFMs();
            Config.ClearAllFilters();
            Config.GameTab = GameIndex.Thief1;
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

        bool playWithoutFMControlsStateSet = false;

        void SetPlayWithoutFMControlsState_Once()
        {
            if (!playWithoutFMControlsStateSet)
            {
                View.SetPlayOriginalGameControlsState();
                playWithoutFMControlsStateSet = true;
            }
        }

        #region Refresh (if applicable)

        bool keepSel = false;
        bool sortAndSetFilter = false;

        if (playWithFMButtonStyleChanged)
        {
            SetPlayWithoutFMControlsState_Once();
        }

        if (archivePathsChanged || gamePathsChanged || gameOrganizationChanged || articlesChanged ||
            daysRecentChanged)
        {
            if (gamePathsChanged)
            {
                SetPlayWithoutFMControlsState_Once();
#if !ReleaseBeta && !ReleasePublic
                View.UpdateGameModes();
#endif
            }

            keepSel = !gameOrganizationChanged;
            sortAndSetFilter = true;
        }
        else if (dateFormatChanged || languageChanged)
        {
            View.RefreshFMsListRowsOnlyKeepSelection();
        }

        #endregion

        if (fmsViewListUnscanned?.Count > 0)
        {
            await FMScan.ScanNewFMs(fmsViewListUnscanned);
        }

        if (fuzzySearchChanged)
        {
            sortAndSetFilter = true;
        }

        if (sortAndSetFilter)
        {
            // Just always force refresh of FM to make sure whatever we've changed will take.
            // We don't care about possible unnecessary refreshes because we only happen on settings OK click.
            // @DISPLAYED_FM_SYNC(OpenSettings() SortAndSetFilter() call):
            // It is REQUIRED to force-display the FM, to ensure the main view's internal displayed FM field
            // is not referencing a stale FM object that no longer exists in the list!
            await View.SortAndSetFilter(forceDisplayFM: true, keepSelection: keepSel);
        }
        else
        {
            View.RefreshMods();
        }

        TDM.UpdateTDMDataFromDisk(refresh: true);
        TDMWatchers.DeferredWatchersEnable(enableTDMWatchers);

        Ini.WriteConfigIni();

        return true;
    }

    /// <summary>
    /// Sets the per-game config data that we pull directly from the game folders on disk. Game paths,
    /// FM install paths, editors detected, pre-modification cam_mod.ini lines, etc.
    /// </summary>
    /// <param name="gameIndex"></param>
    /// <param name="storeConfigInfo"></param>
    private static (Error Error, bool EnableTDMWatchers, List<string>? CamModIniLines)
    SetGameDataFromDisk(GameIndex gameIndex, bool storeConfigInfo)
    {
        Error error = Error.None;
        bool enableTDMWatchers = false;
        List<string>? camModIniLines = null;
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
                ? GameConfigFiles.GetInfoFromCamModIni(gamePath, langOnly: false, returnAllLines: true)
                : (
                    FMsPath: "",
                    FMLanguage: "",
                    FMLanguageForced: false,
                    FMSelectorLines: new List<string>(),
                    AlwaysShowLoader: false,
                    AllLines: null
                );
            camModIniLines = data.AllLines;
            Config.SetFMInstallPath(gameIndex, data.FMsPath);
            Config.SetGameEditorDetected(gameIndex, gameExeSpecified && !GetEditorExe_FromDisk(gameIndex).IsEmpty());

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

            if (gameIndex == GameIndex.Thief2)
            {
                Config.T2MPDetected = gameExeSpecified && !GetT2MultiplayerExe_FromDisk().IsEmpty();
            }

            var hash = new HashSetPathI();
            List<Mod>? mods = null;

            if (!gamePath.IsEmpty() && data.AllLines != null && Directory.Exists(gamePath))
            {
                (bool success, mods) = GameConfigFiles.GetGameMods(data.AllLines);
                if (success)
                {
                    for (int i = 0; i < mods.Count; i++)
                    {
                        Mod mod = mods[i];
                        if (!GameConfigFiles.ModExistsOnDisk(gamePath, mod.InternalName) ||
                            !hash.Add(mod.InternalName))
                        {
                            mods.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            Config.SetMods(gameIndex, mods ?? new List<Mod>());

            List<Mod> configMods = Config.GetMods(gameIndex);
            if (configMods.Count > 0)
            {
                var tempList = new List<Mod>(configMods.Count);

                for (int i = 0; i < configMods.Count; i++)
                {
                    Mod mod = configMods[i];
                    if (mod.IsUber)
                    {
                        configMods.RemoveAt(i);
                        i--;
                        tempList.Add(mod);
                    }
                }

                // Use a temp list to prevent any issues with in-place moving in a loop
                for (int i = 0; i < tempList.Count; i++)
                {
                    configMods.Add(tempList[i]);
                }
            }
        }
        else if (gameIndex == GameIndex.TDM)
        {
            if (gameExeSpecified && !gamePath.IsEmpty())
            {
                Config.SetFMInstallPath(GameIndex.TDM, Path.Combine(gamePath, "fms"));
                enableTDMWatchers = true;
            }
            else
            {
                Config.SetFMInstallPath(GameIndex.TDM, "");
                enableTDMWatchers = false;
            }
        }
        else
        {
            if (gameExeSpecified)
            {
                var t3Data = GameConfigFiles.GetInfoFromSneakyOptionsIni();
                if (t3Data.Error == Error.None)
                {
                    Config.SetFMInstallPath(GameIndex.Thief3, t3Data.FMInstallPath);
                    Config.T3UseCentralSaves = t3Data.UseCentralSaves;
                }
                else
                {
                    error = t3Data.Error;
                    Config.SetFMInstallPath(GameIndex.Thief3, "");
                }
                // Do this even if there was an error, because we could still have a valid selector line
                if (storeConfigInfo)
                {
                    Config.GetStartupFMSelectorLines(GameIndex.Thief3).Clear();
                    if (!t3Data.PrevFMSelectorValue.IsEmpty())
                    {
                        Config.GetStartupFMSelectorLines(GameIndex.Thief3).Add(t3Data.PrevFMSelectorValue);
                    }
                    Config.SetStartupAlwaysStartSelector(GameIndex.Thief3, t3Data.AlwaysShowLoader);
                }
            }
            else
            {
                Config.SetFMInstallPath(GameIndex.Thief3, "");
                if (storeConfigInfo)
                {
                    Config.GetStartupFMSelectorLines(GameIndex.Thief3).Clear();
                    Config.SetStartupAlwaysStartSelector(GameIndex.Thief3, false);
                }
                Config.T3UseCentralSaves = false;
            }

            // We don't set mod dirs for Thief 3 because it doesn't support programmatic mod enabling/disabling
        }

        return (error, enableTDMWatchers, camModIniLines);
    }

    internal static void SortFMsViewList(Column column, SortDirection sortDirection)
    {
        Comparers.IDirectionalSortFMComparer comparer = Comparers.ColumnComparers[(int)column];

        comparer.SortDirection = sortDirection;

        FMsViewList.Sort(comparer);

        var tempFMs = new List<FanMission>();

        if (View.GetShowRecentAtTop())
        {
            // Store it so it doesn't change
            DateTime dtNow = DateTime.Now;

            for (int i = 0; i < FMsViewList.Count; i++)
            {
                FanMission fm = FMsViewList[i];
                fm.MarkedRecent = false;

                if (
                    // Don't mess with the sort order of pinned FMs, because they should be in the same sort
                    // order as the main list but just placed at the top. Whereas the recent FMs will always
                    // be displayed in order of date added.
                    !fm.Pinned &&
                    fm.DateAdded != null &&
                    ((DateTime)fm.DateAdded).CompareTo(dtNow) <= 0 &&
                    (dtNow - (DateTime)fm.DateAdded).TotalDays <= Config.DaysRecent)
                {
                    tempFMs.Add(fm);
                }
            }

            Comparers.ColumnComparers[(int)Column.DateAdded].SortDirection = SortDirection.Ascending;
            tempFMs.Sort(Comparers.ColumnComparers[(int)Column.DateAdded]);

            for (int i = 0; i < tempFMs.Count; i++)
            {
                FanMission fm = tempFMs[i];
                fm.MarkedRecent = true;
                FMsViewList.Remove(fm);
                FMsViewList.Insert(0, fm);
            }
        }
        else
        {
            for (int i = 0; i < FMsViewList.Count; i++) FMsViewList[i].MarkedRecent = false;
        }

        if (View.GetShowUnavailableFMsFilter()) return;

        #region Pinned

        tempFMs.Clear();

        for (int i = 0; i < FMsViewList.Count; i++)
        {
            FanMission fm = FMsViewList[i];
            if (fm.Pinned) tempFMs.Add(fm);
        }

        for (int i = tempFMs.Count - 1; i >= 0; i--)
        {
            FanMission fm = tempFMs[i];
            FMsViewList.Remove(fm);
            FMsViewList.Insert(0, fm);
        }

        #endregion
    }

    // @BetterErrors(RefreshFMsListFromDisk): This one ties into FindFMs (see note there)
    public static async Task RefreshFMsListFromDisk(SelectedFM? selFM = null)
    {
        selFM ??= View.GetMainSelectedFMPosInfo();
        try
        {
            // Cursor here instead of in Find(), so we can keep it over the case where we load an RTF readme
            // and it also sets the wait cursor, to avoid flickering it on and off twice.
            View.SetWaitCursor(true);

            List<FanMission> fmsViewListUnscanned = FindFMs.Find();

            // @TDM_CASE: Case-sensitive dictionary
            var tdmFMsDict = new Dictionary<string, int>(FMDataIniListTDM.Count);
            foreach (FanMission fm in FMDataIniListTDM)
            {
                if (!fm.MarkedUnavailable)
                {
                    tdmFMsDict[fm.TDMInstalledDir] = fm.TDMVersion;
                }
            }

            TDM.UpdateTDMDataFromDisk(refresh: false);

            foreach (FanMission fm in FMDataIniListTDM)
            {
                if (!fm.MarkedUnavailable)
                {
                    if (tdmFMsDict.TryGetValue(fm.TDMInstalledDir, out int version))
                    {
                        if (version != fm.TDMVersion)
                        {
                            /*
                            @TDM_NOTE: Re-scan only on next startup, because:
                            We only get here after detecting a modification to missions.tdminfo, but the actual
                            copying in of the updated FMs may not have finished yet, so we have a race condition
                            and the scanner may not be able to find the right pk4 file.
                            */
                            fm.MarkedScanned = false;
                        }
                    }
                }
            }

            if (fmsViewListUnscanned.Count > 0)
            {
                View.SetWaitCursor(false);
                await FMScan.ScanNewFMs(fmsViewListUnscanned);
            }
            // @DISPLAYED_FM_SYNC(RefreshFMsListFromDisk() SortAndSetFilter() call):
            // It is REQUIRED to force-display the FM, to ensure the main view's internal displayed FM field
            // is not referencing a stale FM object that no longer exists in the list!
            await View.SortAndSetFilter(selFM, forceDisplayFM: true);
        }
        finally
        {
            View.SetWaitCursor(false);
        }
    }

    internal static (bool Matched, bool ExactMatch)
    ContainsI_TextFilter(this string hay, string needle)
    {
        if (Config.EnableFuzzySearch)
        {
            return hay.ContainsI_Subsequence(needle);
        }
        else
        {
            bool contains = hay.ContainsI(needle);
            return (contains, contains);
        }
    }

    internal static (bool Matched, bool ExactMatch)
    FMTitleContains_AllTests(FanMission fm, string title, string titleTrimmed)
    {
        bool extIsArchive = fm.Archive.ExtIsArchive();
        if (extIsArchive && (titleTrimmed.EqualsI(".zip") || titleTrimmed.EqualsI(".7z")))
        {
            bool matched = fm.Archive.EndsWithI(titleTrimmed);
            return (matched, matched);
        }
        else
        {
            var titleContains = fm.Title.ContainsI_TextFilter(title);
            return titleContains.Matched
                ? titleContains
                : extIsArchive
                    ? (fm.Archive.IndexOf(title, 0, fm.Archive.LastIndexOf('.'), StringComparison.OrdinalIgnoreCase) > -1, true)
                    : fm.Game == Game.TDM ? (fm.DisplayArchive.IndexOf(title, 0, StringComparison.OrdinalIgnoreCase) > -1, true)
                    : (false, false);
        }
    }

    internal static (FanMission? TitleExactMatch, FanMission? AuthorExactMatch)
    SetFilter()
    {
#if DEBUG || (Release_Testing && !RT_StartupOnly)
        View.SetDebug2Text(Int_TryParseInv(View.GetDebug2Text(), out int result) ? (result + 1).ToStrInv() : "1");
#endif

        (FanMission? titleExactMatch, FanMission? authorExactMatch) ret = (null, null);

        Filter viewFilter = View.GetFilter();

        #region Set filters that are stored in control state

        viewFilter.Title = View.GetTitleFilter();
        viewFilter.Author = View.GetAuthorFilter();

        viewFilter.Games = View.GetGameFiltersEnabled();

        viewFilter.Finished = FinishedState.Null;
        if (View.GetFinishedFilter()) viewFilter.Finished |= FinishedState.Finished;
        if (View.GetUnfinishedFilter()) viewFilter.Finished |= FinishedState.Unfinished;

        #endregion

        List<int> filterShownIndexList = View.GetFilterShownIndexList();

        filterShownIndexList.Clear();

        // These are checked in a loop, so cache them. Others are only checked once, so leave them be.
        bool titleIsWhitespace = viewFilter.Title.IsWhiteSpace();
        string titleTrimmed = viewFilter.Title.Trim();

        #region Title / initial

        for (int i = 0; i < FMsViewList.Count; i++)
        {
            FanMission fm = FMsViewList[i];

            (bool Match, bool ExactMatch) match = (false, false);

            if (FMIsTopped(fm) ||
                titleIsWhitespace ||
                (match = FMTitleContains_AllTests(fm, viewFilter.Title, titleTrimmed)).Match)
            {
                filterShownIndexList.Add(i);
                if (match.ExactMatch && ret.titleExactMatch == null && !FMIsTopped(fm))
                {
                    ret.titleExactMatch = fm;
                }
            }
        }

        #endregion

        #region Author

        if (!viewFilter.Author.IsWhiteSpace())
        {
            for (int i = 0; i < filterShownIndexList.Count; i++)
            {
                FanMission fm = FMsViewList[filterShownIndexList[i]];

                (bool Match, bool ExactMatch) match = (false, false);

                if (!FMIsTopped(fm) &&
                    !(match = fm.Author.ContainsI_TextFilter(viewFilter.Author)).Match)
                {
                    filterShownIndexList.RemoveAt(i);
                    i--;
                }
                else
                {
                    if (match.ExactMatch && ret.authorExactMatch == null && !FMIsTopped(fm))
                    {
                        ret.authorExactMatch = fm;
                    }
                }
            }
        }

        #endregion

        #region Show unsupported

        if (!View.GetShowUnsupportedFilter())
        {
            for (int i = 0; i < filterShownIndexList.Count; i++)
            {
                FanMission fm = FMsViewList[filterShownIndexList[i]];
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
                FanMission fm = FMsViewList[filterShownIndexList[i]];
                if (GameIsKnownAndSupported(fm.Game) &&
                    (Config.GameOrganization == GameOrganization.ByTab || !FMIsTopped(fm)) &&
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
            FMCategoriesCollection andTags = viewFilter.Tags.AndTags;
            FMCategoriesCollection orTags = viewFilter.Tags.OrTags;
            FMCategoriesCollection notTags = viewFilter.Tags.NotTags;

            for (int i = 0; i < filterShownIndexList.Count; i++)
            {
                FanMission fm = FMsViewList[filterShownIndexList[i]];

                if (FMIsTopped(fm)) continue;

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
                    foreach (CatAndTagsList andTag in andTags)
                    {
                        if (!fm.Tags.TryGetValue(andTag.Category, out FMTagsCollection match))
                        {
                            andPass = false;
                            break;
                        }

                        if (andTag.Tags.Count > 0)
                        {
                            foreach (string andTagTag in andTag.Tags)
                            {
                                if (!match.Contains(andTagTag))
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
                    foreach (CatAndTagsList orTag in orTags)
                    {
                        if (!fm.Tags.TryGetValue(orTag.Category, out FMTagsCollection match))
                        {
                            continue;
                        }

                        if (orTag.Tags.Count > 0)
                        {
                            foreach (string orTagTag in orTag.Tags)
                            {
                                if (match.Contains(orTagTag))
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
                    foreach (CatAndTagsList notTag in notTags)
                    {
                        if (!fm.Tags.TryGetValue(notTag.Category, out FMTagsCollection match))
                        {
                            continue;
                        }

                        if (notTag.Tags.Count == 0)
                        {
                            notPass = false;
                            continue;
                        }

                        if (notTag.Tags.Count > 0)
                        {
                            foreach (string notTagTag in notTag.Tags)
                            {
                                if (match.Contains(notTagTag))
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

        if (viewFilter.RatingIsSet())
        {
            int rf = viewFilter.RatingFrom;
            int rt = viewFilter.RatingTo;

            for (int i = 0; i < filterShownIndexList.Count; i++)
            {
                FanMission fm = FMsViewList[filterShownIndexList[i]];
                if (!FMIsTopped(fm) &&
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
                FanMission fm = FMsViewList[filterShownIndexList[i]];
                if (!FMIsTopped(fm) &&
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
                FanMission fm = FMsViewList[filterShownIndexList[i]];
                if (!FMIsTopped(fm) &&
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
                FanMission fm = FMsViewList[filterShownIndexList[i]];
                uint fmFinished = fm.FinishedOn;
                bool fmFinishedOnUnknown = fm.FinishedOnUnknown;

                if (!FMIsTopped(fm) &&
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
                FanMission fm = FMsViewList[filterShownIndexList[i]];
                if (fm.MarkedUnavailable)
                {
                    filterShownIndexList.RemoveAt(i);
                    i--;
                }
            }
        }
        else
        {
            for (int i = 0; i < filterShownIndexList.Count; i++)
            {
                FanMission fm = FMsViewList[filterShownIndexList[i]];
                if (!fm.MarkedUnavailable)
                {
                    filterShownIndexList.RemoveAt(i);
                    i--;
                }
            }
        }

        #endregion

        return ret;
    }

    #region DML

    internal static bool AddDML(FanMission fm, string sourceDMLPath)
    {
        AssertR(GameIsDark(fm.Game), nameof(AddDML) + ": " + nameof(fm) + " is not Dark");

        if (!FMIsReallyInstalled(fm, out string installedFMPath))
        {
            LogFMInfo(fm, ErrorText.FMInstDirNF);
            Dialogs.ShowError(LText.AlertMessages.Patch_AddDML_InstallDirNotFound);
            return false;
        }

        try
        {
            string dmlFile = Path.GetFileName(sourceDMLPath);
            if (dmlFile.IsEmpty()) return false;
            File.Copy(sourceDMLPath, Path.Combine(installedFMPath, dmlFile), overwrite: true);
        }
        catch (Exception ex)
        {
            LogFMInfo(fm, ErrorText.Un + "add .dml to installed folder.", ex);
            Dialogs.ShowError(LText.AlertMessages.Patch_AddDML_UnableToAdd);
            return false;
        }

        return true;
    }

    internal static bool RemoveDML(FanMission fm, string dmlFile)
    {
        AssertR(GameIsDark(fm.Game), nameof(RemoveDML) + ": " + nameof(fm) + " is not Dark");

        if (!FMIsReallyInstalled(fm, out string installedFMPath))
        {
            LogFMInfo(fm, ErrorText.FMInstDirNF);
            Dialogs.ShowError(LText.AlertMessages.Patch_RemoveDML_InstallDirNotFound);
            return false;
        }

        try
        {
            File.Delete(Path.Combine(installedFMPath, dmlFile));
        }
        catch (Exception ex)
        {
            LogFMInfo(fm, ErrorText.Un + "remove .dml from installed folder.", ex);
            Dialogs.ShowError(LText.AlertMessages.Patch_RemoveDML_UnableToRemove);
            return false;
        }

        return true;
    }

    internal static (bool Success, List<string> DMLFiles)
    GetDMLFiles(FanMission fm)
    {
        AssertR(GameIsDark(fm.Game), nameof(GetDMLFiles) + ": " + nameof(fm) + " is not Dark");

        if (!fm.Game.ConvertsToDark(out GameIndex gameIndex)) return (false, new List<string>());

        try
        {
            List<string> dmlFiles = FastIO.GetFilesTopOnly(
                Path.Combine(Config.GetFMInstallPath(gameIndex), fm.RealInstalledDir), "*.dml",
                returnFullPaths: false);
            return (true, dmlFiles);
        }
        catch (Exception ex)
        {
            LogFMInfo(fm, ErrorText.ExGet + "DML files.", ex);
            return (false, new List<string>());
        }
    }

    #endregion

    #region Readme

    private static string GetReadmeFileFullPath(FanMission fm) =>
        FMIsReallyInstalled(fm, out string fmInstalledPath)
            ? Path.Combine(fmInstalledPath, fm.SelectedReadme)
            : Path.Combine(Paths.FMsCache, fm.InstalledDir, fm.SelectedReadme);

    internal static (string ReadmePath, ReadmeType ReadmeType)
    GetReadmeFileAndType(FanMission fm)
    {
        string readmeOnDisk = GetReadmeFileFullPath(fm);

        if (fm.SelectedReadme.ExtIsHtml()) return (readmeOnDisk, ReadmeType.HTML);
        if (fm.SelectedReadme.ExtIsGlml()) return (readmeOnDisk, ReadmeType.GLML);

        if (fm.SelectedReadme.ExtIsWri() &&
            WriConversion.IsWriFile(readmeOnDisk))
        {
            return (readmeOnDisk, ReadmeType.Wri);
        }

        // This might throw, but all calls to this method are supposed to be wrapped in a try-catch block
        using (var fs = File_OpenReadFast(readmeOnDisk))
        {
            int headerLen = RTFHeaderBytes.Length;
            // Fix: In theory, the readme could be less than headerLen bytes long and then we would throw and
            // end up with an "unable to load readme" error.
            if (fs.Length >= headerLen)
            {
                // This method can run in a thread now, so let's just allocate this locally and not be stupid
                byte[] header = new byte[headerLen];
                int bytesRead = fs.ReadAll(header, 0, headerLen);
                if (bytesRead >= headerLen && header.SequenceEqual(RTFHeaderBytes))
                {
                    return (readmeOnDisk, ReadmeType.RichText);
                }
            }
        }

        return (readmeOnDisk, ReadmeType.PlainText);
    }

#if ENABLE_README_TESTS
    internal static async void SafeReadmeIdenticalityTest()
    {
        using (var sw = new StreamWriter(@"C:\readme_test_old.txt"))
        {
            var langs = Config.LanguageNames.Keys.ToList();
            langs.Sort(StringComparer.Ordinal);

            foreach (string lang in langs)
            {
                Config.Language = lang;
                foreach (FanMission fm in FMDataIniList)
                {
                    var cache = await FMCache.GetCacheableData(fm, false);
                    string safeReadme = DetectSafeReadme(cache.Readmes, fm.Title);
                    await sw.WriteLineAsync(safeReadme);
                }
            }
            Config.Language = "English";
        }
    }

    internal static async Task ReadmeEncodingIdenticalityTest()
    {
        using (var sw = new StreamWriter(@"C:\_readme_encoding_test_new.txt"))
        {
            var fms = new List<FanMission>(FMsViewList);
            fms = fms.OrderBy(static x => x.InstalledDir, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (FanMission fm in fms)
            {
                bool wroteFMName = false;
                var cache = await FMCache.GetCacheableData(fm, false);
                string oldFMSelectedReadme = fm.SelectedReadme;
                foreach (string readme in cache.Readmes)
                {
                    fm.SelectedReadme = readme;
                    (string readmePath, ReadmeType readmeType) = GetReadmeFileAndType(fm);
                    if (readmeType == ReadmeType.PlainText)
                    {
                        if (!wroteFMName)
                        {
                            sw.WriteLine(fm.InstalledDir);
                            wroteFMName = true;
                        }
                        sw.WriteLine(readme);
                        var fe = new FMScanner.SimpleHelpers.FileEncoding();
                        using var fs = File.OpenRead(readmePath);
                        Encoding? encoding = fe.DetectFileEncoding(fs);
                        if (encoding != null)
                        {
                            sw.WriteLine(encoding.EncodingName + " (" + encoding.CodePage + ")");
                        }
                        else
                        {
                            sw.WriteLine("<null>");
                        }
                    }
                    fm.SelectedReadme = oldFMSelectedReadme;
                }
            }
        }
    }
#endif

    /// <summary>
    /// Given a list of readme filenames, attempts to find one that doesn't contain spoilers by "eyeballing"
    /// the list of names similarly to how a human would to determine the same thing.
    /// </summary>
    /// <param name="readmeFiles"></param>
    /// <param name="fmTitle"></param>
    /// <returns></returns>
    private static string DetectSafeReadme(List<string> readmeFiles, string fmTitle)
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

        static bool EndsWithLangCode(string fn_orig, string[] langCodes)
        {
            for (int i = 0; i < langCodes.Length; i++)
            {
                string langCode = langCodes[i];
                if (fn_orig.EndsWithI("_" + langCode) ||
                    fn_orig.EndsWithI("-" + langCode))
                {
                    return true;
                }
            }
            return false;
        }

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
            bool langCodesExist = TryGetLanguageCodes(Config.Language, out string[] langCodes);

            var safeReadmes = new List<string>(readmeFiles.Count);
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
                    // End original English-favoring section
                    (langCodesExist &&
                     !ContainsUnsafeOrJunkPhrase(fn) &&
                     EndsWithLangCode(fn_orig, langCodes)) ||
                    (fn.StartsWithI(StripPunctuation(fmTitle)) && !ContainsUnsafeOrJunkPhrase(fn)) ||
                    (fn.EndsWithI("Readme") && !ContainsUnsafePhrase(fn)))
                {
                    safeReadmes.Add(rf);
                }
            }

            if (safeReadmes.Count > 0)
            {
                safeReadmes.Sort(Comparers.FileNameNoExt);

                // Allocation every time, but leaving it in
                // since this method will only be called the first time an FM is loaded. So it's not worth
                // keeping this array around permanently.
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
                    if (langCodesExist && EndsWithLangCode(srNoExt, langCodes))
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

    internal static void LoadReadme(FanMission fm)
    {
        string path = "";
        try
        {
            (path, ReadmeType fileType) = GetReadmeFileAndType(fm);
            #region Debug

            // Tells me whether a readme got reloaded more than once, which should never be allowed to happen
            // due to performance concerns.
#if DEBUG || (Release_Testing && !RT_StartupOnly)
            View.SetDebug1Text(Int_TryParseInv(View.GetDebug1Text(), out int result) ? (result + 1).ToStrInv() : "1");
#endif

            #endregion

            if (fileType == ReadmeType.HTML)
            {
                View.SetReadmeState(ReadmeState.HTML);
            }
            else
            {
                View.SetReadmeState(fileType == ReadmeType.PlainText ? ReadmeState.PlainText : ReadmeState.OtherSupported);

                Encoding? encoding = null;
                if (fileType == ReadmeType.PlainText &&
                    fm.ReadmeCodePages.TryGetValue(fm.SelectedReadme, out int codePage))
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(codePage);
                    }
                    catch
                    {
                        encoding = null;
                    }
                }

                Encoding? newEncoding = View.LoadReadmeContent(path, fileType, encoding);

                Encoding? finalEncoding = newEncoding ?? encoding;

                // 0 = default, and we don't handle that - if it's default, then we'll just autodetect it
                // every time until the user explicitly requests something different.
                if (fileType == ReadmeType.PlainText && newEncoding?.CodePage > 0)
                {
                    UpdateFMSelectedReadmeCodePage(fm, newEncoding.CodePage);
                }

                if (fileType == ReadmeType.PlainText && finalEncoding != null)
                {
                    View.SetSelectedReadmeEncoding(finalEncoding);
                }
            }
        }
        catch (Exception ex)
        {
            LogFMInfo(fm,
                "Readme load failed.\r\n" +
                "FM selected readme: " + fm.SelectedReadme + "\r\n" +
                "Path: " + path,
                ex);
            View.SetReadmeState(ReadmeState.LoadError);
            View.SetReadmeLocalizableMessage(ReadmeLocalizableMessage.UnableToLoadReadme);
        }
    }

    internal static void ChangeEncodingForFMSelectedReadme(FanMission fm, int codePage)
    {
        if (codePage == -1)
        {
            Encoding? enc = View.ChangeReadmeEncoding(null);
            if (enc != null)
            {
                UpdateFMSelectedReadmeCodePage(fm, enc.CodePage);
                View.SetSelectedReadmeEncoding(enc);
            }
        }
        else
        {
            Encoding enc;
            try
            {
                enc = Encoding.GetEncoding(codePage);
            }
            catch
            {
                return;
            }

            View.SetSelectedReadmeEncoding(enc);
            View.ChangeReadmeEncoding(enc);
            UpdateFMSelectedReadmeCodePage(fm, enc.CodePage);
        }
    }

    private static void UpdateFMSelectedReadmeCodePage(FanMission fm, int codePage)
    {
        fm.ReadmeCodePages[fm.SelectedReadme] = codePage;
    }

    #endregion

    #region Open / run

    internal static void OpenFMFolder(FanMission fm)
    {
        if (!GameIsKnownAndSupported(fm.Game))
        {
            LogFMInfo(fm, ErrorText.FMGameU, stackTrace: true);
            Dialogs.ShowError(ErrorText.UnOpenFMDir);
            return;
        }

        if (!FMIsReallyInstalled(fm, out string fmDir))
        {
            LogFMInfo(fm, ErrorText.FMInstDirNF);
            Dialogs.ShowError(LText.AlertMessages.Patch_FMFolderNotFound);
            return;
        }

        try
        {
            ProcessStart_UseShellExecute(fmDir);
        }
        catch (Exception ex)
        {
            LogFMInfo(fm, ErrorText.ExTry + "open FM folder " + fmDir, ex);
            Dialogs.ShowError(ErrorText.UnOpenFMDir);
        }
    }

    internal static void OpenWebSearchUrl()
    {
        FanMission? fm = View.GetMainSelectedFMOrNull();
        if (fm == null || !fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex)) return;

        string url = Config.GetWebSearchUrl(gameIndex);

        if (!CheckUrl(url)) return;

        // Possible exceptions are:
        // ArgumentNullException (stringToEscape is null)
        // UriFormatException (The length of stringToEscape exceeds 32766 characters)
        // Those are both checked for above so we're good.
        url = Uri.EscapeUriString(url);

        if (!CheckUrl(url)) return;

        int index = url.IndexOf("$TITLE$", StringComparison.OrdinalIgnoreCase);

        try
        {
            string finalUrl = index == -1
                ? url
                : url.Substring(0, index) + Uri.EscapeDataString(fm.Title) + url.Substring(index + "$TITLE$".Length);
            ProcessStart_UseShellExecute(finalUrl);
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExOpen + "web search URL", ex);
            Dialogs.ShowError(LText.AlertMessages.WebSearchURL_ProblemOpening);
        }

        return;

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
            Log(ErrorText.UnOpenHTMLReadme + "\r\n" + fm.SelectedReadme, ex);
            Dialogs.ShowError(ErrorText.UnOpenHTMLReadme);
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
                Log(ErrorText.UnOpenHTMLReadme + "\r\n" + path, ex);
                Dialogs.ShowError(ErrorText.UnOpenHTMLReadme + path);
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
                // This one isn't important enough to put a dialog
                Log(ErrorText.ExWrite + "temp help redirect file. Using un-anchored path (help file will be positioned at top, not at requested section)...", ex);
                finalUri = helpFileUri;
            }
        }

        try
        {
            ProcessStart_UseShellExecute(finalUri);
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExOpen + "help file '" + finalUri + "'", ex);
            Dialogs.ShowError(LText.AlertMessages.Help_UnableToOpenHelpFile);
        }
    }

    internal static void OpenLink(string link, bool fixUpEmailLinks = false)
    {
        try
        {
            // The RichTextBox may send a link that's supposed to be an email but without the "mailto:" prefix,
            // so use a crappy heuristic to add it if necessary.
            if (fixUpEmailLinks && !link.StartsWithI("mailto:") && link.CharAppearsExactlyOnce('@'))
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
            Log(ErrorText.ExOpen + "link '" + link + "'", ex);
            Dialogs.ShowError(ErrorText.UnOpenLink + "\r\n\r\n" + link);
        }
    }

    internal static void OpenLogFile()
    {
        try
        {
            ProcessStart_UseShellExecute(Paths.LogFile);
        }
        catch
        {
            Dialogs.ShowAlert(ErrorText.UnOpenLogFile + "\r\n\r\n" + Paths.LogFile, LText.AlertMessages.Error);
        }
    }

    #endregion

    #region Get special exes

    /// <summary>
    /// Returns the full path of the editor for <paramref name="gameIndex"/> if it exists on disk.
    /// Otherwise, returns the empty string. It will also return the empty string if <paramref name="gameIndex"/>
    /// is not Dark.
    /// </summary>
    /// <param name="gameIndex"></param>
    /// <returns></returns>
    internal static string GetEditorExe_FromDisk(GameIndex gameIndex)
    {
        static string Fallback(string gamePath, string editorName) =>
            TryCombineFilePathAndCheckExistence(gamePath, editorName + ".exe", out string fullPathExe)
                ? fullPathExe
                : "";

        string gamePath;
        string editorName;
        if (!GameIsDark(gameIndex) ||
            (editorName = GetGameEditorName(gameIndex)).IsEmpty() ||
            (gamePath = Config.GetGamePath(gameIndex)).IsEmpty())
        {
            return "";
        }

        try
        {
            List<string> exeFiles = FastIO.GetFilesTopOnly(gamePath, "*.exe");
            for (int i = 0; i < exeFiles.Count; i++)
            {
                string exeFile = exeFiles[i];
                if (exeFile.GetFileNameFast().ContainsI(editorName))
                {
                    return exeFile;
                }
            }
        }
        catch (Exception ex)
        {
            Log(ErrorText.ExTry + "detect game editor exe\r\nGame: " + gameIndex, ex);
        }

        return Fallback(gamePath, editorName);
    }

    /// <summary>
    /// Returns the full path of Thief2MP.exe if any only if it exists on disk in the same directory as the
    /// specified Thief 2 executable. Otherwise, returns the empty string.
    /// </summary>
    /// <returns></returns>
    internal static string GetT2MultiplayerExe_FromDisk()
    {
        string gamePath = Config.GetGamePath(GameIndex.Thief2);
        return !gamePath.IsEmpty() && TryCombineFilePathAndCheckExistence(gamePath, Paths.T2MPExe, out string fullPathExe)
            ? fullPathExe
            : "";
    }

    #endregion

    #region Dropped files

    internal static bool FilesDropped(object data, [NotNullWhen(true)] out string[]? droppedItems)
    {
        if (View.UIEnabled &&
            data is string[] droppedItems_Internal &&
            AtLeastOneDroppedFileValid(droppedItems_Internal))
        {
            droppedItems = droppedItems_Internal;
            return true;
        }
        else
        {
            droppedItems = null;
            return false;
        }
    }

    private static bool AtLeastOneDroppedFileValid(string[] droppedItems)
    {
        for (int i = 0; i < droppedItems.Length; i++)
        {
            string item = droppedItems[i];
            if (!item.IsEmpty() && item.ExtIsArchive())
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    internal static (Error Error, Version? Version, string VersionString)
    GetGameVersion(GameIndex game)
    {
        string gameExe = Config.GetGameExe(game);
        if (gameExe.IsWhiteSpace()) return (Error.GameExeNotSpecified, null, "");
        if (!File.Exists(gameExe)) return (Error.GameExeNotFound, null, "");

        bool gameIsNormal = GameIsDark(game) || game == GameIndex.TDM;
        string exeToSearch;
        if (gameIsNormal)
        {
            exeToSearch = gameExe;
        }
        else
        {
            // TODO: If Sneaky.dll not found, just use the version from specified exe and don't say "Sneaky Upgrade" before it
            if (!TryCombineFilePathAndCheckExistence(Config.GetGamePath(GameIndex.Thief3), Paths.SneakyDll, out exeToSearch))
            {
                return (Error.SneakyDllNotFound, null, "");
            }
        }

        FileVersionInfo vi;
        try
        {
            vi = FileVersionInfo.GetVersionInfo(exeToSearch);
        }
        catch (FileNotFoundException)
        {
            return (gameIsNormal ? Error.GameExeNotFound : Error.SneakyDllNotFound, null, "");
        }

        Version? version;
        try
        {
            version = game == GameIndex.TDM
                ? new Version(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart)
                : new Version(vi.ProductMajorPart, vi.ProductMinorPart, vi.ProductBuildPart, vi.ProductPrivatePart);
        }
        catch
        {
            version = null;
        }

        // For some reason the TDM file version ends up being "DM_VERSION_COMPLETE" (?!) so construct it manually
        string finalVersion = game == GameIndex.TDM ? vi.GetFileVersionManual() : vi.ProductVersion;

        return finalVersion.IsEmpty() ? (Error.GameVersionNotFound, null, "") : (Error.None, version, finalVersion);
    }

    internal static Task PinOrUnpinFM(bool pin)
    {
        FanMission[] selFMs = View.GetSelectedFMs();
        if (selFMs.Length == 0) return VoidTask;

        int rowCount = View.GetRowCount();
        if (rowCount == 0) return VoidTask;

        bool singleFMSelected = selFMs.Length == 1;

        for (int i = 0; i < selFMs.Length; i++)
        {
            selFMs[i].Pinned = pin;
        }

        if (singleFMSelected) View.SetPinnedMenuState(pin);

        SelectedFM? selFM = null;
        if (!pin && rowCount > 1)
        {
            selFM = FindNearestUnselectedFM(View.GetMainSelectedRowIndex(), rowCount);
        }

        return View.SortAndSetFilter(
            selectedFM: selFM,
            keepSelection: pin,
            keepMultiSelection: !singleFMSelected && pin);
    }

    internal static SelectedFM? FindNearestUnselectedFM(int index, int rowCount)
    {
        if (rowCount <= 1) return null;

        if (index == rowCount - 1)
        {
            return View.GetFMPosInfoFromIndex(index - 1);
        }
        else
        {
            for (int i = index; i < rowCount; i++)
            {
                if (!View.RowSelected(i))
                {
                    return View.GetFMPosInfoFromIndex(i);
                }
            }
        }

        return null;
    }

    // TODO(DisplayFM/sel change/int index):
    // Looking at the logic, and testing, I'm 99% sure this index var is not actually ever needed and that
    // it always is >-1 and matches the currently selected FM/row. Can probably be removed.
    [MustUseReturnValue]
    internal static async Task<FanMission?>
    DisplayFM(int index = -1, bool refreshCache = false)
    {
        FanMission? fm = index > -1 ? View.GetFMFromIndex(index) : View.GetMainSelectedFMOrNull();
        AssertR(fm != null, nameof(fm) + " == null");
        if (fm == null) return fm;

        if (fm.NeedsScan())
        {
            if (await FMScan.ScanFMs(new List<FanMission> { fm }, suppressSingleFMProgressBoxIfFast: true))
            {
                View.RefreshFM(fm, rowOnly: true);
            }
        }

        View.UpdateAllFMUIDataExceptReadme(fm);

        if (fm.ForceReadmeReCache)
        {
            // For 7z FMs, we'll have already refreshed the cache during the scan, and the whole point is to
            // avoid doing it again
            if (!refreshCache) refreshCache = !fm.Archive.ExtIs7z();
            fm.ForceReadmeReCache = false;
            Ini.WriteFullFMDataIni();
        }

        FMCache.CacheData cacheData = await FMCache.GetCacheableData(fm, refreshCache);

        #region Readme

        List<string> readmeFiles = cacheData.Readmes;
        readmeFiles.Sort(StringComparer.Ordinal);

        if (!readmeFiles.PathContainsI(fm.SelectedReadme)) fm.SelectedReadme = "";

        View.ClearReadmesList();

        if (!fm.SelectedReadme.IsEmpty())
        {
            if (readmeFiles.Count > 1)
            {
                View.ReadmeListFillAndSelect(readmeFiles, fm.SelectedReadme);
            }
            else
            {
                View.ShowReadmeChooser(false);
            }
        }
        else // if fm.SelectedReadme is empty
        {
            switch (readmeFiles.Count)
            {
                case 0:
                    View.SetReadmeState(ReadmeState.LoadError);
                    View.SetReadmeLocalizableMessage(ReadmeLocalizableMessage.NoReadmeFound);
                    return fm;
                case > 1:
                    string safeReadme = DetectSafeReadme(readmeFiles, fm.Title);
                    if (!safeReadme.IsEmpty())
                    {
                        fm.SelectedReadme = safeReadme;
                        // @DIRSEP: Pass only fm.SelectedReadme, otherwise we might end up with un-normalized dirseps
                        View.ReadmeListFillAndSelect(readmeFiles, fm.SelectedReadme);
                    }
                    else
                    {
                        View.SetReadmeState(ReadmeState.InitialReadmeChooser, readmeFiles);
                        return fm;
                    }
                    break;
                case 1:
                    fm.SelectedReadme = readmeFiles[0];
                    View.ShowReadmeChooser(false);
                    break;
            }
        }

        View.ShowInitialReadmeChooser(false);

        LoadReadme(fm);

        #endregion

        return fm;
    }

    #region Shutdown

    internal static void UpdateConfig(
        WindowState mainWindowState,
        Size mainWindowSize,
        Point mainWindowLocation,
        float mainSplitterPercent,
        float topSplitterPercent,
        ColumnData[] columns,
        Column sortedColumn,
        SortDirection sortDirection,
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

        Array.Copy(columns, Config.Columns, ColumnCount);

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
                Config.GameTab = GameIndex.Thief1;
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
    private static void EnvironmentExitDoShutdownTasks(int exitCode)
    {
        DoShutdownTasks();
        Environment.Exit(exitCode);
    }

    // @CAN_RUN_BEFORE_VIEW_INIT
    private static void DoShutdownTasks()
    {
        GameConfigFiles.ResetGameConfigTempChanges();
    }

    internal static void Shutdown()
    {
        Ini.WriteConfigIni();
        Ini.WriteFullFMDataIni();

        DoShutdownTasks();

        ViewEnv.ApplicationExit();
    }

    #endregion

    #region Command line

    internal static void ActivateMainView()
    {
        if (View != null!)
        {
            View.ActivateThisInstance();
        }
    }

    #endregion

    internal static bool SelectedFMIsPlayable([NotNullWhen(true)] out FanMission? fm)
    {
        fm = View.GetMainSelectedFMOrNull();
        if (fm != null &&
            !View.MultipleFMsSelected() &&
            GameIsKnownAndSupported(fm.Game) &&
            !fm.MarkedUnavailable)
        {
            return true;
        }
        else
        {
            fm = null;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool FMIsTopped(FanMission fm) => fm.MarkedRecent || fm.Pinned;

    internal static string CreateMisCountMessageText(int misCount) => misCount switch
    {
        < 1 => "",
        1 => LText.StatisticsTab.MissionCount_Single,
        > 1 => LText.StatisticsTab.MissionCount_BeforeNumber +
               misCount.ToString(CultureInfo.CurrentCulture) +
               LText.StatisticsTab.MissionCount_AfterNumber
    };

    internal static void JustInTimeProcessFM(FanMission fm)
    {
        #region Languages

        if (GameSupportsLanguages(fm.Game))
        {
            if (!fm.LangsScanned)
            {
                FMLanguages.FillFMSupportedLangs(fm);
                Ini.WriteFullFMDataIni();
            }
        }
        else
        {
            fm.Langs = Language.Default;
            fm.SelectedLang = Language.Default;
            fm.LangsScanned = true;
        }

        #endregion

        #region Tags

        fm.Tags.SortAndMoveMiscToEnd();

        #endregion

        #region Mods

        if (fm.Game.ConvertsToModSupporting(out GameIndex gameIndex) && fm.DisableAllMods)
        {
            fm.DisabledMods = "";

            List<Mod> mods = Config.GetMods(gameIndex);
            for (int i = 0; i < mods.Count; i++)
            {
                Mod mod = mods[i];
                if (!mod.IsUber)
                {
                    if (!fm.DisabledMods.IsEmpty()) fm.DisabledMods += "+";
                    fm.DisabledMods += mod.InternalName;
                }
            }

            fm.DisableAllMods = false;
        }

        #endregion
    }
}

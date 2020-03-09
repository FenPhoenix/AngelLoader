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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
using AngelLoader.Forms.Import;
using AngelLoader.Importing;
using AngelLoader.WinAPI;
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

        internal static void Init(Task configTask)
        {
            bool openSettings;
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
                            string gameExe = Config.GetGameExe((GameIndex)i);
                            gameExeExists[i] = !gameExe.IsEmpty() && File.Exists(gameExe);
                            bool exe_Specified = false;
                            if (gameExeExists[i]) exe_Specified = SetGameData((GameIndex)i, storeConfigInfo: true);
                            if ((GameIndex)i == Thief2) Config.T2MPDetected = exe_Specified && !GetT2MultiplayerExe().IsEmpty();
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
                            Ini.ReadLocalizationIni(f);
                            selFound = true;
                        }
                        catch (Exception ex)
                        {
                            Log("There was an error while reading " + f + ".", ex);
                        }
                    }
                    Ini.ReadTranslatedLanguageName(f);

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
                OpenSettings(startup: true);
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
        public static async Task OpenSettings(bool startup = false)
        {
            using var sf = new SettingsForm(View, Config, startup);

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
                (!Config.FMArchivePaths.SequenceEqual(sf.OutConfig.FMArchivePaths, StringComparer.OrdinalIgnoreCase) ||
                 Config.FMArchivePathsIncludeSubfolders != sf.OutConfig.FMArchivePathsIncludeSubfolders);

            bool gamePathsChanged =
                !startup &&
                !Config.GameExes.SequenceEqual(sf.OutConfig.GameExes, StringComparer.OrdinalIgnoreCase);

            // We need these in order to decide which, if any, startup config infos to re-read
            bool[] individualGamePathsChanged = new bool[SupportedGameCount];

            for (int i = 0; i < SupportedGameCount; i++)
            {
                individualGamePathsChanged[i] =
                    !startup && !Config.GetGameExe((GameIndex)i).EqualsI(sf.OutConfig.GetGameExe((GameIndex)i));
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
            if (gamePathsChanged)
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    var game = (GameIndex)i;
                    string gameExe = Config.GetGameExe(game);
                    // Only try to reset the loader on the old game if the old game was actually specified,
                    // obviously
                    if (individualGamePathsChanged[i] && !gameExe.IsWhiteSpace())
                    {
                        // For Dark, we need to know if the game exe itself actually exists.
                        if (GameIsDark(game) && File.Exists(gameExe))
                        {
                            ResetDarkConfigFileValues(game);
                        }
                        else
                        {
                            // For Thief 3, we actually just want to know if SneakyOptions.ini exists. The game
                            // itself existing is not technically a requirement.
                            string soIni = Paths.GetSneakyOptionsIni();
                            if (!soIni.IsEmpty() && File.Exists(soIni)) FMInstallAndPlay.SetT3FMSelector(resetSelector: true);
                        }
                    }
                }
            }

            for (int i = 0; i < Config.GameExes.Length; i++)
            {
                Config.GameExes[i] = sf.OutConfig.GameExes[i];
            }

            // TODO: These should probably go in the Settings form along with the cam_mod.ini check
            // Note: SettingsForm is supposed to check these for validity, so we shouldn't have any exceptions
            //       being thrown here.

            for (int i = 0; i < SupportedGameCount; i++)
            {
                bool exe_Specified = SetGameData((GameIndex)i, storeConfigInfo: startup || individualGamePathsChanged[i]);
                if ((GameIndex)i == Thief2) Config.T2MPDetected = exe_Specified && !GetT2MultiplayerExe().IsEmpty();
            }

            #endregion

            Config.SteamExe = sf.OutConfig.SteamExe;
            Config.LaunchGamesWithSteam = sf.OutConfig.LaunchGamesWithSteam;

            for (int i = 0; i < Config.UseSteamSwitches.Length; i++)
            {
                Config.UseSteamSwitches[i] = sf.OutConfig.UseSteamSwitches[i];
            }

            Config.FMsBackupPath = sf.OutConfig.FMsBackupPath;

            Config.FMArchivePaths.ClearAndAdd(sf.OutConfig.FMArchivePaths);

            Config.FMArchivePathsIncludeSubfolders = sf.OutConfig.FMArchivePathsIncludeSubfolders;

            #endregion

            if (startup)
            {
                Config.Language = sf.OutConfig.Language;

                // We don't need to set the paths again, because we've already done so above

                Ini.WriteConfigIni(Config, Paths.ConfigIni);

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
            Config.SortedColumn = (Column)View.CurrentSortedColumnIndex;
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

            // Game paths should have been checked and verified before OK was clicked, so assume they're good here
            if (archivePathsChanged || gamePathsChanged || gameOrganizationChanged || articlesChanged ||
                daysRecentChanged)
            {
                if (archivePathsChanged || gamePathsChanged)
                {
                    if (ViewListUnscanned.Count > 0) await FMScan.ScanNewFMs();
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

        private static bool SetGameData(GameIndex game, bool storeConfigInfo)
        {
            string gameExe = Config.GetGameExe(game);
            bool gameExeSpecified = !gameExe.IsWhiteSpace();

            string gamePath = "";
            if (gameExeSpecified)
            {
                try
                {
                    gamePath = Path.GetDirectoryName(gameExe);
                }
                catch
                {
                    // ignore for now
                }
            }

            // This must come first, so below methods can use it
            Config.SetGamePath(game, gamePath);
            if (GameIsDark(game))
            {
                var data = gameExeSpecified
                    ? GetInfoFromCamModIni(gamePath, out Error _)
                    : (FMsPath: "", FMLanguage: "", FMLanguageForced: false, FMSelectorLines: new List<string>(),
                        AlwaysShowLoader: false);

                Config.SetFMInstallPath(game, data.FMsPath);
                Config.SetGameEditorDetected(game, gameExeSpecified && !GetEditorExe(game).IsEmpty());
#if false
                Config.SetPerGameFMLanguage(game, data.FMLanguage);
                Config.SetPerGameFMForcedLanguage(game, data.FMLanguageForced);
#endif

                if (storeConfigInfo)
                {
                    if (gameExeSpecified)
                    {
                        Config.SetStartupFMSelectorLines(game, data.FMSelectorLines);
                        Config.SetStartupAlwaysStartSelector(game, data.AlwaysShowLoader);
                    }
                    else
                    {
                        Config.GetStartupFMSelectorLines(game).Clear();
                        Config.SetStartupAlwaysStartSelector(game, false);
                    }
                }
            }
            else
            {
                if (gameExeSpecified)
                {
                    var t3Data = GetInfoFromSneakyOptionsIni();
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

            return gameExeSpecified;
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

            Debug.Assert(comparer != null, nameof(comparer) + "==null: column not being handled");

            // @R#_FALSE_POSITIVE
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

        #region Get info from game config files

        internal static (string FMsPath, string FMLanguage, bool FMLanguageForced,
            List<string> FMSelectorLines, bool AlwaysShowLoader)
        GetInfoFromCamModIni(string gamePath, out Error error, bool langOnly = false)
        {
            string CreateAndReturnFMsPath()
            {
                string fmsPath = Path.Combine(gamePath, "FMs");
                try
                {
                    Directory.CreateDirectory(fmsPath);
                }
                catch (Exception ex)
                {
                    Log("Exception creating FM installed base dir", ex);
                }

                return fmsPath;
            }

            string camModIni = Path.Combine(gamePath, Paths.CamModIni);

            var fmSelectorLines = new List<string>();
            bool alwaysShowLoader = false;

            if (!File.Exists(camModIni))
            {
                //error = Error.CamModIniNotFound;
                error = Error.None;
                return (!langOnly ? CreateAndReturnFMsPath() : "", "", false, fmSelectorLines, false);
            }

            string path = "";
            string fm_language = "";
            bool fm_language_forced = false;

            using (var sr = new StreamReader(camModIni))
            {
                /*
                 Conforms to the way NewDark reads it:
                 - Zero or more whitespace characters allowed at the start of the line (before the key)
                 - The key-value separator is one or more whitespace characters
                 - Keys are case-insensitive
                 - If duplicate keys exist, later ones replace earlier ones
                 - Comment lines start with ;
                 - No section headers
                */
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.IsEmpty()) continue;

                    line = line.TrimStart();

                    // Quick check; these lines will be checked more thoroughly when we go to use them
                    if (!langOnly && line.ContainsI("fm_selector")) fmSelectorLines.Add(line);
                    if (!langOnly && line.Trim().EqualsI("fm")) alwaysShowLoader = true;

                    if (line.IsEmpty() || line[0] == ';') continue;

                    if (!langOnly && line.StartsWithI(@"fm_path") && line.Length > 7 && char.IsWhiteSpace(line[7]))
                    {
                        path = line.Substring(7).Trim();
                    }
                    else if (line.StartsWithI(@"fm_language") && line.Length > 11 && char.IsWhiteSpace(line[11]))
                    {
                        fm_language = line.Substring(11).Trim();
                    }
                    else if (line.StartsWithI(@"fm_language_forced"))
                    {
                        if (line.Trim().Length == 18)
                        {
                            fm_language_forced = true;
                        }
                        else if (char.IsWhiteSpace(line[18]))
                        {
                            fm_language_forced = line.Substring(18).Trim() != "0";
                        }
                    }
                }
            }

            if (langOnly)
            {
                error = Error.None;
                return ("", fm_language, fm_language_forced, new List<string>(), false);
            }

            if (PathIsRelative(path))
            {
                try
                {
                    path = Paths.RelativeToAbsolute(gamePath, path);
                }
                catch (Exception)
                {
                    error = Error.None;
                    return (CreateAndReturnFMsPath(), fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader);
                }
            }

            error = Error.None;
            return (Directory.Exists(path) ? path : CreateAndReturnFMsPath(),
                fm_language, fm_language_forced, fmSelectorLines, alwaysShowLoader);
        }

        private static (Error Error, bool UseCentralSaves, string FMInstallPath,
            string PrevFMSelectorValue, bool AlwaysShowLoader)
        GetInfoFromSneakyOptionsIni()
        {
            string soIni = Paths.GetSneakyOptionsIni();
            Error soError = soIni.IsEmpty() ? Error.SneakyOptionsNoRegKey : !File.Exists(soIni) ? Error.SneakyOptionsNotFound : Error.None;
            if (soError != Error.None)
            {
                // Has to be MessageBox (not View.ShowAlert()) because the view may not have been created yet
                MessageBox.Show(LText.AlertMessages.Misc_SneakyOptionsIniNotFound, LText.AlertMessages.Alert, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (soError, false, "", "", false);
            }

            bool ignoreSavesKeyFound = false;
            bool ignoreSavesKey = true;

            bool fmInstPathFound = false;
            string fmInstPath = "";

            bool externSelectorFound = false;
            string prevFMSelectorValue = "";

            bool alwaysShowLoaderFound = false;
            bool alwaysShowLoader = false;

            string[] lines = File.ReadAllLines(soIni);
            for (int i = 0; i < lines.Length; i++)
            {
                string lineT = lines[i].Trim();
                if (lineT.EqualsI("[Loader]"))
                {
                    /*
                     Conforms to the way Sneaky Upgrade reads it:
                     - Whitespace allowed on both sides of section headers (but not within brackets)
                     - Section headers and keys are case-insensitive
                     - Key-value separator is '='
                     - Whitespace allowed on left side of key (but not right side before '=')
                     - Case-insensitive "true" is true, anything else is false
                     - If duplicate keys exist, the earliest one is used
                    */
                    while (i < lines.Length - 1)
                    {
                        string lt = lines[i + 1].Trim();
                        if (!ignoreSavesKeyFound &&
                            !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("IgnoreSavesKey="))
                        {
                            ignoreSavesKey = lt.Substring(lt.IndexOf('=') + 1).EqualsTrue();
                            ignoreSavesKeyFound = true;
                        }
                        else if (!fmInstPathFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("InstallPath="))
                        {
                            fmInstPath = lt.Substring(lt.IndexOf('=') + 1).Trim();
                            fmInstPathFound = true;
                        }
                        else if (!externSelectorFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("ExternSelector="))
                        {
                            prevFMSelectorValue = lt.Substring(lt.IndexOf('=') + 1).Trim();
                            externSelectorFound = true;
                        }
                        else if (!alwaysShowLoaderFound &&
                                 !lt.IsEmpty() && lt[0] != '[' && lt.StartsWithI("AlwaysShow="))
                        {
                            alwaysShowLoader = lt.Substring(lt.IndexOf('=') + 1).Trim().EqualsTrue();
                            alwaysShowLoaderFound = true;
                        }
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }

                        // TODO: @Robustness: Easy to forget to add stuff here, and I don't think we need this really
                        // as long as we're only in the Loader section, it doesn't really give a speedup I don't
                        // think
                        if (ignoreSavesKeyFound &&
                            fmInstPathFound &&
                            externSelectorFound &&
                            alwaysShowLoaderFound)
                        {
                            break;
                        }

                        i++;
                    }
                    break;
                }
            }

            return fmInstPathFound
                ? (Error.None, !ignoreSavesKey, fmInstPath, prevFMSelectorValue, alwaysShowLoader)
                : (Error.T3FMInstPathNotFound, false, "", prevFMSelectorValue, alwaysShowLoader);
        }

        #endregion

        #region Importing

        internal static async Task ImportFromDarkLoader()
        {
            string iniFile;
            bool importFMData,
                importSaves,
                importTitle,
                importSize,
                importComment,
                importReleaseDate,
                importLastPlayed,
                importFinishedOn;
            using (var f = new ImportFromDarkLoaderForm())
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                iniFile = f.DarkLoaderIniFile;
                importFMData = f.ImportFMData;
                importTitle = f.ImportTitle;
                importSize = f.ImportSize;
                importComment = f.ImportComment;
                importReleaseDate = f.ImportReleaseDate;
                importLastPlayed = f.ImportLastPlayed;
                importFinishedOn = f.ImportFinishedOn;
                importSaves = f.ImportSaves;
            }

            if (!importFMData && !importSaves)
            {
                MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
            // list when it's in an indeterminate state (which can cause a selection change (bad) and/or a visible
            // change of the list (not really bad but unprofessional looking))
            View.SetRowCount(0);

            var fields = new FieldsToImport
            {
                Title = importTitle,
                ReleaseDate = importReleaseDate,
                LastPlayed = importLastPlayed,
                Size = importSize,
                Comment = importComment,
                FinishedOn = importFinishedOn
            };

            await ImportDarkLoader.Import(iniFile, importFMData, importSaves, fields);

            // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
            await View.SortAndSetFilter(forceDisplayFM: true);
        }

        internal static async Task ImportFromNDLOrFMSel(ImportType importType)
        {
            List<string> iniFiles = new List<string>();
            bool importTitle,
                importReleaseDate,
                importLastPlayed,
                importComment,
                importRating,
                importDisabledMods,
                importTags,
                importSelectedReadme,
                importFinishedOn,
                importSize;
            using (var f = new ImportFromMultipleInisForm(importType))
            {
                if (f.ShowDialog() != DialogResult.OK) return;
                foreach (string file in f.IniFiles) iniFiles.Add(file);
                importTitle = f.ImportTitle;
                importReleaseDate = f.ImportReleaseDate;
                importLastPlayed = f.ImportLastPlayed;
                importComment = f.ImportComment;
                importRating = f.ImportRating;
                importDisabledMods = f.ImportDisabledMods;
                importTags = f.ImportTags;
                importSelectedReadme = f.ImportSelectedReadme;
                importFinishedOn = f.ImportFinishedOn;
                importSize = f.ImportSize;
            }

            if (iniFiles.All(x => x.IsWhiteSpace()))
            {
                MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
                return;
            }

            // Do this every time we modify FMsViewList in realtime, to prevent FMsDGV from redrawing from the
            // list when it's in an indeterminate state (which can cause a selection change (bad) and/or a visible
            // change of the list (not really bad but unprofessional looking))
            // We're modifying the data that FMsDGV pulls from when it redraws. This will at least prevent a
            // selection changed event from firing while we do it, as that could be really bad potentially.
            View.SetRowCount(0);

            var fields = new FieldsToImport
            {
                Title = importTitle,
                ReleaseDate = importReleaseDate,
                LastPlayed = importLastPlayed,
                Comment = importComment,
                Rating = importRating,
                DisabledMods = importDisabledMods,
                Tags = importTags,
                SelectedReadme = importSelectedReadme,
                FinishedOn = importFinishedOn,
                Size = importSize
            };

            foreach (string file in iniFiles)
            {
                if (file.IsWhiteSpace()) continue;

                bool success = await (importType == ImportType.FMSel
                    ? ImportFMSel.Import(file, fields)
                    : ImportNDL.Import(file, fields));
            }

            // Do this no matter what; because we set the row count to 0 the list MUST be refreshed
            await View.SortAndSetFilter(forceDisplayFM: true);
        }

        // TODO: Finish implementing
        #region ImportFromMultipleLoaders
        //internal static async Task ImportFromMultipleLoaders()
        //{
        //    ImportList importPriorities;
        //    string dlIniFile;
        //    bool dlImportSaves;
        //    var FMSelIniFiles = new List<string>();
        //    var NDLIniFiles = new List<string>();
        //    using (var f = new ImportFromMultipleLoadersForm())
        //    {
        //        if (f.ShowDialog() != DialogResult.OK) return;

        //        importPriorities = f.ImportPriorities.DeepCopy();
        //        dlIniFile = f.DL_IniFile;
        //        dlImportSaves = f.DL_ImportSaves;
        //        foreach (var item in f.FMSelIniFiles) FMSelIniFiles.Add(item);
        //        foreach (var item in f.NDLIniFiles) NDLIniFiles.Add(item);
        //    }

        //    var dlFields = new FieldsToImport();
        //    var fmSelFields = new FieldsToImport();
        //    var ndlFields = new FieldsToImport();

        //    #region Fill DL fields

        //    dlFields.Title = importPriorities.Title == ImportPriority.DarkLoader;
        //    dlFields.ReleaseDate = importPriorities.ReleaseDate == ImportPriority.DarkLoader;
        //    dlFields.LastPlayed = importPriorities.LastPlayed == ImportPriority.DarkLoader;
        //    dlFields.FinishedOn = importPriorities.FinishedOn == ImportPriority.DarkLoader;
        //    dlFields.Comment = importPriorities.Comment == ImportPriority.DarkLoader;
        //    dlFields.Size = importPriorities.Size == ImportPriority.DarkLoader;

        //    #endregion

        //    #region Fill FMSel fields

        //    fmSelFields.Title = importPriorities.Title == ImportPriority.FMSel;
        //    fmSelFields.ReleaseDate = importPriorities.ReleaseDate == ImportPriority.FMSel;
        //    fmSelFields.LastPlayed = importPriorities.LastPlayed == ImportPriority.FMSel;
        //    fmSelFields.FinishedOn = importPriorities.FinishedOn == ImportPriority.FMSel;
        //    fmSelFields.Comment = importPriorities.Comment == ImportPriority.FMSel;
        //    fmSelFields.Rating = importPriorities.Rating == ImportPriority.FMSel;
        //    fmSelFields.DisabledMods = importPriorities.DisabledMods == ImportPriority.FMSel;
        //    fmSelFields.Tags = importPriorities.Tags == ImportPriority.FMSel;
        //    fmSelFields.SelectedReadme = importPriorities.SelectedReadme == ImportPriority.FMSel;
        //    fmSelFields.Size = importPriorities.Size == ImportPriority.FMSel;

        //    #endregion

        //    #region Fill NDL fields

        //    ndlFields.Title = importPriorities.Title == ImportPriority.NewDarkLoader;
        //    ndlFields.ReleaseDate = importPriorities.ReleaseDate == ImportPriority.NewDarkLoader;
        //    ndlFields.LastPlayed = importPriorities.LastPlayed == ImportPriority.NewDarkLoader;
        //    ndlFields.FinishedOn = importPriorities.FinishedOn == ImportPriority.NewDarkLoader;
        //    ndlFields.Comment = importPriorities.Comment == ImportPriority.NewDarkLoader;
        //    ndlFields.Rating = importPriorities.Rating == ImportPriority.NewDarkLoader;
        //    ndlFields.DisabledMods = importPriorities.DisabledMods == ImportPriority.NewDarkLoader;
        //    ndlFields.Tags = importPriorities.Tags == ImportPriority.NewDarkLoader;
        //    ndlFields.SelectedReadme = importPriorities.SelectedReadme == ImportPriority.NewDarkLoader;
        //    ndlFields.Size = importPriorities.Size == ImportPriority.NewDarkLoader;

        //    #endregion

        //    bool importFromDL = false;
        //    bool importFromFMSel = false;
        //    bool importFromNDL = false;

        //    #region Set import bools

        //    // There's enough manual twiddling of these fields going on, so using reflection.
        //    // Not a bottleneck here.

        //    const BindingFlags bFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        //    foreach (var p in dlFields.GetType().GetFields(bFlags))
        //    {
        //        if (p.FieldType == typeof(bool) && (bool)p.GetValue(dlFields))
        //        {
        //            importFromDL = true;
        //            break;
        //        }
        //    }

        //    foreach (var p in fmSelFields.GetType().GetFields(bFlags))
        //    {
        //        if (p.FieldType == typeof(bool) && (bool)p.GetValue(fmSelFields))
        //        {
        //            importFromFMSel = true;
        //            break;
        //        }
        //    }

        //    foreach (var p in ndlFields.GetType().GetFields(bFlags))
        //    {
        //        if (p.FieldType == typeof(bool) && (bool)p.GetValue(ndlFields))
        //        {
        //            importFromNDL = true;
        //            break;
        //        }
        //    }

        //    #endregion

        //    #region Check for if nothing was selected to import

        //    if (!dlImportSaves &&
        //        ((!importFromDL && !importFromFMSel && !importFromNDL) ||
        //        (dlIniFile.IsEmpty() && FMSelIniFiles.Count == 0 && NDLIniFiles.Count == 0)))
        //    {
        //        MessageBox.Show(LText.Importing.NothingWasImported, LText.AlertMessages.Alert);
        //        return;
        //    }

        //    #endregion

        //    try
        //    {
        //        // Must do this
        //        View.SetRowCount(0);

        //        // Extremely important!
        //        ImportCommon.FMsPriority.Clear();

        //        if (importFromDL || dlImportSaves)
        //        {
        //            bool success = await ImportDarkLoader.Import(dlIniFile, true, dlImportSaves, FMDataIniList, dlFields);
        //            if (!success) return;
        //        }

        //        if (importFromFMSel)
        //        {
        //            foreach (var f in FMSelIniFiles)
        //            {
        //                if (f.IsWhiteSpace()) continue;
        //                bool success = await ImportFMSel.Import(f, FMDataIniList, fmSelFields);
        //                if (!success) return;
        //            }
        //        }

        //        if (importFromNDL)
        //        {
        //            foreach (var f in NDLIniFiles)
        //            {
        //                if (f.IsWhiteSpace()) continue;
        //                bool success = await ImportNDL.Import(f, FMDataIniList, ndlFields);
        //                if (!success) return;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        // Must do this
        //        await View.SortAndSetFilter(forceDisplayFM: true);
        //    }
        //}
        #endregion

        #endregion

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

            char[] rtfHeader = new char[6];

            // This might throw, but all calls to this method are supposed to be wrapped in a try-catch block
            using (var sr = new StreamReader(readmeOnDisk, Encoding.ASCII)) sr.ReadBlock(rtfHeader, 0, 6);

            var rType = string.Concat(rtfHeader).EqualsI(@"{\rtf1") ? ReadmeType.RichText : ReadmeType.PlainText;

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
                // Because there's no built-in way to tell it to find a key case-insensitively, we just convert
                // to lowercase cause whatever. Perf doesn't really matter here.
                string langLower = Config.Language.ToLowerInvariant();
                // Because we allow arbitrary languages, it's theoretically possible to get one that doesn't have
                // a language code.
                bool langCodeExists = LangCodes.ContainsKey(langLower);
                string langCode = langCodeExists ? LangCodes[langLower] : "";
                bool altLangCodeExists = AltLangCodes.ContainsKey(langCode);
                string altLangCode = altLangCodeExists ? AltLangCodes[langCode] : "";

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

        internal static void UpdateConfig(
            FormWindowState mainWindowState,
            Size mainWindowSize,
            Point mainWindowLocation,
            float mainSplitterPercent,
            float topSplitterPercent,
            List<ColumnData> columns,
            int sortedColumn,
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
            Config.MainWindowSize = new Size { Width = mainWindowSize.Width, Height = mainWindowSize.Height };
            Config.MainWindowLocation = new Point(mainWindowLocation.X, mainWindowLocation.Y);
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

        private static void ResetDarkConfigFileValues(GameIndex game)
        {
            FMInstallAndPlay.SetCamCfgLanguage(Config.GetGamePath(game), "");
            FMInstallAndPlay.SetDarkFMSelector(game, Config.GetGamePath(game), resetSelector: true);
        }

        internal static void Shutdown()
        {
            try
            {
                Ini.WriteConfigIni(Config, Paths.ConfigIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing config ini", ex);
            }

            Ini.WriteFullFMDataIni();

            DoShutdownTasks();

            Application.Exit();
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
            // Restore previous loader (or FMSel if all else fails) on shutdown.
            // If it fails, oh well. It's no worse than before, we just end up with ourselves as the loader,
            // and the user will get a message about that if they start the game later.
            try
            {
                for (int i = 0; i < SupportedGameCount; i++)
                {
                    string exe = Config.GetGameExe((GameIndex)i);
                    if (!exe.IsEmpty())
                    {
                        var game = (GameIndex)i;
                        if (GameIsDark(game))
                        {
                            ResetDarkConfigFileValues(game);
                        }
                        else
                        {
                            FMInstallAndPlay.SetT3FMSelector(resetSelector: true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Exception trying to write to config files to unset " + Paths.StubFileName + " as the loader on shutdown", ex);
            }
        }
    }
}

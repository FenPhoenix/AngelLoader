using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;
using AngelLoader.Forms.Import;
using AngelLoader.Importing;
using AngelLoader.WinAPI;
using FMScanner;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.DataClasses.TopRightTabEnumStatic;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.CustomControls.ProgressPanel;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal static class Core
    {
        internal static IView View;

        internal static readonly List<FanMission> FMsViewList = new List<FanMission>();
        private static readonly List<FanMission> FMDataIniList = new List<FanMission>();

        private static CancellationTokenSource ScanCts;

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
                        ReadConfigIni(Paths.ConfigIni, Config);
                        openSettings = SetPaths() == Error.BackupPathNotSpecified;
                    }
                    catch (Exception ex)
                    {
                        var message = Paths.ConfigIni + " exists but there was an error while reading it.";
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
                    var f = langFiles[i];
                    var fn = f.GetFileNameFast().RemoveExtension();
                    if (!selFound && fn.EqualsI(Config.Language))
                    {
                        try
                        {
                            ReadLocalizationIni(f);
                            selFound = true;
                        }
                        catch (Exception ex)
                        {
                            Log("There was an error while reading " + f + ".", ex);
                        }
                    }
                    ReadTranslatedLanguageName(f);

                    // These need to be set after language read. Slightly awkward but oh well.
                    //SetDefaultConfigVarNamesToLocalized();
                }

                #endregion

                if (!openSettings)
                {
                    #region Parallel load

                    using (var findFMsTask = Task.Run(() => FindFMs.Find(Config.FMInstallPaths, FMDataIniList, startup: true)))
                    {
                        // It's safe to overlap this with Find(), but not with MainForm.ctor()
                        configTask.Wait();

                        // Construct and init the view both right here, because they're both heavy operations and
                        // we want them both to run in parallel with Find() to the greatest extent possible.
                        View = new MainForm();
                        View.InitThreadable();

                        findFMsTask.Wait();
                    }

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
                View.FinishInitAndShow();
            }
        }

        public static async Task OpenSettings(bool startup = false)
        {
            using (var sf = new SettingsForm(View, Config, startup))
            {
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
                    if (startup) Environment.Exit(0);
                    return;
                }

                #region Set changed bools

                bool archivePathsChanged =
                    !startup &&
                    (!Config.FMArchivePaths.SequenceEqual(sf.OutConfig.FMArchivePaths, StringComparer.OrdinalIgnoreCase) ||
                    Config.FMArchivePathsIncludeSubfolders != sf.OutConfig.FMArchivePathsIncludeSubfolders);

                bool gamePathsChanged =
                    !startup &&
                    (!Config.T1Exe.EqualsI(sf.OutConfig.T1Exe) ||
                    !Config.T2Exe.EqualsI(sf.OutConfig.T2Exe) ||
                    !Config.T3Exe.EqualsI(sf.OutConfig.T3Exe));

                bool gameOrganizationChanged =
                    !startup && (Config.GameOrganization != sf.OutConfig.GameOrganization);

                bool articlesChanged =
                    !startup &&
                    (Config.EnableArticles != sf.OutConfig.EnableArticles ||
                    !Config.Articles.SequenceEqual(sf.OutConfig.Articles, StringComparer.InvariantCultureIgnoreCase) ||
                    Config.MoveArticlesToEnd != sf.OutConfig.MoveArticlesToEnd);

                bool dateFormatChanged =
                    !startup &&
                    (Config.DateFormat != sf.OutConfig.DateFormat ||
                    Config.DateCustomFormatString != sf.OutConfig.DateCustomFormatString);

                bool ratingDisplayStyleChanged =
                    !startup &&
                    (Config.RatingDisplayStyle != sf.OutConfig.RatingDisplayStyle ||
                    Config.RatingUseStars != sf.OutConfig.RatingUseStars);

                bool languageChanged =
                    !startup && !Config.Language.EqualsI(sf.OutConfig.Language);

                bool useFixedFontChanged =
                    !startup && Config.ReadmeUseFixedWidthFont != sf.OutConfig.ReadmeUseFixedWidthFont;

                #endregion

                #region Set config data

                // Set values individually (rather than deep-copying) so that non-Settings values don't get
                // overwritten.

                #region Paths tab

                #region Game exes

                Config.T1Exe = sf.OutConfig.T1Exe;
                Config.T2Exe = sf.OutConfig.T2Exe;
                Config.T3Exe = sf.OutConfig.T3Exe;

                // TODO: These should probably go in the Settings form along with the cam_mod.ini check
                // Note: SettingsForm is supposed to check these for validity, so we shouldn't have any exceptions
                //       being thrown here.
                Config.SetT1FMInstPath(!Config.T1Exe.IsWhiteSpace()
                    ? GetInstFMsPathFromCamModIni(Path.GetDirectoryName(Config.T1Exe), out Error _)
                    : "");
                Config.T1DromEdDetected = !GetDromEdExe(Game.Thief1).IsEmpty();

                Config.SetT2FMInstPath(!Config.T2Exe.IsWhiteSpace()
                    ? GetInstFMsPathFromCamModIni(Path.GetDirectoryName(Config.T2Exe), out Error _)
                    : "");
                Config.T2DromEdDetected = !GetDromEdExe(Game.Thief2).IsEmpty();

                Config.T2MPDetected = !GetT2MultiplayerExe().IsEmpty();

                if (!Config.T3Exe.IsWhiteSpace())
                {
                    var (error, useCentralSaves, t3FMInstPath) = GetInstFMsPathFromT3();
                    if (error == Error.None)
                    {
                        Config.SetT3FMInstPath(t3FMInstPath);
                        Config.T3UseCentralSaves = useCentralSaves;
                    }
                }
                else
                {
                    Config.SetT3FMInstPath("");
                }

                #endregion

                Config.SteamExe = sf.OutConfig.SteamExe;
                Config.LaunchGamesWithSteam = sf.OutConfig.LaunchGamesWithSteam;
                Config.T1UseSteam = sf.OutConfig.T1UseSteam;
                Config.T2UseSteam = sf.OutConfig.T2UseSteam;
                Config.T3UseSteam = sf.OutConfig.T3UseSteam;

                Config.FMsBackupPath = sf.OutConfig.FMsBackupPath;

                Config.FMArchivePaths.ClearAndAdd(sf.OutConfig.FMArchivePaths);

                Config.FMArchivePathsIncludeSubfolders = sf.OutConfig.FMArchivePathsIncludeSubfolders;

                #endregion

                if (startup)
                {
                    Config.Language = sf.OutConfig.Language;

                    // We don't need to set the paths again, because we've already done so above
#if DEBUG
                    var checkPaths = SetPaths();
                    Debug.Assert(checkPaths == Error.None, "checkPaths returned an error the second time");
#endif

                    WriteConfigIni(Config, Paths.ConfigIni);

                    // We have to do this here because we won't have before
                    using (var findFMsTask = Task.Run(() => FindFMs.Find(Config.FMInstallPaths, FMDataIniList, startup: true)))
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

                // For clarity, don't copy the other tabs' data on startup, because their tabs won't be shown and
                // so they won't have been changed

                #region FM Display tab

                Config.GameOrganization = sf.OutConfig.GameOrganization;

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
                    FindFMs.Find(Config.FMInstallPaths, FMDataIniList);
                }
                if (gameOrganizationChanged)
                {
                    // Clear everything to defaults so we don't have any leftover state screwing things all up
                    Config.ClearAllSelectedFMs();
                    Config.ClearAllFilters();
                    Config.GameTab = Game.Thief1;
                    View.ClearAllUIAndInternalFilters();
                    if (Config.GameOrganization == GameOrganization.ByTab) Config.Filter.Games = Game.Thief1;
                    View.ChangeGameOrganization();
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

                // Game paths should have been checked and verified before OK was clicked, so assume they're good
                // here
                if (archivePathsChanged || gamePathsChanged || gameOrganizationChanged || articlesChanged)
                {
                    if (archivePathsChanged || gamePathsChanged)
                    {
                        if (ViewListGamesNull.Count > 0) await ScanNewFMsForGameType();
                    }

                    // TODO: forceDisplayFM is always true so that this always works, but it could be smarter
                    // If I store the selected FM up above the Find(), I can make the FM not have to reload if
                    // it's still selected
                    await View.SortAndSetFilter(keepSelection: !gameOrganizationChanged, forceDisplayFM: true);
                }
                else if (dateFormatChanged || languageChanged)
                {
                    View.RefreshFMsListKeepSelection();
                }

                #endregion
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
            var comparer =
                column == Column.Game ? new FMGameComparer(sortDirection) :
                column == Column.Installed ? new FMInstalledComparer(sortDirection) :
                column == Column.Title ? new FMTitleComparer(sortDirection) :
                column == Column.Archive ? new FMArchiveComparer(sortDirection) :
                column == Column.Author ? new FMAuthorComparer(sortDirection) :
                column == Column.Size ? new FMSizeComparer(sortDirection) :
                column == Column.Rating ? new FMRatingComparer(sortDirection) :
                column == Column.Finished ? new FMFinishedComparer(sortDirection) :
                column == Column.ReleaseDate ? new FMReleaseDateComparer(sortDirection) :
                column == Column.LastPlayed ? new FMLastPlayedComparer(sortDirection) :
                column == Column.DisabledMods ? new FMDisabledModsComparer(sortDirection) :
                column == Column.Comment ? new FMCommentComparer(sortDirection) :
                (IComparer<FanMission>)null;

            Debug.Assert(comparer != null, nameof(comparer) + "==null: column not being handled");

            FMsViewList.Sort(comparer);
        }

        private static Error SetPaths()
        {
            // TODO: These single-error returns don't work, change to something else

            // PERF: 9ms, but it's mostly IO. Darn.
            var t1Exists = !Config.T1Exe.IsEmpty() && File.Exists(Config.T1Exe);
            var t2Exists = !Config.T2Exe.IsEmpty() && File.Exists(Config.T2Exe);
            var t3Exists = !Config.T3Exe.IsEmpty() && File.Exists(Config.T3Exe);

            if (t1Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T1Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                Config.T1DromEdDetected = !GetDromEdExe(Game.Thief1).IsEmpty();
                //if (error == Error.CamModIniNotFound) return Error.T1CamModIniNotFound;
                Config.SetT1FMInstPath(gameFMsPath);
            }
            if (t2Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T2Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                Config.T2DromEdDetected = !GetDromEdExe(Game.Thief2).IsEmpty();
                Config.T2MPDetected = !GetT2MultiplayerExe().IsEmpty();
                //if (error == Error.CamModIniNotFound) return Error.T2CamModIniNotFound;
                Config.SetT2FMInstPath(gameFMsPath);
            }
            if (t3Exists)
            {
                var (error, useCentralSaves, path) = GetInstFMsPathFromT3();
                //if (error != Error.None) return error;
                Config.SetT3FMInstPath(path);
                Config.T3UseCentralSaves = useCentralSaves;
            }

            return
                // Must be first, otherwise other stuff overrides it and then we don't act on it
                !Directory.Exists(Config.FMsBackupPath) ? Error.BackupPathNotSpecified :
                !t1Exists && !t2Exists && !t3Exists ? Error.NoGamesSpecified :
                Error.None;
        }

        #region Get FM install paths

        private static string GetInstFMsPathFromCamModIni(string gamePath, out Error error)
        {
            string CreateAndReturn(string fmsPath)
            {
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

            var camModIni = Path.Combine(gamePath, "cam_mod.ini");

            if (!File.Exists(camModIni))
            {
                //error = Error.CamModIniNotFound;
                //return null;
                error = Error.None;
                return CreateAndReturn(Path.Combine(gamePath, "FMs"));
            }

            string path = null;

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

                    if (line.IsEmpty() || line[0] == ';') continue;

                    if (line.StartsWithI(@"fm_path") && line.Length > 7 && char.IsWhiteSpace(line[7]))
                    {
                        path = line.Substring(7).Trim();
                    }
                }
            }

            if (!path.IsEmpty() &&
                (path.StartsWithFast_NoNullChecks(".\\") || path.StartsWithFast_NoNullChecks("..\\") ||
                path.StartsWithFast_NoNullChecks("./") || path.StartsWithFast_NoNullChecks("../")))
            {
                try
                {
                    path = Paths.RelativeToAbsolute(gamePath, path);
                }
                catch (Exception)
                {
                    error = Error.None;
                    return CreateAndReturn(Path.Combine(gamePath, "FMs"));
                }
            }

            error = Error.None;
            return Directory.Exists(path) ? path : CreateAndReturn(Path.Combine(gamePath, "FMs"));
        }

        private static (Error Error, bool UseCentralSaves, string Path)
        GetInstFMsPathFromT3()
        {
            var soIni = Paths.GetSneakyOptionsIni();
            var soError = soIni.IsEmpty() ? Error.SneakyOptionsNoRegKey : !File.Exists(soIni) ? Error.SneakyOptionsNotFound : Error.None;
            if (soError != Error.None)
            {
                // Has to be MessageBox (not View.ShowAlert()) because the view may not have been created yet
                MessageBox.Show(LText.AlertMessages.Misc_SneakyOptionsIniNotFound, LText.AlertMessages.Alert, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (soError, false, null);
            }

            bool ignoreSavesKeyFound = false;
            bool ignoreSavesKey = true;

            bool fmInstPathFound = false;
            string fmInstPath = "";

            var lines = File.ReadAllLines(soIni);
            for (var i = 0; i < lines.Length; i++)
            {
                var lineT = lines[i].Trim();
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
                        var lt = lines[i + 1].Trim();
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
                        else if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']')
                        {
                            break;
                        }

                        if (ignoreSavesKeyFound && fmInstPathFound) break;

                        i++;
                    }
                    break;
                }
            }

            return fmInstPathFound
                ? (Error.None, !ignoreSavesKey, fmInstPath)
                : (Error.T3FMInstPathNotFound, false, null);
        }

        #endregion

        #region Scan

        // Super quick-n-cheap hack for perf
        internal static readonly List<int> ViewListGamesNull = new List<int>();

        internal static async Task ScanFMAndRefresh(FanMission fm, ScanOptions scanOptions = null)
        {
            if (scanOptions == null) scanOptions = GetDefaultScanOptions();
            bool success = await ScanFM(fm, scanOptions);
            if (success) await View.RefreshSelectedFM(refreshReadme: false);
        }

        internal static Task<bool> ScanFM(FanMission fm, ScanOptions scanOptions) => ScanFMs(new List<FanMission> { fm }, scanOptions);

        internal static async Task<bool> ScanFMs(List<FanMission> fmsToScan, ScanOptions scanOptions, bool markAsScanned = true)
        {
            if (fmsToScan == null || fmsToScan.Count == 0 || (fmsToScan.Count == 1 && fmsToScan[0] == null))
            {
                return false;
            }

            var scanningOne = fmsToScan.Count == 1;

            #region Show progress box or block UI thread

            try
            {
                if (scanningOne)
                {
                    Log(nameof(ScanFMs) + ": Scanning one", methodName: false);
                    // Just use a cheap check and throw up the progress box for .7z files, otherwise not. Not as
                    // nice as the timer method, but that can cause race conditions I don't know how to fix, so
                    // whatever.
                    if (fmsToScan[0].Archive.ExtIs7z())
                    {
                        View.ShowProgressBox(ProgressTasks.ScanAllFMs);
                    }
                    else
                    {
                        // Block user input to the form to mimic the UI thread being blocked, because we're async
                        // here
                        View.Block(true);
                        // Doesn't actually show the box, but shows the meter on the taskbar I guess?
                        View.ShowProgressBox(ProgressTasks.ScanAllFMs, suppressShow: true);
                    }
                }
                else
                {
                    View.ShowProgressBox(ProgressTasks.ScanAllFMs);
                }

                #endregion

                void ReportProgress(ProgressReport pr)
                {
                    var fmIsZip = pr.FMName.ExtIsArchive();
                    var name = fmIsZip ? pr.FMName.GetFileNameFast() : pr.FMName.GetDirNameFast();
                    View.ReportScanProgress(pr.FMNumber, pr.FMsTotal, pr.Percent, name);
                }

                #region Init

                ScanCts = new CancellationTokenSource();

                var fms = new List<string>();

                Log(nameof(ScanFMs) + ": about to call " + nameof(GetFMArchivePaths) + " with subfolders=" +
                    Config.FMArchivePathsIncludeSubfolders);

                // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                // cause then it will hit the actual disk rather than just going through a list of paths in
                // memory
                var archivePaths = await Task.Run(GetFMArchivePaths);

                #endregion

                #region Filter out invalid FMs from scan list

                // Safety net to guarantee that the in and out lists will have the same count and order
                var fmsToScanFiltered = new List<FanMission>();

                for (var i = 0; i < fmsToScan.Count; i++)
                {
                    var fm = fmsToScan[i];
                    var fmArchivePath = await Task.Run(() => FindFMArchive(fm, archivePaths));
                    if (!fm.Archive.IsEmpty() && !fmArchivePath.IsEmpty())
                    {
                        fmsToScanFiltered.Add(fm);
                        fms.Add(fmArchivePath);
                    }
                    else if (GameIsKnownAndSupported(fm))
                    {
                        var fmInstalledPath = GetFMInstallsBasePath(fm.Game);
                        if (!fmInstalledPath.IsEmpty())
                        {
                            fmsToScanFiltered.Add(fm);
                            fms.Add(Path.Combine(fmInstalledPath, fm.InstalledDir));
                        }
                    }

                    if (ScanCts.IsCancellationRequested) return false;
                }

                if (fmsToScanFiltered.Count == 0) return false;

                #endregion

                #region Run scanner

                List<ScannedFMData> fmDataList;
                try
                {
                    var progress = new Progress<ProgressReport>(ReportProgress);

                    await Task.Run(() => Paths.PrepareTempPath(Paths.FMScannerTemp));

                    using (var scanner = new Scanner())
                    {
                        scanner.LogFile = Paths.ScannerLogFile;
                        scanner.ZipEntryNameEncoding = Encoding.UTF8;

                        fmDataList = await scanner.ScanAsync(fms, Paths.FMScannerTemp, scanOptions, progress, ScanCts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    return false;
                }

                #endregion

                #region Copy scanned data to FMs

                for (var i = 0; i < fmsToScanFiltered.Count; i++)
                {
                    var scannedFM = fmDataList[i];

                    #region Checks

                    if (scannedFM == null)
                    {
                        // We need to return fail for scanning one, else we get into an infinite loop because of
                        // a refresh that gets called in that case
                        if (scanningOne)
                        {
                            Log(nameof(ScanFMs) + " (one) scanned FM was null. FM was:\r\n" +
                                "Archive: " + fmsToScanFiltered[0].Archive + "\r\n" +
                                "InstalledDir: " + fmsToScanFiltered[0].InstalledDir,
                                methodName: false);
                            return false;
                        }
                        continue;
                    }

                    var sel = fmsToScanFiltered[i];
                    if (sel == null)
                    {
                        // Same as above (this should never happen now, but hey)
                        if (scanningOne) return false;
                        continue;
                    }

                    #endregion

                    #region Set FM fields

                    var gameSup = scannedFM.Game != Games.Unsupported;

                    if (scanOptions.ScanTitle)
                    {
                        sel.Title =
                            !scannedFM.Title.IsEmpty() ? scannedFM.Title
                            : scannedFM.ArchiveName.ExtIsArchive() ? scannedFM.ArchiveName.RemoveExtension()
                            : scannedFM.ArchiveName;

                        if (gameSup)
                        {
                            sel.AltTitles.ClearAndAdd(sel.Title);
                            sel.AltTitles.AddRange(scannedFM.AlternateTitles);
                        }
                        else
                        {
                            sel.AltTitles.Clear();
                        }
                    }

                    if (scanOptions.ScanSize)
                    {
                        sel.SizeBytes = (ulong)(gameSup ? scannedFM.Size ?? 0 : 0);
                    }
                    if (scanOptions.ScanReleaseDate)
                    {
                        sel.ReleaseDate = gameSup ? scannedFM.LastUpdateDate : null;
                    }
                    if (scanOptions.ScanCustomResources)
                    {
                        sel.HasMap = gameSup ? scannedFM.HasMap : null;
                        sel.HasAutomap = gameSup ? scannedFM.HasAutomap : null;
                        sel.HasScripts = gameSup ? scannedFM.HasCustomScripts : null;
                        sel.HasTextures = gameSup ? scannedFM.HasCustomTextures : null;
                        sel.HasSounds = gameSup ? scannedFM.HasCustomSounds : null;
                        sel.HasObjects = gameSup ? scannedFM.HasCustomObjects : null;
                        sel.HasCreatures = gameSup ? scannedFM.HasCustomCreatures : null;
                        sel.HasMotions = gameSup ? scannedFM.HasCustomMotions : null;
                        sel.HasMovies = gameSup ? scannedFM.HasMovies : null;
                        sel.HasSubtitles = gameSup ? scannedFM.HasCustomSubtitles : null;
                    }

                    if (scanOptions.ScanAuthor)
                    {
                        sel.Author = gameSup ? scannedFM.Author : "";
                    }

                    if (scanOptions.ScanGameType)
                    {
                        sel.Game =
                            scannedFM.Game == Games.Unsupported ? Game.Unsupported :
                            scannedFM.Game == Games.TDP ? Game.Thief1 :
                            scannedFM.Game == Games.TMA ? Game.Thief2 :
                            scannedFM.Game == Games.TDS ? Game.Thief3 :
                            Game.Null;
                    }

                    if (scanOptions.ScanLanguages)
                    {
                        sel.Languages = gameSup ? scannedFM.Languages : new string[0];
                        sel.LanguagesString = gameSup
                            ? scannedFM.Languages != null ? string.Join(", ", scannedFM.Languages) : ""
                            : "";
                    }

                    if (scanOptions.ScanTags)
                    {
                        sel.TagsString = gameSup ? scannedFM.TagsString : "";

                        // Don't clear the tags, because the user could have added a bunch and we should only
                        // add to those, not overwrite them
                        if (gameSup) AddTagsToFMAndGlobalList(sel.TagsString, sel.Tags);
                    }

                    sel.MarkedScanned = markAsScanned;

                    #endregion
                }

                #endregion

                WriteFullFMDataIni();
            }
            catch (Exception ex)
            {
                Log("Exception in ScanFMs", ex);
                var message = scanningOne
                    ? LText.AlertMessages.Scan_ExceptionInScanOne
                    : LText.AlertMessages.Scan_ExceptionInScanMultiple;
                View.ShowAlert(message, LText.AlertMessages.Error);
                return false;
            }
            finally
            {
                ScanCts?.Dispose();
                View.Block(false);
                View.HideProgressBox();
            }

            return true;
        }

        internal static void CancelScan() => ScanCts.CancelIfNotDisposed();

        internal static async Task ScanNewFMsForGameType()
        {
            var fmsToScan = new List<FanMission>();

            try
            {
                // NOTE: We use FMDataIniList index because that's the list that the indexes are pulled from!
                // (not FMsViewList)
                foreach (var index in ViewListGamesNull) fmsToScan.Add(FMDataIniList[index]);
            }
            catch
            {
                // Cheap fallback in case something goes wrong, because what we're doing is a little iffy
                fmsToScan.Clear();
                // Since we're doing it manually here, we can pull from FMsViewList for perf (it'll be the same
                // size or smaller than FMDataIniList)
                foreach (var fm in FMsViewList) if (fm.Game == Game.Null) fmsToScan.Add(fm);
            }
            finally
            {
                // Critical that this gets cleared immediately after use!
                ViewListGamesNull.Clear();
            }

            if (fmsToScan.Count > 0)
            {
                try
                {
                    await ScanFMs(fmsToScan, ScanOptions.FalseDefault(scanGameType: true), markAsScanned: false);
                }
                catch (Exception ex)
                {
                    Log("Exception in ScanFMs", ex);
                }
            }
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

            await ImportDarkLoader.Import(iniFile, importFMData, importSaves, FMDataIniList, fields);

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
                foreach (var file in f.IniFiles) iniFiles.Add(file);
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

            foreach (var file in iniFiles)
            {
                if (file.IsWhiteSpace()) continue;

                bool success = await (importType == ImportType.FMSel
                    ? ImportFMSel.Import(file, FMDataIniList, fields)
                    : ImportNDL.Import(file, FMDataIniList, fields));
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

        internal static async Task ScanAndFind(List<FanMission> fms, ScanOptions scanOptions)
        {
            if (fms.Count == 0) return;

            await ScanFMs(fms, scanOptions);
            // TODO: Why am I doing a find after a scan?!?!?! WTF use is this?
            // Note: I might be doing it to get rid of any duplicates or bad data that may have been imported?
            FindFMs.Find(Config.FMInstallPaths, FMDataIniList);
        }

        #endregion

        internal static async Task FindNewFMsAndScanForGameType()
        {
            FindFMs.Find(Config.FMInstallPaths, FMDataIniList);
            // This await call takes 15ms just to make the call alone(?!) so don't do it unless we have to
            if (ViewListGamesNull.Count > 0) await ScanNewFMsForGameType();
        }

        #region Audio conversion (mainly for pre-checks)

        internal static async Task ConvertOGGsToWAVs(FanMission fm)
        {
            if (!fm.Installed || !GameIsDark(fm)) return;

            Debug.Assert(fm.Game != Game.Null, "fm.Game is Game.Null");

            var gameExe = GetGameExeFromGameType(fm.Game);
            var gameName = GetGameNameFromGameType(fm.Game);
            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.FileConversion_GameIsRunning,
                    LText.AlertMessages.Alert);
                return;
            }

            if (!FMIsReallyInstalled(fm))
            {
                var yes = View.AskToContinue(LText.AlertMessages.Misc_FMMarkedInstalledButNotInstalled,
                    LText.AlertMessages.Alert);
                if (yes)
                {
                    fm.Installed = false;
                    await View.RefreshSelectedFM(refreshReadme: false);
                }
                return;
            }

            Debug.Assert(fm.Installed, "fm is not installed");

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm.Game));
            try
            {
                View.ShowProgressBox(ProgressTasks.ConvertFiles);
                await ac.ConvertOGGsToWAVs();
            }
            finally
            {
                View.HideProgressBox();
            }
        }

        internal static async Task ConvertWAVsTo16Bit(FanMission fm)
        {
            if (!fm.Installed || !GameIsDark(fm)) return;

            Debug.Assert(fm.Game != Game.Null, "fm.Game is Game.Null");

            var gameExe = GetGameExeFromGameType(fm.Game);
            var gameName = GetGameNameFromGameType(fm.Game);
            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.FileConversion_GameIsRunning,
                    LText.AlertMessages.Alert);
                return;
            }

            if (!FMIsReallyInstalled(fm))
            {
                var yes = View.AskToContinue(LText.AlertMessages.Misc_FMMarkedInstalledButNotInstalled,
                    LText.AlertMessages.Alert);
                if (yes)
                {
                    fm.Installed = false;
                    await View.RefreshSelectedFM(refreshReadme: false);
                }
                return;
            }

            Debug.Assert(fm.Installed, "fm is not installed");

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm.Game));
            try
            {
                View.ShowProgressBox(ProgressTasks.ConvertFiles);
                await ac.ConvertWAVsTo16Bit();
            }
            finally
            {
                View.HideProgressBox();
            }
        }

        #endregion

        #region DML

        internal static bool AddDML(FanMission fm, string sourceDMLPath)
        {
            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_AddDML_InstallDirNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var installedFMPath = Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir);
            try
            {
                var dmlFile = Path.GetFileName(sourceDMLPath);
                if (dmlFile == null) return false;
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
            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_RemoveDML_InstallDirNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var installedFMPath = Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir);
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
            try
            {
                var dmlFiles = FastIO.GetFilesTopOnly(Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir), "*.dml");
                for (int i = 0; i < dmlFiles.Count; i++) dmlFiles[i] = dmlFiles[i].GetFileNameFast();
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

        private static string GetReadmeFileFullPath(FanMission fm)
        {
            return FMIsReallyInstalled(fm)
                ? Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir, fm.SelectedReadme)
                : Path.Combine(Paths.FMsCache, fm.InstalledDir, fm.SelectedReadme);
        }

        internal static (string ReadmePath, ReadmeType ReadmeType)
        GetReadmeFileAndType(FanMission fm)
        {
            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var readmeOnDisk = GetReadmeFileFullPath(fm);

            if (fm.SelectedReadme.ExtIsHtml()) return (readmeOnDisk, ReadmeType.HTML);
            if (fm.SelectedReadme.ExtIsGlml()) return (readmeOnDisk, ReadmeType.GLML);

            var rtfHeader = new char[6];

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

            string StripPunctuation(string str)
            {
                return str.Replace(" ", "").Replace("-", "").Replace("_", "").Replace(".", "").Replace(",", "")
                    .Replace(";", "").Replace("'", "");
            }

            bool allEqual = true;
            for (var i = 0; i < readmeFiles.Count; i++)
            {
                var rf = readmeFiles[i];
                if (rf == null) continue;

                if (i > 0 && !StripPunctuation(Path.GetFileNameWithoutExtension(readmeFiles[i]))
                        .EqualsI(StripPunctuation(Path.GetFileNameWithoutExtension(readmeFiles[i - 1]))))
                {
                    allEqual = false;
                    break;
                }
            }

            string FirstByPreferredFormat(List<string> files)
            {
                // Don't use IsValidReadme(), because we want a specific search order
                return
                    files.FirstOrDefault(x => x.ExtIsGlml()) ??
                    files.FirstOrDefault(x => x.ExtIsRtf()) ??
                    files.FirstOrDefault(x => x.ExtIsTxt()) ??
                    files.FirstOrDefault(x => x.ExtIsWri()) ??
                    files.FirstOrDefault(x => x.ExtIsHtml());
            }

            bool ContainsUnsafePhrase(string str)
            {
                return str.ContainsI("loot") ||
                       str.ContainsI("walkthrough") ||
                       str.ContainsI("walkthru") ||
                       str.ContainsI("secret") ||
                       str.ContainsI("spoiler") ||
                       str.ContainsI("tips") ||
                       str.ContainsI("convo") ||
                       str.ContainsI("conversation") ||
                       str.ContainsI("cheat") ||
                       str.ContainsI("notes");
            }

            bool ContainsUnsafeOrJunkPhrase(string str)
            {
                return ContainsUnsafePhrase(str) ||
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
            }

            var safeReadme = "";
            if (allEqual)
            {
                safeReadme = FirstByPreferredFormat(readmeFiles);
            }
            else
            {

                var safeReadmes = new List<string>();
                foreach (var rf in readmeFiles)
                {
                    if (rf == null) continue;

                    var fn = StripPunctuation(Path.GetFileNameWithoutExtension(rf));

                    if (fn.EqualsI("Readme") || fn.EqualsI("ReadmeEn") || fn.EqualsI("ReadmeEng") ||
                        fn.EqualsI("FMInfo") || fn.EqualsI("FMInfoEn") || fn.EqualsI("FMInfoEng") ||
                        fn.EqualsI("fm") || fn.EqualsI("fmEn") || fn.EqualsI("fmEng") ||
                        fn.EqualsI("GameInfo") || fn.EqualsI("GameInfoEn") || fn.EqualsI("GameInfoEng") ||
                        fn.EqualsI("Mission") || fn.EqualsI("MissionEn") || fn.EqualsI("MissionEng") ||
                        fn.EqualsI("MissionInfo") || fn.EqualsI("MissionInfoEn") || fn.EqualsI("MissionInfoEng") ||
                        fn.EqualsI("Info") || fn.EqualsI("InfoEn") || fn.EqualsI("InfoEng") ||
                        fn.EqualsI("Entry") || fn.EqualsI("EntryEn") || fn.EqualsI("EntryEng") ||
                        fn.EqualsI("English") ||
                        (fn.StartsWithI(StripPunctuation(fmTitle)) && !ContainsUnsafeOrJunkPhrase(fn)) ||
                        (fn.EndsWithI("Readme") && !ContainsUnsafePhrase(fn)))
                    {
                        safeReadmes.Add(rf);
                    }
                }

                if (safeReadmes.Count > 0)
                {
                    safeReadmes.Sort(new FileNameNoExtComparer());

                    var eng = safeReadmes.FirstOrDefault(
                        x => Path.GetFileNameWithoutExtension(x).EndsWithI("en") ||
                             Path.GetFileNameWithoutExtension(x).EndsWithI("eng"));
                    foreach (var item in new[] { "readme", "fminfo", "fm", "gameinfo", "mission", "missioninfo", "info", "entry" })
                    {
                        var str = safeReadmes.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).EqualsI(item));
                        if (str != null)
                        {
                            safeReadmes.Remove(str);
                            safeReadmes.Insert(0, str);
                        }
                    }
                    if (eng != null)
                    {
                        safeReadmes.Remove(eng);
                        safeReadmes.Insert(0, eng);
                    }
                    safeReadme = FirstByPreferredFormat(safeReadmes);
                }
            }

            if (safeReadme.IsEmpty())
            {
                int numSafe = 0;
                int safeIndex = -1;
                for (var i = 0; i < readmeFiles.Count; i++)
                {
                    var rf = readmeFiles[i];
                    if (rf == null) continue;

                    var fn = StripPunctuation(Path.GetFileNameWithoutExtension(rf));
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
            var installsBasePath = GetFMInstallsBasePath(fm.Game);
            string fmDir;
            if (installsBasePath.IsEmpty() || !Directory.Exists(fmDir = Path.Combine(installsBasePath, fm.InstalledDir)))
            {
                View.ShowAlert(LText.AlertMessages.Patch_FMFolderNotFound, LText.AlertMessages.Alert);
                return;
            }

            try
            {
                Process.Start(fmDir);
            }
            catch (Exception ex)
            {
                Log("Exception trying to open FM folder " + fmDir, ex);
            }
        }

        internal static void OpenWebSearchUrl(FanMission fm)
        {
            var url = Config.WebSearchUrl;
            if (url.IsWhiteSpace() || url.Length > 32766) return;

            var index = url.IndexOf("$TITLE$", StringComparison.OrdinalIgnoreCase);

            var finalUrl = Uri.EscapeUriString(index == -1
                ? url
                : url.Substring(0, index) + fm.Title + url.Substring(index + "$TITLE$".Length));

            try
            {
                Process.Start(finalUrl);
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
                    Process.Start(path);
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
                Process.Start(link);
            }
            catch (Exception ex)
            {
                Log("Problem opening clickable link from rtfbox", ex);
            }
        }

        #endregion

        #region Add/remove tag

        internal static List<string> ListMatchingTags(string searchText)
        {
            // Smartasses who try to break it get nothing
            if (searchText.CountChars(':') > 1 || searchText.IsWhiteSpace()) return null;

            (string First, string Second) text;

            var index = searchText.IndexOf(':');
            if (index > -1)
            {
                text.First = searchText.Substring(0, index).Trim();
                text.Second = searchText.Substring(index + 1).Trim();
            }
            else
            {
                text.First = searchText.Trim();
                text.Second = "";
            }

            // Shut up, it works
            var list = new List<string>();
            foreach (var gCat in GlobalTags)
            {
                if (gCat.Category.Name.ContainsI(text.First))
                {
                    if (gCat.Tags.Count == 0)
                    {
                        if (gCat.Category.Name != "misc") list.Add(gCat.Category.Name + ":");
                    }
                    else
                    {
                        foreach (var gTag in gCat.Tags)
                        {
                            if (!text.Second.IsWhiteSpace() && !gTag.Name.ContainsI(text.Second)) continue;
                            if (gCat.Category.Name == "misc")
                            {
                                if (text.Second.IsWhiteSpace() && !gCat.Category.Name.ContainsI(text.First))
                                {
                                    list.Add(gTag.Name);
                                }
                            }
                            else
                            {
                                list.Add(gCat.Category.Name + ": " + gTag.Name);
                            }
                        }
                    }
                }
                // if, not else if - we want to display found tags both categorized and uncategorized
                if (gCat.Category.Name == "misc")
                {
                    foreach (var gTag in gCat.Tags)
                    {
                        if (gTag.Name.ContainsI(searchText)) list.Add(gTag.Name);
                    }
                }
            }

            list.Sort(StringComparer.OrdinalIgnoreCase);

            return list;
        }

        internal static void AddTagToFM(FanMission fm, string catAndTag)
        {
            AddTagsToFMAndGlobalList(catAndTag, fm.Tags);
            UpdateFMTagsString(fm);
            WriteFullFMDataIni();
        }

        internal static bool RemoveTagFromFM(FanMission fm, string catText, string tagText)
        {
            if (tagText.IsEmpty()) return false;

            // Parent node (category)
            if (catText.IsEmpty())
            {
                // TODO: These messageboxes are annoying, but they prevent accidental deletion.
                // Figure out something better.
                var cont = View.AskToContinue(LText.TagsTab.AskRemoveCategory, LText.TagsTab.TabText, true);
                if (!cont) return false;

                var cat = fm.Tags.FirstOrDefault(x => x.Category == tagText);
                if (cat != null)
                {
                    fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    // TODO: Profile the FirstOrDefaults and see if I should make them for loops
                    var globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    if (globalCat != null && !globalCat.Category.IsPreset)
                    {
                        if (globalCat.Category.UsedCount > 0) globalCat.Category.UsedCount--;
                        if (globalCat.Category.UsedCount == 0) GlobalTags.Remove(globalCat);
                    }
                }
            }
            // Child node (tag)
            else
            {
                var cont = View.AskToContinue(LText.TagsTab.AskRemoveTag, LText.TagsTab.TabText, true);
                if (!cont) return false;

                var cat = fm.Tags.FirstOrDefault(x => x.Category == catText);
                var tag = cat?.Tags.FirstOrDefault(x => x == tagText);
                if (tag != null)
                {
                    cat.Tags.Remove(tag);
                    if (cat.Tags.Count == 0) fm.Tags.Remove(cat);
                    UpdateFMTagsString(fm);

                    var globalCat = GlobalTags.FirstOrDefault(x => x.Category.Name == cat.Category);
                    var globalTag = globalCat?.Tags.FirstOrDefault(x => x.Name == tagText);
                    if (globalTag != null && !globalTag.IsPreset)
                    {
                        if (globalTag.UsedCount > 0) globalTag.UsedCount--;
                        if (globalTag.UsedCount == 0) globalCat.Tags.Remove(globalTag);
                        if (globalCat.Tags.Count == 0) GlobalTags.Remove(globalCat);
                    }
                }
            }

            WriteFullFMDataIni();

            return true;
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
            Game gameTab,
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
                    Config.GameTab = Game.Thief1;
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

        private static readonly ReaderWriterLockSlim FMDataIniRWLock = new ReaderWriterLockSlim();

        internal static void WriteFullFMDataIni()
        {
            try
            {
                FMDataIniRWLock.EnterWriteLock();
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing FM data ini", ex);
            }
            finally
            {
                try
                {
                    FMDataIniRWLock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Log("Exception exiting " + nameof(FMDataIniRWLock) + " in " + nameof(WriteFullFMDataIni), ex);
                }
            }
        }

        internal static void Shutdown()
        {
            try
            {
                WriteConfigIni(Config, Paths.ConfigIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing config ini", ex);
            }

            WriteFullFMDataIni();

            Application.Exit();
        }
    }
}

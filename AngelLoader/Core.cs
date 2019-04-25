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
using AngelLoader.CustomControls;
using AngelLoader.Forms;
using AngelLoader.Importing;
using FMScanner;
using Ookii.Dialogs.WinForms;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.Ini.Ini;

namespace AngelLoader
{
    internal interface IView
    {
        void Init();
        void SortFMTable(Column column, SortOrder sortDirection);
        void Show();
        void ShowAlert(string message, string title);
        Task<bool> OpenSettings(bool startup = false);
        object InvokeSync(Delegate method);
        object InvokeSync(Delegate method, params object[] args);
        object InvokeAsync(Delegate method);
        object InvokeAsync(Delegate method, params object[] args);
        void Block(bool block);
        Task RefreshSelectedFM(bool refreshReadme, bool refreshGridRowOnly = false);
        bool AskToContinue(string message, string title);

        (bool Cancel, bool DontAskAgain)
        AskToContinueYesNoCustomStrings(string message, string title, TaskDialogIcon icon,
            bool showDontAskAgain, string yes, string no);

        (bool Cancel, bool Continue, bool DontAskAgain)
        AskToContinueWithCancelCustomStrings(string message, string title, TaskDialogIcon? icon,
            bool showDontAskAgain, string yes, string no, string cancel);
    }

    internal static class Core
    {
        internal static IView View { get; set; }
        internal static ProgressPanel ProgressBox;

        internal static List<FanMission> FMsViewList = new List<FanMission>();
        private static readonly List<FanMission> FMDataIniList = new List<FanMission>();

        private static CancellationTokenSource ScanCts;

        internal static async Task Init()
        {
            View = new MainForm();

            try
            {
                Directory.CreateDirectory(Paths.Data);
                Directory.CreateDirectory(Paths.Languages);
            }
            catch (Exception ex)
            {
                const string message = "Failed to create required application directories on startup.";
                Log(message, ex);
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }

            bool openSettings;
            if (File.Exists(Paths.ConfigIni))
            {
                try
                {
                    ReadConfigIni(Paths.ConfigIni, Config);
                    var checkPaths = CheckPaths();
                    openSettings = checkPaths == Error.BackupPathNotSpecified;
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

            // Have to read langs here because which language to use will be stored in the config file.
            // Gather all lang files in preparation to read their LanguageName= value so we can get the lang's
            // name in its own language
            var langFiles = Directory.GetFiles(Paths.Languages, "*.ini", SearchOption.TopDirectoryOnly);
            bool selFound = false;
            for (int i = 0; i < langFiles.Length; i++)
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
            }

            if (openSettings)
            {
                if (await View.OpenSettings(startup: true))
                {
                    var checkPaths = CheckPaths();

                    Debug.Assert(checkPaths == Error.None, "checkPaths returned an error the second time");

                    WriteConfigIni(Config, Paths.ConfigIni);
                }
                else
                {
                    // Since nothing of consequence has yet happened, it's okay to do the brutal quit
                    Environment.Exit(0);
                }
            }

            FindFMs(startup: true);
            View.Init();
            View.Show();
        }

        internal static void SortFMsViewList(Column column, SortOrder sortDirection)
        {
            var articles = Config.EnableArticles ? Config.Articles : new List<string>();

            void SortByTitle(bool reverse = false)
            {
                var ascending = reverse ? SortOrder.Descending : SortOrder.Ascending;

                FMsViewList = sortDirection == ascending
                    ? FMsViewList.OrderBy(x => x.Title, new FMTitleComparer(articles)).ToList()
                    : FMsViewList.OrderByDescending(x => x.Title, new FMTitleComparer(articles)).ToList();
            }

            // For any column which could have empty entries, sort by title first in order to maintain a
            // consistent order

            switch (column)
            {
                case Column.Game:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Game).ToList()
                        : FMsViewList.OrderByDescending(x => x.Game).ToList();
                    break;

                case Column.Installed:
                    SortByTitle();
                    // Reverse this because "Installed" should go on top and blanks should go on bottom
                    FMsViewList = sortDirection == SortOrder.Descending
                        ? FMsViewList.OrderBy(x => x.Installed).ToList()
                        : FMsViewList.OrderByDescending(x => x.Installed).ToList();
                    break;

                case Column.Title:
                    SortByTitle();
                    break;

                case Column.Archive:
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Archive).ToList()
                        : FMsViewList.OrderByDescending(x => x.Archive).ToList();
                    break;

                case Column.Author:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Author).ToList()
                        : FMsViewList.OrderByDescending(x => x.Author).ToList();
                    break;

                case Column.Size:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.SizeBytes).ToList()
                        : FMsViewList.OrderByDescending(x => x.SizeBytes).ToList();
                    break;

                case Column.Rating:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.Rating).ToList()
                        : FMsViewList.OrderByDescending(x => x.Rating).ToList();
                    break;

                case Column.Finished:
                    SortByTitle();
                    // FinishedOnUnknown is a separate value, so...
                    if (sortDirection == SortOrder.Ascending)
                    {
                        FMsViewList = FMsViewList.OrderBy(x => x.FinishedOn).ToList();
                        FMsViewList = FMsViewList.OrderBy(x => x.FinishedOnUnknown).ToList();
                    }
                    else
                    {
                        FMsViewList = FMsViewList.OrderByDescending(x => x.FinishedOn).ToList();
                        FMsViewList = FMsViewList.OrderByDescending(x => x.FinishedOnUnknown).ToList();
                    }
                    break;

                case Column.ReleaseDate:
                    SortByTitle();
                    // Sort this one down to the day only, because the exact time may very well not be known, and
                    // even if it is, it's not visible or editable anywhere and it'd be weird to have missions
                    // sorted out of name order because of an invisible time difference.
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.ReleaseDate?.Date ?? x.ReleaseDate).ToList()
                        : FMsViewList.OrderByDescending(x => x.ReleaseDate?.Date ?? x.ReleaseDate).ToList();
                    break;

                case Column.LastPlayed:
                    SortByTitle();
                    // Sort this one by exact DateTime because the time is (indirectly) changeable down to the
                    // second (you change it by playing it), and the user will expect precise sorting.
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.LastPlayed).ToList()
                        : FMsViewList.OrderByDescending(x => x.LastPlayed).ToList();
                    break;

                case Column.DisabledMods:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.DisabledMods).ToList()
                        : FMsViewList.OrderByDescending(x => x.DisabledMods).ToList();
                    break;

                case Column.Comment:
                    SortByTitle();
                    FMsViewList = sortDirection == SortOrder.Ascending
                        ? FMsViewList.OrderBy(x => x.CommentSingleLine).ToList()
                        : FMsViewList.OrderByDescending(x => x.CommentSingleLine).ToList();
                    break;
            }
        }

        private static Error CheckPaths()
        {
            var t1Exists = !Config.T1Exe.IsEmpty() && File.Exists(Config.T1Exe);
            var t2Exists = !Config.T2Exe.IsEmpty() && File.Exists(Config.T2Exe);
            var t3Exists = !Config.T3Exe.IsEmpty() && File.Exists(Config.T3Exe);

            if (t1Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T1Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                Config.T1DromEdDetected = !GetDromEdExe(Game.Thief1).IsEmpty();
                if (error == Error.CamModIniNotFound) return Error.T1CamModIniNotFound;
                Config.T1FMInstallPath = gameFMsPath;
            }
            if (t2Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T2Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                Config.T2DromEdDetected = !GetDromEdExe(Game.Thief2).IsEmpty();
                if (error == Error.CamModIniNotFound) return Error.T2CamModIniNotFound;
                Config.T2FMInstallPath = gameFMsPath;
            }
            if (t3Exists)
            {
                var (error, useCentralSaves, path) = GetInstFMsPathFromT3();
                if (error != Error.None) return error;
                Config.T3FMInstallPath = path;
                Config.T3UseCentralSaves = useCentralSaves;
            }

            if (!t1Exists && !t2Exists && !t3Exists) return Error.NoGamesSpecified;

            if (!Directory.Exists(Config.FMsBackupPath))
            {
                return Error.BackupPathNotSpecified;
            }

            return Error.None;
        }

        internal static string GetDromEdExe(Game game)
        {
            var gameExe = GetGameExeFromGameType(game);
            if (gameExe.IsEmpty()) return "";

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty()) return "";

            var dromEdExe = Path.Combine(gamePath, Paths.DromEdExe);
            return !gamePath.IsEmpty() && File.Exists(dromEdExe) ? dromEdExe : "";
        }

        internal static string GetInstFMsPathFromCamModIni(string gamePath, out Error error)
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

            // Note: Using StartsWithI here because it's fast; obviously there's no need for case-awareness
            if (!path.IsEmpty() &&
                (path.StartsWithI(".\\") || path.StartsWithI("..\\") ||
                path.StartsWithI("./") || path.StartsWithI("../")))
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

        internal static (Error Error, bool UseCentralSaves, string Path)
        GetInstFMsPathFromT3()
        {
            var soIni = Paths.GetSneakyOptionsIni();
            var errorMessage = LText.AlertMessages.Misc_SneakyOptionsIniNotFound;
            if (soIni.IsEmpty())
            {
                MessageBox.Show(errorMessage, LText.AlertMessages.Alert, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (Error.SneakyOptionsNoRegKey, false, null);
            }

            if (!File.Exists(soIni))
            {
                MessageBox.Show(errorMessage, LText.AlertMessages.Alert, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (Error.SneakyOptionsNotFound, false, null);
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

        internal static void FindFMs(bool startup = false)
        {
            // Make sure we don't lose anything when we re-find!
            if (!startup) WriteFullFMDataIni();

            // Init or reinit - must be deep-copied or changes propagate back because reference types
            DeepCopyGlobalTags(PresetTags, GlobalTags);

            // Copy FMs to backup lists before clearing, in case we can't read the ini file. We don't want to end
            // up with a blank or incomplete list and then glibly save it out later.
            var backupList = new List<FanMission>();
            foreach (var fm in FMDataIniList) backupList.Add(fm);

            var viewBackupList = new List<FanMission>();
            foreach (var fm in FMsViewList) viewBackupList.Add(fm);

            FMDataIniList.Clear();
            FMsViewList.Clear();

            var fmDataIniExists = File.Exists(Paths.FMDataIni);

            if (fmDataIniExists)
            {
                try
                {
                    ReadFMDataIni(Paths.FMDataIni, FMDataIniList);
                }
                catch (Exception ex)
                {
                    Log("Exception reading FM data ini", ex);
                    if (startup)
                    {
                        MessageBox.Show("Exception reading FM data ini. Exiting. Please check " + Paths.LogFile,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(1);
                    }
                    else
                    {
                        FMDataIniList.Clear();
                        foreach (var fm in backupList) FMDataIniList.Add(fm);
                        FMsViewList.Clear();
                        foreach (var fm in viewBackupList) FMsViewList.Add(fm);
                        return;
                    }
                }
            }

            // Could check inside the folder for a .mis file to confirm it's really an FM folder, but that's
            // horrendously expensive. Talking like eight seconds vs. < 4ms for the 1098 set. Weird.
            var t1InstalledFMDirs = new List<string>();
            var t2InstalledFMDirs = new List<string>();
            var t3InstalledFMDirs = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                var instFMDirs = i == 0 ? t1InstalledFMDirs : i == 1 ? t2InstalledFMDirs : t3InstalledFMDirs;
                var instPath = i == 0 ? Config.T1FMInstallPath : i == 1 ? Config.T2FMInstallPath : Config.T3FMInstallPath;

                if (Directory.Exists(instPath))
                {
                    try
                    {
                        foreach (var d in Directory.GetDirectories(instPath, "*", SearchOption.TopDirectoryOnly))
                        {
                            var dirName = d.GetTopmostDirName();
                            if (!dirName.EqualsI(".fmsel.cache")) instFMDirs.Add(dirName);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("Exception getting directories in " + instPath, ex);
                    }
                }
            }

            var fmArchives = new List<string>();

            foreach (var path in GetFMArchivePaths())
            {
                try
                {
                    var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
                    foreach (var f in files)
                    {
                        if (!fmArchives.ContainsI(f.GetFileNameFast()) &&
                            f.ExtIsArchive() && !f.ContainsI(Paths.FMSelBak))
                        {
                            fmArchives.Add(f.GetFileNameFast());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception getting files in " + path, ex);
                }
            }

            #region PERF WORK

            var t1List = new List<FanMission>();
            var t2List = new List<FanMission>();
            var t3List = new List<FanMission>();

            for (int i = 0; i < 3; i++)
            {
                var instFMDirs = i == 0 ? t1InstalledFMDirs : i == 1 ? t2InstalledFMDirs : t3InstalledFMDirs;
                var list = i == 0 ? t1List : i == 1 ? t2List : t3List;
                var game = i == 0 ? Game.Thief1 : i == 1 ? Game.Thief2 : Game.Thief3;

                foreach (var item in instFMDirs)
                {
                    list.Add(new FanMission { InstalledDir = item, Game = game, Installed = true });
                }
            }

            #region Archive union

            var initCountA = FMDataIniList.Count;
            // For some reason if this ISN'T a method, it screws all up and doesn't increment i(?!). Meh!
            void ArchiveUnion()
            {
                var checkedList = new List<FanMission>();

                for (int ai = 0; ai < fmArchives.Count; ai++)
                {
                    var archive = fmArchives[ai];

                    // perf perf blah
                    string aRemoveExt = null;
                    string aFMSel = null;
                    string aFMSelTrunc = null;
                    string aNDL = null;

                    bool existingFound = false;
                    for (int i = 0; i < initCountA; i++)
                    {
                        var fm = FMDataIniList[i];

                        if (!fm.Checked &&
                            fm.Archive.IsEmpty() &&
                            (fm.InstalledDir.EqualsI(aRemoveExt ?? (aRemoveExt = archive.RemoveExtension())) ||
                             fm.InstalledDir.EqualsI(aFMSel ?? (aFMSel = archive.ToInstDirNameFMSel(false))) ||
                             fm.InstalledDir.EqualsI(aFMSelTrunc ?? (aFMSelTrunc = archive.ToInstDirNameFMSel(true))) ||
                             fm.InstalledDir.EqualsI(aNDL ?? (aNDL = archive.ToInstDirNameNDL()))))
                        {
                            fm.Archive = archive;

                            fm.Checked = true;
                            checkedList.Add(fm);
                            existingFound = true;
                            break;
                        }
                        else if (!fm.Checked &&
                                 !fm.Archive.IsEmpty() && fm.Archive.EqualsI(archive))
                        {
                            fm.Checked = true;
                            checkedList.Add(fm);
                            existingFound = true;
                            break;
                        }
                    }
                    if (!existingFound)
                    {
                        FMDataIniList.Add(new FanMission { Archive = archive });
                    }
                }

                // Reset temp bool
                for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;
            }

            ArchiveUnion();

            #endregion

            #region Game union

            // -Attempt at a perf optimization: we don't need to search anything we've added onto the end.
            // -This is outside GameUnion() so it doesn't get set to the new length every time it's called.
            var initCount = FMDataIniList.Count;

            // I'm pretty sure there's some clever algorithmic math-genius way to make this faster, but I don't
            // know it. I did my best to tune it as is.
            void GameUnion(List<FanMission> installedList)
            {
                var checkedList = new List<FanMission>();

                for (int gFMi = 0; gFMi < installedList.Count; gFMi++)
                {
                    var gFM = installedList[gFMi];

                    // bool check seems to be faster than a null check
                    bool isEmpty = gFM.InstalledDir.IsEmpty();

                    bool existingFound = false;
                    for (int i = 0; i < initCount; i++)
                    {
                        var fm = FMDataIniList[i];

                        if (!isEmpty &&
                            // Early-out bool - much faster than checking EqualsI()
                            !fm.Checked &&
                            !fm.InstalledDir.IsEmpty() &&
                            fm.InstalledDir.EqualsI(gFM.InstalledDir))
                        {
                            fm.Game = gFM.Game;
                            fm.Installed = true;
                            fm.Checked = true;
                            // So we only loop through checked FMs when we reset them
                            checkedList.Add(fm);
                            existingFound = true;
                            break;
                        }
                    }
                    if (!existingFound)
                    {
                        FMDataIniList.Add(new FanMission
                        {
                            InstalledDir = gFM.InstalledDir,
                            Game = gFM.Game,
                            Installed = true
                        });
                    }
                }

                // Reset temp bool
                for (int i = 0; i < checkedList.Count; i++) checkedList[i].Checked = false;
            }

            if (t1List.Count > 0) GameUnion(t1List);
            if (t2List.Count > 0) GameUnion(t2List);
            if (t3List.Count > 0) GameUnion(t3List);

            #endregion

            // Attempt to fill in empty archive names, and I guess set installed dirs sometimes?!
            for (var i = 0; i < FMDataIniList.Count; i++)
            {
                var fm = FMDataIniList[i];

                if (fm.Archive.IsEmpty())
                {
                    if (fm.InstalledDir.IsEmpty())
                    {
                        FMDataIniList.RemoveAt(i);
                        i--;
                        continue;
                    }

                    var archiveName = GetArchiveNameFromInstalledDir(fm, fmArchives);
                    if (archiveName.IsEmpty()) continue;

                    // Exponential (slow) stuff, but we only do it once to correct the list and then never again
                    // NOTE: I guess this removes duplicates, which is why it has to do the search?
                    var existingFM = FMDataIniList.FirstOrDefault(x => x.Archive.EqualsI(archiveName));
                    if (existingFM != null)
                    {
                        existingFM.InstalledDir = fm.InstalledDir;
                        existingFM.Installed = true;
                        existingFM.Game = fm.Game;
                        FMDataIniList.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        fm.Archive = archiveName;
                    }
                }
            }

            // Set installed dir names, handling collisions
            foreach (var fm in FMDataIniList)
            {
                if (fm.InstalledDir.IsEmpty())
                {
                    var truncate = fm.Game != Game.Thief3;
                    var instDir = fm.Archive.ToInstDirNameFMSel(truncate);
                    int i = 0;

                    // Again, an exponential search, but again, we only do it once to correct the list and then
                    // never again
                    while (FMDataIniList.Any(x => x.InstalledDir.EqualsI(instDir)))
                    {
                        // Yeah, this'll never happen, but hey
                        if (i > 999) break;

                        // Conform to FMSel's numbering format
                        var numStr = (i + 2).ToString();
                        instDir = instDir.Substring(0, (instDir.Length - 2) - numStr.Length) + "(" + numStr + ")";

                        Debug.Assert(truncate && instDir.Length == 30,
                            nameof(instDir) + "should have been truncated but its length is not 30");

                        i++;
                    }

                    // If it overflowed, oh well. You get what you deserve in that case.
                    fm.InstalledDir = instDir;
                }
            }

            #endregion

            ViewListGamesNull.Clear();

            for (var i = 0; i < FMDataIniList.Count; i++)
            {
                var item = FMDataIniList[i];

                #region Checks

                // Attempt to avoid re-searching lists
                bool? notInT1Dirs = null;
                bool? notInT2Dirs = null;
                bool? notInT3Dirs = null;

                bool NotInT1Dirs()
                {
                    if (notInT1Dirs == null) notInT1Dirs = !t1InstalledFMDirs.ContainsI(item.InstalledDir);
                    return (bool)notInT1Dirs;
                }
                bool NotInT2Dirs()
                {
                    if (notInT2Dirs == null) notInT2Dirs = !t2InstalledFMDirs.ContainsI(item.InstalledDir);
                    return (bool)notInT2Dirs;
                }
                bool NotInT3Dirs()
                {
                    if (notInT3Dirs == null) notInT3Dirs = !t3InstalledFMDirs.ContainsI(item.InstalledDir);
                    return (bool)notInT3Dirs;
                }

                if (item.Installed &&
                    ((item.Game == Game.Thief1 && NotInT1Dirs()) ||
                     (item.Game == Game.Thief2 && NotInT2Dirs()) ||
                     (item.Game == Game.Thief3 && NotInT3Dirs())))
                {
                    item.Installed = false;
                }

                // NOTE: Old data
                // FMDataIniList: Thief1(personal)+Thief2(personal)+All(1098 set)
                // Archive dirs: Thief1(personal)+Thief2(personal)
                // Total time taken running this for all FMs in FMDataIniList: 3~7ms
                // Good enough?
                if ((!item.Installed ||
                     (item.Game == Game.Thief1 && NotInT1Dirs()) ||
                     (item.Game == Game.Thief2 && NotInT2Dirs()) ||
                     (item.Game == Game.Thief3 && NotInT3Dirs())) &&
                    // Shrink the list as we get matches so we can reduce our search time as we go
                    !fmArchives.ContainsIRemoveFirstHit(item.Archive))
                {
                    continue;
                }

                #endregion

                // Perf so we don't have to iterate the list again later
                if (item.Game == null) ViewListGamesNull.Add(i);

                FMsViewList.Add(item);

                item.Title =
                    !item.Title.IsEmpty() ? item.Title :
                    !item.Archive.IsEmpty() ? item.Archive.RemoveExtension() :
                    item.InstalledDir;
                item.SizeString = ((long?)item.SizeBytes).ConvertSize();
                item.CommentSingleLine = item.Comment.FromEscapes().ToSingleLineComment(100);
                AddTagsToFMAndGlobalList(item.TagsString, item.Tags);
            }
        }

        // Super quick-n-cheap hack for perf
        internal static List<int> ViewListGamesNull = new List<int>();

        internal static async Task<bool> ScanFM(FanMission fm, ScanOptions scanOptions,
            bool overwriteUnscannedFields = true, bool markAsScanned = false)
        {
            return await ScanFMs(new List<FanMission> { fm }, scanOptions, overwriteUnscannedFields, markAsScanned);
        }

        private static string GetArchiveNameFromInstalledDir(FanMission fm, List<string> archives)
        {
            // The game type is supposed to be inferred from the installed location, so it should always be known
            Debug.Assert(fm.Game != null, "fm.Game == null: Game type is blank for an installed FM?!");

            var gamePath =
                fm.Game == Game.Thief1 ? Config.T1FMInstallPath :
                fm.Game == Game.Thief2 ? Config.T2FMInstallPath :
                // TODO: If SU's FMSel mangles install names in a different way, I need to account for it here
                fm.Game == Game.Thief3 ? Config.T3FMInstallPath :
                null;

            if (gamePath.IsEmpty()) return null;

            var fmDir = Path.Combine(gamePath, fm.InstalledDir);
            var fmselInf = Path.Combine(fmDir, Paths.FMSelInf);

            string FixUp(bool createFmselInf)
            {
                // Make a best-effort attempt to find what this FM's archive name should be
                // TODO: This is really slow. 8ms to run it once (~1500 set) with no hits.
                bool truncate = fm.Game != Game.Thief3;
                var tryArchive =
                    archives.FirstOrDefault(x => x.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir)) ??
                    archives.FirstOrDefault(x => x.ToInstDirNameNDL().EqualsI(fm.InstalledDir)) ??
                    archives.FirstOrDefault(x => x.EqualsI(fm.InstalledDir)) ??
                    FMDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameFMSel(truncate).EqualsI(fm.InstalledDir))?.Archive ??
                    FMDataIniList.FirstOrDefault(x => x.Archive.ToInstDirNameNDL().EqualsI(fm.InstalledDir))?.Archive ??
                    FMDataIniList.FirstOrDefault(x => x.InstalledDir.EqualsI(fm.InstalledDir))?.Archive;

                // TODO: Look in FMSel/NDL ini files here too?

                if (tryArchive.IsEmpty()) return null;

                if (!createFmselInf) return tryArchive;

                try
                {
                    using (var sw = new StreamWriter(fmselInf, append: false))
                    {
                        sw.WriteLine("Name=" + fm.InstalledDir);
                        sw.WriteLine("Archive=" + tryArchive);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception in creating or overwriting" + fmselInf, ex);
                }

                return tryArchive;
            }

            if (!File.Exists(fmselInf)) return FixUp(true);

            string[] lines;
            try
            {
                lines = File.ReadAllLines(fmselInf);
            }
            catch (Exception ex)
            {
                Log("Exception reading " + fmselInf, ex);
                return null;
            }

            if (lines.Length < 2 || !lines[0].StartsWithI("Name=") || !lines[1].StartsWithI("Archive="))
            {
                return FixUp(true);
            }

            var installedName = lines[0].Substring(lines[0].IndexOf('=') + 1).Trim();
            if (!installedName.EqualsI(fm.InstalledDir))
            {
                return FixUp(true);
            }

            var archiveName = lines[1].Substring(lines[1].IndexOf('=') + 1).Trim();
            if (archiveName.IsEmpty())
            {
                return FixUp(true);
            }

            return archiveName;
        }

        internal static async Task<bool> ScanFMs(List<FanMission> fmsToScan, ScanOptions scanOptions,
            bool overwriteUnscannedFields = true, bool markAsScanned = false)
        {
            if (fmsToScan == null || fmsToScan.Count == 0 || (fmsToScan.Count == 1 && fmsToScan[0] == null))
            {
                return false;
            }

            void ReportProgress(ProgressReport pr)
            {
                var fmIsZip = pr.FMName.ExtIsArchive();
                var name = fmIsZip ? pr.FMName.GetFileNameFast() : pr.FMName.GetDirNameFast();
                ProgressBox.ReportScanProgress(pr.FMNumber, pr.FMsTotal, pr.Percent, name);
            }

            var scanningOne = fmsToScan.Count == 1;

            if (scanningOne)
            {
                Log(nameof(ScanFMs) + ": Scanning one", methodName: false);
                // Just use a cheap check and throw up the progress box for .7z files, otherwise not. Not as nice
                // as the timer method, but that can cause race conditions I don't know how to fix, so whatever.
                if (fmsToScan[0].Archive.ExtIs7z())
                {
                    ProgressBox.ShowScanningAllFMs();
                }
                else
                {
                    // Block user input to the form to mimic the UI thread being blocked, because we're async here
                    //View.BeginInvoke(new Action(View.Block));
                    View.Block(true);
                    ProgressBox.ProgressTask = ProgressPanel.ProgressTasks.ScanAllFMs;
                    ProgressBox.ShowProgressWindow(ProgressBox.ProgressTask, suppressShow: true);
                }
            }
            else
            {
                ProgressBox.ShowScanningAllFMs();
            }

            // TODO: This is pretty hairy, try and organize this better
            try
            {
                ScanCts = new CancellationTokenSource();

                var fms = new List<string>();

                Log(nameof(ScanFMs) + ": about to call " + nameof(GetFMArchivePaths) + " with subfolders=" +
                    Config.FMArchivePathsIncludeSubfolders);

                // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                // cause then it will hit the actual disk rather than just going through a list of paths in
                // memory
                var archivePaths = await Task.Run(GetFMArchivePaths);

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

                    if (ScanCts.IsCancellationRequested)
                    {
                        ScanCts?.Dispose();
                        return false;
                    }
                }

                List<ScannedFMData> fmDataList;
                try
                {
                    var progress = new Progress<ProgressReport>(ReportProgress);

                    Paths.PrepareTempPath(Paths.FMScannerTemp);

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
                finally
                {
                    ScanCts?.Dispose();
                }

                for (var i = 0; i < fmsToScanFiltered.Count; i++)
                {
                    var scannedFM = fmDataList[i];

                    if (scannedFM == null)
                    {
                        // We need to return fail for scanning one, else we get into an infinite loop because
                        // of a refresh that gets called in that case
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

                    var gameSup = scannedFM.Game != Games.Unsupported;

                    if (overwriteUnscannedFields || scanOptions.ScanTitle)
                    {
                        sel.Title =
                            !scannedFM.Title.IsEmpty() ? scannedFM.Title
                            : scannedFM.ArchiveName.ExtIsArchive() ? scannedFM.ArchiveName.RemoveExtension()
                            : scannedFM.ArchiveName;

                        if (gameSup)
                        {
                            sel.AltTitles.Clear();
                            sel.AltTitles.Add(sel.Title);
                            sel.AltTitles.AddRange(scannedFM.AlternateTitles);
                        }
                        else
                        {
                            sel.AltTitles.Clear();
                        }
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanSize)
                    {
                        sel.SizeString = gameSup ? scannedFM.Size.ConvertSize() : "";
                        sel.SizeBytes = (ulong)(gameSup ? scannedFM.Size ?? 0 : 0);
                    }
                    if (overwriteUnscannedFields || scanOptions.ScanReleaseDate)
                    {
                        sel.ReleaseDate = gameSup ? scannedFM.LastUpdateDate : null;
                    }
                    if (overwriteUnscannedFields || scanOptions.ScanCustomResources)
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

                    if (overwriteUnscannedFields || scanOptions.ScanAuthor)
                    {
                        sel.Author = gameSup ? scannedFM.Author : "";
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanGameType)
                    {
                        sel.Game =
                            scannedFM.Game == Games.Unsupported ? Game.Unsupported :
                            scannedFM.Game == Games.TDP ? Game.Thief1 :
                            scannedFM.Game == Games.TMA ? Game.Thief2 :
                            scannedFM.Game == Games.TDS ? Game.Thief3 :
                            (Game?)null;
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanLanguages)
                    {
                        sel.Languages = gameSup ? scannedFM.Languages : new string[0];
                        sel.LanguagesString = gameSup
                            ? scannedFM.Languages != null ? string.Join(", ", scannedFM.Languages) : ""
                            : "";
                    }

                    if (overwriteUnscannedFields || scanOptions.ScanTags)
                    {
                        sel.TagsString = gameSup ? scannedFM.TagsString : "";

                        // Don't clear the tags, because the user could have added a bunch and we should only
                        // add to those, not overwrite them
                        if (gameSup) AddTagsToFMAndGlobalList(sel.TagsString, sel.Tags);
                    }

                    sel.MarkedScanned = markAsScanned;
                }

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
                //View.BeginInvoke(new Action(View.Unblock));
                View.Block(false);
                View.InvokeSync(new Action(() => ProgressBox.HideThis()));
            }

            return true;
        }

        internal static void CancelScan()
        {
            try
            {
                ScanCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal static async Task ScanNewFMsForGameType()
        {
            var fmsToScan = new List<FanMission>();
            foreach (var fm in FMsViewList)
            {
                if (fm.Game == null) fmsToScan.Add(fm);
            }
            if (fmsToScan.Count > 0)
            {
                var scanOptions = ScanOptions.FalseDefault(scanGameType: true);

                try
                {
                    await ScanFMs(fmsToScan, scanOptions, overwriteUnscannedFields: false);
                }
                catch (Exception ex)
                {
                    Log("Exception in ScanFMs", ex);
                }
            }
        }

        #region Importing

        internal static async Task<bool>
        ImportFromDarkLoader(string iniFile, bool importFMData, bool importSaves)
        {
            ProgressBox.ShowImportDarkLoader();
            try
            {
                var (error, fmsToScan) = await ImportDarkLoader.Import(iniFile, importFMData, importSaves, FMDataIniList);
                if (error != ImportError.None)
                {
                    Log("Import.Error: " + error, stackTrace: true);

                    if (error == ImportError.NoArchiveDirsFound)
                    {
                        View.ShowAlert(LText.Importing.DarkLoader_NoArchiveDirsFound, LText.AlertMessages.Alert);
                        return false;
                    }

                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true));
            }
            catch (Exception ex)
            {
                Log("Exception in DarkLoader import", ex);
                return false;
            }
            finally
            {
                ProgressBox.HideThis();
            }

            return true;
        }

        internal static async Task<bool> ImportFromNDL(string iniFile)
        {
            ProgressBox.ShowImportNDL();
            try
            {
                var (error, fmsToScan) = await ImportNDL.Import(iniFile, FMDataIniList);
                if (error != ImportError.None)
                {
                    Log("Import error: " + error, stackTrace: true);
                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true));
            }
            catch (Exception ex)
            {
                Log("Exception in NewDarkLoader import", ex);
                return false;
            }
            finally
            {
                ProgressBox.HideThis();
            }

            return true;
        }

        internal static async Task<bool> ImportFromFMSel(string iniFile)
        {
            ProgressBox.ShowImportFMSel();
            try
            {
                var (error, fmsToScan) = await ImportFMSel.Import(iniFile, FMDataIniList);
                if (error != ImportError.None)
                {
                    Log("Import error: " + error, stackTrace: true);
                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true, scanSize: true));
            }
            catch (Exception ex)
            {
                Log("Exception in FMSel import", ex);
                return false;
            }
            finally
            {
                ProgressBox.HideThis();
            }

            return true;
        }

        private static async Task ScanAndFind(List<FanMission> fms, ScanOptions scanOptions, bool overwriteUnscannedFields = false)
        {
            if (fms.Count == 0) return;

            await ScanFMs(fms, scanOptions, overwriteUnscannedFields, markAsScanned: true);
            FindFMs();
        }

        #endregion

        #region Install, Uninstall, Play

        internal static async Task InstallOrUninstall(FanMission fm)
        {
            await (fm.Installed ? FMInstallAndPlay.UninstallFM(fm) : FMInstallAndPlay.InstallFM(fm));
        }

        internal static async Task ConvertOGGsToWAVs(FanMission fm)
        {
            if (!GameIsDark(fm)) return;

            Debug.Assert(fm.Game != null, "fm.Game != null");

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
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
                ProgressBox.ShowConvertingFiles();
                await ac.ConvertOGGsToWAVsInternal();
            }
            finally
            {
                ProgressBox.HideThis();
            }
        }

        internal static async Task ConvertWAVsTo16Bit(FanMission fm)
        {
            if (!GameIsDark(fm)) return;

            Debug.Assert(fm.Game != null, "fm.Game != null");

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
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
                ProgressBox.ShowConvertingFiles();
                await ac.ConvertWAVsTo16BitInternal();
            }
            finally
            {
                ProgressBox.HideThis();
            }
        }

        internal static void ReportFMExtractProgress(int percent)
        {
            View.InvokeSync(new Action(() => ProgressBox.ReportFMExtractProgress(percent)));
        }

        internal static void HideProgressBox()
        {
            View.InvokeSync(new Action(() => ProgressBox.HideThis()));
        }

        internal static void ProgressBoxSetCancelingFMInstall()
        {
            View.InvokeSync(new Action(ProgressBox.SetCancelingFMInstall));
        }

        internal static bool PlayOriginalGame(Game game)
        {
            var gameExe = GetGameExeFromGameType(game);

            #region Exe: Fail if blank or not found

            var gameName = GetGameNameFromGameType(game);

            if (gameExe.IsEmpty() || !File.Exists(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_ExecutableNotFound,
                    LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            #region Exe: Fail if already running

            if (GameIsRunning(gameExe, checkAllGames: true))
            {
                View.ShowAlert(LText.AlertMessages.Play_AnyGameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty())
            {
                View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_GamePathNotFound,
                    LText.AlertMessages.Alert);
                return false;
            }

            // When the stub finds nothing in the stub comm folder, it will just start the game with no FM
            Paths.PrepareTempPath(Paths.StubCommTemp);

            try
            {
                using (var proc = new Process())
                {
                    proc.StartInfo.FileName = gameExe;
                    proc.StartInfo.WorkingDirectory = gamePath;
                    proc.Start();
                }
            }
            catch (Exception ex)
            {
                Log("Exception starting " + gameExe, ex);
            }

            return true;
        }

        private static bool SetDarkFMSelectorToAngelLoader(Game game)
        {
            const string fmSelectorKey = "fm_selector";
            var gameExe = GetGameExeFromGameType(game);
            if (gameExe.IsEmpty())
            {
                Log("gameExe is empty for " + game, stackTrace: true);
                return false;
            }

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty())
            {
                Log("gamePath is empty for " + game, stackTrace: true);
                return false;
            }

            var camModIni = Path.Combine(gamePath, "cam_mod.ini");
            if (!File.Exists(camModIni))
            {
                Log("cam_mod.ini not found for " + gameExe, stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(camModIni).ToList();
            }
            catch (Exception ex)
            {
                Log("Exception reading cam_mod.ini for " + gameExe, ex);
                return false;
            }

            // Confirmed ND T1/T2 can read this with both forward and backward slashes
            var stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            /*
             Conforms to the way NewDark reads it:
             - Zero or more whitespace characters allowed at the start of the line (before the key)
             - The key-value separator is one or more whitespace characters
             - Keys are case-insensitive
             - If duplicate keys exist, later ones replace earlier ones
             - Comment lines start with ;
             - No section headers
            */
            int lastSelKeyIndex = -1;
            bool loaderIsAlreadyUs = false;
            for (int i = 0; i < lines.Count; i++)
            {
                var lt = lines[i].TrimStart();

                do
                {
                    lt = lt.TrimStart(';').Trim();
                } while (lt.Length > 0 && lt[0] == ';');

                if (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length && lt
                        .Substring(fmSelectorKey.Length + 1).TrimStart().ToBackSlashes()
                        .EqualsI(stubPath.ToBackSlashes()))
                {
                    if (loaderIsAlreadyUs)
                    {
                        lines.RemoveAt(i);
                        i--;
                        lastSelKeyIndex = (lastSelKeyIndex - 1).Clamp(-1, int.MaxValue);
                    }
                    else
                    {
                        lines[i] = fmSelectorKey + " " + stubPath;
                        loaderIsAlreadyUs = true;
                    }
                    continue;
                }

                if (lt.EqualsI(fmSelectorKey) ||
                    (lt.StartsWithI(fmSelectorKey) && lt.Length > fmSelectorKey.Length &&
                    (lt[fmSelectorKey.Length] == ' ' || lt[fmSelectorKey.Length] == '\t')))
                {
                    if (!lines[i].TrimStart().StartsWith(";")) lines[i] = ";" + lines[i];
                    lastSelKeyIndex = i;
                }
            }

            if (!loaderIsAlreadyUs)
            {
                if (lastSelKeyIndex == -1 || lastSelKeyIndex == lines.Count - 1)
                {
                    lines.Add(fmSelectorKey + " " + stubPath);
                }
                else
                {
                    lines.Insert(lastSelKeyIndex + 1, fmSelectorKey + " " + stubPath);
                }
            }

            try
            {
                File.WriteAllLines(camModIni, lines);
            }
            catch (Exception ex)
            {
                Log("Exception writing cam_mod.ini for " + gameExe, ex);
                return false;
            }

            return true;
        }

        // If only you could do this with a command-line switch. You can say -fm to always start with the loader,
        // and you can say -fm=name to always start with the named FM, but you can't specify WHICH loader to use
        // on the command line. Only way to do it is through a file. Meh.
        private static bool SetT3FMSelectorToAngelLoader()
        {
            const string externSelectorKey = "ExternSelector=";
            bool existingKeyOverwritten = false;
            int insertLineIndex = -1;

            var ini = Paths.GetSneakyOptionsIni();
            if (ini.IsEmpty())
            {
                Log("Couldn't set us as the loader for Thief: Deadly Shadows because SneakyOptions.ini could not be found", stackTrace: true);
                return false;
            }

            List<string> lines;
            try
            {
                lines = File.ReadAllLines(ini, Encoding.Default).ToList();
            }
            catch (Exception ex)
            {
                Log("Problem reading SneakyOptions.ini", ex);
                return false;
            }

            // Confirmed SU can read this with both forward and backward slashes
            var stubPath = Path.Combine(Paths.Startup, Paths.StubFileName);

            for (var i = 0; i < lines.Count; i++)
            {
                if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                insertLineIndex = i + 1;
                while (i < lines.Count - 1)
                {
                    var lt = lines[i + 1].Trim();
                    if (lt.StartsWithI(externSelectorKey))
                    {
                        lines[i + 1] = externSelectorKey + stubPath;
                        existingKeyOverwritten = true;
                        break;
                    }

                    if (!lt.IsEmpty() && lt[0] == '[' && lt[lt.Length - 1] == ']') break;

                    i++;
                }
                break;
            }

            if (!existingKeyOverwritten)
            {
                if (insertLineIndex < 0) return false;
                lines.Insert(insertLineIndex, externSelectorKey + stubPath);
            }

            try
            {
                File.WriteAllLines(ini, lines, Encoding.Default);
            }
            catch (Exception ex)
            {
                Log("Problem writing SneakyOptions.ini", ex);
                return false;
            }

            return true;
        }

        internal static bool PlayFM(FanMission fm)
        {
            if (fm.Game == null)
            {
                View.ShowAlert(LText.AlertMessages.Play_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var gameExe = GetGameExeFromGameType((Game)fm.Game);

            #region Exe: Fail if blank or not found

            var gameName = GetGameNameFromGameType((Game)fm.Game);

            if (gameExe.IsEmpty() || !File.Exists(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_ExecutableNotFoundFM,
                    LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            #region Exe: Fail if already running

            if (GameIsRunning(gameExe, checkAllGames: true))
            {
                View.ShowAlert(LText.AlertMessages.Play_AnyGameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty())
            {
                return false;
            }

            if (GameIsDark(fm))
            {
                var success = SetDarkFMSelectorToAngelLoader((Game)fm.Game);
                if (!success)
                {
                    Log("Unable to set us as the selector for " + fm.Game + " (" +
                             nameof(SetDarkFMSelectorToAngelLoader) + " returned false)", stackTrace: true);
                }
            }
            else if (fm.Game == Game.Thief3)
            {
                var success = SetT3FMSelectorToAngelLoader();
                if (!success)
                {
                    Log("Unable to set us as the selector for Thief: Deadly Shadows (" +
                             nameof(SetT3FMSelectorToAngelLoader) + " returned false)", stackTrace: true);
                }
            }

            // Only use the stub if we need to pass something we can't pass on the command line
            // Add quotes around it in case there are spaces in the dir name. Will only happen if you put an FM
            // dir in there manually. Which if you do, you're on your own mate.
            var args = "-fm=\"" + fm.InstalledDir + "\"";
            if (!fm.DisabledMods.IsWhiteSpace() || fm.DisableAllMods)
            {
                args = "-fm";
                Paths.PrepareTempPath(Paths.StubCommTemp);

                try
                {
                    using (var sw = new StreamWriter(Paths.StubCommFilePath, false, Encoding.UTF8))
                    {
                        sw.WriteLine("SelectedFMName=" + fm.InstalledDir);
                        sw.WriteLine("DisabledMods=" + (fm.DisableAllMods ? "*" : fm.DisabledMods));
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception writing stub file " + Paths.StubFileName, ex);
                }
            }

            using (var proc = new Process())
            {
                proc.StartInfo.FileName = gameExe;
                proc.StartInfo.Arguments = args;
                proc.StartInfo.WorkingDirectory = gamePath;
                try
                {
                    proc.Start();
                }
                catch (Exception ex)
                {
                    Log("Exception starting game " + gameExe, ex);
                }
            }

            // Don't clear the temp folder here, because the stub program will need to read from it. It will
            // delete the temp file itself after it's done with it.

            return true;
        }

        internal static bool OpenFMInDromEd(FanMission fm)
        {
            if (!GameIsDark(fm)) return false;

            if (fm.Game == null)
            {
                View.ShowAlert(LText.AlertMessages.DromEd_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            if (gameExe.IsEmpty())
            {
                Log("gameExe is empty for " + fm.Game, stackTrace: true);
                return false;
            }

            #region Exe: Fail if blank or not found

            var dromedExe = GetDromEdExe((Game)fm.Game);
            if (dromedExe.IsEmpty())
            {
                View.ShowAlert(LText.AlertMessages.DromEd_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            var success = SetDarkFMSelectorToAngelLoader((Game)fm.Game);
            if (!success)
            {
                Log("Unable to set us as the selector for " + fm.Game + " (" +
                         nameof(SetDarkFMSelectorToAngelLoader) + " returned false)", stackTrace: true);
            }

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty()) return false;

            // We don't need the stub for DromEd, cause we don't need to pass anything except the fm folder
            using (var proc = new Process())
            {
                proc.StartInfo.FileName = dromedExe;
                proc.StartInfo.Arguments = "-fm=\"" + fm.InstalledDir + "\"";
                proc.StartInfo.WorkingDirectory = gamePath;

                try
                {
                    proc.Start();
                }
                catch (Exception ex)
                {
                    Log("Exception starting " + dromedExe, ex);
                }
            }

            return true;
        }

        #endregion

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

        internal static (bool Success, string[] DMLFiles)
        GetDMLFiles(FanMission fm)
        {
            try
            {
                var dmlFiles = Directory.GetFiles(Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir),
                    "*.dml", SearchOption.TopDirectoryOnly);
                for (int i = 0; i < dmlFiles.Length; i++)
                {
                    dmlFiles[i] = Path.GetFileName(dmlFiles[i]);
                }
                return (true, dmlFiles);
            }
            catch (Exception ex)
            {
                Log("Exception getting DML files for " + fm.InstalledDir + ", game: " + fm.Game, ex);
                return (false, new string[] { });
            }
        }

        #region Cacheable FM data

        // If some files exist but not all that are in the zip, the user can just re-scan for this data by clicking
        // a button, so don't worry about it
        internal static async Task<CacheData> GetCacheableData(FanMission fm)
        {
            if (fm.Game == Game.Unsupported)
            {
                if (!fm.InstalledDir.IsEmpty())
                {
                    var fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);
                    if (!fmCachePath.TrimEnd('\\').EqualsI(Paths.FMsCache.TrimEnd('\\')) && Directory.Exists(fmCachePath))
                    {
                        try
                        {
                            foreach (var f in Directory.EnumerateFiles(fmCachePath, "*",
                                SearchOption.TopDirectoryOnly))
                            {
                                File.Delete(f);
                            }

                            foreach (var d in Directory.EnumerateDirectories(fmCachePath, "*",
                                SearchOption.TopDirectoryOnly))
                            {
                                Directory.Delete(d, recursive: true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log(
                                "Exception enumerating files or directories in cache for " + fm.Archive + " / " +
                                fm.InstalledDir, ex);
                        }
                    }
                }
                return new CacheData();
            }

            return FMIsReallyInstalled(fm)
                ? FMCache.GetCacheableDataInFMInstalledDir(fm)
                : await FMCache.GetCacheableDataInFMCacheDir(fm, ProgressBox);
        }

        #endregion

        internal static (string ReadmePath, ReadmeType ReadmeType)
        GetReadmeFileAndType(FanMission fm)
        {
            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var instBasePath = GetFMInstallsBasePath(fm.Game);
            if (fm.Installed)
            {
                if (instBasePath.IsWhiteSpace())
                {
                    var ex = new ArgumentException(@"FM installs base path is empty", nameof(instBasePath));
                    Log(ex.Message, ex);
                    throw ex;
                }
                else if (!Directory.Exists(instBasePath))
                {
                    var ex = new DirectoryNotFoundException("FM installs base path doesn't exist");
                    Log(ex.Message, ex);
                    throw ex;
                }
            }

            var readmeOnDisk = FMIsReallyInstalled(fm)
                ? Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir, fm.SelectedReadme)
                : Path.Combine(Paths.FMsCache, fm.InstalledDir, fm.SelectedReadme);

            if (fm.SelectedReadme.ExtIsHtml()) return (readmeOnDisk, ReadmeType.HTML);

            var rtfHeader = new char[6];

            try
            {
                using (var sr = new StreamReader(readmeOnDisk, Encoding.ASCII)) sr.ReadBlock(rtfHeader, 0, 6);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var rType = string.Concat(rtfHeader).EqualsI(@"{\rtf1")
                ? ReadmeType.RichText
                : ReadmeType.PlainText;

            return (readmeOnDisk, rType);
        }

        // Autodetect safe (non-spoiler) readme
        internal static string DetectSafeReadme(List<string> readmeFiles, string fmTitle)
        {
            // Since an FM's readmes are very few in number, we can afford to be all kinds of lazy and slow here

            string StripPunctuation(string str)
            {
                return str.Replace(" ", "").Replace("-", "").Replace("_", "").Replace(".", "")
                    .Replace(",", "").Replace(";", "").Replace("'", "");
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
                    safeReadmes.Sort(new SafeReadmeComparer());

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

                if (numSafe == 1 && safeIndex > -1)
                {
                    safeReadme = readmeFiles[safeIndex];
                }
            }

            return safeReadme;
        }

        internal static void OpenFMFolder(FanMission fm)
        {
            var installsBasePath = GetFMInstallsBasePath(fm.Game);
            if (installsBasePath.IsEmpty())
            {
                View.ShowAlert(LText.AlertMessages.Patch_FMFolderNotFound, LText.AlertMessages.Alert);
                return;
            }
            var fmDir = Path.Combine(installsBasePath, fm.InstalledDir);
            if (!Directory.Exists(fmDir))
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
                (path, _) = GetReadmeFileAndType(fm);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(GetReadmeFileAndType), ex);
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
            TopRightTab topRightTab,
            TopRightTabOrder topRightTabOrder,
            bool topRightPanelCollapsed,
            float readmeZoomFactor)
        {
            Config.MainWindowState = mainWindowState;
            Config.MainWindowSize = new Size { Width = mainWindowSize.Width, Height = mainWindowSize.Height };
            Config.MainWindowLocation = new Point(mainWindowLocation.X, mainWindowLocation.Y);
            Config.MainSplitterPercent = mainSplitterPercent;
            Config.TopSplitterPercent = topSplitterPercent;

            Config.Columns.Clear();
            Config.Columns.AddRange(columns);

            Config.SortedColumn = (Column)sortedColumn;
            Config.SortDirection = sortDirection;

            Config.FMsListFontSizeInPoints = fmsListFontSizeInPoints;

            filter.DeepCopyTo(Config.Filter);

            Config.TopRightTab = topRightTab;

            Config.TopRightTabOrder.StatsTabPosition = topRightTabOrder.StatsTabPosition;
            Config.TopRightTabOrder.EditFMTabPosition = topRightTabOrder.EditFMTabPosition;
            Config.TopRightTabOrder.CommentTabPosition = topRightTabOrder.CommentTabPosition;
            Config.TopRightTabOrder.TagsTabPosition = topRightTabOrder.TagsTabPosition;
            Config.TopRightTabOrder.PatchTabPosition = topRightTabOrder.PatchTabPosition;

            Config.TopRightPanelCollapsed = topRightPanelCollapsed;

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

            Config.ReadmeZoomFactor = readmeZoomFactor;
        }

        private static readonly ReaderWriterLockSlim ReadWriteLock = new ReaderWriterLockSlim();

        internal static void WriteFullFMDataIni()
        {
            try
            {
                ReadWriteLock.EnterWriteLock();
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
                ReadWriteLock.ExitWriteLock();
            }
            catch (Exception ex)
            {
                Log("Exception writing FM data ini", ex);
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

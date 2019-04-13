using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.FMBackupAndRestore;
using static AngelLoader.Ini.Ini;
using Timer = System.Timers.Timer;

namespace AngelLoader
{
    internal sealed class BusinessLogic
    {
        private readonly MainForm View;
        private readonly ProgressPanel ProgressBox;

        internal List<FanMission> FMsViewList = new List<FanMission>();
        private List<FanMission> FMDataIniList = new List<FanMission>();

        private CancellationTokenSource ScanCts;
        private CancellationTokenSource ExtractCts;

        internal BusinessLogic(MainForm view, ProgressPanel progressBox)
        {
            View = view;
            ProgressBox = progressBox;
        }

        internal async Task Init()
        {
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
        }

        private Error CheckPaths()
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

        internal string GetDromEdExe(Game game)
        {
            var gameExe = GetGameExeFromGameType(game);
            if (gameExe.IsEmpty()) return "";

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty()) return "";

            var dromEdExe = Path.Combine(gamePath, Paths.DromEdExe);
            return !gamePath.IsEmpty() && File.Exists(dromEdExe) ? dromEdExe : "";
        }

        internal string GetInstFMsPathFromCamModIni(string gamePath, out Error error)
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

        internal (Error Error, bool UseCentralSaves, string Path)
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

        internal void FindFMs(bool startup = false)
        {
            if (!startup)
            {
                // Make sure we don't lose anything when we re-find!
                try
                {
                    WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
                }
                catch (Exception ex)
                {
                    Log("Exception writing FM data ini", ex);
                }
            }

            // Init or reinit - must be deep-copied or changes propagate back because reference types
            DeepCopyGlobalTags(PresetTags, GlobalTags);

            // This will also clear the Checked status of all FMs. Crucial if we're running this again.
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
                    MessageBox.Show("Exception reading FM data ini. Exiting. Please check " + Paths.LogFile,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(1);
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
                            (f.ExtEqualsI(".zip") || f.ExtEqualsI(".7z")) && !f.ContainsI(Paths.FMSelBak))
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

            // Convert the archive and folder names to FM objects so as to allow them to be Union'd
            var fmaList = new List<FanMission>();
            foreach (var item in fmArchives)
            {
                fmaList.Add(new FanMission { Archive = item });
            }

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

            // NOTE: The order of lists in the Union methods matters - a.Union(b) is different from b.Union(a).
            // Don't change them.

            // NOTE: Pretty sure I used Union because it's supposed to be faster than a nested loop?

            if (FMDataIniList.Count > 0)
            {
                // Push back existing entries to the new list so none will be removed when they get combined
                fmaList = FMDataIniList.Union(fmaList, new FMArchiveNameComparer()).ToList();
            }

            // TODO: If an FM already in the list doesn't have an archive name, it could be deleted from the list here.
            // Won't happen unless the user actually went in and deleted the archive name themselves.
            // Won't fix? Fix if I ever clean this up and make the Unions manual?
            FMDataIniList = FMDataIniList.Union(fmaList, new FMArchiveNameComparer()).ToList();

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

            // Set archive installed folder names right off the bat and store them permanently
            for (var i = 0; i < FMDataIniList.Count; i++)
            {
                var fm = FMDataIniList[i];

                if (!fm.InstalledDir.IsEmpty() && fm.Archive.IsEmpty())
                {
                    var archiveName = GetArchiveNameFromInstalledDir(fm, fmArchives);
                    if (archiveName.IsEmpty()) continue;

                    // Exponential (slow) stuff, but we only do it once to correct the list and then never again
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

            // Handle installed folder name collisions
            foreach (var fm in FMDataIniList)
            {
                if (fm.InstalledDir.IsEmpty())
                {
                    var truncate = fm.Game != Game.Thief3;
                    var instDir = fm.Archive.ToInstalledFMDirNameFMSel(truncate);
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
                    !fmArchives.ContainsI(item.Archive))
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

            // Link the lists back up because they get broken in here
            View.LinkViewList();
        }

        // Super quick-n-cheap hack for perf
        internal List<int> ViewListGamesNull = new List<int>();

        internal async Task<bool> ScanFM(FanMission fm, ScanOptions scanOptions,
            bool overwriteUnscannedFields = true, bool markAsScanned = false)
        {
            return await ScanFMs(new List<FanMission> { fm }, scanOptions, overwriteUnscannedFields, markAsScanned);
        }

        private string GetArchiveNameFromInstalledDir(FanMission fm, List<string> archives)
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
                bool truncate = fm.Game != Game.Thief3;
                var tryArchive =
                    archives.FirstOrDefault(x => x.ToInstalledFMDirNameFMSel(truncate).EqualsI(fm.InstalledDir)) ??
                    archives.FirstOrDefault(x => x.ToInstalledFMDirNameNDL().EqualsI(fm.InstalledDir)) ??
                    FMDataIniList.FirstOrDefault(x => x.Archive.ToInstalledFMDirNameFMSel(truncate).EqualsI(fm.InstalledDir))?.Archive ??
                    FMDataIniList.FirstOrDefault(x => x.Archive.ToInstalledFMDirNameNDL().EqualsI(fm.InstalledDir))?.Archive;

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

        internal async Task<bool> ScanFMs(List<FanMission> fmsToScan, ScanOptions scanOptions,
            bool overwriteUnscannedFields = true, bool markAsScanned = false)
        {
            if (fmsToScan.Count == 0) return false;

            void ReportProgress(ProgressReport pr)
            {
                var fmIsZip = pr.FMName.ExtEqualsI(".zip") || pr.FMName.ExtEqualsI(".7z");
                var name = fmIsZip ? pr.FMName.GetFileNameFast() : pr.FMName.GetDirNameFast();
                ProgressBox.ReportScanProgress(pr.FMNumber, pr.FMsTotal, pr.Percent, name);
            }

            var scanningOne = fmsToScan.Count == 1;

            if (scanningOne)
            {
                Log(nameof(ScanFMs) + ": Scanning one", methodName: false);
                View.Block();
                ProgressBox.ProgressTask = ProgressPanel.ProgressTasks.ScanAllFMs;
                ProgressBox.ShowProgressWindow(ProgressBox.ProgressTask, suppressShow: true);
            }
            else
            {
                ProgressBox.ShowScanningAllFMs();
            }

            // Block user input to the form initially to mimic the UI thread being blocked, but then if the scan
            // is taking more than 500ms then unblock input and throw up the progress box
            // TODO: This is pretty hairy, try and organize this better
            using (var timeOut = new Timer(500) { AutoReset = false })
            {
                timeOut.Elapsed += (sender, e) =>
                {
                    if (scanningOne)
                    {
                        Log(nameof(ScanFMs) + ": timeOut.Elapsed: showing ProgressBox");
                        ProgressBox.BeginInvoke(new Action(ProgressBox.ShowThis));
                        View.BeginInvoke(new Action(View.Unblock));
                    }
                };
                timeOut.Start();

                ScanCts = new CancellationTokenSource();

                try
                {
                    var fms = new List<string>();
                    // Get archive paths list only once and cache it - in case of "include subfolders" being true,
                    // cause then it will hit the actual disk rather than just going through a list of paths in
                    // memory
                    Log(nameof(ScanFMs) + ": about to call " + nameof(GetFMArchivePaths) + " with subfolders=" + Config.FMArchivePathsIncludeSubfolders);
                    var archivePaths = await Task.Run(GetFMArchivePaths);
                    foreach (var fm in fmsToScan)
                    {
                        var fmArchivePath = await Task.Run(() => FindFMArchive(fm, archivePaths));
                        if (!fm.Archive.IsEmpty() && !fmArchivePath.IsEmpty())
                        {
                            fms.Add(fmArchivePath);
                        }
                        else if (GameIsKnownAndSupported(fm))
                        {
                            var fmInstalledPath = GetFMInstallsBasePath(fm);
                            if (!fmInstalledPath.IsEmpty())
                            {
                                fms.Add(Path.Combine(fmInstalledPath, fm.InstalledDir));
                            }
                        }
                        else
                        {
                            continue;
                        }

                        if (ScanCts.IsCancellationRequested)
                        {
                            ScanCts?.Dispose();
                            ProgressBox.HideThis();
                            return false;
                        }
                    }

                    List<ScannedFMData> fmDataList;
                    try
                    {
                        var progress = new Progress<ProgressReport>(ReportProgress);

                        using (var scanner = new Scanner())
                        {
                            scanner.LogFile = Paths.ScannerLogFile;
                            scanner.ZipEntryNameEncoding = Encoding.UTF8;
                            Paths.PrepareTempPath(Paths.FMScannerTemp);
                            fmDataList = await scanner.ScanAsync(fms, Paths.FMScannerTemp, scanOptions, progress,
                                ScanCts.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        return false;
                    }
                    finally
                    {
                        ScanCts?.Dispose();
                        ProgressBox.HideThis();
                    }

                    foreach (var fm in fmDataList)
                    {
                        if (fm == null)
                        {
                            // We need to return fail for scanning one, else we get into an infinite loop because
                            // of a refresh that gets called in that case
                            if (scanningOne) return false;
                            continue;
                        }

                        var sel = fmsToScan.FirstOrDefault(x =>
                            x.Archive.RemoveExtension().EqualsI(fm.ArchiveName.RemoveExtension()) ||
                            x.InstalledDir.EqualsI(fm.ArchiveName.RemoveExtension()));

                        if (sel == null) continue;

                        var gameSup = fm.Game != Games.Unsupported;

                        if (overwriteUnscannedFields || scanOptions.ScanTitle)
                        {
                            sel.Title = !fm.Title.IsEmpty() ? fm.Title : fm.ArchiveName.RemoveExtension();

                            if (gameSup)
                            {
                                sel.AltTitles = new List<string> { fm.Title };
                                sel.AltTitles.AddRange(fm.AlternateTitles);
                            }
                            else
                            {
                                sel.AltTitles = new List<string>();
                            }
                        }

                        if (overwriteUnscannedFields || scanOptions.ScanSize)
                        {
                            sel.SizeString = gameSup ? fm.Size.ConvertSize() : "";
                            sel.SizeBytes = (ulong)(gameSup ? fm.Size ?? 0 : 0);
                        }
                        if (overwriteUnscannedFields || scanOptions.ScanReleaseDate)
                        {
                            sel.ReleaseDate = gameSup ? fm.LastUpdateDate : null;
                        }
                        if (overwriteUnscannedFields || scanOptions.ScanCustomResources)
                        {
                            sel.HasMap = gameSup ? fm.HasMap : null;
                            sel.HasAutomap = gameSup ? fm.HasAutomap : null;
                            sel.HasScripts = gameSup ? fm.HasCustomScripts : null;
                            sel.HasTextures = gameSup ? fm.HasCustomTextures : null;
                            sel.HasSounds = gameSup ? fm.HasCustomSounds : null;
                            sel.HasObjects = gameSup ? fm.HasCustomObjects : null;
                            sel.HasCreatures = gameSup ? fm.HasCustomCreatures : null;
                            sel.HasMotions = gameSup ? fm.HasCustomMotions : null;
                            sel.HasMovies = gameSup ? fm.HasMovies : null;
                            sel.HasSubtitles = gameSup ? fm.HasCustomSubtitles : null;
                        }

                        if (overwriteUnscannedFields || scanOptions.ScanAuthor)
                        {
                            sel.Author = gameSup ? fm.Author : "";
                        }

                        if (overwriteUnscannedFields || scanOptions.ScanGameType)
                        {
                            sel.Game =
                                fm.Game == Games.Unsupported ? Game.Unsupported :
                                fm.Game == Games.TDP ? Game.Thief1 :
                                fm.Game == Games.TMA ? Game.Thief2 :
                                fm.Game == Games.TDS ? Game.Thief3 :
                                (Game?)null;
                        }

                        if (overwriteUnscannedFields || scanOptions.ScanLanguages)
                        {
                            sel.Languages = gameSup ? fm.Languages : new string[0];
                            sel.LanguagesString = gameSup
                                ? fm.Languages != null ? string.Join(", ", fm.Languages) : ""
                                : "";
                        }

                        if (overwriteUnscannedFields || scanOptions.ScanTags)
                        {
                            sel.TagsString = gameSup ? fm.TagsString : "";

                            // Don't clear the tags, because the user could have added a bunch and we should only
                            // add to those, not overwrite them
                            if (gameSup) AddTagsToFMAndGlobalList(sel.TagsString, sel.Tags);
                        }

                        sel.MarkedScanned = markAsScanned;
                    }

                    ProgressBox.HideThis();

                    WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
                }
                catch (Exception ex)
                {
                    Log("Exception in ScanFMs", ex);
                }
                finally
                {
                    ProgressBox.HideThis();
                    View.Unblock();
                }
            }

            return true;
        }

        internal void CancelScan()
        {
            try
            {
                ScanCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal async Task ScanNewFMsForGameType()
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

        internal async Task<bool>
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

        internal async Task<bool> ImportFromNDL(string iniFile)
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

        internal async Task<bool> ImportFromFMSel(string iniFile)
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

        private async Task ScanAndFind(List<FanMission> fms, ScanOptions scanOptions, bool overwriteUnscannedFields = false)
        {
            if (fms.Count == 0) return;

            await ScanFMs(fms, scanOptions, overwriteUnscannedFields, markAsScanned: true);
            FindFMs();
        }

        #endregion

        #region Install, Uninstall, Play

        internal async Task InstallOrUninstall(FanMission fm)
        {
            await (fm.Installed ? UninstallFM(fm) : InstallFM(fm));
        }

        internal async Task<bool> InstallFM(FanMission fm)
        {
            Debug.Assert(!fm.Installed, "!fm.Installed");

            if (fm.Game == null)
            {
                View.ShowAlert(LText.AlertMessages.Install_UnknownGameType, LText.AlertMessages.Alert);
                return false;
            }

            if (fm.Game == Game.Unsupported)
            {
                View.ShowAlert(LText.AlertMessages.Install_UnsupportedGameType, LText.AlertMessages.Alert);
                return false;
            }

            var fmArchivePath = FindFMArchive(fm);

            if (fmArchivePath.IsEmpty())
            {
                View.ShowAlert(LText.AlertMessages.Install_ArchiveNotFound, LText.AlertMessages.Alert);
                return false;
            }

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
            if (!File.Exists(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_ExecutableNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var instBasePath = GetFMInstallsBasePath(fm);

            if (!Directory.Exists(instBasePath))
            {
                View.ShowAlert(LText.AlertMessages.Install_FMInstallPathNotFound, LText.AlertMessages.Alert);
                return false;
            }

            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" +
                               LText.AlertMessages.Install_GameIsRunning, LText.AlertMessages.Alert);
                return false;
            }

            var fmInstalledPath = Path.Combine(instBasePath, fm.InstalledDir);

            ExtractCts = new CancellationTokenSource();

            ProgressBox.ShowInstallingFM();

            // Framework zip extracting is much faster, so use it if possible
            bool canceled = fmArchivePath.ExtEqualsI(".zip")
                ? !await InstallFMZip(fmArchivePath, fmInstalledPath)
                : !await InstallFMSevenZip(fmArchivePath, fmInstalledPath);

            if (canceled)
            {
                ProgressBox.SetCancelingFMInstall();
                await Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete(fmInstalledPath, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        Log("Unable to delete FM installed directory " + fmInstalledPath, ex);
                    }
                });
                ProgressBox.HideThis();
                return false;
            }

            fm.Installed = true;

            WriteFMDataIni(FMDataIniList, Paths.FMDataIni);

            try
            {
                using (var sw = new StreamWriter(Path.Combine(fmInstalledPath, Paths.FMSelInf), append: false))
                {
                    await sw.WriteLineAsync("Name=" + fm.InstalledDir);
                    await sw.WriteLineAsync("Archive=" + fm.Archive);
                }
            }
            catch (Exception ex)
            {
                Log("Couldn't create " + Paths.FMSelInf + " in " + fmInstalledPath, ex);
            }

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm));
            try
            {
                ProgressBox.ShowConvertingFiles();
                await ac.ConvertMP3sToWAVs();

                if (Config.ConvertOGGsToWAVsOnInstall)
                {
                    await ac.ConvertOGGsToWAVsInternal();
                }
                else if (Config.ConvertWAVsTo16BitOnInstall)
                {
                    await ac.ConvertWAVsTo16BitInternal();
                }
            }
            finally
            {
                ProgressBox.HideThis();
            }

            try
            {
                await RestoreSavesAndScreenshots(fm);
            }
            catch (Exception ex)
            {
                Log("Exception in " + nameof(RestoreSavesAndScreenshots), ex);
            }
            finally
            {
                ProgressBox.HideThis();
            }

            // Not doing RefreshSelectedFMRowOnly() because that wouldn't update the install/uninstall buttons
            await View.RefreshSelectedFM(refreshReadme: false);

            return true;
        }

        private async Task<bool> InstallFMZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(fmInstalledPath);

                    var fs = new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read);
                    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read, leaveOpen: false))
                    {
                        int filesCount = archive.Entries.Count;
                        for (var i = 0; i < filesCount; i++)
                        {
                            var entry = archive.Entries[i];

                            var fileName = entry.FullName.Replace('/', '\\');

                            if (fileName[fileName.Length - 1] == '\\') continue;

                            if (fileName.Contains('\\'))
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                    fileName.Substring(0, fileName.LastIndexOf('\\'))));
                            }

                            var extractedName = Path.Combine(fmInstalledPath, fileName);
                            entry.ExtractToFile(extractedName, overwrite: true);

                            UnSetReadOnly(Path.Combine(fmInstalledPath, extractedName));

                            int percent = (100 * (i + 1)) / filesCount;

                            View.BeginInvoke(new Action(() => ProgressBox.ReportFMExtractProgress(percent)));

                            if (ExtractCts.Token.IsCancellationRequested)
                            {
                                canceled = true;
                                return;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception while installing zip " + fmArchivePath + " to " + fmInstalledPath, ex);
                    View.BeginInvoke(new Action(() =>
                        View.ShowAlert(LText.AlertMessages.Extract_ZipExtractFailedFullyOrPartially,
                            LText.AlertMessages.Alert)));
                }
                finally
                {
                    View.BeginInvoke(new Action(() => ProgressBox.HideThis()));
                }
            });

            return !canceled;
        }

        private async Task<bool> InstallFMSevenZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(fmInstalledPath);

                    using (var extractor = new SevenZipExtractor(fmArchivePath))
                    {
                        extractor.Extracting += (sender, e) =>
                        {
                            if (!canceled && ExtractCts.Token.IsCancellationRequested)
                            {
                                canceled = true;
                            }
                            if (canceled)
                            {
                                ProgressBox.BeginInvoke(new Action(ProgressBox.SetCancelingFMInstall));
                                return;
                            }
                            ProgressBox.BeginInvoke(new Action(() =>
                                ProgressBox.ReportFMExtractProgress(e.PercentDone)));
                        };

                        extractor.FileExtractionFinished += (sender, e) =>
                        {
                            SetFileAttributesFromSevenZipEntry(e.FileInfo,
                                Path.Combine(fmInstalledPath, e.FileInfo.FileName));

                            if (ExtractCts.Token.IsCancellationRequested)
                            {
                                ProgressBox.BeginInvoke(new Action(ProgressBox.SetCancelingFMInstall));
                                canceled = true;
                                e.Cancel = true;
                            }
                        };

                        try
                        {
                            extractor.ExtractArchive(fmInstalledPath);
                        }
                        catch (Exception ex)
                        {
                            // Throws a weird exception even if everything's fine
                            Log("extractor.ExtractArchive(fmInstalledPath) exception (probably ignorable)",
                                ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception extracting 7z " + fmArchivePath + " to " + fmInstalledPath, ex);
                    View.BeginInvoke(new Action(() =>
                        View.ShowAlert(LText.AlertMessages.Extract_SevenZipExtractFailedFullyOrPartially,
                            LText.AlertMessages.Alert)));
                }
                finally
                {
                    View.BeginInvoke(new Action(() => ProgressBox.HideThis()));
                }
            });

            return !canceled;
        }

        internal void CancelInstallFM(FanMission fm) => ExtractCts.Cancel();

        internal async Task UninstallFM(FanMission fm)
        {
            if (!fm.Installed || !GameIsKnownAndSupported(fm)) return;

            Debug.Assert(fm.Game != null, "fm.Game != null");

            var gameExe = GetGameExeFromGameType((Game)fm.Game);
            var gameName = GetGameNameFromGameType((Game)fm.Game);
            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(
                    gameName + ":\r\n" + LText.AlertMessages.Uninstall_GameIsRunning, LText.AlertMessages.Alert);
                return;
            }

            ProgressBox.ShowUninstallingFM();

            try
            {
                var fmInstalledPath = Path.Combine(GetFMInstallsBasePath(fm), fm.InstalledDir);

                var fmDirExists = await Task.Run(() => Directory.Exists(fmInstalledPath));
                if (!fmDirExists)
                {
                    var yes = View.AskToContinue(LText.AlertMessages.Uninstall_FMAlreadyUninstalled,
                        LText.AlertMessages.Alert);
                    if (yes)
                    {
                        fm.Installed = false;
                        await View.RefreshSelectedFM(refreshReadme: false);
                    }
                    return;
                }

                var fmArchivePath = await Task.Run(() => FindFMArchive(fm));

                if (fmArchivePath.IsEmpty())
                {
                    var cont = View.AskToContinue(LText.AlertMessages.Uninstall_ArchiveNotFound,
                        LText.AlertMessages.Warning);

                    if (!cont) return;
                }

                // If fm.Archive is blank, then fm.InstalledDir will be used for the backup file name instead.
                // This file will be included in the search when restoring, and the newest will be taken as
                // usual.

                // fm.Archive can be blank at this point when all of the following conditions are true:
                // -fm is installed
                // -fm does not have fmsel.inf in its installed folder (or its fmsel.inf is blank or invalid)
                // -fm was not in the database on startup
                // -the folder where the FM's archive is located is not in Config.FMArchivePaths (or its sub-
                //  folders if that option is enabled)

                // It's not particularly likely, but it could happen if the user had NDL-installed FMs (which
                // don't have fmsel.inf), started AngelLoader for the first time, didn't specify the right
                // archive folder on initial setup, and hasn't imported from NDL by this point.

                if (Config.BackupAlwaysAsk)
                {
                    var message = Config.BackupFMData == BackupFMData.SavesAndScreensOnly
                        ? LText.AlertMessages.Uninstall_BackupSavesAndScreenshots
                        : LText.AlertMessages.Uninstall_BackupAllData;
                    var (cancel, cont, dontAskAgain) =
                        View.AskToContinueWithCancel_TD(message, LText.AlertMessages.Confirm);
                    Config.BackupAlwaysAsk = !dontAskAgain;
                    if (cancel) return;
                    if (cont) await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }
                else
                {
                    await BackupFM(fm, fmInstalledPath, fmArchivePath);
                }

                // --- DEBUG
                //return;

                // TODO: Give the user the option to retry or something, if it's cause they have a file open
                if (!await DeleteFMInstalledDirectory(fmInstalledPath))
                {
                    // TODO: Make option to open the folder in Explorer and delete it manually?
                    View.ShowAlert(LText.AlertMessages.Uninstall_UninstallNotCompleted,
                        LText.AlertMessages.Alert);
                }

                fm.Installed = false;

                // NewDarkLoader still truncates its Thief 3 install names, but the "official" way is not to
                // do it for Thief 3. If the user already has FMs that were installed with NewDarkLoader, we
                // just read in the truncated names and treat them as normal for compatibility purposes. But
                // if we've just uninstalled the mission, then we can safely convert InstalledDir back to full
                // un-truncated form for future use.
                if (fm.Game == Game.Thief3 && !fm.Archive.IsEmpty())
                {
                    fm.InstalledDir = fm.Archive.ToInstalledFMDirNameFMSel(truncate: false);
                }

                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
                await View.RefreshSelectedFM(refreshReadme: false);
            }
            catch (Exception ex)
            {
                Log("Exception uninstalling FM " + fm.Archive + ", " + fm.InstalledDir, ex);
                View.BeginInvoke(new Action(() =>
                    View.ShowAlert(LText.AlertMessages.Uninstall_FailedFullyOrPartially,
                        LText.AlertMessages.Alert)));
            }
            finally
            {
                ProgressBox.HideThis();
            }
        }

        private static async Task<bool> DeleteFMInstalledDirectory(string path)
        {
            bool result = await Task.Run(() =>
            {
                var triedReadOnlyRemove = false;

                // Failsafe cause this is nasty
                for (int i = 0; i < 2; i++)
                {
                    try
                    {
                        Directory.Delete(path, recursive: true);
                        return true;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            if (triedReadOnlyRemove) return false;

                            // FMs installed by us will not have any readonly attributes set, so we work on the
                            // assumption that this is the rarer case and only do this extra work if we need to.
                            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                            {
                                new FileInfo(f).IsReadOnly = false;
                            }

                            foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                            {
                                new DirectoryInfo(d).Attributes = FileAttributes.Normal;
                            }

                            triedReadOnlyRemove = true;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }

                return false;
            });

            return result;
        }

        internal async Task ConvertOGGsToWAVs(FanMission fm)
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

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm));
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

        internal async Task ConvertWAVsTo16Bit(FanMission fm)
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

            var ac = new AudioConverter(fm, GetFMInstallsBasePath(fm));
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

        private static bool GameIsRunning(string gameExe, bool checkAllGames = false)
        {
            Log("Checking if " + gameExe + " is running. Listing processes...");

            // We're doing this whole rigamarole because the game might have been started by someone other than
            // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    var fn = GetProcessPath(proc.Id);
                    //Log.Info("Process filename: " + fn);
                    if (!fn.IsEmpty())
                    {
                        var fnb = fn.ToBackSlashes();
                        if ((checkAllGames &&
                             ((!Config.T1Exe.IsEmpty() && fnb.EqualsI(Config.T1Exe.ToBackSlashes())) ||
                              (!Config.T2Exe.IsEmpty() && fnb.EqualsI(Config.T2Exe.ToBackSlashes())) ||
                              (!Config.T3Exe.IsEmpty() && fnb.EqualsI(Config.T3Exe.ToBackSlashes())))) ||
                            (!checkAllGames &&
                             (!gameExe.IsEmpty() && fnb.EqualsI(gameExe.ToBackSlashes()))))
                        {
                            var logExe = checkAllGames ? "a game exe" : gameExe;

                            Log("Found " + logExe + " running: " + fn +
                                "\r\nReturning true, game should be blocked from starting");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Even if this were to be one of our games, if .NET won't let us find out then all we can do
                    // is shrug and move on.
                    Log("Exception caught in GameIsRunning", ex);
                }
            }

            return false;
        }

        private static string GetGameNameFromGameType(Game gameType)
        {
            return
                gameType == Game.Thief1 ? "Thief 1" :
                gameType == Game.Thief2 ? "Thief 2" :
                gameType == Game.Thief3 ? "Thief 3" :
                "[UnknownGameType]";
        }

        private static string GetGameExeFromGameType(Game gameType)
        {
            return
                gameType == Game.Thief1 ? Config.T1Exe :
                gameType == Game.Thief2 ? Config.T2Exe :
                gameType == Game.Thief3 ? Config.T3Exe :
                null;
        }

        internal bool PlayOriginalGame(Game game)
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
            bool matchedLineIsCommented = false;
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

        internal bool PlayFM(FanMission fm)
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
            var args = "-fm=" + fm.InstalledDir;
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

        internal bool OpenFMInDromEd(FanMission fm)
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
                proc.StartInfo.Arguments = "-fm=" + fm.InstalledDir;
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

        internal bool AddDML(FanMission fm, string sourceDMLPath)
        {
            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_AddDML_InstallDirNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var installedFMPath = Path.Combine(GetFMInstallsBasePath(fm), fm.InstalledDir);
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

        internal bool RemoveDML(FanMission fm, string dmlFile)
        {
            if (!FMIsReallyInstalled(fm))
            {
                View.ShowAlert(LText.AlertMessages.Patch_RemoveDML_InstallDirNotFound, LText.AlertMessages.Alert);
                return false;
            }

            var installedFMPath = Path.Combine(GetFMInstallsBasePath(fm), fm.InstalledDir);
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

        internal (bool Success, string[] DMLFiles)
        GetDMLFiles(FanMission fm)
        {
            try
            {
                var dmlFiles = Directory.GetFiles(Path.Combine(GetFMInstallsBasePath(fm), fm.InstalledDir),
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

        private static bool FMIsReallyInstalled(FanMission fm)
        {
            return fm.Installed &&
                   Directory.Exists(Path.Combine(GetFMInstallsBasePath(fm), fm.InstalledDir));
        }

        #region Cacheable FM data

        // If some files exist but not all that are in the zip, the user can just re-scan for this data by clicking
        // a button, so don't worry about it
        internal async Task<CacheData> GetCacheableData(FanMission fm)
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

        internal (string ReadmePath, ReadmeType ReadmeType)
        GetReadmeFileAndType(FanMission fm)
        {
            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var instBasePath = GetFMInstallsBasePath(fm);
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
                ? Path.Combine(GetFMInstallsBasePath(fm), fm.InstalledDir, fm.SelectedReadme)
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
        internal string DetectSafeReadme(List<string> readmeFiles, string fmTitle)
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
                    files.FirstOrDefault(x => x.ExtEqualsI(".glml")) ??
                    files.FirstOrDefault(x => x.ExtEqualsI(".rtf")) ??
                    files.FirstOrDefault(x => x.ExtEqualsI(".txt")) ??
                    files.FirstOrDefault(x => x.ExtEqualsI(".wri")) ??
                    files.FirstOrDefault(x => x.ExtEqualsI(".html")) ??
                    files.FirstOrDefault(x => x.ExtEqualsI(".htm"));
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

        internal void OpenFMFolder(FanMission fm)
        {
            var installsBasePath = GetFMInstallsBasePath(fm);
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

        internal void OpenWebSearchUrl(FanMission fm)
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

        internal void ViewHTMLReadme(FanMission fm)
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

        internal void OpenLink(string link)
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

        internal void UpdateConfig(
            FormWindowState mainWindowState,
            Size mainWindowSize,
            Point mainWindowLocation,
            float mainSplitterPercent,
            float topSplitterPercent,
            List<ColumnData> columns, int sortedColumn, SortOrder sortDirection,
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

        internal void WriteFullFMDataIni()
        {
            try
            {
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing FM data ini", ex);
            }
        }

        internal void Shutdown()
        {
            try
            {
                WriteConfigIni(Config, Paths.ConfigIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing config ini", ex);
            }

            try
            {
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing FM data ini", ex);
            }

            Application.Exit();
        }
    }
}

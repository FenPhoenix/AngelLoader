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
            Directory.CreateDirectory(Paths.Data);
            Directory.CreateDirectory(Paths.Languages);

            bool openSettings;
            if (File.Exists(Paths.ConfigIni))
            {
                ReadConfigIni(Paths.ConfigIni, Config);
                openSettings = !CheckPaths();
            }
            else
            {
                openSettings = true;
            }

            if (openSettings)
            {
                if (await View.OpenSettings(startup: true))
                {
                    var checkPaths = CheckPaths();

                    Debug.Assert(checkPaths, "checkPaths == true");

                    WriteConfigIni(Config, Paths.ConfigIni);
                }
                else
                {
                    // Since nothing of consequence has yet happened, it's okay to do the brutal quit
                    Environment.Exit(0);
                }
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
                    ReadLocalizationIni(f);
                    selFound = true;
                }
                ReadLanguageName(f);
            }
        }

        private bool CheckPaths()
        {
            var t1Exists = !Config.T1Exe.IsEmpty() && File.Exists(Config.T1Exe);
            var t2Exists = !Config.T2Exe.IsEmpty() && File.Exists(Config.T2Exe);
            var t3Exists = !Config.T3Exe.IsEmpty() && File.Exists(Config.T3Exe);

            if (t1Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T1Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                if (error == Error.CamModIniNotFound) return false;
                Config.T1FMInstallPath = gameFMsPath;
            }
            if (t2Exists)
            {
                var gamePath = Path.GetDirectoryName(Config.T2Exe);
                var gameFMsPath = GetInstFMsPathFromCamModIni(gamePath, out Error error);
                if (error == Error.CamModIniNotFound) return false;
                Config.T2FMInstallPath = gameFMsPath;
            }
            if (t3Exists)
            {
                // TODO: Thief 3 error info return
                // Thief 3 is going to have to return more error info than normal, because the error could be that
                // Sneaky Upgrade is not installed, that its SneakyOptions file doesn't exist, that the installed
                // FM path is not specified, that the registry key doesn't exist, that the folder it points to
                // doesn't exist... maybe some of that could be handled elsewhere, nearer to the time of install
                // (saves folder not found could happen when you go to back up the saves)
                var (success, useCentralSaves, path) = GetInstFMsPathFromT3();
                if (!success) return false;
                Config.T3FMInstallPath = path;
                Config.T3UseCentralSaves = useCentralSaves;
            }

            if (!t1Exists && !t2Exists && !t3Exists) return false;

            if (!Directory.Exists(Config.FMsBackupPath))
            {
                return false;
            }

            return true;
        }

        internal (bool Success, bool UseCentralSaves, string Path)
        GetInstFMsPathFromT3()
        {
            var soIni = Paths.GetSneakyOptionsIni();

            if (!File.Exists(soIni)) return (false, false, null);

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

            return fmInstPathFound ? (true, !ignoreSavesKey, fmInstPath) : (false, false, null);
        }

        internal string GetInstFMsPathFromCamModIni(string gamePath, out Error error)
        {
            var camModIni = Path.Combine(gamePath, "cam_mod.ini");

            if (!File.Exists(camModIni))
            {
                //error = Error.CamModIniNotFound;
                //return null;
                error = Error.None;
                return Path.Combine(gamePath, "FMs");
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
                    return Path.Combine(gamePath, "FMs");
                }
            }

            error = Error.None;
            return Directory.Exists(path) ? path : Path.Combine(gamePath, "FMs");
        }

        internal void FindFMs(bool startup = false)
        {
            var t = new Stopwatch();

            void timeCheck(string task)
            {
                t.Stop();
                Trace.WriteLine("Finished " + task + " in:\r\n" + t.Elapsed);
                t.Restart();
            }

            var overallTimer = new Stopwatch();
            overallTimer.Start();

            if (!startup)
            {
                // Make sure we don't lose anything when we re-find!
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
            }

            // Init or reinit - must be deep-copied or changes propagate back because reference types
            DeepCopyGlobalTags(PresetTags, GlobalTags);

            // This will also clear the Checked status of all FMs. Crucial if we're running this again.
            FMDataIniList.Clear();
            FMsViewList.Clear();

            t.Start();

            var fmDataIniExists = File.Exists(Paths.FMDataIni);

            timeCheck("File.Exists(Paths.FMDataIniPath);");

            if (fmDataIniExists)
            {
                ReadFMDataIni(Paths.FMDataIni, FMDataIniList);

                timeCheck("FMDataIniList = ReadFmDataIni(Paths.FMDataIniPath);");
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
                    foreach (var d in Directory.EnumerateDirectories(instPath, "*",
                        SearchOption.TopDirectoryOnly))
                    {
                        var dirName = d.GetTopmostDirName();
                        if (!dirName.EqualsI(".fmsel.cache")) instFMDirs.Add(dirName);
                    }
                }
            }

            timeCheck("EnumerateDirectories(Config.Thief*FMInstalledPath)");

            var fmArchives = new List<string>();

            foreach (var path in GetFMArchivePaths())
            {
                var files = Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);
                foreach (var f in files)
                {
                    if (!fmArchives.ContainsI(f.GetFileNameFast()) &&
                        (f.ExtEqualsI(".zip") || f.ExtEqualsI(".7z")) && !f.ContainsI(Paths.FMSelBak))
                    {
                        fmArchives.Add(f.GetFileNameFast());
                    }
                }
            }

            timeCheck("EnumerateFiles(all FM archive dirs)");

            #region PERF WORK

            var perfWholeTimer = new Stopwatch();
            perfWholeTimer.Start();

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

            var guT = new Stopwatch();
            guT.Start();

            if (t1List.Count > 0) GameUnion(t1List);
            if (t2List.Count > 0) GameUnion(t2List);
            if (t3List.Count > 0) GameUnion(t3List);

            guT.Stop();
            Trace.WriteLine("GameUnion timer: " + guT.Elapsed);

            #endregion

            // Set archive installed folder names right off the bat and store them permanently
            for (var i = 0; i < FMDataIniList.Count; i++)
            {
                var fm = FMDataIniList[i];

                if (!fm.InstalledDir.IsEmpty() && fm.Archive.IsEmpty())
                {
                    // TODO: Here's where I want to add something for installed FMs without fmsel.inf
                    var archiveName = GetArchiveNameFromInstalledDir(fm, fmArchives);
                    if (archiveName.IsEmpty())
                    {
                        //FMDataIniList.RemoveAt(i);
                        //i--;
                        continue;
                    }

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

            timeCheck("sort fmArchives and sort FMDataIniList");

            perfWholeTimer.Stop();
            Trace.WriteLine("Merging of lists took: " + perfWholeTimer.Elapsed);

            #endregion

            ViewListGamesNull.Clear();

            for (var i = 0; i < FMDataIniList.Count; i++)
            {
                var item = FMDataIniList[i];
                // FMDataIniList: Thief1(personal)+Thief2(personal)+All(1098 set)
                // Archive dirs: Thief1(personal)+Thief2(personal)
                // Total time taken running this for all FMs in FMDataIniList: 3~7ms
                // Good enough?
                if (!fmArchives.ContainsI(item.Archive) &&
                    (!item.Installed ||
                     // This is new and hasn't been timed, but whatever
                     (item.Game == Game.Thief1 && !t1InstalledFMDirs.ContainsI(item.InstalledDir)) ||
                     (item.Game == Game.Thief2 && !t2InstalledFMDirs.ContainsI(item.InstalledDir)) ||
                     (item.Game == Game.Thief3 && !t3InstalledFMDirs.ContainsI(item.InstalledDir))))
                {
                    continue;
                }

                // Perf so we don't have to iterate the list again later
                var (isNull, isSupported) = GameIsKnownAndSupportedReportIfNull(item);
                if (isNull) ViewListGamesNull.Add(i);
                if (isSupported && GetFMInstallsBasePath(item).IsEmpty()) continue;

                FMsViewList.Add(item);

                item.Title =
                    !item.Title.IsEmpty() ? item.Title :
                    !item.Archive.IsEmpty() ? item.Archive.RemoveExtension() :
                    item.InstalledDir;
                item.SizeString = ((long?)item.SizeBytes).ConvertSize();
                item.CommentSingleLine = item.Comment.FromEscapes().ToSingleLineComment(100);
                AddTagsToFMAndGlobalList(item.TagsString, item.Tags);
            }

            timeCheck("Fill FMsList");

            overallTimer.Stop();

            Trace.WriteLine("FindFMs() took: " + overallTimer.Elapsed);

            // Link the lists back up because they get broken in here
            View.LinkViewList();
        }

        // Super quick-n-cheap hack for perf
        internal List<int> ViewListGamesNull = new List<int>();

        internal async Task<bool> ScanFM(FanMission fm, ScanOptions scanOptions, bool overwriteUnscannedFields = true)
        {
            return await ScanFMs(new List<FanMission> { fm }, scanOptions, overwriteUnscannedFields);
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
            var fmselInf = Path.Combine(fmDir, "fmsel.inf");

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

                File.Delete(fmselInf);
                using (var sw = new StreamWriter(fmselInf, append: false))
                {
                    sw.WriteLine("Name=" + fm.InstalledDir);
                    sw.WriteLine("Archive=" + tryArchive);
                }

                return tryArchive;
            }

            if (!File.Exists(fmselInf)) return FixUp(true);

            var lines = File.ReadAllLines(fmselInf);

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
            bool overwriteUnscannedFields = true)
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
                        View.BeginInvoke(new Action(View.Unblock));
                        ProgressBox.BeginInvoke(new Action(ProgressBox.ShowThis));
                    }
                };
                timeOut.Start();

                ScanCts = new CancellationTokenSource();

                try
                {
                    var fms = new List<string>();
                    foreach (var fm in fmsToScan)
                    {
                        var fmArchivePath = await Task.Run(() => FindFMArchive(fm));
                        if (!fm.Archive.IsEmpty() && !fmArchivePath.IsEmpty())
                        {
                            fms.Add(fmArchivePath);
                        }
                        else if (GameIsKnownAndSupported(fm))
                        {
                            var fmInstalledPath = GetFMInstallsBasePath(fm);

                            fms.Add(Path.Combine(fmInstalledPath, fm.InstalledDir));
                        }
                        else
                        {
                            continue;
                        }

                        if (ScanCts.IsCancellationRequested)
                        {
                            ScanCts?.Dispose();
                            ProgressBox.Hide();
                            return false;
                        }
                    }

                    List<ScannedFMData> fmDataList;
                    try
                    {
                        var progress = new Progress<ProgressReport>(ReportProgress);

                        using (var scanner = new Scanner())
                        {
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
                        ProgressBox.Hide();
                    }

                    foreach (var fm in fmDataList)
                    {
                        if (fm == null) continue;

                        var sel = fmsToScan.FirstOrDefault(x =>
                            x.Archive.RemoveExtension().EqualsI(fm.ArchiveName.RemoveExtension()) ||
                            x.InstalledDir.EqualsI(fm.ArchiveName.RemoveExtension()));

                        if (sel == null) continue;

                        bool fmIsArchive = fm.ArchiveName.ExtEqualsI(".zip") || fm.ArchiveName.ExtEqualsI(".7z");

                        if (fmIsArchive) sel.RefreshCache = true;

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
                            sel.LanguagesString = gameSup ? fm.Languages != null ? string.Join(", ", fm.Languages) : "" : "";
                        }

                        if (overwriteUnscannedFields || scanOptions.ScanTags)
                        {
                            sel.TagsString = gameSup ? fm.TagsString : "";

                            // Don't clear the tags, because the user could have added a bunch and we should only
                            // add to those, not overwrite them
                            if (gameSup) AddTagsToFMAndGlobalList(sel.TagsString, sel.Tags);
                        }
                    }

                    ProgressBox.Hide();

                    WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
                }
                finally
                {
                    View.Unblock();
                }
            }

            return true;
        }

        internal async Task<bool> ScanAllFMs(ScanOptions scanOptions, bool overwriteUnscannedFields = true)
        {
            return await ScanFMs(FMsViewList, scanOptions, overwriteUnscannedFields);
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
            // TODO: This can be canceled, so make sure the world won't explode if the user cancels
            // and leaves some FMs in an un-game-type-scanned state.
            var fmsToScan = new List<FanMission>();
            foreach (var fm in FMsViewList)
            {
                if (fm.Game == null) fmsToScan.Add(fm);
            }
            if (fmsToScan.Count > 0)
            {
                var scanOptions = new ScanOptions
                {
                    ScanTitle = false,
                    ScanCampaignMissionNames = false,
                    ScanAuthor = false,
                    ScanVersion = false,
                    ScanLanguages = false,
                    ScanGameType = true,
                    ScanNewDarkRequired = false,
                    ScanNewDarkMinimumVersion = false,
                    ScanCustomResources = false,
                    ScanSize = false,
                    ScanReleaseDate = false,
                    ScanTags = false
                };

                await ScanFMs(fmsToScan, scanOptions, overwriteUnscannedFields: false);
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
                    // log it

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
            finally
            {
                ProgressBox.Hide();
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
                    // log it
                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true));
            }
            finally
            {
                ProgressBox.Hide();
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
                    // log it
                    return false;
                }

                await ScanAndFind(fmsToScan,
                    ScanOptions.FalseDefault(scanGameType: true, scanCustomResources: true, scanSize: true));
            }
            finally
            {
                ProgressBox.Hide();
            }

            return true;
        }

        private async Task ScanAndFind(List<FanMission> fms, ScanOptions scanOptions, bool overwriteUnscannedFields = false)
        {
            if (fms.Count == 0) return;

            // TODO: This can be canceled, so make sure the world won't explode if the user cancels
            await ScanFMs(fms, scanOptions, overwriteUnscannedFields);
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
                await Task.Run(() => Directory.Delete(fmInstalledPath, recursive: true));
                ProgressBox.Hide();
                return false;
            }

            fm.Installed = true;

            WriteFMDataIni(FMDataIniList, Paths.FMDataIni);

            using (var sw = new StreamWriter(Path.Combine(fmInstalledPath, "fmsel.inf"), append: false))
            {
                await sw.WriteLineAsync("Name=" + fm.InstalledDir);
                await sw.WriteLineAsync("Archive=" + fm.Archive);
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
                ProgressBox.Hide();
            }

            await RestoreSavesAndScreenshots(fm);

            // Not doing RefreshSelectedFMRowOnly() because that wouldn't update the install/uninstall buttons
            await View.RefreshSelectedFM(refreshReadme: false);

            ProgressBox.Hide();

            return true;
        }

        private async Task<bool> InstallFMSevenZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
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
                        ProgressBox.BeginInvoke(new Action(() => ProgressBox.ReportFMExtractProgress(e.PercentDone)));
                    };

                    extractor.FileExtractionFinished += (sender, e) =>
                    {
                        SetFileAttributesFromZipEntry(e.FileInfo, Path.Combine(fmInstalledPath, e.FileInfo.FileName));

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
                    catch
                    {
                        // Throws a weird exception even if everything's fine
                        // log it anyway
                    }
                }
            });

            return !canceled;
        }

        private async Task<bool> InstallFMZip(string fmArchivePath, string fmInstalledPath)
        {
            bool canceled = false;

            await Task.Run(() =>
            {
                Directory.CreateDirectory(fmInstalledPath);

                var fs0 = new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read);
                using (var archive = new ZipArchive(fs0, ZipArchiveMode.Read, leaveOpen: false))
                {
                    int filesCount = archive.Entries.Count;
                    for (var i = 0; i < filesCount; i++)
                    {
                        var f = archive.Entries[i];

                        var fileName = f.FullName.Replace('/', '\\');

                        if (fileName[fileName.Length - 1] == '\\') continue;

                        if (fileName.Contains('\\'))
                        {
                            Directory.CreateDirectory(Path.Combine(fmInstalledPath,
                                fileName.Substring(0, fileName.LastIndexOf('\\'))));
                        }

                        f.ExtractToFile(Path.Combine(fmInstalledPath, fileName), overwrite: true);

                        int percent = (100 * (i + 1)) / filesCount;

                        View.BeginInvoke(new Action(() => ProgressBox.ReportFMExtractProgress(percent)));

                        if (ExtractCts.Token.IsCancellationRequested)
                        {
                            canceled = true;
                            return;
                        }
                    }
                }
            });

            return !canceled;
        }

        internal void CancelInstallFM(FanMission fm) => ExtractCts.Cancel();

        internal async Task UninstallFM(FanMission fm)
        {
            if (!fm.Installed) return;

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

                var fmArchivePath = FindFMArchive(fm);

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

                switch (Config.BackupSaves)
                {
                    case BackupSaves.AlwaysAsk:
                        {
                            // TODO: Make this dialog have a "don't ask again" option
                            var cont = View.AskToContinue(
                                LText.AlertMessages.Uninstall_BackupSavesAndScreenshots, "AngelLoader");
                            if (cont) await BackupSavesAndScreenshots(fm);
                            break;
                        }
                    case BackupSaves.AlwaysBackup:
                        await BackupSavesAndScreenshots(fm);
                        break;
                }

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
            finally
            {
                ProgressBox.Hide();
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
                // TODO: This should probably be an option
                View.ShowAlert("This FM is marked as installed, but its folder cannot be found. " +
                               "It will now be marked as uninstalled.", LText.AlertMessages.Alert);
                fm.Installed = false;
                await View.RefreshSelectedFM(refreshReadme: false);
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
                ProgressBox.Hide();
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
                // TODO: This should probably be an option
                View.ShowAlert("This FM is marked as installed, but its folder cannot be found. " +
                               "It will now be marked as uninstalled.", LText.AlertMessages.Alert);
                fm.Installed = false;
                await View.RefreshSelectedFM(refreshReadme: false);
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
                ProgressBox.Hide();
            }
        }

        private static bool GameIsRunning(string gameExe)
        {
            // We're doing this whole rigamarole because the game might have been started by someone other than
            // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (proc.MainModule.FileName.EqualsI(gameExe)) return true;
                }
                catch (Win32Exception)
                {
                    // The process is 64-bit, which means not only is it definitely not one of our games, but we
                    // can't even access its module info anyway. There's a way to check if a process is 64-bit in
                    // advance, but it's fiddly. Easier just to swallow the exception and move on.
                }
                catch (Exception)
                {
                    // Even if this were to be one of our games, if .NET won't let us find out then all we can do
                    // is shrug and move on.
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

            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_GameIsRunning,
                    LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            // We will have verified this on startup and on settings close, and it won't change anywhere else.
            // Also, we know gameExe exists, so we also know its directory is valid.
            var gamePath = Path.GetDirectoryName(gameExe);

            // When the stub finds nothing in the stub comm folder, it will just start the game with no FM
            Paths.PrepareTempPath(Paths.StubCommTemp);

            using (var proc = new Process())
            {
                proc.StartInfo.FileName = gameExe;
                proc.StartInfo.WorkingDirectory = gamePath;
                proc.Start();
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

            var lines = File.ReadAllLines(Paths.GetSneakyOptionsIni(), Encoding.Default).ToList();
            for (var i = 0; i < lines.Count; i++)
            {
                if (!lines[i].Trim().EqualsI("[Loader]")) continue;

                insertLineIndex = i + 1;
                while (i < lines.Count - 1)
                {
                    var lt = lines[i + 1].Trim();
                    if (lt.StartsWithI(externSelectorKey))
                    {
                        lines[i + 1] = externSelectorKey + Paths.StubFileName;
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
                lines.Insert(insertLineIndex, externSelectorKey + Paths.StubFileName);
            }

            File.WriteAllLines(Paths.GetSneakyOptionsIni(), lines, Encoding.Default);

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

            if (GameIsRunning(gameExe))
            {
                View.ShowAlert(gameName + ":\r\n" + LText.AlertMessages.Play_GameIsRunning,
                    LText.AlertMessages.Alert);
                return false;
            }

            #endregion

            if (fm.Game == Game.Thief3)
            {
                var success = SetT3FMSelectorToAngelLoader();
                if (!success)
                {
                    // log it here
                }
            }

            // We will have verified this on startup and on settings close, and it won't change anywhere else.
            // Also, we know gameExe exists, so we also know its directory is valid.
            var gamePath = Path.GetDirectoryName(gameExe);

            Paths.PrepareTempPath(Paths.StubCommTemp);
            using (var sw = new StreamWriter(Paths.StubCommFilePath, false, Encoding.UTF8))
            {
                sw.WriteLine("SelectedFMName=" + fm.InstalledDir);
                sw.WriteLine("DisabledMods=" + (fm.DisableAllMods ? "*" : fm.DisabledMods));
            }

            using (var proc = new Process())
            {
                proc.StartInfo.FileName = gameExe;
                proc.StartInfo.Arguments = "-fm";
                proc.StartInfo.WorkingDirectory = gamePath;
                proc.Start();
            }

            // Don't clear the temp folder here, because the stub program will need to read from it. It will
            // delete the temp file itself after it's done with it.

            return true;
        }

        #endregion

        private static bool FMIsReallyInstalled(FanMission fm)
        {
            return fm.Installed &&
                   Directory.Exists(Path.Combine(GetFMInstallsBasePath(fm), fm.InstalledDir));
        }

        #region Cacheable FM data

        // TODO: Handle if there do exist files, but not all of them from the zip are there
        internal async Task<CacheData> GetCacheableData(FanMission fm)
        {
            if (fm.Game == Game.Unsupported)
            {
                if (!fm.InstalledDir.IsEmpty())
                {
                    var fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);
                    if (!fmCachePath.TrimEnd('\\').EqualsI(Paths.FMsCache.TrimEnd('\\')) && Directory.Exists(fmCachePath))
                    {
                        foreach (var f in Directory.EnumerateFiles(fmCachePath, "*", SearchOption.TopDirectoryOnly))
                        {
                            File.Delete(f);
                        }

                        foreach (var d in Directory.EnumerateDirectories(fmCachePath, "*", SearchOption.TopDirectoryOnly))
                        {
                            Directory.Delete(d, recursive: true);
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
                    // log it
                    throw new ArgumentNullException(nameof(instBasePath), "FM installs base path is empty");
                }
                else if (!Directory.Exists(instBasePath))
                {
                    // log it
                    throw new ArgumentNullException(nameof(instBasePath), "FM installs base path doesn't exist");
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

        internal void UpdateConfig(
            FormWindowState mainWindowState,
            Size mainWindowSize,
            int mainHorizontalSplitterDistance,
            int topVerticalSplitterDistance,
            List<ColumnData> columns, int sortedColumn, SortOrder sortDirection,
            Filter filter,
            SelectedFM selectedFM,
            GameTabsState gameTabsState,
            Game gameTab,
            TopRightTab topRightTab,
            float readmeZoomFactor)
        {
            Config.MainWindowState = mainWindowState;
            Config.MainWindowSize = new Size { Width = mainWindowSize.Width, Height = mainWindowSize.Height };
            Config.MainHorizontalSplitterDistance = mainHorizontalSplitterDistance;
            Config.TopVerticalSplitterDistance = topVerticalSplitterDistance;

            Config.Columns.Clear();
            Config.Columns.AddRange(columns);

            Config.SortedColumn = (Column)sortedColumn;
            Config.SortDirection = sortDirection;

            filter.DeepCopyTo(Config.Filter);

            Config.TopRightTab = topRightTab;

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

        internal void WriteFullFMDataIni() => WriteFMDataIni(FMDataIniList, Paths.FMDataIni);

        internal void Shutdown()
        {
            WriteConfigIni(Config, Paths.ConfigIni);

            WriteFMDataIni(FMDataIniList, Paths.FMDataIni);

            Application.Exit();
        }
    }
}

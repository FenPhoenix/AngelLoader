﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using SevenZip;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Misc;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace AngelLoader
{
    // @BetterErrors(Backup/restore): We really need to not be silent if there are problems here.
    // We could be in a messed-up state and the user won't know and we don't even try to fix it.

    // Zip quirk: LastWriteTime (and presumably any other metadata) must be set BEFORE opening the entry
    // for writing. Even if you put it after the using block, it throws. So always set this before writing!

    // @DIRSEP: Anything of the form "Substring(somePath.Length).Trim('\\', '/') is fine
    // Because we're trimming from the start of a relative path, so we won't trim any "\\" from "\\netPC" or anything

    internal static class FMBackupAndRestore
    {
        #region Private fields

        private const string _startMisSav = "startmis.sav";

        // Note: Either dirsep is okay because our comparisons are dirsep-agnostic in here

        private const string _darkSavesDir = "saves";
        private const string _darkSavesDirS = _darkSavesDir + "/";

        private const string _t3SavesDir = "SaveGames";
        private const string _t3SavesDirS = _t3SavesDir + "/";

        private const string _ss2CurrentDir = "current";
        private const string _ss2CurrentDirS = _ss2CurrentDir + "/";

        // For multiplayer (currently T2-only)
        private const string _darkNetSavesDir = "netsaves";
        private const string _darkNetSavesDirS = _darkNetSavesDir + "/";

        private const string _screensDir = "screenshots";
        private const string _screensDirS = _screensDir + "/";

        private const string _removeFileEq = "RemoveFile=";
        private const string _removeDirEq = "RemoveDir=";

        // IMPORTANT: @DIRSEP: Always say [/\\] for dirsep chars, to be manually dirsep-agnostic
        private static readonly Regex _ss2SaveDirsInZipRegex = new Regex(@"^save_[0123456789]{1,2}[/\\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex _ss2SaveDirsOnDiskRegex = new Regex(@"[/\\]save_[0123456789]{1,2}[/\\]?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        #endregion

        private sealed class BackupFile
        {
            internal bool Found;
            internal string Name;
            internal bool DarkLoader;

            internal BackupFile()
            {
                Found = false;
                Name = "";
                DarkLoader = false;
            }

            internal void Set(bool found, string name, bool darkLoader)
            {
                Found = found;
                Name = name;
                DarkLoader = darkLoader;
            }
        }

        private sealed class FileNameBoth
        {
            internal readonly List<string> FullPaths;
            internal readonly List<string> FileNamesMinusSavesSuffix;

            internal FileNameBoth(List<string> fullPaths, List<string> fileNamesMinusSavesSuffix)
            {
                FullPaths = fullPaths;
                FileNamesMinusSavesSuffix = fileNamesMinusSavesSuffix;
            }
        }

        #region Public methods

        private static BackupFile
        GetBackupFile(
            FanMission fm,
            bool findDarkLoaderOnly = false)
        {
            static FileNameBoth GetDarkLoaderArchiveFiles()
            {
                // @MEM/@PERF_TODO: Why tf are we doing this get-all-files loop?!
                // Can't we just say "if file exists(archive without ext + "_saves.zip")"?!
                var fullPaths = FastIO.GetFilesTopOnly(Config.DarkLoaderBackupPath, "*.zip");
                var fileNamesMinusSavesSuffix = new List<string>(fullPaths.Count);

                for (int i = 0; i < fullPaths.Count; i++)
                {
                    string fullPath = fullPaths[i];

                    string fileNameOnly = fullPath.GetFileNameFast();

                    int index = fileNameOnly.LastIndexOf("_saves.zip", StringComparison.OrdinalIgnoreCase);

                    string fileNameWithTrimmedSavesSuffix = index > -1 ? fileNameOnly.Substring(0, index).Trim() : "";

                    fileNamesMinusSavesSuffix.Add(fileNameWithTrimmedSavesSuffix);
                }

                return new FileNameBoth(fullPaths, fileNamesMinusSavesSuffix);
            }

            var ret = new BackupFile();

            // TODO: Do I need both or is the use of the non-trimmed version a mistake?
            string fmArchiveNoExt = fm.Archive.RemoveExtension();
            string fmArchiveNoExtTrimmed = fmArchiveNoExt.Trim();

            #region DarkLoader

            if (Directory.Exists(Config.DarkLoaderBackupPath))
            {
                // TODO(DarkLoader backups): Is there a reason I'm getting all files on disk and looping through?
                // Rather than just using File.Exists()?!
                FileNameBoth dlArchives = GetDarkLoaderArchiveFiles();
                for (int i = 0; i < dlArchives.FullPaths.Count; i++)
                {
                    string f = dlArchives.FullPaths[i];

                    string an = dlArchives.FileNamesMinusSavesSuffix[i];
                    if (an.IsEmpty()) continue;

                    // Account for the fact that DarkLoader trims archive names for save backup zips
                    // Note: I guess it doesn't?! The code heavily implies it does. Still, it works either
                    // way, so whatever.
                    if (!an.IsEmpty() && an.PathEqualsI(fmArchiveNoExtTrimmed))
                    {
                        ret.Set(true, f, true);
                        if (findDarkLoaderOnly) return ret;
                        break;
                    }
                }
            }

            #endregion

            if (findDarkLoaderOnly)
            {
                ret.Set(false, "", false);
                return ret;
            }

            #region AngelLoader / FMSel / NewDarkLoader

            if (ret.Name.IsEmpty())
            {
                // This is as much as we can cache unfortunately. Every FM's name will be different each call
                // so we can't cache the combined config path and FM name with backup extension. But at least
                // we can cache just the FM name with backup extension, so it's better than nothing.
                string fmArchivePlusBackupExt = fmArchiveNoExt + Paths.FMBackupSuffix;
                string fmInstalledDirPlusBackupExt = fm.InstalledDir + Paths.FMBackupSuffix;
                var bakFiles = new List<FileInfo>();

                void AddBakFilesFrom(string path)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        string fNoExt = i == 0 ? fmArchivePlusBackupExt : fmInstalledDirPlusBackupExt;
                        string bakFile = Path.Combine(path, fNoExt);
                        if (File.Exists(bakFile)) bakFiles.Add(new FileInfo(bakFile));
                    }
                }

                // Our backup path, separate to avoid creating any more ambiguity
                AddBakFilesFrom(Config.FMsBackupPath);

                // If ArchiveName.bak and InstalledName.bak files both exist, use the newest of the two
                ret.Name = bakFiles.Count == 1
                    ? bakFiles[0].FullName
                    : bakFiles.Count > 1
                        ? bakFiles.OrderByDescending(x => x.LastWriteTime).ToList()[0].FullName
                        : "";

                bakFiles.Clear();

                // Use file from our bak dir if it exists, otherwise use the newest file from all archive dirs
                // (for automatic use of FMSel/NDL saves)
                if (ret.Name.IsEmpty())
                {
                    foreach (string path in FMArchives.GetFMArchivePaths())
                    {
                        AddBakFilesFrom(path);
                    }

                    if (bakFiles.Count == 0)
                    {
                        ret.Set(false, "", false);
                        return ret;
                    }

                    // Use the newest of all files found in all archive dirs
                    ret.Name = bakFiles.OrderByDescending(x => x.LastWriteTime).ToList()[0].FullName;
                }
            }

            #endregion

            ret.Found = true;
            return ret;
        }

        internal static async Task BackupFM(FanMission fm, string fmInstalledPath, string fmArchivePath)
        {
            bool backupSavesAndScreensOnly = fmArchivePath.IsEmpty() ||
                                             (Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                              (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves));

            if (!GameIsKnownAndSupported(fm.Game))
            {
                LogFMInfo(fm, ErrorText.FMGameU, stackTrace: true);
                return;
            }

            await Task.Run(() =>
            {
                if (backupSavesAndScreensOnly && fm.InstalledDir.IsEmpty()) return;

                string thisFMInstallsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
                string savesDir = fm.Game == Game.Thief3 ? _t3SavesDir : _darkSavesDir;
                string savesPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, savesDir);
                string netSavesPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, _darkNetSavesDir);
                // Screenshots directory name is the same for T1/T2/T3/SS2
                string screensPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, _screensDir);
                string ss2CurrentPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, _ss2CurrentDir);

                string bakFile = Path.Combine(Config.FMsBackupPath,
                    (!fm.Archive.IsEmpty() ? fm.Archive.RemoveExtension() : fm.InstalledDir) +
                    Paths.FMBackupSuffix);

                if (backupSavesAndScreensOnly)
                {
                    var savesAndScreensFiles = new List<string>();

                    if (Directory.Exists(savesPath))
                    {
                        savesAndScreensFiles.AddRange(Directory.GetFiles(savesPath, "*", SearchOption.AllDirectories));
                    }
                    if (Directory.Exists(netSavesPath))
                    {
                        savesAndScreensFiles.AddRange(Directory.GetFiles(netSavesPath, "*", SearchOption.AllDirectories));
                    }
                    if (Directory.Exists(screensPath))
                    {
                        savesAndScreensFiles.AddRange(Directory.GetFiles(screensPath, "*", SearchOption.AllDirectories));
                    }
                    if (fm.Game == Game.SS2)
                    {
                        savesAndScreensFiles.AddRange(Directory.GetFiles(ss2CurrentPath, "*", SearchOption.AllDirectories));

                        var ss2SaveDirs = FastIO.GetDirsTopOnly(
                            Path.Combine(thisFMInstallsBasePath, fm.InstalledDir), "save_*");

                        foreach (string dir in ss2SaveDirs)
                        {
                            if (_ss2SaveDirsOnDiskRegex.IsMatch(dir))
                            {
                                savesAndScreensFiles.AddRange(Directory.GetFiles(dir, "*", SearchOption.AllDirectories));
                            }
                        }
                    }

                    if (savesAndScreensFiles.Count == 0) return;

                    using var archive = new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                            ZipArchiveMode.Create, leaveOpen: false);

                    foreach (string f in savesAndScreensFiles)
                    {
                        string fn = f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS);
                        AddEntry(archive, f, fn);
                    }

                    MoveDarkLoaderBackup(fm);
                    return;
                }

                var installedFMFiles = Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories).ToHashSetPathI();

                var (changedList, addedList, fullList) =
                    GetFMDiff(fm, installedFMFiles, fmInstalledPath, fmArchivePath);

                // If >90% of files are different, re-run and use only size difference
                // They could have been extracted with NDL which uses SevenZipSharp and that one puts different
                // timestamps, when it puts the right ones at all
                if (changedList.Count > 0 && ((double)changedList.Count / fullList.Count) > 0.9)
                {
                    (changedList, addedList, fullList) =
                        GetFMDiff(fm, installedFMFiles, fmInstalledPath, fmArchivePath, useOnlySize: true);
                }

                try
                {
                    using (var archive = new ZipArchive(
                               new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                               ZipArchiveMode.Create,
                               leaveOpen: false))
                    {
                        foreach (string f in installedFMFiles)
                        {
                            string fn = f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS);
                            if (IsSaveOrScreenshot(fn, fm.Game) ||
                                (!fn.EqualsI(Paths.FMSelInf) && !fn.EqualsI(_startMisSav) &&
                                 (changedList.Contains(fn) || addedList.Contains(fn))))
                            {
                                AddEntry(archive, f, fn);
                            }
                        }

                        string fmSelInfString = "";
                        foreach (string f in fullList)
                        {
                            if (!installedFMFiles.Contains(Path.Combine(fmInstalledPath, f)))
                            {
                                // @DIRSEP: Test if FMSel is dirsep-agnostic here. If so, remove the ToSystemDirSeps()
                                fmSelInfString += _removeFileEq + f.ToSystemDirSeps() + "\r\n";
                            }
                        }

                        if (!fmSelInfString.IsEmpty())
                        {
                            var entry = archive.CreateEntry(Paths.FMSelInf, CompressionLevel.Fastest);
                            using var eo = entry.Open();
                            using var sw = new StreamWriter(eo, Encoding.UTF8);
                            sw.Write(fmSelInfString);
                        }
                    }

                    MoveDarkLoaderBackup(fm);
                }
                catch (Exception ex)
                {
                    LogFMInfo(fm, ErrorText.Ex + "in zip archive create and/or write", ex);
                }
            });
        }

        internal static async Task RestoreFM(FanMission fm, CancellationToken? ct = null)
        {
            static bool Canceled(CancellationToken? ct) => ct != null && ((CancellationToken)ct).IsCancellationRequested;

            if (!GameIsKnownAndSupported(fm.Game))
            {
                LogFMInfo(fm, ErrorText.FMGameU, stackTrace: true);
                return;
            }

            bool restoreSavesAndScreensOnly = Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                             (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves);
            bool fmIsT3 = fm.Game == Game.Thief3;

            await Task.Run(() =>
            {
                BackupFile backupFile = GetBackupFile(fm);
                if (!backupFile.Found) return;

                if (Canceled(ct)) return;

                var fileExcludes = new HashSetPathI();
                //var dirExcludes = new HashSetIP();

                string thisFMInstallsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
                string fmInstalledPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);

                using (var archive = GetZipArchiveCharEnc(backupFile.Name))
                {
                    if (Canceled(ct)) return;

                    var entries = archive.Entries;

                    int entriesCount = entries.Count;

                    if (Canceled(ct)) return;

                    if (backupFile.DarkLoader)
                    {
                        for (int i = 0; i < entriesCount; i++)
                        {
                            ZipArchiveEntry entry = entries[i];
                            string fn = entry.FullName;
                            if (!fn.Rel_ContainsDirSep())
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath, _darkSavesDir));
                                entry.ExtractToFile(Path.Combine(fmInstalledPath, _darkSavesDir, fn), overwrite: true);
                            }
                            else if (fm.Game == Game.SS2 && (_ss2SaveDirsInZipRegex.IsMatch(fn) || fn.PathStartsWithI(_ss2CurrentDirS)))
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.Rel_LastIndexOfDirSep())));
                                entry.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                            }

                            if (Canceled(ct)) return;
                        }
                    }
                    else
                    {
                        string savesDirS = fmIsT3 ? _t3SavesDirS : _darkSavesDirS;
                        if (restoreSavesAndScreensOnly)
                        {
                            for (int i = 0; i < entriesCount; i++)
                            {
                                ZipArchiveEntry entry = entries[i];

                                if (Canceled(ct)) return;

                                string fn = entry.FullName;

                                if (fn.Length > 0 && !fn[fn.Length - 1].IsDirSep() &&
                                    (fn.PathStartsWithI(savesDirS) ||
                                     fn.PathStartsWithI(_darkNetSavesDirS) ||
                                     fn.PathStartsWithI(_screensDirS) ||
                                     (fm.Game == Game.SS2 &&
                                     (_ss2SaveDirsInZipRegex.IsMatch(fn) || fn.PathStartsWithI(_ss2CurrentDirS)))))
                                {
                                    Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.Rel_LastIndexOfDirSep())));
                                    entry.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                                }

                                if (Canceled(ct)) return;
                            }
                        }
                        else
                        {
                            var fmSelInf = archive.GetEntry(Paths.FMSelInf);

                            if (Canceled(ct)) return;

                            // Cap the length, cause... well, nobody's going to put a 500MB binary file named
                            // fmsel.inf, but hey...
                            // Null check required because GetEntry() can return null
                            if (fmSelInf?.Length < ByteSize.MB * 10)
                            {
                                using var eo = fmSelInf.Open();

                                if (Canceled(ct)) return;

                                using var sr = new StreamReader(eo);

                                if (Canceled(ct)) return;

                                while (sr.ReadLine() is { } line)
                                {
                                    bool startsWithRemoveFile = line.StartsWithFast_NoNullChecks(_removeFileEq);
                                    bool startsWithRemoveDir = false;
                                    if (!startsWithRemoveFile)
                                    {
                                        startsWithRemoveDir = line.StartsWithFast_NoNullChecks(_removeDirEq);
                                    }

                                    if (!startsWithRemoveFile && !startsWithRemoveDir) continue;

                                    string val = line.Substring(startsWithRemoveFile ? 11 : 10).Trim();
                                    if (!val.PathStartsWithI(savesDirS) &&
                                        !val.PathStartsWithI(_darkNetSavesDirS) &&
                                        !val.PathStartsWithI(_screensDirS) &&
                                        (fm.Game != Game.SS2 ||
                                        (!_ss2SaveDirsInZipRegex.IsMatch(val) && !val.PathStartsWithI(_ss2CurrentDirS))) &&
                                        !val.EqualsI(Paths.FMSelInf) &&
                                        !val.EqualsI(_startMisSav) &&
                                        // Reject malformed and/or maliciously formed paths - we're going to
                                        // delete these files, and we don't want to delete anything outside
                                        // the FM folder
                                        // @DIRSEP: Relative, no UNC paths can occur here (and if they do we want to reject them anyway)
                                        !val.StartsWithDirSep() &&
                                        !val.Contains(':') &&
                                        // @DIRSEP: Critical: Check both / and \ here because we have no dirsep-agnostic string.Contains()
                                        !val.Contains("./") &&
                                        !val.Contains(".\\"))
                                    {
                                        if (startsWithRemoveFile)
                                        {
                                            fileExcludes.Add(val);
                                        }
                                        //else
                                        //{
                                        //    dirExcludes.Add(val);
                                        //}
                                    }

                                    if (Canceled(ct)) return;
                                }
                            }

                            for (int i = 0; i < entriesCount; i++)
                            {
                                ZipArchiveEntry entry = entries[i];
                                string efn = entry.FullName.ToBackSlashes();

                                if (IsIgnoredFile(efn) ||
                                    efn.EndsWithDirSep() ||
                                    fileExcludes.Contains(efn))
                                {
                                    continue;
                                }

                                if (efn.Rel_ContainsDirSep())
                                {
                                    Directory.CreateDirectory(Path.Combine(fmInstalledPath, efn.Substring(0, efn.Rel_LastIndexOfDirSep())));
                                }

                                entry.ExtractToFile(Path.Combine(fmInstalledPath, efn), overwrite: true);

                                if (Canceled(ct)) return;
                            }
                        }
                    }
                }

                if (!restoreSavesAndScreensOnly)
                {
                    foreach (string f in Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        if (fileExcludes.Contains(f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS)))
                        {
                            // TODO: Deleted dirs are not detected, they're detected as "delete every file in this dir"
                            // If we have crf files replacing dirs, the empty dir will override the crf. We want
                            // to store whether dirs were actually removed so we can remove them again.
                            File.Delete(f);
                        }

                        if (Canceled(ct)) return;
                    }

                    // Disabled till this is working completely
#if false
                    // Crappy hack method
                    var crfs = Directory.GetFiles(fmInstalledPath, "*.crf", SearchOption.TopDirectoryOnly);
                    var dirRemoveList = new List<string>();
                    foreach (string d in Directory.GetDirectories(fmInstalledPath, "*", SearchOption.TopDirectoryOnly))
                    {
                        string dt = d.GetDirNameFast();
                        if (Directory.GetFiles(d, "*", SearchOption.AllDirectories).Length == 0)
                        {
                            // @BigO(FMBackupAndRestore disabled dir excludes code)
                            for (int i = 0; i < crfs.Length; i++)
                            {
                                string ft = crfs[i].GetFileNameFast().RemoveExtension();
                                if (ft.PathEqualsI(dt))
                                {
                                    dirRemoveList.Add(d);
                                }
                            }
                        }
                    }

                    if (dirRemoveList.Count > 0)
                    {
                        for (int i = 0; i < dirRemoveList.Count; i++)
                        {
                            Directory.Delete(dirRemoveList[i], recursive: true);
                        }
                    }

                    // Proper method
                    foreach (string d in Directory.GetDirectories(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        if (dirExcludes.Contains(d.Substring(fmInstalledPath.Length).Trim(CA_BS_FS)))
                        {
                            Directory.Delete(d, recursive: true);
                        }
                    }
#endif
                }
            });
        }

        #endregion

        #region Private methods

        /*
        Do this after backup, NOT after restore! Otherwise, we could end up with the following scenario:
        -User installs FM, we restore DarkLoader backup, we move DarkLoader backup to Original folder
        -User uninstalls FM and chooses "don't back up"
        -Next time user goes to install, we DON'T find the DarkLoader backup (because we moved it) and we also
        don't find any new-style backup (because we didn't create one). Therefore we don't restore the backup,
        which is not at all what the user expects given we tell them that existing backups haven't been changed.
        */
        private static void MoveDarkLoaderBackup(FanMission fm)
        {
            var dlBackup = GetBackupFile(fm, findDarkLoaderOnly: true);
            if (dlBackup.Found)
            {
                Directory.CreateDirectory(Config.DarkLoaderOriginalBackupPath);
                File.Move(dlBackup.Name, Path.Combine(Config.DarkLoaderOriginalBackupPath, dlBackup.Name.GetFileNameFast()));
            }
        }

        private static void AddEntry(ZipArchive archive, string fileNameOnDisk, string entryFileName)
        {
            // @DIRSEP: Converting to '/' because it will be a zip archive name and '/' is to spec
            var entry = archive.CreateEntry(entryFileName.ToForwardSlashes(), CompressionLevel.Fastest);
            entry.LastWriteTime = new FileInfo(fileNameOnDisk).LastWriteTime;
            using var fs = File.OpenRead(fileNameOnDisk);
            using var eo = entry.Open();
            fs.CopyTo(eo);
        }

        private static bool IsSaveOrScreenshot(string path, Game game) =>
            path.PathStartsWithI(_screensDirS) ||
            (game == Game.Thief3 &&
             path.PathStartsWithI(_t3SavesDirS)) ||
            (game == Game.SS2 &&
             (_ss2SaveDirsInZipRegex.IsMatch(path) || path.PathStartsWithI(_ss2CurrentDirS))) ||
            (game != Game.Thief3 &&
             (path.PathStartsWithI(_darkSavesDirS) || path.PathStartsWithI(_darkNetSavesDirS)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsIgnoredFile(string fn) => fn.EqualsI(Paths.FMSelInf) || fn.EqualsI(_startMisSav);

        private static (HashSetPathI ChangedList, HashSetPathI, HashSetPathI FullList)
        GetFMDiff(FanMission fm, HashSetPathI installedFMFiles, string fmInstalledPath, string fmArchivePath, bool useOnlySize = false)
        {
            var changedList = new HashSetPathI();
            var addedList = new HashSetPathI();
            var fullList = new HashSetPathI();

            bool fmIsZip = fmArchivePath.ExtIsZip();
            if (fmIsZip)
            {
                using var archive = GetZipArchiveCharEnc(fmArchivePath);

                var entries = archive.Entries;

                int entriesCount = entries.Count;

                var entriesFullNamesHash = new HashSetPathI(entriesCount);

                for (int i = 0; i < entriesCount; i++)
                {
                    ZipArchiveEntry entry = entries[i];
                    string efn = entry.FullName.ToBackSlashes();

                    entriesFullNamesHash.Add(efn);

                    if (IsIgnoredFile(efn) ||
                        efn.EndsWithDirSep() ||
                        IsSaveOrScreenshot(efn, fm.Game))
                    {
                        continue;
                    }

                    fullList.Add(efn);

                    string fileInInstalledDir = Path.Combine(fmInstalledPath, efn);
                    if (installedFMFiles.Contains(fileInInstalledDir))
                    {
                        try
                        {
                            var fi = new FileInfo(fileInInstalledDir);

                            if (useOnlySize)
                            {
                                if (fi.Length != entry.Length)
                                {
                                    changedList.Add(efn);
                                }
                                continue;
                            }

                            DateTime fiDT = fi.LastWriteTime.ToUniversalTime();
                            DateTime eDT = entry.LastWriteTime.ToUniversalTime().DateTime;
                            // Zip format timestamps have a resolution of 2 seconds, so consider anything +/- 2s as the same
                            if ((fiDT == eDT ||
                                 (DateTime.Compare(fiDT, eDT) < 0 && (eDT - fiDT).TotalSeconds < 3) ||
                                 (DateTime.Compare(fiDT, eDT) > 0 && (fiDT - eDT).TotalSeconds < 3)) &&
                                fi.Length == entry.Length)
                            {
                                continue;
                            }

                            changedList.Add(efn);
                        }
                        catch (Exception ex)
                        {
                            LogFMInfo(fm, ErrorText.Ex + "in last write time compare (zip)", ex);
                        }
                    }
                }

                foreach (string f in installedFMFiles)
                {
                    string fn = f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS);

                    if (IsIgnoredFile(fn) ||
                        IsSaveOrScreenshot(fn, fm.Game))
                    {
                        continue;
                    }

                    if (!entriesFullNamesHash.Contains(fn))
                    {
                        addedList.Add(fn);
                    }
                }
            }
            else
            {
                using var archive = new SevenZipExtractor(fmArchivePath);

                int entriesCount = archive.ArchiveFileData.Count;

                var entriesFullNamesHash = new HashSetPathI(entriesCount);

                for (int i = 0; i < entriesCount; i++)
                {
                    var entry = archive.ArchiveFileData[i];
                    string efn = entry.FileName.ToBackSlashes();

                    entriesFullNamesHash.Add(efn);

                    if (IsIgnoredFile(efn) ||
                        // IsDirectory has been unreliable in the past, so check manually here too
                        entry.IsDirectory || efn.EndsWithDirSep() ||
                        IsSaveOrScreenshot(efn, fm.Game))
                    {
                        continue;
                    }

                    fullList.Add(efn);

                    string fileInInstalledDir = Path.Combine(fmInstalledPath, efn);
                    if (installedFMFiles.Contains(fileInInstalledDir))
                    {
                        try
                        {
                            var fi = new FileInfo(fileInInstalledDir);

                            if (useOnlySize)
                            {
                                if ((ulong)fi.Length != entry.Size)
                                {
                                    changedList.Add(efn);
                                }
                                continue;
                            }

                            if (fi.LastWriteTime.ToUniversalTime() != entry.LastWriteTime.ToUniversalTime() ||
                                (ulong)fi.Length != entry.Size)
                            {
                                changedList.Add(efn);
                            }
                        }
                        catch (Exception ex)
                        {
                            LogFMInfo(fm, ErrorText.Ex + "in last write time compare (7z)", ex);
                        }
                    }
                }

                foreach (string f in installedFMFiles)
                {
                    string fnTemp = Path.GetFileName(f);
                    if (IsIgnoredFile(fnTemp))
                    {
                        continue;
                    }

                    string fn = f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS);

                    if (!entriesFullNamesHash.Contains(fn))
                    {
                        addedList.Add(fn);
                    }
                }
            }

            return (changedList, addedList, fullList);
        }

        #endregion
    }
}

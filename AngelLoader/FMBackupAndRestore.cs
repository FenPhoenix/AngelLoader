using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using SevenZip;
using static AL_Common.CommonUtils;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace AngelLoader
{
    // NOTE: Zip quirk: LastWriteTime (and presumably any other metadata) must be set BEFORE opening the entry
    //       for writing. Even if you put it after the using block, it throws. So always set this before writing!

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

        // IMPORTANT: Always say [/\\] for dirsep chars, to be manually dirsep-agnostic
        private static readonly Regex _ss2SaveDirsInZipRegex = new Regex(@"^save_[0123456789]{1,2}[/\\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex _ss2SaveDirsOnDiskRegex = new Regex(@"[/\\]save_[0123456789]{1,2}[/\\]?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        #endregion

        internal static async Task BackupFM(FanMission fm, string fmInstalledPath, string fmArchivePath)
        {
            bool backupSavesAndScreensOnly = fmArchivePath.IsEmpty() ||
                                             (Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                              (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves));

            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game type is unknown or unsupported (" + fm.Archive + ", " + fm.InstalledDir + ", " + fm.Game + ")", stackTrace: true);
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

                    return;
                }

                string[] installedFMFiles = Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories);

                var (changedList, addedList, fullList) =
                    GetFMDiff(installedFMFiles, fmInstalledPath, fmArchivePath, fm.Game);

                // If >90% of files are different, re-run and use only size difference
                // They could have been extracted with NDL which uses SevenZipSharp and that one puts different
                // timestamps, when it puts the right ones at all
                if (changedList.Count > 0 && ((double)changedList.Count / fullList.Count) > 0.9)
                {
                    (changedList, addedList, fullList) =
                        GetFMDiff(installedFMFiles, fmInstalledPath, fmArchivePath, fm.Game, useOnlySize: true);
                }

                try
                {
                    using var archive = new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                        ZipArchiveMode.Create, leaveOpen: false);

                    foreach (string f in installedFMFiles)
                    {
                        string fn = f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS);
                        if (IsSaveOrScreenshot(fn, fm.Game) ||
                            (!fn.PathEqualsI(Paths.FMSelInf) && !fn.PathEqualsI(_startMisSav) &&
                            (changedList.PathContainsI(fn) || addedList.PathContainsI(fn))))
                        {
                            AddEntry(archive, f, fn);
                        }
                    }

                    string fmSelInfString = "";
                    for (int i = 0; i < fullList.Count; i++)
                    {
                        string f = fullList[i];
                        if (!installedFMFiles.PathContainsI(Path.Combine(fmInstalledPath, f)))
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
                catch (Exception ex)
                {
                    Log("Exception in zip archive create and/or write (" + fm.Archive + ", " + fm.InstalledDir + ", " + fm.Game + ")", ex);
                }
            });
        }

        internal static async Task RestoreFM(FanMission fm)
        {
            if (!GameIsKnownAndSupported(fm.Game))
            {
                Log("Game type is unknown or unsupported (" + fm.Archive + ", " + fm.InstalledDir + ", " + fm.Game + ")", stackTrace: true);
                return;
            }

            bool restoreSavesAndScreensOnly = Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                             (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves);
            bool fmIsT3 = fm.Game == Game.Thief3;

            await Task.Run(() =>
            {
                (string Name, bool DarkLoader) fileToUse = ("", false);

                #region DarkLoader

                string dlBakDir = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveBakDir);

                if (Directory.Exists(dlBakDir))
                {
                    foreach (string f in FastIO.GetFilesTopOnly(dlBakDir, "*.zip"))
                    {
                        string fn = f.GetFileNameFast();
                        int index = fn.LastIndexOf("_saves.zip", StringComparison.OrdinalIgnoreCase);
                        if (index == -1) continue;

                        string an = fn.Substring(0, index).Trim();
                        // Account for the fact that DarkLoader trims archive names for save backup zips
                        // Note: I guess it doesn't?! The code heavily implies it does. Still, it works either
                        // way, so whatever.
                        if (!an.IsEmpty() && an.PathEqualsI(fm.Archive.RemoveExtension().Trim()))
                        {
                            fileToUse = (f, true);
                            break;
                        }
                    }
                }

                #endregion

                #region AngelLoader / FMSel / NewDarkLoader

                if (fileToUse.Name.IsEmpty())
                {
                    var bakFiles = new List<FileInfo>();

                    void AddBakFilesFrom(string path)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            string fNoExt = i == 0 ? fm.Archive.RemoveExtension() : fm.InstalledDir;
                            string bakFile = Path.Combine(path, fNoExt + Paths.FMBackupSuffix);
                            if (File.Exists(bakFile)) bakFiles.Add(new FileInfo(bakFile));
                        }
                    }

                    // Our backup path, separate to avoid creating any more ambiguity
                    AddBakFilesFrom(Config.FMsBackupPath);

                    // If ArchiveName.bak and InstalledName.bak files both exist, use the newest of the two
                    fileToUse.Name = bakFiles.Count == 1
                        ? bakFiles[0].FullName
                        : bakFiles.Count > 1
                        ? bakFiles.OrderByDescending(x => x.LastWriteTime).ToList()[0].FullName
                        : "";

                    bakFiles.Clear();

                    // Use file from our bak dir if it exists, otherwise use the newest file from all archive dirs
                    // (for automatic use of FMSel/NDL saves)
                    if (fileToUse.Name.IsEmpty())
                    {
                        foreach (string path in FMArchives.GetFMArchivePaths()) AddBakFilesFrom(path);

                        if (bakFiles.Count == 0) return;

                        // Use the newest of all files found in all archive dirs
                        fileToUse.Name = bakFiles.OrderByDescending(x => x.LastWriteTime).ToList()[0].FullName;
                    }
                }

                #endregion

                var fileExcludes = new List<string>();
                //var dirExcludes = new List<string>();

                string thisFMInstallsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
                string fmInstalledPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);

                using (var archive = GetZipArchiveCharEnc(fileToUse.Name))
                {
                    int filesCount = archive.Entries.Count;
                    if (fileToUse.DarkLoader)
                    {
                        for (int i = 0; i < filesCount; i++)
                        {
                            var entry = archive.Entries[i];
                            string fn = entry.FullName;
                            if (!fn.ContainsDirSep())
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath, _darkSavesDir));
                                entry.ExtractToFile(Path.Combine(fmInstalledPath, _darkSavesDir, fn), overwrite: true);
                            }
                            else if (fm.Game == Game.SS2 && (_ss2SaveDirsInZipRegex.IsMatch(fn) || fn.PathStartsWithI(_ss2CurrentDirS)))
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.LastIndexOfDirSep())));
                                entry.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                            }
                        }
                    }
                    else
                    {
                        string savesDirS = fmIsT3 ? _t3SavesDirS : _darkSavesDirS;
                        if (restoreSavesAndScreensOnly)
                        {
                            for (int i = 0; i < filesCount; i++)
                            {
                                var entry = archive.Entries[i];
                                string fn = entry.FullName;

                                if (fn.Length > 0 && !fn[fn.Length - 1].IsDirSep() &&
                                    (fn.PathStartsWithI(savesDirS) ||
                                     fn.PathStartsWithI(_darkNetSavesDirS) ||
                                     fn.PathStartsWithI(_screensDirS) ||
                                     (fm.Game == Game.SS2 &&
                                     (_ss2SaveDirsInZipRegex.IsMatch(fn) || fn.PathStartsWithI(_ss2CurrentDirS)))))
                                {
                                    Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.LastIndexOfDirSep())));
                                    entry.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                                }
                            }
                        }
                        else
                        {
                            var fmSelInf = archive.GetEntry(Paths.FMSelInf);
                            // Cap the length, cause... well, nobody's going to put a 500MB binary file named
                            // fmsel.inf, but hey...
                            // Null check required because GetEntry() can return null
                            if (fmSelInf?.Length < ByteSize.MB * 10)
                            {
                                using var eo = fmSelInf.Open();
                                using var sr = new StreamReader(eo);

                                string? line;
                                while ((line = sr.ReadLine()) != null)
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
                                        !val.PathEqualsI(Paths.FMSelInf) &&
                                        !val.PathEqualsI(_startMisSav) &&
                                        // Reject malformed and/or maliciously formed paths - we're going to
                                        // delete these files, and we don't want to delete anything outside
                                        // the FM folder
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
                                }
                            }

                            for (int i = 0; i < filesCount; i++)
                            {
                                var f = archive.Entries[i];
                                string fn = f.FullName;

                                if (fn.PathEqualsI(Paths.FMSelInf) ||
                                    fn.PathEqualsI(_startMisSav) ||
                                    (fn.Length > 0 && fn[fn.Length - 1].IsDirSep()) ||
                                    fileExcludes.PathContainsI(fn))
                                {
                                    continue;
                                }

                                if (fn.ContainsDirSep())
                                {
                                    Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.LastIndexOfDirSep())));
                                }

                                f.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                            }
                        }
                    }
                }

                if (!restoreSavesAndScreensOnly)
                {
                    foreach (string f in Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        if (fileExcludes.PathContainsI(f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS)))
                        {
                            // TODO: Deleted dirs are not detected, they're detected as "delete every file in this dir"
                            // If we have crf files replacing dirs, the empty dir will override the crf. We want
                            // to store whether dirs were actually removed so we can remove them again.
                            File.Delete(f);
                        }
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
                        if (dirExcludes.PathContainsI(d.Substring(fmInstalledPath.Length).Trim(CA_BS_FS)))
                        {
                            Directory.Delete(d, recursive: true);
                        }
                    }
#endif
                }
                if (fileToUse.DarkLoader)
                {
                    string dlOrigBakDir = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveOrigBakDir);
                    Directory.CreateDirectory(dlOrigBakDir);
                    File.Move(fileToUse.Name, Path.Combine(dlOrigBakDir, fileToUse.Name.GetFileNameFast()));
                }
            });
        }

        #region Helpers / private

        private static void AddEntry(ZipArchive archive, string fileNameOnDisk, string entryFileName)
        {
            // @DIRSEP: Converting to '/' because it will be a zip archive name and '/' is to spec
            var entry = archive.CreateEntry(entryFileName.ToForwardSlashes(), CompressionLevel.Fastest);
            entry.LastWriteTime = new FileInfo(fileNameOnDisk).LastWriteTime;
            using var fs = new FileStream(fileNameOnDisk, FileMode.Open, FileAccess.Read);
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

        private static (List<string> ChangedList, List<string> AddedList, List<string> FullList)
        GetFMDiff(string[] installedFMFiles, string fmInstalledPath, string fmArchivePath, Game game, bool useOnlySize = false)
        {
            var changedList = new List<string>();
            var addedList = new List<string>();
            var fullList = new List<string>();

            bool fmIsZip = fmArchivePath.ExtIsZip();
            if (fmIsZip)
            {
                using var archive = GetZipArchiveCharEnc(fmArchivePath);

                for (int i = 0; i < archive.Entries.Count; i++)
                {
                    var entry = archive.Entries[i];
                    string efn = entry.FullName;

                    if (efn.PathEqualsI(Paths.FMSelInf) ||
                        efn.PathEqualsI(_startMisSav) ||
                        (efn.Length > 0 && efn[efn.Length - 1].IsDirSep()) ||
                        IsSaveOrScreenshot(efn, game))
                    {
                        continue;
                    }

                    fullList.Add(entry.FullName);

                    string fileInInstalledDir = Path.Combine(fmInstalledPath, entry.FullName);
                    if (installedFMFiles.PathContainsI(fileInInstalledDir))
                    {
                        try
                        {
                            var fi = new FileInfo(fileInInstalledDir);

                            if (useOnlySize)
                            {
                                if (fi.Length != entry.Length)
                                {
                                    changedList.Add(entry.FullName);
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

                            changedList.Add(entry.FullName);
                        }
                        catch (Exception ex)
                        {
                            Log("Exception in last write time compare (zip) (" + fmArchivePath + ", " + fmInstalledPath + ", game: " + game + ")", ex);
                        }
                    }
                }
                foreach (string f in installedFMFiles)
                {
                    string fn = f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS);

                    if (fn.PathEqualsI(Paths.FMSelInf) ||
                        fn.PathEqualsI(_startMisSav) ||
                        IsSaveOrScreenshot(fn, game))
                    {
                        continue;
                    }

                    bool found = false;
                    for (int i = 0; i < archive.Entries.Count; i++)
                    {
                        if (archive.Entries[i].FullName.PathEqualsI(fn))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) addedList.Add(fn);
                }
            }
            else
            {
                using var archive = new SevenZipExtractor(fmArchivePath);

                for (int i = 0; i < archive.ArchiveFileData.Count; i++)
                {
                    var entry = archive.ArchiveFileData[i];
                    string efn = entry.FileName;

                    if (efn.PathEqualsI(Paths.FMSelInf) ||
                        efn.PathEqualsI(_startMisSav) ||
                        // IsDirectory has been unreliable in the past, so check manually here too
                        entry.IsDirectory || (efn.Length > 0 && efn[efn.Length - 1].IsDirSep()) ||
                        IsSaveOrScreenshot(efn, game))
                    {
                        continue;
                    }

                    fullList.Add(efn);

                    string fileInInstalledDir = Path.Combine(fmInstalledPath, efn);
                    if (File.Exists(fileInInstalledDir))
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
                            Log("Exception in last write time compare (7z) (" + fmArchivePath + ", " + fmInstalledPath + ", game: " + game + ")", ex);
                        }
                    }
                }
                foreach (string f in installedFMFiles)
                {
                    string fnTemp = Path.GetFileName(f);
                    if (fnTemp.PathEqualsI(Paths.FMSelInf) || fnTemp.PathEqualsI(_startMisSav))
                    {
                        continue;
                    }

                    string fn = f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS);

                    bool found = false;
                    for (int i = 0; i < archive.ArchiveFileData.Count; i++)
                    {
                        var entry = archive.ArchiveFileData[i];
                        if (!entry.IsDirectory && entry.FileName.PathEqualsI(fn))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) addedList.Add(fn);
                }
            }

            return (changedList, addedList, fullList);
        }

        #endregion
    }
}

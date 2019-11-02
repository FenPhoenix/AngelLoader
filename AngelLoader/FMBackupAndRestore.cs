using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Ini;
using AngelLoader.WinAPI;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.GameSupport;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace AngelLoader
{
    // TODO: Allow import of NDL's .dml fixes
    /* Process:
    -Find dml backups (easy)
    -If FM is installed, put it into the installed folder (always do this)
    -then:
     -if our bak file exists, put it into there, making sure to also remove it from the fmsel.inf remove list if
      it's there
     -else if our bak file doesn't exist:
      -If NDL's bak file exists, create a new bak file in our folder and put everything in NDL's bak file, plus
       our found .dml, into there, making sure to also remove it from the fmsel.inf remove list if it's there
      -else if no bak files exist:
       -Just create a new bak file in our folder, and put the dml in
    */

    // NOTE: Zip quirk: LastWriteTime (and presumably any other metadata) must be set BEFORE opening the entry
    //       for writing. Even if you put it after the using block, it throws. So always set this before writing!

    internal static class FMBackupAndRestore
    {
        private const string T3SavesDir = "SaveGames";
        private const string DarkSavesDir = "saves";
        private const string SS2CurrentDir = "current";
        // For multiplayer (currently T2-only)
        private const string DarkNetSavesDir = "netsaves";
        private const string ScreensDir = "screenshots";
        private const string RemoveFileEq = "RemoveFile=";

        private static readonly Regex SS2SaveDirsInZipRegex = new Regex(@"^save_[0123456789]{1,2}/",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SS2SaveDirsOnDiskRegex = new Regex(@"[/\\]save_[0123456789]{1,2}[/\\]?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string ToSystemDirSep(this string str)
        {
            return str.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

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

                var thisFMInstallsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
                var savesDir = fm.Game == Game.Thief3 ? T3SavesDir : DarkSavesDir;
                var savesPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, savesDir);
                var netSavesPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, DarkNetSavesDir);
                // Screenshots directory name is the same for T1/T2/T3
                var screensPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, ScreensDir);
                var ss2CurrentPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, SS2CurrentDir);

                var bakFile = Path.Combine(Config.FMsBackupPath,
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

                        foreach (var dir in ss2SaveDirs)
                        {
                            if (SS2SaveDirsOnDiskRegex.IsMatch(dir))
                            {
                                savesAndScreensFiles.AddRange(Directory.GetFiles(dir, "*", SearchOption.AllDirectories));
                            }
                        }
                    }

                    if (savesAndScreensFiles.Count == 0) return;

                    using var archive =
                        new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                            ZipArchiveMode.Create);
                    foreach (var f in savesAndScreensFiles)
                    {
                        var fn = f.Substring(fmInstalledPath.Length).Trim(Path.DirectorySeparatorChar);
                        AddEntry(archive, f, fn);
                    }

                    return;
                }

                var installedFMFiles = Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories);

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
                        ZipArchiveMode.Create);

                    foreach (var f in installedFMFiles)
                    {
                        var fn = f.Substring(fmInstalledPath.Length).ToForwardSlashes().Trim('/');
                        if (IsSaveOrScreenshot(fn, fm.Game) ||
                            (!fn.EqualsI(Paths.FMSelInf) && (changedList.ContainsI(fn) || addedList.ContainsI(fn))))
                        {
                            AddEntry(archive, f, fn);
                        }
                    }

                    var fmSelInfString = "";
                    for (var i = 0; i < fullList.Count; i++)
                    {
                        var f = fullList[i];
                        if (!installedFMFiles.ContainsI(
                            Path.Combine(fmInstalledPath, f).Replace('/', Path.DirectorySeparatorChar)))
                        {
                            fmSelInfString += RemoveFileEq + f.ToSystemDirSep() + "\r\n";
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

        private static void AddEntry(ZipArchive archive, string fileNameOnDisk, string entryFileName,
            CompressionLevel compressionLevel = CompressionLevel.Fastest)
        {
            var entry = archive.CreateEntry(entryFileName, compressionLevel);
            entry.LastWriteTime = new FileInfo(fileNameOnDisk).LastWriteTime;
            using var fs = new FileStream(fileNameOnDisk, FileMode.Open, FileAccess.Read);
            using var eo = entry.Open();
            fs.CopyTo(eo);
        }

        private static bool IsSaveOrScreenshot(string path, Game game)
        {
            return path.StartsWithI(ScreensDir + "/") ||
                   (game == Game.Thief3 && path.StartsWithI(T3SavesDir + "/")) ||
                   (game == Game.SS2 &&
                    (SS2SaveDirsInZipRegex.IsMatch(path) || path.StartsWithI(SS2CurrentDir + "/"))) ||
                   (game != Game.Thief3 && (path.StartsWithI(DarkSavesDir + "/") || path.StartsWithI(DarkNetSavesDir + "/")));
        }

        private static (List<string> ChangedList, List<string> AddedList, List<string> FullList)
        GetFMDiff(string[] installedFMFiles, string fmInstalledPath, string fmArchivePath, Game game, bool useOnlySize = false)
        {
            var changedList = new List<string>();
            var addedList = new List<string>();
            var fullList = new List<string>();

            bool fmIsZip = fmArchivePath.ExtIsZip();
            if (fmIsZip)
            {
                using var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read, leaveOpen: false);

                for (var i = 0; i < archive.Entries.Count; i++)
                {
                    var entry = archive.Entries[i];
                    var efn = entry.FullName.ToForwardSlashes();

                    if (efn.EqualsI(Paths.FMSelInf) ||
                        (efn.Length > 0 && efn[efn.Length - 1] == '/') ||
                        IsSaveOrScreenshot(efn, game))
                    {
                        continue;
                    }

                    fullList.Add(entry.FullName);

                    var fileInInstalledDir = Path.Combine(fmInstalledPath, entry.FullName);
                    if (installedFMFiles.ContainsI(fileInInstalledDir.Replace('/', Path.DirectorySeparatorChar)))
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

                            var fiDT = fi.LastWriteTime.ToUniversalTime();
                            var eDT = entry.LastWriteTime.ToUniversalTime().DateTime;
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
                foreach (var f in installedFMFiles)
                {
                    var fn = f.Substring(fmInstalledPath.Length).ToForwardSlashes().Trim('/');

                    if (fn.EqualsI(Paths.FMSelInf) || IsSaveOrScreenshot(fn, game))
                    {
                        continue;
                    }

                    bool found = false;
                    for (var i = 0; i < archive.Entries.Count; i++)
                    {
                        if (archive.Entries[i].FullName.EqualsI(fn))
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

                for (var i = 0; i < archive.ArchiveFileData.Count; i++)
                {
                    var entry = archive.ArchiveFileData[i];
                    var efn = entry.FileName.ToForwardSlashes();

                    if (efn.EqualsI(Paths.FMSelInf) ||
                        // IsDirectory has been unreliable in the past, so check manually here too
                        entry.IsDirectory || (efn.Length > 0 && efn[efn.Length - 1] == '/') ||
                        IsSaveOrScreenshot(efn, game))
                    {
                        continue;
                    }

                    fullList.Add(efn);

                    var fileInInstalledDir = Path.Combine(fmInstalledPath, efn);
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
                foreach (var f in installedFMFiles)
                {
                    if (Path.GetFileName(f).EqualsI(Paths.FMSelInf)) continue;

                    var fn = f.Substring(fmInstalledPath.Length).ToForwardSlashes().Trim('/');

                    bool found = false;
                    for (var i = 0; i < archive.ArchiveFileData.Count; i++)
                    {
                        var entry = archive.ArchiveFileData[i];
                        var efn = entry.FileName.ToForwardSlashes();
                        if (!entry.IsDirectory && efn.EqualsI(fn))
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

        internal static async Task RestoreSavesAndScreenshots(FanMission fm)
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
                    foreach (var f in FastIO.GetFilesTopOnly(dlBakDir, "*.zip"))
                    {
                        var fn = f.GetFileNameFast();
                        int index = fn.LastIndexOf("_saves.zip", StringComparison.OrdinalIgnoreCase);
                        if (index == -1) continue;

                        var an = fn.Substring(0, index).Trim();
                        // Account for the fact that DarkLoader trims archive names for save backup zips
                        // Note: I guess it doesn't?! The code heavily implies it does. Still, it works either
                        // way, so whatever.
                        if (!an.IsEmpty() && an.EqualsI(fm.Archive.RemoveExtension().Trim()))
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
                            var fNoExt = i == 0 ? fm.Archive.RemoveExtension() : fm.InstalledDir;
                            var bakFile = Path.Combine(path, fNoExt + Paths.FMBackupSuffix);
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
                        foreach (var path in GetFMArchivePaths()) AddBakFilesFrom(path);

                        if (bakFiles.Count == 0) return;

                        // Use the newest of all files found in all archive dirs
                        fileToUse.Name = bakFiles.OrderByDescending(x => x.LastWriteTime).ToList()[0].FullName;
                    }
                }

                #endregion

                var excludes = new List<string>();

                var thisFMInstallsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);
                var fmInstalledPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);

                using (var archive = new ZipArchive(new FileStream(fileToUse.Name, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read))
                {
                    var filesCount = archive.Entries.Count;
                    if (fileToUse.DarkLoader)
                    {
                        for (var i = 0; i < filesCount; i++)
                        {
                            var entry = archive.Entries[i];
                            var fn = entry.FullName.ToForwardSlashes();
                            if (!fn.Contains('/'))
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath, DarkSavesDir));
                                entry.ExtractToFile(Path.Combine(fmInstalledPath, DarkSavesDir, fn), overwrite: true);
                            }
                            else if (fm.Game == Game.SS2 && (SS2SaveDirsInZipRegex.IsMatch(fn) || fn.StartsWithI(SS2CurrentDir + "/")))
                            {
                                Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.LastIndexOf('/'))));
                                entry.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                            }
                        }
                    }
                    else
                    {
                        var savesDir = fmIsT3 ? T3SavesDir : DarkSavesDir;
                        if (restoreSavesAndScreensOnly)
                        {
                            for (var i = 0; i < filesCount; i++)
                            {
                                var entry = archive.Entries[i];
                                var fn = entry.FullName.ToForwardSlashes();

                                if (fn.Length > 0 && fn[fn.Length - 1] != '/' &&
                                    (fn.StartsWithI(savesDir + "/") ||
                                     fn.StartsWithI(DarkNetSavesDir + "/") ||
                                     fn.StartsWithI(ScreensDir + "/") ||
                                     (fm.Game == Game.SS2 &&
                                     (SS2SaveDirsInZipRegex.IsMatch(fn) || fn.StartsWithI(SS2CurrentDir + "/")))))
                                {
                                    Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.LastIndexOf('/'))));
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
                            if (fmSelInf != null && fmSelInf.Length < ByteSize.MB * 10)
                            {
                                using var eo = fmSelInf.Open();
                                using var sr = new StreamReader(eo);

                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    if (!line.StartsWithFast_NoNullChecks(RemoveFileEq)) continue;

                                    var val = line.Substring(11).ToForwardSlashes().Trim();
                                    if (!val.StartsWithI(savesDir + "/") &&
                                        !val.StartsWithI(DarkNetSavesDir + "/") &&
                                        !val.StartsWithI(ScreensDir + "/") &&
                                        (fm.Game != Game.SS2 ||
                                        (!SS2SaveDirsInZipRegex.IsMatch(val) && !val.StartsWithI(SS2CurrentDir + "/"))) &&
                                        !val.EqualsI(Paths.FMSelInf) &&
                                        // Reject malformed and/or maliciously formed paths - we're going to
                                        // delete these files, and we don't want to delete anything outside
                                        // the FM folder
                                        !val.StartsWith("/") &&
                                        !val.Contains(':') &&
                                        !val.Contains("./"))
                                    {
                                        excludes.Add(val);
                                    }
                                }
                            }

                            for (var i = 0; i < filesCount; i++)
                            {
                                var f = archive.Entries[i];
                                var fn = f.FullName.ToForwardSlashes();

                                if (fn.EqualsI(Paths.FMSelInf) ||
                                    (fn.Length > 0 && fn[fn.Length - 1] == '/') ||
                                    excludes.Contains(fn))
                                {
                                    continue;
                                }

                                if (fn.Contains('/'))
                                {
                                    Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.LastIndexOf('/'))));
                                }

                                f.ExtractToFile(Path.Combine(fmInstalledPath, fn), overwrite: true);
                            }
                        }
                    }
                }

                if (!restoreSavesAndScreensOnly)
                {
                    foreach (var f in Directory.EnumerateFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        if (excludes.ContainsI(f.Substring(fmInstalledPath.Length).Replace(Path.DirectorySeparatorChar, '/').Trim('/')))
                        {
                            File.Delete(f);
                        }
                    }
                }

                if (fileToUse.DarkLoader)
                {
                    var dlOrigBakDir = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveOrigBakDir);
                    Directory.CreateDirectory(dlOrigBakDir);
                    File.Move(fileToUse.Name, Path.Combine(dlOrigBakDir, fileToUse.Name.GetFileNameFast()));
                }
            });
        }
    }
}

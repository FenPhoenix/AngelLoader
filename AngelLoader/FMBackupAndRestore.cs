using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Utility.Methods;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace AngelLoader
{
    // TODO: Important! FMSel probably puts any other diffed stuff in its backup files too, so account for that.
    // If we want just the screens and saves, just extract those. If we implement a similar thing to FMSel for
    // backing up and restoring diffs, then we can just extract everything again.

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

    internal static class FMBackupAndRestore
    {
        internal static async Task BackupFM(FanMission fm, string fmInstalledPath, string fmArchivePath)
        {
            bool backupSavesAndScreensOnly = Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                         (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves);
            bool backupAll = Config.BackupFMData == BackupFMData.AllChangedFiles;

            if (!GameIsKnownAndSupported(fm))
            {
                // log it
                return;
            }

            await Task.Run(() =>
            {
                if (backupSavesAndScreensOnly && fm.InstalledDir.IsEmpty()) return;

                var thisFMInstallsBasePath = GetFMInstallsBasePath(fm);
                var savesDir = fm.Game == Game.Thief3 ? "SaveGames" : "saves";
                var savesPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, savesDir);
                // Screenshots directory name is the same for T1/T2/T3
                var screensPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, "screenshots");

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
                    if (Directory.Exists(screensPath))
                    {
                        savesAndScreensFiles.AddRange(Directory.GetFiles(screensPath, "*", SearchOption.AllDirectories));
                    }

                    if (savesAndScreensFiles.Count == 0) return;

                    using (var archive =
                        new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                            ZipArchiveMode.Create))
                    {
                        foreach (var f in savesAndScreensFiles)
                        {
                            var fn = f.Substring(fmInstalledPath.Length).Trim(Path.DirectorySeparatorChar);
                            var entry = archive.CreateEntry(fn);
                            using (var sr = new FileStream(f, FileMode.Open, FileAccess.Read))
                            using (var eo = entry.Open())
                            {
                                sr.CopyTo(eo);
                            }
                            entry.LastWriteTime = new FileInfo(f).LastWriteTime;
                        }
                    }

                    return;
                }

                var installedFMFiles = Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories);

                bool fmIsT3 = fm.Game == Game.Thief3;
                var (changedList, addedList, fullList) =
                    GetFMDiff(installedFMFiles, fmInstalledPath, fmArchivePath, fmIsT3);

                Trace.WriteLine("changedList:");
                foreach (var item in changedList) Trace.WriteLine(item);
                Trace.WriteLine("addedList:");
                foreach (var item in addedList) Trace.WriteLine(item);
                //Trace.WriteLine("removedList:");
                //foreach (var item in removedList) Trace.WriteLine(item);

                //Debugger.Break();

                try
                {
                    using (var archive =
                        new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                            ZipArchiveMode.Create))
                    {
                        foreach (var f in installedFMFiles)
                        {
                            var fn = f.Substring(fmInstalledPath.Length).Replace("\\", "/").Trim('/');
                            if (fn.StartsWithI("screenshots/") ||
                                (fmIsT3 && fn.StartsWithI("SaveGames/")) ||
                                (!fmIsT3 && fn.StartsWithI("saves/")))
                            {
                                var entry = archive.CreateEntry(fn, CompressionLevel.Fastest);
                                entry.LastWriteTime = new FileInfo(f).LastWriteTime;
                                using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read))
                                using (var eo = entry.Open())
                                {
                                    fs.CopyTo(eo);
                                }
                            }
                            else
                            {
                                if (fn.EqualsI("fmsel.inf")) continue;
                                if (changedList.ContainsI(fn) || addedList.ContainsI(fn))
                                {
                                    var entry = archive.CreateEntry(fn, CompressionLevel.Fastest);
                                    entry.LastWriteTime = new FileInfo(f).LastWriteTime;
                                    using (var sr = new FileStream(f, FileMode.Open, FileAccess.Read))
                                    using (var eo = entry.Open())
                                    {
                                        sr.CopyTo(eo);
                                    }
                                }
                            }
                        }

                        var fmSelInfString = "";
                        for (var i = 0; i < fullList.Count; i++)
                        {
                            var f = fullList[i].Replace("/", "\\");
                            if (!installedFMFiles.Contains(Path.Combine(fmInstalledPath, f)))
                            {
                                fmSelInfString += "RemoveFile=" + f.Replace("\\", "/") + "\r\n";
                            }
                        }

                        if (!fmSelInfString.IsEmpty())
                        {
                            var entry = archive.CreateEntry("fmsel.inf", CompressionLevel.Fastest);
                            using (var eo = entry.Open())
                            using (var sw = new StreamWriter(eo, Encoding.UTF8))
                            {
                                sw.Write(fmSelInfString);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // log it
                }
            });
        }

        private static (List<string> ChangedList, List<string> AddedList, List<string> FullList)
        GetFMDiff(string[] installedFMFiles, string fmInstalledPath, string fmArchivePath, bool fmIsT3)
        {
            var changedList = new List<string>();
            var addedList = new List<string>();
            //var removedList = new List<string>();
            var fullList = new List<string>();

            bool fmIsZip = fmArchivePath.ExtEqualsI(".zip");
            if (fmIsZip)
            {
                using (var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read, leaveOpen: false))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EqualsI("fmsel.inf") ||
                            entry.FullName[entry.FullName.Length - 1] == '\\' ||
                            entry.FullName[entry.FullName.Length - 1] == '/' ||
                            entry.FullName.StartsWithI("screenshots/") ||
                            entry.FullName.StartsWithI("screenshots\\") ||
                            (fmIsT3 &&
                             (entry.FullName.StartsWithI("SaveGames/") ||
                              entry.FullName.StartsWithI("SaveGames\\"))) ||
                            (entry.FullName.StartsWithI("saves/") ||
                             entry.FullName.StartsWithI("saves\\")))
                        {
                            continue;
                        }

                        fullList.Add(entry.FullName);

                        var fileInInstalledDir = Path.Combine(fmInstalledPath, entry.FullName);
                        //if (!File.Exists(fileInInstalledDir))
                        //{
                        //    removedList.Add(entry.FullName);
                        //}
                        if (File.Exists(fileInInstalledDir))
                        {
                            try
                            {
                                var fi = new FileInfo(fileInInstalledDir);
                                if (fi.LastWriteTime.ToUniversalTime() != entry.LastWriteTime.ToUniversalTime())
                                {
                                    changedList.Add(entry.FullName);
                                }
                            }
                            catch (Exception ex)
                            {
                                // log it
                            }
                        }
                    }
                    foreach (var f in installedFMFiles)
                    {
                        var fn = f.Substring(fmInstalledPath.Length).Trim(Path.DirectorySeparatorChar);

                        if (fn.EqualsI("fmsel.inf") ||
                            fn.StartsWithI("screenshots/") ||
                            fn.StartsWithI("screenshots\\") ||
                            (fmIsT3 &&
                             (fn.StartsWithI("SaveGames/") ||
                              fn.StartsWithI("SaveGames\\"))) ||
                            (fn.StartsWithI("saves/") ||
                             fn.StartsWithI("saves\\")))
                        {
                            continue;
                        }

                        if (archive.Entries.FirstOrDefault(x =>
                                x.FullName.EqualsI(fn.Replace("/", "\\")) ||
                                x.FullName.EqualsI(fn.Replace("\\", "/"))) == null)
                        {
                            addedList.Add(fn);
                        }
                    }
                }
            }
            else
            {
                using (var archive = new SevenZipExtractor(fmArchivePath))
                {
                    foreach (var entry in archive.ArchiveFileData)
                    {
                        if (entry.IsDirectory || entry.FileName.EqualsI("fmsel.inf") ||
                            entry.FileName.StartsWithI("screenshots/") ||
                            entry.FileName.StartsWithI("screenshots\\") ||
                            (fmIsT3 &&
                             (entry.FileName.StartsWithI("SaveGames/") ||
                              entry.FileName.StartsWithI("SaveGames\\"))) ||
                            (entry.FileName.StartsWithI("saves/") ||
                             entry.FileName.StartsWithI("saves\\")))
                        {
                            continue;
                        }

                        fullList.Add(entry.FileName);

                        var fileInInstalledDir = Path.Combine(fmInstalledPath, entry.FileName);
                        //if (!File.Exists(fileInInstalledDir))
                        //{
                        //    removedList.Add(entry.FileName);
                        //}
                        if (File.Exists(fileInInstalledDir))
                        {
                            try
                            {
                                var fi = new FileInfo(fileInInstalledDir);
                                if (fi.LastWriteTime != entry.LastWriteTime)
                                {
                                    changedList.Add(entry.FileName);
                                }
                            }
                            catch (Exception ex)
                            {
                                // log it
                            }
                        }
                    }
                    foreach (var f in installedFMFiles)
                    {
                        if (Path.GetFileName(f).EqualsI("fmsel.inf")) continue;

                        var fn = f.Substring(fmInstalledPath.Length).Trim(Path.DirectorySeparatorChar);
                        if (!archive.ArchiveFileData.Any(x => !x.IsDirectory && x.FileName.EqualsI(fn)))
                        {
                            addedList.Add(fn);
                        }
                    }
                }
            }

            return (changedList, addedList, fullList);
        }

        internal static async Task RestoreSavesAndScreenshots(FanMission fm)
        {
            if (fm.Game == Game.Thief3 && Config.T3UseCentralSaves)
            {
                // log it
                return;
            }

            await Task.Run(() =>
            {
                (string Name, bool DarkLoader) fileToUse = ("", false);

                #region DarkLoader

                string dlBakDir = Path.Combine(Config.FMsBackupPath, Paths.DarkLoaderSaveBakDir);

                if (Directory.Exists(dlBakDir))
                {
                    foreach (var f in Directory.EnumerateFiles(dlBakDir, "*.zip", SearchOption.TopDirectoryOnly))
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

                using (var extractor = new SevenZipExtractor(fileToUse.Name))
                {
                    var thisFMInstallsBasePath = GetFMInstallsBasePath(fm);
                    var fullFMInstalledPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);
                    if (fileToUse.DarkLoader)
                    {
                        var fmSavesPath = Path.Combine(fullFMInstalledPath, "saves");
                        Directory.CreateDirectory(fmSavesPath);
                        extractor.ExtractArchive(fmSavesPath);
                    }
                    else
                    {
                        // TODO: Important! See header todo (FMSel full diffs etc.)
                        extractor.ExtractArchive(fullFMInstalledPath);
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

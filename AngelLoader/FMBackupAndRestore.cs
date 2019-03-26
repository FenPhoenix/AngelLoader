using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Utility.Methods;
using CompressionLevel = SevenZip.CompressionLevel;
using CompressionMode = SevenZip.CompressionMode;

namespace AngelLoader
{
    // TODO: Important! FMSel probably puts any other diffed stuff in its backup files too, so account for that.
    // If we want just the screens and saves, just extract those. If we implement a similar thing to FMSel for
    // backing up and restoring diffs, then we can just extract everything again.
    internal static class FMBackupAndRestore
    {
        internal static async Task BackupFM(FanMission fm, string fmInstalledPath, string fmArchivePath)
        {
            bool backupSavesAndScreens = fm.Game != Game.Thief3 || Config.T3UseCentralSaves;
            bool backupAll = Config.BackupFMData == BackupFMData.AllChangedFiles;

            if (!GameIsKnownAndSupported(fm))
            {
                // log it
                return;
            }

            await Task.Run(() =>
            {
                //if (fm.InstalledDir.IsEmpty()) return;

                var thisFMInstallsBasePath = GetFMInstallsBasePath(fm);
                var savesDir = fm.Game == Game.Thief3 ? "SaveGames" : "saves";
                var savesPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, savesDir);
                // Screenshots directory name is the same for T1/T2/T3
                var screensPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir, "screenshots");

                bool anySavesExist = false;
                bool anyScreensExist = false;
                if (backupSavesAndScreens)
                {
                    anySavesExist =
                       Directory.Exists(savesPath) &&
                       Directory.GetFiles(savesPath, "*", SearchOption.AllDirectories).Length > 0;

                    anyScreensExist =
                    Directory.Exists(screensPath) &&
                    Directory.GetFiles(screensPath, "*", SearchOption.AllDirectories).Length > 0;

                    if (!backupAll && !anyScreensExist) return;
                }

                var bakFile =
                    Path.Combine(Config.FMsBackupPath,
                        (fm.Archive.RemoveExtension() ?? fm.InstalledDir) + Paths.FMBackupSuffix);

                var installedFMFiles = Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories);

                bool fmIsT3 = fm.Game == Game.Thief3;
                var (changedList, addedList, removedList) =
                    GetFMDiff(installedFMFiles, fmInstalledPath, fmArchivePath, fmIsT3);

                bool fmIsZip = fmArchivePath.ExtEqualsI(".zip");

                if (fmIsZip)
                {
                    var fmSelInfTempPath = Path.Combine(Paths.BaseTemp, "fmselInf");

                    try
                    {
                        Paths.PrepareTempPath(fmSelInfTempPath);

                        using (var archive =
                            new ZipArchive(new FileStream(bakFile, FileMode.Create, FileAccess.Write),
                                ZipArchiveMode.Create))
                        {
                            var fmSelInfTemp = Path.Combine(fmSelInfTempPath, "fmsel.inf");
                            using (var fmselInfSW = new StreamWriter(fmSelInfTemp, append: false))
                            {
                                foreach (var f in installedFMFiles)
                                {
                                    var fn = f.Substring(fmInstalledPath.Length)
                                        .Trim(Path.DirectorySeparatorChar);
                                    if (fn.StartsWithI("screenshots/") ||
                                        fn.StartsWithI("screenshots\\") ||
                                        (fmIsT3 &&
                                         (fn.StartsWithI("SaveGames/") ||
                                          fn.StartsWithI("SaveGames\\"))) ||
                                        (fn.StartsWithI("saves/") ||
                                         fn.StartsWithI("saves\\")))
                                    {
                                        var entry = archive.CreateEntry(fn);
                                        using (var sr = new FileStream(f, FileMode.Open, FileAccess.Read))
                                        using (var eo = entry.Open())
                                        {
                                            sr.CopyTo(eo);
                                        }
                                        entry.LastWriteTime = new FileInfo(f).LastWriteTime;
                                    }
                                    else
                                    {
                                        if (fn.EqualsI("fmsel.inf")) continue;
                                        if (changedList.ContainsI(fn) || addedList.ContainsI(fn))
                                        {
                                            var entry = archive.CreateEntry(fn);
                                            using (var sr = new FileStream(f, FileMode.Open, FileAccess.Read))
                                            using (var eo = entry.Open())
                                            {
                                                sr.CopyTo(eo);
                                            }
                                            entry.LastWriteTime = new FileInfo(f).LastWriteTime;
                                        }
                                        else if (removedList.ContainsI(fn))
                                        {
                                            fmselInfSW.WriteLine("RemoveFile=" + fn);
                                        }
                                    }
                                }
                            }
                            if (new FileInfo(fmSelInfTemp).Length > 0)
                            {
                                var entry = archive.CreateEntry("fmsel.inf");
                                using (var sr = new FileStream(fmSelInfTemp, FileMode.Open, FileAccess.Read))
                                using (var eo = entry.Open())
                                {
                                    sr.CopyTo(eo);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // log it
                    }
                    finally
                    {
                        // Guaranteed cleanup
                        Paths.PrepareTempPath(fmSelInfTempPath);
                    }
                }
                else // TODO: SevenZipSharp version
                {
                    try
                    {
                        Paths.PrepareTempPath(Paths.CompressorTemp);

                        var compressor = new SevenZipCompressor(Paths.CompressorTemp)
                        {
                            ArchiveFormat = OutArchiveFormat.Zip,
                            CompressionLevel = CompressionLevel.Normal,
                            PreserveDirectoryRoot = true
                        };

                        if (anySavesExist)
                        {
                            compressor.CompressDirectory(savesPath, bakFile);
                            compressor.CompressionMode = CompressionMode.Append;
                        }

                        if (anyScreensExist)
                        {
                            compressor.CompressDirectory(screensPath, bakFile);
                        }
                    }
                    finally
                    {
                        // Clean up after ourselves, just in case something went wrong and SevenZipCompressor didn't.
                        // We don't want to be like some apps that pile junk in the temp folder and never delete it.
                        // We're a good temp folder citizen.
                        Paths.PrepareTempPath(Paths.CompressorTemp);
                    }
                }
            });
        }

        private static (List<string> ChangedList, List<string> AddedList, List<string> RemovedList)
        GetFMDiff(string[] installedFMFiles, string fmInstalledPath, string fmArchivePath, bool fmIsT3)
        {
            var changedList = new List<string>();
            var addedList = new List<string>();
            var removedList = new List<string>();

            bool fmIsZip = fmArchivePath.ExtEqualsI(".zip");
            if (fmIsZip)
            {
                using (var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read, leaveOpen: false))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EqualsI("fmsel.inf") ||
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

                        var fileInInstalledDir = Path.Combine(fmInstalledPath, entry.FullName);
                        if (!File.Exists(fileInInstalledDir))
                        {
                            removedList.Add(entry.FullName);
                        }
                        else
                        {
                            try
                            {
                                var fi = new FileInfo(fileInInstalledDir);
                                if (fi.LastWriteTime != entry.LastWriteTime)
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
                    foreach (var f in Directory.EnumerateFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
                    {
                        if (Path.GetFileName(f).EqualsI("fmsel.inf")) continue;

                        var fn = f.Substring(fmInstalledPath.Length).Trim(Path.DirectorySeparatorChar);
                        if (archive.Entries.FirstOrDefault(x => x.FullName.EqualsI(fn)) == null)
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

                        var fileInInstalledDir = Path.Combine(fmInstalledPath, entry.FileName);
                        if (!File.Exists(fileInInstalledDir))
                        {
                            removedList.Add(entry.FileName);
                        }
                        else
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
                    foreach (var f in Directory.EnumerateFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
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

            return (changedList, addedList, removedList);
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

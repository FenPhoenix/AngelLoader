using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AL_Common;
using AngelLoader.DataClasses;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using static AL_Common.Common;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Utils;

namespace AngelLoader;

/*
@BetterErrors(Backup/restore): We really need to not be silent if there are problems here.
We could be in a messed-up state and the user won't know and we don't even try to fix it.

Zip quirk: LastWriteTime (and presumably any other metadata) must be set BEFORE opening the entry
for writing. Even if you put it after the using block, it throws. So always set this before writing!

@DIRSEP: Anything of the form "Substring(somePath.Length).Trim('\\', '/') is fine
Because we're trimming from the start of a relative path, so we won't trim any "\\" from "\\netPC" or anything

NOTE(Backup/restore): Note about empty directories
It seems we already explicitly ignore empty directory entries in GetFMDiff(), which means empty dirs are never
backed up - nor deleted empty dirs marked as deleted - in any case. So, we could just leave this alone and we'd
have the same behavior as before. To be completely correct, we should count empty dirs in the diff, but then any
FM that contained empty dirs and was installed with a previous AL version would get those dirs marked as deleted
in its backup, and then they'd always end up deleted on subsequent installs. To prevent this, we should stick to
ignoring empty dirs for now.
*/

internal static partial class FMInstallAndPlay
{
    #region Fields

    private static readonly object _darkLoaderMoveLock = new();

    // fmsel source code says:
    // "not necessary, it's only an internal temp file for thief (it gets created each time a savegame is loaded
    // or a mission is started)"
    // Nobody knows what it does, hooray
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
    private const int _removeFileEqLen = 11;

    // IMPORTANT: @DIRSEP: Always say [/\\] for dirsep chars, to be manually dirsep-agnostic
    private static readonly Regex _ss2SaveDirsInZipRegex = new Regex(@"^save_[0-9]{1,2}[/\\]",
        RegexOptions.Compiled | IgnoreCaseInvariant);

    private static readonly Regex _ss2SaveDirsOnDiskRegex = new Regex(@"[/\\]save_[0-9]{1,2}[/\\]?$",
        RegexOptions.Compiled | IgnoreCaseInvariant);

    #endregion

    #region Classes

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

    #endregion

    private static void BackupFM(
        DarkLoaderBackupContext ctx,
        FMData fmData,
        List<string> archivePaths,
        FixedLengthByteArrayPool fileBufferPool)
    {
        FanMission fm = fmData.FM;

        bool backupSavesAndScreensOnly = fmData.ArchivePath.IsEmpty() ||
                                         (Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                          (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves));

        if (!GameIsKnownAndSupported(fm.Game))
        {
            fm.LogInfo(ErrorText.FMGameU, stackTrace: true);
            return;
        }

        if (backupSavesAndScreensOnly && fm.InstalledDir.IsEmpty()) return;

        string savesDir = fm.Game == Game.Thief3 ? _t3SavesDir : _darkSavesDir;
        string savesPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, savesDir);
        string netSavesPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, _darkNetSavesDir);
        // Screenshots directory name is the same for T1/T2/T3/SS2
        string screensPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, _screensDir);
        string ss2CurrentPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, _ss2CurrentDir);

        /*
        @MT_TASK(BackupFM): Conflict possibility with backup archive name
        If one FM's archive is my_mission.zip and another is my_mission.7z, both will end up the same
        */
        string bakFile = fmData.BakFile;

        using FixedLengthByteArrayRentScope fileStreamBufferScope = new(fileBufferPool);

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

                List<string> ss2SaveDirs = FastIO.GetDirsTopOnly(
                    Path.Combine(fmData.InstBasePath, fm.InstalledDir), "save_*");

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
                string fn = f.Substring(fmData.InstalledPath.Length).Trim(CA_BS_FS);
                AddEntry(archive, f, fn, fileStreamBufferScope.Array);
            }

            MoveDarkLoaderBackup(ctx, fm, archivePaths);
            return;
        }

        HashSetPathI installedFMFiles = Directory.GetFiles(fmData.InstalledPath, "*", SearchOption.AllDirectories).ToHashSetPathI();

        (HashSetPathI changedList, HashSetPathI addedList, HashSetPathI fullList) =
            GetFMDiff(fm, installedFMFiles, fmData.InstalledPath, fmData.ArchivePath, fileStreamBufferScope.Array);

        // If >90% of files are different, re-run and use only size difference
        // They could have been extracted with NDL which uses SevenZipSharp and that one puts different
        // timestamps, when it puts the right ones at all
        if (changedList.Count > 0 && ((double)changedList.Count / fullList.Count) > 0.9)
        {
            (changedList, addedList, fullList) =
                GetFMDiff(fm, installedFMFiles, fmData.InstalledPath, fmData.ArchivePath, fileStreamBufferScope.Array, useOnlySize: true);
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
                    string fn = f.Substring(fmData.InstalledPath.Length).Trim(CA_BS_FS);
                    if (IsSaveOrScreenshot(fn, fm.Game) ||
                        (!fn.EqualsI(Paths.FMSelInf) && !fn.EqualsI(_startMisSav) &&
                         (changedList.Contains(fn) || addedList.Contains(fn))))
                    {
                        AddEntry(archive, f, fn, fileStreamBufferScope.Array);
                    }
                }

                string fmSelInfString = "";
                foreach (string f in fullList)
                {
                    if (!installedFMFiles.Contains(Path.Combine(fmData.InstalledPath, f)))
                    {
                        // @DIRSEP: Test if FMSel is dirsep-agnostic here. If so, remove the ToSystemDirSeps()
                        fmSelInfString += _removeFileEq + f.ToSystemDirSeps() + "\r\n";
                    }
                }

                if (!fmSelInfString.IsEmpty())
                {
                    ZipArchiveEntry entry = archive.CreateEntry(Paths.FMSelInf, CompressionLevel.Fastest);
                    using var eo = entry.Open();
                    using var sw = new StreamWriter(eo, Encoding.UTF8);
                    sw.Write(fmSelInfString);
                }
            }

            MoveDarkLoaderBackup(ctx, fm, archivePaths);
        }
        catch (Exception ex)
        {
            fm.LogInfo(ErrorText.Ex + "in zip archive create and/or write", ex);
            throw;
        }

        return;

        #region Local functions

        /*
        Do this after backup, NOT after restore! Otherwise, we could end up with the following scenario:
        -User installs FM, we restore DarkLoader backup, we move DarkLoader backup to Original folder
        -User uninstalls FM and chooses "don't back up"
        -Next time user goes to install, we DON'T find the DarkLoader backup (because we moved it) and we also
        don't find any new-style backup (because we didn't create one). Therefore we don't restore the backup,
        which is not at all what the user expects given we tell them that existing backups haven't been changed.

        @MT_TASK(MoveDarkLoaderBackup): Possible conflict here - can we be trying to move the same DL bak file for multiple FMs?
        Don't know for sure, we need to check if it's possible
        */
        static void MoveDarkLoaderBackup(DarkLoaderBackupContext ctx, FanMission fm, List<string> archivePaths)
        {
            try
            {
                /*
                @MT_TASK: Cheap solution #2 - just lock around the DarkLoader bak file move
                @MT_TASK(DarkLoader move lock): Test this!
                This part of the backup will take minimal time, so forcing it serial shouldn't have much impact,
                especially since it'll only be done if there's a DarkLoader bak file to begin with, and then it
                won't be done again because that DL bak will have been moved.
                But, we should test it in the worst case still (a ton of FMs all with DL bak files to be moved).

                @MT_TASK(DarkLoader move lock): We want GetBackupFile() to be outside the lock ideally.
                Because then we won't take the lock at all if there's no bak file to move. And we know we're
                serial if there were any duplicate FM archives, so that shouldn't be a problem either, except
                that GetBackupFile() does like:

                string fmArchiveNoExt = fm.Archive.RemoveExtension();
                string fmArchiveNoExtTrimmed = fmArchiveNoExt.Trim();

                My head's not in a space to figure out if the trimming of the name introduces the possibility of
                a file being moved out from under us now, but we need to check into that before knowing if it's
                safe to move the GetBackupFile() call above the lock.
                */
                lock (_darkLoaderMoveLock)
                {
                    BackupFile dlBackup = GetBackupFile(ctx, fm, archivePaths, findDarkLoaderOnly: true);
                    if (dlBackup.Found)
                    {
                        Directory.CreateDirectory(ctx.DarkLoaderOriginalBackupPath);
                        string originalDest = Path.Combine(ctx.DarkLoaderOriginalBackupPath, dlBackup.Name.GetFileNameFast());
                        string finalDest = originalDest;
                        int i = 1;
                        while (File.Exists(finalDest) && i < int.MaxValue)
                        {
                            finalDest = originalDest.RemoveExtension() + "(" + i.ToStrInv() + ")" + Path.GetExtension(originalDest);
                            i++;
                        }
                        File.Move(dlBackup.Name, finalDest);
                    }
                }
            }
            catch (Exception ex)
            {
                fm.LogInfo(
                    ErrorText.Ex + "trying to move DarkLoader backup to " + ctx.DarkLoaderOriginalBackupPath,
                    ex);
                throw;
            }
        }

        static void AddEntry(ZipArchive archive, string fileNameOnDisk, string entryFileName, byte[] buffer)
        {
            // @DIRSEP: Converting to '/' because it will be a zip archive name and '/' is to spec
            ZipArchiveEntry entry = archive.CreateEntry(entryFileName.ToForwardSlashes(), CompressionLevel.Fastest);
            entry.LastWriteTime = new FileInfo(fileNameOnDisk).LastWriteTime;
            using var fs = File_OpenReadFast(fileNameOnDisk);
            using var eo = entry.Open();
            StreamCopyNoAlloc(fs, eo, buffer);
        }

        static bool IsSaveOrScreenshot(string path, Game game) =>
            path.PathStartsWithI(_screensDirS) ||
            (game == Game.Thief3 &&
             path.PathStartsWithI(_t3SavesDirS)) ||
            (game == Game.SS2 &&
             (_ss2SaveDirsInZipRegex.IsMatch(path) || path.PathStartsWithI(_ss2CurrentDirS))) ||
            (game != Game.Thief3 &&
             (path.PathStartsWithI(_darkSavesDirS) || path.PathStartsWithI(_darkNetSavesDirS)));

        static (HashSetPathI ChangedList, HashSetPathI, HashSetPathI FullList)
        GetFMDiff(
            FanMission fm,
            HashSetPathI installedFMFiles,
            string fmInstalledPath,
            string fmArchivePath,
            byte[] fileStreamBuffer,
            bool useOnlySize = false)
        {
            var changedList = new HashSetPathI();
            var addedList = new HashSetPathI();
            var fullList = new HashSetPathI();

            if (fmArchivePath.ExtIsZip())
            {
                using ZipArchive archive = GetReadModeZipArchiveCharEnc(fmArchivePath, fileStreamBuffer);

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
                            fm.LogInfo(ErrorText.ExInLWT + "(zip)", ex);
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
            else if (fmArchivePath.ExtIsRar())
            {
                using var fs = File_OpenReadFast(fmArchivePath);
                using var archive = RarArchive.Open(fmArchivePath);

                ICollection<RarArchiveEntry> entries = archive.Entries;
                int entriesCount = entries.Count;

                var entriesFullNamesHash = new HashSetPathI(entriesCount);

                foreach (RarArchiveEntry entry in entries)
                {
                    string efn = entry.Key.ToBackSlashes();

                    entriesFullNamesHash.Add(efn);

                    if (IsIgnoredFile(efn) ||
                        // IsDirectory has been unreliable in the past, so check manually here too
                        entry.IsDirectory ||
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
                                if (fi.Length != entry.Size)
                                {
                                    changedList.Add(efn);
                                }
                                continue;
                            }
                            if (entry.LastModifiedTime != null)
                            {
                                DateTime fiDT = fi.LastWriteTime.ToUniversalTime();
                                DateTime eDT = ((DateTime)entry.LastModifiedTime).ToUniversalTime();
                                // I think RAR can sometimes use the DOS datetime, so to be safe let's do the 2 second tolerance
                                if ((fiDT == eDT ||
                                     (DateTime.Compare(fiDT, eDT) < 0 && (eDT - fiDT).TotalSeconds < 3) ||
                                     (DateTime.Compare(fiDT, eDT) > 0 && (fiDT - eDT).TotalSeconds < 3)) &&
                                    fi.Length == entry.Size)
                                {
                                    continue;
                                }
                                changedList.Add(efn);
                            }
                        }
                        catch (Exception ex)
                        {
                            fm.LogInfo(ErrorText.ExInLWT + "(7z)", ex);
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
            else
            {
                using var fs = File_OpenReadFast(fmArchivePath);
                var archive = new SevenZipArchive(fs);

                ListFast<SevenZipArchiveEntry> entries = archive.Entries;
                int entriesCount = entries.Count;

                var entriesFullNamesHash = new HashSetPathI(entriesCount);

                for (int i = 0; i < entriesCount; i++)
                {
                    SevenZipArchiveEntry entry = entries[i];

                    if (entry.IsAnti) continue;

                    string efn = entry.FileName.ToBackSlashes();

                    entriesFullNamesHash.Add(efn);

                    if (IsIgnoredFile(efn) ||
                        // IsDirectory has been unreliable in the past, so check manually here too
                        entry.IsDirectory ||
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
                                if (fi.Length != entry.UncompressedSize)
                                {
                                    changedList.Add(efn);
                                }
                                continue;
                            }

                            if ((entry.LastModifiedTime != null &&
                                fi.LastWriteTime.ToUniversalTime() != ((DateTime)entry.LastModifiedTime).ToUniversalTime()) ||
                                fi.Length != entry.UncompressedSize)
                            {
                                changedList.Add(efn);
                            }
                        }
                        catch (Exception ex)
                        {
                            fm.LogInfo(ErrorText.ExInLWT + "(7z)", ex);
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

    private static void RestoreFM(
        DarkLoaderBackupContext ctx,
        FanMission fm,
        List<string> archivePaths,
        IOBufferPools ioBufferPools,
        CancellationToken ct)
    {
        if (!fm.Game.ConvertsToKnownAndSupported(out GameIndex gameIndex))
        {
            fm.LogInfo(ErrorText.FMGameU, stackTrace: true);
            return;
        }

        bool restoreSavesAndScreensOnly = Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                          (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves);
        bool fmIsT3 = fm.Game == Game.Thief3;

        BackupFile backupFile = GetBackupFile(ctx, fm, archivePaths);
        if (!backupFile.Found) return;

        if (ct.IsCancellationRequested) return;

        var fileExcludes = new HashSetPathI();

        string thisFMInstallsBasePath = Config.GetFMInstallPath(gameIndex);
        string fmInstalledPath = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);

        byte[] fileStreamReadBuffer = ioBufferPools.FileStream.Rent();
        try
        {
            using ZipArchive archive = GetReadModeZipArchiveCharEnc(backupFile.Name, fileStreamReadBuffer);
            if (ct.IsCancellationRequested) return;

            var entries = archive.Entries;

            int entriesCount = entries.Count;

            if (ct.IsCancellationRequested) return;

            if (backupFile.DarkLoader)
            {
                for (int i = 0; i < entriesCount; i++)
                {
                    ZipArchiveEntry entry = entries[i];
                    string fn = entry.FullName;
                    if (!fn.Rel_ContainsDirSep())
                    {
                        string savesFullPath = Path.Combine(fmInstalledPath, _darkSavesDir);
                        if (!TryGetExtractedNameOrFailIfMalicious(savesFullPath, fn, out string finalFilePath))
                        {
                            continue;
                        }
                        Directory.CreateDirectory(savesFullPath);
                        entry.ExtractToFile_Fast(finalFilePath, overwrite: true, ioBufferPools);
                    }
                    else if (fm.Game == Game.SS2 && (_ss2SaveDirsInZipRegex.IsMatch(fn) || fn.PathStartsWithI(_ss2CurrentDirS)))
                    {
                        if (!TryGetExtractedNameOrFailIfMalicious(fmInstalledPath, fn, out string finalFilePath))
                        {
                            continue;
                        }
                        Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.Rel_LastIndexOfDirSep())));
                        entry.ExtractToFile_Fast(finalFilePath, overwrite: true, ioBufferPools);
                    }

                    if (ct.IsCancellationRequested) return;
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

                        if (ct.IsCancellationRequested) return;

                        string fn = entry.FullName;

                        if (fn.Length > 0 && !fn[^1].IsDirSep() &&
                            (fn.PathStartsWithI(savesDirS) ||
                             fn.PathStartsWithI(_darkNetSavesDirS) ||
                             fn.PathStartsWithI(_screensDirS) ||
                             (fm.Game == Game.SS2 &&
                              (_ss2SaveDirsInZipRegex.IsMatch(fn) || fn.PathStartsWithI(_ss2CurrentDirS)))))
                        {
                            if (!TryGetExtractedNameOrFailIfMalicious(fmInstalledPath, fn, out string finalFileName))
                            {
                                continue;
                            }
                            Directory.CreateDirectory(Path.Combine(fmInstalledPath, fn.Substring(0, fn.Rel_LastIndexOfDirSep())));
                            entry.ExtractToFile_Fast(finalFileName, overwrite: true, ioBufferPools);
                        }

                        if (ct.IsCancellationRequested) return;
                    }
                }
                else
                {
                    ZipArchiveEntry? fmSelInf = archive.GetEntry(Paths.FMSelInf);

                    if (ct.IsCancellationRequested) return;

                    // Null check required because GetEntry() can return null
                    if (fmSelInf != null)
                    {
                        using var eo = fmSelInf.Open();

                        if (ct.IsCancellationRequested) return;

                        using var sr = new StreamReader(eo);

                        if (ct.IsCancellationRequested) return;

                        while (sr.ReadLine() is { } line)
                        {
                            bool startsWithRemoveFile = line.StartsWithFast(_removeFileEq);

                            if (!startsWithRemoveFile) continue;

                            string val = line.Substring(_removeFileEqLen).Trim();
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
                                !val.EndsWithDirSep() &&
                                !val.Contains(':') &&
                                // @DIRSEP: Critical: Check both / and \ here because we have no dirsep-agnostic string.Contains()
                                !val.Contains("./", StringComparison.Ordinal) &&
                                !val.Contains(".\\", StringComparison.Ordinal))
                            {
                                fileExcludes.Add(val);
                            }

                            if (ct.IsCancellationRequested) return;
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

                        if (!TryGetExtractedNameOrFailIfMalicious(fmInstalledPath, efn, out string finalFileName))
                        {
                            continue;
                        }
                        if (efn.Rel_ContainsDirSep())
                        {
                            Directory.CreateDirectory(Path.Combine(fmInstalledPath, efn.Substring(0, efn.Rel_LastIndexOfDirSep())));
                        }
                        entry.ExtractToFile_Fast(finalFileName, overwrite: true, ioBufferPools);

                        if (ct.IsCancellationRequested) return;
                    }
                }
            }
        }
        finally
        {
            ioBufferPools.FileStream.Return(fileStreamReadBuffer);
        }

        if (!restoreSavesAndScreensOnly)
        {
            foreach (string f in Directory.GetFiles(fmInstalledPath, "*", SearchOption.AllDirectories))
            {
                if (fileExcludes.Contains(f.Substring(fmInstalledPath.Length).Trim(CA_BS_FS)))
                {
                    // @ZipSafety: This delete is safe, because we're only deleting files that have come straight from a GetFiles() call.
                    // So we know they're in the actual folder.

                    // TODO: Deleted dirs are not detected, they're detected as "delete every file in this dir"
                    // If we have crf files replacing dirs, the empty dir will override the crf. We want
                    // to store whether dirs were actually removed so we can remove them again.
                    File.Delete(f);
                }

                if (ct.IsCancellationRequested) return;
            }
        }
    }

    #region Helpers

    /*
    @MT_TASK(GetBackupFile() thread safety):
    For Restore, we're only ever reading the archive we get back from this, but the potential problem is that if
    it's the DarkLoader file we get back, that one could get moved out from under us if we're not absolutely sure
    it's unique (ie. no other thread will move it).
    */
    private static BackupFile GetBackupFile(
    DarkLoaderBackupContext ctx,
    FanMission fm,
    List<string> archivePaths,
    bool findDarkLoaderOnly = false)
    {
        var ret = new BackupFile();

        // TODO: Do I need both or is the use of the non-trimmed version a mistake?
        string fmArchiveNoExt = fm.Archive.RemoveExtension();

        #region DarkLoader

        if (Directory.Exists(ctx.DarkLoaderBackupPath))
        {
            // TODO: Do I need both or is the use of the non-trimmed version a mistake?
            string fmArchiveNoExtTrimmed = fmArchiveNoExt.Trim();

            // TODO(DarkLoader backups): Is there a reason I'm getting all files on disk and looping through?
            // Rather than just using File.Exists()?!
            FileNameBoth dlArchives = GetDarkLoaderArchiveFiles(ctx);
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
                    ? bakFiles.OrderByDescending(static x => x.LastWriteTime).ToList()[0].FullName
                    : "";

            bakFiles.Clear();

            // Use file from our bak dir if it exists, otherwise use the newest file from all archive dirs
            // (for automatic use of FMSel/NDL saves)
            if (ret.Name.IsEmpty())
            {
                foreach (string path in archivePaths)
                {
                    AddBakFilesFrom(path);
                }

                if (bakFiles.Count == 0)
                {
                    ret.Set(false, "", false);
                    return ret;
                }

                // Use the newest of all files found in all archive dirs
                ret.Name = bakFiles.OrderByDescending(static x => x.LastWriteTime).ToList()[0].FullName;
            }
        }

        #endregion

        ret.Found = true;
        return ret;

        static FileNameBoth GetDarkLoaderArchiveFiles(DarkLoaderBackupContext ctx)
        {
            // @MEM/@PERF_TODO: Why tf are we doing this get-all-files loop?!
            // Can't we just say "if file exists(archive without ext + "_saves.zip")"?!
            List<string> fullPaths = FastIO.GetFilesTopOnly(ctx.DarkLoaderBackupPath, "*.zip");
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIgnoredFile(string fn) => fn.EqualsI(Paths.FMSelInf) || fn.EqualsI(_startMisSav);

    #endregion
}

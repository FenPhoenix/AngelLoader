﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AL_Common.FastZipReader;
using AngelLoader.DataClasses;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
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
It seems we already explicitly ignore empty directory entries in the get diff function, which means empty dirs
are never backed up - nor deleted empty dirs marked as deleted - in any case. So, we could just leave this alone
and we'd have the same behavior as before. To be completely correct, we should count empty dirs in the diff, but
then any FM that contained empty dirs and was installed with a previous AL version would get those dirs marked as
deleted in its backup, and then they'd always end up deleted on subsequent installs. To prevent this, we should
stick to ignoring empty dirs for now.
*/

internal static partial class FMInstallAndPlay
{
    #region Fields

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
        Regex_IgnoreCaseInvariant);

    private static readonly Regex _ss2SaveDirsOnDiskRegex = new Regex(@"[/\\]save_[0-9]{1,2}[/\\]?$",
        Regex_IgnoreCaseInvariant);

    #endregion

    #region Classes

    private sealed class BackupFile
    {
        internal bool Found;
        internal string Name;
        internal bool DarkLoader;

        internal BackupFile(bool found, string name, bool darkLoader)
        {
            Found = found;
            Name = name;
            DarkLoader = darkLoader;
        }

        internal void Set(bool found, string name, bool darkLoader)
        {
            Found = found;
            Name = name;
            DarkLoader = darkLoader;
        }
    }

    #endregion

    private static void BackupFM(
        DarkLoaderBackupContext ctx,
        FMData fmData,
        byte[] fileStreamBuffer)
    {
        FanMission fm = fmData.FM;

        bool backupSavesAndScreensOnly = fmData.ArchiveFilePath.IsEmpty() ||
                                         (Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                          (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves));

        if (backupSavesAndScreensOnly && fm.InstalledDir.IsEmpty()) return;

        string savesDir = fm.Game == Game.Thief3 ? _t3SavesDir : _darkSavesDir;
        string savesPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, savesDir);
        string netSavesPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, _darkNetSavesDir);
        // Screenshots directory name is the same for T1/T2/T3/SS2
        string screensPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, _screensDir);
        string ss2CurrentPath = Path.Combine(fmData.InstBasePath, fm.InstalledDir, _ss2CurrentDir);

        string bakFile = fmData.BakFile;

        if (backupSavesAndScreensOnly)
        {
            List<string> savesAndScreensFiles = new();

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

            using (ZipArchive archive = new(
                       // @FileStreamNET: Use of FileStream
                       new FileStream(
                           bakFile,
                           FileMode.Create,
                           FileAccess.Write),
                       ZipArchiveMode.Create,
                       leaveOpen: false))
            {
                foreach (string f in savesAndScreensFiles)
                {
                    string fn = f.Substring(fmData.InstalledPath.Length).Trim(CA_BS_FS);
                    AddEntry(archive, f, fn, fileStreamBuffer);
                }
            }

            MoveDarkLoaderBackup(ctx, fmData);
            return;
        }

        HashSetPathI installedFMFiles = Directory.GetFiles(fmData.InstalledPath, "*", SearchOption.AllDirectories).ToHashSetPathI();

        (HashSetPathI changedList, HashSetPathI addedList, HashSetPathI fullList) =
            GetDiffBetweenFMInstalledDirAndFMArchive(
                installedFMFiles,
                fmData,
                fileStreamBuffer);

        // If >90% of files are different, re-run and use only size difference
        // They could have been extracted with NDL which uses SevenZipSharp and that one puts different
        // timestamps, when it puts the right ones at all
        if (changedList.Count > 0 && ((double)changedList.Count / fullList.Count) > 0.9)
        {
            (changedList, addedList, fullList) =
                GetDiffBetweenFMInstalledDirAndFMArchive(
                    installedFMFiles,
                    fmData,
                    fileStreamBuffer,
                    useOnlySize: true);
        }

        try
        {
            List<(string FileName, string FileNameRelative)> filesList = new();
            string fmSelInfString = "";

            foreach (string f in installedFMFiles)
            {
                string fn = f.Substring(fmData.InstalledPath.Length).Trim(CA_BS_FS);
                if (IsSaveOrScreenshot(fn, fm.Game) ||
                    (!fn.EqualsI(Paths.FMSelInf) && !fn.EqualsI(_startMisSav) &&
                     (changedList.Contains(fn) || addedList.Contains(fn))))
                {
                    filesList.Add((f, fn));
                }
            }

            foreach (string f in fullList)
            {
                if (!installedFMFiles.Contains(Path.Combine(fmData.InstalledPath, f)))
                {
                    // @DIRSEP: Test if FMSel is dirsep-agnostic here. If so, remove the ToSystemDirSeps()
                    fmSelInfString += _removeFileEq + f.ToSystemDirSeps() + "\r\n";
                }
            }

            if (filesList.Count == 0 && fmSelInfString.IsEmpty()) return;

            using (ZipArchive archive = new(
                       // @FileStreamNET: Use of FileStream
                       new FileStream(
                           bakFile,
                           FileMode.Create,
                           FileAccess.Write),
                       ZipArchiveMode.Create,
                       leaveOpen: false))
            {
                foreach ((string fileName, string fileNameRelative) in filesList)
                {
                    AddEntry(archive, fileName, fileNameRelative, fileStreamBuffer);
                }

                if (!fmSelInfString.IsEmpty())
                {
                    ZipArchiveEntry entry = archive.CreateEntry(Paths.FMSelInf, CompressionLevel.Fastest);
                    using Stream eo = entry.Open();
                    using StreamWriter sw = new(eo, Encoding.UTF8);
                    sw.Write(fmSelInfString);
                }
            }

            MoveDarkLoaderBackup(ctx, fmData);
        }
        catch (Exception ex)
        {
            fm.LogInfo(ErrorText.Ex + "in zip archive create and/or write", ex);
            throw;
        }
    }

    private static void RestoreFM(
        DarkLoaderBackupContext ctx,
        FMData fmData,
        List<string> archivePaths,
        IOBufferPools ioBufferPools,
        CancellationToken ct)
    {
        FanMission fm = fmData.FM;

        bool restoreSavesAndScreensOnly = Config.BackupFMData == BackupFMData.SavesAndScreensOnly &&
                                          (fm.Game != Game.Thief3 || !Config.T3UseCentralSaves);
        bool fmIsT3 = fm.Game == Game.Thief3;

        BackupFile backupFile = GetBackupFile(ctx, fmData, archivePaths);
        if (!backupFile.Found) return;

        if (ct.IsCancellationRequested) return;

        HashSetPathI fileExcludes = new();

        string thisFMInstallsBasePath = Config.GetFMInstallPath(fmData.GameIndex);
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
                        if (!ZipHelpers.TryGetExtractedNameOrFailIfMalicious(savesFullPath, fn, out string finalFilePath))
                        {
                            continue;
                        }
                        Directory.CreateDirectory(savesFullPath);
                        entry.ExtractToFile_Fast(finalFilePath, overwrite: true, ioBufferPools);
                    }
                    else if (fm.Game == Game.SS2 && (_ss2SaveDirsInZipRegex.IsMatch(fn) || fn.PathStartsWithI(_ss2CurrentDirS)))
                    {
                        if (!ZipHelpers.TryGetExtractedNameOrFailIfMalicious(fmInstalledPath, fn, out string finalFilePath))
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
                            if (!ZipHelpers.TryGetExtractedNameOrFailIfMalicious(fmInstalledPath, fn, out string finalFileName))
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
                        using Stream eo = fmSelInf.Open();

                        if (ct.IsCancellationRequested) return;

                        using StreamReader sr = new(eo);

                        if (ct.IsCancellationRequested) return;

                        while (sr.ReadLine() is { } line)
                        {
                            bool startsWithRemoveFile = line.StartsWithFast(_removeFileEq);

                            if (!startsWithRemoveFile) continue;

                            string val = line.Substring(_removeFileEqLen).Trim();
                            if (// We must not exclude (delete) any ignored files, otherwise if for example audio
                                // was mp3s and was converted to wavs and thus all mp3s were marked as remove,
                                // all audio will be deleted on install and not replaced.
                                !IsIgnoredFile(val) &&
                                !val.PathStartsWithI(savesDirS) &&
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

                        if (!ZipHelpers.TryGetExtractedNameOrFailIfMalicious(fmInstalledPath, efn, out string finalFileName))
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

    /*
    Do this after backup, NOT after restore! Otherwise, we could end up with the following scenario:
    -User installs FM, we restore DarkLoader backup, we move DarkLoader backup to Original folder
    -User uninstalls FM and chooses "don't back up"
    -Next time user goes to install, we DON'T find the DarkLoader backup (because we moved it) and we also
    don't find any new-style backup (because we didn't create one). Therefore we don't restore the backup,
    which is not at all what the user expects given we tell them that existing backups haven't been changed.
    */
    private static void MoveDarkLoaderBackup(DarkLoaderBackupContext ctx, FMData fmData)
    {
        try
        {
            BackupFile dlBackup = GetDarkLoaderBackupFile(ctx, fmData.ArchiveNoExtensionWhitespaceTrimmed);
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
        catch (Exception ex)
        {
            fmData.FM.LogInfo(
                ErrorText.Ex + "trying to move DarkLoader backup to " + ctx.DarkLoaderOriginalBackupPath,
                ex);
            throw;
        }
    }

    private static void AddEntry(ZipArchive archive, string fileNameOnDisk, string entryFileName, byte[] buffer)
    {
        // @DIRSEP: Converting to '/' because it will be a zip archive name and '/' is to spec
        ZipArchiveEntry entry = archive.CreateEntry(entryFileName.ToForwardSlashes(), CompressionLevel.Fastest);
        entry.LastWriteTime = new FileInfo(fileNameOnDisk).LastWriteTime;
        using FileStream_Read_WithRentedBuffer fs = new(fileNameOnDisk);
        using Stream eo = entry.Open();
        StreamCopyNoAlloc(fs.FileStream, eo, buffer);
    }

    private static bool IsSaveOrScreenshot(string path, Game game) =>
        path.PathStartsWithI(_screensDirS) ||
        (game == Game.Thief3 &&
         path.PathStartsWithI(_t3SavesDirS)) ||
        (game == Game.SS2 &&
         (_ss2SaveDirsInZipRegex.IsMatch(path) || path.PathStartsWithI(_ss2CurrentDirS))) ||
        (game != Game.Thief3 &&
         (path.PathStartsWithI(_darkSavesDirS) || path.PathStartsWithI(_darkNetSavesDirS)));

    private static (HashSetPathI ChangedList, HashSetPathI, HashSetPathI FullList)
    GetDiffBetweenFMInstalledDirAndFMArchive(
        HashSetPathI installedFMFiles,
        FMData fmData,
        byte[] fileStreamBuffer,
        bool useOnlySize = false)
    {
        HashSetPathI changedList = new();
        HashSetPathI addedList = new();
        HashSetPathI fullList = new();

        FanMission fm = fmData.FM;
        string fmArchivePath = fmData.ArchiveFilePath;
        string fmInstalledPath = fmData.InstalledPath;

        if (fmArchivePath.ExtIsZip())
        {
            using ZipArchive archive = GetReadModeZipArchiveCharEnc(fmArchivePath, fileStreamBuffer);

            var entries = archive.Entries;

            int entriesCount = entries.Count;

            HashSetPathI entriesFullNamesHash = new(entriesCount);

            for (int i = 0; i < entriesCount; i++)
            {
                ZipArchiveEntry entry = entries[i];
                string efn = entry.FullName.ToBackSlashes();

                /*
                @ZipSafety: One_Mans_Trash_v1.2.zip has a duplicate entry: obj/txt16/gold.gif
                The first is dated 2004/7/19, the other 2005/10/22
                The end result is we end up extracting both and the second overwrites the first, so the 2005
                one is the one that ends up on disk. Then when we diff against the archive we diff against
                the 2004 one and detect a modified file which then gets put into the backup archive.
                FMSel also overwrites the 2004 with the 2005, so nobody gets the edge case right.
                WinRAR and 7-Zip, upon full archive extract, both say there's a file with the same name and
                ask if you want to overwrite.
                If we wanted to be as correct as we can in the face of this, we should build a dictionary of
                <Entry, List<Entry>> and put dupes in there, then check against it for the diff.
                Or, just make each entry a List<Entry> and check all in the list for the diff.
                */
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
                        FileInfo fi = new(fileInInstalledDir);

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
            using RarArchive archive = RarArchive.Open(fmArchivePath);

            ICollection<RarArchiveEntry> entries = archive.Entries;
            int entriesCount = entries.Count;

            HashSetPathI entriesFullNamesHash = new(entriesCount);

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
                        FileInfo fi = new(fileInInstalledDir);

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
            using FileStream_Read_WithRentedBuffer fs = new(fmArchivePath);
            SevenZipArchive archive = new(fs.FileStream);

            ListFast<SevenZipArchiveEntry> entries = archive.Entries;
            int entriesCount = entries.Count;

            HashSetPathI entriesFullNamesHash = new(entriesCount);

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
                        FileInfo fi = new(fileInInstalledDir);

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

    private static BackupFile GetBackupFile(
        DarkLoaderBackupContext ctx,
        FMData fmData,
        List<string> archivePaths)
    {
        BackupFile ret = GetDarkLoaderBackupFile(ctx, fmData.ArchiveNoExtensionWhitespaceTrimmed);

        FanMission fm = fmData.FM;

        if (ret.Name.IsEmpty())
        {
            string fmArchiveNoExt = fmData.FMArchiveNoExtension;
            // This is as much as we can cache unfortunately. Every FM's name will be different each call
            // so we can't cache the combined config path and FM name with backup extension. But at least
            // we can cache just the FM name with backup extension, so it's better than nothing.
            string fmArchivePlusBackupExt = fmArchiveNoExt + Paths.FMBackupSuffix;
            string fmInstalledDirPlusBackupExt = fm.InstalledDir + Paths.FMBackupSuffix;
            List<FileInfo> bakFiles = new();

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

        ret.Found = true;
        return ret;
    }

    #region Helpers

    private static BackupFile GetDarkLoaderBackupFile(DarkLoaderBackupContext ctx, string fmArchiveNoExtTrimmed)
    {
        // Account for the fact that DarkLoader trims archive names for save backup zips
        // Note: I guess it doesn't?! The code heavily implies it does. Still, it works either way, so whatever.
        string dlSavesBakFileName = Path.Combine(ctx.DarkLoaderBackupPath, fmArchiveNoExtTrimmed + "_saves.zip");
        return File.Exists(dlSavesBakFileName)
            ? new BackupFile(true, dlSavesBakFileName, true)
            : new BackupFile(false, "", false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIgnoredFile(string fn) =>
        fn.EqualsI(Paths.FMSelInf) ||
        fn.EqualsI(_startMisSav) ||
        (
            Config.ExcludeSoundDirsFromBackupAndRestore &&
            (fn.PathStartsWithI(@"snd\") || fn.PathStartsWithI(@"snd2\"))
        );

    #endregion
}

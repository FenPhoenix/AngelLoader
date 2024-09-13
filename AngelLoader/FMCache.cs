using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using SharpCompress;
using SharpCompress.Archives.Rar;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers.Rar;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

/*
@BetterErrors(FMCache)
We should probably rethrow after logging so we can put up one dialog if any exceptions and then they can view the
log.
*/

internal static class FMCache
{
    #region Private fields

    private const string _t3ReadmeDir1 = "Fan Mission Extras";
    private const string _t3ReadmeDir1S = _t3ReadmeDir1 + "/";
    private const string _t3ReadmeDir2 = "FanMissionExtras";
    private const string _t3ReadmeDir2S = _t3ReadmeDir2 + "/";

    private static readonly string[] _imageFileExtensions =
    {
        ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp", ".jpg", ".jpeg", ".tiff",
    };

    #endregion

    // We might want to add other things (thumbnails etc.) later, so it's a class
    internal sealed class CacheData
    {
        internal readonly List<string> Readmes;
        internal CacheData() => Readmes = new List<string>();
        internal CacheData(List<string> readmes) => Readmes = readmes;
    }

    internal static void ClearCacheDir(FanMission fm, bool deleteCacheDirItself = false)
    {
        string fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);
        if (!fmCachePath.TrimEnd(CA_BS_FS).PathEqualsI(Paths.FMsCache.TrimEnd(CA_BS_FS)) && Directory.Exists(fmCachePath))
        {
            try
            {
                DirAndFileTree_UnSetReadOnly(fmCachePath);

                foreach (string f in FastIO.GetFilesTopOnly(fmCachePath, "*"))
                {
                    File.Delete(f);
                }
                foreach (string d in FastIO.GetDirsTopOnly(fmCachePath, "*"))
                {
                    Directory.Delete(d, recursive: true);
                }
                // TODO(ClearCacheDir/deleteCacheDirItself): This is here to keep the same behavior as before
                // The cache dir itself wasn't deleted before if called internally, which may be a bug(?)
                if (deleteCacheDirItself) Directory.Delete(fmCachePath);
            }
            catch (Exception ex)
            {
                fm.LogInfo(ErrorText.Ex + "clearing files in FM cache.", ex);
            }
        }
    }

    // If some files exist but not all that are in the zip, the user can just re-scan for this data by clicking
    // a button, so don't worry about it
    internal static async Task<CacheData> GetCacheableData(FanMission fm, bool refreshCache)
    {
        if (fm.Game == Game.Unsupported)
        {
            if (!fm.InstalledDir.IsEmpty()) ClearCacheDir(fm);
            return new CacheData();
        }

        try
        {
            if (FMIsReallyInstalled(fm, out string fmReadmesBasePath))
            {
                return new CacheData(GetValidReadmes(fmReadmesBasePath));
            }
            else
            {
                var readmes = new List<string>();

                string fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);

                if (!refreshCache)
                {
                    if (Directory.Exists(fmCachePath))
                    {
                        readmes = GetValidReadmes(fmCachePath);
                        if (readmes.Count > 0) return new CacheData(readmes);
                    }

                    if (fm.NoReadmes) return new CacheData();
                }

                #region Refresh cache from archive

                // This is in the same method in order to avoid unnecessary async/await machinery

                readmes.Clear();
                ClearCacheDir(fm);

                string fmArchivePath;
                if (fm.Game == Game.TDM)
                {
                    string fmInstallPath = Config.GetFMInstallPath(GameIndex.TDM);
                    if (fmInstallPath.IsEmpty()) return new CacheData();
                    DictionaryI<string> pk4FilesConverted = TDM.GetTDMBaseFMsDirPK4sConverted();
                    fmArchivePath = pk4FilesConverted.TryGetValue(fm.TDMInstalledDir, out string realPK4)
                        ? Path.Combine(fmInstallPath, realPK4)
                        : Path.Combine(fmInstallPath, fm.TDMInstalledDir + ".pk4");
                }
                else
                {
                    fmArchivePath = FMArchives.FindFirstMatch(fm.Archive, FMArchives.GetFMArchivePaths());
                }

                // In weird situations this could be true, so just say none and at least don't crash
                if (fmArchivePath.IsEmpty()) return new CacheData();

                if ((fm.Game == Game.TDM &&
                     (fmArchivePath.EndsWithI(".pk4") || fmArchivePath.EndsWithI(".zip"))) ||
                    fm.Archive.ExtIsZip())
                {
                    byte[] zipExtractTempBuffer = new byte[StreamCopyBufferSize];
                    byte[] fileStreamBuffer = new byte[FileStreamBufferSize];

                    Extract_Zip(fmArchivePath, fmCachePath, readmes, fm.Game == Game.TDM, zipExtractTempBuffer, fileStreamBuffer);

                    if (fm.Game != Game.TDM)
                    {
                        if (HtmlReadmeExists(readmes, fmCachePath))
                        {
                            ExtractHtmlRefFiles_Zip(fmArchivePath, fmCachePath, zipExtractTempBuffer, fileStreamBuffer);
                        }
                    }
                }
                else if (fm.Archive.ExtIsRar())
                {
                    byte[] rarExtractTempBuffer = new byte[StreamCopyBufferSize];

                    RarArchive? archive = null;
                    try
                    {
                        archive = RarArchive.Open(fmArchivePath);
                        int entriesCount = archive.Entries.Count;

                        if (archive.IsSolid)
                        {
                            archive.Dispose();
                            using var fs = File_OpenReadFast(fmArchivePath);
                            using (var reader = RarReader.Open(fs))
                            {
                                await Extract_RarSolid(reader, fmCachePath, readmes, rarExtractTempBuffer, entriesCount);
                            }

                            if (HtmlReadmeExists(readmes, fmCachePath))
                            {
                                await ExtractHtmlRefFiles_RarSolid(fmArchivePath, fmCachePath);
                            }
                        }
                        else
                        {
                            Extract_Rar(archive, fmCachePath, readmes, rarExtractTempBuffer);
                            archive.Dispose();

                            if (HtmlReadmeExists(readmes, fmCachePath))
                            {
                                ExtractHtmlRefFiles_Rar(fmArchivePath, fmCachePath, rarExtractTempBuffer);
                            }
                        }
                    }
                    finally
                    {
                        archive?.Dispose();
                    }
                }
                else
                {
                    await Extract_7z(fmArchivePath, fmCachePath, readmes);

                    if (HtmlReadmeExists(readmes, fmCachePath))
                    {
                        await ExtractHtmlRefFiles_7z(fmArchivePath, fmCachePath);
                    }
                }

                fm.NoReadmes = readmes.Count == 0;

                #endregion

                return new CacheData(readmes);
            }
        }
        catch (Exception ex)
        {
            Log(ex: ex);
            return new CacheData();
        }
        finally
        {
            Core.View.HideProgressBox();
        }

        // Does not check basePath for existence, so check it first before calling.
        static List<string> GetValidReadmes(string basePath)
        {
            string[] readmePaths =
            {
                basePath,
                Path.Combine(basePath, _t3ReadmeDir1),
                Path.Combine(basePath, _t3ReadmeDir2),
            };

            var readmes = new List<string>();

            for (int i = 0; i < readmePaths.Length; i++)
            {
                string readmePath = readmePaths[i];

                // We assume the first dir exists (to prevent an expensive duplicate check), so only check for others
                if (i == 0 || Directory.Exists(readmePath))
                {
                    foreach (string fn in FastIO.GetFilesTopOnly(readmePath, "*"))
                    {
                        if (fn.IsValidReadme() && new FileInfo(fn).Length > 0)
                        {
                            readmes.Add(fn.Substring(basePath.Length + 1));
                        }
                    }
                }
            }

            return readmes;
        }

        static bool HtmlReadmeExists(List<string> readmes, string fmCachePath)
        {
            if (!Directory.Exists(fmCachePath)) return false;

            for (int i = 0; i < readmes.Count; i++)
            {
                if (readmes[i].ExtIsHtml())
                {
                    return true;
                }
            }

            return false;
        }
    }

    #region Extract

    // We need to block the UI thread one way or another, to prevent starting a zillion parallel tasks that
    // could interfere with each other, especially as they include disk access. Zip extraction, being fast,
    // just blocks by not being async, while the async 7-zip extraction blocks by putting up a progress box.
    private static void Extract_Zip(string fmArchivePath, string fmCachePath, List<string> readmes, bool isTDM, byte[] zipExtractTempBuffer, byte[] fileStreamBuffer)
    {
        try
        {
            using ZipArchive archive = GetReadModeZipArchiveCharEnc(fmArchivePath, fileStreamBuffer);

            for (int i = 0; i < archive.Entries.Count; i++)
            {
                ZipArchiveEntry entry = archive.Entries[i];
                string fn = entry.FullName;
                string? t3ReadmeDir = null;

                if (isTDM)
                {
                    // @TDM_CASE("darkmod.txt", "readme.txt" constants)
                    if (entry.Length == 0 || (!fn.EqualsI("darkmod.txt") && !fn.EqualsI("readme.txt")))
                    {
                        continue;
                    }
                }
                else
                {
                    if (!fn.IsValidReadme() || entry.Length == 0) continue;

                    int dirSeps = fn.Rel_CountDirSepsUpToAmount(2);
                    if (dirSeps == 1)
                    {
                        if (fn.PathStartsWithI(_t3ReadmeDir1S))
                        {
                            t3ReadmeDir = _t3ReadmeDir1;
                        }
                        else if (fn.PathStartsWithI(_t3ReadmeDir2S))
                        {
                            t3ReadmeDir = _t3ReadmeDir2;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (dirSeps > 1)
                    {
                        continue;
                    }
                }

                Directory.CreateDirectory(!t3ReadmeDir.IsEmpty()
                    ? Path.Combine(fmCachePath, t3ReadmeDir)
                    : fmCachePath);

                string fileNameFull;
                try
                {
                    fileNameFull = GetExtractedNameOrThrowIfMalicious(fmCachePath, fn);
                }
                catch
                {
                    // ignore, message already logged
                    continue;
                }
                entry.ExtractToFile_Fast(fileNameFull, overwrite: true, zipExtractTempBuffer);
                File_UnSetReadOnly(fileNameFull);
                readmes.Add(fn);
            }
        }
        catch (Exception ex)
        {
            Log(ErrorText.Ex + "in zip extract to cache", ex);
        }
    }

    private static async Task Extract_7z(string fmArchivePath, string fmCachePath, List<string> readmes)
    {
        InitProgressBoxForSolidExtract();

        await Task.Run(() =>
        {
            List<string> fileNamesList = new();
            try
            {
                Directory.CreateDirectory(fmCachePath);

                int extractorFilesCount;

                using (var fs = File_OpenReadFast(fmArchivePath))
                {
                    var archive = new SevenZipArchive(fs);
                    ListFast<SevenZipArchiveEntry> entries = archive.Entries;
                    extractorFilesCount = entries.Count;
                    for (int i = 0; i < entries.Count; i++)
                    {
                        SevenZipArchiveEntry entry = entries[i];

                        if (entry.IsAnti) continue;

                        string fn = entry.FileName;
                        int dirSeps;
                        if (fn.IsValidReadme() && entry.UncompressedSize > 0 &&
                            (((dirSeps = fn.Rel_CountDirSepsUpToAmount(2)) == 1 &&
                              (fn.PathStartsWithI(_t3ReadmeDir1S) ||
                               fn.PathStartsWithI(_t3ReadmeDir2S))) ||
                             dirSeps == 0))
                        {
                            fileNamesList.Add(fn);
                            readmes.Add(fn);
                        }
                    }
                }

                if (fileNamesList.Count == 0) return;

                Paths.CreateOrClearTempPath(TempPaths.SevenZipList);

                static void ReportProgress(Fen7z.ProgressReport pr)
                {
                    // For selective-file extracts, we want percent-of-bytes, otherwise if we ask for 3 files
                    // but there's 8014 in the archive, it counts "100%" as "3 files out of 8014", thus giving
                    // us a useless "percentage" value for this purpose.
                    // Even if we used the files list count as the max, the percentage bar wouldn't be smooth.
                    Core.View.SetProgressPercent(pr.PercentOfBytes);
                }

                var progress = new Progress<Fen7z.ProgressReport>(ReportProgress);

                string listFile = Path.Combine(Paths.SevenZipListTemp, fmCachePath.GetDirNameFast() + ".7zl");

                Fen7z.Result result = Fen7z.Extract(
                    sevenZipWorkingPath: Paths.SevenZipPath,
                    sevenZipPathAndExe: Paths.SevenZipExe,
                    archivePath: fmArchivePath,
                    outputPath: fmCachePath,
                    entriesCount: extractorFilesCount,
                    listFile: listFile,
                    fileNamesList: fileNamesList,
                    progress: progress);

                if (result.ErrorOccurred)
                {
                    Log("Readme caching (7z): " + fmCachePath + $":{NL}" + result);
                }
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + "in 7z extract to cache", ex);
            }
            finally
            {
                try
                {
                    foreach (string file in Directory.GetFiles(fmCachePath, "*", SearchOption.AllDirectories))
                    {
                        File_UnSetReadOnly(file);
                    }
                }
                catch
                {
                    // ignore
                }
            }
        });
    }

    private static void Extract_Rar(RarArchive archive, string fmCachePath, List<string> readmes, byte[] rarExtractTempBuffer)
    {
        try
        {
            foreach (var entry in archive.Entries)
            {
                string fn = entry.Key;
                string? t3ReadmeDir = null;

                if (!fn.IsValidReadme() || entry.Size == 0) continue;

                int dirSeps = fn.Rel_CountDirSepsUpToAmount(2);
                if (dirSeps == 1)
                {
                    if (fn.PathStartsWithI(_t3ReadmeDir1S))
                    {
                        t3ReadmeDir = _t3ReadmeDir1;
                    }
                    else if (fn.PathStartsWithI(_t3ReadmeDir2S))
                    {
                        t3ReadmeDir = _t3ReadmeDir2;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (dirSeps > 1)
                {
                    continue;
                }

                Directory.CreateDirectory(!t3ReadmeDir.IsEmpty()
                    ? Path.Combine(fmCachePath, t3ReadmeDir)
                    : fmCachePath);

                string fileNameFull;
                try
                {
                    fileNameFull = GetExtractedNameOrThrowIfMalicious(fmCachePath, fn);
                }
                catch
                {
                    // ignore, message already logged
                    continue;
                }
                entry.ExtractToFile_Fast(fileNameFull, overwrite: true, rarExtractTempBuffer);
                File_UnSetReadOnly(fileNameFull);
                readmes.Add(fn);
            }
        }
        catch (Exception ex)
        {
            Log(ErrorText.Ex + "in rar extract to cache", ex);
        }
    }

    private static async Task Extract_RarSolid(RarReader reader, string fmCachePath, List<string> readmes, byte[] rarExtractTempBuffer, int entriesCount)
    {
        InitProgressBoxForSolidExtract();

        await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(fmCachePath);

                int i = -1;
                while (reader.MoveToNextEntry())
                {
                    i++;
                    RarReaderEntry entry = reader.Entry;
                    string fn = entry.Key;
                    string? t3ReadmeDir = null;

                    Core.View.SetProgressPercent(GetPercentFromValue_Int(i, entriesCount));

                    if (!fn.IsValidReadme() || entry.Size == 0) continue;

                    int dirSeps = fn.Rel_CountDirSepsUpToAmount(2);
                    if (dirSeps == 1)
                    {
                        if (fn.PathStartsWithI(_t3ReadmeDir1S))
                        {
                            t3ReadmeDir = _t3ReadmeDir1;
                        }
                        else if (fn.PathStartsWithI(_t3ReadmeDir2S))
                        {
                            t3ReadmeDir = _t3ReadmeDir2;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (dirSeps > 1)
                    {
                        continue;
                    }

                    Directory.CreateDirectory(!t3ReadmeDir.IsEmpty()
                        ? Path.Combine(fmCachePath, t3ReadmeDir)
                        : fmCachePath);

                    string fileNameFull;
                    try
                    {
                        fileNameFull = GetExtractedNameOrThrowIfMalicious(fmCachePath, fn);
                    }
                    catch
                    {
                        // ignore, message already logged
                        continue;
                    }
                    reader.ExtractToFile_Fast(fileNameFull, overwrite: true, rarExtractTempBuffer);
                    File_UnSetReadOnly(fileNameFull);
                    readmes.Add(fn);
                }
            }
            catch (Exception ex)
            {
                Log(ErrorText.Ex + "in rar (solid) extract to cache", ex);
            }
        });
    }

    #endregion

    #region HTML reference extract

    // An html file might have other files it references, so do a recursive search through the archive to find
    // them all, and extract only the required files to the cache. That way we keep the disk footprint way down.
    private static void ExtractHtmlRefFiles_Zip(string fmArchivePath, string fmCachePath, byte[] zipExtractTempBuffer, byte[] fileStreamBuffer)
    {
        List<NameAndIndex> htmlRefFiles = new();

        using ZipArchive archive = GetReadModeZipArchiveCharEnc(fmArchivePath, fileStreamBuffer);

        var entries = archive.Entries;

        foreach (string f in Directory.GetFiles(fmCachePath, "*", SearchOption.AllDirectories))
        {
            if (!f.ExtIsHtml()) continue;

            string html = File.ReadAllText(f);

            for (int i = 0; i < entries.Count; i++)
            {
                ZipArchiveEntry e = entries[i];
                AddHtmlRefFile(name: e.Name, fullName: e.FullName, i, html, htmlRefFiles);
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            for (int ri = 0; ri < htmlRefFiles.Count; ri++)
            {
                NameAndIndex f = htmlRefFiles[ri];
                ZipArchiveEntry re = entries[f.Index];

                if (RefFileExcluded(f.Name, re.Length)) continue;

                string content;
                using (var es = re.Open())
                {
                    using var sr = new StreamReader(es);
                    content = sr.ReadToEnd();
                }

                for (int ei = 0; ei < entries.Count; ei++)
                {
                    ZipArchiveEntry e = entries[ei];
                    AddHtmlRefFile(name: e.Name, fullName: e.FullName, ei, content, htmlRefFiles);
                }
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            foreach (NameAndIndex f in htmlRefFiles)
            {
                string finalFileName;
                try
                {
                    finalFileName = GetExtractedNameOrThrowIfMalicious(fmCachePath, f.Name);
                }
                catch
                {
                    // ignore, message already logged
                    continue;
                }
                string? path = Path.GetDirectoryName(f.Name);
                if (!path.IsEmpty()) Directory.CreateDirectory(Path.Combine(fmCachePath, path));
                entries[f.Index].ExtractToFile_Fast(finalFileName, overwrite: true, zipExtractTempBuffer);
                File_UnSetReadOnly(finalFileName);
            }
        }
    }

    private static async Task ExtractHtmlRefFiles_7z(string fmArchivePath, string fmCachePath)
    {
        await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(fmCachePath);
                Paths.CreateOrClearTempPath(TempPaths.SevenZipList);
                Paths.CreateOrClearTempPath(TempPaths.FMCache);

                string cacheTempPath = Paths.FMCacheTemp;

                int entriesCount;

                string[] cacheFiles = Directory.GetFiles(fmCachePath, "*", SearchOption.AllDirectories);

                List<string> archiveFileNamesNameOnly = new(0);
                List<string> archiveNonExcludedFullFileNames = new();
                using (FileStreamCustom fs = File_OpenReadFast(fmArchivePath))
                {
                    SevenZipArchive extractor = new(fs);
                    entriesCount = extractor.GetEntryCountOnly();
                    archiveFileNamesNameOnly.Capacity = entriesCount;
                    ListFast<SevenZipArchiveEntry> entries = extractor.Entries;
                    for (int i = 0; i < entriesCount; i++)
                    {
                        SevenZipArchiveEntry entry = entries[i];
                        if (!HtmlRefExcludes.Any(entry.FileName.EndsWithI))
                        {
                            archiveFileNamesNameOnly.Add(entry.FileName.GetFileNameFast());
                            archiveNonExcludedFullFileNames.Add(entry.FileName);
                        }
                    }
                }

                if (!HtmlNeedsReferenceExtract(cacheFiles, archiveFileNamesNameOnly))
                {
                    return (false, false);
                }

                Core.View.SetProgressBoxState_Single(
                    message1: LText.ProgressBox.CachingHTMLReferencedFiles,
                    percent: 0);

                var progress = new Progress<Fen7z.ProgressReport>(ReportProgress);

                string listFile = Path.Combine(Paths.SevenZipListTemp, fmCachePath.GetDirNameFast() + ".7zl");

                Fen7z.Result result = Fen7z.Extract(
                    sevenZipWorkingPath: Paths.SevenZipPath,
                    sevenZipPathAndExe: Paths.SevenZipExe,
                    archivePath: fmArchivePath,
                    outputPath: cacheTempPath,
                    entriesCount: entriesCount,
                    listFile: listFile,
                    fileNamesList: archiveNonExcludedFullFileNames,
                    progress: progress
                );

                if (result.ErrorOccurred)
                {
                    Log("Error extracting 7z " + fmArchivePath + " to " + cacheTempPath + $"{NL}" + result);
                    return (result.Canceled, true);
                }

                DoHtmlReferenceCopy(cacheTempPath, fmCachePath, cacheFiles);

                return (result.Canceled, false);
            }
            catch (Exception ex)
            {
                Log($"Error extracting dependent files for html readme(s).{NL}" +
                    $"FM archive type: 7z{NL}" +
                    "FM archive path: " + fmArchivePath, ex);
                return (false, true);
            }
            finally
            {
                Paths.CreateOrClearTempPath(TempPaths.FMCache);
            }
        });

        return;

        static void ReportProgress(Fen7z.ProgressReport pr)
        {
            if (!pr.Canceling)
            {
                Core.View.SetProgressPercent(pr.PercentOfEntries);
            }
        }
    }

    private static void ExtractHtmlRefFiles_Rar(string fmArchivePath, string fmCachePath, byte[] zipExtractTempBuffer)
    {
        List<NameAndIndex> htmlRefFiles = new();

        using RarArchive archive = RarArchive.Open(fmArchivePath);

        RarArchiveEntry[] entries = archive.Entries.ToArray();

        foreach (string f in Directory.GetFiles(fmCachePath, "*", SearchOption.AllDirectories))
        {
            if (!f.ExtIsHtml()) continue;

            string html = File.ReadAllText(f);

            for (int i = 0; i < entries.Length; i++)
            {
                RarArchiveEntry e = entries[i];
                AddHtmlRefFile(name: e.Key.GetDirNameFast(), fullName: e.Key, i, html, htmlRefFiles);
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            for (int ri = 0; ri < htmlRefFiles.Count; ri++)
            {
                NameAndIndex f = htmlRefFiles[ri];
                RarArchiveEntry re = entries[f.Index];

                if (RefFileExcluded(f.Name, re.Size)) continue;

                string content;
                using (var es = re.OpenEntryStream())
                {
                    using var sr = new StreamReader(es);
                    content = sr.ReadToEnd();
                }

                for (int ei = 0; ei < entries.Length; ei++)
                {
                    RarArchiveEntry e = entries[ei];
                    AddHtmlRefFile(name: e.Key.GetDirNameFast(), fullName: e.Key, ei, content, htmlRefFiles);
                }
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            foreach (NameAndIndex f in htmlRefFiles)
            {
                string finalFileName;
                try
                {
                    finalFileName = GetExtractedNameOrThrowIfMalicious(fmCachePath, f.Name);
                }
                catch
                {
                    // ignore, message already logged
                    continue;
                }
                string? path = Path.GetDirectoryName(f.Name);
                if (!path.IsEmpty()) Directory.CreateDirectory(Path.Combine(fmCachePath, path));
                entries[f.Index].ExtractToFile_Fast(finalFileName, overwrite: true, zipExtractTempBuffer);
                File_UnSetReadOnly(finalFileName);
            }
        }
    }

    private static async Task ExtractHtmlRefFiles_RarSolid(string fmArchivePath, string fmCachePath)
    {
        await Task.Run(() =>
        {
            try
            {
                Directory.CreateDirectory(fmCachePath);
                Paths.CreateOrClearTempPath(TempPaths.FMCache);

                string cacheTempPath = Paths.FMCacheTemp;

                string[] cacheFiles = Directory.GetFiles(fmCachePath, "*", SearchOption.AllDirectories);

                List<string> archiveFileNamesNameOnly = new(0);

                using (FileStreamCustom fs = File_OpenReadFast(fmArchivePath))
                {
                    int entriesCount;
                    using (var archive = RarArchive.Open(fs))
                    {
                        LazyReadOnlyCollection<RarArchiveEntry> entries = archive.Entries;
                        entriesCount = entries.Count;
                        for (int i = 0; i < entriesCount; i++)
                        {
                            archiveFileNamesNameOnly.Add(entries[i].Key.GetFileNameFast());
                        }
                        fs.Position = 0;
                    }

                    if (!HtmlNeedsReferenceExtract(cacheFiles, archiveFileNamesNameOnly))
                    {
                        return;
                    }

                    Core.View.SetProgressBoxState_Single(
                        message1: LText.ProgressBox.CachingHTMLReferencedFiles,
                        percent: 0);

                    byte[] tempBuffer = new byte[StreamCopyBufferSize];

                    using (RarReader reader = RarReader.Open(fs))
                    {
                        int i = -1;
                        while (reader.MoveToNextEntry())
                        {
                            i++;
                            RarReaderEntry entry = reader.Entry;
                            string fullName = entry.Key;
                            string name = fullName.GetFileNameFast();

                            if (!entry.IsDirectory &&
                                !fullName.IsEmpty() &&
                                !fullName[^1].IsDirSep() &&
                                !HtmlRefExcludes.Any(name.EndsWithI))
                            {
                                string extractedName;
                                try
                                {
                                    extractedName = GetExtractedNameOrThrowIfMalicious(cacheTempPath, fullName);
                                }
                                catch
                                {
                                    // ignore, message already logged
                                    Core.View.SetProgressPercent(GetPercentFromValue_Int(i + 1, entriesCount));
                                    continue;
                                }

                                if (fullName.Rel_ContainsDirSep())
                                {
                                    Directory.CreateDirectory(Path.Combine(cacheTempPath,
                                        fullName.Substring(0, fullName.Rel_LastIndexOfDirSep())));
                                }

                                reader.ExtractToFile_Fast(extractedName, overwrite: true, tempBuffer);
                                File_UnSetReadOnly(extractedName);
                            }

                            Core.View.SetProgressPercent(GetPercentFromValue_Int(i + 1, entriesCount));
                        }
                    }
                }

                DoHtmlReferenceCopy(cacheTempPath, fmCachePath, cacheFiles);
            }
            catch (Exception ex)
            {
                Log($"Error extracting dependent files for html readme(s).{NL}" +
                    $"FM archive type: RAR (Solid){NL}" +
                    "FM archive path: " + fmArchivePath, ex);
            }
            finally
            {
                Paths.CreateOrClearTempPath(TempPaths.FMCache);
            }
        });
    }

    private static void InitProgressBoxForSolidExtract()
    {
        // Critical
        Core.View.Invoke(new Action(static () => Core.View.Show()));

        // Block the view immediately after starting another thread, because otherwise we could end
        // up allowing multiple of these to be called and all that insanity...

        // Show progress box on UI thread to seal thread gaps (make auto-refresh blocking airtight)
        Core.View.ShowProgressBox_Single(LText.ProgressBox.CachingReadmeFiles);
    }

    private static void DoHtmlReferenceCopy(string cacheTempPath, string fmCachePath, string[] cacheFiles)
    {
        List<NameAndIndex> htmlRefFiles = new();

        string cacheTempPathWithTrailingSep = cacheTempPath.Length > 0 && !cacheTempPath[^1].IsDirSep()
            ? cacheTempPath + "\\"
            : cacheTempPath;

        string[] tempCacheFiles = Directory.GetFiles(cacheTempPath, "*", SearchOption.AllDirectories);

        foreach (string cacheFile in cacheFiles)
        {
            if (!cacheFile.ExtIsHtml()) continue;

            string html = File.ReadAllText(cacheFile);

            for (int i = 0; i < tempCacheFiles.Length; i++)
            {
                string tempCacheFile = tempCacheFiles[i];
                AddHtmlRefFile(name: tempCacheFile.GetFileNameFast(), fullName: tempCacheFile, i, html, htmlRefFiles);
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            for (int ri = 0; ri < htmlRefFiles.Count; ri++)
            {
                NameAndIndex f = htmlRefFiles[ri];
                string refFile = tempCacheFiles[f.Index];

                FileInfo fi = new(refFile);

                if (RefFileExcluded(f.Name, fi.Length)) continue;

                string content = File.ReadAllText(f.Name);

                for (int ei = 0; ei < tempCacheFiles.Length; ei++)
                {
                    string tempCacheFile = tempCacheFiles[ei];
                    AddHtmlRefFile(name: tempCacheFile.GetFileNameFast(), fullName: tempCacheFile, ei, content, htmlRefFiles);
                }
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            foreach (NameAndIndex f in htmlRefFiles)
            {
                string tempCacheFile = tempCacheFiles[f.Index];

                string finalFileName = Path.Combine(fmCachePath, tempCacheFile.Substring(cacheTempPathWithTrailingSep.Length).Trim(CA_BS_FS));
                string? path = Path.GetDirectoryName(finalFileName);
                if (!path.IsEmpty()) Directory.CreateDirectory(Path.Combine(fmCachePath, path));
                File.Copy(tempCacheFile, finalFileName, overwrite: true);
                File_UnSetReadOnly(finalFileName);
            }
        }
    }

    private static bool RefFileExcluded(string name, long size) =>
        HtmlRefExcludes.Any(name.EndsWithI) ||
        _imageFileExtensions.Any(name.EndsWithI) ||
        // 128k is generous. Any text or markup sort of file should be WELL under that.
        size > ByteSize.KB * 128;

    private static void AddHtmlRefFile(string name, string fullName, int i, string content, List<NameAndIndex> htmlRefFiles)
    {
        /*
        We just do a dumb string-match search through the whole file. While it's true that HTML files have their
        links in specific structures (href tags etc.), we don't attempt to narrow it down to these because a) we
        want to future-proof against any new ways to link that might come about, and b) HTML files can link out
        to other formats like CSS and who knows what else, and we don't want to write parsers for every format
        under the sun.
        */
        if (!name.IsEmpty() && name.Contains('.') && !HtmlRefExcludes.Any(name.EndsWithI) &&
            content.ContainsI(name) && htmlRefFiles.TrueForAll(x => x.Index != i))
        {
            htmlRefFiles.Add(new NameAndIndex(fullName, i));
        }
    }

    #endregion
}

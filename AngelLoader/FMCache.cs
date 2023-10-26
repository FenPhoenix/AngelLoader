using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AL_Common;
using AngelLoader.DataClasses;
using SharpCompress.Archives.SevenZip;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.GameSupport;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader;

// @BetterErrors(FMCache)
// We should probably rethrow after logging so we can put up one dialog if any exceptions and then they can
// view the log

internal static class FMCache
{
    #region Private fields

    private const string _t3ReadmeDir1 = "Fan Mission Extras";
    private const string _t3ReadmeDir1S = _t3ReadmeDir1 + "/";
    private const string _t3ReadmeDir2 = "FanMissionExtras";
    private const string _t3ReadmeDir2S = _t3ReadmeDir2 + "/";

    private static readonly string[] _imageFileExtensions =
    {
        ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp", ".jpg", ".jpeg", ".tiff"
    };

    // Try to reject formats that don't make sense. Exclude instead of include for future-proofing.
    private static readonly string[] _htmlRefExcludes =
    {
        ".osm", ".exe", ".dll", ".ose", ".mis", ".gam", ".ibt", ".cbt", ".gmp", ".ned", ".unr", ".wav",
        ".mp3", ".ogg", ".aiff", ".aif", ".flac", ".bin", ".dlx", ".mc", ".mi", ".avi", ".mp4", ".mkv",
        ".flv", ".log", ".str", ".nut", ".db", ".obj"
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

                foreach (string f in FastIO.GetFilesTopOnly(fmCachePath, "*")) File.Delete(f);
                foreach (string d in FastIO.GetDirsTopOnly(fmCachePath, "*")) Directory.Delete(d, recursive: true);
                // TODO(ClearCacheDir/deleteCacheDirItself): This is here to keep the same behavior as before
                // The cache dir itself wasn't deleted before if called internally, which may be a bug(?)
                if (deleteCacheDirItself) Directory.Delete(fmCachePath);
            }
            catch (Exception ex)
            {
                LogFMInfo(fm, ErrorText.Ex + "clearing files in FM cache.", ex);
            }
        }
    }

    // If some files exist but not all that are in the zip, the user can just re-scan for this data by clicking
    // a button, so don't worry about it
    internal static async Task<CacheData> GetCacheableData(FanMission fm, bool refreshCache)
    {
        #region Local functions

        // Does not check basePath for existence, so check it first before calling.
        static List<string> GetValidReadmes(string basePath)
        {
            string[] readmePaths =
            {
                basePath,
                Path.Combine(basePath, _t3ReadmeDir1),
                Path.Combine(basePath, _t3ReadmeDir2)
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

        #endregion

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
                    fmArchivePath = Path.Combine(fmInstallPath, fm.TDMInstalledDir + ".pk4");
                }
                else
                {
                    fmArchivePath = FMArchives.FindFirstMatch(fm.Archive, FMArchives.GetFMArchivePaths());
                }

                // In weird situations this could be true, so just say none and at least don't crash
                if (fmArchivePath.IsEmpty()) return new CacheData();

                if ((fm.Game == Game.TDM && fmArchivePath.EndsWithI(".pk4")) || fm.Archive.ExtIsZip())
                {
                    byte[] zipExtractTempBuffer = new byte[StreamCopyBufferSize];
                    byte[] fileStreamBuffer = new byte[FileStreamBufferSize];

                    ZipExtract(fmArchivePath, fmCachePath, readmes, fm.Game == Game.TDM, zipExtractTempBuffer, fileStreamBuffer);

                    if (fm.Game != Game.TDM)
                    {
                        // @HTMLRefExtraction(FMCache):
                        // TODO: Support HTML ref extraction for .7z files too
                        // Will require full extract for the same reason scan does - we need to scan files to
                        // know what other files to scan, etc. and a full extract is with 99.9999% certainty
                        // going to be faster than chugging through the whole thing over and over and over for
                        // each new file we find we need

                        // Guard check so we don't do useless HTML work if we don't have any HTML readmes
                        bool htmlReadmeExists = false;
                        for (int i = 0; i < readmes.Count; i++)
                        {
                            if (readmes[i].ExtIsHtml())
                            {
                                htmlReadmeExists = true;
                                break;
                            }
                        }

                        if (htmlReadmeExists && Directory.Exists(fmCachePath))
                        {
                            try
                            {
                                ExtractHTMLRefFiles(fmArchivePath, fmCachePath, zipExtractTempBuffer, fileStreamBuffer);
                            }
                            catch (Exception ex)
                            {
                                Log(ex: ex);
                            }
                        }
                    }
                }
                else
                {
                    await Task.Run(() => SevenZipExtract(fmArchivePath, fmCachePath, readmes));
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
    }

    #region Extract

    // An html file might have other files it references, so do a recursive search through the archive to find
    // them all, and extract only the required files to the cache. That way we keep the disk footprint way down.
    private static void ExtractHTMLRefFiles(string fmArchivePath, string fmCachePath, byte[] zipExtractTempBuffer, byte[] fileStreamBuffer)
    {
        var htmlRefFiles = new List<NameAndIndex>();

        using ZipArchive archive = GetReadModeZipArchiveCharEnc(fmArchivePath, fileStreamBuffer);

        foreach (string f in Directory.GetFiles(fmCachePath, "*", SearchOption.AllDirectories))
        {
            if (!f.ExtIsHtml()) continue;

            string html = File.ReadAllText(f);

            for (int i = 0; i < archive.Entries.Count; i++)
            {
                ZipArchiveEntry e = archive.Entries[i];
                if (e.Name.IsEmpty() || !e.Name.Contains('.') || _htmlRefExcludes.Any(e.Name.EndsWithI))
                {
                    continue;
                }

                // We just do a dumb string-match search through the whole file. While it's true that HTML
                // files have their links in specific structures (href tags etc.), we don't attempt to
                // narrow it down to these because a) we want to future-proof against any new ways to link
                // that might come about, and b) HTML files can link out to other formats like CSS and
                // who knows what else, and we don't want to write parsers for every format under the sun.
                if (html.ContainsI(e.Name) && htmlRefFiles.TrueForAll(x => x.Index != i))
                {
                    htmlRefFiles.Add(new NameAndIndex(e.FullName, i));
                }
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            for (int ri = 0; ri < htmlRefFiles.Count; ri++)
            {
                NameAndIndex f = htmlRefFiles[ri];
                if (_htmlRefExcludes.Any(f.Name.EndsWithI) ||
                    _imageFileExtensions.Any(f.Name.EndsWithI))
                {
                    continue;
                }

                ZipArchiveEntry re = archive.Entries[f.Index];

                // 128k is generous. Any text or markup sort of file should be WELL under that.
                if (re.Length > ByteSize.KB * 128) continue;

                string content;
                using (var es = re.Open())
                {
                    using var sr = new StreamReader(es);
                    content = sr.ReadToEnd();
                }

                for (int ei = 0; ei < archive.Entries.Count; ei++)
                {
                    ZipArchiveEntry e = archive.Entries[ei];
                    if (e.Name.IsEmpty() || !e.Name.Contains('.') || _htmlRefExcludes.Any(e.Name.EndsWithI))
                    {
                        continue;
                    }

                    if (content.ContainsI(e.Name) && htmlRefFiles.TrueForAll(x => x.Index != ei))
                    {
                        htmlRefFiles.Add(new NameAndIndex(e.FullName, ei));
                    }
                }
            }
        }

        if (htmlRefFiles.Count > 0)
        {
            foreach (NameAndIndex f in htmlRefFiles)
            {
                string? path = Path.GetDirectoryName(f.Name);
                if (!path.IsEmpty()) Directory.CreateDirectory(Path.Combine(fmCachePath, path));
                archive.Entries[f.Index].ExtractToFile_Fast(Path.Combine(fmCachePath, f.Name), overwrite: true, zipExtractTempBuffer);
            }
        }
    }

    // We need to block the UI thread one way or another, to prevent starting a zillion parallel tasks that
    // could interfere with each other, especially as they include disk access. Zip extraction, being fast,
    // just blocks by not being async, while the async 7-zip extraction blocks by putting up a progress box.
    private static void ZipExtract(string fmArchivePath, string fmCachePath, List<string> readmes, bool isTDM, byte[] zipExtractTempBuffer, byte[] fileStreamBuffer)
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

                string fileNameFull = Path.Combine(fmCachePath, fn);
                entry.ExtractToFile_Fast(fileNameFull, overwrite: true, zipExtractTempBuffer);
                readmes.Add(fn);
            }
        }
        catch (Exception ex)
        {
            Log(ErrorText.Ex + "in zip extract to cache", ex);
        }
    }

    private static async Task SevenZipExtract(string fmArchivePath, string fmCachePath, List<string> readmes)
    {
        var fileNamesList = new List<string>();
        try
        {
            // Critical
            Core.View.Invoke(new Action(static () => Core.View.Show()));

            // Block the view immediately after starting another thread, because otherwise we could end
            // up allowing multiple of these to be called and all that insanity...

            // Show progress box on UI thread to seal thread gaps (make auto-refresh blocking airtight)
            Core.View.ShowProgressBox_Single(message1: LText.ProgressBox.CachingReadmeFiles);

            await Task.Run(() =>
            {
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

                    Paths.CreateOrClearTempPath(Paths.SevenZipListTemp);

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
                        Log("Readme caching (7z): " + fmCachePath + ":\r\n" + result);
                    }
                }
                catch (Exception ex)
                {
                    Log(ErrorText.Ex + "in 7z extract to cache", ex);
                }
                finally
                {
                    foreach (string file in fileNamesList)
                    {
                        try
                        {
                            // Stupid Path.Combine might in theory throw
                            File_UnSetReadOnly(Path.Combine(fmCachePath, file));
                        }
                        catch
                        {
                            // ignore
                        }
                    }
                }
            });
        }
        finally
        {
            Core.View.HideProgressBox();
        }
    }

    #endregion
}

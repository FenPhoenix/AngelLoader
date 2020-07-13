using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using SevenZip;
using static AngelLoader.GameSupport;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class FMCache
    {
        #region Private fields

        private const string _t3ReadmeDir1 = "Fan Mission Extras";
        private const string _t3ReadmeDir1S = _t3ReadmeDir1 + "/";
        private const string _t3ReadmeDir2 = "FanMissionExtras";
        private const string _t3ReadmeDir2S = _t3ReadmeDir2 + "/";

        private static readonly string[]
        _imageFileExtensions =
        {
            ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp", ".jpg", ".jpeg", ".tiff"
        };

        // Try to reject formats that don't make sense. Exclude instead of include for future-proofing.
        private static readonly string[]
        _htmlRefExcludes =
        {
            ".osm", ".exe", ".ose", ".mis", ".ibt", ".cbt", ".gmp", ".ned", ".unr", ".wav", ".mp3", ".ogg",
            ".aiff", ".aif", ".flac", ".bin", ".dlx", ".mc", ".mi", ".avi", ".mp4", ".mkv", ".flv", ".log",
            ".str", ".nut", ".db", ".obj"
        };

        private sealed class NameAndIndex
        {
            internal string Name = "";
            internal int Index = -1;
        }

        #endregion

        // We might want to add other things (thumbnails etc.) later, so it's a class
        internal sealed class CacheData
        {
            internal readonly List<string> Readmes;
            internal CacheData() => Readmes = new List<string>();
            internal CacheData(List<string> readmes) => Readmes = readmes;
        }

        // If some files exist but not all that are in the zip, the user can just re-scan for this data by clicking
        // a button, so don't worry about it
        internal static async Task<CacheData> GetCacheableData(FanMission fm, bool refreshCache)
        {
            if (fm.Game == Game.Unsupported)
            {
                if (!fm.InstalledDir.IsEmpty())
                {
                    ClearCacheDir(fm);
                }
                return new CacheData();
            }

            try
            {
                return FMIsReallyInstalled(fm)
                    ? GetCacheableDataInFMInstalledDir(fm)
                    : await GetCacheableDataInFMCacheDir(fm, refreshCache);
            }
            catch (Exception ex)
            {
                Log("Exception in GetCacheableData", ex);
                return new CacheData();
            }
        }

        #region Helpers

        private static void ClearCacheDir(FanMission fm)
        {
            string fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);
            if (!fmCachePath.TrimEnd(CA_BS_FS).PathEqualsI(Paths.FMsCache.TrimEnd(CA_BS_FS)) && Directory.Exists(fmCachePath))
            {
                try
                {
                    foreach (string f in FastIO.GetFilesTopOnly(fmCachePath, "*")) File.Delete(f);
                    foreach (string d in FastIO.GetDirsTopOnly(fmCachePath, "*")) Directory.Delete(d, recursive: true);
                }
                catch (Exception ex)
                {
                    Log("Exception clearing files in FM cache for " + fm.Archive + " / " + fm.InstalledDir, ex);
                }
            }
        }

        /// <summary>
        /// Does not check <paramref name="basePath"/> for existence, so check it first before calling.
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        private static List<string> GetValidReadmes(string basePath)
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

        #region Get cacheable data

        private static CacheData GetCacheableDataInFMInstalledDir(FanMission fm)
        {
            AssertR(fm.Installed, "fm.Installed is false when it should be true");

            string fmReadmesBasePath = Path.Combine(Config.GetFMInstallPathUnsafe(fm.Game), fm.InstalledDir);

            return new CacheData(GetValidReadmes(fmReadmesBasePath));
        }

        private static async Task<CacheData> GetCacheableDataInFMCacheDir(FanMission fm, bool refreshCache)
        {
            AssertR(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

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

            string fmArchivePath = FindFMArchive(fm.Archive);

            // In weird situations this could be true, so just say none and at least don't crash
            if (fmArchivePath.IsEmpty()) return new CacheData();

            if (fm.Archive.ExtIsZip())
            {
                ZipExtract(fmArchivePath, fmCachePath, readmes);

                // TODO: Support HTML ref extraction for .7z files too
                // Will require full extract for the same reason scan does - we need to scan files to know what
                // other files to scan, etc. and a full extract is with 99.9999% certainty going to be faster
                // than chugging through the whole thing over and over and over for each new file we find we need

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
                        ExtractHTMLRefFiles(fmArchivePath, fmCachePath);
                    }
                    catch (Exception ex)
                    {
                        Log("Exception in " + nameof(ExtractHTMLRefFiles), ex);
                    }
                }
            }
            else
            {
                await SevenZipExtract(fmArchivePath, fmCachePath, readmes);
            }

            fm.NoReadmes = readmes.Count == 0;

            #endregion

            return new CacheData(readmes);
        }

        #endregion

        #region Extract

        // An html file might have other files it references, so do a recursive search through the archive to find
        // them all, and extract only the required files to the cache. That way we keep the disk footprint way down.
        private static void ExtractHTMLRefFiles(string fmArchivePath, string fmCachePath)
        {
            var htmlRefFiles = new List<NameAndIndex>();

            using var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                ZipArchiveMode.Read, leaveOpen: false);

            foreach (string f in Directory.GetFiles(fmCachePath, "*", SearchOption.AllDirectories))
            {
                if (!f.ExtIsHtml()) continue;

                string html = File.ReadAllText(f);

                for (int i = 0; i < archive.Entries.Count; i++)
                {
                    var e = archive.Entries[i];
                    if (e.Name.IsEmpty() || !e.Name.Contains('.') || _htmlRefExcludes.Any(e.Name.EndsWithI))
                    {
                        continue;
                    }

                    // We just do a dumb string-match search through the whole file. While it's true that HTML
                    // files have their links in specific structures (href tags etc.), we don't attempt to
                    // narrow it down to these because a) we want to future-proof against any new ways to link
                    // that might come about, and b) HTML files can link out to other formats like CSS and
                    // who knows what else, and we don't want to write parsers for every format under the sun.
                    if (html.ContainsI(e.Name) && htmlRefFiles.All(x => x.Index != i))
                    {
                        htmlRefFiles.Add(new NameAndIndex { Index = i, Name = e.FullName });
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

                    var re = archive.Entries[f.Index];

                    // 128k is generous. Any text or markup sort of file should be WELL under that.
                    if (re.Length > 131_072) continue;

                    string content;
                    using (var es = re.Open())
                    {
                        using var sr = new StreamReader(es);
                        content = sr.ReadToEnd();
                    }

                    for (int ei = 0; ei < archive.Entries.Count; ei++)
                    {
                        var e = archive.Entries[ei];
                        if (e.Name.IsEmpty() || !e.Name.Contains('.') || _htmlRefExcludes.Any(e.Name.EndsWithI))
                        {
                            continue;
                        }

                        if (content.ContainsI(e.Name) && htmlRefFiles.All(x => x.Index != ei))
                        {
                            htmlRefFiles.Add(new NameAndIndex { Index = ei, Name = e.FullName });
                        }
                    }
                }
            }

            if (htmlRefFiles.Count > 0)
            {
                foreach (NameAndIndex f in htmlRefFiles)
                {
                    string path = Path.GetDirectoryName(f.Name);
                    if (!path.IsEmpty()) Directory.CreateDirectory(Path.Combine(fmCachePath, path));
                    archive.Entries[f.Index].ExtractToFile(Path.Combine(fmCachePath, f.Name), overwrite: true);
                }
            }
        }

        // We need to block the UI thread one way or another, to prevent starting a zillion parallel tasks that
        // could interfere with each other, especially as they include disk access. Zip extraction, being fast,
        // just blocks by not being async, while the async 7-zip extraction blocks by putting up a progress box.
        private static void ZipExtract(string fmArchivePath, string fmCachePath, List<string> readmes)
        {
            try
            {
                using var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read, leaveOpen: false);

                for (int i = 0; i < archive.Entries.Count; i++)
                {
                    var entry = archive.Entries[i];
                    string fn = entry.FullName;
                    if (!fn.IsValidReadme() || entry.Length == 0) continue;

                    string? t3ReadmeDir = null;
                    int dirSeps = fn.CountDirSeps();
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

                    string fileNameFull = Path.Combine(fmCachePath, fn);
                    entry.ExtractToFile(fileNameFull, overwrite: true);
                    readmes.Add(fn);
                }
            }
            catch (Exception ex)
            {
                Log("Exception in zip extract to cache", ex);
            }
        }

        private static async Task SevenZipExtract(string fmArchivePath, string fmCachePath, List<string> readmes)
        {
            // Critical
            Core.View.ShowOnly();

            await Task.Run(() =>
            {
                try
                {
                    // Block the view immediately after starting another thread, because otherwise we could end
                    // up allowing multiple of these to be called and all that insanity...
                    Core.View.InvokeAsync(new Action(() => Core.View.ShowProgressBox(ProgressTasks.CacheFM)));

                    Directory.CreateDirectory(fmCachePath);

                    using var extractor = new SevenZipExtractor(fmArchivePath);

                    var indexesList = new List<int>();
                    uint extractorFilesCount = extractor.FilesCount;
                    for (int i = 0; i < extractorFilesCount; i++)
                    {
                        var entry = extractor.ArchiveFileData[i];
                        string fn = entry.FileName;
                        int dirSeps;
                        if (entry.FileName.IsValidReadme() && entry.Size > 0 &&
                            (((dirSeps = fn.CountDirSeps()) == 1 &&
                              (fn.PathStartsWithI(_t3ReadmeDir1S) ||
                               fn.PathStartsWithI(_t3ReadmeDir2S))) ||
                             dirSeps == 0))
                        {
                            indexesList.Add(i);
                            readmes.Add(entry.FileName);
                        }
                    }

                    if (indexesList.Count == 0) return;

                    extractor.Extracting += (sender, e) =>
                    {
                        Core.View.InvokeAsync(new Action(() => Core.View.ReportCachingProgress(e.PercentDone)));
                    };

                    extractor.FileExtractionFinished += (sender, e) =>
                    {
                        // This event gets fired for every file, even skipped files. So check if it's actually
                        // one of ours.
                        if (indexesList.Contains(e.FileInfo.Index))
                        {
                            SetFileAttributesFromSevenZipEntry(e.FileInfo, Path.Combine(fmCachePath, e.FileInfo.FileName));
                        }
                    };

                    try
                    {
                        extractor.ExtractFiles(fmCachePath, indexesList.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Log("Exception in 7z ExtractFiles() call", ex);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception in 7z extract to cache", ex);
                }
                finally
                {
                    Core.View.InvokeAsync(new Action(Core.View.HideProgressBox));
                }
            });
        }

        #endregion
    }
}

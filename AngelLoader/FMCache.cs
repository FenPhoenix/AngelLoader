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
        private static readonly string[] ImageFileExtensions =
        {
            ".gif", ".pcx", ".tga", ".dds", ".png", ".bmp", ".jpg", ".jpeg", ".tiff"
        };

        // Try to reject formats that don't make sense. Exclude instead of include for future-proofing.
        private static readonly string[] HTMLRefExcludes =
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

        private static void RemoveEmptyFiles(List<string> files)
        {
            for (int i = 0; i < files.Count; i++)
            {
                if (new FileInfo(files[i]).Length == 0)
                {
                    files.RemoveAt(i);
                    i--;
                }
            }
        }

        #endregion

        #region Get cacheable data

        private static CacheData GetCacheableDataInFMInstalledDir(FanMission fm)
        {
            AssertR(fm.Installed, "fm.Installed is false when it should be true");

            string thisFMInstallsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);

            string path = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);
            var files = FastIO.GetFilesTopOnly(path, "*");
            string t3ReadmePath1 = Path.Combine(path, Paths.T3ReadmeDir1);
            string t3ReadmePath2 = Path.Combine(path, Paths.T3ReadmeDir2);
            if (Directory.Exists(t3ReadmePath1)) files.AddRange(FastIO.GetFilesTopOnly(t3ReadmePath1, "*"));
            if (Directory.Exists(t3ReadmePath2)) files.AddRange(FastIO.GetFilesTopOnly(t3ReadmePath2, "*"));

            RemoveEmptyFiles(files);

            var readmes = new List<string>(files.Count);

            foreach (string f in files)
            {
                if (f.IsValidReadme())
                {
                    readmes.Add(f.Substring(path.Length + 1));
                }
            }

            return new CacheData(readmes);
        }

        private static async Task<CacheData> GetCacheableDataInFMCacheDir(FanMission fm, bool refreshCache)
        {
            var readmes = new List<string>();

            AssertR(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            string fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);

            if (Directory.Exists(fmCachePath))
            {
                foreach (string fn in FastIO.GetFilesTopOnly(fmCachePath, "*"))
                {
                    if (fn.IsValidReadme() && new FileInfo(fn).Length > 0)
                    {
                        readmes.Add(fn.Substring(fmCachePath.Length + 1));
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    string t3ReadmePath = Path.Combine(fmCachePath, i == 0 ? Paths.T3ReadmeDir1 : Paths.T3ReadmeDir2);

                    if (Directory.Exists(t3ReadmePath))
                    {
                        foreach (string fn in FastIO.GetFilesTopOnly(t3ReadmePath, "*"))
                        {
                            if (fn.IsValidReadme() && new FileInfo(fn).Length > 0)
                            {
                                readmes.Add(fn.Substring(fmCachePath.Length + 1));
                            }
                        }
                    }
                }

                bool checkArchive = refreshCache || (readmes.Count == 0 && !fm.NoReadmes);

                if (!checkArchive) return new CacheData(readmes);
            }

            // If cache dir DOESN'T exist, the above checkArchive decision won't be run, so run it here (prevents
            // FMs with no readmes from being reloaded from their archive all the time, which is the whole purpose
            // of NoReadmes in the first place).
            if (!refreshCache && fm.NoReadmes) return new CacheData();

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
                    if (e.Name.IsEmpty() || !e.Name.Contains('.') || HTMLRefExcludes.Any(e.Name.EndsWithI))
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
                    if (HTMLRefExcludes.Any(f.Name.EndsWithI) ||
                        ImageFileExtensions.Any(f.Name.EndsWithI))
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
                        if (e.Name.IsEmpty() || !e.Name.Contains('.') || HTMLRefExcludes.Any(e.Name.EndsWithI))
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
                    if (fn.CountDirSeps() == 1)
                    {
                        if (fn.PathStartsWithI(Paths.T3ReadmeDir1S))
                        {
                            t3ReadmeDir = Paths.T3ReadmeDir1;
                        }
                        else if (fn.PathStartsWithI(Paths.T3ReadmeDir2S))
                        {
                            t3ReadmeDir = Paths.T3ReadmeDir2;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (fn.ContainsDirSep())
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
            Log(nameof(SevenZipExtract) + ": about to show progress box and extract", methodName: false);

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
                        if (entry.FileName.IsValidReadme() && entry.Size > 0 &&
                            ((fn.CountDirSeps() == 1 &&
                              (fn.PathStartsWithI(Paths.T3ReadmeDir1S) ||
                               fn.PathStartsWithI(Paths.T3ReadmeDir2S))) ||
                             !fn.ContainsDirSep()))
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

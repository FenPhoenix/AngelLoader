using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using AngelLoader.Common;
using AngelLoader.Common.DataClasses;
using AngelLoader.Common.Utility;
using AngelLoader.Forms;
using AngelLoader.WinAPI;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.GameSupport;
using static AngelLoader.Common.Logger;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.CustomControls.ProgressPanel;

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
            internal string Name;
            internal int Index = -1;
        }

        // If some files exist but not all that are in the zip, the user can just re-scan for this data by clicking
        // a button, so don't worry about it
        internal static async Task<CacheData> GetCacheableData(FanMission fm, IView view, bool refreshCache)
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
                    : await GetCacheableDataInFMCacheDir(fm, view, refreshCache);
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
            var fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);
            if (!fmCachePath.TrimEnd('\\').EqualsI(Paths.FMsCache.TrimEnd('\\')) && Directory.Exists(fmCachePath))
            {
                try
                {
                    foreach (var f in FastIO.GetFilesTopOnly(fmCachePath, "*"))
                    {
                        File.Delete(f);
                    }

                    foreach (var d in Directory.EnumerateDirectories(fmCachePath, "*", SearchOption.TopDirectoryOnly))
                    {
                        Directory.Delete(d, recursive: true);
                    }
                }
                catch (Exception ex)
                {
                    Log("Exception enumerating files or directories in cache for " + fm.Archive + " / " +
                        fm.InstalledDir, ex);
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
            Debug.Assert(fm.Installed, "fm.Installed is false when it should be true");


            var thisFMInstallsBasePath = Config.GetFMInstallPathUnsafe(fm.Game);

            var path = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);
            var files = FastIO.GetFilesTopOnly(path, "*");
            var t3ReadmePath1 = Path.Combine(path, Paths.T3ReadmeDir1);
            var t3ReadmePath2 = Path.Combine(path, Paths.T3ReadmeDir2);
            if (Directory.Exists(t3ReadmePath1)) files.AddRange(FastIO.GetFilesTopOnly(t3ReadmePath1, "*"));
            if (Directory.Exists(t3ReadmePath2)) files.AddRange(FastIO.GetFilesTopOnly(t3ReadmePath2, "*"));

            RemoveEmptyFiles(files);

            var readmes = new List<string>(files.Count);

            foreach (var f in files)
            {
                if (f.IsValidReadme())
                {
                    readmes.Add(f.Substring(path.Length + 1));
                }
            }

            return new CacheData { Readmes = readmes };
        }

        private static async Task<CacheData> GetCacheableDataInFMCacheDir(FanMission fm, IView view, bool refreshCache)
        {
            var readmes = new List<string>();

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);

            if (Directory.Exists(fmCachePath))
            {
                foreach (var fn in FastIO.GetFilesTopOnly(fmCachePath, "*"))
                {
                    if (fn.IsValidReadme() && new FileInfo(fn).Length > 0)
                    {
                        readmes.Add(fn.Substring(fmCachePath.Length + 1));
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    var t3ReadmePath = Path.Combine(fmCachePath, i == 0 ? Paths.T3ReadmeDir1 : Paths.T3ReadmeDir2);

                    if (Directory.Exists(t3ReadmePath))
                    {
                        foreach (var fn in FastIO.GetFilesTopOnly(t3ReadmePath, "*"))
                        {
                            if (fn.IsValidReadme() && new FileInfo(fn).Length > 0)
                            {
                                readmes.Add(fn.Substring(fmCachePath.Length + 1));
                            }
                        }
                    }
                }

                bool checkArchive = refreshCache || (readmes.Count == 0 && !fm.NoReadmes);

                if (!checkArchive) return new CacheData { Readmes = readmes };
            }

            // If cache dir DOESN'T exist, the above checkArchive decision won't be run, so run it here (prevents
            // FMs with no readmes from being reloaded from their archive all the time, which is the whole purpose
            // of NoReadmes in the first place).
            if (!refreshCache && fm.NoReadmes) return new CacheData();

            readmes.Clear();
            ClearCacheDir(fm);

            var fmArchivePath = FindFMArchive(fm.Archive);

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
                await SevenZipExtract(fmArchivePath, fmCachePath, readmes, view);
            }

            fm.NoReadmes = readmes.Count == 0;

            return new CacheData { Readmes = readmes };
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

            foreach (var f in Directory.EnumerateFiles(fmCachePath, "*", SearchOption.AllDirectories))
            {
                if (!f.ExtIsHtml()) continue;

                var html = File.ReadAllText(f);

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
                for (var ri = 0; ri < htmlRefFiles.Count; ri++)
                {
                    var f = htmlRefFiles[ri];
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

                    for (int eI = 0; eI < archive.Entries.Count; eI++)
                    {
                        var e = archive.Entries[eI];
                        if (e.Name.IsEmpty() || !e.Name.Contains('.') || HTMLRefExcludes.Any(e.Name.EndsWithI))
                        {
                            continue;
                        }

                        if (content.ContainsI(e.Name) && htmlRefFiles.All(x => x.Index != eI))
                        {
                            htmlRefFiles.Add(new NameAndIndex { Index = eI, Name = e.FullName });
                        }
                    }
                }
            }

            if (htmlRefFiles.Count > 0)
            {
                foreach (var f in htmlRefFiles)
                {
                    var path = Path.GetDirectoryName(f.Name);
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

                for (var i = 0; i < archive.Entries.Count; i++)
                {
                    var entry = archive.Entries[i];
                    var fn = entry.FullName;
                    if (!fn.IsValidReadme() || entry.Length == 0) continue;

                    string t3ReadmeDir = null;
                    if (fn.CountChars('/') + fn.CountChars('\\') == 1)
                    {
                        if (fn.StartsWithI(Paths.T3ReadmeDir1 + '/') ||
                            fn.StartsWithI(Paths.T3ReadmeDir1 + '\\'))
                        {
                            t3ReadmeDir = Paths.T3ReadmeDir1;
                        }
                        else if (fn.StartsWithI(Paths.T3ReadmeDir2 + '/') ||
                                 fn.StartsWithI(Paths.T3ReadmeDir2 + '\\'))
                        {
                            t3ReadmeDir = Paths.T3ReadmeDir2;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (fn.Contains('/') || fn.Contains('\\'))
                    {
                        continue;
                    }

                    Directory.CreateDirectory(!t3ReadmeDir.IsEmpty()
                        ? Path.Combine(fmCachePath, t3ReadmeDir)
                        : fmCachePath);

                    var fileNameFull = Path.Combine(fmCachePath, fn);
                    entry.ExtractToFile(fileNameFull, overwrite: true);
                    readmes.Add(fn);
                }
            }
            catch (Exception ex)
            {
                Log("Exception in zip extract to cache", ex);
            }
        }

        private static async Task SevenZipExtract(string fmArchivePath, string fmCachePath, List<string> readmes,
            IView view)
        {
            Log(nameof(SevenZipExtract) + ": about to show progress box and extract", methodName: false);

            // Critical
            view.ShowOnly();

            await Task.Run(() =>
            {
                try
                {
                    // Block the view immediately after starting another thread, because otherwise we could end
                    // up allowing multiple of these to be called and all that insanity...
                    view.InvokeAsync(new Action(() => view.ShowProgressBox(ProgressTasks.CacheFM)));

                    Directory.CreateDirectory(fmCachePath);

                    using var extractor = new SevenZipExtractor(fmArchivePath);

                    var indexesList = new List<int>();
                    for (var i = 0; i < extractor.FilesCount; i++)
                    {
                        var entry = extractor.ArchiveFileData[i];
                        var fn = entry.FileName;
                        if (entry.FileName.IsValidReadme() && entry.Size > 0 &&
                            ((fn.CountChars('/') + fn.CountChars('\\') == 1 &&
                              (fn.StartsWithI(Paths.T3ReadmeDir1 + '/') ||
                               fn.StartsWithI(Paths.T3ReadmeDir1 + '\\') ||
                               fn.StartsWithI(Paths.T3ReadmeDir2 + '/') ||
                               fn.StartsWithI(Paths.T3ReadmeDir2 + '\\'))) ||
                             (!fn.Contains('/') && !fn.Contains('\\'))))
                        {
                            indexesList.Add(i);
                            readmes.Add(entry.FileName);
                        }
                    }

                    if (indexesList.Count == 0) return;


                    extractor.Extracting += (sender, e) =>
                    {
                        view.InvokeAsync(new Action(() => view.ReportCachingProgress(e.PercentDone)));
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
                    view.InvokeAsync(new Action(view.HideProgressBox));
                }
            });
        }

        #endregion
    }
}

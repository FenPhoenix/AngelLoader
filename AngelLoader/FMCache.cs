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
using AngelLoader.CustomControls;
using SevenZip;
using static AngelLoader.Common.Utility.Methods;
using static AngelLoader.Common.Logger;

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

        internal sealed class NameAndIndex
        {
            internal string Name { get; set; }
            internal int Index { get; set; } = -1;
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

        internal static CacheData GetCacheableDataInFMInstalledDir(FanMission fm)
        {
            Debug.Assert(fm.Installed, "fm.Installed is false when it should be true");

            var readmes = new List<string>();

            var thisFMInstallsBasePath = GetFMInstallsBasePath(fm.Game);

            var path = Path.Combine(thisFMInstallsBasePath, fm.InstalledDir);
            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).ToList();
            var t3ReadmePath1 = Path.Combine(path, Paths.T3ReadmeDir1);
            var t3ReadmePath2 = Path.Combine(path, Paths.T3ReadmeDir2);
            if (Directory.Exists(t3ReadmePath1)) files.AddRange(Directory.GetFiles(t3ReadmePath1));
            if (Directory.Exists(t3ReadmePath2)) files.AddRange(Directory.GetFiles(t3ReadmePath2));

            RemoveEmptyFiles(files);

            foreach (var f in files)
            {
                if (f.IsValidReadme()) readmes.Add(f.Substring(path.Length + 1));
            }

            return new CacheData { Readmes = readmes };
        }

        internal static async Task<CacheData> GetCacheableDataInFMCacheDir(FanMission fm, ProgressPanel progressBox)
        {
            var readmes = new List<string>();

            Debug.Assert(!fm.InstalledDir.IsEmpty(), "fm.InstalledFolderName is null or empty");

            var fmCachePath = Path.Combine(Paths.FMsCache, fm.InstalledDir);

            if (Directory.Exists(fmCachePath))
            {
                foreach (var fn in Directory.EnumerateFiles(fmCachePath, "*", SearchOption.TopDirectoryOnly))
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
                        foreach (var fn in Directory.EnumerateFiles(t3ReadmePath, "*", SearchOption.TopDirectoryOnly))
                        {
                            if (fn.IsValidReadme() && new FileInfo(fn).Length > 0)
                            {
                                readmes.Add(fn.Substring(fmCachePath.Length + 1));
                            }
                        }
                    }
                }

                bool checkArchive = fm.RefreshCache || (readmes.Count == 0 && !fm.NoReadmes);

                if (!checkArchive) return new CacheData { Readmes = readmes };
            }

            readmes.Clear();

            var fmArchivePath = FindFMArchive(fm);

            // In weird situations this could be true, so just say none and at least don't crash
            if (fmArchivePath.IsEmpty()) return new CacheData();

            if (fm.Archive.ExtEqualsI(".zip"))
            {
                ZipExtract(fmArchivePath, fmCachePath, readmes);
            }
            else
            {
                await SevenZipExtract(fmArchivePath, fm.InstalledDir, fmCachePath, readmes, progressBox);
            }

            // TODO: Support .7z here too
            if (fmArchivePath.ExtEqualsI(".zip") && Directory.Exists(fmCachePath))
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

            fm.NoReadmes = readmes.Count == 0;

            fm.RefreshCache = false;

            return new CacheData { Readmes = readmes };
        }

        // An html file might have other files it references, so do a recursive search through the archive to find
        // them all, and extract only the required files to the cache. That way we keep the disk footprint way down.
        private static void ExtractHTMLRefFiles(string fmArchivePath, string fmCachePath)
        {
            var htmlRefFiles = new List<NameAndIndex>();

            using (var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                ZipArchiveMode.Read, leaveOpen: false))
            {
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
                        using (var sr = new StreamReader(es))
                        {
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
        }

        // We need to block the UI thread one way or another, to prevent starting a zillion parallel tasks that
        // could interfere with each other, especially as they include disk access. Zip extraction, being fast,
        // just blocks by not being async, while the async 7-zip extraction blocks by putting up a progress box.
        private static void ZipExtract(string fmArchivePath, string fmCachePath, List<string> readmes)
        {
            try
            {
                using (var archive = new ZipArchive(new FileStream(fmArchivePath, FileMode.Open, FileAccess.Read),
                    ZipArchiveMode.Read, leaveOpen: false))
                {
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
            }
            catch (Exception ex)
            {
                Log("Exception in zip extract to cache", ex);
            }
        }

        private static async Task SevenZipExtract(string fmArchivePath, string fmInstalledDir, string fmCachePath,
            List<string> readmes, ProgressPanel progressBox)
        {
            await Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(fmCachePath);

                    using (var extractor = new SevenZipExtractor(fmArchivePath))
                    {
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

                        Log(nameof(SevenZipExtract) + ": about to show progress box and extract", methodName: false);

                        progressBox.BeginInvoke(new Action(progressBox.ShowCachingFM));

                        extractor.Extracting += (sender, e) =>
                        {
                            progressBox.BeginInvoke(new Action(() => progressBox.ReportCachingProgress(e.PercentDone)));
                        };

                        extractor.FileExtractionFinished += (sender, e) =>
                        {
                            SetFileAttributesFromSevenZipEntry(e.FileInfo, Path.Combine(fmCachePath, e.FileInfo.FileName));
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
                }
                catch (Exception ex)
                {
                    Log("Exception in 7z extract to cache", ex);
                }
                finally
                {
                    progressBox.BeginInvoke(new Action(progressBox.HideThis));
                }
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AngelLoader.Common.DataClasses;
using FMScanner;
using SevenZip;
using static AngelLoader.Common.Common;
using static AngelLoader.Common.Logger;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.Common.Utility
{
    internal static class Methods
    {
        internal static string GetDromEdExe(Game game)
        {
            var gameExe = GetGameExeFromGameType(game);
            if (gameExe.IsEmpty()) return "";

            var gamePath = Path.GetDirectoryName(gameExe);
            if (gamePath.IsEmpty()) return "";

            var dromEdExe = Path.Combine(gamePath, Paths.DromEdExe);
            return !gamePath.IsEmpty() && File.Exists(dromEdExe) ? dromEdExe : "";
        }

        internal static void SetFMSizesToLocalized()
        {
            // This will set "KB" / "MB" / "GB" to localized, and decimal separator to current culture
            foreach (var fm in Core.FMsViewList) fm.SizeString = ((long?)fm.SizeBytes).ConvertSize();
        }

        internal static ScanOptions GetDefaultScanOptions()
        {
            return ScanOptions.FalseDefault(
                scanTitle: true,
                scanAuthor: true,
                scanGameType: true,
                scanCustomResources: true,
                scanSize: true,
                scanReleaseDate: true,
                scanTags: true);
        }

        internal static bool FMCustomResourcesScanned(FanMission fm)
        {
            return fm.HasMap != null &&
                   fm.HasAutomap != null &&
                   fm.HasScripts != null &&
                   fm.HasTextures != null &&
                   fm.HasSounds != null &&
                   fm.HasObjects != null &&
                   fm.HasCreatures != null &&
                   fm.HasMotions != null &&
                   fm.HasMovies != null &&
                   fm.HasSubtitles != null;
        }

        internal static bool FMIsReallyInstalled(FanMission fm)
        {
            return fm.Installed &&
                   Directory.Exists(Path.Combine(GetFMInstallsBasePath(fm.Game), fm.InstalledDir));
        }

        internal static bool GameIsRunning(string gameExe, bool checkAllGames = false)
        {
            //Log("Checking if " + gameExe + " is running. Listing processes...");
            Log("Checking if " + gameExe + " is running.");

            // We're doing this whole rigamarole because the game might have been started by someone other than
            // us. Otherwise, we could just persist our process object and then we wouldn't have to do this check.
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    var fn = GetProcessPath(proc.Id);
                    //Log.Info("Process filename: " + fn);
                    if (!fn.IsEmpty())
                    {
                        var fnb = fn.ToBackSlashes();
                        if ((checkAllGames &&
                             ((!Config.T1Exe.IsEmpty() && fnb.EqualsI(Config.T1Exe.ToBackSlashes())) ||
                              (!Config.T2Exe.IsEmpty() && fnb.EqualsI(Config.T2Exe.ToBackSlashes())) ||
                              (!Config.T3Exe.IsEmpty() && fnb.EqualsI(Config.T3Exe.ToBackSlashes())))) ||
                            (!checkAllGames &&
                             (!gameExe.IsEmpty() && fnb.EqualsI(gameExe.ToBackSlashes()))))
                        {
                            var logExe = checkAllGames ? "a game exe" : gameExe;

                            Log("Found " + logExe + " running: " + fn +
                                "\r\nReturning true, game should be blocked from starting");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Even if this were to be one of our games, if .NET won't let us find out then all we can do
                    // is shrug and move on.
                    Log("Exception caught in GameIsRunning", ex);
                }
            }

            return false;
        }

        internal static string GetGameNameFromGameType(Game gameType)
        {
            return
                gameType == Game.Thief1 ? "Thief 1" :
                gameType == Game.Thief2 ? "Thief 2" :
                gameType == Game.Thief3 ? "Thief 3" :
                "[UnknownGameType]";
        }

        internal static string GetGameExeFromGameType(Game gameType)
        {
            return
                gameType == Game.Thief1 ? Config.T1Exe :
                gameType == Game.Thief2 ? Config.T2Exe :
                gameType == Game.Thief3 ? Config.T3Exe :
                null;
        }

        internal static string GetProcessPath(int procId)
        {
            using (var hProc = OpenProcess(ProcessAccessFlags.QueryLimitedInformation, false, procId))
            {
                if (!hProc.IsInvalid)
                {
                    var buffer = new StringBuilder(1024);
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hProc, 0, buffer, ref size)) return buffer.ToString();
                }
            }
            return "";
        }

        internal static string GetFMInstallsBasePath(Game? game)
        {
            var thisFMInstallsBasePath =
                game == Game.Thief1 ? Config.T1FMInstallPath :
                game == Game.Thief2 ? Config.T2FMInstallPath :
                game == Game.Thief3 ? Config.T3FMInstallPath :
                null;

            return thisFMInstallsBasePath ?? "";
        }

        /// <summary>
        /// Returns the list of FM archive paths, returning subfolders as well if that option is enabled.
        /// </summary>
        /// <returns></returns>
        internal static List<string> GetFMArchivePaths()
        {
            var paths = new List<string>();
            foreach (var path in Config.FMArchivePaths)
            {
                paths.Add(path);
                if (Config.FMArchivePathsIncludeSubfolders)
                {
                    try
                    {
                        foreach (var dir in Directory.GetDirectories(path, "*", SearchOption.AllDirectories))
                        {
                            if (!dir.GetDirNameFast().EqualsI(".fix") &&
                                !dir.ContainsI(Path.DirectorySeparatorChar + ".fix" + Path.DirectorySeparatorChar))
                            {
                                paths.Add(dir);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log(nameof(GetFMArchivePaths) + " : subfolders=true, Exception in GetDirectories", ex);
                    }
                }
            }

            return paths;
        }

        internal static string FindFMArchive(FanMission fm, List<string> archivePaths = null)
        {
            if (fm.Archive.IsEmpty()) return null;

            foreach (var path in (archivePaths != null && archivePaths.Count > 0 ? archivePaths : GetFMArchivePaths()))
            {
                var f = Path.Combine(path, fm.Archive);
                if (File.Exists(f)) return f;
            }

            return null;
        }

        internal static void UnSetReadOnly(string fileOnDiskFullPath)
        {
            try
            {
                new FileInfo(fileOnDiskFullPath).IsReadOnly = false;
            }
            catch (Exception ex)
            {
                Log("Unable to set file attributes for " + fileOnDiskFullPath, ex);
            }
        }

        internal static void SetFileAttributesFromSevenZipEntry(ArchiveFileInfo archiveFileInfo, string fileOnDiskFullPath)
        {
            // ExtractFile() doesn't set these, so we have to set them ourselves.
            // ExtractArchive() sets them though, so we don't need to call this when using that.
            try
            {
                _ = new FileInfo(fileOnDiskFullPath)
                {
                    IsReadOnly = false,
                    LastWriteTime = archiveFileInfo.LastWriteTime,
                    CreationTime = archiveFileInfo.CreationTime,
                    LastAccessTime = archiveFileInfo.LastAccessTime
                };
            }
            catch (Exception ex)
            {
                Log("Unable to set file attributes for " + fileOnDiskFullPath, ex);
            }
        }

        internal static bool GameIsDark(FanMission fm) => fm.Game == Game.Thief1 || fm.Game == Game.Thief2;

        internal static bool GameIsDark(Game? game) => game == Game.Thief1 || game == Game.Thief2;

        internal static bool GameIsKnownAndSupported(FanMission fm) => fm.Game != null && fm.Game != Game.Unsupported;

        internal static bool GameIsKnownAndSupported(Game? game) => game != null && game != Game.Unsupported;

        // Update fm.TagsString here. We keep TagsString around because when we're reading, writing, and merging
        // FMs, we don't want to spend time converting back and forth. So Tags is session-only, and only gets
        // filled out for FMs that will be displayed. TagsString is the one that gets saved and loaded, and must
        // be kept in sync with Tags. This should ONLY be called when a tag is added or removed. Keep it simple
        // so we can see and follow the logic.
        internal static void UpdateFMTagsString(FanMission fm)
        {
            var intermediateList = new List<string>();
            foreach (var item in fm.Tags)
            {
                if (item.Tags.Count == 0)
                {
                    intermediateList.Add(item.Category);
                }
                else
                {
                    foreach (var tag in item.Tags)
                    {
                        intermediateList.Add(item.Category + ":" + tag);
                    }
                }
            }

            string finalValue = "";
            for (int i = 0; i < intermediateList.Count; i++)
            {
                if (i > 0) finalValue += ",";
                finalValue += intermediateList[i];
            }

            fm.TagsString = finalValue;
        }

        // Very awkward procedure that accesses global state in the name of only doing one iteration
        // TODO: Test perf when 1000+ FMs each have a bunch of tags
        internal static void AddTagsToFMAndGlobalList(string tagsToAdd, List<CatAndTags> existingFMTags)
        {
            if (tagsToAdd.IsEmpty()) return;

            var tagsArray = tagsToAdd.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in tagsArray)
            {
                string cat, tag;

                var colonCount = item.CountChars(':');

                // No way josé
                if (colonCount > 1) continue;

                if (colonCount == 1)
                {
                    var index = item.IndexOf(':');
                    cat = item.Substring(0, index).Trim().ToLowerInvariant();
                    tag = item.Substring(index + 1).Trim();
                    if (cat.IsEmpty() || tag.IsEmpty()) continue;
                }
                else
                {
                    cat = "misc";
                    tag = item.Trim();
                }

                // Note: We've already converted cat to lowercase, so we just do straight == to shave time off

                #region FM tags

                CatAndTags match = null;
                for (int i = 0; i < existingFMTags.Count; i++)
                {
                    if (existingFMTags[i].Category == cat) match = existingFMTags[i];
                }
                if (match == null)
                {
                    existingFMTags.Add(new CatAndTags { Category = cat });
                    existingFMTags[existingFMTags.Count - 1].Tags.Add(tag);
                }
                else
                {
                    if (!match.Tags.ContainsI(tag)) match.Tags.Add(tag);
                }

                #endregion

                #region Global tags

                GlobalCatAndTags globalMatch = null;
                for (int i = 0; i < GlobalTags.Count; i++)
                {
                    if (GlobalTags[i].Category.Name == cat) globalMatch = GlobalTags[i];
                }
                if (globalMatch == null)
                {
                    GlobalTags.Add(new GlobalCatAndTags { Category = new GlobalCatOrTag { Name = cat, UsedCount = 1 } });
                    GlobalTags[GlobalTags.Count - 1].Tags.Add(new GlobalCatOrTag { Name = tag, UsedCount = 1 });
                }
                else
                {
                    globalMatch.Category.UsedCount++;

                    var ft = FirstTagOrNull(globalMatch.Tags, tag);
                    if (ft == null)
                    {
                        globalMatch.Tags.Add(new GlobalCatOrTag { Name = tag, UsedCount = 1 });
                    }
                    else
                    {
                        ft.UsedCount++;
                    }
                }

                #endregion
            }
        }

        // Avoid the overhead of FirstOrDefault()
        private static GlobalCatOrTag FirstTagOrNull(List<GlobalCatOrTag> tagsList, string tag)
        {
            for (int i = 0; i < tagsList.Count; i++)
            {
                if (tagsList[i].Name.EqualsI(tag)) return tagsList[i];
            }

            return null;
        }

        // Break the frigging references!! Arrrrrrrgh!
        private static void DeepCopyCatAndTagsList(List<CatAndTags> source, List<CatAndTags> dest)
        {
            dest.Clear();

            if (source.Count == 0) return;

            foreach (var catAndTag in source)
            {
                var item = new CatAndTags { Category = catAndTag.Category };
                foreach (var tag in catAndTag.Tags) item.Tags.Add(tag);
                dest.Add(item);
            }
        }

        internal static void DeepCopyTagsFilter(TagsFilter source, TagsFilter dest)
        {
            DeepCopyCatAndTagsList(source.AndTags, dest.AndTags);
            DeepCopyCatAndTagsList(source.OrTags, dest.OrTags);
            DeepCopyCatAndTagsList(source.NotTags, dest.NotTags);
        }

        internal static void DeepCopyGlobalTags(List<GlobalCatAndTags> source, List<GlobalCatAndTags> dest)
        {
            dest.Clear();

            if (source.Count == 0) return;

            foreach (var catAndTag in source)
            {
                var item = new GlobalCatAndTags
                {
                    Category = new GlobalCatOrTag
                    {
                        Name = catAndTag.Category.Name,
                        IsPreset = catAndTag.Category.IsPreset,
                        UsedCount = catAndTag.Category.UsedCount
                    }
                };
                foreach (var tag in catAndTag.Tags)
                {
                    item.Tags.Add(new GlobalCatOrTag
                    {
                        Name = tag.Name,
                        IsPreset = tag.IsPreset,
                        UsedCount = tag.UsedCount
                    });
                }

                dest.Add(item);
            }
        }
    }
}

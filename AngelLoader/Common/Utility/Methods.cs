using System;
using System.Collections.Generic;
using System.IO;
using AngelLoader.Common.DataClasses;
using SevenZip;
using static AngelLoader.Common.Common;

namespace AngelLoader.Common.Utility
{
    internal static class Methods
    {
        internal static string GetFMInstallsBasePath(FanMission fm)
        {
            var thisFMInstallsBasePath =
                fm.Game == Game.Thief1 ? Config.T1FMInstallPath :
                fm.Game == Game.Thief2 ? Config.T2FMInstallPath :
                fm.Game == Game.Thief3 ? Config.T3FMInstallPath :
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
                    paths.AddRange(Directory.GetDirectories(path, "*", SearchOption.AllDirectories));
                }
            }

            return paths;
        }

        internal static string FindFMArchive(FanMission fm)
        {
            if (fm.Archive.IsEmpty()) return null;

            foreach (var path in GetFMArchivePaths())
            {
                var f = Path.Combine(path, fm.Archive);
                if (File.Exists(f)) return f;
            }

            return null;
        }

        internal static void UnSetReadOnly(string fileOnDiskFullPath)
        {
            // FileAttributes.Normal: prevents files from being readonly
            var fi = new FileInfo(fileOnDiskFullPath) { Attributes = FileAttributes.Normal };
        }

        internal static void SetFileAttributesFromSevenZipEntry(ArchiveFileInfo archiveFileInfo, string fileOnDiskFullPath)
        {
            // ExtractFile() doesn't set these, so we have to set them ourselves.
            // ExtractArchive() sets them though, so we don't need to call this when using that.
            try
            {
                var fi = new FileInfo(fileOnDiskFullPath)
                {
                    LastWriteTime = archiveFileInfo.LastWriteTime,
                    CreationTime = archiveFileInfo.CreationTime,
                    // Set this one to prevent files from being readonly
                    Attributes = FileAttributes.Normal,
                    LastAccessTime = archiveFileInfo.LastAccessTime
                };
            }
            catch (Exception ex)
            {
                // log it
            }
        }

        internal static bool GameIsDark(FanMission fm) => fm.Game == Game.Thief1 || fm.Game == Game.Thief2;

        internal static bool GameIsKnownAndSupported(FanMission fm) => fm.Game != null && fm.Game != Game.Unsupported;

        internal static (bool IsNull, bool IsSupported) GameIsKnownAndSupportedReportIfNull(FanMission fm)
        {
            return fm.Game == null ? (true, false) : fm.Game == Game.Unsupported ? (false, false) : (false, true);
        }

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

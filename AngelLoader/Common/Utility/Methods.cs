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

        internal static void SetFileAttributesFromZipEntry(ArchiveFileInfo archiveFileInfo, string fileOnDiskFullPath)
        {
            // ExtractFile() doesn't set these, so we have to set them ourselves.
            // ExtractArchive() sets them though, so we don't need to call this when using that.
            try
            {
                var fi = new FileInfo(fileOnDiskFullPath)
                {
                    LastWriteTime = archiveFileInfo.LastWriteTime,
                    CreationTime = archiveFileInfo.CreationTime,
                    // Set this one to prevent files being readonly
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

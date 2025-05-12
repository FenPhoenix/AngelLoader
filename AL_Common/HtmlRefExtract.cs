using System.Collections.Generic;
using System.IO;

namespace AL_Common;

public static partial class Common
{
    // Try to reject formats that don't make sense. Exclude instead of include for future-proofing.
    public static readonly string[] HtmlRefExcludes =
    {
        // Mission files
        ".cbt",
        ".gam",
        ".gmp",
        ".ibt",
        ".mis",
        ".ned",
        ".unr",

        // Misc
        ".bin",
        ".db",
        ".dll",
        ".dlx",
        ".exe",
        ".ose",
        ".osm",
        ".mc",
        ".mi",
        ".log",
        ".str",
        ".nut",
        ".obj",

        // Audio
        ".aif",
        ".aiff",
        ".flac",
        ".mp1",
        ".mp2",
        ".mp3",
        ".oga",
        ".ogg",
        ".opus",
        ".pcm",
        ".w64",
        ".wav",

        // Video
        ".avi",
        ".flv",
        ".mkv",
        ".mov",
        ".mp4",
        ".ogv",
        ".webm",
        ".wmf",
    };

    public static bool IsExcludedFileType(string[] excludes, string name)
    {
        foreach (string item in excludes)
        {
            if (name.EndsWithI(item)) return true;
        }
        return false;
    }

    public static bool HtmlNeedsReferenceExtract(string[] cacheFiles, List<string> archiveFileNamesNameOnly)
    {
        foreach (string cacheFile in cacheFiles)
        {
            if (!cacheFile.ExtIsHtml()) continue;

            // @FileStreamNET: Implicit use of FileStream
            string content = File.ReadAllText(cacheFile);

            for (int i = 0; i < archiveFileNamesNameOnly.Count; i++)
            {
                string name = archiveFileNamesNameOnly[i];
                if (!name.IsEmpty() &&
                    !name.EndsWithDirSep() &&
                    name.Contains('.') &&
                    !IsExcludedFileType(HtmlRefExcludes, name) &&
                    content.ContainsI(name))
                {
                    return true;
                }
            }
        }

        return false;
    }
}

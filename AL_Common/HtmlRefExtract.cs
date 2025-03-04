using System.Collections.Generic;
using System.IO;

namespace AL_Common;

public static partial class Common
{
    // Try to reject formats that don't make sense. Exclude instead of include for future-proofing.
    public static readonly string[] HtmlRefExcludes =
    {
        ".osm", ".exe", ".dll", ".ose", ".mis", ".gam", ".ibt", ".cbt", ".gmp", ".ned", ".unr", ".wav",
        ".mp3", ".ogg", ".aiff", ".aif", ".flac", ".bin", ".dlx", ".mc", ".mi", ".avi", ".mp4", ".mkv",
        ".flv", ".log", ".str", ".nut", ".db", ".obj",
    };

    private static bool IsExcludedFileType(string name)
    {
        foreach (string item in HtmlRefExcludes)
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
                    !IsExcludedFileType(name) &&
                    content.ContainsI(name))
                {
                    return true;
                }
            }
        }

        return false;
    }
}

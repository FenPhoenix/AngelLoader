using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                    !HtmlRefExcludes.Any(name.EndsWithI) &&
                    content.ContainsI(name))
                {
                    return true;
                }
            }
        }

        return false;
    }
}

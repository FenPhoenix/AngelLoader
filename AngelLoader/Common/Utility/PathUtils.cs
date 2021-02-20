using System;
using System.IO;
using AL_Common;
using static AL_Common.CommonUtils;

namespace AngelLoader
{
    public static partial class Misc
    {
        internal static bool PathIsRelative(string path) =>
            path.Length > 1 && path[0] == '.' &&
            (path[1].IsDirSep() || (path[1] == '.' && path.Length > 2 && path[2].IsDirSep()));

        internal static string RelativeToAbsolute(string basePath, string relativePath)
        {
            AssertR(!basePath.IsEmpty(), "basePath is null or empty");

            return relativePath.IsEmpty() ? basePath : Path.GetFullPath(Path.Combine(basePath, relativePath));
        }

        #region Forward/backslash conversion

        internal static string ToForwardSlashes(this string value) => value.Replace('\\', '/');

        internal static string ToBackSlashes(this string value) => value.Replace('/', '\\');

        internal static string ToSystemDirSeps(this string value) => value.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

        #endregion

        #region Get file / dir names

        /// <summary>
        /// Strips the leading path from the filename, taking into account both / and \ chars.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetFileNameFast(this string path)
        {
            int i1 = path.LastIndexOf('\\');
            int i2 = path.LastIndexOf('/');

            return i1 == -1 && i2 == -1 ? path : path.Substring(Math.Max(i1, i2) + 1);
        }

        internal static string GetDirNameFast(this string path) => GetFileNameFast(path.TrimEnd(CA_BS_FS));

        #endregion

        #region File extensions

        #region Filename extension checks

        internal static bool IsValidReadme(this string value)
        {
            // Well, this is embarrassing... Apparently EndsWithI is faster than the baked-in ones.
            // Dunno how that could be the case, but whatever...
            return value.EndsWithI(".txt") ||
                   value.EndsWithI(".rtf") ||
                   value.EndsWithI(".wri") ||
                   value.EndsWithI(".glml") ||
                   value.EndsWithI(".html") ||
                   value.EndsWithI(".htm");
        }

        #region Baked-in extension checks
        // TODO: Just passthroughs now because EndsWithI turned out to be faster(?!)

        internal static bool ExtIsTxt(this string value) => value.EndsWithI(".txt");

        internal static bool ExtIsRtf(this string value) => value.EndsWithI(".rtf");

        internal static bool ExtIsWri(this string value) => value.EndsWithI(".wri");

        internal static bool ExtIsHtml(this string value) => value.EndsWithI(".html") || value.EndsWithI(".htm");

        internal static bool ExtIsGlml(this string value) => value.EndsWithI(".glml");

        internal static bool ExtIsArchive(this string value) => value.EndsWithI(".zip") || value.EndsWithI(".7z");

        internal static bool ExtIsZip(this string value) => value.EndsWithI(".zip");

        internal static bool ExtIs7z(this string value) => value.EndsWithI(".7z");

        #endregion

        #endregion

        #endregion

        #region Try combine path and check existence

        internal static bool TryCombineFilePathAndCheckExistence(
            string pathPart1,
            string pathPart2,
            out string combinedPath)
        {
            try
            {
                string ret = Path.Combine(pathPart1, pathPart2);
                if (File.Exists(ret))
                {
                    combinedPath = ret;
                    return true;
                }
                else
                {
                    combinedPath = "";
                    return false;
                }
            }
            catch
            {
                combinedPath = "";
                return false;
            }
        }

        internal static bool TryCombineFilePathAndCheckExistence(
            string pathPart1,
            string pathPart2,
            string pathPart3,
            out string combinedPath)
        {
            try
            {
                string ret = Path.Combine(pathPart1, pathPart2, pathPart3);
                if (File.Exists(ret))
                {
                    combinedPath = ret;
                    return true;
                }
                else
                {
                    combinedPath = "";
                    return false;
                }
            }
            catch
            {
                combinedPath = "";
                return false;
            }
        }

        #region Disabled until needed

        /*
        internal static bool TryCombineFilePathAndCheckExistence(string[] pathParts, out string combinedPath)
        {
            try
            {
                string ret = Path.Combine(pathParts);
                if (File.Exists(ret))
                {
                    combinedPath = ret;
                    return true;
                }
                else
                {
                    combinedPath = "";
                    return false;
                }
            }
            catch
            {
                combinedPath = "";
                return false;
            }
        }

        */

        #endregion

        #endregion
    }
}

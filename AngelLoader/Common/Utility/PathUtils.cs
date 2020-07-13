using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static System.StringComparison;

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

        // Note: We hardcode '/' and '\' for now because we can get paths from archive files too, where the dir
        // sep chars are in no way guaranteed to match those of the OS.
        // Not like any OS is likely to use anything other than '/' or '\' anyway.

        // We hope not to have to call this too often, but it's here as a fallback.
        private static string CanonicalizePath(string value) => value.Replace('/', '\\');

        #region Contains / count / find

        internal static bool PathContainsI_Dir(this List<string> value, string substring)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].PathEqualsI_Dir(substring)) return true;
            return false;
        }

        internal static bool PathContainsI_Dir(this string[] value, string substring)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].PathEqualsI_Dir(substring)) return true;
            return false;
        }

        internal static bool PathContainsI(this List<string> value, string substring)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].PathEqualsI(substring)) return true;
            return false;
        }

        internal static bool PathContainsI(this string[] value, string substring)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].PathEqualsI(substring)) return true;
            return false;
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> contains either directory separator character.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool ContainsDirSep(this string value)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].IsDirSep()) return true;
            return false;
        }

        /// <summary>
        /// Counts the total occurrences of both directory separator characters in <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        internal static int CountDirSeps(this string value, int start = 0)
        {
            int count = 0;
            for (int i = start; i < value.Length; i++) if (value[i].IsDirSep()) count++;
            return count;
        }

        /// <summary>
        /// Counts dir seps up to <paramref name="count"/> occurrences and then returns, skipping further counting.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        internal static bool DirSepCountIsAtLeast(this string value, int count, int start = 0)
        {
            int foundCount = 0;
            for (int i = start; i < value.Length; i++)
            {
                if (value[i].IsDirSep()) foundCount++;
                if (foundCount == count) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the last index of either directory separator character in <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int LastIndexOfDirSep(this string value)
        {
            int i1 = value.LastIndexOf('/');
            int i2 = value.LastIndexOf('\\');

            return i1 == -1 && i2 == -1 ? -1 : Math.Max(i1, i2);
        }

        #endregion

        #region Equality / StartsWith / EndsWith

        internal static bool PathSequenceEqualI_Dir(this IList<string> first, IList<string> second)
        {
            int firstCount;
            if ((firstCount = first.Count) != second.Count) return false;

            for (int i = 0; i < firstCount; i++) if (!first[i].PathEqualsI_Dir(second[i])) return false;
            return true;
        }

        internal static bool PathSequenceEqualI(this IList<string> first, IList<string> second)
        {
            int firstCount;
            if ((firstCount = first.Count) != second.Count) return false;

            for (int i = 0; i < firstCount; i++) if (!first[i].PathEqualsI(second[i])) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool PathCharsConsideredEqual_Win(char char1, char char2) =>
            char1.EqualsIAscii(char2) ||
            (char1.IsDirSep() && char2.IsDirSep());

        /// <summary>
        /// Path equality check ignoring case and directory separator differences. Directory version: Ignores
        /// trailing path separators.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool PathEqualsI_Dir(this string first, string second) => first.TrimEnd(CA_BS_FS).PathEqualsI(second.TrimEnd(CA_BS_FS));

        /// <summary>
        /// Path equality check ignoring case and directory separator differences.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool PathEqualsI(this string first, string second)
        {
            if (first == second) return true;

            int firstLen = first.Length;
            if (firstLen != second.Length) return false;

            for (int i = 0; i < firstLen; i++)
            {
                char fc = first[i];
                char sc = second[i];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.Equals(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).Equals(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (!PathCharsConsideredEqual_Win(fc, sc)) return false;
            }

            return true;
        }

        /// <summary>
        /// Path starts-with check ignoring case and directory separator differences.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool PathStartsWithI(this string first, string second)
        {
            int secondLength;
            if (first == null || first.Length < (secondLength = second.Length)) return false;

            for (int i = 0; i < secondLength; i++)
            {
                char fc = first[i];
                char sc = second[i];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.StartsWith(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).StartsWith(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (!PathCharsConsideredEqual_Win(fc, sc)) return false;
            }

            return true;
        }

        /// <summary>
        /// Path ends-with check ignoring case and directory separator differences.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool PathEndsWithI(this string first, string second)
        {
            int firstLength, secondLength;
            if (first == null || (firstLength = first.Length) < (secondLength = second.Length)) return false;

            for (int fi = firstLength - secondLength, si = 0; fi < firstLength; fi++, si++)
            {
                char fc = first[fi];
                char sc = second[si];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.EndsWith(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).EndsWith(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (!PathCharsConsideredEqual_Win(fc, sc)) return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDirSep(this char character) => character == '/' || character == '\\';

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool StartsWithDirSep(this string value) => value.Length > 0 && value[0].IsDirSep();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EndsWithDirSep(this string value) => value.Length > 0 && value[value.Length - 1].IsDirSep();

        #endregion

        #region File extensions

        /// <summary>
        /// Just removes the extension from a filename, without the rather large overhead of
        /// Path.GetFileNameWithoutExtension().
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string RemoveExtension(this string fileName)
        {
            int i;
            return (i = fileName.LastIndexOf('.')) == -1 ? fileName : fileName.Substring(0, i);
        }

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

        internal static bool TryCombineFilePathAndCheckExistence(string pathPart1, string pathPart2,
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

        internal static bool TryCombineFilePathAndCheckExistence(string pathPart1, string pathPart2, string pathPart3,
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

        #endregion
    }
}

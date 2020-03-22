/*
FMScanner - A fast, thorough, accurate scanner for Thief 1 and Thief 2 fan missions.

Written in 2017-2020 by FenPhoenix.

To the extent possible under law, the author(s) have dedicated all copyright and related and neighboring rights
to this software to the public domain worldwide. This software is distributed without any warranty.

You should have received a copy of the CC0 Public Domain Dedication along with this software.
If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using static System.StringComparison;
using static FMScanner.Regexes;

namespace FMScanner
{
    internal static class Utility
    {
        #region Extensions

        #region Queries

        /// <summary>
        /// Returns a value between 0 and 1.0 that indicates how similar the two strings are.
        /// </summary>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        internal static double SimilarityTo(this string string1, string string2, StringComparison stringComparison)
        {
            if (string1.Equals(string2, stringComparison)) return 1.0;
            if (string1.Length == 0) return 0;
            if (string2.Length == 0) return 0;

            int[] vec1 = new int[string2.Length + 1];
            int[] vec2 = new int[string2.Length + 1];

            for (int i = 0; i < vec1.Length; i++) vec1[i] = i;

            for (int i = 0; i < string1.Length; i++)
            {
                vec2[0] = i + 1;

                for (int j = 0; j < string2.Length; j++)
                {
                    int delCost = vec1[j + 1] + 1;
                    int insCost = vec2[j] + 1;
                    int substCost = string1[i].ToString().Equals(string2[j].ToString(), stringComparison) ? 0 : 1;
                    vec2[j + 1] = Math.Min(insCost, Math.Min(delCost, vec1[j] + substCost));
                }

                Array.Copy(vec2, vec1, vec1.Length);
            }

            return 1.0 - ((double)vec2[string2.Length] / Math.Max(string1.Length, string2.Length));
        }

        internal static bool IsEnglishReadme(this string value)
        {
            int dotIndex = value.IndexOf('.');
            return dotIndex > -1 &&
                   ((dotIndex == 9 && value.StartsWithI("fminfo-en")) ||
                    (dotIndex == 10 && value.StartsWithI("fminfo-eng")) ||
                    !(dotIndex > 6 && value.StartsWithI("fminfo")));
        }

        /// <summary>
        /// Returns the number of times a character appears in a string.
        /// Avoids whatever silly overhead junk Count(predicate) is doing.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        internal static int CountChars(this string value, char character)
        {
            int count = 0;
            for (int i = 0; i < value.Length; i++) if (value[i] == character) count++;

            return count;
        }

        #region Fast byte[] / char[] search

        // I don't know if this is "supposed" to be the fastest way, but every other algorithm I've tried is at
        // least 2-8x slower. IndexOf() calls an internal method TrySZIndexOf() which is obviously some voodoo
        // speed demon stuff because none of this Moyer-Bohr-Kensington-Smythe-Wappcapplet fancy stuff beats it.
        // Or maybe I just don't know what I'm doing. Either way.
        internal static bool Contains(this byte[] input, byte[] pattern)
        {
            byte firstByte = pattern[0];
            int index = Array.IndexOf(input, firstByte);

            while (index > -1)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (index + i >= input.Length) return false;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return false;
                        break;
                    }

                    if (i == pattern.Length - 1) return true;
                }
            }

            return index > -1;
        }

        #endregion

        #region Contains

        internal static bool Contains(this string value, char character) => value.IndexOf(character) >= 0;

        /// <summary>
        /// Determines whether a string contains a specified substring. Uses
        /// <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        internal static bool ContainsI(this string value, string substring) => value.IndexOf(substring, OrdinalIgnoreCase) >= 0;

        /// <summary>
        /// Determines whether a List&lt;string&gt; contains a specified element. Uses 
        /// <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        internal static bool ContainsI(this string[] value, string substring)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].Equals(substring, OrdinalIgnoreCase)) return true;
            return false;

        }

        /// <summary>
        /// Determines whether a string[] contains a specified element. Uses 
        /// <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        internal static bool ContainsI(this List<string> value, string substring)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].Equals(substring, OrdinalIgnoreCase)) return true;
            return false;
        }

        #endregion

        /// <summary>
        /// Determines whether this string and a specified <see langword="string"/> object have the same value.
        /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool EqualsI(this string first, string second) => first.Equals(second, OrdinalIgnoreCase);

        #region Path-specific string queries (separator-agnostic)

        // Note: We hardcode '/' and '\' for now because we can get paths from archive files too, where the dir
        // sep chars are in no way guaranteed to match those of the OS.
        // Not like any OS is likely to use anything other than '/' or '\' anyway.

        // We hope not to have to call this too often, but it's here as a fallback.
        private static string CanonicalizePath(string value) => value.Replace('/', '\\');

        /// <summary>
        /// Returns true if <paramref name="value"/> contains either directory separator character.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool ContainsDirSep(this string value)
        {
            for (int i = 0; i < value.Length; i++) if (value[i] == '/' || value[i] == '\\') return true;
            return false;
        }

        /// <summary>
        /// Counts the total occurrences of both directory separator characters in <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int CountDirSeps(this string value)
        {
            int count = 0;
            for (int i = 0; i < value.Length; i++) if (value[i] == '/' || value[i] == '\\') count++;
            return count;
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

            if (i1 == -1 && i2 == -1) return -1;

            return Math.Max(i1, i2);
        }

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

                if (fc == sc ||
                    ((fc == '\\' || fc == '/') &&
                    (sc == '\\' || sc == '/')) ||
                    ((fc >= 65 && fc <= 90 && sc >= 97 && sc <= 122 && fc == sc - 32) ||
                    (fc >= 97 && fc <= 122 && sc >= 65 && sc <= 90 && fc == sc + 32)))
                {
                    continue;
                }

                return false;
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
            if (first == null || first.Length < second.Length) return false;

            for (int i = 0; i < second.Length; i++)
            {
                char fc = first[i];
                char sc = second[i];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.StartsWith(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).StartsWith(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (fc == sc ||
                    ((fc == '\\' || fc == '/') &&
                     (sc == '\\' || sc == '/')) ||
                    ((fc >= 65 && fc <= 90 && sc >= 97 && sc <= 122 && fc == sc - 32) ||
                     (fc >= 97 && fc <= 122 && sc >= 65 && sc <= 90 && fc == sc + 32)))
                {
                    continue;
                }

                return false;
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
            if (first == null || first.Length < second.Length) return false;

            for (int fi = first.Length - second.Length, si = 0; fi < first.Length; fi++, si++)
            {
                char fc = first[fi];
                char sc = second[si];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.EndsWith(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).EndsWith(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (fc == sc ||
                    ((fc == '\\' || fc == '/') &&
                     (sc == '\\' || sc == '/')) ||
                    ((fc >= 65 && fc <= 90 && sc >= 97 && sc <= 122 && fc == sc - 32) ||
                     (fc >= 97 && fc <= 122 && sc >= 65 && sc <= 90 && fc == sc + 32)))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        #endregion

        #region File extensions

        /// <summary>
        /// Determines whether this string ends with a file extension. Obviously only makes sense for strings
        /// that are supposed to be file names.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool HasFileExtension(this string value)
        {
            int lastDotIndex = value.LastIndexOf('.');
            return lastDotIndex > value.LastIndexOf('/') ||
                   lastDotIndex > value.LastIndexOf('\\');
        }

        internal static bool IsValidReadme(this string readme, bool englishOnly = false)
        {
            return (readme.ExtIsTxt() ||
                    readme.ExtIsRtf() ||
                    readme.ExtIsWri() ||
                    readme.ExtIsGlml() ||
                    readme.ExtIsHtml()) &&
                   (!englishOnly || readme.IsEnglishReadme());
        }

        #region Baked-in extension checks (generated)

        // TODO: These may actually be slower... just so I remember in case I wanna change them back
        private static bool ExtIsTxt(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'T' || value[len - 3] == 't') &&
                   (value[len - 2] == 'X' || value[len - 2] == 'x') &&
                   (value[len - 1] == 'T' || value[len - 1] == 't');
        }

        private static bool ExtIsRtf(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'R' || value[len - 3] == 'r') &&
                   (value[len - 2] == 'T' || value[len - 2] == 't') &&
                   (value[len - 1] == 'F' || value[len - 1] == 'f');
        }

        private static bool ExtIsWri(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'W' || value[len - 3] == 'w') &&
                   (value[len - 2] == 'R' || value[len - 2] == 'r') &&
                   (value[len - 1] == 'I' || value[len - 1] == 'i');
        }

        internal static bool ExtIsHtml(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return (len > 4 &&
                    value[len - 4] == '.' &&
                    (value[len - 3] == 'H' || value[len - 3] == 'h') &&
                    (value[len - 2] == 'T' || value[len - 2] == 't') &&
                    (value[len - 1] == 'M' || value[len - 1] == 'm')) ||
                   (len > 5 &&
                    value[len - 5] == '.' &&
                    (value[len - 4] == 'H' || value[len - 4] == 'h') &&
                    (value[len - 3] == 'T' || value[len - 3] == 't') &&
                    (value[len - 2] == 'M' || value[len - 2] == 'm') &&
                    (value[len - 1] == 'L' || value[len - 1] == 'l'));
        }

        internal static bool ExtIsGlml(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 5 &&
                   value[len - 5] == '.' &&
                   (value[len - 4] == 'G' || value[len - 4] == 'g') &&
                   (value[len - 3] == 'L' || value[len - 3] == 'l') &&
                   (value[len - 2] == 'M' || value[len - 2] == 'm') &&
                   (value[len - 1] == 'L' || value[len - 1] == 'l');
        }

        internal static bool ExtIsZip(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'Z' || value[len - 3] == 'z') &&
                   (value[len - 2] == 'I' || value[len - 2] == 'i') &&
                   (value[len - 1] == 'P' || value[len - 1] == 'p');
        }

        internal static bool ExtIs7z(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 3 &&
                   value[len - 3] == '.' &&
                   value[len - 2] == '7' &&
                   (value[len - 1] == 'Z' || value[len - 1] == 'z');
        }

        internal static bool ExtIsRar(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'R' || value[len - 3] == 'r') &&
                   (value[len - 2] == 'A' || value[len - 2] == 'a') &&
                   (value[len - 1] == 'R' || value[len - 1] == 'r');
        }

        internal static bool ExtIsIbt(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'I' || value[len - 3] == 'i') &&
                   (value[len - 2] == 'B' || value[len - 2] == 'b') &&
                   (value[len - 1] == 'T' || value[len - 1] == 't');
        }

        internal static bool ExtIsCbt(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'C' || value[len - 3] == 'c') &&
                   (value[len - 2] == 'B' || value[len - 2] == 'b') &&
                   (value[len - 1] == 'T' || value[len - 1] == 't');
        }

        internal static bool ExtIsGmp(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'G' || value[len - 3] == 'g') &&
                   (value[len - 2] == 'M' || value[len - 2] == 'm') &&
                   (value[len - 1] == 'P' || value[len - 1] == 'p');
        }

        internal static bool ExtIsNed(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'N' || value[len - 3] == 'n') &&
                   (value[len - 2] == 'E' || value[len - 2] == 'e') &&
                   (value[len - 1] == 'D' || value[len - 1] == 'd');
        }

        internal static bool ExtIsUnr(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'U' || value[len - 3] == 'u') &&
                   (value[len - 2] == 'N' || value[len - 2] == 'n') &&
                   (value[len - 1] == 'R' || value[len - 1] == 'r');
        }

        internal static bool EndsWithRaDotBin(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 6 &&
                   (value[len - 6] == 'R' || value[len - 6] == 'r') &&
                   (value[len - 5] == 'A' || value[len - 5] == 'a') &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'B' || value[len - 3] == 'b') &&
                   (value[len - 2] == 'I' || value[len - 2] == 'i') &&
                   (value[len - 1] == 'N' || value[len - 1] == 'n');
        }

        internal static bool ExtIsBin(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'B' || value[len - 3] == 'b') &&
                   (value[len - 2] == 'I' || value[len - 2] == 'i') &&
                   (value[len - 1] == 'N' || value[len - 1] == 'n');
        }

        internal static bool ExtIsSub(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'S' || value[len - 3] == 's') &&
                   (value[len - 2] == 'U' || value[len - 2] == 'u') &&
                   (value[len - 1] == 'B' || value[len - 1] == 'b');
        }

        internal static bool ExtIsMis(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'M' || value[len - 3] == 'm') &&
                   (value[len - 2] == 'I' || value[len - 2] == 'i') &&
                   (value[len - 1] == 'S' || value[len - 1] == 's');
        }

        internal static bool ExtIsGam(this string value)
        {
            if (value == null) return false;

            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'G' || value[len - 3] == 'g') &&
                   (value[len - 2] == 'A' || value[len - 2] == 'a') &&
                   (value[len - 1] == 'M' || value[len - 1] == 'm');
        }

        #endregion

        #endregion

        #region StartsWith and EndsWith

        private enum CaseComparison
        {
            CaseSensitive,
            CaseInsensitive,
            GivenOrUpper,
            GivenOrLower
        }

        private enum StartOrEnd { Start, End }

        /// <summary>
        /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithI(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.CaseInsensitive, StartOrEnd.Start);
        }

        /// <summary>
        /// StartsWith (given case or uppercase). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithGU(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.GivenOrUpper, StartOrEnd.Start);
        }

        /// <summary>
        /// StartsWith (given case or lowercase). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithGL(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.GivenOrLower, StartOrEnd.Start);
        }

        /// <summary>
        /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool EndsWithI(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.CaseInsensitive, StartOrEnd.End);
        }

        private static bool StartsWithOrEndsWithFast(this string str, string value,
            CaseComparison caseComparison, StartOrEnd startOrEnd)
        {
            if (str.IsEmpty() || str.Length < value.Length) return false;

            // Note: ASCII chars are 0-127. Uppercase is 65-90; lowercase is 97-122.
            // Therefore, if a char is in one of these ranges, one can convert between cases by simply adding or
            // subtracting 32.

            bool start = startOrEnd == StartOrEnd.Start;
            int siStart = start ? 0 : str.Length - value.Length;
            int siEnd = start ? value.Length : str.Length;

            for (int si = siStart, vi = 0; si < siEnd; si++, vi++)
            {
                // If we find a non-ASCII character, give up and run the slow check on the whole string. We do
                // this because one .NET char doesn't necessarily equal one Unicode char. Multiple .NET chars
                // might be needed. So we grit our teeth and take the perf hit of letting .NET handle it.
                // This is tuned for ASCII being the more common case, so we can save an advance check for non-
                // ASCII chars, at the expense of being slightly (probably insignificantly) slower if there are
                // in fact non-ASCII chars in value.
                if (value[vi] > 127)
                {
                    switch (caseComparison)
                    {
                        case CaseComparison.CaseSensitive:
                            return start
                                ? str.StartsWith(value, Ordinal)
                                : str.EndsWith(value, Ordinal);
                        case CaseComparison.CaseInsensitive:
                            return start
                                ? str.StartsWith(value, OrdinalIgnoreCase)
                                : str.EndsWith(value, OrdinalIgnoreCase);
                        case CaseComparison.GivenOrUpper:
                            return start
                                ? str.StartsWith(value, Ordinal) ||
                                  str.StartsWith(value.ToUpperInvariant(), Ordinal)
                                : str.EndsWith(value, Ordinal) ||
                                  str.EndsWith(value.ToUpperInvariant(), Ordinal);
                        case CaseComparison.GivenOrLower:
                            return start
                                ? str.StartsWith(value, Ordinal) ||
                                  str.StartsWith(value.ToLowerInvariant(), Ordinal)
                                : str.EndsWith(value, Ordinal) ||
                                  str.EndsWith(value.ToLowerInvariant(), Ordinal);
                    }
                }

                if (str[si] >= 65 && str[si] <= 90 && value[vi] >= 97 && value[vi] <= 122)
                {
                    if (caseComparison == CaseComparison.GivenOrLower || str[si] != value[vi] - 32) return false;
                }
                else if (value[vi] >= 65 && value[vi] <= 90 && str[si] >= 97 && str[si] <= 122)
                {
                    if (caseComparison == CaseComparison.GivenOrUpper || str[si] != value[vi] + 32) return false;
                }
                else if (str[si] != value[vi])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Empty / whitespace checks

        /// <summary>
        /// Returns true if <paramref name="value"/> is null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsEmpty(this string value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        #endregion

        #endregion

        #region Modifications

        /// <summary>
        /// Removes all matching pairs of parentheses that surround the entire string, while leaving
        /// non-surrounding parentheses intact.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string RemoveSurroundingParentheses(this string value)
        {
            if (value[0] != '(' || value[value.Length - 1] != ')') return value;

            bool surroundedByParens = false;
            do
            {
                var stack = new Stack<int>();
                for (int i = 0; i < value.Length; i++)
                {
                    switch (value[i])
                    {
                        case '(':
                            stack.Push(i);
                            surroundedByParens = false;
                            break;
                        case ')':
                            int index = stack.Any() ? stack.Pop() : -1;
                            surroundedByParens = index == 0;
                            break;
                        default:
                            surroundedByParens = false;
                            break;
                    }
                }

                if (surroundedByParens) value = value.Substring(1, value.Length - 2);
            } while (surroundedByParens);

            return value;
        }

        // Doesn't handle unicode left and right double quotes, but meh...
        internal static string RemoveUnpairedLeadingOrTrailingQuotes(this string value)
        {
            if (value.CountChars('\"') != 1) return value;

            if (value[0] == '\"')
            {
                value = value.Substring(1);
            }
            else if (value[value.Length - 1] == '\"')
            {
                value = value.Substring(0, value.Length - 1);
            }

            return value;
        }

        /// <summary>
        /// Just removes the extension from a filename, without the rather large overhead of
        /// Path.GetFileNameWithoutExtension().
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string RemoveExtension(this string fileName)
        {
            if (fileName == null) return null;
            int i = fileName.LastIndexOf('.');
            return i > -1 && i > fileName.LastIndexOf('\\') && i > fileName.LastIndexOf('/')
                ? fileName.Substring(0, i)
                : fileName;
        }

        #endregion

        #endregion

        internal static bool StringToDate(string dateString, out DateTime dateTime)
        {
            dateString = dateString.Replace(",", " ");
            dateString = Regex.Replace(dateString, @"\s+", @" ");
            dateString = Regex.Replace(dateString, @"\s+-\s+", "-");
            dateString = Regex.Replace(dateString, @"\s+/\s+", "/");
            dateString = Regex.Replace(dateString, @"\s+of\s+", " ");

            // Remove "st", "nd", "rd, "th" if present, as DateTime.TryParse() will choke on them
            Match match = DaySuffixesRegex.Match(dateString);
            if (match.Success)
            {
                Group suffix = match.Groups["Suffix"];
                dateString = dateString.Substring(0, suffix.Index) +
                             dateString.Substring(suffix.Index + suffix.Length);
            }

            // We pass specific date formats to ensure that no field will be inferred: if there's no year, we
            // want to fail, and not assume the current year.
            bool success = DateTime.TryParseExact(dateString, FMConstants.DateFormats,
                DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime result);
            dateTime = success ? result : new DateTime();
            return success;
        }
    }
}

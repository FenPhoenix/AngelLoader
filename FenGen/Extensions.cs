using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FenGen
{
    internal static class Extensions
    {
        #region Queries

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
            for (int i = 0; i < value.Length; i++)
                if (value[i] == character)
                    count++;

            return count;
        }

        internal static bool Contains(this string value, string substring, StringComparison comparison)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(substring)) return false;
            return value.IndexOf(substring, comparison) >= 0;
        }

        internal static bool Contains(this string value, char character)
        {
            return value.IndexOf(character) >= 0;
        }

        /// <summary>
        /// Case-insensitive Contains.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        internal static bool ContainsI(this string value, string substring)
        {
            return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ContainsI(this IEnumerable<string> value, string substring)
        {
            return value.Contains(substring, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Case-insensitive Equals.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool EqualsI(this string first, string second)
        {
            return first.Equals(second, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsTrue(this string value)
        {
            return value.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsFalse(this string value)
        {
            return value.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true if the string ends with extension (case-insensitive).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal static bool ExtEqualsI(this string value, string extension)
        {
            if (extension[0] != '.') extension = "." + extension;

            return !string.IsNullOrEmpty(value) &&
                   Path.GetExtension(value).Equals(extension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> is null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        #region StartsWith and EndsWith

        private enum CaseComparison
        {
            CaseSensitive,
            CaseInsensitive,
            GivenOrUpper,
            GivenOrLower
        }

        private enum StartOrEnd
        {
            Start,
            End
        }

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
            if (string.IsNullOrEmpty(str) || str.Length < value.Length) return false;

            // Note: ASCII chars are 0-127. Uppercase is 65-90; lowercase is 97-122.
            // Therefore, if a char is in one of these ranges, one can convert between cases by simply adding or
            // subtracting 32.

            var start = startOrEnd == StartOrEnd.Start;
            var siStart = start ? 0 : str.Length - value.Length;
            var siEnd = start ? value.Length : str.Length;

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
                                ? str.StartsWith(value, StringComparison.Ordinal)
                                : str.EndsWith(value, StringComparison.Ordinal);
                        case CaseComparison.CaseInsensitive:
                            return start
                                ? str.StartsWith(value, StringComparison.OrdinalIgnoreCase)
                                : str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
                        case CaseComparison.GivenOrUpper:
                            return start
                                ? str.StartsWith(value, StringComparison.Ordinal) ||
                                  str.StartsWith(value.ToUpperInvariant(), StringComparison.Ordinal)
                                : str.EndsWith(value, StringComparison.Ordinal) ||
                                  str.EndsWith(value.ToUpperInvariant(), StringComparison.Ordinal);
                        case CaseComparison.GivenOrLower:
                            return start
                                ? str.StartsWith(value, StringComparison.Ordinal) ||
                                  str.StartsWith(value.ToLowerInvariant(), StringComparison.Ordinal)
                                : str.EndsWith(value, StringComparison.Ordinal) ||
                                  str.EndsWith(value.ToLowerInvariant(), StringComparison.Ordinal);
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

        #endregion

        #region Modifications

        /// <summary>
        /// Clamps a number to between min and max.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
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
            int i;
            return (i = fileName.LastIndexOf('.')) == -1 ? fileName : fileName.Substring(0, i);
        }

        internal static string GetFileNameFast(this string path)
        {
            if (path == null) return null;
            int i;
            return (i = path.LastIndexOf('\\')) == -1 ? path : path.Substring(i + 1);
        }

        internal static string GetTopmostDirName(this string path)
        {
            var i = path.LastIndexOf('\\');
            if (i == -1) return path;

            var end = path.Length;
            if (i == path.Length - 1)
            {
                end--;
                i = path.LastIndexOf('\\', end);
            }

            return path.Substring(i + 1, end - (i + 1));
        }

        /// <summary>
        /// If <paramref name="value"/> is null or empty, returns null. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string ThisOrNull(this string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// If <paramref name="value"/> is null, empty, or whitespace, returns null. Otherwise, returns
        /// <paramref name="value"/> unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string ThisOrNullWS(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ClampToZero(this int value)
        {
            return Math.Max(value, 0);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using static System.StringComparison;

namespace AngelLoader
{
    public static partial class Misc
    {
        #region ASCII-specific

        [PublicAPI]
        internal static bool IsAsciiUpper(this char c) => c >= 65 && c <= 90;

        [PublicAPI]
        internal static bool IsAsciiLower(this char c) => c >= 97 && c <= 122;

        [PublicAPI]
        internal static bool EqualsIAscii(this char char1, char char2) =>
            char1 == char2 ||
            (char1.IsAsciiUpper() && char2.IsAsciiLower() && char1 == char2 - 32) ||
            (char1.IsAsciiLower() && char2.IsAsciiUpper() && char1 == char2 + 32);

        [PublicAPI]
        internal static bool IsAsciiAlpha(this char c) => c.IsAsciiUpper() || c.IsAsciiLower();

        [PublicAPI]
        internal static bool IsAsciiNumeric(this char c) => c >= 48 && c <= 57;

        [PublicAPI]
        internal static bool IsAsciiAlphanumeric(this char c) => c.IsAsciiAlpha() || c.IsAsciiNumeric();

        [PublicAPI]
        internal static bool IsAsciiAlphaUpper(this string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!str[i].IsAsciiUpper()) return false;
            }
            return true;
        }

        #endregion

        /// <summary>
        /// Returns the number of times a character appears in a string. Avoids whatever silly overhead junk Count(predicate) is doing.
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

        /// <summary>
        /// Returns the number of times a character appears in a string, earlying-out once it's counted <paramref name="maxToCount"/>
        /// occurrences.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <param name="maxToCount">The maximum number of occurrences to count before earlying-out.</param>
        /// <returns></returns>
        internal static int CountCharsUpToAmount(this string value, char character, int maxToCount)
        {
            int foundCount = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == character)
                {
                    foundCount++;
                    if (foundCount == maxToCount) break;
                }
            }

            return foundCount;
        }

        internal static bool CharCountIsAtLeast(this string value, char character, int count, int start = 0)
        {
            int foundCount = 0;
            for (int i = start; i < value.Length; i++)
            {
                if (value[i] == character)
                {
                    foundCount++;
                    if (foundCount == count) return true;
                }
            }

            return false;
        }

        #region Contains

        [PublicAPI]
        internal static bool Contains(this string value, string substring, StringComparison comparison)
        {
            return !value.IsEmpty() && !substring.IsEmpty() && value.IndexOf(substring, comparison) >= 0;
        }

        [PublicAPI]
        internal static bool Contains(this string value, char character) => value.IndexOf(character) >= 0;

        /// <summary>
        /// Case-insensitive Contains.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool ContainsI(this string value, string substring) => value.Contains(substring, OrdinalIgnoreCase);

        /// <summary>
        /// Case-insensitive Contains for List&lt;string&gt;. Avoiding IEnumerable like the plague for speed.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool ContainsI(this List<string> list, string str) => list.Contains(str, OrdinalIgnoreCase);

        /// <summary>
        /// Case-insensitive Contains for string[]. Avoiding IEnumerable like the plague for speed.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool ContainsI(this string[] array, string str) => array.Contains(str, OrdinalIgnoreCase);

        [PublicAPI]
        internal static bool ContainsIRemoveFirstHit(this List<string> list, string str) => list.ContainsRemoveFirstHit(str, OrdinalIgnoreCase);

        [PublicAPI]
        internal static bool ContainsRemoveFirstHit(this List<string> value, string substring, StringComparison stringComparison = Ordinal)
        {
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].Equals(substring, stringComparison))
                {
                    value.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        [PublicAPI]
        internal static bool Contains(this List<string> value, string substring, StringComparison stringComparison = Ordinal)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].Equals(substring, stringComparison)) return true;
            return false;
        }

        [PublicAPI]
        internal static bool Contains(this string[] value, string substring, StringComparison stringComparison = Ordinal)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].Equals(substring, stringComparison)) return true;
            return false;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Case-insensitive Equals.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool EqualsI(this string first, string second) => string.Equals(first, second, OrdinalIgnoreCase);

        internal static bool EqualsTrue(this string value) => string.Equals(value, bool.TrueString, OrdinalIgnoreCase);

        #endregion

        #region Empty / whitespace checks

        /// <summary>
        /// Returns true if <paramref name="value"/> is null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

        #endregion

        #region StartsWith and EndsWith

        private enum StartOrEnd { Start, End }

        /// <summary>
        /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, StartOrEnd.Start);

        /// <summary>
        /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool EndsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, StartOrEnd.End);

        private static bool StartsWithOrEndsWithIFast(this string str, string value, StartOrEnd startOrEnd)
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
                    return start
                        ? str.StartsWith(value, OrdinalIgnoreCase)
                        : str.EndsWith(value, OrdinalIgnoreCase);
                }

                if (!str[si].EqualsIAscii(value[vi])) return false;
            }

            return true;
        }

        #endregion

        #region Clamping

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
        /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ClampToZero(this int value) => Math.Max(value, 0);

        #endregion

        #region FM installed name conversion

        /// <summary>
        /// Format an FM archive name to conform to NewDarkLoader's FM install directory name requirements.
        /// </summary>
        /// <param name="archiveName">Filename without path or extension.</param>
        /// <param name="truncate"></param>
        /// <returns></returns>
        internal static string ToInstDirNameNDL(this string archiveName, bool truncate = true) => ToInstDirName(archiveName, "+.~ ", truncate);

        /// <summary>
        /// Format an FM archive name to conform to FMSel's FM install directory name requirements.
        /// </summary>
        /// <param name="archiveName">Filename without path or extension.</param>
        /// <param name="truncate">Whether to truncate the name to 30 characters or less.</param>
        /// <returns></returns>
        internal static string ToInstDirNameFMSel(this string archiveName, bool truncate = true) => ToInstDirName(archiveName, "+;:.,<>?*~| ", truncate);

        private static readonly StringBuilder ToInstDirNameSB = new StringBuilder(30);
        private static string ToInstDirName(string archiveName, string illegalChars, bool truncate)
        {
            int count = archiveName.LastIndexOf('.');
            if (truncate)
            {
                if (count == -1 || count > 30) count = Math.Min(archiveName.Length, 30);
            }
            else
            {
                if (count == -1) count = archiveName.Length;
            }

            ToInstDirNameSB.Clear();
            ToInstDirNameSB.Append(archiveName);
            for (int i = 0; i < illegalChars.Length; i++) ToInstDirNameSB.Replace(illegalChars[i], '_', 0, count);

            return ToInstDirNameSB.ToString(0, count);
        }

        #endregion

        internal static string ToSingleLineComment(this string value, int maxLength)
        {
            if (value.IsEmpty()) return "";

            int linebreakIndex = value.IndexOf("\r\n", InvariantCulture);

            return linebreakIndex > -1 && linebreakIndex <= maxLength
                ? value.Substring(0, linebreakIndex)
                : value.Substring(0, Math.Min(value.Length, maxLength));
        }

        #region Escaping

        internal static string FromRNEscapes(this string value) => value.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\");

        internal static string ToRNEscapes(this string value) => value.Replace("\\", @"\\").Replace("\r\n", @"\r\n");

        /// <summary>
        /// For text that goes in menus: "&" is a reserved character, so escape "&" to "&&"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string EscapeAmpersands(this string value) => value.Replace("&", "&&");

        /// <summary>
        /// Just puts a \ in front of each character in the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string EscapeAllChars(this string value)
        {
            // Don't remove this freaking null check, or Config reading might fail
            if (value.IsEmpty()) return "";

            string ret = "";
            foreach (char c in value) ret += '\\' + c.ToString();

            return ret;
        }

        #endregion
    }
}

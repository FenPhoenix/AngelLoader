using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using static System.StringComparison;

namespace FenGen
{
    internal static partial class Misc
    {
        #region ASCII-specific

        [PublicAPI]
        internal static bool IsAsciiUpper(this char c) => c >= 'A' && c <= 'Z';

        [PublicAPI]
        internal static bool IsAsciiLower(this char c) => c >= 'a' && c <= 'z';

        [PublicAPI]
        internal static bool EqualsIAscii(this char char1, char char2) =>
            char1 == char2 ||
            (char1.IsAsciiUpper() && char2.IsAsciiLower() && char1 == char2 - 32) ||
            (char1.IsAsciiLower() && char2.IsAsciiUpper() && char1 == char2 + 32);

        [PublicAPI]
        internal static bool IsAsciiAlpha(this char c) => c.IsAsciiUpper() || c.IsAsciiLower();

        [PublicAPI]
        internal static bool IsAsciiNumeric(this char c) => c >= '0' && c <= '9';

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
        [PublicAPI]
        internal static bool EqualsI(this string first, string second) => string.Equals(first, second, OrdinalIgnoreCase);

        [PublicAPI]
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

        /// <summary>
        /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool StartsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, start: true);

        /// <summary>
        /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool EndsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, start: false);

        private static bool StartsWithOrEndsWithIFast(this string str, string value, bool start)
        {
            if (str.IsEmpty() || str.Length < value.Length) return false;

            // Note: ASCII chars are 0-127. Uppercase is 65-90; lowercase is 97-122.
            // Therefore, if a char is in one of these ranges, one can convert between cases by simply adding or
            // subtracting 32.

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

        [PublicAPI]
        internal static bool EqualsOrStartsWithPlusWhiteSpaceI(this string str, string value) => str.EqualsI(value) || str.StartsWithPlusWhiteSpaceI(value);

        [PublicAPI]
        internal static bool StartsWithPlusWhiteSpaceI(this string str, string value) => str.StartsWithPlusWhiteSpace(value, OrdinalIgnoreCase);

        [PublicAPI]
        internal static bool StartsWithPlusWhiteSpace(this string str, string value, StringComparison comparison = Ordinal)
        {
            int valLen;
            return str.StartsWith(value, comparison) &&
                   str.Length > (valLen = value.Length) &&
                   char.IsWhiteSpace(str[valLen]);
        }

        #endregion

        [PublicAPI]
        internal static string FirstCharToLower(this string str) => char.ToLowerInvariant(str[0]) + str.Substring(1);
    }
}

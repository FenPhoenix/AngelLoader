using System;
using System.Collections.Generic;
using AL_Common;
using static System.StringComparison;

namespace FMScanner
{
    internal static class Utility
    {
        /// <summary>Returns a value between 0 and 1.0 that indicates how similar the two strings are.</summary>
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

        #region Readme validation

        internal static bool IsEnglishReadme(this string value)
        {
            int dotIndex = value.IndexOf('.');
            return dotIndex > -1 &&
                   ((dotIndex == 9 && value.StartsWithI("fminfo-en")) ||
                    (dotIndex == 10 && value.StartsWithI("fminfo-eng")) ||
                    !(dotIndex > 6 && value.StartsWithI("fminfo")));
        }

        internal static bool IsValidReadme(this string readme) =>
            readme.ExtIsTxt() ||
            readme.ExtIsRtf() ||
            readme.ExtIsWri() ||
            readme.ExtIsGlml() ||
            // We don't scan HTML files, but we may still need them to check their dates
            readme.ExtIsHtml();

        #endregion

        #region File extensions

        #region Baked-in extension checks (generated)

        // TODO: These may actually be slower... just so I remember in case I wanna change them back
        private static bool ExtIsTxt(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'T' || value[len - 3] == 't') &&
                   (value[len - 2] == 'X' || value[len - 2] == 'x') &&
                   (value[len - 1] == 'T' || value[len - 1] == 't');
        }

        private static bool ExtIsRtf(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'R' || value[len - 3] == 'r') &&
                   (value[len - 2] == 'T' || value[len - 2] == 't') &&
                   (value[len - 1] == 'F' || value[len - 1] == 'f');
        }

        private static bool ExtIsWri(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'W' || value[len - 3] == 'w') &&
                   (value[len - 2] == 'R' || value[len - 2] == 'r') &&
                   (value[len - 1] == 'I' || value[len - 1] == 'i');
        }

        internal static bool ExtIsHtml(this string value)
        {
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
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'Z' || value[len - 3] == 'z') &&
                   (value[len - 2] == 'I' || value[len - 2] == 'i') &&
                   (value[len - 1] == 'P' || value[len - 1] == 'p');
        }

        internal static bool ExtIs7z(this string value)
        {
            int len = value.Length;
            return len > 3 &&
                   value[len - 3] == '.' &&
                   value[len - 2] == '7' &&
                   (value[len - 1] == 'Z' || value[len - 1] == 'z');
        }

        internal static bool ExtIsRar(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'R' || value[len - 3] == 'r') &&
                   (value[len - 2] == 'A' || value[len - 2] == 'a') &&
                   (value[len - 1] == 'R' || value[len - 1] == 'r');
        }

        internal static bool ExtIsIbt(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'I' || value[len - 3] == 'i') &&
                   (value[len - 2] == 'B' || value[len - 2] == 'b') &&
                   (value[len - 1] == 'T' || value[len - 1] == 't');
        }

        internal static bool ExtIsCbt(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'C' || value[len - 3] == 'c') &&
                   (value[len - 2] == 'B' || value[len - 2] == 'b') &&
                   (value[len - 1] == 'T' || value[len - 1] == 't');
        }

        internal static bool ExtIsGmp(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'G' || value[len - 3] == 'g') &&
                   (value[len - 2] == 'M' || value[len - 2] == 'm') &&
                   (value[len - 1] == 'P' || value[len - 1] == 'p');
        }

        internal static bool ExtIsNed(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'N' || value[len - 3] == 'n') &&
                   (value[len - 2] == 'E' || value[len - 2] == 'e') &&
                   (value[len - 1] == 'D' || value[len - 1] == 'd');
        }

        internal static bool ExtIsUnr(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'U' || value[len - 3] == 'u') &&
                   (value[len - 2] == 'N' || value[len - 2] == 'n') &&
                   (value[len - 1] == 'R' || value[len - 1] == 'r');
        }

        internal static bool ExtIsBin(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'B' || value[len - 3] == 'b') &&
                   (value[len - 2] == 'I' || value[len - 2] == 'i') &&
                   (value[len - 1] == 'N' || value[len - 1] == 'n');
        }

        internal static bool ExtIsSub(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'S' || value[len - 3] == 's') &&
                   (value[len - 2] == 'U' || value[len - 2] == 'u') &&
                   (value[len - 1] == 'B' || value[len - 1] == 'b');
        }

        internal static bool ExtIsMis(this string value)
        {
            int len = value.Length;
            return len > 4 &&
                   value[len - 4] == '.' &&
                   (value[len - 3] == 'M' || value[len - 3] == 'm') &&
                   (value[len - 2] == 'I' || value[len - 2] == 'i') &&
                   (value[len - 1] == 'S' || value[len - 1] == 's');
        }

        internal static bool ExtIsGam(this string value)
        {
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

                if (str[si].IsAsciiUpper() && value[vi].IsAsciiLower())
                {
                    if (caseComparison == CaseComparison.GivenOrLower || str[si] != value[vi] - 32) return false;
                }
                else if (value[vi].IsAsciiUpper() && str[si].IsAsciiLower())
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

        #region String value cleanup

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
                            int index = stack.Count > 0 ? stack.Pop() : -1;
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
        internal static string RemoveUnpairedLeadingOrTrailingQuotes(this string value) =>
            value.CountCharsUpToAmount('\"', 2) != 1 ? value :
            value[0] == '\"' ? value.Substring(1) :
            value[value.Length - 1] == '\"' ? value.Substring(0, value.Length - 1) : value;

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AL_Common;
using static System.StringComparison;

namespace FMScanner;

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

                char str1Char = string1[i];
                char str2Char = string2[j];
                int substCost =
                    str1Char < 128 && str2Char < 128
                        ? str1Char.EqualsIAscii(str2Char)
                            ? 0
                            : 1
                        : str1Char == str2Char ||
                          str1Char.ToString().Equals(str2Char.ToString(), stringComparison)
                            ? 0
                            : 1;

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
               ((dotIndex == 9 && value.StartsWithI_Local("fminfo-en")) ||
                (dotIndex == 10 && value.StartsWithI_Local("fminfo-eng")) ||
                !(dotIndex > 6 && value.StartsWithI_Local("fminfo")));
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

    internal readonly ref struct StringCompareReturn
    {
        internal readonly bool RequiresStringComparison;
        internal readonly int Compare;

        public StringCompareReturn(int compare)
        {
            RequiresStringComparison = false;
            Compare = compare;
        }

        public StringCompareReturn(bool requiresStringComparison)
        {
            RequiresStringComparison = requiresStringComparison;
            Compare = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool EqualsI_Local(this string str1, string str2)
    {
        if (str1.Length != str2.Length) return false;

        StringCompareReturn result = CompareToOrdinalIgnoreCase(str1.AsSpan(), str2.AsSpan());
        return result.RequiresStringComparison ? str1.Equals(str2, OrdinalIgnoreCase) : result.Compare == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool EqualsI_Local(this ReadOnlySpan<char> str1, string str2)
    {
        int str1Len = str1.Length;
        int str2Len = str2.Length;

        if (str1Len != str2Len) return false;

        StringCompareReturn result = CompareToOrdinalIgnoreCase(str1, str2.AsSpan());
        return result.RequiresStringComparison ? str1.ToString().Equals(str2, OrdinalIgnoreCase) : result.Compare == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool StartsWithI_Local(this string str1, string str2)
    {
        int str1Len = str1.Length;
        int str2Len = str2.Length;

        if (str1Len < str2Len) return false;

        StringCompareReturn result = CompareToOrdinalIgnoreCase(str1.AsSpan().Slice(0, str2Len), str2.AsSpan());
        return result.RequiresStringComparison ? str1.StartsWith(str2, OrdinalIgnoreCase) : result.Compare == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool EndsWithI_Local(this ReadOnlySpan<char> str1, string str2)
    {
        int str1Len = str1.Length;
        int str2Len = str2.Length;

        if (str1Len < str2Len) return false;

        StringCompareReturn result = CompareToOrdinalIgnoreCase(str1.Slice(str1Len - str2Len), str2.AsSpan());
        return result.RequiresStringComparison ? str1.ToString().EndsWith(str2, OrdinalIgnoreCase) : result.Compare == 0;
    }

    // Copied from .NET 7 and slightly modified
    internal static unsafe StringCompareReturn CompareToOrdinalIgnoreCase(
        ReadOnlySpan<char> strA,
        ReadOnlySpan<char> strB)
    {
        int num1 = Math.Min(strA.Length, strB.Length);
        fixed (char* chPtr1 = &MemoryMarshal.GetReference(strA))
        fixed (char* chPtr2 = &MemoryMarshal.GetReference(strB))
        {
            char* chPtr3 = chPtr1;
            char* chPtr4 = chPtr2;
            while (num1 != 0 &&
                   //*chPtr3 <= '\u007F' &&
                   // Only check the needle for non-ASCII chars, matching our old custom StartsWith/EndsWith
                   // function, and avoiding 99.9999% of ToString() allocations that would otherwise happen here.
                   *chPtr4 <= '\u007F')
            {
                int num3 = *chPtr3;
                int num4 = *chPtr4;
                if (num3 == num4)
                {
                    ++chPtr3;
                    ++chPtr4;
                    --num1;
                }
                else
                {
                    if ((uint)(num3 - 97) <= 25U)
                    {
                        num3 -= 32;
                    }
                    if ((uint)(num4 - 97) <= 25U)
                    {
                        num4 -= 32;
                    }
                    if (num3 != num4)
                    {
                        return new StringCompareReturn(compare: num3 - num4);
                    }
                    ++chPtr3;
                    ++chPtr4;
                    --num1;
                }
            }
            return num1 == 0
                ? new StringCompareReturn(compare: strA.Length - strB.Length)
                : new StringCompareReturn(requiresStringComparison: true);
        }
    }

    private enum CaseComparison
    {
        GivenOrUpper,
        GivenOrLower
    }

    /// <summary>
    /// StartsWith (given case or uppercase). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool StartsWithGU(this string str, string value)
    {
        return StartsWithFast(str, value, CaseComparison.GivenOrUpper);
    }

    /// <summary>
    /// StartsWith (given case or lowercase). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool StartsWithGL(this string str, string value)
    {
        return StartsWithFast(str, value, CaseComparison.GivenOrLower);
    }

    private static bool StartsWithFast(this string str, string value, CaseComparison caseComparison)
    {
        if (str.IsEmpty() || str.Length < value.Length) return false;

        int valueLength = value.Length;

        for (int si = 0, vi = 0; si < valueLength; si++, vi++)
        {
            char sc = str[si];
            char vc = value[vi];

            if (vc > 127)
            {
                switch (caseComparison)
                {
                    case CaseComparison.GivenOrUpper:
                        return str.StartsWith(value, Ordinal) ||
                               str.StartsWith(value.ToUpperInvariant(), Ordinal);
                    case CaseComparison.GivenOrLower:
                        return str.StartsWith(value, Ordinal) ||
                               str.StartsWith(value.ToLowerInvariant(), Ordinal);
                }
            }

            if (sc.IsAsciiUpper() && vc.IsAsciiLower())
            {
                if (caseComparison == CaseComparison.GivenOrLower || sc != vc - 32) return false;
            }
            else if (vc.IsAsciiUpper() && sc.IsAsciiLower())
            {
                if (caseComparison == CaseComparison.GivenOrUpper || sc != vc + 32) return false;
            }
            else if (sc != vc)
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
        if (value.IsEmpty() || value[0] != '(' || value[value.Length - 1] != ')') return value;

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
        value.CharAppearsExactlyOnce('\"')
            ? value[0] == '\"'
                ? value.Substring(1)
                : value[value.Length - 1] == '\"'
                    ? value.Substring(0, value.Length - 1)
                    : value
            : value;

    #endregion

    internal static string ExtractFromQuotedSection(string line)
    {
        int i1 = line.IndexOf('"') + 1;
        int i2 = line.IndexOf('\"', i1);

        return i2 > i1 ? line.Substring(i1, i2 - i1) : "";
    }

    internal static bool AnyConsecutiveWordChars(string value)
    {
        int consecutiveWordCharCount = 0;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            UnicodeCategory cat = char.GetUnicodeCategory(c);
            // Matching the "\w\w" regex behavior exactly
            if (cat
                is UnicodeCategory.LowercaseLetter
                or UnicodeCategory.UppercaseLetter
                or UnicodeCategory.TitlecaseLetter
                or UnicodeCategory.OtherLetter
                or UnicodeCategory.ModifierLetter
                or UnicodeCategory.NonSpacingMark
                or UnicodeCategory.DecimalDigitNumber
                or UnicodeCategory.ConnectorPunctuation)
            {
                consecutiveWordCharCount++;
                if (consecutiveWordCharCount > 1)
                {
                    return true;
                }
            }
            else
            {
                consecutiveWordCharCount = 0;
            }
        }

        return false;
    }

    #region GLML

    private enum GLMLTagType
    {
        None,
        Opening,
        Closing
    }

    internal static string GLMLToPlainText(string glml)
    {
        // @MEM: We could cache these, and maybe even as ListFast<char>s to avoid the cruft of StringBuilder appending?
        var sb = new StringBuilder(glml.Length);
        var subSB = new StringBuilder(16);

        static bool SBEquals(StringBuilder sb, string value)
        {
            if (sb.Length != value.Length) return false;
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] != value[i]) return false;
            }
            return true;
        }

        GLMLTagType tagType = GLMLTagType.None;

        for (int i = 0; i < glml.Length; i++)
        {
            char c = glml[i];

            if (tagType > GLMLTagType.None)
            {
                if (c == '/')
                {
                    tagType = GLMLTagType.Closing;
                }
                else if (c == ']')
                {
                    if (tagType != GLMLTagType.Closing &&
                        (SBEquals(subSB, "GLNL") || SBEquals(subSB, "GLLINE")))
                    {
                        sb.Append("\r\n");
                    }

                    tagType = GLMLTagType.None;
                }
                else
                {
                    subSB.Append(c);
                }
            }
            else if (c == '[')
            {
                subSB.Clear();
                tagType = GLMLTagType.Opening;
            }
            else if (c == '&')
            {
                subSB.Clear();

                // HTML Unicode numeric character references
                if (i < glml.Length - 4 && glml[i + 1] == '#')
                {
                    int end = Math.Min(i + 12, glml.Length);
                    for (int j = i + 2; i < end; j++)
                    {
                        if (j == i + 2 && glml[j] == 'x')
                        {
                            end = Math.Min(end + 1, glml.Length);
                            subSB.Append(glml[j]);
                        }
                        else if (glml[j] == ';')
                        {
                            string num = subSB.ToString();

                            bool success = num.Length > 0 && num[0] == 'x'
                                ? int.TryParse(num.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result)
                                : int.TryParse(num, out result);

                            if (success)
                            {
                                // @MEM: We can use the version from the rtf converter to avoid allocs and exception throwing
                                sb.Append(char.ConvertFromUtf32(result));
                            }
                            else
                            {
                                sb.Append("&#").Append(subSB).Append(';');
                            }
                            i = j;
                            break;
                        }
                        else
                        {
                            subSB.Append(glml[j]);
                        }
                    }
                }
                // HTML Unicode named character references
                else if (i < glml.Length - 3 && glml[i + 1].IsAsciiAlpha())
                {
                    for (int j = i + 1; i < glml.Length; j++)
                    {
                        if (glml[j] == ';')
                        {
                            string name = subSB.ToString();

                            if (HTML.HTMLNamedEntities.TryGetValue(name, out string value) &&
                                int.TryParse(value, out int result))
                            {
                                // @MEM: We can use the version from the rtf converter to avoid allocs and exception throwing
                                sb.Append(char.ConvertFromUtf32(result));
                            }
                            else
                            {
                                sb.Append('&').Append(subSB).Append(';');
                            }
                            i = j;
                            break;
                        }
                        // Support named references with numbers somewhere after their first char ("blk34" for instance)
                        else if (!glml[j].IsAsciiAlphanumeric())
                        {
                            sb.Append('&').Append(subSB).Append(glml[j]);
                            i = j;
                            break;
                        }
                        else
                        {
                            subSB.Append(glml[j]);
                        }
                    }
                }
                else
                {
                    sb.Append('&');
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    #endregion
}

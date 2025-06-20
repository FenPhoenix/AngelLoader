﻿using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Readers.Rar;
using static System.StringComparison;
using static FMScanner.ReadOnlyDataContext;

namespace FMScanner;

internal static class Utility
{
    /// <summary>Returns a value between 0 and 1.0 that indicates how similar the two strings are (case-insensitive).</summary>
    internal static double SimilarityTo(this string string1, string string2, SevenZipContext sevenZipContext)
    {
        if (string1.EqualsI(string2)) return 1.0;

        int string1Length = string1.Length;
        if (string1Length == 0) return 0;
        int string2Length = string2.Length;
        if (string2Length == 0) return 0;

        int vecLength = string2Length + 1;

        double ret;

        int[] vec1 = sevenZipContext.IntArrayPool.Rent(vecLength);
        int[] vec2 = sevenZipContext.IntArrayPool.Rent(vecLength);
        try
        {
            for (int i = 0; i < vecLength; i++)
            {
                vec1[i] = i;
            }

            for (int i = 0; i < string1Length; i++)
            {
                vec2[0] = i + 1;

                for (int j = 0; j < string2Length; j++)
                {
                    int delCost = vec1[j + 1] + 1;
                    int insCost = vec2[j] + 1;

                    char str1Char = string1[i];
                    char str2Char = string2[j];
                    int substCost =
                        BothAreAscii(str1Char, str2Char)
                            ? str1Char.EqualsIAscii(str2Char)
                                ? 0
                                : 1
                            : str1Char == str2Char ||
                              char.ToUpperInvariant(str1Char) == char.ToUpperInvariant(str2Char)
                                ? 0
                                : 1;

                    vec2[j + 1] = Math.Min(insCost, Math.Min(delCost, vec1[j] + substCost));
                }

                Array.Copy(vec2, vec1, vecLength);
            }

            ret = 1.0 - ((double)vec2[string2Length] / Math.Max(string1Length, string2Length));
        }
        finally
        {
            sevenZipContext.IntArrayPool.Return(vec1);
            sevenZipContext.IntArrayPool.Return(vec2);
        }

        return ret;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsIbt(this string value) => value.EndsWithI_Ascii(".ibt");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsCbt(this string value) => value.EndsWithI_Ascii(".cbt");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsGmp(this string value) => value.EndsWithI_Ascii(".gmp");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsNed(this string value) => value.EndsWithI_Ascii(".ned");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsUnr(this string value) => value.EndsWithI_Ascii(".unr");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsBin(this string value) => value.EndsWithI_Ascii(".bin");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsSub(this string value) => value.EndsWithI_Ascii(".sub");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsMis(this string value) => value.EndsWithI_Ascii(".mis");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ExtIsGam(this string value) => value.EndsWithI_Ascii(".gam");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsBaseDirMisOrGamFile(this string value) =>
        !value.Rel_ContainsDirSep() && (value.ExtIsMis() || value.ExtIsGam());

    #endregion

    #region StartsWith and EndsWith

    // @FileStreamNET: Span conversions - can we cache some of them caller-side?

    [StructLayout(LayoutKind.Auto)]
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
    internal static bool EqualsI_Local(ReadOnlySpan<char> str1, string str2)
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

        StringCompareReturn result = CompareToOrdinalIgnoreCase(str1.AsSpan()[..str2Len], str2.AsSpan());
        return result.RequiresStringComparison ? str1.StartsWith(str2, OrdinalIgnoreCase) : result.Compare == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool EndsWithI_Local(ReadOnlySpan<char> str1, string str2)
    {
        int str1Len = str1.Length;
        int str2Len = str2.Length;

        if (str1Len < str2Len) return false;

        StringCompareReturn result = CompareToOrdinalIgnoreCase(str1[(str1Len - str2Len)..], str2.AsSpan());
        return result.RequiresStringComparison ? str1.ToString().EndsWith(str2, OrdinalIgnoreCase) : result.Compare == 0;
    }

    // Copied from .NET 7 and slightly modified
    internal static unsafe StringCompareReturn CompareToOrdinalIgnoreCase(
        ReadOnlySpan<char> strA,
        ReadOnlySpan<char> strB)
    {
        int length = Math.Min(strA.Length, strB.Length);
        fixed (char* strARef = &MemoryMarshal.GetReference(strA))
        fixed (char* strBRef = &MemoryMarshal.GetReference(strB))
        {
            char* charA = strARef;
            char* charB = strBRef;
            while (length != 0 &&
                   //*charA <= '\u007F' &&
                   // Only check the needle for non-ASCII chars, matching our old custom StartsWith/EndsWith
                   // function, and avoiding 99.9999% of ToString() allocations that would otherwise happen here.
                   *charB <= '\u007F')
            {
                int currentA = *charA;
                int currentB = *charB;
                if (currentA == currentB)
                {
                    ++charA;
                    ++charB;
                    --length;
                }
                else
                {
                    if ((uint)(currentA - 97) <= 25U)
                    {
                        currentA -= 32;
                    }
                    if ((uint)(currentB - 97) <= 25U)
                    {
                        currentB -= 32;
                    }
                    if (currentA != currentB)
                    {
                        return new StringCompareReturn(compare: currentA - currentB);
                    }
                    ++charA;
                    ++charB;
                    --length;
                }
            }
            return length == 0
                ? new StringCompareReturn(compare: strA.Length - strB.Length)
                : new StringCompareReturn(requiresStringComparison: true);
        }
    }

    /// <summary>
    /// StartsWith (given case or uppercase). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool StartsWith_GivenOrUpper(this string str, string value)
    {
        if (str.IsEmpty() || str.Length < value.Length) return false;

        int valueLength = value.Length;

        for (int si = 0, vi = 0; si < valueLength; si++, vi++)
        {
            char sc = str[si];
            char vc = value[vi];

            if (vc > 127)
            {
                return str.StartsWith(value, Ordinal) ||
                       str.StartsWith(value.ToUpperInvariant(), Ordinal);
            }

            if (sc.IsAsciiUpper() && vc.IsAsciiLower())
            {
                if (sc != vc - 32) return false;
            }
            else if (sc != vc)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// StartsWith (given case or lowercase). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static bool StartsWith_GivenOrLower(this string str, string value)
    {
        if (str.IsEmpty() || str.Length < value.Length) return false;

        int valueLength = value.Length;

        for (int si = 0, vi = 0; si < valueLength; si++, vi++)
        {
            char sc = str[si];
            char vc = value[vi];

            if (vc > 127)
            {
                return str.StartsWith(value, Ordinal) ||
                       str.StartsWith(value.ToLowerInvariant(), Ordinal);
            }

            if (vc.IsAsciiUpper() && sc.IsAsciiLower())
            {
                if (sc != vc + 32) return false;
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
    /// <param name="stack"></param>
    /// <returns></returns>
    internal static string RemoveSurroundingParentheses(this string value, StackFast<int> stack)
    {
        if (value.IsEmpty() || value[0] != '(' || value[^1] != ')') return value;

        stack.ClearFast();

        ReadOnlySpan<char> valueSpan = value.AsSpan();

        bool surroundedByParens = false;
        do
        {
            for (int i = 0; i < valueSpan.Length; i++)
            {
                switch (valueSpan[i])
                {
                    case '(':
                        stack.Push(i);
                        surroundedByParens = false;
                        break;
                    case ')':
                        int index = stack.TryPop(out int result) ? result : -1;
                        surroundedByParens = index == 0;
                        break;
                    default:
                        surroundedByParens = false;
                        break;
                }
            }

            if (surroundedByParens) valueSpan = valueSpan.Slice(1, value.Length - 2);
        } while (surroundedByParens);

        return value.Length == valueSpan.Length ? value : valueSpan.ToString();
    }

    // Doesn't handle unicode left and right double quotes, but meh...
    internal static string RemoveUnpairedLeadingOrTrailingQuotes(this string value) =>
        value.CharAppearsExactlyOnce('\"')
            ? value[0] == '\"'
                ? value.Substring(1)
                : value[^1] == '\"'
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

    #region Acronym detection

    internal static void GetAcronym(string title, ListFast<char> acronymChars)
    {
        for (int titleIndex = 0; titleIndex < title.Length; titleIndex++)
        {
            char c = title[titleIndex];
            if (c.IsAsciiNumeric() || c.IsAsciiUpper())
            {
                acronymChars.Add(c);
            }
        }
    }

    internal static void GetAcronym_SupportRomanNumerals(
        string title,
        ListFast<char> acronymChars,
        ListFast<char> romanNumeralRun,
        byte[] romanNumeralToDecimalTable)
    {
        romanNumeralRun.ClearFast();

        for (int titleIndex = 0; titleIndex < title.Length; titleIndex++)
        {
            char c = title[titleIndex];
            if (CharacterIsSupportedRomanNumeral(c))
            {
                romanNumeralRun.Add(c);

                int romanNumeralIndex;
                for (romanNumeralIndex = titleIndex + 1; romanNumeralIndex < title.Length; romanNumeralIndex++)
                {
                    c = title[romanNumeralIndex];
                    if (CharacterIsSupportedRomanNumeral(c))
                    {
                        romanNumeralRun.Add(c);
                    }
                    else
                    {
                        break;
                    }
                }
                AddRomanConvertedChar(romanNumeralRun, acronymChars, romanNumeralToDecimalTable);
                titleIndex = romanNumeralIndex - 1;
            }
            else if (c.IsAsciiNumeric() || c.IsAsciiUpper())
            {
                acronymChars.Add(c);
            }
        }

        return;

        // Nothing past 'X' because no mission number is going to be that high and we don't want something like
        // "MIX" being interpreted as a Roman numeral
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool CharacterIsSupportedRomanNumeral(char c) => c is 'I' or 'V' or 'X';

        static void AddRomanConvertedChar(ListFast<char> romanNumeralRun, ListFast<char> acronymChars, byte[] romanNumeralToDecimalTable)
        {
            byte number = RomanToInteger(romanNumeralRun, romanNumeralToDecimalTable);
            int digits = number <= 9 ? 1 : number <= 99 ? 2 : 3;
            for (int digitIndex = 0; digitIndex < digits; digitIndex++)
            {
                char decimalChar = (char)((number % 10) + '0');
                acronymChars.Add(decimalChar);
                number /= 10;
            }
        }

        static byte RomanToInteger(ListFast<char> romanNumeralRun, byte[] romanNumeralToDecimalTable)
        {
            byte number = 0;
            for (int i = 0; i < romanNumeralRun.Count; i++)
            {
                // We're indexing into a sub-byte-length array with a char-length value, but we know we only have
                // Roman numeral chars in the run, so we won't go out of bounds.
                byte current = romanNumeralToDecimalTable[romanNumeralRun[i]];
                if (i < romanNumeralRun.Count - 1 && current < romanNumeralToDecimalTable[romanNumeralRun[i + 1]])
                {
                    number -= current;
                }
                else
                {
                    number += current;
                }
            }
            return number;
        }
    }

    internal static bool SequenceEqual(ListFast<char> first, ListFast<char> second)
    {
        if (first.Count != second.Count) return false;

        for (int i = 0; i < first.Count; i++)
        {
            if (first[i] != second[i]) return false;
        }
        return true;
    }

    internal static bool ContainsMultipleWords(this string value)
    {
        int length = value.Length;
        int upperCaseIndex = -1;
        for (int i = 0; i < length; i++)
        {
            char c = value[i];
            if (c.IsAsciiUpper())
            {
                if (upperCaseIndex == -1)
                {
                    upperCaseIndex = i;
                }
                else if (i > upperCaseIndex + 1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal static bool ContainsWhiteSpace(this string value)
    {
        int length = value.Length;
        for (int i = 0; i < length; i++)
        {
            if (char.IsWhiteSpace(value[i])) return true;
        }

        return false;
    }

    internal static bool EqualsIgnoreCaseAndWhiteSpace(this string str1, string str2, ListFast<char> temp1, ListFast<char> temp2)
    {
        int str1Length = str1.Length;
        temp1.ClearFast();
        for (int i = 0; i < str1Length; i++)
        {
            char c = str1[i];
            if (!char.IsWhiteSpace(c))
            {
                temp1.Add(c);
            }
        }

        int str2Length = str2.Length;
        temp2.ClearFast();
        for (int i = 0; i < str2Length; i++)
        {
            char c = str2[i];
            if (!char.IsWhiteSpace(c))
            {
                temp2.Add(c);
            }
        }

        if (temp1.Count != temp2.Count) return false;

        for (int i = 0; i < temp1.Count; i++)
        {
            char c1 = temp1[i];
            char c2 = temp2[i];
            if (BothAreAscii(c1, c2))
            {
                if (!c1.EqualsIAscii(c2)) return false;
            }
            else
            {
                if (char.ToUpperInvariant(c1) != char.ToUpperInvariant(c2)) return false;
            }
        }

        return true;
    }

    #endregion

    // @RAR: Duplicate because we don't want to put it in Common cause then it has to reference SharpCompress
    //  and then we couldn't have SharpCompress reference Common. Meh.
    internal static void ExtractToFile_Fast(
        this RarReader reader,
        string fileName,
        bool overwrite)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using (Stream destination = File.Open(fileName, mode, FileAccess.Write, FileShare.Read))
        {
            reader.WriteEntryTo(destination);
        }
        DateTime? lastModifiedTime = reader.Entry.LastModifiedTime;
        if (lastModifiedTime != null)
        {
            SetLastWriteTime_Fast(fileName, (DateTime)lastModifiedTime);
        }
    }

    /// <summary>
    /// Removes trailing period unless it's at the end of a single letter (eg. "Robin G.")
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    internal static string RemoveNonSemanticTrailingPeriod(this string value)
    {
        if (value.CharAppearsExactlyOnce('.') && value[^1] == '.' &&
            !((value.Length >= 3 && !char.IsWhiteSpace(value[^2]) && char.IsWhiteSpace(value[^3])) ||
              (value.Length == 2 && !char.IsWhiteSpace(value[^2]))))
        {
            value = value.Substring(0, value.Length - 1);
        }

        return value;
    }

    #region Get item by filename

    // Cached predicates for allocation avoidance
    private static readonly Predicate<NameAndIndex> MissFlagPredicate1 = static x => x.Name.PathEqualsI_AsciiSecond(FMFiles.StringsMissFlag);
    private static readonly Predicate<NameAndIndex> MissFlagPredicate2 = static x => x.Name.PathEqualsI_AsciiSecond(FMFiles.StringsEnglishMissFlag);
    private static readonly Predicate<NameAndIndex> MissFlagPredicate3 = static x => x.Name.PathEndsWithI_AsciiSecond(FMFiles.SMissFlag);

    internal static bool TryGetMissFlag(ListFast<NameAndIndex> list, out NameAndIndex result)
    {
        result = default;
        return list.Count > 0 &&
               (TryGetItem(list, MissFlagPredicate1, out result) ||
                TryGetItem(list, MissFlagPredicate2, out result) ||
                TryGetItem(list, MissFlagPredicate3, out result));
    }

    private static readonly Predicate<NameAndIndex> NewGameStrPredicate1 = static x => x.Name.PathEqualsI_AsciiSecond(FMFiles.IntrfaceEnglishNewGameStr);
    private static readonly Predicate<NameAndIndex> NewGameStrPredicate2 = static x => x.Name.PathEqualsI_AsciiSecond(FMFiles.IntrfaceNewGameStr);
    private static readonly Predicate<NameAndIndex> NewGameStrPredicate3 = static x => x.Name.PathEndsWithI_AsciiSecond(FMFiles.SNewGameStr);

    internal static bool TryGetNewGameStr(ListFast<NameAndIndex> list, out NameAndIndex result)
    {
        result = default;
        return list.Count > 0 &&
               (TryGetItem(list, NewGameStrPredicate1, out result) ||
                TryGetItem(list, NewGameStrPredicate2, out result) ||
                TryGetItem(list, NewGameStrPredicate3, out result));
    }

    internal static bool TryGetTitlesFile(ListFast<NameAndIndex> list, string[] locations, out NameAndIndex result)
    {
        result = default;
        if (list.Count == 0) return false;

        foreach (string location in locations)
        {
            for (int i = 0; i < list.Count; i++)
            {
                NameAndIndex item = list[i];
                if (item.Name.PathEqualsI_AsciiSecond(location))
                {
                    result = item;
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryGetItem(ListFast<NameAndIndex> list, Predicate<NameAndIndex> predicate, out NameAndIndex result)
    {
        for (int i = 0; i < list.Count; i++)
        {
            NameAndIndex item = list[i];
            if (predicate(item))
            {
                result = item;
                return true;
            }
        }

        result = default!;
        return false;
    }

    #endregion

    #region GLML

    private enum GLMLTagType
    {
        None,
        Opening,
        Closing,
    }

    internal static string GLMLToPlainText(string glml, ListFast<char> charBuffer)
    {
        // @MEM: We could cache these, and maybe even as ListFast<char>s to avoid the cruft of StringBuilder appending?
        StringBuilder sb = new(glml.Length);
        StringBuilder subSB = new(16);

        const char unicodeUnknownChar = '\u25A1';

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
                                ? uint.TryParse(num.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result)
                                : UInt_TryParseInv(num, out result);

                            if (success)
                            {
                                ListFast<char>? chars = ConvertFromUtf32(result, charBuffer);
                                if (chars == null)
                                {
                                    sb.Append(unicodeUnknownChar);
                                }
                                else
                                {
                                    sb.Append(chars.ItemsArray, 0, chars.Count);
                                }
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

                            if (HTML.HTML401NamedEntities.TryGetValue(name, out string value) &&
                                UInt_TryParseInv(value, out uint result))
                            {
                                ListFast<char>? chars = ConvertFromUtf32(result, charBuffer);
                                if (chars == null)
                                {
                                    sb.Append(unicodeUnknownChar);
                                }
                                else
                                {
                                    sb.Append(chars.ItemsArray, 0, chars.Count);
                                }
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

        static bool SBEquals(StringBuilder sb, string value)
        {
            if (sb.Length != value.Length) return false;
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] != value[i]) return false;
            }
            return true;
        }
    }

    #endregion
}

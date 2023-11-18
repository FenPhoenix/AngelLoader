using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static System.StringComparison;

namespace AL_Common;

public static partial class Common
{
    #region Methods

    /// <summary>
    /// Uses <see cref="StringComparison.Ordinal"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithO(this string str, string value) => str.StartsWith(value, Ordinal);

    /// <summary>
    /// Uses <see cref="StringComparison.Ordinal"/>.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithO(this string str, string value) => str.EndsWith(value, Ordinal);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithI(this string str, string value) => str.StartsWith(value, OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithI(this string str, string value) => str.EndsWith(value, OrdinalIgnoreCase);

    public static bool StartsWithI(this ReadOnlySpan<char> str, string value) => str.StartsWith(value, OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithI(this ReadOnlySpan<char> str, string value) => str.EndsWith(value, OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIniHeader(this string line) => !line.IsEmpty() && line[0] == '[' && line[^1] == ']';

    #region ASCII-specific

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BothAreAscii(char char1, char char2) => (char1 | char2) < 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiUpper(this char c) => (uint)(c - 'A') <= 'Z' - 'A';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiLower(this char c) => (uint)(c - 'a') <= 'z' - 'a';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIAscii(this char char1, char char2) =>
        char1 == char2 ||
        (char1.IsAsciiAlpha() && char2.IsAsciiAlpha() && (char1 & '_') == (char2 & '_'));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlpha(this char c) => (((uint)c - 'A') & ~0x20) < 26;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlpha(this byte b) => (((uint)b - 'A') & ~0x20) < 26;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiNumeric(this char c) => (uint)(c - '0') <= '9' - '0';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiNumeric(this byte c) => (uint)(c - '0') <= '9' - '0';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlphanumeric(this char c) => IsAsciiAlpha(c) || IsAsciiNumeric(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiHex(this byte b) =>
        b.IsAsciiNumeric() || ((((uint)b - 'A') & ~0x20) < 6);

    public static bool IsAsciiAlphaUpper(this string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (!str[i].IsAsciiUpper()) return false;
        }
        return true;
    }

    public static bool IsAsciiLower(this string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (c > 127 || IsAsciiUpper(c)) return false;
        }
        return true;
    }

    public static bool IsAsciiLower(this string str, int start)
    {
        for (int i = start; i < str.Length; i++)
        {
            char c = str[i];
            if (c > 127 || IsAsciiUpper(c)) return false;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [ContractAnnotation("null => true")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    #endregion

    #region Equals

    /// <summary>
    /// Determines whether this string and a specified <see langword="string"/> object have the same value.
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsI(this string first, string second) => first.Equals(second, OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this string and a specified <see langword="string"/> object have the same value.
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsI(this ReadOnlySpan<char> first, string second) => first.Equals(second, OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether this string and a specified <see langword="string"/> object have the same value.
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsI(this ReadOnlySpan<char> first, ReadOnlySpan<char> second) => first.Equals(second, OrdinalIgnoreCase);

    /// <summary>
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsTrue(this string value) => string.Equals(value, bool.TrueString, OrdinalIgnoreCase);

    /// <summary>
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsTrue(this ReadOnlySpan<char> value) => value.Equals(bool.TrueString, OrdinalIgnoreCase);

    /// <summary>
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsFalse(this ReadOnlySpan<char> value) => value.Equals(bool.FalseString, OrdinalIgnoreCase);

    /// <summary>
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="indexAfterEq"></param>
    /// <returns></returns>
    public static bool EndEqualsTrue(this string value, int indexAfterEq)
    {
        int valueLen = value.Length;
        return valueLen - indexAfterEq == 4 &&
               (value[valueLen - 4] == 'T' || value[valueLen - 4] == 't') &&
               (value[valueLen - 3] == 'R' || value[valueLen - 3] == 'r') &&
               (value[valueLen - 2] == 'U' || value[valueLen - 2] == 'u') &&
               (value[valueLen - 1] == 'E' || value[valueLen - 1] == 'e');
    }

    /// <summary>
    /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="indexAfterEq"></param>
    /// <returns></returns>
    public static bool EndEqualsFalse(this string value, int indexAfterEq)
    {
        int valueLen = value.Length;
        return valueLen - indexAfterEq == 5 &&
               (value[valueLen - 5] == 'F' || value[valueLen - 5] == 'f') &&
               (value[valueLen - 4] == 'A' || value[valueLen - 4] == 'a') &&
               (value[valueLen - 3] == 'L' || value[valueLen - 3] == 'l') &&
               (value[valueLen - 2] == 'S' || value[valueLen - 2] == 's') &&
               (value[valueLen - 1] == 'E' || value[valueLen - 1] == 'e');
    }

    /// <summary>
    /// Uses ASCII case-insensitivity. Should not be used with non-ASCII values!
    /// </summary>
    /// <param name="str"></param>
    /// <param name="str2"></param>
    /// <param name="indexAfterEq"></param>
    /// <returns></returns>
    public static bool ValueEqualsIAscii(this string str, string str2, int indexAfterEq)
    {
        int strLen = str.Length;
        int str2Len = str2.Length;

        if (strLen - indexAfterEq != str2Len) return false;

        for (int i = indexAfterEq; i < strLen; i++)
        {
            if (!str[i].EqualsIAscii(str2[i - indexAfterEq]))
            {
                return false;
            }
        }

        return true;
    }

    #endregion

    #region Contains

    /// <summary>
    /// Determines whether a string contains a specified substring. Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="substring"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsI(this string value, string substring) => value.Contains(substring, OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether a string[] contains a specified element. Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="substring"></param>
    /// <returns></returns>
    public static bool ContainsI(this string[] value, string substring)
    {
        for (int i = 0; i < value.Length; i++) if (value[i].Equals(substring, OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>
    /// Determines whether a List&lt;string&gt; contains a specified element. Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="substring"></param>
    /// <returns></returns>
    public static bool ContainsI(this List<string> value, string substring)
    {
        for (int i = 0; i < value.Count; i++) if (value[i].Equals(substring, OrdinalIgnoreCase)) return true;
        return false;
    }

    /// <summary>
    /// Very simple "fuzzy" search, only finds exact strings or strings present with other chars in between.
    /// <br/>
    /// Does no Levenshtein or anything fancy.
    /// </summary>
    /// <param name="hay"></param>
    /// <param name="needle"></param>
    /// <returns></returns>
    public static (bool Matched, bool ExactMatch)
    ContainsI_Subsequence(this string hay, string needle)
    {
        var fail = (false, false);

        int hayLength = hay.Length;
        int needleLength = needle.Length;

        if (needleLength == 0) return fail;
        // Don't do a needle > hay length check, because we want to support many duplicate chars (possibly
        // beyond the hay length) in the needle

        /*
        This algo sometimes rejects results that have the actual exact string in them, and if you try to
        tune it so it doesn't, then it gets other problems. It's just too simplistic to really work that
        well, so do a strict check first to cover that case.
        */
        if (hay.ContainsI(needle)) return (true, true);

        // Repetition everywhere so that we make sure only the ascii path runs if it's ascii, because with
        // big if((this and that) or that) statements, the non-ascii path always runs even if we're ascii and
        // blah.

        int needleUsed = 0;
        int skippedInARow = 0;

        int startIndex = -1;
        for (int i = 0; i < hayLength; i++)
        {
            if (BothAreAscii(hay[i], needle[0]))
            {
                if (hay[i].EqualsIAscii(needle[0]))
                {
                    startIndex = i;
                    break;
                }
            }
            else if (hay[i].EqualsIAscii(needle[0]) || hay[i].ToString().EqualsI(needle[0].ToString()))
            {
                startIndex = i;
                break;
            }
        }

        if (startIndex == -1) return fail;

        for (int i = startIndex; i < hayLength; ++i)
        {
            if (needleUsed == needleLength)
            {
                return (true, false);
            }

            if (skippedInARow > 2) return fail;

            char hayChar = hay[i];
            char needleChar = needle[needleUsed];

            // Don't allocate unless we need to...
            if (BothAreAscii(hayChar, needleChar))
            {
                if (hayChar.EqualsIAscii(needleChar))
                {
                    skippedInARow = 0;
                    char lastChar;
                    char currentChar;
                    do
                    {
                        ++needleUsed;
                        lastChar = needleChar;
                    } while (needleUsed < needleLength - 1 && ((currentChar = needle[needleUsed]).EqualsIAscii(lastChar) || char.IsWhiteSpace(currentChar)));
                }
                else if (!char.IsWhiteSpace(hayChar))
                {
                    ++skippedInARow;
                }
            }
            else
            {
                if (hayChar.EqualsIAscii(needleChar) || hayChar.ToString().EqualsI(needleChar.ToString()))
                {
                    skippedInARow = 0;
                    char lastChar;
                    do
                    {
                        ++needleUsed;
                        lastChar = needleChar;
                    } while (needleUsed < needleLength - 1 && needle[needleUsed].EqualsIAscii(lastChar));
                }
                else if (!char.IsWhiteSpace(hayChar))
                {
                    ++skippedInARow;
                }
            }
        }

        return (needleUsed == needleLength, false);
    }

    #endregion

    #region Count chars

    #region Disabled until needed

#if false

    /// <summary>
    /// Returns the number of times a character appears in a string.
    /// Avoids whatever silly overhead junk Count(predicate) is doing.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="character"></param>
    /// <returns></returns>
    public static int CountChars(this string value, char character)
    {
        int count = 0;
        for (int i = 0; i < value.Length; i++) if (value[i] == character) count++;

        return count;
    }

#endif

    #endregion

    /// <summary>
    /// Returns the number of times a character appears in a string, earlying-out once it's counted <paramref name="maxToCount"/>
    /// occurrences.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="character"></param>
    /// <param name="maxToCount">The maximum number of occurrences to count before earlying-out.</param>
    /// <returns></returns>
    public static int CountCharsUpToAmount(this string value, char character, int maxToCount)
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

    public static bool CharCountIsAtLeast(this string value, char character, int count)
    {
        int foundCount = 0;
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == character)
            {
                foundCount++;
                if (foundCount == count) return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CharAppearsExactlyOnce(this string value, char character) => value.CountCharsUpToAmount(character, 2) == 1;

    #endregion

    #endregion
}

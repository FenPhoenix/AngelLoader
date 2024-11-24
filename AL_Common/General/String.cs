﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static System.StringComparison;

namespace AL_Common;

public static partial class Common
{
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool StartsWithI(this ReadOnlySpan<char> str, string value) => str.StartsWith(value, OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EndsWithI(this ReadOnlySpan<char> str, string value) => str.EndsWith(value, OrdinalIgnoreCase);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsIniHeader(this string line) => !line.IsEmpty() && line[0] == '[' && line[^1] == ']';

    #region ASCII-specific

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BothAreAscii(char char1, char char2) => (char1 | char2) < 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToAsciiUpper(this char c)
    {
        if (char.IsAsciiLetterLower(c))
        {
            c = (char)(c & 0x5F); // = low 7 bits of ~0x20
        }
        return c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIAscii(this char char1, char char2) =>
        char1 == char2 ||
        (char.IsAsciiLetter(char1) && char.IsAsciiLetter(char2) && (char1 & '_') == (char2 & '_'));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlpha(this byte b) => char.IsAsciiLetter((char)b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiNumeric(this byte b) => char.IsAsciiDigit((char)b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiHex(this byte b) => char.IsAsciiHexDigit((char)b);

    public static bool IsAsciiAlphaUpper(this string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (!char.IsAsciiLetterUpper(str[i])) return false;
        }
        return true;
    }

    public static bool IsAsciiLower(this string str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (c > 127 || char.IsAsciiLetterUpper(c)) return false;
        }
        return true;
    }

    public static bool IsAsciiLower(this ReadOnlySpan<char> str)
    {
        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            if (c > 127 || char.IsAsciiLetterUpper(c)) return false;
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

    public static bool IsWhiteSpace(this ReadOnlySpan<byte> span)
    {
        for (int i = 0; i < span.Length; i++)
        {
            if (!char.IsWhiteSpace((char)span[i])) return false;
        }
        return true;
    }

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
        foreach (string str in value)
        {
            if (str.Equals(substring, OrdinalIgnoreCase)) return true;
        }
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
        for (int i = 0; i < value.Count; i++)
        {
            if (value[i].Equals(substring, OrdinalIgnoreCase)) return true;
        }
        return false;
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
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == character) count++;
        }

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

    /// <summary>
    /// Returns the number of times a character appears in a string, earlying-out once it's counted <paramref name="maxToCount"/>
    /// occurrences.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="character"></param>
    /// <param name="maxToCount">The maximum number of occurrences to count before earlying-out.</param>
    /// <returns></returns>
    public static int CountCharsUpToAmount(this ReadOnlySpan<char> value, char character, int maxToCount)
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
}

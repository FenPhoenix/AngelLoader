using System;
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

    /// <summary>
    /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool StartsWithI(this string str, string value)
    {
        if (str.IsEmpty()) return false;
        int valueLength = value.Length;
        if (str.Length < valueLength) return false;

        for (int si = 0, vi = 0; si < valueLength; si++, vi++)
        {
            char vc = value[vi];

            if (vc > 127)
            {
                return str.StartsWith(value, OrdinalIgnoreCase);
            }

            if (!str[si].EqualsIAscii(vc)) return false;
        }

        return true;
    }

    /// <summary>
    /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool EndsWithI(this string str, string value)
    {
        if (str.IsEmpty()) return false;
        int strLength = str.Length;
        int valueLength = value.Length;
        if (strLength < valueLength) return false;

        int start = strLength - valueLength;

        for (int si = start, vi = 0; si < strLength; si++, vi++)
        {
            char vc = value[vi];

            if (vc > 127)
            {
                return str.EndsWith(value, OrdinalIgnoreCase);
            }

            if (!str[si].EqualsIAscii(vc)) return false;
        }

        return true;
    }

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
    public static char ToAsciiUpper(this char c)
    {
        if (c.IsAsciiLower())
        {
            c = (char)(c & 0x5F); // = low 7 bits of ~0x20
        }
        return c;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIAscii(this char char1, char char2) =>
        char1 == char2 ||
        (char1.IsAsciiAlpha() && char2.IsAsciiAlpha() && (char1 & '_') == (char2 & '_'));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlpha(this char c) => (uint)((c | 0x20) - 'a') <= 'z' - 'a';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlpha(this byte b) => (uint)((b | 0x20) - 'a') <= 'z' - 'a';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiNumeric(this char c) => (uint)(c - '0') <= '9' - '0';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiNumeric(this byte c) => (uint)(c - '0') <= '9' - '0';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlphanumeric(this char c) => IsAsciiAlpha(c) || IsAsciiNumeric(c);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiAlphanumeric(this byte b) => IsAsciiAlpha(b) || IsAsciiNumeric(b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAsciiHex(this byte b)
    {
        char c = (char)b;
#if X64
        // Comment and code in this X64 block is from .NET 8

        // This code path, when used, has no branches and doesn't depend on cache hits,
        // so it's faster and does not vary in speed depending on input data distribution.
        // We only use this logic on 64-bit systems, as using 64 bit values would otherwise
        // be much slower than just using the lookup table anyway (no hardware support).
        // The magic constant 18428868213665201664 is a 64 bit value containing 1s at the
        // indices corresponding to all the valid hex characters (ie. "0123456789ABCDEFabcdef")
        // minus 48 (ie. '0'), and backwards (so from the most significant bit and downwards).
        // The offset of 48 for each bit is necessary so that the entire range fits in 64 bits.
        // First, we subtract '0' to the input digit (after casting to uint to account for any
        // negative inputs). Note that even if this subtraction underflows, this happens before
        // the result is zero-extended to ulong, meaning that `i` will always have upper 32 bits
        // equal to 0. We then left shift the constant with this offset, and apply a bitmask that
        // has the highest bit set (the sign bit) if and only if `c` is in the ['0', '0' + 64) range.
        // Then we only need to check whether this final result is less than 0: this will only be
        // the case if both `i` was in fact the index of a set bit in the magic constant, and also
        // `c` was in the allowed range (this ensures that false positive bit shifts are ignored).
        ulong i = (uint)c - '0';
        ulong shift = 18428868213665201664UL << (int)i;
        ulong mask = i - 64;

        return (long)(shift & mask) < 0;
#else
        return b.IsAsciiNumeric() || (uint)((b | 0x20) - 'a') < 6;
#endif
    }

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
    public static bool EqualsFalse(this string value) => string.Equals(value, bool.FalseString, OrdinalIgnoreCase);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Contains(this string value, char character) => value.IndexOf(character) >= 0;

    public static bool Contains(this string value, string substring, StringComparison comparison) =>
        // If substring is empty, IndexOf(string) returns 0, which would be a false "success" return
        !value.IsEmpty() && !substring.IsEmpty() && value.IndexOf(substring, comparison) >= 0;

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

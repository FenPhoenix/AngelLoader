using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AL_Common;

public static partial class Common
{
    // Meziantou.Polyfill's versions are less efficient (ie. > '0' && <'9' etc.) at the time of writing.
    extension(char)
    {
        /// <summary>
        /// Returns <see langword="true"/> if <paramref name="c"/> is an ASCII
        /// character ([ U+0000..U+007F ]).
        /// </summary>
        /// <remarks>
        /// Per http://www.unicode.org/glossary/#ASCII, ASCII is only U+0000..U+007F.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAscii(char c) => (uint)c <= '\x007f';

        /// <summary>Indicates whether a character is categorized as an ASCII digit.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII digit; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range '0' through '9', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiDigit(char c) => IsBetween(c, '0', '9');

        /// <summary>Indicates whether a character is categorized as an ASCII letter.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII letter; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range 'A' through 'Z', inclusive,
        /// or 'a' through 'z', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiLetter(char c) => (uint)((c | 0x20) - 'a') <= 'z' - 'a';

        /// <summary>Indicates whether a character is categorized as a lowercase ASCII letter.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is a lowercase ASCII letter; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range 'a' through 'z', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiLetterLower(char c) => IsBetween(c, 'a', 'z');

        /// <summary>Indicates whether a character is categorized as an uppercase ASCII letter.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is an uppercase ASCII letter; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range 'A' through 'Z', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiLetterUpper(char c) => IsBetween(c, 'A', 'Z');

        /// <summary>Indicates whether a character is categorized as an ASCII letter or digit.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is an ASCII letter or digit; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range 'A' through 'Z', inclusive,
        /// 'a' through 'z', inclusive, or '0' through '9', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiLetterOrDigit(char c) => IsAsciiLetter(c) | IsBetween(c, '0', '9');

        /// <summary>Indicates whether a character is categorized as an ASCII hexadecimal digit.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is a hexadecimal digit; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range '0' through '9', inclusive,
        /// 'A' through 'F', inclusive, or 'a' through 'f', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiHexDigit(char c) => IsHexChar(c);

        /// <summary>Indicates whether a character is categorized as an ASCII upper-case hexadecimal digit.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is a hexadecimal digit; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range '0' through '9', inclusive,
        /// or 'A' through 'F', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiHexDigitUpper(char c) => (uint)(c - '0') <= 9 || (uint)(c - 'A') <= ('F' - 'A');

        /// <summary>Indicates whether a character is categorized as an ASCII lower-case hexadecimal digit.</summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns><see langword="true"/> if <paramref name="c"/> is a lower-case hexadecimal digit; otherwise, <see langword="false"/>.</returns>
        /// <remarks>
        /// This determines whether the character is in the range '0' through '9', inclusive,
        /// or 'a' through 'f', inclusive.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiHexDigitLower(char c) => (uint)(c - '0') <= 9 || (uint)(c - 'a') <= ('f' - 'a');
    }

    /// <summary>Map from an ASCII char to its hex value, e.g. arr['b'] == 11. 0xFF means it's not a hex digit.</summary>
    internal static readonly byte[] CharToHexLookup =
    [
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 15
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 31
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 47
        0x0,  0x1,  0x2,  0x3,  0x4,  0x5,  0x6,  0x7,  0x8,  0x9,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 63
        0xFF, 0xA,  0xB,  0xC,  0xD,  0xE,  0xF,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 79
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 95
        0xFF, 0xa,  0xb,  0xc,  0xd,  0xe,  0xf,  0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 111
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 127
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 143
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 159
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 175
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 191
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 207
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 223
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 239
        0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, // 255
    ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsHexChar(int c)
    {
        if (IntPtr.Size == 8)
        {
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

            return (long)(shift & mask) < 0 ? true : false;
        }

        return FromChar(c) != 0xFF;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FromChar(int c)
    {
        return c >= CharToHexLookup.Length ? 0xFF : CharToHexLookup[c];
    }

    extension(Marshal)
    {
        /// <inheritdoc cref="Marshal.GetLastWin32Error"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetLastPInvokeError() => Marshal.GetLastWin32Error();
    }

    extension(Math)
    {
        public static double Cbrt(double d) => Math.Pow(d, 1f / 3f);
    }
}

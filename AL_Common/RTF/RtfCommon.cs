using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace AL_Common.RTF;

public static class RtfCommon
{
    public const int KeywordMaxLen = 32;
    // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
    public const int ParamMaxLen = 10;

    /// <summary>
    /// Since font numbers can be negative, let's just use a slightly less likely value than the already unlikely
    /// enough -1...
    /// </summary>
    public const int NoFontNumber = int.MinValue;

    public const ushort NoCodePage = ushort.MaxValue;

    public const int KeywordParseMaxRequiredBytes =
        KeywordMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1 +                 // Minus sign
        ParamMaxLen + 1 +   // +1 to read one beyond for length checking purposes
        1;                  // Space at end

    public const int KeywordVector128ParseMaxRequiredBytes =
        16 +              // Vector128<byte>.Count (no need for +1 for this codepath)
        1 +               // Minus sign
        ParamMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1;                // Space at end

    // "\bin"
    public const int BinLength = 4;
    public const uint BinUInt = 0x6E69625Cu;

    // Perf: On modern .NET, the "ReadOnlySpan<> x =>" pattern removes bounds checking (assuming you index with a
    // numeric type that's <= the length of the span), and generates only a tiny amount of asm. But on Framework,
    // the JIT doesn't recognize the pattern, and performance is catastrophic. So we have to use an old-fashioned
    // bounds-checked array.
    public static ReadOnlySpan<bool> IsNonPlainText =>
    [
        true, // '\0' (0)
        false, false, false, false, false, false, false, false, false,
        true, // '\n' (10)
        false, false,
        true, // '\r' (13)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false,
        true, // '\\' (92)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        true, // '{' (123)
        false,
        true, // '}' (125)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
    ];

    public static ReadOnlySpan<bool> IsIgnoreChar =>
    [
        true, // '\0' (0)
        false, false, false, false, false, false, false, false, false,
        true, // '\n' (10)
        false, false,
        true, // '\r' (13)
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false, false,
    ];

    #region SIMD

    public static readonly Vector512<byte> ZeroVector512 = Vector512.Create((byte)'\0');
    public static readonly Vector512<byte> LF_Vector512 = Vector512.Create((byte)'\n');
    public static readonly Vector512<byte> CR_Vector512 = Vector512.Create((byte)'\r');
    public static readonly Vector512<byte> BackslashVector512 = Vector512.Create((byte)'\\');
    public static readonly Vector512<byte> OpenBraceVector512 = Vector512.Create((byte)'{');
    public static readonly Vector512<byte> ClosingBraceVector512 = Vector512.Create((byte)'}');
    public static readonly Vector512<byte> n_Vector512 = Vector512.Create((byte)'n');
    // CreateSequence() was introduced in .NET 9, so since we support 8, we need to do it manually for now.
    public static readonly Vector512<byte> IndexVec_512 = Vector512.Create(
        (byte)
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
        32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47,
        48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63
    );

    public static readonly Vector256<byte> ZeroVector256 = Vector256.Create((byte)'\0');
    public static readonly Vector256<byte> LF_Vector256 = Vector256.Create((byte)'\n');
    public static readonly Vector256<byte> CR_Vector256 = Vector256.Create((byte)'\r');
    public static readonly Vector256<byte> BackslashVector256 = Vector256.Create((byte)'\\');
    public static readonly Vector256<byte> OpenBraceVector256 = Vector256.Create((byte)'{');
    public static readonly Vector256<byte> ClosingBraceVector256 = Vector256.Create((byte)'}');
    public static readonly Vector256<byte> n_Vector256 = Vector256.Create((byte)'n');
    public static readonly Vector256<byte> IndexVec_256 = Vector256.Create(
        (byte)
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
    );

    public static readonly Vector128<byte> ZeroVector128 = Vector128.Create((byte)'\0');
    public static readonly Vector128<byte> LF_Vector128 = Vector128.Create((byte)'\n');
    public static readonly Vector128<byte> CR_Vector128 = Vector128.Create((byte)'\r');
    public static readonly Vector128<byte> BackslashVector128 = Vector128.Create((byte)'\\');
    public static readonly Vector128<byte> OpenBraceVector128 = Vector128.Create((byte)'{');
    public static readonly Vector128<byte> ClosingBraceVector128 = Vector128.Create((byte)'}');
    public static readonly Vector128<byte> n_Vector128 = Vector128.Create((byte)'n');
    public static readonly Vector128<byte> IndexVec_128 = Vector128.Create(
        (byte)
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15
    );

    internal static ReadOnlySpan<byte> _isParEndingChar =>
    [
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        4, // '\n' (10)
        0, 0,
        4, // '\r' (13)
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        5, // ' ' (32)
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0,
        4, // '\\' (92)
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0,
    ];

    internal enum InterTextKeyword
    {
        Par,
        Tab,
    }

    // "\bin"
    internal const int _binLength = 4;
    internal static readonly uint _binUInt = BitConverter.IsLittleEndian ? 0x6E69625Cu : 0x5C62696Eu;
    // "\par " (ending space optional)
    internal const int _parMaxLength = 5;
    internal static readonly uint _parUInt = BitConverter.IsLittleEndian ? 0x7261705Cu : 0x5C706172u;
    // "\tab " (ending space optional)
    internal static readonly uint _tabUInt = BitConverter.IsLittleEndian ? 0x6261745Cu : 0x5C746162u;

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    internal static int SIMD_SkipDest(
        ref byte bufferRef,
        int startIndex,
        int spanLength)
    {
        if (!Vector512.IsHardwareAccelerated &&
            !Vector256.IsHardwareAccelerated &&
            !Vector128.IsHardwareAccelerated)
        {
            return -1;
        }

        if (Vector512.IsHardwareAccelerated && spanLength >= Vector512<byte>.Count)
        {
            ref byte searchSpace = ref Unsafe.AddByteOffset(ref bufferRef, startIndex);
            Vector512<byte> equalsBraces;
            Vector512<byte> equalsBackslash;
            Vector512<byte> equals;
            Vector512<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, spanLength - Vector512<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector512.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector512.Equals(OpenBraceVector512, current) | Vector512.Equals(ClosingBraceVector512, current);
                equalsBackslash = Vector512.Equals(BackslashVector512, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector512<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector512<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector512<byte>.Zero)
                {
                    ulong notEqualsElementsBackslash = equalsBackslash.ExtractMostSignificantBits();
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector512<byte>.Zero;
                    if (!bracesFound || (backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash)) < (bracesIndex = BitOperations.TrailingZeroCount(equalsBraces.ExtractMostSignificantBits())))
                    {
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector512<byte>.Count + (_binLength - 1)) <= spanLength)
                        {
                            Vector512<byte> lastBlock = Vector512.LoadUnsafe(ref Unsafe.AddByteOffset(ref currentSearchSpace, _binLength - 1));
                            Vector512<byte> lastEquals = Vector512.Equals(n_Vector512, lastBlock);

                            ulong mask = Vector512.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, BitOperations.TrailingZeroCount(mask));
                                if (index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, index)) == _binUInt)
                                {
                                    if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                            int currentVectorIndex = backslashIndex;
                            ulong mask = notEqualsElementsBackslash;
                            while (currentVectorIndex < Vector512<byte>.Count)
                            {
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, spanIndex)) == _binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector512<byte>.Count);
                            continue;
                        }
                        else
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                        }
                    }
                }

                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)spanLength % Vector512<byte>.Count != 0)
            {
                current = Vector512.LoadUnsafe(ref oneVectorAwayFromEnd);
                equalsBraces = Vector512.Equals(OpenBraceVector512, current) | Vector512.Equals(ClosingBraceVector512, current);
                equalsBackslash = Vector512.Equals(BackslashVector512, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector512<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }
        else if (Vector256.IsHardwareAccelerated && spanLength >= Vector256<byte>.Count)
        {
            ref byte searchSpace = ref Unsafe.AddByteOffset(ref bufferRef, startIndex);
            Vector256<byte> equalsBraces;
            Vector256<byte> equalsBackslash;
            Vector256<byte> equals;
            Vector256<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, spanLength - Vector256<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector256.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector256.Equals(OpenBraceVector256, current) | Vector256.Equals(ClosingBraceVector256, current);
                equalsBackslash = Vector256.Equals(BackslashVector256, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector256<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector256<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector256<byte>.Zero)
                {
                    uint notEqualsElementsBackslash = equalsBackslash.ExtractMostSignificantBits();
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector256<byte>.Zero;
                    if (!bracesFound || (backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash)) < (bracesIndex = BitOperations.TrailingZeroCount(equalsBraces.ExtractMostSignificantBits())))
                    {
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector256<byte>.Count + (_binLength - 1)) <= spanLength)
                        {
                            Vector256<byte> lastBlock = Vector256.LoadUnsafe(ref Unsafe.AddByteOffset(ref currentSearchSpace, _binLength - 1));
                            Vector256<byte> lastEquals = Vector256.Equals(n_Vector256, lastBlock);

                            uint mask = Vector256.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, BitOperations.TrailingZeroCount(mask));
                                if (index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, index)) == _binUInt)
                                {
                                    if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                            int currentVectorIndex = backslashIndex;
                            uint mask = notEqualsElementsBackslash;
                            while (currentVectorIndex < Vector256<byte>.Count)
                            {
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, spanIndex)) == _binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector256<byte>.Count);
                            continue;
                        }
                        else
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                        }
                    }
                }

                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)spanLength % Vector256<byte>.Count != 0)
            {
                current = Vector256.LoadUnsafe(ref oneVectorAwayFromEnd);
                equalsBraces = Vector256.Equals(OpenBraceVector256, current) | Vector256.Equals(ClosingBraceVector256, current);
                equalsBackslash = Vector256.Equals(BackslashVector256, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector256<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }
        else if (Vector128.IsHardwareAccelerated && spanLength >= Vector128<byte>.Count)
        {
            ref byte searchSpace = ref Unsafe.AddByteOffset(ref bufferRef, startIndex);
            Vector128<byte> equalsBraces;
            Vector128<byte> equalsBackslash;
            Vector128<byte> equals;
            Vector128<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, spanLength - Vector128<byte>.Count);

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Vector128.LoadUnsafe(ref currentSearchSpace);
                equalsBraces = Vector128.Equals(OpenBraceVector128, current) | Vector128.Equals(ClosingBraceVector128, current);
                equalsBackslash = Vector128.Equals(BackslashVector128, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector128<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector128<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector128<byte>.Zero)
                {
                    uint notEqualsElementsBackslash = equalsBackslash.ExtractMostSignificantBits();
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector128<byte>.Zero;
                    if (!bracesFound || (backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash)) < (bracesIndex = BitOperations.TrailingZeroCount(equalsBraces.ExtractMostSignificantBits())))
                    {
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector128<byte>.Count + (_binLength - 1)) <= spanLength)
                        {
                            Vector128<byte> lastBlock = Vector128.LoadUnsafe(ref Unsafe.AddByteOffset(ref currentSearchSpace, _binLength - 1));
                            Vector128<byte> lastEquals = Vector128.Equals(n_Vector128, lastBlock);

                            uint mask = Vector128.BitwiseAnd(equalsBackslash, lastEquals).ExtractMostSignificantBits();
                            while (mask != 0)
                            {
                                int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, BitOperations.TrailingZeroCount(mask));
                                if (index >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, index)) == _binUInt)
                                {
                                    if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ResetLowestSetBit(mask);
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = BitOperations.TrailingZeroCount(notEqualsElementsBackslash);
                            int currentVectorIndex = backslashIndex;
                            uint mask = notEqualsElementsBackslash;
                            while (currentVectorIndex < Vector128<byte>.Count)
                            {
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, spanIndex)) == _binUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }
                                mask = ResetLowestSetBit(mask);
                                currentVectorIndex = BitOperations.TrailingZeroCount(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector128<byte>.Count);
                            continue;
                        }
                        else
                        {
                            return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                        }
                    }
                }

                return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, equals);
            }
            while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));

            // If any elements remain, process the last vector in the search space.
            if ((uint)spanLength % Vector128<byte>.Count != 0)
            {
                current = Vector128.LoadUnsafe(ref oneVectorAwayFromEnd);
                equalsBraces = Vector128.Equals(OpenBraceVector128, current) | Vector128.Equals(ClosingBraceVector128, current);
                equalsBackslash = Vector128.Equals(BackslashVector128, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector128<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }

        return -1;
    }

    #region SIMD Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ResetLowestSetBit(uint value)
    {
        // It's lowered to BLSR on x86
        return value & (value - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ResetLowestSetBit(ulong value)
    {
        // It's lowered to BLSR on x86
        return value & (value - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector512<byte> equals)
    {
        ulong notEqualsElements = equals.ExtractMostSignificantBits();
        int index = BitOperations.TrailingZeroCount(notEqualsElements);
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector256<byte> equals)
    {
        uint notEqualsElements = equals.ExtractMostSignificantBits();
        int index = BitOperations.TrailingZeroCount(notEqualsElements);
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector128<byte> equals)
    {
        uint notEqualsElements = equals.ExtractMostSignificantBits();
        int index = BitOperations.TrailingZeroCount(notEqualsElements);
        return index + (int)(nuint)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ComputeFirstIndex(ref byte searchSpace, ref byte current, int index)
    {
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    #endregion

    #endregion

    // Total hack so we don't have to return and check a value eight trillion times (perf)
    public sealed class UnmatchedBraceException : Exception;

    // Set to a length that no reasonable font name would be above, to minimize the chance of having to do a slow
    // bounds-checked read-and-throw-away of the rest of the bytes.
    public const int MaxSymbolFontNameLength = 64;

    /// <summary>
    /// Returns the correct (clamped to 16) number of trailing zeros given a uint bitmask.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Vector128_TrailingZeroCount(uint value)
    {
        int tzc = BitOperations.TrailingZeroCount(value);
        return tzc > Vector128<byte>.Count
            ? Vector128<byte>.Count
            : tzc;
    }

    // Construct dynamically (static, so only done once) to cut down on file size for the .NET versions. We do
    // this because we can't do the ReadOnlySpan<> => trick with structs, so there's no gain in writing it all
    // out manually.
    internal static Vector128<byte>[] CreateVectorKeywordTable(Symbol?[] symbolTable)
    {
        Vector128<byte>[] ret = new Vector128<byte>[symbolTable.Length];

        Span<byte> keywordBytes = stackalloc byte[16];

        for (int symbolTableIndex = 0; symbolTableIndex < symbolTable.Length; symbolTableIndex++)
        {
            Symbol? symbol = symbolTable[symbolTableIndex];
            if (symbol == null)
            {
                ret[symbolTableIndex] = Vector128<byte>.Zero;
            }
            else
            {
                string keyword = symbol.Keyword;
                if (keyword.Length > 16)
                {
                    ret[symbolTableIndex] = Vector128<byte>.Zero;
                }
                else
                {
                    keywordBytes.Clear();
                    for (int i = 0; i < keyword.Length; i++)
                    {
                        keywordBytes[i] = (byte)keyword[i];
                    }
                    ret[symbolTableIndex] = Vector128.Create(keywordBytes);
                }
            }
        }

        return ret;
    }
}

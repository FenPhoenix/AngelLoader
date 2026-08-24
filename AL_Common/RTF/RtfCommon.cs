//#define PATCH_ALL_LANGS

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AL_Common.RTF;

public static class RtfCommon
{
    public const int KeywordMaxLen = 32;
    // Most are signed int16 (5 chars), but a few can be signed int32 (10 chars)
    public const int ParamMaxLen = 10;

    public const int UndefinedLanguage = 1024;

    /// <summary>
    /// Since font numbers can be negative, let's just use a slightly less likely value than the already unlikely
    /// enough -1...
    /// </summary>
    public const int NoFontNumber = int.MinValue;

    public const int MaxCodePageDigits = 5;
    public const int MaxLangNumIndex = 21514;
    public const ushort NoLang = ushort.MaxValue;

    public const ushort NoCodePage = ushort.MaxValue;

    public const int KeywordParseMaxRequiredBytes =
        KeywordMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1 + // Minus sign
        ParamMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1; // Space at end

    /*
    FMs can have 100+ of these...
    Highest measured was 131
    Fonts can specify themselves as whatever number they want, so we can't just count by index
    eg. you could have \f1 \f2 \f3 but you could also have \f1 \f14 \f45

    The size of a Dictionary<int, FontEntry> with 150 entries when FontEntry is 4 bytes would be approximately
    3 KB (from looking at its internal memory use per entry, which is larger than just the KeyValuePair<>).
    That's totally fine to keep around permanently.
    */
    public const int FontTableDefaultCapacity = 150;

    // "\bin"
    public const int BinLength = 4;
    public const uint BinUInt = 0x6E69625Cu;

    // Perf: On modern .NET, the "ReadOnlySpan<> x =>" pattern removes bounds checking (assuming you index with a
    // numeric type that's <= the length of the span), and generates only a tiny amount of asm. But on Framework,
    // the JIT doesn't recognize the pattern, and performance is catastrophic. So we have to use an old-fashioned
    // bounds-checked array.
    public static readonly bool[] IsNonPlainText =
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

    public static readonly bool[] IsIgnoreChar =
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

    #region Charset to code page

    public const int CharSetToCodePageLength = 256;

    public static readonly ushort[] CharSetToCodePage = RunFunc(static () =>
    {
        ushort[] charSetToCodePage = InitializedArray(CharSetToCodePageLength, NoCodePage);

        charSetToCodePage[0] = 1252;   // "ANSI" (1252) (Yes, this is specified as _explicitly_ 1252, so this
                                       // isn't a straggling 1252-default)

        charSetToCodePage[1] = 0;      // Default

        charSetToCodePage[2] = 42;     // Symbol
        charSetToCodePage[77] = 10000; // Mac Roman
        charSetToCodePage[78] = 10001; // Mac Shift Jis
        charSetToCodePage[79] = 10003; // Mac Hangul
        charSetToCodePage[80] = 10008; // Mac GB2312
        charSetToCodePage[81] = 10002; // Mac Big5
        //charSetToCodePage[82] = ?    // Mac Johab (old)
        charSetToCodePage[83] = 10005; // Mac Hebrew
        charSetToCodePage[84] = 10004; // Mac Arabic
        charSetToCodePage[85] = 10006; // Mac Greek
        charSetToCodePage[86] = 10081; // Mac Turkish
        charSetToCodePage[87] = 10021; // Mac Thai
        charSetToCodePage[88] = 10029; // Mac East Europe
        charSetToCodePage[89] = 10007; // Mac Russian
        charSetToCodePage[128] = 932;  // Shift JIS (Windows-31J) (932)
        charSetToCodePage[129] = 949;  // Hangul
        charSetToCodePage[130] = 1361; // Johab
        charSetToCodePage[134] = 936;  // GB2312
        charSetToCodePage[136] = 950;  // Big5
        charSetToCodePage[161] = 1253; // Greek
        charSetToCodePage[162] = 1254; // Turkish
        charSetToCodePage[163] = 1258; // Vietnamese
        charSetToCodePage[177] = 1255; // Hebrew
        charSetToCodePage[178] = 1256; // Arabic
        //charSetToCodePage[179] = ?   // Arabic Traditional (old)
        //charSetToCodePage[180] = ?   // Arabic user (old)
        //charSetToCodePage[181] = ?   // Hebrew user (old)
        charSetToCodePage[186] = 1257; // Baltic
        charSetToCodePage[204] = 1251; // Russian
        charSetToCodePage[222] = 874;  // Thai
        charSetToCodePage[238] = 1250; // Eastern European
        charSetToCodePage[254] = 437;  // PC 437
        charSetToCodePage[255] = 850;  // OEM

        return charSetToCodePage;
    });

    #endregion

    #region SIMD

    public static readonly Vector<byte> ZeroVector = new((byte)'\0');
    public static readonly Vector<byte> LF_Vector = new((byte)'\n');
    public static readonly Vector<byte> CR_Vector = new((byte)'\r');
    public static readonly Vector<byte> BackslashVector = new((byte)'\\');
    public static readonly Vector<byte> OpenBraceVector = new((byte)'{');
    public static readonly Vector<byte> ClosingBraceVector = new((byte)'}');
    public static readonly Vector<byte> n_Vector = new((byte)'n');
    public static readonly Vector<byte> SemicolonVector = new((byte)';');

    public const ulong XorPowerOfTwoToHighByte = (0x07ul |
                                                   0x06ul << 8 |
                                                   0x05ul << 16 |
                                                   0x04ul << 24 |
                                                   0x03ul << 32 |
                                                   0x02ul << 40 |
                                                   0x01ul << 48) + 1;

    // Vector length is unknowable at compile time, so make sure this program still runs on AVX2048 in 200 years
    public static readonly bool VectorLengthFitsInAByte = Vector<byte>.Count <= 256;
    public static readonly Vector<byte> IndexVec = RunFunc(static () =>
    {
        if (VectorLengthFitsInAByte)
        {
            byte[] bytes = new byte[Vector<byte>.Count];
            for (byte i = 0; i < Vector<byte>.Count; i++)
            {
                bytes[i] = i;
            }
            return new Vector<byte>(bytes);
        }
        else
        {
            return Vector<byte>.Zero;
        }
    });


    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    public static int SIMD_SkipDest(
    ref byte bufferRef,
    int startIndex,
    int spanLength)
    {
        if (!Vector.IsHardwareAccelerated || !VectorLengthFitsInAByte)
        {
            return -1;
        }

        if (spanLength >= Vector<byte>.Count)
        {
            ref byte searchSpace = ref Unsafe.AddByteOffset(ref bufferRef, (nint)startIndex);
            Vector<byte> equalsBraces;
            Vector<byte> equalsBackslash;
            Vector<byte> equals;
            Vector<byte> current;
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (nint)(spanLength - Vector<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                current = Unsafe.ReadUnaligned<Vector<byte>>(ref currentSearchSpace);
                equalsBraces = Vector.Equals(OpenBraceVector, current) | Vector.Equals(ClosingBraceVector, current);
                equalsBackslash = Vector.Equals(BackslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals == Vector<byte>.Zero)
                {
                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
                    continue;
                }

                if (equalsBackslash != Vector<byte>.Zero)
                {
                    int backslashIndex = -1;
                    int bracesIndex = 0;

                    bool bracesFound = equalsBraces != Vector<byte>.Zero;
                    if (!bracesFound || (backslashIndex = LocateFirstFoundByte(equalsBackslash)) < (bracesIndex = LocateFirstFoundByte(equalsBraces)))
                    {
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector<byte>.Count + (BinLength - 1)) <= spanLength)
                        {
                            Vector<byte> lastBlock = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref currentSearchSpace, BinLength - 1));
                            Vector<byte> lastEquals = Vector.Equals(n_Vector, lastBlock);

                            Vector<byte> containsBin = Vector.BitwiseAnd(equalsBackslash, lastEquals);

                            if (containsBin == Vector<byte>.Zero)
                            {
                                if (!bracesFound)
                                {
                                    currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
                                    continue;
                                }
                                else
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, bracesIndex);
                                }
                            }
                            else
                            {
                                Vector<byte> mask = Vector.BitwiseAnd(equalsBackslash, lastEquals);
                                while (mask != Vector<byte>.Zero)
                                {
                                    int vectorIndex = LocateFirstFoundByte(mask);
                                    int index = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, vectorIndex);
                                    if (index >= spanLength - sizeof(uint) ||
                                        Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, (nint)index)) == BinUInt)
                                    {
                                        if (backslashIndex == -1) backslashIndex = LocateFirstFoundByte(equalsBackslash);
                                        return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                    }

                                    mask = ClearMaskElementAtIndex(mask, vectorIndex);
                                }
                            }
                        }
                        else
                        {
                            if (backslashIndex == -1) backslashIndex = LocateFirstFoundByte(equalsBackslash);
                            int currentVectorIndex = backslashIndex;
                            Vector<byte> mask = ClearMaskElementAtIndex(equalsBackslash, currentVectorIndex);
                            while (currentVectorIndex < Vector<byte>.Count)
                            {
                                int spanIndex = ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, currentVectorIndex);
                                if (spanIndex >= spanLength - sizeof(uint) ||
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, (nint)spanIndex)) == BinUInt)
                                {
                                    return startIndex + ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, backslashIndex);
                                }

                                mask = ClearMaskElementAtIndex(mask, currentVectorIndex);
                                currentVectorIndex = LocateFirstFoundByte_VectorCountOnFail(mask);
                            }
                        }

                        if (!bracesFound)
                        {
                            currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
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
            if ((uint)spanLength % Vector<byte>.Count != 0)
            {
                current = Unsafe.ReadUnaligned<Vector<byte>>(ref oneVectorAwayFromEnd);
                equalsBraces = Vector.Equals(OpenBraceVector, current) | Vector.Equals(ClosingBraceVector, current);
                equalsBackslash = Vector.Equals(BackslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }

        return -1;
    }

    #region SIMD Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<byte> ClearMaskElementAtIndex(Vector<byte> mask, int index)
    {
        return Vector.BitwiseAnd(mask, Vector.LessThan(new Vector<byte>((byte)index), IndexVec));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector<byte> equals)
    {
        int index = LocateFirstFoundByte(equals);
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ComputeFirstIndex(ref byte searchSpace, ref byte current, int index)
    {
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LocateFirstFoundByte(Vector<byte> match)
    {
        Vector<ulong> vector64 = Vector.AsVectorUInt64(match);
        ulong candidate = 0;
        int i = 0;
        // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
        for (; i < Vector<ulong>.Count; i++)
        {
            candidate = vector64[i];
            if (candidate != 0)
            {
                break;
            }
        }

        // Single LEA instruction with jitted const (using function result)
        return i * 8 + LocateFirstFoundByte(candidate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LocateFirstFoundByte_VectorCountOnFail(Vector<byte> match)
    {
        Vector<ulong> vector64 = Vector.AsVectorUInt64(match);
        int i = 0;
        // Pattern unrolled by jit https://github.com/dotnet/coreclr/pull/8001
        for (; i < Vector<ulong>.Count; i++)
        {
            ulong candidate = vector64[i];
            if (candidate != 0)
            {
                // Single LEA instruction with jitted const (using function result)
                return i * 8 + LocateFirstFoundByte(candidate);
            }
        }

        return Vector<byte>.Count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LocateFirstFoundByte(ulong match)
    {
        // Flag least significant power of two bit
        ulong powerOfTwoFlag = match ^ (match - 1);
        // Shift all powers of two into the high byte and extract
        return (int)((powerOfTwoFlag * XorPowerOfTwoToHighByte) >> 57);
    }

    #endregion

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RTF_Array_IndexOfByte_Fast(byte[] array, byte value, int startIndex, int count)
    {
        /*
        On .NET, Array.IndexOf() uses crazy fast SIMD. On Framework, it normally doesn't.
        However, on Framework 64-bit only, we can make it use SIMD by using span.IndexOf(), if we reference the
        appropriate package (directly or indirectly), System.Memory or whatever it is.
        If we're 32-bit, though, SIMD is not supported, so we just stick to the regular Array.IndexOf(), which
        while substantially slower than the SIMD version, is still reasonably fast.

        But instead of checking for 64-bit vs. 32-bit, we can just check directly if SIMD is supported.
        */
        if (Vector.IsHardwareAccelerated)
        {
            int index = array.AsSpan(startIndex, count).IndexOf(value);
            if (index > -1) index += startIndex;
            return index;
        }
        else
        {
            return Array.IndexOf(array, value, startIndex, count);
        }
    }

    // Total hack so we don't have to return and check a value eight trillion times (perf)
    public sealed class UnmatchedBraceException : Exception;

    public readonly struct FontNameData
    {
        public readonly byte[] Bytes;
        public readonly ushort CodePage;
        public readonly byte[] CodePageBytes;

        public FontNameData(byte[] bytes, ushort codePage, byte[] codePageBytes)
        {
            Bytes = bytes;
            CodePage = codePage;
            CodePageBytes = codePageBytes;
        }
    }

    // All code pages are 4 in length plus 1 for the keyword-ending space, and since these are old-style RTF and
    // the RTF spec hasn't been updated since 2008 in any case, it's basically impossible that this ever changes.
    // So let's be efficient and use a constant.
    public const int FontNameSuffixCodePageLength = 5;

    public static readonly FontNameData[] FontNameSuffixes =
    [
        // IMPORTANT: Spaces at the end serve as the keyword-ending spaces for safety. Do not remove!
        new FontNameData(" Baltic"u8.ToArray(), 1257, "1257 "u8.ToArray()),
        new FontNameData(" CE"u8.ToArray(), 1250, "1250 "u8.ToArray()),
        new FontNameData(" Cyr"u8.ToArray(), 1251, "1251 "u8.ToArray()),
        new FontNameData(" Greek"u8.ToArray(), 1253, "1253 "u8.ToArray()),
        new FontNameData(" Tur"u8.ToArray(), 1254, "1254 "u8.ToArray()),
        new FontNameData(" (Hebrew)"u8.ToArray(), 1255, "1255 "u8.ToArray()),
        new FontNameData(" (Arabic)"u8.ToArray(), 1256, "1256 "u8.ToArray()),
        new FontNameData(" (Vietnamese)"u8.ToArray(), 1258, "1258 "u8.ToArray()),
    ];

    // Set to a length that no reasonable font name would be above, to minimize the chance of having to do a slow
    // bounds-checked read-and-throw-away of the rest of the bytes.
    public const int MaxSymbolFontNameLength = 64;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNonEmptyUShortParam(int value)
    {
        /*
        The whole ushort range except 0xFFFF - that's our value for "not set" (-1 equivalent). As 0xFFFF (65535)
        is not a valid codepage in either the RTF spec or .NET (any version), we can hijack it for this
        purpose without issue.
        */
        return (uint)(value - ushort.MinValue) <= (ushort.MaxValue - 1) - ushort.MinValue;
    }
}

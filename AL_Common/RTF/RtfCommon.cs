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

    public const int MaxLangNumDigits = 5;
    public const int MaxLangNumIndex = 16385;
    public const ushort NoLang = ushort.MaxValue;

    public const ushort NoCodePage = ushort.MaxValue;

    public const int KeywordParseMaxRequiredBytes =
        KeywordMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1 + // Minus sign
        ParamMaxLen + 1 + // +1 to read one beyond for length checking purposes
        1; // Space at end

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

    // This is fine to use the "ReadOnlySpan<> x =>" pattern with though, because we only take a reference to it
    // and then just use the reference from then on.
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

    public static readonly ushort[] LangToCodePage = RunFunc(static () =>
    {
        ushort[] langToCodePage = InitializedArray(MaxLangNumIndex + 1, NoCodePage);

        /*
        There's a ton more languages than this, but it's not clear what code page they all translate to.
        This should be enough to get on with for now though...

        Note: 1024 is implicitly rejected by simply not being in the list, so we're all good there.

        2023-03-31: Only handle 1049 for now (and leave in 1033 for the plaintext converter).
        */

#if false
        // Arabic
        langToCodePage[1065] = 1256;
        langToCodePage[1025] = 1256;
        langToCodePage[2049] = 1256;
        langToCodePage[3073] = 1256;
        langToCodePage[4097] = 1256;
        langToCodePage[5121] = 1256;
        langToCodePage[6145] = 1256;
        langToCodePage[7169] = 1256;
        langToCodePage[8193] = 1256;
        langToCodePage[9217] = 1256;
        langToCodePage[10241] = 1256;
        langToCodePage[11265] = 1256;
        langToCodePage[12289] = 1256;
        langToCodePage[13313] = 1256;
        langToCodePage[14337] = 1256;
        langToCodePage[15361] = 1256;
        langToCodePage[16385] = 1256;
        langToCodePage[1056] = 1256;
        langToCodePage[2118] = 1256;
        langToCodePage[2137] = 1256;
        langToCodePage[1119] = 1256;
        langToCodePage[1120] = 1256;
        langToCodePage[1123] = 1256;
        langToCodePage[1164] = 1256;
#endif

        // Cyrillic
        langToCodePage[1049] = 1251;
#if false
        langToCodePage[1026] = 1251;
        langToCodePage[10266] = 1251;
        langToCodePage[1058] = 1251;
        langToCodePage[2073] = 1251;
        langToCodePage[3098] = 1251;
        langToCodePage[7194] = 1251;
        langToCodePage[8218] = 1251;
        langToCodePage[12314] = 1251;
        langToCodePage[1059] = 1251;
        langToCodePage[1064] = 1251;
        langToCodePage[2092] = 1251;
        langToCodePage[1071] = 1251;
        langToCodePage[1087] = 1251;
        langToCodePage[1088] = 1251;
        langToCodePage[2115] = 1251;
        langToCodePage[1092] = 1251;
        langToCodePage[1104] = 1251;
        langToCodePage[1133] = 1251;
        langToCodePage[1157] = 1251;

        // Greek
        langToCodePage[1032] = 1253;

        // Hebrew
        langToCodePage[1037] = 1255;
        langToCodePage[1085] = 1255;

        // Vietnamese
        langToCodePage[1066] = 1258;
#endif

        // Western European
        langToCodePage[1033] = 1252;

        return langToCodePage;
    });

    #region Charset to code page

    public const int CharSetToCodePageLength = 256;

    public static readonly ushort[] CharSetToCodePage =
    [
        1252, // 0 - "ANSI" (1252) (Yes, this is specified as _explicitly_ 1252, so this isn't a straggling 1252-default)
        0, // 1 - Default
        42, // 2 - Symbol
        NoCodePage, // 3
        NoCodePage, // 4
        NoCodePage, // 5
        NoCodePage, // 6
        NoCodePage, // 7
        NoCodePage, // 8
        NoCodePage, // 9
        NoCodePage, // 10
        NoCodePage, // 11
        NoCodePage, // 12
        NoCodePage, // 13
        NoCodePage, // 14
        NoCodePage, // 15
        NoCodePage, // 16
        NoCodePage, // 17
        NoCodePage, // 18
        NoCodePage, // 19
        NoCodePage, // 20
        NoCodePage, // 21
        NoCodePage, // 22
        NoCodePage, // 23
        NoCodePage, // 24
        NoCodePage, // 25
        NoCodePage, // 26
        NoCodePage, // 27
        NoCodePage, // 28
        NoCodePage, // 29
        NoCodePage, // 30
        NoCodePage, // 31
        NoCodePage, // 32
        NoCodePage, // 33
        NoCodePage, // 34
        NoCodePage, // 35
        NoCodePage, // 36
        NoCodePage, // 37
        NoCodePage, // 38
        NoCodePage, // 39
        NoCodePage, // 40
        NoCodePage, // 41
        NoCodePage, // 42
        NoCodePage, // 43
        NoCodePage, // 44
        NoCodePage, // 45
        NoCodePage, // 46
        NoCodePage, // 47
        NoCodePage, // 48
        NoCodePage, // 49
        NoCodePage, // 50
        NoCodePage, // 51
        NoCodePage, // 52
        NoCodePage, // 53
        NoCodePage, // 54
        NoCodePage, // 55
        NoCodePage, // 56
        NoCodePage, // 57
        NoCodePage, // 58
        NoCodePage, // 59
        NoCodePage, // 60
        NoCodePage, // 61
        NoCodePage, // 62
        NoCodePage, // 63
        NoCodePage, // 64
        NoCodePage, // 65
        NoCodePage, // 66
        NoCodePage, // 67
        NoCodePage, // 68
        NoCodePage, // 69
        NoCodePage, // 70
        NoCodePage, // 71
        NoCodePage, // 72
        NoCodePage, // 73
        NoCodePage, // 74
        NoCodePage, // 75
        NoCodePage, // 76
        10000, // 77 - Mac Roman
        10001, // 78 - Mac Shift Jis
        10003, // 79 - Mac Hangul
        10008, // 80 - Mac GB2312
        10002, // 81 - Mac Big5
        NoCodePage, // 82 - Mac Johab (old) (codepage unknown)
        10005, // 83 - Mac Hebrew
        10004, // 84 - Mac Arabic
        10006, // 85 - Mac Greek
        10081, // 86 - Mac Turkish
        10021, // 87 - Mac Thai
        10029, // 88 - Mac East Europe
        10007, // 89 - Mac Russian
        NoCodePage, // 90
        NoCodePage, // 91
        NoCodePage, // 92
        NoCodePage, // 93
        NoCodePage, // 94
        NoCodePage, // 95
        NoCodePage, // 96
        NoCodePage, // 97
        NoCodePage, // 98
        NoCodePage, // 99
        NoCodePage, // 100
        NoCodePage, // 101
        NoCodePage, // 102
        NoCodePage, // 103
        NoCodePage, // 104
        NoCodePage, // 105
        NoCodePage, // 106
        NoCodePage, // 107
        NoCodePage, // 108
        NoCodePage, // 109
        NoCodePage, // 110
        NoCodePage, // 111
        NoCodePage, // 112
        NoCodePage, // 113
        NoCodePage, // 114
        NoCodePage, // 115
        NoCodePage, // 116
        NoCodePage, // 117
        NoCodePage, // 118
        NoCodePage, // 119
        NoCodePage, // 120
        NoCodePage, // 121
        NoCodePage, // 122
        NoCodePage, // 123
        NoCodePage, // 124
        NoCodePage, // 125
        NoCodePage, // 126
        NoCodePage, // 127
        932, // 128 - Shift JIS (Windows-31J) (932)
        949, // 129 - Hangul
        1361, // 130 - Johab
        NoCodePage, // 131
        NoCodePage, // 132
        NoCodePage, // 133
        936, // 134 - GB2312
        NoCodePage, // 135
        950, // 136 - Big5
        NoCodePage, // 137
        NoCodePage, // 138
        NoCodePage, // 139
        NoCodePage, // 140
        NoCodePage, // 141
        NoCodePage, // 142
        NoCodePage, // 143
        NoCodePage, // 144
        NoCodePage, // 145
        NoCodePage, // 146
        NoCodePage, // 147
        NoCodePage, // 148
        NoCodePage, // 149
        NoCodePage, // 150
        NoCodePage, // 151
        NoCodePage, // 152
        NoCodePage, // 153
        NoCodePage, // 154
        NoCodePage, // 155
        NoCodePage, // 156
        NoCodePage, // 157
        NoCodePage, // 158
        NoCodePage, // 159
        NoCodePage, // 160
        1253, // 161 - Greek
        1254, // 162 - Turkish
        1258, // 163 - Vietnamese
        NoCodePage, // 164
        NoCodePage, // 165
        NoCodePage, // 166
        NoCodePage, // 167
        NoCodePage, // 168
        NoCodePage, // 169
        NoCodePage, // 170
        NoCodePage, // 171
        NoCodePage, // 172
        NoCodePage, // 173
        NoCodePage, // 174
        NoCodePage, // 175
        NoCodePage, // 176
        1255, // 177 - Hebrew
        1256, // 178 - Arabic
        NoCodePage, // 179 - Arabic Traditional (old) (codepage unknown)
        NoCodePage, // 180 - Arabic user (old) (codepage unknown)
        NoCodePage, // 181 - Hebrew user (old) (codepage unknown)
        NoCodePage, // 182
        NoCodePage, // 183
        NoCodePage, // 184
        NoCodePage, // 185
        1257, // 186 - Baltic
        NoCodePage, // 187
        NoCodePage, // 188
        NoCodePage, // 189
        NoCodePage, // 190
        NoCodePage, // 191
        NoCodePage, // 192
        NoCodePage, // 193
        NoCodePage, // 194
        NoCodePage, // 195
        NoCodePage, // 196
        NoCodePage, // 197
        NoCodePage, // 198
        NoCodePage, // 199
        NoCodePage, // 200
        NoCodePage, // 201
        NoCodePage, // 202
        NoCodePage, // 203
        1251, // 204 - Russian
        NoCodePage, // 205
        NoCodePage, // 206
        NoCodePage, // 207
        NoCodePage, // 208
        NoCodePage, // 209
        NoCodePage, // 210
        NoCodePage, // 211
        NoCodePage, // 212
        NoCodePage, // 213
        NoCodePage, // 214
        NoCodePage, // 215
        NoCodePage, // 216
        NoCodePage, // 217
        NoCodePage, // 218
        NoCodePage, // 219
        NoCodePage, // 220
        NoCodePage, // 221
        874, // 222 - Thai
        NoCodePage, // 223
        NoCodePage, // 224
        NoCodePage, // 225
        NoCodePage, // 226
        NoCodePage, // 227
        NoCodePage, // 228
        NoCodePage, // 229
        NoCodePage, // 230
        NoCodePage, // 231
        NoCodePage, // 232
        NoCodePage, // 233
        NoCodePage, // 234
        NoCodePage, // 235
        NoCodePage, // 236
        NoCodePage, // 237
        1250, // 238 - Eastern European
        NoCodePage, // 239
        NoCodePage, // 240
        NoCodePage, // 241
        NoCodePage, // 242
        NoCodePage, // 243
        NoCodePage, // 244
        NoCodePage, // 245
        NoCodePage, // 246
        NoCodePage, // 247
        NoCodePage, // 248
        NoCodePage, // 249
        NoCodePage, // 250
        NoCodePage, // 251
        NoCodePage, // 252
        NoCodePage, // 253
        437, // 254 - PC 437
        850, // 255 - OEM
    ];

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
}

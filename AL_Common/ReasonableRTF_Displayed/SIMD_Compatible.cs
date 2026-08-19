/*
 * The MIT License (MIT)
 * 
 * Copyright (c) .NET Foundation and Contributors
 * 
 * All rights reserved.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
*/

using System.Numerics;
using System.Runtime.CompilerServices;

namespace ReasonableRTF_Displayed;

public sealed partial class RRTF_RtfDisplayedReadmeParser
{
    #region Private fields

    private static readonly Vector<byte> _zeroVector = new((byte)'\0');
    private static readonly Vector<byte> _lfVector = new((byte)'\n');
    private static readonly Vector<byte> _crVector = new((byte)'\r');
    private static readonly Vector<byte> _backslashVector = new((byte)'\\');
    private static readonly Vector<byte> _openBraceVector = new((byte)'{');
    private static readonly Vector<byte> _closingBraceVector = new((byte)'}');
    private static readonly Vector<byte> _nVector = new((byte)'n');

    private const ulong XorPowerOfTwoToHighByte = (0x07ul |
                                                   0x06ul << 8 |
                                                   0x05ul << 16 |
                                                   0x04ul << 24 |
                                                   0x03ul << 32 |
                                                   0x02ul << 40 |
                                                   0x01ul << 48) + 1;

    // Vector length is unknowable at compile time, so make sure this program still runs on AVX2048 in 200 years
    private static readonly bool _vectorLengthFitsInAByte = Vector<byte>.Count <= 256;
    private static Vector<byte> InitIndexVec()
    {
        if (_vectorLengthFitsInAByte)
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
    }
    private static readonly Vector<byte> _indexVec = InitIndexVec();

    #endregion

    #region API

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    // Made to handle the \binN situation while losing as little performance as possible.
    private int SIMD_SkipDest(
        ref byte bufferRef,
        int startIndex,
        int spanLength)
    {
        if (!Vector.IsHardwareAccelerated || !_vectorLengthFitsInAByte)
        {
            return -1;
        }

        if (spanLength >= Vector<byte>.Count)
        {
            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);
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
                equalsBraces = Vector.Equals(_openBraceVector, current) | Vector.Equals(_closingBraceVector, current);
                equalsBackslash = Vector.Equals(_backslashVector, current);
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
                        if (ComputeFirstIndex(ref searchSpace, ref currentSearchSpace, Vector<byte>.Count + (_binLength - 1)) <= spanLength)
                        {
                            Vector<byte> lastBlock = Unsafe.ReadUnaligned<Vector<byte>>(ref Unsafe.AddByteOffset(ref currentSearchSpace, _binLength - 1));
                            Vector<byte> lastEquals = Vector.Equals(_nVector, lastBlock);

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
                                        Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, (nint)index)) == _binUInt)
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
                                    Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref searchSpace, (nint)spanIndex)) == _binUInt)
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
                equalsBraces = Vector.Equals(_openBraceVector, current) | Vector.Equals(_closingBraceVector, current);
                equalsBackslash = Vector.Equals(_backslashVector, current);
                equals = equalsBraces | equalsBackslash;
                if (equals != Vector<byte>.Zero)
                {
                    return startIndex + ComputeFirstIndex(ref searchSpace, ref oneVectorAwayFromEnd, equals);
                }
            }
        }

        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector<byte> ClearMaskElementAtIndex(Vector<byte> mask, int index)
    {
        return Vector.BitwiseAnd(mask, Vector.LessThan(new Vector<byte>((byte)index), _indexVec));
    }

    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    /*
    NOTE: The reason we're not doing the \par-supporting copy like the modern .NET version is because it needs
    the Shuffle instruction and that's not available on Framework. Also, we're not doing the thing where we set
    the current position for the next vector load to just after the \par either, because that's still slower for
    some reason. Too many overlapping loads just like the keyword thing, I guess.
    */
    private bool SIMD_SkipPlainText(ref byte bufferRef)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            return false;
        }

        int startIndex = _currentPos;
        int spanLength = _rtfBytesLength - _currentPos;

        ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);

        if (spanLength >= Vector<byte>.Count)
        {
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (uint)(spanLength - Vector<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector<byte> current = Unsafe.ReadUnaligned<Vector<byte>>(ref currentSearchSpace);

                Vector<byte> equals =
                    Vector.Equals(_zeroVector, current) |
                    Vector.Equals(_lfVector, current) |
                    Vector.Equals(_crVector, current) |
                    Vector.Equals(_backslashVector, current) |
                    Vector.Equals(_openBraceVector, current) |
                    Vector.Equals(_closingBraceVector, current);

                if (equals != Vector<byte>.Zero)
                {
                    int index = LocateFirstFoundByte(equals);
                    if (index > 0)
                    {
                        _currentPos += index;
                    }

                    return true;
                }

                _currentPos += Vector<byte>.Count;
                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }

        return false;
    }

    #endregion

    #region Helpers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, Vector<byte> equals)
    {
        int index = LocateFirstFoundByte(equals);
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeFirstIndex(ref byte searchSpace, ref byte current, int index)
    {
        return index + (int)Unsafe.ByteOffset(ref searchSpace, ref current);
    }

    // Vector sub-search adapted from https://github.com/aspnet/KestrelHttpServer/pull/1138
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int LocateFirstFoundByte(Vector<byte> match)
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
    private static int LocateFirstFoundByte_VectorCountOnFail(Vector<byte> match)
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
    private static int LocateFirstFoundByte(ulong match)
    {
        // Flag least significant power of two bit
        ulong powerOfTwoFlag = match ^ (match - 1);
        // Shift all powers of two into the high byte and extract
        return (int)((powerOfTwoFlag * XorPowerOfTwoToHighByte) >> 57);
    }

    #endregion
}

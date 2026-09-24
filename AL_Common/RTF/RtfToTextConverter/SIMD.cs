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
using System.Runtime.Intrinsics;
using static AL_Common.RTF.RtfCommon;

namespace AL_Common.RTF;

public sealed partial class RtfToTextConverter
{
    // Heavily modified version of .NET SpanHelpers.IndexOfAnyValueType().
    /*
    NOTE: The reason we're not doing the \par-supporting copy like the modern .NET version is because it needs
    the Shuffle instruction and that's not available on Framework. Also, we're not doing the thing where we set
    the current position for the next vector load to just after the \par either, because that's still slower for
    some reason. Too many overlapping loads just like the keyword thing, I guess.
    */
    private bool SIMD_CopyPlainText(ref byte bufferRef)
    {
        if (!Vector.IsHardwareAccelerated)
        {
            return false;
        }

        int currentPosLocal = _currentPos;

        int startIndex = currentPosLocal;
        int spanLength = _currentBufferChunkLength - startIndex;

        if (Vector512.IsHardwareAccelerated && spanLength >= Vector512<byte>.Count)
        {
            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (uint)(spanLength - Vector512<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector512<byte> current = Vector512.LoadUnsafe(ref currentSearchSpace);

                Vector512<byte> equalsBackslash =
                    Vector512.Equals(BackslashVector512, current);

                Vector512<byte> equalsBraces =
                    Vector512.Equals(OpenBraceVector512, current) |
                    Vector512.Equals(ClosingBraceVector512, current);

                Vector512<byte> equalsLineBreaks =
                    Vector512.Equals(LF_Vector512, current) |
                    Vector512.Equals(CR_Vector512, current);

                Vector512<byte> equalsZeroBytes =
                    Vector512.Equals(ZeroVector512, current);

                Vector512<byte> equalsOtherExceptLineBreaks =
                    equalsZeroBytes |
                    equalsBraces;

                Vector512<byte> equals =
                    equalsZeroBytes |
                    equalsLineBreaks |
                    equalsBraces |
                    equalsBackslash;

                Vector512<byte> equalsBackslashOrLineBreak = equalsBackslash | equalsLineBreaks;

                if (equalsBackslash != Vector512<byte>.Zero && equalsOtherExceptLineBreaks == Vector512<byte>.Zero)
                {
                    ulong mask = equalsBackslashOrLineBreak.ExtractMostSignificantBits();
                    uint shiftLeftCount = 0;
                    while (true)
                    {
                        int parLength;
                        byte index = (byte)BitOperations.TrailingZeroCount(mask);
                        if (index == Vector512<byte>.Count)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            currentPosLocal += index;
                            break;
                        }
                        else if (index >= Vector512<byte>.Count - _parMaxLength)
                        {
                            if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                            {
                                parLength = 1;
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                            }
                            else
                            {
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                                currentPosLocal += index;
                                if (currentPosLocal < _currentBufferChunkLength - _parMaxLength)
                                {
                                    if (IsPar(ref currentSearchSpace, index, out parLength, out InterTextKeyword interTextKeyword))
                                    {
                                        currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                        currentPosLocal += parLength;
                                        if (interTextKeyword == InterTextKeyword.Par)
                                        {
                                            AddLineBreak();
                                        }
                                        else
                                        {
                                            PlainText_Add('\t');
                                        }
                                        goto outerLoop;
                                    }
                                }

                                _currentPos = currentPosLocal;
                                return true;
                            }
                        }
                        else if (IsPar(ref currentSearchSpace, index, out int length, out InterTextKeyword interTextKeyword))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            if (interTextKeyword == InterTextKeyword.Par)
                            {
                                AddLineBreak();
                            }
                            else
                            {
                                PlainText_Add('\t');
                            }
                        }
                        else if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos = currentPosLocal + index;
                            return true;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector512<byte>.Zero)
                {
                    int index = BitOperations.TrailingZeroCount(equals.ExtractMostSignificantBits());
                    _currentPos = currentPosLocal + index;
                    if (index == 0) return true;
                    CopyVector(current, index);
                    return true;
                }
                else
                {
                    CopyVector(current, Vector512<byte>.Count);
                    currentPosLocal += Vector512<byte>.Count;
                }

                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector512<byte>.Count);
                outerLoop:;
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector256.IsHardwareAccelerated && spanLength >= Vector256<byte>.Count)
        {
            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (uint)(spanLength - Vector256<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector256<byte> current = Vector256.LoadUnsafe(ref currentSearchSpace);

                Vector256<byte> equalsBackslash =
                    Vector256.Equals(BackslashVector256, current);

                Vector256<byte> equalsBraces =
                    Vector256.Equals(OpenBraceVector256, current) |
                    Vector256.Equals(ClosingBraceVector256, current);

                Vector256<byte> equalsLineBreaks =
                    Vector256.Equals(LF_Vector256, current) |
                    Vector256.Equals(CR_Vector256, current);

                Vector256<byte> equalsZeroBytes =
                    Vector256.Equals(ZeroVector256, current);

                Vector256<byte> equalsOtherExceptLineBreaks =
                    equalsZeroBytes |
                    equalsBraces;

                Vector256<byte> equals =
                    equalsZeroBytes |
                    equalsLineBreaks |
                    equalsBraces |
                    equalsBackslash;

                Vector256<byte> equalsBackslashOrLineBreak = equalsBackslash | equalsLineBreaks;

                if (equalsBackslash != Vector256<byte>.Zero && equalsOtherExceptLineBreaks == Vector256<byte>.Zero)
                {
                    uint mask = equalsBackslashOrLineBreak.ExtractMostSignificantBits();
                    uint shiftLeftCount = 0;
                    while (true)
                    {
                        int parLength;
                        byte index = (byte)BitOperations.TrailingZeroCount(mask);
                        if (index == Vector256<byte>.Count)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            currentPosLocal += index;
                            break;
                        }
                        else if (index >= Vector256<byte>.Count - _parMaxLength)
                        {
                            if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                            {
                                parLength = 1;
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                            }
                            else
                            {
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                                currentPosLocal += index;
                                if (currentPosLocal < _currentBufferChunkLength - _parMaxLength)
                                {
                                    if (IsPar(ref currentSearchSpace, index, out parLength, out InterTextKeyword interTextKeyword))
                                    {
                                        currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                        currentPosLocal += parLength;
                                        if (interTextKeyword == InterTextKeyword.Par)
                                        {
                                            AddLineBreak();
                                        }
                                        else
                                        {
                                            PlainText_Add('\t');
                                        }
                                        goto outerLoop;
                                    }
                                }

                                _currentPos = currentPosLocal;
                                return true;
                            }
                        }
                        else if (IsPar(ref currentSearchSpace, index, out int length, out InterTextKeyword interTextKeyword))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            if (interTextKeyword == InterTextKeyword.Par)
                            {
                                AddLineBreak();
                            }
                            else
                            {
                                PlainText_Add('\t');
                            }
                        }
                        else if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos = currentPosLocal + index;
                            return true;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector256<byte>.Zero)
                {
                    int index = BitOperations.TrailingZeroCount(equals.ExtractMostSignificantBits());
                    _currentPos = currentPosLocal + index;
                    if (index == 0) return true;
                    CopyVector(current, index);
                    return true;
                }
                else
                {
                    CopyVector(current, Vector256<byte>.Count);
                    currentPosLocal += Vector256<byte>.Count;
                }

                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector256<byte>.Count);
                outerLoop:;
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }
        else if (Vector128.IsHardwareAccelerated && spanLength >= Vector128<byte>.Count)
        {
            ref byte searchSpace = ref GetRefAtPos(ref bufferRef, startIndex);
            ref byte currentSearchSpace = ref searchSpace;
            ref byte oneVectorAwayFromEnd = ref Unsafe.AddByteOffset(ref searchSpace, (uint)(spanLength - Vector128<byte>.Count));

            // Loop until either we've finished all elements or there's less than a vector's-worth remaining.
            do
            {
                Vector128<byte> current = Vector128.LoadUnsafe(ref currentSearchSpace);

                Vector128<byte> equalsBackslash =
                    Vector128.Equals(BackslashVector128, current);

                Vector128<byte> equalsBraces =
                    Vector128.Equals(OpenBraceVector128, current) |
                    Vector128.Equals(ClosingBraceVector128, current);

                Vector128<byte> equalsLineBreaks =
                    Vector128.Equals(LF_Vector128, current) |
                    Vector128.Equals(CR_Vector128, current);

                Vector128<byte> equalsZeroBytes =
                    Vector128.Equals(ZeroVector128, current);

                Vector128<byte> equalsOtherExceptLineBreaks =
                    equalsZeroBytes |
                    equalsBraces;

                Vector128<byte> equals =
                    equalsZeroBytes |
                    equalsLineBreaks |
                    equalsBraces |
                    equalsBackslash;

                Vector128<byte> equalsBackslashOrLineBreak = equalsBackslash | equalsLineBreaks;

                if (equalsBackslash != Vector128<byte>.Zero && equalsOtherExceptLineBreaks == Vector128<byte>.Zero)
                {
                    uint mask = equalsBackslashOrLineBreak.ExtractMostSignificantBits();
                    uint shiftLeftCount = 0;
                    while (true)
                    {
                        int parLength;
                        byte index = (byte)Vector128_TrailingZeroCount(mask);
                        if (index == Vector128<byte>.Count)
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            currentPosLocal += index;
                            break;
                        }
                        else if (index >= Vector128<byte>.Count - _parMaxLength)
                        {
                            if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                            {
                                parLength = 1;
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                            }
                            else
                            {
                                CopyVector_ParSupport(current, index, shiftLeftCount);
                                currentPosLocal += index;
                                if (currentPosLocal < _currentBufferChunkLength - _parMaxLength)
                                {
                                    if (IsPar(ref currentSearchSpace, index, out parLength, out InterTextKeyword interTextKeyword))
                                    {
                                        currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                        currentPosLocal += parLength;
                                        if (interTextKeyword == InterTextKeyword.Par)
                                        {
                                            AddLineBreak();
                                        }
                                        else
                                        {
                                            PlainText_Add('\t');
                                        }
                                        goto outerLoop;
                                    }
                                }

                                _currentPos = currentPosLocal;
                                return true;
                            }
                        }
                        else if (IsPar(ref currentSearchSpace, index, out int length, out InterTextKeyword interTextKeyword))
                        {
                            parLength = length;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            if (interTextKeyword == InterTextKeyword.Par)
                            {
                                AddLineBreak();
                            }
                            else
                            {
                                PlainText_Add('\t');
                            }
                        }
                        else if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                        {
                            parLength = 1;
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                        }
                        else
                        {
                            CopyVector_ParSupport(current, index, shiftLeftCount);
                            _currentPos = currentPosLocal + index;
                            return true;
                        }

                        shiftLeftCount = (uint)(index + parLength);
                        mask = ResetLowestSetBit(mask);
                    }
                }
                else if (equals != Vector128<byte>.Zero)
                {
                    int index = Vector128_TrailingZeroCount(equals.ExtractMostSignificantBits());
                    _currentPos = currentPosLocal + index;
                    if (index == 0) return true;
                    CopyVector(current, index);
                    return true;
                }
                else
                {
                    CopyVector(current, Vector128<byte>.Count);
                    currentPosLocal += Vector128<byte>.Count;
                }

                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, Vector128<byte>.Count);
                outerLoop:;
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }

        // I think Vector128 should be supported on literally anything these days, but if it's not, just fall out
        // without doing anything and we'll take the non-SIMD path. We don't fall back to Vector64 because that's
        // slower than just doing the 8 bytes scalar.

        _currentPos = currentPosLocal;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector(Vector512<byte> current, int length)
    {
        (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(current);

        PlainText_EnsureCapacity(_plainText_Count + Vector512<byte>.Count);

        char[] plainText = _plainText;
        int plainTextCount = _plainText_Count;

        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount]), lower);
        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount + (Vector512<byte>.Count / 2)]), upper);

        _plainText_Count += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector(Vector256<byte> current, int length)
    {
        (Vector256<ushort> lower, Vector256<ushort> upper) = Vector256.Widen(current);

        PlainText_EnsureCapacity(_plainText_Count + Vector256<byte>.Count);

        char[] plainText = _plainText;
        int plainTextCount = _plainText_Count;

        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount]), lower);
        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount + (Vector256<byte>.Count / 2)]), upper);

        _plainText_Count += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector(Vector128<byte> current, int length)
    {
        (Vector128<ushort> lower, Vector128<ushort> upper) = Vector128.Widen(current);

        PlainText_EnsureCapacity(_plainText_Count + Vector128<byte>.Count);

        char[] plainText = _plainText;
        int plainTextCount = _plainText_Count;

        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount]), lower);
        Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount + (Vector128<byte>.Count / 2)]), upper);

        _plainText_Count += length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsPar(ref byte currentSearchSpace, byte index, out int parLength, out InterTextKeyword interTextKeyword)
    {
        uint value = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref currentSearchSpace, index));
        if ((value == _parUInt || value == _tabUInt) &&
            (parLength = _isParEndingChar[GetByteAtPos(ref currentSearchSpace, index + 4)]) > 0)
        {
            interTextKeyword = value == _parUInt ? InterTextKeyword.Par : InterTextKeyword.Tab;
            return true;
        }
        else
        {
            parLength = 0;
            interTextKeyword = default;
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<byte> ShiftVectorElementsLeft(Vector512<byte> v, uint count)
    {
        Vector512<byte> mask = Vector512.Add(IndexVec_512, Vector512.Create((byte)count));
        return Vector512.Shuffle(v, mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<byte> ShiftVectorElementsLeft(Vector256<byte> v, uint count)
    {
        Vector256<byte> mask = Vector256.Add(IndexVec_256, Vector256.Create((byte)count));
        return Vector256.Shuffle(v, mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> ShiftVectorElementsLeft(Vector128<byte> v, uint count)
    {
        Vector128<byte> mask = Vector128.Add(IndexVec_128, Vector128.Create((byte)count));
        return Vector128.Shuffle(v, mask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector512<byte> current,
        int index,
        uint shiftLeftCount)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector512<byte> maskVec = Vector512.GreaterThan(Vector512.Create((byte)index), IndexVec_512);
            Vector512<byte> working = Vector512.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector512<ushort> lower, Vector512<ushort> upper) = Vector512.Widen(working);

            PlainText_EnsureCapacity(_plainText_Count + Vector512<byte>.Count);

            char[] plainText = _plainText;
            int plainTextCount = _plainText_Count;

            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount]), lower);
            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount + (Vector512<byte>.Count / 2)]), upper);

            _plainText_Count += diff;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector256<byte> current,
        int index,
        uint shiftLeftCount)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector256<byte> maskVec = Vector256.GreaterThan(Vector256.Create((byte)index), IndexVec_256);
            Vector256<byte> working = Vector256.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector256<ushort> lower, Vector256<ushort> upper) = Vector256.Widen(working);

            PlainText_EnsureCapacity(_plainText_Count + Vector256<byte>.Count);

            char[] plainText = _plainText;
            int plainTextCount = _plainText_Count;

            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount]), lower);
            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount + (Vector256<byte>.Count / 2)]), upper);

            _plainText_Count += diff;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector_ParSupport(
        Vector128<byte> current,
        int index,
        uint shiftLeftCount)
    {
        int diff = index - (int)shiftLeftCount;
        if (diff > 0)
        {
            Vector128<byte> maskVec = Vector128.GreaterThan(Vector128.Create((byte)index), IndexVec_128);
            Vector128<byte> working = Vector128.BitwiseAnd(current, maskVec);
            working = ShiftVectorElementsLeft(working, shiftLeftCount);

            (Vector128<ushort> lower, Vector128<ushort> upper) = Vector128.Widen(working);

            PlainText_EnsureCapacity(_plainText_Count + Vector128<byte>.Count);

            char[] plainText = _plainText;
            int plainTextCount = _plainText_Count;

            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount]), lower);
            Unsafe.WriteUnaligned(ref Unsafe.As<char, byte>(ref plainText[plainTextCount + (Vector128<byte>.Count / 2)]), upper);

            _plainText_Count += diff;
        }
    }
}

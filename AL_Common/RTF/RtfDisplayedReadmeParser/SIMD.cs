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

public sealed partial class RtfDisplayedReadmeParser
{
    #region API

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

        int currentPosLocal = _currentPos;

        int startIndex = currentPosLocal;
        int spanLength = _rtfBytesLength - startIndex;

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
                            currentPosLocal += index;
                            break;
                        }
                        else if (index >= Vector512<byte>.Count - _parMaxLength)
                        {
                            if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                            {
                                parLength = 1;
                            }
                            else
                            {
                                currentPosLocal += index;
                                if (currentPosLocal < _rtfBytesLength - _parMaxLength)
                                {
                                    if (IsPar(ref currentSearchSpace, index, out parLength, out InterTextKeyword interTextKeyword))
                                    {
                                        currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                        currentPosLocal += parLength;
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
                        }
                        else if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                        {
                            parLength = 1;
                        }
                        else
                        {
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
                    return true;
                }
                else
                {
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
                            currentPosLocal += index;
                            break;
                        }
                        else if (index >= Vector256<byte>.Count - _parMaxLength)
                        {
                            if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                            {
                                parLength = 1;
                            }
                            else
                            {
                                currentPosLocal += index;
                                if (currentPosLocal < _rtfBytesLength - _parMaxLength)
                                {
                                    if (IsPar(ref currentSearchSpace, index, out parLength, out InterTextKeyword interTextKeyword))
                                    {
                                        currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                        currentPosLocal += parLength;
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
                        }
                        else if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                        {
                            parLength = 1;
                        }
                        else
                        {
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
                    return true;
                }
                else
                {
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
                            currentPosLocal += index;
                            break;
                        }
                        else if (index >= Vector128<byte>.Count - _parMaxLength)
                        {
                            if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                            {
                                parLength = 1;
                            }
                            else
                            {
                                currentPosLocal += index;
                                if (currentPosLocal < _rtfBytesLength - _parMaxLength)
                                {
                                    if (IsPar(ref currentSearchSpace, index, out parLength, out InterTextKeyword interTextKeyword))
                                    {
                                        currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, index + parLength);
                                        currentPosLocal += parLength;
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
                        }
                        else if (GetByteAtPos(ref currentSearchSpace, index) is < 14 and not (byte)'\t')
                        {
                            parLength = 1;
                        }
                        else
                        {
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
                    return true;
                }
                else
                {
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

    #endregion
}

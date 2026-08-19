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
using static AL_Common.RTF.RTFParserCommon;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    #region API

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

        int startIndex = _currentPos;
        int spanLength = _currentBufferChunkLength - _currentPos;

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
                    Vector.Equals(ZeroVector, current) |
                    Vector.Equals(LF_Vector, current) |
                    Vector.Equals(CR_Vector, current) |
                    Vector.Equals(BackslashVector, current) |
                    Vector.Equals(OpenBraceVector, current) |
                    Vector.Equals(ClosingBraceVector, current);

                if (equals != Vector<byte>.Zero)
                {
                    int index = LocateFirstFoundByte(equals);
                    if (index > 0)
                    {
                        CopyVector(current, index);
                    }

                    return true;
                }

                CopyVector(current, Vector<byte>.Count);
                currentSearchSpace = ref Unsafe.AddByteOffset(ref currentSearchSpace, (nint)Vector<byte>.Count);
            } while (!Unsafe.IsAddressGreaterThan(ref currentSearchSpace, ref oneVectorAwayFromEnd));
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CopyVector(Vector<byte> current, int index)
    {
        Vector.Widen(current, out Vector<ushort> lower, out Vector<ushort> upper);

        PlainText_EnsureCapacity(_plainText_Count + Vector<byte>.Count);
        lower.CopyTo(Unsafe.As<char[], ushort[]>(ref _plainText), _plainText_Count);
        upper.CopyTo(Unsafe.As<char[], ushort[]>(ref _plainText), _plainText_Count + (Vector<byte>.Count / 2));

        _plainText_Count += index;
        _currentPos += index;
    }

    #endregion
}

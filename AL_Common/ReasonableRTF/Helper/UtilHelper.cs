/*
 * MIT License
 * 
 * Copyright (c) 2024-2026 Brian Tobin
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
*/

using System;
using System.Runtime.CompilerServices;

namespace ReasonableRTF.Helper;

internal static class UtilHelper
{
    /// <summary>
    /// Returns an array of type <typeparamref name="T"/> with all elements initialized to <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="length"></param>
    /// <param name="value">The value to initialize all elements with.</param>
    internal static T[] InitializedArray<T>(int length, T value) where T : new()
    {
        T[] ret = new T[length];
        for (int i = 0; i < length; i++)
        {
            ret[i] = value;
        }
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Array_IndexOfByte_Fast(byte[] array, byte value, int startIndex, int count)
    {
        /*
        On .NET, Array.IndexOf() uses crazy fast SIMD. On Framework, it normally doesn't.
        However, on Framework 64-bit only, we can make it use SIMD by using span.IndexOf(), if we reference the
        appropriate package (directly or indirectly), System.Memory or whatever it is.
        If we're 32-bit, though, SIMD is not supported, so we just stick to the regular Array.IndexOf(), which
        while substantially slower than the SIMD version, is still reasonably fast.

        But instead of checking for 64-bit vs. 32-bit, we can just check directly if SIMD is supported.
        */
        if (System.Numerics.Vector.IsHardwareAccelerated)
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

    internal static void ValidateArgs(byte[] source, int length)
    {
        if (length > source.Length)
        {
            throw new ArgumentException(nameof(length) + " is greater than the length of " + nameof(source) + ".", nameof(length));
        }
    }
}

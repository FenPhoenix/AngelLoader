// TODO: Parallelize block-swap algorithm to speedup in-place merge, possibly using SSE with instruction to reverse order within an SSE Vector
//       Or, maybe SSE rotation/reverse order can be avoided, knowing that we'll be rotating it back in the following pass.
// TODO: Parallelize block-swap algorithms to see if there is a benefit, now that unit testing and benchmarking in C# is in place
//       Try simple things first like scalar parallelism of reversal algorithms middle stage of reversal by running the two portions reversal
//       in parallel to see if it speeds up at all.
// TODO: Consider implementing in-place array rotation, such as possibly this (https://www.geeksforgeeks.org/block-swap-algorithm-for-array-rotation/)
//       and do a parallel version as well.
// TODO: Add another termination condition to Gries-Mills block swap algorithm of one of the array portions being a single element, and handle that case
//       with a simple array rotation (if it's worthwhile).
// TODO: Combine Reversal and Gries-Mills algorithms, to eliminate rotation of the smaller half of the array, when it pays off, since now the other
//       half has to be "fixed". There may be certain ratios between halves that work well using one algorithm versus another.
// TODO: Fix a bug with Bentley's Juggling algorithm when the starting index is non-zero.
// TODO: Use Array.Copy to copy 3X faster for those algorithms that don't reverse, which is as fast as SSE copy.

#pragma warning disable CA1510

using System;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        // reverse/mirror a range from l to r, inclusively, in-place
        public static void Reversal<T>(this T[] array, int l, int r)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            for (; l < r; l++, r--)
            {
                T temp   = array[l];  // swap of array[l] and array[r]
                array[l] = array[r];
                array[r] = temp;
            }
        }
    }
}

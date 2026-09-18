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
        public static void Swap<T>(this T[] array, int indexA, int indexB, int length, bool reverse = false)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (!reverse)
            {
                while (length-- > 0)
                {
                    T temp          = array[indexA];                 // inlining Swap() increases performance by 25%
                    array[indexA++] = array[indexB];
                    array[indexB++] = temp;
                }
            }
            else
            {
                int currIndexB = indexB + length - 1;
                while (length-- > 0)
                {
                    T temp              = array[indexA];             // inlining Swap() increases performance by 25%
                    array[indexA++]     = array[currIndexB];
                    array[currIndexB--] = temp;
                }
            }
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

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

        // Swaps two sequential subarrays ranges a[ l .. m ] and a[ m + 1 .. r ]
        public static void BlockSwapReversal<T>(T[] array, int l, int m, int r, int threshold = 1024)
        {
            if ((r - l) < threshold)
            {
                array.Reversal(l,     m);
                array.Reversal(m + 1, r);
                array.Reversal(l,     r);
            }
            else
            {
                Array.Reverse(array, l,     m - l + 1);   // 2X slower than array.Reversal when used in In-Place Merge Sort, but is 2X faster when this funciton is benchmarked by itself
                Array.Reverse(array, m + 1, r - m    );   // Theory: Array.Reverse() has large overhead => does not peform well for small arrays
                Array.Reverse(array, l,     r - l + 1);
            }
        }

        public static void BlockSwapGriesMills<T>(T[] array, int l, int m, int r)
        {
            int rotdist = m - l + 1;
            int n       = r - l + 1;
            if (rotdist == 0 || rotdist == n) return;
            int p, i = p = rotdist;
            int j = n - p;
            p += l;
            while (i != j)
            {
                if (i > j)
                {
                    array.Swap(p - i, p, j);
                    i -= j;
                }
                else
                {
                    array.Swap(p - i, p + j - i, i);
                    j -= i;
                }
            }
            array.Swap(p - i, p, i);
        }
    }
}

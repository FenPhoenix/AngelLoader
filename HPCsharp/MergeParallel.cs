// TODO: Create a single multi-merge generic algorithm (inner) where the 2-way merge is passed in as a function parameter (serial or parallel)
//       The trouble is where does this generic algorithm live, ParallelAlgorithm or Algorithm class? Maybe we should have a single class
// TODO: For Divide-and-Conquer parallel merge split the array on cache line boundaries to eliminate sharing of cache lines between threads.
// TODO: Port my C++ parallel in-place merge algorithm from (https://www.drdobbs.com/parallel/parallel-in-place-merge/240008783?pgno=1) to C#,
//       as a user requested a truly in-place version, and it would be good to see how well it performs on 6, 14, and 32-core CPUs with 2, 4, 8 memory channels.
// TODO: Parallelize Algorithm.BlockSwapReversal(arr, q1, midIndex, q2 - 1); to use SSE/SIMD instructions and to use as few cores as necessary to use full memory bandwidth.
// TODO: Benchmark in-place versus not-in-place Merge and parallel Merge. Develop an adaptive in-place/not-in-place Merge and Parallel Merge if there is a large performance difference,
//       and use it inside Parallel Merge Sort and serial Merge Sort
// TODO: Parallel Merge running on all 32-core with hyperthreading (64-core in AWS) varied in performance dramatically from 200 Million to 1.7 Billion 
//       530-741 Million on a 6-core (with hyperthreading) is much more consitent in performance (on battery). Figure out why it's not scaling well - interference?
//       Single-core merge is 150-170 Million (very consistent) - on battery power; and 245-246 Million (extremely consistent - on wall power.
//       ParallelMerge was 750-770 Million when threshold set to be arra.Length / numberOfCores, and 770-1.1 Billion when set to 32K or 64K on 6-core laptop (with 128K possibly even better).
// TODO: Figure out why Merge and Merge2 are consistent in performance, with neither showing advantage, especially when order of measurement was swapped and then the other showed up as faster.
//       Seems to be a measurement flaw.
// TODO: Port my median-of-two-sorted-arrays algorithm from Dr. Dobb's to reduce the number of levels in the parallel merge to be exactly Log2(N).
// TODO: Improve termination case of Parallel Merge to a better measure than (length1 + length2) < threshold, to possibly include the case of one of the lengths being smaller than a threshold

#pragma warning disable CA1510
#pragma warning disable CA1002

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HPCsharp
{
    /// <summary>
    /// Parallel Algorithms operating on variety of containers, providing trade-off between abstraction and performance
    /// </summary>
    public static partial class ParallelAlgorithm
    {
        // Merge two ranges of source array T[ l .. m, m+1 .. r ] in-place.
        // Based on not-in-place algorithm in 3rd ed. of "Introduction to Algorithms" p. 798-802, extending it to be in-place
        // and my Dr. Dobb's paper https://www.drdobbs.com/parallel/parallel-in-place-merge/240008783 or https://web.archive.org/web/20141217133856/http://www.drdobbs.com/parallel/parallel-in-place-merge/240008783
        public static void MergeDivideAndConquerInPlacePar<T>(T[] arr, int startIndex, int midIndex, int endIndex, IComparer<T> comparer = null, int threshold0 = 16 * 1024, int threshold1 = 16 * 1024)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr));
            //Console.WriteLine("MergeDivideAndConquerInPlacePar: start = {0}, mid = {1}, end = {2}", startIndex, midIndex, endIndex);
            int length1 = midIndex - startIndex + 1;
            int length2 = endIndex - midIndex;
            if (length1 >= length2)
            {
                if (length2 <= 0) return;                       // if the smaller segment has zero elements, then nothing to merge
                int q1 = startIndex / 2 + midIndex / 2 + (startIndex % 2 + midIndex % 2) / 2;     // q1 is mid-point of the larger segment. length1 >= length2 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, midIndex + 1, endIndex, comparer);  // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q1 + (q2 - midIndex - 1);
                BlockSwapReversalPar(arr, q1, midIndex, q2 - 1, threshold0);
                //Algorithm.BlockSwapReversal(arr, q1, midIndex, q2 - 1);
                //Algorithm.BlockSwapGriesMills(arr, q1, midIndex, q2 - 1);

                if (length1 < threshold1)
                {
                    MergeDivideAndConquerInPlacePar(arr, startIndex, q1 - 1, q3 - 1, comparer);
                    MergeDivideAndConquerInPlacePar(arr, q3 + 1, q2 - 1, endIndex, comparer);
                }
                else
                {
                    Parallel.Invoke(
                        () => { MergeDivideAndConquerInPlacePar(arr, startIndex, q1 - 1, q3 - 1, comparer); },   // note that q3 is now in its final place and no longer participates in further processing
                        () => { MergeDivideAndConquerInPlacePar(arr, q3 + 1, q2 - 1, endIndex, comparer); }
                    );
                }
            }
            else
            {   // length1 < length2
                if (length1 <= 0) return;                       // if the smaller segment has zero elements, then nothing to merge
                int q1 = (midIndex + 1) / 2 + endIndex / 2 + ((midIndex + 1) % 2 + endIndex % 2) / 2;   // q1 is mid-point of the larger segment.  length2 > length1 > 0
                int q2 = Algorithm.BinarySearch(arr[q1], arr, startIndex, midIndex, comparer);          // q2 is q1 partitioning element within the smaller sub-array (and q2 itself is part of the sub-array that does not move)
                int q3 = q2 + (q1 - midIndex - 1);
                BlockSwapReversalPar(arr, q2, midIndex, q1, threshold0);
                //Algorithm.BlockSwapReversal(arr, q2, midIndex, q1);
                //Algorithm.BlockSwapGriesMills(arr, q2, midIndex, q1);

                if (length1 < threshold1)
                {
                    MergeDivideAndConquerInPlacePar(arr, startIndex, q2 - 1, q3 - 1, comparer);
                    MergeDivideAndConquerInPlacePar(arr, q3 + 1, q1, endIndex, comparer);
                }
                else
                {
                    Parallel.Invoke(
                        () => { MergeDivideAndConquerInPlacePar(arr, startIndex, q2 - 1, q3 - 1, comparer); },   // note that q3 is now in its final place and no longer participates in further processing
                        () => { MergeDivideAndConquerInPlacePar(arr, q3 + 1, q1, endIndex, comparer); }
                    );
                }
            }
        }
    }
}

// TODO: Implement support for Comparison functions as well as Comparer (eek - double the fun), as Standard C# libraries support both methods.
// TODO: See if implementing stable IEnumerable sorting is faster than LINQ sorting, since that's the only stable one.
// TODO: Compare performance of sorting arrays and lists of classes with .Sort and LINQ to see if the advantages are bigger or smaller
// TODO: Expose all of the thresholds for users to be able to conrol. These can just be a List of thresholds that match up with the list of algorithms - i.e. a pair of algorithm and threshold.
//       Thresholds could be allowed to be negative or zero disable the associated algorithm.
// TODO: Test whether Merge is also stable, so that we don't have to resort to DivideAndConquerMerge for stability and give up performance in the process.
// TODO: Wonder if getting rid off the comparer function would speed up Merge Sort substantially, because C# is calling this function per element with all of the
//       overhead of the function call. Yes, having a comparison function provides the flexibility of handling any data type and comparing any field within that
//       data type, as well as ascending/decending selection by the user. However, we could setup special cases for sorting arrays of common data types much faster
//       by eliminating the comparison function, or detecting when it's null and seeing if the resulting hard-coded merge implementation would be much faster.
// TODO: Create a hybrid of in-place MSD Radix Sort and in-place Merge Sort to see if the combined algorithm is faster than .Sort and MSD Radix Sort running
//       on a single core. Study different thresholds.
// TODO: Combine LSD Radix Sort with Priority Queue, where LSD Radix Sort is doing L2 cache size chunks.
// TODO: For parallel in-place Merge Sort where recursion levels are expensive, to minimize the number of recursions and maximize parallelism, if the array size
//       is large enough, to where the amount of work is bigger than the threshold set, set the threshold internally to array/numberOfCores to maximize
//       parallelism and minimize the number of recursion levels within the Merge portion of the algorithm. Figure out the optimal thing to do, by
//       measuring the threshold versus array size and the number of memory channels and number of cores.
// TODO: The above idea of minimizing recursions is great for creating a parallel Array.Sort(), which is in-place but lacks .AsParallel(), which this method
//       would provide. It is also generic and in-place, which is enormously useful (and already implemented). This definitely needs to be tested and optimized
//       on 14-core and 32-core CPUs.
// TODO: Fix inconsistent parallel threshold settings
// TODO: Fix List sorting that is currently hidden because it's not truly in-place. Either make it truly in-place or call it not-in-place
// TODO: See if the experimental hidden algorithm is worthwhile
// TODO: Use Selection Sort instead of Insertion Sort for faster bottom of the recursion tree.
// TODO: Use Heap Sort or Array.Sort for faster bottom of the recursion tree, especially for in-place versions.
// TODO: Fix all average calculation to not cause over/underflow.

#pragma warning disable CA1510
#pragma warning disable CA1002

using System;
using System.Collections.Generic;
using System.Xml.Schema;
using HPCsharp.ParallelAlgorithms;

namespace HPCsharp
{
    public static partial class Algorithm
    {
        /// <summary>
        /// Arrays or Lists smaller than this value will use Insertion Sort
        /// </summary>
        public static Int32 SortMergeInsertionThreshold { get; set; } = 16;

        internal static void SortMergeInner<T>(this T[] src, int l, int r, T[] dst, bool stable = true, bool srcToDst = true, IComparer<T> comparer = null, int threshold = 1024)
        {
            if (r < l) return;
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            if (stable)
            {
                if ((r - l) < SortMergeInsertionThreshold)
                {
                    HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                    if (srcToDst)
                        for (int i = l; i <= r; i++) dst[i] = src[i];   // copy from src to dst, when the result needs to be in dst
                    return;
                }
            }
            else if ((r - l) < threshold)
            {
                Array.Sort(src, l, r - l + 1, comparer);
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }

            int m = r / 2 + l / 2 + (r % 2 + l % 2) / 2;    // (l + r) / 2 without overflow or underflow
            int length1 = m - l + 1;
            int length2 = r - (m + 1) + 1;

            SortMergeInner(src, l,     m, dst, stable, !srcToDst, comparer, threshold);		// reverse direction of srcToDst for the next level of recursion
            SortMergeInner(src, m + 1, r, dst, stable, !srcToDst, comparer, threshold);

            if (srcToDst) Merge(src, l, length1, m + 1, length2, dst, l, comparer);
            else          Merge(dst, l, length1, m + 1, length2, src, l, comparer);
        }

        internal static void SortMergeInner2<T>(this T[] src, int l, int r, T[] dst, bool srcToDst = true, IComparer<T> comparer = null, int threshold = 32)
        {
            if (r < l) return;
            if (r == l)
            {    // termination/base case of sorting a single element
                if (srcToDst) dst[l] = src[l];    // copy the single element from src to dst
                return;
            }
            //else if ((r - l) < SortMergeInsertionThreshold)
            else if ((r - l) < threshold)
            {
                HPCsharp.Algorithm.InsertionSort<T>(src, l, r - l + 1, comparer);  // want to do dstToSrc, can just do it in-place, just sort the src, no need to copy
                //Array.Sort(src, l, r - l + 1, comparer);
                if (srcToDst)
                    for (int i = l; i <= r; i++) dst[i] = src[i];	// copy from src to dst, when the result needs to be in dst
                return;
            }

            int m = r / 2 + l / 2 + (r % 2 + l % 2) / 2;    // (l + r) / 2 without overflow or underflow
            int length1 = m - l + 1;
            int length2 = r - (m + 1) + 1;

            SortMergeInner2(src, l,     m, dst, !srcToDst, comparer, threshold);		// reverse direction of srcToDst for the next level of recursion
            SortMergeInner2(src, m + 1, r, dst, !srcToDst, comparer, threshold);

            if (srcToDst) Merge(src, l, length1, m + 1, length2, dst, l, comparer);
            else          Merge(dst, l, length1, m + 1, length2, src, l, comparer);
        }
    }
}

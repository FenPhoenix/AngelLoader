// TODO: Need a Merge Sort Stable that is not in-place.
// TODO: Figure out a way to specify both stable as a method like LINQ does .Stable and .Parallel
// TODO: Expose all of the thresholds for users to be able to conrol
// TODO: Tune Merge Sort Stable threshold to my current laptop (as a good starting point)
// TODO: Implement Parallel Merge Sort in a more generic way where there are not just thresholds, but also selection of serial algorithms to choose from
//       to optimize depending on how well the serial algorithms perform on a particular hardware platform.
// TODO: For readme, compare Radix Sort with Linq Sort and C# .Sort using several array sizes to show that the delta grows as size grows
// TODO: Show the difference between Stable Merge Sort and Linq Sorting to compare apples to apples
// TODO: While doing the Binary Search looking for the split for divide-and-conquer portion, since we know the index of each element as we are working on them before
//       moving them, could we keep the position in mind as part of the comparison to break the ties during the comparison? Would that help?
// TODO: Make sure to document the fact that not-in-place merge and merge sort change the input array in the process of sorting.
// TODO: Focus not only on the worst case of random value arrays, but also on the best case of nearly sorted arrays to compete with TimSort, as Merge Sort does well in this case.
// TODO: Implement HeapSort to be able to create more hybrix variations.
// TODO: Try removing Insertion Sort, since ArraySort already implements it, and to also verify that Insertion Sort (with a copy) is helping performance in the worst case and best case.
// TODO: Implement a hybrid of Parallel Merge Sort and Radix Sort as base case, with the threshold of being completely inside the cache, to allow better random access pattern (inside cache)
//       as that should help Radix Sort.
// TODO: Add the ability to limit the recursion depth to limit parallelism, as is done in http://dzmitryhuba.blogspot.com/2010/10/parallel-merge-sort.html this may help control parallelism better
//       than Microsoft does and possibly limit oversubscription.
// TODO: For sort of two arrays (one keys and one items), a potentially faster method would be to have keys to not only hold a key at each location, but also
//       an index, and then sort these keys/indexes pairs. Once the key/index array has been sorted, do a single pass of moving the items into their final locations.
// TODO: Once in-place parallel merge sort is working, implement an adaptive parallel in-place merge sort algorithm, which tries to allocate memory to use not-in-place algorithm
//       first catches an out of memory exception and performs in-place merge sort algorithm in that case - same as C++ STL implementation idea, but parallel for merge sort.
// TODO: Develop several in-place parallel hybrid sort algorithms: in-place parallel merge using Array.Sort as the base case, in-place parallel merge using in-place MSD Radix Sort as
//       the base case with the base size such that it fits within L2 cache of the CPU.
// TODO: Determine which of the three BlockSwap algorithms uses the least amount of memory bandwidth, as this may be the most important
//       factor for allowing parallel scalability. Test each of these algorithms to compare parallel scalability.
// TODO: Check if offering a destination array as an argument instead of a return array would provide a performance benefit when re-using the destination array,
//       for not-in-place version. This idea may provide 25% performance improvement, as seen from benchmarks when memory allocator re-uses the array.
// TODO: See if threshold for Insertion Sort can be removed, since .Sort() already uses it and has its own threshold internally for it.
// TODO: Eliminate copy operation in the versions of Parallel Merge sort where the startIndex and length are specified, to reduce the memory footprint.
// TODO: Improve efficiency and memory usage size of the Adaptive Merge Sort for a sub-array by allocating the destination of the size of the sub-array, and for
//       the source sub-array and parallel copying the source. Determine when it is advantageous versus creating the destination array that is as big as the source array,
//       for the not-in-place part of the algorithm. It would mean two smaller allocations, where both need to succeed to be able to proceed.
// TODO: Implement functional usage equivalent methods for in-place sorting methods that return the source array, to make functional style usage convenient for functional programming use.

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
        /// <summary>
        /// Purely in-place Parallel Merge Sort algorithm. Takes a range of the src array, and sorts just that range.
        /// </summary>
        /// <typeparam name="T">array of type T</typeparam>
        /// <param name="src">source array</param>
        /// <param name="startIndex">index within the src array where sorting starts</param>
        /// <param name="length">number of elements starting with startIndex to be sorted</param>
        /// <param name="comparer">comparer used to compare two array elements of type T</param>
        /// <param name="parallelThreshold">arrays larger than this value will be sorted using multiple cores</param>
        public static void SortMergeInPlacePar<T>(this T[] src, int startIndex, int length, IComparer<T> comparer, int parallelThreshold = 16 * 1024)
        {
            if ((parallelThreshold * Environment.ProcessorCount) < src.Length)
                parallelThreshold = src.Length / Environment.ProcessorCount;

            SortMergeInPlaceHybridInnerPar<T>(src, startIndex, startIndex + length - 1, comparer, parallelThreshold);
        }

        // start and end indexes are inclusive
        private static void SortMergeInPlaceHybridInnerPar<T>(this T[] src, int startIndex, int endIndex, IComparer<T> comparer, int threshold0 = 16 * 1024,
                                                              int threshold1 = 256 * 1024, int threshold2 = 256 * 1024 )
        {
            //Console.WriteLine("merge sort: start = {0}, length = {1}", startIndex, length);
            int length = endIndex - startIndex + 1;
            if (length <= 1) return;
            if (length <= threshold0)
            {
                Array.Sort(src, startIndex, length, comparer);
                return;
            }
            int midIndex = endIndex / 2 + startIndex / 2 + ((endIndex % 2 + startIndex % 2) / 2);  // average without overflow
            Parallel.Invoke(
                () => { SortMergeInPlaceHybridInnerPar<T>(src, startIndex,   midIndex, comparer, threshold0, threshold1, threshold2); },  // recursive call left  half
                () => { SortMergeInPlaceHybridInnerPar<T>(src, midIndex + 1, endIndex, comparer, threshold0, threshold1, threshold2); }   // recursive call right half
            );
            MergeDivideAndConquerInPlacePar(src, startIndex, midIndex, endIndex, comparer, threshold1, threshold2);     // merge the results
        }
    }
}

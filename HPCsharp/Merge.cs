// TODO: Implement a knob for the merge algorithm, to use multi-merge and specify how many way to split the merge, for divide-and-conquer too.
// TODO: Multi-merge should use 2-way, 3-way and possibly 4-way before using the more general multi-way merge to go faster.
// TODO: Is it faster to copy a List to an Array, then do the merge and then to copy the result back to a List? Currently, List merge runs at 1/2 the speed of Array merge.
// TODO: Does it pay off to use the parallel copy.
// TODO: Change all in-place sorting functions to return the original reference to improve Functional composition and pipelining, since it's returning void anyways.
// TODO: Refactor other methods to possibly return a destination, if it makes sense.
// TODO: Develop a faster version of Divide-And-Conquer in-place merge which uses a not-in-place merge as the recursion termination base case
//       allocating an array and copying back from it, as is done in C++ std::inplace_merge to provide a faster in-place merge
//       Offer two version of Divide-And-Conquer in-place merge "purely in-place" (no additional allocations) and "adaptive in-place" (additional allocation
//       when memory is available). May be able to do a single allocation of a full size array and use it for merging.
// TODO: Parallelize block-swap algorithm to speedup in-place merge, possibly using SSE with instruction to reverse order within an SSE Vector
// TODO: For inner merge could we process a chunk at a time without comparisons for the ending conditions? We could take the min(A.Length, B.Length) and run for that
//       with only element comparison and length comparison, and then switch to the Merge2 clever comparison reduction method to finish the operation, or do it again
//       with left-over pieces of A and B. This method would pay even higher dividends for multi-way merge. I may have done this already in C++ and need to check.
//       This idea may be more CPU architecture friendly, due to fewer mispredictions, since the for loop one is easy to predict.
// TODO: Test Array.Copy versus copying using a for loop for the merge algorithms. Figure out the size threshold when the for loop is faster.

#pragma warning disable CA1510
#pragma warning disable CA1303
#pragma warning disable CA1002

using System;
using System.Collections.Generic;

namespace HPCsharp
{
    public struct SortedSpan
    {
        public Int32 Start;
        public Int32 Length;
    }

    public static partial class Algorithm
    {
        /// <summary>
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="a">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="b">second source Array to be merged</param>
        /// <param name="bStart">starting index of the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        public static void MergeFaster<T>(T[] a, Int32 aStart, Int32 aLength,
                                          T[] b, Int32 bStart, Int32 bLength,
                                          T[] dst, Int32 dstStart,
                                          IComparer<T> comparer = null)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            while (aStart <= aEnd && bStart <= bEnd)
            {
                while (true)
                {
                    // a[aStart] <= b[bStart]
                    if (equalityComparer.Compare(a[aStart], b[bStart]) <= 0)   	// if elements are equal, then a[] element is output
                    {
                        dst[dstStart++] = a[aStart++];
                        if (aStart > aEnd) break;
                    }
                    else
                    {
                        dst[dstStart++] = b[bStart++];
                        if (bStart > bEnd) break;
                    }
                }
            }
            if (aStart <= aEnd)
                Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            //while (aStart <= aEnd) dst[dstStart++] = a[aStart++];    // copy(a[aStart, aEnd] to dst[dstStart]
            if (bStart <= bEnd)
                Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            //while (bStart <= bEnd) dst[dstStart++] = b[bStart++];
        }
        /// <summary>
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="a">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bStart">starting index of the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dst">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        public static void Merge<T>(T[] a, Int32 aStart, Int32 aLength, Int32 bStart, Int32 bLength,
                                    T[] dst, Int32 dstStart,
                                    IComparer<T> comparer = null)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;

            while (aStart <= aEnd && bStart <= bEnd)
            {
                if (equalityComparer.Compare(a[aStart], a[bStart]) <= 0)   	    // if elements are equal, then a[] element is output
                    dst[dstStart++] = a[aStart++];
                else
                    dst[dstStart++] = a[bStart++];
            }

            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd) dst[dstStart++] = a[aStart++];           // copy(a[aStart, aEnd] to dst[dstStart]
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd) dst[dstStart++] = a[bStart++];
        }

        /// <summary>
        /// Merge two sorted Array segments, placing the result into a destination Array, starting at an index.
        /// </summary>
        /// <param name="aKeys">first source Array to be merged</param>
        /// <param name="aStart">starting index of the first sorted Array, inclusive</param>
        /// <param name="aLength">length of the first sorted segment</param>
        /// <param name="bKeys">second source Array to be merged</param>
        /// <param name="bStart">starting index of the second sorted Array, inclusive</param>
        /// <param name="bLength">length of the second sorted segment</param>
        /// <param name="dstKeys">destination Array where the result of two merged Arrays is to be placed</param>
        /// <param name="dstStart">starting index within the destination Array where the merged sorted Array is to be placed</param>
        /// <param name="comparer">optional method to compare array elements</param>
        public static void Merge<T1, T2>(T1[] aKeys, T2[] aItems, Int32 aStart, Int32 aLength,
                                         T1[] bKeys, T2[] bItems, Int32 bStart, Int32 bLength,
                                         T1[] dstKeys, T2[] dstItems, Int32 dstStart,
                                         IComparer<T1> comparer = null)
        {
            if (aItems == null)
                throw new ArgumentNullException(nameof(aItems));
            if (aKeys == null)
                throw new ArgumentNullException(nameof(aKeys));
            if (bKeys == null)
                throw new ArgumentNullException(nameof(bKeys));
            if (bItems == null)
                throw new ArgumentNullException(nameof(bItems));
            if (dstKeys == null)
                throw new ArgumentNullException(nameof(dstKeys));
            if (dstItems == null)
                throw new ArgumentNullException(nameof(dstItems));
            var equalityComparer = comparer ?? Comparer<T1>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;

            while (aStart <= aEnd && bStart <= bEnd)
            {
                if (equalityComparer.Compare(aKeys[aStart], bKeys[bStart]) <= 0)        // if elements are equal, then a[] element is output
                {
                    dstKeys[dstStart] = aKeys[aStart];
                    dstItems[dstStart++] = aItems[aStart++];
                }
                else
                {
                    dstKeys[dstStart] = bKeys[bStart];
                    dstItems[dstStart++] = bItems[bStart++];
                }
            }

            //Array.Copy(a, aStart, dst, dstStart, aEnd - aStart + 1);
            while (aStart <= aEnd)
            {
                dstKeys[dstStart] = aKeys[aStart];    // copy(a[aStart, aEnd] to dst[dstStart]
                dstItems[dstStart++] = aItems[aStart++];
            }
            //Array.Copy(b, bStart, dst, dstStart, bEnd - bStart + 1);
            while (bStart <= bEnd)
            {
                dstKeys[dstStart] = bKeys[bStart];
                dstItems[dstStart++] = bItems[bStart++];
            }
        }

        public static void MergeThreeWay2<T>(T[] src, Int32 aStart, Int32 aLength,
                                                      Int32 bStart, Int32 bLength,
                                                      Int32 cStart, Int32 cLength,
                                             T[] dst, Int32 dstStart,
                                             IComparer<T> comparer = null)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            Int32 cEnd = cStart + cLength - 1;
            while (aStart <= aEnd && bStart <= bEnd && cStart <= cEnd)
            {
                while (true)
                {
                    // a[aStart] <= b[bStart]
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)
                    {   // a <= b
                        if (equalityComparer.Compare(src[aStart], src[cStart]) <= 0)
                        {
                            dst[dstStart++] = src[aStart++];   // a is smallest
                            if (aStart > aEnd) break;
                        }
                        else
                        {
                            dst[dstStart++] = src[cStart++];   // c is smallest
                            if (cStart > cEnd) break;
                        }
                    }
                    else
                    {   // b < a
                        if (equalityComparer.Compare(src[bStart], src[cStart]) <= 0)
                        {
                            dst[dstStart++] = src[bStart++];   // b is smallest
                            if (bStart > bEnd) break;
                        }
                        else
                        {
                            dst[dstStart++] = src[cStart++];   // c is smallest
                            if (cStart > cEnd) break;
                        }
                    }
                }
            }
            // Ran out of elements in one of the segments - i.e. 2 segments are available for merging, but which 2
            // Length needs to be adjusted, to be lengths left yet to be merged for each segment
            aLength = aEnd - aStart + 1;
            bLength = bEnd - bStart + 1;
            cLength = cEnd - cStart + 1;
            if (aStart > aEnd)
            {
                Merge(src, bStart, bLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                Merge(src, aStart, aLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
            else   // (cStart > cEnd)
            {
                Merge(src, aStart, aLength, bStart, bLength, dst, dstStart, equalityComparer);
            }
        }

        // Strategy is to handle 4 segments while 4 are available, 3 while 3 are available, 2 while 2 are available
        // This extends the strategy used for merging two segments nicely
        public static void MergeFourWay2<T>(T[] src, Int32 aStart, Int32 aLength,
                                                     Int32 bStart, Int32 bLength,
                                                     Int32 cStart, Int32 cLength,
                                                     Int32 dStart, Int32 dLength,
                                            T[] dst, Int32 dstStart,
                                            IComparer<T> comparer = null)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            if (dst == null)
                throw new ArgumentNullException(nameof(dst));
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 aEnd = aStart + aLength - 1;
            Int32 bEnd = bStart + bLength - 1;
            Int32 cEnd = cStart + cLength - 1;
            Int32 dEnd = dStart + dLength - 1;
            while (aStart <= aEnd && bStart <= bEnd && cStart <= cEnd && dStart <= dEnd)
            {
                while (true)
                {
                    if (equalityComparer.Compare(src[aStart], src[bStart]) <= 0)
                    {   // a <= b
                        if (equalityComparer.Compare(src[cStart], src[dStart]) <= 0)
                        {   // c <= d
                            if (equalityComparer.Compare(src[aStart], src[cStart]) <= 0)
                            {
                                dst[dstStart++] = src[aStart++];   // a is smallest
                                if (aStart > aEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[cStart++];   // c is smallest
                                if (cStart > cEnd) break;
                            }
                        }
                        else
                        {   // d < c
                            if (equalityComparer.Compare(src[aStart], src[dStart]) <= 0)
                            {
                                dst[dstStart++] = src[aStart++];   // a is smallest
                                if (aStart > aEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[dStart++];   // d is smallest
                                if (dStart > dEnd) break;
                            }
                        }
                    }
                    else
                    {   // b < a
                        if (equalityComparer.Compare(src[cStart], src[dStart]) <= 0)
                        {   // c <= d
                            if (equalityComparer.Compare(src[bStart], src[cStart]) <= 0)
                            {
                                dst[dstStart++] = src[bStart++];   // b is smallest
                                if (bStart > bEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[cStart++];   // c is smallest
                                if (cStart > cEnd) break;
                            }
                        }
                        else
                        {   // d < c
                            if (equalityComparer.Compare(src[bStart], src[dStart]) <= 0)
                            {
                                dst[dstStart++] = src[bStart++];   // b is smallest
                                if (bStart > bEnd) break;
                            }
                            else
                            {
                                dst[dstStart++] = src[dStart++];   // d is smallest
                                if (dStart > dEnd) break;
                            }
                        }
                    }
                }
            }
            // Ran out of elements in one of the four segments - i.e. 3 segments are available for merging, but which 3
            // Length needs to be adjusted, to be lengths left yet to be merged for each segment
            aLength = aEnd - aStart + 1;
            bLength = bEnd - bStart + 1;
            cLength = cEnd - cStart + 1;
            dLength = dEnd - dStart + 1;
            if (aStart > aEnd)
            {
                MergeThreeWay2(src, bStart, bLength, cStart, cLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (bStart > bEnd)
            {
                MergeThreeWay2(src, aStart, aLength, cStart, cLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else if (cStart > cEnd)
            {
                MergeThreeWay2(src, aStart, aLength, bStart, bLength, dStart, dLength, dst, dstStart, equalityComparer);
            }
            else   // (dStart > dEnd)
            {
                MergeThreeWay2(src, aStart, aLength, bStart, bLength, cStart, cLength, dst, dstStart, equalityComparer);
            }
        }
    }
}

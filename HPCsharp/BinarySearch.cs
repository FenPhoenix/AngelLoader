#pragma warning disable CA1510
#pragma warning disable CA1002

using System;
using System.Collections.Generic;

namespace HPCsharp
{
    /// <summary>
    /// Algorithms operating on variety of containers, providing trade-off between abstraction and performance
    /// </summary>
    static public partial class Algorithm
    {
        /// <summary>
        /// Binary Search algorithm which searches for a value within a sorted array
        /// </summary>
        /// <typeparam name="T">data type of each element within the array</typeparam>
        /// <param name="value">value being searched for</param>
        /// <param name="a">List being searched within</param>
        /// <param name="left">left/low index of the source array, inclusive</param>
        /// <param name="right">right/high index of the source array, inclusive</param>
        /// <param name="comparer">comparison to be used</param>
        /// <returns>Returns the the left-most index at which the element of the array is larger or equal to the value</returns>
        public static Int32 BinarySearch<T>(T value, T[] a, Int32 left, Int32 right, IComparer<T> comparer = null)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            var equalityComparer = comparer ?? Comparer<T>.Default;
            Int32 low = left;
            Int32 high = Math.Max(left, right + 1);
            while (low < high)
            {
                Int32 mid = low + ((high - low) / 2);   // overflow-free average calculation, since high > low is the condition for entering while-loop body
                if (equalityComparer.Compare(value, a[mid]) <= 0)   // if (value <= a[mid])
                    high = mid;
                else low = mid + 1; // because we compared to a[mid] and the value was larger than a[mid].
                                    // Thus, the next array element to the right from mid is the next possible
                                    // candidate for low, and a[mid] can not possibly be that candidate.
            }
            return high;
        }
    }
}

// TODO: We may need to determine work quanta size and then use Min(Array.Length/workQuanta, numberOfCores) number of cores to keep performance from dropping when going parallel.
//       This is a nice ramp-up from scalar to fully parallel concept, where performance should not drop off as the Array size gets smaller. Show with benchmark data
//       this method to always be as good as scalar for small arrays and then ramp up to being way better for large arrays, where developers have nothing to prevent them
//       from using the parallel version everywhere and always (except for setting the Threshold/MinimumWorkQuanta value).
// TODO: Change Threshold to MinimumWorkQuanta to make it clearer on what the quantity really means.
// TODO: Determine the minimum work quanta where doing work by more than one core/worker uses less time than doing the same amount of work by a single core/worker.
// TODO: Start a document/paper with all of these parallel factors that matter, such as this minimum work quanta, paged in or not of memory, aligned or unaligned scalar
//       and SIMD/SSE size items from memory.
// TODO: Develop a good example for List.ToArrayPar() and Array.ToArray() and CopyTo() to show the performance benfit immediately
// TODO: See if List.CopyTo() can be implemented faster using the same methodology.
// TODO: https://stackoverflow.com/questions/1105990/is-it-better-to-call-tolist-or-toarray-in-linq-queries?rq=1 contribute to this entry with everything learned
// TODO: Contribute to https://stackoverflow.com/questions/6750447/c-toarray-performance and show performance and point to HCPsharp
// TODO: Figure out how to support .AsParallel() by supporting ParallelQuery<List<</List>>
// TODO: Check if it's faster to convert List to Array in parallel and then to create a List from an array
// TODO: Would it be faster to page in a new Array when returning a new Array, followed by copying to it? Could we page in a new array in parallel?
// TODO: Could we create a List from an Array in parallel? Possibly create a .CopyToPar() and/or .ToListPar() and bring parallelism to construction.

#pragma warning disable CA1510

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Copy
    {
        private static void CopyToArrayParallelInnerDac<T>(this List<T> src, Int32 srcStart, T[] dst, Int32 dstStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (length <= 0)      // zero elements to copy
                return;
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount);      // default values for parallelThreshold and degreeOfParallelism
            if (length <= minWorkQuanta || degreeOfParallelism == 1)
            {
                src.CopyTo(srcStart, dst, dstStart, length);
                return;
            }
            int lengthFirstHalf  = length / 2;
            int lengthSecondHalf = length - lengthFirstHalf;
            Parallel.Invoke(
                () => { CopyToArrayParallelInnerDac<T>(src, srcStart,                   dst, dstStart,                   lengthFirstHalf,  (minWorkQuanta, degreeOfParallelism)); },
                () => { CopyToArrayParallelInnerDac<T>(src, srcStart + lengthFirstHalf, dst, dstStart + lengthFirstHalf, lengthSecondHalf, (minWorkQuanta, degreeOfParallelism)); }
            );
            return;
        }

        // TODO: Figure out how to support .AsParallel() by supporting ParallelQuery<List<</List>>. Below is only partially figured out yet.
        //public static T[] ToArray<T>(this ParallelQuery<List<T>> src, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        //{
        //    (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (16 * 1024, 0);      // default values for parallelThreshold and degreeOfParallelism
        //    //var sourceArray = src.;
        //    T[] dst = new T[src.Count()];
        //    CopyToArrayParallelInner<T>(src.Cast<List<T>>(), 0, dst, 0, src.Count(), (minWorkQuanta, degreeOfParallelism));
        //    return dst;
        //}

        /// <summary>
        /// Create a new Array from the source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null)
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[src.Count];
            if ((minWorkQuanta * degreeOfParallelism) < src.Count)
                minWorkQuanta = src.Count / degreeOfParallelism;
            CopyToArrayParallelInnerDac<T>(src, 0, dst, 0, src.Count, (minWorkQuanta, degreeOfParallelism));
            return dst;
        }
        /// <summary>
        /// Create a new Array from a portion of source List
        /// </summary>
        /// <typeparam name="T">data type of each element</typeparam>
        /// <param name="src">source List</param>
        /// <param name="srcStart">starting index within src List</param>
        /// <param name="length">number of elements to be copied</param>
        /// <param name="parSettings">minWorkQuanta = number of array elements efficient to process per core; degreeOfParallelism = maximum number of CPU cores that will be used</param>
        public static T[] ToArrayPar<T>(this List<T> src, Int32 srcStart, Int32 length, (Int32 minWorkQuanta, Int32 degreeOfParallelism)? parSettings = null )
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));
            (Int32 minWorkQuanta, Int32 degreeOfParallelism) = parSettings ?? (64 * 1024, Environment.ProcessorCount / SystemAttributes.HyperthreadingNumberOfWays);      // default values for parallelThreshold and degreeOfParallelism
            T[] dst = new T[length];
            if ((minWorkQuanta * degreeOfParallelism) < src.Count)
                minWorkQuanta = src.Count / degreeOfParallelism;
            CopyToArrayParallelInnerDac<T>(src, srcStart, dst, 0, length, (minWorkQuanta, degreeOfParallelism));
            return dst;
        }
    }
}

// TODO: Write a technical paper on RadixSortFaster, with it's new method to improve memory access pattern of Radix Sort, which is especially affective when sorting
//       arrays or user defined classes, which use references and thus can be scattered all over the heap. Measurements are showing 10X performance improvement.
// TODO: To potentially improve performance of LSD Radix Sort of User Defined Types, create an array of structs that contain a reference to each UDT and key to be sorted on. This way only a single
//       array is being read/written instead of two, which may perform better than two arrays being read/written currently.
// TODO: Try Radix Sort by processing 4-bits at a time to reduce random memory access pattern. Characterize memory access patterns that that CPU can support. When does the performance fall off? 8 buffers?
// TODO: To reduce random memory access pattern develop a multi-buffer class (maybe) that you write thru, which has multiple buffers that you write to, and then automatically
//       flushes them to memory when the individual buffer size reaches its limit. This turns random memory accesses into sequential memory accesses. It also allows
//       to flush all of the buffers at any time. This is a similar strategy to what Intel used to speed up Radix Sort. Then play with the buffer size - i.e. is it cache
//       line size or bigger. This would be a class that you write thru. You would set the address of each destination for each buffer, and then you would write data thru
//       individual buffer to system memory.
// TODO: Make all multi-dimensional buffers a single array, to keep cache usage and mapping to the same cache location impossible.
// TODO: For parallel algorithm, parallel the counting portion of the algorithm, as I've done in my Dr. Dobb's papers for the parallel counting sort and MSD Radix Sort.
//       To start with don't parallel the permuting portion of the algorithm until we figure out how to do it with a performance gain.
// TODO: Figure out why for pre-sorted arrays of uint Radix Sort's 4 passes are much slower on the middle two passes than on the first and last pass. The last pass is
//       understandable since most likely all elements go into a single bin and that's why it's fast. This could be an opportunity to expose a weakness in this algorithm
//       or an opportunity to fix this weakness.
// TODO: Change the count array into a 1-D array to minimize cache contention, since 2-D array gets allocated one row at a time and may cache interfere between rows,
//       depending on how each row gets allocated. With 1-D the memory layout is guaranteed to be contigous, which should produce less cache contention.
//       Do the same with startOfBin 2-D array. It'll be a little bit more painful to use, but performance gains should make it worthwhile.
// TODO: Implement a generic Sort (in-place and not versions) for all of the data types that John listed that would select internally which algorithm to use, so that
//       the user doesn't have to. Maybe allow the user to select the algorithm.
// TODO: Extend Radix Sort to borrow some good ideas from the Radix Sort video and expand on them, such instead of returning just one number from the user defined
//       function, but also accept returning a Tuple of supported data types, and then sort based on these in-order, and throw an unsupported type exception if the user
//       provides a data type that is not supported.
// TODO: Separate the Counting Sort portion from Radix Sort and not only parallelize the counting portion, since that parallelizes counting and reading of the source array
//       but also parallelize the writing portion by splitting up the count array into either divide-and-conquer write method, or more regular chunks.
// TODO: Instead of divide-and-conquer parallel Count by splitting the input array into page size chunks on cache line boundaries (possibly by checking the array starting
//       pointer address) and then divide on page boundaried in a for loop, which should lead to even higher performance.
// TODO: Do a similar method of splitting up the Count array into page size chunks and checking the destination pointer address and splitting up by either divide-and-conquer
//       and by a for loop where each iteration is in parallel and a certain number of pages.
// TODO: Add a task to clean-up some of the sorting interfaces to not have "ref to arrays", just allocate needed arrays internally (less for the user to worry about)
//       when the additional memory is needed and just document it not being a true-inplace algorithm, but just has an in-place interface
// TODO: Document these algorithms as "not truly in-place", but providing an in-place interface. Explain how much additional memory each algorithm uses.
// TODO: Instead of masking and shifting in the inner loop of Radix Sort, use the union, once writes have been de-randomized, it may to improve performance then.
//       Tried replacing the inner loop with union and it turned out to be slower, but this was before de-randomization.
// TODO: Reduce memory footprint of partial array sort by allocating only enough memory for the partial array, instead of needing a temporary array that is a full size.
// TODO: Figure out how to end RadixSort early for those cases where the keys being sorted are within a limited range, such as for keys in a database - e.g. fewer than 16 M keys which are 0 to 16M
//       which is within 24-bits the lower bits. Bring this optimization from MSD Radix Sort, as it should help here as well. It doesn't help LSD Radix Sort as much
//       because for slong when negative and positive values are used we end up with two bins as we get to more significant digits (unless we limit it to just positives eventhough
//       the data type is an slong). However, two bins are find (and possibly even more), since if these bins are already in-order then there is permuting is not needed!
//       This will be a huge speed-up for John's use case! Is it easy to tell if the bins are in the correct order quickly? All negative and all positive? This is extra overhead.
// TODO: Wonder if paging the temporary/destination array into memory first would help performance. Otherwise, if C# does it lazily, then random access may make that slower? Maybe.
// TODO: See if Wikipedia Counting Sort ideas and concepts could possibly help improve performance https://en.wikipedia.org/wiki/Counting_sort
// TODO: Since LSD Radix Sort now does the Counting portion of the algorithm in a single pass outside of the permutation loop that processes based on digits, we could add statistics detection
//       during that pass for very low cost, or possibly almost no cost, since that pass is memory bandwidth limited. Detection of presorted, reverse sorted and constant (which is also presorted)
//       should be simple to detect, even in parallel.
// TODO: If the input array is completely pre-sorted then just output it, otherwise if a certain percentage is pre-sorted (i.e. close to pre-sorted) then use Array.Sort, otherwise use LSD Radix Sort.
// TODO: Apply the Counting/Histogram optimization from my blog to Radix Sort of user defined types (actually across all of the LSD Radix Sorts).
// TODO: Consistently switch to int[] for StartOfBin everywhere, since that improves performance for index operations, since in C# int is the native index type (or is it, something to double-check!)
//       It seems that C# supports several data types for indexes (int, uint, long and ulong). Need to experiment which data type C# prefers (guessing uint, but not sure) to generate the least IL instructions.
//       Post the best type to use on https://stackoverflow.com/questions/16486533/type-of-array-index-in-c once I figure out what that is, whichever generates the least amount of IL
// TODO: figure out why for long[] trick of checking for a single bucket, we still have to sort using the last digit, otherwise incrementing test case fails for some sizes of input array.
// TODO: Improve the interface of LSD Radix Sort function that pass in a key array and a user type array and sort both simultaneously, to return a tuple instead of using a reference, since we want to return
//       two arrays.
// TODO: Consider LSD Radix Sort version that returns sorted array (e.g. array of bytes that was passed in to be sorted) and also creates and returns an array of indexes. In this case, the developer doesn't
//       even have to provide the second input array.
// TODO: JavaScript original version of LSD Radix Sort for UDT's is way faster when the array fits in cache (10X). This is a good behavior to test and document in JavaScript and C#
// TODO: Use the same optimization as done for MSD Radix Sort, which detects if all values fell in a single bin and then doesn't do any work. This helps not only the constant array case, but also
//       the case of indexes being only as large as the array size. This optimization is worthwhile since it helps more than one special case.
// TODO: Optimize floating-point Radix Sort by minimize flipping by doing flipping once at the beginning and at the end.
// TODO: Implement LSD Radix sort for the larger memory array support of C# that I found on StackOverflow.
// TODO: Implement Rick's request on HPCsharp github repo for support of double[] keys, separate from the array being sorted, also with swapped arguments
//       to match Array.Sort(array1, array2) interface. These functions also need to be updated to the latest highest performance versions.
// TODO: Implement ascending/descending options for all LSD Radix Sort algorithms, like is done for the byte[] case, as this can be done without any performance penalties. Show that
//       Array.Sort has performance penalties due to the need to create a lambda function to reverse the comparison.
// TODO: Implement adaptive Radix Sort (serial and parallel) where if there is enough memory, LSD Radix Sort is used for faster performance, otherwise MSD Radix Sort kicks in when memory is tight.

#pragma warning disable CA1510
#pragma warning disable CA1002

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        [StructLayout(LayoutKind.Explicit)]
        internal struct Int32ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;

            [FieldOffset(0)]
            public Int32 integer;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct UInt32ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;

            [FieldOffset(0)]
            public UInt32 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct UInt64ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;
            [FieldOffset(4)]
            public byte byte4;
            [FieldOffset(5)]
            public byte byte5;
            [FieldOffset(6)]
            public byte byte6;
            [FieldOffset(7)]
            public byte byte7;

            [FieldOffset(0)]
            public UInt64 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        internal struct Int64ByteUnion
        {
            [FieldOffset(0)]
            public byte byte0;
            [FieldOffset(1)]
            public byte byte1;
            [FieldOffset(2)]
            public byte byte2;
            [FieldOffset(3)]
            public byte byte3;
            [FieldOffset(4)]
            public byte byte4;
            [FieldOffset(5)]
            public byte byte5;
            [FieldOffset(6)]
            public byte byte6;
            [FieldOffset(7)]
            public byte byte7;

            [FieldOffset(0)]
            public Int64 integer;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct FloatUInt32Union
        {
            [FieldOffset(0)]
            public uint uintegerValue;
            [FieldOffset(0)]
            public float floatValue;
        }
        [StructLayout(LayoutKind.Explicit)]
        public struct DoubleUInt64Union
        {
            [FieldOffset(0)]
            public UInt64 ulongInteger;
            [FieldOffset(0)]
            public double doubleValue;
        }

        /// <summary>
        /// Sort an array of unsigned integers using Radix Sorting algorithm (least significant digit variation - LSD)
        /// This algorithm is not in-place. This algorithm is stable. Two-phase implementation.
        /// </summary>
        /// <param name="inOutArray">array of unsigned integers to be sorted, and where the sorted array will be returned</param>
        /// <param name="startIndex">index of the first element where sorting is to start</param>
        /// <param name="length">number of array elements to sort</param>
        /// <returns>sorted array of unsigned integers</returns>
        public static void SortRadix(this uint[] inOutArray, int startIndex, int length)
        {
            if (inOutArray == null)
                throw new ArgumentNullException(nameof(inOutArray));
            const int bitsPerDigit = 8;
            uint numberOfBins = 1 << bitsPerDigit;
            uint numberOfDigits = (sizeof(uint) * 8 + bitsPerDigit - 1) / bitsPerDigit;
            //Console.WriteLine("SortRadix: NumberOfDigits = {0}", numberOfDigits);
            uint[] workBuffer = new uint[inOutArray.Length];    // TODO: Reduce to length instead of inOutArray.Length
            int d;

            uint[][] startOfBin = new uint[numberOfDigits][];
            for (int i = 0; i < numberOfDigits; i++)
                startOfBin[i] = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = numberOfBins - 1;
            int shiftRightAmount = 0;

            //Stopwatch stopwatch = new Stopwatch();
            //long frequency = Stopwatch.Frequency;
            ////Console.WriteLine("  Timer frequency in ticks per second = {0}", frequency);
            //long nanosecPerTick = (1000L * 1000L * 1000L) / frequency;

            //stopwatch.Restart();
            uint[][] count = HistogramByteComponents(inOutArray, startIndex, startIndex + length - 1);
            //stopwatch.Stop();
            //double timeForCounting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
            //Console.WriteLine("Time for counting: {0}", timeForCounting);

            for (d = 0; d < numberOfDigits; d++)
            {
                startOfBin[d][0] = (uint)startIndex;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[d][i] = startOfBin[d][i - 1] + count[d][i - 1];
            }

            d = 0;
            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                //stopwatch.Restart();
                uint[] startOfBinLoc = startOfBin[d];
                for (int current = startIndex; current < (startIndex + length); current++)
                {
                    workBuffer[startOfBinLoc[(inOutArray[current] & bitMask) >> shiftRightAmount]++] = inOutArray[current];
                    //Console.WriteLine("curr: {0}, index: {1}, startOfBin: {2}", current, (inputArray[current] & bitMask) >> shiftRightAmount, startOfBinLoc[(inputArray[current] & bitMask) >> shiftRightAmount]);
                }
                //stopwatch.Stop();
                //double timeForPermuting = stopwatch.ElapsedTicks * nanosecPerTick / 1000000000.0;
                //Console.WriteLine("Time for permuting: {0}", timeForPermuting);

                bitMask <<= bitsPerDigit;
                shiftRightAmount += bitsPerDigit;
                outputArrayHasResult = !outputArrayHasResult;
                d++;

                uint[] tmp = inOutArray;       // swap input and output arrays
                inOutArray = workBuffer;
                workBuffer = tmp;
            }
        }

        /// <summary>
        /// Sort an array of user defined class containing an unsigned 64-bit integer Key, using Radix Sorting algorithm. Linear time sort algorithm.
        /// Slower algorithm that allocates only 3K bytes of extra memory.
        /// </summary>
        /// <param name="inputArray"></param>
        /// <param name="getKey">user provided function to extract the unsigned 64-bit key sorted on, from the user defined class</param>
        /// <returns>sorted array of user defined class</returns>
        public static void SortRadix<T>(this T[] inputArray, Int32 start, Int32 length, T[] outputArray, Func<T, UInt32> getKey)
        {
            if (inputArray == null)
                throw new ArgumentNullException(nameof(inputArray));
            if (outputArray == null)
                throw new ArgumentNullException(nameof(outputArray));
            if (getKey == null)
                throw new ArgumentNullException(nameof(getKey));
            int numberOfBins = 256;
            int Log2ofPowerOfTwoRadix = 8;
            uint[] count = new uint[numberOfBins];
            bool outputArrayHasResult = false;

            uint bitMask = 255;
            int shiftRightAmount = 0;

            uint[] startOfBin = new uint[numberOfBins];

            while (bitMask != 0)    // end processing digits when all the mask bits have been processed and shifted out, leaving no bits set in the bitMask
            {
                // TODO: This can be optimized by changing to the two phase strategy of counting in a single-pass for all the digits
                for (uint i = 0; i < numberOfBins; i++)
                    count[i] = 0;
                for (int current = start; current < (start + length); current++)    // Scan the array and count the number of times each digit value appears - i.e. size of each bin
                    count[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++;

                startOfBin[0] = (uint)start;
                for (uint i = 1; i < numberOfBins; i++)
                    startOfBin[i] = startOfBin[i - 1] + count[i - 1];

                for (int current = start; current < (start + length); current++)
                    outputArray[startOfBin[ExtractDigit(getKey(inputArray[current]), bitMask, shiftRightAmount)]++] = inputArray[current];

                bitMask <<= Log2ofPowerOfTwoRadix;
                shiftRightAmount += Log2ofPowerOfTwoRadix;
                outputArrayHasResult = !outputArrayHasResult;

                T[] tmp = inputArray;       // swap input and output arrays
                inputArray = outputArray;
                outputArray = tmp;
            }
            if (!outputArrayHasResult)
                for (int current = start; current < (start + length); current++)
                    outputArray[current] = inputArray[current];
        }
        private static UInt32 ExtractDigit(UInt32 value, UInt32 bitMask, int shiftRightAmount)
        {
            return (value & bitMask) >> shiftRightAmount;	// extract the digit we are sorting based on
        }
    }
}

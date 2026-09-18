// TODO: This problem is related to https://stackoverflow.com/questions/49308115/c-sharp-vectordouble-copyto-barely-faster-than-non-simd-version?rq=1
// TODO: Could we improve performance even further by having a single array that is a structure of { A, B } elements
//       making it a single array that is brought into the cache. May not be as generally useful, but there may be cases
//       which benefit from it.
// TODO: Implement AddKernelSSE() function that can be composed with other kernels, to make more complex molecules out atomic
//       functions, and keep performance high. No loops, no if statements, no overhead.

#pragma warning disable CA1510

using System;
using System.Numerics;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class Addition
    {
        public static void AddToSse(this uint[] arrayA, uint[] arrayB)
        {
            if (arrayA == null)
                throw new ArgumentNullException(nameof(arrayA));
            if (arrayB == null)
                throw new ArgumentNullException(nameof(arrayB));
            arrayA.AddToSseInner(arrayB, 0, arrayA.Length - 1);
        }

        private static void AddToSseInner(this uint[] arrayA, uint[] arrayB, int l, int r)
        {
            int sseIndexEnd = l + ((r - l + 1) / Vector<uint>.Count) * Vector<uint>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<int>.Count)
            {
                var inVectorA = new Vector<uint>(arrayA, i);
                var inVectorB = new Vector<uint>(arrayB, i);
                inVectorA += inVectorB;
                inVectorA.CopyTo(arrayA, i);
            }
            for (; i <= r; i++)
                arrayA[i] += arrayB[i];
        }
    }
}

// TODO: Add these algorithms to the Readme table/list of algorithms. Now, that we have SSE that's way faster than scalar.
#pragma warning disable CA1510

using System;

namespace HPCsharp.Algorithms
{
    static public partial class Addition
    {
        public static void AddTo(this int[] arrayA, int[] arrayB)
        {
            if (arrayA == null)
                throw new ArgumentNullException(nameof(arrayA));
            if (arrayB == null)
                throw new ArgumentNullException(nameof(arrayB));
            arrayA.AddToInner(arrayB, 0, arrayA.Length - 1);
        }

        private static void AddToInner(this int[] arrayA, int[] arrayB, int l, int r)
        {
            for (int i = l; i <= r; i++)
                arrayA[i] += arrayB[i];
        }
    }
}

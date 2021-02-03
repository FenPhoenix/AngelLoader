using System;
using System.Collections.Generic;

namespace AngelLoader
{
    public static partial class Misc
    {
        #region Array initialization

        /// <summary>
        /// Returns an array of type <typeparamref name="T"/> with all elements initialized to non-null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        internal static T[] InitializedArray<T>(int length) where T : new()
        {
            T[] ret = new T[length];
            for (int i = 0; i < length; i++) ret[i] = new T();
            return ret;
        }

        /// <summary>
        /// Returns an array of type <typeparamref name="T"/> with all elements initialized to <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        /// <param name="value">The value to initialize all elements with.</param>
        internal static T[] InitializedArray<T>(int length, T value) where T : new()
        {
            T[] ret = new T[length];
            for (int i = 0; i < length; i++) ret[i] = value;
            return ret;
        }

        /// <summary>
        /// Returns two arrays of type <typeparamref name="T1"/> and <typeparamref name="T2"/> respectively,
        /// with all elements initialized to non-null. Uses a single assignment loop for performance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="length"></param>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        internal static void InitializeArrays<T1, T2>(int length,
            out T1[] array1,
            out T2[] array2)
            where T1 : new()
            where T2 : new()
        {
            array1 = new T1[length];
            array2 = new T2[length];
            for (int i = 0; i < length; i++)
            {
                array1[i] = new T1();
                array2[i] = new T2();
            }
        }

        #endregion

        internal static T[] CombineArrays<T>(params T[][] arrays)
        {
            int totalLen = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                totalLen += arrays[i].Length;
            }

            T[] ret = new T[totalLen];

            int pos = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                T[] array = arrays[i];
                int arrayLen = array.Length;

                Array.Copy(array, 0, ret, pos, arrayLen);

                pos += arrayLen;
            }

            return ret;
        }

        #region Clear and add

        internal static void ClearAndAdd<T>(this List<T> list, T item)
        {
            list.Clear();
            list.Add(item);
        }

        internal static void ClearAndAdd<T>(this List<T> list, IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }

        #endregion

        internal static int FindIndexOfByteSequence(List<byte> input, byte[] pattern, int start = 0)
        {
            byte firstByte = pattern[0];
            int index = input.IndexOf(firstByte, start);

            while (index > -1)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (index + i >= input.Count) return -1;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = input.IndexOf(firstByte, index + i)) == -1) return -1;
                        break;
                    }

                    if (i == pattern.Length - 1) return index;
                }
            }

            return -1;
        }
    }
}

using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace FMScanner;

internal static class StringSplit
{
    [StructLayout(LayoutKind.Auto)]
    private readonly ref struct RentedArray<T>
    {
        public readonly T[] Array;
        public readonly int Length;

        public RentedArray(T[] array, int length)
        {
            Array = array;
            Length = length;
        }

        public static RentedArray<T> Empty => new();

        public RentedArray()
        {
            Array = System.Array.Empty<T>();
            Length = -1;
        }
    }

    // Framework 4.8 versions but with the ability to rent the arrays

    internal static string[] Split_String(this string str, string[] separator, StringSplitOptions options, ArrayPool<int> arrayPool)
    {
        bool omitEmptyEntries = options == StringSplitOptions.RemoveEmptyEntries;

        if (separator.Length == 0)
        {
            return Split_Char(str, null, options, arrayPool);
        }

        if (omitEmptyEntries && str.Length == 0)
        {
            return Array.Empty<string>();
        }

        var sepListRA = new RentedArray<int>(arrayPool.Rent(str.Length), str.Length);
        var lengthListRA = new RentedArray<int>(arrayPool.Rent(str.Length), str.Length);
        try
        {
            int numReplaces = MakeSeparatorList(str, separator, ref sepListRA, ref lengthListRA);

            //Handle the special case of no replaces and special count.
            if (numReplaces == 0)
            {
                string[] stringArray = new string[1];
                stringArray[0] = str;
                return stringArray;
            }

            if (omitEmptyEntries)
            {
                return InternalSplitOmitEmptyEntries(str, sepListRA, lengthListRA, numReplaces);
            }
            else
            {
                return InternalSplitKeepEmptyEntries(str, sepListRA, lengthListRA, numReplaces);
            }
        }
        finally
        {
            arrayPool.Return(sepListRA.Array);
            arrayPool.Return(lengthListRA.Array);
        }
    }

    internal static string[] Split_Char(this string str, char[]? separator, StringSplitOptions options, ArrayPool<int> arrayPool)
    {
        bool omitEmptyEntries = options == StringSplitOptions.RemoveEmptyEntries;

        if (omitEmptyEntries && str.Length == 0)
        {
            return Array.Empty<string>();
        }

        var sepListRA = new RentedArray<int>(arrayPool.Rent(str.Length), str.Length);
        try
        {
            int numReplaces = MakeSeparatorList(str, separator, ref sepListRA);

            //Handle the special case of no replaces and special count.
            if (numReplaces == 0)
            {
                string[] stringArray = new string[1];
                stringArray[0] = str;
                return stringArray;
            }

            if (omitEmptyEntries)
            {
                return InternalSplitOmitEmptyEntries(str, sepListRA, RentedArray<int>.Empty, numReplaces);
            }
            else
            {
                return InternalSplitKeepEmptyEntries(str, sepListRA, RentedArray<int>.Empty, numReplaces);
            }
        }
        finally
        {
            arrayPool.Return(sepListRA.Array);
        }
    }

    #region Private methods

    private static unsafe int MakeSeparatorList(string str, char[]? separator, ref RentedArray<int> sepList)
    {
        int foundCount = 0;

        if (separator == null || separator.Length == 0)
        {
            fixed (char* pwzChars = str)
            {
                //If they passed null or an empty string, look for whitespace.
                for (int i = 0; i < str.Length && foundCount < sepList.Length; i++)
                {
                    if (char.IsWhiteSpace(pwzChars[i]))
                    {
                        sepList.Array[foundCount++] = i;
                    }
                }
            }
        }
        else
        {
            int sepListCount = sepList.Length;
            int sepCount = separator.Length;
            //If they passed in a string of chars, actually look for those chars.
            fixed (char* pwzChars = str, pSepChars = separator)
            {
                for (int i = 0; i < str.Length && foundCount < sepListCount; i++)
                {
                    char* pSep = pSepChars;
                    for (int j = 0; j < sepCount; j++, pSep++)
                    {
                        if (pwzChars[i] == *pSep)
                        {
                            sepList.Array[foundCount++] = i;
                            break;
                        }
                    }
                }
            }
        }
        return foundCount;
    }

    private static string[] InternalSplitOmitEmptyEntries(string str, RentedArray<int> sepList, RentedArray<int> lengthList, int numReplaces)
    {
        // Allocate array to hold items. This array may not be
        // filled completely in this function, we will create a
        // new array and copy string references to that new array.

        const int count = int.MaxValue;

        int maxItems = numReplaces < count ? numReplaces + 1 : count;
        string[] splitStrings = new string[maxItems];

        int currIndex = 0;
        int arrIndex = 0;

        for (int i = 0; i < numReplaces && currIndex < str.Length; i++)
        {
            if (sepList.Array[i] - currIndex > 0)
            {
                splitStrings[arrIndex++] = str.Substring(currIndex, sepList.Array[i] - currIndex);
            }
            currIndex = sepList.Array[i] + (lengthList.Length == -1 ? 1 : lengthList.Array[i]);
            if (arrIndex == count - 1)
            {
                // If all the remaining entries at the end are empty, skip them
                while (i < numReplaces - 1 && currIndex == sepList.Array[++i])
                {
                    currIndex += lengthList.Length == -1 ? 1 : lengthList.Array[i];
                }
                break;
            }
        }

        //Handle the last string at the end of the array if there is one.
        if (currIndex < str.Length)
        {
            splitStrings[arrIndex++] = str.Substring(currIndex);
        }

        string[] stringArray = splitStrings;
        if (arrIndex != maxItems)
        {
            stringArray = new string[arrIndex];
            for (int j = 0; j < arrIndex; j++)
            {
                stringArray[j] = splitStrings[j];
            }
        }
        return stringArray;
    }

    private static string[] InternalSplitKeepEmptyEntries(string str, RentedArray<int> sepList, RentedArray<int> lengthList, int numReplaces)
    {
        int currIndex = 0;
        int arrIndex = 0;

        const int count = int.MaxValue - 1;

        int numActualReplaces = numReplaces < count ? numReplaces : count;

        //Allocate space for the new array.
        //+1 for the string from the end of the last replace to the end of the String.
        string[] splitStrings = new string[numActualReplaces + 1];

        for (int i = 0; i < numActualReplaces && currIndex < str.Length; i++)
        {
            splitStrings[arrIndex++] = str.Substring(currIndex, sepList.Array[i] - currIndex);
            currIndex = sepList.Array[i] + (lengthList.Length == -1 ? 1 : lengthList.Array[i]);
        }

        //Handle the last string at the end of the array if there is one.
        if (currIndex < str.Length && numActualReplaces >= 0)
        {
            splitStrings[arrIndex] = str.Substring(currIndex);
        }
        else if (arrIndex == numActualReplaces)
        {
            //We had a separator character at the end of a string.  Rather than just allowing
            //a null character, we'll replace the last element in the array with an empty string.
            splitStrings[arrIndex] = string.Empty;

        }

        return splitStrings;
    }

    private static unsafe int MakeSeparatorList(string str, string[] separators, ref RentedArray<int> sepList, ref RentedArray<int> lengthList)
    {
        int foundCount = 0;
        int sepListCount = sepList.Length;
        int sepCount = separators.Length;

        fixed (char* pwzChars = str)
        {
            for (int i = 0; i < str.Length && foundCount < sepListCount; i++)
            {
                for (int j = 0; j < sepCount; j++)
                {
                    string separator = separators[j];
                    if (string.IsNullOrEmpty(separator))
                    {
                        continue;
                    }
                    int currentSepLength = separator.Length;
                    if (pwzChars[i] == separator[0] && currentSepLength <= str.Length - i)
                    {
                        if (currentSepLength == 1
                            || string.CompareOrdinal(str, i, separator, 0, currentSepLength) == 0)
                        {
                            sepList.Array[foundCount] = i;
                            lengthList.Array[foundCount] = currentSepLength;
                            foundCount++;
                            i += currentSepLength - 1;
                            break;
                        }
                    }
                }
            }
        }
        return foundCount;
    }

    #endregion
}

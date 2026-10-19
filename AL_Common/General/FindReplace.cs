using System;
using System.Runtime.CompilerServices;

namespace AL_Common;

public static partial class Common
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Array_IndexOfByte_Fast(byte[] array, byte value)
    {
#if X64
        return array.AsSpan().IndexOf(value);
#else
        return Array.IndexOf(array, value);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Array_IndexOfByte_Fast(byte[] array, byte value, int startIndex)
    {
#if X64
        int index = array.AsSpan(startIndex).IndexOf(value);
        if (index > -1) index += startIndex;
        return index;
#else
        return Array.IndexOf(array, value, startIndex);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Array_IndexOfByte_Fast(byte[] array, byte value, int startIndex, int count)
    {
#if X64
        int index = array.AsSpan(startIndex, count).IndexOf(value);
        if (index > -1) index += startIndex;
        return index;
#else
        return Array.IndexOf(array, value, startIndex, count);
#endif
    }

    public static int FindIndexOfByteSequence(byte[] input, byte[] pattern, int start = 0)
    {
#if X64
        int index = input.AsSpan(start).IndexOf(pattern);
        if (index > -1) index += start;
        return index;
#else
        byte firstByte = pattern[0];
        int index = Array.IndexOf(input, firstByte, start);

        while (index > -1)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (index + i >= input.Length) return -1;
                if (pattern[i] != input[index + i])
                {
                    if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return -1;
                    break;
                }

                if (i == pattern.Length - 1) return index;
            }
        }

        return -1;
#endif
    }

    /// <summary>
    /// Replaces all instances in <see paramref="input"/> of <see paramref="pattern"/> with <see paramref="replacePattern"/>.
    /// <see paramref="pattern"/> and <see paramref="replacePattern"/> MUST both be the same length.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="pattern"></param>
    /// <param name="replacePattern"></param>
    public static void ReplaceByteSequence(byte[] input, byte[] pattern, byte[] replacePattern)
    {
#if X64
        int patternLength = pattern.Length;
        int index = 0;
        while (true)
        {
            index = FindIndexOfByteSequence(input, pattern, index);
            if (index == -1) break;

            Array.Copy(replacePattern, 0, input, index, patternLength);
            index += patternLength;
        }
#else
        byte firstByte = pattern[0];
        int index = Array_IndexOfByte_Fast(input, firstByte);
        int pLen = pattern.Length;

        while (index > -1)
        {
            for (int i = 0; i < pLen; i++)
            {
                if (index + i >= input.Length) return;
                if (pattern[i] != input[index + i])
                {
                    if ((index = Array_IndexOfByte_Fast(input, firstByte, index + i)) == -1) return;
                    break;
                }

                if (i == pLen - 1)
                {
                    for (int j = index, ri = 0; j < index + pLen; j++, ri++)
                    {
                        input[j] = replacePattern[ri];
                    }
                }
            }
        }
#endif
    }

    public static bool Contains(this byte[] input, byte[] pattern, int length = -1)
    {
        if (length == -1) length = input.Length;
#if X64
        return input.AsSpan(0, length).IndexOf(pattern.AsSpan()) > -1;
#else

        byte firstByte = pattern[0];
        int index = Array.IndexOf(input, firstByte, 0, length);

        while (index > -1)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (index + i >= length) return false;
                if (pattern[i] != input[index + i])
                {
                    if ((index = Array.IndexOf(input, firstByte, index + i, length - (index + i))) == -1) return false;
                    break;
                }

                if (i == pattern.Length - 1) return true;
            }
        }

        return false;
#endif
    }
}

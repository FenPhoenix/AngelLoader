using System;

namespace AL_Common;

public static partial class Common
{
    public static int FindIndexOfByteSequence(byte[] input, byte[] pattern, int start = 0)
    {
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
    }

    public static void ReplaceByteSequence(byte[] input, byte[] pattern, byte[] replacePattern)
    {
        byte firstByte = pattern[0];
        int index = Array.IndexOf(input, firstByte);
        int pLen = pattern.Length;

        while (index > -1)
        {
            for (int i = 0; i < pLen; i++)
            {
                if (index + i >= input.Length) return;
                if (pattern[i] != input[index + i])
                {
                    if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return;
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
    }

    public static bool Contains(this byte[] input, byte[] pattern, int length = -1)
    {
        if (length == -1) length = input.Length;
        return input.AsSpan(0, length).IndexOf(pattern.AsSpan()) > -1;
    }
}

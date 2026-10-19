using System;

namespace AL_Common;

public static partial class Common
{
    public static int FindIndexOfByteSequence(byte[] input, byte[] pattern, int start = 0)
    {
        int index = input.AsSpan(start).IndexOf(pattern);
        if (index > -1) index += start;
        return index;
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
        int patternLength = pattern.Length;
        int index = 0;
        while (true)
        {
            index = FindIndexOfByteSequence(input, pattern, index);
            if (index == -1) break;

            Array.Copy(replacePattern, 0, input, index, patternLength);
            index += patternLength;
        }
    }

    public static bool Contains(this byte[] input, byte[] pattern, int length = -1)
    {
        if (length == -1) length = input.Length;
        return input.AsSpan(0, length).IndexOf(pattern.AsSpan()) > -1;
    }
}

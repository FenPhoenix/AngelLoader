using System;

namespace AL_Common;

public static partial class Common
{
    #region Methods

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

#if false
    public static int FindIndexOfCharSequence(string input, string pattern, int start = 0)
    {
        char firstChar = pattern[0];
        int index = input.IndexOf(firstChar, start);

        while (index > -1)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (index + i >= input.Length) return -1;
                if (pattern[i] != input[index + i])
                {
                    if ((index = input.IndexOf(firstChar, index + i)) == -1) return -1;
                    break;
                }

                if (i == pattern.Length - 1) return index;
            }
        }

        return -1;
    }

    public static int FindIndexOfByteSequence(List<byte> input, byte[] pattern, int start = 0)
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

    /// <summary>
    /// What it says
    /// </summary>
    /// <param name="input"></param>
    /// <param name="groupControlWord">Must start with '{', for example "{\fonttbl"</param>
    /// <param name="start"></param>
    /// <returns></returns>
    public static (bool Found, int StartIndex, int EndIndex)
    FindStartAndEndIndicesOfRtfGroup(byte[] input, byte[] groupControlWord, int start = 0)
    {
        int index = FindIndexOfByteSequence(input, groupControlWord, start: start);
        if (index == -1) return (false, -1, -1);

        int braceLevel = 1;

        for (int i = index + 1; i < input.Length; i++)
        {
            byte b = input[i];
            if (b == (byte)'{')
            {
                braceLevel++;
            }
            else if (b == (byte)'}')
            {
                braceLevel--;
            }

            if (braceLevel < 1) return (true, index, i + 1);
        }

        return (false, -1, -1);
    }

#endif

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

#if false

    public static void ReplaceByteSequence(List<byte> input, byte[] pattern, byte[] replacePattern)
    {
        byte firstByte = pattern[0];
        int index = input.IndexOf(firstByte);
        int pLen = pattern.Length;

        while (index > -1)
        {
            for (int i = 0; i < pLen; i++)
            {
                if (index + i >= input.Count) return;
                if (pattern[i] != input[index + i])
                {
                    if ((index = input.IndexOf(firstByte, index + i)) == -1) return;
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

#endif

    // I don't know if this is "supposed" to be the fastest way, but every other algorithm I've tried is at
    // least 2-8x slower. IndexOf() calls an internal method TrySZIndexOf() which is obviously some voodoo
    // speed demon stuff because none of this Moyer-Bohr-Kensington-Smythe-Wappcapplet fancy stuff beats it.
    // Or maybe I just don't know what I'm doing. Either way.
    public static bool Contains(this byte[] input, byte[] pattern, int length = -1)
    {
        if (length == -1) length = input.Length;

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
    }

    #endregion
}

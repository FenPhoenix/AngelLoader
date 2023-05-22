using System;
using System.IO;

namespace SharpCompress;

internal static class Utility
{
    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Amount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    internal static int URShift(int number, int bits)
    {
        if (number >= 0)
        {
            return number >> bits;
        }
        return (number >> bits) + (2 << ~bits);
    }

    /// <summary>
    /// Performs an unsigned bitwise right shift with the specified number
    /// </summary>
    /// <param name="number">Number to operate on</param>
    /// <param name="bits">Amount of bits to shift</param>
    /// <returns>The resulting number from the shift operation</returns>
    internal static long URShift(long number, int bits)
    {
        if (number >= 0)
        {
            return number >> bits;
        }
        return (number >> bits) + (2L << ~bits);
    }

    internal static DateTime? TranslateTime(ulong time)
    {
        //maximum Windows file time 31.12.9999
        return time <= 2_650_467_743_999_999_999 ? DateTime.FromFileTimeUtc((long)time).ToLocalTime() : null;
    }

    internal static void ReadExact(this Stream stream, byte[] buffer, int offset, int length)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (buffer is null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0 || offset > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (length < 0 || length > buffer.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        while (length > 0)
        {
            int fetched = stream.Read(buffer, offset, length);
            if (fetched <= 0)
            {
                throw new EndOfStreamException();
            }

            offset += fetched;
            length -= fetched;
        }
    }
}

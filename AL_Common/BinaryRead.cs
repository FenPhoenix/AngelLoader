﻿using System;
using System.IO;
using AL_Common.FastZipReader;

namespace AL_Common;

/// <summary>
/// Static methods as a replacement for BinaryReader, where you pass in your own buffer so it doesn't have to
/// allocate its own fifteen trillion times. Also, we avoid the unnecessary encoding junk that it also does
/// because we're reading binary, not encoded data, sheesh!
/// </summary>
public static class BinaryRead
{
#if false

    /// <summary>Reads a <see langword="Boolean" /> value from the current stream and advances the current position of the stream by one byte.</summary>
    /// <returns>
    /// <see langword="true" /> if the byte is nonzero; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static bool ReadBoolean(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 1, buffer);
        return buffer[0] > 0;
    }

#endif

    /// <summary>Reads the next byte from the current stream and advances the current position of the stream by one byte.</summary>
    /// <returns>The next byte read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static byte ReadByte(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 1, buffer);
        return buffer[0];
    }

#if false

    /// <summary>Reads a signed byte from this stream and advances the current position of the stream by one byte.</summary>
    /// <returns>A signed byte read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static sbyte ReadSByte(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 1, buffer);
        return (sbyte)buffer[0];
    }

    /// <summary>Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.</summary>
    /// <returns>A 2-byte signed integer read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static short ReadInt16(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 2, buffer);
        return (short)((int)buffer[0] | (int)buffer[1] << 8);
    }

#endif

    /// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
    /// <returns>A 2-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static ushort ReadUInt16(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 2, buffer);
        return (ushort)((uint)buffer[0] | (uint)buffer[1] << 8);
    }

#if false

    /// <summary>Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.</summary>
    /// <returns>A 4-byte signed integer read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static int ReadInt32(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 4, buffer);
        return (int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24;
    }

#endif

    /// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
    /// <returns>A 4-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static uint ReadUInt32(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 4, buffer);
        return (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
    }

#if false

    /// <summary>Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte signed integer read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static long ReadInt64(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 8, buffer);
        return (long)(uint)((int)buffer[4] | (int)buffer[5] << 8 | (int)buffer[6] << 16 | (int)buffer[7] << 24) << 32 | (long)(uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
    }

#endif

    /// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    public static ulong ReadUInt64(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 8, buffer);
        return (ulong)(uint)((int)buffer[4] | (int)buffer[5] << 8 | (int)buffer[6] << 16 | (int)buffer[7] << 24) << 32 | (ulong)(uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
    }

#if false

    /// <summary>Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.</summary>
    /// <returns>A 4-byte floating point value read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static unsafe float ReadSingle(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 4, buffer);
        uint tmpBuffer = (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
        return *((float*)&tmpBuffer);
    }

    /// <summary>Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte floating point value read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static unsafe double ReadDouble(Stream stream, byte[] buffer)
    {
        FillBuffer(stream, 8, buffer);
        uint lo = (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
        uint hi = (uint)(buffer[4] | buffer[5] << 8 | buffer[6] << 16 | buffer[7] << 24);
        ulong tmpBuffer = ((ulong)hi) << 32 | lo;
        return *((double*)&tmpBuffer);
    }

#endif

    /// <summary>Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.</summary>
    /// <param name="stream"></param>
    /// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
    /// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
    /// <exception cref="T:System.ArgumentException">The number of decoded characters to read is greater than <paramref name="count" />. This can happen if a Unicode decoder returns fallback characters or a surrogate pair.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="count" /> is negative.</exception>
    public static byte[] ReadBytes(Stream stream, int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_NeedNonNegNum);
        }
        if (count == 0)
        {
            return Array.Empty<byte>();
        }

        byte[] numArray = new byte[count];
        int length = 0;
        do
        {
            int num = stream.Read(numArray, length, count);
            if (num != 0)
            {
                length += num;
                count -= num;
            }
            else
            {
                break;
            }
        }
        while (count > 0);
        if (length != numArray.Length)
        {
            byte[] dst = new byte[length];
            Buffer.BlockCopy(numArray, 0, dst, 0, length);
            numArray = dst;
        }
        return numArray;
    }

    private static void FillBuffer(Stream stream, int numBytes, byte[] buffer)
    {
        if (numBytes > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(numBytes), SR.ArgumentOutOfRange_BinaryReaderFillBuffer);
        }

        int offset = 0;

        if (numBytes == 1)
        {
            // Avoid calling Stream.ReadByte() because it allocates a 1-byte buffer every time (ridiculous)
            int num = stream.Read(buffer, 0, 1);
            if (num <= 0) ThrowHelper.EndOfFile();
        }
        else
        {
            do
            {
                int num = stream.Read(buffer, offset, numBytes - offset);
                if (num == 0) ThrowHelper.EndOfFile();
                offset += num;
            }
            while (offset < numBytes);
        }
    }
}

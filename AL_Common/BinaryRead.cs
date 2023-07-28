//#define ENABLE_UNUSED

using System.IO;
namespace AL_Common;

public readonly struct BinaryBuffer
{
    public readonly byte[] Buffer;
    public BinaryBuffer() => Buffer = new byte[8];
}

/// <summary>
/// Static methods as a replacement for BinaryReader, where you pass in your own buffer so it doesn't have to
/// allocate its own fifteen trillion times. Also, we avoid the unnecessary encoding junk that it also does
/// because we're reading binary, not encoded data, sheesh!
/// </summary>
public static class BinaryRead
{
#if ENABLE_UNUSED

    /// <summary>Reads a <see langword="Boolean" /> value from the current stream and advances the current position of the stream by one byte.</summary>
    /// <returns>
    /// <see langword="true" /> if the byte is nonzero; otherwise, <see langword="false" />.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static bool ReadBoolean(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 1, buffer);
        return buffer.Buffer[0] != 0;
    }

#endif

    /// <summary>Reads the next byte from the current stream and advances the current position of the stream by one byte.</summary>
    /// <returns>The next byte read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static byte ReadByte(Stream stream, BinaryBuffer buffer)
    {
        int bytesRead = stream.Read(buffer.Buffer, 0, 1);
        if (bytesRead == 0) ThrowHelper.EndOfFile();
        return buffer.Buffer[0];
    }

#if ENABLE_UNUSED

    /// <summary>Reads a signed byte from this stream and advances the current position of the stream by one byte.</summary>
    /// <returns>A signed byte read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static sbyte ReadSByte(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 1, buffer);
        return (sbyte)(buffer.Buffer[0]);
    }

    /// <summary>Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.</summary>
    /// <returns>A 2-byte signed integer read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static short ReadInt16(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 2, buffer);
        return (short)(buffer.Buffer[0] | buffer.Buffer[1] << 8);
    }

#endif

    /// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
    /// <returns>A 2-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static ushort ReadUInt16(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 2, buffer);
        return (ushort)(buffer.Buffer[0] | buffer.Buffer[1] << 8);
    }

#if ENABLE_UNUSED

    /// <summary>Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.</summary>
    /// <returns>A 4-byte signed integer read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static int ReadInt32(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 4, buffer);
        return (int)(buffer.Buffer[0] | buffer.Buffer[1] << 8 | buffer.Buffer[2] << 16 | buffer.Buffer[3] << 24);
    }

#endif

    /// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
    /// <returns>A 4-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static uint ReadUInt32(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 4, buffer);
        return (uint)(buffer.Buffer[0] | buffer.Buffer[1] << 8 | buffer.Buffer[2] << 16 | buffer.Buffer[3] << 24);
    }

#if ENABLE_UNUSED

    /// <summary>Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte signed integer read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static long ReadInt64(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 8, buffer);
        uint lo = (uint)(buffer.Buffer[0] | buffer.Buffer[1] << 8 |
                         buffer.Buffer[2] << 16 | buffer.Buffer[3] << 24);
        uint hi = (uint)(buffer.Buffer[4] | buffer.Buffer[5] << 8 |
                         buffer.Buffer[6] << 16 | buffer.Buffer[7] << 24);
        return (long)((ulong)hi) << 32 | lo;
    }

#endif

    /// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    public static ulong ReadUInt64(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 8, buffer);
        uint lo = (uint)(buffer.Buffer[0] | buffer.Buffer[1] << 8 |
                         buffer.Buffer[2] << 16 | buffer.Buffer[3] << 24);
        uint hi = (uint)(buffer.Buffer[4] | buffer.Buffer[5] << 8 |
                         buffer.Buffer[6] << 16 | buffer.Buffer[7] << 24);
        return ((ulong)hi) << 32 | lo;
    }

#if ENABLE_UNUSED

    /// <summary>Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.</summary>
    /// <returns>A 4-byte floating point value read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static unsafe float ReadSingle(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 4, buffer);
        uint tmpBuffer = (uint)(buffer.Buffer[0] | buffer.Buffer[1] << 8 | buffer.Buffer[2] << 16 | buffer.Buffer[3] << 24);
        return *((float*)&tmpBuffer);
    }

    /// <summary>Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte floating point value read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static unsafe double ReadDouble(Stream stream, BinaryBuffer buffer)
    {
        FillBuffer(stream, 8, buffer);
        uint lo = (uint)(buffer.Buffer[0] | buffer.Buffer[1] << 8 |
                         buffer.Buffer[2] << 16 | buffer.Buffer[3] << 24);
        uint hi = (uint)(buffer.Buffer[4] | buffer.Buffer[5] << 8 |
                         buffer.Buffer[6] << 16 | buffer.Buffer[7] << 24);

        ulong tmpBuffer = ((ulong)hi) << 32 | lo;
        return *((double*)&tmpBuffer);
    }

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
            ThrowHelper.ArgumentOutOfRange(nameof(count), SR.ArgumentOutOfRange_NeedNonNegNum);
        }
        if (count == 0)
        {
            return System.Array.Empty<byte>();
        }

        byte[] result = new byte[count];

        int numRead = 0;
        do
        {
            int n = stream.Read(result, numRead, count);
            if (n != 0)
            {
                numRead += n;
                count -= n;
            }
            else
            {
                break;
            }
        }
        while (count > 0);

        if (numRead != result.Length)
        {
            byte[] copy = new byte[numRead];
            System.Buffer.BlockCopy(result, 0, copy, 0, numRead);
            result = copy;
        }

        return result;
    }

#endif

    private static void FillBuffer(Stream stream, int numBytes, BinaryBuffer buffer)
    {
        int bytesRead = 0;
        do
        {
            int n = stream.Read(buffer.Buffer, bytesRead, numBytes - bytesRead);
            if (n == 0) ThrowHelper.EndOfFile();
            bytesRead += n;
        }
        while (bytesRead < numBytes);
    }
}

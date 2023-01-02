// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using AL_Common;

namespace FMScanner.FastZipReader;

public sealed class ZipCompressionMethodException : Exception
{
    public ZipCompressionMethodException()
    {
    }

    public ZipCompressionMethodException(string message) : base(message)
    {
    }

    public ZipCompressionMethodException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

internal static class ThrowHelper
{
    internal static void ArgumentException(string message) => throw new ArgumentException(message);
    internal static void ArgumentOutOfRange(string paramName, string message) => throw new ArgumentOutOfRangeException(paramName, message);
    internal static void EndOfFile() => throw new EndOfStreamException(SR.EOF_ReadBeyondEOF);
    internal static void InvalidData(string message) => throw new InvalidDataException(message);
    internal static void IOException(string message) => throw new IOException(message);
    internal static void NotSupported(string message) => throw new NotSupportedException(message);
    internal static void ReadModeCapabilities() => throw new ArgumentException(SR.ReadModeCapabilities);
    internal static void SplitSpanned() => throw new InvalidDataException(SR.SplitSpanned);
    internal static void ZipCompressionMethodException(string message) => throw new ZipCompressionMethodException(message);
}

// We should try to just make the zip archive classes be like the scanner, where it's one object that just
// has like a Reset(stream) method that loads another stream and resets all its values. That'd be much nicer.
public sealed class ZipReusableBundle : IDisposable
{
    internal readonly SubReadStream ArchiveSubReadStream = new();

    internal readonly byte[] FileStreamBuffer = new byte[4096];

    internal readonly byte[] DataBuffer = new byte[ushort.MaxValue];
    internal readonly byte[] FilenameBuffer = new byte[ushort.MaxValue];

    private const int _backwardsSeekingBufferSize = 32;
    internal const int ThrowAwayBufferSize = 64;

    internal readonly byte[] BackwardsSeekingBuffer = new byte[_backwardsSeekingBufferSize];
    internal readonly byte[] ThrowawayBuffer = new byte[ThrowAwayBufferSize];

    private readonly byte[] _buffer = new byte[16];

    /// <summary>Reads the next byte from the current stream and advances the current position of the stream by one byte.</summary>
    /// <returns>The next byte read from the current stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    internal byte ReadByte(Stream stream)
    {
        // Avoid calling Read() because it allocates a 1-byte buffer every time (ridiculous)
        int num = stream.Read(_buffer, 0, 1);
        if (num <= 0) ThrowHelper.EndOfFile();
        return (byte)num;
    }

    /// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
    /// <returns>A 2-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    internal ushort ReadUInt16(Stream stream)
    {
        FillBuffer(stream, 2);
        return (ushort)((uint)_buffer[0] | (uint)_buffer[1] << 8);
    }

    /// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
    /// <returns>A 4-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    internal uint ReadUInt32(Stream stream)
    {
        FillBuffer(stream, 4);
        return (uint)((int)_buffer[0] | (int)_buffer[1] << 8 | (int)_buffer[2] << 16 | (int)_buffer[3] << 24);
    }

    /// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    internal ulong ReadUInt64(Stream stream)
    {
        FillBuffer(stream, 8);
        return (ulong)(uint)((int)_buffer[4] | (int)_buffer[5] << 8 | (int)_buffer[6] << 16 | (int)_buffer[7] << 24) << 32 | (ulong)(uint)((int)_buffer[0] | (int)_buffer[1] << 8 | (int)_buffer[2] << 16 | (int)_buffer[3] << 24);
    }

    /// <summary>Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.</summary>
    /// <param name="stream"></param>
    /// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
    /// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="count" /> is negative.</exception>
    internal static byte[] ReadBytes_UShort(Stream stream, ushort count)
    {
        // ushort because that's all we ever call it with, and then we also don't need to do the negative check.
        if (count == 0) return Array.Empty<byte>();

        int countInternal = count;
        byte[] numArray = new byte[countInternal];
        int length = 0;

        do
        {
            int num = stream.Read(numArray, length, countInternal);
            if (num != 0)
            {
                length += num;
                countInternal -= num;
            }
            else
            {
                break;
            }
        } while (countInternal > 0);

        if (length != numArray.Length)
        {
            byte[] dst = new byte[length];
            Buffer.BlockCopy(numArray, 0, dst, 0, length);
            numArray = dst;
        }

        return numArray;
    }

    private void FillBuffer(Stream stream, int numBytes)
    {
        int offset = 0;

        do
        {
            int num = stream.Read(_buffer, offset, numBytes - offset);
            if (num == 0) ThrowHelper.EndOfFile();
            offset += num;
        }
        while (offset < numBytes);
    }

    public void Dispose() => ArchiveSubReadStream.Dispose();
}

internal static class ZipHelpers
{
    private const int ValidZipDate_YearMin = 1980;

    /// <summary>
    /// Converts a Zip timestamp to a DateTime object. If <paramref name="zipDateTime"/> is not a
    /// valid Zip timestamp, an indicator value of 1980 January 1 at midnight will be returned.
    /// </summary>
    /// <param name="zipDateTime"></param>
    /// <returns></returns>
    internal static DateTime ZipTimeToDateTime(uint zipDateTime)
    {
        // DosTime format 32 bits
        // Year: 7 bits, 0 is 1980
        // Month: 4 bits
        // Day: 5 bits
        // Hour: 5 bits
        // Minute: 6 bits
        // Second: 5 bits

        // do the bit shift as unsigned because the fields are unsigned, but
        // we can safely convert to int, because they won't be too big
        int year = (int)(ValidZipDate_YearMin + (zipDateTime >> 25));
        int month = (int)((zipDateTime >> 21) & 0xF);
        int day = (int)((zipDateTime >> 16) & 0x1F);
        int hour = (int)((zipDateTime >> 11) & 0x1F);
        int minute = (int)((zipDateTime >> 5) & 0x3F);
        int second = (int)((zipDateTime & 0x001F) * 2); // only 5 bits for second, so we only have a granularity of 2 sec.

        try
        {
            return new DateTime(year, month, day, hour, minute, second);
        }
        // Note: This is where my mischievous little "'System.ArgumentOutOfRangeException' in mscorlib.dll"
        // is coming from. Turns out the perf penalty was inconsequential even when this was being done for
        // every single entry, but now that I'm only doing it for a handful per FM, it's not worth worrying
        // about at all.
        catch (ArgumentOutOfRangeException)
        {
            return new DateTime(ValidZipDate_YearMin, 1, 1, 0, 0, 0);
        }
    }

    internal const uint Mask32Bit = 0xFFFFFFFF;
    internal const ushort Mask16Bit = 0xFFFF;

    /// <summary>
    /// Reads exactly bytesToRead out of stream, unless it is out of bytes
    /// </summary>
    private static void ReadBytes(Stream stream, byte[] buffer, int bytesToRead)
    {
        int bytesLeftToRead = bytesToRead;

        int totalBytesRead = 0;

        while (bytesLeftToRead > 0)
        {
            // @MEM: FileStream() has an internal buffer that you can't pass in, so it gets recreated ten trillion times
            // Just reading the zips for total uncompressed size causes ~8MB of allocations in these file
            // streams, when checking the ~1600 set.
            // 2041 allocations of (4096 + 12 overhead per array object) = 8,384,428 bytes
            int bytesRead = stream.Read(buffer, totalBytesRead, bytesLeftToRead);
            if (bytesRead == 0) ThrowHelper.IOException(SR.UnexpectedEndOfStream);

            totalBytesRead += bytesRead;
            bytesLeftToRead -= bytesRead;
        }
    }

    // assumes all bytes of signatureToFind are non zero, looks backwards from current position in stream,
    // if the signature is found then returns true and positions stream at first byte of signature
    // if the signature is not found, returns false
    internal static bool SeekBackwardsToSignature(Stream stream, uint signatureToFind, ZipReusableBundle bundle)
    {
        int bufferPointer = 0;
        uint currentSignature = 0;
        bundle.BackwardsSeekingBuffer.Clear();

        bool outOfBytes = false;
        bool signatureFound = false;

        while (!signatureFound && !outOfBytes)
        {
            outOfBytes = SeekBackwardsAndRead(stream, bundle.BackwardsSeekingBuffer, out bufferPointer);

            Debug.Assert(bufferPointer < bundle.BackwardsSeekingBuffer.Length);

            while (bufferPointer >= 0 && !signatureFound)
            {
                currentSignature = (currentSignature << 8) | bundle.BackwardsSeekingBuffer[bufferPointer];
                if (currentSignature == signatureToFind)
                {
                    signatureFound = true;
                }
                else
                {
                    bufferPointer--;
                }
            }
        }

        if (!signatureFound)
        {
            return false;
        }
        else
        {
            stream.Seek(bufferPointer, SeekOrigin.Current);
            return true;
        }
    }

    // Skip to a further position downstream (without relying on the stream being seekable)
    internal static void AdvanceToPosition(this Stream stream, long position, ZipReusableBundle bundle)
    {
        long numBytesLeft = position - stream.Position;
        Debug.Assert(numBytesLeft >= 0);
        while (numBytesLeft != 0)
        {
            bundle.ThrowawayBuffer.Clear();
            int numBytesToSkip = numBytesLeft > ZipReusableBundle.ThrowAwayBufferSize ? ZipReusableBundle.ThrowAwayBufferSize : (int)numBytesLeft;
            int numBytesActuallySkipped = stream.Read(bundle.ThrowawayBuffer, 0, numBytesToSkip);
            if (numBytesActuallySkipped == 0) ThrowHelper.IOException(SR.UnexpectedEndOfStream);
            numBytesLeft -= numBytesActuallySkipped;
        }
    }

    // Returns true if we are out of bytes
    private static bool SeekBackwardsAndRead(Stream stream, byte[] buffer, out int bufferPointer)
    {
        if (stream.Position >= buffer.Length)
        {
            stream.Seek(-buffer.Length, SeekOrigin.Current);
            ReadBytes(stream, buffer, buffer.Length);
            stream.Seek(-buffer.Length, SeekOrigin.Current);
            bufferPointer = buffer.Length - 1;
            return false;
        }
        else
        {
            int bytesToRead = (int)stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            ReadBytes(stream, buffer, bytesToRead);
            stream.Seek(0, SeekOrigin.Begin);
            bufferPointer = bytesToRead - 1;
            return true;
        }
    }

    internal static unsafe int ReadInt32(byte[] value, int valueLength, int startIndex)
    {
        if ((long)(uint)startIndex >= (long)valueLength)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), "ArgumentOutOfRange_Index");
        }
        if (startIndex > valueLength - 4)
        {
            ThrowHelper.ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        fixed (byte* numPtr = &value[startIndex])
        {
            return startIndex % 4 == 0
                ? *(int*)numPtr
                : BitConverter.IsLittleEndian
                    ? (int)*numPtr | (int)numPtr[1] << 8 | (int)numPtr[2] << 16 | (int)numPtr[3] << 24
                    : (int)*numPtr << 24 | (int)numPtr[1] << 16 | (int)numPtr[2] << 8 | (int)numPtr[3];
        }
    }

    internal static unsafe long ReadInt64(byte[] value, int valueLength, int startIndex)
    {
        if ((long)(uint)startIndex >= (long)valueLength)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), "ArgumentOutOfRange_Index");
        }
        if (startIndex > valueLength - 8)
        {
            ThrowHelper.ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        fixed (byte* numPtr = &value[startIndex])
        {
            if (startIndex % 8 == 0) return *(long*)numPtr;

            if (BitConverter.IsLittleEndian)
            {
                return (long)(uint)((int)*numPtr | (int)numPtr[1] << 8 | (int)numPtr[2] << 16 |
                                    (int)numPtr[3] << 24) | (long)((int)numPtr[4] | (int)numPtr[5] << 8 |
                                                                   (int)numPtr[6] << 16 |
                                                                   (int)numPtr[7] << 24) << 32;
            }

            int num = (int)*numPtr << 24 | (int)numPtr[1] << 16 | (int)numPtr[2] << 8 | (int)numPtr[3];
            return (long)((uint)((int)numPtr[4] << 24 | (int)numPtr[5] << 16 | (int)numPtr[6] << 8) |
                          (uint)numPtr[7]) | (long)num << 32;
        }
    }
}
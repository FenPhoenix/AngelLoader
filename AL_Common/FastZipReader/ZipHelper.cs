// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using static AL_Common.Common;

namespace AL_Common.FastZipReader;

public sealed class ZipCompressionMethodException(string message) : Exception(message);

// We should try to just make the zip archive classes be like the scanner, where it's one object that just
// has like a Reset(stream) method that loads another stream and resets all its values. That'd be much nicer.
public sealed class ZipContext : IDisposable
{
    internal readonly ListFast<ZipArchiveFastEntry> Entries = new(0);

    internal readonly SubReadStream ArchiveSubReadStream = new();

    public readonly byte[] FileStreamBuffer = new byte[4096];

    internal readonly byte[] DataBuffer = new byte[65536];
    internal readonly byte[] FilenameBuffer = new byte[65536];

    private const int _backwardsSeekingBufferSize = 32;
    internal const int ThrowAwayBufferSize = 64;

    internal readonly byte[] BackwardsSeekingBuffer = new byte[_backwardsSeekingBufferSize];
    internal readonly byte[] ThrowawayBuffer = new byte[ThrowAwayBufferSize];

    internal readonly BinaryBuffer BinaryReadBuffer = new();

    public void Dispose() => ArchiveSubReadStream.Dispose();
}

public static class ZipHelpers
{
    private const int ValidZipDate_YearMin = 1980;

    private static readonly DateTime _invalidDateIndicator = new(ValidZipDate_YearMin, 1, 1, 0, 0, 0);

    /// <summary>
    /// Converts a Zip timestamp to a DateTime object. If <paramref name="zipDateTime"/> is not a
    /// valid Zip timestamp, an indicator value of 1980 January 1 at midnight will be returned.
    /// </summary>
    /// <param name="zipDateTime"></param>
    /// <returns></returns>
    public static DateTime ZipTimeToDateTime(uint zipDateTime)
    {
        if (zipDateTime == 0)
        {
            return _invalidDateIndicator;
        }

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
            return _invalidDateIndicator;
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
            int bytesRead = stream.Read(buffer, totalBytesRead, bytesLeftToRead);
            if (bytesRead == 0) ThrowHelper.IOException(SR.UnexpectedEndOfStream);

            totalBytesRead += bytesRead;
            bytesLeftToRead -= bytesRead;
        }
    }

    // assumes all bytes of signatureToFind are non zero, looks backwards from current position in stream,
    // if the signature is found then returns true and positions stream at first byte of signature
    // if the signature is not found, returns false
    internal static bool SeekBackwardsToSignature(Stream stream, uint signatureToFind, ZipContext context)
    {
        int bufferPointer = 0;
        uint currentSignature = 0;
        context.BackwardsSeekingBuffer.Clear();

        bool outOfBytes = false;
        bool signatureFound = false;

        while (!signatureFound && !outOfBytes)
        {
            outOfBytes = SeekBackwardsAndRead(stream, context.BackwardsSeekingBuffer, out bufferPointer);

            Debug.Assert(bufferPointer < context.BackwardsSeekingBuffer.Length);

            while (bufferPointer >= 0 && !signatureFound)
            {
                currentSignature = (currentSignature << 8) | context.BackwardsSeekingBuffer[bufferPointer];
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
    internal static void AdvanceToPosition(this Stream stream, long position, ZipContext context)
    {
        long numBytesLeft = position - stream.Position;
        Debug.Assert(numBytesLeft >= 0);
        while (numBytesLeft != 0)
        {
            int numBytesToSkip = numBytesLeft > ZipContext.ThrowAwayBufferSize ? ZipContext.ThrowAwayBufferSize : (int)numBytesLeft;
            int numBytesActuallySkipped = stream.Read(context.ThrowawayBuffer, 0, numBytesToSkip);
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

    // These come from BitConverter.ToInt32/64 methods (.NET 8 version)
    internal static uint ReadUInt32(byte[] value, int valueLength, int startIndex)
    {
        if (unchecked((uint)startIndex) >= unchecked((uint)valueLength))
        {
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), "ArgumentOutOfRange_Index");
        }
        if (startIndex > valueLength - sizeof(int))
        {
            ThrowHelper.ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        return unchecked((uint)Unsafe.ReadUnaligned<int>(ref value[startIndex]));
    }

    internal static long ReadInt64(byte[] value, int valueLength, int startIndex)
    {
        if (unchecked((uint)startIndex) >= unchecked((uint)valueLength))
        {
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), "ArgumentOutOfRange_Index");
        }
        if (startIndex > valueLength - sizeof(long))
        {
            ThrowHelper.ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        return Unsafe.ReadUnaligned<long>(ref value[startIndex]);
    }
}

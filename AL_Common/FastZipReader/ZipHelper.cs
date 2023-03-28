// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;

namespace AL_Common.FastZipReader;

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
    internal static void EndOfFile() => throw new EndOfStreamException(SR.EOF_ReadBeyondEOF);
}

// We should try to just make the zip archive classes be like the scanner, where it's one object that just
// has like a Reset(stream) method that loads another stream and resets all its values. That'd be much nicer.
public sealed class ZipReusableBundle : IDisposable
{
    internal readonly SubReadStream ArchiveSubReadStream = new();

    private const int _backwardsSeekingBufferSize = 32;
    internal const int ThrowAwayBufferSize = 64;

    internal readonly byte[] BackwardsSeekingBuffer = new byte[_backwardsSeekingBufferSize];
    internal readonly byte[] ThrowawayBuffer = new byte[ThrowAwayBufferSize];

    public readonly byte[] BinaryReadBuffer = new byte[16];

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
        int month = (int)(zipDateTime >> 21 & 0xF);
        int day = (int)(zipDateTime >> 16 & 0x1F);
        int hour = (int)(zipDateTime >> 11 & 0x1F);
        int minute = (int)(zipDateTime >> 5 & 0x3F);
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
            int bytesRead = stream.Read(buffer, totalBytesRead, bytesLeftToRead);
            if (bytesRead == 0) throw new IOException(SR.UnexpectedEndOfStream);

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
                currentSignature = currentSignature << 8 | bundle.BackwardsSeekingBuffer[bufferPointer];
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
            if (numBytesActuallySkipped == 0) throw new IOException(SR.UnexpectedEndOfStream);
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
}

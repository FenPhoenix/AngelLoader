// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;

namespace FMScanner.FastZipReader
{
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
    }

    internal static class ZipHelper
    {
        internal const uint Mask32Bit = 0xFFFFFFFF;
        internal const ushort Mask16Bit = 0xFFFF;

        private const int _backwardsSeekingBufferSize = 32;
        private const int throwAwayBufferSize = 64;

        // Don't recreate constantly
        // Statics for ergonomics of calling - they're both tiny so who cares if they stay around forever
        private static readonly byte[] _backwardsSeekingBuffer = new byte[_backwardsSeekingBufferSize];
        private static readonly byte[] _throwawayBuffer = new byte[throwAwayBufferSize];

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
        internal static bool SeekBackwardsToSignature(Stream stream, uint signatureToFind)
        {
            int bufferPointer = 0;
            uint currentSignature = 0;
            Array.Clear(_backwardsSeekingBuffer, 0, _backwardsSeekingBuffer.Length);

            bool outOfBytes = false;
            bool signatureFound = false;

            while (!signatureFound && !outOfBytes)
            {
                outOfBytes = SeekBackwardsAndRead(stream, _backwardsSeekingBuffer, out bufferPointer);

                Debug.Assert(bufferPointer < _backwardsSeekingBuffer.Length);

                while (bufferPointer >= 0 && !signatureFound)
                {
                    currentSignature = (currentSignature << 8) | _backwardsSeekingBuffer[bufferPointer];
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
        internal static void AdvanceToPosition(this Stream stream, long position)
        {
            long numBytesLeft = position - stream.Position;
            Debug.Assert(numBytesLeft >= 0);
            while (numBytesLeft != 0)
            {
                Array.Clear(_throwawayBuffer, 0, _throwawayBuffer.Length);
                int numBytesToSkip = numBytesLeft > throwAwayBufferSize ? throwAwayBufferSize : (int)numBytesLeft;
                int numBytesActuallySkipped = stream.Read(_throwawayBuffer, 0, numBytesToSkip);
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
}

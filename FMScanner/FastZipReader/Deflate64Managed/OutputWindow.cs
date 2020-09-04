// Fen's note: This is straight out of .NET Core 3 with no functional changes. I'm trusting it to be correct and
// working, and I'm not gonna touch it even for nullability.

#nullable disable

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace FMScanner.FastZipReader.Deflate64Managed
{
    /// <summary>
    /// This class maintains a window for decompressed output.
    /// We need to keep this because the decompressed information can be
    /// a literal or a length/distance pair. For length/distance pair,
    /// we need to look back in the output window and copy bytes from there.
    /// We use a byte array of WindowSize circularly.
    /// </summary>
    internal sealed class OutputWindow
    {
        // With Deflate64 we can have up to a 65536 length as well as up to a 65538 distance. This means we need a Window that is at
        // least 131074 bytes long so we have space to retrieve up to a full 64kb in lookback and place it in our buffer without 
        // overwriting existing data. OutputWindow requires that the WindowSize be an exponent of 2, so we round up to 2^18.
        private const int WindowSize = 262144;
        private const int WindowMask = 262143;

        private readonly byte[] _window = new byte[WindowSize]; // The window is 2^18 bytes
        private int _end;       // this is the position to where we should write next byte

        /// <summary>Add a byte to output window.</summary>
        internal void Write(byte b)
        {
            Debug.Assert(AvailableBytes < WindowSize, "Can't add byte when window is full!");
            _window[_end++] = b;
            _end &= WindowMask;
            ++AvailableBytes;
        }

        internal void WriteLengthDistance(int length, int distance)
        {
            Debug.Assert(AvailableBytes + length <= WindowSize, "No Enough space");

            // move backwards distance bytes in the output stream,
            // and copy length bytes from this position to the output stream.
            AvailableBytes += length;
            int copyStart = (_end - distance) & WindowMask; // start position for coping.

            int border = WindowSize - length;
            if (copyStart <= border && _end < border)
            {
                if (length <= distance)
                {
                    Array.Copy(_window, copyStart, _window, _end, length);
                    _end += length;
                }
                else
                {
                    // The referenced string may overlap the current
                    // position; for example, if the last 2 bytes decoded have values
                    // X and Y, a string reference with <length = 5, distance = 2>
                    // adds X,Y,X,Y,X to the output stream.
                    while (length-- > 0)
                    {
                        _window[_end++] = _window[copyStart++];
                    }
                }
            }
            else
            {
                // copy byte by byte
                while (length-- > 0)
                {
                    _window[_end++] = _window[copyStart++];
                    _end &= WindowMask;
                    copyStart &= WindowMask;
                }
            }
        }

        /// <summary>
        /// Copy up to length of bytes from input directly.
        /// This is used for uncompressed block.
        /// </summary>
        internal int CopyFrom(InputBuffer input, int length)
        {
            length = Math.Min(Math.Min(length, WindowSize - AvailableBytes), input.AvailableBytes);
            int copied;

            // We might need wrap around to copy all bytes.
            int tailLen = WindowSize - _end;
            if (length > tailLen)
            {
                // copy the first part
                copied = input.CopyTo(_window, _end, tailLen);
                if (copied == tailLen)
                {
                    // only try to copy the second part if we have enough bytes in input
                    copied += input.CopyTo(_window, 0, length - tailLen);
                }
            }
            else
            {
                // only one copy is needed if there is no wrap around.
                copied = input.CopyTo(_window, _end, length);
            }

            _end = (_end + copied) & WindowMask;
            AvailableBytes += copied;
            return copied;
        }

        /// <summary>Free space in output window.</summary>
        internal int FreeBytes => WindowSize - AvailableBytes;

        /// <summary>Bytes not consumed in output window.</summary>
        internal int AvailableBytes;

        /// <summary>Copy the decompressed bytes to output array.</summary>
        internal int CopyTo(byte[] output, int offset, int length)
        {
            int copyEnd;

            if (length > AvailableBytes)
            {
                // we can copy all the decompressed bytes out
                copyEnd = _end;
                length = AvailableBytes;
            }
            else
            {
                copyEnd = (_end - AvailableBytes + length) & WindowMask; // copy length of bytes
            }

            int copied = length;

            int tailLen = length - copyEnd;
            if (tailLen > 0)
            {
                // this means we need to copy two parts separately
                // copy tailLen bytes from the end of output window
                Array.Copy(_window, WindowSize - tailLen,
                                  output, offset, tailLen);
                offset += tailLen;
                length = copyEnd;
            }
            Array.Copy(_window, copyEnd - length, output, offset, length);
            AvailableBytes -= copied;
            Debug.Assert(AvailableBytes >= 0, "check this function and find why we copied more bytes than we have");
            return copied;
        }
    }
}

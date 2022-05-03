using System;
using System.IO;
using System.Runtime.InteropServices;
using AL_Common;

namespace FMScanner.FastZipReader
{
    public sealed class BinaryReader_Custom : IDisposable
    {
        private static readonly int _encodingMaxByteCount = ZipArchiveFast.UTF8EncodingNoBOM.GetMaxByteCount(1).ClampToMin(16);
        private static readonly byte[] _buffer = new byte[_encodingMaxByteCount];
        private readonly bool _isMemoryStream;

        public readonly Stream BaseStream;

        public BinaryReader_Custom(Stream input)
        {
            BaseStream = input;
            _isMemoryStream = BaseStream is MemoryStream_Custom;
        }

        /// <summary>Reads the next byte from the current stream and advances the current position of the stream by one byte.</summary>
        /// <returns>The next byte read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        public byte ReadByte()
        {
            int num = BaseStream.ReadByte();
            if (num == -1) __Error.EndOfFile();
            return (byte)num;
        }

        /// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        public ushort ReadUInt16()
        {
            this.FillBuffer(2);
            return (ushort)((uint)_buffer[0] | (uint)_buffer[1] << 8);
        }

        /// <summary>Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.</summary>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        public int ReadInt32()
        {
            if (_isMemoryStream)
            {
                return ((MemoryStream_Custom)BaseStream).InternalReadInt32();
            }
            FillBuffer(4);
            return (int)_buffer[0] | (int)_buffer[1] << 8 | (int)_buffer[2] << 16 | (int)_buffer[3] << 24;
        }

        /// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
        /// <returns>A 4-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        public uint ReadUInt32()
        {
            this.FillBuffer(4);
            return (uint)((int)_buffer[0] | (int)_buffer[1] << 8 | (int)_buffer[2] << 16 | (int)_buffer[3] << 24);
        }

        /// <summary>Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte signed integer read from the current stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        public long ReadInt64()
        {
            this.FillBuffer(8);
            return (long)(uint)((int)_buffer[4] | (int)_buffer[5] << 8 | (int)_buffer[6] << 16 | (int)_buffer[7] << 24) << 32 | (long)(uint)((int)_buffer[0] | (int)_buffer[1] << 8 | (int)_buffer[2] << 16 | (int)_buffer[3] << 24);
        }

        /// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
        /// <returns>An 8-byte unsigned integer read from this stream.</returns>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        public ulong ReadUInt64()
        {
            this.FillBuffer(8);
            return (ulong)(uint)((int)_buffer[4] | (int)_buffer[5] << 8 | (int)_buffer[6] << 16 | (int)_buffer[7] << 24) << 32 | (ulong)(uint)((int)_buffer[0] | (int)_buffer[1] << 8 | (int)_buffer[2] << 16 | (int)_buffer[3] << 24);
        }

        /// <summary>Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.</summary>
        /// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
        /// <exception cref="T:System.ArgumentException">The number of decoded characters to read is greater than <paramref name="count" />. This can happen if a Unicode decoder returns fallback characters or a surrogate pair.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="count" /> is negative.</exception>
        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Environment.GetResourceString(\"ArgumentOutOfRange_NeedNonNegNum\")");
            }
            if (count == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] numArray = new byte[count];
            int length = 0;
            do
            {
                int num = BaseStream.Read(numArray, length, count);
                if (num != 0)
                {
                    length += num;
                    count -= num;
                }
                else
                    break;
            }
            while (count > 0);
            if (length != numArray.Length)
            {
                byte[] dst = new byte[length];
                Buffer.BlockCopy((Array)numArray, 0, (Array)dst, 0, length);
                numArray = dst;
            }
            return numArray;
        }

        /// <summary>Fills the internal buffer with the specified number of bytes read from the stream.</summary>
        /// <param name="numBytes">The number of bytes to be read.</param>
        /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached before <paramref name="numBytes" /> could be read.</exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Requested <paramref name="numBytes" /> is larger than the internal buffer size.</exception>
        private void FillBuffer(int numBytes)
        {
            if (numBytes < 0 || numBytes > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(numBytes), "Environment.GetResourceString(\"ArgumentOutOfRange_BinaryReaderFillBuffer\")");
            }

            int offset = 0;

            if (numBytes == 1)
            {
                int num = BaseStream.ReadByte();
                if (num == -1) __Error.EndOfFile();
                _buffer[0] = (byte)num;
            }
            else
            {
                do
                {
                    int num = BaseStream.Read(_buffer, offset, numBytes - offset);
                    if (num == 0) __Error.EndOfFile();
                    offset += num;
                }
                while (offset < numBytes);
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                BaseStream.Dispose();
            }
        }

        public void Dispose() => Dispose(true);
    }

    internal static class __Error
    {
        internal static void EndOfFile() => throw new EndOfStreamException("Environment.GetResourceString(\"IO.EOF_ReadBeyondEOF\")");

        internal static void FileNotOpen() => throw new ObjectDisposedException((string)null, "Environment.GetResourceString(\"ObjectDisposed_FileClosed\")");
    }
}

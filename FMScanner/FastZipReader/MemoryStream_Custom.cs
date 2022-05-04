using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FMScanner.FastZipReader
{
    /// <summary>Creates a stream whose backing store is memory.</summary>
    [ComVisible(true)]
    [Serializable]
    public class MemoryStream_Custom : Stream
    {
        private readonly byte[] _buffer;
        private readonly int _origin;
        private int _position;
        private readonly int _length;

        /// <summary>Initializes a new non-resizable instance of the <see cref="T:System.IO.MemoryStream"/> class based on the specified byte array.</summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        public MemoryStream_Custom(byte[] buffer)
        {
            _buffer = buffer;
            _length = buffer.Length;
            _origin = 0;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        internal int InternalReadInt32()
        {
            int num = _position += 4;
            if (num > _length)
            {
                _position = _length;
                __Error.EndOfFile();
            }
            return (int)_buffer[num - 4] | (int)_buffer[num - 3] << 8 | (int)_buffer[num - 2] << 16 | (int)_buffer[num - 1] << 24;
        }

        public override long Length => _length - _origin;

        /// <summary>Reads a block of bytes from the current stream and writes the data to a buffer.</summary>
        /// <param name="buffer">When this method returns, contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the characters read from the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing data from the current stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The total number of bytes written into the buffer. This can be less than the number of bytes requested if that number of bytes are not currently available, or zero if the end of the stream is reached before any bytes are read.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="offset" /> subtracted from the buffer length is less than <paramref name="count" />.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The current stream instance is closed.</exception>
        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Environment.GetResourceString(\"ArgumentOutOfRange_NeedNonNegNum\")");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Environment.GetResourceString(\"ArgumentOutOfRange_NeedNonNegNum\")");
            if (buffer.Length - offset < count)
                throw new ArgumentException("Environment.GetResourceString(\"Argument_InvalidOffLen\")");
            int byteCount = _length - _position;
            if (byteCount > count)
                byteCount = count;
            if (byteCount <= 0)
                return 0;
            if (byteCount <= 8)
            {
                int num = byteCount;
                while (--num >= 0)
                    buffer[offset + num] = _buffer[_position + num];
            }
            else
            {
                Buffer.BlockCopy(_buffer, _position, buffer, offset, byteCount);
            }
            _position += byteCount;
            return byteCount;
        }

        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Flush() => throw new NotImplementedException();

        public override Task FlushAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin loc) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
    }
}

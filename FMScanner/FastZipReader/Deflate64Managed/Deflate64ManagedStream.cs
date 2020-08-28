// Fen's note: This is straight out of .NET Core 3 with no functional changes. I'm trusting it to be correct and
// working, and I'm not gonna touch it even for nullability.

#nullable disable

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FMScanner.FastZipReader.Deflate64Managed
{
    internal sealed class Deflate64ManagedStream : Stream
    {
        private const int DefaultBufferSize = 8192;

        private Stream _stream;
        private bool _leaveOpen;
        private Inflater64Managed _inflater64;
        private byte[] _buffer;

        // A specific constructor to allow decompression of Deflate64
        internal Deflate64ManagedStream(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (!stream.CanRead)
            {
                throw new ArgumentException(SR.NotSupported_UnreadableStream, nameof(stream));
            }

            InitializeInflater(stream);
        }

        /// <summary>
        /// Sets up this DeflateManagedStream to be used for Inflation/Decompression
        /// </summary>
        private void InitializeInflater(Stream stream)
        {
            Debug.Assert(stream != null);
            if (!stream.CanRead) throw new ArgumentException(SR.NotSupported_UnreadableStream, nameof(stream));

            _inflater64 = new Inflater64Managed(reader: null);

            _stream = stream;
            _leaveOpen = false;
            _buffer = new byte[DefaultBufferSize];
        }

        public override bool CanRead => _stream != null && _stream.CanRead;

        public override bool CanWrite => false;

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException(SR.NotSupported);

        public override long Position
        {
            get => throw new NotSupportedException(SR.NotSupported);
            set => throw new NotSupportedException(SR.NotSupported);
        }

        public override void Flush()
        {
            ThrowIfDisposed();
            throw new NotSupportedException(SR.WritingNotSupported);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            throw new NotSupportedException(SR.WritingNotSupported);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.NotSupported);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.NotSupported);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(SR.WritingNotSupported);
        }

        public override int Read(byte[] array, int offset, int count)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));

            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            if (array.Length - offset < count) throw new ArgumentException(SR.InvalidArgumentOffsetCount);

            ThrowIfDisposed();
            int currentOffset = offset;
            int remainingCount = count;

            while (true)
            {
                int bytesRead = _inflater64.Inflate(array, currentOffset, remainingCount);
                currentOffset += bytesRead;
                remainingCount -= bytesRead;

                if (remainingCount == 0)
                {
                    break;
                }

                if (_inflater64.Finished())
                {
                    // if we finished decompressing, we can't have anything left in the outputwindow.
                    Debug.Assert(_inflater64.AvailableOutput == 0, "We should have copied all stuff out!");
                    break;
                }

                int bytes = _stream.Read(_buffer, 0, _buffer.Length);
                if (bytes <= 0)
                {
                    break;
                }
                else if (bytes > _buffer.Length)
                {
                    // The stream is either malicious or poorly implemented and returned a number of
                    // bytes larger than the buffer supplied to it.
                    throw new InvalidDataException(SR.GenericInvalidData);
                }

                _inflater64.SetInput(_buffer, 0, bytes);
            }

            return count - remainingCount;
        }

        private void ThrowIfDisposed()
        {
            if (_stream == null) throw new ObjectDisposedException(null, SR.ObjectDisposed_StreamClosed);
        }

        protected override void Dispose(bool disposing)
        {
            // Close the underlying stream even if PurgeBuffers threw.
            // Stream.Close() may throw here (may or may not be due to the same error).
            // In this case, we still need to clean up internal resources, hence the inner finally blocks.
            try
            {
                if (disposing && !_leaveOpen) _stream?.Dispose();
            }
            finally
            {
                _stream = null;

                try
                {
                    _inflater64?.Dispose();
                }
                finally
                {
                    _inflater64 = null;
                    base.Dispose(disposing);
                }
            }
        }
    }
}

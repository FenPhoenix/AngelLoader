// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using AL_Common.NETM_IO.Strategies;

namespace AL_Common.NETM_IO
{
    public class FileStream_NET : Stream
    {
        private readonly BufferedFileStreamStrategy _strategy;

        private static void ValidateHandle(AL_SafeFileHandle handle, FileAccess access)
        {
            if (handle.IsInvalid)
            {
                throw new ArgumentException(SR.Arg_InvalidHandle, nameof(handle));
            }
            else if (access < FileAccess.Read || access > FileAccess.ReadWrite)
            {
                throw new ArgumentOutOfRangeException(nameof(access), SR.ArgumentOutOfRange_Enum);
            }
            else if (handle.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }
        }

        public FileStream_NET(AL_SafeFileHandle handle, FileAccess access, byte[] buffer)
        {
            ValidateHandle(handle, access);

            _strategy = FileStreamHelpers.ChooseStrategy(handle, access, buffer);
        }

        public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, byte[] buffer)
            : this(path, mode, access, share, buffer, FileOptions.None)
        {
        }

        public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, byte[] buffer, FileOptions options)
            : this(path, mode, access, share, buffer, options, 0)
        {
        }

        ~FileStream_NET()
        {
            // Preserved for compatibility since FileStream has defined a
            // finalizer in past releases and derived classes may depend
            // on Dispose(false) call.
            Dispose(false);
        }

        private FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, byte[] buffer, FileOptions options, long preallocationSize)
        {
            FileStreamHelpers.ValidateArguments(path, mode, access, share, options, preallocationSize);

            _strategy = FileStreamHelpers.ChooseStrategy(path, mode, access, share, buffer, options, preallocationSize);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ValidateReadWriteArgs(buffer, offset, count);

            return _strategy.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ValidateReadWriteArgs(buffer, offset, count);

            _strategy.Write(buffer, offset, count);
        }

        /// <summary>
        /// Clears buffers for this stream and causes any buffered data to be written to the file.
        /// </summary>
        public override void Flush()
        {
            // Make sure that we call through the public virtual API
            Flush(flushToDisk: false);
        }

        /// <summary>
        /// Clears buffers for this stream, and if <param name="flushToDisk"/> is true,
        /// causes any buffered data to be written to the file.
        /// </summary>
        public virtual void Flush(bool flushToDisk)
        {
            if (_strategy.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }

            _strategy.Flush(flushToDisk);
        }

        /// <summary>Gets a value indicating whether the current stream supports reading.</summary>
        public override bool CanRead => _strategy.CanRead;

        /// <summary>Gets a value indicating whether the current stream supports writing.</summary>
        public override bool CanWrite => _strategy.CanWrite;

        /// <summary>Validates arguments to Read and Write and throws resulting exceptions.</summary>
        /// <param name="buffer">The buffer to read from or write to.</param>
        /// <param name="offset">The zero-based offset into the buffer.</param>
        /// <param name="count">The maximum number of bytes to read or write.</param>
        private void ValidateReadWriteArgs(byte[] buffer, int offset, int count)
        {
            BufferedFileStreamStrategy.ValidateBufferArguments(buffer, offset, count);
            if (_strategy.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }
        }

        /// <summary>Sets the length of this stream to the given value.</summary>
        /// <param name="value">The new length of the stream.</param>
        public override void SetLength(long value)
        {
            if (value < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            else if (_strategy.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }
            else if (!CanSeek)
            {
                ThrowHelper.ThrowNotSupportedException_UnseekableStream();
            }
            else if (!CanWrite)
            {
                ThrowHelper.ThrowNotSupportedException_UnwritableStream();
            }

            _strategy.SetLength(value);
        }

        public virtual AL_SafeFileHandle SafeFileHandle => _strategy.SafeFileHandle;

        /// <summary>Gets the path that was passed to the constructor.</summary>
        public virtual string Name => _strategy.Name;

        /// <summary>Gets the length of the stream in bytes.</summary>
        public override long Length
        {
            get
            {
                if (_strategy.IsClosed)
                {
                    ThrowHelper.ThrowObjectDisposedException_FileClosed();
                }
                else if (!CanSeek)
                {
                    ThrowHelper.ThrowNotSupportedException_UnseekableStream();
                }

                return _strategy.Length;
            }
        }

        /// <summary>Gets or sets the position within the current stream</summary>
        public override long Position
        {
            get
            {
                if (_strategy.IsClosed)
                {
                    ThrowHelper.ThrowObjectDisposedException_FileClosed();
                }
                else if (!CanSeek)
                {
                    ThrowHelper.ThrowNotSupportedException_UnseekableStream();
                }

                return _strategy.Position;
            }
            set
            {
                if (value < 0)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), SR.ArgumentOutOfRange_NeedNonNegNum);
                }
                else if (!CanSeek)
                {
                    if (_strategy.IsClosed)
                    {
                        ThrowHelper.ThrowObjectDisposedException_FileClosed();
                    }

                    ThrowHelper.ThrowNotSupportedException_UnseekableStream();
                }

                _strategy.Position = value;
            }
        }

        /// <summary>
        /// Reads a byte from the file stream.  Returns the byte cast to an int
        /// or -1 if reading from the end of the stream.
        /// </summary>
        public override int ReadByte() => _strategy.ReadByte();

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position
        /// within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        public override void WriteByte(byte value) => _strategy.WriteByte(value);

        // _strategy can be null only when ctor has thrown
        protected override void Dispose(bool disposing) => _strategy.Dispose(disposing);

        // @FileStreamNET: This can't be overridden in Framework... Either it has to be removed, or new'd in which
        //  case it wouldn't be hit if the thing was passed as a less-derived type. Ugh.
        //public override void CopyTo(Stream destination, int bufferSize)
        //{
        //    ValidateCopyToArguments(destination, bufferSize);
        //    _strategy.CopyTo(destination, bufferSize);
        //}

#if false
        /// <summary>Validates arguments provided to the <see cref="CopyTo(Stream, int)"/> or <see cref="CopyToAsync(Stream, int, CancellationToken)"/> methods.</summary>
        /// <param name="destination">The <see cref="Stream"/> "destination" argument passed to the copy method.</param>
        /// <param name="bufferSize">The integer "bufferSize" argument passed to the copy method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> was null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> was not a positive value.</exception>
        /// <exception cref="NotSupportedException"><paramref name="destination"/> does not support writing.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="destination"/> does not support writing or reading.</exception>
        protected static void ValidateCopyToArguments(Stream destination, int bufferSize)
        {
            ArgumentNullException_NET.ThrowIfNull(destination);

            ThrowIfNegativeOrZero(bufferSize);

            if (!destination.CanWrite)
            {
                if (destination.CanRead)
                {
                    ThrowHelper.ThrowNotSupportedException_UnwritableStream();
                }

                ThrowHelper.ThrowObjectDisposedException_StreamClosed(destination.GetType().Name);
            }
        }
#endif

        public override bool CanSeek => _strategy.CanSeek;

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin < SeekOrigin.Begin || origin > SeekOrigin.End)
            {
                throw new ArgumentException(SR.Argument_InvalidSeekOrigin, nameof(origin));
            }
            else if (!CanSeek)
            {
                if (_strategy.IsClosed)
                {
                    ThrowHelper.ThrowObjectDisposedException_FileClosed();
                }

                ThrowHelper.ThrowNotSupportedException_UnseekableStream();
            }

            return _strategy.Seek(offset, origin);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.</summary>
        /// <param name="value">The argument to validate as non-zero or non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfNegativeOrZero(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value <= 0)
            {
                ThrowHelper.ThrowNegativeOrZero(value, paramName);
            }
        }
    }
}

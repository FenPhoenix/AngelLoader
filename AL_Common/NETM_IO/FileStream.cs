// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AL_Common.NETM_IO.Strategies;

namespace AL_Common.NETM_IO
{
    public class FileStream_NET : Stream
    {
        internal const int DefaultBufferSize = 4096;
        internal const FileShare DefaultShare = FileShare.Read;
        private const bool DefaultIsAsync = false;

        private readonly FileStreamStrategy _strategy;

        private static void ValidateHandle(AL_SafeFileHandle handle, FileAccess access, int bufferSize)
        {
            if (handle.IsInvalid)
            {
                throw new ArgumentException(SR.Arg_InvalidHandle, nameof(handle));
            }
            else if (access < FileAccess.Read || access > FileAccess.ReadWrite)
            {
                throw new ArgumentOutOfRangeException(nameof(access), SR.ArgumentOutOfRange_Enum);
            }
            else if (bufferSize < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(nameof(bufferSize));
            }
            else if (handle.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }
        }

        private static void ValidateHandle(AL_SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
        {
            ValidateHandle(handle, access, bufferSize);

            if (isAsync && !handle.IsAsync)
            {
                ThrowHelper.ThrowArgumentException_HandleNotAsync(nameof(handle));
            }
            else if (!isAsync && handle.IsAsync)
            {
                ThrowHelper.ThrowArgumentException_HandleNotSync(nameof(handle));
            }
        }

        public FileStream_NET(AL_SafeFileHandle handle, FileAccess access)
            : this(handle, access, DefaultBufferSize)
        {
        }

        public FileStream_NET(AL_SafeFileHandle handle, FileAccess access, int bufferSize)
        {
            ValidateHandle(handle, access, bufferSize);

            _strategy = FileStreamHelpers.ChooseStrategy(this, handle, access, bufferSize, handle.IsAsync);
        }

        public FileStream_NET(AL_SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
        {
            ValidateHandle(handle, access, bufferSize, isAsync);

            _strategy = FileStreamHelpers.ChooseStrategy(this, handle, access, bufferSize, isAsync);
        }

        public FileStream_NET(string path, FileMode mode)
            : this(path, mode, mode == FileMode.Append ? FileAccess.Write : FileAccess.ReadWrite, DefaultShare, DefaultBufferSize, DefaultIsAsync)
        {
        }

        public FileStream_NET(string path, FileMode mode, FileAccess access)
            : this(path, mode, access, DefaultShare, DefaultBufferSize, DefaultIsAsync)
        {
        }

        public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share)
            : this(path, mode, access, share, DefaultBufferSize, DefaultIsAsync)
        {
        }

        public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
            : this(path, mode, access, share, bufferSize, DefaultIsAsync)
        {
        }

        public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
            : this(path, mode, access, share, bufferSize, useAsync ? FileOptions.Asynchronous : FileOptions.None)
        {
        }

        public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
            : this(path, mode, access, share, bufferSize, options, 0)
        {
        }

        ~FileStream_NET()
        {
            // Preserved for compatibility since FileStream has defined a
            // finalizer in past releases and derived classes may depend
            // on Dispose(false) call.
            Dispose(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStream_NET" /> class with the specified path, creation mode, read/write and sharing permission, the access other FileStreams can have to the same file, the buffer size,  additional file options and the allocation size.
        /// </summary>
        /// <param name="path">A relative or absolute path for the file that the current <see cref="FileStream_NET" /> instance will encapsulate.</param>
        /// <param name="options">An object that describes optional <see cref="FileStream_NET" /> parameters to use.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="path" /> or <paramref name="options" /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="path" /> is an empty string (""), contains only white space, or contains one or more invalid characters.
        /// -or-
        /// <paramref name="path" /> refers to a non-file device, such as <c>CON:</c>, <c>COM1:</c>, <c>LPT1:</c>, etc. in an NTFS environment.</exception>
        /// <exception cref="T:System.NotSupportedException"><paramref name="path" /> refers to a non-file device, such as <c>CON:</c>, <c>COM1:</c>, <c>LPT1:</c>, etc. in a non-NTFS environment.</exception>
        /// <exception cref="T:AL_Common.NETM_IO.FileNotFoundException">The file cannot be found, such as when <see cref="FileStreamOptions.Mode" /> is <see langword="FileMode.Truncate" /> or <see langword="FileMode.Open" />, and the file specified by <paramref name="path" /> does not exist. The file must already exist in these modes.</exception>
        /// <exception cref="T:AL_Common.NETM_IO.IOException">An I/O error, such as specifying <see langword="FileMode.CreateNew" /> when the file specified by <paramref name="path" /> already exists, occurred.
        ///  -or-
        ///  The stream has been closed.
        ///  -or-
        ///  The disk was full (when <see cref="FileStreamOptions.PreallocationSize" /> was provided and <paramref name="path" /> was pointing to a regular file).
        ///  -or-
        ///  The file was too large (when <see cref="FileStreamOptions.PreallocationSize" /> was provided and <paramref name="path" /> was pointing to a regular file).</exception>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="T:AL_Common.NETM_IO.DirectoryNotFoundException">The specified path is invalid, such as being on an unmapped drive.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The <see cref="FileStreamOptions.Access" /> requested is not permitted by the operating system for the specified <paramref name="path" />, such as when <see cref="FileStreamOptions.Access" />  is <see cref="FileAccess.Write" /> or <see cref="FileAccess.ReadWrite" /> and the file or directory is set for read-only access.
        ///  -or-
        /// <see cref="F:AL_Common.NETM_IO.FileOptions.Encrypted" /> is specified for <see cref="FileStreamOptions.Options" /> , but file encryption is not supported on the current platform.</exception>
        /// <exception cref="T:AL_Common.NETM_IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length. </exception>
        public FileStream_NET(string path, FileStreamOptions options)
        {
            ArgumentException_NET.ThrowIfNullOrEmpty(path);
            ArgumentNullException_NET.ThrowIfNull(options);
            if ((options.Access & FileAccess.Read) != 0 && options.Mode == FileMode.Append)
            {
                throw new ArgumentException(SR.Argument_InvalidAppendMode, nameof(options));
            }
            else if ((options.Access & FileAccess.Write) == 0)
            {
                if (options.Mode == FileMode.Truncate || options.Mode == FileMode.CreateNew || options.Mode == FileMode.Create || options.Mode == FileMode.Append)
                {
                    throw new ArgumentException(SR.Format(SR.Argument_InvalidFileModeAndAccessCombo, options.Mode, options.Access), nameof(options));
                }
            }

            if (options.PreallocationSize > 0)
            {
                FileStreamHelpers.ValidateArgumentsForPreallocation(options.Mode, options.Access);
            }

            FileStreamHelpers.SerializationGuard(options.Access);

            _strategy = FileStreamHelpers.ChooseStrategy(
                this, path, options.Mode, options.Access, options.Share, options.BufferSize, options.Options, options.PreallocationSize);
        }

        private FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, long preallocationSize)
        {
            FileStreamHelpers.ValidateArguments(path, mode, access, share, bufferSize, options, preallocationSize);

            _strategy = FileStreamHelpers.ChooseStrategy(this, path, mode, access, share, bufferSize, options, preallocationSize);
        }

        [Obsolete("FileStream.Handle has been deprecated. Use FileStream's AL_SafeFileHandle property instead.")]
        public virtual IntPtr Handle => _strategy.Handle;

        public virtual void Lock(long position, long length)
        {
            if (position < 0 || length < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(position < 0 ? nameof(position) : nameof(length));
            }
            else if (_strategy.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }

            _strategy.Lock(position, length);
        }

        public virtual void Unlock(long position, long length)
        {
            if (position < 0 || length < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(position < 0 ? nameof(position) : nameof(length));
            }
            else if (_strategy.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }

            _strategy.Unlock(position, length);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            else if (_strategy.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }

            return _strategy.FlushAsync(cancellationToken);
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
            ValidateBufferArguments(buffer, offset, count);
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
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument_NET.value, ExceptionResource_NET.ArgumentOutOfRange_NeedNonNegNum);
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

        public virtual AL_SafeFileHandle AL_SafeFileHandle => _strategy.AL_SafeFileHandle;

        /// <summary>Gets the path that was passed to the constructor.</summary>
        public virtual string Name => _strategy.Name;

        /// <summary>Gets a value indicating whether the stream was opened for I/O to be performed synchronously or asynchronously.</summary>
        public virtual bool IsAsync => _strategy.IsAsync;

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
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument_NET.value, ExceptionResource_NET.ArgumentOutOfRange_NeedNonNegNum);
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
        protected override void Dispose(bool disposing) => _strategy?.DisposeInternal(disposing);

        // @FileStreamNET: This can't be overridden in Framework... Either it has to be removed, or new'd in which
        //  case it wouldn't be hit if the thing was passed as a less-derived type. Ugh.
        //public override void CopyTo(Stream destination, int bufferSize)
        //{
        //    ValidateCopyToArguments(destination, bufferSize);
        //    _strategy.CopyTo(destination, bufferSize);
        //}

        // @FileStreamNET:
        //public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        //{
        //    ValidateCopyToArguments(destination, bufferSize);
        //    return _strategy.CopyToAsync(destination, bufferSize, cancellationToken);
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

        internal Task BaseFlushAsync(CancellationToken cancellationToken)
            => base.FlushAsync(cancellationToken);

        internal Task BaseCopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => base.CopyToAsync(destination, bufferSize, cancellationToken);

        /// <summary>Validates arguments provided to reading and writing methods on <see cref="Stream"/>.</summary>
        /// <param name="buffer">The array "buffer" argument passed to the reading or writing method.</param>
        /// <param name="offset">The integer "offset" argument passed to the reading or writing method.</param>
        /// <param name="count">The integer "count" argument passed to the reading or writing method.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> was null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="offset"/> was outside the bounds of <paramref name="buffer"/>, or
        /// <paramref name="count"/> was negative, or the range specified by the combination of
        /// <paramref name="offset"/> and <paramref name="count"/> exceed the length of <paramref name="buffer"/>.
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void ValidateBufferArguments(byte[] buffer, int offset, int count)
        {
            if (buffer is null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument_NET.buffer);
            }

            if (offset < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument_NET.offset, ExceptionResource_NET.ArgumentOutOfRange_NeedNonNegNum);
            }

            if ((uint)count > buffer.Length - offset)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument_NET.count, ExceptionResource_NET.Argument_InvalidOffLen);
            }
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

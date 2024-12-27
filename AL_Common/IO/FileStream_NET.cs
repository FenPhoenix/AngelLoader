// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace AL_Common;

public sealed class FileStream_NET : Stream
{
    private readonly AL_SafeFileHandle _fileHandle; // only ever null if ctor throws
    private readonly FileAccess _access; // What file was opened for.

    private long _filePosition;
    private readonly long _appendStart; // When appending, prevent overwriting file.

    private readonly int _bufferSize;

    private readonly byte[] _buffer;
    private int _writePos;
    private int _readPos;
    private int _readLen;

#if false
    public FileStream_NET(AL_SafeFileHandle handle, FileAccess access, byte[] buffer, int bufferSize)
    {
        ValidateHandle(handle, access);

        _bufferSize = bufferSize;
        _buffer = buffer;

        _access = access;

        if (handle.CanSeek)
        {
            // given strategy was created out of existing handle, so we have to perform
            // a syscall to get the current handle offset
            _filePosition = FileStreamHelpers.Seek(handle, 0, SeekOrigin.Current);
        }
        else
        {
            _filePosition = 0;
        }

        _fileHandle = handle;
    }
#endif

    public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, byte[] buffer, int bufferSize)
        : this(path, mode, access, share, buffer, bufferSize, FileOptions.None)
    {
    }

    public FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, byte[] buffer, int bufferSize, FileOptions options)
        : this(path, mode, access, share, buffer, bufferSize, options, 0)
    {
    }

    // Disable because we don't need it and we don't want it running when we're in a weird state and _fileHandle
    // could be null etc.
#if false
    ~FileStream_NET()
    {
        // Preserved for compatibility since FileStream has defined a
        // finalizer in past releases and derived classes may depend
        // on Dispose(false) call.
        Dispose(false);
    }
#endif

    private FileStream_NET(string path, FileMode mode, FileAccess access, FileShare share, byte[] buffer, int bufferSize, FileOptions options, long preallocationSize)
    {
        FileStreamHelpers.ValidateArguments(path, mode, access, share, options, preallocationSize);

        _bufferSize = bufferSize;
        _buffer = buffer;

        string fullPath = Path.GetFullPath(path);

        _access = access;

        try
        {
            /*
            IMPORTANT: This MUST go inside the try block as it can throw!
             The modern .NET version, through a complicated chain of logic, avoids this problem, but we don't.
             The finalizer will run and _fileHandle will be null and then we crash the whole app.
            */
            _fileHandle = AL_SafeFileHandle.Open(fullPath, mode, access, share, options, preallocationSize);
            if (mode == FileMode.Append && CanSeek)
            {
                _appendStart = _filePosition = GetLengthCore;
            }
            else
            {
                _appendStart = -1;
            }
        }
        catch
        {
            // If anything goes wrong while setting up the stream, make sure we deterministically dispose
            // of the opened handle.
            _fileHandle?.Dispose();
            // If we want to enable the ctor that takes a file handle, we need to re-enable this and then re-do
            // the logic to expect that this could null in the finalizer if we enable that too.
#if false
            _fileHandle = null;
#endif
            throw;
        }
    }

    public override bool CanSeek => _fileHandle.CanSeek;

    /// <summary>Gets a value indicating whether the current stream supports reading.</summary>
    public override bool CanRead => !_fileHandle.IsClosed && (_access & FileAccess.Read) != 0;

    /// <summary>Gets a value indicating whether the current stream supports writing.</summary>
    public override bool CanWrite => !_fileHandle.IsClosed && (_access & FileAccess.Write) != 0;

    private bool IsClosed => _fileHandle.IsClosed;

    public AL_SafeFileHandle SafeFileHandle
    {
        get
        {
            // BufferedFileStreamStrategy must flush before the handle is exposed
            // so whoever uses AL_SafeFileHandle to access disk data can see
            // the changes that were buffered in memory so far
            FlushCore(flushToDisk: false);

            if (CanSeek)
            {
                // Update the file offset before exposing it since it's possible that
                // in memory position is out-of-sync with the actual file position.
                FileStreamHelpers.Seek(_fileHandle, _filePosition, SeekOrigin.Begin);
            }

            return _fileHandle;
        }
    }

    /// <summary>Gets the path that was passed to the constructor.</summary>
    public string Name => _fileHandle.Path ?? SR.IO_UnknownFileName;

    /// <summary>Sets the length of this stream to the given value.</summary>
    /// <param name="value">The new length of the stream.</param>
    public override void SetLength(long value)
    {
        if (value < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), SR.ArgumentOutOfRange_NeedNonNegNum);
        }
        else if (IsClosed)
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

        FlushCore(flushToDisk: false);

        if (_appendStart != -1 && value < _appendStart)
        {
            throw new IOException(SR.IO_SetLengthAppendTruncate);
        }

        Debug.Assert(value >= 0);

        RandomAccess.SetFileLength(_fileHandle, value);
        Debug.Assert(!_fileHandle.TryGetCachedLength(out _), "If length can be cached (file opened for reading, not shared for writing), it should be impossible to modify file length");

        if (_filePosition > value)
        {
            _filePosition = value;
        }
    }

    /// <summary>Gets the length of the stream in bytes.</summary>
    public override long Length
    {
        get
        {
            if (IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }
            else if (!CanSeek)
            {
                ThrowHelper.ThrowNotSupportedException_UnseekableStream();
            }

            return GetLengthCore;
        }
    }

    private long GetLengthCore
    {
        get
        {
            long len = _fileHandle.GetFileLength();

            // If we're writing near the end of the file, we must include our
            // internal buffer in our Length calculation.  Don't flush because
            // we use the length of the file in AsyncWindowsFileStreamStrategy.WriteAsync
            if (_writePos > 0 && _filePosition + _writePos > len)
            {
                len = _writePos + _filePosition;
            }

            return len;
        }
    }

    /// <summary>Gets or sets the position within the current stream</summary>
    public override long Position
    {
        get
        {
            if (IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }
            else if (!CanSeek)
            {
                ThrowHelper.ThrowNotSupportedException_UnseekableStream();
            }

            return PositionCore;
        }
        set
        {
            if (value < 0)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), SR.ArgumentOutOfRange_NeedNonNegNum);
            }
            else if (!CanSeek)
            {
                if (IsClosed)
                {
                    ThrowHelper.ThrowObjectDisposedException_FileClosed();
                }

                ThrowHelper.ThrowNotSupportedException_UnseekableStream();
            }

            SeekInternal(value, SeekOrigin.Begin);
        }
    }

    private long PositionCore
    {
        get
        {
            Debug.Assert(!(_writePos > 0 && _readPos != _readLen), "Read and Write buffers cannot both have data in them at the same time.");
            return _filePosition + _readPos - _readLen + _writePos;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateReadWriteArgs(buffer, offset, count);

        AssertBufferArguments(buffer, offset, count);

        Debug.Assert((_readPos == 0 && _readLen == 0 && _writePos >= 0) || (_writePos == 0 && _readPos <= _readLen),
            "We're either reading or writing, but not both.");

        bool isBlocked = false;
        int n = _readLen - _readPos;
        // if the read buffer is empty, read into either user's array or our
        // buffer, depending on number of bytes user asked for and buffer size.
        if (n == 0)
        {
            EnsureCanRead();

            if (_writePos > 0)
            {
                FlushWrite();
            }

            if (!CanSeek || (count >= _bufferSize))
            {
                // For async file stream strategies the call to Read(Span) is translated to Stream.Read(Span),
                // which rents an array from the pool, copies the data, and then calls Read(Array). This is expensive!
                // To avoid that (and code duplication), the Read(Array) method passes ArraySegment to this method
                // which allows for calling Strategy.Read(Array) instead of Strategy.Read(Span).
                n = ReadCore(buffer, offset, count);

                // Throw away read buffer.
                _readPos = 0;
                _readLen = 0;
                return n;
            }

            n = ReadCore(_buffer, 0, _bufferSize);

            if (n == 0)
            {
                return 0;
            }

            isBlocked = n < _bufferSize;
            _readPos = 0;
            _readLen = n;
        }
        // Now copy min of count or numBytesAvailable (i.e. near EOF) to array.
        if (n > count)
        {
            n = count;
        }
        Buffer.BlockCopy(_buffer, _readPos, buffer, offset, n);
        _readPos += n;

        // We may have read less than the number of bytes the user asked
        // for, but that is part of the Stream contract.  Reading again for
        // more data may cause us to block if we're using a device with
        // no clear end of file, such as a serial port or pipe.  If we
        // blocked here & this code was used with redirected pipes for a
        // process's standard output, this can lead to deadlocks involving
        // two processes. But leave this here for files to avoid what would
        // probably be a breaking change.         --

        // If we are reading from a device with no clear EOF like a
        // serial port or a pipe, this will cause us to block incorrectly.
        if (CanSeek)
        {
            // If we hit the end of the buffer and didn't have enough bytes, we must
            // read some more from the underlying stream.  However, if we got
            // fewer bytes from the underlying stream than we asked for (i.e. we're
            // probably blocked), don't ask for more bytes.
            if (n < count && !isBlocked)
            {
                Debug.Assert(_readPos == _readLen, "Read buffer should be empty!");

                int moreBytesRead = ReadCore(buffer, offset + n, count - n);

                n += moreBytesRead;
                // We've just made our buffer inconsistent with our position
                // pointer.  We must throw away the read buffer.
                _readPos = 0;
                _readLen = 0;
            }
        }

        return n;
    }

    /// <summary>
    /// Reads a byte from the file stream.  Returns the byte cast to an int
    /// or -1 if reading from the end of the stream.
    /// </summary>
    public override int ReadByte() => _readPos != _readLen ? _buffer[_readPos++] : ReadByteSlow();

    private int ReadByteSlow()
    {
        Debug.Assert(_readPos == _readLen);

        // We want to check for whether the underlying stream has been closed and whether
        // it's readable, but we only need to do so if we don't have data in our buffer,
        // as any data we have came from reading it from an open stream, and we don't
        // care if the stream has been closed or become unreadable since. Further, if
        // the stream is closed, its read buffer is flushed, so we'll take this slow path.
        EnsureNotClosed();
        EnsureCanRead();

        if (_writePos > 0)
        {
            FlushWrite();
        }

        _readLen = ReadCore(_buffer, 0, _bufferSize);
        _readPos = 0;

        if (_readLen == 0)
        {
            return -1;
        }

        return _buffer[_readPos++];
    }

    private int ReadCore(byte[] buffer, int offset, int count)
    {
        if (_fileHandle.IsClosed)
        {
            ThrowHelper.ThrowObjectDisposedException_FileClosed();
        }
        else if ((_access & FileAccess.Read) == 0)
        {
            ThrowHelper.ThrowNotSupportedException_UnreadableStream();
        }

        int r = RandomAccess.ReadAtOffset_Fast(_fileHandle, buffer, offset, count, _filePosition);
        Debug.Assert(r >= 0, $"RandomAccess.ReadAtOffset returned {r}.");
        _filePosition += r;

        return r;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        ValidateReadWriteArgs(buffer, offset, count);

        AssertBufferArguments(buffer, offset, count);

        ReadOnlySpan<byte> source = new ReadOnlySpan<byte>(buffer, offset, count);
        ArraySegment<byte> arraySegment = new ArraySegment<byte>(buffer, offset, count);

        if (_writePos == 0)
        {
            EnsureCanWrite();
            ClearReadBufferBeforeWrite();
        }

        // If our buffer has data in it, copy data from the user's array into
        // the buffer, and if we can fit it all there, return.  Otherwise, write
        // the buffer to disk and copy any remaining data into our buffer.
        // The assumption here is memcpy is cheaper than disk (or net) IO.
        // (10 milliseconds to disk vs. ~20-30 microseconds for a 4K memcpy)
        // So the extra copying will reduce the total number of writes, in
        // non-pathological cases (i.e. write 1 byte, then write for the buffer
        // size repeatedly)
        if (_writePos > 0)
        {
            int numBytes = _bufferSize - _writePos;   // space left in buffer
            if (numBytes > 0)
            {
                if (numBytes >= source.Length)
                {
                    source.CopyTo(_buffer.AsSpan(_writePos));
                    _writePos += source.Length;
                    return;
                }
                else
                {
                    source[..numBytes].CopyTo(_buffer.AsSpan(_writePos));
                    _writePos += numBytes;
                    source = source[numBytes..];
                    if (arraySegment.Array != null)
                    {
                        arraySegment = arraySegment.Slice(numBytes);
                    }
                }
            }

            FlushWrite();
            Debug.Assert(_writePos == 0, "FlushWrite must set _writePos to 0");
        }

        // If the buffer would slow _bufferSize down, avoid buffer completely.
        if (source.Length >= _bufferSize)
        {
            Debug.Assert(_writePos == 0, "FileStream cannot have buffered data to write here!  Your stream will be corrupted.");

            // For async file stream strategies the call to Write(Span) is translated to Stream.Write(Span),
            // which rents an array from the pool, copies the data, and then calls Write(Array). This is expensive!
            // To avoid that (and code duplication), the Write(Array) method passes ArraySegment to this method
            // which allows for calling Strategy.Write(Array) instead of Strategy.Write(Span).
            WriteCore(arraySegment.Array!, arraySegment.Offset, arraySegment.Count);

            return;
        }
        else if (source.Length == 0)
        {
            return;  // Don't allocate a buffer then call memcpy for 0 bytes.
        }

        // Copy remaining bytes into buffer, to write at a later date.
        source.CopyTo(_buffer.AsSpan(_writePos));
        _writePos = source.Length;
    }

    /// <summary>
    /// Writes a byte to the current position in the stream and advances the position
    /// within the stream by one byte.
    /// </summary>
    /// <param name="value">The byte to write to the stream.</param>
    public override void WriteByte(byte value)
    {
        if (_writePos > 0 && _writePos < _bufferSize - 1)
        {
            _buffer[_writePos++] = value;
        }
        else
        {
            WriteByteSlow(value);
        }
    }

    private void WriteByteSlow(byte value)
    {
        if (_writePos == 0)
        {
            EnsureNotClosed();
            EnsureCanWrite();
            ClearReadBufferBeforeWrite();
        }
        else
        {
            Debug.Assert(_writePos <= _bufferSize);
            FlushWrite();
        }

        _buffer[_writePos++] = value;
    }

    private void WriteCore(byte[] buffer, int offset, int count)
    {
        if (_fileHandle.IsClosed)
        {
            ThrowHelper.ThrowObjectDisposedException_FileClosed();
        }
        else if ((_access & FileAccess.Write) == 0)
        {
            ThrowHelper.ThrowNotSupportedException_UnwritableStream();
        }

        ReadOnlySpan<byte> bufferSpan = new ReadOnlySpan<byte>(buffer, offset, count);
        RandomAccess.WriteAtOffset(_fileHandle, bufferSpan, _filePosition);
        _filePosition += bufferSpan.Length;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (!CanSeek)
        {
            if (IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }

            ThrowHelper.ThrowNotSupportedException_UnseekableStream();
        }

        return SeekInternal(offset, origin);
    }

    private long SeekInternal(long offset, SeekOrigin origin)
    {
        // If we have bytes in the write buffer, flush them out, seek and be done.
        if (_writePos > 0)
        {
            FlushWrite();
            return SeekCore(offset, origin);
        }

        // The buffer is either empty or we have a buffered read.
        if (_readLen - _readPos > 0 && origin == SeekOrigin.Current)
        {
            // If we have bytes in the read buffer, adjust the seek offset to account for the resulting difference
            // between this stream's position and the underlying stream's position.
            offset -= (_readLen - _readPos);
        }

        long oldPos = PositionCore;
        Debug.Assert(oldPos == _filePosition + (_readPos - _readLen));

        long newPos = SeekCore(offset, origin);

        // If the seek destination is still within the data currently in the buffer, we want to keep the buffer data and continue using it.
        // Otherwise we will throw away the buffer. This can only happen on read, as we flushed write data above.

        // The offset of the new/updated seek pointer within _buffer:
        long readPos = (newPos - (oldPos - _readPos));

        // If the offset of the updated seek pointer in the buffer is still legal, then we can keep using the buffer:
        if (0 <= readPos && readPos < _readLen)
        {
            _readPos = (int)readPos;
            // Adjust the seek pointer of the underlying stream to reflect the amount of useful bytes in the read buffer:
            SeekCore(_readLen - _readPos, SeekOrigin.Current);
        }
        else
        {  // The offset of the updated seek pointer is not a legal offset. Loose the buffer.
            _readPos = _readLen = 0;
        }

        Debug.Assert(newPos == PositionCore, $"newPos (={newPos}) == Position (={PositionCore})");
        return newPos;
    }

    private long SeekCore(long offset, SeekOrigin origin)
    {
        long oldPos = _filePosition;
        long pos = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.End => GetLengthCore + offset,
            _ => _filePosition + offset, // SeekOrigin.Current
        };

        if (pos >= 0)
        {
            _filePosition = pos;
        }
        else
        {
            // keep throwing the same exception we did when seek was causing actual offset change
            FileStreamHelpers.ThrowInvalidArgument(_fileHandle);
        }

        // Prevent users from overwriting data in a file that was opened in append mode.
        if (_appendStart != -1 && pos < _appendStart)
        {
            _filePosition = oldPos;
            throw new IOException(SR.IO_SeekAppendOverwrite);
        }

        return pos;
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
    public void Flush(bool flushToDisk)
    {
        if (IsClosed)
        {
            ThrowHelper.ThrowObjectDisposedException_FileClosed();
        }

        FlushCore(flushToDisk);
    }

    // Reading is done in blocks, but someone could read 1 byte from the buffer then write.
    // At that point, the underlying stream's pointer is out of sync with this stream's position.
    // All write functions should call this function to ensure that the buffered data is not lost.
    private void FlushRead()
    {
        Debug.Assert(_writePos == 0, "Write buffer must be empty in FlushRead!");

        if (_readPos - _readLen != 0)
        {
            SeekCore(_readPos - _readLen, SeekOrigin.Current);
        }

        _readPos = 0;
        _readLen = 0;
    }

    private void FlushWrite()
    {
        Debug.Assert(_readPos == 0 && _readLen == 0, "Read buffer must be empty in FlushWrite!");

        WriteCore(_buffer, 0, _writePos);
        _writePos = 0;
    }

    private void FlushCore(bool flushToDisk)
    {
        Debug.Assert(!IsClosed, "FileStream responsibility");
        Debug.Assert((_readPos == 0 && _readLen == 0 && _writePos >= 0) || (_writePos == 0 && _readPos <= _readLen),
            "We're either reading or writing, but not both.");

        if (_writePos > 0)
        {
            FlushWrite();
        }
        else if (_readLen > 0)
        {
            // If the underlying strategy is not seekable AND we have something in the read buffer, then FlushRead would throw.
            // We can either throw away the buffer resulting in data loss (!) or ignore the Flush.
            // We cannot throw because it would be a breaking change. We opt into ignoring the Flush in that situation.
            if (CanSeek)
            {
                FlushRead();
            }
        }

        // We still need to tell the underlying strategy to flush. It's NOP for !flushToDisk or !CanWrite.
        if (flushToDisk && CanWrite)
        {
            FileStreamHelpers.FlushToDisk(_fileHandle);
        }
        // If the Stream was seekable, then we should have called FlushRead which resets _readPos & _readLen.
        Debug.Assert(_writePos == 0 && (!CanSeek || (_readPos == 0 && _readLen == 0)));
    }

    /// <summary>
    /// Called by Write methods to clear the Read Buffer
    /// </summary>
    private void ClearReadBufferBeforeWrite()
    {
        Debug.Assert(_readPos <= _readLen, $"_readPos <= _readLen [{_readPos} <= {_readLen}]");

        // No read data in the buffer:
        if (_readPos == _readLen)
        {
            _readPos = _readLen = 0;
            return;
        }

        // Must have read data.
        Debug.Assert(_readPos < _readLen);
        FlushRead();
    }

    private void EnsureNotClosed()
    {
        if (IsClosed)
        {
            ThrowHelper.ThrowObjectDisposedException_StreamClosed(null);
        }
    }

    private void EnsureCanSeek()
    {
        if (!CanSeek)
        {
            ThrowHelper.ThrowNotSupportedException_UnseekableStream();
        }
    }

    private void EnsureCanRead()
    {
        if (!CanRead)
        {
            ThrowHelper.ThrowNotSupportedException_UnreadableStream();
        }
    }

    private void EnsureCanWrite()
    {
        if (!CanWrite)
        {
            ThrowHelper.ThrowNotSupportedException_UnwritableStream();
        }
    }

    [Conditional("DEBUG")]
    private void AssertBufferArguments(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count); // FileStream is supposed to call this
        Debug.Assert(!IsClosed, "FileStream ensures that strategy is not closed");
    }

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
    private static void ValidateBufferArguments(byte[] buffer, int offset, int count)
    {
        if (offset < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_NeedNonNegNum);
        }

        if ((uint)count > buffer.Length - offset)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(count), SR.Argument_InvalidOffLen);
        }
    }

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

    /// <summary>Validates arguments to Read and Write and throws resulting exceptions.</summary>
    /// <param name="buffer">The buffer to read from or write to.</param>
    /// <param name="offset">The zero-based offset into the buffer.</param>
    /// <param name="count">The maximum number of bytes to read or write.</param>
    private void ValidateReadWriteArgs(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        if (IsClosed)
        {
            ThrowHelper.ThrowObjectDisposedException_FileClosed();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (IsClosed)
        {
            return;
        }

        try
        {
            FlushCore(flushToDisk: false);
        }
        catch (Exception e) when (!disposing && FileStreamHelpers.IsIoRelatedException(e))
        {
            // On finalization, ignore failures from trying to flush the write buffer,
            // e.g. if this stream is wrapping a pipe and the pipe is now broken.
        }
        finally
        {
            // Don't set the buffer to null, to avoid a NullReferenceException
            // when users have a race condition in their code (i.e. they call
            // FileStream.Close when calling another method on FileStream like Read).

            // There is no need to call base.Dispose as it's empty
            _writePos = 0;

            if (disposing && _fileHandle != null! && !_fileHandle.IsClosed)
            {
                _fileHandle.Dispose();
            }
        }
    }
}

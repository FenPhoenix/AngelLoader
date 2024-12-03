// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;

namespace AL_Common.NETM_IO.Strategies
{
    // this type exists so we can avoid duplicating the buffering logic in every FileStreamStrategy implementation
    internal sealed class BufferedFileStreamStrategy : FileStreamStrategy
    {
        private readonly FileStreamStrategy _strategy;
        private readonly int _bufferSize;

        private readonly byte[] _buffer;
        private int _writePos;
        private int _readPos;
        private int _readLen;

        internal BufferedFileStreamStrategy(FileStreamStrategy strategy, byte[] buffer)
        {
            Debug.Assert(buffer.Length > 1, "Buffering must not be enabled for smaller buffer sizes");

            _strategy = strategy;
            _bufferSize = buffer.Length;
            _buffer = buffer;
        }

        public override bool CanRead => _strategy.CanRead;

        public override bool CanWrite => _strategy.CanWrite;

        public override bool CanSeek => _strategy.CanSeek;

        public override long Length
        {
            get
            {
                long len = _strategy.Length;

                // If we're writing near the end of the file, we must include our
                // internal buffer in our Length calculation.  Don't flush because
                // we use the length of the file in AsyncWindowsFileStreamStrategy.WriteAsync
                if (_writePos > 0 && _strategy.Position + _writePos > len)
                {
                    len = _writePos + _strategy.Position;
                }

                return len;
            }
        }

        public override long Position
        {
            get
            {
                Debug.Assert(!(_writePos > 0 && _readPos != _readLen), "Read and Write buffers cannot both have data in them at the same time.");

                return _strategy.Position + _readPos - _readLen + _writePos;
            }
            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        internal override bool IsClosed => _strategy.IsClosed;

        internal override string Name => _strategy.Name;

        internal override AL_SafeFileHandle AL_SafeFileHandle
        {
            get
            {
                // BufferedFileStreamStrategy must flush before the handle is exposed
                // so whoever uses AL_SafeFileHandle to access disk data can see
                // the changes that were buffered in memory so far
                Flush();

                return _strategy.AL_SafeFileHandle;
            }
        }

        protected sealed override void Dispose(bool disposing)
        {
            if (_strategy.IsClosed)
            {
                return;
            }

            try
            {
                Flush();
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

                _strategy.DisposeInternal(disposing);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            AssertBufferArguments(buffer, offset, count);

            return ReadSpan(buffer, offset, count);
        }

        private int ReadSpan(byte[] destination, int destOffset, int destLength)
        {
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

                if (!_strategy.CanSeek || (destLength >= _bufferSize))
                {
                    // For async file stream strategies the call to Read(Span) is translated to Stream.Read(Span),
                    // which rents an array from the pool, copies the data, and then calls Read(Array). This is expensive!
                    // To avoid that (and code duplication), the Read(Array) method passes ArraySegment to this method
                    // which allows for calling Strategy.Read(Array) instead of Strategy.Read(Span).
                    n = _strategy.Read(destination, destOffset, destLength);

                    // Throw away read buffer.
                    _readPos = 0;
                    _readLen = 0;
                    return n;
                }

                n = _strategy.Read(_buffer, 0, _bufferSize);

                if (n == 0)
                {
                    return 0;
                }

                isBlocked = n < _bufferSize;
                _readPos = 0;
                _readLen = n;
            }
            // Now copy min of count or numBytesAvailable (i.e. near EOF) to array.
            if (n > destLength)
            {
                n = destLength;
            }
            new ReadOnlySpan<byte>(_buffer, _readPos, n).CopyTo(destination);
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
            if (_strategy.CanSeek)
            {
                // If we hit the end of the buffer and didn't have enough bytes, we must
                // read some more from the underlying stream.  However, if we got
                // fewer bytes from the underlying stream than we asked for (i.e. we're
                // probably blocked), don't ask for more bytes.
                if (n < destLength && !isBlocked)
                {
                    Debug.Assert(_readPos == _readLen, "Read buffer should be empty!");

                    int moreBytesRead = _strategy.Read(destination, destOffset + n, destLength - n);

                    n += moreBytesRead;
                    // We've just made our buffer inconsistent with our position
                    // pointer.  We must throw away the read buffer.
                    _readPos = 0;
                    _readLen = 0;
                }
            }

            return n;
        }

        public override int ReadByte() => _readPos != _readLen ? _buffer![_readPos++] : ReadByteSlow();

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

            _readLen = _strategy.Read(_buffer, 0, _bufferSize);
            _readPos = 0;

            if (_readLen == 0)
            {
                return -1;
            }

            return _buffer[_readPos++];
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            AssertBufferArguments(buffer, offset, count);

            WriteSpan(new ReadOnlySpan<byte>(buffer, offset, count), new ArraySegment<byte>(buffer, offset, count));
        }

        private void WriteSpan(ReadOnlySpan<byte> source, ArraySegment<byte> arraySegment)
        {
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
                        source.CopyTo(_buffer!.AsSpan(_writePos));
                        _writePos += source.Length;
                        return;
                    }
                    else
                    {
                        source.Slice(0, numBytes).CopyTo(_buffer!.AsSpan(_writePos));
                        _writePos += numBytes;
                        source = source.Slice(numBytes);
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
                _strategy.Write(arraySegment.Array, arraySegment.Offset, arraySegment.Count);

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

        public override void WriteByte(byte value)
        {
            if (_writePos > 0 && _writePos < _bufferSize - 1)
            {
                _buffer![_writePos++] = value;
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

            _buffer![_writePos++] = value;
        }

        public override void SetLength(long value)
        {
            Flush();

            _strategy.SetLength(value);
        }

        public override void Flush() => Flush(flushToDisk: false);

        internal override void Flush(bool flushToDisk)
        {
            Debug.Assert(!_strategy.IsClosed, "FileStream responsibility");
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
                if (_strategy.CanSeek)
                {
                    FlushRead();
                }
            }

            // We still need to tell the underlying strategy to flush. It's NOP for !flushToDisk or !CanWrite.
            _strategy.Flush(flushToDisk);
            // If the Stream was seekable, then we should have called FlushRead which resets _readPos & _readLen.
            Debug.Assert(_writePos == 0 && (!_strategy.CanSeek || (_readPos == 0 && _readLen == 0)));
        }

        //public override void CopyTo(Stream destination, int bufferSize)
        //{
        //    EnsureNotClosed();
        //    EnsureCanRead();

        //    int readBytes = _readLen - _readPos;
        //    Debug.Assert(readBytes >= 0, $"Expected a non-negative number of bytes in buffer, got {readBytes}");

        //    if (readBytes > 0)
        //    {
        //        // If there's any read data in the buffer, write it all to the destination stream.
        //        Debug.Assert(_writePos == 0, "Write buffer must be empty if there's data in the read buffer");
        //        destination.Write(_buffer!, _readPos, readBytes);
        //        _readPos = _readLen = 0;
        //    }
        //    else if (_writePos > 0)
        //    {
        //        // If there's write data in the buffer, flush it back to the underlying stream, as does ReadAsync.
        //        FlushWrite();
        //    }

        //    // Our buffer is now clear. Copy data directly from the source stream to the destination stream.
        //    _strategy.CopyTo(destination, bufferSize);
        //}

        public override long Seek(long offset, SeekOrigin origin)
        {
            // If we have bytes in the write buffer, flush them out, seek and be done.
            if (_writePos > 0)
            {
                FlushWrite();
                return _strategy.Seek(offset, origin);
            }

            // The buffer is either empty or we have a buffered read.
            if (_readLen - _readPos > 0 && origin == SeekOrigin.Current)
            {
                // If we have bytes in the read buffer, adjust the seek offset to account for the resulting difference
                // between this stream's position and the underlying stream's position.
                offset -= (_readLen - _readPos);
            }

            long oldPos = Position;
            Debug.Assert(oldPos == _strategy.Position + (_readPos - _readLen));

            long newPos = _strategy.Seek(offset, origin);

            // If the seek destination is still within the data currently in the buffer, we want to keep the buffer data and continue using it.
            // Otherwise we will throw away the buffer. This can only happen on read, as we flushed write data above.

            // The offset of the new/updated seek pointer within _buffer:
            long readPos = (newPos - (oldPos - _readPos));

            // If the offset of the updated seek pointer in the buffer is still legal, then we can keep using the buffer:
            if (0 <= readPos && readPos < _readLen)
            {
                _readPos = (int)readPos;
                // Adjust the seek pointer of the underlying stream to reflect the amount of useful bytes in the read buffer:
                _strategy.Seek(_readLen - _readPos, SeekOrigin.Current);
            }
            else
            {  // The offset of the updated seek pointer is not a legal offset. Loose the buffer.
                _readPos = _readLen = 0;
            }

            Debug.Assert(newPos == Position, $"newPos (={newPos}) == Position (={Position})");
            return newPos;
        }

        // Reading is done in blocks, but someone could read 1 byte from the buffer then write.
        // At that point, the underlying stream's pointer is out of sync with this stream's position.
        // All write functions should call this function to ensure that the buffered data is not lost.
        private void FlushRead()
        {
            Debug.Assert(_writePos == 0, "Write buffer must be empty in FlushRead!");

            if (_readPos - _readLen != 0)
            {
                _strategy.Seek(_readPos - _readLen, SeekOrigin.Current);
            }

            _readPos = 0;
            _readLen = 0;
        }

        private void FlushWrite()
        {
            Debug.Assert(_readPos == 0 && _readLen == 0, "Read buffer must be empty in FlushWrite!");
            Debug.Assert(_buffer != null && _bufferSize >= _writePos, "Write buffer must be allocated and write position must be in the bounds of the buffer in FlushWrite!");

            _strategy.Write(_buffer, 0, _writePos);
            _writePos = 0;
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
            if (_strategy.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_StreamClosed(null);
            }
        }

        private void EnsureCanSeek()
        {
            if (!_strategy.CanSeek)
            {
                ThrowHelper.ThrowNotSupportedException_UnseekableStream();
            }
        }

        private void EnsureCanRead()
        {
            if (!_strategy.CanRead)
            {
                ThrowHelper.ThrowNotSupportedException_UnreadableStream();
            }
        }

        private void EnsureCanWrite()
        {
            if (!_strategy.CanWrite)
            {
                ThrowHelper.ThrowNotSupportedException_UnwritableStream();
            }
        }

        [Conditional("DEBUG")]
        private void AssertBufferArguments(byte[] buffer, int offset, int count)
        {
            ValidateBufferArguments(buffer, offset, count); // FileStream is supposed to call this
            Debug.Assert(!_strategy.IsClosed, "FileStream ensures that strategy is not closed");
        }
    }
}

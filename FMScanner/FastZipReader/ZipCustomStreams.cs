// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.IO;

namespace FMScanner.FastZipReader
{
    public sealed class SubReadStream : Stream
    {
        private long _startInSuperStream;
        private long _positionInSuperStream;
        private long _endInSuperStream;
        private Stream _superStream = null!;

        public void SetSuperStream(Stream? stream) => _superStream = stream!;

        internal void Set(long startPosition, long maxLength)
        {
            _startInSuperStream = startPosition;
            _positionInSuperStream = startPosition;
            _endInSuperStream = startPosition + maxLength;
        }

        public override long Length => _endInSuperStream - _startInSuperStream;

        public override long Position
        {
            get => _positionInSuperStream - _startInSuperStream;
            set => throw new NotSupportedException(SR.SeekingNotSupported);
        }

        public override bool CanRead => _superStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        private void ThrowIfCantRead()
        {
            if (!CanRead) throw new NotSupportedException(SR.ReadingNotSupported);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // parameter validation sent to _superStream.Read
            int origCount = count;

            ThrowIfCantRead();

            if (_superStream.Position != _positionInSuperStream)
            {
                _superStream.Seek(_positionInSuperStream, SeekOrigin.Begin);
            }

            if (_positionInSuperStream + count > _endInSuperStream)
            {
                count = (int)(_endInSuperStream - _positionInSuperStream);
            }

            Debug.Assert(count >= 0);
            Debug.Assert(count <= origCount);

            int ret = _superStream.Read(buffer, offset, count);

            _positionInSuperStream += ret;
            return ret;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfCantRead();

            if (origin != SeekOrigin.Current)
            {
                throw new NotSupportedException(SR.SeekingNotSupported);
            }

            if (_superStream.Position != _positionInSuperStream)
            {
                _superStream.Seek(_positionInSuperStream, SeekOrigin.Begin);
            }

            if (_positionInSuperStream + offset > _endInSuperStream)
            {
                offset = (int)(_endInSuperStream - _positionInSuperStream);
            }

            long ret = _superStream.Seek(offset, SeekOrigin.Current);

            _positionInSuperStream += ret;
            return ret;
        }

        public override void SetLength(long value) => throw new NotSupportedException(SR.SetLengthRequiresSeekingAndWriting);

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException(SR.WritingNotSupported);

        public override void Flush() => throw new NotSupportedException(SR.WritingNotSupported);
    }
}

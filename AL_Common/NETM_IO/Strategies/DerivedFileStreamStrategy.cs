// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;

namespace AL_Common.NETM_IO.Strategies
{
    // this type exists so we can avoid GetType() != typeof(FileStream) checks in FileStream
    // when FileStream was supposed to call base.Method() for such cases, we just call _fileStream.BaseMethod()
    // for everything else we fall back to the actual strategy (like FileStream does)
    //
    // it's crucial to NOT use the "base" keyword here! everything must be using _fileStream or _strategy
    internal sealed class DerivedFileStreamStrategy : FileStreamStrategy
    {
        private readonly FileStreamStrategy _strategy;
        private readonly FileStream_NET _fileStream;

        internal DerivedFileStreamStrategy(FileStream_NET fileStream, FileStreamStrategy strategy)
        {
            _fileStream = fileStream;
            _strategy = strategy;
            IsDerived = true;
        }

        public override bool CanRead => _strategy.CanRead;

        public override bool CanWrite => _strategy.CanWrite;

        public override bool CanSeek => _strategy.CanSeek;

        public override long Length => _strategy.Length;

        public override long Position
        {
            get => _strategy.Position;
            set => _strategy.Position = value;
        }

        internal override string Name => _strategy.Name;

        internal override AL_SafeFileHandle AL_SafeFileHandle
        {
            get
            {
                _fileStream.Flush(false);
                return _strategy.AL_SafeFileHandle;
            }
        }

        internal override bool IsClosed => _strategy.IsClosed;

        public override long Seek(long offset, SeekOrigin origin) => _strategy.Seek(offset, origin);

        public override void SetLength(long value) => _strategy.SetLength(value);

        public override int ReadByte() => _strategy.ReadByte();

        public override int Read(byte[] buffer, int offset, int count) => _strategy.Read(buffer, offset, count);

        public override void WriteByte(byte value) => _strategy.WriteByte(value);

        public override void Write(byte[] buffer, int offset, int count) => _strategy.Write(buffer, offset, count);

        public override void Flush() => throw new InvalidOperationException("FileStream should never call this method.");

        internal override void Flush(bool flushToDisk) => _strategy.Flush(flushToDisk);

        protected sealed override void Dispose(bool disposing) => _strategy.DisposeInternal(disposing);
    }
}

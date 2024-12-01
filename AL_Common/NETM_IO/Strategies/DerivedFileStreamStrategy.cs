// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        internal override bool IsAsync => _strategy.IsAsync;

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

        internal override void Lock(long position, long length) => _strategy.Lock(position, length);

        internal override void Unlock(long position, long length) => _strategy.Unlock(position, length);

        public override long Seek(long offset, SeekOrigin origin) => _strategy.Seek(offset, origin);

        public override void SetLength(long value) => _strategy.SetLength(value);

        public override int ReadByte() => _strategy.ReadByte();


        public override int Read(byte[] buffer, int offset, int count) => _strategy.Read(buffer, offset, count);

        public override void WriteByte(byte value) => _strategy.WriteByte(value);

        public override void Write(byte[] buffer, int offset, int count) => _strategy.Write(buffer, offset, count);

        public override void Flush() => throw new InvalidOperationException("FileStream should never call this method.");

        internal override void Flush(bool flushToDisk) => _strategy.Flush(flushToDisk);

        // If we have been inherited into a subclass, the following implementation could be incorrect
        // since it does not call through to Flush() which a subclass might have overridden.  To be safe
        // we will only use this implementation in cases where we know it is safe to do so,
        // and delegate to our base class (which will call into Flush) when we are not sure.
        public override Task FlushAsync(CancellationToken cancellationToken)
            => _fileStream.BaseFlushAsync(cancellationToken);

        // We also need to take this path if this is a derived
        // instance from FileStream, as a derived type could have overridden ReadAsync, in which
        // case our custom CopyToAsync implementation isn't necessarily correct.
        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            => _fileStream.BaseCopyToAsync(destination, bufferSize, cancellationToken);

        protected sealed override void Dispose(bool disposing) => _strategy.DisposeInternal(disposing);
    }
}

using System;
using System.IO;
using SharpCompress.Archives.SevenZip;

namespace SharpCompress.IO;

internal sealed class BufferedSubStream : Stream
{
    private long _position;
    private int _cacheOffset;
    private int _cacheLength;

    private long _bytesLeftToRead;
    private readonly Stream _stream;

    private readonly SevenZipContext _context;

    public BufferedSubStream(Stream stream, long origin, long bytesToRead, SevenZipContext context)
    {
        _stream = stream;

        _position = origin;
        _bytesLeftToRead = bytesToRead;

        _context = context;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override void Flush() => throw new NotSupportedException();

    public override long Length => _bytesLeftToRead;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    // FenPhoenix 2023: avoid a zillion byte[1] allocations
    public override int ReadByte() => Read(_context.Byte1, 0, 1) == 0 ? -1 : _context.Byte1[0];

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count > _bytesLeftToRead)
        {
            count = (int)_bytesLeftToRead;
        }

        if (count > 0)
        {
            if (_cacheLength == 0)
            {
                _cacheOffset = 0;
                _stream.Position = _position;
                _cacheLength = _stream.Read(_context.SubStreamBuffer, 0, SevenZipContext.SubStreamBufferLength);
                _position += _cacheLength;
            }

            if (count > _cacheLength)
            {
                count = _cacheLength;
            }

            Buffer.BlockCopy(_context.SubStreamBuffer, _cacheOffset, buffer, offset, count);
            _cacheOffset += count;
            _cacheLength -= count;
            _bytesLeftToRead -= count;
        }

        return count;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

#if !NETFRAMEWORK && !NETSTANDARD2_0

    public override int Read(Span<byte> buffer) => Stream.Read(buffer);

    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();

#endif
}

using System;
using System.IO;
using SharpCompress.Common;

namespace SharpCompress.IO;

internal sealed class BufferedSubStream : Stream
{
    private long _position;
    private int _cacheOffset;
    private int _cacheLength;
    // @SharpCompress: Crappy static array for now, until we can figure out how to pass it in for the batch scan
    private static readonly byte[] _cache = new byte[32 << 10];

    private long _bytesLeftToRead;
    private readonly Stream _stream;

    public BufferedSubStream(Stream stream, long origin, long bytesToRead)
    {
        _stream = stream;

        _position = origin;
        _bytesLeftToRead = bytesToRead;
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
    public override int ReadByte()
    {
        return Read(FEN_COMMON.Byte1, 0, 1) == 0 ? -1 : FEN_COMMON.Byte1[0];
    }

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
                _cacheLength = _stream.Read(_cache, 0, _cache.Length);
                _position += _cacheLength;
            }

            if (count > _cacheLength)
            {
                count = _cacheLength;
            }

            Buffer.BlockCopy(_cache, _cacheOffset, buffer, offset, count);
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

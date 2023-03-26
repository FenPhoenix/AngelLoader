using System;
using System.IO;

namespace SharpCompress.IO;

internal sealed class BufferedSubStream : NonDisposingStream
{
    private long _position;
    private int _cacheOffset;
    private int _cacheLength;
    // @SharpCompress: Crappy static array for now, until we can figure out how to pass it in for the batch scan
    private static readonly byte[] _cache = new byte[32 << 10];

    public BufferedSubStream(Stream stream, long origin, long bytesToRead)
        : base(stream, throwOnDispose: false)
    {
        _position = origin;
        BytesLeftToRead = bytesToRead;
    }

    private long BytesLeftToRead { get; set; }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override void Flush() => throw new NotSupportedException();

    public override long Length => BytesLeftToRead;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count > BytesLeftToRead)
        {
            count = (int)BytesLeftToRead;
        }

        if (count > 0)
        {
            if (_cacheLength == 0)
            {
                _cacheOffset = 0;
                Stream.Position = _position;
                _cacheLength = Stream.Read(_cache, 0, _cache.Length);
                _position += _cacheLength;
            }

            if (count > _cacheLength)
            {
                count = _cacheLength;
            }

            Buffer.BlockCopy(_cache, _cacheOffset, buffer, offset, count);
            _cacheOffset += count;
            _cacheLength -= count;
            BytesLeftToRead -= count;
        }

        return count;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}

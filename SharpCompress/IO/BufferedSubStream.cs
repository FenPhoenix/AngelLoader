using System;
using System.IO;

namespace SharpCompress.IO;

internal sealed class BufferedSubStream : NonDisposingStream
{
    private long position;
    private int cacheOffset;
    private int cacheLength;
    // @SharpCompress: Crappy static array for now, until we can figure out how to pass it in for the batch scan
    private static readonly byte[] cache = new byte[32 << 10];

    public BufferedSubStream(Stream stream, long origin, long bytesToRead)
        : base(stream, throwOnDispose: false)
    {
        position = origin;
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
            if (cacheLength == 0)
            {
                cacheOffset = 0;
                Stream.Position = position;
                cacheLength = Stream.Read(cache, 0, cache.Length);
                position += cacheLength;
            }

            if (count > cacheLength)
            {
                count = cacheLength;
            }

            Buffer.BlockCopy(cache, cacheOffset, buffer, offset, count);
            cacheOffset += count;
            cacheLength -= count;
            BytesLeftToRead -= count;
        }

        return count;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();
}

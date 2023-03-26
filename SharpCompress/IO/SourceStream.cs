using System;
using System.IO;
using SharpCompress.Common;

namespace SharpCompress.IO;

internal sealed class SourceStream : Stream
{
    private readonly Stream _onlyStream;

    internal SourceStream(Stream stream)
    {
        _onlyStream = stream;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => _onlyStream.Length;

    public override long Position
    {
        get => _onlyStream.Position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override void Flush() => _onlyStream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        var total = count;
        var r = -1;

        while (count != 0 && r != 0)
        {
            r = _onlyStream.Read(
                buffer,
                offset,
                (int)Math.Min(count, _onlyStream.Length - _onlyStream.Position)
            );
            count -= r;
            offset += r;

            if (count != 0 && _onlyStream.Position == _onlyStream.Length)
            {
                // Current stream switched
                // Add length of previous stream
                _onlyStream.Seek(0, SeekOrigin.Begin);
                r = -1; //BugFix: reset to allow loop if count is still not 0 - was breaking split zipx (lzma xz etc)
            }
        }

        return total - count;
    }

    // FenPhoenix 2023: avoid a zillion byte[1] allocations
    public override int ReadByte()
    {
        return Read(FEN_COMMON.Byte1, 0, 1) == 0 ? -1 : (int)FEN_COMMON.Byte1[0];
    }

    public override long Seek(long offset, SeekOrigin origin) => _onlyStream.Seek(offset, origin);

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

    public override void Close() => _onlyStream.Dispose();

    protected override void Dispose(bool disposing)
    {
        Close();
        base.Dispose(disposing);
    }
}

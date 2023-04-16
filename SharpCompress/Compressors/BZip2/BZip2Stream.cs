using System.IO;

namespace SharpCompress.Compressors.BZip2;

internal sealed class BZip2Stream : Stream
{
    private readonly Stream _stream;
    private bool _isDisposed;

    /// <summary>
    /// Create a BZip2Stream
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    internal BZip2Stream(Stream stream)
    {
        _stream = new CBZip2InputStream(stream);
    }

    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        if (disposing)
        {
            _stream.Dispose();
        }
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override void Flush() => _stream.Flush();

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        _stream.Read(buffer, offset, count);

    public override int ReadByte() => _stream.ReadByte();

    public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

    public override void SetLength(long value) => _stream.SetLength(value);

#if !NETFRAMEWORK && !NETSTANDARD2_0

    public override int Read(Span<byte> buffer) => stream.Read(buffer);

    public override void Write(ReadOnlySpan<byte> buffer) => stream.Write(buffer);
#endif

    public override void Write(byte[] buffer, int offset, int count) =>
        _stream.Write(buffer, offset, count);

    public override void WriteByte(byte value) => _stream.WriteByte(value);
}

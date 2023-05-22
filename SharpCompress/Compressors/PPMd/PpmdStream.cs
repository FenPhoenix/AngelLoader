#nullable disable

using System;
using System.IO;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Compressors.LZMA.RangeCoder;
using SharpCompress.Compressors.PPMd.H;
using SharpCompress.Compressors.PPMd.I1;

namespace SharpCompress.Compressors.PPMd;

internal sealed class PpmdStream : Stream
{
    private readonly PpmdProperties _properties;
    private readonly Stream _stream;
    private readonly SevenZipContext _context;
    private readonly Model _model;
    private readonly ModelPpm _modelH;
    private readonly Decoder _decoder;
    private long _position;
    private bool _isDisposed;

    internal PpmdStream(PpmdProperties properties, Stream stream, SevenZipContext context)
    {
        _properties = properties;
        _stream = stream;
        _context = context;

        if (properties.Version == PpmdVersion.I1)
        {
            _model = new Model();
            _model.DecodeStart(stream, properties);
        }
        if (properties.Version == PpmdVersion.H)
        {
            _modelH = new ModelPpm();
            _modelH.DecodeInit(stream, properties.ModelOrder, properties.AllocatorSize);
        }
        if (properties.Version == PpmdVersion.H7Z)
        {
            _modelH = new ModelPpm();
            _modelH.DecodeInit(null, properties.ModelOrder, properties.AllocatorSize);
            _decoder = new Decoder();
            _decoder.Init(stream);
        }
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override void Flush() { }

    protected override void Dispose(bool isDisposing)
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        base.Dispose(isDisposing);
    }

    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int size = 0;
        if (_properties.Version == PpmdVersion.I1)
        {
            size = _model.DecodeBlock(_stream, buffer, offset, count);
        }
        if (_properties.Version == PpmdVersion.H)
        {
            int c;
            while (size < count && (c = _modelH.DecodeChar()) >= 0)
            {
                buffer[offset++] = (byte)c;
                size++;
            }
        }
        if (_properties.Version == PpmdVersion.H7Z)
        {
            int c;
            while (size < count && (c = _modelH.DecodeChar(_decoder)) >= 0)
            {
                buffer[offset++] = (byte)c;
                size++;
            }
        }
        _position += size;
        return size;
    }

    // FenPhoenix 2023: avoid a zillion byte[1] allocations
    public override int ReadByte() => Read(_context.Byte1, 0, 1) == 0 ? -1 : _context.Byte1[0];

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
    {
    }
}

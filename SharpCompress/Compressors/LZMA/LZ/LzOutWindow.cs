#nullable disable

using System;
using System.IO;
using AL_Common;
using SharpCompress.Archives.SevenZip;

namespace SharpCompress.Compressors.LZMA.LZ;

internal sealed class OutWindow : IDisposable
{
    private byte[] _buffer;
    private int _windowSize;
    private int _pos;
    private int _streamPos;
    private int _pendingLen;
    private int _pendingDist;

    internal long _total;
    private long _limit;

    private readonly SevenZipContext _context;

    public OutWindow(SevenZipContext context) => _context = context;

    public void Create(int windowSize)
    {
        if (_windowSize != windowSize)
        {
            if (_buffer != null)
            {
                _context.ByteArrayPool.Return(_buffer);
            }
            _buffer = _context.ByteArrayPool.Rent(windowSize);
        }
        else
        {
            _buffer[windowSize - 1] = 0;
        }

        // Need to always clear or we get wrong behavior/errors
        _buffer.Clear();

        _windowSize = windowSize;
        _pos = 0;
        _streamPos = 0;
        _pendingLen = 0;
        _total = 0;
        _limit = 0;
    }

    public void Reset() => Create(_windowSize);

    public void CopyBlock(int distance, int len)
    {
        int size = len;
        int pos = _pos - distance - 1;
        if (pos < 0)
        {
            pos += _windowSize;
        }
        for (; size > 0 && _pos < _windowSize && _total < _limit; size--)
        {
            if (pos >= _windowSize)
            {
                pos = 0;
            }
            _buffer[_pos++] = _buffer[pos++];
            _total++;
        }
        _pendingLen = size;
        _pendingDist = distance;
    }

    public void PutByte(byte b)
    {
        _buffer[_pos++] = b;
        _total++;
    }

    public byte GetByte(int distance)
    {
        int pos = _pos - distance - 1;
        if (pos < 0)
        {
            pos += _windowSize;
        }
        return _buffer[pos];
    }

    public int CopyStream(Stream stream, int len)
    {
        int size = len;
        while (size > 0 && _pos < _windowSize && _total < _limit)
        {
            int curSize = _windowSize - _pos;
            if (curSize > _limit - _total)
            {
                curSize = (int)(_limit - _total);
            }
            if (curSize > size)
            {
                curSize = size;
            }
            int numReadBytes = stream.Read(_buffer, _pos, curSize);
            if (numReadBytes == 0)
            {
                throw new DataErrorException();
            }
            size -= numReadBytes;
            _pos += numReadBytes;
            _total += numReadBytes;
        }
        return len - size;
    }

    public void SetLimit(long size) => _limit = _total + size;

    public bool HasSpace => _pos < _windowSize && _total < _limit;

    public bool HasPending => _pendingLen > 0;

    public int Read(byte[] buffer, int offset, int count)
    {
        if (_streamPos >= _pos)
        {
            return 0;
        }

        int size = _pos - _streamPos;
        if (size > count)
        {
            size = count;
        }
        Buffer.BlockCopy(_buffer, _streamPos, buffer, offset, size);
        _streamPos += size;
        if (_streamPos >= _windowSize)
        {
            _pos = 0;
            _streamPos = 0;
        }
        return size;
    }

    public void CopyPending()
    {
        if (_pendingLen > 0)
        {
            CopyBlock(_pendingDist, _pendingLen);
        }
    }

    public int AvailableBytes => _pos - _streamPos;

    public void Dispose()
    {
        if (_buffer != null)
        {
            _context.ByteArrayPool.Return(_buffer);
        }
    }
}

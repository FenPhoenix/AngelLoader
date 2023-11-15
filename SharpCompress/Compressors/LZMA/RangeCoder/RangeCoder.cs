#nullable disable

using System.IO;

namespace SharpCompress.Compressors.LZMA.RangeCoder;

internal sealed class Decoder
{
    public const uint K_TOP_VALUE = (1 << 24);
    public uint _range;
    public uint _code;

    // public Buffer.InBuffer Stream = new Buffer.InBuffer(1 << 16);
    public Stream _stream;
    public long _total;

    public void Init(Stream stream)
    {
        // Stream.Init(stream);
        _stream = stream;

        _code = 0;
        _range = 0xFFFFFFFF;
        for (int i = 0; i < 5; i++)
        {
            _code = (_code << 8) | (byte)_stream.ReadByte();
        }
        _total = 5;
    }

    public void ReleaseStream() =>
        // Stream.ReleaseStream();
        _stream = null;

    private void Normalize()
    {
        while (_range < K_TOP_VALUE)
        {
            _code = (_code << 8) | (byte)_stream.ReadByte();
            _range <<= 8;
            _total++;
        }
    }

    public uint GetThreshold(uint total) => _code / (_range /= total);

    public void Decode(uint start, uint size)
    {
        _code -= start * _range;
        _range *= size;
        Normalize();
    }

    public uint DecodeDirectBits(int numTotalBits)
    {
        uint range = _range;
        uint code = _code;
        uint result = 0;
        for (int i = numTotalBits; i > 0; i--)
        {
            range >>= 1;
            /*
            result <<= 1;
            if (code >= range)
            {
                code -= range;
                result |= 1;
            }
            */
            uint t = (code - range) >> 31;
            code -= range & (t - 1);
            result = (result << 1) | (1 - t);

            if (range < K_TOP_VALUE)
            {
                code = (code << 8) | (byte)_stream.ReadByte();
                range <<= 8;
                _total++;
            }
        }
        _range = range;
        _code = code;
        return result;
    }

    public uint DecodeBit(uint size0, int numTotalBits)
    {
        uint newBound = (_range >> numTotalBits) * size0;
        uint symbol;
        if (_code < newBound)
        {
            symbol = 0;
            _range = newBound;
        }
        else
        {
            symbol = 1;
            _code -= newBound;
            _range -= newBound;
        }
        Normalize();
        return symbol;
    }

    public bool IsFinished => _code == 0;

    // ulong GetProcessedSize() {return Stream.GetProcessedSize(); }
}

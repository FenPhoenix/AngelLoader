#nullable disable

using System.IO;

namespace SharpCompress.Compressors.PPMd.H;

internal sealed class RangeCoder
{
    private const int TOP = 1 << 24;
    private const int BOT = 1 << 15;
    internal const long UINT_MASK = 0xFFFFffffL;

    // uint low, code, range;
    private long _low,
        _code,
        _range;
    private readonly Stream _stream;

    internal RangeCoder(Stream stream)
    {
        _stream = stream;
        Init();
    }

    private void Init()
    {
        SubRange = new SubRange();

        _low = _code = 0L;
        _range = 0xFFFFffffL;
        for (int i = 0; i < 4; i++)
        {
            _code = ((_code << 8) | Char) & UINT_MASK;
        }
    }

    internal int CurrentCount
    {
        get
        {
            _range = (_range / SubRange.Scale) & UINT_MASK;
            return (int)((_code - _low) / _range);
        }
    }

    private long Char
    {
        get
        {
            if (_stream != null)
            {
                return _stream.ReadByte();
            }
            return -1;
        }
    }

    //internal SubRange SubRange { get; private set; }
    internal SubRange SubRange;

    internal long GetCurrentShiftCount(int shift)
    {
        _range >>>= shift;
        return ((_code - _low) / _range) & UINT_MASK;
    }

    internal void Decode()
    {
        _low = (_low + (_range * SubRange.LowCount)) & UINT_MASK;
        _range = (_range * (SubRange.HighCount - SubRange.LowCount)) & UINT_MASK;
    }

    internal void AriDecNormalize()
    {
        //		while ((low ^ (low + range)) < TOP || range < BOT && ((range = -low & (BOT - 1)) != 0 ? true : true))
        //		{
        //			code = ((code << 8) | unpackRead.getChar()&0xff)&uintMask;
        //			range = (range << 8)&uintMask;
        //			low = (low << 8)&uintMask;
        //		}

        // Rewrote for clarity
        bool c2 = false;
        while ((_low ^ (_low + _range)) < TOP || (c2 = _range < BOT))
        {
            if (c2)
            {
                _range = (-_low & (BOT - 1)) & UINT_MASK;
                c2 = false;
            }
            _code = ((_code << 8) | Char) & UINT_MASK;
            _range = (_range << 8) & UINT_MASK;
            _low = (_low << 8) & UINT_MASK;
        }
    }
}

internal sealed class SubRange
{
    // uint LowCount, HighCount, scale;
    private long _lowCount,
        _highCount,
        _scale;

    internal void IncScale(int dScale) => Scale += dScale;

    internal long HighCount
    {
        get => _highCount;
        set => _highCount = value & RangeCoder.UINT_MASK;
    }

    internal long LowCount
    {
        get => _lowCount & RangeCoder.UINT_MASK;
        set => _lowCount = value & RangeCoder.UINT_MASK;
    }

    internal long Scale
    {
        get => _scale;
        set => _scale = value & RangeCoder.UINT_MASK;
    }
}

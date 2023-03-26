using System;

namespace SharpCompress.Compressors.LZMA;

internal sealed class BitVector
{
    private readonly uint[] _mBits;

    public BitVector(int length)
    {
        Length = length;
        _mBits = new uint[(length + 31) >> 5];
    }

    public BitVector(int length, bool initValue)
    {
        Length = length;
        _mBits = new uint[(length + 31) >> 5];

        if (initValue)
        {
            for (var i = 0; i < _mBits.Length; i++)
            {
                _mBits[i] = ~0u;
            }
        }
    }

    public readonly int Length;

    public bool this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return (_mBits[index >> 5] & (1u << (index & 31))) != 0;
        }
    }

    public void SetBit(int index)
    {
        if (index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        _mBits[index >> 5] |= 1u << (index & 31);
    }

    internal bool GetAndSet(int index)
    {
        if (index < 0 || index >= Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var bits = _mBits[index >> 5];
        var mask = 1u << (index & 31);
        _mBits[index >> 5] |= mask;
        return (bits & mask) != 0;
    }
}

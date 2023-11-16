using System;

namespace SharpCompress.Compressors.LZMA;

internal readonly ref struct BitVector
{
    // @SharpCompress: Recycle this
    private readonly uint[] _mBits;

    // Just to be explicit to tell it not to allocate an array
    public BitVector() => _mBits = null!;

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
            for (int i = 0; i < _mBits.Length; i++)
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

        uint bits = _mBits[index >> 5];
        uint mask = 1u << (index & 31);
        _mBits[index >> 5] |= mask;
        return (bits & mask) != 0;
    }
}

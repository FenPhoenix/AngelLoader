using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AL_Common;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Compressors.LZMA;
using static AL_Common.Common;

namespace SharpCompress.Common.SevenZip;

internal sealed class CFolder
{
    internal readonly List<CCoderInfo> _coders = new();
    internal readonly ListFast<CBindPair> _bindPairs = new(0);
    internal readonly ListFast<int> _packStreams = new(0);
    internal readonly ListFast<long> _unpackSizes = new(0);
    internal uint? _unpackCrc;

    internal bool UnpackCrcDefined => _unpackCrc != null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Reset()
    {
        _coders.Clear();
        _bindPairs.ClearFast();
        _packStreams.ClearFast();
        _unpackSizes.ClearFast();
        _unpackCrc = null;
    }

    public long GetUnpackSize()
    {
        if (_unpackSizes.Count == 0)
        {
            return 0;
        }

        for (int i = _unpackSizes.Count - 1; i >= 0; i--)
        {
            if (FindBindPairForOutStream(i) < 0)
            {
                return _unpackSizes[i];
            }
        }

        throw new InvalidOperationException();
    }

    public int GetNumOutStreams()
    {
        int count = 0;
        for (int i = 0; i < _coders.Count; i++)
        {
            count += _coders[i]._numOutStreams;
        }

        return count;
    }

    public int FindBindPairForInStream(int inStreamIndex)
    {
        for (int i = 0; i < _bindPairs.Count; i++)
        {
            if (_bindPairs[i].InIndex == inStreamIndex)
            {
                return i;
            }
        }

        return -1;
    }

    public int FindBindPairForOutStream(int outStreamIndex)
    {
        for (int i = 0; i < _bindPairs.Count; i++)
        {
            if (_bindPairs[i].OutIndex == outStreamIndex)
            {
                return i;
            }
        }

        return -1;
    }

    public int FindPackStreamArrayIndex(int inStreamIndex)
    {
        for (int i = 0; i < _packStreams.Count; i++)
        {
            if (_packStreams[i] == inStreamIndex)
            {
                return i;
            }
        }

        return -1;
    }

    private const int kNumCodersMax = 32; // don't change it
    internal const int kMaskSize = 32; // it must be >= kNumCodersMax
    private const int kNumBindsMax = 32;

    public bool CheckStructure(SevenZipContext context)
    {

        if (_coders.Count > kNumCodersMax || _bindPairs.Count > kNumBindsMax)
        {
            return false;
        }

        {
            var v = new BitVector(_bindPairs.Count + _packStreams.Count);

            for (int i = 0; i < _bindPairs.Count; i++)
            {
                if (v.GetAndSet(_bindPairs[i].InIndex))
                {
                    return false;
                }
            }

            for (int i = 0; i < _packStreams.Count; i++)
            {
                if (v.GetAndSet(_packStreams[i]))
                {
                    return false;
                }
            }
        }

        {
            var v = new BitVector(_unpackSizes.Count);
            for (int i = 0; i < _bindPairs.Count; i++)
            {
                if (v.GetAndSet(_bindPairs[i].OutIndex))
                {
                    return false;
                }
            }
        }

        uint[] mask = context.CFolder_Mask.Cleared();
        {
            var inStreamToCoder = new List<int>();
            var outStreamToCoder = new List<int>();
            for (int i = 0; i < _coders.Count; i++)
            {
                CCoderInfo coder = _coders[i];
                for (int j = 0; j < coder._numInStreams; j++)
                {
                    inStreamToCoder.Add(i);
                }
                for (int j = 0; j < coder._numOutStreams; j++)
                {
                    outStreamToCoder.Add(i);
                }
            }

            for (int i = 0; i < _bindPairs.Count; i++)
            {
                CBindPair bp = _bindPairs[i];
                mask[inStreamToCoder[bp.InIndex]] |= (1u << outStreamToCoder[bp.OutIndex]);
            }
        }

        for (int i = 0; i < kMaskSize; i++)
        {
            for (int j = 0; j < kMaskSize; j++)
            {
                if (((1u << j) & mask[i]) != 0)
                {
                    mask[i] |= mask[j];
                }
            }
        }

        for (int i = 0; i < kMaskSize; i++)
        {
            if (((1u << i) & mask[i]) != 0)
            {
                return false;
            }
        }

        return true;
    }
}

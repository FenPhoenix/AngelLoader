namespace SharpCompress.Compressors.LZMA.RangeCoder;

internal struct BitDecoder
{
    public const int K_NUM_BIT_MODEL_TOTAL_BITS = 11;
    public const uint K_BIT_MODEL_TOTAL = (1 << K_NUM_BIT_MODEL_TOTAL_BITS);
    private const int K_NUM_MOVE_BITS = 5;

    private uint _prob;

    public void Init() => _prob = K_BIT_MODEL_TOTAL >> 1;

    public uint Decode(Decoder rangeDecoder)
    {
        var newBound = (rangeDecoder._range >> K_NUM_BIT_MODEL_TOTAL_BITS) * _prob;
        if (rangeDecoder._code < newBound)
        {
            rangeDecoder._range = newBound;
            _prob += (K_BIT_MODEL_TOTAL - _prob) >> K_NUM_MOVE_BITS;
            if (rangeDecoder._range < Decoder.K_TOP_VALUE)
            {
                rangeDecoder._code =
                    (rangeDecoder._code << 8) | (byte)rangeDecoder._stream.ReadByte();
                rangeDecoder._range <<= 8;
                rangeDecoder._total++;
            }
            return 0;
        }
        rangeDecoder._range -= newBound;
        rangeDecoder._code -= newBound;
        _prob -= (_prob) >> K_NUM_MOVE_BITS;
        if (rangeDecoder._range < Decoder.K_TOP_VALUE)
        {
            rangeDecoder._code = (rangeDecoder._code << 8) | (byte)rangeDecoder._stream.ReadByte();
            rangeDecoder._range <<= 8;
            rangeDecoder._total++;
        }
        return 1;
    }
}

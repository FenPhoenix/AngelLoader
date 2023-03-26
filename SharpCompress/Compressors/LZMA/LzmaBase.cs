namespace SharpCompress.Compressors.LZMA;

internal static class Base
{
    internal const uint K_NUM_STATES = 12;

    internal struct State
    {
        public uint _index;

        public void Init() => _index = 0;

        public void UpdateChar()
        {
            if (_index < 4)
            {
                _index = 0;
            }
            else if (_index < 10)
            {
                _index -= 3;
            }
            else
            {
                _index -= 6;
            }
        }

        public void UpdateMatch() => _index = (uint)(_index < 7 ? 7 : 10);

        public void UpdateRep() => _index = (uint)(_index < 7 ? 8 : 11);

        public void UpdateShortRep() => _index = (uint)(_index < 7 ? 9 : 11);

        public readonly bool IsCharState() => _index < 7;
    }

    internal const int K_NUM_POS_SLOT_BITS = 6;

    private const int K_NUM_LEN_TO_POS_STATES_BITS = 2; // it's for speed optimization
    internal const uint K_NUM_LEN_TO_POS_STATES = 1 << K_NUM_LEN_TO_POS_STATES_BITS;

    internal const uint K_MATCH_MIN_LEN = 2;

    internal static uint GetLenToPosState(uint len)
    {
        len -= K_MATCH_MIN_LEN;
        if (len < K_NUM_LEN_TO_POS_STATES)
        {
            return len;
        }
        return K_NUM_LEN_TO_POS_STATES - 1;
    }

    internal const int K_NUM_ALIGN_BITS = 4;

    internal const uint K_START_POS_MODEL_INDEX = 4;
    internal const uint K_END_POS_MODEL_INDEX = 14;

    internal const uint K_NUM_FULL_DISTANCES = 1 << ((int)K_END_POS_MODEL_INDEX / 2);

    internal const int K_NUM_POS_STATES_BITS_MAX = 4;
    internal const uint K_NUM_POS_STATES_MAX = (1 << K_NUM_POS_STATES_BITS_MAX);

    internal const int K_NUM_LOW_LEN_BITS = 3;
    internal const int K_NUM_MID_LEN_BITS = 3;
    internal const int K_NUM_HIGH_LEN_BITS = 8;
    internal const uint K_NUM_LOW_LEN_SYMBOLS = 1 << K_NUM_LOW_LEN_BITS;
    internal const uint K_NUM_MID_LEN_SYMBOLS = 1 << K_NUM_MID_LEN_BITS;
}

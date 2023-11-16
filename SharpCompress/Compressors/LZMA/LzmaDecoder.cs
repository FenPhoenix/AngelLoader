#nullable disable

using System;
using SharpCompress.Compressors.LZMA.LZ;
using SharpCompress.Compressors.LZMA.RangeCoder;

namespace SharpCompress.Compressors.LZMA;

internal sealed class Decoder
{
    private sealed class LenDecoder
    {
        private BitDecoder _choice = new BitDecoder();
        private BitDecoder _choice2 = new BitDecoder();
        private readonly BitTreeDecoder[] _lowCoder = new BitTreeDecoder[Base.K_NUM_POS_STATES_MAX];
        private readonly BitTreeDecoder[] _midCoder = new BitTreeDecoder[Base.K_NUM_POS_STATES_MAX];
        private readonly BitTreeDecoder _highCoder = new BitTreeDecoder(Base.K_NUM_HIGH_LEN_BITS);
        private uint _numPosStates;

        public void Create(uint numPosStates)
        {
            for (uint posState = _numPosStates; posState < numPosStates; posState++)
            {
                _lowCoder[posState] = new BitTreeDecoder(Base.K_NUM_LOW_LEN_BITS);
                _midCoder[posState] = new BitTreeDecoder(Base.K_NUM_MID_LEN_BITS);
            }
            _numPosStates = numPosStates;
        }

        public void Init()
        {
            _choice.Init();
            for (uint posState = 0; posState < _numPosStates; posState++)
            {
                _lowCoder[posState].Init();
                _midCoder[posState].Init();
            }
            _choice2.Init();
            _highCoder.Init();
        }

        public uint Decode(RangeCoder.Decoder rangeDecoder, uint posState)
        {
            if (_choice.Decode(rangeDecoder) == 0)
            {
                return _lowCoder[posState].Decode(rangeDecoder);
            }
            uint symbol = Base.K_NUM_LOW_LEN_SYMBOLS;
            if (_choice2.Decode(rangeDecoder) == 0)
            {
                symbol += _midCoder[posState].Decode(rangeDecoder);
            }
            else
            {
                symbol += Base.K_NUM_MID_LEN_SYMBOLS;
                symbol += _highCoder.Decode(rangeDecoder);
            }
            return symbol;
        }
    }

    private sealed class LiteralDecoder
    {
        private struct Decoder2
        {
            private BitDecoder[] _decoders;

            public void Create() => _decoders = new BitDecoder[0x300];

            public readonly void Init()
            {
                for (int i = 0; i < 0x300; i++)
                {
                    _decoders[i].Init();
                }
            }

            public byte DecodeNormal(RangeCoder.Decoder rangeDecoder)
            {
                uint symbol = 1;
                do
                {
                    symbol = (symbol << 1) | _decoders[symbol].Decode(rangeDecoder);
                } while (symbol < 0x100);
                return (byte)symbol;
            }

            public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, byte matchByte)
            {
                uint symbol = 1;
                do
                {
                    uint matchBit = (uint)(matchByte >> 7) & 1;
                    matchByte <<= 1;
                    uint bit = _decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
                    symbol = (symbol << 1) | bit;
                    if (matchBit != bit)
                    {
                        while (symbol < 0x100)
                        {
                            symbol = (symbol << 1) | _decoders[symbol].Decode(rangeDecoder);
                        }
                        break;
                    }
                } while (symbol < 0x100);
                return (byte)symbol;
            }
        }

        private Decoder2[] _coders;
        private int _numPrevBits;
        private int _numPosBits;
        private uint _posMask;

        public void Create(int numPosBits, int numPrevBits)
        {
            if (_coders != null && _numPrevBits == numPrevBits && _numPosBits == numPosBits)
            {
                return;
            }
            _numPosBits = numPosBits;
            _posMask = ((uint)1 << numPosBits) - 1;
            _numPrevBits = numPrevBits;
            uint numStates = (uint)1 << (_numPrevBits + _numPosBits);
            _coders = new Decoder2[numStates];
            for (uint i = 0; i < numStates; i++)
            {
                _coders[i].Create();
            }
        }

        public void Init()
        {
            uint numStates = (uint)1 << (_numPrevBits + _numPosBits);
            for (uint i = 0; i < numStates; i++)
            {
                _coders[i].Init();
            }
        }

        private uint GetState(uint pos, byte prevByte) =>
            ((pos & _posMask) << _numPrevBits) + (uint)(prevByte >> (8 - _numPrevBits));

        public byte DecodeNormal(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte) =>
            _coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder);

        public byte DecodeWithMatchByte(
            RangeCoder.Decoder rangeDecoder,
            uint pos,
            byte prevByte,
            byte matchByte
        ) => _coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte);
    }

    private readonly BitDecoder[] _isMatchDecoders = new BitDecoder[
        Base.K_NUM_STATES << Base.K_NUM_POS_STATES_BITS_MAX
    ];
    private readonly BitDecoder[] _isRepDecoders = new BitDecoder[Base.K_NUM_STATES];
    private readonly BitDecoder[] _isRepG0Decoders = new BitDecoder[Base.K_NUM_STATES];
    private readonly BitDecoder[] _isRepG1Decoders = new BitDecoder[Base.K_NUM_STATES];
    private readonly BitDecoder[] _isRepG2Decoders = new BitDecoder[Base.K_NUM_STATES];
    private readonly BitDecoder[] _isRep0LongDecoders = new BitDecoder[
        Base.K_NUM_STATES << Base.K_NUM_POS_STATES_BITS_MAX
    ];

    private readonly BitTreeDecoder[] _posSlotDecoder = new BitTreeDecoder[
        Base.K_NUM_LEN_TO_POS_STATES
    ];
    private readonly BitDecoder[] _posDecoders = new BitDecoder[
        Base.K_NUM_FULL_DISTANCES - Base.K_END_POS_MODEL_INDEX
    ];

    private readonly BitTreeDecoder _posAlignDecoder = new BitTreeDecoder(Base.K_NUM_ALIGN_BITS);

    private readonly LenDecoder _lenDecoder = new LenDecoder();
    private readonly LenDecoder _repLenDecoder = new LenDecoder();

    private readonly LiteralDecoder _literalDecoder = new LiteralDecoder();

    private uint _posStateMask;

    private Base.State _state = new Base.State();
    private uint _rep0,
        _rep1,
        _rep2,
        _rep3;

    public Decoder()
    {
        for (int i = 0; i < Base.K_NUM_LEN_TO_POS_STATES; i++)
        {
            _posSlotDecoder[i] = new BitTreeDecoder(Base.K_NUM_POS_SLOT_BITS);
        }
    }

    private void SetLiteralProperties(int lp, int lc)
    {
        if (lp > 8)
        {
            throw new InvalidParamException();
        }
        if (lc > 8)
        {
            throw new InvalidParamException();
        }
        _literalDecoder.Create(lp, lc);
    }

    private void SetPosBitsProperties(int pb)
    {
        if (pb > Base.K_NUM_POS_STATES_BITS_MAX)
        {
            throw new InvalidParamException();
        }
        uint numPosStates = (uint)1 << pb;
        _lenDecoder.Create(numPosStates);
        _repLenDecoder.Create(numPosStates);
        _posStateMask = numPosStates - 1;
    }

    private void Init()
    {
        uint i;
        for (i = 0; i < Base.K_NUM_STATES; i++)
        {
            for (uint j = 0; j <= _posStateMask; j++)
            {
                uint index = (i << Base.K_NUM_POS_STATES_BITS_MAX) + j;
                _isMatchDecoders[index].Init();
                _isRep0LongDecoders[index].Init();
            }
            _isRepDecoders[i].Init();
            _isRepG0Decoders[i].Init();
            _isRepG1Decoders[i].Init();
            _isRepG2Decoders[i].Init();
        }

        _literalDecoder.Init();
        for (i = 0; i < Base.K_NUM_LEN_TO_POS_STATES; i++)
        {
            _posSlotDecoder[i].Init();
        }

        // _PosSpecDecoder.Init();
        for (i = 0; i < Base.K_NUM_FULL_DISTANCES - Base.K_END_POS_MODEL_INDEX; i++)
        {
            _posDecoders[i].Init();
        }

        _lenDecoder.Init();
        _repLenDecoder.Init();
        _posAlignDecoder.Init();

        _state.Init();
        _rep0 = 0;
        _rep1 = 0;
        _rep2 = 0;
        _rep3 = 0;
    }

    internal bool Code(int dictionarySize, OutWindow outWindow, RangeCoder.Decoder rangeDecoder)
    {
        int dictionarySizeCheck = Math.Max(dictionarySize, 1);

        outWindow.CopyPending();

        while (outWindow.HasSpace)
        {
            uint posState = (uint)outWindow._total & _posStateMask;
            if (
                _isMatchDecoders[
                    (_state._index << Base.K_NUM_POS_STATES_BITS_MAX) + posState
                ].Decode(rangeDecoder) == 0
            )
            {
                byte b;
                byte prevByte = outWindow.GetByte(0);
                if (!_state.IsCharState())
                {
                    b = _literalDecoder.DecodeWithMatchByte(
                        rangeDecoder,
                        (uint)outWindow._total,
                        prevByte,
                        outWindow.GetByte((int)_rep0)
                    );
                }
                else
                {
                    b = _literalDecoder.DecodeNormal(
                        rangeDecoder,
                        (uint)outWindow._total,
                        prevByte
                    );
                }
                outWindow.PutByte(b);
                _state.UpdateChar();
            }
            else
            {
                uint len;
                if (_isRepDecoders[_state._index].Decode(rangeDecoder) == 1)
                {
                    if (_isRepG0Decoders[_state._index].Decode(rangeDecoder) == 0)
                    {
                        if (
                            _isRep0LongDecoders[
                                (_state._index << Base.K_NUM_POS_STATES_BITS_MAX) + posState
                            ].Decode(rangeDecoder) == 0
                        )
                        {
                            _state.UpdateShortRep();
                            outWindow.PutByte(outWindow.GetByte((int)_rep0));
                            continue;
                        }
                    }
                    else
                    {
                        uint distance;
                        if (_isRepG1Decoders[_state._index].Decode(rangeDecoder) == 0)
                        {
                            distance = _rep1;
                        }
                        else
                        {
                            if (_isRepG2Decoders[_state._index].Decode(rangeDecoder) == 0)
                            {
                                distance = _rep2;
                            }
                            else
                            {
                                distance = _rep3;
                                _rep3 = _rep2;
                            }
                            _rep2 = _rep1;
                        }
                        _rep1 = _rep0;
                        _rep0 = distance;
                    }
                    len = _repLenDecoder.Decode(rangeDecoder, posState) + Base.K_MATCH_MIN_LEN;
                    _state.UpdateRep();
                }
                else
                {
                    _rep3 = _rep2;
                    _rep2 = _rep1;
                    _rep1 = _rep0;
                    len = Base.K_MATCH_MIN_LEN + _lenDecoder.Decode(rangeDecoder, posState);
                    _state.UpdateMatch();
                    uint posSlot = _posSlotDecoder[Base.GetLenToPosState(len)].Decode(rangeDecoder);
                    if (posSlot >= Base.K_START_POS_MODEL_INDEX)
                    {
                        int numDirectBits = (int)((posSlot >> 1) - 1);
                        _rep0 = ((2 | (posSlot & 1)) << numDirectBits);
                        if (posSlot < Base.K_END_POS_MODEL_INDEX)
                        {
                            _rep0 += BitTreeDecoder.ReverseDecode(
                                _posDecoders,
                                _rep0 - posSlot - 1,
                                rangeDecoder,
                                numDirectBits
                            );
                        }
                        else
                        {
                            _rep0 += (
                                rangeDecoder.DecodeDirectBits(numDirectBits - Base.K_NUM_ALIGN_BITS)
                                << Base.K_NUM_ALIGN_BITS
                            );
                            _rep0 += _posAlignDecoder.ReverseDecode(rangeDecoder);
                        }
                    }
                    else
                    {
                        _rep0 = posSlot;
                    }
                }
                if (_rep0 >= outWindow._total || _rep0 >= dictionarySizeCheck)
                {
                    if (_rep0 == 0xFFFFFFFF)
                    {
                        return true;
                    }
                    throw new DataErrorException();
                }
                outWindow.CopyBlock((int)_rep0, (int)len);
            }
        }
        return false;
    }

    internal void SetDecoderProperties(byte[] properties)
    {
        if (properties.Length < 1)
        {
            throw new InvalidParamException();
        }
        int lc = properties[0] % 9;
        int remainder = properties[0] / 9;
        int lp = remainder % 5;
        int pb = remainder / 5;
        if (pb > Base.K_NUM_POS_STATES_BITS_MAX)
        {
            throw new InvalidParamException();
        }
        SetLiteralProperties(lp, lc);
        SetPosBitsProperties(pb);
        Init();
    }
}

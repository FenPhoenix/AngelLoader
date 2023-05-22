#nullable disable

using System;
using System.Buffers.Binary;

namespace SharpCompress.Compressors.PPMd.H;

internal sealed class PpmContext : Pointer
{
    internal readonly FreqData FreqData;

    public int NumStats
    {
        get
        {
            if (Memory != null)
            {
                _numStats = BinaryPrimitives.ReadInt16LittleEndian(Memory.AsSpan(Address)) & 0xffff;
            }
            return _numStats;
        }
        set
        {
            _numStats = value & 0xffff;
            if (Memory != null)
            {
                BinaryPrimitives.WriteInt16LittleEndian(Memory.AsSpan(Address), (short)value);
            }
        }
    }

    //UPGRADE_NOTE: Final was removed from the declaration of 'unionSize '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    //UPGRADE_NOTE: The initialization of  'unionSize' was moved to static method 'SharpCompress.Unpack.PPM.PPMContext'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1005'"
    private static readonly int UNION_SIZE;

    //UPGRADE_NOTE: Final was removed from the declaration of 'size '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    public static readonly int SIZE = 2 + UNION_SIZE + 4; // 12

    // ushort NumStats;
    private int _numStats; // determines if feqData or onstate is used

    // (1==onestate)

    //UPGRADE_NOTE: Final was removed from the declaration of 'freqData '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"

    // |-> union
    //UPGRADE_NOTE: Final was removed from the declaration of 'oneState '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _oneState; // -/

    private int _suffix; // pointer ppmcontext

    //UPGRADE_NOTE: Final was removed from the declaration of 'ExpEscape'. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    public static readonly int[] EXP_ESCAPE = { 25, 14, 9, 7, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 2 };

    // Temp fields
    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState1 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState1 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState2 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState2 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState3 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState3 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState4 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState4 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState5 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState5 = new State(null);
    private PpmContext _tempPpmContext;

    //UPGRADE_NOTE: Final was removed from the declaration of 'ps '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    internal readonly int[] _ps = new int[256];

    public PpmContext(byte[] memory)
        : base(memory)
    {
        _oneState = new State(memory);
        FreqData = new FreqData(memory);
    }

    internal PpmContext Initialize(byte[] mem)
    {
        _oneState.Initialize(mem);
        FreqData.Initialize(mem);
        return Initialize<PpmContext>(mem);
    }

    internal State GetOneState() => _oneState;

    private void SetOneState(StateRef oneState) => _oneState.SetValues(oneState);

    internal int GetSuffix()
    {
        if (Memory != null)
        {
            _suffix = BinaryPrimitives.ReadInt32LittleEndian(Memory.AsSpan(Address + 8));
        }
        return _suffix;
    }

    private void SetSuffix(PpmContext suffix) => SetSuffix(suffix.Address);

    internal void SetSuffix(int suffix)
    {
        _suffix = suffix;
        if (Memory != null)
        {
            BinaryPrimitives.WriteInt32LittleEndian(Memory.AsSpan(Address + 8), suffix);
        }
    }

    internal override int Address
    {
        get => base.Address;
        set
        {
            base.Address = value;
            _oneState.Address = value + 2;
            FreqData.Address = value + 2;
        }
    }

    private PpmContext GetTempPpmContext(byte[] memory)
    {
        _tempPpmContext ??= new PpmContext(null);
        return _tempPpmContext.Initialize(memory);
    }

    internal int CreateChild(ModelPpm model, State pStats, StateRef firstState)
    {
        PpmContext pc = GetTempPpmContext(model.SubAlloc.Heap);
        pc.Address = model.SubAlloc.AllocContext();
        if (pc != null)
        {
            pc.NumStats = 1;
            pc.SetOneState(firstState);
            pc.SetSuffix(this);
            pStats.SetSuccessor(pc);
        }
        return pc.Address;
    }

    private void Rescale(ModelPpm model)
    {
        int oldNs = NumStats,
            i = NumStats - 1,
            adder,
            escFreq;

        // STATE* p1, * p;
        var p1 = new State(model.Heap);
        var p = new State(model.Heap);
        var temp = new State(model.Heap);

        for (
            p.Address = model.FoundState.Address;
            p.Address != FreqData.GetStats();
            p.DecrementAddress()
        )
        {
            temp.Address = p.Address - State.SIZE;
            State.PpmdSwap(p, temp);
        }
        temp.Address = FreqData.GetStats();
        temp.IncrementFreq(4);
        FreqData.IncrementSummFreq(4);
        escFreq = FreqData.SummFreq - p.Freq;
        adder = (model.OrderFall != 0) ? 1 : 0;
        p.Freq = (p.Freq + adder) >>> 1;
        FreqData.SummFreq = p.Freq;
        do
        {
            p.IncrementAddress();
            escFreq -= p.Freq;
            p.Freq = (p.Freq + adder) >>> 1;
            FreqData.IncrementSummFreq(p.Freq);
            temp.Address = p.Address - State.SIZE;
            if (p.Freq > temp.Freq)
            {
                p1.Address = p.Address;
                var tmp = new StateRef
                {
                    Values = p1
                };
                var temp2 = new State(model.Heap);
                var temp3 = new State(model.Heap);
                do
                {
                    // p1[0]=p1[-1];
                    temp2.Address = p1.Address - State.SIZE;
                    p1.SetValues(temp2);
                    p1.DecrementAddress();
                    temp3.Address = p1.Address - State.SIZE;
                } while (p1.Address != FreqData.GetStats() && tmp.Freq > temp3.Freq);
                p1.SetValues(tmp);
            }
        } while (--i != 0);
        if (p.Freq == 0)
        {
            do
            {
                i++;
                p.DecrementAddress();
            } while (p.Freq == 0);
            escFreq += i;
            NumStats -= i;
            if (NumStats == 1)
            {
                var tmp = new StateRef();
                temp.Address = FreqData.GetStats();
                tmp.Values = temp;

                // STATE tmp=*U.Stats;
                do
                {
                    // tmp.Freq-=(tmp.Freq >> 1)
                    tmp.DecrementFreq(tmp.Freq >>> 1);
                    escFreq >>>= 1;
                } while (escFreq > 1);
                model.SubAlloc.FreeUnits(FreqData.GetStats(), (oldNs + 1) >>> 1);
                _oneState.SetValues(tmp);
                model.FoundState.Address = _oneState.Address;
                return;
            }
        }
        escFreq -= (escFreq >>> 1);
        FreqData.IncrementSummFreq(escFreq);
        int n0 = (oldNs + 1) >>> 1,
            n1 = (NumStats + 1) >>> 1;
        if (n0 != n1)
        {
            FreqData.SetStats(model.SubAlloc.ShrinkUnits(FreqData.GetStats(), n0, n1));
        }
        model.FoundState.Address = FreqData.GetStats();
    }

    internal int GetArrayIndex(ModelPpm model, State rs)
    {
        PpmContext tempSuffix = GetTempPpmContext(model.SubAlloc.Heap);
        tempSuffix.Address = GetSuffix();
        int ret = 0;
        ret += model.PrevSuccess;
        ret += model.GetNs2BsIndx()[tempSuffix.NumStats - 1];
        ret += model.HiBitsFlag + (2 * model.GetHb2Flag()[rs.Symbol]);
        ret += (model.RunLength >>> 26) & 0x20;
        return ret;
    }

    internal static int GetMean(int summ, int shift, int round) =>
        (summ + (1 << (shift - round))) >>> shift;

    internal void DecodeBinSymbol(ModelPpm model)
    {
        State rs = _tempState1.Initialize(model.Heap);
        rs.Address = _oneState.Address; // State&
        model.HiBitsFlag = model.GetHb2Flag()[model.FoundState.Symbol];
        int off1 = rs.Freq - 1;
        int off2 = GetArrayIndex(model, rs);
        int bs = model.BinSumm[off1][off2];
        if (model.Coder.GetCurrentShiftCount(ModelPpm.TOT_BITS) < bs)
        {
            model.FoundState.Address = rs.Address;
            rs.IncrementFreq((rs.Freq < 128) ? 1 : 0);
            model.Coder.SubRange.LowCount = 0;
            model.Coder.SubRange.HighCount = bs;
            bs = ((bs + ModelPpm.INTERVAL - GetMean(bs, ModelPpm.PERIOD_BITS, 2)) & 0xffff);
            model.BinSumm[off1][off2] = bs;
            model.PrevSuccess = 1;
            model.IncRunLength(1);
        }
        else
        {
            model.Coder.SubRange.LowCount = bs;
            bs = (bs - GetMean(bs, ModelPpm.PERIOD_BITS, 2)) & 0xFFFF;
            model.BinSumm[off1][off2] = bs;
            model.Coder.SubRange.HighCount = ModelPpm.BIN_SCALE;
            model.NumMasked = 1;
            model.CharMask[rs.Symbol] = model.EscCount;
            model.PrevSuccess = 0;
            model.FoundState.Address = 0;
        }

        //int a = 0;//TODO just 4 debugging
    }

    //	public static void ppmdSwap(ModelPPM model, StatePtr state1, StatePtr state2)
    //	{
    //		byte[] bytes = model.getSubAlloc().getHeap();
    //		int p1 = state1.Address;
    //		int p2 = state2.Address;
    //
    //		for (int i = 0; i < StatePtr.size; i++) {
    //			byte temp = bytes[p1+i];
    //			bytes[p1+i] = bytes[p2+i];
    //			bytes[p2+i] = temp;
    //		}
    //		state1.Address=p1);
    //		state2.Address=p2);
    //	}

    internal void Update1(ModelPpm model, int p)
    {
        model.FoundState.Address = p;
        model.FoundState.IncrementFreq(4);
        FreqData.IncrementSummFreq(4);
        State p0 = _tempState3.Initialize(model.Heap);
        State p1 = _tempState4.Initialize(model.Heap);
        p0.Address = p;
        p1.Address = p - State.SIZE;
        if (p0.Freq > p1.Freq)
        {
            State.PpmdSwap(p0, p1);
            model.FoundState.Address = p1.Address;
            if (p1.Freq > ModelPpm.MAX_FREQ)
            {
                Rescale(model);
            }
        }
    }

    internal void Update1_0(ModelPpm model, int p)
    {
        model.FoundState.Address = p;
        model.PrevSuccess = 2 * model.FoundState.Freq > FreqData.SummFreq ? 1 : 0;
        model.IncRunLength(model.PrevSuccess);
        FreqData.IncrementSummFreq(4);
        model.FoundState.IncrementFreq(4);
        if (model.FoundState.Freq > ModelPpm.MAX_FREQ)
        {
            Rescale(model);
        }
    }

    internal bool DecodeSymbol2(ModelPpm model)
    {
        long count;
        int hiCnt,
            i = NumStats - model.NumMasked;
        See2Context psee2C = MakeEscFreq2(model, i);
        RangeCoder coder = model.Coder;

        // STATE* ps[256], ** pps=ps, * p=U.Stats-1;
        State p = _tempState1.Initialize(model.Heap);
        State temp = _tempState2.Initialize(model.Heap);
        p.Address = FreqData.GetStats() - State.SIZE;
        int pps = 0;
        hiCnt = 0;

        do
        {
            do
            {
                p.IncrementAddress(); // p++;
            } while (model.CharMask[p.Symbol] == model.EscCount);
            hiCnt += p.Freq;
            _ps[pps++] = p.Address;
        } while (--i != 0);
        coder.SubRange.IncScale(hiCnt);
        count = coder.CurrentCount;
        if (count >= coder.SubRange.Scale)
        {
            return false;
        }
        pps = 0;
        p.Address = _ps[pps];
        if (count < hiCnt)
        {
            hiCnt = 0;
            while ((hiCnt += p.Freq) <= count)
            {
                p.Address = _ps[++pps]; // p=*++pps;
            }
            coder.SubRange.HighCount = hiCnt;
            coder.SubRange.LowCount = hiCnt - p.Freq;
            psee2C.Update();
            Update2(model, p.Address);
        }
        else
        {
            coder.SubRange.LowCount = hiCnt;
            coder.SubRange.HighCount = coder.SubRange.Scale;
            i = NumStats - model.NumMasked; // ->NumMasked;
            pps--;
            do
            {
                temp.Address = _ps[++pps]; // (*++pps)
                model.CharMask[temp.Symbol] = model.EscCount;
            } while (--i != 0);
            psee2C.IncSumm((int)coder.SubRange.Scale);
            model.NumMasked = NumStats;
        }
        return true;
    }

    internal void Update2(ModelPpm model, int p)
    {
        State temp = _tempState5.Initialize(model.Heap);
        temp.Address = p;
        model.FoundState.Address = p;
        model.FoundState.IncrementFreq(4);
        FreqData.IncrementSummFreq(4);
        if (temp.Freq > ModelPpm.MAX_FREQ)
        {
            Rescale(model);
        }
        model.IncEscCount(1);
        model.RunLength = model.InitRl;
    }

    private See2Context MakeEscFreq2(ModelPpm model, int diff)
    {
        See2Context psee2C;
        int numStats = NumStats;
        if (numStats != 256)
        {
            PpmContext suff = GetTempPpmContext(model.Heap);
            suff.Address = GetSuffix();
            int idx1 = model.GetNs2Indx()[diff - 1];
            int idx2 = 0;
            idx2 += ((diff < suff.NumStats - numStats) ? 1 : 0);
            idx2 += 2 * ((FreqData.SummFreq < 11 * numStats) ? 1 : 0);
            idx2 += 4 * ((model.NumMasked > diff) ? 1 : 0);
            idx2 += model.HiBitsFlag;
            psee2C = model.GetSee2Cont()[idx1][idx2];
            model.Coder.SubRange.Scale = psee2C.Mean;
        }
        else
        {
            psee2C = model.DummySee2Cont;
            model.Coder.SubRange.Scale = 1;
        }
        return psee2C;
    }

    internal See2Context MakeEscFreq(ModelPpm model, int numMasked, out int escFreq)
    {
        See2Context psee2C;
        int numStats = NumStats;
        int nonMasked = numStats - numMasked;
        if (numStats != 256)
        {
            PpmContext suff = GetTempPpmContext(model.Heap);
            suff.Address = GetSuffix();
            int idx1 = model.GetNs2Indx()[nonMasked - 1];
            int idx2 = 0;
            idx2 += ((nonMasked < suff.NumStats - numStats) ? 1 : 0);
            idx2 += 2 * ((FreqData.SummFreq < 11 * numStats) ? 1 : 0);
            idx2 += 4 * ((numMasked > nonMasked) ? 1 : 0);
            idx2 += model.HiBitsFlag;
            psee2C = model.GetSee2Cont()[idx1][idx2];
            escFreq = psee2C.Mean;
        }
        else
        {
            psee2C = model.DummySee2Cont;
            escFreq = 1;
        }
        return psee2C;
    }

    internal bool DecodeSymbol1(ModelPpm model)
    {
        RangeCoder coder = model.Coder;
        coder.SubRange.Scale = FreqData.SummFreq;
        var p = new State(model.Heap)
        {
            Address = FreqData.GetStats()
        };
        int hiCnt;
        long count = coder.CurrentCount;
        if (count >= coder.SubRange.Scale)
        {
            return false;
        }
        if (count < (hiCnt = p.Freq))
        {
            coder.SubRange.HighCount = hiCnt;
            model.PrevSuccess = (2 * hiCnt > coder.SubRange.Scale) ? 1 : 0;
            model.IncRunLength(model.PrevSuccess);
            hiCnt += 4;
            model.FoundState.Address = p.Address;
            model.FoundState.Freq = hiCnt;
            FreqData.IncrementSummFreq(4);
            if (hiCnt > ModelPpm.MAX_FREQ)
            {
                Rescale(model);
            }
            coder.SubRange.LowCount = 0;
            return true;
        }
        if (model.FoundState.Address == 0)
        {
            return false;
        }
        model.PrevSuccess = 0;
        int numStats = NumStats;
        int i = numStats - 1;
        while ((hiCnt += p.IncrementAddress().Freq) <= count)
        {
            if (--i == 0)
            {
                model.HiBitsFlag = model.GetHb2Flag()[model.FoundState.Symbol];
                coder.SubRange.LowCount = hiCnt;
                model.CharMask[p.Symbol] = model.EscCount;
                model.NumMasked = numStats;
                i = numStats - 1;
                model.FoundState.Address = 0;
                do
                {
                    model.CharMask[p.DecrementAddress().Symbol] = model.EscCount;
                } while (--i != 0);
                coder.SubRange.HighCount = coder.SubRange.Scale;
                return true;
            }
        }
        coder.SubRange.LowCount = hiCnt - p.Freq;
        coder.SubRange.HighCount = hiCnt;
        Update1(model, p.Address);
        return true;
    }

    static PpmContext() => UNION_SIZE = Math.Max(FreqData.SIZE, State.SIZE);
}

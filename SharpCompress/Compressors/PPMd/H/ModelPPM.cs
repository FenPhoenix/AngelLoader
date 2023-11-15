#nullable disable

using System;
using System.Text;
using SharpCompress.Compressors.Rar;

namespace SharpCompress.Compressors.PPMd.H;

internal sealed class ModelPpm
{
    private void InitBlock()
    {
        for (var i = 0; i < 25; i++)
        {
            _see2Cont[i] = new See2Context[16];
        }
        for (var i2 = 0; i2 < 128; i2++)
        {
            _binSumm[i2] = new int[64];
        }
    }

    public SubAllocator SubAlloc { get; } = new SubAllocator();

    public See2Context DummySee2Cont => _dummySee2Cont;

    public int InitRl => _initRl;

    public int EscCount
    {
        get => _escCount;
        set => _escCount = value & 0xff;
    }

    public int[] CharMask => _charMask;

    public int NumMasked
    {
        get => _numMasked;
        set => _numMasked = value;
    }

    public int PrevSuccess
    {
        get => _prevSuccess;
        set => _prevSuccess = value & 0xff;
    }

    public int InitEsc
    {
        set => _initEsc = value;
    }

    public int RunLength
    {
        get => _runLength;
        set => _runLength = value;
    }

    public int HiBitsFlag
    {
        get => _hiBitsFlag;
        set => _hiBitsFlag = value & 0xff;
    }

    public int[][] BinSumm => _binSumm;

    internal RangeCoder Coder { get; private set; }

    internal State FoundState { get; private set; }

    public byte[] Heap => SubAlloc.Heap;

    public int OrderFall => _orderFall;

    public const int MAX_O = 64; /* maximum allowed model order */

    public const int INT_BITS = 7;

    public const int PERIOD_BITS = 7;

    //UPGRADE_NOTE: Final was removed from the declaration of 'TOT_BITS '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    public static readonly int TOT_BITS = INT_BITS + PERIOD_BITS;

    //UPGRADE_NOTE: Final was removed from the declaration of 'INTERVAL '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    public static readonly int INTERVAL = 1 << INT_BITS;

    //UPGRADE_NOTE: Final was removed from the declaration of 'BIN_SCALE '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    public static readonly int BIN_SCALE = 1 << TOT_BITS;

    public const int MAX_FREQ = 124;

    private readonly See2Context[][] _see2Cont = new See2Context[25][];

    private See2Context _dummySee2Cont;

    private PpmContext _minContext; //medContext

    private PpmContext _maxContext;

    private int _numMasked,
        _initEsc,
        _orderFall,
        _maxOrder,
        _runLength,
        _initRl;

    private readonly int[] _charMask = new int[256];

    private readonly int[] _ns2Indx = new int[256];

    private readonly int[] _ns2BsIndx = new int[256];

    private readonly int[] _hb2Flag = new int[256];

    // byte EscCount, PrevSuccess, HiBitsFlag;
    private int _escCount,
        _prevSuccess,
        _hiBitsFlag;

    private readonly int[][] _binSumm = new int[128][]; // binary SEE-contexts

    private static readonly int[] INIT_BIN_ESC =
    {
        0x3CDD,
        0x1F3F,
        0x59BF,
        0x48F3,
        0x64A1,
        0x5ABC,
        0x6632,
        0x6051
    };

    // Temp fields
    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState1 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState1 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState2 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState2 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState3 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState3 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempState4 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly State _tempState4 = new State(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempStateRef1 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly StateRef _tempStateRef1 = new StateRef();

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempStateRef2 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly StateRef _tempStateRef2 = new StateRef();

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempPPMContext1 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly PpmContext _tempPpmContext1 = new PpmContext(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempPPMContext2 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly PpmContext _tempPpmContext2 = new PpmContext(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempPPMContext3 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly PpmContext _tempPpmContext3 = new PpmContext(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'tempPPMContext4 '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly PpmContext _tempPpmContext4 = new PpmContext(null);

    //UPGRADE_NOTE: Final was removed from the declaration of 'ps '. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1003'"
    private readonly int[] _ps = new int[MAX_O];

    public ModelPpm()
    {
        InitBlock();
        _minContext = null;
        _maxContext = null;

        //medContext = null;
    }

    private void RestartModelRare()
    {
        new Span<int>(_charMask).Clear();
        SubAlloc.InitSubAllocator();
        _initRl = -(_maxOrder < 12 ? _maxOrder : 12) - 1;
        var addr = SubAlloc.AllocContext();
        _minContext.Address = addr;
        _maxContext.Address = addr;
        _minContext.SetSuffix(0);
        _orderFall = _maxOrder;
        _minContext.NumStats = 256;
        _minContext.FreqData.SummFreq = _minContext.NumStats + 1;

        addr = SubAlloc.AllocUnits(256 / 2);
        FoundState.Address = addr;
        _minContext.FreqData.SetStats(addr);

        var state = new State(SubAlloc.Heap);
        addr = _minContext.FreqData.GetStats();
        _runLength = _initRl;
        _prevSuccess = 0;
        for (var i = 0; i < 256; i++)
        {
            state.Address = addr + (i * State.SIZE);
            state.Symbol = i;
            state.Freq = 1;
            state.SetSuccessor(0);
        }

        for (var i = 0; i < 128; i++)
        {
            for (var k = 0; k < 8; k++)
            {
                for (var m = 0; m < 64; m += 8)
                {
                    _binSumm[i][k + m] = BIN_SCALE - (INIT_BIN_ESC[k] / (i + 2));
                }
            }
        }
        for (var i = 0; i < 25; i++)
        {
            for (var k = 0; k < 16; k++)
            {
                _see2Cont[i][k].Initialize((5 * i) + 10);
            }
        }
    }

    private void StartModelRare(int maxOrder)
    {
        int i,
            k,
            m,
            step;
        _escCount = 1;
        _maxOrder = maxOrder;
        RestartModelRare();

        // Bug Fixed
        _ns2BsIndx[0] = 0;
        _ns2BsIndx[1] = 2;
        for (var j = 0; j < 9; j++)
        {
            _ns2BsIndx[2 + j] = 4;
        }
        for (var j = 0; j < 256 - 11; j++)
        {
            _ns2BsIndx[11 + j] = 6;
        }
        for (i = 0; i < 3; i++)
        {
            _ns2Indx[i] = i;
        }
        for (m = i, k = 1, step = 1; i < 256; i++)
        {
            _ns2Indx[i] = m;
            if ((--k) == 0)
            {
                k = ++step;
                m++;
            }
        }
        for (var j = 0; j < 0x40; j++)
        {
            _hb2Flag[j] = 0;
        }
        for (var j = 0; j < 0x100 - 0x40; j++)
        {
            _hb2Flag[0x40 + j] = 0x08;
        }
        _dummySee2Cont.Shift = PERIOD_BITS;
    }

    private void ClearMask()
    {
        _escCount = 1;
        new Span<int>(_charMask).Clear();
    }

    internal bool DecodeInit(IRarUnpack unpackRead)
    {
        var maxOrder = unpackRead.Char & 0xff;
        var reset = ((maxOrder & 0x20) != 0);

        var maxMb = 0;
        if (reset)
        {
            maxMb = unpackRead.Char;
        }
        else
        {
            if (SubAlloc.GetAllocatedMemory() == 0)
            {
                return (false);
            }
        }
        if ((maxOrder & 0x40) != 0)
        {
        }
        Coder = new RangeCoder(unpackRead);
        if (reset)
        {
            maxOrder = (maxOrder & 0x1f) + 1;
            if (maxOrder > 16)
            {
                maxOrder = 16 + ((maxOrder - 16) * 3);
            }
            if (maxOrder == 1)
            {
                SubAlloc.StopSubAllocator();
                return (false);
            }
            SubAlloc.StartSubAllocator((maxMb + 1) << 20);
            _minContext = new PpmContext(Heap);

            //medContext = new PPMContext(Heap);
            _maxContext = new PpmContext(Heap);
            FoundState = new State(Heap);
            _dummySee2Cont = new See2Context();
            for (var i = 0; i < 25; i++)
            {
                for (var j = 0; j < 16; j++)
                {
                    _see2Cont[i][j] = new See2Context();
                }
            }
            StartModelRare(maxOrder);
        }
        return (_minContext.Address != 0);
    }

    public int DecodeChar()
    {
        // Debug
        //subAlloc.dumpHeap();

        if (_minContext.Address <= SubAlloc.PText || _minContext.Address > SubAlloc.HeapEnd)
        {
            return (-1);
        }

        if (_minContext.NumStats != 1)
        {
            if (
                _minContext.FreqData.GetStats() <= SubAlloc.PText
                || _minContext.FreqData.GetStats() > SubAlloc.HeapEnd
            )
            {
                return (-1);
            }
            if (!_minContext.DecodeSymbol1(this))
            {
                return (-1);
            }
        }
        else
        {
            _minContext.DecodeBinSymbol(this);
        }
        Coder.Decode();
        while (FoundState.Address == 0)
        {
            Coder.AriDecNormalize();
            do
            {
                _orderFall++;
                _minContext.Address = _minContext.GetSuffix(); // =MinContext->Suffix;
                if (_minContext.Address <= SubAlloc.PText || _minContext.Address > SubAlloc.HeapEnd)
                {
                    return (-1);
                }
            } while (_minContext.NumStats == _numMasked);
            if (!_minContext.DecodeSymbol2(this))
            {
                return (-1);
            }
            Coder.Decode();
        }
        var symbol = FoundState.Symbol;
        if ((_orderFall == 0) && FoundState.GetSuccessor() > SubAlloc.PText)
        {
            // MinContext=MaxContext=FoundState->Successor;
            var addr = FoundState.GetSuccessor();
            _minContext.Address = addr;
            _maxContext.Address = addr;
        }
        else
        {
            UpdateModel();

            //this.foundState.Address=foundState.Address);//TODO just 4 debugging
            if (_escCount == 0)
            {
                ClearMask();
            }
        }
        Coder.AriDecNormalize(); // ARI_DEC_NORMALIZE(Coder.code,Coder.low,Coder.range,Coder.UnpackRead);
        return (symbol);
    }

    public See2Context[][] GetSee2Cont() => _see2Cont;

    public void IncEscCount(int dEscCount) => EscCount += dEscCount;

    public void IncRunLength(int dRunLength) => RunLength += dRunLength;

    public int[] GetHb2Flag() => _hb2Flag;

    public int[] GetNs2BsIndx() => _ns2BsIndx;

    public int[] GetNs2Indx() => _ns2Indx;

    private int CreateSuccessors(bool skip, State p1)
    {
        //State upState = tempState1.Initialize(null);
        var upState = _tempStateRef2;
        var tempState = _tempState1.Initialize(Heap);

        // PPM_CONTEXT* pc=MinContext, * UpBranch=FoundState->Successor;
        var pc = _tempPpmContext1.Initialize(Heap);
        pc.Address = _minContext.Address;
        var upBranch = _tempPpmContext2.Initialize(Heap);
        upBranch.Address = FoundState.GetSuccessor();

        // STATE * p, * ps[MAX_O], ** pps=ps;
        var p = _tempState2.Initialize(Heap);
        var pps = 0;

        var noLoop = false;

        if (!skip)
        {
            _ps[pps++] = FoundState.Address; // *pps++ = FoundState;
            if (pc.GetSuffix() == 0)
            {
                noLoop = true;
            }
        }
        if (!noLoop)
        {
            var loopEntry = false;
            if (p1.Address != 0)
            {
                p.Address = p1.Address;
                pc.Address = pc.GetSuffix(); // =pc->Suffix;
                loopEntry = true;
            }
            do
            {
                if (!loopEntry)
                {
                    pc.Address = pc.GetSuffix(); // pc=pc->Suffix;
                    if (pc.NumStats != 1)
                    {
                        p.Address = pc.FreqData.GetStats(); // p=pc->U.Stats
                        if (p.Symbol != FoundState.Symbol)
                        {
                            do
                            {
                                p.IncrementAddress();
                            } while (p.Symbol != FoundState.Symbol);
                        }
                    }
                    else
                    {
                        p.Address = pc.GetOneState().Address; // p=&(pc->OneState);
                    }
                } // LOOP_ENTRY:
                loopEntry = false;
                if (p.GetSuccessor() != upBranch.Address)
                {
                    pc.Address = p.GetSuccessor(); // =p->Successor;
                    break;
                }
                _ps[pps++] = p.Address;
            } while (pc.GetSuffix() != 0);
        } // NO_LOOP:
        if (pps == 0)
        {
            return pc.Address;
        }
        upState.Symbol = Heap[upBranch.Address]; // UpState.Symbol=*(byte*)

        // UpBranch;
        // UpState.Successor=(PPM_CONTEXT*) (((byte*) UpBranch)+1);
        upState.SetSuccessor(upBranch.Address + 1); //TODO check if +1 necessary
        if (pc.NumStats != 1)
        {
            if (pc.Address <= SubAlloc.PText)
            {
                return (0);
            }
            p.Address = pc.FreqData.GetStats();
            if (p.Symbol != upState.Symbol)
            {
                do
                {
                    p.IncrementAddress();
                } while (p.Symbol != upState.Symbol);
            }
            var cf = p.Freq - 1;
            var s0 = pc.FreqData.SummFreq - pc.NumStats - cf;

            // UpState.Freq=1+((2*cf <= s0)?(5*cf > s0):((2*cf+3*s0-1)/(2*s0)));
            upState.Freq =
                1
                + ((2 * cf <= s0) ? (5 * cf > s0 ? 1 : 0) : (((2 * cf) + (3 * s0) - 1) / (2 * s0)));
        }
        else
        {
            upState.Freq = pc.GetOneState().Freq; // UpState.Freq=pc->OneState.Freq;
        }
        do
        {
            // pc = pc->createChild(this,*--pps,UpState);
            tempState.Address = _ps[--pps];
            pc.Address = pc.CreateChild(this, tempState, upState);
            if (pc.Address == 0)
            {
                return 0;
            }
        } while (pps != 0);
        return pc.Address;
    }

    private void UpdateModelRestart()
    {
        RestartModelRare();
        _escCount = 0;
    }

    private void UpdateModel()
    {
        //System.out.println("ModelPPM.updateModel()");
        // STATE fs = *FoundState, *p = NULL;
        var fs = _tempStateRef1;
        fs.Values = FoundState;
        var p = _tempState3.Initialize(Heap);
        var tempState = _tempState4.Initialize(Heap);

        var pc = _tempPpmContext3.Initialize(Heap);
        var successor = _tempPpmContext4.Initialize(Heap);

        int ns1,
            ns,
            cf,
            sf,
            s0;
        pc.Address = _minContext.GetSuffix();
        if (fs.Freq < MAX_FREQ / 4 && pc.Address != 0)
        {
            if (pc.NumStats != 1)
            {
                p.Address = pc.FreqData.GetStats();
                if (p.Symbol != fs.Symbol)
                {
                    do
                    {
                        p.IncrementAddress();
                    } while (p.Symbol != fs.Symbol);
                    tempState.Address = p.Address - State.SIZE;
                    if (p.Freq >= tempState.Freq)
                    {
                        State.PpmdSwap(p, tempState);
                        p.DecrementAddress();
                    }
                }
                if (p.Freq < MAX_FREQ - 9)
                {
                    p.IncrementFreq(2);
                    pc.FreqData.IncrementSummFreq(2);
                }
            }
            else
            {
                p.Address = pc.GetOneState().Address;
                if (p.Freq < 32)
                {
                    p.IncrementFreq(1);
                }
            }
        }
        if (_orderFall == 0)
        {
            FoundState.SetSuccessor(CreateSuccessors(true, p));
            _minContext.Address = FoundState.GetSuccessor();
            _maxContext.Address = FoundState.GetSuccessor();
            if (_minContext.Address == 0)
            {
                UpdateModelRestart();
                return;
            }
            return;
        }
        SubAlloc.Heap[SubAlloc.PText] = (byte)fs.Symbol;
        SubAlloc.IncPText();
        successor.Address = SubAlloc.PText;
        if (SubAlloc.PText >= SubAlloc.FakeUnitsStart)
        {
            UpdateModelRestart();
            return;
        }

        //        // Debug
        //        subAlloc.dumpHeap();
        if (fs.GetSuccessor() != 0)
        {
            if (fs.GetSuccessor() <= SubAlloc.PText)
            {
                fs.SetSuccessor(CreateSuccessors(false, p));
                if (fs.GetSuccessor() == 0)
                {
                    UpdateModelRestart();
                    return;
                }
            }
            if (--_orderFall == 0)
            {
                successor.Address = fs.GetSuccessor();
                if (_maxContext.Address != _minContext.Address)
                {
                    SubAlloc.DecPText(1);
                }
            }
        }
        else
        {
            FoundState.SetSuccessor(successor.Address);
            fs.SetSuccessor(_minContext);
        }

        //        // Debug
        //        subAlloc.dumpHeap();
        ns = _minContext.NumStats;
        s0 = _minContext.FreqData.SummFreq - (ns) - (fs.Freq - 1);
        for (
            pc.Address = _maxContext.Address;
            pc.Address != _minContext.Address;
            pc.Address = pc.GetSuffix()
        )
        {
            if ((ns1 = pc.NumStats) != 1)
            {
                if ((ns1 & 1) == 0)
                {
                    //System.out.println(ns1);
                    pc.FreqData.SetStats(
                        SubAlloc.ExpandUnits(pc.FreqData.GetStats(), Utility.URShift(ns1, 1))
                    );
                    if (pc.FreqData.GetStats() == 0)
                    {
                        UpdateModelRestart();
                        return;
                    }
                }

                // bug fixed
                //				int sum = ((2 * ns1 < ns) ? 1 : 0) +
                //                        2 * ((4 * ((ns1 <= ns) ? 1 : 0)) & ((pc.getFreqData()
                //								.getSummFreq() <= 8 * ns1) ? 1 : 0));
                var sum =
                    ((2 * ns1 < ns) ? 1 : 0)
                    + (
                        2
                        * (((4 * ns1 <= ns) ? 1 : 0) & ((pc.FreqData.SummFreq <= 8 * ns1) ? 1 : 0))
                    );
                pc.FreqData.IncrementSummFreq(sum);
            }
            else
            {
                p.Address = SubAlloc.AllocUnits(1);
                if (p.Address == 0)
                {
                    UpdateModelRestart();
                    return;
                }
                p.SetValues(pc.GetOneState());
                pc.FreqData.SetStats(p);
                if (p.Freq < (MAX_FREQ / 4) - 1)
                {
                    p.IncrementFreq(p.Freq);
                }
                else
                {
                    p.Freq = MAX_FREQ - 4;
                }
                pc.FreqData.SummFreq = (p.Freq + _initEsc + (ns > 3 ? 1 : 0));
            }
            cf = 2 * fs.Freq * (pc.FreqData.SummFreq + 6);
            sf = s0 + pc.FreqData.SummFreq;
            if (cf < 6 * sf)
            {
                cf = 1 + (cf > sf ? 1 : 0) + (cf >= 4 * sf ? 1 : 0);
                pc.FreqData.IncrementSummFreq(3);
            }
            else
            {
                cf = 4 + (cf >= 9 * sf ? 1 : 0) + (cf >= 12 * sf ? 1 : 0) + (cf >= 15 * sf ? 1 : 0);
                pc.FreqData.IncrementSummFreq(cf);
            }
            p.Address = pc.FreqData.GetStats() + (ns1 * State.SIZE);
            p.SetSuccessor(successor);
            p.Symbol = fs.Symbol;
            p.Freq = cf;
            pc.NumStats = ++ns1;
        }

        var address = fs.GetSuccessor();
        _maxContext.Address = address;
        _minContext.Address = address;
    }
}

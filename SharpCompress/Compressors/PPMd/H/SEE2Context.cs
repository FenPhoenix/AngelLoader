namespace SharpCompress_7z.Compressors.PPMd.H;

internal sealed class See2Context
{
    public int Mean
    {
        get
        {
            var retVal = (_summ >>> Shift);
            _summ -= retVal;
            return retVal + ((retVal == 0) ? 1 : 0);
        }
    }

    public int Summ
    {
        get => _summ;
        set => _summ = value & 0xffff;
    }

    // ushort Summ;
    private int _summ;

    // byte Shift;
    internal int Shift;

    // byte Count;
    private int _count;

    public void Initialize(int initVal)
    {
        Shift = (ModelPpm.PERIOD_BITS - 4) & 0xff;
        _summ = (initVal << Shift) & 0xffff;
        _count = 4;
    }

    public void Update()
    {
        if (Shift < ModelPpm.PERIOD_BITS && --_count == 0)
        {
            _summ += _summ;
            _count = (3 << Shift++);
        }
        _summ &= 0xffff;
        _count &= 0xff;
        Shift &= 0xff;
    }

    public void IncSumm(int dSumm) => Summ += dSumm;
}

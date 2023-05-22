namespace SharpCompress.Compressors.PPMd.H;

internal sealed class See2Context
{
    public int Mean
    {
        get
        {
            int retVal = _summ >>> _shift;
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
    private int _shift;

    // byte Count;
    private int _count;

    public void Initialize(int initVal)
    {
        _shift = (ModelPpm.PERIOD_BITS - 4) & 0xff;
        _summ = (initVal << _shift) & 0xffff;
        _count = 4;
    }

    public void Update()
    {
        if (_shift < ModelPpm.PERIOD_BITS && --_count == 0)
        {
            _summ += _summ;
            _count = (3 << _shift++);
        }
        _summ &= 0xffff;
        _count &= 0xff;
        _shift &= 0xff;
    }

    public void IncSumm(int dSumm) => Summ += dSumm;
}

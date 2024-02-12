using System.Text;

namespace SharpCompress.Compressors.PPMd.H;

internal sealed class See2Context
{
    public int Mean
    {
        get
        {
            var retVal = (_summ >>> _shift);
            _summ -= retVal;
            return retVal + ((retVal == 0) ? 1 : 0);
        }
    }

    public int Shift
    {
        get => _shift;
        set => _shift = value & 0xff;
    }

    public int Summ
    {
        get => _summ;
        set => _summ = value & 0xffff;
    }

    private const int SIZE = 4;

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

    public override string ToString()
    {
        var buffer = new StringBuilder();
        buffer.Append("SEE2Context[");
        buffer.Append("\n  size=");
        buffer.Append(SIZE);
        buffer.Append("\n  summ=");
        buffer.Append(_summ);
        buffer.Append("\n  shift=");
        buffer.Append(_shift);
        buffer.Append("\n  count=");
        buffer.Append(_count);
        buffer.Append("\n]");
        return buffer.ToString();
    }
}

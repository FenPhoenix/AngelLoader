namespace SharpCompress.Compressors.PPMd.H;

internal sealed class StateRef
{
    private int _symbol;

    private int _freq;

    private int _successor; // pointer ppmcontext

    internal int Symbol
    {
        get => _symbol;
        set => _symbol = value & 0xff;
    }

    internal int Freq
    {
        get => _freq;
        set => _freq = value & 0xff;
    }

    internal State Values
    {
        set
        {
            Freq = value.Freq;
            SetSuccessor(value.GetSuccessor());
            Symbol = value.Symbol;
        }
    }

    public void DecrementFreq(int dFreq) => _freq = (_freq - dFreq) & 0xff;

    public int GetSuccessor() => _successor;

    public void SetSuccessor(PpmContext successor) => SetSuccessor(successor.Address);

    public void SetSuccessor(int successor) => _successor = successor;
}

namespace SharpCompress_7z.Common.SevenZip;

internal readonly struct CBindPair
{
    internal readonly int InIndex;
    internal readonly int OutIndex;

    public CBindPair(int inIndex, int outIndex)
    {
        InIndex = inIndex;
        OutIndex = outIndex;
    }
}

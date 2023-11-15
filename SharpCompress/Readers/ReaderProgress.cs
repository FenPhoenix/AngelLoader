using System;
using SharpCompress.Common;
using SharpCompress.Common.Rar;

namespace SharpCompress.Readers;

public sealed class ReaderProgress
{
    private readonly RarEntry _entry;
    public long BytesTransferred { get; }
    public int Iterations { get; }

    public int PercentageRead => (int)Math.Round(PercentageReadExact);
    public double PercentageReadExact => (float)BytesTransferred / _entry.Size * 100;

    public ReaderProgress(RarEntry entry, long bytesTransferred, int iterations)
    {
        _entry = entry;
        BytesTransferred = bytesTransferred;
        Iterations = iterations;
    }
}

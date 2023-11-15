using System;
using SharpCompress_7z.Readers;

namespace SharpCompress_7z.Common;

public sealed class ReaderExtractionEventArgs<T> : EventArgs
{
    internal ReaderExtractionEventArgs(T entry, ReaderProgress? readerProgress = null)
    {
        Item = entry;
        ReaderProgress = readerProgress;
    }

    public T Item { get; }

    public ReaderProgress? ReaderProgress { get; }
}

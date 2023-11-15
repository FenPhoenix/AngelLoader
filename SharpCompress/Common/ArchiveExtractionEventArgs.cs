using System;

namespace SharpCompress_7z.Common;

public class ArchiveExtractionEventArgs<T> : EventArgs
{
    internal ArchiveExtractionEventArgs(T entry) => Item = entry;

    public T Item { get; }
}

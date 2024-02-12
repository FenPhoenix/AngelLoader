using System.Collections.Generic;

namespace SharpCompress.Common;

public abstract class Entry : IEntry
{
    /// <summary>
    /// The string key of the file internal to the Archive.
    /// </summary>
    public abstract string Key { get; }

    /// <summary>
    /// The compressed file size
    /// </summary>
    public abstract long CompressedSize { get; }

    /// <summary>
    /// The uncompressed file size
    /// </summary>
    public abstract long Size { get; }

    /// <summary>
    /// Entry is directory.
    /// </summary>
    public abstract bool IsDirectory { get; }

    /// <inheritdoc/>
    public override string ToString() => Key;

    internal abstract IEnumerable<FilePart> Parts { get; }

    public bool IsSolid { get; protected set; }
}

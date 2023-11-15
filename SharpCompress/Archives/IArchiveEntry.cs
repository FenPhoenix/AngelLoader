using SharpCompress.Common;

namespace SharpCompress.Archives;

public interface IArchiveEntry
{
    /// <summary>
    /// The archive can find all the parts of the archive needed to extract this entry.
    /// </summary>
    bool IsComplete { get; }
    long CompressedSize { get; }
    long Size { get; }
}

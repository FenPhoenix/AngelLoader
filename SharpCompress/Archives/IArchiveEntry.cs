namespace SharpCompress.Archives;

public interface IArchiveEntry
{
    long CompressedSize { get; }
    long Size { get; }
}

namespace SharpCompress.Common;

public interface IEntry
{
    long CompressedSize { get; }
    long Size { get; }
}

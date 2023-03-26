#nullable disable

namespace SharpCompress.Common.SevenZip;

internal sealed class CFileItem
{
    public long Size { get; internal set; }

    public string Name { get; internal set; }

    public bool HasStream { get; internal set; }

    public bool IsDir { get; internal set; }

    public long? MTime { get; internal set; }

    public bool IsAnti { get; internal set; }

    internal CFileItem() => HasStream = true;
}

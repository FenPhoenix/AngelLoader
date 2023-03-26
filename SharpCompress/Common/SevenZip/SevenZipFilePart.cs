namespace SharpCompress.Common.SevenZip;

internal sealed class SevenZipFilePart
{
    internal SevenZipFilePart(CFileItem fileEntry) => Header = fileEntry;

    internal CFileItem Header { get; }
}

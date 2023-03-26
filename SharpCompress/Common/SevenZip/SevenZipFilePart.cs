namespace SharpCompress.Common.SevenZip;

internal sealed class SevenZipFilePart
{
    internal SevenZipFilePart(ArchiveDatabase database,
        int index,
        CFileItem fileEntry
    )
    {
        Header = fileEntry;
        if (Header.HasStream)
        {
            Folder = database._folders[database._fileIndexToFolderIndexMap[index]];
        }
    }

    internal CFileItem Header { get; }

    internal CFolder? Folder { get; }
}

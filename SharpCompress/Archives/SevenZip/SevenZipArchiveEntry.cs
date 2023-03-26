using System;
using SharpCompress.Common.SevenZip;
using SharpCompress.Compressors.LZMA.Utilities;

namespace SharpCompress.Archives.SevenZip;

public sealed class SevenZipArchiveEntry
{
    private readonly CFileItem FilePart;

    internal SevenZipArchiveEntry(CFileItem part) => FilePart = part;

    /// <summary>
    /// This is a 7Zip Anti item
    /// </summary>
    public bool IsAnti => FilePart.IsAnti;

    public string FileName => FilePart.Name;

    public long UncompressedSize => FilePart.Size;

    // Because null can be a valid value, we need an extra flag bool
    private bool _lastModifiedTimeSet;
    private DateTime? _lastModifiedTime;
    public DateTime? LastModifiedTime
    {
        get
        {
            if (!_lastModifiedTimeSet)
            {
                _lastModifiedTime = Utils.TranslateTime(FilePart.MTime);
                _lastModifiedTimeSet = true;
            }

            return _lastModifiedTime;
        }
    }

    public bool IsDirectory => FilePart.IsDir;
}

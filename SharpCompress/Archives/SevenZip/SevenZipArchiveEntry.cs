using System;
using SharpCompress.Common.SevenZip;
using SharpCompress.Compressors.LZMA.Utilites;

namespace SharpCompress.Archives.SevenZip;

public sealed class SevenZipArchiveEntry
{
    internal SevenZipArchiveEntry(SevenZipFilePart part)
    {
        FilePart = part;
    }

    /// <summary>
    /// This is a 7Zip Anti item
    /// </summary>
    public bool IsAnti => FilePart.Header.IsAnti;

    private SevenZipFilePart FilePart { get; }

    public string FileName => FilePart.Header.Name;

    public long UncompressedSize => FilePart.Header.Size;

    // Because null can be a valid value, we need an extra flag bool
    private bool _lastModifiedTimeSet;
    private DateTime? _lastModifiedTime;
    public DateTime? LastModifiedTime
    {
        get
        {
            if (!_lastModifiedTimeSet)
            {
                _lastModifiedTime = Utils.TranslateTime(FilePart.Header.MTime);
                _lastModifiedTimeSet = true;
            }

            return _lastModifiedTime;
        }
    }

    public bool IsDirectory => FilePart.Header.IsDir;
}

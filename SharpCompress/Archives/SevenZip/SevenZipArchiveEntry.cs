using System;

namespace SharpCompress.Archives.SevenZip;

public sealed class SevenZipArchiveEntry
{
    internal bool HasStream = true;
    internal ulong MTime;

    /// <summary>
    /// This is a 7Zip Anti item
    /// </summary>
    public bool IsAnti;

    public string FileName = "";

    public long UncompressedSize;

    // Because null can be a valid value, we need an extra flag bool
    private bool _lastModifiedTimeSet;
    private DateTime? _lastModifiedTime;
    public DateTime? LastModifiedTime
    {
        get
        {
            if (!_lastModifiedTimeSet)
            {
                _lastModifiedTime = Utility.TranslateTime(MTime);
                _lastModifiedTimeSet = true;
            }

            return _lastModifiedTime;
        }
    }

    public bool IsDirectory;
}

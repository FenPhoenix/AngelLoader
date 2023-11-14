﻿using System;
using System.Runtime.CompilerServices;

namespace SharpCompress_7z.Archives.SevenZip;

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
                //maximum Windows file time 31.12.9999
                _lastModifiedTime = MTime <= 2_650_467_743_999_999_999
                    ? DateTime.FromFileTimeUtc((long)MTime).ToLocalTime()
                    : null;
                _lastModifiedTimeSet = true;
            }

            return _lastModifiedTime;
        }
    }

    public bool IsDirectory;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Reset()
    {
        HasStream = true;
        MTime = 0;
        IsAnti = false;
        FileName = "";
        UncompressedSize = 0;
        _lastModifiedTimeSet = false;
        _lastModifiedTime = null;
        IsDirectory = false;
    }
}

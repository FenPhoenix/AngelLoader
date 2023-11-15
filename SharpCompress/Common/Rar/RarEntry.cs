using System;
using System.Collections.Generic;
using SharpCompress.Common.Rar.Headers;

namespace SharpCompress.Common.Rar;

public abstract class RarEntry
{
    internal abstract FileHeader FileHeader { get; }

    /// <summary>
    /// As the V2017 port isn't complete, add this check to use the legacy Rar code.
    /// </summary>
    internal bool IsRarV3 =>
        FileHeader.CompressionAlgorithm == 15
        || FileHeader.CompressionAlgorithm == 20
        || FileHeader.CompressionAlgorithm == 26
        || FileHeader.CompressionAlgorithm == 29
        || FileHeader.CompressionAlgorithm == 36; //Nanook - Added 20+26 as Test arc from WinRar2.8 (algo 20) was failing with 2017 code

    /// <summary>
    /// The path of the file internal to the Rar Archive.
    /// </summary>
    public string Key => FileHeader.FileName;

    /// <summary>
    /// The entry last modified time in the archive, if recorded
    /// </summary>
    public DateTime? LastModifiedTime => FileHeader.FileLastModifiedTime;

    /// <summary>
    /// Entry is password protected and encrypted and cannot be extracted.
    /// </summary>
    public bool IsDirectory => FileHeader.IsDirectory;

    public bool IsSolid { get; set; }

    /// <summary>
    /// The compressed file size
    /// </summary>
    public abstract long CompressedSize { get; }

    internal virtual IEnumerable<FilePart> Parts { get; }
}

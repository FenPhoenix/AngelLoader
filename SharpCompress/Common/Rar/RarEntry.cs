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
    /// The File's 32 bit CRC Hash
    /// </summary>
    public long Crc => FileHeader.FileCrc;

    /// <summary>
    /// The path of the file internal to the Rar Archive.
    /// </summary>
    public string Key => FileHeader.FileName;

    public string? LinkTarget => null;

    /// <summary>
    /// The entry last modified time in the archive, if recorded
    /// </summary>
    public DateTime? LastModifiedTime => FileHeader.FileLastModifiedTime;

    /// <summary>
    /// The entry create time in the archive, if recorded
    /// </summary>
    public DateTime? CreatedTime => FileHeader.FileCreatedTime;

    /// <summary>
    /// The entry last accessed time in the archive, if recorded
    /// </summary>
    public DateTime? LastAccessedTime => FileHeader.FileLastAccessedTime;

    /// <summary>
    /// The entry time whend archived, if recorded
    /// </summary>
    public DateTime? ArchivedTime => FileHeader.FileArchivedTime;

    /// <summary>
    /// Entry is password protected and encrypted and cannot be extracted.
    /// </summary>
    public bool IsEncrypted => FileHeader.IsEncrypted;

    /// <summary>
    /// Entry is password protected and encrypted and cannot be extracted.
    /// </summary>
    public bool IsDirectory => FileHeader.IsDirectory;

    public bool IsSplitAfter => FileHeader.IsSplitAfter;

    public bool IsSolid { get; set; }

    /// <summary>
    /// The compressed file size
    /// </summary>
    public abstract long CompressedSize { get; }

    /// <summary>
    /// The uncompressed file size.
    /// </summary>
    public virtual long Size { get; }

    internal virtual IEnumerable<FilePart> Parts { get; }
}

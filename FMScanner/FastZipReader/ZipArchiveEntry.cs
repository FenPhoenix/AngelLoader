// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using AL_Common;
using JetBrains.Annotations;

namespace FMScanner.FastZipReader;

[PublicAPI]
public sealed class ZipArchiveEntry
{
    #region Fields

    internal long OffsetOfLocalHeader;
    internal ZipArchiveFast.CompressionMethodValues CompressionMethod;
    internal long? StoredOffsetOfCompressedData;

    #endregion

    #region Properties

    /// <summary>
    /// The compressed size of the entry.
    /// </summary>
    public long CompressedLength;

    /// <summary>
    /// The last write time of the entry as stored in the Zip archive. To convert to a DateTime object, use
    /// <see cref="ZipHelpers.ZipTimeToDateTime"/>.
    /// </summary>
    public uint LastWriteTime;

    /// <summary>
    /// The uncompressed size of the entry.
    /// </summary>
    public long Length;

    /// <summary>
    /// The relative path of the entry as stored in the Zip archive. Note that Zip archives allow any string
    /// to be the path of the entry, including invalid and absolute paths.
    /// </summary>
    public string FullName;

    #endregion

    internal ZipArchiveEntry(ZipCentralDirectoryFileHeader cd) => Set(cd);

    [MemberNotNull(nameof(FullName))]
    internal void Set(ZipCentralDirectoryFileHeader cd)
    {
        CompressionMethod = (ZipArchiveFast.CompressionMethodValues)cd.CompressionMethod;

        // Leave this as a uint and let the caller convert it if it wants (perf optimization)
        LastWriteTime = cd.LastModified;

        CompressedLength = cd.CompressedSize;
        Length = cd.UncompressedSize;
        OffsetOfLocalHeader = cd.RelativeOffsetOfLocalHeader;

        // we don't know this yet: should be _offsetOfLocalHeader + 30 + _storedEntryNameBytes.Length + extrafieldlength
        // but entryname/extra length could be different in LH
        StoredOffsetOfCompressedData = null;

        // Sacrifice a slight amount of time for safety. Zips entry names are emphatically NOT supposed to
        // have backslashes according to the spec, but they might anyway, so normalize them all to forward slashes.
        FullName = Encoding.UTF8.GetString(cd.Filename, 0, cd.FilenameLength).ToForwardSlashes();
        // Turns out we don't even need the Name property, as the only thing we used it for was checking
        // the extension.
    }
}
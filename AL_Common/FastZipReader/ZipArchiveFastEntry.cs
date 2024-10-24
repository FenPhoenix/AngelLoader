// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;

namespace AL_Common.FastZipReader;

[PublicAPI]
public sealed class ZipArchiveFastEntry
{
    #region Fields

    internal long OffsetOfLocalHeader;
    internal CompressionMethodValues CompressionMethod;
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
    // .NET Framework: LastWriteTimes for full set tested identical between ZipArchive and ZipArchiveFast
    // @MT_TASK/@NET5: Test identicality of LastWriteTime with ZipArchive vs ZipArchiveFast with .NET modern
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

    internal ZipArchiveFastEntry(
        ZipCentralDirectoryFileHeader cd,
        Encoding? entryNameEncoding,
        bool useEntryNameEncodingCodePath)
    {
        Set(in cd, entryNameEncoding, useEntryNameEncodingCodePath);
    }

    [MemberNotNull(nameof(FullName))]
    internal void Set(
        in ZipCentralDirectoryFileHeader cd,
        Encoding? entryNameEncoding,
        bool useEntryNameEncodingCodePath)
    {
        CompressionMethod = (CompressionMethodValues)cd.CompressionMethod;

        // Leave this as a uint and let the caller convert it if it wants (perf optimization)
        LastWriteTime = cd.LastModified;

        CompressedLength = cd.CompressedSize;
        Length = cd.UncompressedSize;
        OffsetOfLocalHeader = cd.RelativeOffsetOfLocalHeader;

        // we don't know this yet: should be _offsetOfLocalHeader + 30 + _storedEntryNameBytes.Length + extrafieldlength
        // but entryname/extra length could be different in LH
        StoredOffsetOfCompressedData = null;

        // .NET Framework: Filenames for full set tested identical between ZipArchive and ZipArchiveFast
        // @MT_TASK/@NET5: Test filename identicality for .NET modern
        // @MT_TASK: For .NET modern, ensure we're matching the fixed behavior: https://learn.microsoft.com/en-ca/dotnet/core/compatibility/core-libraries/9.0/ziparchiveentry-encoding
        Encoding finalEncoding;
        if (!useEntryNameEncodingCodePath)
        {
            finalEncoding = Encoding.UTF8;
        }
        else if (((BitFlagValues)cd.GeneralPurposeBitFlag & BitFlagValues.UnicodeFileName) != 0)
        {
            finalEncoding = Encoding.UTF8;
        }
        else
        {
            // @MT_TASK: .NET modern replaces "default" with UTF8:
            // _storedEntryName = (_archive.EntryNameAndCommentEncoding ?? Encoding.UTF8).GetString(_storedEntryNameBytes);
            // Encoding.GetEncoding(0) is the same as Encoding.Default
            finalEncoding = entryNameEncoding ?? Encoding.Default;
        }

        // Sacrifice a slight amount of time for safety. Zip entry names are emphatically NOT supposed to have
        // backslashes according to the spec, but they might anyway, so normalize them all to forward slashes.
        FullName = finalEncoding.GetString(cd.Filename, 0, cd.FilenameLength).ToForwardSlashes();
    }
}

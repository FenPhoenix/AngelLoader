// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Zip Spec here: http://www.pkware.com/documents/casestudies/APPNOTE.TXT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using AL_Common.FastZipReader.DeflateManaged;
using JetBrains.Annotations;
using static AL_Common.Common;

namespace AL_Common.FastZipReader;

public sealed class ZipArchiveFast : IDisposable
{
    #region Enums

    internal enum CompressionMethodValues : ushort
    {
        Stored = 0,
        Deflate = 8,
        Deflate64 = 9,
        BZip2 = 12,
        LZMA = 14
    }

    #endregion

    // We don't want to bloat the archive entry class with crap that's only there for error checking purposes.
    // Since errors should be the rare case, we'll check for errors as we do the initial read, and just
    // put bad entries in here and check it when we go to open.
    private Dictionary<ZipArchiveFastEntry, string>? _unopenableArchives;
    private Dictionary<ZipArchiveFastEntry, string> UnopenableArchives => _unopenableArchives ??= new Dictionary<ZipArchiveFastEntry, string>();

    // invalid until ReadCentralDirectory
    private long _centralDirectoryStart;

    private bool _isDisposed;
    private long _expectedNumberOfEntries;

    private readonly FileStream _archiveStream;
    public readonly long ArchiveStreamLength;

    // invalid until ReadCentralDirectory
    private uint _numberOfThisDisk;

    private readonly ZipContext _context;

    private readonly bool _disposeContext;

    private readonly bool _allowUnsupportedEntries;

    /// <summary>
    /// Initializes a new instance of ZipArchive on the given stream.
    /// </summary>
    /// <exception cref="ArgumentException">The stream is already closed.</exception>
    /// <exception cref="ArgumentNullException">The stream is null.</exception>
    /// <exception cref="InvalidDataException">The contents of the stream could not be interpreted as a Zip file.</exception>
    /// <param name="stream">The input or output stream.</param>
    /// <param name="allowUnsupportedEntries">
    /// If <see langword="true"/>, entries with unsupported compression methods will only throw when opened.
    /// <br/>
    /// If <see langword="false"/>, the <see cref="T:Entries"/> collection will throw immediately if any
    /// entries with unsupported compression methods are found.
    /// </param>
    public ZipArchiveFast(FileStream stream, bool allowUnsupportedEntries) :
        this(stream, new ZipContext(), disposeContext: true, allowUnsupportedEntries)
    {
    }

    /// <summary>
    /// Initializes a new instance of ZipArchive on the given stream.
    /// </summary>
    /// <exception cref="ArgumentException">The stream is already closed.</exception>
    /// <exception cref="ArgumentNullException">The stream is null.</exception>
    /// <exception cref="InvalidDataException">The contents of the stream could not be interpreted as a Zip file.</exception>
    /// <param name="stream">The input or output stream.</param>
    /// <param name="context"></param>
    /// <param name="allowUnsupportedEntries">
    /// If <see langword="true"/>, entries with unsupported compression methods will only throw when opened.
    /// <br/>
    /// If <see langword="false"/>, the <see cref="T:Entries"/> collection will throw immediately if any
    /// entries with unsupported compression methods are found.
    /// </param>
    [PublicAPI]
    public ZipArchiveFast(FileStream stream, ZipContext context, bool allowUnsupportedEntries) :
        this(stream, context, disposeContext: false, allowUnsupportedEntries)
    {
    }

    [PublicAPI]
    private ZipArchiveFast(FileStream stream, ZipContext context, bool disposeContext, bool allowUnsupportedEntries)
    {
        ArgumentNullException.ThrowIfNull(stream);

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        _allowUnsupportedEntries = allowUnsupportedEntries;

        _disposeContext = disposeContext;

        _context = context;

        // Fen's note: Inlined Init() for nullable detection purposes...
        #region Init

        if (!stream.CanRead)
        {
            ThrowHelper.ReadModeCapabilities();
        }

        _archiveStream = stream;
        ArchiveStreamLength = _archiveStream.Length;

        context.ArchiveSubReadStream.SetSuperStream(_archiveStream);

        ReadEndOfCentralDirectory();

        #endregion
    }

    /// <summary>
    /// Opens the entry in Read mode. The returned stream will be readable, and it may or may not be seekable.
    /// </summary>
    /// <param name="entry">The entry to open.</param>
    /// <returns>A Stream that represents the contents of the entry.</returns>
    /// <exception cref="InvalidDataException">
    /// The entry is missing from the archive or is corrupt and cannot be read.
    /// -or-
    /// The entry has been compressed using a compression method that is not supported.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// The ZipArchive that this entry belongs to has been disposed.
    /// </exception>
    public Stream OpenEntry(ZipArchiveFastEntry entry)
    {
        ThrowIfDisposed();

        if (!IsOpenable(entry, out string message)) ThrowHelper.InvalidData(message);

        // _storedOffsetOfCompressedData will never be null, since we know IsOpenable is true

        _context.ArchiveSubReadStream.Set((long)entry.StoredOffsetOfCompressedData!, entry.CompressedLength);

        return GetDataDecompressor(entry, _context.ArchiveSubReadStream);
    }

    private static Stream GetDataDecompressor(ZipArchiveFastEntry entry, SubReadStream compressedStreamToRead)
    {
        Stream uncompressedStream;
        switch (entry.CompressionMethod)
        {
            case CompressionMethodValues.Deflate:
                uncompressedStream = new DeflateStream(compressedStreamToRead, CompressionMode.Decompress, leaveOpen: true);
                break;
            case CompressionMethodValues.Deflate64:
                // This is always in decompress-only mode
                uncompressedStream = new DeflateManagedStream(compressedStreamToRead, entry.Length);
                break;
            case CompressionMethodValues.Stored:
            default:
                // we can assume that only deflate/deflate64/stored are allowed because we assume that
                // IsOpenable is checked before this function is called
                Debug.Assert(entry.CompressionMethod == CompressionMethodValues.Stored);

                uncompressedStream = compressedStreamToRead;
                break;
        }

        return uncompressedStream;
    }

    private bool IsOpenable(ZipArchiveFastEntry entry, out string message)
    {
        message = "";

        if (_unopenableArchives != null && _unopenableArchives.TryGetValue(entry, out string? result))
        {
            message = result;
            return false;
        }

        if (entry.StoredOffsetOfCompressedData != null)
        {
            _archiveStream.Seek((long)entry.StoredOffsetOfCompressedData, SeekOrigin.Begin);
            return true;
        }

        if (entry.OffsetOfLocalHeader > ArchiveStreamLength)
        {
            message = SR.LocalFileHeaderCorrupt;
            return false;
        }

        _archiveStream.Seek(entry.OffsetOfLocalHeader, SeekOrigin.Begin);
        if (!ZipLocalFileHeader.TrySkipBlock(_archiveStream, ArchiveStreamLength, _context))
        {
            message = SR.LocalFileHeaderCorrupt;
            return false;
        }

        // At this point, this is really just caching a FileStream.Position, which does have some logic in
        // its getter, but probably isn't slow enough to warrant being cached... but I guess ArchiveStream
        // could be any kind of stream, so better to guarantee performance than to hope for it, I guess.
        entry.StoredOffsetOfCompressedData ??= _archiveStream.Position;

        if (entry.StoredOffsetOfCompressedData + entry.CompressedLength > ArchiveStreamLength)
        {
            message = SR.LocalFileHeaderCorrupt;
            return false;
        }

        return true;
    }

    private bool _entriesInitialized;

    /// <summary>
    /// The collection of entries that are currently in the ZipArchive. This may not accurately represent the actual entries that are present in the underlying file or stream.
    /// </summary>
    /// <exception cref="NotSupportedException">The ZipArchive does not support reading.</exception>
    /// <exception cref="ObjectDisposedException">The ZipArchive has already been closed.</exception>
    /// <exception cref="InvalidDataException">The Zip archive is corrupt and the entries cannot be retrieved.</exception>
    [PublicAPI]
    public ListFast<ZipArchiveFastEntry> Entries
    {
        get
        {
            ThrowIfDisposed();

            if (!_entriesInitialized)
            {
                _context.Entries.SetRecycleState((int)_expectedNumberOfEntries);

                #region Read central directory

                try
                {
                    // assume ReadEndOfCentralDirectory has been called and has populated _centralDirectoryStart

                    _archiveStream.Seek(_centralDirectoryStart, SeekOrigin.Begin);

                    long numberOfEntries = 0;

                    //read the central directory
                    while (ZipCentralDirectoryFileHeader.TryReadBlock(_archiveStream, _context, out var currentHeader))
                    {
                        ZipArchiveFastEntry entry;
                        if (_context.Entries.Count > numberOfEntries)
                        {
                            entry = _context.Entries[(int)numberOfEntries];
                            if (entry == null!)
                            {
                                entry = new ZipArchiveFastEntry(currentHeader);
                                _context.Entries[(int)numberOfEntries] = entry;
                            }
                            else
                            {
                                entry.Set(in currentHeader);
                            }
                        }
                        else
                        {
                            entry = new ZipArchiveFastEntry(currentHeader);
                            _context.Entries.Add(entry);
                        }

                        numberOfEntries++;

                        if (currentHeader.DiskNumberStart != _numberOfThisDisk)
                        {
                            ThrowHelper.SplitSpanned();
                        }
                        else
                        {
                            CompressionMethodValues compressionMethod = (CompressionMethodValues)currentHeader.CompressionMethod;
                            if (compressionMethod != CompressionMethodValues.Stored &&
                                compressionMethod != CompressionMethodValues.Deflate &&
                                compressionMethod != CompressionMethodValues.Deflate64)
                            {
                                switch (compressionMethod)
                                {
                                    case CompressionMethodValues.BZip2:
                                    case CompressionMethodValues.LZMA:
                                        string msg = string.Format(SR.UnsupportedCompressionMethod, compressionMethod.ToString());
                                        UnopenableArchives[entry] = msg;
                                        if (!_allowUnsupportedEntries)
                                        {
                                            ThrowHelper.ZipCompressionMethodException(msg);
                                        }
                                        break;
                                    default:
                                        UnopenableArchives[entry] = SR.UnsupportedCompression;
                                        if (!_allowUnsupportedEntries)
                                        {
                                            ThrowHelper.ZipCompressionMethodException(SR.UnsupportedCompression);
                                        }
                                        break;
                                }
                            }
                        }
                    }

                    if (numberOfEntries != _expectedNumberOfEntries)
                    {
                        ThrowHelper.InvalidData(SR.NumEntriesWrong);
                    }
                }
                catch (EndOfStreamException ex)
                {
                    ThrowHelper.InvalidData(SR.CentralDirectoryInvalid, ex);
                }

                _entriesInitialized = true;

                #endregion
            }

            return _context.Entries;
        }
    }

    // This function reads all the EOCD stuff it needs to find the offset to the start of the central directory
    // This offset gets put in _centralDirectoryStart and the number of this disk gets put in _numberOfThisDisk
    // Also does some verification that this isn't a split/spanned archive
    // Also checks that offset to CD isn't out of bounds
    private void ReadEndOfCentralDirectory()
    {
        try
        {
            // this seeks to the start of the end of central directory record
            _archiveStream.Seek(-ZipEndOfCentralDirectoryBlock.SizeOfBlockWithoutSignature, SeekOrigin.End);
            if (!ZipHelpers.SeekBackwardsToSignature(_archiveStream, ZipEndOfCentralDirectoryBlock.SignatureConstant, _context))
            {
                ThrowHelper.InvalidData(SR.EOCDNotFound);
            }

            long eocdStart = _archiveStream.Position;

            // read the EOCD
            bool eocdProper = ZipEndOfCentralDirectoryBlock.TryReadBlock(_archiveStream, _context, out ZipEndOfCentralDirectoryBlock eocd);
            Debug.Assert(eocdProper); // we just found this using the signature finder, so it should be okay

            if (eocd.NumberOfThisDisk != eocd.NumberOfTheDiskWithTheStartOfTheCentralDirectory)
            {
                ThrowHelper.SplitSpanned();
            }

            _numberOfThisDisk = eocd.NumberOfThisDisk;
            _centralDirectoryStart = eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
            if (eocd.NumberOfEntriesInTheCentralDirectory != eocd.NumberOfEntriesInTheCentralDirectoryOnThisDisk)
            {
                ThrowHelper.SplitSpanned();
            }
            _expectedNumberOfEntries = eocd.NumberOfEntriesInTheCentralDirectory;

            // only bother looking for zip64 EOCD stuff if we suspect it is needed because some value is FFFFFFFFF
            // because these are the only two values we need, we only worry about these
            // if we don't find the zip64 EOCD, we just give up and try to use the original values
            if (eocd.NumberOfThisDisk == ZipHelpers.Mask16Bit ||
                eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber == ZipHelpers.Mask32Bit ||
                eocd.NumberOfEntriesInTheCentralDirectory == ZipHelpers.Mask16Bit)
            {
                // we need to look for zip 64 EOCD stuff
                // seek to the zip 64 EOCD locator
                _archiveStream.Seek(eocdStart - Zip64EndOfCentralDirectoryLocator.SizeOfBlockWithoutSignature, SeekOrigin.Begin);
                // if we don't find it, assume it doesn't exist and use data from normal eocd
                if (ZipHelpers.SeekBackwardsToSignature(_archiveStream, Zip64EndOfCentralDirectoryLocator.SignatureConstant, _context))
                {
                    // use locator to get to Zip64EOCD
                    bool zip64EOCDLocatorProper = Zip64EndOfCentralDirectoryLocator.TryReadBlock(_archiveStream, _context, out Zip64EndOfCentralDirectoryLocator locator);
                    Debug.Assert(zip64EOCDLocatorProper); // we just found this using the signature finder, so it should be okay

                    if (locator.OffsetOfZip64EOCD > long.MaxValue)
                    {
                        ThrowHelper.InvalidData(SR.FieldTooBigOffsetToZip64EOCD);
                    }
                    long zip64EOCDOffset = (long)locator.OffsetOfZip64EOCD;

                    _archiveStream.Seek(zip64EOCDOffset, SeekOrigin.Begin);

                    // read Zip64EOCD
                    if (!Zip64EndOfCentralDirectoryRecord.TryReadBlock(_archiveStream, _context, out Zip64EndOfCentralDirectoryRecord record))
                    {
                        ThrowHelper.InvalidData(SR.Zip64EOCDNotWhereExpected);
                    }

                    _numberOfThisDisk = record.NumberOfThisDisk;

                    if (record.NumberOfEntriesTotal > long.MaxValue)
                    {
                        ThrowHelper.InvalidData(SR.FieldTooBigNumEntries);
                    }
                    if (record.OffsetOfCentralDirectory > long.MaxValue)
                    {
                        ThrowHelper.InvalidData(SR.FieldTooBigOffsetToCD);
                    }
                    if (record.NumberOfEntriesTotal != record.NumberOfEntriesOnThisDisk)
                    {
                        ThrowHelper.SplitSpanned();
                    }

                    _expectedNumberOfEntries = (long)record.NumberOfEntriesTotal;
                    _centralDirectoryStart = (long)record.OffsetOfCentralDirectory;
                }
            }

            if (_centralDirectoryStart > ArchiveStreamLength)
            {
                ThrowHelper.InvalidData(SR.FieldTooBigOffsetToCD);
            }
        }
        catch (EndOfStreamException ex)
        {
            ThrowHelper.InvalidData(SR.CDCorrupt, ex);
        }
        catch (IOException ex)
        {
            ThrowHelper.InvalidData(SR.CDCorrupt, ex);
        }
    }

    #region Dispose

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, typeof(ZipArchiveFast).ToString());
    }

    /// <summary>
    /// Releases the unmanaged resources used by ZipArchive and optionally finishes writing the archive and releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to finish writing the archive and release unmanaged and managed resources, false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _archiveStream.Dispose();
            _context.ArchiveSubReadStream.SetSuperStream(null);

            if (_disposeContext) _context.Dispose();

            _isDisposed = true;
        }
    }

    /// <summary>
    /// Finishes writing the archive and releases all resources used by the ZipArchive object, unless the object was constructed with leaveOpen as true. Any streams from opened entries in the ZipArchive still open will throw exceptions on subsequent writes, as the underlying streams will have been closed.
    /// </summary>
    public void Dispose() => Dispose(true);

    #endregion
}

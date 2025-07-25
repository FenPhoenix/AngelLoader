// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Zip Spec here: http://www.pkware.com/documents/casestudies/APPNOTE.TXT

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using static AL_Common.FastZipReader.ZipArchiveFast_Common;

namespace AL_Common.FastZipReader;

public sealed class ZipArchiveFast : IDisposable
{
    // invalid until ReadCentralDirectory
    private long _centralDirectoryStart;

    private bool _isDisposed;
    private long _expectedNumberOfEntries;

    private readonly FileStream_NET _archiveStream;
    public readonly long ArchiveStreamLength;

    // invalid until ReadCentralDirectory
    private uint _numberOfThisDisk;

    private readonly ZipContext _context;

    private readonly bool _allowUnsupportedEntries;

    private readonly bool _ignoreNonBaseDirFileNames;

    // Differentiate between "null encoding because the user used a ctor that doesn't take one" vs. "null encoding
    // because the user used a ctor that takes an encoding but they explicitly passed a null one", because what
    // we do when we find a null encoding is different in those two situations.
    private readonly bool _useEntryNameEncodingCodePath;

    private Encoding? _entryNameEncoding;
    internal Encoding? EntryNameEncoding
    {
        get => _entryNameEncoding;
        private set
        {
            if (value != null &&
                (value.Equals(Encoding.BigEndianUnicode) ||
                 value.Equals(Encoding.Unicode) ||
                 value.Equals(Encoding.UTF32) ||
                 value.Equals(Encoding.UTF7)))
            {
                ThrowHelper.ArgumentException("Entry name encoding not supported", nameof(EntryNameEncoding));
            }
            _entryNameEncoding = value;
        }
    }

    public static ZipArchiveFast Create_General(
        FileStream_NET stream,
        ZipContext context,
        Encoding entryNameEncoding)
    {
        return new ZipArchiveFast(
            stream: stream,
            context: context,
            allowUnsupportedEntries: true,
            entryNameEncoding: entryNameEncoding,
            useEntryNameEncodingCodePath: true,
            ignoreNonBaseDirFileNames: false);
    }

    public static ZipArchiveFast Create_LanguageSearch(
        FileStream_NET stream)
    {
        return new ZipArchiveFast(
            stream,
            new ZipContext(),
            allowUnsupportedEntries: true,
            entryNameEncoding: null,
            useEntryNameEncodingCodePath: false,
            ignoreNonBaseDirFileNames: false);
    }

    public static ZipArchiveFast Create_Scan(
        FileStream_NET stream,
        ZipContext context,
        bool darkMod)
    {
        ZipArchiveFast ret = new(
            stream: stream,
            context: context,
            allowUnsupportedEntries: false,
            entryNameEncoding: null,
            useEntryNameEncodingCodePath: false,
            ignoreNonBaseDirFileNames: darkMod);
        return ret;
    }

    [PublicAPI]
    public ZipArchiveFast(
        FileStream_NET stream,
        ZipContext context,
        bool allowUnsupportedEntries,
        Encoding? entryNameEncoding,
        bool useEntryNameEncodingCodePath,
        bool ignoreNonBaseDirFileNames)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Matching Framework
        if (useEntryNameEncodingCodePath &&
            entryNameEncoding != null &&
            (entryNameEncoding.Equals(Encoding.BigEndianUnicode) ||
             entryNameEncoding.Equals(Encoding.Unicode) ||
             entryNameEncoding.Equals(Encoding.UTF32) ||
             entryNameEncoding.Equals(Encoding.UTF7)))
        {
            throw new ArgumentException(SR.EntryNameEncodingNotSupported, nameof(entryNameEncoding));
        }

        _ignoreNonBaseDirFileNames = ignoreNonBaseDirFileNames;

        _allowUnsupportedEntries = allowUnsupportedEntries;

        if (stream == null) throw new ArgumentNullException(nameof(stream));

        _context = context;

        _useEntryNameEncodingCodePath = useEntryNameEncodingCodePath;
        EntryNameEncoding = entryNameEncoding;

        // Fen's note: Inlined Init() for nullable detection purposes...
        #region Init

        if (!stream.CanRead)
        {
            ThrowHelper.ReadModeCapabilities();
        }
        if (!stream.CanSeek)
        {
            ThrowHelper.NotSupported(SR.NotSupported_UnseekableStream);
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

        if (!IsOpenable(entry, _archiveStream, ArchiveStreamLength, _context.BinaryReadBuffer, out string message))
        {
            ThrowHelper.InvalidData(message);
        }

        // _storedOffsetOfCompressedData will never be null, since we know IsOpenable is true

        _context.ArchiveSubReadStream.Set((long)entry.StoredOffsetOfCompressedData!, entry.CompressedLength);

        return GetDataDecompressor(entry, _context.ArchiveSubReadStream);
    }

    /// <summary>
    /// Returns a list of entries that have been prepared for use in a per-entry threaded scenario.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="zipCtx"></param>
    /// <param name="bufferPool"></param>
    /// <returns></returns>
    public static ListFast<ZipArchiveFastEntry> GetThreadableEntries(string fileName, ZipContext zipCtx, FixedLengthByteArrayPool bufferPool)
    {
        byte[] buffer = bufferPool.Rent();
        try
        {
            using FileStream_NET fs = GetReadModeFileStreamWithCachedBuffer(fileName, buffer);
            using ZipArchiveFast archive = Create_General(
                stream: fs,
                context: zipCtx,
                entryNameEncoding: GetOEMCodePageOrFallback(Encoding.UTF8));

            return archive.Entries;
        }
        finally
        {
            bufferPool.Return(buffer);
        }
    }

    private bool _entriesInitialized;

    [PublicAPI]
    public ListFast<ZipArchiveFastEntry> Entries
    {
        get
        {
            if (!_entriesInitialized)
            {
                LoadEntries();
            }

            return _context.Entries;
        }
    }

    private void LoadEntries()
    {
        _context.Entries.SetRecycleState((int)_expectedNumberOfEntries);

        #region Read central directory

        try
        {
            // assume ReadEndOfCentralDirectory has been called and has populated _centralDirectoryStart

            _archiveStream.Seek(_centralDirectoryStart, SeekOrigin.Begin);

            long numberOfEntries = 0;

            //read the central directory
            while (ZipCentralDirectoryFileHeader.TryReadBlock(
                       _archiveStream,
                       _context,
                       out ZipCentralDirectoryFileHeader currentHeader))
            {
                ZipArchiveFastEntry entry;
                if (_context.Entries.Count > numberOfEntries)
                {
                    entry = _context.Entries[(int)numberOfEntries];
                    if (entry == null!)
                    {
                        entry = new ZipArchiveFastEntry(currentHeader, _entryNameEncoding, _useEntryNameEncodingCodePath, _ignoreNonBaseDirFileNames);
                        _context.Entries[(int)numberOfEntries] = entry;
                    }
                    else
                    {
                        entry.Set(in currentHeader, _entryNameEncoding, _useEntryNameEncodingCodePath, _ignoreNonBaseDirFileNames);
                    }
                }
                else
                {
                    entry = new ZipArchiveFastEntry(currentHeader, _entryNameEncoding, _useEntryNameEncodingCodePath, _ignoreNonBaseDirFileNames);
                    _context.Entries.Add(entry);
                }

                numberOfEntries++;

                if (currentHeader.DiskNumberStart != _numberOfThisDisk)
                {
                    ThrowHelper.SplitSpanned();
                }
                else if (!_allowUnsupportedEntries)
                {
                    CompressionMethodValues compressionMethod = (CompressionMethodValues)currentHeader.CompressionMethod;
                    if (!CompressionMethodSupported(compressionMethod))
                    {
                        ThrowHelper.ZipCompressionMethodException(GetUnsupportedCompressionMethodErrorMessage(compressionMethod));
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

        #endregion

        _entriesInitialized = true;
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
            if (!SeekBackwardsToSignature(_archiveStream, ZipEndOfCentralDirectoryBlock.SignatureConstant, _context))
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
            if (eocd.NumberOfThisDisk == Mask16Bit ||
                eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber == Mask32Bit ||
                eocd.NumberOfEntriesInTheCentralDirectory == Mask16Bit)
            {
                // we need to look for zip 64 EOCD stuff
                // seek to the zip 64 EOCD locator
                _archiveStream.Seek(eocdStart - Zip64EndOfCentralDirectoryLocator.SizeOfBlockWithoutSignature, SeekOrigin.Begin);
                // if we don't find it, assume it doesn't exist and use data from normal eocd
                if (SeekBackwardsToSignature(_archiveStream, Zip64EndOfCentralDirectoryLocator.SignatureConstant, _context))
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

    public void ExtractToFile_Fast(
        ZipArchiveFastEntry entry,
        string fileName,
        bool overwrite,
        bool unSetReadOnly,
        byte[] fileStreamWriteBuffer,
        byte[] streamCopyBuffer)
    {
        using (FileStream_NET destination = GetWriteModeFileStreamWithCachedBuffer(fileName, overwrite, fileStreamWriteBuffer))
        using (Stream source = OpenEntry(entry))
        {
            StreamCopyNoAlloc(source, destination, streamCopyBuffer);
        }

        SetLastWriteTime_Fast(fileName, ZipHelpers.ZipTimeToDateTime(entry.LastWriteTime));

        if (unSetReadOnly)
        {
            File_UnSetReadOnly(fileName);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(typeof(ZipArchiveFast).ToString());
        }
    }

    #region Dispose

    private void Dispose(bool disposing)
    {
        if (disposing && !_isDisposed)
        {
            _archiveStream.Dispose();
            _context.ArchiveSubReadStream.SetSuperStream(null);

            _isDisposed = true;
        }
    }

    public void Dispose() => Dispose(true);

    #endregion
}

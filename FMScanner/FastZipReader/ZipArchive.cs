// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Zip Spec here: http://www.pkware.com/documents/casestudies/APPNOTE.TXT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using FMScanner.FastZipReader.Deflate64Managed;
using JetBrains.Annotations;

namespace FMScanner.FastZipReader
{
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
        private Dictionary<ZipArchiveEntry, string>? _unopenableArchives;
        private Dictionary<ZipArchiveEntry, string> UnopenableArchives => _unopenableArchives ??= new Dictionary<ZipArchiveEntry, string>();

        private List<ZipArchiveEntry>? _entries;
        private ReadOnlyCollection<ZipArchiveEntry>? _entriesCollection;
        private long _centralDirectoryStart; //only valid after ReadCentralDirectory
        private bool _isDisposed;
        private long _expectedNumberOfEntries;
        private readonly Stream? _backingStream;

        internal readonly Stream ArchiveStream;

        private uint _numberOfThisDisk;

        private readonly ZipReusableBundle _bundle;

        private readonly bool _disposeBundle;

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
        public ZipArchiveFast(Stream stream, bool allowUnsupportedEntries) : this(stream,
            new ZipReusableBundle(), disposeBundle: true, allowUnsupportedEntries)
        {
        }

        /// <summary>
        /// Initializes a new instance of ZipArchive on the given stream.
        /// </summary>
        /// <exception cref="ArgumentException">The stream is already closed.</exception>
        /// <exception cref="ArgumentNullException">The stream is null.</exception>
        /// <exception cref="InvalidDataException">The contents of the stream could not be interpreted as a Zip file.</exception>
        /// <param name="stream">The input or output stream.</param>
        /// <param name="bundle"></param>
        /// <param name="allowUnsupportedEntries">
        /// If <see langword="true"/>, entries with unsupported compression methods will only throw when opened.
        /// <br/>
        /// If <see langword="false"/>, the <see cref="T:Entries"/> collection will throw immediately if any
        /// entries with unsupported compression methods are found.
        /// </param>
        [PublicAPI]
        public ZipArchiveFast(Stream stream, ZipReusableBundle bundle, bool allowUnsupportedEntries) : this(
            stream, bundle, disposeBundle: false, allowUnsupportedEntries)
        {
        }

        [PublicAPI]
        private ZipArchiveFast(Stream stream, ZipReusableBundle bundle, bool disposeBundle, bool allowUnsupportedEntries)
        {
            _allowUnsupportedEntries = allowUnsupportedEntries;

            _disposeBundle = disposeBundle;

            if (stream == null) throw new ArgumentNullException(nameof(stream));

            _bundle = bundle;

            // Fen's note: Inlined Init() for nullable detection purposes...
            #region Init

            Stream? extraTempStream = null;

            try
            {
                _backingStream = null;

                if (!stream.CanRead)
                {
                    throw new ArgumentException(SR.ReadModeCapabilities);
                }
                if (!stream.CanSeek)
                {
                    _backingStream = stream;
                    extraTempStream = stream = new MemoryStream();
                    _backingStream.CopyTo(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                }

                ArchiveStream = stream;

                bundle.ArchiveSubReadStream.SetSuperStream(ArchiveStream);

                _centralDirectoryStart = 0; // invalid until ReadCentralDirectory
                _isDisposed = false;
                _numberOfThisDisk = 0; // invalid until ReadCentralDirectory

                ReadEndOfCentralDirectory();
            }
            catch
            {
                extraTempStream?.Dispose();
                throw;
            }

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
        public Stream OpenEntry(ZipArchiveEntry entry)
        {
            ThrowIfDisposed();

            if (!IsOpenable(entry, out string message)) throw new InvalidDataException(message);

            // _storedOffsetOfCompressedData will never be null, since we know IsOpenable is true

            _bundle.ArchiveSubReadStream.Set((long)entry.StoredOffsetOfCompressedData!, entry.CompressedLength);

            return GetDataDecompressor(entry, _bundle.ArchiveSubReadStream);
        }

        private static Stream GetDataDecompressor(ZipArchiveEntry entry, Stream compressedStreamToRead)
        {
            Stream uncompressedStream;
            switch (entry.CompressionMethod)
            {
                case CompressionMethodValues.Deflate:
                    uncompressedStream = new DeflateStream(compressedStreamToRead, CompressionMode.Decompress);
                    break;
                case CompressionMethodValues.Deflate64:
                    // This is always in decompress-only mode
                    uncompressedStream = new Deflate64ManagedStream(compressedStreamToRead);
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

        private bool IsOpenable(ZipArchiveEntry entry, out string message)
        {
            message = "";

            if (UnopenableArchives.TryGetValue(entry, out string result))
            {
                message = result;
                return false;
            }

            if (entry.StoredOffsetOfCompressedData != null)
            {
                ArchiveStream.Seek((long)entry.StoredOffsetOfCompressedData, SeekOrigin.Begin);
                return true;
            }

            if (entry.OffsetOfLocalHeader > ArchiveStream.Length)
            {
                message = SR.LocalFileHeaderCorrupt;
                return false;
            }

            ArchiveStream.Seek(entry.OffsetOfLocalHeader, SeekOrigin.Begin);
            if (!ZipLocalFileHeader.TrySkipBlock(ArchiveStream, _bundle))
            {
                message = SR.LocalFileHeaderCorrupt;
                return false;
            }

            // At this point, this is really just caching a FileStream.Position, which does have some logic in
            // its getter, but probably isn't slow enough to warrant being cached... but I guess ArchiveStream
            // could be any kind of stream, so better to guarantee performance than to hope for it, I guess.
            entry.StoredOffsetOfCompressedData ??= ArchiveStream.Position;

            if (entry.StoredOffsetOfCompressedData + entry.CompressedLength > ArchiveStream.Length)
            {
                message = SR.LocalFileHeaderCorrupt;
                return false;
            }

            return true;
        }

        /// <summary>
        /// The collection of entries that are currently in the ZipArchive. This may not accurately represent the actual entries that are present in the underlying file or stream.
        /// </summary>
        /// <exception cref="NotSupportedException">The ZipArchive does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">The ZipArchive has already been closed.</exception>
        /// <exception cref="InvalidDataException">The Zip archive is corrupt and the entries cannot be retrieved.</exception>
        [PublicAPI]
        public ReadOnlyCollection<ZipArchiveEntry> Entries
        {
            get
            {
                ThrowIfDisposed();

                if (_entries == null)
                {
                    _entries = new List<ZipArchiveEntry>((int)_expectedNumberOfEntries);
                    _entriesCollection = new ReadOnlyCollection<ZipArchiveEntry>(_entries);

                    #region Read central directory

                    try
                    {
                        // assume ReadEndOfCentralDirectory has been called and has populated _centralDirectoryStart

                        ArchiveStream.Seek(_centralDirectoryStart, SeekOrigin.Begin);

                        long numberOfEntries = 0;

                        //read the central directory
                        while (ZipCentralDirectoryFileHeader.TryReadBlock(ArchiveStream, _bundle, out var currentHeader))
                        {
                            var entry = new ZipArchiveEntry(currentHeader);
                            _entries.Add(entry);
                            numberOfEntries++;

                            if (currentHeader.DiskNumberStart != _numberOfThisDisk)
                            {
                                UnopenableArchives[entry] = SR.SplitSpanned;
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
                                                throw new ZipCompressionMethodException(msg);
                                            }
                                            break;
                                        default:
                                            UnopenableArchives[entry] = SR.UnsupportedCompression;
                                            if (!_allowUnsupportedEntries)
                                            {
                                                throw new ZipCompressionMethodException(SR.UnsupportedCompression);
                                            }
                                            break;
                                    }
                                }
                            }
                        }

                        if (numberOfEntries != _expectedNumberOfEntries)
                        {
                            throw new InvalidDataException(SR.NumEntriesWrong);
                        }
                    }
                    catch (EndOfStreamException ex)
                    {
                        throw new InvalidDataException(SR.CentralDirectoryInvalid + "\r\nInner exception:\r\n" + ex);
                    }

                    #endregion
                }

                return _entriesCollection!;
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
                ArchiveStream.Seek(-ZipEndOfCentralDirectoryBlock.SizeOfBlockWithoutSignature, SeekOrigin.End);
                if (!ZipHelpers.SeekBackwardsToSignature(ArchiveStream, ZipEndOfCentralDirectoryBlock.SignatureConstant, _bundle))
                {
                    throw new InvalidDataException(SR.EOCDNotFound);
                }

                long eocdStart = ArchiveStream.Position;

                // read the EOCD
                bool eocdProper = ZipEndOfCentralDirectoryBlock.TryReadBlock(ArchiveStream, _bundle, out ZipEndOfCentralDirectoryBlock eocd);
                Debug.Assert(eocdProper); // we just found this using the signature finder, so it should be okay

                if (eocd.NumberOfThisDisk != eocd.NumberOfTheDiskWithTheStartOfTheCentralDirectory)
                {
                    throw new InvalidDataException(SR.SplitSpanned);
                }

                _numberOfThisDisk = eocd.NumberOfThisDisk;
                _centralDirectoryStart = eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
                if (eocd.NumberOfEntriesInTheCentralDirectory != eocd.NumberOfEntriesInTheCentralDirectoryOnThisDisk)
                {
                    throw new InvalidDataException(SR.SplitSpanned);
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
                    ArchiveStream.Seek(eocdStart - Zip64EndOfCentralDirectoryLocator.SizeOfBlockWithoutSignature, SeekOrigin.Begin);
                    // if we don't find it, assume it doesn't exist and use data from normal eocd
                    if (ZipHelpers.SeekBackwardsToSignature(ArchiveStream, Zip64EndOfCentralDirectoryLocator.SignatureConstant, _bundle))
                    {
                        // use locator to get to Zip64EOCD
                        bool zip64EOCDLocatorProper = Zip64EndOfCentralDirectoryLocator.TryReadBlock(ArchiveStream, _bundle, out Zip64EndOfCentralDirectoryLocator locator);
                        Debug.Assert(zip64EOCDLocatorProper); // we just found this using the signature finder, so it should be okay

                        if (locator.OffsetOfZip64EOCD > long.MaxValue)
                        {
                            throw new InvalidDataException(SR.FieldTooBigOffsetToZip64EOCD);
                        }
                        long zip64EOCDOffset = (long)locator.OffsetOfZip64EOCD;

                        ArchiveStream.Seek(zip64EOCDOffset, SeekOrigin.Begin);

                        // read Zip64EOCD
                        if (!Zip64EndOfCentralDirectoryRecord.TryReadBlock(ArchiveStream, _bundle, out Zip64EndOfCentralDirectoryRecord record))
                        {
                            throw new InvalidDataException(SR.Zip64EOCDNotWhereExpected);
                        }

                        _numberOfThisDisk = record.NumberOfThisDisk;

                        if (record.NumberOfEntriesTotal > long.MaxValue)
                        {
                            throw new InvalidDataException(SR.FieldTooBigNumEntries);
                        }
                        if (record.OffsetOfCentralDirectory > long.MaxValue)
                        {
                            throw new InvalidDataException(SR.FieldTooBigOffsetToCD);
                        }
                        if (record.NumberOfEntriesTotal != record.NumberOfEntriesOnThisDisk)
                        {
                            throw new InvalidDataException(SR.SplitSpanned);
                        }

                        _expectedNumberOfEntries = (long)record.NumberOfEntriesTotal;
                        _centralDirectoryStart = (long)record.OffsetOfCentralDirectory;
                    }
                }

                if (_centralDirectoryStart > ArchiveStream.Length)
                {
                    throw new InvalidDataException(SR.FieldTooBigOffsetToCD);
                }
            }
            catch (EndOfStreamException ex)
            {
                throw new InvalidDataException(SR.CDCorrupt, ex);
            }
            catch (IOException ex)
            {
                throw new InvalidDataException(SR.CDCorrupt, ex);
            }
        }

        #region Dispose

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().ToString());
        }

        /// <summary>
        /// Releases the unmanaged resources used by ZipArchive and optionally finishes writing the archive and releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to finish writing the archive and release unmanaged and managed resources, false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                ArchiveStream.Dispose();
                _bundle.ArchiveSubReadStream.SetSuperStream(null);
                _backingStream?.Dispose();

                if (_disposeBundle) _bundle.Dispose();

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finishes writing the archive and releases all resources used by the ZipArchive object, unless the object was constructed with leaveOpen as true. Any streams from opened entries in the ZipArchive still open will throw exceptions on subsequent writes, as the underlying streams will have been closed.
        /// </summary>
        public void Dispose() => Dispose(true);

        #endregion
    }
}

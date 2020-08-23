// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Zip Spec here: http://www.pkware.com/documents/casestudies/APPNOTE.TXT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace FMScanner.FastZipReader
{
    public sealed class ZipArchiveFast : IDisposable
    {
        private readonly List<ZipArchiveEntry> _entries;
        private readonly ReadOnlyCollection<ZipArchiveEntry> _entriesCollection;
        private readonly Dictionary<string, ZipArchiveEntry> _entriesDictionary;
        private bool _readEntries;
        private readonly bool _leaveOpen;
        private long _centralDirectoryStart; //only valid after ReadCentralDirectory
        private bool _isDisposed;
        private long _expectedNumberOfEntries;
        private readonly Stream? _backingStream;
        private Encoding? _entryNameEncoding;

        internal readonly BinaryReader ArchiveReader;

        internal readonly Stream ArchiveStream;

        internal uint NumberOfThisDisk;

        internal Encoding? EntryNameEncoding
        {
            get { return _entryNameEncoding; }

            private set
            {
                // value == null is fine. This means the user does not want to overwrite default encoding picking logic.

                // The Zip file spec [http://www.pkware.com/documents/casestudies/APPNOTE.TXT] specifies a bit in the entry header
                // (specifically: the language encoding flag (EFS) in the general purpose bit flag of the local file header) that
                // basically says: UTF8 (1) or CP437 (0). But in reality, tools replace CP437 with "something else that is not UTF8".
                // For instance, the Windows Shell Zip tool takes "something else" to mean "the local system codepage".
                // We default to the same behaviour, but we let the user explicitly specify the encoding to use for cases where they
                // understand their use case well enough.
                // Since the definition of acceptable encodings for the "something else" case is in reality by convention, it is not
                // immediately clear, whether non-UTF8 Unicode encodings are acceptable. To determine that we would need to survey
                // what is currently being done in the field, but we do not have the time for it right now.
                // So, we artificially disallow non-UTF8 Unicode encodings for now to make sure we are not creating a compat burden
                // for something other tools do not support. If we realise in future that "something else" should include non-UTF8
                // Unicode encodings, we can remove this restriction.

                if (value != null &&
                        (value.Equals(Encoding.BigEndianUnicode)
                        || value.Equals(Encoding.Unicode)
#if FEATURE_UTF32
                        || value.Equals(Encoding.UTF32)
#endif // FEATURE_UTF32
#if FEATURE_UTF7
                        || value.Equals(Encoding.UTF7)
#endif // FEATURE_UTF7
                        ))
                {
                    throw new ArgumentException(SR.EntryNameEncodingNotSupported, nameof(EntryNameEncoding));
                }

                _entryNameEncoding = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of ZipArchive on the given stream for reading.
        /// </summary>
        /// <exception cref="ArgumentException">The stream is already closed or does not support reading.</exception>
        /// <exception cref="ArgumentNullException">The stream is null.</exception>
        /// <exception cref="InvalidDataException">The contents of the stream could not be interpreted as a Zip archive.</exception>
        /// <param name="stream">The stream containing the archive to be read.</param>
        public ZipArchiveFast(Stream stream) : this(stream, leaveOpen: false, entryNameEncoding: null) { }

        /// <summary>
        /// Initializes a new instance of ZipArchive on the given stream, specifying whether to leave the stream open.
        /// </summary>
        /// <exception cref="ArgumentException">The stream is already closed.</exception>
        /// <exception cref="ArgumentNullException">The stream is null.</exception>
        /// <exception cref="InvalidDataException">The contents of the stream could not be interpreted as a Zip file.</exception>
        /// <param name="stream">The input or output stream.</param>
        /// <param name="leaveOpen">true to leave the stream open upon disposing the ZipArchive, otherwise false.</param>
        public ZipArchiveFast(Stream stream, bool leaveOpen) : this(stream, leaveOpen, entryNameEncoding: null) { }

        /// <summary>
        /// Initializes a new instance of ZipArchive on the given stream, specifying whether to leave the stream open.
        /// </summary>
        /// <exception cref="ArgumentException">The stream is already closed.</exception>
        /// <exception cref="ArgumentNullException">The stream is null.</exception>
        /// <exception cref="InvalidDataException">The contents of the stream could not be interpreted as a Zip file.</exception>
        /// <param name="stream">The input or output stream.</param>
        /// <param name="leaveOpen">true to leave the stream open upon disposing the ZipArchive, otherwise false.</param>
        /// <param name="entryNameEncoding">The encoding to use when reading or writing entry names in this ZipArchive.
        ///         ///     <para>NOTE: Specifying this parameter to values other than <c>null</c> is discouraged.
        ///         However, this may be necessary for interoperability with ZIP archive tools and libraries that do not correctly support
        ///         UTF-8 encoding for entry names.<br />
        ///         This value is used as follows:</para>
        ///     <para><strong>Reading (opening) ZIP archive files:</strong></para>
        ///     <para>If <c>entryNameEncoding</c> is not specified (<c>== null</c>):</para>
        ///     <list>
        ///         <item>For entries where the language encoding flag (EFS) in the general purpose bit flag of the local file header is <em>not</em> set,
        ///         use the current system default code page (<c>Encoding.Default</c>) in order to decode the entry name.</item>
        ///         <item>For entries where the language encoding flag (EFS) in the general purpose bit flag of the local file header <em>is</em> set,
        ///         use UTF-8 (<c>Encoding.UTF8</c>) in order to decode the entry name.</item>
        ///     </list>
        ///     <para>If <c>entryNameEncoding</c> is specified (<c>!= null</c>):</para>
        ///     <list>
        ///         <item>For entries where the language encoding flag (EFS) in the general purpose bit flag of the local file header is <em>not</em> set,
        ///         use the specified <c>entryNameEncoding</c> in order to decode the entry name.</item>
        ///         <item>For entries where the language encoding flag (EFS) in the general purpose bit flag of the local file header <em>is</em> set,
        ///         use UTF-8 (<c>Encoding.UTF8</c>) in order to decode the entry name.</item>
        ///     </list>
        ///     <para>Note that Unicode encodings other than UTF-8 may not be currently used for the <c>entryNameEncoding</c>,
        ///     otherwise an <see cref="ArgumentException"/> is thrown.</para>
        /// </param>
        /// <exception cref="ArgumentException">If a Unicode encoding other than UTF-8 is specified for the <code>entryNameEncoding</code>.</exception>
        public ZipArchiveFast(Stream stream, bool leaveOpen, Encoding? entryNameEncoding)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            EntryNameEncoding = entryNameEncoding;

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

                ArchiveReader = new BinaryReader(ArchiveStream);
                _entries = new List<ZipArchiveEntry>();
                _entriesCollection = new ReadOnlyCollection<ZipArchiveEntry>(_entries);
                _entriesDictionary = new Dictionary<string, ZipArchiveEntry>();
                _readEntries = false;
                _leaveOpen = leaveOpen;
                _centralDirectoryStart = 0; // invalid until ReadCentralDirectory
                _isDisposed = false;
                NumberOfThisDisk = 0; // invalid until ReadCentralDirectory

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

                EnsureCentralDirectoryRead();
                return _entriesCollection;
            }
        }

        /// <summary>
        /// Retrieves a wrapper for the file entry in the archive with the specified name. Names are compared using ordinal comparison. If there are multiple entries in the archive with the specified name, the first one found will be returned.
        /// </summary>
        /// <exception cref="ArgumentException">entryName is a zero-length string.</exception>
        /// <exception cref="ArgumentNullException">entryName is null.</exception>
        /// <exception cref="NotSupportedException">The ZipArchive does not support reading.</exception>
        /// <exception cref="ObjectDisposedException">The ZipArchive has already been closed.</exception>
        /// <exception cref="InvalidDataException">The Zip archive is corrupt and the entries cannot be retrieved.</exception>
        /// <param name="entryName">A path relative to the root of the archive, identifying the desired entry.</param>
        /// <returns>A wrapper for the file entry in the archive. If no entry in the archive exists with the specified name, null will be returned.</returns>
        [PublicAPI]
        public ZipArchiveEntry? GetEntry(string entryName)
        {
            if (entryName == null) throw new ArgumentNullException(nameof(entryName));

            EnsureCentralDirectoryRead();
            _entriesDictionary.TryGetValue(entryName, out ZipArchiveEntry result);
            return result;
        }

        private void AddEntry(ZipArchiveEntry entry)
        {
            _entries.Add(entry);

            string entryName = entry.FullName;
            if (!_entriesDictionary.ContainsKey(entryName))
            {
                _entriesDictionary.Add(entryName, entry);
            }
        }

        private void EnsureCentralDirectoryRead()
        {
            if (!_readEntries)
            {
                ReadCentralDirectory();
                _readEntries = true;
            }
        }

        private void ReadCentralDirectory()
        {
            try
            {
                // assume ReadEndOfCentralDirectory has been called and has populated _centralDirectoryStart

                ArchiveStream.Seek(_centralDirectoryStart, SeekOrigin.Begin);

                long numberOfEntries = 0;

                //read the central directory
                while (ZipCentralDirectoryFileHeader.TryReadBlock(ArchiveReader, out var currentHeader))
                {
                    AddEntry(new ZipArchiveEntry(this, currentHeader));
                    numberOfEntries++;
                }

                if (numberOfEntries != _expectedNumberOfEntries)
                {
                    throw new InvalidDataException(SR.NumEntriesWrong);
                }
            }
            catch (EndOfStreamException ex)
            {
                throw new InvalidDataException(SR.Format(SR.CentralDirectoryInvalid, ex));
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
                if (!ZipHelper.SeekBackwardsToSignature(ArchiveStream, ZipEndOfCentralDirectoryBlock.SignatureConstant))
                {
                    throw new InvalidDataException(SR.EOCDNotFound);
                }

                long eocdStart = ArchiveStream.Position;

                // read the EOCD
                bool eocdProper = ZipEndOfCentralDirectoryBlock.TryReadBlock(ArchiveReader, out ZipEndOfCentralDirectoryBlock eocd);
                Debug.Assert(eocdProper); // we just found this using the signature finder, so it should be okay

                if (eocd.NumberOfThisDisk != eocd.NumberOfTheDiskWithTheStartOfTheCentralDirectory)
                {
                    throw new InvalidDataException(SR.SplitSpanned);
                }

                NumberOfThisDisk = eocd.NumberOfThisDisk;
                _centralDirectoryStart = eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
                if (eocd.NumberOfEntriesInTheCentralDirectory != eocd.NumberOfEntriesInTheCentralDirectoryOnThisDisk)
                {
                    throw new InvalidDataException(SR.SplitSpanned);
                }
                _expectedNumberOfEntries = eocd.NumberOfEntriesInTheCentralDirectory;

                // only bother looking for zip64 EOCD stuff if we suspect it is needed because some value is FFFFFFFFF
                // because these are the only two values we need, we only worry about these
                // if we don't find the zip64 EOCD, we just give up and try to use the original values
                if (eocd.NumberOfThisDisk == ZipHelper.Mask16Bit ||
                    eocd.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber == ZipHelper.Mask32Bit ||
                    eocd.NumberOfEntriesInTheCentralDirectory == ZipHelper.Mask16Bit)
                {
                    // we need to look for zip 64 EOCD stuff
                    // seek to the zip 64 EOCD locator
                    ArchiveStream.Seek(eocdStart - Zip64EndOfCentralDirectoryLocator.SizeOfBlockWithoutSignature, SeekOrigin.Begin);
                    // if we don't find it, assume it doesn't exist and use data from normal eocd
                    if (ZipHelper.SeekBackwardsToSignature(ArchiveStream, Zip64EndOfCentralDirectoryLocator.SignatureConstant))
                    {
                        // use locator to get to Zip64EOCD
                        bool zip64EOCDLocatorProper = Zip64EndOfCentralDirectoryLocator.TryReadBlock(ArchiveReader, out Zip64EndOfCentralDirectoryLocator locator);
                        Debug.Assert(zip64EOCDLocatorProper); // we just found this using the signature finder, so it should be okay

                        if (locator.OffsetOfZip64EOCD > long.MaxValue)
                        {
                            throw new InvalidDataException(SR.FieldTooBigOffsetToZip64EOCD);
                        }
                        long zip64EOCDOffset = (long)locator.OffsetOfZip64EOCD;

                        ArchiveStream.Seek(zip64EOCDOffset, SeekOrigin.Begin);

                        // read Zip64EOCD
                        if (!Zip64EndOfCentralDirectoryRecord.TryReadBlock(ArchiveReader, out Zip64EndOfCentralDirectoryRecord record))
                        {
                            throw new InvalidDataException(SR.Zip64EOCDNotWhereExpected);
                        }

                        NumberOfThisDisk = record.NumberOfThisDisk;

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

        internal void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(GetType().ToString());
        }

        private void CloseStreams()
        {
            if (!_leaveOpen)
            {
                ArchiveStream.Dispose();
                _backingStream?.Dispose();
                ArchiveReader?.Dispose();
            }
            else
            {
                // if _backingStream isn't null, that means we assigned the original stream they passed
                // us to _backingStream (which they requested we leave open), and _archiveStream was
                // the temporary copy that we needed
                if (_backingStream != null) ArchiveStream.Dispose();
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by ZipArchive and optionally finishes writing the archive and releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to finish writing the archive and release unmanaged and managed resources, false to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                CloseStreams();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finishes writing the archive and releases all resources used by the ZipArchive object, unless the object was constructed with leaveOpen as true. Any streams from opened entries in the ZipArchive still open will throw exceptions on subsequent writes, as the underlying streams will have been closed.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

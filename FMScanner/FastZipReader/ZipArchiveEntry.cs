// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text;
using FMScanner.FastZipReader.Deflate64Managed;

namespace FMScanner.FastZipReader
{
    // The disposable fields that this class owns get disposed when the ZipArchive it belongs to gets disposed
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class ZipArchiveEntry
    {
        #region Fields

        private readonly int _diskNumberStart;
        private readonly BitFlagValues _generalPurposeBitFlag;
        private readonly long _offsetOfLocalHeader;
        private readonly CompressionMethodValues _compressionMethod;
        private long? _storedOffsetOfCompressedData;

        #endregion

        #region Enums

        /// <summary>
        /// The upper byte of the "version made by" flag in the central directory header of a zip file represents the
        /// OS of the system on which the zip was created. Any zip created with an OS byte not equal to Windows (0)
        /// or Unix (3) will be treated as equal to the current OS.
        /// </summary>
        /// <remarks>
        /// The value of 0 more specifically corresponds to the FAT file system while NTFS is assigned a higher value. However
        /// for historical and compatibility reasons, Windows is always assigned a 0 value regardless of file system.
        /// </remarks>
        internal enum ZipVersionMadeByPlatform : byte
        {
            Windows = 0,
            Unix = 3
        }

        [Flags]
        private enum BitFlagValues : ushort
        {
            DataDescriptor = 0x8,
            UnicodeFileName = 0x800
        }

        private enum CompressionMethodValues : ushort
        {
            Stored = 0x0,
            Deflate = 0x8,
            Deflate64 = 0x9,
            BZip2 = 0xC,
            LZMA = 0xE
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ZipArchive that this entry belongs to.
        /// </summary>
        private ZipArchive Archive { get; }

        [CLSCompliant(false)]
        internal uint Crc32 { get; }

        /// <summary>
        /// The compressed size of the entry.
        /// </summary>
        internal long CompressedLength { get; }

        /// <summary>
        /// OS and Application specific file attributes.
        /// </summary>
        internal int ExternalAttributes { get; }

        /// <summary>
        /// The last write time of the entry as stored in the Zip archive. To convert to a DateTime object, use
        /// <see cref="ZipHelpers.ZipTimeToDateTime"/>.
        /// </summary>
        internal uint LastWriteTime { get; }

        /// <summary>
        /// The uncompressed size of the entry.
        /// </summary>
        internal long Length { get; }

        /// <summary>
        /// The relative path of the entry as stored in the Zip archive. Note that Zip archives allow any string
        /// to be the path of the entry, including invalid and absolute paths.
        /// </summary>
        internal string FullName { get; }

        /// <summary>
        /// The filename of the entry. This is equivalent to the substring of <see cref="FullName"/> that follows
        /// the final directory separator character.
        /// </summary>
        internal string Name { get; }

        #endregion

        // Initializes, attaches it to archive
        internal ZipArchiveEntry(ZipArchive archive, ZipCentralDirectoryFileHeader cd)
        {
            Archive = archive;

            _diskNumberStart = cd.DiskNumberStart;
            _generalPurposeBitFlag = (BitFlagValues)cd.GeneralPurposeBitFlag;
            _compressionMethod = (CompressionMethodValues)cd.CompressionMethod;

            // Leave this as a uint and let the caller convert it if it wants (perf optimization)
            LastWriteTime = cd.LastModified;

            CompressedLength = cd.CompressedSize;
            Length = cd.UncompressedSize;
            ExternalAttributes = (int)cd.ExternalFileAttributes;
            _offsetOfLocalHeader = cd.RelativeOffsetOfLocalHeader;

            // we don't know this yet: should be _offsetOfLocalHeader + 30 + _storedEntryNameBytes.Length + extrafieldlength
            // but entryname/extra length could be different in LH
            _storedOffsetOfCompressedData = null;

            Crc32 = cd.Crc32;

            FullName = DecodeEntryName(cd.Filename) ?? throw new ArgumentNullException(nameof(FullName));
            Name = ParseFileName(FullName, (ZipVersionMadeByPlatform)cd.VersionMadeByCompatibility);
        }

        private string DecodeEntryName(byte[] entryNameBytes)
        {
            Debug.Assert(entryNameBytes != null);

            Encoding readEntryNameEncoding;
            if ((_generalPurposeBitFlag & BitFlagValues.UnicodeFileName) == 0)
            {
                #region Original corefx
                readEntryNameEncoding = Archive == null ?
                    Encoding.UTF8 :
                    Archive.EntryNameEncoding ?? Encoding.UTF8;
                #endregion

                #region .NET Framework 4.7.2
                // This is what .NET Framework 4.7.2 seems to be doing (at least I get the same result with this)
                //readEntryNameEncoding = Archive == null
                //    ? Encoding.UTF8
                //    : Archive.EntryNameEncoding ?? Encoding.Default;
                #endregion
            }
            else
            {
                readEntryNameEncoding = Encoding.UTF8;
            }

            return readEntryNameEncoding.GetString(entryNameBytes);
        }

        #region Open

        /// <summary>
        /// Opens the entry in Read mode. The returned stream will be readable, and it may or may not be seekable.
        /// </summary>
        /// <returns>A Stream that represents the contents of the entry.</returns>
        /// <exception cref="InvalidDataException">
        /// The entry is missing from the archive or is corrupt and cannot be read.
        /// -or-
        /// The entry has been compressed using a compression method that is not supported.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// The ZipArchive that this entry belongs to has been disposed.
        /// </exception>
        internal Stream Open()
        {
            Archive.ThrowIfDisposed();

            if (!IsOpenable(out var message)) throw new InvalidDataException(message);

            // _storedOffsetOfCompressedData will never be null, since we know IsOpenable is true
            Debug.Assert(_storedOffsetOfCompressedData != null, nameof(_storedOffsetOfCompressedData) + " != null");

            Stream compressedStream =
                new SubReadStream(Archive.ArchiveStream, (long)_storedOffsetOfCompressedData, CompressedLength);

            return GetDataDecompressor(compressedStream);
        }

        private bool IsOpenable(out string message)
        {
            message = null;

            if (_compressionMethod != CompressionMethodValues.Stored &&
                _compressionMethod != CompressionMethodValues.Deflate &&
                _compressionMethod != CompressionMethodValues.Deflate64)
            {
                switch (_compressionMethod)
                {
                    case CompressionMethodValues.BZip2:
                    case CompressionMethodValues.LZMA:
                        message = SR.Format(SR.UnsupportedCompressionMethod, _compressionMethod.ToString());
                        break;
                    default:
                        message = SR.UnsupportedCompression;
                        break;
                }

                return false;
            }

            if (_diskNumberStart != Archive.NumberOfThisDisk)
            {
                message = SR.SplitSpanned;
                return false;
            }

            if (_offsetOfLocalHeader > Archive.ArchiveStream.Length)
            {
                message = SR.LocalFileHeaderCorrupt;
                return false;
            }

            Archive.ArchiveStream.Seek(_offsetOfLocalHeader, SeekOrigin.Begin);
            if (!ZipLocalFileHeader.TrySkipBlock(Archive.ArchiveReader))
            {
                message = SR.LocalFileHeaderCorrupt;
                return false;
            }

            // At this point, this is really just caching a FileStream.Position, which does have some logic in
            // its getter, but probably isn't slow enough to warrant being cached... but I guess ArchiveStream
            // could be any kind of stream, so better to guarantee performance than to hope for it, I guess.
            if (_storedOffsetOfCompressedData == null)
            {
                _storedOffsetOfCompressedData = Archive.ArchiveStream.Position;
            }

            if (_storedOffsetOfCompressedData + CompressedLength > Archive.ArchiveStream.Length)
            {
                message = SR.LocalFileHeaderCorrupt;
                return false;
            }

            return true;
        }

        private Stream GetDataDecompressor(Stream compressedStreamToRead)
        {
            Stream uncompressedStream;
            switch (_compressionMethod)
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
                    Debug.Assert(_compressionMethod == CompressionMethodValues.Stored);

                    uncompressedStream = compressedStreamToRead;
                    break;
            }

            return uncompressedStream;
        }

        #endregion

        #region Parse file name

        /// <summary>
        /// To get the file name of a ZipArchiveEntry, we should be parsing the FullName based
        /// on the path specifications and requirements of the OS that ZipArchive was created on.
        /// This method takes in a FullName and the platform of the ZipArchiveEntry and returns
        /// the platform-correct file name.
        /// </summary>
        /// <remarks>This method ensures no validation on the paths. Invalid characters are allowed.</remarks>
        private static string ParseFileName(string path, ZipVersionMadeByPlatform madeByPlatform)
        {
            return madeByPlatform == ZipVersionMadeByPlatform.Windows
                ? GetFileName_Windows(path)
                : GetFileName_Unix(path);
        }

        /// <summary>
        /// Gets the file name of the path based on Windows path separator characters
        /// </summary>
        private static string GetFileName_Windows(string path)
        {
            int length = path.Length;
            for (int i = length; --i >= 0;)
            {
                char ch = path[i];
                if (ch == '\\' || ch == '/' || ch == ':')
                    return path.Substring(i + 1);
            }
            return path;
        }

        /// <summary>
        /// Gets the file name of the path based on Unix path separator characters
        /// </summary>
        private static string GetFileName_Unix(string path)
        {
            int length = path.Length;
            for (int i = length; --i >= 0;)
                if (path[i] == '/')
                    return path.Substring(i + 1);
            return path;
        }

        #endregion

        /// <summary>
        /// Returns the FullName of the entry.
        /// </summary>
        /// <returns>FullName of the entry.</returns>
        public override string ToString() => FullName;
    }
}

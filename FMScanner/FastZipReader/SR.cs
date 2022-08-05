// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace FMScanner.FastZipReader
{
    // We don't want these as constants because we don't want them duplicated everywhere bloating things up.
    // We don't care about the infinitesimal perf increase of constants either, because these are error messages
    // and will rarely be hit anyway.
    [SuppressMessage("ReSharper", "ConvertToConstant.Global")]
    internal static class SR
    {
        // Not localizable cause who the hell cares
        internal static readonly string GenericInvalidData = "Found invalid data while decoding.";
        internal static readonly string InvalidArgumentOffsetCount = "Offset plus count is larger than the length of target array.";
        internal static readonly string InvalidBlockLength = "Block length does not match with its complement.";
        internal static readonly string InvalidHuffmanData = "Failed to construct a huffman tree using the length array. The stream might be corrupted.";
        internal static readonly string NotSupported = "This operation is not supported.";
        internal static readonly string NotSupported_UnreadableStream = "Stream does not support reading.";
        internal static readonly string ObjectDisposed_StreamClosed = "Cannot access a closed stream.";
        internal static readonly string UnknownBlockType = "Unknown block type. Stream might be corrupted.";
        internal static readonly string UnknownState = "Decoder is in some unknown state. This might be caused by corrupted data.";
        internal static readonly string CDCorrupt = "Central Directory corrupt.";
        internal static readonly string CentralDirectoryInvalid = "Central Directory is invalid.";
        internal static readonly string EOCDNotFound = "End of Central Directory record could not be found.";
        internal static readonly string FieldTooBigCompressedSize = "Compressed Size cannot be held in an Int64.";
        internal static readonly string FieldTooBigLocalHeaderOffset = "Local Header Offset cannot be held in an Int64.";
        internal static readonly string FieldTooBigNumEntries = "Number of Entries cannot be held in an Int64.";
        internal static readonly string FieldTooBigOffsetToCD = "Offset to Central Directory cannot be held in an Int64.";
        internal static readonly string FieldTooBigOffsetToZip64EOCD = "Offset to Zip64 End Of Central Directory record cannot be held in an Int64.";
        internal static readonly string FieldTooBigStartDiskNumber = "Start Disk Number cannot be held in an Int64.";
        internal static readonly string FieldTooBigUncompressedSize = "Uncompressed Size cannot be held in an Int64.";
        internal static readonly string LocalFileHeaderCorrupt = "A local file header is corrupt.";
        internal static readonly string NumEntriesWrong = "Number of entries expected in End Of Central Directory does not correspond to number of entries in Central Directory.";
        internal static readonly string ReadingNotSupported = "This stream from ZipArchiveEntry does not support reading.";
        internal static readonly string ReadModeCapabilities = "Cannot use read mode on a non-readable stream.";
        internal static readonly string SeekingNotSupported = "This stream from ZipArchiveEntry does not support seeking.";
        internal static readonly string SetLengthRequiresSeekingAndWriting = "SetLength requires a stream that supports seeking and writing.";
        internal static readonly string SplitSpanned = "Split or spanned archives are not supported.";
        internal static readonly string UnexpectedEndOfStream = "Zip file corrupt: unexpected end of stream reached.";
        internal static readonly string UnsupportedCompression = "The archive entry was compressed using an unsupported compression method.";
        internal static readonly string UnsupportedCompressionMethod = "The archive entry was compressed using {0} and is not supported.";
        internal static readonly string WritingNotSupported = "This stream from ZipArchiveEntry does not support writing.";
        internal static readonly string Zip64EOCDNotWhereExpected = "Zip 64 End of Central Directory Record not where indicated.";
        internal static readonly string EOF_ReadBeyondEOF = "Unable to read beyond the end of the stream.";
        internal static readonly string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
        internal static readonly string ArgumentOutOfRange_BinaryReaderFillBuffer = "The number of bytes requested does not fit into BinaryReader's internal buffer.";
    }
}

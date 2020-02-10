// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace FMScanner.FastZipReader
{
    internal static class SR
    {
        #region Strings

        // Not localizable cause who the hell cares
        internal static string GenericInvalidData = "Found invalid data while decoding.";
        internal static string InvalidArgumentOffsetCount = "Offset plus count is larger than the length of target array.";
        internal static string InvalidBlockLength = "Block length does not match with its complement.";
        internal static string InvalidHuffmanData = "Failed to construct a huffman tree using the length array. The stream might be corrupted.";
        internal static string NotSupported = "This operation is not supported.";
        internal static string NotSupported_UnreadableStream = "Stream does not support reading.";
        internal static string ObjectDisposed_StreamClosed = "Cannot access a closed stream.";
        internal static string UnknownBlockType = "Unknown block type. Stream might be corrupted.";
        internal static string UnknownState = "Decoder is in some unknown state. This might be caused by corrupted data.";
        internal static string CDCorrupt = "Central Directory corrupt.";
        internal static string CentralDirectoryInvalid = "Central Directory is invalid.";
        internal static string EntryNameEncodingNotSupported = "The specified entry name encoding is not supported.";
        internal static string EOCDNotFound = "End of Central Directory record could not be found.";
        internal static string FieldTooBigCompressedSize = "Compressed Size cannot be held in an Int64.";
        internal static string FieldTooBigLocalHeaderOffset = "Local Header Offset cannot be held in an Int64.";
        internal static string FieldTooBigNumEntries = "Number of Entries cannot be held in an Int64.";
        internal static string FieldTooBigOffsetToCD = "Offset to Central Directory cannot be held in an Int64.";
        internal static string FieldTooBigOffsetToZip64EOCD = "Offset to Zip64 End Of Central Directory record cannot be held in an Int64.";
        internal static string FieldTooBigStartDiskNumber = "Start Disk Number cannot be held in an Int64.";
        internal static string FieldTooBigUncompressedSize = "Uncompressed Size cannot be held in an Int64.";
        internal static string HiddenStreamName = "A stream from ZipArchiveEntry has been disposed.";
        internal static string LocalFileHeaderCorrupt = "A local file header is corrupt.";
        internal static string NumEntriesWrong = "Number of entries expected in End Of Central Directory does not correspond to number of entries in Central Directory.";
        internal static string ReadingNotSupported = "This stream from ZipArchiveEntry does not support reading.";
        internal static string ReadModeCapabilities = "Cannot use read mode on a non-readable stream.";
        internal static string SeekingNotSupported = "This stream from ZipArchiveEntry does not support seeking.";
        internal static string SetLengthRequiresSeekingAndWriting = "SetLength requires a stream that supports seeking and writing.";
        internal static string SplitSpanned = "Split or spanned archives are not supported.";
        internal static string UnexpectedEndOfStream = "Zip file corrupt: unexpected end of stream reached.";
        internal static string UnsupportedCompression = "The archive entry was compressed using an unsupported compression method.";
        internal static string UnsupportedCompressionMethod = "The archive entry was compressed using {0} and is not supported.";
        internal static string WritingNotSupported = "This stream from ZipArchiveEntry does not support writing.";
        internal static string Zip64EOCDNotWhereExpected = "Zip 64 End of Central Directory Record not where indicated.";

        #endregion

        internal static string Format(string resourceFormat, object p1) => string.Format(resourceFormat, p1);
    }
}

//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;

namespace AL_Common;

// We don't want these as constants because we don't want them duplicated everywhere bloating things up.
// We don't care about the infinitesimal perf increase of constants either, because these are error messages
// and will rarely be hit anyway.
[SuppressMessage("ReSharper", "ConvertToConstant.Global")]
public static class SR
{
    // Not localizable cause who the hell cares
    public static readonly string GenericInvalidData = "Found invalid data while decoding.";
    public static readonly string InvalidArgumentOffsetCount = "Offset plus count is larger than the length of target array.";
    public static readonly string InvalidBlockLength = "Block length does not match with its complement.";
    public static readonly string InvalidHuffmanData = "Failed to construct a huffman tree using the length array. The stream might be corrupted.";
    public static readonly string NotSupported = "This operation is not supported.";
    public static readonly string NotSupported_UnreadableStream = "Stream does not support reading.";
    public static readonly string ObjectDisposed_StreamClosed = "Cannot access a closed stream.";
    public static readonly string UnknownBlockType = "Unknown block type. Stream might be corrupted.";
    public static readonly string UnknownState = "Decoder is in some unknown state. This might be caused by corrupted data.";
    public static readonly string CDCorrupt = "Central Directory corrupt.";
    public static readonly string CentralDirectoryInvalid = "Central Directory is invalid.";
    public static readonly string EOCDNotFound = "End of Central Directory record could not be found.";
    public static readonly string FieldTooBigCompressedSize = "Compressed Size cannot be held in an Int64.";
    public static readonly string FieldTooBigLocalHeaderOffset = "Local Header Offset cannot be held in an Int64.";
    public static readonly string FieldTooBigNumEntries = "Number of Entries cannot be held in an Int64.";
    public static readonly string FieldTooBigOffsetToCD = "Offset to Central Directory cannot be held in an Int64.";
    public static readonly string FieldTooBigOffsetToZip64EOCD = "Offset to Zip64 End Of Central Directory record cannot be held in an Int64.";
    public static readonly string FieldTooBigUncompressedSize = "Uncompressed Size cannot be held in an Int64.";
    public static readonly string LocalFileHeaderCorrupt = "A local file header is corrupt.";
    public static readonly string NumEntriesWrong = "Number of entries expected in End Of Central Directory does not correspond to number of entries in Central Directory.";
    public static readonly string ReadingNotSupported = "This stream from ZipArchiveEntry does not support reading.";
    public static readonly string ReadModeCapabilities = "Cannot use read mode on a non-readable stream.";
    public static readonly string SeekingNotSupported = "This stream from ZipArchiveEntry does not support seeking.";
    public static readonly string SetLengthRequiresSeekingAndWriting = "SetLength requires a stream that supports seeking and writing.";
    public static readonly string SplitSpanned = "Split or spanned archives are not supported.";
    public static readonly string UnexpectedEndOfStream = "Zip file corrupt: unexpected end of stream reached.";
    public static readonly string UnsupportedCompressionMethod = "The archive entry was compressed using {0} and is not supported.";
    public static readonly string WritingNotSupported = "This stream from ZipArchiveEntry does not support writing.";
    public static readonly string Zip64EOCDNotWhereExpected = "Zip 64 End of Central Directory Record not where indicated.";
    public static readonly string EOF_ReadBeyondEOF = "Unable to read beyond the end of the stream.";
    public static readonly string IO_NoPermissionToDirectoryName = "<Path discovery permission to the specified directory was denied.>";
    public static readonly string FileNotFound = "Unable to find the specified file.";
    public static readonly string FileNotFound_FileName = "Could not find file '{0}'.";
    public static readonly string PathNotFound_NoPathName = "Could not find a part of the path.";
    public static readonly string PathNotFound_Path = "Could not find a part of the path '{0}'.";
    public static readonly string UnauthorizedAccess_IODenied_NoPathName = "Access to the path is denied.";
    public static readonly string UnauthorizedAccess_IODenied_Path = "Access to the path '{0}' is denied.";
    public static readonly string IO_AlreadyExists_Name = "Cannot create \"{0}\" because a file or directory with the same name already exists.";
    public static readonly string PathTooLong = "The specified path, file name, or both are too long. The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.";
    public static readonly string PathTooLong_Path = "The path '{0}' is too long, or a component of the specified path is too long.";
    public static readonly string DriveNotFound_Drive = "Could not find the drive '{0}'. The drive might not be ready or might not be mapped.";
    public static readonly string IO_SharingViolation_NoFileName = "The process cannot access the file because it is being used by another process.";
    public static readonly string IO_SharingViolation_File = "The process cannot access the file '{0}' because it is being used by another process.";
    public static readonly string IO_FileExists_Name = "The file '{0}' already exists.";
    public static readonly string Arg_ArrayPlusOffTooSmall = "Destination array is not long enough to copy all the items in the collection. Check array index and length.";
    public static readonly string ArgumentOutOfRange_IndexMustBeLess = "Index was out of range. Must be non-negative and less than the size of the collection.";
    public static readonly string ArgumentOutOfRange_IndexMustBeLessOrEqual = "Index was out of range. Must be non-negative and less than or equal to the size of the collection.";
    public static readonly string ObjectDisposed_StreamReaderCustomClosed = "Cannot read from a closed stream reader.";
    public static readonly string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";
    public static readonly string Arg_InvalidHandle = "Invalid handle.";
    public static readonly string ObjectDisposed_FileClosed = "Cannot access a closed file.";
    public static readonly string NotSupported_UnseekableStream = "Stream does not support seeking.";
    public static readonly string EntryNameEncodingNotSupported = "Entry name encoding not supported.";
    public static readonly string IO_FileTooLong2GB = "The file is too long. This operation is currently limited to supporting files less than 2 gigabytes in size.";
    public static readonly string Argument_EmptyString = "The value cannot be an empty string.";
    public static readonly string ArgumentOutOfRange_Enum = "Enum value was out of legal range.";
    public static readonly string Argument_InvalidAppendMode = "Append access can be requested only in write-only mode.";
    public static readonly string Argument_InvalidFileModeAndAccessCombo = "Combining FileMode: {0} with FileAccess: {1} is invalid.";
    public static readonly string IO_DiskFull_Path_AllocationSize = "Failed to create '{0}' with allocation size '{1}' because the disk was full.";
    public static readonly string IO_FileTooLarge_Path_AllocationSize = "Failed to create '{0}' with allocation size '{1}' because the file was too large.";
    public static readonly string Argument_InvalidPreallocateAccess = "Preallocation size can be requested only in write mode.";
    public static readonly string Argument_InvalidPreallocateMode = "Preallocation size can be requested only for new files.";
    public static readonly string IO_UnknownFileName = "[Unknown]";
    public static readonly string IO_SeekAppendOverwrite = "Unable seek backward to overwrite data that previously existed in a file opened in Append mode.";
    public static readonly string IO_SetLengthAppendTruncate = "Unable to truncate data that previously existed in a file opened in Append mode.";
    public static readonly string ArgumentOutOfRange_FileLengthTooBig = "Specified file length was too large for the file system.";
    public static readonly string NotSupported_UnwritableStream = "Stream does not support writing.";
    public static readonly string InvalidOperation_NullArray = "The underlying array is null.";
    public static readonly string Argument_InvalidOffLen = "Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection.";
#if ENABLE_UNUSED
    public static readonly string Argument_InvalidSeekOrigin = "Invalid seek origin.";
    public static readonly string ArgumentOutOfRange_Generic_MustBeNonNegativeNonZero = "{0} ('{1}') must be a non-negative and non-zero value.";
    public static readonly string Argument_EmptyOrWhiteSpaceString = "The value cannot be an empty string or composed entirely of whitespace.";
    public static readonly string ArgumentNull_Buffer = "Buffer cannot be null.";
#endif

    internal static string Format(string resourceFormat, object? p1, object? p2) => string.Format(resourceFormat, p1, p2);
}

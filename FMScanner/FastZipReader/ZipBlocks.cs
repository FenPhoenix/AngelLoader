// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace FMScanner.FastZipReader
{
    /*
    Fen's note(@NET5 vs. Framework file I/O perf hack):
    Wherever possible, we read instead of calling Seek or setting Position, because in Framework, that causes an
    expensive system call to SetFilePointer(), whereas in .NET 6+ they reworked it to not have to do that.
    Note that all the SetFilePointer() calls are in aggregate VERY expensive, so much so that reads are hugely
    faster even with their overhead. But if we are ever able to move to .NET 6+, we should change all dummy reads
    back to seeks.
    */

    // All blocks.TryReadBlock do a check to see if signature is correct. Generic extra field is slightly different
    // all of the TryReadBlocks will throw if there are not enough bytes in the stream

    internal struct ZipGenericExtraField
    {
        internal ushort Tag;
        // returns size of data, not of the entire block
        internal ushort Size;
        internal byte[] Data;

        // shouldn't ever read the byte at position endExtraField
        // assumes we are positioned at the beginning of an extra field subfield
        internal static bool TryReadBlock(
            Stream stream,
            long endExtraField,
            ZipReusableBundle bundle,
            out ZipGenericExtraField field)
        {
            field = new ZipGenericExtraField();

            // not enough bytes to read tag + size
            if (endExtraField - stream.Position < 4) return false;

            field.Tag = bundle.ReadUInt16(stream);
            field.Size = bundle.ReadUInt16(stream);

            // not enough bytes to read the data
            if (endExtraField - stream.Position < field.Size) return false;

            field.Data = ZipReusableBundle.ReadBytes(stream, field.Size);

            return true;
        }
    }

    internal struct Zip64ExtraField
    {
        // Size is size of the record not including the tag or size fields
        // If the extra field is going in the local header, it cannot include only
        // one of uncompressed/compressed size

        private const ushort TagConstant = 1;

        private ushort _size;

        internal long? UncompressedSize;

        internal long? CompressedSize;

        internal long? LocalHeaderOffset;

        internal int? StartDiskNumber;

        // There is a small chance that something very weird could happen here. The code calling into this function
        // will ask for a value from the extra field if the field was masked with FF's. It's theoretically possible
        // that a field was FF's legitimately, and the writer didn't decide to write the corresponding extra field.
        // Also, at the same time, other fields were masked with FF's to indicate looking in the zip64 record.
        // Then, the search for the zip64 record will fail because the expected size is wrong,
        // and a nulled out Zip64ExtraField will be returned. Thus, even though there was Zip64 data,
        // it will not be used. It is questionable whether this situation is possible to detect

        // unlike the other functions that have try-pattern semantics, these functions always return a
        // Zip64ExtraField. If a Zip64 extra field actually doesn't exist, all of the fields in the
        // returned struct will be null
        //
        // If there are more than one Zip64 extra fields, we take the first one that has the expected size
        //
        internal static Zip64ExtraField GetJustZip64Block(
            Stream extraFieldStream,
            bool readUncompressedSize,
            bool readCompressedSize,
            bool readLocalHeaderOffset,
            bool readStartDiskNumber,
            ZipReusableBundle bundle)
        {
            Zip64ExtraField zip64Field;
            while (ZipGenericExtraField.TryReadBlock(
                       extraFieldStream,
                       extraFieldStream.Length,
                       bundle,
                       out var currentExtraField))
            {
                if (TryGetZip64BlockFromGenericExtraField(currentExtraField, readUncompressedSize,
                        readCompressedSize, readLocalHeaderOffset, readStartDiskNumber, out zip64Field))
                {
                    return zip64Field;
                }
            }

            zip64Field = new Zip64ExtraField
            {
                CompressedSize = null,
                UncompressedSize = null,
                LocalHeaderOffset = null,
                StartDiskNumber = null
            };

            return zip64Field;
        }

        private static bool TryGetZip64BlockFromGenericExtraField(
            ZipGenericExtraField extraField,
            bool readUncompressedSize,
            bool readCompressedSize,
            bool readLocalHeaderOffset,
            bool readStartDiskNumber,
            out Zip64ExtraField zip64Block)
        {
            zip64Block = new Zip64ExtraField
            {
                CompressedSize = null,
                UncompressedSize = null,
                LocalHeaderOffset = null,
                StartDiskNumber = null
            };

            if (extraField.Tag != TagConstant) return false;

            zip64Block._size = extraField.Size;

            ushort expectedSize = 0;

            if (readUncompressedSize) expectedSize += 8;
            if (readCompressedSize) expectedSize += 8;
            if (readLocalHeaderOffset) expectedSize += 8;
            if (readStartDiskNumber) expectedSize += 4;

            // if it is not the expected size, perhaps there is another extra field that matches
            if (expectedSize != zip64Block._size) return false;

            // No need for a MemoryStream, just read straight out of the array
            int arrayIndex = 0;
            if (readUncompressedSize)
            {
                zip64Block.UncompressedSize = BitConverter.ToInt64(extraField.Data, arrayIndex);
                arrayIndex += 8;
            }
            if (readCompressedSize)
            {
                zip64Block.CompressedSize = BitConverter.ToInt64(extraField.Data, arrayIndex);
                arrayIndex += 8;
            }
            if (readLocalHeaderOffset)
            {
                zip64Block.LocalHeaderOffset = BitConverter.ToInt64(extraField.Data, arrayIndex);
                arrayIndex += 8;
            }
            if (readStartDiskNumber)
            {
                zip64Block.StartDiskNumber = BitConverter.ToInt32(extraField.Data, arrayIndex);
            }

            // original values are unsigned, so implies value is too big to fit in signed integer
            if (zip64Block.UncompressedSize < 0) throw new InvalidDataException(SR.FieldTooBigUncompressedSize);
            if (zip64Block.CompressedSize < 0) throw new InvalidDataException(SR.FieldTooBigCompressedSize);
            if (zip64Block.LocalHeaderOffset < 0) throw new InvalidDataException(SR.FieldTooBigLocalHeaderOffset);
            if (zip64Block.StartDiskNumber < 0) throw new InvalidDataException(SR.FieldTooBigStartDiskNumber);

            return true;
        }
    }

    internal struct Zip64EndOfCentralDirectoryLocator
    {
        internal const uint SignatureConstant = 0x07064B50;
        internal const int SizeOfBlockWithoutSignature = 16;

        internal ulong OffsetOfZip64EOCD;

        internal static bool TryReadBlock(
            Stream stream,
            ZipReusableBundle bundle,
            out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();

            if (bundle.ReadUInt32(stream) != SignatureConstant) return false;

            bundle.ReadUInt32(stream); // NumberOfDiskWithZip64EOCD
            zip64EOCDLocator.OffsetOfZip64EOCD = bundle.ReadUInt64(stream);
            bundle.ReadUInt32(stream); // TotalNumberOfDisks
            return true;
        }
    }

    internal struct Zip64EndOfCentralDirectoryRecord
    {
        private const uint SignatureConstant = 0x06064B50;

        internal uint NumberOfThisDisk;
        internal ulong NumberOfEntriesOnThisDisk;
        internal ulong NumberOfEntriesTotal;
        internal ulong OffsetOfCentralDirectory;

        internal static bool TryReadBlock(
            Stream stream,
            ZipReusableBundle bundle,
            out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();

            if (bundle.ReadUInt32(stream) != SignatureConstant) return false;

            bundle.ReadUInt64(stream); // SizeOfThisRecord
            bundle.ReadUInt16(stream); // VersionMadeBy
            bundle.ReadUInt16(stream); // VersionNeededToExtract
            zip64EOCDRecord.NumberOfThisDisk = bundle.ReadUInt32(stream);
            bundle.ReadUInt32(stream); // NumberOfDiskWithStartOfCD
            zip64EOCDRecord.NumberOfEntriesOnThisDisk = bundle.ReadUInt64(stream);
            zip64EOCDRecord.NumberOfEntriesTotal = bundle.ReadUInt64(stream);
            bundle.ReadUInt64(stream); // SizeOfCentralDirectory
            zip64EOCDRecord.OffsetOfCentralDirectory = bundle.ReadUInt64(stream);

            return true;
        }
    }

    internal readonly struct ZipLocalFileHeader
    {
        private const uint SignatureConstant = 0x04034B50;

        // will not throw end of stream exception
        internal static bool TrySkipBlock(Stream stream, ZipReusableBundle bundle)
        {
            const int offsetToFilenameLength = 22; // from the point after the signature

            if (bundle.ReadUInt32(stream) != SignatureConstant) return false;
            if (stream.Length < stream.Position + offsetToFilenameLength) return false;

            stream.Seek(offsetToFilenameLength, SeekOrigin.Current);

            ushort filenameLength = bundle.ReadUInt16(stream);
            ushort extraFieldLength = bundle.ReadUInt16(stream);

            if (stream.Length < stream.Position + filenameLength + extraFieldLength)
            {
                return false;
            }

            stream.Seek(filenameLength + extraFieldLength, SeekOrigin.Current);

            return true;
        }
    }

    internal struct ZipCentralDirectoryFileHeader
    {
        private const uint SignatureConstant = 0x02014B50;
        internal ushort CompressionMethod;
        internal uint LastModified;
        internal long CompressedSize;
        internal long UncompressedSize;
        private ushort FilenameLength;
        private ushort ExtraFieldLength;
        private ushort FileCommentLength;
        internal int DiskNumberStart;
        internal long RelativeOffsetOfLocalHeader;

        internal byte[]? Filename;

        // if saveExtraFieldsAndComments is false, FileComment and ExtraFields will be null
        // in either case, the zip64 extra field info will be incorporated into other fields
        internal static bool TryReadBlock(
            Stream stream,
            ZipReusableBundle bundle,
            out ZipCentralDirectoryFileHeader header)
        {
            header = new ZipCentralDirectoryFileHeader();

            if (bundle.ReadUInt32(stream) != SignatureConstant) return false;

            bundle.ReadByte(stream); // VersionMadeBySpecification
            bundle.ReadByte(stream); // VersionMadeByCompatibility
            bundle.ReadUInt16(stream); // VersionNeededToExtract
            bundle.ReadUInt16(stream); // GeneralPurposeBitFlag
            header.CompressionMethod = bundle.ReadUInt16(stream);
            header.LastModified = bundle.ReadUInt32(stream);
            bundle.ReadUInt32(stream); // Crc32
            uint compressedSizeSmall = bundle.ReadUInt32(stream);
            uint uncompressedSizeSmall = bundle.ReadUInt32(stream);
            header.FilenameLength = bundle.ReadUInt16(stream);
            header.ExtraFieldLength = bundle.ReadUInt16(stream);
            header.FileCommentLength = bundle.ReadUInt16(stream);
            ushort diskNumberStartSmall = bundle.ReadUInt16(stream);
            bundle.ReadUInt16(stream); // InternalFileAttributes
            bundle.ReadUInt32(stream); // ExternalFileAttributes
            uint relativeOffsetOfLocalHeaderSmall = bundle.ReadUInt32(stream);

            header.Filename = ZipReusableBundle.ReadBytes(stream, header.FilenameLength);

            bool uncompressedSizeInZip64 = uncompressedSizeSmall == ZipHelpers.Mask32Bit;
            bool compressedSizeInZip64 = compressedSizeSmall == ZipHelpers.Mask32Bit;
            bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == ZipHelpers.Mask32Bit;
            bool diskNumberStartInZip64 = diskNumberStartSmall == ZipHelpers.Mask16Bit;

            long endExtraFields = stream.Position + header.ExtraFieldLength;

            bundle.ArchiveSubReadStream.Set(stream.Position, header.ExtraFieldLength);

            Zip64ExtraField zip64 = Zip64ExtraField.GetJustZip64Block(
                bundle.ArchiveSubReadStream,
                uncompressedSizeInZip64,
                compressedSizeInZip64,
                relativeOffsetInZip64,
                diskNumberStartInZip64,
                bundle);

            // There are zip files that have malformed ExtraField blocks in which GetJustZip64Block() silently
            // bails out without reading all the way to the end of the ExtraField block. Thus we must force the
            // stream's position to the proper place.

            stream.AdvanceToPosition(endExtraFields + header.FileCommentLength, bundle);

            header.UncompressedSize = zip64.UncompressedSize ?? uncompressedSizeSmall;
            header.CompressedSize = zip64.CompressedSize ?? compressedSizeSmall;
            header.RelativeOffsetOfLocalHeader = zip64.LocalHeaderOffset ?? relativeOffsetOfLocalHeaderSmall;
            header.DiskNumberStart = zip64.StartDiskNumber ?? diskNumberStartSmall;

            return true;
        }
    }

    internal struct ZipEndOfCentralDirectoryBlock
    {
        internal const uint SignatureConstant = 0x06054B50;
        internal const int SizeOfBlockWithoutSignature = 18;
        internal ushort NumberOfThisDisk;
        internal ushort NumberOfTheDiskWithTheStartOfTheCentralDirectory;
        internal ushort NumberOfEntriesInTheCentralDirectoryOnThisDisk;
        internal ushort NumberOfEntriesInTheCentralDirectory;
        internal uint OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;

        internal static bool TryReadBlock(
            Stream stream,
            ZipReusableBundle bundle,
            out ZipEndOfCentralDirectoryBlock eocdBlock)
        {
            eocdBlock = new ZipEndOfCentralDirectoryBlock();
            if (bundle.ReadUInt32(stream) != SignatureConstant) return false;

            eocdBlock.NumberOfThisDisk = bundle.ReadUInt16(stream);
            eocdBlock.NumberOfTheDiskWithTheStartOfTheCentralDirectory = bundle.ReadUInt16(stream);
            eocdBlock.NumberOfEntriesInTheCentralDirectoryOnThisDisk = bundle.ReadUInt16(stream);
            eocdBlock.NumberOfEntriesInTheCentralDirectory = bundle.ReadUInt16(stream);
            bundle.ReadUInt32(stream); // SizeOfCentralDirectory
            eocdBlock.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = bundle.ReadUInt32(stream);

            ushort commentLength = bundle.ReadUInt16(stream);
            stream.Seek(commentLength, SeekOrigin.Current); // ArchiveComment

            return true;
        }
    }
}

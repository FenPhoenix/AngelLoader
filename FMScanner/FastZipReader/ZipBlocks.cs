// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Text;

namespace FMScanner.FastZipReader
{
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
        internal static bool TryReadBlock(Stream stream, long endExtraField, out ZipGenericExtraField field)
        {
            field = new ZipGenericExtraField();

            // not enough bytes to read tag + size
            if (endExtraField - stream.Position < 4) return false;

            field.Tag = BinRead.ReadUInt16(stream);
            field.Size = BinRead.ReadUInt16(stream);

            // not enough bytes to read the data
            if (endExtraField - stream.Position < field.Size) return false;

            field.Data = BinRead.ReadBytes(stream, field.Size);
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
        internal static Zip64ExtraField GetJustZip64Block(Stream extraFieldStream,
            bool readUncompressedSize, bool readCompressedSize,
            bool readLocalHeaderOffset, bool readStartDiskNumber)
        {
            Zip64ExtraField zip64Field;
            while (ZipGenericExtraField.TryReadBlock(extraFieldStream, extraFieldStream.Length, out var currentExtraField))
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

        private static bool TryGetZip64BlockFromGenericExtraField(ZipGenericExtraField extraField,
            bool readUncompressedSize, bool readCompressedSize,
            bool readLocalHeaderOffset, bool readStartDiskNumber,
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

            // this pattern needed because nested using blocks trigger CA2202
            MemoryStream_Custom? ms = null;
            try
            {
                ms = new MemoryStream_Custom(extraField.Data);

                zip64Block._size = extraField.Size;

                ushort expectedSize = 0;

                if (readUncompressedSize) expectedSize += 8;
                if (readCompressedSize) expectedSize += 8;
                if (readLocalHeaderOffset) expectedSize += 8;
                if (readStartDiskNumber) expectedSize += 4;

                // if it is not the expected size, perhaps there is another extra field that matches
                if (expectedSize != zip64Block._size) return false;

                if (readUncompressedSize) zip64Block.UncompressedSize = BinRead.ReadInt64(ms);
                if (readCompressedSize) zip64Block.CompressedSize = BinRead.ReadInt64(ms);
                if (readLocalHeaderOffset) zip64Block.LocalHeaderOffset = BinRead.ReadInt64(ms);
                if (readStartDiskNumber) zip64Block.StartDiskNumber = BinRead.ReadInt32(ms);

                // original values are unsigned, so implies value is too big to fit in signed integer
                if (zip64Block.UncompressedSize < 0) throw new InvalidDataException(SR.FieldTooBigUncompressedSize);
                if (zip64Block.CompressedSize < 0) throw new InvalidDataException(SR.FieldTooBigCompressedSize);
                if (zip64Block.LocalHeaderOffset < 0) throw new InvalidDataException(SR.FieldTooBigLocalHeaderOffset);
                if (zip64Block.StartDiskNumber < 0) throw new InvalidDataException(SR.FieldTooBigStartDiskNumber);

                return true;
            }
            finally
            {
                ms?.Dispose();
            }
        }
    }

    internal struct Zip64EndOfCentralDirectoryLocator
    {
        internal const uint SignatureConstant = 0x07064B50;
        internal const int SizeOfBlockWithoutSignature = 16;

        internal ulong OffsetOfZip64EOCD;

        internal static bool TryReadBlock(Stream stream, out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();

            if (BinRead.ReadUInt32(stream) != SignatureConstant) return false;

            BinRead.ReadUInt32(stream); // NumberOfDiskWithZip64EOCD
            zip64EOCDLocator.OffsetOfZip64EOCD = BinRead.ReadUInt64(stream);
            BinRead.ReadUInt32(stream); // TotalNumberOfDisks
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

        internal static bool TryReadBlock(Stream stream, out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();

            if (BinRead.ReadUInt32(stream) != SignatureConstant) return false;

            BinRead.ReadUInt64(stream); // SizeOfThisRecord
            BinRead.ReadUInt16(stream); // VersionMadeBy
            BinRead.ReadUInt16(stream); // VersionNeededToExtract
            zip64EOCDRecord.NumberOfThisDisk = BinRead.ReadUInt32(stream);
            BinRead.ReadUInt32(stream); // NumberOfDiskWithStartOfCD
            zip64EOCDRecord.NumberOfEntriesOnThisDisk = BinRead.ReadUInt64(stream);
            zip64EOCDRecord.NumberOfEntriesTotal = BinRead.ReadUInt64(stream);
            BinRead.ReadUInt64(stream); // SizeOfCentralDirectory
            zip64EOCDRecord.OffsetOfCentralDirectory = BinRead.ReadUInt64(stream);

            return true;
        }
    }

    internal readonly struct ZipLocalFileHeader
    {
        private const uint SignatureConstant = 0x04034B50;

        // will not throw end of stream exception
        internal static bool TrySkipBlock(Stream stream)
        {
            const int offsetToFilenameLength = 22; // from the point after the signature

            if (BinRead.ReadUInt32(stream) != SignatureConstant) return false;
            if (stream.Length < stream.Position + offsetToFilenameLength) return false;

            stream.Seek(offsetToFilenameLength, SeekOrigin.Current);

            ushort filenameLength = BinRead.ReadUInt16(stream);
            ushort extraFieldLength = BinRead.ReadUInt16(stream);

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
        internal byte VersionMadeByCompatibility;
        internal ushort GeneralPurposeBitFlag;
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
        internal static bool TryReadBlock(ZipArchiveFast archive, bool fileNameNeeded, out ZipCentralDirectoryFileHeader header)
        {
            header = new ZipCentralDirectoryFileHeader();

            var stream = archive.ArchiveStream;

            if (BinRead.ReadUInt32(stream) != SignatureConstant) return false;

            BinRead.ReadByte(stream); // VersionMadeBySpecification
            header.VersionMadeByCompatibility = BinRead.ReadByte(stream);
            BinRead.ReadUInt16(stream); // VersionNeededToExtract
            header.GeneralPurposeBitFlag = BinRead.ReadUInt16(stream);
            header.CompressionMethod = BinRead.ReadUInt16(stream);
            header.LastModified = BinRead.ReadUInt32(stream);
            BinRead.ReadUInt32(stream); // Crc32
            uint compressedSizeSmall = BinRead.ReadUInt32(stream);
            uint uncompressedSizeSmall = BinRead.ReadUInt32(stream);
            header.FilenameLength = BinRead.ReadUInt16(stream);
            header.ExtraFieldLength = BinRead.ReadUInt16(stream);
            header.FileCommentLength = BinRead.ReadUInt16(stream);
            ushort diskNumberStartSmall = BinRead.ReadUInt16(stream);
            BinRead.ReadUInt16(stream); // InternalFileAttributes
            BinRead.ReadUInt32(stream); // ExternalFileAttributes
            uint relativeOffsetOfLocalHeaderSmall = BinRead.ReadUInt32(stream);

            if (fileNameNeeded)
            {
                header.Filename = BinRead.ReadBytes(stream, header.FilenameLength);
            }
            else
            {
                stream.Seek(header.FilenameLength, SeekOrigin.Current);
            }

            bool uncompressedSizeInZip64 = uncompressedSizeSmall == ZipHelper.Mask32Bit;
            bool compressedSizeInZip64 = compressedSizeSmall == ZipHelper.Mask32Bit;
            bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == ZipHelper.Mask32Bit;
            bool diskNumberStartInZip64 = diskNumberStartSmall == ZipHelper.Mask16Bit;

            Zip64ExtraField zip64;

            long endExtraFields = stream.Position + header.ExtraFieldLength;

            archive.ArchiveSubReadStream.Set(stream.Position, header.ExtraFieldLength);

            zip64 = Zip64ExtraField.GetJustZip64Block(archive.ArchiveSubReadStream,
                uncompressedSizeInZip64, compressedSizeInZip64,
                relativeOffsetInZip64, diskNumberStartInZip64);

            // There are zip files that have malformed ExtraField blocks in which GetJustZip64Block() silently
            // bails out without reading all the way to the end of the ExtraField block. Thus we must force the
            // stream's position to the proper place.

            // Fen's note: Original did a seek here, which for some reason is like 300x slower than a read, and
            // also inexplicably causes ReadUInt32() to be 4x as slow and/or occur 4x as often(?!)
            // Buffer alignments...? I dunno. Anyway. Speed.
            // Also maybe not a good idea to use something that's faster when I don't know why it's faster.
            // But my results are the same as the old method, so herpaderp.
            stream.AdvanceToPosition(endExtraFields + header.FileCommentLength);

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

        internal static bool TryReadBlock(Stream stream, out ZipEndOfCentralDirectoryBlock eocdBlock)
        {
            eocdBlock = new ZipEndOfCentralDirectoryBlock();
            if (BinRead.ReadUInt32(stream) != SignatureConstant) return false;

            eocdBlock.NumberOfThisDisk = BinRead.ReadUInt16(stream);
            eocdBlock.NumberOfTheDiskWithTheStartOfTheCentralDirectory = BinRead.ReadUInt16(stream);
            eocdBlock.NumberOfEntriesInTheCentralDirectoryOnThisDisk = BinRead.ReadUInt16(stream);
            eocdBlock.NumberOfEntriesInTheCentralDirectory = BinRead.ReadUInt16(stream);
            BinRead.ReadUInt32(stream); // SizeOfCentralDirectory
            eocdBlock.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = BinRead.ReadUInt32(stream);

            ushort commentLength = BinRead.ReadUInt16(stream);
            stream.Seek(commentLength, SeekOrigin.Current); // ArchiveComment

            return true;
        }
    }
}

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
        internal static bool TryReadBlock(BinaryReader_Custom reader, long endExtraField, out ZipGenericExtraField field)
        {
            field = new ZipGenericExtraField();

            // not enough bytes to read tag + size
            if (endExtraField - reader.BaseStream.Position < 4) return false;

            field.Tag = reader.ReadUInt16();
            field.Size = reader.ReadUInt16();

            // not enough bytes to read the data
            if (endExtraField - reader.BaseStream.Position < field.Size) return false;

            field.Data = reader.ReadBytes(field.Size);
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
            using (var reader = new BinaryReader_Custom(extraFieldStream))
            {
                while (ZipGenericExtraField.TryReadBlock(reader, extraFieldStream.Length, out var currentExtraField))
                {
                    if (TryGetZip64BlockFromGenericExtraField(currentExtraField, readUncompressedSize,
                                readCompressedSize, readLocalHeaderOffset, readStartDiskNumber, out zip64Field))
                    {
                        return zip64Field;
                    }
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
                using var reader = new BinaryReader_Custom(ms);

                // Why did they do this and how does it still work?!
                ms = null;

                zip64Block._size = extraField.Size;

                ushort expectedSize = 0;

                if (readUncompressedSize) expectedSize += 8;
                if (readCompressedSize) expectedSize += 8;
                if (readLocalHeaderOffset) expectedSize += 8;
                if (readStartDiskNumber) expectedSize += 4;

                // if it is not the expected size, perhaps there is another extra field that matches
                if (expectedSize != zip64Block._size) return false;

                if (readUncompressedSize) zip64Block.UncompressedSize = reader.ReadInt64();
                if (readCompressedSize) zip64Block.CompressedSize = reader.ReadInt64();
                if (readLocalHeaderOffset) zip64Block.LocalHeaderOffset = reader.ReadInt64();
                if (readStartDiskNumber) zip64Block.StartDiskNumber = reader.ReadInt32();

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

        internal static bool TryReadBlock(BinaryReader_Custom reader, out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();

            if (reader.ReadUInt32() != SignatureConstant) return false;

            reader.ReadUInt32(); // NumberOfDiskWithZip64EOCD
            zip64EOCDLocator.OffsetOfZip64EOCD = reader.ReadUInt64();
            reader.ReadUInt32(); // TotalNumberOfDisks
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

        internal static bool TryReadBlock(BinaryReader_Custom reader, out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();

            if (reader.ReadUInt32() != SignatureConstant) return false;

            reader.ReadUInt64(); // SizeOfThisRecord
            reader.ReadUInt16(); // VersionMadeBy
            reader.ReadUInt16(); // VersionNeededToExtract
            zip64EOCDRecord.NumberOfThisDisk = reader.ReadUInt32();
            reader.ReadUInt32(); // NumberOfDiskWithStartOfCD
            zip64EOCDRecord.NumberOfEntriesOnThisDisk = reader.ReadUInt64();
            zip64EOCDRecord.NumberOfEntriesTotal = reader.ReadUInt64();
            reader.ReadUInt64(); // SizeOfCentralDirectory
            zip64EOCDRecord.OffsetOfCentralDirectory = reader.ReadUInt64();

            return true;
        }
    }

    internal readonly struct ZipLocalFileHeader
    {
        private const uint SignatureConstant = 0x04034B50;

        // will not throw end of stream exception
        internal static bool TrySkipBlock(BinaryReader_Custom reader)
        {
            const int offsetToFilenameLength = 22; // from the point after the signature

            if (reader.ReadUInt32() != SignatureConstant) return false;
            if (reader.BaseStream.Length < reader.BaseStream.Position + offsetToFilenameLength) return false;

            reader.BaseStream.Seek(offsetToFilenameLength, SeekOrigin.Current);

            ushort filenameLength = reader.ReadUInt16();
            ushort extraFieldLength = reader.ReadUInt16();

            if (reader.BaseStream.Length < reader.BaseStream.Position + filenameLength + extraFieldLength)
            {
                return false;
            }

            reader.BaseStream.Seek(filenameLength + extraFieldLength, SeekOrigin.Current);

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
        internal static bool TryReadBlock(BinaryReader_Custom reader, bool fileNameNeeded, out ZipCentralDirectoryFileHeader header)
        {
            header = new ZipCentralDirectoryFileHeader();

            if (reader.ReadUInt32() != SignatureConstant) return false;

            reader.ReadByte(); // VersionMadeBySpecification
            header.VersionMadeByCompatibility = reader.ReadByte();
            reader.ReadUInt16(); // VersionNeededToExtract
            header.GeneralPurposeBitFlag = reader.ReadUInt16();
            header.CompressionMethod = reader.ReadUInt16();
            header.LastModified = reader.ReadUInt32();
            reader.ReadUInt32(); // Crc32
            uint compressedSizeSmall = reader.ReadUInt32();
            uint uncompressedSizeSmall = reader.ReadUInt32();
            header.FilenameLength = reader.ReadUInt16();
            header.ExtraFieldLength = reader.ReadUInt16();
            header.FileCommentLength = reader.ReadUInt16();
            ushort diskNumberStartSmall = reader.ReadUInt16();
            reader.ReadUInt16(); // InternalFileAttributes
            reader.ReadUInt32(); // ExternalFileAttributes
            uint relativeOffsetOfLocalHeaderSmall = reader.ReadUInt32();

            if (fileNameNeeded)
            {
                header.Filename = reader.ReadBytes(header.FilenameLength);
            }
            else
            {
                reader.BaseStream.Seek(header.FilenameLength, SeekOrigin.Current);
            }

            bool uncompressedSizeInZip64 = uncompressedSizeSmall == ZipHelper.Mask32Bit;
            bool compressedSizeInZip64 = compressedSizeSmall == ZipHelper.Mask32Bit;
            bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == ZipHelper.Mask32Bit;
            bool diskNumberStartInZip64 = diskNumberStartSmall == ZipHelper.Mask16Bit;

            Zip64ExtraField zip64;

            long endExtraFields = reader.BaseStream.Position + header.ExtraFieldLength;
            using (Stream str = new SubReadStream(reader.BaseStream, reader.BaseStream.Position, header.ExtraFieldLength))
            {
                zip64 = Zip64ExtraField.GetJustZip64Block(str,
                    uncompressedSizeInZip64, compressedSizeInZip64,
                    relativeOffsetInZip64, diskNumberStartInZip64);
            }

            // There are zip files that have malformed ExtraField blocks in which GetJustZip64Block() silently
            // bails out without reading all the way to the end of the ExtraField block. Thus we must force the
            // stream's position to the proper place.

            // Fen's note: Original did a seek here, which for some reason is like 300x slower than a read, and
            // also inexplicably causes ReadUInt32() to be 4x as slow and/or occur 4x as often(?!)
            // Buffer alignments...? I dunno. Anyway. Speed.
            // Also maybe not a good idea to use something that's faster when I don't know why it's faster.
            // But my results are the same as the old method, so herpaderp.
            reader.BaseStream.AdvanceToPosition(endExtraFields + header.FileCommentLength);

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

        internal static bool TryReadBlock(BinaryReader_Custom reader, out ZipEndOfCentralDirectoryBlock eocdBlock)
        {
            eocdBlock = new ZipEndOfCentralDirectoryBlock();
            if (reader.ReadUInt32() != SignatureConstant) return false;

            eocdBlock.NumberOfThisDisk = reader.ReadUInt16();
            eocdBlock.NumberOfTheDiskWithTheStartOfTheCentralDirectory = reader.ReadUInt16();
            eocdBlock.NumberOfEntriesInTheCentralDirectoryOnThisDisk = reader.ReadUInt16();
            eocdBlock.NumberOfEntriesInTheCentralDirectory = reader.ReadUInt16();
            reader.ReadUInt32(); // SizeOfCentralDirectory
            eocdBlock.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = reader.ReadUInt32();

            ushort commentLength = reader.ReadUInt16();
            reader.BaseStream.Seek(commentLength, SeekOrigin.Current); // ArchiveComment

            return true;
        }
    }
}

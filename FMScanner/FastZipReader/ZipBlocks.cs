// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FMScanner.FastZipReader
{
    // All blocks.TryReadBlock do a check to see if signature is correct. Generic extra field is slightly different
    // all of the TryReadBlocks will throw if there are not enough bytes in the stream

    internal struct ZipGenericExtraField
    {
        internal ushort Tag { get; private set; }
        // returns size of data, not of the entire block
        internal ushort Size { get; private set; }
        internal byte[] Data { get; private set; }

        // shouldn't ever read the byte at position endExtraField
        // assumes we are positioned at the beginning of an extra field subfield
        internal static bool TryReadBlock(BinaryReader reader, long endExtraField, out ZipGenericExtraField field)
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
        private long? _uncompressedSize;
        private long? _compressedSize;
        private long? _localHeaderOffset;

        internal long? UncompressedSize
        {
            get => _uncompressedSize;
            set { _uncompressedSize = value; UpdateSize(); }
        }

        internal long? CompressedSize
        {
            get => _compressedSize;
            set { _compressedSize = value; UpdateSize(); }
        }

        internal long? LocalHeaderOffset
        {
            get => _localHeaderOffset;
            set { _localHeaderOffset = value; UpdateSize(); }
        }

        internal int? StartDiskNumber { get; private set; }

        private void UpdateSize()
        {
            _size = 0;
            if (_uncompressedSize != null) _size += 8;
            if (_compressedSize != null) _size += 8;
            if (_localHeaderOffset != null) _size += 8;
            if (StartDiskNumber != null) _size += 4;
        }

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
        // @Fen_added: Instantiate this once and pass it every time, otherwise we're just constructing and GC-ing
        // a default UTF8Encoding object a bazillion times.
        private static readonly Encoding _utf8EncodingNoBOM = new UTF8Encoding();
        internal static Zip64ExtraField GetJustZip64Block(Stream extraFieldStream,
            bool readUncompressedSize, bool readCompressedSize,
            bool readLocalHeaderOffset, bool readStartDiskNumber)
        {
            Zip64ExtraField zip64Field;
            using (var reader = new BinaryReader(extraFieldStream, _utf8EncodingNoBOM))
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
                _compressedSize = null,
                _uncompressedSize = null,
                _localHeaderOffset = null,
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
                _compressedSize = null,
                _uncompressedSize = null,
                _localHeaderOffset = null,
                StartDiskNumber = null
            };

            if (extraField.Tag != TagConstant) return false;

            // this pattern needed because nested using blocks trigger CA2202
            MemoryStream? ms = null;
            try
            {
                ms = new MemoryStream(extraField.Data);
                using var reader = new BinaryReader(ms);

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

                if (readUncompressedSize) zip64Block._uncompressedSize = reader.ReadInt64();
                if (readCompressedSize) zip64Block._compressedSize = reader.ReadInt64();
                if (readLocalHeaderOffset) zip64Block._localHeaderOffset = reader.ReadInt64();
                if (readStartDiskNumber) zip64Block.StartDiskNumber = reader.ReadInt32();

                // original values are unsigned, so implies value is too big to fit in signed integer
                if (zip64Block._uncompressedSize < 0) throw new InvalidDataException(SR.FieldTooBigUncompressedSize);
                if (zip64Block._compressedSize < 0) throw new InvalidDataException(SR.FieldTooBigCompressedSize);
                if (zip64Block._localHeaderOffset < 0) throw new InvalidDataException(SR.FieldTooBigLocalHeaderOffset);
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

        internal uint NumberOfDiskWithZip64EOCD;
        internal ulong OffsetOfZip64EOCD;
        internal uint TotalNumberOfDisks;

        internal static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();

            if (reader.ReadUInt32() != SignatureConstant) return false;

            zip64EOCDLocator.NumberOfDiskWithZip64EOCD = reader.ReadUInt32();
            zip64EOCDLocator.OffsetOfZip64EOCD = reader.ReadUInt64();
            zip64EOCDLocator.TotalNumberOfDisks = reader.ReadUInt32();
            return true;
        }
    }

    internal struct Zip64EndOfCentralDirectoryRecord
    {
        private const uint SignatureConstant = 0x06064B50;
        private const ulong NormalSize = 0x2C; // the size of the data excluding the size/signature fields if no extra data included

        internal ulong SizeOfThisRecord;
        internal ushort VersionMadeBy;
        internal ushort VersionNeededToExtract;
        internal uint NumberOfThisDisk;
        internal uint NumberOfDiskWithStartOfCD;
        internal ulong NumberOfEntriesOnThisDisk;
        internal ulong NumberOfEntriesTotal;
        internal ulong SizeOfCentralDirectory;
        internal ulong OffsetOfCentralDirectory;

        internal static bool TryReadBlock(BinaryReader reader, out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();

            if (reader.ReadUInt32() != SignatureConstant) return false;

            zip64EOCDRecord.SizeOfThisRecord = reader.ReadUInt64();
            zip64EOCDRecord.VersionMadeBy = reader.ReadUInt16();
            zip64EOCDRecord.VersionNeededToExtract = reader.ReadUInt16();
            zip64EOCDRecord.NumberOfThisDisk = reader.ReadUInt32();
            zip64EOCDRecord.NumberOfDiskWithStartOfCD = reader.ReadUInt32();
            zip64EOCDRecord.NumberOfEntriesOnThisDisk = reader.ReadUInt64();
            zip64EOCDRecord.NumberOfEntriesTotal = reader.ReadUInt64();
            zip64EOCDRecord.SizeOfCentralDirectory = reader.ReadUInt64();
            zip64EOCDRecord.OffsetOfCentralDirectory = reader.ReadUInt64();

            return true;
        }
    }

    internal readonly struct ZipLocalFileHeader
    {
        internal const uint SignatureConstant = 0x04034B50;

        // will not throw end of stream exception
        internal static bool TrySkipBlock(BinaryReader reader)
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
        internal const uint SignatureConstant = 0x02014B50;
        internal byte VersionMadeByCompatibility;
        internal byte VersionMadeBySpecification;
        internal ushort VersionNeededToExtract;
        internal ushort GeneralPurposeBitFlag;
        internal ushort CompressionMethod;
        internal uint LastModified;
        internal uint Crc32;
        internal long CompressedSize;
        internal long UncompressedSize;
        internal ushort FilenameLength;
        internal ushort ExtraFieldLength;
        internal ushort FileCommentLength;
        internal int DiskNumberStart;
        internal ushort InternalFileAttributes;
        internal uint ExternalFileAttributes;
        internal long RelativeOffsetOfLocalHeader;

        internal byte[] Filename;
        internal byte[]? FileComment;
        internal List<ZipGenericExtraField>? ExtraFields;

        // if saveExtraFieldsAndComments is false, FileComment and ExtraFields will be null
        // in either case, the zip64 extra field info will be incorporated into other fields
        internal static bool TryReadBlock(BinaryReader reader, out ZipCentralDirectoryFileHeader header)
        {
            header = new ZipCentralDirectoryFileHeader();

            if (reader.ReadUInt32() != SignatureConstant) return false;

            header.VersionMadeBySpecification = reader.ReadByte();
            header.VersionMadeByCompatibility = reader.ReadByte();
            header.VersionNeededToExtract = reader.ReadUInt16();
            header.GeneralPurposeBitFlag = reader.ReadUInt16();
            header.CompressionMethod = reader.ReadUInt16();
            header.LastModified = reader.ReadUInt32();
            header.Crc32 = reader.ReadUInt32();
            uint compressedSizeSmall = reader.ReadUInt32();
            uint uncompressedSizeSmall = reader.ReadUInt32();
            header.FilenameLength = reader.ReadUInt16();
            header.ExtraFieldLength = reader.ReadUInt16();
            header.FileCommentLength = reader.ReadUInt16();
            ushort diskNumberStartSmall = reader.ReadUInt16();
            header.InternalFileAttributes = reader.ReadUInt16();
            header.ExternalFileAttributes = reader.ReadUInt32();
            uint relativeOffsetOfLocalHeaderSmall = reader.ReadUInt32();

            header.Filename = reader.ReadBytes(header.FilenameLength);

            bool uncompressedSizeInZip64 = uncompressedSizeSmall == ZipHelper.Mask32Bit;
            bool compressedSizeInZip64 = compressedSizeSmall == ZipHelper.Mask32Bit;
            bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == ZipHelper.Mask32Bit;
            bool diskNumberStartInZip64 = diskNumberStartSmall == ZipHelper.Mask16Bit;

            Zip64ExtraField zip64;

            long endExtraFields = reader.BaseStream.Position + header.ExtraFieldLength;
            using (Stream str = new SubReadStream(reader.BaseStream, reader.BaseStream.Position, header.ExtraFieldLength))
            {
                header.ExtraFields = null;
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

            header.FileComment = null;

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
        internal uint Signature;
        internal ushort NumberOfThisDisk;
        internal ushort NumberOfTheDiskWithTheStartOfTheCentralDirectory;
        internal ushort NumberOfEntriesInTheCentralDirectoryOnThisDisk;
        internal ushort NumberOfEntriesInTheCentralDirectory;
        internal uint SizeOfCentralDirectory;
        internal uint OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
        internal byte[] ArchiveComment;

        internal static bool TryReadBlock(BinaryReader reader, out ZipEndOfCentralDirectoryBlock eocdBlock)
        {
            eocdBlock = new ZipEndOfCentralDirectoryBlock();
            if (reader.ReadUInt32() != SignatureConstant) return false;

            eocdBlock.Signature = SignatureConstant;
            eocdBlock.NumberOfThisDisk = reader.ReadUInt16();
            eocdBlock.NumberOfTheDiskWithTheStartOfTheCentralDirectory = reader.ReadUInt16();
            eocdBlock.NumberOfEntriesInTheCentralDirectoryOnThisDisk = reader.ReadUInt16();
            eocdBlock.NumberOfEntriesInTheCentralDirectory = reader.ReadUInt16();
            eocdBlock.SizeOfCentralDirectory = reader.ReadUInt32();
            eocdBlock.OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = reader.ReadUInt32();

            ushort commentLength = reader.ReadUInt16();
            eocdBlock.ArchiveComment = reader.ReadBytes(commentLength);

            return true;
        }
    }
}

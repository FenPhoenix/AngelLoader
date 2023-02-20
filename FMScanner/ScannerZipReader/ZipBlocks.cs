// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using AL_Common;
using AL_Common.FastZipReader;

namespace FMScanner.ScannerZipReader;

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

internal readonly ref struct ZipGenericExtraField
{
    internal readonly ushort Tag;
    // returns size of data, not of the entire block
    internal readonly ushort DataSize;
    internal readonly byte[] Data = Array.Empty<byte>();

    private ZipGenericExtraField(ushort tag, ushort dataSize)
    {
        Tag = tag;
        DataSize = dataSize;
    }

    private ZipGenericExtraField(ushort tag, ushort dataSize, byte[] data)
    {
        Tag = tag;
        DataSize = dataSize;
        Data = data;
    }

    // shouldn't ever read the byte at position endExtraField
    // assumes we are positioned at the beginning of an extra field subfield
    internal static bool TryReadBlock(
        Stream stream,
        long endExtraField,
        ZipReusableBundleS bundle,
        out ZipGenericExtraField field)
    {
        // not enough bytes to read tag + size
        if (endExtraField - stream.Position < 4)
        {
            field = new ZipGenericExtraField();
            return false;
        }

        ushort tag = bundle.ReadUInt16(stream);
        ushort dataSize = bundle.ReadUInt16(stream);

        // not enough bytes to read the data
        if (endExtraField - stream.Position < dataSize)
        {
            field = new ZipGenericExtraField(tag: tag, dataSize: dataSize);
            return false;
        }

        stream.ReadAll(bundle.DataBuffer, 0, dataSize);

        field = new ZipGenericExtraField(tag: tag, dataSize: dataSize, data: bundle.DataBuffer);

        return true;
    }
}

internal readonly ref struct Zip64ExtraField
{
    // Size is size of the record not including the tag or size fields
    // If the extra field is going in the local header, it cannot include only
    // one of uncompressed/compressed size

    private const ushort TagConstant = 1;

    internal readonly long? UncompressedSize;
    internal readonly long? CompressedSize;
    internal readonly long? LocalHeaderOffset;
    internal readonly int? StartDiskNumber;

    private Zip64ExtraField(
        long? uncompressedSize,
        long? compressedSize,
        long? localHeaderOffset,
        int? startDiskNumber)
    {
        UncompressedSize = uncompressedSize;
        CompressedSize = compressedSize;
        LocalHeaderOffset = localHeaderOffset;
        StartDiskNumber = startDiskNumber;
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
    internal static Zip64ExtraField GetJustZip64Block(
        Stream extraFieldStream,
        bool readUncompressedSize,
        bool readCompressedSize,
        bool readLocalHeaderOffset,
        bool readStartDiskNumber,
        ZipReusableBundleS bundle)
    {
        while (ZipGenericExtraField.TryReadBlock(
                   stream: extraFieldStream,
                   endExtraField: extraFieldStream.Length,
                   bundle: bundle,
                   out ZipGenericExtraField currentExtraField))
        {
            if (TryGetZip64BlockFromGenericExtraField(
                    extraField: currentExtraField,
                    readUncompressedSize: readUncompressedSize,
                    readCompressedSize: readCompressedSize,
                    readLocalHeaderOffset: readLocalHeaderOffset,
                    readStartDiskNumber: readStartDiskNumber,
                    out Zip64ExtraField zip64Field))
            {
                return zip64Field;
            }
        }

        return new Zip64ExtraField();
    }

    private static bool TryGetZip64BlockFromGenericExtraField(
        ZipGenericExtraField extraField,
        bool readUncompressedSize,
        bool readCompressedSize,
        bool readLocalHeaderOffset,
        bool readStartDiskNumber,
        out Zip64ExtraField zip64Block)
    {
        if (extraField.Tag != TagConstant)
        {
            zip64Block = new Zip64ExtraField();
            return false;
        }

        ushort dataSize = extraField.DataSize;

        ushort expectedDataSize = 0;

        if (readUncompressedSize) expectedDataSize += 8;
        if (readCompressedSize) expectedDataSize += 8;
        if (readLocalHeaderOffset) expectedDataSize += 8;
        if (readStartDiskNumber) expectedDataSize += 4;

        // if it is not the expected size, perhaps there is another extra field that matches
        if (expectedDataSize != dataSize)
        {
            zip64Block = new Zip64ExtraField();
            return false;
        }

        long? uncompressedSize = null;
        long? compressedSize = null;
        long? localHeaderOffset = null;
        int? startDiskNumber = null;

        // No need for a MemoryStream, just read straight out of the array
        int arrayIndex = 0;
        if (readUncompressedSize)
        {
            uncompressedSize = ZipHelpers.ReadInt64(extraField.Data, dataSize, arrayIndex);
            arrayIndex += 8;
        }
        if (readCompressedSize)
        {
            compressedSize = ZipHelpers.ReadInt64(extraField.Data, dataSize, arrayIndex);
            arrayIndex += 8;
        }
        if (readLocalHeaderOffset)
        {
            localHeaderOffset = ZipHelpers.ReadInt64(extraField.Data, dataSize, arrayIndex);
            arrayIndex += 8;
        }
        if (readStartDiskNumber)
        {
            startDiskNumber = ZipHelpers.ReadInt32(extraField.Data, dataSize, arrayIndex);
        }

        // original values are unsigned, so implies value is too big to fit in signed integer
        if (uncompressedSize is < 0) throw new InvalidDataException(SR.FieldTooBigUncompressedSize);
        if (compressedSize is < 0) throw new InvalidDataException(SR.FieldTooBigCompressedSize);
        if (localHeaderOffset is < 0) throw new InvalidDataException(SR.FieldTooBigLocalHeaderOffset);
        if (startDiskNumber is < 0) throw new InvalidDataException(SR.FieldTooBigStartDiskNumber);

        zip64Block = new Zip64ExtraField(
            uncompressedSize: uncompressedSize,
            compressedSize: compressedSize,
            localHeaderOffset: localHeaderOffset,
            startDiskNumber: startDiskNumber
        );

        return true;
    }
}

internal readonly ref struct Zip64EndOfCentralDirectoryLocator
{
    internal const uint SignatureConstant = 0x07064B50;
    internal const int SizeOfBlockWithoutSignature = 16;

    internal readonly ulong OffsetOfZip64EOCD;

    private Zip64EndOfCentralDirectoryLocator(ulong offsetOfZip64EOCD)
    {
        OffsetOfZip64EOCD = offsetOfZip64EOCD;
    }

    internal static bool TryReadBlock(
        Stream stream,
        ZipReusableBundleS bundle,
        out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
    {
        if (bundle.ReadUInt32(stream) != SignatureConstant)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();
            return false;
        }

        bundle.ReadUInt32(stream); // NumberOfDiskWithZip64EOCD
        ulong offsetOfZip64EOCD = bundle.ReadUInt64(stream);
        bundle.ReadUInt32(stream); // TotalNumberOfDisks

        zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator(offsetOfZip64EOCD: offsetOfZip64EOCD);

        return true;
    }
}

internal readonly ref struct Zip64EndOfCentralDirectoryRecord
{
    private const uint SignatureConstant = 0x06064B50;

    internal readonly uint NumberOfThisDisk;
    internal readonly ulong NumberOfEntriesOnThisDisk;
    internal readonly ulong NumberOfEntriesTotal;
    internal readonly ulong OffsetOfCentralDirectory;

    private Zip64EndOfCentralDirectoryRecord(
        uint numberOfThisDisk,
        ulong numberOfEntriesOnThisDisk,
        ulong numberOfEntriesTotal,
        ulong offsetOfCentralDirectory)
    {
        NumberOfThisDisk = numberOfThisDisk;
        NumberOfEntriesOnThisDisk = numberOfEntriesOnThisDisk;
        NumberOfEntriesTotal = numberOfEntriesTotal;
        OffsetOfCentralDirectory = offsetOfCentralDirectory;
    }

    internal static bool TryReadBlock(
        Stream stream,
        ZipReusableBundleS bundle,
        out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
    {
        if (bundle.ReadUInt32(stream) != SignatureConstant)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();
            return false;
        }

        bundle.ReadUInt64(stream); // SizeOfThisRecord
        bundle.ReadUInt16(stream); // VersionMadeBy
        bundle.ReadUInt16(stream); // VersionNeededToExtract
        uint numberOfThisDisk = bundle.ReadUInt32(stream);
        bundle.ReadUInt32(stream); // NumberOfDiskWithStartOfCD
        ulong numberOfEntriesOnThisDisk = bundle.ReadUInt64(stream);
        ulong numberOfEntriesTotal = bundle.ReadUInt64(stream);
        bundle.ReadUInt64(stream); // SizeOfCentralDirectory
        ulong offsetOfCentralDirectory = bundle.ReadUInt64(stream);

        zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord(
            numberOfThisDisk: numberOfThisDisk,
            numberOfEntriesOnThisDisk: numberOfEntriesOnThisDisk,
            numberOfEntriesTotal: numberOfEntriesTotal,
            offsetOfCentralDirectory: offsetOfCentralDirectory);

        return true;
    }
}

internal readonly ref struct ZipLocalFileHeader
{
    private const uint SignatureConstant = 0x04034B50;

    // will not throw end of stream exception
    internal static bool TrySkipBlock(Stream stream, long streamLength, ZipReusableBundleS bundle)
    {
        const int offsetToFilenameLength = 22; // from the point after the signature

        if (bundle.ReadUInt32(stream) != SignatureConstant) return false;
        if (streamLength < stream.Position + offsetToFilenameLength) return false;

        stream.Seek(offsetToFilenameLength, SeekOrigin.Current);

        ushort filenameLength = bundle.ReadUInt16(stream);
        ushort extraFieldLength = bundle.ReadUInt16(stream);

        if (streamLength < stream.Position + filenameLength + extraFieldLength)
        {
            return false;
        }

        stream.Seek(filenameLength + extraFieldLength, SeekOrigin.Current);

        return true;
    }
}

internal readonly ref struct ZipCentralDirectoryFileHeader
{
    private const uint SignatureConstant = 0x02014B50;

    internal readonly ushort CompressionMethod;
    internal readonly uint LastModified;
    internal readonly long CompressedSize;
    internal readonly long UncompressedSize;
    internal readonly int DiskNumberStart;
    internal readonly long RelativeOffsetOfLocalHeader;

    internal readonly byte[] Filename = Array.Empty<byte>();
    internal readonly ushort FilenameLength;

    private ZipCentralDirectoryFileHeader(
        ushort compressionMethod,
        uint lastModified,
        long compressedSize,
        long uncompressedSize,
        int diskNumberStart,
        long relativeOffsetOfLocalHeader,
        byte[] filename,
        ushort filenameLength)
    {
        CompressionMethod = compressionMethod;
        LastModified = lastModified;
        CompressedSize = compressedSize;
        UncompressedSize = uncompressedSize;
        DiskNumberStart = diskNumberStart;
        RelativeOffsetOfLocalHeader = relativeOffsetOfLocalHeader;
        Filename = filename;
        FilenameLength = filenameLength;
    }

    // if saveExtraFieldsAndComments is false, FileComment and ExtraFields will be null
    // in either case, the zip64 extra field info will be incorporated into other fields
    internal static bool TryReadBlock(
        Stream stream,
        ZipReusableBundleS bundle,
        out ZipCentralDirectoryFileHeader header)
    {
        if (bundle.ReadUInt32(stream) != SignatureConstant)
        {
            header = new ZipCentralDirectoryFileHeader();
            return false;
        }

        bundle.ReadByte(stream); // VersionMadeBySpecification
        bundle.ReadByte(stream); // VersionMadeByCompatibility
        bundle.ReadUInt16(stream); // VersionNeededToExtract
        bundle.ReadUInt16(stream); // GeneralPurposeBitFlag
        ushort compressionMethod = bundle.ReadUInt16(stream);
        uint lastModified = bundle.ReadUInt32(stream);
        bundle.ReadUInt32(stream); // Crc32
        uint compressedSizeSmall = bundle.ReadUInt32(stream);
        uint uncompressedSizeSmall = bundle.ReadUInt32(stream);
        ushort filenameLength = bundle.ReadUInt16(stream);
        ushort extraFieldLength = bundle.ReadUInt16(stream);
        ushort fileCommentLength = bundle.ReadUInt16(stream);
        ushort diskNumberStartSmall = bundle.ReadUInt16(stream);
        bundle.ReadUInt16(stream); // InternalFileAttributes
        bundle.ReadUInt32(stream); // ExternalFileAttributes
        uint relativeOffsetOfLocalHeaderSmall = bundle.ReadUInt32(stream);

        stream.ReadAll(bundle.FilenameBuffer, 0, filenameLength);

        bool uncompressedSizeInZip64 = uncompressedSizeSmall == ZipHelpers.Mask32Bit;
        bool compressedSizeInZip64 = compressedSizeSmall == ZipHelpers.Mask32Bit;
        bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == ZipHelpers.Mask32Bit;
        bool diskNumberStartInZip64 = diskNumberStartSmall == ZipHelpers.Mask16Bit;

        long endExtraFields = stream.Position + extraFieldLength;

        bundle.ArchiveSubReadStream.Set(stream.Position, extraFieldLength);

        Zip64ExtraField zip64 = Zip64ExtraField.GetJustZip64Block(
            extraFieldStream: bundle.ArchiveSubReadStream,
            readUncompressedSize: uncompressedSizeInZip64,
            readCompressedSize: compressedSizeInZip64,
            readLocalHeaderOffset: relativeOffsetInZip64,
            readStartDiskNumber: diskNumberStartInZip64,
            bundle: bundle);

        // There are zip files that have malformed ExtraField blocks in which GetJustZip64Block() silently
        // bails out without reading all the way to the end of the ExtraField block. Thus we must force the
        // stream's position to the proper place.

        stream.AdvanceToPosition(endExtraFields + fileCommentLength, bundle);

        long uncompressedSize = zip64.UncompressedSize ?? uncompressedSizeSmall;
        long compressedSize = zip64.CompressedSize ?? compressedSizeSmall;
        long relativeOffsetOfLocalHeader = zip64.LocalHeaderOffset ?? relativeOffsetOfLocalHeaderSmall;
        int diskNumberStart = zip64.StartDiskNumber ?? diskNumberStartSmall;

        header = new ZipCentralDirectoryFileHeader(
            compressionMethod: compressionMethod,
            lastModified: lastModified,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            diskNumberStart: diskNumberStart,
            relativeOffsetOfLocalHeader: relativeOffsetOfLocalHeader,
            filename: bundle.FilenameBuffer,
            filenameLength: filenameLength
        );

        return true;
    }
}

internal readonly ref struct ZipEndOfCentralDirectoryBlock
{
    internal const uint SignatureConstant = 0x06054B50;
    internal const int SizeOfBlockWithoutSignature = 18;

    internal readonly ushort NumberOfThisDisk;
    internal readonly ushort NumberOfTheDiskWithTheStartOfTheCentralDirectory;
    internal readonly ushort NumberOfEntriesInTheCentralDirectoryOnThisDisk;
    internal readonly ushort NumberOfEntriesInTheCentralDirectory;
    internal readonly uint OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;

    private ZipEndOfCentralDirectoryBlock(
        ushort numberOfThisDisk,
        ushort numberOfTheDiskWithTheStartOfTheCentralDirectory,
        ushort numberOfEntriesInTheCentralDirectoryOnThisDisk,
        ushort numberOfEntriesInTheCentralDirectory,
        uint offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber)
    {
        NumberOfThisDisk = numberOfThisDisk;
        NumberOfTheDiskWithTheStartOfTheCentralDirectory = numberOfTheDiskWithTheStartOfTheCentralDirectory;
        NumberOfEntriesInTheCentralDirectoryOnThisDisk = numberOfEntriesInTheCentralDirectoryOnThisDisk;
        NumberOfEntriesInTheCentralDirectory = numberOfEntriesInTheCentralDirectory;
        OffsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber;
    }

    internal static bool TryReadBlock(
        Stream stream,
        ZipReusableBundleS bundle,
        out ZipEndOfCentralDirectoryBlock eocdBlock)
    {
        if (bundle.ReadUInt32(stream) != SignatureConstant)
        {
            eocdBlock = new ZipEndOfCentralDirectoryBlock();
            return false;
        }

        ushort numberOfThisDisk = bundle.ReadUInt16(stream);
        ushort numberOfTheDiskWithTheStartOfTheCentralDirectory = bundle.ReadUInt16(stream);
        ushort numberOfEntriesInTheCentralDirectoryOnThisDisk = bundle.ReadUInt16(stream);
        ushort numberOfEntriesInTheCentralDirectory = bundle.ReadUInt16(stream);
        bundle.ReadUInt32(stream); // SizeOfCentralDirectory
        uint offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = bundle.ReadUInt32(stream);

        ushort commentLength = bundle.ReadUInt16(stream);
        stream.Seek(commentLength, SeekOrigin.Current); // ArchiveComment

        eocdBlock = new ZipEndOfCentralDirectoryBlock(
            numberOfThisDisk: numberOfThisDisk,
            numberOfTheDiskWithTheStartOfTheCentralDirectory: numberOfTheDiskWithTheStartOfTheCentralDirectory,
            numberOfEntriesInTheCentralDirectoryOnThisDisk: numberOfEntriesInTheCentralDirectoryOnThisDisk,
            numberOfEntriesInTheCentralDirectory: numberOfEntriesInTheCentralDirectory,
            offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber: offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber);

        return true;
    }
}

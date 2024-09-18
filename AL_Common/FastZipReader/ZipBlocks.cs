// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AL_Common.FastZipReader;

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

[StructLayout(LayoutKind.Auto)]
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
        ZipContext context,
        out ZipGenericExtraField field)
    {
        // not enough bytes to read tag + size
        if (endExtraField - stream.Position < 4)
        {
            field = new ZipGenericExtraField();
            return false;
        }

        ushort tag = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort dataSize = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);

        // not enough bytes to read the data
        if (endExtraField - stream.Position < dataSize)
        {
            field = new ZipGenericExtraField(tag: tag, dataSize: dataSize);
            return false;
        }

        stream.ReadAll(context.DataBuffer, 0, dataSize);

        field = new ZipGenericExtraField(tag: tag, dataSize: dataSize, data: context.DataBuffer);

        return true;
    }
}

[StructLayout(LayoutKind.Auto)]
internal readonly ref struct Zip64ExtraField
{
    // Size is size of the record not including the tag or size fields
    // If the extra field is going in the local header, it cannot include only
    // one of uncompressed/compressed size

    private const ushort TagConstant = 1;

    internal readonly long? UncompressedSize;
    internal readonly long? CompressedSize;
    internal readonly long? LocalHeaderOffset;
    internal readonly uint? StartDiskNumber;

    private Zip64ExtraField(
        long? uncompressedSize,
        long? compressedSize,
        long? localHeaderOffset,
        uint? startDiskNumber)
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
        ZipContext context)
    {
        while (ZipGenericExtraField.TryReadBlock(
                   stream: extraFieldStream,
                   endExtraField: extraFieldStream.Length,
                   context: context,
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
        uint? startDiskNumber = null;

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
            startDiskNumber = ZipHelpers.ReadUInt32(extraField.Data, dataSize, arrayIndex);
        }

        // original values are unsigned, so implies value is too big to fit in signed integer
        if (uncompressedSize is < 0) ThrowHelper.InvalidData(SR.FieldTooBigUncompressedSize);
        if (compressedSize is < 0) ThrowHelper.InvalidData(SR.FieldTooBigCompressedSize);
        if (localHeaderOffset is < 0) ThrowHelper.InvalidData(SR.FieldTooBigLocalHeaderOffset);

        zip64Block = new Zip64ExtraField(
            uncompressedSize: uncompressedSize,
            compressedSize: compressedSize,
            localHeaderOffset: localHeaderOffset,
            startDiskNumber: startDiskNumber
        );

        return true;
    }
}

[StructLayout(LayoutKind.Auto)]
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
        ZipContext context,
        out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
    {
        if (BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer) != SignatureConstant)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();
            return false;
        }

        BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer); // NumberOfDiskWithZip64EOCD
        ulong offsetOfZip64EOCD = BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer);
        BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer); // TotalNumberOfDisks

        zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator(offsetOfZip64EOCD: offsetOfZip64EOCD);

        return true;
    }
}

[StructLayout(LayoutKind.Auto)]
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
        ZipContext context,
        out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
    {
        if (BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer) != SignatureConstant)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();
            return false;
        }

        BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer); // SizeOfThisRecord
        BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer); // VersionMadeBy
        BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer); // VersionNeededToExtract
        uint numberOfThisDisk = BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer);
        BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer); // NumberOfDiskWithStartOfCD
        ulong numberOfEntriesOnThisDisk = BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer);
        ulong numberOfEntriesTotal = BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer);
        BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer); // SizeOfCentralDirectory
        ulong offsetOfCentralDirectory = BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer);

        zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord(
            numberOfThisDisk: numberOfThisDisk,
            numberOfEntriesOnThisDisk: numberOfEntriesOnThisDisk,
            numberOfEntriesTotal: numberOfEntriesTotal,
            offsetOfCentralDirectory: offsetOfCentralDirectory);

        return true;
    }
}

[StructLayout(LayoutKind.Auto)]
internal readonly ref struct ZipLocalFileHeader
{
    private const uint SignatureConstant = 0x04034B50;

    // will not throw end of stream exception
    // @MT_TASK (Framework Seek perf): Could we use reads instead of seeks here, since we're always going forward?
    // Try it and measure the difference. It could be too minor to matter, but not sure.
    internal static bool TrySkipBlock(Stream stream, long streamLength, ZipContext context)
    {
        const int offsetToFilenameLength = 22; // from the point after the signature

        if (BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer) != SignatureConstant) return false;
        if (stream.Length < stream.Position + offsetToFilenameLength) return false;

        stream.Seek(offsetToFilenameLength, SeekOrigin.Current);

        ushort filenameLength = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort extraFieldLength = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);

        if (streamLength < stream.Position + filenameLength + extraFieldLength)
        {
            return false;
        }

        stream.Seek(filenameLength + extraFieldLength, SeekOrigin.Current);

        return true;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly ref struct ZipCentralDirectoryFileHeader
{
    private const uint SignatureConstant = 0x02014B50;

    internal readonly ushort GeneralPurposeBitFlag;
    internal readonly ushort CompressionMethod;
    internal readonly uint LastModified;
    internal readonly long CompressedSize;
    internal readonly long UncompressedSize;
    internal readonly uint DiskNumberStart;
    internal readonly long RelativeOffsetOfLocalHeader;

    internal readonly byte[] Filename = Array.Empty<byte>();
    internal readonly ushort FilenameLength;

    private ZipCentralDirectoryFileHeader(
        ushort generalPurposeBitFlag,
        ushort compressionMethod,
        uint lastModified,
        long compressedSize,
        long uncompressedSize,
        uint diskNumberStart,
        long relativeOffsetOfLocalHeader,
        byte[] filename,
        ushort filenameLength)
    {
        GeneralPurposeBitFlag = generalPurposeBitFlag;
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
        ZipContext context,
        out ZipCentralDirectoryFileHeader header)
    {
        if (BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer) != SignatureConstant)
        {
            header = new ZipCentralDirectoryFileHeader();
            return false;
        }

        BinaryRead.ReadByte(stream, context.BinaryReadBuffer); // VersionMadeBySpecification
        BinaryRead.ReadByte(stream, context.BinaryReadBuffer); // VersionMadeByCompatibility
        BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer); // VersionNeededToExtract
        ushort generalPurposeBitFlag = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort compressionMethod = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        uint lastModified = BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer);
        BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer); // Crc32
        uint compressedSizeSmall = BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer);
        uint uncompressedSizeSmall = BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer);
        ushort filenameLength = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort extraFieldLength = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort fileCommentLength = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort diskNumberStartSmall = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer); // InternalFileAttributes
        BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer); // ExternalFileAttributes
        uint relativeOffsetOfLocalHeaderSmall = BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer);

        stream.ReadAll(context.FilenameBuffer, 0, filenameLength);

        bool uncompressedSizeInZip64 = uncompressedSizeSmall == ZipHelpers.Mask32Bit;
        bool compressedSizeInZip64 = compressedSizeSmall == ZipHelpers.Mask32Bit;
        bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == ZipHelpers.Mask32Bit;
        bool diskNumberStartInZip64 = diskNumberStartSmall == ZipHelpers.Mask16Bit;

        long endExtraFields = stream.Position + extraFieldLength;

        context.ArchiveSubReadStream.Set(stream.Position, extraFieldLength);

        Zip64ExtraField zip64 = Zip64ExtraField.GetJustZip64Block(
            extraFieldStream: context.ArchiveSubReadStream,
            readUncompressedSize: uncompressedSizeInZip64,
            readCompressedSize: compressedSizeInZip64,
            readLocalHeaderOffset: relativeOffsetInZip64,
            readStartDiskNumber: diskNumberStartInZip64,
            context: context);

        // There are zip files that have malformed ExtraField blocks in which GetJustZip64Block() silently
        // bails out without reading all the way to the end of the ExtraField block. Thus we must force the
        // stream's position to the proper place.

        stream.AdvanceToPosition(endExtraFields + fileCommentLength, context);

        long uncompressedSize = zip64.UncompressedSize ?? uncompressedSizeSmall;
        long compressedSize = zip64.CompressedSize ?? compressedSizeSmall;
        long relativeOffsetOfLocalHeader = zip64.LocalHeaderOffset ?? relativeOffsetOfLocalHeaderSmall;
        uint diskNumberStart = zip64.StartDiskNumber ?? diskNumberStartSmall;

        header = new ZipCentralDirectoryFileHeader(
            generalPurposeBitFlag: generalPurposeBitFlag,
            compressionMethod: compressionMethod,
            lastModified: lastModified,
            compressedSize: compressedSize,
            uncompressedSize: uncompressedSize,
            diskNumberStart: diskNumberStart,
            relativeOffsetOfLocalHeader: relativeOffsetOfLocalHeader,
            filename: context.FilenameBuffer,
            filenameLength: filenameLength
        );

        return true;
    }
}

[StructLayout(LayoutKind.Auto)]
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
        ZipContext context,
        out ZipEndOfCentralDirectoryBlock eocdBlock)
    {
        if (BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer) != SignatureConstant)
        {
            eocdBlock = new ZipEndOfCentralDirectoryBlock();
            return false;
        }

        ushort numberOfThisDisk = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort numberOfTheDiskWithTheStartOfTheCentralDirectory = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort numberOfEntriesInTheCentralDirectoryOnThisDisk = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        ushort numberOfEntriesInTheCentralDirectory = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
        BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer); // SizeOfCentralDirectory
        uint offsetOfStartOfCentralDirectoryWithRespectToTheStartingDiskNumber = BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer);

        ushort commentLength = BinaryRead.ReadUInt16(stream, context.BinaryReadBuffer);
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

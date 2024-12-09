// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.IO;
using System.Runtime.InteropServices;
using AL_Common.NETM_IO;
using static AL_Common.Common;
using static AL_Common.FastZipReader.ZipArchiveFast_Common;

namespace AL_Common.FastZipReader;

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
        SubReadStream stream,
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

        byte[] buffer = context.GenericExtraFieldBuffer;

        int bytesRead = stream.ReadAll(buffer, 0, ZipContext.GenericExtraFieldBufferSize);
        if (bytesRead < ZipContext.GenericExtraFieldBufferSize)
        {
            ThrowHelper.EndOfFile();
        }

        int bufferIndex = 0;
        ushort tag = ReadUInt16_Fast(buffer, ref bufferIndex);
        ushort dataSize = ReadUInt16_Fast(buffer, ref bufferIndex);

        // not enough bytes to read the data
        if (endExtraField - stream.Position < dataSize)
        {
            field = new ZipGenericExtraField(tag: tag, dataSize: dataSize);
            return false;
        }

        byte[] dataBuffer = context.DataBuffer.GetArray(dataSize);
        stream.ReadAll(dataBuffer, 0, dataSize);

        field = new ZipGenericExtraField(tag: tag, dataSize: dataSize, data: dataBuffer);

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
        SubReadStream extraFieldStream,
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
            uncompressedSize = ReadInt64(extraField.Data, dataSize, arrayIndex);
            arrayIndex += 8;
        }
        if (readCompressedSize)
        {
            compressedSize = ReadInt64(extraField.Data, dataSize, arrayIndex);
            arrayIndex += 8;
        }
        if (readLocalHeaderOffset)
        {
            localHeaderOffset = ReadInt64(extraField.Data, dataSize, arrayIndex);
            arrayIndex += 8;
        }
        if (readStartDiskNumber)
        {
            startDiskNumber = ReadUInt32(extraField.Data, dataSize, arrayIndex);
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
        FileStream_NET stream,
        ZipContext context,
        out Zip64EndOfCentralDirectoryLocator zip64EOCDLocator)
    {
        if (BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer) != SignatureConstant)
        {
            zip64EOCDLocator = new Zip64EndOfCentralDirectoryLocator();
            return false;
        }

        stream.Seek(ByteLengths.Int32, SeekOrigin.Current); // NumberOfDiskWithZip64EOCD
        ulong offsetOfZip64EOCD = BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer);
        stream.Seek(ByteLengths.Int32, SeekOrigin.Current); // TotalNumberOfDisks

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
        FileStream_NET stream,
        ZipContext context,
        out Zip64EndOfCentralDirectoryRecord zip64EOCDRecord)
    {
        if (BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer) != SignatureConstant)
        {
            zip64EOCDRecord = new Zip64EndOfCentralDirectoryRecord();
            return false;
        }

        stream.Seek(
            ByteLengths.Int64 + // SizeOfThisRecord
            ByteLengths.Int16 + // VersionMadeBy
            ByteLengths.Int16,  // VersionNeededToExtract
            SeekOrigin.Current);
        uint numberOfThisDisk = BinaryRead.ReadUInt32(stream, context.BinaryReadBuffer);
        stream.Seek(ByteLengths.Int32, SeekOrigin.Current); // NumberOfDiskWithStartOfCD
        ulong numberOfEntriesOnThisDisk = BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer);
        ulong numberOfEntriesTotal = BinaryRead.ReadUInt64(stream, context.BinaryReadBuffer);
        stream.Seek(ByteLengths.Int64, SeekOrigin.Current); // SizeOfCentralDirectory
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
    internal static bool TrySkipBlock(FileStream_NET stream, long streamLength, BinaryBuffer binaryReadBuffer)
    {
        const int offsetToFilenameLength = 22; // from the point after the signature

        if (BinaryRead.ReadUInt32(stream, binaryReadBuffer) != SignatureConstant) return false;
        if (stream.Length < stream.Position + offsetToFilenameLength) return false;

        stream.Seek(offsetToFilenameLength, SeekOrigin.Current);

        ushort filenameLength = BinaryRead.ReadUInt16(stream, binaryReadBuffer);
        ushort extraFieldLength = BinaryRead.ReadUInt16(stream, binaryReadBuffer);

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
        FileStream_NET stream,
        ZipContext context,
        out ZipCentralDirectoryFileHeader header)
    {
        if (stream.Position > stream.Length - ZipContext.EntryFieldsBufferSize)
        {
            header = new ZipCentralDirectoryFileHeader();
            return false;
        }

        byte[] buffer = context.EntryFieldsBuffer;

        int bytesRead = stream.ReadAll(buffer, 0, ZipContext.EntryFieldsBufferSize);

        if (bytesRead < ZipContext.EntryFieldsBufferSize)
        {
            ThrowHelper.EndOfFile();
        }

        int bufferIndex = 0;

        if (ReadUInt32_Fast(buffer, ref bufferIndex) != SignatureConstant)
        {
            header = new ZipCentralDirectoryFileHeader();
            return false;
        }

        bufferIndex +=
            ByteLengths.Byte + // VersionMadeBySpecification
            ByteLengths.Byte + // VersionMadeByCompatibility
            ByteLengths.Int16; // VersionNeededToExtract
        ushort generalPurposeBitFlag = ReadUInt16_Fast(buffer, ref bufferIndex);
        ushort compressionMethod = ReadUInt16_Fast(buffer, ref bufferIndex);
        uint lastModified = ReadUInt32_Fast(buffer, ref bufferIndex);
        bufferIndex += ByteLengths.Int32; // Crc32
        uint compressedSizeSmall = ReadUInt32_Fast(buffer, ref bufferIndex);
        uint uncompressedSizeSmall = ReadUInt32_Fast(buffer, ref bufferIndex);
        ushort fileNameLength = ReadUInt16_Fast(buffer, ref bufferIndex);
        ushort extraFieldLength = ReadUInt16_Fast(buffer, ref bufferIndex);
        ushort fileCommentLength = ReadUInt16_Fast(buffer, ref bufferIndex);
        ushort diskNumberStartSmall = ReadUInt16_Fast(buffer, ref bufferIndex);
        bufferIndex += ByteLengths.Int16 + // InternalFileAttributes
                       ByteLengths.Int32;  // ExternalFileAttributes
        uint relativeOffsetOfLocalHeaderSmall = ReadUInt32_Fast(buffer, ref bufferIndex);

        byte[] fileNameBuffer = context.FileNameBuffer.GetArray(fileNameLength);
        stream.ReadAll(fileNameBuffer, 0, fileNameLength);

        bool uncompressedSizeInZip64 = uncompressedSizeSmall == Mask32Bit;
        bool compressedSizeInZip64 = compressedSizeSmall == Mask32Bit;
        bool relativeOffsetInZip64 = relativeOffsetOfLocalHeaderSmall == Mask32Bit;
        bool diskNumberStartInZip64 = diskNumberStartSmall == Mask16Bit;

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

        stream.Seek(endExtraFields + fileCommentLength, SeekOrigin.Begin);

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
            filename: fileNameBuffer,
            filenameLength: fileNameLength
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
        FileStream_NET stream,
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
        stream.Seek(ByteLengths.Int32, SeekOrigin.Current); // SizeOfCentralDirectory
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

#nullable disable

/*
@MEM(SharpCompress/array pooling) notes:
We shouldn't pool arrays that get set as class-level fields or added to class-level lists or anything like that.
We should only do it if we can guarantee the array can be rented and returned in scope without leaking out.
We should also see if we can refactor the code to allow the above.
*/

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Compressors.LZMA;
using static AL_Common.Common;

namespace SharpCompress.Common.SevenZip;

internal sealed class ArchiveReader
{
    private enum BlockType : byte
    {
        End = 0,
        Header = 1,
        ArchiveProperties = 2,
        AdditionalStreamsInfo = 3,
        MainStreamsInfo = 4,
        FilesInfo = 5,
        PackInfo = 6,
        UnpackInfo = 7,
        SubStreamsInfo = 8,
        Size = 9,
        Crc = 10,
        Folder = 11,
        CodersUnpackSize = 12,
        NumUnpackStream = 13,
        EmptyStream = 14,
        EmptyFile = 15,
        Anti = 16,
        Name = 17,
        CTime = 18,
        ATime = 19,
        MTime = 20,
        WinAttributes = 21,
        Comment = 22,
        EncodedHeader = 23,
        StartPos = 24,
        Dummy = 25
    }

    private readonly Stack<DataReader> _readerStack = new();
    private DataReader _currentReader;
    private long _streamOrigin;
    private long _streamEnding;

    private readonly SevenZipContext _context;

    internal ArchiveReader(SevenZipContext context) => _context = context;

    internal void AddByteStream(byte[] buffer, int offset, int length)
    {
        _readerStack.Push(_currentReader);
        _currentReader = new DataReader(buffer, offset, length);
    }

    internal void DeleteByteStream() => _currentReader = _readerStack.Pop();

    #region Private Methods - Data Reader

    internal byte ReadByte() => _currentReader.ReadByte();

    private void ReadBytes(byte[] buffer, int offset, int length) =>
        _currentReader.ReadBytes(buffer, offset, length);

    private ulong ReadNumber() => _currentReader.ReadNumber();

    internal int ReadNum() => _currentReader.ReadNum();

    private uint ReadUInt32() => _currentReader.ReadUInt32();

    private ulong ReadUInt64() => _currentReader.ReadUInt64();

    private BlockType? ReadId()
    {
        ulong id = _currentReader.ReadNumber();
        if (id > 25)
        {
            return null;
        }
        return (BlockType)id;
    }

    private void SkipData(long size) => _currentReader.SkipData(size);

    private void SkipData() => _currentReader.SkipData();

    private void WaitAttribute(BlockType attribute)
    {
        for (; ; )
        {
            BlockType? type = ReadId();
            if (type == attribute)
            {
                return;
            }
            if (type == BlockType.End)
            {
                throw new InvalidOperationException();
            }
            SkipData();
        }
    }

    private void ReadArchiveProperties()
    {
        while (ReadId() != BlockType.End)
        {
            SkipData();
        }
    }

    #endregion

    #region Private Methods - Reader Utilities

    private BitVector ReadBitVector(int length)
    {
        var bits = new BitVector(length);

        byte data = 0;
        byte mask = 0;

        for (int i = 0; i < length; i++)
        {
            if (mask == 0)
            {
                data = ReadByte();
                mask = 0x80;
            }

            if ((data & mask) != 0)
            {
                bits.SetBit(i);
            }

            mask >>= 1;
        }

        return bits;
    }

    private BitVector ReadOptionalBitVector(int length)
    {
        byte allTrue = ReadByte();
        if (allTrue != 0)
        {
            return new BitVector(length, true);
        }

        return ReadBitVector(length);
    }

    private void ReadDummyNumberVector(List<byte[]> dataVector, int numFiles)
    {
        BitVector defined = ReadOptionalBitVector(numFiles);

        using var streamSwitch = new CStreamSwitch();
        streamSwitch.Set(this, dataVector);

        for (int i = 0; i < numFiles; i++)
        {
            if (defined[i])
            {
                _ = checked((long)ReadUInt64());
            }
        }
    }

#if false
    private void ReadNumberVector(List<byte[]> dataVector, int numFiles, Action<int, long?> action)
    {
        var defined = ReadOptionalBitVector(numFiles);

        using var streamSwitch = new CStreamSwitch();
        streamSwitch.Set(this, dataVector);

        for (var i = 0; i < numFiles; i++)
        {
            if (defined[i])
            {
                action(i, checked((long)ReadUInt64()));
            }
            else
            {
                action(i, null);
            }
        }
    }

    private void ReadDateTimeVector(
        List<byte[]> dataVector,
        int numFiles,
        Action<int, long?> action
    )
    {
        //ReadNumberVector(
        //    dataVector,
        //    numFiles,
        //    (index, value) => action(index, value)
        //);

        var defined = ReadOptionalBitVector(numFiles);

        using var streamSwitch = new CStreamSwitch();
        streamSwitch.Set(this, dataVector);

        for (var i = 0; i < numFiles; i++)
        {
            if (defined[i])
            {
                action(i, checked((long)ReadUInt64()));
            }
            else
            {
                action(i, null);
            }
        }
    }
#endif

#if false
    private void ReadAttributeVector(
        List<byte[]> dataVector,
        int numFiles,
        Action<int, uint?> action
    )
    {
        var boolVector = ReadOptionalBitVector(numFiles);
        using var streamSwitch = new CStreamSwitch();
        streamSwitch.Set(this, dataVector);
        for (var i = 0; i < numFiles; i++)
        {
            if (boolVector[i])
            {
                action(i, ReadUInt32());
            }
            else
            {
                action(i, null);
            }
        }
    }
#endif

    #endregion

    #region Private Methods

    private void GetNextFolderItem(CFolder folder)
    {
        int numCoders = ReadNum();
        folder._coders.ClearAndEnsureCapacity(numCoders);
        int numInStreams = 0;
        int numOutStreams = 0;
        for (int i = 0; i < numCoders; i++)
        {
            var coder = new CCoderInfo();
            folder._coders.Add(coder);

            byte mainByte = ReadByte();
            int idSize = (mainByte & 0xF);
            byte[] longId = _context.ByteArrayPool.Rent(idSize);
            try
            {
                ReadBytes(longId, 0, idSize);
                if (idSize > 8)
                {
                    throw new NotSupportedException();
                }
                ulong id = 0;
                for (int j = 0; j < idSize; j++)
                {
                    id |= (ulong)longId[idSize - 1 - j] << (8 * j);
                }
                coder._methodId = id;
            }
            finally
            {
                _context.ByteArrayPool.Return(longId);
            }

            if ((mainByte & 0x10) != 0)
            {
                coder._numInStreams = ReadNum();
                coder._numOutStreams = ReadNum();
            }
            else
            {
                coder._numInStreams = 1;
                coder._numOutStreams = 1;
            }

            if ((mainByte & 0x20) != 0)
            {
                int propsSize = ReadNum();
                coder._props = new byte[propsSize];
                ReadBytes(coder._props, 0, propsSize);
            }

            if ((mainByte & 0x80) != 0)
            {
                throw new NotSupportedException();
            }

            numInStreams += coder._numInStreams;
            numOutStreams += coder._numOutStreams;
        }

        int numBindPairs = numOutStreams - 1;
        folder._bindPairs.ClearFastAndEnsureCapacity(numBindPairs);
        for (int i = 0; i < numBindPairs; i++)
        {
            CBindPair bp = new(inIndex: ReadNum(), outIndex: ReadNum());
            folder._bindPairs.AddFast(bp);
        }
        if (numInStreams < numBindPairs)
        {
            throw new NotSupportedException();
        }

        int numPackStreams = numInStreams - numBindPairs;

        folder._packStreams.EnsureCapacity(numInStreams);
        if (numPackStreams == 1)
        {
            for (int i = 0; i < numInStreams; i++)
            {
                if (folder.FindBindPairForInStream(i) < 0)
                {
                    folder._packStreams.AddFast(i);
                    break;
                }
            }

            if (folder._packStreams.Count != 1)
            {
                throw new NotSupportedException();
            }
        }
        else
        {
            for (int i = 0; i < numPackStreams; i++)
            {
                int num = ReadNum();
                folder._packStreams.AddFast(num);
            }
        }
    }

    private void ReadHashDigestsDummy(int count)
    {
        BitVector defined = ReadOptionalBitVector(count);
        for (int i = 0; i < count; i++)
        {
            if (defined[i])
            {
                _ = ReadUInt32();
            }
        }
    }

    private void ReadPackInfo(out long dataOffset)
    {
        dataOffset = checked((long)ReadNumber());

        int numPackStreams = ReadNum();

        WaitAttribute(BlockType.Size);
        ListFast<long> packSizes = _context.ArchiveDatabase.PackSizes;
        packSizes.ClearFastAndEnsureCapacity(numPackStreams);
        for (int i = 0; i < numPackStreams; i++)
        {
            long size = checked((long)ReadNumber());
            packSizes.AddFast(size);
        }

        for (; ; )
        {
            BlockType? type = ReadId();
            if (type == BlockType.End)
            {
                break;
            }
            if (type == BlockType.Crc)
            {
                ReadHashDigestsDummy(numPackStreams);
                continue;
            }
            SkipData();
        }
    }

    private void ReadUnpackInfo(List<byte[]> dataVector)
    {
        ArchiveDatabase db = _context.ArchiveDatabase;

        WaitAttribute(BlockType.Folder);
        int numFolders = ReadNum();

        using (var streamSwitch = new CStreamSwitch())
        {
            streamSwitch.Set(this, dataVector);

            db._folders.SetRecycleState(numFolders);
            for (int i = 0; i < numFolders; i++)
            {
                CFolder folder = db._folders[i];
                if (folder != null)
                {
                    folder.Reset();
                }
                else
                {
                    folder = new CFolder();
                    db._folders[i] = folder;
                }

                GetNextFolderItem(folder);
            }
        }

        WaitAttribute(BlockType.CodersUnpackSize);
        for (int i = 0; i < numFolders; i++)
        {
            CFolder folder = db._folders[i];
            int numOutStreams = folder.GetNumOutStreams();
            for (int j = 0; j < numOutStreams; j++)
            {
                long size = checked((long)ReadNumber());
                folder._unpackSizes.Add(size);
            }
        }

        for (; ; )
        {
            BlockType? type = ReadId();
            if (type == BlockType.End)
            {
                return;
            }

            if (type == BlockType.Crc)
            {
                BitVector defined = ReadOptionalBitVector(numFolders);
                for (int i = 0; i < numFolders; i++)
                {
                    if (defined[i])
                    {
                        db._folders[i]._unpackCrc = ReadUInt32();
                    }
                }
                continue;
            }

            SkipData();
        }
    }

    private void ReadSubStreamsInfo()
    {
        ArchiveDatabase db = _context.ArchiveDatabase;

        bool numUnpackStreamsInFoldersInitialized = false;

        BlockType? type;
        for (; ; )
        {
            type = ReadId();
            if (type == BlockType.NumUnpackStream)
            {
                db._numUnpackStreamsVector.ClearFastAndEnsureCapacity(db._folders.Count);
                numUnpackStreamsInFoldersInitialized = true;

                for (int i = 0; i < db._folders.Count; i++)
                {
                    int num = ReadNum();
                    db._numUnpackStreamsVector.AddFast(num);
                }
                continue;
            }
            if (type is BlockType.Crc or BlockType.Size)
            {
                break;
            }
            if (type == BlockType.End)
            {
                break;
            }
            SkipData();
        }

        if (!numUnpackStreamsInFoldersInitialized)
        {
            db._numUnpackStreamsVector.ClearFastAndEnsureCapacity(db._folders.Count);
            for (int i = 0; i < db._folders.Count; i++)
            {
                db._numUnpackStreamsVector.AddFast(1);
            }
        }

        ListFast<long> unpackSizes = _context.ArchiveDatabase.UnpackSizes.ClearedAndWithCapacityAtLeast(db._folders.Count);
        for (int i = 0; i < db._numUnpackStreamsVector.Count; i++)
        {
            // v3.13 incorrectly worked with empty folders
            // v4.07: we check that folder is empty
            int numSubstreams = db._numUnpackStreamsVector[i];
            if (numSubstreams == 0)
            {
                continue;
            }
            long sum = 0;
            for (int j = 1; j < numSubstreams; j++)
            {
                if (type == BlockType.Size)
                {
                    long size = checked((long)ReadNumber());
                    unpackSizes.Add(size);
                    sum += size;
                }
            }
            unpackSizes.Add(db._folders[i].GetUnpackSize() - sum);
        }
        if (type == BlockType.Size)
        {
            type = ReadId();
        }

        int numDigests = 0;
        for (int i = 0; i < db._folders.Count; i++)
        {
            int numSubstreams = db._numUnpackStreamsVector[i];
            if (numSubstreams != 1 || !db._folders[i].UnpackCrcDefined)
            {
                numDigests += numSubstreams;
            }
        }

        for (; ; )
        {
            if (type == BlockType.Crc)
            {
                ReadHashDigestsDummy(numDigests);
            }
            else if (type == BlockType.End)
            {
                return;
            }
            else
            {
                SkipData();
            }

            type = ReadId();
        }
    }

    private void ReadStreamsInfo(
        List<byte[]> dataVector,
        out long dataOffset)
    {
        dataOffset = long.MinValue;

        for (; ; )
        {
            switch (ReadId())
            {
                case BlockType.End:
                    return;
                case BlockType.PackInfo:
                    ReadPackInfo(out dataOffset);
                    break;
                case BlockType.UnpackInfo:
                    ReadUnpackInfo(dataVector);
                    break;
                case BlockType.SubStreamsInfo:
                    ReadSubStreamsInfo();
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    private List<byte[]> ReadAndDecodePackedStreams(Stream stream, long baseOffset)
    {
        ArchiveDatabase db = _context.ArchiveDatabase;

        ReadStreamsInfo(null, out long dataStartPos);

        dataStartPos += baseOffset;

        var dataVector = new List<byte[]>(db._folders.Count);
        const int packIndex = 0;
        for (int folderIndex = 0; folderIndex < db._folders.Count; folderIndex++)
        {
            CFolder folder = db._folders[folderIndex];

            long oldDataStartPos = dataStartPos;
            int myPackSizesLength = folder._packStreams.Count;
            long[] myPackSizes = _context.LongArrayPool.Rent(myPackSizesLength);
            try
            {
                for (int i = 0; i < myPackSizesLength; i++)
                {
                    long packSize = db.PackSizes[packIndex + i];
                    myPackSizes[i] = packSize;
                    dataStartPos += packSize;
                }

                using Stream outStream = DecoderStreamHelper.CreateDecoderStream(
                    stream,
                    oldDataStartPos,
                    myPackSizes,
                    myPackSizesLength,
                    folder,
                    _context
                );

                int unpackSize = checked((int)folder.GetUnpackSize());
                byte[] data = new byte[unpackSize];
                ReadExact(outStream, data);
                if (outStream.ReadByte() >= 0)
                {
                    throw new InvalidOperationException("Decoded stream is longer than expected.");
                }
                dataVector.Add(data);

                if (folder.UnpackCrcDefined)
                {
                    if (Crc.Finish(Crc.Update(Crc.INIT_CRC, data, 0, unpackSize)) != folder._unpackCrc)
                    {
                        throw new InvalidOperationException("Decoded stream does not match expected CRC.");
                    }
                }
            }
            finally
            {
                _context.LongArrayPool.Return(myPackSizes);
            }
        }
        return dataVector;
    }

    private void ReadHeader(Stream stream, ArchiveDatabase db, bool onlyGetEntryCount, out int entriesCount)
    {
        entriesCount = 0;

        BlockType? type = ReadId();

        if (type == BlockType.ArchiveProperties)
        {
            ReadArchiveProperties();
            type = ReadId();
        }

        List<byte[]> dataVector = null;
        if (type == BlockType.AdditionalStreamsInfo)
        {
            dataVector = ReadAndDecodePackedStreams(
                stream,
                db._startPositionAfterHeader
            );
            type = ReadId();
        }

        ListFast<long> unpackSizes;

        if (type == BlockType.MainStreamsInfo)
        {
            unpackSizes = _context.ArchiveDatabase.UnpackSizes;

            ReadStreamsInfo(
                dataVector,
                out _
            );

            type = ReadId();
        }
        else
        {
            int foldersCount = db._folders.Count;
            unpackSizes = _context.ArchiveDatabase.UnpackSizes.ClearedAndWithCapacityAtLeast(foldersCount);
            db._numUnpackStreamsVector.ClearFastAndEnsureCapacity(foldersCount);
            for (int i = 0; i < foldersCount; i++)
            {
                CFolder folder = db._folders[i];
                unpackSizes.AddFast(folder.GetUnpackSize());
                db._numUnpackStreamsVector.AddFast(1);
            }
        }

        db._files.ClearFast();

        if (type == BlockType.End)
        {
            return;
        }

        if (type != BlockType.FilesInfo)
        {
            throw new InvalidOperationException();
        }

        int numFiles = ReadNum();

        entriesCount = numFiles;

        if (onlyGetEntryCount)
        {
            return;
        }

        db._files.SetRecycleState(numFiles);

        for (int i = 0; i < numFiles; i++)
        {
            SevenZipArchiveEntry file = db._files[i];
            if (file != null)
            {
                file.Reset();
            }
            else
            {
                db._files[i] = new SevenZipArchiveEntry();
            }
        }

        var emptyStreamVector = new BitVector(numFiles);
        var emptyFileVector = new BitVector();
        var antiFileVector = new BitVector();
        int numEmptyStreams = 0;

        for (; ; )
        {
            type = ReadId();
            if (type == BlockType.End)
            {
                break;
            }

            long size = checked((long)ReadNumber()); // TODO: throw invalid data on negative
            int oldPos = _currentReader.Offset;
            switch (type)
            {
                case BlockType.Name:
                    using (var streamSwitch = new CStreamSwitch())
                    {
                        streamSwitch.Set(this, dataVector);
                        for (int i = 0; i < db._files.Count; i++)
                        {
                            db._files[i].FileName = _currentReader.ReadString();
                        }
                    }
                    break;
                case BlockType.WinAttributes:
                {
                    // FenPhoenix 2023: We don't use it so just read past it
                    BitVector boolVector = ReadOptionalBitVector(numFiles);
                    using var streamSwitch = new CStreamSwitch();
                    streamSwitch.Set(this, dataVector);
                    for (int i = 0; i < numFiles; i++)
                    {
                        if (boolVector[i])
                        {
                            ReadUInt32();
                        }
                    }
                    /*
                    ReadAttributeVector(
                        dataVector,
                        numFiles,
                        delegate (int i, uint? attr)
                        {
                            // Some third party implementations established an unofficial extension
                            // of the 7z archive format by placing posix file attributes in the high
                            // bits of the windows file attributes. This makes use of the fact that
                            // the official implementation does not perform checks on this value.
                            //
                            // Newer versions of the official 7z GUI client will try to parse this
                            // extension, thus acknowledging the unofficial use of these bits.
                            //
                            // For us it is safe to just discard the upper bits if they are set and
                            // keep the windows attributes from the lower bits (which should be set
                            // properly even if posix file attributes are present, in order to be
                            // compatible with older 7z archive readers)
                            //
                            // Note that the 15th bit is used by some implementations to indicate
                            // presence of the extension, but not all implementations do that so
                            // we can't trust that bit and must ignore it.
                            //
                            if (attr.HasValue && (attr.Value >> 16) != 0)
                            {
                                attr = attr.Value & 0x7FFFu;
                            }
                        }
                    );
                    */
                }
                break;
                case BlockType.EmptyStream:
                    emptyStreamVector = ReadBitVector(numFiles);
                    for (int i = 0; i < emptyStreamVector.Length; i++)
                    {
                        if (emptyStreamVector[i])
                        {
                            numEmptyStreams++;
                        }
                    }

                    emptyFileVector = new BitVector(numEmptyStreams);
                    antiFileVector = new BitVector(numEmptyStreams);
                    break;
                case BlockType.EmptyFile:
                    emptyFileVector = ReadBitVector(numEmptyStreams);
                    break;
                case BlockType.Anti:
                    antiFileVector = ReadBitVector(numEmptyStreams);
                    break;
                case BlockType.StartPos:
                    ReadDummyNumberVector(dataVector, numFiles);
                    //ReadNumberVector(
                    //    dataVector,
                    //    numFiles,
                    //    delegate (int i, long? startPos)
                    //    {
                    //    }
                    //);
                    break;
                case BlockType.CTime:
                    ReadDummyNumberVector(dataVector, numFiles);
                    //ReadDateTimeVector(
                    //    dataVector,
                    //    numFiles,
                    //    delegate (int i, long? time)
                    //    {
                    //    }
                    //);
                    break;
                case BlockType.ATime:
                    ReadDummyNumberVector(dataVector, numFiles);
                    //ReadDateTimeVector(
                    //    dataVector,
                    //    numFiles,
                    //    delegate (int i, long? time)
                    //    {
                    //    }
                    //);
                    break;
                case BlockType.MTime:
                {
                    BitVector defined = ReadOptionalBitVector(numFiles);

                    using var streamSwitch = new CStreamSwitch();
                    streamSwitch.Set(this, dataVector);

                    for (int i = 0; i < numFiles; i++)
                    {
                        //action(i, checked((long)ReadUInt64()));
                        // The date has a cap well under the ulong max value, so setting max value will cause it
                        // to be interpreted as invalid and the entry's date will be set to null, as desired
                        db._files[i].MTime = defined[i] ? ReadUInt64() : ulong.MaxValue;
                    }
                    //ReadDateTimeVector(
                    //    dataVector,
                    //    numFiles,
                    //    delegate (int i, long? time)
                    //    {
                    //        db._files[i].MTime = time;
                    //    }
                    //);
                }
                break;
                case BlockType.Dummy:
                    for (long j = 0; j < size; j++)
                    {
                        if (ReadByte() != 0)
                        {
                            throw new InvalidOperationException();
                        }
                    }
                    break;
                default:
                    SkipData(size);
                    break;
            }

            // since 0.3 record sizes must be correct
            bool checkRecordsSize = (db._majorVersion > 0 || db._minorVersion > 2);
            if (checkRecordsSize && _currentReader.Offset - oldPos != size)
            {
                throw new InvalidOperationException();
            }
        }

        int emptyFileIndex = 0;
        int sizeIndex = 0;
        for (int i = 0; i < numFiles; i++)
        {
            SevenZipArchiveEntry file = db._files[i];
            file.HasStream = !emptyStreamVector[i];
            if (file.HasStream)
            {
                file.IsDirectory = false;
                file.IsAnti = false;
                file.UncompressedSize = unpackSizes[sizeIndex];
                sizeIndex++;
            }
            else
            {
                file.IsDirectory = !emptyFileVector[emptyFileIndex];
                file.IsAnti = antiFileVector[emptyFileIndex];
                emptyFileIndex++;
                file.UncompressedSize = 0;
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="onlyGetEntryCount"></param>
    /// <returns>The number of entries in the 7z file.</returns>
    /// <exception cref="EndOfStreamException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public int ReadDatabase(Stream stream, bool onlyGetEntryCount = false)
    {
        int entriesCount = 0;

        ArchiveDatabase db = _context.ArchiveDatabase;

        db.Clear();

        _streamOrigin = stream.Position;
        _streamEnding = stream.Length;

        // TODO: Check Signature!
        for (int offset = 0; offset < 0x20;)
        {
            int delta = stream.Read(_context.ArchiveHeader, offset, 0x20 - offset);
            if (delta == 0)
            {
                throw new EndOfStreamException();
            }
            offset += delta;
        }

        db._majorVersion = _context.ArchiveHeader[6];
        db._minorVersion = _context.ArchiveHeader[7];

        if (db._majorVersion != 0)
        {
            throw new InvalidOperationException();
        }

        uint crcFromArchive = DataReader.Get32(_context.ArchiveHeader, 8);
        long nextHeaderOffset = (long)DataReader.Get64(_context.ArchiveHeader, 0xC);
        long nextHeaderSize = (long)DataReader.Get64(_context.ArchiveHeader, 0x14);
        uint nextHeaderCrc = DataReader.Get32(_context.ArchiveHeader, 0x1C);

        uint crc = Crc.INIT_CRC;
        crc = Crc.Update(crc, nextHeaderOffset);
        crc = Crc.Update(crc, nextHeaderSize);
        crc = Crc.Update(crc, nextHeaderCrc);
        crc = Crc.Finish(crc);

        if (crc != crcFromArchive)
        {
            throw new InvalidOperationException();
        }

        db._startPositionAfterHeader = _streamOrigin + 0x20;

        // empty header is ok
        if (nextHeaderSize == 0)
        {
            db.Fill();
            return entriesCount;
        }

        if (nextHeaderOffset < 0 || nextHeaderSize < 0 || nextHeaderSize > int.MaxValue)
        {
            throw new InvalidOperationException();
        }

        if (nextHeaderOffset > _streamEnding - db._startPositionAfterHeader)
        {
            throw new InvalidOperationException("nextHeaderOffset is invalid");
        }

        stream.Seek(nextHeaderOffset, SeekOrigin.Current);

        byte[] header = new byte[nextHeaderSize];
        ReadExact(stream, header);

        if (Crc.Finish(Crc.Update(Crc.INIT_CRC, header, 0, header.Length)) != nextHeaderCrc)
        {
            throw new InvalidOperationException();
        }

        using (var streamSwitch = new CStreamSwitch())
        {
            streamSwitch.Set(this, header);

            BlockType? type = ReadId();
            if (type != BlockType.Header)
            {
                if (type != BlockType.EncodedHeader)
                {
                    throw new InvalidOperationException();
                }

                List<byte[]> dataVector = ReadAndDecodePackedStreams(
                    stream,
                    db._startPositionAfterHeader
                );

                // compressed header without content is odd but ok
                if (dataVector.Count == 0)
                {
                    db.Fill();
                    return entriesCount;
                }

                if (dataVector.Count != 1)
                {
                    throw new InvalidOperationException();
                }

                streamSwitch.Set(this, dataVector[0]);

                if (ReadId() != BlockType.Header)
                {
                    throw new InvalidOperationException();
                }
            }

            ReadHeader(stream, db, onlyGetEntryCount, out entriesCount);
        }
        if (!onlyGetEntryCount) db.Fill();
        return entriesCount;
    }

    // @SharpCompress(ReadExact): Not 100% sure if stream can't be null, check into this
    private static void ReadExact([CanBeNull] Stream stream, [NotNull] byte[] buffer)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        int offset = 0;
        int length = buffer.Length;

        while (length > 0)
        {
            int fetched = stream.Read(buffer, offset, length);
            if (fetched <= 0)
            {
                throw new EndOfStreamException();
            }

            offset += fetched;
            length -= fetched;
        }
    }

    #endregion
}

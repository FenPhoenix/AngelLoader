#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SharpCompress.Compressors.LZMA;
using SharpCompress.Compressors.LZMA.Utilities;

namespace SharpCompress.Common.SevenZip;

internal sealed class ArchiveReader
{
    private Stream _stream;
    private readonly Stack<DataReader> _readerStack = new();
    private DataReader _currentReader;
    private long _streamOrigin;
    private long _streamEnding;
    private byte[] _header;

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
        var id = _currentReader.ReadNumber();
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
            var type = ReadId();
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

        for (var i = 0; i < length; i++)
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
        var allTrue = ReadByte();
        if (allTrue != 0)
        {
            return new BitVector(length, true);
        }

        return ReadBitVector(length);
    }

    private void ReadDummyNumberVector(List<byte[]> dataVector, int numFiles)
    {
        var defined = ReadOptionalBitVector(numFiles);

        using var streamSwitch = new CStreamSwitch();
        streamSwitch.Set(this, dataVector);

        for (var i = 0; i < numFiles; i++)
        {
            if (defined[i])
            {
                _ = checked((long)ReadUInt64());
            }
        }
    }

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
    ) =>
        ReadNumberVector(
            dataVector,
            numFiles,
            (index, value) => action(index, value)
        );

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

    #endregion

    #region Private Methods

    private void GetNextFolderItem(CFolder folder)
    {
        var numCoders = ReadNum();
        folder._coders = new List<CCoderInfo>(numCoders);
        var numInStreams = 0;
        var numOutStreams = 0;
        for (var i = 0; i < numCoders; i++)
        {
            var coder = new CCoderInfo();
            folder._coders.Add(coder);

            var mainByte = ReadByte();
            var idSize = (mainByte & 0xF);
            var longId = new byte[idSize];
            ReadBytes(longId, 0, idSize);
            if (idSize > 8)
            {
                throw new NotSupportedException();
            }
            ulong id = 0;
            for (var j = 0; j < idSize; j++)
            {
                id |= (ulong)longId[idSize - 1 - j] << (8 * j);
            }
            coder._methodId = new CMethodId(id);

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
                var propsSize = ReadNum();
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

        var numBindPairs = numOutStreams - 1;
        folder._bindPairs = new List<CBindPair>(numBindPairs);
        for (var i = 0; i < numBindPairs; i++)
        {
            var bp = new CBindPair();
            bp._inIndex = ReadNum();
            bp._outIndex = ReadNum();
            folder._bindPairs.Add(bp);
        }
        if (numInStreams < numBindPairs)
        {
            throw new NotSupportedException();
        }

        var numPackStreams = numInStreams - numBindPairs;

        //folder.PackStreams.Reserve(numPackStreams);
        if (numPackStreams == 1)
        {
            for (var i = 0; i < numInStreams; i++)
            {
                if (folder.FindBindPairForInStream(i) < 0)
                {
                    folder._packStreams.Add(i);
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
            for (var i = 0; i < numPackStreams; i++)
            {
                var num = ReadNum();
                folder._packStreams.Add(num);
            }
        }
    }

    private List<uint?> ReadHashDigests(int count)
    {
        var defined = ReadOptionalBitVector(count);
        var digests = new List<uint?>(count);
        for (var i = 0; i < count; i++)
        {
            if (defined[i])
            {
                var crc = ReadUInt32();
                digests.Add(crc);
            }
            else
            {
                digests.Add(null);
            }
        }
        return digests;
    }

    private void ReadPackInfo(
        out long dataOffset,
        out List<long> packSizes,
        out List<uint?> packCrCs
    )
    {
        packCrCs = null;

        dataOffset = checked((long)ReadNumber());

        var numPackStreams = ReadNum();

        WaitAttribute(BlockType.Size);
        packSizes = new List<long>(numPackStreams);
        for (var i = 0; i < numPackStreams; i++)
        {
            var size = checked((long)ReadNumber());
            packSizes.Add(size);
        }

        BlockType? type;
        for (; ; )
        {
            type = ReadId();
            if (type == BlockType.End)
            {
                break;
            }
            if (type == BlockType.Crc)
            {
                packCrCs = ReadHashDigests(numPackStreams);
                continue;
            }
            SkipData();
        }

        if (packCrCs is null)
        {
            packCrCs = new List<uint?>(numPackStreams);
            for (var i = 0; i < numPackStreams; i++)
            {
                packCrCs.Add(null);
            }
        }
    }

    private void ReadUnpackInfo(List<byte[]> dataVector, out List<CFolder> folders)
    {
        WaitAttribute(BlockType.Folder);
        var numFolders = ReadNum();

        using (var streamSwitch = new CStreamSwitch())
        {
            streamSwitch.Set(this, dataVector);

            folders = new List<CFolder>(numFolders);
            for (var i = 0; i < numFolders; i++)
            {
                var f = new CFolder();
                folders.Add(f);
                GetNextFolderItem(f);
            }
        }

        WaitAttribute(BlockType.CodersUnpackSize);
        for (var i = 0; i < numFolders; i++)
        {
            var folder = folders[i];
            var numOutStreams = folder.GetNumOutStreams();
            for (var j = 0; j < numOutStreams; j++)
            {
                var size = checked((long)ReadNumber());
                folder._unpackSizes.Add(size);
            }
        }

        for (; ; )
        {
            var type = ReadId();
            if (type == BlockType.End)
            {
                return;
            }

            if (type == BlockType.Crc)
            {
                var crcs = ReadHashDigests(numFolders);
                for (var i = 0; i < numFolders; i++)
                {
                    folders[i]._unpackCrc = crcs[i];
                }
                continue;
            }

            SkipData();
        }
    }

    private void ReadSubStreamsInfo(
        List<CFolder> folders,
        out List<int> numUnpackStreamsInFolders,
        out List<long> unpackSizes,
        out List<uint?> digests
    )
    {
        numUnpackStreamsInFolders = null;

        BlockType? type;
        for (; ; )
        {
            type = ReadId();
            if (type == BlockType.NumUnpackStream)
            {
                // @SharpCompress: Can we recycle this?
                numUnpackStreamsInFolders = new List<int>(folders.Count);
                for (var i = 0; i < folders.Count; i++)
                {
                    var num = ReadNum();
                    numUnpackStreamsInFolders.Add(num);
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

        if (numUnpackStreamsInFolders is null)
        {
            numUnpackStreamsInFolders = new List<int>(folders.Count);
            for (var i = 0; i < folders.Count; i++)
            {
                numUnpackStreamsInFolders.Add(1);
            }
        }

        unpackSizes = new List<long>(folders.Count);
        for (var i = 0; i < numUnpackStreamsInFolders.Count; i++)
        {
            // v3.13 incorrectly worked with empty folders
            // v4.07: we check that folder is empty
            var numSubstreams = numUnpackStreamsInFolders[i];
            if (numSubstreams == 0)
            {
                continue;
            }
            long sum = 0;
            for (var j = 1; j < numSubstreams; j++)
            {
                if (type == BlockType.Size)
                {
                    var size = checked((long)ReadNumber());
                    unpackSizes.Add(size);
                    sum += size;
                }
            }
            unpackSizes.Add(folders[i].GetUnpackSize() - sum);
        }
        if (type == BlockType.Size)
        {
            type = ReadId();
        }

        var numDigests = 0;
        var numDigestsTotal = 0;
        for (var i = 0; i < folders.Count; i++)
        {
            var numSubstreams = numUnpackStreamsInFolders[i];
            if (numSubstreams != 1 || !folders[i].UnpackCrcDefined)
            {
                numDigests += numSubstreams;
            }
            numDigestsTotal += numSubstreams;
        }

        digests = null;

        for (; ; )
        {
            if (type == BlockType.Crc)
            {
                digests = new List<uint?>(numDigestsTotal);

                var digests2 = ReadHashDigests(numDigests);

                var digestIndex = 0;
                for (var i = 0; i < folders.Count; i++)
                {
                    var numSubstreams = numUnpackStreamsInFolders[i];
                    var folder = folders[i];
                    if (numSubstreams == 1 && folder.UnpackCrcDefined)
                    {
                        digests.Add(folder._unpackCrc.Value);
                    }
                    else
                    {
                        for (var j = 0; j < numSubstreams; j++, digestIndex++)
                        {
                            digests.Add(digests2[digestIndex]);
                        }
                    }
                }

                if (digestIndex != numDigests || numDigestsTotal != digests.Count)
                {
                    Debugger.Break();
                }
            }
            else if (type == BlockType.End)
            {
                if (digests is null)
                {
                    digests = new List<uint?>(numDigestsTotal);
                    for (var i = 0; i < numDigestsTotal; i++)
                    {
                        digests.Add(null);
                    }
                }
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
        out long dataOffset,
        out List<long> packSizes,
        out List<uint?> packCrCs,
        out List<CFolder> folders,
        out List<int> numUnpackStreamsInFolders,
        out List<long> unpackSizes,
        out List<uint?> digests)
    {
        dataOffset = long.MinValue;
        packSizes = null;
        packCrCs = null;
        folders = null;
        numUnpackStreamsInFolders = null;
        unpackSizes = null;
        digests = null;

        for (; ; )
        {
            switch (ReadId())
            {
                case BlockType.End:
                    return;
                case BlockType.PackInfo:
                    ReadPackInfo(out dataOffset, out packSizes, out packCrCs);
                    break;
                case BlockType.UnpackInfo:
                    ReadUnpackInfo(dataVector, out folders);
                    break;
                case BlockType.SubStreamsInfo:
                    ReadSubStreamsInfo(
                        folders,
                        out numUnpackStreamsInFolders,
                        out unpackSizes,
                        out digests
                    );
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    private List<byte[]> ReadAndDecodePackedStreams(long baseOffset)
    {
        ReadStreamsInfo(
            null,
            out var dataStartPos,
            out var packSizes,
            out _,
            out var folders,
            out _,
            out _,
            out _
        );

        dataStartPos += baseOffset;

        var dataVector = new List<byte[]>(folders.Count);
        const int packIndex = 0;
        foreach (var folder in folders)
        {
            var oldDataStartPos = dataStartPos;
            var myPackSizes = new long[folder._packStreams.Count];
            for (var i = 0; i < myPackSizes.Length; i++)
            {
                var packSize = packSizes[packIndex + i];
                myPackSizes[i] = packSize;
                dataStartPos += packSize;
            }

            var outStream = DecoderStreamHelper.CreateDecoderStream(
                _stream,
                oldDataStartPos,
                myPackSizes,
                folder
            );

            var unpackSize = checked((int)folder.GetUnpackSize());
            var data = new byte[unpackSize];
            outStream.ReadExact(data, 0, data.Length);
            if (outStream.ReadByte() >= 0)
            {
                throw new InvalidOperationException("Decoded stream is longer than expected.");
            }
            dataVector.Add(data);

            if (folder.UnpackCrcDefined)
            {
                if (
                    Crc.Finish(Crc.Update(Crc.INIT_CRC, data, 0, unpackSize))
                    != folder._unpackCrc
                )
                {
                    throw new InvalidOperationException(
                        "Decoded stream does not match expected CRC."
                    );
                }
            }
        }
        return dataVector;
    }

    private void ReadHeader(ArchiveDatabase db)
    {
        var type = ReadId();

        if (type == BlockType.ArchiveProperties)
        {
            ReadArchiveProperties();
            type = ReadId();
        }

        List<byte[]> dataVector = null;
        if (type == BlockType.AdditionalStreamsInfo)
        {
            dataVector = ReadAndDecodePackedStreams(
                db._startPositionAfterHeader
            );
            type = ReadId();
        }

        List<long> unpackSizes;
        List<uint?> digests;

        if (type == BlockType.MainStreamsInfo)
        {
            ReadStreamsInfo(
                dataVector,
                out _,
                out _,
                out _,
                out db._folders,
                out db._numUnpackStreamsVector,
                out unpackSizes,
                out digests
            );

            type = ReadId();
        }
        else
        {
            unpackSizes = new List<long>(db._folders.Count);
            digests = new List<uint?>(db._folders.Count);
            db._numUnpackStreamsVector = new List<int>(db._folders.Count);
            for (var i = 0; i < db._folders.Count; i++)
            {
                var folder = db._folders[i];
                unpackSizes.Add(folder.GetUnpackSize());
                digests.Add(folder._unpackCrc);
                db._numUnpackStreamsVector.Add(1);
            }
        }

        db._files.Clear();

        if (type == BlockType.End)
        {
            return;
        }

        if (type != BlockType.FilesInfo)
        {
            throw new InvalidOperationException();
        }

        var numFiles = ReadNum();
        db._files = new List<CFileItem>(numFiles);
        for (var i = 0; i < numFiles; i++)
        {
            db._files.Add(new CFileItem());
        }

        var emptyStreamVector = new BitVector(numFiles);
        BitVector emptyFileVector = null;
        BitVector antiFileVector = null;
        var numEmptyStreams = 0;

        for (; ; )
        {
            type = ReadId();
            if (type == BlockType.End)
            {
                break;
            }

            var size = checked((long)ReadNumber()); // TODO: throw invalid data on negative
            var oldPos = _currentReader.Offset;
            switch (type)
            {
                case BlockType.Name:
                    using (var streamSwitch = new CStreamSwitch())
                    {
                        streamSwitch.Set(this, dataVector);
                        for (var i = 0; i < db._files.Count; i++)
                        {
                            db._files[i].Name = _currentReader.ReadString();
                        }
                    }
                    break;
                case BlockType.WinAttributes:
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
                    break;
                case BlockType.EmptyStream:
                    emptyStreamVector = ReadBitVector(numFiles);
                    for (var i = 0; i < emptyStreamVector.Length; i++)
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
                    ReadDateTimeVector(
                        dataVector,
                        numFiles,
                        delegate (int i, long? time)
                        {
                            db._files[i].MTime = time;
                        }
                    );
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
            var checkRecordsSize = (db._majorVersion > 0 || db._minorVersion > 2);
            if (checkRecordsSize && _currentReader.Offset - oldPos != size)
            {
                throw new InvalidOperationException();
            }
        }

        var emptyFileIndex = 0;
        var sizeIndex = 0;
        for (var i = 0; i < numFiles; i++)
        {
            var file = db._files[i];
            file.HasStream = !emptyStreamVector[i];
            if (file.HasStream)
            {
                file.IsDir = false;
                file.IsAnti = false;
                file.Size = unpackSizes[sizeIndex];
                sizeIndex++;
            }
            else
            {
                file.IsDir = !emptyFileVector[emptyFileIndex];
                file.IsAnti = antiFileVector[emptyFileIndex];
                emptyFileIndex++;
                file.Size = 0;
            }
        }
    }

    #endregion

    #region Public Methods

    public void Open(Stream stream)
    {
        Close();

        _streamOrigin = stream.Position;
        _streamEnding = stream.Length;

        // TODO: Check Signature!
        _header = new byte[0x20];
        for (var offset = 0; offset < 0x20;)
        {
            var delta = stream.Read(_header, offset, 0x20 - offset);
            if (delta == 0)
            {
                throw new EndOfStreamException();
            }
            offset += delta;
        }

        _stream = stream;
    }

    private void Close() => _stream?.Dispose();

    public ArchiveDatabase ReadDatabase()
    {
        var db = new ArchiveDatabase();
        db.Clear();

        db._majorVersion = _header[6];
        db._minorVersion = _header[7];

        if (db._majorVersion != 0)
        {
            throw new InvalidOperationException();
        }

        var crcFromArchive = DataReader.Get32(_header, 8);
        var nextHeaderOffset = (long)DataReader.Get64(_header, 0xC);
        var nextHeaderSize = (long)DataReader.Get64(_header, 0x14);
        var nextHeaderCrc = DataReader.Get32(_header, 0x1C);

        var crc = Crc.INIT_CRC;
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
            return db;
        }

        if (nextHeaderOffset < 0 || nextHeaderSize < 0 || nextHeaderSize > int.MaxValue)
        {
            throw new InvalidOperationException();
        }

        if (nextHeaderOffset > _streamEnding - db._startPositionAfterHeader)
        {
            throw new InvalidOperationException("nextHeaderOffset is invalid");
        }

        _stream.Seek(nextHeaderOffset, SeekOrigin.Current);

        var header = new byte[nextHeaderSize];
        _stream.ReadExact(header, 0, header.Length);

        if (Crc.Finish(Crc.Update(Crc.INIT_CRC, header, 0, header.Length)) != nextHeaderCrc)
        {
            throw new InvalidOperationException();
        }

        using (var streamSwitch = new CStreamSwitch())
        {
            streamSwitch.Set(this, header);

            var type = ReadId();
            if (type != BlockType.Header)
            {
                if (type != BlockType.EncodedHeader)
                {
                    throw new InvalidOperationException();
                }

                var dataVector = ReadAndDecodePackedStreams(
                    db._startPositionAfterHeader
                );

                // compressed header without content is odd but ok
                if (dataVector.Count == 0)
                {
                    db.Fill();
                    return db;
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

            ReadHeader(db);
        }
        db.Fill();
        return db;
    }

    #endregion
}

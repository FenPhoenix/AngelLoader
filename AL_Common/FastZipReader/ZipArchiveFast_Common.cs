﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using AL_Common.FastZipReader.Deflate64Managed;
using JetBrains.Annotations;
using static AL_Common.Common;

namespace AL_Common.FastZipReader;

[Flags]
internal enum BitFlagValues : ushort
{
    DataDescriptor = 8,
    UnicodeFileName = 2048, // 0x0800
}

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal enum CompressionMethodValues : ushort
{
    Stored = 0,
    Shrink = 1,
    ReduceWithCompressionFactor1 = 2,
    ReduceWithCompressionFactor2 = 3,
    ReduceWithCompressionFactor3 = 4,
    ReduceWithCompressionFactor4 = 5,
    Implode = 6,
    // "Reserved for Tokenizing compression algorithm" = 7,
    Deflate = 8,
    Deflate64 = 9,
    IBM_TERSE_OLD = 10,
    // Reserved = 11,
    BZip2 = 12,
    // Reserved = 13,
    LZMA = 14,
    // Reserved - 15,
    IBM_z_OS_CMPSC = 16,
    // Reserved - 17,
    IBM_TERSE_NEW = 18,
    IBM_LZ77_z_Architecture = 19,
    ZStandard_Deprecated = 20,
    ZStandard = 93,
    MP3 = 94,
    XZ = 95,
    JPEGVariant = 96,
    WavPack = 97,
    PPMd = 98,
    // "AE-x encryption marker (see APPENDIX E)" = 99,
}

public sealed class ZipCompressionMethodException(string message) : Exception(message);

// We should try to just make the zip archive classes be like the scanner, where it's one object that just
// has like a Reset(stream) method that loads another stream and resets all its values. That'd be much nicer.
public sealed class ZipContext : IDisposable
{
    internal readonly ListFast<ZipArchiveFastEntry> Entries = new(0);

    internal readonly SubReadStream ArchiveSubReadStream = new();

    public readonly byte[] FileStreamBuffer = new byte[4096];

    internal readonly byte[] DataBuffer = new byte[65536];
    internal readonly byte[] FilenameBuffer = new byte[65536];

    private const int _backwardsSeekingBufferSize = 32;
    internal const int ThrowAwayBufferSize = 64;

    internal readonly byte[] BackwardsSeekingBuffer = new byte[_backwardsSeekingBufferSize];
    internal readonly byte[] ThrowawayBuffer = new byte[ThrowAwayBufferSize];

    internal readonly BinaryBuffer BinaryReadBuffer = new();

    public void Dispose() => ArchiveSubReadStream.Dispose();
}

public sealed class ZipContext_Threaded_Pool
{
    private readonly ConcurrentBag<ZipContext_Threaded> _contexts = new();

    public ZipContext_Threaded Rent(FileStreamReadFast archiveStream, long archiveStreamLength)
    {
        if (_contexts.TryTake(out ZipContext_Threaded item))
        {
            item.Set(archiveStream, archiveStreamLength);
            return item;
        }
        else
        {
            return new ZipContext_Threaded(archiveStream, archiveStreamLength);
        }
    }

    public void Return(ZipContext_Threaded item)
    {
        item.Unset();
        _contexts.Add(item);
    }
}

public sealed class ZipContext_Threaded
{
    internal Stream ArchiveStream = null!;
    internal long ArchiveStreamLength;

    internal readonly SubReadStream ArchiveSubReadStream;

    internal readonly BinaryBuffer BinaryReadBuffer = new();

    internal readonly byte[] TempBuffer = new byte[StreamCopyBufferSize];

    // Don't do anything in any of these that could throw, because then the sub-stream may not be disposed...
    // That's actually fine in this case because Dispose() is a no-op on our sub-stream, but meh...
    // Take the length explicitly so that if a stream throws on Length access it'll do it somewhere else so we
    // won't have any problems in here.
    public ZipContext_Threaded(FileStreamReadFast archiveStream, long archiveStreamLength)
    {
        ArchiveSubReadStream = new SubReadStream();
        Set(archiveStream, archiveStreamLength);
    }

    public void Set(FileStreamReadFast archiveStream, long archiveStreamLength)
    {
        ArchiveStream = archiveStream;
        ArchiveStreamLength = archiveStreamLength;
        ArchiveSubReadStream.SetSuperStream(ArchiveStream);
    }

    public void Unset()
    {
        ArchiveSubReadStream.SetSuperStream(null);
    }

    // We're not IDisposable because of "disposal outside of captured closure" nonsense.
    // We hold a SubReadStream but we don't need to dispose it because disposal is a no-op on that one.
}

internal static class ZipArchiveFast_Common
{
    internal const uint Mask32Bit = 0xFFFFFFFF;
    internal const ushort Mask16Bit = 0xFFFF;

    /// <summary>
    /// Reads exactly bytesToRead out of stream, unless it is out of bytes
    /// </summary>
    private static void ReadBytes(Stream stream, byte[] buffer, int bytesToRead)
    {
        int bytesLeftToRead = bytesToRead;

        int totalBytesRead = 0;

        while (bytesLeftToRead > 0)
        {
            int bytesRead = stream.Read(buffer, totalBytesRead, bytesLeftToRead);
            if (bytesRead == 0) ThrowHelper.IOException(SR.UnexpectedEndOfStream);

            totalBytesRead += bytesRead;
            bytesLeftToRead -= bytesRead;
        }
    }

    // assumes all bytes of signatureToFind are non zero, looks backwards from current position in stream,
    // if the signature is found then returns true and positions stream at first byte of signature
    // if the signature is not found, returns false
    internal static bool SeekBackwardsToSignature(Stream stream, uint signatureToFind, ZipContext context)
    {
        int bufferPointer = 0;
        uint currentSignature = 0;
        context.BackwardsSeekingBuffer.Clear();

        bool outOfBytes = false;
        bool signatureFound = false;

        while (!signatureFound && !outOfBytes)
        {
            outOfBytes = SeekBackwardsAndRead(stream, context.BackwardsSeekingBuffer, out bufferPointer);

            Debug.Assert(bufferPointer < context.BackwardsSeekingBuffer.Length);

            while (bufferPointer >= 0 && !signatureFound)
            {
                currentSignature = (currentSignature << 8) | context.BackwardsSeekingBuffer[bufferPointer];
                if (currentSignature == signatureToFind)
                {
                    signatureFound = true;
                }
                else
                {
                    bufferPointer--;
                }
            }
        }

        if (!signatureFound)
        {
            return false;
        }
        else
        {
            stream.Seek(bufferPointer, SeekOrigin.Current);
            return true;
        }
    }

    // Skip to a further position downstream (without relying on the stream being seekable)
    internal static void AdvanceToPosition(this Stream stream, long position, ZipContext context)
    {
        long numBytesLeft = position - stream.Position;
        Debug.Assert(numBytesLeft >= 0);
        while (numBytesLeft != 0)
        {
            int numBytesToSkip = numBytesLeft > ZipContext.ThrowAwayBufferSize ? ZipContext.ThrowAwayBufferSize : (int)numBytesLeft;
            int numBytesActuallySkipped = stream.Read(context.ThrowawayBuffer, 0, numBytesToSkip);
            if (numBytesActuallySkipped == 0) ThrowHelper.IOException(SR.UnexpectedEndOfStream);
            numBytesLeft -= numBytesActuallySkipped;
        }
    }

    // Returns true if we are out of bytes
    private static bool SeekBackwardsAndRead(Stream stream, byte[] buffer, out int bufferPointer)
    {
        if (stream.Position >= buffer.Length)
        {
            stream.Seek(-buffer.Length, SeekOrigin.Current);
            ReadBytes(stream, buffer, buffer.Length);
            stream.Seek(-buffer.Length, SeekOrigin.Current);
            bufferPointer = buffer.Length - 1;
            return false;
        }
        else
        {
            int bytesToRead = (int)stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            ReadBytes(stream, buffer, bytesToRead);
            stream.Seek(0, SeekOrigin.Begin);
            bufferPointer = bytesToRead - 1;
            return true;
        }
    }

    // These come from BitConverter.ToInt32/64 methods (.NET 8 version)
    internal static uint ReadUInt32(byte[] value, int valueLength, int startIndex)
    {
        if (unchecked((uint)startIndex) >= unchecked((uint)valueLength))
        {
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), "ArgumentOutOfRange_Index");
        }
        if (startIndex > valueLength - sizeof(int))
        {
            ThrowHelper.ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        return unchecked((uint)Unsafe.ReadUnaligned<int>(ref value[startIndex]));
    }

    internal static long ReadInt64(byte[] value, int valueLength, int startIndex)
    {
        if (unchecked((uint)startIndex) >= unchecked((uint)valueLength))
        {
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), "ArgumentOutOfRange_Index");
        }
        if (startIndex > valueLength - sizeof(long))
        {
            ThrowHelper.ArgumentException("Arg_ArrayPlusOffTooSmall");
        }

        return Unsafe.ReadUnaligned<long>(ref value[startIndex]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string GetUnsupportedCompressionMethodErrorMessage(CompressionMethodValues compressionMethod)
    {
        return string.Format(SR.UnsupportedCompressionMethod, compressionMethod.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool CompressionMethodSupported(CompressionMethodValues compressionMethod) =>
        compressionMethod
            is CompressionMethodValues.Stored
            or CompressionMethodValues.Deflate
            or CompressionMethodValues.Deflate64;

    internal static Stream GetDataDecompressor(ZipArchiveFastEntry entry, SubReadStream compressedStreamToRead)
    {
        Stream uncompressedStream;
        switch (entry.CompressionMethod)
        {
            case CompressionMethodValues.Deflate:
                uncompressedStream = new System.IO.Compression.DeflateStream(compressedStreamToRead, System.IO.Compression.CompressionMode.Decompress, leaveOpen: true);
                break;
            case CompressionMethodValues.Deflate64:
                // This is always in decompress-only mode
                uncompressedStream = new Deflate64ManagedStream(compressedStreamToRead);
                break;
            case CompressionMethodValues.Stored:
            default:
                // we can assume that only deflate/deflate64/stored are allowed because we assume that
                // IsOpenable is checked before this function is called
                Debug.Assert(entry.CompressionMethod == CompressionMethodValues.Stored);

                uncompressedStream = compressedStreamToRead;
                break;
        }

        return uncompressedStream;
    }

    internal static bool IsOpenable(
        ZipArchiveFastEntry entry,
        Stream archiveStream,
        long archiveStreamLength,
        BinaryBuffer binaryReadBuffer,
        out string message)
    {
        message = "";

        if (!CompressionMethodSupported(entry.CompressionMethod))
        {
            message = GetUnsupportedCompressionMethodErrorMessage(entry.CompressionMethod);
            return false;
        }

        if (entry.StoredOffsetOfCompressedData != null)
        {
            archiveStream.Seek((long)entry.StoredOffsetOfCompressedData, SeekOrigin.Begin);
            return true;
        }

        if (entry.OffsetOfLocalHeader > archiveStreamLength)
        {
            message = SR.LocalFileHeaderCorrupt;
            return false;
        }

        archiveStream.Seek(entry.OffsetOfLocalHeader, SeekOrigin.Begin);
        if (!ZipLocalFileHeader.TrySkipBlock(archiveStream, archiveStreamLength, binaryReadBuffer))
        {
            message = SR.LocalFileHeaderCorrupt;
            return false;
        }

        entry.StoredOffsetOfCompressedData ??= archiveStream.Position;

        if (entry.StoredOffsetOfCompressedData + entry.CompressedLength > archiveStreamLength)
        {
            message = SR.LocalFileHeaderCorrupt;
            return false;
        }

        return true;
    }
}

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using AL_Common.FastZipReader.Deflate64Managed;
using AL_Common.NETM_IO;
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
public sealed class ZipContext
{
    internal readonly ListFast<ZipArchiveFastEntry> Entries = new(0);

    internal readonly SubReadStream ArchiveSubReadStream = new();

    public readonly byte[] FileStreamBuffer = new byte[4096];

    internal readonly byte[] DataBuffer = new byte[65536];
    internal readonly byte[] FilenameBuffer = new byte[65536];

    private const int _backwardsSeekingBufferSize = 32;
    internal const int EntryFieldsBufferSize = 46;

    internal readonly byte[] BackwardsSeekingBuffer = new byte[_backwardsSeekingBufferSize];
    internal readonly byte[] EntryFieldsBuffer = new byte[EntryFieldsBufferSize];

    internal readonly BinaryBuffer BinaryReadBuffer = new();

    public void Set()
    {
        ArchiveSubReadStream.SetSuperStream(null);
    }

    public void Unset()
    {
        ArchiveSubReadStream.SetSuperStream(null);
    }

    // Keep it simple and don't dispose sub read stream as disposal is a no-op for it
}

public sealed class ZipContext_Pool
{
    private readonly ConcurrentBag<ZipContext> _contexts = new();

    public ZipContext Rent()
    {
        if (_contexts.TryTake(out ZipContext item))
        {
            item.Set();
            return item;
        }
        else
        {
            return new ZipContext();
        }
    }

    public void Return(ZipContext item)
    {
        item.Unset();
        _contexts.Add(item);
    }
}

public readonly ref struct ZipContextRentScope
{
    private readonly ZipContext_Pool _pool;
    public readonly ZipContext Ctx;

    public ZipContextRentScope(ZipContext_Pool pool)
    {
        _pool = pool;
        Ctx = pool.Rent();
    }

    public void Dispose()
    {
        _pool.Return(Ctx);
    }
}

public sealed class ZipContext_Threaded
{
    internal FileStream_NET ArchiveStream = null!;
    internal long ArchiveStreamLength;

    internal readonly SubReadStream ArchiveSubReadStream;

    internal readonly BinaryBuffer BinaryReadBuffer = new();

    internal readonly byte[] TempBuffer = new byte[StreamCopyBufferSize];

    // Take the length explicitly so that if a stream throws on Length access it'll do it somewhere else so we
    // won't have any problems in here.
    public ZipContext_Threaded(FileStream_NET archiveStream, long archiveStreamLength)
    {
        ArchiveSubReadStream = new SubReadStream();
        Set(archiveStream, archiveStreamLength);
    }

    public void Set(FileStream_NET archiveStream, long archiveStreamLength)
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
    // We hold the sub read stream but we don't need to dispose it because disposal is a no-op on that one.
}

public sealed class ZipContext_Threaded_Pool
{
    private readonly ConcurrentBag<ZipContext_Threaded> _contexts = new();

    public ZipContext_Threaded Rent(FileStream_NET archiveStream, long archiveStreamLength)
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

public readonly ref struct ZipContextThreadedRentScope
{
    private readonly ZipContext_Threaded_Pool _pool;
    public readonly ZipContext_Threaded Ctx;

    public ZipContextThreadedRentScope(ZipContext_Threaded_Pool pool, FileStream_NET fs, long fsLength)
    {
        _pool = pool;
        Ctx = pool.Rent(fs, fsLength);
    }

    public void Dispose()
    {
        _pool.Return(Ctx);
    }
}

internal static class ZipArchiveFast_Common
{
    internal const uint Mask32Bit = 0xFFFFFFFF;
    internal const ushort Mask16Bit = 0xFFFF;

    /// <summary>
    /// Reads exactly bytesToRead out of stream, unless it is out of bytes
    /// </summary>
    private static void ReadBytes(FileStream_NET stream, byte[] buffer, int bytesToRead)
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
    internal static bool SeekBackwardsToSignature(FileStream_NET stream, uint signatureToFind, ZipContext context)
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

    // Returns true if we are out of bytes
    private static bool SeekBackwardsAndRead(FileStream_NET stream, byte[] buffer, out int bufferPointer)
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
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), SR.ArgumentOutOfRange_IndexMustBeLess);
        }
        if (startIndex > valueLength - sizeof(int))
        {
            ThrowHelper.ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
        }

        return unchecked((uint)Unsafe.ReadUnaligned<int>(ref value[startIndex]));
    }

    internal static long ReadInt64(byte[] value, int valueLength, int startIndex)
    {
        if (unchecked((uint)startIndex) >= unchecked((uint)valueLength))
        {
            ThrowHelper.ArgumentOutOfRange(nameof(startIndex), SR.ArgumentOutOfRange_IndexMustBeLess);
        }
        if (startIndex > valueLength - sizeof(long))
        {
            ThrowHelper.ArgumentException(SR.Arg_ArrayPlusOffTooSmall);
        }

        return Unsafe.ReadUnaligned<long>(ref value[startIndex]);
    }

    #region Fast readers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static short ReadInt16_Fast(byte[] value, ref int startIndex)
    {
        short ret = Unsafe.ReadUnaligned<short>(ref value[startIndex]);
        startIndex += ByteLengths.Int16;
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ushort ReadUInt16_Fast(byte[] value, ref int startIndex) => unchecked((ushort)ReadInt16_Fast(value, ref startIndex));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int ReadInt32_Fast(byte[] value, ref int startIndex)
    {
        int ret = Unsafe.ReadUnaligned<int>(ref value[startIndex]);
        startIndex += ByteLengths.Int32;
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint ReadUInt32_Fast(byte[] value, ref int startIndex) => unchecked((uint)ReadInt32_Fast(value, ref startIndex));

#if false
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long ReadInt64_Fast(byte[] value, ref int startIndex)
    {
        long ret = Unsafe.ReadUnaligned<long>(ref value[startIndex]);
        startIndex += ByteLengths.Int64;
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64_Fast(byte[] value, ref int startIndex) => unchecked((ulong)ReadInt64_Fast(value, ref startIndex));
#endif

    #endregion

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
        FileStream_NET archiveStream,
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

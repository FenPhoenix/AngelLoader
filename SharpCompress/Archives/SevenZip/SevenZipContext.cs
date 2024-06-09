using System.Buffers;
using SharpCompress.Common.SevenZip;

namespace SharpCompress.Archives.SevenZip;

public sealed class SevenZipContext
{
    public readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Create();
    public readonly ArrayPool<int> IntArrayPool = ArrayPool<int>.Create();
    public readonly ArrayPool<long> LongArrayPool = ArrayPool<long>.Create();

    public const int SubStreamBufferLength = 32768;
    public readonly byte[] SubStreamBuffer = new byte[SubStreamBufferLength];

    public readonly byte[] Byte1 = new byte[1];
    public readonly uint[] CFolder_Mask = new uint[CFolder.kMaskSize];

    public readonly byte[] ArchiveHeader = new byte[0x20];

    internal readonly ArchiveDatabase ArchiveDatabase = new();
}

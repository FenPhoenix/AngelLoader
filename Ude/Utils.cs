using System.IO;
using System.IO.Compression;

namespace Ude.NetStandard;
internal static class Utils
{
    public static byte[] Decompress(byte[] bytes, int finalSize)
    {
        using var decompressedMS = new MemoryStream(finalSize);
        using var compressedMS = new MemoryStream(bytes);
        using var ds = new DeflateStream(compressedMS, CompressionMode.Decompress);
        ds.CopyTo(decompressedMS);
        return decompressedMS.GetBuffer();
    }
}

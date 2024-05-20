namespace Ude.NetStandard;

internal static class Utils
{
#if CompressUdeDataArrays
    internal static byte[] Decompress(byte[] bytes, int finalSize)
    {
        using var decompressedMS = new System.IO.MemoryStream(finalSize);
        using var compressedMS = new System.IO.MemoryStream(bytes);
        using var ds = new System.IO.Compression.DeflateStream(compressedMS, System.IO.Compression.CompressionMode.Decompress);
        ds.CopyTo(decompressedMS);
        return decompressedMS.GetBuffer();
    }
#endif

    internal static int UnpackBitPackage(int[] data, int i)
    {
        const int INDEX_SHIFT_4BITS = 3;
        const int SHIFT_MASK_4BITS = 7;
        const int BIT_SHIFT_4BITS = 2;
        const int UNIT_MASK_4BITS = 0x0000000F;

        return data[i >> INDEX_SHIFT_4BITS] >> ((i & SHIFT_MASK_4BITS) << BIT_SHIFT_4BITS) & UNIT_MASK_4BITS;
    }
}

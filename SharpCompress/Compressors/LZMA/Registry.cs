using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Compressors.BZip2;
using SharpCompress.Compressors.Filters;
using SharpCompress.Compressors.PPMd;

namespace SharpCompress.Compressors.LZMA;

internal static class DecoderRegistry
{
    private const uint K_COPY = 0x0;
    private const uint K_DELTA = 0x3;
    private const uint K_LZMA2 = 0x21;
    private const uint K_LZMA = 0x030101;
    private const uint K_PPMD = 0x030401;
    private const uint K_BCJ = 0x03030103;
    private const uint K_BCJ2 = 0x0303011B;
    private const uint K_DEFLATE = 0x040108;
    private const uint K_B_ZIP2 = 0x040202;
    private const ulong K_AES_ID = 0x06F10701;

    internal static Stream CreateDecoderStream(
        ulong id,
        Stream[] inStreams,
        byte[] info,
        long limit,
        SevenZipContext context
    )
    {
        switch (id)
        {
            case K_COPY:
                if (info != null)
                {
                    throw new NotSupportedException();
                }
                return inStreams.Single();
            case K_DELTA:
                return new DeltaFilter(inStreams.Single(), info);
            case K_LZMA:
            case K_LZMA2:
                return new LzmaStream(info, inStreams.Single(), -1, limit, context);
            case K_AES_ID:
                throw new Common.CryptographicException("7Zip archive is encrypted; this is not supported.");
            case K_BCJ:
                return new BCJFilter(inStreams.Single());
            case K_BCJ2:
                return new Bcj2DecoderStream(inStreams, info);
            case K_B_ZIP2:
                return new BZip2Stream(inStreams.Single());
            case K_PPMD:
                return new PpmdStream(new PpmdProperties(info), inStreams.Single(), context);
            case K_DEFLATE:
                return new DeflateStream(inStreams.Single(), CompressionMode.Decompress);
            default:
                throw new NotSupportedException();
        }
    }
}

namespace SharpCompress.Compressors.Rar.UnpackV1.Decode;

internal enum FilterType : byte
{
    // These values must not be changed, because we use them directly
    // in RAR5 compression and decompression code.
    FILTER_DELTA = 0
}

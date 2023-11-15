namespace SharpCompress_7z.Compressors.Rar.UnpackV2017;

internal static class PackDef
{
    // Combine pack and unpack constants to class to avoid polluting global
    // namespace with numerous short names.
    public const uint MAX_LZ_MATCH = 0x1001;

    public const uint NC = 306; /* alphabet = {0, 1, 2, ..., NC - 1} */
    public const uint DC = 64;
    public const uint LDC = 16;
    public const uint RC = 44;
    public const uint HUFF_TABLE_SIZE = NC + DC + RC + LDC;
    public const uint BC = 20;

    public const uint NC30 = 299; /* alphabet = {0, 1, 2, ..., NC - 1} */

    public const uint NC20 = 298; /* alphabet = {0, 1, 2, ..., NC - 1} */
    public const uint DC20 = 48;
    public const uint RC20 = 28;
    public const uint BC20 = 19;
    public const uint MC20 = 257;

    // Largest alphabet size among all values listed above.
    public const uint LARGEST_TABLE_SIZE = 306;

    //    enum {
    //      CODE_HUFFMAN, CODE_LZ, CODE_REPEATLZ, CODE_CACHELZ, CODE_STARTFILE,
    //      CODE_ENDFILE, CODE_FILTER, CODE_FILTERDATA
    //    };


    //enum FilterType {
    // These values must not be changed, because we use them directly
    // in RAR5 compression and decompression code.
    public const int FILTER_DELTA = 0;
    public const int FILTER_E8 = 1;
    public const int FILTER_E8E9 = 2;
    public const int FILTER_ARM = 3;
    public const int FILTER_NONE = 8;
    //}
}

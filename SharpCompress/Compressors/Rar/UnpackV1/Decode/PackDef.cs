namespace SharpCompress.Compressors.Rar.UnpackV1.Decode;

internal static class PackDef
{
    // 20171217 NOTE: these contants are gone from unrar src code
    // seems to be more dynamic
    public const int MAXWINSIZE = 0x400000;
    public const int MAXWINMASK = MAXWINSIZE - 1;

    public const uint MAX_LZ_MATCH = 0x1001;
    public const int LOW_DIST_REP_COUNT = 16;

    public const int NC = 299; /* alphabet = {0, 1, 2, ..., NC - 1} */
    public const int DC = 60;
    public const int LDC = 17;
    public const int RC = 28;

    // 20171217: NOTE: these constants seem to have been updated in the unrar src code
    // at some unknown point.  updating causes decompression failure, not sure why.
    //        public const int NC    = 306; /* alphabet = {0, 1, 2, ..., NC - 1} */
    //        public const int DC    = 64;
    //        public const int LDC   = 16;
    //        public const int RC    = 44;
    public const int HUFF_TABLE_SIZE = NC + DC + RC + LDC;
    public const int BC = 20;

    public const int NC20 = 298; /* alphabet = {0, 1, 2, ..., NC - 1} */
    public const int DC20 = 48;
    public const int RC20 = 28;
    public const int BC20 = 19;
    public const int MC20 = 257;
}

namespace SharpCompress.Compressors.LZMA;

internal static class Crc
{
    internal const uint INIT_CRC = 0xFFFFFFFF;
    private static readonly uint[] TABLE = new uint[4 * 256];

    static Crc()
    {
        const uint kCrcPoly = 0xEDB88320;

        for (uint i = 0; i < 256; i++)
        {
            uint r = i;
            for (int j = 0; j < 8; j++)
            {
                r = (r >> 1) ^ (kCrcPoly & ~((r & 1) - 1));
            }

            TABLE[i] = r;
        }

        for (uint i = 256; i < TABLE.Length; i++)
        {
            uint r = TABLE[i - 256];
            TABLE[i] = TABLE[r & 0xFF] ^ (r >> 8);
        }
    }

    public static uint Finish(uint crc) => ~crc;

    private static uint Update(uint crc, byte bt) => TABLE[(crc & 0xFF) ^ bt] ^ (crc >> 8);

    public static uint Update(uint crc, uint value)
    {
        crc ^= value;
        return TABLE[0x300 + (crc & 0xFF)]
            ^ TABLE[0x200 + ((crc >> 8) & 0xFF)]
            ^ TABLE[0x100 + ((crc >> 16) & 0xFF)]
            ^ TABLE[0x000 + (crc >> 24)];
    }

    private static uint Update(uint crc, ulong value) =>
        Update(Update(crc, (uint)value), (uint)(value >> 32));

    public static uint Update(uint crc, long value) => Update(crc, (ulong)value);

    public static uint Update(uint crc, byte[] buffer, int offset, int length)
    {
        for (int i = 0; i < length; i++)
        {
            crc = Update(crc, buffer[offset + i]);
        }

        return crc;
    }
}

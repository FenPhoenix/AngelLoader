namespace Ude.NetStandard;

internal static class Utils
{
    internal static int UnpackBitPackage(int[] data, int i)
    {
        const int INDEX_SHIFT_4BITS = 3;
        const int SHIFT_MASK_4BITS = 7;
        const int BIT_SHIFT_4BITS = 2;
        const int UNIT_MASK_4BITS = 0x0000000F;

        return data[i >> INDEX_SHIFT_4BITS] >> ((i & SHIFT_MASK_4BITS) << BIT_SHIFT_4BITS) & UNIT_MASK_4BITS;
    }
}

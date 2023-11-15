#nullable disable

namespace SharpCompress.Compressors.PPMd.I1;

/// Allocate a single, large array and then provide sections of this array to callers.  Callers are provided with
/// instances of <see cref="Pointer"/> (which simply contain a single address value, representing a location
/// in the large array).  Callers can then cast <see cref="Pointer"/> to one of the following structures (all
/// of which also simply contain a single address value):
internal class Allocator
{
    // reserve space for the array of memory nodes

    private const uint N1 = 4;
    private const uint N2 = 4;
    private const uint N3 = 4;
    private const uint N4 = (128 + 3 - (1 * N1) - (2 * N2) - (3 * N3)) / 4;
    private const uint INDEX_COUNT = N1 + N2 + N3 + N4;

    private static readonly byte[] INDEX_TO_UNITS;
    private static readonly byte[] UNITS_TO_INDEX;

    public uint _allocatorSize;
    public uint _glueCount;
    public Pointer _baseUnit;
    public Pointer _lowUnit;
    public Pointer _highUnit;
    public Pointer _text;
    public Pointer _heap;

    public byte[] _memory;

    /// <summary>
    /// Initializes static read-only arrays used by the <see cref="Allocator"/>.
    /// </summary>
    static Allocator()
    {
        // Construct the static index to units lookup array.  It will contain the following values.
        //
        // 1 2 3 4 6 8 10 12 15 18 21 24 28 32 36 40 44 48 52 56 60 64 68 72 76 80 84 88 92 96 100 104 108
        // 112 116 120 124 128

        uint index;
        uint unitCount;

        INDEX_TO_UNITS = new byte[INDEX_COUNT];

        for (index = 0, unitCount = 1; index < N1; index++, unitCount += 1)
        {
            INDEX_TO_UNITS[index] = (byte)unitCount;
        }

        for (unitCount++; index < N1 + N2; index++, unitCount += 2)
        {
            INDEX_TO_UNITS[index] = (byte)unitCount;
        }

        for (unitCount++; index < N1 + N2 + N3; index++, unitCount += 3)
        {
            INDEX_TO_UNITS[index] = (byte)unitCount;
        }

        for (unitCount++; index < N1 + N2 + N3 + N4; index++, unitCount += 4)
        {
            INDEX_TO_UNITS[index] = (byte)unitCount;
        }

        // Construct the static units to index lookup array.  It will contain the following values.
        //
        // 00 01 02 03 04 04 05 05 06 06 07 07 08 08 08 09 09 09 10 10 10 11 11 11 12 12 12 12 13 13 13 13
        // 14 14 14 14 15 15 15 15 16 16 16 16 17 17 17 17 18 18 18 18 19 19 19 19 20 20 20 20 21 21 21 21
        // 22 22 22 22 23 23 23 23 24 24 24 24 25 25 25 25 26 26 26 26 27 27 27 27 28 28 28 28 29 29 29 29
        // 30 30 30 30 31 31 31 31 32 32 32 32 33 33 33 33 34 34 34 34 35 35 35 35 36 36 36 36 37 37 37 37

        UNITS_TO_INDEX = new byte[128];

        for (unitCount = index = 0; unitCount < 128; unitCount++)
        {
            index += (uint)((INDEX_TO_UNITS[index] < unitCount + 1) ? 1 : 0);
            UNITS_TO_INDEX[unitCount] = (byte)index;
        }
    }
}

#nullable disable



// This is a port of Dmitry Shkarin's PPMd Variant I Revision 1.
// Ported by Michael Bone (mjbone03@yahoo.com.au).

namespace SharpCompress.Compressors.PPMd.I1;

/// <summary>
/// The model.
/// </summary>
internal partial class Model
{
    private const byte UPPER_FREQUENCY = 5;

    private PpmContext _maximumContext;
    private readonly byte[] _numberStatisticsToBinarySummaryIndex = new byte[256];
    private readonly byte[] _probabilities = new byte[260];
    private byte _escapeCount;
    private int _modelOrder;
    private int _orderFall;
    private int _initialEscape;
    private int _initialRunLength;
    private int _runLength;
    private byte _previousSuccess;
    private byte _numberMasked;
    private ModelRestorationMethod _method;
    private PpmState _foundState; // found next state transition

    private Allocator _allocator;
    private Coder _coder;
    private PpmContext _minimumContext;
    private byte _numberStatistics;

    #region Public Methods

    public Model()
    {
        // Construct the conversion table for number statistics.  Initially it will contain the following values.
        //
        // 0 2 4 4 4 4 4 4 4 4 4 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
        // 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
        // 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
        // 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
        // 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
        // 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
        // 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6
        // 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6 6

        _numberStatisticsToBinarySummaryIndex[0] = 2 * 0;
        _numberStatisticsToBinarySummaryIndex[1] = 2 * 1;
        for (var index = 2; index < 11; index++)
        {
            _numberStatisticsToBinarySummaryIndex[index] = 2 * 2;
        }
        for (var index = 11; index < 256; index++)
        {
            _numberStatisticsToBinarySummaryIndex[index] = 2 * 3;
        }

        // Construct the probability table.  Initially it will contain the following values (depending on the value of
        // the upper frequency).
        //
        // 00 01 02 03 04 05 06 06 07 07 07 08 08 08 08 09 09 09 09 09 10 10 10 10 10 10 11 11 11 11 11 11
        // 11 12 12 12 12 12 12 12 12 13 13 13 13 13 13 13 13 13 14 14 14 14 14 14 14 14 14 14 15 15 15 15
        // 15 15 15 15 15 15 15 16 16 16 16 16 16 16 16 16 16 16 16 17 17 17 17 17 17 17 17 17 17 17 17 17
        // 18 18 18 18 18 18 18 18 18 18 18 18 18 18 19 19 19 19 19 19 19 19 19 19 19 19 19 19 19 20 20 20
        // 20 20 20 20 20 20 20 20 20 20 20 20 20 21 21 21 21 21 21 21 21 21 21 21 21 21 21 21 21 21 22 22
        // 22 22 22 22 22 22 22 22 22 22 22 22 22 22 22 22 23 23 23 23 23 23 23 23 23 23 23 23 23 23 23 23
        // 23 23 23 24 24 24 24 24 24 24 24 24 24 24 24 24 24 24 24 24 24 24 24 25 25 25 25 25 25 25 25 25
        // 25 25 25 25 25 25 25 25 25 25 25 25 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26 26
        // 26 26 27 27

        uint count = 1;
        uint step = 1;
        uint probability = UPPER_FREQUENCY;

        for (var index = 0; index < UPPER_FREQUENCY; index++)
        {
            _probabilities[index] = (byte)index;
        }

        for (int index = UPPER_FREQUENCY; index < 260; index++)
        {
            _probabilities[index] = (byte)probability;
            count--;
            if (count == 0)
            {
                step++;
                count = step;
                probability++;
            }
        }
    }

    #endregion
}

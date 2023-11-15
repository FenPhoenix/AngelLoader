#nullable disable

using System;

// This is a port of Dmitry Shkarin's PPMd Variant I Revision 1.
// Ported by Michael Bone (mjbone03@yahoo.com.au).

namespace SharpCompress.Compressors.PPMd.I1;

/// <summary>
/// The model.
/// </summary>
internal partial class Model
{
    public const uint SIGNATURE = 0x84acaf8fU;
    public const char VARIANT = 'I';
    public const int MAXIMUM_ORDER = 16; // maximum allowed model order

    private const byte UPPER_FREQUENCY = 5;
    private const byte INTERVAL_BIT_COUNT = 7;
    private const byte PERIOD_BIT_COUNT = 7;
    private const byte TOTAL_BIT_COUNT = INTERVAL_BIT_COUNT + PERIOD_BIT_COUNT;
    private const uint BINARY_SCALE = 1 << TOTAL_BIT_COUNT;
    private const uint MAXIMUM_FREQUENCY = 124;
    private const uint ORDER_BOUND = 9;

    private readonly See2Context[,] _see2Contexts;
    private readonly See2Context _emptySee2Context;
    private PpmContext _maximumContext;
    private readonly ushort[,] _binarySummary = new ushort[25, 64]; // binary SEE-contexts
    private readonly byte[] _numberStatisticsToBinarySummaryIndex = new byte[256];
    private readonly byte[] _probabilities = new byte[260];
    private readonly byte[] _characterMask = new byte[256];
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
    private readonly PpmState[] _decodeStates = new PpmState[256];

    private static readonly ushort[] INITIAL_BINARY_ESCAPES =
    {
        0x3CDD,
        0x1F3F,
        0x59BF,
        0x48F3,
        0x64A1,
        0x5ABC,
        0x6632,
        0x6051
    };

    private static ReadOnlySpan<byte> EXPONENTIAL_ESCAPES =>
        new byte[] { 25, 14, 9, 7, 5, 5, 4, 4, 4, 3, 3, 3, 2, 2, 2, 2 };

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

        // Create the context array.

        _see2Contexts = new See2Context[24, 32];
        for (var index1 = 0; index1 < 24; index1++)
        {
            for (var index2 = 0; index2 < 32; index2++)
            {
                _see2Contexts[index1, index2] = new See2Context();
            }
        }

        // Set the signature (identifying the algorithm).

        _emptySee2Context = new See2Context();
        _emptySee2Context._summary = (ushort)(SIGNATURE & 0x0000ffff);
        _emptySee2Context._shift = (byte)((SIGNATURE >> 16) & 0x000000ff);
        _emptySee2Context._count = (byte)(SIGNATURE >> 24);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialise the model (unless the model order is set to 1 in which case the model should be cleared so that
    /// the statistics are carried over, allowing "solid" mode compression).
    /// </summary>
    private void StartModel(int modelOrder, ModelRestorationMethod modelRestorationMethod)
    {
        Array.Clear(_characterMask, 0, _characterMask.Length);
        _escapeCount = 1;

        // Compress in "solid" mode if the model order value is set to 1 (this will examine the current PPM context
        // structures to determine the value of orderFall).

        if (modelOrder < 2)
        {
            _orderFall = _modelOrder;
            for (
                var context = _maximumContext;
                context.Suffix != PpmContext.ZERO;
                context = context.Suffix
            )
            {
                _orderFall--;
            }
            return;
        }

        _modelOrder = modelOrder;
        _orderFall = modelOrder;
        _method = modelRestorationMethod;
        _allocator.Initialize();
        _initialRunLength = -((modelOrder < 12) ? modelOrder : 12) - 1;
        _runLength = _initialRunLength;

        // Allocate the context structure.

        _maximumContext = _allocator.AllocateContext();
        _maximumContext.Suffix = PpmContext.ZERO;
        _maximumContext.NumberStatistics = 255;
        _maximumContext.SummaryFrequency = (ushort)(_maximumContext.NumberStatistics + 2);
        _maximumContext.Statistics = _allocator.AllocateUnits(256 / 2);

        // allocates enough space for 256 PPM states (each is 6 bytes)

        _previousSuccess = 0;
        for (var index = 0; index < 256; index++)
        {
            var state = _maximumContext.Statistics[index];
            state.Symbol = (byte)index;
            state.Frequency = 1;
            state.Successor = PpmContext.ZERO;
        }

        uint probability = 0;
        for (var index1 = 0; probability < 25; probability++)
        {
            while (_probabilities[index1] == probability)
            {
                index1++;
            }
            for (var index2 = 0; index2 < 8; index2++)
            {
                _binarySummary[probability, index2] = (ushort)(
                    BINARY_SCALE - (INITIAL_BINARY_ESCAPES[index2] / (index1 + 1))
                );
            }
            for (var index2 = 8; index2 < 64; index2 += 8)
            {
                for (var index3 = 0; index3 < 8; index3++)
                {
                    _binarySummary[probability, index2 + index3] = _binarySummary[
                        probability,
                        index3
                    ];
                }
            }
        }

        probability = 0;
        for (uint index1 = 0; probability < 24; probability++)
        {
            while (_probabilities[index1 + 3] == probability + 3)
            {
                index1++;
            }
            for (var index2 = 0; index2 < 32; index2++)
            {
                _see2Contexts[probability, index2].Initialize((2 * index1) + 5);
            }
        }
    }

    private PpmContext CreateSuccessors(bool skip, PpmState state, PpmContext context)
    {
        var upBranch = _foundState.Successor;
        var states = new PpmState[MAXIMUM_ORDER];
        uint stateIndex = 0;
        var symbol = _foundState.Symbol;

        if (!skip)
        {
            states[stateIndex++] = _foundState;
            if (context.Suffix == PpmContext.ZERO)
            {
                goto NoLoop;
            }
        }

        var gotoLoopEntry = false;
        if (state != PpmState.ZERO)
        {
            context = context.Suffix;
            gotoLoopEntry = true;
        }

        do
        {
            if (gotoLoopEntry)
            {
                gotoLoopEntry = false;
                goto LoopEntry;
            }

            context = context.Suffix;
            if (context.NumberStatistics != 0)
            {
                byte temporary;
                state = context.Statistics;
                if (state.Symbol != symbol)
                {
                    do
                    {
                        temporary = state[1].Symbol;
                        state++;
                    } while (temporary != symbol);
                }
                temporary = (byte)((state.Frequency < MAXIMUM_FREQUENCY - 9) ? 1 : 0);
                state.Frequency += temporary;
                context.SummaryFrequency += temporary;
            }
            else
            {
                state = context.FirstState;
                state.Frequency += (byte)(
                    ((context.Suffix.NumberStatistics == 0) ? 1 : 0)
                    & ((state.Frequency < 24) ? 1 : 0)
                );
            }

            LoopEntry:
            if (state.Successor != upBranch)
            {
                context = state.Successor;
                break;
            }
            states[stateIndex++] = state;
        } while (context.Suffix != PpmContext.ZERO);

        NoLoop:
        if (stateIndex == 0)
        {
            return context;
        }

        byte localNumberStatistics = 0;
        var localFlags = (byte)((symbol >= 0x40) ? 0x10 : 0x00);
        symbol = upBranch.NumberStatistics;
        var localSymbol = symbol;
        byte localFrequency;
        PpmContext localSuccessor = ((Pointer)upBranch) + 1;
        localFlags |= (byte)((symbol >= 0x40) ? 0x08 : 0x00);

        if (context.NumberStatistics != 0)
        {
            state = context.Statistics;
            if (state.Symbol != symbol)
            {
                byte temporary;
                do
                {
                    temporary = state[1].Symbol;
                    state++;
                } while (temporary != symbol);
            }
            var cf = (uint)(state.Frequency - 1);
            var s0 = (uint)(context.SummaryFrequency - context.NumberStatistics - cf);
            localFrequency = (byte)(
                1 + ((2 * cf <= s0) ? (uint)((5 * cf > s0) ? 1 : 0) : ((cf + (2 * s0) - 3) / s0))
            );
        }
        else
        {
            localFrequency = context.FirstStateFrequency;
        }

        do
        {
            PpmContext currentContext = _allocator.AllocateContext();
            if (currentContext == PpmContext.ZERO)
            {
                return PpmContext.ZERO;
            }
            currentContext.NumberStatistics = localNumberStatistics;
            currentContext.Flags = localFlags;
            currentContext.FirstStateSymbol = localSymbol;
            currentContext.FirstStateFrequency = localFrequency;
            currentContext.FirstStateSuccessor = localSuccessor;
            currentContext.Suffix = context;
            context = currentContext;
            states[--stateIndex].Successor = context;
        } while (stateIndex != 0);

        return context;
    }

    private static void Swap(PpmState state1, PpmState state2)
    {
        var swapSymbol = state1.Symbol;
        var swapFrequency = state1.Frequency;
        var swapSuccessor = state1.Successor;

        state1.Symbol = state2.Symbol;
        state1.Frequency = state2.Frequency;
        state1.Successor = state2.Successor;

        state2.Symbol = swapSymbol;
        state2.Frequency = swapFrequency;
        state2.Successor = swapSuccessor;
    }

    private static void Copy(PpmState state1, PpmState state2)
    {
        state1.Symbol = state2.Symbol;
        state1.Frequency = state2.Frequency;
        state1.Successor = state2.Successor;
    }

    #endregion
}

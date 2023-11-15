#nullable disable

namespace SharpCompress.Compressors.PPMd.I1;

/// <summary>
/// The PPM context structure.  This is tightly coupled with <see cref="Model"/>.
/// </summary>
/// <remarks>
/// <para>
/// This must be a structure rather than a class because several places in the associated code assume that
/// <see cref="PpmContext"/> is a value type (meaning that assignment creates a completely new copy of
/// the instance rather than just copying a reference to the same instance).
/// </para>
/// </remarks>
internal partial class Model
{
    /// <summary>
    /// The structure which represents the current PPM context.  This is 12 bytes in size.
    /// </summary>
    internal struct PpmContext
    {
        public uint _address;
        public byte[] _memory;
        public static readonly PpmContext ZERO = new PpmContext(0, null);
        public const int SIZE = 12;

        /// <summary>
        /// Initializes a new instance of the <see cref="PpmContext"/> structure.
        /// </summary>
        public PpmContext(uint address, byte[] memory)
        {
            _address = address;
            _memory = memory;
        }

        /// <summary>
        /// Gets or sets the number statistics.
        /// </summary>
        public byte NumberStatistics
        {
            get => _memory[_address];
            set => _memory[_address] = value;
        }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        public byte Flags
        {
            get => _memory[_address + 1];
            set => _memory[_address + 1] = value;
        }

        /// <summary>
        /// Gets or sets the summary frequency.
        /// </summary>
        public ushort SummaryFrequency
        {
            get => (ushort)(_memory[_address + 2] | (_memory[_address + 3] << 8));
            set
            {
                _memory[_address + 2] = (byte)value;
                _memory[_address + 3] = (byte)(value >> 8);
            }
        }

        /// <summary>
        /// Gets or sets the statistics.
        /// </summary>
        public PpmState Statistics
        {
            get =>
                new PpmState(
                    _memory[_address + 4]
                        | (((uint)_memory[_address + 5]) << 8)
                        | (((uint)_memory[_address + 6]) << 16)
                        | (((uint)_memory[_address + 7]) << 24),
                    _memory
                );
            set
            {
                _memory[_address + 4] = (byte)value._address;
                _memory[_address + 5] = (byte)(value._address >> 8);
                _memory[_address + 6] = (byte)(value._address >> 16);
                _memory[_address + 7] = (byte)(value._address >> 24);
            }
        }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        public PpmContext Suffix
        {
            get =>
                new PpmContext(
                    _memory[_address + 8]
                        | (((uint)_memory[_address + 9]) << 8)
                        | (((uint)_memory[_address + 10]) << 16)
                        | (((uint)_memory[_address + 11]) << 24),
                    _memory
                );
            set
            {
                _memory[_address + 8] = (byte)value._address;
                _memory[_address + 9] = (byte)(value._address >> 8);
                _memory[_address + 10] = (byte)(value._address >> 16);
                _memory[_address + 11] = (byte)(value._address >> 24);
            }
        }

        /// <summary>
        /// The first PPM state associated with the PPM context.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The first PPM state overlaps this PPM context instance (the context.SummaryFrequency and context.Statistics members
        /// of PpmContext use 6 bytes and so can therefore fit into the space used by the Symbol, Frequency and
        /// Successor members of PpmState, since they also add up to 6 bytes).
        /// </para>
        /// <para>
        /// PpmContext (context.SummaryFrequency and context.Statistics use 6 bytes)
        ///     1 context.NumberStatistics
        ///     1 context.Flags
        ///     2 context.SummaryFrequency
        ///     4 context.Statistics (pointer to PpmState)
        ///     4 context.Suffix (pointer to PpmContext)
        /// </para>
        /// <para>
        /// PpmState (total of 6 bytes)
        ///     1 Symbol
        ///     1 Frequency
        ///     4 Successor (pointer to PpmContext)
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public PpmState FirstState => new PpmState(_address + 2, _memory);

        /// <summary>
        /// Gets or sets the symbol of the first PPM state.  This is provided for convenience.  The same
        /// information can be obtained using the Symbol property on the PPM state provided by the
        /// <see cref="FirstState"/> property.
        /// </summary>
        public byte FirstStateSymbol
        {
            get => _memory[_address + 2];
            set => _memory[_address + 2] = value;
        }

        /// <summary>
        /// Gets or sets the frequency of the first PPM state.  This is provided for convenience.  The same
        /// information can be obtained using the Frequency property on the PPM state provided by the
        ///context.FirstState property.
        /// </summary>
        public byte FirstStateFrequency
        {
            get => _memory[_address + 3];
            set => _memory[_address + 3] = value;
        }

        /// <summary>
        /// Gets or sets the successor of the first PPM state.  This is provided for convenience.  The same
        /// information can be obtained using the Successor property on the PPM state provided by the
        /// </summary>
        public PpmContext FirstStateSuccessor
        {
            get =>
                new PpmContext(
                    _memory[_address + 4]
                        | (((uint)_memory[_address + 5]) << 8)
                        | (((uint)_memory[_address + 6]) << 16)
                        | (((uint)_memory[_address + 7]) << 24),
                    _memory
                );
            set
            {
                _memory[_address + 4] = (byte)value._address;
                _memory[_address + 5] = (byte)(value._address >> 8);
                _memory[_address + 6] = (byte)(value._address >> 16);
                _memory[_address + 7] = (byte)(value._address >> 24);
            }
        }

        /// <summary>
        /// Allow a pointer to be implicitly converted to a PPM context.
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static implicit operator PpmContext(Pointer pointer) =>
            new PpmContext(pointer._address, pointer._memory);

        /// <summary>
        /// Allow pointer-like addition on a PPM context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static PpmContext operator +(PpmContext context, int offset)
        {
            context._address = (uint)(context._address + (offset * SIZE));
            return context;
        }

        /// <summary>
        /// Allow pointer-like subtraction on a PPM context.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static PpmContext operator -(PpmContext context, int offset)
        {
            context._address = (uint)(context._address - (offset * SIZE));
            return context;
        }

        /// <summary>
        /// Compare two PPM contexts.
        /// </summary>
        /// <param name="context1"></param>
        /// <param name="context2"></param>
        /// <returns></returns>
        public static bool operator <=(PpmContext context1, PpmContext context2) =>
            context1._address <= context2._address;

        /// <summary>
        /// Compare two PPM contexts.
        /// </summary>
        /// <param name="context1"></param>
        /// <param name="context2"></param>
        /// <returns></returns>
        public static bool operator >=(PpmContext context1, PpmContext context2) =>
            context1._address >= context2._address;

        /// <summary>
        /// Compare two PPM contexts.
        /// </summary>
        /// <param name="context1"></param>
        /// <param name="context2"></param>
        /// <returns></returns>
        public static bool operator ==(PpmContext context1, PpmContext context2) =>
            context1._address == context2._address;

        /// <summary>
        /// Compare two PPM contexts.
        /// </summary>
        /// <param name="context1"></param>
        /// <param name="context2"></param>
        /// <returns></returns>
        public static bool operator !=(PpmContext context1, PpmContext context2) =>
            context1._address != context2._address;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
        /// <param name="obj">Another object to compare to.</param>
        public override bool Equals(object obj)
        {
            if (obj is PpmContext context)
            {
                return context._address == _address;
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => _address.GetHashCode();
    }

    private void Refresh(uint oldUnitCount, bool scale, PpmContext context)
    {
        int index = context.NumberStatistics;
        int escapeFrequency;
        var scaleValue = (scale ? 1 : 0);

        context.Statistics = _allocator.ShrinkUnits(
            context.Statistics,
            oldUnitCount,
            (uint)((index + 2) >> 1)
        );
        var statistics = context.Statistics;
        context.Flags = (byte)(
            (context.Flags & (0x10 + (scale ? 0x04 : 0x00)))
            + ((statistics.Symbol >= 0x40) ? 0x08 : 0x00)
        );
        escapeFrequency = context.SummaryFrequency - statistics.Frequency;
        statistics.Frequency = (byte)((statistics.Frequency + scaleValue) >> scaleValue);
        context.SummaryFrequency = statistics.Frequency;

        do
        {
            escapeFrequency -= (++statistics).Frequency;
            statistics.Frequency = (byte)((statistics.Frequency + scaleValue) >> scaleValue);
            context.SummaryFrequency += statistics.Frequency;
            context.Flags |= (byte)((statistics.Symbol >= 0x40) ? 0x08 : 0x00);
        } while (--index != 0);

        escapeFrequency = (escapeFrequency + scaleValue) >> scaleValue;
        context.SummaryFrequency += (ushort)escapeFrequency;
    }

    private PpmContext CutOff(int order, PpmContext context)
    {
        int index;
        PpmState state;

        if (context.NumberStatistics == 0)
        {
            state = context.FirstState;
            if ((Pointer)state.Successor >= _allocator._baseUnit)
            {
                if (order < _modelOrder)
                {
                    state.Successor = CutOff(order + 1, state.Successor);
                }
                else
                {
                    state.Successor = PpmContext.ZERO;
                }

                if (state.Successor == PpmContext.ZERO && order > ORDER_BOUND)
                {
                    _allocator.SpecialFreeUnits(context);
                    return PpmContext.ZERO;
                }

                return context;
            }
            _allocator.SpecialFreeUnits(context);
            return PpmContext.ZERO;
        }

        var unitCount = (uint)((context.NumberStatistics + 2) >> 1);
        context.Statistics = _allocator.MoveUnitsUp(context.Statistics, unitCount);
        index = context.NumberStatistics;
        for (state = context.Statistics + index; state >= context.Statistics; state--)
        {
            if (state.Successor < _allocator._baseUnit)
            {
                state.Successor = PpmContext.ZERO;
                Swap(state, context.Statistics[index--]);
            }
            else if (order < _modelOrder)
            {
                state.Successor = CutOff(order + 1, state.Successor);
            }
            else
            {
                state.Successor = PpmContext.ZERO;
            }
        }

        if (index != context.NumberStatistics && order != 0)
        {
            context.NumberStatistics = (byte)index;
            state = context.Statistics;
            if (index < 0)
            {
                _allocator.FreeUnits(state, unitCount);
                _allocator.SpecialFreeUnits(context);
                return PpmContext.ZERO;
            }
            if (index == 0)
            {
                context.Flags = (byte)(
                    (context.Flags & 0x10) + ((state.Symbol >= 0x40) ? 0x08 : 0x00)
                );
                Copy(context.FirstState, state);
                _allocator.FreeUnits(state, unitCount);
                context.FirstStateFrequency = (byte)((context.FirstStateFrequency + 11) >> 3);
            }
            else
            {
                Refresh(unitCount, context.SummaryFrequency > 16 * index, context);
            }
        }

        return context;
    }

    private PpmContext RemoveBinaryContexts(int order, PpmContext context)
    {
        if (context.NumberStatistics == 0)
        {
            var state = context.FirstState;
            if ((Pointer)state.Successor >= _allocator._baseUnit && order < _modelOrder)
            {
                state.Successor = RemoveBinaryContexts(order + 1, state.Successor);
            }
            else
            {
                state.Successor = PpmContext.ZERO;
            }
            if (
                (state.Successor == PpmContext.ZERO)
                && (context.Suffix.NumberStatistics == 0 || context.Suffix.Flags == 0xff)
            )
            {
                _allocator.FreeUnits(context, 1);
                return PpmContext.ZERO;
            }
            return context;
        }

        for (
            var state = context.Statistics + context.NumberStatistics;
            state >= context.Statistics;
            state--
        )
        {
            if ((Pointer)state.Successor >= _allocator._baseUnit && order < _modelOrder)
            {
                state.Successor = RemoveBinaryContexts(order + 1, state.Successor);
            }
            else
            {
                state.Successor = PpmContext.ZERO;
            }
        }

        return context;
    }
}

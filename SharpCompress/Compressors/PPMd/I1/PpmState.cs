#nullable disable

namespace SharpCompress_7z.Compressors.PPMd.I1;

/// <summary>
/// PPM state.
/// </summary>
/// <remarks>
/// <para>
/// This must be a structure rather than a class because several places in the associated code assume that
/// <see cref="PpmState"/> is a value type (meaning that assignment creates a completely new copy of the
/// instance rather than just copying a reference to the same instance).
/// </para>
/// <para>
/// Note that <see cref="_address"/> is a field rather than a property for performance reasons.
/// </para>
/// </remarks>
internal struct PpmState
{
    public uint _address;
    public readonly byte[] _memory;
    public const int SIZE = 6;

    /// <summary>
    /// Initializes a new instance of the <see cref="PpmState"/> structure.
    /// </summary>
    public PpmState(uint address, byte[] memory)
    {
        _address = address;
        _memory = memory;
    }

    /// <summary>
    /// Allow a pointer to be implicitly converted to a PPM state.
    /// </summary>
    /// <param name="pointer"></param>
    /// <returns></returns>
    public static implicit operator PpmState(Pointer pointer) =>
        new PpmState(pointer._address, pointer._memory);

    /// <summary>
    /// Allow pointer-like addition on a PPM state.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static PpmState operator +(PpmState state, int offset)
    {
        state._address = (uint)(state._address + (offset * SIZE));
        return state;
    }

    /// <summary>
    /// Allow pointer-like incrementing on a PPM state.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static PpmState operator ++(PpmState state)
    {
        state._address += SIZE;
        return state;
    }

    /// <summary>
    /// Allow pointer-like subtraction on a PPM state.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static PpmState operator -(PpmState state, int offset)
    {
        state._address = (uint)(state._address - (offset * SIZE));
        return state;
    }

    /// <summary>
    /// Allow pointer-like decrementing on a PPM state.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static PpmState operator --(PpmState state)
    {
        state._address -= SIZE;
        return state;
    }

    /// <summary>
    /// Compare two PPM states.
    /// </summary>
    /// <param name="state1"></param>
    /// <param name="state2"></param>
    /// <returns></returns>
    public static bool operator <=(PpmState state1, PpmState state2) =>
        state1._address <= state2._address;

    /// <summary>
    /// Compare two PPM states.
    /// </summary>
    /// <param name="state1"></param>
    /// <param name="state2"></param>
    /// <returns></returns>
    public static bool operator >=(PpmState state1, PpmState state2) =>
        state1._address >= state2._address;

    /// <summary>
    /// Compare two PPM states.
    /// </summary>
    /// <param name="state1"></param>
    /// <param name="state2"></param>
    /// <returns></returns>
    public static bool operator ==(PpmState state1, PpmState state2) =>
        state1._address == state2._address;

    /// <summary>
    /// Compare two PPM states.
    /// </summary>
    /// <param name="state1"></param>
    /// <param name="state2"></param>
    /// <returns></returns>
    public static bool operator !=(PpmState state1, PpmState state2) =>
        state1._address != state2._address;

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
    /// <param name="obj">Another object to compare to.</param>
    public override bool Equals(object obj)
    {
        if (obj is PpmState state)
        {
            return state._address == _address;
        }
        return base.Equals(obj);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => _address.GetHashCode();
}

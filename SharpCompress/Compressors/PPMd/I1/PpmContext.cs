#nullable disable

namespace SharpCompress_7z.Compressors.PPMd.I1;

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
        public readonly byte[] _memory;
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
}

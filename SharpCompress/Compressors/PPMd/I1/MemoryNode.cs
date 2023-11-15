#nullable disable

namespace SharpCompress.Compressors.PPMd.I1;

/// <summary>
/// A structure containing a single address.  The address represents a location in the <see cref="_memory"/>
/// array.  That location in the <see cref="_memory"/> array contains information itself describing a section
/// of the <see cref="_memory"/> array (ie. a block of memory).
/// </summary>
/// <remarks>
/// <para>
/// This must be a structure rather than a class because several places in the associated code assume that
/// <see cref="MemoryNode"/> is a value type (meaning that assignment creates a completely new copy of
/// the instance rather than just copying a reference to the same instance).
/// </para>
/// <para>
/// MemoryNode
///     4 Stamp
///     4 Next
///     4 UnitCount
/// </para>
/// <para>
/// Note that <see cref="_address"/> is a field rather than a property for performance reasons.
/// </para>
/// </remarks>
internal struct MemoryNode
{
    public uint _address;
    public readonly byte[] _memory;
    public const int SIZE = 12;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryNode"/> structure.
    /// </summary>
    public MemoryNode(uint address, byte[] memory)
    {
        _address = address;
        _memory = memory;
    }

    /// <summary>
    /// Allow a pointer to be implicitly converted to a memory node.
    /// </summary>
    /// <param name="pointer"></param>
    /// <returns></returns>
    public static implicit operator MemoryNode(Pointer pointer) =>
        new MemoryNode(pointer._address, pointer._memory);

    /// <summary>
    /// Allow pointer-like addition on a memory node.
    /// </summary>
    /// <param name="memoryNode"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static MemoryNode operator +(MemoryNode memoryNode, int offset)
    {
        memoryNode._address = (uint)(memoryNode._address + (offset * SIZE));
        return memoryNode;
    }

    /// <summary>
    /// Allow pointer-like addition on a memory node.
    /// </summary>
    /// <param name="memoryNode"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static MemoryNode operator +(MemoryNode memoryNode, uint offset)
    {
        memoryNode._address += offset * SIZE;
        return memoryNode;
    }

    /// <summary>
    /// Allow pointer-like subtraction on a memory node.
    /// </summary>
    /// <param name="memoryNode"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static MemoryNode operator -(MemoryNode memoryNode, int offset)
    {
        memoryNode._address = (uint)(memoryNode._address - (offset * SIZE));
        return memoryNode;
    }

    /// <summary>
    /// Allow pointer-like subtraction on a memory node.
    /// </summary>
    /// <param name="memoryNode"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static MemoryNode operator -(MemoryNode memoryNode, uint offset)
    {
        memoryNode._address -= offset * SIZE;
        return memoryNode;
    }

    /// <summary>
    /// Compare two memory nodes.
    /// </summary>
    /// <param name="memoryNode1"></param>
    /// <param name="memoryNode2"></param>
    /// <returns></returns>
    public static bool operator ==(MemoryNode memoryNode1, MemoryNode memoryNode2) =>
        memoryNode1._address == memoryNode2._address;

    /// <summary>
    /// Compare two memory nodes.
    /// </summary>
    /// <param name="memoryNode1"></param>
    /// <param name="memoryNode2"></param>
    /// <returns></returns>
    public static bool operator !=(MemoryNode memoryNode1, MemoryNode memoryNode2) =>
        memoryNode1._address != memoryNode2._address;

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <returns>true if obj and this instance are the same type and represent the same value; otherwise, false.</returns>
    /// <param name="obj">Another object to compare to.</param>
    public override bool Equals(object obj)
    {
        if (obj is MemoryNode memoryNode)
        {
            return memoryNode._address == _address;
        }
        return base.Equals(obj);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => _address.GetHashCode();
}

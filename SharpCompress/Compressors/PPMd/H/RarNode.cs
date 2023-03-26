using System;
using System.Buffers.Binary;

namespace SharpCompress.Compressors.PPMd.H;

internal sealed class RarNode : Pointer
{
    private int _next; //rarnode pointer

    public const int SIZE = 4;

    public RarNode(byte[] memory)
        : base(memory) { }

    internal int GetNext()
    {
        if (Memory != null)
        {
            _next = BinaryPrimitives.ReadInt32LittleEndian(Memory.AsSpan(Address));
        }
        return _next;
    }

    internal void SetNext(RarNode next) => SetNext(next.Address);

    internal void SetNext(int next)
    {
        _next = next;
        if (Memory != null)
        {
            BinaryPrimitives.WriteInt32LittleEndian(Memory.AsSpan(Address), next);
        }
    }
}

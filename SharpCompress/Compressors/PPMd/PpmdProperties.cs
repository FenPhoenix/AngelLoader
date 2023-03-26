using System;
using System.Buffers.Binary;
using SharpCompress.Compressors.PPMd.I1;

namespace SharpCompress.Compressors.PPMd;

public sealed class PpmdProperties
{
    private int _allocatorSize;
    internal Allocator? _allocator;

    public readonly int ModelOrder;
    public readonly PpmdVersion Version = PpmdVersion.I1;
    internal readonly ModelRestorationMethod RestorationMethod;

    public PpmdProperties(byte[] properties)
        : this(properties.AsSpan()) { }

    private PpmdProperties(ReadOnlySpan<byte> properties)
    {
        if (properties.Length == 2)
        {
            var props = BinaryPrimitives.ReadUInt16LittleEndian(properties);
            AllocatorSize = (((props >> 4) & 0xff) + 1) << 20;
            ModelOrder = (props & 0x0f) + 1;
            RestorationMethod = (ModelRestorationMethod)(props >> 12);
        }
        else if (properties.Length == 5)
        {
            Version = PpmdVersion.H7Z;
            AllocatorSize = BinaryPrimitives.ReadInt32LittleEndian(properties.Slice(1));
            ModelOrder = properties[0];
        }
    }

    public int AllocatorSize
    {
        get => _allocatorSize;
        private set
        {
            _allocatorSize = value;
            if (Version == PpmdVersion.I1)
            {
                _allocator ??= new Allocator();

                _allocator.Start(_allocatorSize);
            }
        }
    }
}

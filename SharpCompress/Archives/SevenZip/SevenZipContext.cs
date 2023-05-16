using System.Buffers;

namespace SharpCompress.Archives.SevenZip;

#if false
public sealed class ArrayWithLength<T>
{
    public readonly T[] Array;
    public readonly int Length;

    public ArrayWithLength(T[] array, int length)
    {
        Array = array;
        Length = length;
    }

    public T this[int index]
    {
        get => Array[index];
        set => Array[index] = value;
    }
}
#endif

public sealed class SevenZipContext
{
#if false
    public readonly ref struct ByteRentScope
    {
        private readonly SevenZipContext _context;
        public readonly ArrayWithLength<byte> ByteArray;

        public ByteRentScope(SevenZipContext context, int minSize)
        {
            _context = context;
            ByteArray = new ArrayWithLength<byte>(context.ByteArrayPool.Rent(minSize), minSize);
        }

        public void Dispose()
        {
            _context.ByteArrayPool.Return(ByteArray.Array);
        }
    }
#endif

    public readonly ArrayPool<byte> ByteArrayPool = ArrayPool<byte>.Create();

    public const int SubStreamBufferLength = 32768;
    public readonly byte[] SubStreamBuffer = new byte[SubStreamBufferLength];

    public readonly byte[] Byte1 = new byte[1];
}

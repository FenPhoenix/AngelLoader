using System.Buffers;
using System.Collections.Generic;
using SharpCompress.Common.SevenZip;
using static AL_Common.Common;

namespace SharpCompress.Archives.SevenZip;

public readonly struct ArrayWithLength<T>
{
    public readonly T[] Array;
    public readonly int Length;

    public ArrayWithLength(T[] array, int length)
    {
        Array = array;
        Length = length;
    }

#if false
    public T this[int index]
    {
        get => Array[index];
        set => Array[index] = value;
    }
#endif
}

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
    public readonly ArrayPool<int> IntArrayPool = ArrayPool<int>.Create();

    public const int SubStreamBufferLength = 32768;
    public readonly byte[] SubStreamBuffer = new byte[SubStreamBufferLength];

    public readonly byte[] Byte1 = new byte[1];
    public readonly uint[] CFolder_Mask = new uint[CFolder.kMaskSize];
    public readonly ListFast<long> ListOfLong = new(16);

    public readonly byte[] ArchiveHeader = new byte[0x20];
    public readonly List<ArrayWithLength<byte>> ListOfOneByteArray = new(1);

    public List<ArrayWithLength<byte>> ClearAndReturn(List<ArrayWithLength<byte>> list)
    {
        int listCount = list.Count;
        for (int i = 0; i < listCount; i++)
        {
            ByteArrayPool.Return(list[i].Array);
        }
        list.Clear();
        return list;
    }

    public void ReturnPossiblyRentedArrays()
    {
        int listCount = ListOfOneByteArray.Count;
        for (int i = 0; i < listCount; i++)
        {
            ByteArrayPool.Return(ListOfOneByteArray[i].Array);
        }
    }
}

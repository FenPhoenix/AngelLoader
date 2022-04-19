using System;

namespace AngelLoader
{
    public readonly struct ByteArraySegmentSlim
    {
        public ByteArraySegmentSlim(byte[] array, int offset, int count)
        {
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset), "offset is negative");
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count), "count is negative");
            if (array.Length - offset < count) throw new ArgumentException("invalid offset length");
            Array = array;
            Offset = offset;
            Count = count;
        }

        public byte this[int index]
        {
            get => Array[Offset + index];
            set => Array[Offset + index] = value;
        }

        public byte[] Array { get; }

        public int Offset { get; }

        public int Count { get; }
    }
}

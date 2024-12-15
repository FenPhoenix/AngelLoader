using System.Collections.Concurrent;

namespace AL_Common;

public static partial class Common
{
    // @MT_TASK_NOTE: This is rented/returned in tight loops a lot, we should back it out to reduce call count
    public sealed class FixedLengthByteArrayPool
    {
        private readonly int _length;
        private readonly ConcurrentBag<byte[]> _items = new();

        public FixedLengthByteArrayPool(int length) => _length = length;

        public byte[] Rent() => _items.TryTake(out byte[] item) ? item : new byte[_length];

        public void Return(byte[] item) => _items.Add(item);
    }

    public sealed class IOBufferPools
    {
        public readonly FixedLengthByteArrayPool StreamCopy = new(StreamCopyBufferSize);
        public readonly FixedLengthByteArrayPool FileStream = new(FileStreamBufferSize);
    }
}

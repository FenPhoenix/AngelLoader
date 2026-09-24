using System.Buffers;

namespace Pfim
{
    public static class DefaultAllocator
    {
        public static byte[] Rent(int size)
        {
            return ArrayPool<byte>.Shared.Rent(size);
        }

        public static void Return(byte[] data)
        {
            ArrayPool<byte>.Shared.Return(data);
        }
    }
}

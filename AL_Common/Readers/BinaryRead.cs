using System.Buffers.Binary;
using System.IO;

namespace AL_Common;

public readonly struct BinaryBuffer()
{
    public readonly byte[] Buffer = new byte[8];
}

/// <summary>
/// Static methods as a replacement for BinaryReader, where you pass in your own buffer so it doesn't have to
/// allocate its own fifteen trillion times. Also, we avoid the unnecessary encoding junk that it also does
/// because we're reading binary, not encoded data, sheesh!
/// </summary>
public static class BinaryRead
{
    public static ushort ReadUInt16(Stream stream, BinaryBuffer buffer)
    {
        ReadInternal(stream, buffer.Buffer, 2);
        return BinaryPrimitives.ReadUInt16LittleEndian(buffer.Buffer);
    }

    public static uint ReadUInt32(Stream stream, BinaryBuffer buffer)
    {
        ReadInternal(stream, buffer.Buffer, 4);
        return BinaryPrimitives.ReadUInt32LittleEndian(buffer.Buffer);
    }

    public static ulong ReadUInt64(Stream stream, BinaryBuffer buffer)
    {
        ReadInternal(stream, buffer.Buffer, 8);
        return BinaryPrimitives.ReadUInt64LittleEndian(buffer.Buffer);
    }

    private static void ReadInternal(Stream stream, byte[] buffer, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = stream.Read(buffer, 0, count);
            if (read == 0)
            {
                if (totalRead != count)
                {
                    ThrowHelper.EndOfFile();
                }
                return;
            }

            totalRead += read;
        }
    }
}

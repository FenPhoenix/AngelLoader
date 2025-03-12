using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AL_Common;

[StructLayout(LayoutKind.Auto)]
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
    /// <summary>Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.</summary>
    /// <returns>A 2-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static ushort ReadUInt16(Stream stream, BinaryBuffer buffer)
    {
        ReadInternal(stream, buffer.Buffer, 2);
        return Unsafe.ReadUnaligned<ushort>(ref buffer.Buffer[0]);
    }

    /// <summary>Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.</summary>
    /// <returns>A 4-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    public static uint ReadUInt32(Stream stream, BinaryBuffer buffer)
    {
        ReadInternal(stream, buffer.Buffer, 4);
        return Unsafe.ReadUnaligned<uint>(ref buffer.Buffer[0]);
    }

    /// <summary>Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.</summary>
    /// <returns>An 8-byte unsigned integer read from this stream.</returns>
    /// <exception cref="T:System.IO.EndOfStreamException">The end of the stream is reached.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The stream is closed.</exception>
    public static ulong ReadUInt64(Stream stream, BinaryBuffer buffer)
    {
        ReadInternal(stream, buffer.Buffer, 8);
        return Unsafe.ReadUnaligned<ulong>(ref buffer.Buffer[0]);
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

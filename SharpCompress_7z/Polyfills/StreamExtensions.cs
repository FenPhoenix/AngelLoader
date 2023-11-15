#if NETFRAMEWORK || NETSTANDARD2_0

using System;
using System.Buffers;
using System.IO;

namespace SharpCompress_7z.Polyfills;

internal static class StreamExtensions
{
    internal static int Read(this Stream stream, Span<byte> buffer)
    {
        var temp = ArrayPool<byte>.Shared.Rent(buffer.Length);

        try
        {
            var read = stream.Read(temp, 0, buffer.Length);

            temp.AsSpan(0, read).CopyTo(buffer);

            return read;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(temp);
        }
    }
}

#endif

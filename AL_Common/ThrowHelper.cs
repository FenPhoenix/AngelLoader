using System;
using System.IO;
using AL_Common.FastZipReader;

namespace AL_Common;

internal static class ThrowHelper
{
    internal static void ArgumentException(string message) => throw new ArgumentException(message);
    internal static void ArgumentOutOfRange(string paramName, string message) => throw new ArgumentOutOfRangeException(paramName, message);
    internal static void EndOfFile() => throw new EndOfStreamException(SR.EOF_ReadBeyondEOF);
    internal static void InvalidData(string message) => throw new InvalidDataException(message);
    internal static void IOException(string message) => throw new IOException(message);
#if false
    internal static void NotSupported(string message) => throw new NotSupportedException(message);
#endif
    internal static void ReadModeCapabilities() => throw new ArgumentException(SR.ReadModeCapabilities);
    internal static void SplitSpanned() => throw new InvalidDataException(SR.SplitSpanned);
    internal static void ZipCompressionMethodException(string message) => throw new ZipCompressionMethodException(message);

    internal static void ReaderClosed() => throw new ObjectDisposedException(null, "ObjectDisposed_ReaderClosed");
}

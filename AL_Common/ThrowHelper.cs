using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AL_Common.FastZipReader;

namespace AL_Common;

public static class ThrowHelper
{
    [DoesNotReturn]
    public static void ArgumentException(string message) => throw new ArgumentException(message);
    [DoesNotReturn]
    public static void ArgumentException(string message, string paramName) => throw new ArgumentException(message, paramName);
    [DoesNotReturn]
    public static void ArgumentOutOfRange(string paramName, string message) => throw new ArgumentOutOfRangeException(paramName, message);
    [DoesNotReturn]
    public static void EndOfFile() => throw new EndOfStreamException(SR.EOF_ReadBeyondEOF);
    [DoesNotReturn]
    public static void InvalidData(string message) => throw new InvalidDataException(message);
    [DoesNotReturn]
    public static void InvalidData(string message, Exception innerException) => throw new InvalidDataException(message, innerException);
    [DoesNotReturn]
    public static void IOException(string message) => throw new IOException(message);
    [DoesNotReturn]
    public static void NotSupported(string message) => throw new NotSupportedException(message);
    [DoesNotReturn]
    public static void ReadModeCapabilities() => throw new ArgumentException(SR.ReadModeCapabilities);
    [DoesNotReturn]
    public static void SplitSpanned() => throw new InvalidDataException(SR.SplitSpanned);
    [DoesNotReturn]
    public static void ZipCompressionMethodException(string message) => throw new ZipCompressionMethodException(message);
    [DoesNotReturn]
    public static void ReaderClosed() => throw new ObjectDisposedException(null, "ObjectDisposed_ReaderClosed");
}

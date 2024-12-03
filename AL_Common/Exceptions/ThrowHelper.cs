using System;
using System.Diagnostics;
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
    internal static void ArgumentNullException(string argument) => throw new ArgumentNullException(argument);

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
    public static void ObjectDisposed(string message) => throw new ObjectDisposedException(null, message);
    [DoesNotReturn]
    public static void IndexOutOfRange() => throw new IndexOutOfRangeException();
    [DoesNotReturn]
    public static void EncryptionNotSupported() => throw new NotSupportedException("Encrypted archives are not supported.");

    [DoesNotReturn]
    internal static void ThrowArgumentNullException(string argument)
    {
        throw new ArgumentNullException(argument);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException_NeedNonNegNum(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName, SR.ArgumentOutOfRange_NeedNonNegNum);
    }

    [DoesNotReturn]
    internal static void ThrowObjectDisposedException_FileClosed()
    {
        throw new ObjectDisposedException(null, SR.ObjectDisposed_FileClosed);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException(string argument, string resource)
    {
        throw new ArgumentOutOfRangeException(argument, resource);
    }

    private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(string argument, string resource)
    {
        return new ArgumentOutOfRangeException(argument, resource);
    }

    [DoesNotReturn]
    internal static void ThrowNotSupportedException_UnseekableStream()
    {
        throw new NotSupportedException(SR.NotSupported_UnseekableStream);
    }

    [DoesNotReturn]
    internal static void ThrowNotSupportedException_UnreadableStream()
    {
        throw new NotSupportedException(SR.NotSupported_UnreadableStream);
    }

    [DoesNotReturn]
    internal static void ThrowNotSupportedException_UnwritableStream()
    {
        throw new NotSupportedException(SR.NotSupported_UnwritableStream);
    }

    [DoesNotReturn]
    internal static void ThrowNegativeOrZero(int value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeNonNegativeNonZero, paramName, value));

    [DoesNotReturn]
    internal static void ThrowObjectDisposedException_StreamClosed(string? objectName)
    {
        throw new ObjectDisposedException(objectName, SR.ObjectDisposed_StreamClosed);
    }

    [DoesNotReturn]
    internal static void ThrowInvalidOperationException(string message)
    {
        throw new InvalidOperationException(message);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException(string argument)
    {
        throw new ArgumentOutOfRangeException(argument, SR.ArgumentOutOfRange_IndexMustBeLessOrEqual);
    }
}

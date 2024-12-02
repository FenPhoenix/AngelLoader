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
    internal static void ThrowArgumentNullException(ExceptionArgument_NET argument)
    {
        throw new ArgumentNullException(GetArgumentName(argument));
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
    internal static void ThrowArgumentException_HandleNotSync(string paramName)
    {
        throw new ArgumentException(SR.Arg_HandleNotSync, paramName);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentException_HandleNotAsync(string paramName)
    {
        throw new ArgumentException(SR.Arg_HandleNotAsync, paramName);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException(ExceptionArgument_NET argument)
    {
        throw new ArgumentOutOfRangeException(GetArgumentName(argument));
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException(ExceptionArgument_NET argument, ExceptionResource_NET resource)
    {
        throw GetArgumentOutOfRangeException(argument, resource);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException(ExceptionArgument_NET argument, int paramNumber, ExceptionResource_NET resource)
    {
        throw GetArgumentOutOfRangeException(argument, paramNumber, resource);
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
    internal static void ThrowInvalidOperationException(ExceptionResource_NET resource)
    {
        throw GetInvalidOperationException(resource);
    }

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
    {
        throw GetArgumentOutOfRangeException(ExceptionArgument_NET.index,
            ExceptionResource_NET.ArgumentOutOfRange_IndexMustBeLessOrEqual);
    }

    private static string GetArgumentName(ExceptionArgument_NET argument)
    {
        switch (argument)
        {
            case ExceptionArgument_NET.obj:
                return "obj";
            case ExceptionArgument_NET.dictionary:
                return "dictionary";
            case ExceptionArgument_NET.array:
                return "array";
            case ExceptionArgument_NET.info:
                return "info";
            case ExceptionArgument_NET.key:
                return "key";
            case ExceptionArgument_NET.text:
                return "text";
            case ExceptionArgument_NET.values:
                return "values";
            case ExceptionArgument_NET.value:
                return "value";
            case ExceptionArgument_NET.startIndex:
                return "startIndex";
            case ExceptionArgument_NET.task:
                return "task";
            case ExceptionArgument_NET.bytes:
                return "bytes";
            case ExceptionArgument_NET.byteIndex:
                return "byteIndex";
            case ExceptionArgument_NET.byteCount:
                return "byteCount";
            case ExceptionArgument_NET.ch:
                return "ch";
            case ExceptionArgument_NET.chars:
                return "chars";
            case ExceptionArgument_NET.charIndex:
                return "charIndex";
            case ExceptionArgument_NET.charCount:
                return "charCount";
            case ExceptionArgument_NET.s:
                return "s";
            case ExceptionArgument_NET.input:
                return "input";
            case ExceptionArgument_NET.ownedMemory:
                return "ownedMemory";
            case ExceptionArgument_NET.list:
                return "list";
            case ExceptionArgument_NET.index:
                return "index";
            case ExceptionArgument_NET.capacity:
                return "capacity";
            case ExceptionArgument_NET.collection:
                return "collection";
            case ExceptionArgument_NET.item:
                return "item";
            case ExceptionArgument_NET.converter:
                return "converter";
            case ExceptionArgument_NET.match:
                return "match";
            case ExceptionArgument_NET.count:
                return "count";
            case ExceptionArgument_NET.action:
                return "action";
            case ExceptionArgument_NET.comparison:
                return "comparison";
            case ExceptionArgument_NET.exceptions:
                return "exceptions";
            case ExceptionArgument_NET.exception:
                return "exception";
            case ExceptionArgument_NET.pointer:
                return "pointer";
            case ExceptionArgument_NET.start:
                return "start";
            case ExceptionArgument_NET.format:
                return "format";
            case ExceptionArgument_NET.formats:
                return "formats";
            case ExceptionArgument_NET.culture:
                return "culture";
            case ExceptionArgument_NET.comparer:
                return "comparer";
            case ExceptionArgument_NET.comparable:
                return "comparable";
            case ExceptionArgument_NET.source:
                return "source";
            case ExceptionArgument_NET.length:
                return "length";
            case ExceptionArgument_NET.comparisonType:
                return "comparisonType";
            case ExceptionArgument_NET.manager:
                return "manager";
            case ExceptionArgument_NET.sourceBytesToCopy:
                return "sourceBytesToCopy";
            case ExceptionArgument_NET.callBack:
                return "callBack";
            case ExceptionArgument_NET.creationOptions:
                return "creationOptions";
            case ExceptionArgument_NET.function:
                return "function";
            case ExceptionArgument_NET.scheduler:
                return "scheduler";
            case ExceptionArgument_NET.continuation:
                return "continuation";
            case ExceptionArgument_NET.continuationAction:
                return "continuationAction";
            case ExceptionArgument_NET.continuationFunction:
                return "continuationFunction";
            case ExceptionArgument_NET.tasks:
                return "tasks";
            case ExceptionArgument_NET.asyncResult:
                return "asyncResult";
            case ExceptionArgument_NET.beginMethod:
                return "beginMethod";
            case ExceptionArgument_NET.endMethod:
                return "endMethod";
            case ExceptionArgument_NET.endFunction:
                return "endFunction";
            case ExceptionArgument_NET.cancellationToken:
                return "cancellationToken";
            case ExceptionArgument_NET.continuationOptions:
                return "continuationOptions";
            case ExceptionArgument_NET.delay:
                return "delay";
            case ExceptionArgument_NET.millisecondsDelay:
                return "millisecondsDelay";
            case ExceptionArgument_NET.millisecondsTimeout:
                return "millisecondsTimeout";
            case ExceptionArgument_NET.stateMachine:
                return "stateMachine";
            case ExceptionArgument_NET.timeout:
                return "timeout";
            case ExceptionArgument_NET.type:
                return "type";
            case ExceptionArgument_NET.sourceIndex:
                return "sourceIndex";
            case ExceptionArgument_NET.sourceArray:
                return "sourceArray";
            case ExceptionArgument_NET.destinationIndex:
                return "destinationIndex";
            case ExceptionArgument_NET.destinationArray:
                return "destinationArray";
            case ExceptionArgument_NET.pHandle:
                return "pHandle";
            case ExceptionArgument_NET.handle:
                return "handle";
            case ExceptionArgument_NET.other:
                return "other";
            case ExceptionArgument_NET.newSize:
                return "newSize";
            case ExceptionArgument_NET.lengths:
                return "lengths";
            case ExceptionArgument_NET.len:
                return "len";
            case ExceptionArgument_NET.keys:
                return "keys";
            case ExceptionArgument_NET.indices:
                return "indices";
            case ExceptionArgument_NET.index1:
                return "index1";
            case ExceptionArgument_NET.index2:
                return "index2";
            case ExceptionArgument_NET.index3:
                return "index3";
            case ExceptionArgument_NET.endIndex:
                return "endIndex";
            case ExceptionArgument_NET.elementType:
                return "elementType";
            case ExceptionArgument_NET.arrayIndex:
                return "arrayIndex";
            case ExceptionArgument_NET.year:
                return "year";
            case ExceptionArgument_NET.codePoint:
                return "codePoint";
            case ExceptionArgument_NET.str:
                return "str";
            case ExceptionArgument_NET.options:
                return "options";
            case ExceptionArgument_NET.prefix:
                return "prefix";
            case ExceptionArgument_NET.suffix:
                return "suffix";
            case ExceptionArgument_NET.buffer:
                return "buffer";
            case ExceptionArgument_NET.buffers:
                return "buffers";
            case ExceptionArgument_NET.offset:
                return "offset";
            case ExceptionArgument_NET.stream:
                return "stream";
            case ExceptionArgument_NET.anyOf:
                return "anyOf";
            case ExceptionArgument_NET.overlapped:
                return "overlapped";
            case ExceptionArgument_NET.minimumBytes:
                return "minimumBytes";
            case ExceptionArgument_NET.arrayType:
                return "arrayType";
            case ExceptionArgument_NET.divisor:
                return "divisor";
            case ExceptionArgument_NET.factor:
                return "factor";
            case ExceptionArgument_NET.set:
                return "set";
            default:
                Debug.Fail("The enum value is not defined, please check the ExceptionArgument_NET Enum.");
                return "";
        }
    }
}

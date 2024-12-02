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

    private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument_NET argument, int paramNumber, ExceptionResource_NET resource)
    {
        return new ArgumentOutOfRangeException(GetArgumentName(argument) + "[" + paramNumber.ToString() + "]", GetResourceString(resource));
    }

    private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument_NET argument, ExceptionResource_NET resource)
    {
        return new ArgumentOutOfRangeException(GetArgumentName(argument), GetResourceString(resource));
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

    private static InvalidOperationException GetInvalidOperationException(ExceptionResource_NET resource)
    {
        return new InvalidOperationException(GetResourceString(resource));
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

    private static string GetResourceString(ExceptionResource_NET resource)
    {
        switch (resource)
        {
            case ExceptionResource_NET.ArgumentOutOfRange_IndexMustBeLessOrEqual:
                return SR.ArgumentOutOfRange_IndexMustBeLessOrEqual;
            case ExceptionResource_NET.ArgumentOutOfRange_IndexMustBeLess:
                return SR.ArgumentOutOfRange_IndexMustBeLess;
            case ExceptionResource_NET.ArgumentOutOfRange_IndexCount:
                return SR.ArgumentOutOfRange_IndexCount;
            case ExceptionResource_NET.ArgumentOutOfRange_IndexCountBuffer:
                return SR.ArgumentOutOfRange_IndexCountBuffer;
            case ExceptionResource_NET.ArgumentOutOfRange_Count:
                return SR.ArgumentOutOfRange_Count;
            case ExceptionResource_NET.ArgumentOutOfRange_Year:
                return SR.ArgumentOutOfRange_Year;
            case ExceptionResource_NET.Arg_ArrayPlusOffTooSmall:
                return SR.Arg_ArrayPlusOffTooSmall;
            case ExceptionResource_NET.Arg_ByteArrayTooSmallForValue:
                return SR.Arg_ByteArrayTooSmallForValue;
            case ExceptionResource_NET.NotSupported_ReadOnlyCollection:
                return SR.NotSupported_ReadOnlyCollection;
            case ExceptionResource_NET.Arg_RankMultiDimNotSupported:
                return SR.Arg_RankMultiDimNotSupported;
            case ExceptionResource_NET.Arg_NonZeroLowerBound:
                return SR.Arg_NonZeroLowerBound;
            case ExceptionResource_NET.ArgumentOutOfRange_GetCharCountOverflow:
                return SR.ArgumentOutOfRange_GetCharCountOverflow;
            case ExceptionResource_NET.ArgumentOutOfRange_ListInsert:
                return SR.ArgumentOutOfRange_ListInsert;
            case ExceptionResource_NET.ArgumentOutOfRange_NeedNonNegNum:
                return SR.ArgumentOutOfRange_NeedNonNegNum;
            case ExceptionResource_NET.ArgumentOutOfRange_SmallCapacity:
                return SR.ArgumentOutOfRange_SmallCapacity;
            case ExceptionResource_NET.Argument_InvalidOffLen:
                return SR.Argument_InvalidOffLen;
            case ExceptionResource_NET.Argument_CannotExtractScalar:
                return SR.Argument_CannotExtractScalar;
            case ExceptionResource_NET.ArgumentOutOfRange_BiggerThanCollection:
                return SR.ArgumentOutOfRange_BiggerThanCollection;
            case ExceptionResource_NET.Serialization_MissingKeys:
                return SR.Serialization_MissingKeys;
            case ExceptionResource_NET.Serialization_NullKey:
                return SR.Serialization_NullKey;
            case ExceptionResource_NET.NotSupported_KeyCollectionSet:
                return SR.NotSupported_KeyCollectionSet;
            case ExceptionResource_NET.NotSupported_ValueCollectionSet:
                return SR.NotSupported_ValueCollectionSet;
            case ExceptionResource_NET.InvalidOperation_NullArray:
                return SR.InvalidOperation_NullArray;
            case ExceptionResource_NET.TaskT_TransitionToFinal_AlreadyCompleted:
                return SR.TaskT_TransitionToFinal_AlreadyCompleted;
            case ExceptionResource_NET.TaskCompletionSourceT_TrySetException_NullException:
                return SR.TaskCompletionSourceT_TrySetException_NullException;
            case ExceptionResource_NET.TaskCompletionSourceT_TrySetException_NoExceptions:
                return SR.TaskCompletionSourceT_TrySetException_NoExceptions;
            case ExceptionResource_NET.NotSupported_StringComparison:
                return SR.NotSupported_StringComparison;
            case ExceptionResource_NET.ConcurrentCollection_SyncRoot_NotSupported:
                return SR.ConcurrentCollection_SyncRoot_NotSupported;
            case ExceptionResource_NET.Task_MultiTaskContinuation_NullTask:
                return SR.Task_MultiTaskContinuation_NullTask;
            case ExceptionResource_NET.InvalidOperation_WrongAsyncResultOrEndCalledMultiple:
                return SR.InvalidOperation_WrongAsyncResultOrEndCalledMultiple;
            case ExceptionResource_NET.Task_MultiTaskContinuation_EmptyTaskList:
                return SR.Task_MultiTaskContinuation_EmptyTaskList;
            case ExceptionResource_NET.Task_Start_TaskCompleted:
                return SR.Task_Start_TaskCompleted;
            case ExceptionResource_NET.Task_Start_Promise:
                return SR.Task_Start_Promise;
            case ExceptionResource_NET.Task_Start_ContinuationTask:
                return SR.Task_Start_ContinuationTask;
            case ExceptionResource_NET.Task_Start_AlreadyStarted:
                return SR.Task_Start_AlreadyStarted;
            case ExceptionResource_NET.Task_RunSynchronously_Continuation:
                return SR.Task_RunSynchronously_Continuation;
            case ExceptionResource_NET.Task_RunSynchronously_Promise:
                return SR.Task_RunSynchronously_Promise;
            case ExceptionResource_NET.Task_RunSynchronously_TaskCompleted:
                return SR.Task_RunSynchronously_TaskCompleted;
            case ExceptionResource_NET.Task_RunSynchronously_AlreadyStarted:
                return SR.Task_RunSynchronously_AlreadyStarted;
            case ExceptionResource_NET.AsyncMethodBuilder_InstanceNotInitialized:
                return SR.AsyncMethodBuilder_InstanceNotInitialized;
            case ExceptionResource_NET.Task_ContinueWith_ESandLR:
                return SR.Task_ContinueWith_ESandLR;
            case ExceptionResource_NET.Task_ContinueWith_NotOnAnything:
                return SR.Task_ContinueWith_NotOnAnything;
            case ExceptionResource_NET.Task_InvalidTimerTimeSpan:
                return SR.Task_InvalidTimerTimeSpan;
            case ExceptionResource_NET.Task_Delay_InvalidMillisecondsDelay:
                return SR.Task_Delay_InvalidMillisecondsDelay;
            case ExceptionResource_NET.Task_Dispose_NotCompleted:
                return SR.Task_Dispose_NotCompleted;
            case ExceptionResource_NET.Task_ThrowIfDisposed:
                return SR.Task_ThrowIfDisposed;
            case ExceptionResource_NET.Task_WaitMulti_NullTask:
                return SR.Task_WaitMulti_NullTask;
            case ExceptionResource_NET.ArgumentException_OtherNotArrayOfCorrectLength:
                return SR.ArgumentException_OtherNotArrayOfCorrectLength;
            case ExceptionResource_NET.ArgumentNull_Array:
                return SR.ArgumentNull_Array;
            case ExceptionResource_NET.ArgumentNull_SafeHandle:
                return SR.ArgumentNull_SafeHandle;
            case ExceptionResource_NET.ArgumentOutOfRange_EndIndexStartIndex:
                return SR.ArgumentOutOfRange_EndIndexStartIndex;
            case ExceptionResource_NET.ArgumentOutOfRange_Enum:
                return SR.ArgumentOutOfRange_Enum;
            case ExceptionResource_NET.ArgumentOutOfRange_HugeArrayNotSupported:
                return SR.ArgumentOutOfRange_HugeArrayNotSupported;
            case ExceptionResource_NET.Argument_AddingDuplicate:
                return SR.Argument_AddingDuplicate;
            case ExceptionResource_NET.Argument_InvalidArgumentForComparison:
                return SR.Argument_InvalidArgumentForComparison;
            case ExceptionResource_NET.Arg_LowerBoundsMustMatch:
                return SR.Arg_LowerBoundsMustMatch;
            case ExceptionResource_NET.Arg_MustBeType:
                return SR.Arg_MustBeType;
            case ExceptionResource_NET.Arg_Need1DArray:
                return SR.Arg_Need1DArray;
            case ExceptionResource_NET.Arg_Need2DArray:
                return SR.Arg_Need2DArray;
            case ExceptionResource_NET.Arg_Need3DArray:
                return SR.Arg_Need3DArray;
            case ExceptionResource_NET.Arg_NeedAtLeast1Rank:
                return SR.Arg_NeedAtLeast1Rank;
            case ExceptionResource_NET.Arg_RankIndices:
                return SR.Arg_RankIndices;
            case ExceptionResource_NET.Arg_RanksAndBounds:
                return SR.Arg_RanksAndBounds;
            case ExceptionResource_NET.InvalidOperation_IComparerFailed:
                return SR.InvalidOperation_IComparerFailed;
            case ExceptionResource_NET.NotSupported_FixedSizeCollection:
                return SR.NotSupported_FixedSizeCollection;
            case ExceptionResource_NET.Rank_MultiDimNotSupported:
                return SR.Rank_MultiDimNotSupported;
            case ExceptionResource_NET.Arg_TypeNotSupported:
                return SR.Arg_TypeNotSupported;
            case ExceptionResource_NET.Argument_SpansMustHaveSameLength:
                return SR.Argument_SpansMustHaveSameLength;
            case ExceptionResource_NET.Argument_InvalidFlag:
                return SR.Argument_InvalidFlag;
            case ExceptionResource_NET.CancellationTokenSource_Disposed:
                return SR.CancellationTokenSource_Disposed;
            case ExceptionResource_NET.Argument_AlignmentMustBePow2:
                return SR.Argument_AlignmentMustBePow2;
            case ExceptionResource_NET.ArgumentOutOfRange_NotGreaterThanBufferLength:
                return SR.ArgumentOutOfRange_NotGreaterThanBufferLength;
            case ExceptionResource_NET.InvalidOperation_SpanOverlappedOperation:
                return SR.InvalidOperation_SpanOverlappedOperation;
            case ExceptionResource_NET.InvalidOperation_TimeProviderNullLocalTimeZone:
                return SR.InvalidOperation_TimeProviderNullLocalTimeZone;
            case ExceptionResource_NET.InvalidOperation_TimeProviderInvalidTimestampFrequency:
                return SR.InvalidOperation_TimeProviderInvalidTimestampFrequency;
            case ExceptionResource_NET.Format_UnexpectedClosingBrace:
                return SR.Format_UnexpectedClosingBrace;
            case ExceptionResource_NET.Format_UnclosedFormatItem:
                return SR.Format_UnclosedFormatItem;
            case ExceptionResource_NET.Format_ExpectedAsciiDigit:
                return SR.Format_ExpectedAsciiDigit;
            case ExceptionResource_NET.Argument_HasToBeArrayClass:
                return SR.Argument_HasToBeArrayClass;
            case ExceptionResource_NET.InvalidOperation_IncompatibleComparer:
                return SR.InvalidOperation_IncompatibleComparer;
            default:
                Debug.Fail("The enum value is not defined, please check the ExceptionResource Enum.");
                return "";
        }
    }
}

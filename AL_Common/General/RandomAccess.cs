// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace AL_Common;

public static class RandomAccess
{
    private static void ValidateInput(AL_SafeFileHandle handle, long fileOffset, bool allowUnseekableHandles = false)
    {
        if (handle is null)
        {
            ThrowHelper.ThrowArgumentNullException(ExceptionArgument.handle);
        }
        else if (handle.IsInvalid)
        {
            ThrowHelper.ThrowArgumentException_InvalidHandle(nameof(handle));
        }
        else if (!handle.CanSeek)
        {
            // CanSeek calls IsClosed, we don't want to call it twice for valid handles
            if (handle.IsClosed)
            {
                ThrowHelper.ThrowObjectDisposedException_FileClosed();
            }

            if (!allowUnseekableHandles)
            {
                ThrowHelper.ThrowNotSupportedException_UnseekableStream();
            }
        }
        else if (fileOffset < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(nameof(fileOffset));
        }
    }

    /// <summary>
    /// Reads a sequence of bytes from given file at given offset.
    /// </summary>
    /// <param name="handle">The file handle.</param>
    /// <param name="buffer">A region of memory. When this method returns, the contents of this region are replaced by the bytes read from the file.</param>
    /// <param name="fileOffset">The file position to read from.</param>
    /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes allocated in the buffer if that many bytes are not currently available, or zero (0) if the end of the file has been reached.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="handle" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException"><paramref name="handle" /> is invalid.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The file is closed.</exception>
    /// <exception cref="T:System.NotSupportedException">The file does not support seeking (pipe or socket).</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="fileOffset" /> is negative.</exception>
    /// <exception cref="T:System.UnauthorizedAccessException"><paramref name="handle" /> was not opened for reading.</exception>
    /// <exception cref="T:System.IO.IOException">An I/O error occurred.</exception>
    /// <remarks>Position of the file is not advanced.</remarks>
    public static int Read(AL_SafeFileHandle handle, Span<byte> buffer, long fileOffset)
    {
        ValidateInput(handle, fileOffset);

        return ReadAtOffset(handle, buffer, fileOffset);
    }

    internal static unsafe int ReadAtOffset(AL_SafeFileHandle handle, Span<byte> buffer, long fileOffset)
    {
        //if (handle.IsAsync)
        //{
        //    return ReadSyncUsingAsyncHandle(handle, buffer, fileOffset);
        //}

        NativeOverlapped overlapped = GetNativeOverlappedForSyncHandle(handle, fileOffset);
        fixed (byte* pinned = &MemoryMarshal.GetReference(buffer))
        {
            if (Interop.Kernel32.ReadFile(handle, pinned, buffer.Length, out int numBytesRead, &overlapped) != 0)
            {
                return numBytesRead;
            }

            int errorCode = GetLastWin32ErrorAndDisposeHandleIfInvalid(handle);
            return errorCode switch
            {
                // https://docs.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-readfile#synchronization-and-file-position:
                // "If lpOverlapped is not NULL, then when a synchronous read operation reaches the end of a file,
                // ReadFile returns FALSE and GetLastError returns ERROR_HANDLE_EOF"
                Interop.Errors.ERROR_HANDLE_EOF => numBytesRead,
                _ when IsEndOfFile(errorCode, handle, fileOffset) => 0,
                _ => throw Win32Marshal.GetExceptionForWin32Error(errorCode, handle.Path),
            };
        }
    }

    private static NativeOverlapped GetNativeOverlappedForSyncHandle(AL_SafeFileHandle handle, long fileOffset)
    {
        //Debug.Assert(!handle.IsAsync);

        NativeOverlapped result = default;
        if (handle.CanSeek)
        {
            result.OffsetLow = unchecked((int)fileOffset);
            result.OffsetHigh = (int)(fileOffset >> 32);
        }
        return result;
    }

    internal static int GetLastWin32ErrorAndDisposeHandleIfInvalid(AL_SafeFileHandle handle)
    {
        int errorCode = Marshal.GetLastWin32Error();

        // If ERROR_INVALID_HANDLE is returned, it doesn't suffice to set
        // the handle as invalid; the handle must also be closed.
        //
        // Marking the handle as invalid but not closing the handle
        // resulted in exceptions during finalization and locked column
        // values (due to invalid but unclosed handle) in SQL Win32FileStream
        // scenarios.
        //
        // A more mainstream scenario involves accessing a file on a
        // network share. ERROR_INVALID_HANDLE may occur because the network
        // connection was dropped and the server closed the handle. However,
        // the client side handle is still open and even valid for certain
        // operations.
        //
        // Note that _parent.Dispose doesn't throw so we don't need to special case.
        // SetHandleAsInvalid only sets _closed field to true (without
        // actually closing handle) so we don't need to call that as well.
        if (errorCode == Interop.Errors.ERROR_INVALID_HANDLE)
        {
            handle.Dispose();
        }

        return errorCode;
    }

    internal static bool IsEndOfFile(int errorCode, AL_SafeFileHandle handle, long fileOffset)
    {
        switch (errorCode)
        {
            case Interop.Errors.ERROR_HANDLE_EOF: // logically success with 0 bytes read (read at end of file)
            case Interop.Errors.ERROR_BROKEN_PIPE: // For pipes, ERROR_BROKEN_PIPE is the normal end of the pipe.
            case Interop.Errors.ERROR_PIPE_NOT_CONNECTED: // Named pipe server has disconnected, return 0 to match NamedPipeClientStream behaviour
            case Interop.Errors.ERROR_INVALID_PARAMETER when IsEndOfFileForNoBuffering(handle, fileOffset):
                return true;
            default:
                return false;
        }
    }

    // From https://docs.microsoft.com/en-us/windows/win32/fileio/file-buffering:
    // "File access sizes, including the optional file offset in the OVERLAPPED structure,
    // if specified, must be for a number of bytes that is an integer multiple of the volume sector size."
    // So if buffer and physical sector size is 4096 and the file size is 4097:
    // the read from offset=0 reads 4096 bytes
    // the read from offset=4096 reads 1 byte
    // the read from offset=4097 fails with ERROR_INVALID_PARAMETER (the offset is not a multiple of sector size)
    // Based on feedback received from customers (https://github.com/dotnet/runtime/issues/62851),
    // it was decided to not throw, but just return 0.
    private static bool IsEndOfFileForNoBuffering(AL_SafeFileHandle fileHandle, long fileOffset)
        => fileHandle.IsNoBuffering && fileHandle.CanSeek && fileOffset >= fileHandle.GetFileLength();

    internal static partial class Interop
    {
        internal static partial class Kernel32
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern unsafe int ReadFile(
                SafeHandle handle,
                byte* bytes,
                int numBytesToRead,
                IntPtr numBytesRead_mustBeZero,
                NativeOverlapped* overlapped);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern unsafe int ReadFile(
                SafeHandle handle,
                byte* bytes,
                int numBytesToRead,
                out int numBytesRead,
                NativeOverlapped* overlapped);
        }

        // As defined in winerror.h and https://learn.microsoft.com/windows/win32/debug/system-error-codes
        internal static partial class Errors
        {
            internal const int ERROR_SUCCESS = 0x0;
            internal const int ERROR_INVALID_FUNCTION = 0x1;
            internal const int ERROR_FILE_NOT_FOUND = 0x2;
            internal const int ERROR_PATH_NOT_FOUND = 0x3;
            internal const int ERROR_ACCESS_DENIED = 0x5;
            internal const int ERROR_INVALID_HANDLE = 0x6;
            internal const int ERROR_NOT_ENOUGH_MEMORY = 0x8;
            internal const int ERROR_INVALID_ACCESS = 0xC;
            internal const int ERROR_INVALID_DATA = 0xD;
            internal const int ERROR_OUTOFMEMORY = 0xE;
            internal const int ERROR_INVALID_DRIVE = 0xF;
            internal const int ERROR_NO_MORE_FILES = 0x12;
            internal const int ERROR_NOT_READY = 0x15;
            internal const int ERROR_BAD_COMMAND = 0x16;
            internal const int ERROR_BAD_LENGTH = 0x18;
            internal const int ERROR_SHARING_VIOLATION = 0x20;
            internal const int ERROR_LOCK_VIOLATION = 0x21;
            internal const int ERROR_HANDLE_EOF = 0x26;
            internal const int ERROR_NOT_SUPPORTED = 0x32;
            internal const int ERROR_BAD_NETPATH = 0x35;
            internal const int ERROR_NETWORK_ACCESS_DENIED = 0x41;
            internal const int ERROR_BAD_NET_NAME = 0x43;
            internal const int ERROR_FILE_EXISTS = 0x50;
            internal const int ERROR_INVALID_PARAMETER = 0x57;
            internal const int ERROR_BROKEN_PIPE = 0x6D;
            internal const int ERROR_DISK_FULL = 0x70;
            internal const int ERROR_SEM_TIMEOUT = 0x79;
            internal const int ERROR_CALL_NOT_IMPLEMENTED = 0x78;
            internal const int ERROR_INSUFFICIENT_BUFFER = 0x7A;
            internal const int ERROR_INVALID_NAME = 0x7B;
            internal const int ERROR_INVALID_LEVEL = 0x7C;
            internal const int ERROR_MOD_NOT_FOUND = 0x7E;
            internal const int ERROR_NEGATIVE_SEEK = 0x83;
            internal const int ERROR_DIR_NOT_EMPTY = 0x91;
            internal const int ERROR_BAD_PATHNAME = 0xA1;
            internal const int ERROR_LOCK_FAILED = 0xA7;
            internal const int ERROR_BUSY = 0xAA;
            internal const int ERROR_ALREADY_EXISTS = 0xB7;
            internal const int ERROR_BAD_EXE_FORMAT = 0xC1;
            internal const int ERROR_ENVVAR_NOT_FOUND = 0xCB;
            internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE;
            internal const int ERROR_EXE_MACHINE_TYPE_MISMATCH = 0xD8;
            internal const int ERROR_FILE_TOO_LARGE = 0xDF;
            internal const int ERROR_PIPE_BUSY = 0xE7;
            internal const int ERROR_NO_DATA = 0xE8;
            internal const int ERROR_PIPE_NOT_CONNECTED = 0xE9;
            internal const int ERROR_MORE_DATA = 0xEA;
            internal const int ERROR_NO_MORE_ITEMS = 0x103;
            internal const int ERROR_DIRECTORY = 0x10B;
            internal const int ERROR_NOT_OWNER = 0x120;
            internal const int ERROR_TOO_MANY_POSTS = 0x12A;
            internal const int ERROR_PARTIAL_COPY = 0x12B;
            internal const int ERROR_ARITHMETIC_OVERFLOW = 0x216;
            internal const int ERROR_PIPE_CONNECTED = 0x217;
            internal const int ERROR_PIPE_LISTENING = 0x218;
            internal const int ERROR_MUTANT_LIMIT_EXCEEDED = 0x24B;
            internal const int ERROR_OPERATION_ABORTED = 0x3E3;
            internal const int ERROR_IO_INCOMPLETE = 0x3E4;
            internal const int ERROR_IO_PENDING = 0x3E5;
            internal const int ERROR_INVALID_FLAGS = 0x3EC;
            internal const int ERROR_NO_TOKEN = 0x3f0;
            internal const int ERROR_SERVICE_DOES_NOT_EXIST = 0x424;
            internal const int ERROR_EXCEPTION_IN_SERVICE = 0x428;
            internal const int ERROR_PROCESS_ABORTED = 0x42B;
            internal const int ERROR_FILEMARK_DETECTED = 0x44D;
            internal const int ERROR_NO_UNICODE_TRANSLATION = 0x459;
            internal const int ERROR_DLL_INIT_FAILED = 0x45A;
            internal const int ERROR_COUNTER_TIMEOUT = 0x461;
            internal const int ERROR_NO_ASSOCIATION = 0x483;
            internal const int ERROR_DDE_FAIL = 0x484;
            internal const int ERROR_DLL_NOT_FOUND = 0x485;
            internal const int ERROR_NOT_FOUND = 0x490;
            internal const int ERROR_INVALID_DOMAINNAME = 0x4BC;
            internal const int ERROR_CANCELLED = 0x4C7;
            internal const int ERROR_NETWORK_UNREACHABLE = 0x4CF;
            internal const int ERROR_NON_ACCOUNT_SID = 0x4E9;
            internal const int ERROR_NOT_ALL_ASSIGNED = 0x514;
            internal const int ERROR_UNKNOWN_REVISION = 0x519;
            internal const int ERROR_INVALID_OWNER = 0x51B;
            internal const int ERROR_INVALID_PRIMARY_GROUP = 0x51C;
            internal const int ERROR_NO_LOGON_SERVERS = 0x51F;
            internal const int ERROR_NO_SUCH_LOGON_SESSION = 0x520;
            internal const int ERROR_NO_SUCH_PRIVILEGE = 0x521;
            internal const int ERROR_PRIVILEGE_NOT_HELD = 0x522;
            internal const int ERROR_INVALID_ACL = 0x538;
            internal const int ERROR_INVALID_SECURITY_DESCR = 0x53A;
            internal const int ERROR_INVALID_SID = 0x539;
            internal const int ERROR_BAD_IMPERSONATION_LEVEL = 0x542;
            internal const int ERROR_CANT_OPEN_ANONYMOUS = 0x543;
            internal const int ERROR_NO_SECURITY_ON_OBJECT = 0x546;
            internal const int ERROR_NO_SUCH_DOMAIN = 0x54B;
            internal const int ERROR_CANNOT_IMPERSONATE = 0x558;
            internal const int ERROR_CLASS_ALREADY_EXISTS = 0x582;
            internal const int ERROR_NO_SYSTEM_RESOURCES = 0x5AA;
            internal const int ERROR_TIMEOUT = 0x5B4;
            internal const int ERROR_EVENTLOG_FILE_CHANGED = 0x5DF;
            internal const int RPC_S_OUT_OF_RESOURCES = 0x6B9;
            internal const int RPC_S_SERVER_UNAVAILABLE = 0x6BA;
            internal const int RPC_S_CALL_FAILED = 0x6BE;
            internal const int ERROR_TRUSTED_RELATIONSHIP_FAILURE = 0x6FD;
            internal const int ERROR_RESOURCE_TYPE_NOT_FOUND = 0x715;
            internal const int ERROR_RESOURCE_LANG_NOT_FOUND = 0x717;
            internal const int RPC_S_CALL_CANCELED = 0x71A;
            internal const int ERROR_NO_SITENAME = 0x77F;
            internal const int ERROR_NOT_A_REPARSE_POINT = 0x1126;
            internal const int ERROR_DS_NAME_UNPARSEABLE = 0x209E;
            internal const int ERROR_DS_UNKNOWN_ERROR = 0x20EF;
            internal const int ERROR_DS_DRA_BAD_DN = 0x20F7;
            internal const int ERROR_DS_DRA_OUT_OF_MEM = 0x20FE;
            internal const int ERROR_DS_DRA_ACCESS_DENIED = 0x2105;
            internal const int DNS_ERROR_RCODE_NAME_ERROR = 0x232B;
            internal const int ERROR_EVT_QUERY_RESULT_STALE = 0x3AA3;
            internal const int ERROR_EVT_QUERY_RESULT_INVALID_POSITION = 0x3AA4;
            internal const int ERROR_EVT_INVALID_EVENT_DATA = 0x3A9D;
            internal const int ERROR_EVT_PUBLISHER_METADATA_NOT_FOUND = 0x3A9A;
            internal const int ERROR_EVT_CHANNEL_NOT_FOUND = 0x3A9F;
            internal const int ERROR_EVT_MESSAGE_NOT_FOUND = 0x3AB3;
            internal const int ERROR_EVT_MESSAGE_ID_NOT_FOUND = 0x3AB4;
            internal const int ERROR_EVT_PUBLISHER_DISABLED = 0x3ABD;
        }
    }
}

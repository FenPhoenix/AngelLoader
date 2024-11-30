// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace AL_Common;

public static class RandomAccess
{
    private static void ValidateInput(AL_SafeFileHandle handle, long fileOffset, bool allowUnseekableHandles = false)
    {
        if (handle is null)
        {
            ThrowHelper.ArgumentNullException("handle");
        }
        else if (handle.IsInvalid)
        {
            ThrowHelper.ArgumentException(SR.Arg_InvalidHandle, nameof(handle));
        }
        else if (!handle.CanSeek)
        {
            // CanSeek calls IsClosed, we don't want to call it twice for valid handles
            if (handle.IsClosed)
            {
                ThrowHelper.ObjectDisposed(SR.ObjectDisposed_FileClosed);
            }

            if (!allowUnseekableHandles)
            {
                ThrowHelper.NotSupported(SR.NotSupported_UnseekableStream);
            }
        }
        else if (fileOffset < 0)
        {
            ThrowHelper.ArgumentOutOfRange(nameof(fileOffset), SR.ArgumentOutOfRange_NeedNonNegNum);
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

    public static unsafe int ReadAtOffset(AL_SafeFileHandle handle, Span<byte> buffer, long fileOffset)
    {
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

    /// <summary>
    /// Gets the length of the file in bytes.
    /// </summary>
    /// <param name="handle">The file handle.</param>
    /// <returns>A long value representing the length of the file in bytes.</returns>
    /// <exception cref="T:System.ArgumentNullException"><paramref name="handle" /> is <see langword="null" />.</exception>
    /// <exception cref="T:System.ArgumentException"><paramref name="handle" /> is invalid.</exception>
    /// <exception cref="T:System.ObjectDisposedException">The file is closed.</exception>
    /// <exception cref="T:System.NotSupportedException">The file does not support seeking (pipe or socket).</exception>
    public static long GetLength(AL_SafeFileHandle handle)
    {
        ValidateInput(handle, fileOffset: 0);

        return handle.GetFileLength();
    }

    private static NativeOverlapped GetNativeOverlappedForSyncHandle(AL_SafeFileHandle handle, long fileOffset)
    {
        NativeOverlapped result = default;
        if (handle.CanSeek)
        {
            result.OffsetLow = unchecked((int)fileOffset);
            result.OffsetHigh = (int)(fileOffset >> 32);
        }
        return result;
    }

    private static int GetLastWin32ErrorAndDisposeHandleIfInvalid(AL_SafeFileHandle handle)
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

    private static bool IsEndOfFile(int errorCode, AL_SafeFileHandle handle, long fileOffset)
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
}

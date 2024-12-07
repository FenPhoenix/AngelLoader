// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AL_Common.NETM_IO;

internal static class FileStreamHelpers
{
    // NOTE: any change to FileOptions enum needs to be matched here as it's used in the error validation
    private const FileOptions ValidFileOptions = FileOptions.WriteThrough | FileOptions.Asynchronous | FileOptions.RandomAccess
        | FileOptions.DeleteOnClose | FileOptions.SequentialScan | FileOptions.Encrypted
        | (FileOptions)0x20000000 /* NoBuffering */ | (FileOptions)0x02000000 /* BackupOrRestore */;

    internal static bool IsIoRelatedException(Exception e) =>
        // These all derive from IOException
        //     DirectoryNotFoundException
        //     DriveNotFoundException
        //     EndOfStreamException
        //     FileLoadException
        //     FileNotFoundException
        //     PathTooLongException
        //     PipeException
        e is IOException ||
        // Note that SecurityException is only thrown on runtimes that support CAS
        // e is SecurityException ||
        e is UnauthorizedAccessException ||
        e is NotSupportedException ||
        e is ArgumentException && e is not ArgumentNullException;

    internal static void ValidateArguments(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize)
    {
        ArgumentException_NET.ThrowIfNullOrEmpty(path);

        // don't include inheritable in our bounds check for share
        FileShare tempShare = share & ~FileShare.Inheritable;
        string? badArg = null;

        if (mode < FileMode.CreateNew || mode > FileMode.Append)
        {
            badArg = nameof(mode);
        }
        else if (access < FileAccess.Read || access > FileAccess.ReadWrite)
        {
            badArg = nameof(access);
        }
        else if (tempShare < FileShare.None || tempShare > (FileShare.ReadWrite | FileShare.Delete))
        {
            badArg = nameof(share);
        }

        if (badArg != null)
        {
            throw new ArgumentOutOfRangeException(badArg, SR.ArgumentOutOfRange_Enum);
        }

        // NOTE: any change to FileOptions enum needs to be matched here in the error validation
        if (AreInvalid(options))
        {
            throw new ArgumentOutOfRangeException(nameof(options), SR.ArgumentOutOfRange_Enum);
        }
        else if (preallocationSize < 0)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_NeedNonNegNum(nameof(preallocationSize));
        }

        // Write access validation
        if ((access & FileAccess.Write) == 0)
        {
            if (mode == FileMode.Truncate || mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.Append)
            {
                // No write access, mode and access disagree but flag access since mode comes first
                throw new ArgumentException(SR.Format(SR.Argument_InvalidFileModeAndAccessCombo, mode, access), nameof(access));
            }
        }

        if ((access & FileAccess.Read) != 0 && mode == FileMode.Append)
        {
            throw new ArgumentException(SR.Argument_InvalidAppendMode, nameof(access));
        }

        if (preallocationSize > 0)
        {
            ValidateArgumentsForPreallocation(mode, access);
        }
    }

    private static void ValidateArgumentsForPreallocation(FileMode mode, FileAccess access)
    {
        // The user will be writing into the preallocated space.
        if ((access & FileAccess.Write) == 0)
        {
            throw new ArgumentException(SR.Argument_InvalidPreallocateAccess, nameof(access));
        }

        // Only allow preallocation for newly created/overwritten files.
        // When we fail to preallocate, we'll remove the file.
        if (mode != FileMode.Create &&
            mode != FileMode.CreateNew)
        {
            throw new ArgumentException(SR.Argument_InvalidPreallocateMode, nameof(mode));
        }
    }

    private static bool AreInvalid(FileOptions options) => options != FileOptions.None && (options & ~ValidFileOptions) != 0;

    internal static void FlushToDisk(AL_SafeFileHandle handle)
    {
        if (!Interop.Kernel32.FlushFileBuffers(handle))
        {
            int errorCode = Marshal.GetLastWin32Error();

            // NOTE: unlike fsync() on Unix, the FlushFileBuffers() function on Windows doesn't
            // support flushing handles opened for read-only access and will return an error. We
            // ignore this error to harmonize the two platforms: i.e. users can flush handles
            // opened for read-only access on BOTH platforms and no exception will be thrown.
            if (errorCode != Interop.Errors.ERROR_ACCESS_DENIED)
            {
                throw Win32Marshal.GetExceptionForLastWin32Error(handle.Path);
            }
        }
    }

    internal static long Seek(AL_SafeFileHandle handle, long offset, SeekOrigin origin, bool closeInvalidHandle = false)
    {
        Debug.Assert(origin >= SeekOrigin.Begin && origin <= SeekOrigin.End);

        if (!Interop.Kernel32.SetFilePointerEx(handle, offset, out long ret, (uint)origin))
        {
            if (closeInvalidHandle)
            {
                throw Win32Marshal.GetExceptionForWin32Error(GetLastWin32ErrorAndDisposeHandleIfInvalid(handle), handle.Path);
            }
            else
            {
                throw Win32Marshal.GetExceptionForLastWin32Error(handle.Path);
            }
        }

        return ret;
    }

    internal static void ThrowInvalidArgument(AL_SafeFileHandle handle) =>
        throw Win32Marshal.GetExceptionForWin32Error(Interop.Errors.ERROR_INVALID_PARAMETER, handle.Path);

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
}

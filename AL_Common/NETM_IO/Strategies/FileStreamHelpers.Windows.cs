// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AL_Common.NETM_IO.Strategies
{
    // this type defines a set of stateless FileStream/FileStreamStrategy helper methods
    internal static partial class FileStreamHelpers
    {
        private static OSFileStreamStrategy ChooseStrategyCore(AL_SafeFileHandle handle, FileAccess access) =>
            new SyncWindowsFileStreamStrategy(handle, access);

        private static FileStreamStrategy ChooseStrategyCore(string path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize) =>
                new SyncWindowsFileStreamStrategy(path, mode, access, share, options, preallocationSize);

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
}

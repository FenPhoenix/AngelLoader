// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AL_Common;
public static partial class Common
{
    /// <summary>
    /// Gets the target of the specified file link.
    /// </summary>
    /// <param name="linkPath">The path of the file link.</param>
    /// <param name="returnFinalTarget"><see langword="true"/> to follow links to the final target; <see langword="false"/> to return the immediate next link.</param>
    /// <returns>A <see cref="FileInfo"/> instance if <paramref name="linkPath"/> exists, independently if the target exists or not. <see langword="null"/> if <paramref name="linkPath"/> is not a link.</returns>
    /// <exception cref="IOException">The file on <paramref name="linkPath"/> does not exist.
    /// -or-
    /// The link's file system entry type is inconsistent with that of its target.
    /// -or-
    /// Too many levels of symbolic links.</exception>
    /// <remarks>When <paramref name="returnFinalTarget"/> is <see langword="true"/>, the maximum number of symbolic links that are followed are 40 on Unix and 63 on Windows.</remarks>
    public static FileSystemInfo? File_ResolveLinkTarget(string linkPath, bool returnFinalTarget)
    {
        FileSystem_SymLink.VerifyValidPath(linkPath, nameof(linkPath));
        return FileSystem_SymLink.ResolveLinkTarget(linkPath, returnFinalTarget, isDirectory: false);
    }

    /// <summary>
    /// Gets the target of the specified directory link.
    /// </summary>
    /// <param name="linkPath">The path of the directory link.</param>
    /// <param name="returnFinalTarget"><see langword="true"/> to follow links to the final target; <see langword="false"/> to return the immediate next link.</param>
    /// <returns>A <see cref="DirectoryInfo"/> instance if <paramref name="linkPath"/> exists, independently if the target exists or not. <see langword="null"/> if <paramref name="linkPath"/> is not a link.</returns>
    /// <exception cref="IOException">The directory on <paramref name="linkPath"/> does not exist.
    /// -or-
    /// The link's file system entry type is inconsistent with that of its target.
    /// -or-
    /// Too many levels of symbolic links.</exception>
    /// <remarks>When <paramref name="returnFinalTarget"/> is <see langword="true"/>, the maximum number of symbolic links that are followed are 40 on Unix and 63 on Windows.</remarks>
    public static FileSystemInfo? Directory_ResolveLinkTarget(string linkPath, bool returnFinalTarget)
    {
        FileSystem_SymLink.VerifyValidPath(linkPath, nameof(linkPath));
        return FileSystem_SymLink.ResolveLinkTarget(linkPath, returnFinalTarget, isDirectory: true);
    }

    private static class FileSystem_SymLink
    {
        internal static void VerifyValidPath(string path, string argName)
        {
            if (path.IsEmpty())
            {
                ThrowHelper.ArgumentException(argName + " was null or empty.", argName);
            }
            if (path.Contains('\0'))
            {
                ThrowHelper.ArgumentException("Null character in path.", argName);
            }
        }

        internal static FileSystemInfo? ResolveLinkTarget(string linkPath, bool returnFinalTarget, bool isDirectory)
        {
            string? targetPath = returnFinalTarget ?
                GetFinalLinkTarget(linkPath, isDirectory) :
                GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: true, returnFullPath: true);

            return targetPath == null ? null :
                isDirectory ? new DirectoryInfo(targetPath) : new FileInfo(targetPath);
        }

        private static unsafe SafeFileHandle OpenSafeFileHandle(string path, int flags)
        {
            SafeFileHandle handle = CreateFile(
                path,
                dwDesiredAccess: 0,
                FileShare.ReadWrite | FileShare.Delete,
                lpSecurityAttributes: null,
                FileMode.Open,
                dwFlagsAndAttributes: flags,
                hTemplateFile: IntPtr.Zero);

            return handle;
        }

        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use CreateFile.
        /// </summary>
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern unsafe SafeFileHandle CreateFilePrivate(
            string lpFileName,
            int dwDesiredAccess,
        FileShare dwShareMode,
        Interop.Kernel32.SECURITY_ATTRIBUTES* lpSecurityAttributes,
            FileMode dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        private static unsafe SafeFileHandle CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            FileShare dwShareMode,
            Interop.Kernel32.SECURITY_ATTRIBUTES* lpSecurityAttributes,
            FileMode dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile)
        {
            lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
            return CreateFilePrivate(
                lpFileName,
                dwDesiredAccess,
                dwShareMode,
                lpSecurityAttributes,
                dwCreationDisposition,
                dwFlagsAndAttributes,
                hTemplateFile);
        }

        private static unsafe string? GetFinalLinkTarget(string linkPath, bool isDirectory)
        {
            Interop.Kernel32.WIN32_FIND_DATA data = default;
            Interop.GetFindData(linkPath, isDirectory, ignoreAccessDenied: false, ref data);

            // The file or directory is not a reparse point.
            if ((data.dwFileAttributes & (uint)FileAttributes.ReparsePoint) == 0 ||
                // Only symbolic links and mount points are supported at the moment.
                (data.dwReserved0 != Interop.Kernel32.IOReparseOptions.IO_REPARSE_TAG_SYMLINK &&
                 data.dwReserved0 != Interop.Kernel32.IOReparseOptions.IO_REPARSE_TAG_MOUNT_POINT))
            {
                return null;
            }

            // We try to open the final file since they asked for the final target.
            using SafeFileHandle handle = OpenSafeFileHandle(linkPath,
                    Interop.Kernel32.FileOperations.OPEN_EXISTING |
                    Interop.Kernel32.FileOperations.FILE_FLAG_BACKUP_SEMANTICS);

            if (handle.IsInvalid)
            {
                // If the handle fails because it is unreachable, is because the link was broken.
                // We need to fallback to manually traverse the links and return the target of the last resolved link.
                int error = Marshal.GetLastWin32Error();
                if (FileSystem.IsPathUnreachableError(error))
                {
                    return GetFinalLinkTargetSlow(linkPath, isDirectory);
                }

                throw Win32Marshal.GetExceptionForWin32Error(error, linkPath);
            }

            const int InitialBufferSize = 4096;
            char[] buffer = ArrayPool<char>.Shared.Rent(InitialBufferSize);
            try
            {
                uint result = GetFinalPathNameByHandle(handle, buffer);

                // If the function fails because lpszFilePath is too small to hold the string plus the terminating null character,
                // the return value is the required buffer size, in TCHARs. This value includes the size of the terminating null character.
                if (result > buffer.Length)
                {
                    char[] toReturn = buffer;
                    buffer = ArrayPool<char>.Shared.Rent((int)result);
                    ArrayPool<char>.Shared.Return(toReturn);

                    result = GetFinalPathNameByHandle(handle, buffer);
                }

                // If the function fails for any other reason, the return value is zero.
                if (result == 0)
                {
                    throw Win32Marshal.GetExceptionForLastWin32Error(linkPath);
                }

                Debug.Assert(PathInternal.IsExtended(new string(buffer, 0, (int)result).AsSpan()));
                // GetFinalPathNameByHandle always returns with extended DOS prefix even if the link target was created without one.
                // While this does not interfere with correct behavior, it might be unexpected.
                // Hence we trim it if the passed-in path to the link wasn't extended.
                int start = PathInternal.IsExtended(linkPath.AsSpan()) ? 0 : 4;
                return new string(buffer, start, (int)result - start);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            static uint GetFinalPathNameByHandle(SafeFileHandle handle, char[] buffer)
            {
                fixed (char* bufPtr = buffer)
                {
                    return Interop.Kernel32.GetFinalPathNameByHandle(handle, bufPtr, (uint)buffer.Length, Interop.Kernel32.FILE_NAME_NORMALIZED);
                }
            }

            static string? GetFinalLinkTargetSlow(string linkPath, bool isDirectory)
            {
                // Since all these paths will be passed to CreateFile, which takes a string anyway, it is pointless to use span.
                // I am not sure if it's possible to change CreateFile's param to ROS<char> and avoid all these allocations.

                // We don't throw on error since we already did all the proper validations before.
                string? current = GetImmediateLinkTarget(linkPath, isDirectory, throwOnError: false, returnFullPath: true);
                string? prev = null;

                while (current != null)
                {
                    prev = current;
                    current = GetImmediateLinkTarget(current, isDirectory, throwOnError: false, returnFullPath: true);
                }

                return prev;
            }
        }

        /// <summary>
        /// Gets reparse point information associated to <paramref name="linkPath"/>.
        /// </summary>
        /// <returns>The immediate link target, absolute or relative or null if the file is not a supported link.</returns>
        private static unsafe string? GetImmediateLinkTarget(string linkPath, bool isDirectory, bool throwOnError, bool returnFullPath)
        {
            using SafeFileHandle handle = OpenSafeFileHandle(linkPath,
                    Interop.Kernel32.FileOperations.FILE_FLAG_BACKUP_SEMANTICS |
                    Interop.Kernel32.FileOperations.FILE_FLAG_OPEN_REPARSE_POINT);

            if (handle.IsInvalid)
            {
                if (!throwOnError)
                {
                    return null;
                }

                int error = Marshal.GetLastWin32Error();
                // File not found doesn't make much sense coming from a directory.
                if (isDirectory && error == Interop.Errors.ERROR_FILE_NOT_FOUND)
                {
                    error = Interop.Errors.ERROR_PATH_NOT_FOUND;
                }

                throw Win32Marshal.GetExceptionForWin32Error(error, linkPath);
            }

            byte[] buffer = ArrayPool<byte>.Shared.Rent(Interop.Kernel32.MAXIMUM_REPARSE_DATA_BUFFER_SIZE);
            try
            {
                bool success;

                fixed (byte* pBuffer = buffer)
                {
                    success = Interop.Kernel32.DeviceIoControl(
                        handle,
                        dwIoControlCode: Interop.Kernel32.FSCTL_GET_REPARSE_POINT,
                        lpInBuffer: null,
                        nInBufferSize: 0,
                        lpOutBuffer: pBuffer,
                        nOutBufferSize: Interop.Kernel32.MAXIMUM_REPARSE_DATA_BUFFER_SIZE,
                        out _,
                        IntPtr.Zero);
                }

                if (!success)
                {
                    if (!throwOnError)
                    {
                        return null;
                    }

                    int error = Marshal.GetLastWin32Error();
                    // The file or directory is not a reparse point.
                    if (error == Interop.Errors.ERROR_NOT_A_REPARSE_POINT)
                    {
                        return null;
                    }

                    throw Win32Marshal.GetExceptionForWin32Error(error, linkPath);
                }

                Span<byte> bufferSpan = new(buffer);
                success = MemoryMarshal.TryRead(bufferSpan, out Interop.Kernel32.SymbolicLinkReparseBuffer rbSymlink);
                Debug.Assert(success);

                // We always use SubstituteName(Offset|Length) instead of PrintName(Offset|Length),
                // the latter is just the display name of the reparse point and it can show something completely unrelated to the target.

                if (rbSymlink.ReparseTag == Interop.Kernel32.IOReparseOptions.IO_REPARSE_TAG_SYMLINK)
                {
                    int offset = sizeof(Interop.Kernel32.SymbolicLinkReparseBuffer) + rbSymlink.SubstituteNameOffset;
                    int length = rbSymlink.SubstituteNameLength;

                    Span<char> targetPath = MemoryMarshal.Cast<byte, char>(bufferSpan.Slice(offset, length));

                    bool isRelative = (rbSymlink.Flags & Interop.Kernel32.SYMLINK_FLAG_RELATIVE) != 0;
                    if (!isRelative)
                    {
                        // Absolute target is in NT format and we need to clean it up before return it to the user.
                        if (targetPath.StartsWith(PathInternal.UncNTPathPrefix.AsSpan()))
                        {
                            // We need to prepend the Win32 equivalent of UNC NT prefix.
                            return Path.Combine(PathInternal.UncPathPrefix, targetPath[PathInternal.UncNTPathPrefix.Length..].ToString());
                        }

                        return GetTargetPathWithoutNTPrefix(targetPath);
                    }
                    else if (returnFullPath)
                    {
                        return Path.Combine(Path.GetDirectoryName(linkPath) ?? "", targetPath.ToString());
                    }
                    else
                    {
                        return targetPath.ToString();
                    }
                }
                else if (rbSymlink.ReparseTag == Interop.Kernel32.IOReparseOptions.IO_REPARSE_TAG_MOUNT_POINT)
                {
                    success = MemoryMarshal.TryRead(bufferSpan, out Interop.Kernel32.MountPointReparseBuffer rbMountPoint);
                    Debug.Assert(success);

                    int offset = sizeof(Interop.Kernel32.MountPointReparseBuffer) + rbMountPoint.SubstituteNameOffset;
                    int length = rbMountPoint.SubstituteNameLength;

                    Span<char> targetPath = MemoryMarshal.Cast<byte, char>(bufferSpan.Slice(offset, length));

                    // Unlike symbolic links, mount point paths cannot be relative.
                    Debug.Assert(!PathInternal.IsPartiallyQualified(targetPath));
                    // Mount points cannot point to a remote location.
                    Debug.Assert(!targetPath.StartsWith(PathInternal.UncNTPathPrefix.AsSpan()));
                    return GetTargetPathWithoutNTPrefix(targetPath);
                }

                return null;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            static string GetTargetPathWithoutNTPrefix(ReadOnlySpan<char> targetPath)
            {
                Debug.Assert(targetPath.StartsWith(PathInternal.NTPathPrefix.AsSpan()));
                return targetPath[PathInternal.NTPathPrefix.Length..].ToString();
            }
        }
    }
}

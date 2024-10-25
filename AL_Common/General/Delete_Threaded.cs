// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using static AL_Common.FastIO_Native;

namespace AL_Common;

// @MT_TASK: On 71 set, we get 13 vs. 9 seconds framework/threaded. Test on NVME, presumably we'll get better scaling there.

public static class Delete_Threaded
{
    public static void Delete(string path, bool recursive, int threadCount)
    {
        string fullPath = Path.GetFullPath(path);
        FileSystem.RemoveDirectory(fullPath, recursive, threadCount);
    }

    private static class FileSystem
    {
        private static readonly string DirectorySeparatorCharAsString = Path.DirectorySeparatorChar.ToString();

        // \\
        private const int UncPrefixLength = 2;
        // \\?\UNC\, \\.\UNC\
        private const int UncExtendedPrefixLength = 8;

        /// <summary>
        /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
        /// </summary>
        private static bool IsDevice(ReadOnlySpan<char> path)
        {
            // If the path begins with any two separators is will be recognized and normalized and prepped with
            // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
            return AL_SafeFileHandle.IsExtended(path)
                   ||
                   (
                       path.Length >= AL_SafeFileHandle.DevicePrefixLength
                       && IsDirectorySeparator(path[0])
                       && IsDirectorySeparator(path[1])
                       && (path[2] == '.' || path[2] == '?')
                       && IsDirectorySeparator(path[3])
                   );
        }

        /// <summary>
        /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
        /// </summary>
        private static bool IsDeviceUNC(ReadOnlySpan<char> path)
        {
            return path.Length >= UncExtendedPrefixLength
                   && IsDevice(path)
                   && IsDirectorySeparator(path[7])
                   && path[4] == 'U'
                   && path[5] == 'N'
                   && path[6] == 'C';
        }

        /// <summary>
        /// Gets the length of the root of the path (drive, share, etc.).
        /// </summary>
        private static int GetRootLength(ReadOnlySpan<char> path)
        {
            int pathLength = path.Length;
            int i = 0;

            bool deviceSyntax = IsDevice(path);
            bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

            if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
            {
                // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
                {
                    // UNC (\\?\UNC\ or \\), scan past server\share

                    // Start past the prefix ("\\" or "\\?\UNC\")
                    i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                    // Skip two separators at most
                    int n = 2;
                    while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                        i++;
                }
                else
                {
                    // Current drive rooted (e.g. "\foo")
                    i = 1;
                }
            }
            else if (deviceSyntax)
            {
                // Device path (e.g. "\\?\.", "\\.\")
                // Skip any characters following the prefix that aren't a separator
                i = AL_SafeFileHandle.DevicePrefixLength;
                while (i < pathLength && !IsDirectorySeparator(path[i]))
                    i++;

                // If there is another separator take it, as long as we have had at least one
                // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                if (i < pathLength && i > AL_SafeFileHandle.DevicePrefixLength && IsDirectorySeparator(path[i]))
                    i++;
            }
            else if (pathLength >= 2
                && path[1] == Path.VolumeSeparatorChar
                && AL_SafeFileHandle.IsValidDriveChar(path[0]))
            {
                // Valid drive specified path ("C:", "D:", etc.)
                i = 2;

                // If the colon is followed by a directory separator, move past it (e.g "C:\")
                if (pathLength > 2 && IsDirectorySeparator(path[2]))
                    i++;
            }

            return i;
        }

        private static bool IsRoot(ReadOnlySpan<char> path) => path.Length == GetRootLength(path);

        /// <summary>
        /// True if the given character is a directory separator.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirectorySeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }

        /// <summary>
        /// Returns true if the path ends in a directory separator.
        /// </summary>
        private static bool EndsInDirectorySeparator([NotNullWhen(true)] string? path) =>
            !path.IsEmpty() && IsDirectorySeparator(path[^1]);

        /// <summary>
        /// Trims one trailing directory separator beyond the root of the path.
        /// </summary>
        [return: NotNullIfNotNull(nameof(path))]
        private static string? TrimEndingDirectorySeparator(string? path) =>
            EndsInDirectorySeparator(path) && !IsRoot(path.AsSpan()) ?
                path!.Substring(0, path.Length - 1) :
                path;

        public static void RemoveDirectory(string fullPath, bool recursive, int threadCount)
        {
            if (!recursive)
            {
                RemoveDirectoryInternal(fullPath, topLevel: true);
                return;
            }

            WIN32_FIND_DATAW findData = default;
            // FindFirstFile($path) (used by GetFindData) fails with ACCESS_DENIED when user has no ListDirectory rights
            // but FindFirstFile($path/*") (used by RemoveDirectoryRecursive) works fine in such scenario.
            // So we ignore it here and let RemoveDirectoryRecursive throw if FindFirstFile($path/*") fails with ACCESS_DENIED.
            GetFindData(fullPath, isDirectory: true, ignoreAccessDenied: true, ref findData);
            if (IsNameSurrogateReparsePoint(ref findData))
            {
                // Don't recurse
                RemoveDirectoryInternal(fullPath, topLevel: true);
                return;
            }

            // We want extended syntax so we can delete "extended" subdirectories and files
            // (most notably ones with trailing whitespace or periods)
            fullPath = AL_SafeFileHandle.EnsureExtendedPrefix(fullPath);
            RemoveDirectoryRecursive(fullPath, ref findData, topLevel: true, threadCount);
        }

        private static void RemoveDirectoryRecursive(string fullPath, ref WIN32_FIND_DATAW findData, bool topLevel, int threadCount)
        {
            DeleteInfo deleteInfo = new();
            GetDirectoriesToDeleteRecursive(fullPath, ref findData, topLevel, deleteInfo);
            if (!RemoveDirectoryRecursive(fullPath, deleteInfo, topLevel, threadCount))
            {
                Directory.Delete(fullPath, recursive: true);
            }
        }

        /*
        @MT_TASK(GetDirectoriesToDeleteRecursive parallelization):
        This step is a read step only, so we could separate this out and run this for every FM in parallel
        beforehand (before the backup) (any threading mode), then run the delete step afterward with this data
        and parallelize that too (aggressive threading only?).
        Actually, we're getting all files for the backup, so we could even just reuse that list (if we're doing
        the backup that is) and only get directories here (don't get files, will save a ton of time).
        Although that might add too much complexity.

        Note that we CAN'T parallelize this per-file, because FindFirstFile/FindNextFile are inherently serial
        and there's no other (sane, supported) way to get files.
        */
        private static void GetDirectoriesToDeleteRecursive(string fullPath, ref WIN32_FIND_DATAW findData, bool topLevel, DeleteInfo deleteInfo, string? filePath = null)
        {
            int errorCode;
            Exception? exception = null;

            using (SafeFindHandle handle = Interop.Kernel32.FindFirstFile(Path.Combine(fullPath, "*"), ref findData))
            {
                if (handle.IsInvalid)
                    throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);

                do
                {
                    if ((findData.dwFileAttributes & Interop.Kernel32.FileAttributes.FILE_ATTRIBUTE_DIRECTORY) == 0)
                    {
                        // File
                        string fileName = findData.cFileName;
                        deleteInfo.Files.Add(Path.Combine(fullPath, fileName));
                    }
                    else
                    {
                        // Directory, skip ".", "..".
                        if (findData.cFileName == "." || findData.cFileName == "..")
                            continue;

                        string fileName = findData.cFileName;

                        if (!IsNameSurrogateReparsePoint(ref findData))
                        {
                            // Not a reparse point, or the reparse point isn't a name surrogate, recurse.
                            try
                            {
                                GetDirectoriesToDeleteRecursive(
                                    Path.Combine(fullPath, fileName),
                                    findData: ref findData,
                                    topLevel: false,
                                    deleteInfo,
                                    filePath: filePath);
                            }
                            catch (Exception e)
                            {
                                exception ??= e;
                            }
                        }
                        else
                        {
                            string? mountPoint = null;

                            // Name surrogate reparse point, don't recurse, simply remove the directory.
                            // If a mount point, we have to delete the mount point first.
                            if (findData.dwReserved0 == Interop.Kernel32.IOReparseOptions.IO_REPARSE_TAG_MOUNT_POINT)
                            {
                                // Mount point. Unmount using full path plus a trailing '\'.
                                // (Note: This doesn't remove the underlying directory)
                                mountPoint = Path.Combine(fullPath, fileName, DirectorySeparatorCharAsString);
                            }

                            string directory = Path.Combine(fullPath, fileName);

                            // Note that RemoveDirectory on a symbolic link will remove the link itself.
                            deleteInfo.ReparsePoints.Add(new ReparsePointEntry(directory, fileName, mountPoint));
                        }
                    }
                } while (Interop.Kernel32.FindNextFile(handle, ref findData));

                if (exception != null)
                    throw exception;

                errorCode = Marshal.GetLastWin32Error();
                if (errorCode != Interop.Errors.ERROR_SUCCESS && errorCode != Interop.Errors.ERROR_NO_MORE_FILES)
                    throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);

                deleteInfo.Directories.Add(new DirectoryEntry(fullPath, filePath ?? ""));
            }
        }

        private static bool RemoveDirectoryRecursive(string fullPath, DeleteInfo deleteInfo, bool topLevel, int threadCount)
        {
            threadCount = Math.Min(threadCount, deleteInfo.Files.Count);

            int errorCode;
            // Stupid, but keeps the static analyzer's trap shut about "captured in closure" and blah blah blah
            ConcurrentStack<Exception> exceptions = new();

            if (deleteInfo.Files.Count > 0)
            {
                if (!Common.TryGetParallelForData(threadCount, deleteInfo.Files, CancellationToken.None, out var filesPD))
                {
                    return false;
                }

                Parallel.For(0, threadCount, filesPD.PO, i =>
                {
                    while (filesPD.CQ.TryDequeue(out string fileName))
                    {
                        if (!Interop.Kernel32.DeleteFile(fileName) && !exceptions.TryPeek(out _))
                        {
                            errorCode = Marshal.GetLastWin32Error();

                            // We don't care if something else deleted the file first
                            if (errorCode != Interop.Errors.ERROR_FILE_NOT_FOUND)
                            {
                                exceptions.Push(Win32Marshal.GetExceptionForWin32Error(errorCode, fileName));
                            }
                        }
                    }
                });
            }

            foreach (ReparsePointEntry reparsePointEntry in deleteInfo.ReparsePoints)
            {
                // Name surrogate reparse point, don't recurse, simply remove the directory.
                // If a mount point, we have to delete the mount point first.
                if (reparsePointEntry.VolumeMountPoint != null)
                {
                    // Mount point. Unmount using full path plus a trailing '\'.
                    // (Note: This doesn't remove the underlying directory)
                    if (!Interop.Kernel32.DeleteVolumeMountPoint(reparsePointEntry.VolumeMountPoint) && !exceptions.TryPeek(out _))
                    {
                        errorCode = Marshal.GetLastWin32Error();
                        if (errorCode != Interop.Errors.ERROR_SUCCESS &&
                            errorCode != Interop.Errors.ERROR_PATH_NOT_FOUND)
                        {
                            exceptions.Push(Win32Marshal.GetExceptionForWin32Error(errorCode, reparsePointEntry.DirName));
                        }
                    }
                }

                // Note that RemoveDirectory on a symbolic link will remove the link itself.
                if (!Interop.Kernel32.RemoveDirectory(reparsePointEntry.Directory) && !exceptions.TryPeek(out _))
                {
                    errorCode = Marshal.GetLastWin32Error();
                    if (errorCode != Interop.Errors.ERROR_PATH_NOT_FOUND)
                    {
                        exceptions.Push(Win32Marshal.GetExceptionForWin32Error(errorCode, reparsePointEntry.DirName));
                    }
                }
            }

            foreach (DirectoryEntry directoryEntry in deleteInfo.Directories)
            {
                // As we successfully removed all of the files we shouldn't care about the directory itself
                // not being empty. As file deletion is just a marker to remove the file when all handles
                // are closed we could still have undeleted contents.
                RemoveDirectoryInternal(directoryEntry.Directory, topLevel: topLevel, allowDirectoryNotEmpty: true);
            }

            if (exceptions.TryPeek(out Exception exception))
                throw exception;

            errorCode = Marshal.GetLastWin32Error();
            if (errorCode != Interop.Errors.ERROR_SUCCESS && errorCode != Interop.Errors.ERROR_NO_MORE_FILES)
                throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);

            return true;
        }

        private static bool IsNameSurrogateReparsePoint(ref WIN32_FIND_DATAW data)
        {
            // Name surrogates are reparse points that point to other named entities local to the file system.
            // Reparse points can be used for other types of files, notably OneDrive placeholder files. We
            // should treat reparse points that are not name surrogates as any other directory, e.g. recurse
            // into them. Surrogates should just be detached.
            //
            // See
            // https://github.com/dotnet/runtime/issues/23646
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365511.aspx
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365197.aspx

            return ((FileAttributes)data.dwFileAttributes & FileAttributes.ReparsePoint) != 0
                   && (data.dwReserved0 & 0x20000000) != 0; // IsReparseTagNameSurrogate
        }

        private static void GetFindData(string fullPath, bool isDirectory, bool ignoreAccessDenied, ref WIN32_FIND_DATAW findData)
        {
            using SafeFindHandle handle = Interop.Kernel32.FindFirstFile(TrimEndingDirectorySeparator(fullPath), ref findData);
            if (handle.IsInvalid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                // File not found doesn't make much sense coming from a directory.
                if (isDirectory && errorCode == Interop.Errors.ERROR_FILE_NOT_FOUND)
                    errorCode = Interop.Errors.ERROR_PATH_NOT_FOUND;
                if (ignoreAccessDenied && errorCode == Interop.Errors.ERROR_ACCESS_DENIED)
                    return;
                throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
            }
        }

        private static void RemoveDirectoryInternal(string fullPath, bool topLevel, bool allowDirectoryNotEmpty = false)
        {
            if (!Interop.Kernel32.RemoveDirectory(fullPath))
            {
                int errorCode = Marshal.GetLastWin32Error();
                switch (errorCode)
                {
                    case Interop.Errors.ERROR_FILE_NOT_FOUND:
                        // File not found doesn't make much sense coming from a directory delete.
                        errorCode = Interop.Errors.ERROR_PATH_NOT_FOUND;
                        goto case Interop.Errors.ERROR_PATH_NOT_FOUND;
                    case Interop.Errors.ERROR_PATH_NOT_FOUND:
                        // We only throw for the top level directory not found, not for any contents.
                        if (!topLevel)
                            return;
                        break;
                    case Interop.Errors.ERROR_DIR_NOT_EMPTY:
                        if (allowDirectoryNotEmpty)
                            return;
                        break;
                    case Interop.Errors.ERROR_ACCESS_DENIED:
                        // This conversion was originally put in for Win9x. Keeping for compatibility.
                        throw new IOException(string.Format(SR.UnauthorizedAccess_IODenied_Path, fullPath));
                }

                throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
            }
        }
    }

    private sealed class DirectoryEntry
    {
        internal readonly string Directory;
        internal readonly string DirName;

        public DirectoryEntry(string directory, string dirName)
        {
            Directory = directory;
            DirName = dirName;
        }
    }

    private sealed class ReparsePointEntry
    {
        internal readonly string Directory;
        internal readonly string DirName;
        internal readonly string? VolumeMountPoint;

        internal ReparsePointEntry(string directory, string dirName, string? volumeMountPoint)
        {
            Directory = directory;
            DirName = dirName;
            VolumeMountPoint = volumeMountPoint;
        }
    }

    private sealed class DeleteInfo
    {
        internal readonly List<string> Files = new();
        internal readonly List<DirectoryEntry> Directories = new();
        internal readonly List<ReparsePointEntry> ReparsePoints = new();
    }

    internal sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFindHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle() => FindClose(handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FindClose(IntPtr hFindFile);
    }

    internal static partial class Interop
    {
        internal static class Errors
        {
            internal const int ERROR_SUCCESS = 0x0;
            internal const int ERROR_FILE_NOT_FOUND = 0x2;
            internal const int ERROR_PATH_NOT_FOUND = 0x3;
            internal const int ERROR_ACCESS_DENIED = 0x5;
            internal const int ERROR_NO_MORE_FILES = 0x12;
            internal const int ERROR_DIR_NOT_EMPTY = 0x91;
        }

        internal static partial class Kernel32
        {
            /// <summary>
            /// WARNING: This method does not implicitly handle long paths. Use DeleteVolumeMountPoint.
            /// </summary>
            [DllImport("kernel32", EntryPoint = "DeleteVolumeMountPointW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteVolumeMountPointPrivate(string mountPoint);

            internal static bool DeleteVolumeMountPoint(string mountPoint)
            {
                mountPoint = AL_SafeFileHandle.EnsureExtendedPrefixIfNeeded(mountPoint);
                return DeleteVolumeMountPointPrivate(mountPoint);
            }

            /// <summary>
            /// WARNING: This method does not implicitly handle long paths. Use DeleteFile.
            /// </summary>
            [DllImport("kernel32", EntryPoint = "DeleteFileW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool DeleteFilePrivate(string path);

            internal static bool DeleteFile(string path)
            {
                path = AL_SafeFileHandle.EnsureExtendedPrefixIfNeeded(path);
                return DeleteFilePrivate(path);
            }

            internal static partial class FileAttributes
            {
                internal const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
                internal const int FILE_ATTRIBUTE_READONLY = 0x00000001;
                internal const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
                internal const int FILE_ATTRIBUTE_REPARSE_POINT = 0x00000400;
            }

            internal static partial class IOReparseOptions
            {
                internal const uint IO_REPARSE_TAG_FILE_PLACEHOLDER = 0x80000015;
                internal const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
                internal const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;
            }

            internal static partial class FileOperations
            {
                internal const int OPEN_EXISTING = 3;
                internal const int COPY_FILE_FAIL_IF_EXISTS = 0x00000001;

                internal const int FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
                internal const int FILE_FLAG_FIRST_PIPE_INSTANCE = 0x00080000;
                internal const int FILE_FLAG_OPEN_REPARSE_POINT = 0x00200000;
                internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

                internal const int FILE_LIST_DIRECTORY = 0x0001;

                internal const int FILE_WRITE_ATTRIBUTES = 0x100;
            }

            /// <summary>
            /// WARNING: This method does not implicitly handle long paths. Use RemoveDirectory.
            /// </summary>
            [DllImport("kernel32", EntryPoint = "RemoveDirectoryW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool RemoveDirectoryPrivate(string path);

            internal static bool RemoveDirectory(string path)
            {
                path = AL_SafeFileHandle.EnsureExtendedPrefixIfNeeded(path);
                return RemoveDirectoryPrivate(path);
            }

            /// <summary>
            /// WARNING: This method does not implicitly handle long paths. Use FindFirstFile.
            /// </summary>
            [DllImport("kernel32", EntryPoint = "FindFirstFileExW", SetLastError = true, CharSet = CharSet.Unicode)]
            private static extern SafeFindHandle FindFirstFileExPrivate(
                string lpFileName,
                FINDEX_INFO_LEVELS fInfoLevelId,
                ref WIN32_FIND_DATAW lpFindFileData,
                FINDEX_SEARCH_OPS fSearchOp,
                IntPtr lpSearchFilter,
                int dwAdditionalFlags);

            internal static SafeFindHandle FindFirstFile(string fileName, ref WIN32_FIND_DATAW data)
            {
                fileName = AL_SafeFileHandle.EnsureExtendedPrefixIfNeeded(fileName);

                // use FindExInfoBasic since we don't care about short name and it has better perf
                return FindFirstFileExPrivate(fileName, FINDEX_INFO_LEVELS.FindExInfoBasic, ref data, FINDEX_SEARCH_OPS.FindExSearchNameMatch, IntPtr.Zero, 0);
            }

            internal enum FINDEX_INFO_LEVELS : uint
            {
                FindExInfoStandard = 0x0u,
                FindExInfoBasic = 0x1u,
                FindExInfoMaxInfoLevel = 0x2u,
            }

            internal enum FINDEX_SEARCH_OPS : uint
            {
                FindExSearchNameMatch = 0x0u,
                FindExSearchLimitToDirectories = 0x1u,
                FindExSearchLimitToDevices = 0x2u,
                FindExSearchMaxSearchOp = 0x3u,
            }

            [DllImport("kernel32", EntryPoint = "FindNextFileW", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool FindNextFile(SafeFindHandle hndFindFile, ref WIN32_FIND_DATAW lpFindFileData);
        }
    }
}

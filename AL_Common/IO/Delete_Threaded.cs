// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AL_Common;

/*
@MT_TASK_NOTE(Uninstall perf numbers on 71 set):

SATA:
13 vs. 9 seconds framework/threaded (~31% reduction)

NVME:
4.797 vs. 3.170 seconds framework/threaded (~34% reduction)

NVME is slightly better but not by a whole lot.
However, a ~30% time reduction is still worth having, even if it's less than we would have hoped.
*/

public static class Delete_Threaded
{
    /// <summary>
    /// If <paramref name="threadCount"/> is greater than 1, performs a threaded delete. Otherwise, just calls <see cref="Directory.Delete(string, bool)"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="recursive"></param>
    /// <param name="threadCount"></param>
    public static void Delete(string path, bool recursive, int threadCount)
    {
        if (threadCount <= 1)
        {
            Directory.Delete(path, recursive);
            return;
        }

        string fullPath = Path.GetFullPath(path);
        RemoveDirectory(fullPath, recursive, threadCount);
    }

    private static readonly string DirectorySeparatorCharAsString = Path.DirectorySeparatorChar.ToString();

    private static void RemoveDirectory(string fullPath, bool recursive, int threadCount)
    {
        if (!recursive)
        {
            RemoveDirectoryInternal(fullPath, topLevel: true);
            return;
        }

        Interop.Kernel32.WIN32_FIND_DATA findData = default;
        // FindFirstFile($path) (used by GetFindData) fails with ACCESS_DENIED when user has no ListDirectory rights
        // but FindFirstFile($path/*") (used by RemoveDirectoryRecursive) works fine in such scenario.
        // So we ignore it here and let RemoveDirectoryRecursive throw if FindFirstFile($path/*") fails with ACCESS_DENIED.
        Interop.GetFindData(fullPath, isDirectory: true, ignoreAccessDenied: true, ref findData);
        if (IsNameSurrogateReparsePoint(ref findData))
        {
            // Don't recurse
            RemoveDirectoryInternal(fullPath, topLevel: true);
            return;
        }

        // We want extended syntax so we can delete "extended" subdirectories and files
        // (most notably ones with trailing whitespace or periods)
        fullPath = PathInternal.EnsureExtendedPrefix(fullPath);
        RemoveDirectoryRecursive(fullPath, ref findData, topLevel: true, threadCount);
    }

    private static void RemoveDirectoryRecursive(string fullPath, ref Interop.Kernel32.WIN32_FIND_DATA findData, bool topLevel, int threadCount)
    {
        DeleteInfo deleteInfo = new();
        GetDirectoriesToDeleteRecursive(fullPath, ref findData, deleteInfo);
        if (!RemoveDirectoryRecursive(fullPath, deleteInfo, topLevel, threadCount))
        {
            Directory.Delete(fullPath, recursive: true);
        }
    }

    /*
    @MT_TASK_NOTE(GetDirectoriesToDeleteRecursive parallelization):
    This step is a read step only, so we could separate this out and run this for every FM in parallel
    beforehand (before the backup) (any threading mode), then run the delete step afterward with this data
    and parallelize that too (aggressive threading only?).
    Actually, we're getting all files for the backup, so we could even just reuse that list (if we're doing
    the backup that is) and only get directories here (don't get files, will save a ton of time).
    Although that might add too much complexity.

    Note that we CAN'T parallelize this per-file, because FindFirstFile/FindNextFile are inherently serial
    and there's no other (sane, supported) way to get files.
    */
    private static void GetDirectoriesToDeleteRecursive(string fullPath, ref Interop.Kernel32.WIN32_FIND_DATA findData, DeleteInfo deleteInfo, string? filePath = null)
    {
        Exception? exception = null;

        using SafeFindHandle handle = Interop.Kernel32.FindFirstFile(Path.Combine(fullPath, "*"), ref findData);

        if (handle.IsInvalid)
        {
            throw Win32Marshal.GetExceptionForLastWin32Error(fullPath);
        }

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
                {
                    continue;
                }

                string fileName = findData.cFileName;

                if (!IsNameSurrogateReparsePoint(ref findData))
                {
                    // Not a reparse point, or the reparse point isn't a name surrogate, recurse.
                    try
                    {
                        GetDirectoriesToDeleteRecursive(
                            Path.Combine(fullPath, fileName),
                            findData: ref findData,
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
        {
            throw exception;
        }

        int errorCode = Marshal.GetLastWin32Error();
        if (errorCode != Interop.Errors.ERROR_SUCCESS && errorCode != Interop.Errors.ERROR_NO_MORE_FILES)
        {
            throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
        }

        deleteInfo.Directories.Add(new DirectoryEntry(fullPath, filePath ?? ""));
    }

    private static bool RemoveDirectoryRecursive(string fullPath, DeleteInfo deleteInfo, bool topLevel, int threadCount)
    {
        threadCount = Math.Min(threadCount, deleteInfo.Files.Count);

        int errorCode;
        // Stupid, but keeps the static analyzer's trap shut about "captured in closure" and blah blah blah
        ConcurrentStack<Exception> exceptions = new();

        if (deleteInfo.Files.Count > 0)
        {
            if (!TryGetParallelForData(threadCount, deleteInfo.Files, CancellationToken.None, out var filesPD))
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
        {
            throw exception;
        }

        errorCode = Marshal.GetLastWin32Error();
        if (errorCode != Interop.Errors.ERROR_SUCCESS && errorCode != Interop.Errors.ERROR_NO_MORE_FILES)
        {
            throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
        }

        return true;
    }

    private static bool IsNameSurrogateReparsePoint(ref Interop.Kernel32.WIN32_FIND_DATA data)
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
                    {
                        return;
                    }
                    break;
                case Interop.Errors.ERROR_DIR_NOT_EMPTY:
                    if (allowDirectoryNotEmpty)
                    {
                        return;
                    }
                    break;
                case Interop.Errors.ERROR_ACCESS_DENIED:
                    // This conversion was originally put in for Win9x. Keeping for compatibility.
                    throw new IOException(string.Format(SR.UnauthorizedAccess_IODenied_Path, fullPath));
            }

            throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
        }
    }

    private sealed class DirectoryEntry
    {
        internal readonly string Directory;
        internal readonly string DirName;

        internal DirectoryEntry(string directory, string dirName)
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
}

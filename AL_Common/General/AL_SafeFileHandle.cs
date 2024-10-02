// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified from .NET 8 or 9 rc1 or whatever the heck

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace AL_Common;

[PublicAPI]
public sealed class AL_SafeFileHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private string? _path;
    internal string? Path => _path;
    private bool _lengthCanBeCached; // file has been opened for reading and not shared for writing.

    internal bool IsNoBuffering => (GetFileOptions() & NoBuffering) != 0;

    internal unsafe FileOptions GetFileOptions()
    {
        FileOptions fileOptions = _fileOptions;
        if (fileOptions != (FileOptions)(-1))
        {
            return fileOptions;
        }

        Interop.NtDll.CreateOptions options;
        int ntStatus = Interop.NtDll.NtQueryInformationFile(
            FileHandle: this,
            IoStatusBlock: out _,
            FileInformation: &options,
            Length: sizeof(uint),
            FileInformationClass: Interop.NtDll.FileModeInformation);

        if (ntStatus != Interop.StatusOptions.STATUS_SUCCESS)
        {
            int error = (int)Interop.NtDll.RtlNtStatusToDosError(ntStatus);
            throw Win32Marshal.GetExceptionForWin32Error(error);
        }

        FileOptions result = FileOptions.None;

        if ((options & (Interop.NtDll.CreateOptions.FILE_SYNCHRONOUS_IO_ALERT | Interop.NtDll.CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT)) == 0)
        {
            result |= FileOptions.Asynchronous;
        }
        if ((options & Interop.NtDll.CreateOptions.FILE_WRITE_THROUGH) != 0)
        {
            result |= FileOptions.WriteThrough;
        }
        if ((options & Interop.NtDll.CreateOptions.FILE_RANDOM_ACCESS) != 0)
        {
            result |= FileOptions.RandomAccess;
        }
        if ((options & Interop.NtDll.CreateOptions.FILE_SEQUENTIAL_ONLY) != 0)
        {
            result |= FileOptions.SequentialScan;
        }
        if ((options & Interop.NtDll.CreateOptions.FILE_DELETE_ON_CLOSE) != 0)
        {
            result |= FileOptions.DeleteOnClose;
        }
        if ((options & Interop.NtDll.CreateOptions.FILE_NO_INTERMEDIATE_BUFFERING) != 0)
        {
            result |= NoBuffering;
        }

        return _fileOptions = result;
    }

    /// <summary>
    /// Creates a <see cref="T:AL_SafeFileHandle" /> around a file handle.
    /// </summary>
    /// <param name="preexistingHandle">Handle to wrap</param>
    /// <param name="ownsHandle">Whether to control the handle lifetime</param>
    public AL_SafeFileHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
    {
        SetHandle(preexistingHandle);
    }

    //internal string? Path => _path;

    internal const FileOptions NoBuffering = (FileOptions)0x20000000;
    private long _length = -1; // negative means that hasn't been fetched.
    private volatile FileOptions _fileOptions = (FileOptions)(-1);
    private volatile int _fileType = -1;

    public AL_SafeFileHandle() : base(true)
    {
    }

    internal bool CanSeek => !IsClosed && GetFileType() == FileTypes.FILE_TYPE_DISK;

    internal static AL_SafeFileHandle Open(string fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
    {
        // Don't pop up a dialog for reading from an empty floppy drive
        int oldMode = Win32Native.SetErrorMode(Win32Native.SEM_FAILCRITICALERRORS);
        try
        {
            // we don't use NtCreateFile as there is no public and reliable way
            // of converting DOS to NT file paths (RtlDosPathNameToRelativeNtPathName_U_WithStatus is not documented)
            AL_SafeFileHandle fileHandle = CreateFile(fullPath, mode, access, share, options);

            return fileHandle;
        }
        finally
        {
            Win32Native.SetErrorMode(oldMode);
        }
    }

    private static unsafe AL_SafeFileHandle CreateFile(string fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
    {
        SECURITY_ATTRIBUTES secAttrs = default;
        if ((share & FileShare.Inheritable) != 0)
        {
            secAttrs = new SECURITY_ATTRIBUTES
            {
                nLength = (uint)sizeof(SECURITY_ATTRIBUTES),
                bInheritHandle = BOOL.TRUE,
            };
        }

        int fAccess =
            ((access & FileAccess.Read) == FileAccess.Read ? GenericOperations.GENERIC_READ : 0) |
            ((access & FileAccess.Write) == FileAccess.Write ? GenericOperations.GENERIC_WRITE : 0);

        // Our Inheritable bit was stolen from Windows, but should be set in
        // the security attributes class.  Don't leave this bit set.
        share &= ~FileShare.Inheritable;

        // Must use a valid Win32 constant here...
        if (mode == FileMode.Append)
        {
            mode = FileMode.OpenOrCreate;
        }

        int flagsAndAttributes = (int)options;

        // For mitigating local elevation of privilege attack through named pipes
        // make sure we always call CreateFile with SECURITY_ANONYMOUS so that the
        // named pipe server can't impersonate a high privileged client security context
        // (note that this is the effective default on CreateFile2)
        flagsAndAttributes |= (Win32Native.SECURITY_SQOS_PRESENT | Win32Native.SECURITY_ANONYMOUS);

        AL_SafeFileHandle fileHandle = CreateFile(fullPath, fAccess, share, &secAttrs, mode, flagsAndAttributes, IntPtr.Zero);
        if (fileHandle.IsInvalid)
        {
            // Return a meaningful exception with the full path.

            // NT5 oddity - when trying to open "C:\" as a Win32FileStream,
            // we usually get ERROR_PATH_NOT_FOUND from the OS.  We should
            // probably be consistent w/ every other directory.
            int errorCode = Marshal.GetLastWin32Error();

            if (errorCode == Win32Native.ERROR_PATH_NOT_FOUND &&
                fullPath.Equals(Directory.GetDirectoryRoot(fullPath)))
            {
                errorCode = Win32Native.ERROR_ACCESS_DENIED;
            }

            fileHandle.Dispose();
            __Error.WinIOError(errorCode, fullPath);
        }

        fileHandle._path = fullPath;
        fileHandle._fileOptions = options;
        fileHandle._lengthCanBeCached = (share & FileShare.Write) == 0 && (access & FileAccess.Write) == 0;
        return fileHandle;
    }

    private int GetFileType()
    {
        int fileType = _fileType;
        if (fileType == -1)
        {
            _fileType = fileType = GetFileType(this);
        }

        return fileType;
    }

    protected override bool ReleaseHandle() => CloseHandle(handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct SECURITY_ATTRIBUTES
    {
        internal uint nLength;
        internal unsafe void* lpSecurityDescriptor;
        internal BOOL bInheritHandle;
    }

    /// <summary>
    /// Blittable version of Windows BOOL type. It is convenient in situations where
    /// manual marshalling is required, or to avoid overhead of regular bool marshalling.
    /// </summary>
    /// <remarks>
    /// Some Windows APIs return arbitrary integer values although the return type is defined
    /// as BOOL. It is best to never compare BOOL to TRUE. Always use bResult != BOOL.FALSE
    /// or bResult == BOOL.FALSE .
    /// </remarks>
    private enum BOOL : int
    {
        FALSE = 0,
        TRUE = 1,
    }

    private static class GenericOperations
    {
        internal const int GENERIC_READ = unchecked((int)0x80000000);
        internal const int GENERIC_WRITE = 0x40000000;
    }

    [DllImport("kernel32", SetLastError = true)]
    private static extern int GetFileType(SafeHandle hFile);

    [DllImport("kernel32", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    private static class FileTypes
    {
        internal const int FILE_TYPE_UNKNOWN = 0x0000;
        internal const int FILE_TYPE_DISK = 0x0001;
        internal const int FILE_TYPE_CHAR = 0x0002;
        internal const int FILE_TYPE_PIPE = 0x0003;
    }

    /// <summary>
    /// WARNING: This method does not implicitly handle long paths. Use CreateFile.
    /// </summary>
    //[LibraryImport(Libraries.Kernel32, EntryPoint = "CreateFileW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern unsafe AL_SafeFileHandle CreateFilePrivate(
        string lpFileName,
        int dwDesiredAccess,
        FileShare dwShareMode,
        SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    private static unsafe AL_SafeFileHandle CreateFile(
        string lpFileName,
        int dwDesiredAccess,
        FileShare dwShareMode,
        SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile)
    {
        lpFileName = EnsureExtendedPrefixIfNeeded(lpFileName);
        return CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
    }

    private static bool EndsWithPeriodOrSpace(string? path)
    {
        if (path.IsEmpty())
        {
            return false;
        }

        char c = path[^1];
        return c is ' ' or '.';
    }

    /// <summary>
    /// Adds the extended path prefix (\\?\) if not already a device path, IF the path is not relative,
    /// AND the path is more than 259 characters. (> MAX_PATH + null). This will also insert the extended
    /// prefix if the path ends with a period or a space. Trailing periods and spaces are normally eaten
    /// away from paths during normalization, but if we see such a path at this point it should be
    /// normalized and has retained the final characters. (Typically from one of the *Info classes)
    /// </summary>
    [return: NotNullIfNotNull(nameof(path))]
    private static string? EnsureExtendedPrefixIfNeeded(string? path)
    {
        if (path != null && (path.Length >= Common.MAX_PATH || EndsWithPeriodOrSpace(path)))
        {
            return EnsureExtendedPrefix(path);
        }
        else
        {
            return path;
        }
    }

    private const string ExtendedPathPrefix = @"\\?\";
    private const string UncPathPrefix = @"\\";
    private const string UncExtendedPrefixToInsert = @"?\UNC\";

    /// <summary>
    /// Adds the extended path prefix (\\?\) if not relative or already a device path.
    /// </summary>
    private static string EnsureExtendedPrefix(string path)
    {
        // Putting the extended prefix on the path changes the processing of the path. It won't get normalized, which
        // means adding to relative paths will prevent them from getting the appropriate current directory inserted.

        // If it already has some variant of a device path (\??\, \\?\, \\.\, //./, etc.) we don't need to change it
        // as it is either correct or we will be changing the behavior. When/if Windows supports long paths implicitly
        // in the future we wouldn't want normalization to come back and break existing code.

        // In any case, all internal usages should be hitting normalize path (Path.GetFullPath) before they hit this
        // shimming method. (Or making a change that doesn't impact normalization, such as adding a filename to a
        // normalized base path.)
        if (IsPartiallyQualified(path.AsSpan()) || IsDevice(path.AsSpan()))
        {
            return path;
        }

        // Given \\server\share in longpath becomes \\?\UNC\server\share
        if (path.StartsWith(UncPathPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return path.Insert(2, UncExtendedPrefixToInsert);
        }

        return ExtendedPathPrefix + path;
    }

    /// <summary>
    /// Returns true if the path specified is relative to the current drive or working directory.
    /// Returns false if the path is fixed to a specific drive or UNC path.  This method does no
    /// validation of the path (URIs will be returned as relative as a result).
    /// </summary>
    /// <remarks>
    /// Handles paths that use the alternate directory separator.  It is a frequent mistake to
    /// assume that rooted paths (Path.IsPathRooted) are not relative.  This isn't the case.
    /// "C:a" is drive relative- meaning that it will be resolved against the current directory
    /// for C: (rooted, but relative). "C:\a" is rooted and not relative (the current directory
    /// will not be used to modify the path).
    /// </remarks>
    private static bool IsPartiallyQualified(ReadOnlySpan<char> path)
    {
        if (path.Length < 2)
        {
            // It isn't fixed, it must be relative.  There is no way to specify a fixed
            // path with one character (or less).
            return true;
        }

        if (IsDirectorySeparator(path[0]))
        {
            // There is no valid way to specify a relative path with two initial slashes or
            // \? as ? isn't valid for drive relative paths and \??\ is equivalent to \\?\
            return !(path[1] == '?' || IsDirectorySeparator(path[1]));
        }

        // The only way to specify a fixed path that doesn't begin with two slashes
        // is the drive, colon, slash format- i.e. C:\
        return !((path.Length >= 3)
            && (path[1] == VolumeSeparatorChar)
            && IsDirectorySeparator(path[2])
            // To match old behavior we'll check the drive character for validity as the path is technically
            // not qualified if you don't have a valid drive. "=:\" is the "=" file's default data stream.
            && IsValidDriveChar(path[0]));
    }

    private const char VolumeSeparatorChar = ':';
    private const int DevicePrefixLength = 4;

    /// <summary>
    /// Returns true if the given character is a valid drive letter
    /// </summary>
    private static bool IsValidDriveChar(char value)
    {
        return (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
    }

    /// <summary>
    /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
    /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
    /// and path length checks.
    /// </summary>
    private static bool IsExtended(ReadOnlySpan<char> path)
    {
        // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
        // Skipping of normalization will *only* occur if back slashes ('\') are used.
        return path.Length >= DevicePrefixLength
               && path[0] == '\\'
               && (path[1] == '\\' || path[1] == '?')
               && path[2] == '?'
               && path[3] == '\\';
    }

    /// <summary>
    /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
    /// </summary>
    private static bool IsDevice(ReadOnlySpan<char> path)
    {
        // If the path begins with any two separators it will be recognized and normalized and prepped with
        // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
        return IsExtended(path) ||
               (
                   path.Length >= DevicePrefixLength
                   && IsDirectorySeparator(path[0])
                   && IsDirectorySeparator(path[1])
                   && (path[2] == '.' || path[2] == '?')
                   && IsDirectorySeparator(path[3])
               );
    }

    /// <summary>
    /// True if the given character is a directory separator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsDirectorySeparator(char c)
    {
        return c == System.IO.Path.DirectorySeparatorChar || c == System.IO.Path.AltDirectorySeparatorChar;
    }

    internal long GetFileLength()
    {
        if (!_lengthCanBeCached)
        {
            return GetFileLengthCore();
        }

        // On Windows, when the file is locked for writes we can cache file length
        // in memory and avoid subsequent native calls which are expensive.
        if (_length < 0)
        {
            _length = GetFileLengthCore();
        }

        return _length;

        unsafe long GetFileLengthCore()
        {
            Interop.Kernel32.FILE_STANDARD_INFO info;

            if (Interop.Kernel32.GetFileInformationByHandleEx(this, Interop.Kernel32.FileStandardInfo, &info, (uint)sizeof(Interop.Kernel32.FILE_STANDARD_INFO)))
            {
                return info.EndOfFile;
            }

            // In theory when GetFileInformationByHandleEx fails, then
            // a) IsDevice can modify last error (not true today, but can be in the future),
            // b) DeviceIoControl can succeed (last error set to ERROR_SUCCESS) but return fewer bytes than requested.
            // The error is stored and in such cases exception for the first failure is going to be thrown.
            int lastError = Marshal.GetLastWin32Error();

            if (Path is null || !IsDevice(Path.AsSpan()))
            {
                throw Win32Marshal.GetExceptionForWin32Error(lastError, Path);
            }

            Interop.Kernel32.STORAGE_READ_CAPACITY storageReadCapacity;
            bool success = Interop.Kernel32.DeviceIoControl(
                this,
                dwIoControlCode: Interop.Kernel32.IOCTL_STORAGE_READ_CAPACITY,
                lpInBuffer: null,
                nInBufferSize: 0,
                lpOutBuffer: &storageReadCapacity,
                nOutBufferSize: (uint)sizeof(Interop.Kernel32.STORAGE_READ_CAPACITY),
                out uint bytesReturned,
                IntPtr.Zero);

            if (!success)
            {
                throw Win32Marshal.GetExceptionForLastWin32Error(Path);
            }
            else if (bytesReturned != sizeof(Interop.Kernel32.STORAGE_READ_CAPACITY))
            {
                throw Win32Marshal.GetExceptionForWin32Error(lastError, Path);
            }

            return storageReadCapacity.DiskLength;
        }
    }
}

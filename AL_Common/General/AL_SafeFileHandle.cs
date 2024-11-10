// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified from .NET 8 or 9 rc1 or whatever the heck

using System;
using System.IO;
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

        const uint FileModeInformation = 16;

        CreateOptions options;
        int ntStatus = NtQueryInformationFile_Private(
            FileHandle: this,
            IoStatusBlock: out _,
            FileInformation: &options,
            Length: sizeof(uint),
            FileInformationClass: FileModeInformation);

        if (ntStatus != Interop.StatusOptions.STATUS_SUCCESS)
        {
            int error = (int)RtlNtStatusToDosError(ntStatus);
            throw Win32Marshal.GetExceptionForWin32Error(error);
        }

        FileOptions result = FileOptions.None;

        if ((options & (CreateOptions.FILE_SYNCHRONOUS_IO_ALERT | CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT)) == 0)
        {
            result |= FileOptions.Asynchronous;
        }
        if ((options & CreateOptions.FILE_WRITE_THROUGH) != 0)
        {
            result |= FileOptions.WriteThrough;
        }
        if ((options & CreateOptions.FILE_RANDOM_ACCESS) != 0)
        {
            result |= FileOptions.RandomAccess;
        }
        if ((options & CreateOptions.FILE_SEQUENTIAL_ONLY) != 0)
        {
            result |= FileOptions.SequentialScan;
        }
        if ((options & CreateOptions.FILE_DELETE_ON_CLOSE) != 0)
        {
            result |= FileOptions.DeleteOnClose;
        }
        if ((options & CreateOptions.FILE_NO_INTERMEDIATE_BUFFERING) != 0)
        {
            result |= NoBuffering;
        }

        return _fileOptions = result;

        [DllImport("ntdll.dll", EntryPoint = "NtQueryInformationFile")]
        static extern int NtQueryInformationFile_Private(
            AL_SafeFileHandle FileHandle,
            out Interop.NtDll.IO_STATUS_BLOCK IoStatusBlock,
            void* FileInformation,
            uint Length,
            uint FileInformationClass);
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

    internal const FileOptions NoBuffering = (FileOptions)0x20000000;
    private long _length = -1; // negative means that hasn't been fetched.
    private volatile FileOptions _fileOptions = (FileOptions)(-1);
    private volatile int _fileType = -1;

    public AL_SafeFileHandle() : base(true)
    {
    }

    internal bool CanSeek => !IsClosed && GetFileType() == FileTypes.FILE_TYPE_DISK;

    public static AL_SafeFileHandle Open(string fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
    {
        using (DisableMediaInsertionPrompt.Create())
        {
            // we don't use NtCreateFile as there is no public and reliable way
            // of converting DOS to NT file paths (RtlDosPathNameToRelativeNtPathName_U_WithStatus is not documented)
            AL_SafeFileHandle fileHandle = CreateFile(fullPath, mode, access, share, options);

            return fileHandle;
        }
    }

    private static unsafe AL_SafeFileHandle CreateFile(string fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
    {
        Interop.Kernel32.SECURITY_ATTRIBUTES secAttrs = default;
        if ((share & FileShare.Inheritable) != 0)
        {
            secAttrs = new Interop.Kernel32.SECURITY_ATTRIBUTES
            {
                nLength = (uint)sizeof(Interop.Kernel32.SECURITY_ATTRIBUTES),
                bInheritHandle = Interop.BOOL.TRUE,
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
        flagsAndAttributes |= Interop.Kernel32.SecurityOptions.SECURITY_SQOS_PRESENT |
                              Interop.Kernel32.SecurityOptions.SECURITY_ANONYMOUS;

        AL_SafeFileHandle fileHandle = CreateFile(fullPath, fAccess, share, &secAttrs, mode, flagsAndAttributes, IntPtr.Zero);
        if (fileHandle.IsInvalid)
        {
            // Return a meaningful exception with the full path.

            // NT5 oddity - when trying to open "C:\" as a Win32FileStream,
            // we usually get ERROR_PATH_NOT_FOUND from the OS.  We should
            // probably be consistent w/ every other directory.
            int errorCode = Marshal.GetLastWin32Error();

            if (errorCode == Interop.Errors.ERROR_PATH_NOT_FOUND &&
                fullPath.Equals(Directory.GetDirectoryRoot(fullPath)))
            {
                errorCode = Interop.Errors.ERROR_ACCESS_DENIED;
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

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
    [DllImport("kernel32.dll", EntryPoint = "CreateFileW", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern unsafe AL_SafeFileHandle CreateFilePrivate(
        string lpFileName,
        int dwDesiredAccess,
        FileShare dwShareMode,
        Interop.Kernel32.SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    private static unsafe AL_SafeFileHandle CreateFile(
        string lpFileName,
        int dwDesiredAccess,
        FileShare dwShareMode,
        Interop.Kernel32.SECURITY_ATTRIBUTES* lpSecurityAttributes,
        FileMode dwCreationDisposition,
        int dwFlagsAndAttributes,
        IntPtr hTemplateFile)
    {
        lpFileName = PathInternal.EnsureExtendedPrefixIfNeeded(lpFileName);
        return CreateFilePrivate(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
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

            if (GetFileInformationByHandleEx_Private(this, Interop.Kernel32.FileStandardInfo, &info, (uint)sizeof(Interop.Kernel32.FILE_STANDARD_INFO)))
            {
                return info.EndOfFile;
            }

            // In theory when GetFileInformationByHandleEx fails, then
            // a) IsDevice can modify last error (not true today, but can be in the future),
            // b) DeviceIoControl can succeed (last error set to ERROR_SUCCESS) but return fewer bytes than requested.
            // The error is stored and in such cases exception for the first failure is going to be thrown.
            int lastError = Marshal.GetLastWin32Error();

            if (Path is null || !PathInternal.IsDevice(Path.AsSpan()))
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

            [DllImport("kernel32.dll", EntryPoint = "GetFileInformationByHandleEx", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool GetFileInformationByHandleEx_Private(
                AL_SafeFileHandle hFile,
                int FileInformationClass,
                void* lpFileInformation,
                uint dwBufferSize);
        }
    }

    /// <summary>
    /// Options for creating/opening files with NtCreateFile.
    /// </summary>
    [Flags]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private enum CreateOptions : uint
    {
        /// <summary>
        /// File being created or opened must be a directory file. Disposition must be FILE_CREATE, FILE_OPEN,
        /// or FILE_OPEN_IF.
        /// </summary>
        /// <remarks>
        /// Can only be used with FILE_SYNCHRONOUS_IO_ALERT/NONALERT, FILE_WRITE_THROUGH, FILE_OPEN_FOR_BACKUP_INTENT,
        /// and FILE_OPEN_BY_FILE_ID flags.
        /// </remarks>
        FILE_DIRECTORY_FILE = 0x00000001,

        /// <summary>
        /// Applications that write data to the file must actually transfer the data into
        /// the file before any requested write operation is considered complete. This flag
        /// is set automatically if FILE_NO_INTERMEDIATE_BUFFERING is set.
        /// </summary>
        FILE_WRITE_THROUGH = 0x00000002,

        /// <summary>
        /// All accesses to the file are sequential.
        /// </summary>
        FILE_SEQUENTIAL_ONLY = 0x00000004,

        /// <summary>
        /// File cannot be cached in driver buffers. Cannot use with AppendData desired access.
        /// </summary>
        FILE_NO_INTERMEDIATE_BUFFERING = 0x00000008,

        /// <summary>
        /// All operations are performed synchronously. Any wait on behalf of the caller is
        /// subject to premature termination from alerts.
        /// </summary>
        /// <remarks>
        /// Cannot be used with FILE_SYNCHRONOUS_IO_NONALERT.
        /// Synchronous DesiredAccess flag is required. I/O system will maintain file position context.
        /// </remarks>
        FILE_SYNCHRONOUS_IO_ALERT = 0x00000010,

        /// <summary>
        /// All operations are performed synchronously. Waits in the system to synchronize I/O queuing
        /// and completion are not subject to alerts.
        /// </summary>
        /// <remarks>
        /// Cannot be used with FILE_SYNCHRONOUS_IO_ALERT.
        /// Synchronous DesiredAccess flag is required. I/O system will maintain file position context.
        /// </remarks>
        FILE_SYNCHRONOUS_IO_NONALERT = 0x00000020,

        /// <summary>
        /// File being created or opened must not be a directory file. Can be a data file, device,
        /// or volume.
        /// </summary>
        FILE_NON_DIRECTORY_FILE = 0x00000040,

        /// <summary>
        /// Create a tree connection for this file in order to open it over the network.
        /// </summary>
        /// <remarks>
        /// Not used by device and intermediate drivers.
        /// </remarks>
        FILE_CREATE_TREE_CONNECTION = 0x00000080,

        /// <summary>
        /// Complete the operation immediately with a success code of STATUS_OPLOCK_BREAK_IN_PROGRESS if
        /// the target file is oplocked.
        /// </summary>
        /// <remarks>
        /// Not compatible with ReserveOpfilter or OpenRequiringOplock.
        /// Not used by device and intermediate drivers.
        /// </remarks>
        FILE_COMPLETE_IF_OPLOCKED = 0x00000100,

        /// <summary>
        /// If the extended attributes on an existing file being opened indicate that the caller must
        /// understand extended attributes to properly interpret the file, fail the request.
        /// </summary>
        /// <remarks>
        /// Not used by device and intermediate drivers.
        /// </remarks>
        FILE_NO_EA_KNOWLEDGE = 0x00000200,

        // Behavior undocumented, defined in headers
        // FILE_OPEN_REMOTE_INSTANCE = 0x00000400,

        /// <summary>
        /// Accesses to the file can be random, so no sequential read-ahead operations should be performed
        /// on the file by FSDs or the system.
        /// </summary>
        FILE_RANDOM_ACCESS = 0x00000800,

        /// <summary>
        /// Delete the file when the last handle to it is passed to NtClose. Requires Delete flag in
        /// DesiredAccess parameter.
        /// </summary>
        FILE_DELETE_ON_CLOSE = 0x00001000,

        /// <summary>
        /// Open the file by reference number or object ID. The file name that is specified by the ObjectAttributes
        /// name parameter includes the 8 or 16 byte file reference number or ID for the file in the ObjectAttributes
        /// name field. The device name can optionally be prefixed.
        /// </summary>
        /// <remarks>
        /// NTFS supports both reference numbers and object IDs. 16 byte reference numbers are 8 byte numbers padded
        /// with zeros. ReFS only supports reference numbers (not object IDs). 8 byte and 16 byte reference numbers
        /// are not related. Note that as the UNICODE_STRING will contain raw byte data, it may not be a "valid" string.
        /// Not used by device and intermediate drivers.
        /// </remarks>
        /// <example>
        /// \??\C:\{8 bytes of binary FileID}
        /// \device\HardDiskVolume1\{16 bytes of binary ObjectID}
        /// {8 bytes of binary FileID}
        /// </example>
        FILE_OPEN_BY_FILE_ID = 0x00002000,

        /// <summary>
        /// The file is being opened for backup intent. Therefore, the system should check for certain access rights
        /// and grant the caller the appropriate access to the file before checking the DesiredAccess parameter
        /// against the file's security descriptor.
        /// </summary>
        /// <remarks>
        /// Not used by device and intermediate drivers.
        /// </remarks>
        FILE_OPEN_FOR_BACKUP_INTENT = 0x00004000,

        /// <summary>
        /// When creating a file, specifies that it should not inherit the compression bit from the parent directory.
        /// </summary>
        FILE_NO_COMPRESSION = 0x00008000,

        /// <summary>
        /// The file is being opened and an opportunistic lock (oplock) on the file is being requested as a single atomic
        /// operation.
        /// </summary>
        /// <remarks>
        /// The file system checks for oplocks before it performs the create operation and will fail the create with a
        /// return code of STATUS_CANNOT_BREAK_OPLOCK if the result would be to break an existing oplock.
        /// Not compatible with CompleteIfOplocked or ReserveOpFilter. Windows 7 and up.
        /// </remarks>
        FILE_OPEN_REQUIRING_OPLOCK = 0x00010000,

        /// <summary>
        /// CreateFile2 uses this flag to prevent opening a file that you don't have access to without specifying
        /// FILE_SHARE_READ. (Preventing users that can only read a file from denying access to other readers.)
        /// </summary>
        /// <remarks>
        /// Windows 7 and up.
        /// </remarks>
        FILE_DISALLOW_EXCLUSIVE = 0x00020000,

        /// <summary>
        /// The client opening the file or device is session aware and per session access is validated if necessary.
        /// </summary>
        /// <remarks>
        /// Windows 8 and up.
        /// </remarks>
        FILE_SESSION_AWARE = 0x00040000,

        /// <summary>
        /// This flag allows an application to request a filter opportunistic lock (oplock) to prevent other applications
        /// from getting share violations.
        /// </summary>
        /// <remarks>
        /// Not compatible with CompleteIfOplocked or OpenRequiringOplock.
        /// If there are already open handles, the create request will fail with STATUS_OPLOCK_NOT_GRANTED.
        /// </remarks>
        FILE_RESERVE_OPFILTER = 0x00100000,

        /// <summary>
        /// Open a file with a reparse point attribute, bypassing the normal reparse point processing.
        /// </summary>
        FILE_OPEN_REPARSE_POINT = 0x00200000,

        /// <summary>
        /// Causes files that are marked with the Offline attribute not to be recalled from remote storage.
        /// </summary>
        /// <remarks>
        /// More details can be found in Remote Storage documentation (see Basic Concepts).
        /// https://technet.microsoft.com/en-us/library/cc938459.aspx
        /// </remarks>
        FILE_OPEN_NO_RECALL = 0x00400000,

        // Behavior undocumented, defined in headers
        // FILE_OPEN_FOR_FREE_SPACE_QUERY = 0x00800000
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/ms680600(v=vs.85).aspx
    [DllImport("ntdll.dll")]
    private static extern uint RtlNtStatusToDosError(int Status);
}

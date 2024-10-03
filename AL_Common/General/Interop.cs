// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace AL_Common;

internal static class Interop
{
    internal static class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool GetFileInformationByHandleEx(AL_SafeFileHandle hFile, int FileInformationClass, void* lpFileInformation, uint dwBufferSize);

        // From FILE_INFO_BY_HANDLE_CLASS
        // Use for GetFileInformationByHandleEx
        internal const int FileStandardInfo = 1;

        internal struct FILE_STANDARD_INFO
        {
            internal long AllocationSize;
            internal long EndOfFile;
            internal uint NumberOfLinks;
            internal BOOL DeletePending;
            internal BOOL Directory;
        }

        // https://learn.microsoft.com/windows/win32/api/winioctl/ni-winioctl-fsctl_get_reparse_point
        internal const int FSCTL_GET_REPARSE_POINT = 0x000900a8;

        // https://learn.microsoft.com/windows-hardware/drivers/ddi/ntddstor/ni-ntddstor-ioctl_storage_read_capacity
        internal const int IOCTL_STORAGE_READ_CAPACITY = 0x002D5140;

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool DeviceIoControl(
            SafeHandle hDevice,
            uint dwIoControlCode,
            void* lpInBuffer,
            uint nInBufferSize,
            void* lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        // https://learn.microsoft.com/windows/win32/devio/storage-read-capacity
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct STORAGE_READ_CAPACITY
        {
            internal uint Version;
            internal uint Size;
            internal uint BlockLength;
            internal long NumberOfBlocks;
            internal long DiskLength;
        }

        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const int FORMAT_MESSAGE_FROM_HMODULE = 0x00000800;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x00002000;
        private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

        [DllImport("kernel32.dll", EntryPoint = "FormatMessageW", SetLastError = true)]
        private static extern unsafe int FormatMessage(
            int dwFlags,
            IntPtr lpSource,
            uint dwMessageId,
            int dwLanguageId,
            void* lpBuffer,
            int nSize,
            IntPtr arguments);

        /// <summary>
        ///     Returns a string message for the specified Win32 error code.
        /// </summary>
        internal static string GetMessage(int errorCode) =>
            GetMessage(errorCode, IntPtr.Zero);

        internal static unsafe string GetMessage(int errorCode, IntPtr moduleHandle)
        {
            int flags = FORMAT_MESSAGE_IGNORE_INSERTS | FORMAT_MESSAGE_FROM_SYSTEM |
                        FORMAT_MESSAGE_ARGUMENT_ARRAY;
            if (moduleHandle != IntPtr.Zero)
            {
                flags |= FORMAT_MESSAGE_FROM_HMODULE;
            }

            // First try to format the message into the stack based buffer.  Most error messages willl fit.
            Span<char> stackBuffer = stackalloc char[256]; // arbitrary stack limit
            fixed (char* bufferPtr = stackBuffer)
            {
                int length = FormatMessage(flags, moduleHandle, unchecked((uint)errorCode), 0, bufferPtr,
                    stackBuffer.Length, IntPtr.Zero);
                if (length > 0)
                {
                    return GetAndTrimString(stackBuffer.Slice(0, length));
                }
            }

            // We got back an error.  If the error indicated that there wasn't enough room to store
            // the error message, then call FormatMessage again, but this time rather than passing in
            // a buffer, have the method allocate one, which we then need to free.
            if (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER)
            {
                IntPtr nativeMsgPtr = default;
                try
                {
                    int length = FormatMessage(flags | FORMAT_MESSAGE_ALLOCATE_BUFFER, moduleHandle,
                        unchecked((uint)errorCode), 0, &nativeMsgPtr, 0, IntPtr.Zero);
                    if (length > 0)
                    {
                        return GetAndTrimString(new ReadOnlySpan<char>((char*)nativeMsgPtr, length));
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(nativeMsgPtr);
                }
            }

            // Couldn't get a message, so manufacture one.
            return $"Unknown error (0x{errorCode:x})";
        }

        private static string GetAndTrimString(ReadOnlySpan<char> buffer)
        {
            int length = buffer.Length;
            while (length > 0 && buffer[length - 1] <= 32)
            {
                length--; // trim off spaces and non-printable ASCII chars at the end of the resource
            }
            return buffer.Slice(0, length).ToString();
        }

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
    internal static class Errors
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

    internal static class NtDll
    {
        [DllImport("ntdll.dll")]
        internal static extern unsafe int NtQueryInformationFile(
            AL_SafeFileHandle FileHandle,
            out IO_STATUS_BLOCK IoStatusBlock,
            void* FileInformation,
            uint Length,
            uint FileInformationClass);

        internal const uint FileModeInformation = 16;

        internal const int STATUS_INVALID_HANDLE = unchecked((int)0xC0000008);

        // https://msdn.microsoft.com/en-us/library/windows/hardware/ff550671.aspx
        [StructLayout(LayoutKind.Sequential)]
        public struct IO_STATUS_BLOCK
        {
            /// <summary>
            /// Status
            /// </summary>
            public IO_STATUS Status;

            /// <summary>
            /// Request dependent value.
            /// </summary>
            public IntPtr Information;

            // This isn't an actual Windows type, it is a union within IO_STATUS_BLOCK. We *have* to separate it out as
            // the size of IntPtr varies by architecture and we can't specify the size at compile time to offset the
            // Information pointer in the status block.
            [StructLayout(LayoutKind.Explicit)]
            public struct IO_STATUS
            {
                /// <summary>
                /// The completion status, either STATUS_SUCCESS if the operation was completed successfully or
                /// some other informational, warning, or error status.
                /// </summary>
                [FieldOffset(0)]
                public uint Status;

                /// <summary>
                /// Reserved for internal use.
                /// </summary>
                [FieldOffset(0)]
                public IntPtr Pointer;
            }
        }

        // https://msdn.microsoft.com/en-us/library/bb432380.aspx
        // https://msdn.microsoft.com/en-us/library/windows/hardware/ff566424.aspx
        [DllImport("ntdll.dll")]
        private static extern unsafe uint NtCreateFile(
            IntPtr* FileHandle,
            DesiredAccess DesiredAccess,
            OBJECT_ATTRIBUTES* ObjectAttributes,
            IO_STATUS_BLOCK* IoStatusBlock,
            long* AllocationSize,
            FileAttributes FileAttributes,
            FileShare ShareAccess,
            CreateDisposition CreateDisposition,
            CreateOptions CreateOptions,
            void* EaBuffer,
            uint EaLength);

        internal static unsafe (uint status, IntPtr handle) CreateFile(
            ReadOnlySpan<char> path,
            IntPtr rootDirectory,
            CreateDisposition createDisposition,
            DesiredAccess desiredAccess = DesiredAccess.FILE_GENERIC_READ | DesiredAccess.SYNCHRONIZE,
            FileShare shareAccess = FileShare.ReadWrite | FileShare.Delete,
            FileAttributes fileAttributes = 0,
            CreateOptions createOptions = CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT,
            ObjectAttributes objectAttributes = ObjectAttributes.OBJ_CASE_INSENSITIVE,
            void* eaBuffer = null,
            uint eaLength = 0,
            long* preallocationSize = null,
            SECURITY_QUALITY_OF_SERVICE* securityQualityOfService = null)
        {
            fixed (char* c = &MemoryMarshal.GetReference(path))
            {
                UNICODE_STRING name = new UNICODE_STRING
                {
                    Length = checked((ushort)(path.Length * sizeof(char))),
                    MaximumLength = checked((ushort)(path.Length * sizeof(char))),
                    Buffer = (IntPtr)c
                };

                OBJECT_ATTRIBUTES attributes = new OBJECT_ATTRIBUTES(
                    &name,
                    objectAttributes,
                    rootDirectory,
                    securityQualityOfService);

                IntPtr handle;
                IO_STATUS_BLOCK statusBlock;
                uint status = NtCreateFile(
                    &handle,
                    desiredAccess,
                    &attributes,
                    &statusBlock,
                    AllocationSize: preallocationSize,
                    fileAttributes,
                    shareAccess,
                    createDisposition,
                    createOptions,
                    eaBuffer,
                    eaLength);

                return (status, handle);
            }
        }

        internal static unsafe (uint status, IntPtr handle) NtCreateFile(ReadOnlySpan<char> path, FileMode mode, FileAccess access, FileShare share, FileOptions options, long preallocationSize)
        {
            // For mitigating local elevation of privilege attack through named pipes
            // make sure we always call NtCreateFile with SECURITY_ANONYMOUS so that the
            // named pipe server can't impersonate a high privileged client security context
            SECURITY_QUALITY_OF_SERVICE securityQualityOfService = new SECURITY_QUALITY_OF_SERVICE(
                ImpersonationLevel.Anonymous, // SECURITY_ANONYMOUS
                ContextTrackingMode.Static,
                effectiveOnly: false);

            return CreateFile(
                path: path,
                rootDirectory: IntPtr.Zero,
                createDisposition: GetCreateDisposition(mode),
                desiredAccess: GetDesiredAccess(access, mode, options),
                shareAccess: GetShareAccess(share),
                fileAttributes: GetFileAttributes(options),
                createOptions: GetCreateOptions(options),
                objectAttributes: GetObjectAttributes(share),
                preallocationSize: &preallocationSize,
                securityQualityOfService: &securityQualityOfService);
        }

        private static CreateDisposition GetCreateDisposition(FileMode mode)
        {
            switch (mode)
            {
                case FileMode.CreateNew:
                    return CreateDisposition.FILE_CREATE;
                case FileMode.Create:
                    return CreateDisposition.FILE_SUPERSEDE;
                case FileMode.OpenOrCreate:
                case FileMode.Append: // has extra handling in GetDesiredAccess
                    return CreateDisposition.FILE_OPEN_IF;
                case FileMode.Truncate:
                    return CreateDisposition.FILE_OVERWRITE;
                default:
                    Debug.Assert(mode == FileMode.Open); // the enum value is validated in FileStream ctor
                    return CreateDisposition.FILE_OPEN;
            }
        }

        private static DesiredAccess GetDesiredAccess(FileAccess access, FileMode fileMode, FileOptions options)
        {
            DesiredAccess result = DesiredAccess.FILE_READ_ATTRIBUTES | DesiredAccess.SYNCHRONIZE; // default values used by CreateFileW

            if ((access & FileAccess.Read) != 0)
            {
                result |= DesiredAccess.FILE_GENERIC_READ;
            }
            if ((access & FileAccess.Write) != 0)
            {
                result |= DesiredAccess.FILE_GENERIC_WRITE;
            }
            if (fileMode == FileMode.Append)
            {
                result |= DesiredAccess.FILE_APPEND_DATA;
            }
            if ((options & FileOptions.DeleteOnClose) != 0)
            {
                result |= DesiredAccess.DELETE; // required by FILE_DELETE_ON_CLOSE
            }

            return result;
        }

        private static FileShare GetShareAccess(FileShare share)
            => share & ~FileShare.Inheritable; // FileShare.Inheritable is handled in GetObjectAttributes

        private static FileAttributes GetFileAttributes(FileOptions options)
            => (options & FileOptions.Encrypted) != 0 ? FileAttributes.Encrypted : 0;

        // FileOptions.Encrypted is handled in GetFileAttributes
        private static CreateOptions GetCreateOptions(FileOptions options)
        {
            // Every directory is just a directory FILE.
            // FileStream does not allow for opening directories on purpose.
            // FILE_NON_DIRECTORY_FILE is used to ensure that
            CreateOptions result = CreateOptions.FILE_NON_DIRECTORY_FILE;

            if ((options & FileOptions.WriteThrough) != 0)
            {
                result |= CreateOptions.FILE_WRITE_THROUGH;
            }
            if ((options & FileOptions.RandomAccess) != 0)
            {
                result |= CreateOptions.FILE_RANDOM_ACCESS;
            }
            if ((options & FileOptions.SequentialScan) != 0)
            {
                result |= CreateOptions.FILE_SEQUENTIAL_ONLY;
            }
            if ((options & FileOptions.DeleteOnClose) != 0)
            {
                result |= CreateOptions.FILE_DELETE_ON_CLOSE; // has extra handling in GetDesiredAccess
            }
            if ((options & FileOptions.Asynchronous) == 0)
            {
                // it's async by default, so we need to disable it when async was not requested
                result |= CreateOptions.FILE_SYNCHRONOUS_IO_NONALERT; // has extra handling in GetDesiredAccess
            }
            if (((int)options & 0x20000000) != 0) // NoBuffering
            {
                result |= CreateOptions.FILE_NO_INTERMEDIATE_BUFFERING;
            }

            return result;
        }

        private static ObjectAttributes GetObjectAttributes(FileShare share)
            => ObjectAttributes.OBJ_CASE_INSENSITIVE | // default value used by CreateFileW
                ((share & FileShare.Inheritable) != 0 ? ObjectAttributes.OBJ_INHERIT : 0);

        /// <summary>
        /// File creation disposition when calling directly to NT APIs.
        /// </summary>
        public enum CreateDisposition : uint
        {
            /// <summary>
            /// Default. Replace or create. Deletes existing file instead of overwriting.
            /// </summary>
            /// <remarks>
            /// As this potentially deletes it requires that DesiredAccess must include Delete.
            /// This has no equivalent in CreateFile.
            /// </remarks>
            FILE_SUPERSEDE = 0,

            /// <summary>
            /// Open if exists or fail if doesn't exist. Equivalent to OPEN_EXISTING or
            /// <see cref="FileMode.Open"/>.
            /// </summary>
            /// <remarks>
            /// TruncateExisting also uses Open and then manually truncates the file
            /// by calling NtSetInformationFile with FileAllocationInformation and an
            /// allocation size of 0.
            /// </remarks>
            FILE_OPEN = 1,

            /// <summary>
            /// Create if doesn't exist or fail if does exist. Equivalent to CREATE_NEW
            /// or <see cref="FileMode.CreateNew"/>.
            /// </summary>
            FILE_CREATE = 2,

            /// <summary>
            /// Open if exists or create if doesn't exist. Equivalent to OPEN_ALWAYS or
            /// <see cref="FileMode.OpenOrCreate"/>.
            /// </summary>
            FILE_OPEN_IF = 3,

            /// <summary>
            /// Open and overwrite if exists or fail if doesn't exist. Equivalent to
            /// TRUNCATE_EXISTING or <see cref="FileMode.Truncate"/>.
            /// </summary>
            FILE_OVERWRITE = 4,

            /// <summary>
            /// Open and overwrite if exists or create if doesn't exist. Equivalent to
            /// CREATE_ALWAYS or <see cref="FileMode.Create"/>.
            /// </summary>
            FILE_OVERWRITE_IF = 5
        }

        /// <summary>
        /// Options for creating/opening files with NtCreateFile.
        /// </summary>
        public enum CreateOptions : uint
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
            FILE_OPEN_NO_RECALL = 0x00400000

            // Behavior undocumented, defined in headers
            // FILE_OPEN_FOR_FREE_SPACE_QUERY = 0x00800000
        }

        /// <summary>
        /// System.IO.FileAccess looks up these values when creating handles
        /// </summary>
        /// <remarks>
        /// File Security and Access Rights
        /// https://msdn.microsoft.com/en-us/library/windows/desktop/aa364399.aspx
        /// </remarks>
        [Flags]
        public enum DesiredAccess : uint
        {
            // File Access Rights Constants
            // https://msdn.microsoft.com/en-us/library/windows/desktop/gg258116.aspx

            /// <summary>
            /// For a file, the right to read data from the file.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="FILE_LIST_DIRECTORY"/>.
            /// </remarks>
            FILE_READ_DATA = 0x0001,

            /// <summary>
            /// For a directory, the right to list the contents.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="FILE_READ_DATA"/>.
            /// </remarks>
            FILE_LIST_DIRECTORY = 0x0001,

            /// <summary>
            /// For a file, the right to write data to the file.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="FILE_ADD_FILE"/>.
            /// </remarks>
            FILE_WRITE_DATA = 0x0002,

            /// <summary>
            /// For a directory, the right to create a file in a directory.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="FILE_WRITE_DATA"/>.
            /// </remarks>
            FILE_ADD_FILE = 0x0002,

            /// <summary>
            /// For a file, the right to append data to a file. <see cref="FILE_WRITE_DATA"/> is needed
            /// to overwrite existing data.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="FILE_ADD_SUBDIRECTORY"/>.
            /// </remarks>
            FILE_APPEND_DATA = 0x0004,

            /// <summary>
            /// For a directory, the right to create a subdirectory.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="FILE_APPEND_DATA"/>.
            /// </remarks>
            FILE_ADD_SUBDIRECTORY = 0x0004,

            /// <summary>
            /// For a named pipe, the right to create a pipe instance.
            /// </summary>
            FILE_CREATE_PIPE_INSTANCE = 0x0004,

            /// <summary>
            /// The right to read extended attributes.
            /// </summary>
            FILE_READ_EA = 0x0008,

            /// <summary>
            /// The right to write extended attributes.
            /// </summary>
            FILE_WRITE_EA = 0x0010,

            /// <summary>
            /// The right to execute the file.
            /// </summary>
            /// <remarks>
            /// Directory version of this flag is <see cref="FILE_TRAVERSE"/>.
            /// </remarks>
            FILE_EXECUTE = 0x0020,

            /// <summary>
            /// For a directory, the right to traverse the directory.
            /// </summary>
            /// <remarks>
            /// File version of this flag is <see cref="FILE_EXECUTE"/>.
            /// </remarks>
            FILE_TRAVERSE = 0x0020,

            /// <summary>
            /// For a directory, the right to delete a directory and all
            /// the files it contains, including read-only files.
            /// </summary>
            FILE_DELETE_CHILD = 0x0040,

            /// <summary>
            /// The right to read attributes.
            /// </summary>
            FILE_READ_ATTRIBUTES = 0x0080,

            /// <summary>
            /// The right to write attributes.
            /// </summary>
            FILE_WRITE_ATTRIBUTES = 0x0100,

            /// <summary>
            /// All standard and specific rights. [FILE_ALL_ACCESS]
            /// </summary>
            FILE_ALL_ACCESS = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | 0x1FF,

            /// <summary>
            /// The right to delete the object.
            /// </summary>
            DELETE = 0x00010000,

            /// <summary>
            /// The right to read the information in the object's security descriptor.
            /// Doesn't include system access control list info (SACL).
            /// </summary>
            READ_CONTROL = 0x00020000,

            /// <summary>
            /// The right to modify the discretionary access control list (DACL) in the
            /// object's security descriptor.
            /// </summary>
            WRITE_DAC = 0x00040000,

            /// <summary>
            /// The right to change the owner in the object's security descriptor.
            /// </summary>
            WRITE_OWNER = 0x00080000,

            /// <summary>
            /// The right to use the object for synchronization. Enables a thread to wait until the object
            /// is in the signaled state. This is required if opening a synchronous handle.
            /// </summary>
            SYNCHRONIZE = 0x00100000,

            /// <summary>
            /// Same as READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_READ = READ_CONTROL,

            /// <summary>
            /// Same as READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_WRITE = READ_CONTROL,

            /// <summary>
            /// Same as READ_CONTROL.
            /// </summary>
            STANDARD_RIGHTS_EXECUTE = READ_CONTROL,

            /// <summary>
            /// Maps internally to <see cref="FILE_READ_ATTRIBUTES"/> | <see cref="FILE_READ_DATA"/> | <see cref="FILE_READ_EA"/>
            /// | <see cref="STANDARD_RIGHTS_READ"/> | <see cref="SYNCHRONIZE"/>.
            /// (For directories, <see cref="FILE_READ_ATTRIBUTES"/> | <see cref="FILE_LIST_DIRECTORY"/> | <see cref="FILE_READ_EA"/>
            /// | <see cref="STANDARD_RIGHTS_READ"/> | <see cref="SYNCHRONIZE"/>.)
            /// </summary>
            FILE_GENERIC_READ = 0x80000000, // GENERIC_READ

            /// <summary>
            /// Maps internally to <see cref="FILE_APPEND_DATA"/> | <see cref="FILE_WRITE_ATTRIBUTES"/> | <see cref="FILE_WRITE_DATA"/>
            /// | <see cref="FILE_WRITE_EA"/> | <see cref="STANDARD_RIGHTS_READ"/> | <see cref="SYNCHRONIZE"/>.
            /// (For directories, <see cref="FILE_ADD_SUBDIRECTORY"/> | <see cref="FILE_WRITE_ATTRIBUTES"/> | <see cref="FILE_ADD_FILE"/> AddFile
            /// | <see cref="FILE_WRITE_EA"/> | <see cref="STANDARD_RIGHTS_READ"/> | <see cref="SYNCHRONIZE"/>.)
            /// </summary>
            FILE_GENERIC_WRITE = 0x40000000, // GENERIC WRITE

            /// <summary>
            /// Maps internally to <see cref="FILE_EXECUTE"/> | <see cref="FILE_READ_ATTRIBUTES"/> | <see cref="STANDARD_RIGHTS_EXECUTE"/>
            /// | <see cref="SYNCHRONIZE"/>.
            /// (For directories, <see cref="FILE_DELETE_CHILD"/> | <see cref="FILE_READ_ATTRIBUTES"/> | <see cref="STANDARD_RIGHTS_EXECUTE"/>
            /// | <see cref="SYNCHRONIZE"/>.)
            /// </summary>
            FILE_GENERIC_EXECUTE = 0x20000000 // GENERIC_EXECUTE
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms680600(v=vs.85).aspx
        [DllImport("ntdll.dll")]
        public static extern uint RtlNtStatusToDosError(int Status);
    }

    internal static class StatusOptions
    {
        // See the NT_SUCCESS macro in the Windows SDK, and
        // https://learn.microsoft.com/windows-hardware/drivers/kernel/using-ntstatus-values
        internal static bool NT_SUCCESS(uint ntStatus) => (int)ntStatus >= 0;

        // Error codes from ntstatus.h
        internal const uint STATUS_SUCCESS = 0x00000000;
        internal const uint STATUS_SOME_NOT_MAPPED = 0x00000107;
        internal const uint STATUS_NO_MORE_FILES = 0x80000006;
        internal const uint STATUS_INVALID_PARAMETER = 0xC000000D;
        internal const uint STATUS_FILE_NOT_FOUND = 0xC000000F;
        internal const uint STATUS_NO_MEMORY = 0xC0000017;
        internal const uint STATUS_ACCESS_DENIED = 0xC0000022;
        internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xC0000034;
        internal const uint STATUS_QUOTA_EXCEEDED = 0xC0000044;
        internal const uint STATUS_ACCOUNT_RESTRICTION = 0xC000006E;
        internal const uint STATUS_NONE_MAPPED = 0xC0000073;
        internal const uint STATUS_INSUFFICIENT_RESOURCES = 0xC000009A;
        internal const uint STATUS_DISK_FULL = 0xC000007F;
        internal const uint STATUS_FILE_TOO_LARGE = 0xC0000904;
    }

    /// <summary>
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/hardware/ff557749.aspx">OBJECT_ATTRIBUTES</a> structure.
    /// The OBJECT_ATTRIBUTES structure specifies attributes that can be applied to objects or object handles by routines
    /// that create objects and/or return handles to objects.
    /// </summary>
    internal unsafe struct OBJECT_ATTRIBUTES
    {
        public uint Length;

        /// <summary>
        /// Optional handle to root object directory for the given ObjectName.
        /// Can be a file system directory or object manager directory.
        /// </summary>
        public IntPtr RootDirectory;

        /// <summary>
        /// Name of the object. Must be fully qualified if RootDirectory isn't set.
        /// Otherwise is relative to RootDirectory.
        /// </summary>
        public UNICODE_STRING* ObjectName;

        public ObjectAttributes Attributes;

        /// <summary>
        /// If null, object will receive default security settings.
        /// </summary>
        public void* SecurityDescriptor;

        /// <summary>
        /// Optional quality of service to be applied to the object. Used to indicate
        /// security impersonation level and context tracking mode (dynamic or static).
        /// </summary>
        public SECURITY_QUALITY_OF_SERVICE* SecurityQualityOfService;

        /// <summary>
        /// Equivalent of InitializeObjectAttributes macro with the exception that you can directly set SQOS.
        /// </summary>
        public unsafe OBJECT_ATTRIBUTES(UNICODE_STRING* objectName, ObjectAttributes attributes, IntPtr rootDirectory, SECURITY_QUALITY_OF_SERVICE* securityQualityOfService = null)
        {
            Length = (uint)sizeof(OBJECT_ATTRIBUTES);
            RootDirectory = rootDirectory;
            ObjectName = objectName;
            Attributes = attributes;
            SecurityDescriptor = null;
            SecurityQualityOfService = securityQualityOfService;
        }
    }

    [Flags]
    public enum ObjectAttributes : uint
    {
        // https://msdn.microsoft.com/en-us/library/windows/hardware/ff564586.aspx
        // https://msdn.microsoft.com/en-us/library/windows/hardware/ff547804.aspx

        /// <summary>
        /// This handle can be inherited by child processes of the current process.
        /// </summary>
        OBJ_INHERIT = 0x00000002,

        /// <summary>
        /// This flag only applies to objects that are named within the object manager.
        /// By default, such objects are deleted when all open handles to them are closed.
        /// If this flag is specified, the object is not deleted when all open handles are closed.
        /// </summary>
        OBJ_PERMANENT = 0x00000010,

        /// <summary>
        /// Only a single handle can be open for this object.
        /// </summary>
        OBJ_EXCLUSIVE = 0x00000020,

        /// <summary>
        /// Lookups for this object should be case insensitive.
        /// </summary>
        OBJ_CASE_INSENSITIVE = 0x00000040,

        /// <summary>
        /// Create on existing object should open, not fail with STATUS_OBJECT_NAME_COLLISION.
        /// </summary>
        OBJ_OPENIF = 0x00000080,

        /// <summary>
        /// Open the symbolic link, not its target.
        /// </summary>
        OBJ_OPENLINK = 0x00000100,

        // Only accessible from kernel mode
        // OBJ_KERNEL_HANDLE

        // Access checks enforced, even in kernel mode
        // OBJ_FORCE_ACCESS_CHECK
        // OBJ_VALID_ATTRIBUTES = 0x000001F2
    }

    /// <summary>
    /// <a href="https://learn.microsoft.com/windows/win32/api/winnt/ns-winnt-security_quality_of_service">SECURITY_QUALITY_OF_SERVICE</a> structure.
    ///  Used to support client impersonation. Client specifies this to a server to allow
    ///  it to impersonate the client.
    /// </summary>
    internal unsafe struct SECURITY_QUALITY_OF_SERVICE
    {
        public uint Length;
        public ImpersonationLevel ImpersonationLevel;
        public ContextTrackingMode ContextTrackingMode;
        public BOOLEAN EffectiveOnly;

        public unsafe SECURITY_QUALITY_OF_SERVICE(ImpersonationLevel impersonationLevel, ContextTrackingMode contextTrackingMode, bool effectiveOnly)
        {
            Length = (uint)sizeof(SECURITY_QUALITY_OF_SERVICE);
            ImpersonationLevel = impersonationLevel;
            ContextTrackingMode = contextTrackingMode;
            EffectiveOnly = effectiveOnly ? BOOLEAN.TRUE : BOOLEAN.FALSE;
        }
    }

    /// <summary>
    /// <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa379572.aspx">SECURITY_IMPERSONATION_LEVEL</a> enumeration values.
    ///  [SECURITY_IMPERSONATION_LEVEL]
    /// </summary>
    public enum ImpersonationLevel : uint
    {
        /// <summary>
        ///  The server process cannot obtain identification information about the client and cannot impersonate the client.
        ///  [SecurityAnonymous]
        /// </summary>
        Anonymous,

        /// <summary>
        ///  The server process can obtain identification information about the client, but cannot impersonate the client.
        ///  [SecurityIdentification]
        /// </summary>
        Identification,

        /// <summary>
        ///  The server process can impersonate the client's security context on it's local system.
        ///  [SecurityImpersonation]
        /// </summary>
        Impersonation,

        /// <summary>
        ///  The server process can impersonate the client's security context on remote systems.
        ///  [SecurityDelegation]
        /// </summary>
        Delegation
    }

    /// <summary>
    /// <a href="https://msdn.microsoft.com/en-us/library/cc234317.aspx">SECURITY_CONTEXT_TRACKING_MODE</a>
    /// </summary>
    public enum ContextTrackingMode : byte
    {
        /// <summary>
        ///  The server is given a snapshot of the client's security context.
        ///  [SECURITY_STATIC_TRACKING]
        /// </summary>
        Static = 0x00,

        /// <summary>
        ///  The server is continually updated with changes.
        ///  [SECURITY_DYNAMIC_TRACKING]
        /// </summary>
        Dynamic = 0x01
    }

    // https://msdn.microsoft.com/en-us/library/windows/desktop/aa380518.aspx
    // https://msdn.microsoft.com/en-us/library/windows/hardware/ff564879.aspx
    [StructLayout(LayoutKind.Sequential)]
    internal struct UNICODE_STRING
    {
        /// <summary>
        /// Length in bytes, not including the null terminator, if any.
        /// </summary>
        internal ushort Length;

        /// <summary>
        /// Max size of the buffer in bytes
        /// </summary>
        internal ushort MaximumLength;

        internal IntPtr Buffer;
    }
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
internal enum BOOL : int
{
    FALSE = 0,
    TRUE = 1,
}

/// <summary>
/// Blittable version of Windows BOOLEAN type. It is convenient in situations where
/// manual marshalling is required, or to avoid overhead of regular bool marshalling.
/// </summary>
/// <remarks>
/// Some Windows APIs return arbitrary integer values although the return type is defined
/// as BOOLEAN. It is best to never compare BOOLEAN to TRUE. Always use bResult != BOOLEAN.FALSE
/// or bResult == BOOLEAN.FALSE .
/// </remarks>
internal enum BOOLEAN : byte
{
    FALSE = 0,
    TRUE = 1,
}

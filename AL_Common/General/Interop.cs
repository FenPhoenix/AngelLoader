// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using JetBrains.Annotations;

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

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        internal struct FILE_STANDARD_INFO
        {
            internal long AllocationSize;
            internal long EndOfFile;
            internal uint NumberOfLinks;
            internal BOOL DeletePending;
            internal BOOL Directory;
        }

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
        internal struct STORAGE_READ_CAPACITY
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

        private static unsafe string GetMessage(int errorCode, IntPtr moduleHandle)
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
                    return GetAndTrimString(stackBuffer[..length]);
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
            return buffer[..length].ToString();
        }

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
        internal const int ERROR_FILE_NOT_FOUND = 0x2;
        internal const int ERROR_PATH_NOT_FOUND = 0x3;
        internal const int ERROR_ACCESS_DENIED = 0x5;
        internal const int ERROR_INVALID_HANDLE = 0x6;
        internal const int ERROR_SHARING_VIOLATION = 0x20;
        internal const int ERROR_HANDLE_EOF = 0x26;
        internal const int ERROR_FILE_EXISTS = 0x50;
        internal const int ERROR_INVALID_PARAMETER = 0x57;
        internal const int ERROR_BROKEN_PIPE = 0x6D;
        internal const int ERROR_ALREADY_EXISTS = 0xB7;
        internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE;
        internal const int ERROR_PIPE_NOT_CONNECTED = 0xE9;
        internal const int ERROR_OPERATION_ABORTED = 0x3E3;
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

        /// <summary>
        /// Options for creating/opening files with NtCreateFile.
        /// </summary>
        [Flags]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
            FILE_OPEN_NO_RECALL = 0x00400000,

            // Behavior undocumented, defined in headers
            // FILE_OPEN_FOR_FREE_SPACE_QUERY = 0x00800000
        }

        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms680600(v=vs.85).aspx
        [DllImport("ntdll.dll")]
        public static extern uint RtlNtStatusToDosError(int Status);
    }

    internal static class StatusOptions
    {
        // See the NT_SUCCESS macro in the Windows SDK, and
        // https://learn.microsoft.com/windows-hardware/drivers/kernel/using-ntstatus-values

        // Error codes from ntstatus.h
        internal const uint STATUS_SUCCESS = 0x00000000;
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
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
// ReSharper disable once EnumUnderlyingTypeIsInt
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
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal enum BOOLEAN : byte
{
    FALSE = 0,
    TRUE = 1,
}

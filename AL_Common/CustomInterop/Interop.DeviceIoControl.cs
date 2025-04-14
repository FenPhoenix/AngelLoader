// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        // https://learn.microsoft.com/windows/win32/api/winioctl/ni-winioctl-fsctl_get_reparse_point
        internal const int FSCTL_GET_REPARSE_POINT = 0x000900a8;

        // https://learn.microsoft.com/windows-hardware/drivers/ddi/ntddstor/ni-ntddstor-ioctl_storage_read_capacity
        internal const int IOCTL_STORAGE_READ_CAPACITY = 0x002D5140;

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern unsafe bool DeviceIoControl(
            SafeHandle hDevice,
            uint dwIoControlCode,
            void* lpInBuffer,
            uint nInBufferSize,
            void* lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            nint lpOverlapped);
    }
}

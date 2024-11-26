//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
#if ENABLE_UNUSED
        [System.Runtime.InteropServices.DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern unsafe bool GetFileInformationByHandleEx(
            Microsoft.Win32.SafeHandles.SafeFileHandle hFile,
            int FileInformationClass,
            void* lpFileInformation,
            uint dwBufferSize);
#endif
    }
}

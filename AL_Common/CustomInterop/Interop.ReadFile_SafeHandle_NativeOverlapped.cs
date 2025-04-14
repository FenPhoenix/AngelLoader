//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Threading;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
#if ENABLE_UNUSED
        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern unsafe int ReadFile(
            SafeHandle handle,
            byte* bytes,
            int numBytesToRead,
            nint numBytesRead_mustBeZero,
            NativeOverlapped* overlapped);
#endif

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern unsafe int ReadFile(
            SafeHandle handle,
            byte* bytes,
            int numBytesToRead,
            out int numBytesRead,
            NativeOverlapped* overlapped);
    }
}

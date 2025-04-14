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
        internal static extern unsafe int ReadFile(
            System.Runtime.InteropServices.SafeHandle handle,
            byte* bytes,
            int numBytesToRead,
            out int numBytesRead,
            nint mustBeZero);
#endif
    }
}

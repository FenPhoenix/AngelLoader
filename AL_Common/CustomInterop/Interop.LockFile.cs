// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LockFile(AL_SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnlockFile(AL_SafeFileHandle handle, int offsetLow, int offsetHigh, int countLow, int countHigh);
    }
}

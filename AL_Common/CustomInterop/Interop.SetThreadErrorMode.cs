// @NET5: Remove all references to pre-Win10 stuff, because .NET modern can only run on 10+

//#define ENABLE_UNUSED

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.InteropServices;

namespace AL_Common;

internal static partial class Interop
{
    internal static partial class Kernel32
    {
        // RTM versions of Win7 and Windows Server 2008 R2
        private static readonly Version ThreadErrorModeMinOsVersion = new(6, 1, 7600);

        [LibraryImport("kernel32.dll", EntryPoint = "SetErrorMode", SetLastError = false)]
        private static partial uint SetErrorMode_VistaAndOlder(uint newMode);

        [LibraryImport("kernel32.dll", EntryPoint = "SetThreadErrorMode", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetErrorMode_Win7AndNewer(uint newMode, out uint oldMode);

        // this method uses the thread-safe version of SetErrorMode on Windows 7 / Windows Server 2008 R2 operating systems.
        // Fen: Ported success value return from modern .NET
        internal static bool SetErrorMode(uint newMode, out uint oldMode)
        {
            if (Environment.OSVersion.Version >= ThreadErrorModeMinOsVersion)
            {
                return SetErrorMode_Win7AndNewer(newMode, out oldMode);
            }

            oldMode = SetErrorMode_VistaAndOlder(newMode);
            return true;
        }

        internal const int SEM_FAILCRITICALERRORS = 0x00000001;
#if ENABLE_UNUSED
        internal const int SEM_NOOPENFILEERRORBOX = 0x00008000;
#endif
    }
}
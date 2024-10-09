// From Framework source

using System;
using System.Runtime.InteropServices;

namespace AL_Common;

[StructLayout(LayoutKind.Sequential)]
public struct FILE_TIME
{
    public FILE_TIME(long fileTime)
    {
        ftTimeLow = (uint)fileTime;
        ftTimeHigh = (uint)(fileTime >> 32);
    }

    public readonly long ToTicks() => ((long)ftTimeHigh << 32) + ftTimeLow;

    internal uint ftTimeLow;
    internal uint ftTimeHigh;
}

internal static class Win32Native
{
    internal const int ERROR_FILE_NOT_FOUND = 0x2;
    internal const int ERROR_PATH_NOT_FOUND = 0x3;
    internal const int ERROR_ACCESS_DENIED = 0x5;
    internal const int ERROR_INVALID_NAME = 0x7B;
    internal const int ERROR_BAD_PATHNAME = 0xA1;
    internal const int ERROR_ALREADY_EXISTS = 0xB7;
    internal const int ERROR_FILENAME_EXCED_RANGE = 0xCE;  // filename too long.
    internal const int ERROR_INVALID_DRIVE = 0xf;
    internal const int ERROR_INVALID_PARAMETER = 0x57;
    internal const int ERROR_SHARING_VIOLATION = 0x20;
    internal const int ERROR_FILE_EXISTS = 0x50;
    internal const int ERROR_OPERATION_ABORTED = 0x3E3;  // 995; For IO Cancellation

    // RTM versions of Win7 and Windows Server 2008 R2
    private static readonly Version ThreadErrorModeMinOsVersion = new(6, 1, 7600);

    [DllImport("kernel32.dll", SetLastError = false, EntryPoint = "SetErrorMode", ExactSpelling = true)]
    private static extern uint SetErrorMode_VistaAndOlder(uint newMode);

    [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "SetThreadErrorMode")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetErrorMode_Win7AndNewer(uint newMode, out uint oldMode);

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

    // From WinBase.h
    internal const int SEM_FAILCRITICALERRORS = 1;

    // Security Quality of Service flags
    internal const int SECURITY_ANONYMOUS = 0;
    internal const int SECURITY_SQOS_PRESENT = 0x00100000;

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern unsafe bool SetFileTime(
        AL_SafeFileHandle hFile,
        FILE_TIME* creationTime,
        FILE_TIME* lastAccessTime,
        FILE_TIME* lastWriteTime);
}

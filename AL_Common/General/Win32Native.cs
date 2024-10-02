// From Framework source

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

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

    private const string KERNEL32 = "kernel32.dll";

    [DllImport(KERNEL32, SetLastError = false, EntryPoint = "SetErrorMode", ExactSpelling = true)]
    [ResourceExposure(ResourceScope.Process)]
    private static extern int SetErrorMode_VistaAndOlder(int newMode);

    [DllImport(KERNEL32, SetLastError = true, EntryPoint = "SetThreadErrorMode")]
    [ResourceExposure(ResourceScope.None)]
    private static extern bool SetErrorMode_Win7AndNewer(int newMode, out int oldMode);

    // this method uses the thread-safe version of SetErrorMode on Windows 7 / Windows Server 2008 R2 operating systems.
    // 
    [ResourceExposure(ResourceScope.Process)]
    [ResourceConsumption(ResourceScope.Process)]
    internal static int SetErrorMode(int newMode)
    {
        if (Environment.OSVersion.Version >= ThreadErrorModeMinOsVersion)
        {
            SetErrorMode_Win7AndNewer(newMode, out int oldMode);
            return oldMode;
        }
        return SetErrorMode_VistaAndOlder(newMode);
    }

    // From WinBase.h
    internal const int SEM_FAILCRITICALERRORS = 1;

    internal enum SECURITY_IMPERSONATION_LEVEL
    {
        Anonymous = 0,
        Identification = 1,
        Impersonation = 2,
        Delegation = 3,
    }

    // Security Quality of Service flags
    internal const int SECURITY_ANONYMOUS = ((int)SECURITY_IMPERSONATION_LEVEL.Anonymous << 16);
    internal const int SECURITY_SQOS_PRESENT = 0x00100000;

    [DllImport(KERNEL32, SetLastError = true)]
    [ResourceExposure(ResourceScope.None)]
    internal static extern unsafe bool SetFileTime(AL_SafeFileHandle hFile, FILE_TIME* creationTime,
        FILE_TIME* lastAccessTime, FILE_TIME* lastWriteTime);
}
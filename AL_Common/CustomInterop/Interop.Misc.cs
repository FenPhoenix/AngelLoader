// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AL_Common;

internal static partial class Interop
{
    internal static void GetFindData(string fullPath, bool isDirectory, bool ignoreAccessDenied, ref Kernel32.WIN32_FIND_DATA findData)
    {
        using SafeFindHandle handle = Kernel32.FindFirstFile(PathInternal.TrimEndingDirectorySeparator(fullPath), ref findData);
        if (handle.IsInvalid)
        {
            int errorCode = Marshal.GetLastWin32Error();
            // File not found doesn't make much sense coming from a directory.
            if (isDirectory && errorCode == Errors.ERROR_FILE_NOT_FOUND)
            {
                errorCode = Errors.ERROR_PATH_NOT_FOUND;
            }
            if (ignoreAccessDenied && errorCode == Errors.ERROR_ACCESS_DENIED)
            {
                return;
            }
            throw Win32Marshal.GetExceptionForWin32Error(errorCode, fullPath);
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern unsafe bool SetFileTime(
        AL_SafeFileHandle hFile,
        FILE_TIME* creationTime,
        FILE_TIME* lastAccessTime,
        FILE_TIME* lastWriteTime);
}

[PublicAPI]
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

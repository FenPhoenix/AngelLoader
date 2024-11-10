// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

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
}

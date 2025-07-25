using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader;

internal static class NativeCommon
{
    #region Process

    /*
    We use these instead of the built-in ones because those ones won't always work right unless you have
    Admin privileges(?!). At least on Framework anyway.
    */

    private const uint QUERY_LIMITED_INFORMATION = 0x00001000;

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern SafeProcessHandle OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern unsafe bool QueryFullProcessImageNameW(SafeHandle hProcess, uint dwFlags, char* lpBuffer, ref uint lpdwSize);

    private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

    // From .NET 8
    internal static unsafe string? GetProcessPath(int processId)
    {
        using SafeProcessHandle handle = OpenProcess(QUERY_LIMITED_INFORMATION, false, processId);
        if (handle.IsInvalid) return null;

        Span<char> buffer = stackalloc char[MAX_PATH + 1];
        char[]? rentedArray = null;

        try
        {
            while (true)
            {
                uint length = (uint)buffer.Length;
                fixed (char* pinnedBuffer = &MemoryMarshal.GetReference(buffer))
                {
                    if (QueryFullProcessImageNameW(handle, 0, pinnedBuffer, ref length))
                    {
                        return buffer[..(int)length].ToString();
                    }
                    else if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                    {
                        return null;
                    }
                }

                char[]? toReturn = rentedArray;
                buffer = rentedArray = ArrayPool<char>.Shared.Rent(buffer.Length * 2);
                if (toReturn is not null)
                {
                    ArrayPool<char>.Shared.Return(toReturn);
                }
            }
        }
        finally
        {
            if (rentedArray is not null)
            {
                ArrayPool<char>.Shared.Return(rentedArray);
            }
        }
    }

    #endregion

    #region Open folder and select file

    internal static bool OpenFolderAndSelectFile(string filePath)
    {
        try
        {
            nint pidl = ILCreateFromPathW(filePath);
            if (pidl == 0) return false;

            try
            {
                int result = SHOpenFolderAndSelectItems(pidl, 0, 0, 0);
                return result == 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                ILFree(pidl);
            }
        }
        catch
        {
            return false;
        }
    }

    [DllImport("shell32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private static extern nint ILCreateFromPathW(string pszPath);

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern int SHOpenFolderAndSelectItems(nint pidlFolder, int cild, nint apidl, int dwFlags);

    [DllImport("shell32.dll", ExactSpelling = true)]
    private static extern void ILFree(nint pidl);

    #endregion

    // TODO: procName is defined as LPCSTR (not LPCWSTR), so does this mean CharSet.Ansi is required...?
    [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    internal static extern nint GetProcAddress(nint hModule, string procName);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    internal static extern nint GetModuleHandleW(string lpModuleName);
}

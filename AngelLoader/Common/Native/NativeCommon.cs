using System;
using System.Buffers;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader;

internal static partial class NativeCommon
{
    #region Process

    /*
    We use these instead of the built-in ones because those ones won't always work right unless you have
    Admin privileges(?!). At least on Framework anyway.
    */

    private const uint QUERY_LIMITED_INFORMATION = 0x00001000;

    [LibraryImport("kernel32.dll")]
    private static partial SafeProcessHandle OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    [LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool QueryFullProcessImageNameW(SafeHandle hProcess, uint dwFlags, char* lpBuffer, ref uint lpdwSize);

    private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

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
                    else if (Marshal.GetLastPInvokeError() != ERROR_INSUFFICIENT_BUFFER)
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
            IntPtr pidl = ILCreateFromPathW(filePath);
            if (pidl == IntPtr.Zero) return false;

            try
            {
                int result = SHOpenFolderAndSelectItems(pidl, 0, IntPtr.Zero, 0);
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

    [LibraryImport("shell32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr ILCreateFromPathW(string pszPath);

    [LibraryImport("shell32.dll")]
    private static partial int SHOpenFolderAndSelectItems(IntPtr pidlFolder, int cild, IntPtr apidl, int dwFlags);

    [LibraryImport("shell32.dll")]
    private static partial void ILFree(IntPtr pidl);

    #endregion
}

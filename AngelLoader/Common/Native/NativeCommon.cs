using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using static AL_Common.Common;

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

    // From .NET 8, LibraryImport-supporting version
    [LibraryImport("kernel32.dll", EntryPoint = "QueryFullProcessImageNameW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool QueryFullProcessImageName(SafeHandle hProcess, uint dwFlags, char* lpBuffer, ref uint lpdwSize);

    private const int ERROR_INSUFFICIENT_BUFFER = 0x7A;

    internal static unsafe string? GetProcessName(int processId)
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
                    if (QueryFullProcessImageName(handle, 0, pinnedBuffer, ref length))
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

    [ComImport]
    [Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLink;

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    internal interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        void Resolve(IntPtr hwnd, int fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }
}

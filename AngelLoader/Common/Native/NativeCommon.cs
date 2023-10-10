using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader;

internal static class NativeCommon
{
    #region Process

    /*
    We use these instead of the built-in ones because those ones won't always work right unless you have
    Admin privileges(?!). At least on Framework anyway.
    */

    internal const uint QUERY_LIMITED_INFORMATION = 0x00001000;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool QueryFullProcessImageNameW([In] SafeProcessHandle hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("kernel32.dll")]
    internal static extern SafeProcessHandle OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    #endregion
}

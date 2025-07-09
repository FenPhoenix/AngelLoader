using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Win32;

namespace AngelLoader;

internal static class WinVersion
{
    internal static readonly bool Is7OrAbove = WinVersionIs7OrAbove();
    internal static readonly bool Is11OrAbove = WinVersionIs11OrAbove();
    internal static readonly bool SupportsPersistentToolTips = OSSupportsPersistentToolTips();
    internal static readonly bool SupportsDarkMode = WinVersionSupportsDarkMode();
    internal static readonly bool IsWine = RunningOnWine();

    private static bool WinVersionIs7OrAbove()
    {
        try
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Platform == PlatformID.Win32NT &&
                   osVersion.Version >= new Version(6, 1);

            // Windows 8 is 6, 2
        }
        catch
        {
            return false;
        }
    }

    private static bool WinVersionIs11OrAbove()
    {
        try
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Platform == PlatformID.Win32NT &&
                   osVersion.Version >= new Version(10, 0, 22000);
        }
        catch
        {
            return false;
        }
    }

    private static bool WinVersionSupportsDarkMode()
    {
        try
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Platform == PlatformID.Win32NT &&
                   osVersion.Version >= new Version(10, 0, 17763);
        }
        catch
        {
            return false;
        }
    }

    /*
    https://learn.microsoft.com/en-us/dotnet/framework/whats-new/whats-new-in-accessibility#winforms481
    "ToolTip now follows WCAG2.1 guidelines to be persistent, dismissable, and hoverable on Windows 11.
    Changes to tooltip behavior are limited to Windows 11 systems that have .NET Framework 4.8.1 installed,
    and only apply to applications where a timeout was not set for the tooltip. Tooltips that are persisting
    can be dismissed with either the Esc key or the Ctrl key or by navigating to a control with another
    tooltip set."
    */
    private static bool OSSupportsPersistentToolTips()
    {
        try
        {
            if (!Is11OrAbove) return false;
            using RegistryKey? hklm = (RegistryKey?)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            using RegistryKey? ndpKey = hklm?.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\", writable: false);
            return ndpKey?.GetValue("Release") is >= 533320;
        }
        catch
        {
            return false;
        }
    }

    private static bool RunningOnWine()
    {
        return ExportFound("wine_get_version") ||
               ExportFound("wine_get_host_version");

        static bool ExportFound(string name)
        {
            nint hModule = NativeCommon.GetModuleHandleW("ntdll.dll");
            if (hModule != 0)
            {
                nint procAddress = NativeCommon.GetProcAddress(hModule, name);
                if (procAddress != 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the native Microsoft versions of msftedit.dll and gdiplus.dll are installed.
    /// </summary>
    /// <returns></returns>
    internal static bool Wine_NativeDllsInstalled()
    {
        return IsNativeMicrosoftDll("msftedit.dll") &&
               IsNativeMicrosoftDll("gdiplus.dll");

        static bool IsNativeMicrosoftDll(string dllName)
        {
            try
            {
                nint handle = NativeCommon.GetModuleHandleW(dllName);
                if (handle == 0) return true;

                StringBuilder sb = new(1024);
                uint result = NativeCommon.GetModuleFileNameEx(Process.GetCurrentProcess().Handle, handle, sb, sb.Capacity);
                if (result == 0) return true;

                string fileName = sb.ToString();

                var vi = FileVersionInfo.GetVersionInfo(fileName);

                return vi.ProductName.ContainsI("Microsoft") && !vi.ProductName.ContainsI("Wine");
            }
            catch (Exception ex)
            {
                return true;
            }
        }
    }
}

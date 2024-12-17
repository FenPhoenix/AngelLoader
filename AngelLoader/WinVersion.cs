using System;
using Microsoft.Win32;

namespace AngelLoader;

internal static class WinVersion
{
    internal static readonly bool Is7OrAbove = WinVersionIs7OrAbove();
    internal static readonly bool Is11OrAbove = WinVersionIs11OrAbove();
    internal static readonly bool SupportsPersistentToolTips = OSSupportsPersistentToolTips();
    internal static readonly bool SupportsDarkMode = WinVersionSupportsDarkMode();

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
            /*
            @NET5: Registry check for .NET Framework version 4.8.1 - probably shouldn't have this?
            The .NET Framework 4.8.1 release notes say "Changes to tooltip behavior are limited to Windows 11
            systems that have .NET Framework 4.8.1 installed". I can't find any info on why the .NET Framework
            version matters - the 4.8 and 4.8.1 ToolTip.cs files are identical. One would imagine that Framework
            4.8.1's presence would have nothing to do with how modern .NET would behave, but we should test this
            without the registry call on a Windows 10 install without Framework 4.8.1 and see how it behaves.
            */
            using RegistryKey? hklm = (RegistryKey?)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
            using RegistryKey? ndpKey = hklm?.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\", writable: false);
            return ndpKey?.GetValue("Release") is >= 533320;
        }
        catch
        {
            return false;
        }
    }
}

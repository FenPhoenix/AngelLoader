﻿using System;
using Microsoft.Win32;

namespace AngelLoader;

internal static class WinVersion
{
    internal static readonly bool Is7OrAbove = WinVersionIs7OrAbove();
    internal static readonly bool Is8OrAbove = WinVersionIs8OrAbove();
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
        }
        catch
        {
            return false;
        }
    }

    private static bool WinVersionIs8OrAbove()
    {
        try
        {
            OperatingSystem osVersion = Environment.OSVersion;
            return osVersion.Platform == PlatformID.Win32NT &&
                   osVersion.Version >= new Version(6, 2);
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
}

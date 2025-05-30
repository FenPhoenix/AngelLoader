﻿using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using static Update.Data;

namespace Update;

internal static class Utils
{
    internal static bool EqualsI(this string str1, string str2) => str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

    internal static bool StartsWithI(this string str1, string str2) => str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);

    internal static bool StartsWithO(this string str, string value) => str.StartsWith(value, StringComparison.Ordinal);

    internal static int GetPercentFromValue_Int(int current, int total) => total == 0 ? 0 : (100 * current) / total;

    internal static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;

    internal static void ClearUpdateTempPath() => ClearDir(Paths.UpdateTemp);

    internal static void ClearUpdateBakTempPath() => ClearDir(Paths.UpdateBakTemp);

    private static void ClearDir(string path)
    {
        try
        {
            foreach (string f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                new FileInfo(f).IsReadOnly = false;
            }
        }
        catch (DirectoryNotFoundException)
        {
            return;
        }
        catch
        {
            // ignore
        }

        try
        {
            foreach (string f in Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly))
            {
                File.Delete(f);
            }
            foreach (string d in Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly))
            {
                Directory.Delete(d, recursive: true);
            }
        }
        catch
        {
            // ignore
        }
    }

    #region Process

    /*
    We use these instead of the built-in ones because those ones won't always work right unless you have
    Admin privileges(?!). At least on Framework anyway.
    */

    private const uint QUERY_LIMITED_INFORMATION = 0x00001000;

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageNameW([In] SafeProcessHandle hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern SafeProcessHandle OpenProcess(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    #endregion

    internal static async Task WaitForAngelLoaderToClose(CancellationToken cancellationToken)
    {
        string angelLoaderExe = Path.Combine(Application.StartupPath, "AngelLoader.exe");

        StringBuilder buffer = new(1024);

        bool alIsRunning;
        do
        {
            alIsRunning = false;
            Process[] processes = Process.GetProcesses();

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (Process proc in processes)
                {
                    try
                    {
                        string fn = GetProcessPath(proc.Id, buffer);
                        if (!string.IsNullOrEmpty(fn) && fn.Replace('/', '\\').EqualsI(angelLoaderExe.Replace('/', '\\')))
                        {
                            alIsRunning = true;
                            break;
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                foreach (Process process in processes)
                {
                    process.Dispose();
                }
            }
            await Task.Delay(100, cancellationToken);
        } while (alIsRunning);

        return;

        static string GetProcessPath(int procId, StringBuilder buffer)
        {
            buffer.Clear();

            using SafeProcessHandle hProc = OpenProcess(QUERY_LIMITED_INFORMATION, false, procId);
            if (!hProc.IsInvalid)
            {
                int size = buffer.Capacity;
                if (QueryFullProcessImageNameW(hProc, 0, buffer, ref size)) return buffer.ToString();
            }
            return "";
        }
    }

    internal static bool WinVersionSupportsDarkMode()
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

    internal static VisualTheme ReadThemeFromConfigIni(string path)
    {
        try
        {
            using StreamReader sr = new(path);
            while (sr.ReadLine() is { } line)
            {
                string lineT = line.Trim();
                if (lineT.StartsWithO("VisualTheme="))
                {
                    string value = lineT.Substring("VisualTheme=".Length);
                    return value switch
                    {
                        "FollowSystemTheme" => GetSystemTheme(),
                        "Dark" => VisualTheme.Dark,
                        _ => VisualTheme.Classic,
                    };
                }
            }

            return GetSystemTheme();
        }
        catch
        {
            return GetSystemTheme();
        }
    }

    private static VisualTheme GetSystemTheme()
    {
        try
        {
            // Firefox uses this registry key, so if it's reliable enough for them, it's reliable enough for me
            object? appsUseLightThemeKey = Registry.GetValue(
                keyName: @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                valueName: "AppsUseLightTheme",
                defaultValue: "");

            if (appsUseLightThemeKey is int keyInt)
            {
                return keyInt == 0 ? VisualTheme.Dark : VisualTheme.Classic;
            }
        }
        catch
        {
            return VisualTheme.Classic;
        }

        return VisualTheme.Classic;
    }

    internal static void ShowAlert(
        MainForm view,
        string message,
        string? title = null,
        MessageBoxIcon icon = MessageBoxIcon.Warning) => view.Invoke(() =>
    {
        title ??= LText.AlertMessages.Alert;

        using DarkTaskDialog d = new(
            message: message,
            title: title,
            icon: icon,
            yesText: LText.Global.OK,
            defaultButton: DialogResult.Yes);
        d.ShowDialog(view);
    });

    internal static void ShowError(
        MainForm view,
        string message,
        string? title = null,
        MessageBoxIcon icon = MessageBoxIcon.Error) => view.Invoke(() =>
    {
        title ??= LText.AlertMessages.Error;
        using DarkErrorDialog d = new(message, title, icon);
        d.ShowDialog(view);
    });

    internal static DialogResult ShowDialogCustom(
        MainForm view,
        string message,
        string title,
        MessageBoxIcon icon,
        string yesText,
        string noText,
        DialogResult defaultButton) => (DialogResult)view.Invoke(() =>
    {
        using DarkTaskDialog d = new(
            message: message,
            title: title,
            icon: icon,
            yesText: yesText,
            noText: noText,
            defaultButton: defaultButton);

        return d.ShowDialog(view);
    });
}

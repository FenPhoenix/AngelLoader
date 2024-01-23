using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;

namespace Update;

internal static class Utils
{
    internal static void CenterHOnForm(this Control control, Control parent)
    {
        control.Location = control.Location with { X = (parent.ClientSize.Width / 2) - (control.Width / 2) };
    }

    internal static bool EqualsI(this string str1, string str2) => str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

    internal static bool StartsWithI(this string str1, string str2) => str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);

    internal static int GetPercentFromValue_Int(int current, int total) => total == 0 ? 0 : (100 * current) / total;

    private static int Clamp(this int value, int min, int max) => value < min ? min : value > max ? max : value;

    /// <summary>
    /// Hack for better visuals - value changes visually instantly with this.
    /// </summary>
    /// <param name="progressBar"></param>
    /// <param name="value"></param>
    internal static void SetProgress(this ProgressBar progressBar, int value)
    {
        int min = progressBar.Minimum;
        int max = progressBar.Maximum;

        value = value.Clamp(min, max);

        if (value == max)
        {
            progressBar.Value = max;
        }
        else
        {
            progressBar.Value = (value + 1).Clamp(min, max);
            progressBar.Value = value;
        }
    }

    internal static void ClearUpdateTempPath() => ClearDir(Program.UpdateTempPath);

    internal static void ClearUpdateBakTempPath() => ClearDir(Program.UpdateBakTempPath);

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

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageNameW([In] SafeProcessHandle hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("kernel32.dll")]
    private static extern SafeProcessHandle OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    #endregion

    internal static async Task WaitForAngelLoaderToClose()
    {
        string angelLoaderExe = Path.Combine(Application.StartupPath, "AngelLoader.exe");

        var buffer = new StringBuilder(1024);

        bool alIsRunning;
        do
        {
            alIsRunning = false;
            Process[] processes = Process.GetProcesses();
            try
            {
                foreach (Process proc in processes)
                {
                    try
                    {
                        string fn = GetProcessPath(proc.Id, buffer);
                        if (!string.IsNullOrEmpty(fn) && fn.EqualsI(angelLoaderExe))
                        {
                            alIsRunning = true;
                            break;
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            finally
            {
                foreach (Process process in processes)
                {
                    process.Dispose();
                }
            }
            await Task.Delay(100);
        } while (alIsRunning);

        return;

        static string GetProcessPath(int procId, StringBuilder buffer)
        {
            buffer.Clear();

            using var hProc = OpenProcess(QUERY_LIMITED_INFORMATION, false, procId);
            if (!hProc.IsInvalid)
            {
                int size = buffer.Capacity;
                if (QueryFullProcessImageNameW(hProc, 0, buffer, ref size)) return buffer.ToString();
            }
            return "";
        }
    }
}

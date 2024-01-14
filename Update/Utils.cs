using System;
using System.IO;
using System.Windows.Forms;

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

    internal static void ClearUpdateTempPath()
    {
        if (!Directory.Exists(Program.UpdateTempPath)) return;

        try
        {
            foreach (string f in Directory.GetFiles(Program.UpdateTempPath, "*", SearchOption.AllDirectories))
            {
                new FileInfo(f).IsReadOnly = false;
            }
        }
        catch
        {
            // ignore
        }

        try
        {
            foreach (string f in Directory.GetFiles(Program.UpdateTempPath, "*", SearchOption.TopDirectoryOnly))
            {
                File.Delete(f);
            }
            foreach (string d in Directory.GetDirectories(Program.UpdateTempPath, "*", SearchOption.TopDirectoryOnly))
            {
                Directory.Delete(d, recursive: true);
            }
        }
        catch
        {
            // ignore
        }
    }
}

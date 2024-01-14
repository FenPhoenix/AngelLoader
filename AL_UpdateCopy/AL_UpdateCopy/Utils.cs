using System;
using System.Windows.Forms;

namespace AL_UpdateCopy;

internal static class Utils
{
    internal static void CenterHOnForm(this Control control, Control parent)
    {
        control.Location = control.Location with { X = (parent.ClientSize.Width / 2) - (control.Width / 2) };
    }

    internal static bool EqualsI(this string str1, string str2) => str1.Equals(str2, StringComparison.OrdinalIgnoreCase);

    internal static bool StartsWithI(this string str1, string str2) => str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);

    internal static bool EndsWithDirSep(this string str) => str.Length > 0 && (str[str.Length - 1] == '/' || str[str.Length - 1] == '\\');

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
}

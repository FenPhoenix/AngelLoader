using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FMInfoGen;

internal static class Utility
{
    internal static bool ExtIsArchive(this string str) => str.EndsWithI(".zip") || str.EndsWithI(".7z");

    internal static string FN_NoExt(this string value) => Path.GetFileNameWithoutExtension(value);

    #region Control extensions

    /// <summary>
    /// Sets a progress bar's value, clamping it to between its minimum and maximum values.
    /// Avoids the boneheaded out-of-range exception.
    /// </summary>
    /// <param name="progressBar"></param>
    /// <param name="value"></param>
    internal static void SetValueClamped(this ProgressBar progressBar, int value)
    {
        progressBar.Value = value.Clamp(progressBar.Minimum, progressBar.Maximum);
    }

    internal static void AutoSizeColumns(this ListBox listBox)
    {
        if (listBox.Items.Count == 0) return;

        int width = listBox.ColumnWidth;

        using (Graphics g = listBox.CreateGraphics())
        {
            for (int index = 0; index < listBox.Items.Count; index++)
            {
                string item = (string)listBox.Items[index];
                int newWidth = (int)g.MeasureString(item, listBox.Font).Width;
                if (width < newWidth) width = newWidth;
            }
        }

        listBox.ColumnWidth = width;
    }

    #endregion
}

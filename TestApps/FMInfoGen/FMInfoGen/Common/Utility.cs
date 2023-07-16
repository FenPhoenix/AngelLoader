using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace FMInfoGen;

[PublicAPI]
internal static class Utility
{
    #region Natural sorting

    internal static IEnumerable<string> OrderByNaturalI(this IEnumerable<string> items)
    {
        return items.OrderByNatural(static x => x, StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<T> OrderByNatural<T>(this IEnumerable<T> items, Func<T, string> selector,
        StringComparer? stringComparer = null)
    {
        var regex = new Regex(@"\d+", RegexOptions.Compiled);

        var enumerable = items as T[] ?? items.ToArray();
        int maxDigits = enumerable
            .SelectMany(i =>
                regex.Matches(selector(i)).Cast<Match>()
                    .Select(static digitChunk => (int?)digitChunk.Value.Length))
            .Max() ?? 0;

        return enumerable.OrderBy(
            i => regex.Replace(selector(i), match => match.Value.PadLeft(maxDigits, '0')),
            stringComparer ?? StringComparer.CurrentCulture);
    }

    #endregion

    internal static int[] AllIndexesOf(this string str, string substr, bool ignoreCase = true)
    {
        if (str.IsWhiteSpace() || substr.IsWhiteSpace())
        {
            throw new ArgumentException("String or substring is not specified.");
        }

        var indexes = new List<int>();
        int index = 0;

        while ((index = str.IndexOf(substr, index, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
        {
            indexes.Add(index++);
        }

        return indexes.ToArray();
    }

    internal static bool Contains(this string value, string substring, StringComparison comparison)
    {
        return value.IndexOf(substring, comparison) >= 0;
    }

    internal static bool Contains(this string value, char character)
    {
        return value.IndexOf(character) >= 0;
    }

    internal static bool ContainsI(this string value, string substring)
    {
        return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool ContainsI(this IEnumerable<string> value, string stringToSearchFor)
    {
        return value.Contains(stringToSearchFor, StringComparer.OrdinalIgnoreCase);
    }

    internal static bool ContainsS(this string value, string substring)
    {
        return value.Contains(substring, StringComparison.Ordinal);
    }

    internal static bool ContainsS(this IEnumerable<string> value, string stringToSearchFor)
    {
        return value.Contains(stringToSearchFor, StringComparer.Ordinal);
    }

    internal static bool EqualsI(this string first, string second)
    {
        return first.Equals(second, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool StartsWithI(this string str, string value)
    {
        return str.StartsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool EndsWithI(this string str, string value)
    {
        return str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool ExtIsArchive(this string str) => str.EndsWithI(".zip") || str.EndsWithI(".7z");

    internal static string FN_NoExt(this string value) => Path.GetFileNameWithoutExtension(value);

    #region Empty / whitespace checks

    /// <summary>
    /// Returns true if <paramref name="value"/> is null or empty.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [ContractAnnotation("null => true")]
    internal static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [ContractAnnotation("null => true")]
    internal static bool IsWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

    #endregion

    /// <summary>
    /// Clamps a number to between min and max.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> =>
        value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;

    internal static void CancelIfNotDisposed(this CancellationTokenSource value)
    {
        try { value.Cancel(); } catch (ObjectDisposedException) { }
    }

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

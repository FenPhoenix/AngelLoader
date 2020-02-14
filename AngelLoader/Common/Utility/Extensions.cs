using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.Misc;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader
{
    internal static class Extensions
    {
        #region Queries

        /// <summary>
        /// Returns the number of times a character appears in a string.
        /// Avoids whatever silly overhead junk Count(predicate) is doing.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        internal static int CountChars(this string value, char character)
        {
            int count = 0;
            for (int i = 0; i < value.Length; i++) if (value[i] == character) count++;

            return count;
        }

        #region Contains

#if false

        internal static int IndexOfByteSequence(this byte[] input, byte[] pattern)
        {
            byte firstByte = pattern[0];
            int index = Array.IndexOf(input, firstByte);

            while (index > -1)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (index + i >= input.Length) return -1;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return -1;
                        break;
                    }

                    if (i == pattern.Length - 1) return index;
                }
            }

            return index;
        }

#endif

        [PublicAPI]
        internal static bool Contains(this string value, string substring, StringComparison comparison)
        {
            return !value.IsEmpty() && !substring.IsEmpty() && value.IndexOf(substring, comparison) >= 0;
        }

        [PublicAPI]
        internal static bool Contains(this string value, char character) => value.IndexOf(character) >= 0;

        /// <summary>
        /// Case-insensitive Contains.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool ContainsI(this string value, string substring) => value.Contains(substring, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Case-insensitive Contains for List&lt;string&gt;. Avoiding IEnumerable like the plague for speed.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool ContainsI(this List<string> list, string str) => list.Contains(str, StringComparison.OrdinalIgnoreCase);

        [PublicAPI]
        internal static bool ContainsIRemoveFirstHit(this List<string> list, string str) => list.ContainsRemoveFirstHit(str, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Case-insensitive Contains for string[]. Avoiding IEnumerable like the plague for speed.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        [PublicAPI]
        internal static bool ContainsI(this string[] array, string str) => array.Contains(str, StringComparison.OrdinalIgnoreCase);

        [PublicAPI]
        internal static bool ContainsRemoveFirstHit(this List<string> value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].Equals(substring, stringComparison))
                {
                    value.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        [PublicAPI]
        internal static bool Contains(this List<string> value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].Equals(substring, stringComparison)) return true;
            return false;
        }

        [PublicAPI]
        internal static bool Contains(this string[] value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].Equals(substring, stringComparison)) return true;
            return false;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Case-insensitive Equals.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool EqualsI(this string first, string second) => string.Equals(first, second, StringComparison.OrdinalIgnoreCase);

        internal static bool EqualsTrue(this string value) => string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase);

        #endregion

        #region Filename extension checks

        internal static bool IsValidReadme(this string value)
        {
            // Well, this is embarrassing... Apparently EndsWithI is faster than the baked-in ones.
            // Dunno how that could be the case, but whatever...
            return value.EndsWithI(".txt") ||
                   value.EndsWithI(".rtf") ||
                   value.EndsWithI(".wri") ||
                   value.EndsWithI(".glml") ||
                   value.EndsWithI(".html") ||
                   value.EndsWithI(".htm");
        }

        #region Baked-in extension checks
        // TODO: Just passthroughs now because EndsWithI turned out to be faster(?!)

        internal static bool ExtIsTxt(this string value) => value.EndsWithI(".txt");

        internal static bool ExtIsRtf(this string value) => value.EndsWithI(".rtf");

        internal static bool ExtIsWri(this string value) => value.EndsWithI(".wri");

        internal static bool ExtIsHtml(this string value) => value.EndsWithI(".html") || value.EndsWithI(".htm");

        internal static bool ExtIsGlml(this string value) => value.EndsWithI(".glml");

        internal static bool ExtIsArchive(this string value) => value.EndsWithI(".zip") || value.EndsWithI(".7z");

        internal static bool ExtIsZip(this string value) => value.EndsWithI(".zip");

        internal static bool ExtIs7z(this string value) => value.EndsWithI(".7z");

        #endregion

        #endregion

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

        #region StartsWith and EndsWith

        private enum StartOrEnd { Start, End }

        /// <summary>
        /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, StartOrEnd.Start);

        /// <summary>
        /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool EndsWithI(this string str, string value) => StartsWithOrEndsWithIFast(str, value, StartOrEnd.End);

        private static bool StartsWithOrEndsWithIFast(this string str, string value, StartOrEnd startOrEnd)
        {
            if (str.IsEmpty() || str.Length < value.Length) return false;

            // Note: ASCII chars are 0-127. Uppercase is 65-90; lowercase is 97-122.
            // Therefore, if a char is in one of these ranges, one can convert between cases by simply adding or
            // subtracting 32.

            bool start = startOrEnd == StartOrEnd.Start;
            int siStart = start ? 0 : str.Length - value.Length;
            int siEnd = start ? value.Length : str.Length;

            for (int si = siStart, vi = 0; si < siEnd; si++, vi++)
            {
                // If we find a non-ASCII character, give up and run the slow check on the whole string. We do
                // this because one .NET char doesn't necessarily equal one Unicode char. Multiple .NET chars
                // might be needed. So we grit our teeth and take the perf hit of letting .NET handle it.
                // This is tuned for ASCII being the more common case, so we can save an advance check for non-
                // ASCII chars, at the expense of being slightly (probably insignificantly) slower if there are
                // in fact non-ASCII chars in value.
                if (value[vi] > 127)
                {
                    return start
                        ? str.StartsWith(value, StringComparison.OrdinalIgnoreCase)
                        : str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
                }

                if (str[si] >= 65 && str[si] <= 90 && value[vi] >= 97 && value[vi] <= 122)
                {
                    if (str[si] != value[vi] - 32) return false;
                }
                else if (value[vi] >= 65 && value[vi] <= 90 && str[si] >= 97 && str[si] <= 122)
                {
                    if (str[si] != value[vi] + 32) return false;
                }
                else if (str[si] != value[vi])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion

        #region Modifications

        #region Clear and add

        internal static void ClearAndAdd<T>(this List<T> list, params T[] items)
        {
            list.Clear();
            list.AddRange(items);
        }

        internal static void ClearAndAdd<T>(this List<T> list, IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }

        #endregion

        #region Clamping

        /// <summary>
        /// Clamps a number to between min and max.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        internal static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            return value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;
        }

        /// <summary>
        /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ClampToZero(this int value) => Math.Max(value, 0);

        #endregion

        internal static string FormatSize(this ulong size)
        {
            if (size == 0) return "";

            return size < ByteSize.MB
                 ? Math.Round(size / 1024f).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.KilobyteShort
                 : size >= ByteSize.MB && size < ByteSize.GB
                 ? Math.Round(size / 1024f / 1024f).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.MegabyteShort
                 : Math.Round(size / 1024f / 1024f / 1024f, 2).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.GigabyteShort;
        }

        #region FM installed name conversion

        /// <summary>
        /// Format an FM archive name to conform to NewDarkLoader's FM install directory name requirements.
        /// </summary>
        /// <param name="archiveName">Filename without path or extension.</param>
        /// <param name="truncate"></param>
        /// <returns></returns>
        internal static string ToInstDirNameNDL(this string archiveName, bool truncate = true) => ToInstDirName(archiveName, "+.~ ", truncate);

        /// <summary>
        /// Format an FM archive name to conform to FMSel's FM install directory name requirements.
        /// </summary>
        /// <param name="archiveName">Filename without path or extension.</param>
        /// <param name="truncate">Whether to truncate the name to 30 characters or less.</param>
        /// <returns></returns>
        internal static string ToInstDirNameFMSel(this string archiveName, bool truncate = true) => ToInstDirName(archiveName, "+;:.,<>?*~| ", truncate);

        private static readonly StringBuilder ToInstDirNameSB = new StringBuilder();
        private static string ToInstDirName(string archiveName, string illegalChars, bool truncate)
        {
            int count = archiveName.LastIndexOf('.');
            if (truncate)
            {
                if (count == -1 || count > 30) count = Math.Min(archiveName.Length, 30);
            }
            else
            {
                if (count == -1) count = archiveName.Length;
            }

            ToInstDirNameSB.Clear();
            ToInstDirNameSB.Append(archiveName);
            for (int i = 0; i < illegalChars.Length; i++) ToInstDirNameSB.Replace(illegalChars[i], '_', 0, count);

            return ToInstDirNameSB.ToString(0, count);
        }

        #endregion

        /// <summary>
        /// Just removes the extension from a filename, without the rather large overhead of
        /// Path.GetFileNameWithoutExtension().
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string RemoveExtension(this string fileName)
        {
            int i;
            return (i = fileName.LastIndexOf('.')) == -1 ? fileName : fileName.Substring(0, i);
        }

        #region Get file / dir names

        /// <summary>
        /// Strips the path from the filename, taking into account only the current OS's directory separator char.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetFileNameFast(this string path)
        {
            int i;
            return (i = path.LastIndexOf(Path.DirectorySeparatorChar)) == -1 ? path : path.Substring(i + 1);
        }

        /// <summary>
        /// Strips the leading path from the filename, taking into account both / and \ chars.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string GetFileNameFastBothDSC(this string path)
        {
            int i1 = path.LastIndexOf('\\');
            int i2 = path.LastIndexOf('/');

            if (i1 == -1 && i2 == -1) return path;

            return path.Substring(Math.Max(i1, i2) + 1);
        }

        internal static string GetDirNameFast(this string path)
        {
            path = path.TrimEnd(CA_BS_FS);

            int i1 = path.LastIndexOf('\\');
            int i2 = path.LastIndexOf('/');

            if (i1 == -1 && i2 == -1) return path;

            return path.Substring(Math.Max(i1, i2) + 1);
        }

        #endregion

        internal static string ToSingleLineComment(this string value, int maxLength)
        {
            if (value.IsEmpty()) return "";

            int linebreakIndex = value.IndexOf("\r\n", StringComparison.InvariantCulture);

            return linebreakIndex > -1 && linebreakIndex <= maxLength
                ? value.Substring(0, linebreakIndex)
                : value.Substring(0, Math.Min(value.Length, maxLength));
        }

        #region Escaping

        internal static string FromRNEscapes(this string value) => value.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\");

        internal static string ToRNEscapes(this string value) => value.Replace("\\", @"\\").Replace("\r\n", @"\r\n");

        /// <summary>
        /// For text that goes in menus: "&" is a reserved character, so escape "&" to "&&"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string EscapeAmpersands(this string value) => value.Replace("&", "&&");

        /// <summary>
        /// Just puts a \ in front of each character in the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string EscapeAllChars(this string value)
        {
            // Don't remove this freaking null check, or Config reading might fail
            if (value.IsEmpty()) return "";

            string ret = "";
            foreach (char c in value) ret += '\\' + c.ToString();

            return ret;
        }

        #endregion

        #region Forward/backslash conversion

        internal static string ToForwardSlashes(this string value) => value.Replace('\\', '/');

        internal static string ToBackSlashes(this string value) => value.Replace('/', '\\');

        internal static string ToSystemDirSeps(this string value)
        {
            return value.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        }

        #endregion

        #endregion

        #region Control hacks

        #region Suspend/resume drawing

        internal static void SuspendDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, false, 0);
        }

        internal static void ResumeDrawing(this Control control)
        {
            if (!control.IsHandleCreated || !control.Visible) return;
            SendMessage(control.Handle, WM_SETREDRAW, true, 0);
            control.Refresh();
        }

        #endregion

        // NOTE: Blocking a window causes ding sounds if you interact with it (if you have Windows sounds enabled...)
        // Currently using a transparent panel hack, along with suppressing keys/mouse.
        // Reason we need the transparent panel hack is because mouse doesn't get suppressed enough when it comes
        // to the RichTextBox (you can still scroll, ugh). Also mouse movement still makes highlights happen and
        // stuff. Fine, but I want to make it clear the window is not responding to anything.
        // If I ever put the RichTextBox in a separate app domain or whatever, the new method might not work.
        // This method works in a pinch, dings notwithstanding.
        //internal static void BlockWindow(this Control control, bool block)
        //{
        //    if (!control.IsHandleCreated) return;
        //    EnableWindow(control.Handle, bEnable: !block);
        //}

        /// <summary>
        /// Sets the progress bar's value instantly. Avoids the la-dee-dah catch-up-when-I-feel-like-it nature of
        /// the progress bar that makes it look annoying and unprofessional.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="value"></param>
        public static void SetValueInstant(this ProgressBar pb, int value)
        {
            if (value == pb.Maximum)
            {
                pb.Value = pb.Maximum;
            }
            else
            {
                pb.Value = (value + 1).Clamp(pb.Minimum, pb.Maximum);
                pb.Value = value.Clamp(pb.Minimum, pb.Maximum);
            }
        }

        #region Centering

        [PublicAPI]
        internal static void CenterH(this Control control, Control parent)
        {
            control.Location = new Point((parent.Width / 2) - (control.Width / 2), control.Location.Y);
        }

        [PublicAPI]
        internal static void CenterV(this Control control, Control parent)
        {
            control.Location = new Point(control.Location.X, (parent.Height / 2) - (control.Height / 2));
        }

        [PublicAPI]
        internal static void CenterHV(this Control control, Control parent, bool clientSize = false)
        {
            int pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
            int pHeight = clientSize ? parent.ClientSize.Height : parent.Height;
            control.Location = new Point((pWidth / 2) - (control.Width / 2), (pHeight / 2) - (control.Height / 2));
        }

        #endregion

        #region Autosizing

        // PERF_TODO: These are relatively expensive operations (10ms to make 3 calls from SettingsForm)
        // See if we can manually calculate some or all of this and end up with the same result as if we let the
        // layout do the work as we do now.

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes it horizontally to accomodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="text"></param>
        /// <param name="minWidth"></param>
        /// <param name="preserveHeight"></param>
        internal static void SetTextAutoSize(this Button button, string text, int minWidth = -1, bool preserveHeight = false)
        {
            // Buttons can't be GrowOrShrink because that also shrinks them vertically. So do it manually here.
            button.Text = "";
            button.Width = 2;
            if (!preserveHeight) button.Height = 2;
            button.Text = text;

            if (minWidth > -1 && button.Width < minWidth) button.Width = minWidth;
        }

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes and repositions the <see cref="Button"/> and a
        /// <see cref="TextBox"/> horizontally together to accommodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="textBox"></param>
        /// <param name="text"></param>
        /// <param name="minWidth"></param>
        internal static void SetTextAutoSize(this Button button, TextBox textBox, string text, int minWidth = -1)
        {
            // Quick fix for this not working if layout is suspended.
            // This will then cause any other controls within the same parent to do their full layout.
            // If this becomes a problem, come up with a better solution here.
            button.Parent.ResumeLayout();

            AnchorStyles oldAnchor = button.Anchor;
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            int oldWidth = button.Width;

            button.SetTextAutoSize(text, minWidth);

            int diff =
                button.Width > oldWidth ? -(button.Width - oldWidth) :
                button.Width < oldWidth ? oldWidth - button.Width : 0;

            button.Left += diff;
            // For some reason the diff doesn't work when scaling is > 100% so, yeah
            textBox.Width = button.Left > textBox.Left ? (button.Left - textBox.Left) - 1 : 0;

            button.Anchor = oldAnchor;
        }

        #endregion

        internal static void RemoveAndSelectNearest(this ListBox listBox)
        {
            if (listBox.SelectedIndex == -1) return;

            int oldSelectedIndex = listBox.SelectedIndex;

            listBox.Items.RemoveAt(listBox.SelectedIndex);

            if (oldSelectedIndex < listBox.Items.Count && listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex;
            }
            else if (listBox.Items.Count > 1)
            {
                listBox.SelectedIndex = oldSelectedIndex - 1;
            }
            else if (listBox.Items.Count == 1)
            {
                listBox.SelectedIndex = 0;
            }
        }

        #endregion

        internal static void CancelIfNotDisposed(this CancellationTokenSource value)
        {
            try { value.Cancel(); } catch (ObjectDisposedException) { }
        }
    }
}

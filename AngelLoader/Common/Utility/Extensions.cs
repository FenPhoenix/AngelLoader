using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AngelLoader.Common.DataClasses;
using JetBrains.Annotations;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.Common.Utility
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

        internal static bool Contains(this string value, string substring, StringComparison comparison)
        {
            if (value.IsEmpty() || substring.IsEmpty()) return false;
            return value.IndexOf(substring, comparison) >= 0;
        }

        internal static bool Contains(this string value, char character) => value.IndexOf(character) >= 0;

        /// <summary>
        /// Case-insensitive Contains.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        internal static bool ContainsI(this string value, string substring)
        {
            return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Case-insensitive Contains for List&lt;string&gt;. Avoiding IEnumerable like the plague for speed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        internal static bool ContainsI(this List<string> value, string substring)
        {
            return value.Contains(substring, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool Contains(this List<string> value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            // Dead simple, dead fast
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i].Equals(substring, stringComparison)) return true;
            }
            return false;
        }

        internal static bool Contains(this string[] value, string substring, StringComparison stringComparison = StringComparison.Ordinal)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].Equals(substring, stringComparison)) return true;
            }
            return false;
        }

        /// <summary>
        /// Case-insensitive Equals.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        internal static bool EqualsI(this string first, string second)
        {
            return string.Equals(first, second, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsTrue(this string value)
        {
            return string.Equals(value, bool.TrueString, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsFalse(this string value)
        {
            return string.Equals(value, bool.FalseString, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true if the string ends with extension (case-insensitive).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        internal static bool ExtEqualsI(this string value, string extension)
        {
            if (extension[0] != '.') extension = "." + extension;

            return !value.IsEmpty() &&
                   string.Equals(Path.GetExtension(value), extension, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ExtIsHtml(this string value)
        {
            return value.ExtEqualsI(".htm") || value.ExtEqualsI(".html");
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> is null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsEmpty(this string value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        internal static bool IsWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);

        internal static bool IsValidReadme(this string value)
        {
            return value.ExtEqualsI(".txt") ||
                   value.ExtEqualsI(".rtf") ||
                   value.ExtEqualsI(".wri") ||
                   value.ExtEqualsI(".glml") ||
                   value.ExtIsHtml();
        }

        #region StartsWith and EndsWith

        private enum CaseComparison
        {
            CaseSensitive,
            CaseInsensitive,
            GivenOrUpper,
            GivenOrLower
        }

        private enum StartOrEnd
        {
            Start,
            End
        }

        /// <summary>
        /// StartsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithI(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.CaseInsensitive, StartOrEnd.Start);
        }

        /// <summary>
        /// StartsWith (given case or uppercase). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithGU(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.GivenOrUpper, StartOrEnd.Start);
        }

        /// <summary>
        /// StartsWith (given case or lowercase). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool StartsWithGL(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.GivenOrLower, StartOrEnd.Start);
        }

        /// <summary>
        /// EndsWith (case-insensitive). Uses a fast ASCII compare where possible.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool EndsWithI(this string str, string value)
        {
            return StartsWithOrEndsWithFast(str, value, CaseComparison.CaseInsensitive, StartOrEnd.End);
        }

        private static bool StartsWithOrEndsWithFast(this string str, string value,
            CaseComparison caseComparison, StartOrEnd startOrEnd)
        {
            if (str.IsEmpty() || str.Length < value.Length) return false;

            // Note: ASCII chars are 0-127. Uppercase is 65-90; lowercase is 97-122.
            // Therefore, if a char is in one of these ranges, one can convert between cases by simply adding or
            // subtracting 32.

            var start = startOrEnd == StartOrEnd.Start;
            var siStart = start ? 0 : str.Length - value.Length;
            var siEnd = start ? value.Length : str.Length;

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
                    switch (caseComparison)
                    {
                        case CaseComparison.CaseSensitive:
                            return start
                                ? str.StartsWith(value, StringComparison.Ordinal)
                                : str.EndsWith(value, StringComparison.Ordinal);
                        case CaseComparison.CaseInsensitive:
                            return start
                                ? str.StartsWith(value, StringComparison.OrdinalIgnoreCase)
                                : str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
                        case CaseComparison.GivenOrUpper:
                            return start
                                ? str.StartsWith(value, StringComparison.Ordinal) ||
                                  str.StartsWith(value.ToUpperInvariant(), StringComparison.Ordinal)
                                : str.EndsWith(value, StringComparison.Ordinal) ||
                                  str.EndsWith(value.ToUpperInvariant(), StringComparison.Ordinal);
                        case CaseComparison.GivenOrLower:
                            return start
                                ? str.StartsWith(value, StringComparison.Ordinal) ||
                                  str.StartsWith(value.ToLowerInvariant(), StringComparison.Ordinal)
                                : str.EndsWith(value, StringComparison.Ordinal) ||
                                  str.EndsWith(value.ToLowerInvariant(), StringComparison.Ordinal);
                    }
                }

                if (str[si] >= 65 && str[si] <= 90 && value[vi] >= 97 && value[vi] <= 122)
                {
                    if (caseComparison == CaseComparison.GivenOrLower || str[si] != value[vi] - 32) return false;
                }
                else if (value[vi] >= 65 && value[vi] <= 90 && str[si] >= 97 && str[si] <= 122)
                {
                    if (caseComparison == CaseComparison.GivenOrUpper || str[si] != value[vi] + 32) return false;
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

        internal static string ConvertSize(this long? size)
        {
            if (size == null || size == 0) return "";

            return size < ByteSize.MB
                 ? Math.Round((double)(size / 1024f)).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.KilobyteShort
                 : size >= ByteSize.MB && size < ByteSize.GB
                 ? Math.Round((double)(size / 1024f / 1024f)).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.MegabyteShort
                 : Math.Round((double)(size / 1024f / 1024f / 1024f), 2).ToString(CultureInfo.CurrentCulture) + " " + LText.Global.GigabyteShort;
        }

        /// <summary>
        /// Format an FM archive name to conform to NewDarkLoader's FM install directory name requirements.
        /// </summary>
        /// <param name="archiveName">Filename without path or extension.</param>
        /// <returns></returns>
        internal static string ToInstalledFMDirNameNDL(this string archiveName)
        {
            return ToInstalledFMDirName(archiveName, "+.~ ", truncate: true);
        }

        /// <summary>
        /// Format an FM archive name to conform to FMSel's FM install directory name requirements.
        /// </summary>
        /// <param name="archiveName">Filename without path or extension.</param>
        /// <param name="truncate">Whether to truncate the name to 30 characters or less.</param>
        /// <returns></returns>
        internal static string ToInstalledFMDirNameFMSel(this string archiveName, bool truncate = true)
        {
            return ToInstalledFMDirName(archiveName, "+;:.,<>?*~| ", truncate);
        }

        private static string ToInstalledFMDirName(string archiveName, string illegalChars, bool truncate)
        {
            archiveName = archiveName.RemoveExtension();

            if (truncate) archiveName = archiveName.Length > 30 ? archiveName.Substring(0, 30) : archiveName;

            for (var i = 0; i < illegalChars.Length; i++)
            {
                archiveName = archiveName.Replace(illegalChars[i], '_');
            }

            return archiveName;
        }

        /// <summary>
        /// Just removes the extension from a filename, without the rather large overhead of
        /// Path.GetFileNameWithoutExtension().
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static string RemoveExtension(this string fileName)
        {
            if (fileName == null) return null;
            int i;
            return (i = fileName.LastIndexOf('.')) == -1 ? fileName : fileName.Substring(0, i);
        }

        internal static string GetFileNameFast(this string path)
        {
            if (path == null) return null;
            int i;
            return (i = path.LastIndexOf('\\')) == -1 ? path : path.Substring(i + 1);
        }

        internal static string GetFileNameFastZip(this string path)
        {
            if (path == null) return null;

            int i1 = path.LastIndexOf('\\');
            int i2 = path.LastIndexOf('/');

            if (i1 == -1 & i2 == -1) return path;

            return path.Substring(Math.Max(i1, i2) + 1);
        }

        internal static string GetDirNameFast(this string path)
        {
            if (path == null) return null;

            while (path[path.Length - 1] == '\\' || path[path.Length - 1] == '/')
            {
                path = path.TrimEnd('\\').TrimEnd('/');
            }

            int i1 = path.LastIndexOf('\\');
            int i2 = path.LastIndexOf('/');

            if (i1 == -1 & i2 == -1) return path;

            return path.Substring(Math.Max(i1, i2) + 1);
        }

        internal static string GetTopmostDirName(this string path)
        {
            var i = path.LastIndexOf('\\');
            if (i == -1) return path;

            var end = path.Length;
            if (i == path.Length - 1)
            {
                end--;
                i = path.LastIndexOf('\\', end);
            }

            return path.Substring(i + 1, end - (i + 1));
        }

        /// <summary>
        /// If <paramref name="value"/> is null or empty, returns null. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string ThisOrNull(this string value) => value.IsEmpty() ? null : value;

        /// <summary>
        /// If <paramref name="value"/> is null, empty, or whitespace, returns null. Otherwise, returns
        /// <paramref name="value"/> unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string ThisOrNullWS(this string value) => value.IsWhiteSpace() ? null : value;

        /// <summary>
        /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int ClampToZero(this int value) => Math.Max(value, 0);

        internal static string ToSingleLineComment(this string value, int maxLength)
        {
            if (value.IsEmpty()) return "";

            var linebreakIndex = value.IndexOf("\r\n", StringComparison.InvariantCulture);

            return linebreakIndex > -1 && linebreakIndex <= maxLength
                ? value.Substring(0, linebreakIndex)
                : value.Substring(0, Math.Min(value.Length, maxLength));
        }

        internal static string FromEscapes(this string value)
        {
            return value.IsEmpty() ? value : value.Replace(@"\r\n", "\r\n").Replace(@"\\", "\\");
        }

        internal static string ToEscapes(this string value)
        {
            return value.IsEmpty() ? value : value.Replace("\\", @"\\").Replace("\r\n", @"\r\n");
        }

        /// <summary>
        /// Just puts a \ in front of each character in the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string EscapeAllChars(this string value)
        {
            var ret = "";
            foreach (char c in value) ret += '\\' + c.ToString();

            return ret;
        }

        internal static void SortCat(this List<CatAndTags> list)
        {
            list.Sort(new CategoryComparer());
            var misc = list.FirstOrDefault(x => x.Category == "misc");
            if (misc != null)
            {
                list.Remove(misc);
                list.Add(misc);
            }
        }

        internal static void SortCat(this List<GlobalCatAndTags> list)
        {
            list.Sort(new CategoryComparerGlobal());
            var misc = list.FirstOrDefault(x => x.Category.Name == "misc");
            if (misc != null)
            {
                list.Remove(misc);
                list.Add(misc);
            }
        }

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

        internal static void Block(this Form form)
        {
            if (!form.IsHandleCreated) return;
            EnableWindow(form.Handle, false);
        }

        internal static void Unblock(this Form form)
        {
            if (!form.IsHandleCreated) return;
            EnableWindow(form.Handle, true);
        }

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
                pb.Value = value + 1;
                pb.Value = value;
            }
        }

        #region Centering

        internal static void CenterH(this Control control, Control parent)
        {
            control.Location = new Point((parent.Width / 2) - (control.Width / 2), control.Location.Y);
        }

        internal static void CenterV(this Control control, Control parent)
        {
            control.Location = new Point(control.Location.X, (parent.Height / 2) - (control.Height / 2));
        }

        internal static void CenterHV(this Control control, Control parent, bool clientSize = false)
        {
            var pWidth = clientSize ? parent.ClientSize.Width : parent.Width;
            var pHeight = clientSize ? parent.ClientSize.Height : parent.Height;
            control.Location = new Point(
                (pWidth / 2) - (control.Width / 2),
                (pHeight / 2) - (control.Height / 2));
        }

        #endregion

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes it horizontally to accomodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="text"></param>
        /// <param name="minWidth"></param>
        /// <param name="padding"></param>
        /// <param name="parent">If non-null, this control will have its drawing suspended/resumed during the resize</param>
        internal static void SetTextAutoSize(this Button button, string text, int minWidth = -1, int padding = 12,
            Control parent = null)
        {
            parent?.SuspendDrawing();

            // Buttons can't be GrowOrShrink because that also shrinks them vertically. So do it manually here.
            button.Text = "";
            button.Width = 2;
            button.Text = text;

            if (minWidth > -1)
            {
                if (button.Width < minWidth) button.Width = minWidth;
            }
            else
            {
                // Extra padding for a nicer look
                button.Width += padding;
            }
            parent?.ResumeDrawing();
        }

        /// <summary>
        /// Sets a <see cref="Button"/>'s text, and autosizes and repositions the <see cref="Button"/> and a
        /// <see cref="TextBox"/> horizontally together to accomodate it.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="textBox"></param>
        /// <param name="text"></param>
        /// <param name="minWidth"></param>
        /// <param name="padding"></param>
        internal static void SetTextAutoSize(this Button button, TextBox textBox, string text, int minWidth = -1, int padding = 12)
        {
            var oldAnchor = button.Anchor;
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            int oldWidth = button.Width;

            button.SetTextAutoSize(text, minWidth, padding);

            int diff =
                button.Width > oldWidth ? -(button.Width - oldWidth) :
                button.Width < oldWidth ? oldWidth - button.Width : 0;

            button.Left += diff;
            textBox.Width += diff;

            button.Anchor = oldAnchor;
        }

        #endregion
    }
}

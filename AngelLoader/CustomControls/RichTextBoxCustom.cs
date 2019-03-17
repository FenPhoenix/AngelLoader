using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.HTMLNamedEscapes;

namespace AngelLoader.CustomControls
{
    internal sealed class RichTextBoxCustom : RichTextBox
    {
        private bool initialReadmeZoomSet = true;
        internal float StoredZoomFactor = 1.0f;

        #region Zoom stuff

        internal void ZoomIn()
        {
            try
            {
                ZoomFactor += 0.1f;
            }
            catch (ArgumentException)
            {
                // leave it as is
            }
        }

        internal void ZoomOut()
        {
            try
            {
                ZoomFactor -= 0.1f;
            }
            catch (ArgumentException)
            {
                // leave it as is
            }
        }

        internal void ResetZoomFactor()
        {
            this.SuspendDrawing();

            // We have to set another value first, or it won't take.
            ZoomFactor = 1.1f;
            ZoomFactor = 1.0f;

            this.ResumeDrawing();
        }

        private void SaveZoom()
        {
            // Because the damn thing resets its zoom every time you load new content, we have to keep a global
            // var with the zoom value and keep both values in sync.
            if (initialReadmeZoomSet)
            {
                initialReadmeZoomSet = false;
            }
            else
            {
                // Don't do this if we're just starting up, because then it will throw away our saved value
                StoredZoomFactor = ZoomFactor;
            }
        }

        private void RestoreZoom()
        {
            // Heisenbug: If we step through this, it sets the zoom factor correctly. But if we're actually
            // running normally, it doesn't, and we need to set the size to something else first and THEN it will
            // work. Normally this causes the un-zoomed text to be shown for a split-second before the zoomed text
            // gets shown, so we use custom extensions to suspend and resume drawing while we do this ridiculous
            // hack, so it looks perfectly flawless to the end user.
            ZoomFactor = 1.0f;

            try
            {
                ZoomFactor = StoredZoomFactor;
            }
            catch (ArgumentException)
            {
                // Do nothing; remain at 1.0
            }
        }

        #endregion

        /// <summary>
        /// Sets the text without resetting the zoom factor.
        /// </summary>
        /// <param name="text"></param>
        internal void SetText(string text)
        {
            SaveZoom();

            this.SuspendDrawing();

            // Blank the text to reset the scroll position to the top
            Clear();

            if (!text.IsEmpty()) Text = text;

            RestoreZoom();

            this.ResumeDrawing();
        }

        /// <summary>
        /// Loads a file into the box without resetting the zoom factor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileType"></param>
        internal void LoadContent(string path, RichTextBoxStreamType fileType)
        {
            SaveZoom();

            try
            {
                this.SuspendDrawing();

                // Blank the text to reset the scroll position to the top
                Clear();

                if (path.ExtEqualsI(".glml"))
                {
                    var text = File.ReadAllText(path);
                    Rtf = GLMLToRTF(text);
                }
                else
                {
                    LoadFile(path, fileType);
                }
            }
            finally
            {
                RestoreZoom();
                this.ResumeDrawing();
            }
        }

        #region GLML to RTF

        // RichTextBox steadfastly refuses to understand the normal way of drawing lines, so use a small image
        // and scale the width out to a gazillion
        private const string HorizontalLine =
            // width and height are in twips, 30 twips = 2 pixels, 285 twips = 19 pixels, etc.
            // picscalex is in percent
            // max value for anything is 32767
            @"{\pict\wmetafile8\picw30\pich285\picwgoal32767\pichgoal285\picscalex1600 " +
            @"0100090000039000000000006700000000000400000003010800050000000b0200000000050000" +
            @"000c0213000200030000001e000400000007010400040000000701040067000000410b2000cc00" +
            @"130002000000000013000200000000002800000002000000130000000100040000000000000000" +
            @"000000000000000000000000000000000000000000ffffff006666660000000000000000000000" +
            @"000000000000000000000000000000000000000000000000000000000000000000000000000000" +
            @"0000001101d503110100001101d503110100001101bafd11010000110100001101000011010000" +
            @"2202000011010000110100001101000011010000110100001101803f1101803f1101803f1101c0" +
            @"42040000002701ffff030000000000}";

        private const string RtfHeader =
            // RTF identifier
            @"{\rtf1" +
            // Character encoding (not sure if this matters since we're escaping all non-ASCII chars anyway)
            @"\ansi\ansicpg1252" +
            // Fonts (use a pleasant sans-serif)
            @"\deff0{\fonttbl{\f0\fswiss\fcharset0 Arial;}{\f1\fnil\fcharset0 Arial;}{\f2\fnil\fcharset0 Calibri;}}" +
            // Set up red color
            @"{\colortbl ;\red255\green0\blue0;}" +
            // viewkind4 = normal, uc1 = 1 char Unicode fallback (don't worry about it), f0 = use font 0 I guess?
            @"\viewkind4\uc1\f0 ";

        private const string AlphaCaps = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string AlphaLower = "abcdefghijklmnopqrstuvwxyz";

        private static bool IsAlphaCaps(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!AlphaCaps.Contains(str[i])) return false;
            }
            return true;
        }

        private static string GLMLToRTF(string text)
        {
            var sb = new StringBuilder();

            sb.Append(RtfHeader);

            #region Parse and copy

            // Dunno if this would be considered a "good parser", but it's 20x faster than the regex method and
            // just as accurate, so hey.

            // If we have a \qc (center-align) tag active, we can't put a \ql (left-align) tag on the same line
            // after it or it will retroactively apply itself to the line. Because obviously you can't just have
            // a simple pair of tags that just do what they're told.
            bool alignLeftOnNextLine = false;
            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '[')
                {
                    // In the unlikely event that a tag is escaped, just ignore it
                    if ((i == 1 && text[i - 1] == '\\') ||
                        (i > 1 && text[i - 1] == '\\' && text[i - 2] != '\\'))
                    {
                        sb.Append('[');
                        continue;
                    }

                    // Opening tags
                    if (i < text.Length - 5 && text[i + 1] == 'G' && text[i + 2] == 'L')
                    {
                        var subSB = new StringBuilder();
                        for (int j = i + 3; j < Math.Min(i + 33, text.Length); j++)
                        {
                            if (text[j] == ']')
                            {
                                var tag = subSB.ToString();
                                if (tag == "TITLE")
                                {
                                    sb.Append(@"\fs36\b ");
                                }
                                else if (tag == "SUBTITLE")
                                {
                                    sb.Append(@"\fs28\b ");
                                }
                                else if (tag == "CENTER")
                                {
                                    sb.Append(@"\qc ");
                                }
                                else if (tag == "WARNINGS")
                                {
                                    sb.Append(@"\cf1\b ");
                                }
                                else if (tag == "NL")
                                {
                                    sb.Append(@"\line ");
                                    if (alignLeftOnNextLine)
                                    {
                                        sb.Append(@"\ql ");
                                        alignLeftOnNextLine = false;
                                    }
                                }
                                else if (tag == "LINE")
                                {
                                    sb.Append(HorizontalLine);
                                }
                                else if (!IsAlphaCaps(tag))
                                {
                                    sb.Append("[GL");
                                    sb.Append(subSB);
                                    sb.Append(']');
                                }
                                i = j;
                                break;
                            }
                            else
                            {
                                subSB.Append(text[j]);
                            }
                        }
                    }
                    // Closing tags
                    else if (i < text.Length - 6 &&
                             text[i + 1] == '/' && text[i + 2] == 'G' && text[i + 3] == 'L')
                    {
                        var subSB = new StringBuilder();
                        for (int j = i + 4; j < Math.Min(i + 34, text.Length); j++)
                        {
                            if (text[j] == ']')
                            {
                                var tag = subSB.ToString();
                                if (tag == "TITLE")
                                {
                                    sb.Append(@"\b0\fs24 ");
                                }
                                else if (tag == "SUBTITLE")
                                {
                                    sb.Append(@"\b0\fs24 ");
                                }
                                else if (tag == "CENTER")
                                {
                                    alignLeftOnNextLine = true;
                                }
                                else if (tag == "WARNINGS")
                                {
                                    sb.Append(@"\b0\cf0 ");
                                }
                                else if (tag == "LANGUAGE")
                                {
                                    sb.Append(@"\line\line ");
                                }
                                else if (!IsAlphaCaps(tag))
                                {
                                    sb.Append("[/GL");
                                    sb.Append(subSB);
                                    sb.Append(']');
                                }
                                i = j;
                                break;
                            }
                            else
                            {
                                subSB.Append(text[j]);
                            }
                        }
                    }
                    else
                    {
                        sb.Append('[');
                    }
                }
                else if (c == '&')
                {
                    var subSB = new StringBuilder();

                    // HTML Unicode numeric character references
                    if (i < text.Length - 4 && text[i + 1] == '#')
                    {
                        var end = Math.Min(i + 12, text.Length);
                        for (int j = i + 2; i < end; j++)
                        {
                            if (j == i + 2 && text[j] == 'x')
                            {
                                end = Math.Min(end + 1, text.Length);
                                subSB.Append(text[j]);
                            }
                            else if (text[j] == ';')
                            {
                                var num = subSB.ToString();

                                var success = num.Length > 0 && num[0] == 'x'
                                    ? int.TryParse(num.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result)
                                    : int.TryParse(num, out result);

                                if (success)
                                {
                                    sb.Append(@"\u" + result + "?");
                                }
                                else
                                {
                                    sb.Append("&#");
                                    sb.Append(subSB);
                                    sb.Append(';');
                                }
                                i = j;
                                break;
                            }
                            else
                            {
                                subSB.Append(text[j]);
                            }
                        }
                    }
                    // HTML Unicode named character references
                    else if (i < text.Length - 3 && (AlphaCaps.Contains(text[i + 1]) || AlphaLower.Contains(text[i + 1])))
                    {
                        for (int j = i + 1; i < text.Length; j++)
                        {
                            if (text[j] == ';')
                            {
                                var name = subSB.ToString();

                                if (HTML5NamedEntities.TryGetValue(name, out string value))
                                {
                                    sb.Append(@"\u" + value + "?");
                                }
                                else
                                {
                                    sb.Append("&");
                                    sb.Append(subSB);
                                    sb.Append(';');
                                }
                                i = j;
                                break;
                            }
                            else if (!AlphaCaps.Contains(text[j]) && !AlphaLower.Contains(text[j]))
                            {
                                sb.Append('&');
                                sb.Append(subSB);
                                sb.Append(text[j]);
                                i = j;
                                break;
                            }
                            else
                            {
                                subSB.Append(text[j]);
                            }
                        }
                    }
                    else
                    {
                        sb.Append('&');
                    }
                }
                else if (c > 127)
                {
                    sb.Append(@"\u" + (int)c + "?");
                }
                else
                {
                    if (c == '\\' || c == '{' || c == '}') sb.Append('\\');
                    sb.Append(c);
                }
            }

            #endregion

            sb.Append('}');

            return sb.ToString();
        }

        #endregion
    }
}

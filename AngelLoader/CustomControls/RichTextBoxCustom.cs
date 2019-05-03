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

        private float _storedZoomFactor = 1.0f;
        internal float StoredZoomFactor
        {
            get => _storedZoomFactor;
            set => _storedZoomFactor = value.Clamp(0.1f, 5.0f);
        }

        #region Zoom stuff

        internal void ZoomIn()
        {
            try
            {
                ZoomFactor = (ZoomFactor + 0.1f).Clamp(0.1f, 5.0f);
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
                ZoomFactor = (ZoomFactor - 0.1f).Clamp(0.1f, 5.0f);
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
                StoredZoomFactor = ZoomFactor.Clamp(0.1f, 5.0f);
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
                ZoomFactor = StoredZoomFactor.Clamp(0.1f, 5.0f);
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

        private static readonly byte[] shppict = Encoding.ASCII.GetBytes(@"\shppict");
        private static readonly byte[] shppictBlanked = Encoding.ASCII.GetBytes(@"\xxxxxxx");
        private static readonly byte[] nonshppict = Encoding.ASCII.GetBytes(@"\nonshppict");
        private static readonly byte[] nonshppictBlanked = Encoding.ASCII.GetBytes(@"\xxxxxxxxxx");

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
                // On Windows 10 at least, images don't display if we're ReadOnly. Why not. We need to be ReadOnly
                // though - it doesn't make sense to let the user edit a readme - so un-set us just long enough
                // to load in the content correctly, then set us back again.
                ReadOnly = false;

                // Blank the text to reset the scroll position to the top
                Clear();

                if (path.ExtIsGlml())
                {
                    var text = File.ReadAllText(path);
                    Rtf = GLMLToRTF(text);
                }
                else if (fileType == RichTextBoxStreamType.RichText)
                {
                    // Use ReadAllBytes and byte[] search, because ReadAllText and string.Replace is ~30x slower
                    var bytes = File.ReadAllBytes(path);

                    ReplaceByteSequence(bytes, shppict, shppictBlanked);
                    ReplaceByteSequence(bytes, nonshppict, nonshppictBlanked);

                    using (var ms = new MemoryStream(bytes)) LoadFile(ms, RichTextBoxStreamType.RichText);
                }
                else
                {
                    LoadFile(path, fileType);
                }
            }
            finally
            {
                ReadOnly = true;
                RestoreZoom();
                this.ResumeDrawing();
            }
        }

        /*
        Alright kids, gather round while your ol' Grandpa Fen explains you the deal.
        You can choose two different versions of RichTextBox. Old (3.0) or new (4.1). Both have their own unique
        and beautiful ways of driving you up the wall.
        Old:
        -Garbles right side of horizontal lines when you scale them out too far.
        -Flickers while scrolling if and only if another control is laid overtop of it.
        +Displays image transparency correctly.
        New:
        +Doesn't flicker while scrolling even when controls are laid overtop of it.
        +Doesn't garble the right edge of scaled-out horizontal lines no matter how far you scale.
        -Displays image transparency as pure black.
        -There's a compatibility option "\transmf" that looks like it would fix the above, but guess what, it's
         not supported.

        To stop the new version's brazen headlong charge straight off the edge of Mount Compatible, we replace all
        instances of "\shppict" and "\nonshppict" with dummy strings. This fixes the problem. Hooray. Now get off
        my lawn.
        */

        internal static void ReplaceByteSequence(byte[] input, byte[] pattern, byte[] replacePattern)
        {
            var firstByte = pattern[0];
            int index = Array.IndexOf(input, firstByte);
            var pLen = pattern.Length;

            while (index > -1)
            {
                for (int i = 0; i < pLen; i++)
                {
                    if (index + i >= input.Length) return;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return;
                        break;
                    }

                    if (i == pLen - 1)
                    {
                        for (int j = index, ri = 0; j < index + pLen; j++, ri++)
                        {
                            input[j] = replacePattern[ri];
                        }
                    }
                }
            }
        }

        #region GLML to RTF

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

        // RichTextBox steadfastly refuses to understand the normal way of drawing lines, so use a small image
        // and scale the width out
        private const string HorizontalLineImagePart =
            @"0100090000039000000000006700000000000400000003010800050000000b0200000000050000" +
            @"000c0213000200030000001e000400000007010400040000000701040067000000410b2000cc00" +
            @"130002000000000013000200000000002800000002000000130000000100040000000000000000" +
            @"000000000000000000000000000000000000000000ffffff006666660000000000000000000000" +
            @"000000000000000000000000000000000000000000000000000000000000000000000000000000" +
            @"0000001101d503110100001101d503110100001101bafd11010000110100001101000011010000" +
            @"2202000011010000110100001101000011010000110100001101803f1101803f1101803f1101c0" +
            @"42040000002701ffff030000000000}\line ";

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
            // Now that we're using the latest RichEdit version again, we can go back to just scaling out to a
            // zillion. And we need to, because DPI is involved or something (or maybe Win10 is just different)
            // and the double-screen-width method doesn't give a consistent width anymore.
            // width and height are in twips, 30 twips = 2 pixels, 285 twips = 19 pixels, etc. (at 96 dpi)
            // picscalex is in percent
            // max value for anything is 32767
            const string HorizontalLine =
                @"{\pict\wmetafile8\picw30\pich285\picwgoal32767\pichgoal285\picscalex1600 " +
                HorizontalLineImagePart;

            var sb = new StringBuilder();

            sb.Append(RtfHeader);

            #region Parse and copy

            // Dunno if this would be considered a "good parser", but it's 20x faster than the regex method and
            // just as accurate, so hey.

            // If we have a \qc (center-align) tag active, we can't put a \ql (left-align) tag on the same line
            // after it or it will retroactively apply itself to the line. Because obviously you can't just have
            // a simple pair of tags that just do what they're told.
            bool alignLeftOnNextLine = false;
            bool lastTagWasLineBreak = false;
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
                                    lastTagWasLineBreak = true;
                                    if (alignLeftOnNextLine)
                                    {
                                        // For newest rtfbox on Win10, we need to use \par instead of \line or
                                        // else the \ql doesn't take. This works on Win7 too.
                                        sb.Append(@"\par\ql ");
                                        alignLeftOnNextLine = false;
                                    }
                                    else
                                    {
                                        sb.Append(@"\line ");
                                    }
                                }
                                else if (tag == "LINE")
                                {
                                    if (!lastTagWasLineBreak) sb.Append(@"\line ");
                                    sb.Append(HorizontalLine);
                                }
                                else if (!IsAlphaCaps(tag))
                                {
                                    sb.Append("[GL");
                                    sb.Append(subSB);
                                    sb.Append(']');
                                }

                                if (tag != "NL") lastTagWasLineBreak = false;

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

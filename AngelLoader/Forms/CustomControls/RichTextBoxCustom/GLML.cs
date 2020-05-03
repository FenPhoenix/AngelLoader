using System;
using System.Globalization;
using System.Text;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom
    {
        private static string GLMLToRTF(string text)
        {
            // ReSharper disable StringLiteralTypo
            // ReSharper disable CommentTypo

            const string RtfHeader =
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

            // RichTextBox steadfastly refuses to understand the normal way of drawing lines, so use a small image
            // and scale the width out
            const string HorizontalLineImagePart =
                @"0100090000039000000000006700000000000400000003010800050000000b0200000000050000" +
                @"000c0213000200030000001e000400000007010400040000000701040067000000410b2000cc00" +
                @"130002000000000013000200000000002800000002000000130000000100040000000000000000" +
                @"000000000000000000000000000000000000000000ffffff006666660000000000000000000000" +
                @"000000000000000000000000000000000000000000000000000000000000000000000000000000" +
                @"0000001101d503110100001101d503110100001101bafd11010000110100001101000011010000" +
                @"2202000011010000110100001101000011010000110100001101803f1101803f1101803f1101c0" +
                @"42040000002701ffff030000000000}\line ";

            // Now that we're using the latest RichEdit version again, we can go back to just scaling out to a
            // zillion. And we need to, because DPI is involved or something (or maybe Win10 is just different)
            // and the double-screen-width method doesn't give a consistent width anymore.
            // width and height are in twips, 30 twips = 2 pixels, 285 twips = 19 pixels, etc. (at 96 dpi)
            // picscalex is in percent
            // max value for anything is 32767
            const string HorizontalLine =
                @"{\pict\wmetafile8\picw30\pich285\picwgoal32767\pichgoal285\picscalex1600 " +
                HorizontalLineImagePart;

            // ReSharper restore CommentTypo
            // ReSharper restore StringLiteralTypo

            var sb = new StringBuilder();
            var subSB = new StringBuilder();

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
                char c = text[i];
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
                        subSB.Clear();
                        for (int j = i + 3; j < Math.Min(i + 33, text.Length); j++)
                        {
                            if (text[j] == ']')
                            {
                                string tag = subSB.ToString();
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
                                else if (!tag.IsAsciiAlphaUpper())
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
                        subSB.Clear();
                        for (int j = i + 4; j < Math.Min(i + 34, text.Length); j++)
                        {
                            if (text[j] == ']')
                            {
                                string tag = subSB.ToString();
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
                                else if (!tag.IsAsciiAlphaUpper())
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
                    subSB.Clear();

                    // HTML Unicode numeric character references
                    if (i < text.Length - 4 && text[i + 1] == '#')
                    {
                        int end = Math.Min(i + 12, text.Length);
                        for (int j = i + 2; i < end; j++)
                        {
                            if (j == i + 2 && text[j] == 'x')
                            {
                                end = Math.Min(end + 1, text.Length);
                                subSB.Append(text[j]);
                            }
                            else if (text[j] == ';')
                            {
                                string num = subSB.ToString();

                                bool success = num.Length > 0 && num[0] == 'x'
                                    ? int.TryParse(num.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result)
                                    : int.TryParse(num, out result);

                                if (success)
                                {
                                    sb.Append(@"\u");
                                    sb.Append(result.ToString());
                                    sb.Append('?');
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
                    else if (i < text.Length - 3 && text[i + 1].IsAsciiAlpha())
                    {
                        for (int j = i + 1; i < text.Length; j++)
                        {
                            if (text[j] == ';')
                            {
                                string name = subSB.ToString();

                                if (HTMLNamedEntities.Entities.TryGetValue(name, out string value))
                                {
                                    sb.Append(@"\u");
                                    sb.Append(value);
                                    sb.Append('?');
                                }
                                else
                                {
                                    sb.Append('&');
                                    sb.Append(subSB);
                                    sb.Append(';');
                                }
                                i = j;
                                break;
                            }
                            // Support named references with numbers somewhere after their first char ("blk34" for instance)
                            else if (!text[j].IsAsciiAlphanumeric())
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
                    sb.Append(@"\u");
                    sb.Append(((int)c).ToString());
                    sb.Append('?');
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
    }
}

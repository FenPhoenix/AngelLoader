using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;

namespace AngelLoader;

internal static class GLMLConversion
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    internal static string GLMLToRTF(byte[] glmlBytes, bool darkMode)
    {
        // IMPORTANT: Use Encoding.UTF8 because anything else will break the character encoding!
        string glml = Encoding.UTF8.GetString(glmlBytes);

        static string AddColorToTable(string table, Color color) =>
            table +
            @"\red" + color.R.ToStrInv() +
            @"\green" + color.G.ToStrInv() +
            @"\blue" + color.B.ToStrInv() +
            ";";

        string colorTable = @"{\colortbl ";
        colorTable = darkMode
            ? AddColorToTable(colorTable, DarkColors.Fen_DarkForeground)
            : colorTable + ";";
        colorTable = AddColorToTable(colorTable, darkMode ? DarkColors.GLMLRed_Dark : DarkColors.GLMLRed_Light);
        colorTable += "}";

        string rtfHeader =
            // RTF identifier
            @"{\rtf1" +
            // Character encoding (not sure if this matters since we're escaping all non-ASCII chars anyway)
            @"\ansi\ansicpg1252" +
            // Fonts (use a pleasant sans-serif)
            @"\deff0{\fonttbl{\f0\fswiss\fcharset0 Arial{\*\falt Calibri};}}" +
            // Set up red color and dark mode text colors
            colorTable +
            // \viewkind4 = normal, \uc1 = 1 char Unicode fallback (don't worry about it), \f0 = use font 0,
            // \cf0 = use color 0 as the foreground color
            @"\viewkind4\uc1\f0" + (darkMode ? @"\cf0 " : " ");

        string horizontalLine = RtfProcessing.GetThemedHorizontalLine(darkMode);

        // In quick testing, smallest final rtf size was ~8K chars and largest was ~38K chars. Preallocating
        // 16K chars reduces GC time substantially. 40K or something may be fine but this works for now.
        var sb = new StringBuilder(ByteSize.KB * 16);

        // 16 chars is the default starting capacity. The longest known tag name is "FMSTRUCTURE" at 11 chars,
        // so that's more than enough. We're passing 16 explicitly just in case it ever changes under the hood.
        var subSB = new StringBuilder(16);

        sb.Append(rtfHeader);

        #region Parse and copy

        // Note about Unicode: The RTF spec seems to imply that \uN words' parameters are int16s, but in fact
        // int32s work perfectly fine (verified by putting the tiger-face emoji in a .glml file). So we don't
        // need to translate to negative values and multiple \uN words or anything like that. It just works
        // as it is now.

        // If we have a \qc (center-align) tag active, we can't put a \ql (left-align) tag on the same line
        // after it or it will retroactively apply itself to the line. Because obviously you can't just have
        // a simple pair of tags that just do what they're told.
        bool alignLeftOnNextLine = false;
        bool lastTagWasLineBreak = false;
        for (int i = 0; i < glml.Length; i++)
        {
            char c = glml[i];
            if (c == '[')
            {
                /*
                GLML has no escape mechanism - if you hand-write [GLNL] for example, it just saves it out
                as-is and it becomes a tag. It doesn't go like \[GLNL\] or anything. So we don't have to
                handle escaping at all.
                */
                // Opening tags
                if (i < glml.Length - 5 && glml[i + 1] == 'G' && glml[i + 2] == 'L')
                {
                    subSB.Clear();
                    for (int j = i + 3; j < Math.Min(i + 33, glml.Length); j++)
                    {
                        if (glml[j] == ']')
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
                                sb.Append(horizontalLine);
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
                            subSB.Append(glml[j]);
                        }
                    }
                }
                // Closing tags
                else if (i < glml.Length - 6 &&
                         glml[i + 1] == '/' && glml[i + 2] == 'G' && glml[i + 3] == 'L')
                {
                    subSB.Clear();
                    for (int j = i + 4; j < Math.Min(i + 34, glml.Length); j++)
                    {
                        if (glml[j] == ']')
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
                            subSB.Append(glml[j]);
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
                if (i < glml.Length - 4 && glml[i + 1] == '#')
                {
                    int end = Math.Min(i + 12, glml.Length);
                    for (int j = i + 2; i < end; j++)
                    {
                        if (j == i + 2 && glml[j] == 'x')
                        {
                            end = Math.Min(end + 1, glml.Length);
                            subSB.Append(glml[j]);
                        }
                        else if (glml[j] == ';')
                        {
                            string num = subSB.ToString();

                            bool success = num.Length > 0 && num[0] == 'x'
                                ? int.TryParse(num.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int result)
                                : Int_TryParseInv(num, out result);

                            if (success)
                            {
                                sb.Append(@"\u");
                                sb.Append(result.ToStrInv());
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
                            subSB.Append(glml[j]);
                        }
                    }
                }
                // HTML Unicode named character references
                else if (i < glml.Length - 3 && glml[i + 1].IsAsciiAlpha())
                {
                    for (int j = i + 1; i < glml.Length; j++)
                    {
                        if (glml[j] == ';')
                        {
                            string name = subSB.ToString();

                            if (HTML.HTML401NamedEntities.TryGetValue(name, out string value))
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
                        else if (!glml[j].IsAsciiAlphanumeric())
                        {
                            sb.Append('&');
                            sb.Append(subSB);
                            sb.Append(glml[j]);
                            i = j;
                            break;
                        }
                        else
                        {
                            subSB.Append(glml[j]);
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
                sb.Append(((int)c).ToStrInv());
                sb.Append('?');
            }
            else
            {
                if (c is '\\' or '{' or '}') sb.Append('\\');
                sb.Append(c);
            }
        }

        #endregion

        if (darkMode)
        {
            // Background code at the end again, cause why not, it works
            sb.Append(RtfProcessing.RTF_DarkBackgroundString);
        }

        sb.Append('}');

        return sb.ToString();
    }
}

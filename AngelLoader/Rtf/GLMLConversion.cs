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
    internal static string GLMLToRTF(byte[] glmlBytes, bool darkModeEnabled)
    {
        // IMPORTANT: Use Encoding.UTF8 because anything else will break the character encoding!
        string glml = Encoding.UTF8.GetString(glmlBytes);

        static string AddColorToTable(string table, Color color) => table + @"\red" + color.R + @"\green" + color.G + @"\blue" + color.B + ";";

        string colorTable = @"{\colortbl ";
        colorTable = darkModeEnabled
            ? AddColorToTable(colorTable, DarkColors.Fen_DarkForeground)
            : colorTable + ";";
        colorTable = AddColorToTable(colorTable, darkModeEnabled ? DarkColors.GLMLRed_Dark : DarkColors.GLMLRed_Light);
        colorTable += "}";

        string rtfHeader =
            // RTF identifier
            @"{\rtf1" +
            // Character encoding (not sure if this matters since we're escaping all non-ASCII chars anyway)
            @"\ansi\ansicpg1252" +
            // Fonts (use a pleasant sans-serif)
            @"\deff0{\fonttbl{\f0\fswiss\fcharset0 Arial;}{\f1\fnil\fcharset0 Arial;}{\f2\fnil\fcharset0 Calibri;}}" +
            // Set up red color and dark mode text colors
            colorTable +
            // \viewkind4 = normal, \uc1 = 1 char Unicode fallback (don't worry about it), \f0 = use font 0,
            // \cf0 = use color 0 as the foreground color
            @"\viewkind4\uc1\f0" + (darkModeEnabled ? @"\cf0 " : " ");

        #region Horizontal line setup

        // RichTextBox steadfastly refuses to understand the normal way of drawing lines, so use a small image
        // and scale the width out.
        // Now that we're using the latest RichEdit version again, we can go back to just scaling out to a
        // zillion. And we need to, because DPI is involved or something (or maybe Win10 is just different)
        // and the double-screen-width method doesn't give a consistent width anymore.
        // width and height are in twips, 30 twips = 2 pixels, 285 twips = 19 pixels, etc. (at 96 dpi)
        // picscalex is in percent
        // max value for anything is 32767
        const string horizontalLine_Header =
            @"{\pict\pngblip\picw30\pich285\picwgoal32767\pichgoal285\picscalex1600 ";

        const string horizontalLine_Footer = @"}\line ";

        // These are raw hex bytes straight out of the original png files. Too bad they're pngs and thus we
        // can't easily modify their colors on the fly without writing a png creator, but I don't think RTF
        // supports transparency on anything uncompressed.
        const string horizontalLine_LightMode =
            horizontalLine_Header +
            "89504E470D0A1A0A0000000D4948445200000002000000130806000000BA3CDC1A00000020494441" +
            "5478DA62FCFFFF3F030830314001850CC6909010B0898CD4361920C0009E400819AEAF5DA1000000" +
            "0049454E44AE426082" +
            horizontalLine_Footer;

        const string horizontalLine_DarkMode =
            horizontalLine_Header +
            "89504E470D0A1A0A0000000D4948445200000002000000130806000000BA3CDC1A00000025494441" +
            "5478DA62FAFFFF3F030833314001850C9693274FFE07311841A652C140380320C00005DF0C79948E" +
            "11520000000049454E44AE426082" +
            horizontalLine_Footer;

        string horizontalLine = darkModeEnabled ? horizontalLine_DarkMode : horizontalLine_LightMode;

        #endregion

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

                            if (HTML.HTMLNamedEntities.TryGetValue(name, out string? value))
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

        if (darkModeEnabled)
        {
            // Background code at the end again, cause why not, it works
            sb.Append(RtfProcessing.RTF_DarkBackgroundString);
        }

        sb.Append('}');

        return sb.ToString();
    }
}

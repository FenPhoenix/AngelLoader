//#define PROCESS_README_TIME_TEST

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.RtfDisplayedReadmeParser;

namespace AngelLoader;

internal static class RtfProcessing
{
    #region Private fields

    #region Horizontal line setup

    // RichTextBox steadfastly refuses to understand the normal way of drawing lines, so use a small image
    // and scale the width out.
    // Now that we're using the latest RichEdit version again, we can go back to just scaling out to a
    // zillion. And we need to, because DPI is involved or something (or maybe Win10 is just different)
    // and the double-screen-width method doesn't give a consistent width anymore.
    // width and height are in twips, 30 twips = 2 pixels, 285 twips = 19 pixels, etc. (at 96 dpi)
    // picscalex is in percent
    // max value for anything is 32767
    private const string _horizontalLine_Header =
        @"{\pict\pngblip\picw30\pich285\picwgoal32767\pichgoal285\picscalex1600 ";

    private const string _horizontalLine_Footer = @"}\line ";

    // These are raw hex bytes straight out of the original png files. Too bad they're pngs and thus we
    // can't easily modify their colors on the fly without writing a png creator, but I don't think RTF
    // supports transparency on anything uncompressed.
    private const string HorizontalLine_LightMode =
        _horizontalLine_Header +
        "89504E470D0A1A0A0000000D4948445200000002000000130806000000BA3CDC1A00000020494441" +
        "5478DA62FCFFFF3F030830314001850CC6909010B0898CD4361920C0009E400819AEAF5DA1000000" +
        "0049454E44AE426082" +
        _horizontalLine_Footer;

    private const string HorizontalLine_DarkMode =
        _horizontalLine_Header +
        "89504E470D0A1A0A0000000D4948445200000002000000130806000000BA3CDC1A00000025494441" +
        "5478DA62FAFFFF3F030833314001850C9693274FFE07311841A652C140380320C00005DF0C79948E" +
        "11520000000049454E44AE426082" +
        _horizontalLine_Footer;

    internal static string GetThemedHorizontalLine(bool darkMode) => darkMode
        ? HorizontalLine_DarkMode
        : HorizontalLine_LightMode;

    #endregion

    // Static because we're very likely to need it a lot (for every rtf readme in dark mode), and we don't
    // want to make a new one every time.
    private static RtfDisplayedReadmeParser? _rtfDisplayedReadmeParser;
    private static RtfDisplayedReadmeParser RtfDisplayedReadmeParser => _rtfDisplayedReadmeParser ??= new RtfDisplayedReadmeParser();

    #region Colors

    private static readonly byte[] _colortbl =
    {
        (byte)'{',
        (byte)'\\',
        (byte)'c',
        (byte)'o',
        (byte)'l',
        (byte)'o',
        (byte)'r',
        (byte)'t',
        (byte)'b',
        (byte)'l',
    };

    private static readonly byte[] _redFieldBytes =
    {
        (byte)'\\',
        (byte)'r',
        (byte)'e',
        (byte)'d',
    };

    private static readonly byte[] _greenFieldBytes =
    {
        (byte)'\\',
        (byte)'g',
        (byte)'r',
        (byte)'e',
        (byte)'e',
        (byte)'n',
    };

    private static readonly byte[] _blueFieldBytes =
    {
        (byte)'\\',
        (byte)'b',
        (byte)'l',
        (byte)'u',
        (byte)'e',
    };

    private static readonly byte[] _background =
    {
        (byte)'\\',
        (byte)'*',
        (byte)'\\',
        (byte)'b',
        (byte)'a',
        (byte)'c',
        (byte)'k',
        (byte)'g',
        (byte)'r',
        (byte)'o',
        (byte)'u',
        (byte)'n',
        (byte)'d',
    };

    private static readonly byte[] _backgroundBlanked =
    {
        (byte)'\\',
        (byte)'*',
        (byte)'\\',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
    };

    private static readonly ListFast<byte> _colorNumberBytes = new(3);

    #endregion

    #region Langs

    // +1 for adding a space after the digits
    private static readonly ListFast<byte> _codePageBytes = new(RTFParserCommon.MaxLangNumDigits + 1);

    private static readonly byte[] _lang =
    {
        (byte)'\\',
        (byte)'l',
        (byte)'a',
        (byte)'n',
        (byte)'g',
    };

    private static readonly byte[] _ansicpg =
    {
        (byte)'\\',
        (byte)'a',
        (byte)'n',
        (byte)'s',
        (byte)'i',
        (byte)'c',
        (byte)'p',
        (byte)'g',
    };

    #endregion

    #endregion

    internal static readonly string RTF_DarkBackgroundString = @"{\*\background{\shp{\*\shpinst{\sp{\sn fillColor}{\sv "
                                                               + ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground).ToStrInv()
                                                               + "}}}}}";
    private static readonly byte[] RTF_DarkBackgroundBytes = Encoding.ASCII.GetBytes(RTF_DarkBackgroundString);

    private static ListFast<byte> CreateColorTableRTFBytes(List<Color>? colorTable)
    {
        #region Local functions

        // One file (In These Enlightened Times) had some hidden (white-on-white) text, so make that match
        // our new background color to keep author intent (avoiding spoilers etc.)
        static bool ColorIsTheSameAsBackground(Color color) => color is { R: 255, G: 255, B: 255 };

        static ListFast<byte> ByteToASCIICharBytes(byte number)
        {
            // Use global 3-byte list and do allocation-less clears and inserts, otherwise we would allocate
            // a new byte array EVERY time through here (which is a lot)
            _colorNumberBytes.ClearFast();

            int digits = number <= 9 ? 1 : number <= 99 ? 2 : 3;

            for (int i = 0; i < digits; i++)
            {
                _colorNumberBytes.InsertAtZeroFast((byte)((number % 10) + '0'));
                number /= 10;
            }

            return _colorNumberBytes;
        }

        #endregion

        const int maxColorEntryStringLength = 25; // "\red255\green255\blue255;" = 25 chars

        // Size us large enough that we don't reallocate
        var colorEntriesBytesList = new ListFast<byte>(
            _colortbl.Length +
            (maxColorEntryStringLength * colorTable?.Count ?? 0)
            + 2);

        colorEntriesBytesList.AddRange_Large(_colortbl);

        if (colorTable != null)
        {
            for (int i = 0; i < colorTable.Count; i++)
            {
                Color invertedColor;
                Color currentColor = colorTable[i];
                if (i == 0 && currentColor.A == 0)
                {
                    // We can just do the standard thing now, because with the sys color hook our default color
                    // is now our bright foreground color
                    colorEntriesBytesList.Add((byte)';');
                    continue;
                }
                // Set pure black to custom-white (not pure white), otherwise it would invert around to pure
                // white and that's a bit too bright.
                else if (currentColor is { R: 0, G: 0, B: 0 })
                {
                    invertedColor = DarkColors.Fen_DarkForeground;
                }
                else if (ColorIsTheSameAsBackground(currentColor))
                {
                    invertedColor = DarkColors.Fen_DarkBackground;
                }
                else
                {
                    invertedColor = ColorUtils.InvertLightness(currentColor);

                    // For some reason RTF doesn't accept a \cfN if the color is 255 all around, it has to be
                    // 254 or less... don't ask me
                    if (invertedColor is { R: 255, G: 255, B: 255 })
                    {
                        invertedColor = Color.FromArgb(254, 254, 254);
                    }
                }

                colorEntriesBytesList.AddRange_Large(_redFieldBytes);
                colorEntriesBytesList.AddRange_Large(ByteToASCIICharBytes(invertedColor.R));

                colorEntriesBytesList.AddRange_Large(_greenFieldBytes);
                colorEntriesBytesList.AddRange_Large(ByteToASCIICharBytes(invertedColor.G));

                colorEntriesBytesList.AddRange_Large(_blueFieldBytes);
                colorEntriesBytesList.AddRange_Large(ByteToASCIICharBytes(invertedColor.B));

                colorEntriesBytesList.Add((byte)';');
            }
        }

        colorEntriesBytesList.Add((byte)'}');

        return colorEntriesBytesList;
    }

    internal static byte[] GetProcessedRTFBytes(byte[] currentReadmeBytes, bool darkMode)
    {
        // Avoid allocations as much as possible here, because glibly converting back and forth between lists
        // and arrays for our readme bytes is going to blow out memory.

        #region Local functions

        static (int Start, int End) FindIndexOfLangWithNum(byte[] input, int start = 0)
        {
            byte firstByte = _lang[0];
            int index = Array.IndexOf(input, firstByte, start);

            while (index > -1)
            {
                for (int i = 0; i < _lang.Length; i++)
                {
                    if (index + i >= input.Length) return (-1, -1);
                    if (_lang[i] != input[index + i])
                    {
                        if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return (-1, -1);
                        break;
                    }

                    if (i == _lang.Length - 1)
                    {
                        int firstDigitIndex = index + i + 1;

                        int numIndex = firstDigitIndex;
                        while (numIndex < input.Length - 1 && input[numIndex].IsAsciiNumeric())
                        {
                            numIndex++;
                        }
                        if (numIndex > firstDigitIndex)
                        {
                            return (index, numIndex);
                        }
                        else
                        {
                            index = numIndex;
                        }
                    }
                }
            }

            return (-1, -1);
        }

        #endregion

#if PROCESS_README_TIME_TEST
        TimeSpan totalTime = TimeSpan.Zero;
#endif

        /*
        It's much faster on average to run pre-checks and be able to avoid an expensive parse.
        We don't want to severely degrade every rtf readme's load speed just because some of them need parsing.
        */

        bool colorTableWorkRequired = false;
        bool langWorkRequired = false;

        if (darkMode)
        {
            #region Precheck for \colortbl

#if PROCESS_README_TIME_TEST
            var preCheckForColorTableTimer = new System.Diagnostics.Stopwatch();
            preCheckForColorTableTimer.Start();
#endif

            colorTableWorkRequired = FindIndexOfByteSequence(currentReadmeBytes, _colortbl) > -1;

#if PROCESS_README_TIME_TEST
            preCheckForColorTableTimer.Stop();
            var preCheckForColorTableTimerElapsed = preCheckForColorTableTimer.Elapsed;
            totalTime = totalTime.Add(preCheckForColorTableTimerElapsed);
            System.Diagnostics.Trace.WriteLine(nameof(preCheckForColorTableTimer) + " took:\r\n" + preCheckForColorTableTimerElapsed);
#endif

            #endregion
        }

        #region Precheck for \lang fixing work required

#if PROCESS_README_TIME_TEST
        var preCheckForLangsTimer = new System.Diagnostics.Stopwatch();
        preCheckForLangsTimer.Start();
#endif

        int startFrom = 0;
        while (startFrom > -1 && startFrom < currentReadmeBytes.Length - 1)
        {
            (int start, int end) = FindIndexOfLangWithNum(currentReadmeBytes, startFrom);
            if ((start | end) > -1 &&
                end - (start + 5) <= RTFParserCommon.MaxLangNumDigits)
            {
                int num = 0;
                for (int i = start + 5; i < end; i++)
                {
                    byte b = currentReadmeBytes[i];
                    if (b.IsAsciiNumeric())
                    {
                        num *= 10;
                        num += b - '0';
                    }
                }

                if (num <= RTFParserCommon.MaxLangNumIndex)
                {
                    int codePage = RTFParserCommon.LangToCodePage[num];
                    // The only known broken readmes only need code page 1251 to fix them, so for now let's just
                    // only support that, to exclude as many readmes as possible from an expensive full parse.
#if true
                    if (codePage is 1251)
#else
                    if (codePage is > -1 and not 1252)
#endif
                    {
                        langWorkRequired = true;
                        break;
                    }
                }
            }
            startFrom = end;
        }

#if PROCESS_README_TIME_TEST
        preCheckForLangsTimer.Stop();
        var preCheckForLangsTimerElapsed = preCheckForLangsTimer.Elapsed;
        totalTime = totalTime.Add(preCheckForLangsTimerElapsed);
        System.Diagnostics.Trace.WriteLine(nameof(preCheckForLangsTimer) + " took:\r\n" + preCheckForLangsTimerElapsed);
#endif

        #endregion

        #region Parse

#if PROCESS_README_TIME_TEST
        var parseTimer = new System.Diagnostics.Stopwatch();
        parseTimer.Start();
#endif

        (bool success, List<Color>? colorTable, List<LangItem>? langItems) =
            RtfDisplayedReadmeParser.GetData(
                new ArrayWithLength<byte>(currentReadmeBytes),
                getColorTable: colorTableWorkRequired,
                getLangs: langWorkRequired);

#if PROCESS_README_TIME_TEST
        parseTimer.Stop();
        var parseTimerElapsed = parseTimer.Elapsed;
        totalTime = totalTime.Add(parseTimerElapsed);
        System.Diagnostics.Trace.WriteLine(nameof(parseTimer) + " took:\r\n" + parseTimerElapsed);
#endif

        #endregion

#if PROCESS_README_TIME_TEST
        System.Diagnostics.Trace.WriteLine("Total time:\r\n" + totalTime);
#endif

        int colorTableEntryLength = 0;

        ListFast<byte>? colorEntriesBytesList = null;

        if (success && colorTableWorkRequired)
        {
            colorEntriesBytesList = CreateColorTableRTFBytes(colorTable);
            colorTableEntryLength = colorEntriesBytesList.Count;
        }

        int extraAnsiCpgCombinedLength = 0;
        int ansiCpgLength = _ansicpg.Length;

        if (!(success && langWorkRequired && langItems?.Count > 0) && !darkMode)
        {
            return currentReadmeBytes;
        }

        if (success && langWorkRequired && langItems?.Count > 0)
        {
            for (int i = 0; i < langItems.Count; i++)
            {
                LangItem item = langItems[i];
                item.Index += colorTableEntryLength;
                // +1 for adding a space after the digits
                extraAnsiCpgCombinedLength += ansiCpgLength + item.DigitsCount + 1;
            }
        }

        byte[] retBytes;
        if (darkMode)
        {
            int retBytesLength =
                currentReadmeBytes.Length +
                colorTableEntryLength +
                RTF_DarkBackgroundBytes.Length +
                extraAnsiCpgCombinedLength;
            retBytes = new byte[retBytesLength];

            int lastClosingBraceIndex = Array.LastIndexOf(currentReadmeBytes, (byte)'}');
            int firstIndexPastHeader = FindIndexOfByteSequence(currentReadmeBytes, RTFHeaderBytes) + RTFHeaderBytes.Length;
            // Because we're only matching "{\rtf" and there may or may not be a param, we need to make sure we
            // skip past the entire header.
            for (int i = firstIndexPastHeader; i < currentReadmeBytes.Length; i++)
            {
                if (!currentReadmeBytes[i].IsAsciiAlphanumeric())
                {
                    firstIndexPastHeader = i;
                    break;
                }
            }

            ReadOnlySpan<byte> currentReadmeBytesSpan = currentReadmeBytes.AsSpan();
            Span<byte> retBytesSpan = retBytes.AsSpan();

            ReadOnlySpan<byte> headerSpan = currentReadmeBytesSpan[..firstIndexPastHeader];
            headerSpan.CopyTo(retBytesSpan);

            int lastIndexSource = firstIndexPastHeader;
            int lastIndexDest = firstIndexPastHeader;

            // Copy color table
            // Fortunately, only the first color table is used, so we can just stick ourselves right at the start
            // and not even have to awkwardly delete the old color table.
            // Now watch Windows get an update that breaks that.
            // @DarkModeNote: We could add code to delete the old color table at some point.
            // This would make us some amount slower, and it's not necessary currently, so let's just not do it
            // for now.
            if (colorEntriesBytesList != null)
            {
                ReadOnlySpan<byte> colorTableSpan = colorEntriesBytesList.ItemsArray.AsSpan(0, colorTableEntryLength);
                colorTableSpan.CopyTo(retBytesSpan[lastIndexDest..retBytesLength]);
                lastIndexDest += colorTableEntryLength;
            }

            if (success && langWorkRequired && langItems?.Count > 0)
            {
                CopyInserts(langItems, currentReadmeBytesSpan, retBytesSpan, ansiCpgLength, ref lastIndexSource, ref lastIndexDest);
            }

            ReadOnlySpan<byte> bodyToLastClosingBrace = currentReadmeBytesSpan[lastIndexSource..lastClosingBraceIndex];
            bodyToLastClosingBrace.CopyTo(retBytesSpan[lastIndexDest..]);

            lastIndexSource += bodyToLastClosingBrace.Length;
            lastIndexDest += bodyToLastClosingBrace.Length;

            // Disable any backgrounds that may already be in there, otherwise we sometimes get visual artifacts
            // where the background stays the old color but turns to our new color when portions of the readme
            // get painted (see Thork).
            // Actually, Thork's readme is actually just weirdly broken, the background is sometimes yellow but
            // paints over with white even on classic mode. So oh well.
            // Do this BEFORE putting the dark background control word in, or else it will be overwritten too!
            ReplaceByteSequence(retBytes, _background, _backgroundBlanked);

            // Insert our dark background definition at the end, so we override any other backgrounds that may be set.
            ReadOnlySpan<byte> backgroundSpan = RTF_DarkBackgroundBytes.AsSpan();
            backgroundSpan.CopyTo(retBytesSpan[lastIndexDest..]);

            lastIndexDest += backgroundSpan.Length;

            currentReadmeBytesSpan[lastIndexSource..].CopyTo(retBytesSpan[lastIndexDest..]);

            return retBytes;

            #region Issues/quirks/etc.

            /*
            @DarkModeNote(RTF/DarkTextMode) issues/quirks/etc:
            -Image-as-first-item issue with the \cf0 inserts
             If we put a \cf0 before a transparent image, it makes the background of it white.
             See 2006-09-18_WC_WhatLiesBelow_v1
             Not a huge deal really - lots of readmes end up with bright images due to non-transparency, and
             WLB's transparent title image doesn't look good in dark mode anyway, but, you know...
            *Note: We don't put \cf0 inserts anymore, but the above still applies with having the default color
             be bright which is what we have now.
            -2022-07-01: The "white" is actually our dark mode default text color, which seems to affect
             transparent images. It seems that if you leave the rtf "default color" unhooked, then it makes the
             text black and the image transparent portions whatever color they should be (document background I
             guess). But if we hook the default color, now it makes the text AND transparent image backgrounds
             that color. Except I guess if the images are pngs or whatever the hell "proper" format it wants,
             then transparency works actually properly.

            -Beginning of Era Karath-Din:
             It has dark text on a not-quite-white background, which inverts to light text on an also bright
             background, due to us preventing downward lightness inversion. Probably too much trouble to fix,
             and worst case the user can always just select the text and it'll be visible, but note it...

            -missionx_v113patch.rtf (CoSaS2_MissionX_v113)
             This one has some text in boxes that's black-on-white. At least it's readable though, so not a show-
             stopper.
            */

            #endregion
        }
        else
        {
            if (success && langWorkRequired && langItems?.Count > 0)
            {
                retBytes = new byte[currentReadmeBytes.Length + extraAnsiCpgCombinedLength];

                ReadOnlySpan<byte> currentReadmeBytesSpan = currentReadmeBytes.AsSpan();
                Span<byte> retBytesSpan = retBytes.AsSpan();

                int lastIndexSource = 0;
                int lastIndexDest = 0;

                CopyInserts(langItems, currentReadmeBytesSpan, retBytesSpan, ansiCpgLength, ref lastIndexSource, ref lastIndexDest);

                // One more to copy everything from the last index to the end
                currentReadmeBytesSpan[lastIndexSource..].CopyTo(retBytesSpan[lastIndexDest..]);
                return retBytes;
            }

            return currentReadmeBytes;
        }
    }

    private static void CopyInserts(
        List<LangItem> langItems,
        ReadOnlySpan<byte> currentReadmeBytesSpan,
        Span<byte> retBytesSpan,
        int ansiCpgLength,
        ref int lastIndexSource,
        ref int lastIndexDest)
    {
        int plus = 0;
        ReadOnlySpan<byte> ansiCpgSpan = _ansicpg.AsSpan();
        for (int i = 0; i < langItems.Count; i++)
        {
            LangItem item = langItems[i];
            ListFast<byte> cpgBytes = CodePageToBytes(item.CodePage, item.DigitsCount);

            ReadOnlySpan<byte> bodySpan = currentReadmeBytesSpan.Slice(lastIndexSource, (item.Index - lastIndexDest) + plus);
            bodySpan.CopyTo(retBytesSpan[lastIndexDest..]);
            lastIndexSource += bodySpan.Length;
            lastIndexDest += bodySpan.Length;

            ansiCpgSpan.CopyTo(retBytesSpan[lastIndexDest..]);
            lastIndexDest += ansiCpgLength;
            ReadOnlySpan<byte> codePageSpan = cpgBytes.ItemsArray.AsSpan()[..cpgBytes.Count];
            codePageSpan.CopyTo(retBytesSpan[lastIndexDest..]);
            lastIndexDest += codePageSpan.Length;
            plus += ansiCpgLength + codePageSpan.Length;
        }

        return;

        static ListFast<byte> CodePageToBytes(int codePage, int digits)
        {
            _codePageBytes.ClearFast();

            for (int i = 0; i < digits; i++)
            {
                _codePageBytes.InsertAtZeroFast((byte)((codePage % 10) + '0'));
                codePage /= 10;
            }

            // Use the option for control words to have a space after them, for safety
            _codePageBytes.Add((byte)' ');

            return _codePageBytes;
        }
    }
}

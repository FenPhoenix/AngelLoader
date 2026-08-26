//#define PROCESS_README_TIME_TEST

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AL_Common.RTF;
using AngelLoader.DataClasses;

namespace AngelLoader;

internal static class RtfProcessing
{
    #region Fields

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

    // Static because we're very likely to need it a lot (for every rtf readme in dark mode), and we don't want
    // to make a new one every time.
    private static readonly RtfDisplayedReadmeParser _rtfDisplayedReadmeParser = new();

    #region Colors

    private static readonly byte[] _colortbl = @"{\colortbl"u8.ToArray();

    private static readonly byte[] _redFieldBytes = @"\red"u8.ToArray();

    private static readonly byte[] _greenFieldBytes = @"\green"u8.ToArray();

    private static readonly byte[] _blueFieldBytes = @"\blue"u8.ToArray();

    private static readonly byte[] _background = @"\*\background"u8.ToArray();

    private static readonly byte[] _backgroundBlanked = @"\*\xxxxxxxxxx"u8.ToArray();

    private static readonly ListFast<byte> _colorNumberBytes = new(3);

    #endregion

    internal static readonly string RTF_DarkBackgroundString = @"{\*\background{\shp{\*\shpinst{\sp{\sn fillColor}{\sv "
                                                               + ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground).ToStrInv()
                                                               + "}}}}}";
    private static readonly byte[] RTF_DarkBackgroundBytes = Encoding.ASCII.GetBytes(RTF_DarkBackgroundString);

    #endregion

    /*
    The first parse takes like 20ms, even when subsequent ones take <1ms. So do the first slow parse in parallel
    during startup if we're not already loading an RTF readme right away (if we are then it will already do the
    parse then). This speeds up the first selection of an FM with an RTF readme when the startup FM readme was
    plain text.
    */
    internal static void WarmUpRtfParser()
    {
        // This string contains enough RTF data to trigger the main codepaths to run and warm up. Just "{\rtf1}"
        // is not sufficient.
        _ = _rtfDisplayedReadmeParser.GetData(@"{\rtf1\ansicpg1252{\fonttbl\f0\cpg1252 Dummy;}{\colortbl;}}"u8.ToArray(), true);
    }

    internal static byte[] GetProcessedRTFBytes(byte[] currentReadmeBytes, bool darkMode)
    {
        // Avoid allocations as much as possible here, because glibly converting back and forth between lists
        // and arrays for our readme bytes is going to blow out memory.

#if PROCESS_README_TIME_TEST
        System.Diagnostics.Stopwatch parseTimer = new();
        parseTimer.Start();
#endif

        (bool success, List<RtfColor>? colorTable, List<CodePageItem>? codePageItems) =
            _rtfDisplayedReadmeParser.GetData(currentReadmeBytes, getColorTable: darkMode);

#if PROCESS_README_TIME_TEST
        parseTimer.Stop();
        TimeSpan parseTimerElapsed = parseTimer.Elapsed;
        System.Diagnostics.Trace.WriteLine(nameof(_rtfDisplayedReadmeParser) + "." + nameof(RtfDisplayedReadmeParser.GetData) + "() took:\r\n" + parseTimerElapsed);
#endif

        int colorTableEntryLength = 0;

        ListFast<byte>? colorEntriesBytesList = null;

        if (success && colorTable?.Count > 0)
        {
            colorEntriesBytesList = CreateColorTableRTFBytes(colorTable);
            colorTableEntryLength = colorEntriesBytesList.Count;
        }

        int extraCpgCombinedLength = 0;

        if (!(success && codePageItems?.Count > 0) && !darkMode)
        {
            return currentReadmeBytes;
        }

        if (success && codePageItems?.Count > 0)
        {
            extraCpgCombinedLength = (RtfDisplayedReadmeParser.FontNameSuffixCodePageLength) * codePageItems.Count;
        }

        byte[] retBytes;
        if (darkMode)
        {
            int retBytesLength =
                currentReadmeBytes.Length +
                colorTableEntryLength +
                RTF_DarkBackgroundBytes.Length +
                extraCpgCombinedLength;
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

            if (success && codePageItems?.Count > 0)
            {
                InsertCodePages(codePageItems, colorTableEntryLength, currentReadmeBytesSpan, retBytesSpan, ref lastIndexSource, ref lastIndexDest);
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

            // Insert our dark background definition at the end, so we override any other backgrounds that may be
            // set.
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
            if (success && codePageItems?.Count > 0)
            {
                retBytes = new byte[currentReadmeBytes.Length + extraCpgCombinedLength];

                ReadOnlySpan<byte> currentReadmeBytesSpan = currentReadmeBytes.AsSpan();
                Span<byte> retBytesSpan = retBytes.AsSpan();

                int lastIndexSource = 0;
                int lastIndexDest = 0;

                InsertCodePages(codePageItems, colorTableEntryLength, currentReadmeBytesSpan, retBytesSpan, ref lastIndexSource, ref lastIndexDest);

                // One more to copy everything from the last index to the end
                currentReadmeBytesSpan[lastIndexSource..].CopyTo(retBytesSpan[lastIndexDest..]);
                return retBytes;
            }

            return currentReadmeBytes;
        }
    }

    private static void InsertCodePages(
        List<CodePageItem> codePageItems,
        int colorTableEntryLength,
        ReadOnlySpan<byte> currentReadmeBytesSpan,
        Span<byte> retBytesSpan,
        ref int lastIndexSource,
        ref int lastIndexDest)
    {
        int plus = 0;
        for (int i = 0; i < codePageItems.Count; i++)
        {
            CodePageItem item = codePageItems[i];

            int itemIndex = item.Index + colorTableEntryLength;

            ReadOnlySpan<byte> bodySpan = currentReadmeBytesSpan.Slice(lastIndexSource, (itemIndex - lastIndexDest) + plus);
            bodySpan.CopyTo(retBytesSpan[lastIndexDest..]);
            lastIndexSource += bodySpan.Length;
            lastIndexDest += bodySpan.Length;

            ReadOnlySpan<byte> codePageSpan = item.CodePageBytes.AsSpan();
            codePageSpan.CopyTo(retBytesSpan[lastIndexDest..]);
            lastIndexDest += codePageSpan.Length;
            plus += RtfDisplayedReadmeParser.FontNameSuffixCodePageLength;
        }
    }

    private static ListFast<byte> CreateColorTableRTFBytes(List<RtfColor>? colorTable)
    {
        // "\red255\green255\blue255;" = 25 chars
        const int maxColorEntryStringLength = 25;

        // Size us large enough that we don't reallocate
        ListFast<byte> colorEntriesBytesList = new(
            _colortbl.Length +
            (maxColorEntryStringLength * colorTable?.Count ?? 0)
            + 2);

        colorEntriesBytesList.AddRange_Large(_colortbl);

        if (colorTable != null)
        {
            for (int i = 0; i < colorTable.Count; i++)
            {
                RtfColor invertedColor;
                RtfColor currentColor = colorTable[i];
                if (i == 0 && currentColor.IsDefaultColor)
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
                    invertedColor = DarkColors.Fen_DarkForeground_Rtf;
                }
                else if (ColorIsTheSameAsBackground(currentColor))
                {
                    invertedColor = DarkColors.Fen_DarkBackground_Rtf;
                }
                else
                {
                    invertedColor = ColorUtils.InvertLightness(currentColor);

                    // For some reason RTF doesn't accept a \cfN if the color is 255 all around, it has to be
                    // 254 or less... don't ask me
                    if (invertedColor is { R: 255, G: 255, B: 255 })
                    {
                        invertedColor = new RtfColor(254, 254, 254);
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

        #region Local functions

        // One file (In These Enlightened Times) had some hidden (white-on-white) text, so make that match our
        // new background color to keep author intent (avoiding spoilers etc.)
        static bool ColorIsTheSameAsBackground(RtfColor color) => color is { R: 255, G: 255, B: 255 };

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
    }
}

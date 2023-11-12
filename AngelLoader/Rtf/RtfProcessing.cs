//#define PROCESS_README_TIME_TEST

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AL_Common;
using AngelLoader.DataClasses;
using static AL_Common.Common;
using static AL_Common.RTFParserCommon;

namespace AngelLoader;

/*
@WPF(RTF notes and research):
-We could use WebView2 with WPF and convert our RTF to HTML maybe by modifying the internal RTF-to-XAML code
 in WPF. WebView2 seems fast - ~30ms to start and finish navigation to the AL doc file (long and lots of images).
-It's a full-blown browser like CefSharp was, so we need to delete cache on startup (can't do it after init
 because it throws an access denied exception EVEN AFTER WE DISPOSE THE CONTROL because of course it does)
 and pass it every lockdown parameter possible to get it to act the least like a browser and the most like
 an inert rich text control.
-Because it's just Edge, it's subject to being updated constantly I guess, and also may well be different
 versions on different peoples' PCs, which is a very nervous-making proposition for an app that ideally won't
 have to be babysat by me every 5 minutes when Edge updates. Who knows... :\
-It takes ~230ms to init the browser. Normally I'd throw a hissy fit over that, but these days we take like
 twice that time just to apply the dark theme on WinForms, so until I find out WPF takes a similar time to
 set its theme, I just can't bring myself to care.
-The init method is async, so we can try to overlap it with the app and view init I guess? If it isn't going
 to throw cross-thread exceptions? (it shouldn't, or why would it be async, right?)

WPF RichTextBox and FlowDocument-based controls that can also work:
Pros:
-No need for this horrendous EasyHook/GetSysColor nonsense, you just set the foreground color and it uses it
 as the default when none other is specified, just like we want.
Cons:
-Takes 1.5-20+ seconds to load wmf images. We can work around this by preprocessing the RTF to convert all
 wmf images to png.
-Is fucking gargantuanly slow even on image-less RTF files if there are enough state changes to create enough
 FlowDocument "blocks". Tested, this even happens if you merely copy one FlowDocument to another, so it's not
 even the RTF parser's fault here. Insane and moronic and why did they even bother. Why am I not surprised.

This makes WPF totally unusable for rich text, for ANY FlowDocument-based control, they all have the same
problem. Moving fucking on.

OLD TEXT WITH NOTES ON HOW TO DO THE PRE-CONVERT OF IMAGES:

When loading images in \wmetafileN format, it is HORRENDOUSLY SLOW. I know I always say things are "horrendously
slow" but I mean it this time, we're talking 1.5 to as much as like 20+ seconds sometimes. It's absurd.
What it does is convert the metafile to a bmp, then to a png, then writes it out to the stream again.
If we can do this part ourselves before we pass the byte array (and do it fast), we can dodge this tarpit.

WinUI 3 has an even nicer RichEditBox, it's very fast and you can still pass it a stream. But, it doesn't
even attempt to load metafile-format images. So again, we could convert them to png beforehand and it would
work I guess. But WinUI3 is only for Win10 1809 and up, so meh.

The plan:
-Parse until we find any {\pict with \wmetafileN
-Get the \pichgoal and \picwgoal values (twips) and convert to pixels, rounding AwayFromZero. This is the
 best dimension data we can really get.
-Get the bytes (hex or bin) out of the stream, and convert to actual bytes (binary) in a byte array
-Wrap a MemoryStream around the byte array and pass to a new Metafile(Stream).
-Make another stream and do metafile.Save(Stream, ImageFormat.Png)
-Remove the old \wmetadata pict, and insert our own, with the same picw/h/wgoal/hgoal etc. and whatever others
 are applicable, and do it binary (\binN) so we don't have to convert the bytes back to hex, just to save time

Notes:
-We can't pass the GDI png compressor any params, so we can't say not to compress it to save time. It's pretty
 fast relatively speaking, but we could try ImageMagick.NET or something to see if that's any better.
-This site suggests maybe we can calculate the width/height ourselves?
 https://keestalkstech.com/2016/06/rasterizing-emf-files-png-net-csharp/
*/

internal static class RtfProcessing
{
    #region Private fields

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
        (byte)'l'
    };

    private static readonly byte[] _redFieldBytes =
    {
        (byte)'\\',
        (byte)'r',
        (byte)'e',
        (byte)'d'
    };

    private static readonly byte[] _greenFieldBytes =
    {
        (byte)'\\',
        (byte)'g',
        (byte)'r',
        (byte)'e',
        (byte)'e',
        (byte)'n'
    };

    private static readonly byte[] _blueFieldBytes =
    {
        (byte)'\\',
        (byte)'b',
        (byte)'l',
        (byte)'u',
        (byte)'e'
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
        (byte)'d'
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
        (byte)'x'
    };

    private static readonly ListFast<byte> _colorNumberBytes = new(3);

    #endregion

    #region Langs

    // uint max digits (10) + 1 for adding a space after the digits
    private static readonly ListFast<byte> _paramBytes = new(11);

    private static readonly byte[] _lang =
    {
        (byte)'\\',
        (byte)'l',
        (byte)'a',
        (byte)'n',
        (byte)'g'
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
        (byte)'g'
    };

    private static readonly byte[] _cf =
    {
        (byte)'\\',
        (byte)'c',
        (byte)'f'
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
                else
                {
                    // Set pure black to custom-white (not pure white), otherwise it would invert around to pure
                    // white and that's a bit too bright.
                    invertedColor = currentColor is { R: 0, G: 0, B: 0 }
                        ? DarkColors.Fen_DarkForeground
                        : ColorIsTheSameAsBackground(currentColor)
                            ? DarkColors.Fen_DarkBackground
                            : ColorUtils.InvertLightness(currentColor);
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

        bool colorTableFound = false;
        bool langWorkRequired = false;

        if (darkMode)
        {
            #region Precheck for \colortbl

#if PROCESS_README_TIME_TEST
            var preCheckForColorTableTimer = new System.Diagnostics.Stopwatch();
            preCheckForColorTableTimer.Start();
#endif

            colorTableFound = FindIndexOfByteSequence(currentReadmeBytes, _colortbl) > -1;

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

        (bool success, List<Color>? colorTable, List<UIntParamInsertItem>? langItems) =
            RtfDisplayedReadmeParser.GetData(
                new ArrayWithLength<byte>(currentReadmeBytes),
                getColorTable: darkMode && colorTableFound,
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

        if (success && darkMode && colorTableFound)
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
            #region Calculate new byte array length

            for (int i = 0; i < langItems.Count; i++)
            {
                UIntParamInsertItem item = langItems[i];
                item.Index += colorTableEntryLength;
                // +1 for adding a space after the digits
                extraAnsiCpgCombinedLength += ansiCpgLength + item.ParamLength + 1;
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

            int lastIndexSource = 0;
            int lastIndexDest = 0;

            ReadOnlySpan<byte> currentReadmeBytesSpan = currentReadmeBytes.AsSpan();
            Span<byte> retBytesSpan = retBytes.AsSpan();

            ReadOnlySpan<byte> headerSpan = currentReadmeBytesSpan.Slice(0, firstIndexPastHeader);
            headerSpan.CopyTo(retBytesSpan.Slice(0));

            lastIndexSource += firstIndexPastHeader;
            lastIndexDest = firstIndexPastHeader;

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
                colorTableSpan.CopyTo(retBytesSpan.Slice(lastIndexDest, retBytesLength - lastIndexDest));
                lastIndexDest += colorTableEntryLength;
            }

            if (success && langWorkRequired && langItems?.Count > 0)
            {
                CopyInserts(langItems, currentReadmeBytesSpan, retBytesSpan, ref lastIndexSource, ref lastIndexDest);
            }

            ReadOnlySpan<byte> bodyToLastClosingBrace = currentReadmeBytesSpan.Slice(lastIndexSource, (lastClosingBraceIndex - lastIndexSource));
            bodyToLastClosingBrace.CopyTo(retBytesSpan.Slice(lastIndexDest));

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
            backgroundSpan.CopyTo(retBytesSpan.Slice(lastIndexDest));

            lastIndexDest += backgroundSpan.Length;

            currentReadmeBytesSpan.Slice(lastIndexSource).CopyTo(retBytesSpan.Slice(lastIndexDest));

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

                CopyInserts(langItems, currentReadmeBytesSpan, retBytesSpan, ref lastIndexSource, ref lastIndexDest);

                // One more to copy everything from the last index to the end
                currentReadmeBytesSpan.Slice(lastIndexSource).CopyTo(retBytesSpan.Slice(lastIndexDest));
                return retBytes;
            }

            return currentReadmeBytes;
        }

        #endregion
    }

    private static void CopyInserts(
        List<UIntParamInsertItem> insertItems,
        ReadOnlySpan<byte> sourceBytesSpan,
        Span<byte> destBytesSpan,
        ref int lastIndexSource,
        ref int lastIndexDest)
    {
        int plus = 0;
        for (int i = 0; i < insertItems.Count; i++)
        {
            UIntParamInsertItem item = insertItems[i];

            ListFast<byte> paramBytes = UIntParamToBytes(item.Param, item.ParamLength);

            ReadOnlySpan<byte> bodySpan = sourceBytesSpan.Slice(lastIndexSource, (item.Index - lastIndexDest) + plus);
            bodySpan.CopyTo(destBytesSpan.Slice(lastIndexDest));
            lastIndexSource += bodySpan.Length;
            lastIndexDest += bodySpan.Length;

            ReadOnlySpan<byte> keywordSpan = (item.Kind == InsertItemKind.Lang ? _ansicpg : _cf).AsSpan();
            int keywordLength = keywordSpan.Length;

            keywordSpan.CopyTo(destBytesSpan.Slice(lastIndexDest));
            lastIndexDest += keywordLength;

            ReadOnlySpan<byte> paramSpan = paramBytes.ItemsArray.AsSpan().Slice(0, paramBytes.Count);
            paramSpan.CopyTo(destBytesSpan.Slice(lastIndexDest));

            lastIndexDest += paramSpan.Length;
            plus += keywordLength + paramSpan.Length;
        }

        return;

        static ListFast<byte> UIntParamToBytes(uint param, int digits)
        {
            _paramBytes.ClearFast();

            for (int i = 0; i < digits; i++)
            {
                _paramBytes.InsertAtZeroFast((byte)((param % 10) + '0'));
                param /= 10;
            }

            // Use the option for control words to have a space after them, for safety
            _paramBytes.Add((byte)' ');

            return _paramBytes;
        }
    }
}

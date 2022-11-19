﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AngelLoader.DataClasses;
using static AL_Common.Common;

namespace AngelLoader
{
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

    internal static class RtfTheming
    {
        #region Private fields

        // Static because we're very likely to need it a lot (for every rtf readme in dark mode), and we don't
        // want to make a new one every time.
        private static RtfColorTableParser? _rtfColorTableParser;
        private static RtfColorTableParser RTFColorTableParser => _rtfColorTableParser ??= new RtfColorTableParser();

        #region RTF text coloring byte array nonsense

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

        #endregion

        private static readonly List<byte> _colorNumberBytes = new(3);

        #endregion

        internal static readonly string RTF_DarkBackgroundString = @"{\*\background{\shp{\*\shpinst{\sp{\sn fillColor}{\sv "
                                                        + ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground)
                                                        + "}}}}}";
        private static readonly byte[] RTF_DarkBackgroundBytes = Encoding.ASCII.GetBytes(RTF_DarkBackgroundString);

        private static List<byte> CreateColorTableRTFBytes(List<Color>? colorTable)
        {
            #region Local functions

            // One file (In These Enlightened Times) had some hidden (white-on-white) text, so make that match
            // our new background color to keep author intent (avoiding spoilers etc.)
            static bool ColorIsTheSameAsBackground(Color color) => color.R == 255 && color.G == 255 && color.B == 255;

            static List<byte> ByteToASCIICharBytes(byte number)
            {
                // Use global 3-byte list and do allocation-less clears and inserts, otherwise we would allocate
                // a new byte array EVERY time through here (which is a lot)
                _colorNumberBytes.Clear();

                int digits = number <= 9 ? 1 : number <= 99 ? 2 : 3;

                for (int i = 0; i < digits; i++)
                {
                    _colorNumberBytes.Insert(0, (byte)((number % 10) + '0'));
                    number /= 10;
                }

                return _colorNumberBytes;
            }

            #endregion

            const int maxColorEntryStringLength = 25; // "\red255\green255\blue255;" = 25 chars

            // Size us large enough that we don't reallocate
            var colorEntriesBytesList = new List<byte>(
                _colortbl.Length +
                (maxColorEntryStringLength * colorTable?.Count ?? 0)
                + 2);

            colorEntriesBytesList.AddRange(_colortbl);

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
                        invertedColor = currentColor.R == 0 && currentColor.G == 0 && currentColor.B == 0
                            ? DarkColors.Fen_DarkForeground
                            : ColorIsTheSameAsBackground(currentColor)
                            ? DarkColors.Fen_DarkBackground
                            : ColorUtils.InvertLightness(currentColor);
                    }

                    colorEntriesBytesList.AddRange(_redFieldBytes);
                    colorEntriesBytesList.AddRange(ByteToASCIICharBytes(invertedColor.R));

                    colorEntriesBytesList.AddRange(_greenFieldBytes);
                    colorEntriesBytesList.AddRange(ByteToASCIICharBytes(invertedColor.G));

                    colorEntriesBytesList.AddRange(_blueFieldBytes);
                    colorEntriesBytesList.AddRange(ByteToASCIICharBytes(invertedColor.B));

                    colorEntriesBytesList.Add((byte)';');
                }
            }

            colorEntriesBytesList.Add((byte)'}');

            return colorEntriesBytesList;
        }

        internal static byte[] GetDarkModeRTFBytes(byte[] currentReadmeBytes)
        {
            // Avoid allocations as much as possible here, because glibly converting back and forth between lists
            // and arrays for our readme bytes is going to blow out memory.

            (bool success, List<Color>? colorTable, _, int _) = RTFColorTableParser.GetColorTable(currentReadmeBytes);

            int colorTableEntryLength = 0;

            List<byte>? colorEntriesBytesList = null;

            if (success)
            {
                colorEntriesBytesList = CreateColorTableRTFBytes(colorTable);
                colorTableEntryLength = colorEntriesBytesList.Count;
            }

            byte[] darkModeBytes = new byte[currentReadmeBytes.Length + colorTableEntryLength + RTF_DarkBackgroundBytes.Length];

            int lastClosingBraceIndex = Array.LastIndexOf(currentReadmeBytes, (byte)'}');
            int firstIndexPastHeader = FindIndexOfByteSequence(currentReadmeBytes, RTFHeaderBytes) + RTFHeaderBytes.Length;

            // Copy header
            Array.Copy(
                currentReadmeBytes,
                0,
                darkModeBytes,
                0,
                firstIndexPastHeader
            );

            // Copy color table
            // Fortunately, only the first color table is used, so we can just stick ourselves right at the start
            // and not even have to awkwardly delete the old color table.
            // Now watch Windows get an update that breaks that.
            // @DarkModeNote: We could add code to delete the old color table at some point.
            // This would make us some amount slower, and it's not necessary currently, so let's just not do it
            // for now.
            if (colorEntriesBytesList != null)
            {
                for (int i = 0; i < colorTableEntryLength; i++)
                {
                    darkModeBytes[firstIndexPastHeader + i] = colorEntriesBytesList[i];
                }
            }

            // Copy main body
            Array.Copy(
                currentReadmeBytes,
                firstIndexPastHeader,
                darkModeBytes,
                firstIndexPastHeader + colorTableEntryLength,
                lastClosingBraceIndex - (firstIndexPastHeader - 1)
            );

            // Disable any backgrounds that may already be in there, otherwise we sometimes get visual artifacts
            // where the background stays the old color but turns to our new color when portions of the readme
            // get painted (see Thork).
            // Actually, Thork's readme is actually just weirdly broken, the background is sometimes yellow but
            // paints over with white even on classic mode. So oh well.
            // Do this BEFORE putting the dark background control word in, or else it will be overwritten too!
            ReplaceByteSequence(darkModeBytes, _background, _backgroundBlanked);

            // Insert our dark background definition at the end, so we override any other backgrounds that may be set.
            Array.Copy(
                RTF_DarkBackgroundBytes,
                0,
                darkModeBytes,
                colorTableEntryLength + lastClosingBraceIndex,
                RTF_DarkBackgroundBytes.Length
            );

            // Copy from the last closing brace to the end
            Array.Copy(
                currentReadmeBytes,
                lastClosingBraceIndex,
                darkModeBytes,
                colorTableEntryLength + RTF_DarkBackgroundBytes.Length + lastClosingBraceIndex,
                currentReadmeBytes.Length - lastClosingBraceIndex
            );

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

            // Keep this for debug...
            //System.IO.File.WriteAllBytes(@"C:\darkModeBytes.rtf", darkModeBytes);

            return darkModeBytes;
        }
    }
}

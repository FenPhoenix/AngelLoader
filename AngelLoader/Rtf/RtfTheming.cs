﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using static AL_Common.Common;

namespace AngelLoader
{
    /*
    @WPF(RTF notes and research):
    WPF has a RichTextBox that is really nice all things considered; you can just set the foreground color property
    and just like that, it does what we have to use EasyHook on GetSysColor for. It's generally fast enough, and
    while the way you load content is superficially different, it's basically still the same as what we've got
    now, just pass it a stream and a format. So we can still do the byte array modifications beforehand.
    BUT:
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

            var darkModeBytes = new byte[currentReadmeBytes.Length + colorTableEntryLength + RTF_DarkBackgroundBytes.Length];

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
            // NOTE: Thork's readme is actually just weirdly broken, the background is sometimes yellow but paints
            // over with white even on classic mode. So oh well.
            // NOTE: Do this BEFORE putting the dark background control word in, or else it will be overwritten too!
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
            //File.WriteAllBytes(@"C:\darkModeBytes.rtf", darkModeBytes);

            return darkModeBytes;
        }
    }
}

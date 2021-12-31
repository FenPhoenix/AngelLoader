using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using AngelLoader.Forms.CustomControls;
using static AL_Common.Common;

namespace AngelLoader
{
    internal static class RtfTheming
    {
        #region Private fields

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

        #endregion

        internal static readonly string RTF_DarkBackgroundString = @"{\*\background{\shp{\*\shpinst{\sp{\sn fillColor}{\sv "
                                                        + ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground)
                                                        + "}}}}}";
        private static readonly byte[] RTF_DarkBackgroundBytes = Encoding.ASCII.GetBytes(RTF_DarkBackgroundString);

        private static List<byte> CreateColorTableRTFBytes(List<Color> colorTable)
        {
            #region Local functions

            // One file (In These Enlightened Times) had some hidden (white-on-white) text, so make that match
            // our new background color to keep author intent (avoiding spoilers etc.)
            static bool ColorIsTheSameAsBackground(Color color) => color.R == 255 && color.G == 255 && color.B == 255;

            static byte[] ByteToASCIICharBytes(byte number)
            {
                byte[] ret = new byte[number <= 9 ? 1 : number <= 99 ? 2 : 3];

                for (int i = ret.Length - 1; i >= 0; i--)
                {
                    ret[i] = (byte)((number % 10) + '0');
                    number /= 10;
                }

                return ret;
            }

            #endregion

            const int maxColorEntryStringLength = 25; // "\red255\green255\blue255;" = 25 chars

            // Size us large enough that we don't reallocate
            var colorEntriesBytesList = new List<byte>(
                _colortbl.Length +
                (maxColorEntryStringLength * colorTable.Count)
                + 2);

            colorEntriesBytesList.AddRange(_colortbl);

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
            colorEntriesBytesList.Add((byte)'}');

            return colorEntriesBytesList;
        }

        internal static byte[] GetDarkModeRTFBytes(byte[] currentReadmeBytes)
        {
            var parser = new RtfColorTableParser();
            (bool success, List<Color> colorTable, _, int _) = parser.GetColorTable(currentReadmeBytes);

            int colorTableEntryLength = 0;

            byte[] colorEntriesBytesList = Array.Empty<byte>();

            if (success)
            {
                #region Write new color table

                colorEntriesBytesList = CreateColorTableRTFBytes(colorTable).ToArray();

                // Fortunately, only the first color table is used, so we can just stick ourselves right at the
                // start and not even have to awkwardly delete the old color table.
                // Now watch Windows get an update that breaks that.
                // @DarkModeNote: We could add code to delete the old color table at some point.
                // This would make us some amount slower, and it's not necessary currently, so let's just not do
                // it for now.

                colorTableEntryLength = colorEntriesBytesList.Length;

                #endregion
            }

            var darkModeBytes = new byte[currentReadmeBytes.Length + colorTableEntryLength + RTF_DarkBackgroundBytes.Length];

            int lastClosingBraceIndex = Array.LastIndexOf(currentReadmeBytes, (byte)'}');
            int headerIndex = FindIndexOfByteSequence(currentReadmeBytes, RTFHeaderBytes) + RTFHeaderBytes.Length;

            Array.Copy(
                currentReadmeBytes,
                0,
                darkModeBytes,
                0,
                headerIndex
            );

            for (int i = 0; i < colorEntriesBytesList.Length; i++)
            {
                darkModeBytes[headerIndex + i] = colorEntriesBytesList[i];
            }

            Array.Copy(
                currentReadmeBytes,
                headerIndex,
                darkModeBytes,
                headerIndex + colorTableEntryLength,
                lastClosingBraceIndex - (headerIndex - 1)
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

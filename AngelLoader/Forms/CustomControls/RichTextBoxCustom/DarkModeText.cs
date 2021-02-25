using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;
using static AL_Common.CommonUtils;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom
    {
        #region Private fields

        private byte[] _currentRTFBytes = Array.Empty<byte>();

        #region RTF text coloring byte array nonsense

        private readonly byte[] _colortbl =
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

        private readonly byte[] _fonttbl =
        {
            (byte)'{',
            (byte)'\\',
            (byte)'f',
            (byte)'o',
            (byte)'n',
            (byte)'t',
            (byte)'t',
            (byte)'b',
            (byte)'l'
        };

        private readonly byte[] _redFieldBytes =
        {
            (byte)'\\',
            (byte)'r',
            (byte)'e',
            (byte)'d'
        };

        private readonly byte[] _greenFieldBytes =
        {
            (byte)'\\',
            (byte)'g',
            (byte)'r',
            (byte)'e',
            (byte)'e',
            (byte)'n'
        };

        private readonly byte[] _blueFieldBytes =
        {
            (byte)'\\',
            (byte)'b',
            (byte)'l',
            (byte)'u',
            (byte)'e'
        };

        private readonly byte[] cf0 =
        {
            (byte)'\\',
            (byte)'c',
            (byte)'f',
            (byte)'0'
        };

        private readonly byte[] plain =
        {
            (byte)'\\',
            (byte)'p',
            (byte)'l',
            (byte)'a',
            (byte)'i',
            (byte)'n'
        };

        private readonly byte[] _background =
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

        private readonly byte[] _backgroundBlanked =
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

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                SetReadmeTypeAndColorState(_currentReadmeType);
                RefreshDarkModeState();
                DarkModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private RTFColorStyle _rtfColorStyle;

        internal RTFColorStyle GetRTFColorStyle() => _rtfColorStyle;

        /// <summary>
        /// Set startup to true to suppress refreshing the dark mode state, cause we'll already do that later
        /// when we set the theme.
        /// </summary>
        /// <param name="style"></param>
        /// <param name="startup"></param>
        internal void SetRTFColorStyle(RTFColorStyle style, bool startup = false)
        {
            if (style == _rtfColorStyle) return;

            _rtfColorStyle = style;

            if (!startup && _darkModeEnabled && _currentReadmeType == ReadmeType.RichText)
            {
                RefreshDarkModeState();
            }
        }

        #region Methods

        // Cheap, cheesy, effective
        private static string CreateBGColorRTFCode(Color color) =>
            @"{\*\background{\shp{\*\shpinst{\sp{\sn fillColor}{\sv " +
            ColorTranslator.ToWin32(color) +
            @"}}}}}";

        private static byte[] CreateBGColorRTFCode_Bytes(Color color)
        {
            var first = Encoding.ASCII.GetBytes(@"{\*\background{\shp{\*\shpinst{\sp{\sn fillColor}{\sv ");
            var colorStr = Encoding.ASCII.GetBytes(ColorTranslator.ToWin32(color).ToString());
            var last = Encoding.ASCII.GetBytes(@"}}}}}");

            var ret = new byte[first.Length + colorStr.Length + last.Length];
            Array.Copy(first, 0, ret, 0, first.Length);
            Array.Copy(colorStr, 0, ret, first.Length, colorStr.Length);
            Array.Copy(last, 0, ret, first.Length + colorStr.Length, last.Length);

            return ret;
        }

        private static byte[] Int8BitToASCIIBytes(int number)
        {
            byte[] ret = new byte[number <= 9 ? 1 : number <= 99 ? 2 : 3];

            string numStr = number.ToString();
            for (int i = 0; i < numStr.Length; i++) ret[i] = (byte)numStr[i];

            return ret;
        }

        /// <summary>
        /// Returns the index directly after the last closing curly brace in the given rtf group.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="groupControlWord">Must start with '{', for example "{\fonttbl"</param>
        /// <returns></returns>
        private static int FindEndOfGroup(List<byte> input, byte[] groupControlWord)
        {
            int index = FindIndexOfByteSequence(input, groupControlWord);
            if (index == -1) return -1;

            int braceLevel = 1;

            for (int i = index + 1; i < input.Count; i++)
            {
                byte b = input[i];
                if (b == (byte)'{')
                {
                    braceLevel++;
                }
                else if (b == (byte)'}')
                {
                    braceLevel--;
                }

                if (braceLevel < 1) return i + 1;
            }

            return -1;
        }

        private List<byte> CreateColorTableRTFBytes(List<Color> colorTable)
        {
            // One file (In These Enlightened Times) had some hidden (white-on-white) text, so make that match
            // our new background color to keep author intent (avoiding spoilers etc.)
            static bool ColorIsTheSameAsBackground(Color color) => color.R == 255 && color.G == 255 && color.B == 255;

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
                    // Explicitly set color 0 to our desired default, so we can spam \cf0 everywhere to keep
                    // our text looking right.
                    invertedColor = DarkColors.Fen_DarkForeground;
                }
                else if (_rtfColorStyle == RTFColorStyle.Monochrome)
                {
                    invertedColor = ColorIsTheSameAsBackground(currentColor)
                        ? DarkColors.Fen_DarkBackground
                        : DarkColors.Fen_DarkForeground;
                }
                else // auto-color
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
                colorEntriesBytesList.AddRange(Int8BitToASCIIBytes(invertedColor.R));

                colorEntriesBytesList.AddRange(_greenFieldBytes);
                colorEntriesBytesList.AddRange(Int8BitToASCIIBytes(invertedColor.G));

                colorEntriesBytesList.AddRange(_blueFieldBytes);
                colorEntriesBytesList.AddRange(Int8BitToASCIIBytes(invertedColor.B));

                colorEntriesBytesList.Add((byte)';');
            }
            colorEntriesBytesList.Add((byte)'}');

            return colorEntriesBytesList;
        }

        private byte[] GetDarkModeRTFBytes()
        {
            if (_rtfColorStyle == RTFColorStyle.Original) return _currentRTFBytes;

            var darkModeBytes = _currentRTFBytes.ToList();

            // Disable any backgrounds that may already be in there, otherwise we sometimes get visual artifacts
            // where the background stays the old color but turns to our new color when portions of the readme
            // get painted (see Thork).
            // NOTE: Thork's readme is actually just weirdly broken, the background is sometimes yellow but paints
            // over with white even on classic mode. So oh well.
            ReplaceByteSequence(darkModeBytes, _background, _backgroundBlanked);

            var parser = new RtfColorTableParser();
            (bool success, List<Color> colorTable, _, int _) = parser.GetColorTable(darkModeBytes);

            if (success)
            {
                #region Write new color table

                // Some files don't have a color table, so in that case just add the default black color that we
                // would normally expect to be there.
                if (colorTable.Count == 0)
                {
                    colorTable.Add(_rtfColorStyle == RTFColorStyle.Monochrome
                        ? DarkColors.Fen_DarkForeground
                        : Color.FromArgb(0, 0, 0));
                }

                List<byte> colorEntriesBytesList = CreateColorTableRTFBytes(colorTable);

                // Fortunately, only the first color table is used, so we can just stick ourselves right at the
                // start and not even have to awkwardly delete the old color table.
                // Now watch Windows get an update that breaks that.
                // TODO: @DarkMode: Add code to delete the old color table at some point
                darkModeBytes.InsertRange(FindIndexOfByteSequence(darkModeBytes, RTFHeaderBytes) + RTFHeaderBytes.Length, colorEntriesBytesList);

                #endregion
            }

            #region Insert \cf0 control words as needed

            // Insert a \cf0 right after the \fonttbl group, in case we don't encounter a \plain before any text.
            int fonttbl_EndIndex = FindEndOfGroup(darkModeBytes, _fonttbl);
            if (fonttbl_EndIndex > -1) darkModeBytes.InsertRange(fonttbl_EndIndex, cf0);

            /*
            TODO: @DarkMode: Insert \cf0 after every single one of these in case it ends up being the last one in the header
            I mean that's just being paranoid, but still...

            \rtf1 \fbidis? <character set> <from>? <deffont> <deflang> <fonttbl>? <filetbl>? 
            <colortbl>? <stylesheet>? <stylerestrictions>? <listtables>? <revtbl>? <rsidtable>? 
            <mathprops>? <generator>? 
            */

            // Despite extensive trying with EM_SETCHARFORMAT to tell it to set a new default text color, it just
            // doesn't want to work (best it can do is make ALL the text the default color, rather than just the
            // default text). So we just insert \cf0 directly after each \plain (which resets the character
            // properties). Ugly, but at this point whatever. Of _course_ we have to do this, but it's fast enough
            // and works, so meh.

            /*
            TODO: @DarkMode(RTF/DarkTextMode) issues/quirks/etc:
            -Image-as-first-item issue with the \cf0 inserts
             If we put a \cf0 before a transparent image, it makes the background of it white.
             See 2006-09-18_WC_WhatLiesBelow_v1
             Not a huge deal really - lots of readmes end up with bright images due to non-transparency, and
             WLB's transparent title image doesn't look good in dark mode anyway, but, you know...

            -Maybe don't invert if we're already light...?
             See: LostSouls14
             But note LostSouls14 is now fixed with the upper clamp
            */
            int index = 0;
            while ((index = FindIndexOfByteSequence(darkModeBytes, plain, index)) > -1)
            {
                darkModeBytes.InsertRange(index + plain.Length, cf0);
                index += plain.Length + cf0.Length;
            }

            #endregion

            // Insert our dark background definition at the end, so we override any other backgrounds that may be set.
            darkModeBytes.InsertRange(
                darkModeBytes.LastIndexOf((byte)'}'),
                CreateBGColorRTFCode_Bytes(DarkColors.Fen_DarkBackground));

            // Keep this for debug...
            //File.WriteAllBytes(@"C:\darkModeBytes.rtf", darkModeBytes.ToArray());

            return darkModeBytes.ToArray();
        }

        private void SetReadmeTypeAndColorState(ReadmeType readmeType)
        {
            _currentReadmeType = readmeType;

            if (readmeType == ReadmeType.PlainText)
            {
                (BackColor, ForeColor) = _darkModeEnabled
                    ? (DarkColors.Fen_DarkBackground, DarkColors.Fen_DarkForeground)
                    : (SystemColors.Window, SystemColors.WindowText);
            }
            else
            {
                (BackColor, ForeColor) = (SystemColors.Window, SystemColors.WindowText);
            }
        }

        private void RefreshDarkModeState(bool skipSuspend = false)
        {
            // Save/restore scroll position even for plaintext, because merely setting the fore/back colors makes
            // our scroll position bump itself slightly. Weird.

            bool plainText = _currentReadmeType == ReadmeType.PlainText;

            Native.SCROLLINFO si = GetCurrentScrollInfo(Handle);
            try
            {
                if (!skipSuspend)
                {
                    if (!plainText) SaveZoom();
                    this.SuspendDrawing_Native();
                    if (!plainText) ReadOnly = false;
                }

                if (_currentReadmeType == ReadmeType.RichText)
                {
                    using var ms = new MemoryStream(_darkModeEnabled ? GetDarkModeRTFBytes() : _currentRTFBytes);
                    LoadFile(ms, RichTextBoxStreamType.RichText);
                }
                else if (_currentReadmeType == ReadmeType.GLML)
                {
                    Rtf = GLMLToRTF(Encoding.UTF8.GetString(_currentRTFBytes));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(nameof(RichTextBoxCustom) + ": Couldn't set dark mode to " + _darkModeEnabled, ex);
            }
            finally
            {
                if (!skipSuspend)
                {
                    if (!plainText)
                    {
                        ReadOnly = true;
                        RestoreZoom();
                    }

                    RepositionScroll(Handle, si);
                    this.ResumeDrawing_Native();
                }
            }
        }

        #endregion
    }
}

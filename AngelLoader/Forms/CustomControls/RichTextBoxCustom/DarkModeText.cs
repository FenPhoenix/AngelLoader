using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static AngelLoader.Forms.CustomControls.RichTextBoxCustom_Interop;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom
    {
        #region Private fields

        private byte[] _currentRTFBytes = Array.Empty<byte>();

        #region RTF text coloring byte array nonsense

        private byte[] _colortbl =
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

        private byte[] _fonttbl =
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

        private readonly byte[] _redFieldBytes = { (byte)'\\', (byte)'r', (byte)'e', (byte)'d' };
        private readonly byte[] _greenFieldBytes = { (byte)'\\', (byte)'g', (byte)'r', (byte)'e', (byte)'e', (byte)'n' };
        private readonly byte[] _blueFieldBytes = { (byte)'\\', (byte)'b', (byte)'l', (byte)'u', (byte)'e' };

        private byte[] cf0 =
        {
            (byte)'\\',
            (byte)'c',
            (byte)'f',
            (byte)'0'
        };

        private byte[] pard =
        {
            (byte)'\\',
            (byte)'p',
            (byte)'a',
            (byte)'r',
            (byte)'d'
        };

        private byte[] plain =
        {
            (byte)'\\',
            (byte)'p',
            (byte)'l',
            (byte)'a',
            (byte)'i',
            (byte)'n'
        };

        private byte[] sectd =
        {
            (byte)'\\',
            (byte)'s',
            (byte)'e',
            (byte)'c',
            (byte)'t',
            (byte)'d'
        };

        private static readonly byte[] _background = Encoding.ASCII.GetBytes(@"\*\background");
        private static readonly byte[] _backgroundBlanked = Encoding.ASCII.GetBytes(@"\*\xxxxxxxxxx");

        #endregion

        #endregion

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetReadmeTypeAndColorState(_currentReadmeType);
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
                if (i == 0 && colorTable[i].A == 0)
                {
                    // Explicitly set color 0 to our desired default, so we can spam \cf0 everywhere to keep
                    // our text looking right.
                    invertedColor = DarkUI.Config.Colors.Fen_DarkForeground;
                }
                else
                {
                    invertedColor = ControlUtils.InvertBrightness(colorTable[i], blackToCustomWhite: true, preventFullWhite: true);
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
            var darkModeBytes = _currentRTFBytes.ToList();

            // Disable any backgrounds that may already be in there, otherwise we sometimes get visual artifacts
            // where the background stays the old color but turns to our new color when portions of the readme
            // get painted (see Thork).
            // NOTE: Thork's readme is actually just weirdly broken, the background is sometimes yellow but paints
            // over with white even on classic mode. So oh well.
            ReplaceByteSequence(darkModeBytes, _background, _backgroundBlanked);

            var parser = new RtfColorTableParser();
            (bool success, List<Color> colorTable, _, int _) = parser.GetColorTable(darkModeBytes);

            List<byte> colorEntriesBytesList = CreateColorTableRTFBytes(colorTable);

            if (success)
            {
                #region Write new color table

                // Some files don't have a color table, so in that case just add the default black color that we
                // would normally expect to be there.
                if (colorTable.Count == 0) colorTable.Add(Color.FromArgb(0, 0, 0));

                // Fortunately, only the first color table is used, so we can just stick ourselves right at the
                // start and not even have to awkwardly delete the old color table.
                // Now watch Windows get an update that breaks that.
                // TODO: @DarkMode: Add code to delete the old color table at some point
                darkModeBytes.InsertRange(FindIndexOfByteSequence(darkModeBytes, RTFHeaderBytes) + RTFHeaderBytes.Length, colorEntriesBytesList);

                #endregion
            }

            #region Insert \cf0 control words as needed

            // Insert a \cf0 right after the \fonttbl group, in case we don't encounter a \pard, \plain, or \sectd
            // before any text.
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
            // default text). So we just insert \cf0 directly after each \pard, \plain, and \sectd (which reset
            // the paragraph, character, and section properties, respectively).
            // Ugly, but at this point whatever. Of _course_ we have to do this, but it's fast enough and works,
            // so meh.

            /*
            TODO: @DarkMode(RTF/DarkTextMode) issues/quirks/etc:
            -Image-as-first-item issue with the \cf0 inserts
             If we put a \cf0 before a transparent image, it makes the background of it white.
             See 2006-09-18_WC_WhatLiesBelow_v1
             Not a huge deal really - lots of readmes end up with bright images due to non-transparency, and
             WLB's transparent title image doesn't look good in dark mode anyway, but, you know...

            -Maybe don't invert if we're already light...?
             See: LostSouls14

            -Mysterious Invitation:
             Readme has a section with a \pard but the red color stays active until the next \cf0
             NOTE: Solution is to only insert \cf0 after \plain control words and not any of the others, but:
             TODO: @DarkMode: Check all rtf files to make sure it doesn't break any others!
            */
            int index = 0;
            //while ((index = FindIndexOfByteSequence(darkModeBytes, pard, index)) > -1)
            //{
            //    darkModeBytes.InsertRange(index + pard.Length, cf0);
            //    index += pard.Length + cf0.Length;
            //}

            index = 0;
            while ((index = FindIndexOfByteSequence(darkModeBytes, plain, index)) > -1)
            {
                darkModeBytes.InsertRange(index + plain.Length, cf0);
                index += plain.Length + cf0.Length;
            }

            //index = 0;
            //while ((index = FindIndexOfByteSequence(darkModeBytes, sectd, index)) > -1)
            //{
            //    // Hack: don't match \sectdefaultcl
            //    if (darkModeBytes[index + sectd.Length] != (byte)'e')
            //    {
            //        //darkModeBytes.InsertRange(index + sectd.Length, cf0);
            //    }
            //    index += sectd.Length + cf0.Length;
            //}

            #endregion

            // Insert our dark background definition at the end, so we override any other backgrounds that may be set.
            darkModeBytes.InsertRange(
                darkModeBytes.LastIndexOf((byte)'}'),
                CreateBGColorRTFCode_Bytes(DarkUI.Config.Colors.Fen_DarkBackground));

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
                    ? (DarkUI.Config.Colors.Fen_DarkBackground, DarkUI.Config.Colors.Fen_DarkForeground)
                    : (SystemColors.Window, SystemColors.WindowText);
            }
            else
            {
                (BackColor, ForeColor) = (SystemColors.Window, SystemColors.WindowText);
            }
        }

        private void RefreshDarkModeState(bool skipSuspend = false)
        {
            if (_currentReadmeType == ReadmeType.PlainText) return;

            SCROLLINFO si = GetCurrentScrollInfo(Handle);
            try
            {
                if (!skipSuspend)
                {
                    SaveZoom();
                    this.SuspendDrawing();
                    ReadOnly = false;
                }

                if (_currentReadmeType == ReadmeType.RichText)
                {
                    using var ms = new MemoryStream(_darkModeEnabled ? GetDarkModeRTFBytes() : _currentRTFBytes);
                    LoadFile(ms, RichTextBoxStreamType.RichText);
                }
                else // GLML
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
                    ReadOnly = true;
                    RestoreZoom();
                    RepositionScroll(Handle, si);
                    this.ResumeDrawing();
                }
            }
        }

        #endregion
    }
}

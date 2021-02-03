using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
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

        private byte[] GetDarkModeBytes()
        {
            // TODO: @DarkMode: We should recolor hyperlinks in all cases too, look into how to do it
            // Solution: add a \cfN to the "{\fldrslt { \blah\blah\blah www.example.com }}" right before the URL,
            // so like "{\fldrslt { \blah\blah\blah \cf0 www.example.com }}"

            var darkModeBytes = _currentRTFBytes.ToList();

            var parser = new RtfColorTableParser();
            (bool success, List<Color> colorTable, _, int _) = parser.GetColorTable(darkModeBytes);

            if (success)
            {
                #region Write new color table

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

                // Fortunately, only the first color table is used, so we can just stick ourselves right at the
                // start and not even have to awkwardly delete the old color table.
                // Now watch Windows get an update that breaks that.
                // TODO: @DarkMode: Add code to delete the old color table at some point
                darkModeBytes.InsertRange(FindIndexOfByteSequence(darkModeBytes, RTFHeaderBytes) + RTFHeaderBytes.Length, colorEntriesBytesList);

                #endregion
            }

            #region Insert \cf0 control words as needed

            // Despite extensive trying with EM_SETCHARFORMAT to tell it to set a new default text color, it just
            // doesn't want to work (best it can do is make ALL the text the default color, rather than just the
            // default text). So we just insert \cf0 directly after each \pard, \plain, and \sectd (which reset
            // the paragraph, character, and section properties, respectively).
            // Ugly, but at this point whatever. Of _course_ we have to do this, but it's fast enough and works,
            // so meh.

            int index = 0;
            while ((index = FindIndexOfByteSequence(darkModeBytes, pard, index)) > -1)
            {
                darkModeBytes.InsertRange(index + pard.Length, cf0);
                index += pard.Length + cf0.Length;
            }

            index = 0;
            while ((index = FindIndexOfByteSequence(darkModeBytes, plain, index)) > -1)
            {
                darkModeBytes.InsertRange(index + plain.Length, cf0);
                index += plain.Length + cf0.Length;
            }

            index = 0;
            while ((index = FindIndexOfByteSequence(darkModeBytes, sectd, index)) > -1)
            {
                // Hack: don't match \sectdefaultcl
                if (darkModeBytes[index + sectd.Length] != (byte)'e')
                {
                    darkModeBytes.InsertRange(index + sectd.Length, cf0);
                }
                index += sectd.Length + cf0.Length;
            }

            #endregion

            // Insert our dark background definition at the end, so we override any other backgrounds that may be set.
            darkModeBytes.InsertRange(
                darkModeBytes.LastIndexOf((byte)'}'),
                CreateBGColorRTFCode_Bytes(DarkUI.Config.Colors.Fen_DarkBackground));

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

            RichTextBoxCustom_Interop.SCROLLINFO si = GetCurrentScrollInfo(Handle);
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
                    using var ms = new MemoryStream(_darkModeEnabled ? GetDarkModeBytes() : _currentRTFBytes);
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

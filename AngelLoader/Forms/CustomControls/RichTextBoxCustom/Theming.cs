using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom
    {
        private bool _darkModeEnabled;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled
        {
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
                SetReadmeTypeAndColorState(_currentReadmeType);
                // Perf: Don't load readme twice on startup, and don't load it again if we're on HTML or no FM
                // selected or whatever
                if (Visible) RefreshDarkModeState();
            }
        }

        #region Methods

        private void SetReadmeTypeAndColorState(ReadmeType readmeType)
        {
            _currentReadmeType = readmeType;

            if (readmeType is ReadmeType.PlainText or ReadmeType.Wri)
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

        /// <summary>
        /// Perform pre-processing that needs to be done regardless of visual theme.
        /// </summary>
        /// <param name="bytes"></param>
        private static void GlobalPreProcessRTF(byte[] bytes)
        {
            /*
            It's six of one half a dozen of the other - each method causes rare cases of images
            not showing, but for different files.
            And trying to get too clever and specific about it (if shppict says pngblip, and
            nonshppict says wmetafile, then DON'T patch shppict, otherwise do, etc.) is making
            me uncomfortable. I don't even know what Win7 or Win11 will do with that kind of
            overly-specific meddling. Microsoft have changed their RichEdit control before, and
            they might again, in which case I'm screwed either way.
            */
            ReplaceByteSequence(bytes, _shppict, _shppictBlanked);
            ReplaceByteSequence(bytes, _nonshppict, _nonshppictBlanked);
        }

        private sealed class PreProcessedRTF
        {
            private readonly string FileName;
            internal readonly byte[] Bytes;
            private readonly bool DarkMode;

            /*
            It's possible for us to preload a readme but then end up on a different FM. It could happen if we
            filter out the selected FM that was specified in the config, or if we load in new FMs and we reset
            our selection, etc. So make sure the readme we want to display is in fact the one we preloaded.
            Otherwise, we're just going to cancel the preload and load the new readme normally.
            */
            internal bool Identical(string fileName, bool darkMode) =>
                // Ultra paranoid checks
                !fileName.IsWhiteSpace() &&
                !FileName.IsWhiteSpace() &&
                FileName.PathEqualsI(fileName) &&
                DarkMode == darkMode;

            internal PreProcessedRTF(string fileName, byte[] bytes, bool darkMode)
            {
                FileName = fileName;
                Bytes = bytes;
                DarkMode = darkMode;
            }
        }

        private static PreProcessedRTF? _preProcessedRTF;

        [MemberNotNullWhen(true, nameof(_preProcessedRTF))]
        private static bool InPreloadedState(string readmeFile, bool darkMode)
        {
            if (_preProcessedRTF != null && _preProcessedRTF.Identical(readmeFile, darkMode))
            {
                return true;
            }
            else
            {
                SwitchOffPreloadState();
                return false;
            }
        }

        private static void SwitchOffPreloadState() => _preProcessedRTF = null;

        public static void PreloadRichFormat(string readmeFile, byte[] preloadedBytesRaw, bool darkMode)
        {
            _currentReadmeBytes = preloadedBytesRaw;

            try
            {
                GlobalPreProcessRTF(_currentReadmeBytes);

                _preProcessedRTF = new PreProcessedRTF(
                    readmeFile,
                    darkMode ? RtfTheming.GetDarkModeRTFBytes(_currentReadmeBytes) : _currentReadmeBytes,
                    darkMode
                );
            }
            catch
            {
                SwitchOffPreloadState();
            }
        }

        private void RefreshDarkModeState(string readmeFile = "", bool skipSuspend = false)
        {
            // Save/restore scroll position even for plaintext, because merely setting the fore/back colors makes
            // our scroll position bump itself slightly. Weird.

            bool plainText = _currentReadmeType == ReadmeType.PlainText;

            bool toggleReadOnly = _currentReadmeType is ReadmeType.RichText or ReadmeType.GLML;

            Native.SCROLLINFO si = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
            try
            {
                if (!skipSuspend)
                {
                    if (!plainText) SaveZoom();
                    this.SuspendDrawing();
                    if (toggleReadOnly) ReadOnly = false;
                }

                if (_currentReadmeType == ReadmeType.RichText)
                {
                    if (InPreloadedState(readmeFile, _darkModeEnabled))
                    {
                        using var ms = new MemoryStream(_preProcessedRTF.Bytes);
                        LoadFile(ms, RichTextBoxStreamType.RichText);
                    }
                    else
                    {
                        using var ms = new MemoryStream(_darkModeEnabled ? RtfTheming.GetDarkModeRTFBytes(_currentReadmeBytes) : _currentReadmeBytes);
                        LoadFile(ms, RichTextBoxStreamType.RichText);
                    }
                }
                else if (_currentReadmeType == ReadmeType.GLML)
                {
                    Rtf = GLMLConversion.GLMLToRTF(_currentReadmeBytes, _darkModeEnabled);
                }
            }
            catch (Exception ex)
            {
                Log("Couldn't set dark mode to " + _darkModeEnabled, ex);
            }
            finally
            {
                SwitchOffPreloadState();

                if (!skipSuspend)
                {
                    if (!plainText)
                    {
                        if (toggleReadOnly) ReadOnly = true;
                        RestoreZoom();
                    }

                    ControlUtils.RepositionScroll(Handle, si, Native.SB_VERT);
                    this.ResumeDrawing();
                }
            }
        }

        #endregion
    }
}

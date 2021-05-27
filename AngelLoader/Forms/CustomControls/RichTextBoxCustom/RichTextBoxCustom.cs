using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.WinAPI;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom : RichTextBox, IDarkable
    {
        #region Private fields / properties

        private Font? _monospaceFont;
        private Font MonospaceFont => _monospaceFont ??= new Font(FontFamily.GenericMonospace, 10.0f);

        private bool _contentIsPlainText;
        private bool ContentIsPlainText
        {
            get => _contentIsPlainText;
            set
            {
                _contentIsPlainText = value;
                if (_contentIsPlainText)
                {
                    SetFontTypeInternal(Config.ReadmeUseFixedWidthFont, outsideCall: false);
                }
                else
                {
                    ResetFont();
                }
            }
        }

        private byte[] _currentReadmeBytes = Array.Empty<byte>();

        private ReadmeType _currentReadmeType = ReadmeType.PlainText;
        private bool _currentReadmeSupportsEncodingChange;

        #endregion

        public RichTextBoxCustom() => InitWorkarounds();

        #region Private methods

        private void SetFontTypeInternal(bool useFixed, bool outsideCall)
        {
            if (!ContentIsPlainText) return;

            try
            {
                if (outsideCall)
                {
                    SaveZoom();
                    this.SuspendDrawing();
                }

                Font = useFixed ? MonospaceFont : DefaultFont;

                string savedText = Text;

                if (outsideCall)
                {
                    Clear();
                    ResetScrollInfo();

                    // We have to reload because links don't get recognized until we do
                    Text = savedText;
                }
            }
            finally
            {
                if (outsideCall)
                {
                    RestoreZoom();
                    this.ResumeDrawing();
                }
            }
        }

        private void SuspendState(bool toggleReadOnly)
        {
            SaveZoom();
            this.SuspendDrawing();

            // On Windows 10 at least, RTF images don't display if we're ReadOnly. Sure why not. We need to be
            // ReadOnly though - it doesn't make sense to let the user edit a readme - so un-set us just long
            // enough to load in the content correctly, then set us back again.
            if (toggleReadOnly) ReadOnly = false;

            // Blank the text to reset the scroll position to the top
            Clear();
            ResetScrollInfo();
        }

        private void ResumeState(bool toggleReadOnly)
        {
            if (toggleReadOnly) ReadOnly = true;
            RestoreZoom();
            this.ResumeDrawing();
        }

        private Encoding? ChangeEncodingInternal(MemoryStream ms, Encoding? encoding, bool suspendResume = true)
        {
            Encoding? retEncoding = null;

            Native.SCROLLINFO? si = null;
            try
            {
                if (suspendResume)
                {
                    si = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
                    SaveZoom();
                    this.SuspendDrawing();
                }

                if (encoding == null)
                {
                    var fe = new FMScanner.SimpleHelpers.FileEncoding();
                    encoding = fe.DetectFileEncoding(ms, Encoding.Default) ?? Encoding.Default;
                    retEncoding = encoding;
                    ms.Position = 0;
                }

                using var sr = new StreamReader(ms, encoding);
                Text = sr.ReadToEnd();

                return retEncoding;
            }
            catch (Exception ex)
            {
                // @BetterErrors(RTFBox/ChangeEncodingInternal())
                Logger.Log(nameof(RichTextBoxCustom) + ": Couldn't set encoding", ex);
                return retEncoding;
            }
            finally
            {
                if (suspendResume)
                {
                    RestoreZoom();
                    // Copy only the nPos value, otherwise we get a glitched-length scrollbar if our encoding
                    // change changes the height of the text.
                    Native.SCROLLINFO newSi = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
                    newSi.nPos = ((Native.SCROLLINFO)si!).nPos;
                    ControlUtils.RepositionScroll(Handle, newSi, Native.SB_VERT);
                    this.ResumeDrawing();
                }
            }
        }

        private static (bool Success, uint PlainTextStart, uint PlainTextEnd)
        ReadWriFileHeader(byte[] bytes)
        {
            var fail = (false, (uint)0, (uint)bytes.Length);

            const ushort WIDENT_VALUE = 48689;         // 0137061 octal
            const ushort WIDENT_NO_OLE_VALUE = 48690;  // 0137062 octal
            const ushort WTOOL_VALUE = 43776;          // 0125400 octal
            try
            {
                using var ms = new MemoryStream(bytes);
                using var br = new BinaryReader(ms, Encoding.ASCII, leaveOpen: true);
                ushort wIdent = br.ReadUInt16();
                if (wIdent != WIDENT_VALUE && wIdent != WIDENT_NO_OLE_VALUE)
                {
                    return fail;
                }

                if (br.ReadUInt16() != 0) return fail; // dty
                if (br.ReadUInt16() != WTOOL_VALUE) return fail; // wTool
                if (br.ReadUInt16() != 0) return fail; // Reserved 1
                if (br.ReadUInt16() != 0) return fail; // Reserved 2
                if (br.ReadUInt16() != 0) return fail; // Reserved 3
                if (br.ReadUInt16() != 0) return fail; // Reserved 4
                uint fcMac = br.ReadUInt32();
                br.ReadUInt16(); // pnPara
                br.ReadUInt16(); // pnFntb
                br.ReadUInt16(); // pnSep
                br.ReadUInt16(); // pnSetb
                br.ReadUInt16(); // pnPgtb
                br.ReadUInt16(); // pnFfntb
                br.BaseStream.Position += 66; // szSsht (not used)
                if (br.ReadUInt16() == 0) return fail; // pnMac: 0 means Word file, not Write file

                // Headers are always 128 bytes long I think?!
                return (true, 128, fcMac);
            }
            catch
            {
                return fail;
            }
        }

        #endregion

        #region Public methods

        #region Zoom stuff

        internal void SetAndStoreZoomFactor(float zoomFactor)
        {
            SetStoredZoomFactorClamped(zoomFactor);
            SetZoomFactorClamped(zoomFactor);
        }

        internal void ZoomIn()
        {
            try
            {
                SetZoomFactorClamped(ZoomFactor + 0.1f);
            }
            catch (ArgumentException)
            {
                // leave it as is
            }
        }

        internal void ZoomOut()
        {
            try
            {
                SetZoomFactorClamped(ZoomFactor - 0.1f);
            }
            catch (ArgumentException)
            {
                // leave it as is
            }
        }

        internal void ResetZoomFactor()
        {
            this.SuspendDrawing();

            // We have to set another value first, or it won't take.
            ZoomFactor = 1.1f;
            ZoomFactor = 1.0f;

            this.ResumeDrawing();
        }

        #endregion

        internal void SetFontType(bool useFixed) => SetFontTypeInternal(useFixed, outsideCall: true);

        #region Load content

        /// <summary>
        /// Sets the text without resetting the zoom factor.
        /// </summary>
        /// <param name="text"></param>
        internal void SetText(string text)
        {
            try
            {
                _currentReadmeSupportsEncodingChange = false;
                _currentReadmeBytes = Array.Empty<byte>();

                SuspendState(toggleReadOnly: false);

                ContentIsPlainText = true;
                if (!text.IsEmpty()) Text = text;
            }
            finally
            {
                SetReadmeTypeAndColorState(ReadmeType.PlainText);
                ResumeState(toggleReadOnly: false);
            }
        }

        /// <summary>
        /// Loads a file into the box without resetting the zoom factor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileType"></param>
        /// <param name="encoding">
        /// This parameter only applies to plain text format files. If null, then the file's encoding will be
        /// autodetected; otherwise, the file will be loaded with this encoding.
        /// </param>
        /// <returns>
        /// If the file is plain text format and no explicit encoding was passed in, the autodetected encoding
        /// of the file; otherwise, null.
        /// </returns>
        internal Encoding? LoadContent(string path, ReadmeType fileType, Encoding? encoding = null)
        {
            AssertR(fileType != ReadmeType.HTML, nameof(fileType) + " is ReadmeType.HTML");

            Encoding? retEncoding = null;

            // Do it here because it doesn't work if we set it before we've shown or whatever, and CreateHandle()
            // doesn't make it work either due to it being set back to non-full-detect there or whatever other
            // reason. It's fine, this works, fixed, moving on.
            SetFullUrlsDetect();

            // Do it for GLML files too, as they can have images (horizontal lines)!
            bool toggleReadOnly = fileType is ReadmeType.RichText or ReadmeType.GLML;

            try
            {
                SuspendState(toggleReadOnly);

                SetReadmeTypeAndColorState(fileType);

                switch (fileType)
                {
                    case ReadmeType.GLML:
                        _currentReadmeSupportsEncodingChange = false;

                        _currentReadmeBytes = File.ReadAllBytes(path);

                        // This resets the font if false, so don't do it after the load or it messes up the RTF.
                        ContentIsPlainText = false;

                        RefreshDarkModeState(skipSuspend: true);

                        break;
                    case ReadmeType.RichText:
                        _currentReadmeSupportsEncodingChange = false;

                        _currentReadmeBytes = File.ReadAllBytes(path);

                        ReplaceByteSequence(_currentReadmeBytes, _shppict, _shppictBlanked);
                        ReplaceByteSequence(_currentReadmeBytes, _nonshppict, _nonshppictBlanked);

                        // Ditto the above
                        ContentIsPlainText = false;

                        RefreshDarkModeState(skipSuspend: true);

                        break;
                    case ReadmeType.PlainText:
                        ContentIsPlainText = true;

                        void LoadAsText(byte[]? bytes = null)
                        {
                            _currentReadmeSupportsEncodingChange = true;
                            _currentReadmeBytes = bytes ?? File.ReadAllBytes(path);

                            // Load the file ourselves so we can do encoding detection. Otherwise it just loads
                            // with frigging whatever (default system encoding maybe?)
                            using var ms = new MemoryStream(_currentReadmeBytes);

                            retEncoding = ChangeEncodingInternal(ms, encoding, suspendResume: false);
                        }

                        // Quick and dirty .wri plaintext loader. Lucrative Opportunity is the only known FM with
                        // a .wri readme. For that particular file, we can just cut off the start and end junk
                        // chars and end up with a 100% clean plaintext readme. For other .wri files, there could
                        // be junk chars in the middle too, and then we would have to parse the format properly.
                        // But we only have the one file, so we don't bother.
                        if (path.ExtIsWri())
                        {
                            _currentReadmeSupportsEncodingChange = false;
                            _currentReadmeBytes = Array.Empty<byte>();

                            byte[] bytes = File.ReadAllBytes(path);

                            (bool success, uint plainTextStart, uint plainTextEnd) = ReadWriFileHeader(bytes);

                            if (success)
                            {
                                // Lucrative Opportunity is Windows-1252 encoded, so just go ahead and assume that
                                // encoding. It's probably a reasonable assumption for .wri files anyway.
                                Encoding enc1252 = Encoding.GetEncoding(1252);
                                byte[] tempByte = new byte[1];
                                var sb = new StringBuilder(bytes.Length);
                                for (uint i = plainTextStart; i < plainTextEnd; i++)
                                {
                                    byte b = bytes[i];
                                    if (b is 9 or 10 or 13 || (b >= 32 && b != 127))
                                    {
                                        if (b <= 126)
                                        {
                                            sb.Append((char)b);
                                        }
                                        else
                                        {
                                            tempByte[0] = b;
                                            sb.Append(enc1252.GetChars(tempByte));
                                        }
                                    }
                                }

                                string text = sb.ToString();
                                _currentReadmeBytes = enc1252.GetBytes(text);

                                Text = text;
                            }
                            else
                            {
                                LoadAsText(bytes);
                            }
                        }
                        else
                        {
                            LoadAsText();
                        }
                        break;
                }
            }
            finally
            {
                ResumeState(toggleReadOnly);
            }

            return retEncoding;
        }

        #endregion

        [PublicAPI]
        internal Encoding? ChangeEncoding(Encoding? encoding)
        {
            if (!_currentReadmeSupportsEncodingChange) return null;
            using var ms = new MemoryStream(_currentReadmeBytes);
            return ChangeEncodingInternal(ms, encoding);
        }

        #endregion

        #region Event overrides

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (!_darkModeEnabled) base.OnEnabledChanged(e);
            // Just suppress the base method call and done, no recoloring. Argh! I totally didn't make a huge
            // ridiculous system for getting around it! I totally knew all along!
        }

        protected override void OnVScroll(EventArgs e)
        {
            Workarounds_OnVScroll();
            base.OnVScroll(e);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeWorkarounds();
                _monospaceFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

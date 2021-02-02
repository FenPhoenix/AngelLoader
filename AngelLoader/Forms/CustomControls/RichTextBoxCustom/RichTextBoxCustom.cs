using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DarkUI.Controls;
using static AngelLoader.Misc;

// TODO: @DarkMode(RichTextBoxCustom):
// -There's are lot of byte[]/string/StringBuilder conversions in here now, see if we can remove as many of them
//  as possible.
// -Till then... IMPORTANT: Always use Encoding.UTF8.Get* because ASCII will break the char conversion for GLML!

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

        private byte[] _currentRTFBytes = Array.Empty<byte>();

        private ReadmeType _currentReadmeType = ReadmeType.PlainText;

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
                }

                // We have to reload because links don't get recognized until we do
                Text = savedText;
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

        // Cheap, cheesy, effective
        private static string CreateBGColorRTFCode(Color color) =>
            @"{\*\background{\shp{\*\shpinst{\sp{\sn fillColor}{\sv " +
            ColorTranslator.ToWin32(color) +
            @"}}}}}";

        private byte[] GetDarkModeBytes()
        {
            string newRTF = Encoding.UTF8.GetString(_currentRTFBytes);

            var sb = new StringBuilder(newRTF, newRTF.Length + 1024);

            // Insert us at the end, so we override any other backgrounds that may be set.
            sb.Insert(newRTF.LastIndexOf('}'), CreateBGColorRTFCode(DarkUI.Config.Colors.Fen_DarkBackground));

            // TODO: @DARKMODE: Recalculate and modify color table

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private void SetForeAndBackColorState(ReadmeType readmeType)
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

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
            set
            {
                _darkModeEnabled = value;
                SetForeAndBackColorState(_currentReadmeType);
                RefreshDarkModeState();
            }
        }

        #region API methods

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
            SaveZoom();

            try
            {
                this.SuspendDrawing();

                // Blank the text to reset the scroll position to the top
                Clear();
                ResetScrollInfo();

                ContentIsPlainText = true;
                if (!text.IsEmpty()) Text = text;

                RestoreZoom();
            }
            finally
            {
                SetForeAndBackColorState(ReadmeType.PlainText);
                this.ResumeDrawing();
            }
        }

        /// <summary>
        /// Loads a file into the box without resetting the zoom factor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileType"></param>
        internal void LoadContent(string path, ReadmeType fileType)
        {
            AssertR(fileType != ReadmeType.HTML, nameof(fileType) + " is ReadmeType.HTML");

            SaveZoom();

            try
            {
                this.SuspendDrawing();

                // On Windows 10 at least, images don't display if we're ReadOnly. Sure why not. We need to be
                // ReadOnly though - it doesn't make sense to let the user edit a readme - so un-set us just long
                // enough to load in the content correctly, then set us back again.
                ReadOnly = false;

                // Blank the text to reset the scroll position to the top
                Clear();
                ResetScrollInfo();

                SetForeAndBackColorState(fileType);

                switch (fileType)
                {
                    case ReadmeType.GLML:
                        string glml = File.ReadAllText(path);

                        // Capture the raw glml in this case, so we can run it through the GLML converter on
                        // dark mode refresh
                        _currentRTFBytes = Encoding.UTF8.GetBytes(glml);

                        // This resets the font if false, so don't do it after the load or it messes up the RTF.
                        ContentIsPlainText = false;

                        RefreshDarkModeState(skipSuspend: true);

                        break;
                    case ReadmeType.RichText:
                        // Use ReadAllBytes and byte[] search, because ReadAllText and string.Replace is ~30x slower
                        _currentRTFBytes = File.ReadAllBytes(path);

                        ReplaceByteSequence(_currentRTFBytes, _shppict, _shppictBlanked);
                        ReplaceByteSequence(_currentRTFBytes, _nonshppict, _nonshppictBlanked);

                        // Ditto the above
                        ContentIsPlainText = false;

                        RefreshDarkModeState(skipSuspend: true);

                        break;
                    case ReadmeType.PlainText:
                        ContentIsPlainText = true;

                        // Load the file ourselves so we can do encoding detection. Otherwise it just loads with
                        // frigging whatever (default system encoding maybe?)
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            var fe = new FMScanner.SimpleHelpers.FileEncoding();
                            Encoding? enc = fe.DetectFileEncoding(fs, Encoding.Default);

                            fs.Position = 0;

                            using var sr = new StreamReader(fs, enc ?? Encoding.Default);
                            Text = sr.ReadToEnd();
                        }

                        break;
                }
            }
            finally
            {
                ReadOnly = true;
                RestoreZoom();
                this.ResumeDrawing();
            }
        }

        #endregion

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

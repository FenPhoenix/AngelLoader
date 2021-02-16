using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DarkUI.Controls;
using JetBrains.Annotations;
using static AngelLoader.Misc;

// TODO: @DarkMode(RichTextBoxCustom):
// -There's are lot of byte[]/string/StringBuilder conversions in here now, see if we can remove as many of them
//  as possible.
// -Till then... IMPORTANT: Always use Encoding.UTF8.Get* because ASCII will break the char conversion for GLML!

// TODO: BUG: .wri files are not displayed right. See if a simple binary header-and-footer strip can be done

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom : RichTextBox, IDarkableScrollableNative
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

        private ReadmeType _currentReadmeType = ReadmeType.PlainText;

        #endregion

        public ScrollBarVisualOnly_Native VerticalVisualScrollBar { get; }
        public ScrollBarVisualOnly_Native HorizontalVisualScrollBar { get; }
        public ScrollBarVisualOnly_Corner VisualScrollBarCorner { get; }

        public event EventHandler? Scroll;

        public bool Suspended { get; set; }

        public event EventHandler<DarkModeChangedEventArgs>? DarkModeChanged;
        public event EventHandler? VisibilityChanged;

        public Control? ClosestAddableParent => Parent;

        public RichTextBoxCustom()
        {
            VerticalVisualScrollBar = new ScrollBarVisualOnly_Native(this, isVertical: true, passMouseWheel: true);
            HorizontalVisualScrollBar = null!;
            VisualScrollBarCorner = null!;

            InitWorkarounds();
        }

        #region Private methods

        private void SetFontTypeInternal(bool useFixed, bool outsideCall)
        {
            if (!ContentIsPlainText) return;

            try
            {
                if (outsideCall)
                {
                    SaveZoom();
                    this.SuspendDrawing_Native();
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
                    this.ResumeDrawing_Native();
                }
            }
        }

        private void SuspendState()
        {
            SaveZoom();
            this.SuspendDrawing_Native();

            // On Windows 10 at least, RTF images don't display if we're ReadOnly. Sure why not. We need to be
            // ReadOnly though - it doesn't make sense to let the user edit a readme - so un-set us just long
            // enough to load in the content correctly, then set us back again.
            ReadOnly = false;

            // Blank the text to reset the scroll position to the top
            Clear();
            ResetScrollInfo();
        }

        private void ResumeState()
        {
            ReadOnly = true;

            RestoreZoom();
            // Force visible state update before resuming to avoid a flicker of the classic bar showing up
            VerticalVisualScrollBar.ForceSetVisibleState();
            this.ResumeDrawing_Native();
            // Invoke this after resuming to prevent the scroll bar from disappearing when you move through
            // entries by holding down a key
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
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
            this.SuspendDrawing_Native();

            // We have to set another value first, or it won't take.
            ZoomFactor = 1.1f;
            ZoomFactor = 1.0f;

            this.ResumeDrawing_Native();
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
                _currentRTFBytes = Array.Empty<byte>();

                SuspendState();

                ContentIsPlainText = true;
                if (!text.IsEmpty()) Text = text;
            }
            finally
            {
                SetReadmeTypeAndColorState(ReadmeType.PlainText);
                ResumeState();
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

            try
            {
                SuspendState();

                SetReadmeTypeAndColorState(fileType);

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

                        _currentRTFBytes = Array.Empty<byte>();

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
                ResumeState();
            }
        }

        #endregion

        #endregion

        #region Event overrides

        protected override void OnEnabledChanged(EventArgs e)
        {
            if (!_darkModeEnabled) base.OnEnabledChanged(e);
            // Just suppress the base method call and done, no recoloring. Argh! I totally didn't make a huge
            // ridiculous system for getting around it! I totally knew all along!
        }

        protected override void OnHScroll(EventArgs e)
        {
            base.OnHScroll(e);
            Scroll?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnVScroll(EventArgs e)
        {
            Workarounds_OnVScroll();
            base.OnVScroll(e);
            Scroll?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Visible / Show / Hide overrides

        [PublicAPI]
        public new bool Visible
        {
            get => base.Visible;
            set
            {
                if (value)
                {
                    // Do this before setting the Visible value to avoid the classic-bar-flicker
                    VerticalVisualScrollBar.ForceSetVisibleState(true);
                    base.Visible = value;
                }
                else
                {
                    base.Visible = value;
                    VerticalVisualScrollBar.ForceSetVisibleState(false);
                }
            }
        }

        [PublicAPI]
        public new void Show()
        {
            VerticalVisualScrollBar.ForceSetVisibleState(true);
            base.Show();
        }

        [PublicAPI]
        public new void Hide()
        {
            base.Hide();
            VerticalVisualScrollBar.ForceSetVisibleState(false);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeWorkarounds();
                _monospaceFont?.Dispose();
                VerticalVisualScrollBar?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

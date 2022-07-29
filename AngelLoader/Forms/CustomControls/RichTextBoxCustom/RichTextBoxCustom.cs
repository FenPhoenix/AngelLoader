﻿using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AL_Common.Common;
using static AL_Common.Logger;
using static AngelLoader.Global;
using static AngelLoader.Misc;
using static AngelLoader.Utils;

namespace AngelLoader.Forms.CustomControls
{
    /*
    NOTE: ReadmeRichTextBoxCustom: Memory leak on Windows' side
    The RichTextBox leaks memory when new content is loaded when ReadOnly == false. It does NOT appear to leak
    when ReadOnly == true. But we need ReadOnly to be false during load, otherwise we lose some of the images.
    In real-world use the leak should not be very significant on average (1.4.8 exhibits it too, which means it's
    probably always been there and it took me this long to notice). It can be made to happen very visibly by
    selecting "An Enigmatic Treasure With A Recondite Discovery" and then switching between Classic and Dark
    modes rapidly.

    2021-05-27:
    Tested with a new project, WinForms, .NET 5 (in case they fixed it somehow, but nope):
    With "An Enigmatic Treasure With A Recondite Discovery" and "The Pursuance of an Inscrutable Reciprocity",
    the leak happens. With "Tarnhill_V1" it doesn't. Tarnhill's readme does not contain images.
    However, the leak also does NOT happen with "Feast of Pilgrims", whose readme DOES contain an image.

    2021-06-01:
    https://blogs.lessthandot.com/index.php/desktopdev/mstech/winforms-richtextbox-and-a-memoryleak/
    Tried this (putting ClearUndo()) after every content-loading thing, and it doesn't do jack squat.
    Doesn't do jack squat in the test project either.
    -Test with WPF project: The WPF RichTextBox does NOT leak memory on TPOAIR.
    -WPF implements its RichTextBox from scratch... and it's MIT licensed. So good, some RichEdit-compatible
     (presumably) code exists that I could use to make a custom control in some way, possibly.

    2022-01-18:
    The leak is 100% on Windows' side. Tested with WordPad loading "Enigmatic Treasure" over and over, and it
    exhibits the same behavior: constantly increasing memory.
    Note that even disposing the RichTextBox, and even using reflection to get the "IntPtr moduleHandle" field
    and calling FreeLibrary() on it, STILL doesn't release the unmanaged memory.
    We would have to put it in an entirely separate process and do like RichTextBoxAsync (but without the async)
    and then just restart the process whenever our memory use gets too high. Or, just use WPF's version through
    an ElementHost.
    */

    internal sealed partial class RichTextBoxCustom : RichTextBox, IDarkable
    {
        #region Private fields / properties

        internal ReadmeLocalizableMessage LocalizableMessageType = ReadmeLocalizableMessage.None;

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
        // Despite it _usually_ being the case that plaintext type supports encoding change and everything else
        // doesn't, there are cases where that isn't true and we need to mark this separately. For example, when
        // plaintext is passed in from a source we control (error messages etc.) or if we've failed to load a
        // .wri file and have fallen back to treating it as unparsed plain text.
        private bool _currentReadmeSupportsEncodingChange;

        private Form _owner = null!;

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

                if (outsideCall)
                {
                    string savedText = Text;

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

        private Encoding? ChangeEncodingInternal(Encoding? encoding, bool suspendResume = true)
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

                using var ms = new MemoryStream(_currentReadmeBytes);

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
                Log("Couldn't set encoding", ex);
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

        #endregion

        #region Public methods

        internal void SetOwner(Form owner) => _owner = owner;

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
            try
            {
                this.SuspendDrawing();

                // We have to set another value first, or it won't take.
                ZoomFactor = 1.1f;
                ZoomFactor = 1.0f;
            }
            finally
            {
                this.ResumeDrawing();
            }
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
                Text = text;
            }
            finally
            {
                SetReadmeTypeAndColorState(ReadmeType.PlainText);
                ResumeState(toggleReadOnly: false);
                LocalizableMessageType = ReadmeLocalizableMessage.None;
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

            bool needsCursorReset = false;

            try
            {
                // Use a rough heuristic to guess if we're going to take long enough to warrant a wait cursor.
                // Load time is not about the file size per se, it's just that a big file size probably means
                // big images, which are slow to load.
                // Note that we can't just run a timer and only show the wait cursor if we're taking >100ms,
                // because the UI thread is blocked during this load so we can't change the cursor in the middle
                // of it.
                if (fileType is ReadmeType.RichText)
                {
                    long size = new FileInfo(path).Length;
                    if (size > ByteSize.KB * 300)
                    {
                        _owner.Cursor = Cursors.WaitCursor;
                        needsCursorReset = true;
                    }
                }

                SuspendState(toggleReadOnly);

                SetReadmeTypeAndColorState(fileType);

                switch (fileType)
                {
                    case ReadmeType.GLML:
                    case ReadmeType.RichText:
                        _currentReadmeSupportsEncodingChange = false;

                        _currentReadmeBytes = File.ReadAllBytes(path);

                        // We control the format of GLML-converted files, so no need to do this for those
                        if (fileType == ReadmeType.RichText)
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
                            ReplaceByteSequence(_currentReadmeBytes, _shppict, _shppictBlanked);
                            ReplaceByteSequence(_currentReadmeBytes, _nonshppict, _nonshppictBlanked);
                        }

                        // This resets the font if false, so don't do it after the load or it messes up the RTF.
                        ContentIsPlainText = false;

                        RefreshDarkModeState(skipSuspend: true);

                        break;
                    case ReadmeType.PlainText:
                    case ReadmeType.Wri:
                        ContentIsPlainText = true;

                        void LoadAsText(byte[]? bytes = null)
                        {
                            _currentReadmeSupportsEncodingChange = true;
                            _currentReadmeBytes = bytes ?? File.ReadAllBytes(path);

                            retEncoding = ChangeEncodingInternal(encoding, suspendResume: false);
                        }

                        if (fileType == ReadmeType.Wri)
                        {
                            _currentReadmeSupportsEncodingChange = false;
                            _currentReadmeBytes = Array.Empty<byte>();

                            byte[] bytes = File.ReadAllBytes(path);

                            (bool success, byte[] retBytes, string retText) = WriConversion.LoadWriFileAsPlainText(bytes);

                            if (success)
                            {
                                _currentReadmeBytes = retBytes;
                                Text = retText;
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
                if (needsCursorReset)
                {
                    _owner.Cursor = Cursors.Default;
                }
                LocalizableMessageType = ReadmeLocalizableMessage.None;
            }

            return retEncoding;
        }

        #endregion

        [PublicAPI]
        internal Encoding? ChangeEncoding(Encoding? encoding)
        {
            return _currentReadmeSupportsEncodingChange ? ChangeEncodingInternal(encoding) : null;
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

        /*
        Hack to work around a memory issue...

        When the handle is destroyed, it gets the RTF (through StreamOut, returning a string containing the RTF)
        and assigns the string to a field:

        --- snip ---

        protected override void OnHandleDestroyed(EventArgs e)
        {
            base.OnHandleDestroyed(e);
            if (!this.InConstructor)
            {
                this.textRtf = this.Rtf;
                if (this.textRtf.Length == 0)
                    this.textRtf = (string)null;
            }
            this.oleCallback = (object)null;
            SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.UserPreferenceChangedHandler);
        }

        --- snip ---

        At first this seems pointless, to allocate a potentially multi-megabyte string for seemingly no reason.
        Reading the code, it looks like it's so that it can store the RTF string through a handle recreation.
        Okay, fair I guess if that's needed, but for us, it just wastes a ton of memory for absolutely no reason
        whatsoever. We never recreate the handle and the only time we destroy is it implicitly when we're shutting
        down anyway. Previously, we would spike our memory usage right before shutdown, which is dumb.

        We could set Rtf = "" in the main form closing handler. Except...

        --- snip ---

        public string Rtf
        {
            get
            {
                if (this.IsHandleCreated)
                    return this.StreamOut(2);
                if (this.textPlain == null)
                    return this.textRtf;
                this.ForceHandleCreate();
                return this.StreamOut(2);
            }
            set
            {
                if (value == null)
                    value = "";
                if (value.Equals(this.Rtf))       // <- this line here
                    return;
                this.ForceHandleCreate();
                this.textRtf = value;
                this.StreamIn(value, 2);
                if (!this.CanRaiseTextChangedEvent)
                    return;
                this.OnTextChanged(EventArgs.Empty);
            }
        }

        --- snip ---

        Yeah... it _also_ streams out the rtf into a string just to do a comparison. So. Yeah.

        The only choice - short of just copy-pasting the entire RichTextBox control code and straightening it all
        out to not be dumb* - is to just suppress the base OnHandleDestroyed() method. Probably a fantastically
        bad idea in general, but in our particular case it's fine because we're about to close the app anyway,
        so cleanup doesn't matter.

        *Basically impossible. It has too many tentacles too deep into the entire framework. Ugh.
        */
        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Only suppress if we're closing, because there are a couple other situations in which the handle
            // can be recreated in the normal course of things (changing ScrollBars for example).
            // We don't do any of them at the moment (2022-01-17) but meh.
            if (FindForm() is MainForm { AboutToClose: true })
            {
                return;
            }

            base.OnHandleDestroyed(e);
        }

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

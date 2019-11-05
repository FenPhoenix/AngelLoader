using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using AngelLoader.Common.Utility;
using static AngelLoader.Common.Misc;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.CustomControls
{
    internal sealed partial class RichTextBoxCustom : RichTextBox
    {
        #region Private fields / properties

        private bool LinkCursor;

        // TODO: See if this can be removed.
        private bool initialReadmeZoomSet = true;

        private float _storedZoomFactor = 1.0f;

        #region Workaround to fix black transparent regions in images

        // ReSharper disable IdentifierTypo
        // ReSharper disable StringLiteralTypo
        private static readonly byte[] shppict = Encoding.ASCII.GetBytes(@"\shppict");
        private static readonly byte[] shppictBlanked = Encoding.ASCII.GetBytes(@"\xxxxxxx");
        private static readonly byte[] nonshppict = Encoding.ASCII.GetBytes(@"\nonshppict");
        private static readonly byte[] nonshppictBlanked = Encoding.ASCII.GetBytes(@"\xxxxxxxxxx");
        // ReSharper restore StringLiteralTypo
        // ReSharper restore IdentifierTypo

        #endregion

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
                    SetFontTypeInternal(Config.ReadmeUseFixedWidthFont, false);
                }
                else
                {
                    ResetFont();
                }
            }
        }

        #endregion

        #region API fields / properties

        internal float StoredZoomFactor { get => _storedZoomFactor; set => _storedZoomFactor = value.Clamp(0.1f, 5.0f); }

        #endregion

        public RichTextBoxCustom()
        {
            InitScrollInfo();
            InitReaderMode();
        }

        #region Private methods

        /*
        Alright kids, gather round while your ol' Grandpa Fen explains you the deal.
        You can choose two different versions of RichTextBox. Old (3.0) or new (4.1). Both have their own unique
        and beautiful ways of driving you up the wall.
        Old:
        -Garbles right side of horizontal lines when you scale them out too far.
        -Flickers while scrolling if and only if another control is laid overtop of it.
        +Displays image transparency correctly.
        New:
        +Doesn't flicker while scrolling even when controls are laid overtop of it.
        +Doesn't garble the right edge of scaled-out horizontal lines no matter how far you scale.
        -Displays image transparency as pure black.
        -There's a compatibility option "\transmf" that looks like it would fix the above, but guess what, it's
         not supported.

        To stop the new version's brazen headlong charge straight off the edge of Mount Compatible, we replace all
        instances of "\shppict" and "\nonshppict" with dummy strings. This fixes the problem. Hooray. Now get off
        my lawn.
        */
        private static void ReplaceByteSequence(byte[] input, byte[] pattern, byte[] replacePattern)
        {
            var firstByte = pattern[0];
            int index = Array.IndexOf(input, firstByte);
            var pLen = pattern.Length;

            while (index > -1)
            {
                for (int i = 0; i < pLen; i++)
                {
                    if (index + i >= input.Length) return;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return;
                        break;
                    }

                    if (i == pLen - 1)
                    {
                        for (int j = index, ri = 0; j < index + pLen; j++, ri++)
                        {
                            input[j] = replacePattern[ri];
                        }
                    }
                }
            }
        }

        private void SaveZoom()
        {
            // Because the damn thing resets its zoom every time you load new content, we have to keep a global
            // var with the zoom value and keep both values in sync.
            if (initialReadmeZoomSet)
            {
                initialReadmeZoomSet = false;
            }
            else
            {
                // Don't do this if we're just starting up, because then it will throw away our saved value
                StoredZoomFactor = ZoomFactor.Clamp(0.1f, 5.0f);
            }
        }

        private void RestoreZoom()
        {
            // Heisenbug: If we step through this, it sets the zoom factor correctly. But if we're actually
            // running normally, it doesn't, and we need to set the size to something else first and THEN it will
            // work. Normally this causes the un-zoomed text to be shown for a split-second before the zoomed text
            // gets shown, so we use custom extensions to suspend and resume drawing while we do this ridiculous
            // hack, so it looks perfectly flawless to the end user.
            ZoomFactor = 1.0f;

            try
            {
                ZoomFactor = StoredZoomFactor.Clamp(0.1f, 5.0f);
            }
            catch (ArgumentException)
            {
                // Do nothing; remain at 1.0
            }
        }

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

                var savedText = Text;

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

        #endregion

        #region API methods

        #region Zoom stuff

        internal void ZoomIn()
        {
            try
            {
                ZoomFactor = (ZoomFactor + 0.1f).Clamp(0.1f, 5.0f);
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
                ZoomFactor = (ZoomFactor - 0.1f).Clamp(0.1f, 5.0f);
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
            Debug.Assert(fileType != ReadmeType.HTML, nameof(fileType) + " is ReadmeType.HTML");

            SaveZoom();

            try
            {
                this.SuspendDrawing();

                // On Windows 10 at least, images don't display if we're ReadOnly. Why not. We need to be ReadOnly
                // though - it doesn't make sense to let the user edit a readme - so un-set us just long enough
                // to load in the content correctly, then set us back again.
                ReadOnly = false;

                // Blank the text to reset the scroll position to the top
                Clear();
                ResetScrollInfo();

                switch (fileType)
                {
                    case ReadmeType.GLML:
                        var text = File.ReadAllText(path);
                        // This resets the font if false, so don't do it after the load or it messes up the RTF.
                        ContentIsPlainText = false;
                        Rtf = GLMLToRTF(text);
                        break;
                    case ReadmeType.RichText:
                        // Use ReadAllBytes and byte[] search, because ReadAllText and string.Replace is ~30x slower
                        var bytes = File.ReadAllBytes(path);

                        ReplaceByteSequence(bytes, shppict, shppictBlanked);
                        ReplaceByteSequence(bytes, nonshppict, nonshppictBlanked);

                        // Ditto the above
                        ContentIsPlainText = false;
                        using (var ms = new MemoryStream(bytes)) LoadFile(ms, RichTextBoxStreamType.RichText);
                        break;
                    case ReadmeType.PlainText:
                        ContentIsPlainText = true;
                        LoadFile(path, RichTextBoxStreamType.PlainText);
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

        protected override void WndProc(ref Message m)
        {
            switch ((uint)m.Msg)
            {
                case WM_MOUSEWHEEL:
                    // Intercept the mousewheel call and direct it to use the fixed scrolling
                    InterceptMousewheel(ref m);
                    break;
                case WM_MBUTTONDOWN:
                case WM_MBUTTONDBLCLK:
                    // Intercept the middle mouse button and direct it to use the fixed reader mode
                    InterceptMiddleMouseButton(ref m);
                    break;
                // CursorHandler() essentially "calls" this section, and this section "returns" whether the cursor
                // was over a link (via LinkCursor)
                case WM_REFLECT + WM_NOTIFY:
                    CheckAndHandleEnLinkMsg(ref m);
                    break;
                case WM_SETCURSOR:
                    CursorHandler(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tmrAutoScroll?.Dispose();
                pbGlyph?.Dispose();
                _monospaceFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

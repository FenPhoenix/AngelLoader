using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AngelLoader.Forms.CustomControls.RichTextBoxCustom_Interop;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.Forms.CustomControls
{
    internal sealed partial class RichTextBoxCustom
    {
        private void InitWorkarounds()
        {
            InitScrollInfo();
            InitReaderMode();
        }

        public static IntPtr FromLowHigh(int low, int high) => (IntPtr)ToInt(low, high);

        public static int ToInt(int low, int high) => (high << 16) | (low & 0xffff);

        public static int SignedHIWORD(int n) => (int)(short)HIWORD(n);

        public static int SignedLOWORD(int n) => (int)(short)LOWORD(n);

        public static int SignedHIWORD(IntPtr n) => SignedHIWORD(unchecked((int)(long)n));

        public static int SignedLOWORD(IntPtr n) => SignedLOWORD(unchecked((int)(long)n));

        public static int HIWORD(int n) => (n >> 16) & 0xffff;

        public static int LOWORD(int n) => n & 0xffff;

        public const int SB_THUMBTRACK = 5;
        public const int SB_THUMBPOSITION = 4;

        protected override void WndProc(ref Message m)
        {
            //Trace.WriteLine("m.Msg: " + m.Msg);

            //if (m.Msg == WM_LBUTTONDOWN)
            //{
            //    Trace.WriteLine("********* x: " + SignedLOWORD(m.LParam));
            //    Trace.WriteLine("********* y: " + SignedHIWORD(m.LParam));
            //}
            //else if (m.Msg == WM_NCLBUTTONDOWN)
            //{
            //    Trace.WriteLine("NC ********* x: " + SignedLOWORD(m.LParam));
            //    Trace.WriteLine("NC ********* y: " + SignedHIWORD(m.LParam));
            //}

            //if (m.Msg == WM_VSCROLL)
            //{
            //    int lowWord = LOWORD(m.WParam.ToInt32());
            //    //Trace.WriteLine("WM_VSCROLL low word: " + lowWord);

            //    if (lowWord == SB_THUMBTRACK)
            //    {
            //        int highWord = HIWORD(m.WParam.ToInt32());
            //        Trace.WriteLine("ThumbTrack scroll box position: " + highWord);
            //    }
            //    else if (lowWord == SB_THUMBPOSITION)
            //    {
            //        int highWord = HIWORD(m.WParam.ToInt32());
            //        Trace.WriteLine("ThumbTrack scroll box position: " + highWord);
            //    }
            //    else
            //    {
            //        Trace.WriteLine("up");
            //    }
            //}

            if (m.Msg == WM_NCMOUSEMOVE)
            {
                //Trace.WriteLine("RTFBox NC_MOUSEMOVE x: " + SignedLOWORD(m.LParam));
                //Trace.WriteLine("RTFBox NC_MOUSEMOVE y: " + SignedHIWORD(m.LParam));
            }

            if (m.Msg == WM_NCLBUTTONDOWN
               || m.Msg == WM_NCLBUTTONUP
                || m.Msg == WM_NCLBUTTONDBLCLK

                || m.Msg == WM_NCMBUTTONDOWN
                || m.Msg == WM_NCMBUTTONUP
                || m.Msg == WM_NCMBUTTONDBLCLK

                || m.Msg == WM_NCRBUTTONDOWN
                || m.Msg == WM_NCRBUTTONUP
                || m.Msg == WM_NCRBUTTONDBLCLK
            )
            {
                //Trace.WriteLine("RTFBox: " + m.Msg);
                //Trace.WriteLine("RTFBox x: " + SignedLOWORD(m.LParam));
                //Trace.WriteLine("RTFBox y: " + SignedHIWORD(m.LParam));
            }

            //Trace.WriteLine("RTFBox msg: " + m.Msg);

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

        #region Better vertical scrolling - original contribution by Xanfre

        private SCROLLINFO _scrollInfo;
        private int _wheelAccum;

        private void InitScrollInfo()
        {
            // Make sure this is valid right from the start
            _scrollInfo.cbSize = (uint)Marshal.SizeOf(_scrollInfo);
            _scrollInfo.fMask = (uint)ScrollInfoMask.SIF_ALL;
        }

        private static SCROLLINFO GetCurrentScrollInfo(IntPtr handle)
        {
            var si = new SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = (uint)ScrollInfoMask.SIF_ALL;
            GetScrollInfo(handle, (int)ScrollBarDirection.SB_VERT, ref si);
            return si;
        }

        private void ResetScrollInfo()
        {
            var si = new SCROLLINFO();
            si.cbSize = (uint)Marshal.SizeOf(si);
            si.fMask = (uint)ScrollInfoMask.SIF_ALL;
            si.nPos = 0;
            _scrollInfo = si;
            RepositionScroll(Handle, si);
        }

        /*
        When the rtfbox is first focused after content load, it will scroll to the top automatically (or more
        specifically to the cursor location, which will always be at the top after content load and before focus).
        Any subsequent de-focus and refocus will not cause this behavior, even if the cursor is still at the top,
        until the next content load.
        This auto-scroll-to-the-top behavior doesn't align with any events, and in fact seems to just happen in
        the background as soon as it feels like it and as soon as the executing thread has a slot for it. Hence
        this filthy hack where we keep track of the scroll position, and then on focus we do a brief async delay
        to let the auto-scroll happen, then set correct scroll position again.
        I don't like the "wait-and-hope" method at all, but hey... worst case, the auto-top-scroll will still
        happen, and that's no worse than it was before.
        */
        private void Workarounds_OnVScroll()
        {
            _scrollInfo = GetCurrentScrollInfo(Handle);
        }

        protected override async void OnEnter(EventArgs e)
        {
            _wheelAccum = 0;
            await SetScrollPositionToCorrect();

            base.OnEnter(e);
        }

        private async Task SetScrollPositionToCorrect()
        {
            SCROLLINFO si = _scrollInfo;

            this.SuspendDrawing();
            try
            {
                await Task.Delay(20);

                _scrollInfo = si;

                RepositionScroll(Handle, si);
            }
            finally
            {
                this.ResumeDrawing();
            }
        }

        private static bool VerticalScrollBarVisible(Control ctl)
        {
            int style = GetWindowLong(ctl.Handle, -16);
            return (style & 0x200000) != 0;
        }

        private static void BetterScroll(IntPtr handle, int pixels)
        {
            if (pixels == 0) return;

            SCROLLINFO si = GetCurrentScrollInfo(handle);

            si.nPos += pixels;

            RepositionScroll(handle, si);
        }

        private static void RepositionScroll(IntPtr handle, SCROLLINFO si)
        {
            // Reposition scroll
            SetScrollInfo(handle, (int)ScrollBarDirection.SB_VERT, ref si, true);

            // Send a WM_VSCROLL scroll message using SB_THUMBTRACK as wParam
            // SB_THUMBTRACK: low-order word of wParam, si.nPos high-order word of wParam
            IntPtr ptrWParam = new IntPtr(SB_THUMBTRACK + (0x10000 * si.nPos));
            IntPtr ptrLParam = new IntPtr(0);

            IntPtr wp = (long)ptrWParam >= 0 ? ptrWParam : (IntPtr)SB_THUMBTRACK;
            SendMessage(handle, WM_VSCROLL, wp, ptrLParam);
        }

        // Intercept mousewheel and make RichTextBox scroll using the above method
        private void InterceptMousewheel(ref Message m)
        {
            if (((ModifierKeys & Keys.Control) != 0) || !VerticalScrollBarVisible(this))
            {
                base.WndProc(ref m);
                return;
            }

            const int delta = 120;
            _wheelAccum += (int)m.WParam >> 16;
            if (Math.Abs(_wheelAccum) >= delta)
            {
                while (_wheelAccum >= delta)
                {
                    BetterScroll(m.HWnd, -50);
                    _wheelAccum -= delta;
                }
                while (_wheelAccum <= -delta)
                {
                    BetterScroll(m.HWnd, 50);
                    _wheelAccum += delta;
                }
            }
        }

        #endregion

        #region Better reader mode

        // Better reader mode
        private readonly Timer _autoScrollTimer = new Timer { Interval = 10 };
        private int _scrollIncrementY;
        private Rectangle _cursorScrollBounds = new Rectangle(0, 0, 26, 26);
        private bool _endOnMouseUp;

        private void InitReaderMode()
        {
            _autoScrollTimer.Tick += AutoScrollTimer_Tick;
        }

        private void InterceptMiddleMouseButton(ref Message m)
        {
            if (VerticalScrollBarVisible(this))
            {
                m.Result = (IntPtr)1;
                EnterReaderMode();
            }
        }

        private void EnterReaderMode()
        {
            _cursorScrollBounds.Location = Point.Subtract(PointToClient(MousePosition), new Size(_cursorScrollBounds.Width / 2, _cursorScrollBounds.Height / 2));

            SetCursor(new HandleRef(Cursors.NoMoveVert, Cursors.NoMoveVert.Handle));
            _autoScrollTimer.Start();
            _endOnMouseUp = false;

            // bounds to get the scrolling sensitivity
            // Fen's note: This new Rectangle's dimensions are different, so don't try to get rid of this one and
            // replace it with the global one, or scroll will break.
            var scrollBounds = new Rectangle(_cursorScrollBounds.Left, _cursorScrollBounds.Top, _cursorScrollBounds.Right, _cursorScrollBounds.Bottom);

            IntPtr rectPtr = Marshal.AllocHGlobal(Marshal.SizeOf(scrollBounds));

            try
            {
                Marshal.StructureToPtr(scrollBounds, rectPtr, true);

                var readerInfo = new READERMODEINFO
                {
                    hwnd = Handle,
                    fFlags = ReaderModeFlags.VerticalOnly,
                    prc = rectPtr,
                    lParam = IntPtr.Zero,
                    fFlags2 = TranslateDispatchCallback,
                    pfnScroll = ReaderScrollCallback
                };

                readerInfo.cbSize = Marshal.SizeOf(readerInfo);

                DoReaderMode(ref readerInfo);
            }
            finally
            {
                Marshal.FreeHGlobal(rectPtr);
            }
        }

        private void AutoScrollTimer_Tick(object sender, EventArgs e)
        {
            if (_scrollIncrementY != 0) BetterScroll(Handle, _scrollIncrementY);
        }

        private bool TranslateDispatchCallback(ref Message msg)
        {
            bool isMouseDown = msg.Msg == WM_LBUTTONDOWN ||
                               msg.Msg == WM_MBUTTONDOWN ||
                               msg.Msg == WM_RBUTTONDOWN ||
                               msg.Msg == WM_XBUTTONDOWN ||
                               msg.Msg == WM_MOUSEWHEEL ||
                               msg.Msg == WM_MOUSEHWHEEL ||
                               msg.Msg == WM_KEYDOWN;

            if (isMouseDown || (_endOnMouseUp && (msg.Msg == WM_MBUTTONUP)))
            {
                // exit reader mode
                _autoScrollTimer.Stop();
            }

            if ((!_endOnMouseUp && (msg.Msg == WM_MBUTTONUP)) || (msg.Msg == WM_MOUSELEAVE))
            {
                return true;
            }

            if (isMouseDown)
            {
                msg.Msg = WM_MBUTTONDOWN;
            }

            return false;
        }

        private bool ReaderScrollCallback(ref READERMODEINFO prmi, int dx, int dy)
        {
            if (dy == 0)
            {
                _scrollIncrementY = 0;
            }
            else
            {
                // Could be placebo, but using actual cursor delta rather than dy (which is a much smaller value)
                // seems to allow for a smoother acceleration curve (I feel like I notice some minor chunkiness
                // if I use dy)
                int cursY = Cursor.Position.Y;
                int origY = PointToScreen(_cursorScrollBounds.Location).Y + (_cursorScrollBounds.Height / 2);
                int delta = cursY < origY ? origY - cursY : cursY - origY;

                // Exponential scroll like most apps do - somewhat arbitrary values but has a decent feel.
                // Clamp to 1 (pixel) to correct for loss of precision in divide where it could end up as 0 for
                // too long.

                // OCD
                int deltaSquared;
                try
                {
                    deltaSquared = checked(delta * delta);
                }
                catch (OverflowException)
                {
                    deltaSquared = int.MaxValue;
                }

                int increment = (deltaSquared / 640).Clamp(1, 1024);

                _scrollIncrementY = cursY < origY ? -increment : increment;

                _endOnMouseUp = true;
            }

            return true;
        }

        #endregion

        #region Cursor fixes

        private bool LinkCursor;

        private void CursorHandler(ref Message m)
        {
            LinkCursor = false;
            // We have to call WndProc again via DefWndProc to let it receive the WM_REFLECT + WM_NOTIFY
            // message, which is the one that will actually tell us whether the mouse is over a link.
            // Real dopey, but whatevs.
            DefWndProc(ref m);
            if (LinkCursor)
            {
                SetCursor(new HandleRef(Cursors.Hand, Cursors.Hand.Handle));
                m.Result = (IntPtr)1;
            }
            // If the cursor isn't supposed to be Hand, then leave it be. Prevents cursor fighting where
            // it wants to set right-arrow-pointer but is then told to set IBeam etc.
        }

        private void CheckAndHandleEnLinkMsg(ref Message m)
        {
            if (((NMHDR)m.GetLParam(typeof(NMHDR))).code != EN_LINK)
            {
                base.WndProc(ref m);
                return;
            }

            var enlink = (ENLINK)m.GetLParam(typeof(ENLINK));
            /*
            @X64 (RichTextBox workarounds - link hand cursor handler)
            NOTE:
            If building for x64, then we have to do this instead (requires unsafe code and extra structs etc.):

            if (IntPtr.Size == 8)
            {
                enlink = ConvertFromENLINK64((ENLINK64)m.GetLParam(typeof(ENLINK64)));
            }
            else
            {
                enlink = (ENLINK)m.GetLParam(typeof(ENLINK));
            }
            */

            switch (enlink.msg)
            {
                case WM_SETCURSOR:
                    LinkCursor = true;
                    m.Result = (IntPtr)1;
                    return;
                case WM_LBUTTONDOWN:
                    // Run base link-mousedown handler (eventually) - otherwise we'd have to re-implement
                    // CharRangeToString() in managed code and junk
                    base.WndProc(ref m);
                    // Awful, awful, awful - async method call without await because you can't have async methods
                    // with ref params! But needed to make sure the position-keep hack works (otherwise there could
                    // be too long a delay before the auto-top-scroll happens and then it won't be re-positioned)
#pragma warning disable 4014
                    SetScrollPositionToCorrect();
#pragma warning restore 4014
                    return;
            }
            m.Result = IntPtr.Zero;
        }

        #endregion

        #region Workaround to fix black transparent regions in images

        // ReSharper disable IdentifierTypo
        // ReSharper disable StringLiteralTypo
        private static readonly byte[] _shppict = Encoding.ASCII.GetBytes(@"\shppict");
        private static readonly byte[] _shppictBlanked = Encoding.ASCII.GetBytes(@"\xxxxxxx");
        private static readonly byte[] _nonshppict = Encoding.ASCII.GetBytes(@"\nonshppict");
        private static readonly byte[] _nonshppictBlanked = Encoding.ASCII.GetBytes(@"\xxxxxxxxxx");
        // ReSharper restore StringLiteralTypo
        // ReSharper restore IdentifierTypo

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
            byte firstByte = pattern[0];
            int index = Array.IndexOf(input, firstByte);
            int pLen = pattern.Length;

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

        // List version
        private static void ReplaceByteSequence(List<byte> input, byte[] pattern, byte[] replacePattern)
        {
            byte firstByte = pattern[0];
            int index = input.IndexOf(firstByte);
            int pLen = pattern.Length;

            while (index > -1)
            {
                for (int i = 0; i < pLen; i++)
                {
                    if (index + i >= input.Count) return;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = input.IndexOf(firstByte, index + i)) == -1) return;
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

        #endregion

        #region Save/restore zoom (workaround for zoom factor resetting on content load)

        // Because the damn thing resets its zoom every time you load new content, we have to keep a global var
        // with the zoom value and keep both values in sync.

        private float _storedZoomFactor = 1.0f;

        private void SetZoomFactorClamped(float zoomFactor) => ZoomFactor = zoomFactor.ClampToRichTextBoxZoomMinMax();

        private void SetStoredZoomFactorClamped(float zoomFactor) => _storedZoomFactor = zoomFactor.ClampToRichTextBoxZoomMinMax();

        private void SaveZoom() => SetStoredZoomFactorClamped(ZoomFactor);

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
                SetZoomFactorClamped(_storedZoomFactor);
            }
            catch (ArgumentException)
            {
                // Do nothing; remain at 1.0
            }
        }

        #endregion

        private void DisposeWorkarounds() => _autoScrollTimer.Dispose();
    }
}

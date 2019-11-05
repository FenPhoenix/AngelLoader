using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static AngelLoader.CustomControls.RichTextBoxCustom_Interop;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.CustomControls
{
    internal sealed partial class RichTextBoxCustom
    {
        #region Private fields

        private SCROLLINFO _scrollInfo;

        private readonly Timer tmrAutoScroll = new Timer { Interval = 10 };
        private int scrollIncrementY;
        // No picture is used currently
        private readonly PictureBox pbGlyph = new PictureBox { Size = new Size(26, 26), Visible = false };
        private bool endOnMouseUp;
        private int WheelAccum;

        #endregion

        private void InitScrollInfo()
        {
            // Make sure this is valid right from the start
            _scrollInfo.cbSize = (uint)Marshal.SizeOf(_scrollInfo);
            _scrollInfo.fMask = (uint)ScrollInfoMask.SIF_ALL;
        }

        private void InitReaderMode()
        {
            tmrAutoScroll.Tick += tmrAutoScroll_Tick;
            Controls.Add(pbGlyph);
        }

        #region Better vertical scrolling - original contribution by Xanfre

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
        protected override void OnVScroll(EventArgs e)
        {
            _scrollInfo = GetCurrentScrollInfo(Handle);

            base.OnVScroll(e);
        }

        protected override async void OnEnter(EventArgs e)
        {
            WheelAccum = 0;
            await SetScrollPositionToCorrect();

            base.OnEnter(e);
        }

        private async Task SetScrollPositionToCorrect()
        {
            var si = _scrollInfo;

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

            var si = GetCurrentScrollInfo(handle);

            si.nPos += pixels;

            RepositionScroll(handle, si);
        }

        private static void RepositionScroll(IntPtr handle, SCROLLINFO si)
        {
            // Reposition scroll
            SetScrollInfo(handle, (int)ScrollBarDirection.SB_VERT, ref si, true);

            // Send a WM_VSCROLL scroll message using SB_THUMBTRACK as wParam
            // SB_THUMBTRACK: low-order word of wParam, si.nPos high-order word of wParam
            IntPtr ptrWParam = new IntPtr(SB_THUMBTRACK + 0x10000 * si.nPos);
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
            WheelAccum += (int)m.WParam >> 16;
            if (Math.Abs(WheelAccum) >= delta)
            {
                while (WheelAccum >= delta)
                {
                    BetterScroll(m.HWnd, -50);
                    WheelAccum -= delta;
                }
                while (WheelAccum <= -delta)
                {
                    BetterScroll(m.HWnd, 50);
                    WheelAccum += delta;
                }
            }
        }

        #endregion

        #region Better reader mode

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
            pbGlyph.Location = Point.Subtract(PointToClient(MousePosition), new Size(pbGlyph.Width / 2, pbGlyph.Height / 2));

            SetCursor(new HandleRef(Cursors.NoMoveVert, Cursors.NoMoveVert.Handle));
            tmrAutoScroll.Start();
            endOnMouseUp = false;

            // bounds to get the scrolling sensitivity			
            var scrollBounds = new Rectangle(pbGlyph.Left, pbGlyph.Top, pbGlyph.Right, pbGlyph.Bottom);
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

        private void tmrAutoScroll_Tick(object sender, EventArgs e)
        {
            if (scrollIncrementY != 0) BetterScroll(Handle, scrollIncrementY);
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

            if (isMouseDown || (endOnMouseUp && (msg.Msg == WM_MBUTTONUP)))
            {
                // exit reader mode
                tmrAutoScroll.Stop();
            }

            if ((!endOnMouseUp && (msg.Msg == WM_MBUTTONUP)) || (msg.Msg == WM_MOUSELEAVE))
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
                scrollIncrementY = 0;
            }
            else
            {
                // Could be placebo, but using actual cursor delta rather than dy (which is a much smaller value)
                // seems to allow for a smoother acceleration curve (I feel like I notice some minor chunkiness
                // if I use dy)
                int cursY = Cursor.Position.Y;
                int origY = PointToScreen(pbGlyph.Location).Y + (pbGlyph.Height / 2);
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

                scrollIncrementY = cursY < origY ? -increment : increment;

                endOnMouseUp = true;
            }

            return true;
        }

        #endregion

        #region Cursor fixes

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
                    // Update 2019-06-30: No wonder this works, it's the last line before the return, and that
                    // works as long as you don't involve exceptions, and there are none here. Fantastic!
#pragma warning disable 4014
                    SetScrollPositionToCorrect();
#pragma warning restore 4014
                    return;
            }
            m.Result = IntPtr.Zero;
        }

        #endregion
    }
}

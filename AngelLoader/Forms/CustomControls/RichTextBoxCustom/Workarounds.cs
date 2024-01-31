using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.WinFormsNative;

namespace AngelLoader.Forms.CustomControls;

internal sealed partial class RichTextBoxCustom
{
    private void InitWorkarounds()
    {
        // Make sure this is valid right from the start
        _scrollInfo.cbSize = (uint)Marshal.SizeOf(_scrollInfo);
        _scrollInfo.fMask = (uint)Native.ScrollInfoMask.SIF_ALL;

        _autoScrollTimer.Tick += AutoScrollTimer_Tick;
    }

    private bool _fullDetectUrlsSet;

    /// <summary>
    /// Sets URL detection to include email addresses. This fixes the dark-on-dark coloring of email addresses
    /// that don't have "mailto:" in front of them.
    /// </summary>
    private void SetFullUrlsDetect()
    {
        if (_fullDetectUrlsSet) return;

        Native.SendMessage(Handle, Native.EM_AUTOURLDETECT, (IntPtr)(Native.AURL_ENABLEURL | Native.AURL_ENABLEEMAILADDR), IntPtr.Zero);
        _fullDetectUrlsSet = true;
    }

    protected override void WndProc(ref Message m)
    {
        switch ((uint)m.Msg)
        {
            case Native.WM_PAINT:
                if (_darkModeEnabled)
                {
                    using (new Win32ThemeHooks.OverrideSysColorScope(Win32ThemeHooks.Override.RichText))
                    {
                        base.WndProc(ref m);
                    }
                }
                else
                {
                    base.WndProc(ref m);
                }
                break;
            case Native.WM_MOUSEWHEEL:
                // Intercept the mousewheel call and direct it to use the fixed scrolling
                InterceptMousewheel(ref m);
                break;
            case Native.WM_MBUTTONDOWN:
            case Native.WM_MBUTTONDBLCLK:
                // Intercept the middle mouse button and direct it to use the fixed reader mode
                InterceptMiddleMouseButton(ref m);
                break;
            // CursorHandler() essentially "calls" this section, and this section "returns" whether the cursor
            // was over a link (via LinkCursor)
            case Native.WM_REFLECT + Native.WM_NOTIFY:
                CheckAndHandleEnLinkMsg(ref m);
                break;
            case Native.WM_SETCURSOR:
                CursorHandler(ref m);
                break;
            default:
                base.WndProc(ref m);
                break;
        }
    }

    #region Better vertical scrolling - original contribution by Xanfre

    private Native.SCROLLINFO _scrollInfo;
    private int _wheelAccum;

    private void ResetScrollInfo()
    {
        var si = new Native.SCROLLINFO();
        si.cbSize = (uint)Marshal.SizeOf(si);
        si.fMask = (uint)Native.ScrollInfoMask.SIF_ALL;
        si.nPos = 0;
        _scrollInfo = si;
        ControlUtils.RepositionScroll(Handle, si, Native.SB_VERT);
    }

    /*
    When the rtfbox is first focused after content load, it will scroll to the top automatically (or more
    specifically to the cursor location, which will always be at the top after content load and before focus).
    Any subsequent de-focus and refocus will not cause this behavior, even if the cursor is still at the top,
    until the next content load.
    This auto-scroll-to-the-top behavior doesn't align with any events, and in fact seems to just happen in
    the background as soon as it feels like it. Hence this filthy hack where we keep track of the scroll
    position, and then on focus we do a brief async delay to let the auto-scroll happen, then set correct
    scroll position again. I don't like the "wait-and-hope" method at all, but hey... worst case, the auto-
    top-scroll will still happen, and that's no worse than it was before.
    */
    private void Workarounds_OnVScroll()
    {
        _scrollInfo = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
    }

    protected override async void OnEnter(EventArgs e)
    {
        _wheelAccum = 0;
        await SetScrollPositionToCorrect();

        base.OnEnter(e);
    }

    private async Task SetScrollPositionToCorrect()
    {
        Native.SCROLLINFO si = _scrollInfo;

        this.SuspendDrawing();
        try
        {
            await Task.Delay(20);

            _scrollInfo = si;

            ControlUtils.RepositionScroll(Handle, si, Native.SB_VERT);
        }
        finally
        {
            this.ResumeDrawing();
        }
    }

    private static bool VerticalScrollBarVisible(Control ctl)
    {
        uint style = Native.GetWindowLongPtr(ctl.Handle, -16).ToUInt32();
        return (style & Native.WS_VSCROLL) != 0;
    }

    private static void BetterScroll(IntPtr handle, int pixels)
    {
        if (pixels == 0) return;

        Native.SCROLLINFO si = ControlUtils.GetCurrentScrollInfo(handle, Native.SB_VERT);

        si.nPos += pixels;

        ControlUtils.RepositionScroll(handle, si, Native.SB_VERT);
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
        _wheelAccum += Native.SignedHIWORD(m.WParam);
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

    private readonly Timer _autoScrollTimer = new Timer { Interval = 10 };
    private int _scrollIncrementY;
    private Rectangle _cursorScrollBounds = new Rectangle(0, 0, 26, 26);
    private bool _endOnMouseUp;

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
        _cursorScrollBounds.Location = Point.Subtract(this.PointToClient_Fast(MousePosition), new Size(_cursorScrollBounds.Width / 2, _cursorScrollBounds.Height / 2));

        Native.SetCursor(new HandleRef(Cursors.NoMoveVert, Cursors.NoMoveVert.Handle));
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

            var readerInfo = new Native.READERMODEINFO
            {
                hwnd = Handle,
                fFlags = Native.ReaderModeFlags.VerticalOnly,
                prc = rectPtr,
                lParam = IntPtr.Zero,
                fFlags2 = TranslateDispatchCallback,
                pfnScroll = ReaderScrollCallback
            };

            readerInfo.cbSize = Marshal.SizeOf(readerInfo);

            Native.DoReaderMode(ref readerInfo);
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

    private bool TranslateDispatchCallback(ref Native.MSG msg)
    {
        bool isMouseDown = msg.Msg
            is Native.WM_LBUTTONDOWN
            or Native.WM_MBUTTONDOWN
            or Native.WM_RBUTTONDOWN
            or Native.WM_XBUTTONDOWN
            or Native.WM_MOUSEWHEEL
            or Native.WM_MOUSEHWHEEL
            or Native.WM_KEYDOWN;

        if (isMouseDown || (_endOnMouseUp && (msg.Msg == Native.WM_MBUTTONUP)))
        {
            // exit reader mode
            _autoScrollTimer.Stop();
        }

        if ((!_endOnMouseUp && (msg.Msg == Native.WM_MBUTTONUP)) || (msg.Msg == Native.WM_MOUSELEAVE))
        {
            return true;
        }

        if (isMouseDown)
        {
            msg.Msg = Native.WM_MBUTTONDOWN;
        }

        return false;
    }

    private bool ReaderScrollCallback(ref Native.READERMODEINFO prmi, int dx, int dy)
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
            int cursY = Native.GetCursorPosition_Fast().Y;
            int origY = this.PointToScreen_Fast(_cursorScrollBounds.Location).Y + (_cursorScrollBounds.Height / 2);
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
            Native.SetCursor(new HandleRef(Cursors.Hand, Cursors.Hand.Handle));
            m.Result = (IntPtr)1;
        }
        // If the cursor isn't supposed to be Hand, then leave it be. Prevents cursor fighting where
        // it wants to set right-arrow-pointer but is then told to set IBeam etc.
    }

#if X64
    private static unsafe Native.ENLINK ConvertFromENLINK64(Native.ENLINK64 es64)
    {
        Native.ENLINK es = new();
        fixed (byte* es64p = &es64.contents[0])
        {
            es.nmhdr = new Native.NMHDR();
            es.charrange = new Native.CHARRANGE();
            es.nmhdr.hwndFrom = Marshal.ReadIntPtr((IntPtr)es64p);
            es.nmhdr.idFrom = Marshal.ReadIntPtr((IntPtr)(es64p + 8));
            es.nmhdr.code = Marshal.ReadInt32((IntPtr)(es64p + 16));
            es.msg = Marshal.ReadInt32((IntPtr)(es64p + 24));
            es.wParam = Marshal.ReadIntPtr((IntPtr)(es64p + 28));
            es.lParam = Marshal.ReadIntPtr((IntPtr)(es64p + 36));
            es.charrange.cpMin = Marshal.ReadInt32((IntPtr)(es64p + 44));
            es.charrange.cpMax = Marshal.ReadInt32((IntPtr)(es64p + 48));
        }
        return es;
    }
#endif

    private void CheckAndHandleEnLinkMsg(ref Message m)
    {
        if (((Native.NMHDR)m.GetLParam(typeof(Native.NMHDR))).code != Native.EN_LINK)
        {
            base.WndProc(ref m);
            return;
        }

        /*
        @X64 (RichTextBox workarounds - link hand cursor handler)
        The Framework code does this. It's commented:

        "On 64-bit, we do some custom marshalling to get this to work. The richedit control
        unfortunately does not respect IA64 struct alignment conventions."
        */
        Native.ENLINK enlink =
#if X64
            ConvertFromENLINK64((Native.ENLINK64)m.GetLParam(typeof(Native.ENLINK64)));
#else
            (Native.ENLINK)m.GetLParam(typeof(Native.ENLINK));
#endif

        switch (enlink.msg)
        {
            case Native.WM_SETCURSOR:
                LinkCursor = true;
                m.Result = (IntPtr)1;
                return;
            case Native.WM_LBUTTONDOWN:
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

    internal static readonly byte[] _shppict =
    {
#if false
        (byte)'{',
        (byte)'\\',
        (byte)'*',
#endif
        (byte)'\\',
        (byte)'s',
        (byte)'h',
        (byte)'p',
        (byte)'p',
        (byte)'i',
        (byte)'c',
        (byte)'t'
    };

    internal static readonly byte[] _shppictBlanked =
    {
#if false
        (byte)'{',
        (byte)'\\',
        (byte)'*',
#endif
        (byte)'\\',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x'
    };

    internal static readonly byte[] _nonshppict =
    {
#if false
        (byte)'{',
#endif
        (byte)'\\',
        (byte)'n',
        (byte)'o',
        (byte)'n',
        (byte)'s',
        (byte)'h',
        (byte)'p',
        (byte)'p',
        (byte)'i',
        (byte)'c',
        (byte)'t'
    };

    internal static readonly byte[] _nonshppictBlanked =
    {
#if false
        (byte)'{',
#endif
        (byte)'\\',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x',
        (byte)'x'
    };

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

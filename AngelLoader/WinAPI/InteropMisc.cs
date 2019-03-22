using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace AngelLoader.WinAPI
{
    internal static class InteropMisc
    {
        internal const int WH_MOUSE = 7;
        internal const int WH_MOUSE_LL = 14;

        #region Scrollbar

        internal const int WM_SCROLL = 276;
        internal const int WM_VSCROLL = 277;
        internal const int SB_LINEUP = 0;
        internal const int SB_LINELEFT = 0;
        internal const int SB_LINEDOWN = 1;
        internal const int SB_LINERIGHT = 1;
        internal const int SB_PAGEUP = 2;
        internal const int SB_PAGELEFT = 2;
        internal const int SB_PAGEDOWN = 3;
        internal const int SB_PAGERIGHT = 3;
        internal const int SB_PAGETOP = 6;
        internal const int SB_LEFT = 6;
        internal const int SB_PAGEBOTTOM = 7;
        internal const int SB_RIGHT = 7;
        internal const int SB_ENDSCROLL = 8;
        internal const int SBM_GETPOS = 225;
        internal const int SB_HORZ = 0;

        #endregion

        #region Mouse

        internal const int WM_MOUSEWHEEL = 0x20A;
        internal const int WM_MOUSEMOVE = 0x200;
        internal const int WM_LBUTTONUP = 0x202;
        internal const int WM_MBUTTONUP = 0x208;
        internal const int WM_RBUTTONUP = 0x205;
        internal const int WM_LBUTTONDOWN = 0x201;
        internal const int WM_MBUTTONDOWN = 0x207;
        internal const int WM_RBUTTONDOWN = 0x204;

        internal const int WM_LBUTTONDBLCLK = 0x203;
        internal const int WM_MBUTTONDBLCLK = 0x209;
        internal const int WM_RBUTTONDBLCLK = 0x206;

        #endregion

        #region Keyboard

        internal const int WM_KEYDOWN = 0x100;
        internal const int VK_ESCAPE = 0x1B;

        #endregion

        internal const int WM_SETREDRAW = 11;

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point pt);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        [DllImport("user32.dll")]
        internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [DllImport("kernel32")]
        internal static extern uint GetCurrentThreadId();
    }
}

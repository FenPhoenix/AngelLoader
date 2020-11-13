using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader.WinAPI
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static class InteropMisc
    {
        internal const int WM_USER = 0x0400;
        internal const int WM_REFLECT = WM_USER + 0x1C00;
        internal const int WM_NOTIFY = 0x004E;
        internal const int WM_SETREDRAW = 11;

        internal const int HWND_BROADCAST = 0xffff;

        internal const int EN_LINK = 0x070b;

        internal const int WM_CHANGEUISTATE = 0x0127;

        #region WM_CHANGEUISTATE

        private const int UIS_SET = 1;
        //internal const int UIS_CLEAR = 2;
        //internal const int UIS_INITIALIZE = 3;

        private const int UISF_HIDEFOCUS = 1;
        //internal const int UISF_HIDEACCEL = 2;
        //internal const int UISF_ACTIVE = 4;

        internal const int SetControlFocusToHidden = UISF_HIDEFOCUS + (UIS_SET << 16);

        #endregion

        #region Scrollbar

        internal const int WM_SCROLL = 276;
        internal const int WM_VSCROLL = 277;
        //internal const int SB_LINEUP = 0;
        internal const int SB_LINELEFT = 0;
        //internal const int SB_LINEDOWN = 1;
        internal const int SB_LINERIGHT = 1;
        /*
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
        */
        internal const uint SB_THUMBTRACK = 5;

        #endregion

        #region Mouse

        // NC prefix means the mouse was over a non-client area

        internal const int WM_SETCURSOR = 0x20;

        internal const int WM_MOUSEWHEEL = 0x20A;
        internal const int WM_MOUSEHWHEEL = 0x020E; // Mousewheel tilt

        internal const int WM_MOUSEMOVE = 0x200;
        internal const int WM_NCMOUSEMOVE = 0xA0;

        internal const int WM_MOUSELEAVE = 0x02A3;

        internal const int WM_LBUTTONUP = 0x202;
        internal const int WM_NCLBUTTONUP = 0x00A2;
        internal const int WM_MBUTTONUP = 0x208;
        internal const int WM_NCMBUTTONUP = 0xA8;
        internal const int WM_RBUTTONUP = 0x205;
        internal const int WM_NCRBUTTONUP = 0xA5;

        internal const int WM_LBUTTONDOWN = 0x201;
        internal const int WM_NCLBUTTONDOWN = 0x00A1;
        internal const int WM_MBUTTONDOWN = 0x207;
        internal const int WM_NCMBUTTONDOWN = 0xA7;
        internal const int WM_RBUTTONDOWN = 0x204;
        internal const int WM_NCRBUTTONDOWN = 0xA4;

        internal const int WM_XBUTTONDOWN = 0x020B;
        //internal const int WM_NCXBUTTONDOWN = 0x0AB;

        internal const int WM_LBUTTONDBLCLK = 0x203;
        internal const int WM_NCLBUTTONDBLCLK = 0xA3;
        internal const int WM_MBUTTONDBLCLK = 0x209;
        internal const int WM_NCMBUTTONDBLCLK = 0xA9;
        internal const int WM_RBUTTONDBLCLK = 0x206;
        internal const int WM_NCRBUTTONDBLCLK = 0xA6;

        #endregion

        #region Keyboard

        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_SYSKEYDOWN = 0x104;
        internal const int WM_KEYUP = 0x101;

        // MK_ only to be used in mouse messages
        internal const int MK_CONTROL = 0x8;

        // VK_ only to be used in keyboard messages
        /*
        internal const int VK_SHIFT = 0x10;
        internal const int VK_CONTROL = 0x11;
        internal const int VK_ALT = 0x12; // this is supposed to be called VK_MENU but screw that
        internal const int VK_ESCAPE = 0x1B;
        internal const int VK_PAGEUP = 0x21; // VK_PRIOR
        internal const int VK_PAGEDOWN = 0x22; // VK_NEXT
        internal const int VK_END = 0x23;
        internal const int VK_HOME = 0x24;
        internal const int VK_LEFT = 0x25;
        internal const int VK_UP = 0x26;
        internal const int VK_RIGHT = 0x27;
        internal const int VK_DOWN = 0x28;
        */

        #endregion

        // Second-instance telling first instance to show itself junk
        public static readonly int WM_SHOWFIRSTINSTANCE = RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|" + Misc.AppGuid);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point pt);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern int SendMessage(IntPtr hWnd, int wMsg, bool wParam, int lParam);

        #region Process

        internal const uint QUERY_LIMITED_INFORMATION = 0x00001000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool QueryFullProcessImageName([In] SafeProcessHandle hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll")]
        internal static extern SafeProcessHandle OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace AngelLoader.WinAPI
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static class Native
    {
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr CreateSolidBrush(int crColor);

        #region ListView

        private const int LVM_FIRST = 0x1000;
        internal const int LVM_SETITEMA = LVM_FIRST + 6;
        internal const int LVM_SETITEMW = LVM_FIRST + 76;
        internal const int LVM_INSERTITEMA = LVM_FIRST + 7;
        internal const int LVM_INSERTITEMW = LVM_FIRST + 77;
        internal const int LVM_DELETEITEM = LVM_FIRST + 8;
        internal const int LVM_DELETEALLITEMS = LVM_FIRST + 9;

        #endregion

        private const int WM_USER = 0x0400;
        internal const int WM_REFLECT = WM_USER + 0x1C00;
        internal const int WM_NOTIFY = 0x004E;
        internal const int WM_SETREDRAW = 0x000B;
        internal const int WM_NCPAINT = 0x0085;
        internal const int WM_PAINT = 0x000F;
        internal const int WM_ERASEBKGND = 0x0014;
        internal const int WM_MOVE = 0x0003;
        internal const int WM_SIZE = 0x0005;
        internal const int WM_WINDOWPOSCHANGED = 0x0047;
        internal const int WM_ENABLE = 0x000A;

        internal const uint WM_CTLCOLORLISTBOX = 0x0134;
        internal const int SWP_NOSIZE = 0x0001;

        internal const uint WS_VSCROLL = 0x00200000;

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        public sealed class GraphicsContext : IDisposable
        {
            private readonly IntPtr _hWnd;
            private readonly IntPtr _dc;
            public readonly Graphics G;

            public GraphicsContext(IntPtr hWnd)
            {
                _hWnd = hWnd;
                _dc = GetWindowDC(_hWnd);
                G = Graphics.FromHwnd(_hWnd);
            }

            public void Dispose()
            {
                G.Dispose();
                ReleaseDC(_hWnd, _dc);
            }
        }

        #region Window

        internal static UIntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            return Environment.Is64BitProcess
                ? GetWindowLongPtr64(hWnd, nIndex)
                : GetWindowLong32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern UIntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern UIntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        /*
        // This static method is required because legacy OSes do not support SetWindowLongPtr
        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, UIntPtr dwNewLong)
        {
            return Environment.Is64BitProcess
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : SetWindowLong32(hWnd, nIndex, dwNewLong.ToUInt32());
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, UIntPtr dwNewLong);
        */

        #endregion

        internal const int HWND_BROADCAST = 0xffff;

        internal const int EN_LINK = 0x070b;

        internal const int WM_CHANGEUISTATE = 0x0127;

        internal const uint OBJID_HSCROLL = 0xFFFFFFFA;
        internal const uint OBJID_VSCROLL = 0xFFFFFFFB;

        internal const int SB_HORZ = 0;
        internal const int SB_VERT = 1;

        #region lParam/wParam

        // This code is straight from the .NET source, so keep it exactly the same despite "unnecessary cast" warnings

        // ReSharper disable RedundantCast
#pragma warning disable IDE0004
        [PublicAPI]
        internal static int MAKELONG(int low, int high) => high << 16 | (low & (int)ushort.MaxValue);
        [PublicAPI]
        internal static IntPtr MAKELPARAM(int low, int high) => (IntPtr)(high << 16 | (low & (int)ushort.MaxValue));
        [PublicAPI]
        internal static int HIWORD(int n) => n >> 16 & (int)ushort.MaxValue;
        [PublicAPI]
        internal static int HIWORD(IntPtr n) => HIWORD((int)(long)n);
        [PublicAPI]
        internal static int LOWORD(int n) => n & (int)ushort.MaxValue;
        [PublicAPI]
        internal static int LOWORD(IntPtr n) => LOWORD((int)(long)n);
        [PublicAPI]
        internal static int SignedHIWORD(IntPtr n) => SignedHIWORD((int)(long)n);
        [PublicAPI]
        internal static int SignedLOWORD(IntPtr n) => SignedLOWORD((int)(long)n);
        [PublicAPI]
        internal static int SignedHIWORD(int n) => (int)(short)HIWORD(n);
        [PublicAPI]
        internal static int SignedLOWORD(int n) => (int)(short)LOWORD(n);
#pragma warning restore IDE0004
        // ReSharper restore RedundantCast

        #endregion

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

        internal const int WM_SCROLL = 0x114;
        internal const int WM_VSCROLL = 0x115;
        internal const int WM_HSCROLL = 0x114;
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

        [DllImport("user32.dll")]
        internal static extern void SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, ref DATETIMEPICKERINFO lParam);

        #region Process

        internal const uint QUERY_LIMITED_INFORMATION = 0x00001000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool QueryFullProcessImageName([In] SafeProcessHandle hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll")]
        internal static extern SafeProcessHandle OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        #endregion

        [DllImport("user32.dll")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern IntPtr SetCursor(HandleRef hcursor);

        #region RichTextBox

        #region Reader mode

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal delegate bool TranslateDispatchCallbackDelegate(ref Message lpmsg);

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal delegate bool ReaderScrollCallbackDelegate(ref READERMODEINFO prmi, int dx, int dy);

        [Flags]
        internal enum ReaderModeFlags
        {
            //None = 0x00,
            //ZeroCursor = 0x01,
            VerticalOnly = 0x02,
            //HorizontalOnly = 0x04
        }

        [StructLayout(LayoutKind.Sequential)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal struct READERMODEINFO
        {
            internal int cbSize;
            internal IntPtr hwnd;
            internal ReaderModeFlags fFlags;
            internal IntPtr prc;
            internal ReaderScrollCallbackDelegate pfnScroll;
            internal TranslateDispatchCallbackDelegate fFlags2;
            internal IntPtr lParam;
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [DllImport("comctl32.dll", SetLastError = true, EntryPoint = "#383")]
        internal static extern void DoReaderMode(ref READERMODEINFO prmi);

        #endregion

        #region Cursor fix

        [StructLayout(LayoutKind.Sequential)]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "CommentTypo")]
        internal struct NMHDR
        {
            internal IntPtr hwndFrom;
            internal IntPtr idFrom; //This is declared as UINT_PTR in winuser.h
            internal int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal class ENLINK
        {
            internal NMHDR nmhdr;
            internal int msg = 0;
            internal IntPtr wParam = IntPtr.Zero;
            internal IntPtr lParam = IntPtr.Zero;
            internal CHARRANGE? charrange = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal class CHARRANGE
        {
            internal int cpMin;
            internal int cpMax;
        }

        #endregion

        internal const int EM_AUTOURLDETECT = WM_USER + 91;

        internal const int AURL_ENABLEURL = 1;
        internal const int AURL_ENABLEEMAILADDR = 2;

        #endregion

        #region Scroll

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [StructLayout(LayoutKind.Sequential)]
        internal struct SCROLLINFO
        {
            internal uint cbSize;
            internal uint fMask;
            internal int nMin;
            internal int nMax;
            internal uint nPage;
            internal int nPos;
            internal int nTrackPos;
        }

        //[PublicAPI]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [Flags]
        internal enum ScrollInfoMask
        {
            SIF_RANGE = 0x0001,
            SIF_PAGE = 0x0002,
            SIF_POS = 0x0004,
            //SIF_DISABLENOSCROLL = 0x0008,
            SIF_TRACKPOS = 0x0010,
            SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        #endregion

        #region Scroll bars

        public readonly struct RECT
        {
            public readonly int left;
            public readonly int top;
            public readonly int right;
            public readonly int bottom;

            [UsedImplicitly]
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        [PublicAPI]
        internal struct SCROLLBARINFO
        {
            internal int cbSize;
            internal RECT rcScrollBar;
            internal int dxyLineButton;
            internal int xyThumbTop;
            internal int xyThumbBottom;
            internal int reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            internal int[] rgstate;
        }

        internal const int STATE_SYSTEM_INVISIBLE = 0x00008000;
        internal const int STATE_SYSTEM_UNAVAILABLE = 0x00000001;

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        internal static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref SCROLLBARINFO psbi);

        #endregion

        internal const int TTP_STANDARD = 1;
        internal const int TTP_STANDARDTITLE = 2;
        //internal const int TTP_BALLOON = 3;
        //internal const int TTP_BALLOONTITLE = 4;
        //internal const int TTP_CLOSE = 5;
        //internal const int TTP_BALLOONSTEM = 6;
        //internal const int TTP_WRENCH = 7;

        internal const int DTM_GETDATETIMEPICKERINFO = 0x100E;

        //internal const int STATE_SYSTEM_HOTTRACKED = 0x00000080;
        internal const int STATE_SYSTEM_PRESSED = 0x00000008;

        [StructLayout(LayoutKind.Sequential)]
        [PublicAPI]
        internal struct DATETIMEPICKERINFO
        {
            internal int cbSize;
            internal RECT rcCheck;
            internal int stateCheck;
            internal RECT rcButton;
            internal int stateButton;
            internal IntPtr hwndEdit;
            internal IntPtr hwndUD;
            internal IntPtr hwndDropDown;
        }

        #region Theming

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);

        //[DllImport("uxtheme.dll", ExactSpelling = true)]
        //internal static extern IntPtr GetWindowTheme(IntPtr hWnd);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenThemeData(IntPtr hWnd, string classList);

        [DllImport("uxtheme.dll", ExactSpelling = true)]
        public static extern int CloseThemeData(IntPtr hTheme);

        internal const int
            ABS_UPNORMAL = 1,
            ABS_UPHOT = 2,
            ABS_UPPRESSED = 3,
            ABS_UPDISABLED = 4,
            ABS_DOWNNORMAL = 5,
            ABS_DOWNHOT = 6,
            ABS_DOWNPRESSED = 7,
            ABS_DOWNDISABLED = 8,
            ABS_LEFTNORMAL = 9,
            ABS_LEFTHOT = 10,
            ABS_LEFTPRESSED = 11,
            ABS_LEFTDISABLED = 12,
            ABS_RIGHTNORMAL = 13,
            ABS_RIGHTHOT = 14,
            ABS_RIGHTPRESSED = 15,
            ABS_RIGHTDISABLED = 16,
            ABS_UPHOVER = 17,
            ABS_DOWNHOVER = 18,
            ABS_LEFTHOVER = 19,
            ABS_RIGHTHOVER = 20;

        internal const int
            SCRBS_NORMAL = 1,
            SCRBS_HOT = 2,
            SCRBS_PRESSED = 3,
            //SCRBS_DISABLED = 4,
            SCRBS_HOVER = 5;

        internal const int
            SBP_ARROWBTN = 1,
            SBP_THUMBBTNHORZ = 2,
            SBP_THUMBBTNVERT = 3,
            //SBP_LOWERTRACKHORZ = 4,
            //SBP_UPPERTRACKHORZ = 5,
            //SBP_LOWERTRACKVERT = 6,
            //SBP_UPPERTRACKVERT = 7,
            SBP_GRIPPERHORZ = 8,
            SBP_GRIPPERVERT = 9,
            //SBP_SIZEBOX = 10,
            // Uh, this one isn't listed in vsstyle.h, but it works...?
            SBP_CORNER = 11;

        internal const int WM_THEMECHANGED = 0x031A;

        internal const int TMT_FILLCOLOR = 3802;
        internal const int TMT_TEXTCOLOR = 3803;

        [DllImport("uxtheme.dll", ExactSpelling = true)]
        internal static extern bool IsThemeActive();

        #endregion

        #region MessageBox/TaskDialog

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal enum SHSTOCKICONID : uint
        {
            SIID_HELP = 23,
            SIID_WARNING = 78,
            SIID_INFO = 79,
            SIID_ERROR = 80
        }

        internal const uint SHGSI_ICON = 0x000000100;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [PublicAPI]
        internal struct SHSTOCKICONINFO
        {
            internal uint cbSize;
            internal IntPtr hIcon;
            internal int iSysIconIndex;
            internal int iIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260/*MAX_PATH*/)]
            internal string szPath;
        }

        [DllImport("Shell32.dll", SetLastError = false)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern int SHGetStockIconInfo(SHSTOCKICONID siid, uint uFlags, ref SHSTOCKICONINFO psii);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyIcon(IntPtr hIcon);

        #endregion

        #region Enumerate window handles

        private delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);

        internal static List<IntPtr> GetProcessWindowHandles()
        {
            var handles = new List<IntPtr>();

            var threads = Process.GetCurrentProcess().Threads;
            foreach (ProcessThread thread in threads)
            {
                EnumThreadWindows(thread.Id, (hWnd, _) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }

            return handles;
        }

        #endregion
    }
}

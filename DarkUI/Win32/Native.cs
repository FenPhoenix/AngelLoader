using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DarkUI.Win32
{
    internal sealed class Native
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        #region Fen

        #region Mouse

        // NC prefix means the mouse was over a non-client area

        internal const int WM_SETCURSOR = 0x20;

        internal const int WM_MOUSEWHEEL = 0x20A;
        internal const int WM_MOUSEHWHEEL = 0x020E; // Mousewheel tilt

        internal const int WM_MOUSEMOVE = 0x200;
        internal const int WM_NCMOUSEMOVE = 0xA0;

        internal const int WM_MOUSELEAVE = 0x02A3;
        internal const int WM_NCMOUSELEAVE = 0x02A2;
        internal const int WM_MOUSEHOVER = 0x02A1;
        internal const int WM_NCMOUSEHOVER = 0x02A0;

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

        public const int WM_NCPAINT = 0x0085;
        public const int WM_CTLCOLORSCROLLBAR = 0x0137;

        public struct POINTS
        {
            public short x;
            public short y;

            public POINTS(short x, short y)
            {
                this.x = x;
                this.y = y;
            }
        }

        public const int HTHSCROLL = 6;
        public const int HTVSCROLL = 7;

        [DllImport("user32.dll")]
        internal static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);


        [DllImport("user32.dll")]
        internal static extern IntPtr PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, POINTS lParam);

        #region Scroll bars

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public const uint OBJID_HSCROLL = 0xFFFFFFFA;
        public const uint OBJID_VSCROLL = 0xFFFFFFFB;
        public const uint OBJID_CLIENT = 0xFFFFFFFC;

        public const uint SB_HORZ = 0;
        public const uint SB_VERT = 1;
        public const uint SB_CTL = 2;
        public const uint SB_BOTH = 3;

        public const int STATE_SYSTEM_INVISIBLE = 0x00008000;
        public const int STATE_SYSTEM_UNAVAILABLE = 0x00000001;

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        public static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref SCROLLBARINFO psbi);

        [StructLayout(LayoutKind.Sequential)]
        public struct SCROLLBARINFO
        {
            public int cbSize;
            public RECT rcScrollBar;
            public int dxyLineButton;
            public int xyThumbTop;
            public int xyThumbBottom;
            public int reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetScrollInfo(IntPtr hwnd, uint fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        public static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        public struct SCROLLINFO
        {
            public int cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        public const int SIF_RANGE = 0x0001;
        public const int SIF_PAGE = 0x0002;
        public const int SIF_POS = 0x0004;
        public const int SIF_DISABLENOSCROLL = 0x0008;
        public const int SIF_TRACKPOS = 0x0010;
        public const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

        #endregion

        #endregion
    }
}

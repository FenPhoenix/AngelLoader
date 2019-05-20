using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    internal class RichTextBoxCustom_Interop
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetCursor(HandleRef hcursor);

        #region Reader mode

        internal delegate bool TranslateDispatchCallbackDelegate(ref Message lpmsg);
        internal delegate bool ReaderScrollCallbackDelegate(ref READERMODEINFO prmi, int dx, int dy);

        [Flags]
        internal enum ReaderModeFlags
        {
            None = 0x00,
            ZeroCursor = 0x01,
            VerticalOnly = 0x02,
            HorizontalOnly = 0x04
        }

        [StructLayout(LayoutKind.Sequential)]
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

        [DllImport("comctl32.dll", SetLastError = true, EntryPoint = "#383")]
        public static extern void DoReaderMode(ref READERMODEINFO prmi);

        #endregion

        #region Cursor fix

        [StructLayout(LayoutKind.Sequential)]
        internal struct NMHDR
        {
            internal IntPtr hwndFrom;
            internal IntPtr idFrom; //This is declared as UINT_PTR in winuser.h
            internal int code;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class ENLINK
        {
            internal NMHDR nmhdr;
            internal int msg = 0;
            internal IntPtr wParam = IntPtr.Zero;
            internal IntPtr lParam = IntPtr.Zero;
            internal CHARRANGE charrange = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class CHARRANGE
        {
            internal int cpMin;
            internal int cpMax;
        }

        #endregion

        #region Scroll

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

        internal enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        internal enum ScrollInfoMask
        {
            SIF_RANGE = 0x0001,
            SIF_PAGE = 0x0002,
            SIF_POS = 0x0004,
            SIF_DISABLENOSCROLL = 0x0008,
            SIF_TRACKPOS = 0x0010,
            SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        internal static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int index);

        #endregion
    }
}

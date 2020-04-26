using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    internal static class RichTextBoxCustom_Interop
    {
        [DllImport("user32.dll"), SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern IntPtr SetCursor(HandleRef hcursor);

        #region Reader mode

        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal delegate bool TranslateDispatchCallbackDelegate(ref Message lpmsg);
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal delegate bool ReaderScrollCallbackDelegate(ref READERMODEINFO prmi, int dx, int dy);

        [Flags, UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        internal enum ReaderModeFlags
        {
            None = 0x00,
            ZeroCursor = 0x01,
            VerticalOnly = 0x02,
            HorizontalOnly = 0x04
        }

        [StructLayout(LayoutKind.Sequential), SuppressMessage("ReSharper", "IdentifierTypo")]
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

        [SuppressMessage("ReSharper", "StringLiteralTypo"), SuppressMessage("ReSharper", "IdentifierTypo")]
        [DllImport("comctl32.dll", SetLastError = true, EntryPoint = "#383")]
        internal static extern void DoReaderMode(ref READERMODEINFO prmi);

        #endregion

        #region Cursor fix

        [StructLayout(LayoutKind.Sequential), UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "IdentifierTypo"), SuppressMessage("ReSharper", "CommentTypo")]
        internal struct NMHDR
        {
            internal IntPtr hwndFrom;
            internal IntPtr idFrom; //This is declared as UINT_PTR in winuser.h
            internal int code;
        }

        [StructLayout(LayoutKind.Sequential), UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "RedundantDefaultMemberInitializer"), SuppressMessage("ReSharper", "IdentifierTypo")]
        internal class ENLINK
        {
            internal NMHDR nmhdr;
            internal int msg = 0;
            internal IntPtr wParam = IntPtr.Zero;
            internal IntPtr lParam = IntPtr.Zero;
            internal CHARRANGE? charrange = null;
        }

        [StructLayout(LayoutKind.Sequential), UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal class CHARRANGE
        {
            internal int cpMin;
            internal int cpMax;
        }

        #endregion

        #region Scroll

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers), SuppressMessage("ReSharper", "IdentifierTypo")]
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

        [PublicAPI, SuppressMessage("ReSharper", "IdentifierTypo")]
        internal enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        [PublicAPI, SuppressMessage("ReSharper", "IdentifierTypo")]
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
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll"), SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        [DllImport("user32.dll")]
        internal static extern int GetWindowLong(IntPtr hWnd, int index);

        #endregion
    }
}

using System;
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
        [DllImport("uxtheme.dll")]
        internal static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr CreateSolidBrush(int crColor);

        #region ListView

        internal const int LVM_FIRST = 0x1000;
        internal const int LVM_SETITEMA = (LVM_FIRST + 6);
        internal const int LVM_SETITEMW = (LVM_FIRST + 76);
        internal const int LVM_INSERTITEMA = (LVM_FIRST + 7);
        internal const int LVM_INSERTITEMW = (LVM_FIRST + 77);
        internal const int LVM_DELETEITEM = (LVM_FIRST + 8);
        internal const int LVM_DELETEALLITEMS = (LVM_FIRST + 9);
        internal const int LVM_SCROLL = LVM_FIRST + 20;
        internal const int LVM_REDRAWITEMS = (LVM_FIRST + 21);
        internal const int LVM_GETITEMA = (LVM_FIRST + 5);
        internal const int LVM_GETITEMW = (LVM_FIRST + 75);
        internal const int LVM_GETSELECTEDCOUNT = (LVM_FIRST + 50);
        internal const int LVM_FINDITEMW = (LVM_FIRST + 83);
        internal const int LVM_GETITEMSTATE = (LVM_FIRST + 44);
        internal const int LVM_HITTEST = (LVM_FIRST + 18);
        internal const int LVN_FIRST = -100;
        internal const int LVN_MARQUEEBEGIN = (LVN_FIRST - 56);

        #endregion

        internal const int WM_USER = 0x0400;
        internal const int WM_REFLECT = WM_USER + 0x1C00;
        internal const int WM_NOTIFY = 0x004E;
        internal const int WM_SETREDRAW = 0x000B;
        internal const int WM_NCPAINT = 0x0085;
        internal const int WM_CTLCOLORSCROLLBAR = 0x0137;
        internal const int WM_PAINT = 0x000F;
        internal const int WM_ERASEBKGND = 0x0014;
        internal const int WM_MOVE = 0x0003;
        internal const int WM_SIZE = 0x0005;
        internal const int WM_ACTIVATE = 0x0006;
        internal const int WM_WINDOWPOSCHANGED = 0x0047;

        internal const uint WM_CTLCOLORLISTBOX = 0x0134;
        internal const int SWP_NOSIZE = 0x0001;

        internal const uint WS_EX_CLIENTEDGE = 0x00000200;
        internal const uint WS_BORDER = 0x00800000;
        internal const uint WS_VSCROLL = 0x00200000;

        internal const int WM_SETFOCUS = 0x0007;
        internal const int WM_KILLFOCUS = 0x0008;

        internal const int GWL_STYLE = -16;
        internal const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        public sealed class DeviceContext : IDisposable
        {
            public IntPtr DC;
            private readonly IntPtr _hWnd;
            public DeviceContext(IntPtr hWnd)
            {
                _hWnd = hWnd;
                DC = GetWindowDC(_hWnd);
            }

            public void Dispose() => ReleaseDC(_hWnd, DC);
        }

        #region Window

        [Flags]
        internal enum RedrawWindowFlags : uint
        {
            Invalidate = 0X1,
            InternalPaint = 0X2,
            Erase = 0X4,
            Validate = 0X8,
            NoInternalPaint = 0X10,
            NoErase = 0X20,
            NoChildren = 0X40,
            AllChildren = 0X80,
            UpdateNow = 0X100,
            EraseNow = 0X200,
            Frame = 0X400,
            NoFrame = 0X800
        }

        [DllImport("user32.dll")]
        internal static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

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

        // This static method is required because legacy OSes do not support
        // SetWindowLongPtr
        internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, UIntPtr dwNewLong)
        {
            return Environment.Is64BitProcess
                ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
                : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToUInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, UIntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [Flags]
        internal enum SetWindowPosFlags : uint
        {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues,
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            AsynchronousWindowPosition = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to
            /// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            DoNotActivate = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid
            /// contents of the client area are saved and copied back into the client area after the window is sized or
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            DoNotCopyBits = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            IgnoreMove = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            DoNotChangeOwnerZOrder = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent
            /// window uncovered as a result of the window being moved. When this flag is set, the application must
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            DoNotRedraw = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            DoNotReposition = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            DoNotSendChangingEvent = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            IgnoreResize = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            IgnoreZOrder = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,
        }

        #endregion

        internal const int OCM__BASE = 8192;
        internal const int OCM_HSCROLL = (OCM__BASE + WM_HSCROLL);
        internal const int OCM_VSCROLL = (OCM__BASE + WM_VSCROLL);

        internal const int HWND_BROADCAST = 0xffff;

        internal const int EN_LINK = 0x070b;

        internal const int WM_CHANGEUISTATE = 0x0127;

        public const int SIF_RANGE = 0x0001;
        public const int SIF_PAGE = 0x0002;
        public const int SIF_POS = 0x0004;
        public const int SIF_DISABLENOSCROLL = 0x0008;
        public const int SIF_TRACKPOS = 0x0010;
        public const int SIF_ALL = SIF_RANGE | SIF_PAGE | SIF_POS | SIF_TRACKPOS;

        public const uint OBJID_HSCROLL = 0xFFFFFFFA;
        public const uint OBJID_VSCROLL = 0xFFFFFFFB;
        public const uint OBJID_CLIENT = 0xFFFFFFFC;

        public const int SB_HORZ = 0;
        public const int SB_VERT = 1;
        public const int SB_CTL = 2;
        public const int SB_BOTH = 3;

        #region lParam/wParam

        public static int MAKELONG(int low, int high) => high << 16 | low & (int)ushort.MaxValue;

        public static IntPtr MAKELPARAM(int low, int high) => (IntPtr)(high << 16 | low & (int)ushort.MaxValue);

        public static int HIWORD(int n) => n >> 16 & (int)ushort.MaxValue;

        public static int HIWORD(IntPtr n) => HIWORD((int)(long)n);

        public static int LOWORD(int n) => n & (int)ushort.MaxValue;

        public static int LOWORD(IntPtr n) => LOWORD((int)(long)n);

        public static int SignedHIWORD(IntPtr n) => SignedHIWORD((int)(long)n);

        public static int SignedLOWORD(IntPtr n) => SignedLOWORD((int)(long)n);

        public static int SignedHIWORD(int n) => (int)(short)(HIWORD(n));

        public static int SignedLOWORD(int n) => (int)(short)(LOWORD(n));

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

        #region Color

        /// <summary>
        /// <paramref name="RGB"/> - create it with ColorTranslator.ToWin32(Color)
        /// </summary>
        /// <param name="RGB"></param>
        /// <param name="H"></param>
        /// <param name="L"></param>
        /// <param name="S"></param>
        [DllImport("shlwapi.dll")]
        internal static extern void ColorRGBToHLS(int RGB, ref int H, ref int L, ref int S);

        /// <summary>
        /// ColorTranslator.FromWin32 on return int value
        /// </summary>
        /// <param name="H"></param>
        /// <param name="L"></param>
        /// <param name="S"></param>
        /// <returns></returns>
        [DllImport("shlwapi.dll")]
        internal static extern int ColorHLSToRGB(int H, int L, int S);

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

        // "A" version because RichTextBox uses the "A" version.
        // You'd think this should be a struct, but RichTextBox uses this class version. Shrug.
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class CHARFORMAT2A
        {
            public int cbSize = Marshal.SizeOf(typeof(CHARFORMAT2A));
            public int dwMask;
            public int dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] szFaceName = new byte[32];
            public short wWeight;
            public short sSpacing;
            public int crBackColor;
            public int lcid;
            public int dwReserved;
            public short sStyle;
            public short wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public class CHARFORMATA
        {
            public int cbSize = Marshal.SizeOf(typeof(CHARFORMATA));
            public int dwMask;
            public int dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] szFaceName = new byte[32];
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(
            HandleRef hWnd,
            int msg,
            int wParam,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] CHARFORMATA lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(
            HandleRef hWnd,
            int msg,
            int wParam,
            [MarshalAs(UnmanagedType.LPStruct), In, Out] CHARFORMAT2A lParam);

        internal const int SCF_SELECTION = 0x0001;
        internal const int SCF_WORD = 0x0002;
        internal const int SCF_DEFAULT = 0x0000;
        internal const int SCF_ALL = 0x0004;
        internal const int SCF_USEUIRULES = 0x0008;
        internal const int SCF_ASSOCIATEFONT = 0x0010;
        internal const int SCF_NOKBUPDATE = 0x0020;
        internal const int SCF_ASSOCIATEFONT2 = 0x0040;
        internal const int SCF_SMARTFONT = 0x0080;
        internal const int SCF_CHARREPFROMLCID = 0x0100;
        internal const int SPF_DONTSETDEFAULT = 0x0002;
        internal const int SPF_SETDEFAULT = 0x0004;
        internal const int EM_GETCHARFORMAT = (WM_USER + 58);
        internal const int EM_SETCHARFORMAT = (WM_USER + 68);
        internal const int CFM_COLOR = 0x40000000;
        internal const int CFE_AUTOCOLOR = 0x40000000;
        internal const int EM_AUTOURLDETECT = (WM_USER + 91);

        internal const int AURL_ENABLEURL = 1;
        internal const int AURL_ENABLEEMAILADDR = 2;
        internal const int AURL_ENABLETELNO = 4;
        internal const int AURL_ENABLEEAURLS = 8;
        internal const int AURL_ENABLEDRIVELETTERS = 16;
        internal const int AURL_DISABLEMIXEDLGC = 32; // Disable mixed Latin Greek Cyrillic IDNs

        internal static Native.CHARFORMATA GetCharFormat(RichTextBox rtb)
        {
            Native.CHARFORMATA lParam = new Native.CHARFORMATA();
            Native.SendMessage(new HandleRef(rtb, rtb.Handle), Native.EM_GETCHARFORMAT, Native.SCF_DEFAULT, lParam);
            return lParam;
        }

        internal static Native.CHARFORMAT2A GetCharFormat2(RichTextBox rtb)
        {
            Native.CHARFORMAT2A lParam = new Native.CHARFORMAT2A() { dwMask = Native.CFM_COLOR };
            Native.SendMessage(new HandleRef(rtb, rtb.Handle), Native.EM_GETCHARFORMAT, Native.SCF_DEFAULT, lParam);
            return lParam;
        }

        internal static void SetRichTextBoxDefaultColor(RichTextBox rtb, Color color)
        {
            Native.CHARFORMATA charFormat = GetCharFormat(rtb);

            charFormat.dwMask = Native.CFM_COLOR;
            charFormat.dwEffects = 0;
            charFormat.crTextColor = ColorTranslator.ToWin32(color);
            Native.SendMessage(new HandleRef(rtb, rtb.Handle), Native.EM_SETCHARFORMAT, Native.SCF_DEFAULT, charFormat);
        }
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

        [DllImport("user32.dll")]
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        internal static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        #endregion

        #region Mouse

        // NC prefix means the mouse was over a non-client area

        internal const int WM_NCMOUSELEAVE = 0x02A2;
        internal const int WM_MOUSEHOVER = 0x02A1;
        internal const int WM_NCMOUSEHOVER = 0x02A0;

        #endregion

        #region Scroll bars

        public const int HTHSCROLL = 6;
        public const int HTVSCROLL = 7;

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

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

        public const int STATE_SYSTEM_INVISIBLE = 0x00008000;
        public const int STATE_SYSTEM_UNAVAILABLE = 0x00000001;

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        public static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref SCROLLBARINFO psbi);

        #endregion

        internal const int DTM_FIRST = 0x1000;
        internal const int DTM_GETDATETIMEPICKERINFO = 0x100E;

        internal const int STATE_SYSTEM_HOTTRACKED = 0x00000080;
        internal const int STATE_SYSTEM_PRESSED = 0x00000008;

        [StructLayout(LayoutKind.Sequential)]
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

        [DllImport("uxtheme", ExactSpelling = true)]
        internal static extern IntPtr GetWindowTheme(IntPtr hWnd);

        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        internal static extern IntPtr OpenThemeData(IntPtr hWnd, String classList);
    }
}

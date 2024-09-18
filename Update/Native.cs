using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace Update;

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "CommentTypo")]
[SuppressMessage("ReSharper", "StringLiteralTypo")]
internal static class Native
{
    internal const int WM_NCPAINT = 0x0085;
    internal const int WM_PAINT = 0x000F;
    internal const int WM_ERASEBKGND = 0x0014;
    internal const int WM_MOVE = 0x0003;
    internal const int WM_SIZE = 0x0005;
    internal const int WM_WINDOWPOSCHANGED = 0x0047;

    [StructLayout(LayoutKind.Sequential)]
    private sealed class POINT
    {
        public int x;
        public int y;
    }

    #region Cursor

    private static readonly HandleRef NullHandleRef = new(null, IntPtr.Zero);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos([In, Out] POINT pt);

    [DllImport("user32.dll")]
    private static extern int MapWindowPoints(
        HandleRef hWndFrom,
        HandleRef hWndTo,
        [In, Out] POINT pt,
        int cPoints);

    // Since we know the UI will only ever run on one thread, we can just have one global POINT class and
    // just let anyone populate and use it randomly whenever. It can never be accessed by two things at once
    // because everyone is on one thread.
    private static readonly POINT _globalNativePoint = new();

    private static Point GetCursorPosition_Fast()
    {
        GetCursorPos(_globalNativePoint);
        return new Point(_globalNativePoint.x, _globalNativePoint.y);
    }

    private static Point PointToClient_Fast(this Control control, Point p)
    {
        _globalNativePoint.x = p.X;
        _globalNativePoint.y = p.Y;
        MapWindowPoints(NullHandleRef, new HandleRef(control, control.Handle), _globalNativePoint, 1);
        return new Point(_globalNativePoint.x, _globalNativePoint.y);
    }

    public static Point ClientCursorPos(this Control c) => c.PointToClient_Fast(GetCursorPosition_Fast());

    #endregion

    #region SendMessage/PostMessage

    [DllImport("user32.dll")]
    internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    #endregion

    #region Control-specific

    #region MessageBox/TaskDialog

    internal enum SHSTOCKICONID : uint
    {
        SIID_HELP = 23,
        SIID_WARNING = 78,
        SIID_INFO = 79,
        SIID_ERROR = 80,
    }

    internal const uint SHGSI_ICON = 0x000000100;

    [PublicAPI]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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
    internal static extern int SHGetStockIconInfo(SHSTOCKICONID siid, uint uFlags, ref SHSTOCKICONINFO psii);

    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool DestroyIcon(IntPtr hIcon);

    #endregion

    #endregion

    #region Device context

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [StructLayout(LayoutKind.Auto)]
    public readonly ref struct GraphicsContext
    {
        private readonly IntPtr _hWnd;
        private readonly IntPtr _dc;
        public readonly Graphics G;

        public GraphicsContext(IntPtr hWnd)
        {
            _hWnd = hWnd;
            _dc = GetWindowDC(_hWnd);
            G = Graphics.FromHdc(_dc);
        }

        public void Dispose()
        {
            G.Dispose();
            ReleaseDC(_hWnd, _dc);
        }
    }

    #endregion

    #region Theming

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    internal static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);

    // Ridiculous Windows using a different value on different versions...
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll")]
    internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    #endregion
}

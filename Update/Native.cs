﻿using System;
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

    #region Cursor

    public static Point ClientCursorPos(this Control c) => c.PointToClient(Cursor.Position);

    #endregion

    #region SendMessageW/PostMessageW

    [DllImport("user32.dll", ExactSpelling = true)]
    internal static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

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

    private const int MAX_PATH = 260;

    [PublicAPI]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal unsafe struct SHSTOCKICONINFO
    {
        internal uint cbSize;
        internal IntPtr hIcon;
        internal int iSysIconIndex;
        internal int iIcon;
        internal fixed char szPath[MAX_PATH];
    }

    [DllImport("Shell32.dll", ExactSpelling = true, SetLastError = false)]
    internal static extern int SHGetStockIconInfo(SHSTOCKICONID siid, uint uFlags, ref SHSTOCKICONINFO psii);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool DestroyIcon(IntPtr hIcon);

    #endregion

    #endregion

    #region Device context

    [DllImport("user32.dll", ExactSpelling = true)]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll", ExactSpelling = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
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

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    internal static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);

    // Ridiculous Windows using a different value on different versions...
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE_OLD = 19;
    internal const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll", ExactSpelling = true)]
    internal static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    #endregion
}

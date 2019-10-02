using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static AngelLoader.WinAPI.InteropMisc;

namespace AngelLoader.WinAPI
{
    internal enum MouseButtonState
    {
        None,
        Down,
        Up
    }

    internal class MouseHookEventArgs : MouseEventArgs
    {
        public MouseHookEventArgs(MouseButtons button, int clicks, int x, int y, int delta, uint timestamp,
            MouseButtonState mouseButtonState)
            : base(button, clicks, x, y, delta)
        {
            MouseButtonState = mouseButtonState;
            Timestamp = timestamp;
        }

        internal MouseButtonState MouseButtonState;
        internal uint Timestamp;
        internal bool Handled;
    }

    public static class MouseHook
    {
        #region Windows structure definitions

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            internal long x;
            internal long y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            internal POINT pt;
            internal uint mouseData;
            internal uint flags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }

        #endregion

        #region Windows function imports

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hmod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(IntPtr hhk, int nCode, int wParam, IntPtr lParam);

        #endregion

        public static event MouseEventHandler OnMouseMessage;

        private static IntPtr _mouseHook;
        private delegate int LowLevelMouseProc(int nCode, int wParam, IntPtr lParam);
        private static LowLevelMouseProc MouseHookProcedure;

        public static void Start()
        {
            if (_mouseHook != IntPtr.Zero) return;

            MouseHookProcedure = MouseHookCallback;
            _mouseHook = SetWindowsHookEx(WH_MOUSE, MouseHookProcedure, IntPtr.Zero, GetCurrentThreadId());

            if (_mouseHook == IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHook);
                _mouseHook = IntPtr.Zero;
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public static void Stop()
        {
            if (_mouseHook == IntPtr.Zero) return;

            bool success = UnhookWindowsHookEx(_mouseHook);
            _mouseHook = IntPtr.Zero;

            if (!success) throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private static int MouseHookCallback(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode > -1 && OnMouseMessage != null)
            {
                var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

                var button =
                    wParam == WM_LBUTTONDOWN ? MouseButtons.Left :
                    wParam == WM_MBUTTONDOWN ? MouseButtons.Middle :
                    wParam == WM_RBUTTONDOWN ? MouseButtons.Right :
                    MouseButtons.None;

                int mouseDelta = wParam == WM_MOUSEWHEEL ? (int)((hookStruct.mouseData >> 16) & 0xffff) : 0;

                int clicks =
                    button == MouseButtons.None ? 0 :
                    wParam == WM_LBUTTONDBLCLK || wParam == WM_RBUTTONDBLCLK || wParam == WM_MBUTTONDBLCLK ? 2 :
                    1;

                var e = new MouseEventArgs(button, clicks, (int)hookStruct.pt.x, (int)hookStruct.pt.y, mouseDelta);
                OnMouseMessage(null, e);

                if (button == MouseButtons.Left)
                {
                    return -1;
                }
            }

            return CallNextHookEx(_mouseHook, nCode, wParam, lParam);
        }
    }
}

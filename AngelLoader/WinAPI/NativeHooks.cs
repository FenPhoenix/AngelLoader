using System;
using System.Drawing;
using System.Runtime.InteropServices;
using AngelLoader.DataClasses;
using AngelLoader.Forms.CustomControls;
using EasyHook;

namespace AngelLoader.WinAPI
{
    internal static class NativeHooks
    {
        #region Private fields

        private const int COLOR_HIGHLIGHT = 13;
        private const int COLOR_HIGHLIGHT_TEXT = 14;
        private const int COLOR_WINDOW = 5;
        private const int COLOR_WINDOWTEXT = 8;

        #region GetSysColor

        private static LocalHook? _getSysColorHook;

        private static GetSysColorDelegate? GetSysColorOriginal;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int GetSysColorDelegate(int nIndex);

        #endregion

        #region GetSysColorBrush

        private static LocalHook? _getSysColorBrushHook;

        private static GetSysColorBrushDelegate? GetSysColorBrushOriginal;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate IntPtr GetSysColorBrushDelegate(int nIndex);

        #endregion

        #endregion

        private static KnownColor SysColorToKnownColor(int systemColorIndex)
        {
            // OLE colors are like 0x800000xx where xx is the system color index
            return ColorTranslator.FromOle((int)unchecked(systemColorIndex + 0x80000000)).ToKnownColor();
        }

        internal static void InstallHooks()
        {
            try
            {
                (_getSysColorHook, GetSysColorOriginal) = InstallHook<GetSysColorDelegate>(
                    "user32.dll",
                    "GetSysColor",
                    GetSysColor);

                (_getSysColorBrushHook, GetSysColorBrushOriginal) = InstallHook<GetSysColorBrushDelegate>(
                    "user32.dll",
                    "GetSysColorBrush",
                    GetSysColorBrush);
            }
            catch
            {
                // If we fail, oh well, just keep the classic-mode colors then... better than nothing
                _getSysColorHook?.Dispose();
                _getSysColorBrushHook?.Dispose();
            }
        }

        private static (LocalHook Hook, TDelegate OriginalMethod)
        InstallHook<TDelegate>(string dll, string method, TDelegate hookDelegate) where TDelegate : Delegate
        {
            TDelegate originalMethod;
            LocalHook? hook = null;

            try
            {
                IntPtr address = LocalHook.GetProcAddress(dll, method);
                originalMethod = Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
                hook = LocalHook.Create(address, hookDelegate, null);
                hook.ThreadACL.SetInclusiveACL(new[] { 0 });
            }
            catch
            {
                hook?.Dispose();
                throw;
            }

            return (hook, originalMethod);
        }

        #region Hooked method overrides

        private static int GetSysColor(int nIndex)
        {
            return Misc.Config.VisualTheme == VisualTheme.Dark
                ? nIndex switch
                {
                    COLOR_WINDOW => ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground),
                    COLOR_WINDOWTEXT => ColorTranslator.ToWin32(DarkColors.LightText),
                    COLOR_HIGHLIGHT => ColorTranslator.ToWin32(DarkColors.BlueSelection),
                    COLOR_HIGHLIGHT_TEXT => ColorTranslator.ToWin32(DarkColors.LightText),
                    _ => GetSysColorOriginal!(nIndex)
                }
                : GetSysColorOriginal!(nIndex);
        }

        private static IntPtr GetSysColorBrush(int nIndex)
        {
            if (Misc.Config.VisualTheme == VisualTheme.Dark)
            {
                Color? color = nIndex switch
                {
                    COLOR_WINDOW => DarkColors.Fen_ControlBackground,
                    COLOR_WINDOWTEXT => DarkColors.LightText,
                    COLOR_HIGHLIGHT => DarkColors.BlueSelection,
                    COLOR_HIGHLIGHT_TEXT => DarkColors.LightText,
                    _ => null
                };

                return color != null
                    ? Native.CreateSolidBrush(ColorTranslator.ToWin32((Color)color))
                    : GetSysColorBrushOriginal!(nIndex);
            }
            else
            {
                return GetSysColorBrushOriginal!(nIndex);
            }
        }

        #endregion
    }
}

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using AngelLoader.DataClasses;
using AngelLoader.Forms;
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
        private const int COLOR_3DFACE = 15;
        private const int COLOR_GRAYTEXT = 17;

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

        #region DrawThemeBackground

        private static LocalHook? _drawThemeBackgroundHook;

        private static DrawThemeBackgroundDelegate? DrawThemeBackgroundOriginal;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int DrawThemeBackgroundDelegate(
            IntPtr hTheme,
            IntPtr hdc,
            int partId,
            int stateId,
            ref Native.RECT pRect,
            ref Native.RECT pClipRect);

        #endregion

        #region GetThemeColor

        private static LocalHook? _getThemeColorHook;

        private static GetThemeColorDelegate? GetThemeColorOriginal;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int GetThemeColorDelegate(
            IntPtr hTheme,
            int iPartId,
            int iStateId,
            int iPropId,
            out int pColor);

        #endregion

        private static bool _hooksInstalled;

        // We set/unset this while painting specific controls, so other controls aren't affected by the global
        // color change
        internal static bool EnableSysColorOverride = false;

        #endregion

        internal static void InstallHooks()
        {
            if (_hooksInstalled) return;

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
            try
            {
                (_drawThemeBackgroundHook, DrawThemeBackgroundOriginal) = InstallHook<DrawThemeBackgroundDelegate>(
                    "uxtheme.dll",
                    "DrawThemeBackground",
                    DrawThemeBackgroundHook);

                (_getThemeColorHook, GetThemeColorOriginal) = InstallHook<GetThemeColorDelegate>(
                    "uxtheme.dll",
                    "GetThemeColor",
                    GetThemeColorHook);

                ReloadTheme();
            }
            catch
            {
                _drawThemeBackgroundHook?.Dispose();
                _getThemeColorHook?.Dispose();
            }

            _hooksInstalled = true;
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

        internal static void ReloadTheme()
        {
            if (!Native.IsThemeActive()) return;
            ScrollBarPainter.Reload();
            ToolTipRenderer.Reload();
        }

        #region Hooked method overrides

        private static int DrawThemeBackgroundHook(
            IntPtr hTheme,
            IntPtr hdc,
            int partId,
            int stateId,
            ref Native.RECT pRect,
            ref Native.RECT pClipRect)
        {
            const int success = 0;

            if (Misc.Config.DarkMode)
            {
                if (ScrollBarPainter.HTheme == hTheme)
                {
                    return ScrollBarPainter.Paint(hdc, partId, stateId, pRect)
                        ? success
                        : DrawThemeBackgroundOriginal!(hTheme, hdc, partId, stateId, ref pRect, ref pClipRect);
                }
                else if (ControlUtils.ToolTipsReflectable && ToolTipRenderer.HTheme == hTheme)
                {
                    return ToolTipRenderer.Paint(hdc, partId, pRect)
                        ? success
                        : DrawThemeBackgroundOriginal!(hTheme, hdc, partId, stateId, ref pRect, ref pClipRect);
                }
                else
                {
                    return DrawThemeBackgroundOriginal!(hTheme, hdc, partId, stateId, ref pRect, ref pClipRect);
                }
            }
            else
            {
                return DrawThemeBackgroundOriginal!(hTheme, hdc, partId, stateId, ref pRect, ref pClipRect);
            }
        }

        private static int GetThemeColorHook(
            IntPtr hTheme,
            int iPartId,
            int iStateId,
            int iPropId,
            out int pColor)
        {
            const int success = 0;

            if (Misc.Config.DarkMode)
            {
                if (ScrollBarPainter.HTheme == hTheme)
                {
                    return ScrollBarPainter.TryGetThemeColor(iPartId, iPropId, out pColor)
                        ? success
                        : GetThemeColorOriginal!(hTheme, iPartId, iStateId, iPropId, out pColor);
                }
                else if (ControlUtils.ToolTipsReflectable && ToolTipRenderer.HTheme == hTheme)
                {
                    return ToolTipRenderer.TryGetThemeColor(iPropId, out pColor)
                        ? success
                        : GetThemeColorOriginal!(hTheme, iPartId, iStateId, iPropId, out pColor);
                }
                else
                {
                    return GetThemeColorOriginal!(hTheme, iPartId, iStateId, iPropId, out pColor);
                }
            }
            else
            {
                return GetThemeColorOriginal!(hTheme, iPartId, iStateId, iPropId, out pColor);
            }
        }

        private static int GetSysColor(int nIndex)
        {
            return Misc.Config.DarkMode && EnableSysColorOverride
                ? nIndex switch
                {
                    COLOR_WINDOW => ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground),
                    COLOR_WINDOWTEXT => ColorTranslator.ToWin32(DarkColors.LightText),
                    COLOR_HIGHLIGHT => ColorTranslator.ToWin32(DarkColors.BlueSelection),
                    COLOR_HIGHLIGHT_TEXT => ColorTranslator.ToWin32(DarkColors.Fen_HighlightText),
                    COLOR_3DFACE => ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground),
                    COLOR_GRAYTEXT => ColorTranslator.ToWin32(DarkColors.DisabledText),
                    _ => GetSysColorOriginal!(nIndex)
                }
                : GetSysColorOriginal!(nIndex);
        }

        private static IntPtr GetSysColorBrush(int nIndex)
        {
            return Misc.Config.DarkMode && EnableSysColorOverride
                ? nIndex switch
                {
                    COLOR_WINDOW => Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground)),
                    COLOR_WINDOWTEXT => Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightText)),
                    COLOR_HIGHLIGHT => Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.BlueSelection)),
                    COLOR_HIGHLIGHT_TEXT => Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_HighlightText)),
                    COLOR_3DFACE => Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground)),
                    COLOR_GRAYTEXT => Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DisabledText)),
                    _ => GetSysColorBrushOriginal!(nIndex)
                }
                : GetSysColorBrushOriginal!(nIndex);
        }

        #endregion
    }
}

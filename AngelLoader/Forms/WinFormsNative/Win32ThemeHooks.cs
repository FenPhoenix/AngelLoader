using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.ThemeRenderers;
using EasyHook;
using ScrollBarRenderer = AngelLoader.Forms.ThemeRenderers.ScrollBarRenderer;

namespace AngelLoader.Forms.WinFormsNative;

internal static class Win32ThemeHooks
{
    #region Private fields

    private const int COLOR_HIGHLIGHT = 13;
    private const int COLOR_HIGHLIGHT_TEXT = 14;
    private const int COLOR_WINDOW = 5;
    private const int COLOR_WINDOWTEXT = 8;
    private const int COLOR_3DFACE = 15;
    private const int COLOR_GRAYTEXT = 17;

    private static readonly Dictionary<IntPtr, ThemeRenderer> _themeRenderers = new();

    #region GetSysColor

    private static LocalHook? _getSysColorHook;

    private static GetSysColorDelegate? GetSysColor_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int GetSysColorDelegate(int nIndex);

    #endregion

    #region GetSysColorBrush

    private static LocalHook? _getSysColorBrushHook;

    private static GetSysColorBrushDelegate? GetSysColorBrush_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate IntPtr GetSysColorBrushDelegate(int nIndex);

    #endregion

    #region DrawThemeBackground

    private static LocalHook? _drawThemeBackgroundHook;

    private static DrawThemeBackgroundDelegate? DrawThemeBackground_Original;

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

    private static GetThemeColorDelegate? GetThemeColor_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int GetThemeColorDelegate(
        IntPtr hTheme,
        int iPartId,
        int iStateId,
        int iPropId,
        out int pColor);

    #endregion

    private static bool _hooksInstalled;

    private static bool _disableHookedTheming;

    internal enum Override
    {
        None,
        Full,
        RichText
    }

    // We set/unset this while painting specific controls, so other controls aren't affected by the global
    // color change
    private static Override SysColorOverride = Override.None;

    #endregion

    private static readonly IntPtr SysColorBrush_LightBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightBackground));
    private static readonly IntPtr SysColorBrush_LightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightText));
    private static readonly IntPtr SysColorBrush_BlueSelection = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.BlueSelection));
    private static readonly IntPtr SysColorBrush_Fen_HighlightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_HighlightText));
    private static readonly IntPtr SysColorBrush_Fen_ControlBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground));
    private static readonly IntPtr SysColorBrush_DisabledText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DisabledText));
    private static readonly IntPtr SysColorBrush_Fen_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground));
    private static readonly IntPtr SysColorBrush_Fen_DarkForeground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkForeground));
    private static readonly IntPtr SysColorBrush_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DarkBackground));

    internal readonly ref struct OverrideSysColorScope
    {
        public OverrideSysColorScope(Override @override) => SysColorOverride = @override;

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void Dispose() => SysColorOverride = Override.None;
    }

    internal static void InstallHooks()
    {
        if (_hooksInstalled) return;

        var sbr = new ScrollBarRenderer();
        var ttr = new ToolTipRenderer();
        var tvr = new TreeViewRenderer();
        _themeRenderers[sbr.HTheme] = sbr;
        _themeRenderers[ttr.HTheme] = ttr;
        _themeRenderers[tvr.HTheme] = tvr;

        try
        {
            (_getSysColorHook, GetSysColor_Original) = InstallHook<GetSysColorDelegate>(
                "user32.dll",
                "GetSysColor",
                GetSysColor_Hooked);

            // @x64 (GetSysColorBrush hook):
            // Fails with 'STATUS_NOT_SUPPORTED: Hooking near conditional jumps is not supported. (Code: 487)'
            // on x64. Sigh... Fortunately, this thing seems to only be used for the Win7 non-Aero scroll bar
            // corners...? I don't see any difference with this disabled on Win10. So... not the worst thing...
            if (!Environment.Is64BitProcess)
            {
                (_getSysColorBrushHook, GetSysColorBrush_Original) = InstallHook<GetSysColorBrushDelegate>(
                    "user32.dll",
                    "GetSysColorBrush",
                    GetSysColorBrush_Hooked);
            }

            (_drawThemeBackgroundHook, DrawThemeBackground_Original) = InstallHook<DrawThemeBackgroundDelegate>(
                "uxtheme.dll",
                "DrawThemeBackground",
                DrawThemeBackground_Hooked);

            (_getThemeColorHook, GetThemeColor_Original) = InstallHook<GetThemeColorDelegate>(
                "uxtheme.dll",
                "GetThemeColor",
                GetThemeColor_Hooked);
        }
        catch
        {
            // If we fail, oh well, just keep the classic-mode colors then... better than nothing
            _getSysColorHook?.Dispose();
            _getSysColorBrushHook?.Dispose();
            _drawThemeBackgroundHook?.Dispose();
            _getThemeColorHook?.Dispose();
        }
        finally
        {
            _hooksInstalled = true;
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

    internal static void ReloadTheme()
    {
        if (!_hooksInstalled || !Native.IsThemeActive()) return;

        // We have to re-add the HTheme keys because they may/will(?) have changed

        var tempRenderers = new List<ThemeRenderer>(_themeRenderers.Count);

        foreach (ThemeRenderer renderer in _themeRenderers.Values)
        {
            tempRenderers.Add(renderer);
        }

        _themeRenderers.Clear();

        foreach (ThemeRenderer renderer in tempRenderers)
        {
            renderer.Reload();
            _themeRenderers[renderer.HTheme] = renderer;
        }
    }

    /*
    Hooked themes leak into unthemed dialogs (open file, browse folder etc.), and we can't tell them not to,
    because we can only exclude by thread, and we already know how impossible/focus-fucked threaded dialogs
    are, so we're not even going to go there. The best we can do is to turn off hooked theming _while_ a dialog
    is up. Normally this looks fine, but if our app gets minimized (or Show-Desktop'd) and restored, then the
    hooked themes will be disabled throughout the app (ie. we'll get unthemed scroll bars). This is a fairly
    unlikely scenario though, and even if it happens we refresh the whole app on dialog close anyway so at
    least it's temporary.
    */
    internal readonly ref struct DialogScope
    {
        private readonly bool _active;

        internal DialogScope(bool active = true)
        {
            _active = active;
            if (_active)
            {
                _disableHookedTheming = true;
                ControlUtils.RecreateAllToolTipHandles();
            }
        }

        public void Dispose()
        {
            if (_active)
            {
                _disableHookedTheming = false;
                // Do this AFTER re-enabling hooked theming, otherwise it doesn't take and we end up with
                // dark-on-dark tooltips
                ControlUtils.RecreateAllToolTipHandles();
                List<IntPtr> handles = Native.GetProcessWindowHandles();
                foreach (IntPtr handle in handles)
                {
                    Control? control = Control.FromHandle(handle);
                    if (control is Form form) form.Refresh();
                }
            }
        }
    }

    #region Hooked method overrides

    private static int DrawThemeBackground_Hooked(
        IntPtr hTheme,
        IntPtr hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect,
        ref Native.RECT pClipRect)
    {
        const int success = 0;

        return !_disableHookedTheming &&
               _themeRenderers.TryGetValue(hTheme, out ThemeRenderer renderer) &&
               renderer.Enabled &&
               renderer.TryDrawThemeBackground(hTheme, hdc, iPartId, iStateId, ref pRect, ref pClipRect)
            ? success
            : DrawThemeBackground_Original!(hTheme, hdc, iPartId, iStateId, ref pRect, ref pClipRect);
    }

    private static int GetThemeColor_Hooked(
        IntPtr hTheme,
        int iPartId,
        int iStateId,
        int iPropId,
        out int pColor)
    {
        const int success = 0;

        return !_disableHookedTheming &&
               _themeRenderers.TryGetValue(hTheme, out ThemeRenderer renderer) &&
               renderer.Enabled &&
               renderer.TryGetThemeColor(hTheme, iPartId, iStateId, iPropId, out pColor)
            ? success
            : GetThemeColor_Original!(hTheme, iPartId, iStateId, iPropId, out pColor);
    }

    private static int GetSysColor_Hooked(int nIndex)
    {
        if (!_disableHookedTheming && Global.Config.DarkMode)
        {
            return SysColorOverride switch
            {
                Override.Full => nIndex switch
                {
                    COLOR_WINDOW => ColorTranslator.ToWin32(DarkColors.LightBackground),
                    COLOR_WINDOWTEXT => ColorTranslator.ToWin32(DarkColors.LightText),
                    COLOR_HIGHLIGHT => ColorTranslator.ToWin32(DarkColors.BlueSelection),
                    COLOR_HIGHLIGHT_TEXT => ColorTranslator.ToWin32(DarkColors.Fen_HighlightText),
                    COLOR_3DFACE => ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground),
                    COLOR_GRAYTEXT => ColorTranslator.ToWin32(DarkColors.DisabledText),
                    _ => GetSysColor_Original!(nIndex)
                },
                Override.RichText => nIndex switch
                {
                    // Darker background, more desaturated foreground color
                    COLOR_WINDOW => ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground),
                    COLOR_WINDOWTEXT => ColorTranslator.ToWin32(DarkColors.Fen_DarkForeground),
                    COLOR_HIGHLIGHT => ColorTranslator.ToWin32(DarkColors.BlueSelection),
                    COLOR_HIGHLIGHT_TEXT => ColorTranslator.ToWin32(DarkColors.Fen_HighlightText),
                    _ => GetSysColor_Original!(nIndex)
                },
                // This is for scrollbar vert/horz corners on Win7 (and maybe Win8? Haven't tested it).
                // This is the ONLY way that works on those versions.
                // (note: it's really just GetSysColorBrush() that it calls, we technically don't need to do
                // this here in GetSysColor(), but let's do it for robustness because who knows what could change.)
                _ => nIndex == COLOR_3DFACE
                    ? ColorTranslator.ToWin32(DarkColors.DarkBackground)
                    : GetSysColor_Original!(nIndex)
            };
        }
        else
        {
            return GetSysColor_Original!(nIndex);
        }
    }

    private static IntPtr GetSysColorBrush_Hooked(int nIndex)
    {
        if (!_disableHookedTheming && Global.Config.DarkMode)
        {
            return SysColorOverride switch
            {
                Override.Full => nIndex switch
                {
                    COLOR_WINDOW => SysColorBrush_LightBackground,
                    COLOR_WINDOWTEXT => SysColorBrush_LightText,
                    COLOR_HIGHLIGHT => SysColorBrush_BlueSelection,
                    COLOR_HIGHLIGHT_TEXT => SysColorBrush_Fen_HighlightText,
                    COLOR_3DFACE => SysColorBrush_Fen_ControlBackground,
                    COLOR_GRAYTEXT => SysColorBrush_DisabledText,
                    _ => GetSysColorBrush_Original!(nIndex)
                },
                Override.RichText => nIndex switch
                {
                    // Darker background, more desaturated foreground color
                    COLOR_WINDOW => SysColorBrush_Fen_DarkBackground,
                    COLOR_WINDOWTEXT => SysColorBrush_Fen_DarkForeground,
                    COLOR_HIGHLIGHT => SysColorBrush_BlueSelection,
                    COLOR_HIGHLIGHT_TEXT => SysColorBrush_Fen_HighlightText,
                    _ => GetSysColorBrush_Original!(nIndex)
                },
                // This is for scrollbar vert/horz corners on Win7 (and maybe Win8? Haven't tested it).
                // This is the ONLY way that works on those versions.
                _ => nIndex == COLOR_3DFACE
                    ? SysColorBrush_DarkBackground
                    : GetSysColorBrush_Original!(nIndex)
            };
        }
        else
        {
            return GetSysColorBrush_Original!(nIndex);
        }
    }

    #endregion
}
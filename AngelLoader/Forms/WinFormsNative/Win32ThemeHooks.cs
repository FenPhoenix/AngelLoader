﻿/*
Here's the deal with this thing:

-On Framework x86, we need EasyHook.
-On Framework x64, we can use CoreHook (and do).
-On .NET modern, we need CoreHook.
-On .NET modern, they've added the [SuppressGCTransition] attribute to the internal GetSysColor() extern function,
 which improves performance but causes an ExecutionEngineException when hooked. Even when we use our own defined
 p/invoke for it without the attribute, it still happens somehow. I don't know how this kind of thing works so
 whatever. Therefore, on .NET modern, we include our own custom version of System.Drawing.Primitives.dll without
 the [SuppressGCTransition] attribute on GetSysColor().
*/

// @Wine: 2025/07/11, tested all hooks work in 32-bit Wine

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
#if X64
using CoreHook;
#else
using EasyHook;
#endif

namespace AngelLoader.Forms.WinFormsNative;

internal static class Win32ThemeHooks
{
    #region Hooks

    private enum HookType
    {
        GetSysColor,
#if !X64
        GetSysColorBrush,
#endif
        DrawThemeBackground,
        GetThemeColor,
        PatBlt,
        FillRect,
        DrawTextW,
        Length,
    }

    private readonly struct HookData
    {
        internal readonly string Dll;
        internal readonly string Method;
        internal readonly bool WineOnly;

        public HookData(string dll, string method, bool wineOnly)
        {
            Dll = dll;
            Method = method;
            WineOnly = wineOnly;
        }
    }

    #region Hooks and delegates

    #region GetSysColor

    private static LocalHook? _getSysColorHook;

    private static GetSysColorDelegate? GetSysColor_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int GetSysColorDelegate(int nIndex);

    #endregion

    #region GetSysColorBrush

#if !X64

    private static LocalHook? _getSysColorBrushHook;

    private static GetSysColorBrushDelegate? GetSysColorBrush_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate nint GetSysColorBrushDelegate(int nIndex);

#endif

    #endregion

    #region DrawThemeBackground

    private static LocalHook? _drawThemeBackgroundHook;

    private static DrawThemeBackgroundDelegate? DrawThemeBackground_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int DrawThemeBackgroundDelegate(
        nint hTheme,
        nint hdc,
        int partId,
        int stateId,
        ref Native.RECT pRect,
        nint pClipRect);

    #endregion

    #region GetThemeColor

    private static LocalHook? _getThemeColorHook;

    private static GetThemeColorDelegate? GetThemeColor_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int GetThemeColorDelegate(
        nint hTheme,
        int iPartId,
        int iStateId,
        int iPropId,
        out int pColor);

    #endregion

    #region PatBlt

    private static LocalHook? _patBltHook;

    private static PatBltDelegate? PatBlt_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private delegate bool PatBltDelegate(
        nint hdc,
        int x,
        int y,
        int w,
        int h,
        int rop);

    #endregion

    #region FillRect

    private static LocalHook? _fillRectHook;

    private static FillRectDelegate? FillRect_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int FillRectDelegate(
        nint hdc,
        ref Native.RECT lprc,
        nint hbr);

    #endregion

    #region DrawTextW

    private static LocalHook? _drawTextWHook;

    private static DrawTextWDelegate? DrawTextW_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int DrawTextWDelegate(
        nint hDC,
        [MarshalAs(UnmanagedType.LPWStr)]
        string lpszString,
        int nCount,
        ref Native.RECT lpRect,
        int nFormat);

    #endregion

    #endregion

    // ReSharper disable once RedundantExplicitArraySize
    private static readonly HookData[] _hookData = new HookData[(int)HookType.Length]
    {
        new("user32.dll", "GetSysColor", wineOnly: false),
#if !X64
        new("user32.dll", "GetSysColorBrush", wineOnly: false),
#endif
        new("uxtheme.dll", "DrawThemeBackground", wineOnly: false),
        new("uxtheme.dll", "GetThemeColor", wineOnly: false),
        new("gdi32.dll", "PatBlt", wineOnly: true),
        new("user32.dll", "FillRect", wineOnly: true),
        new("user32.dll", "DrawTextW", wineOnly: true),
    };

    private static HookData GetHookData(HookType hookType) => _hookData[(int)hookType];

    private static readonly nint[] _procAddresses = new nint[(int)HookType.Length];

    #region Hook installing

    private static bool _hooksInstalled;
    private static bool _hooksPreloaded;

    internal static void InstallHooks()
    {
        // UI doesn't work with this crap
#if DEBUG
        return;
#endif

        if (_hooksInstalled) return;

        PreloadHooks();

        try
        {
            (_getSysColorHook, GetSysColor_Original) = InstallHook<GetSysColorDelegate>(
                HookType.GetSysColor,
                GetSysColor_Hooked);

            // @x64 (GetSysColorBrush hook):
            // Fails with 'STATUS_NOT_SUPPORTED: Hooking near conditional jumps is not supported. (Code: 487)'
            // on x64. Sigh... Fortunately, this thing seems to only be used for the Win7 non-Aero scroll bar
            // corners...? I don't see any difference with this disabled on Win10. So... not the worst thing...
            // 2023-11-11: This seems to work with CoreHook.
#if !X64
            (_getSysColorBrushHook, GetSysColorBrush_Original) = InstallHook<GetSysColorBrushDelegate>(
                HookType.GetSysColorBrush,
                GetSysColorBrush_Hooked);
#endif

            (_drawThemeBackgroundHook, DrawThemeBackground_Original) = InstallHook<DrawThemeBackgroundDelegate>(
                HookType.DrawThemeBackground,
                DrawThemeBackground_Hooked);

            (_getThemeColorHook, GetThemeColor_Original) = InstallHook<GetThemeColorDelegate>(
                HookType.GetThemeColor,
                GetThemeColor_Hooked);

            if (WinVersion.IsWine)
            {
                (_patBltHook, PatBlt_Original) = InstallHook<PatBltDelegate>(
                    HookType.PatBlt,
                    PatBlt_Hooked);

                (_fillRectHook, FillRect_Original) = InstallHook<FillRectDelegate>(
                    HookType.FillRect,
                    FillRect_Hooked);

                (_drawTextWHook, DrawTextW_Original) = InstallHook<DrawTextWDelegate>(
                    HookType.DrawTextW,
                    DrawTextW_Hooked);
            }
        }
        catch
        {
            // If we fail, oh well, just keep the classic-mode colors then... better than nothing
            _getSysColorHook?.Dispose();
#if !X64
            _getSysColorBrushHook?.Dispose();
#endif
            _drawThemeBackgroundHook?.Dispose();
            _getThemeColorHook?.Dispose();
            if (WinVersion.IsWine)
            {
                _patBltHook?.Dispose();
                _fillRectHook?.Dispose();
                _drawTextWHook?.Dispose();
            }
        }
        finally
        {
            _hooksInstalled = true;
        }
    }

    /*
    @PERF_TODO(PreloadHooks): This overlap can sometimes just barely fit in the overlap window, when on a clean
    install with no real data to load. We could overlap it with the main form component init and ctor, which is
    currently not done because the InstallHooks() call is in DarkFormBase, and DarkFormBase's ctor runs before
    its derived types (MainForm in this case). We put the call in there for robustness (for example if a dialog
    needs to come up before any other form is loaded, the dialog will trigger the hook install), so we don't
    really want to remove it if we can help it. We could make some disgusting hack to make it not run if our type
    is MainForm or something.
    */
    private static readonly object _preloadHooksLock = new();
    public static void PreloadHooks()
    {
        lock (_preloadHooksLock)
        {
            if (_hooksPreloaded) return;

            ReloadHThemes();

            for (int i = 0; i < (int)HookType.Length; i++)
            {
                HookData hookData = GetHookData((HookType)i);
                if (WinVersion.IsWine || !hookData.WineOnly)
                {
                    _procAddresses[i] = GetProcAddressForHookData(hookData);
                }
            }

            _hooksPreloaded = true;

            return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static IntPtr GetProcAddressForHookData(HookData hookData) => LocalHook.GetProcAddress(hookData.Dll, hookData.Method);
        }
    }

    private static (LocalHook Hook, TDelegate OriginalMethod)
    InstallHook<TDelegate>(HookType hookType, TDelegate hookDelegate) where TDelegate : Delegate
    {
        TDelegate originalMethod;
        LocalHook? hook = null;

        try
        {
            nint address = _procAddresses[(int)hookType];
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

    #endregion

    #region Hooked method overrides

    private static int DrawThemeBackground_Hooked(
        nint hTheme,
        nint hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect,
        nint pClipRect)
    {
        const int success = 0;
        bool succeeded = false;

        if (!_disableHookedTheming)
        {
            if (hTheme == _hThemes[(int)RenderedControl.ScrollBar] && ScrollBarEnabled())
            {
                succeeded = ScrollBar_TryDrawThemeBackground(hdc, iPartId, iStateId, ref pRect);
            }
            else if (hTheme == _hThemes[(int)RenderedControl.ToolTip] && ToolTipEnabled())
            {
                succeeded = ToolTip_TryDrawThemeBackground(hdc, iPartId, ref pRect);
            }
            else if (hTheme == _hThemes[(int)RenderedControl.TreeView])
            {
                succeeded = TreeView_TryDrawThemeBackground(hdc, iPartId, iStateId, ref pRect);
            }
            else if (hTheme == _hThemes[(int)RenderedControl.TabScrollButtons] && TabScrollButtonsEnabled())
            {
                succeeded = TabScrollButtons_TryDrawThemeBackground(hdc, iPartId, iStateId, ref pRect);
            }
            else if (hTheme == _hThemes[(int)RenderedControl.Trackbar] && TrackBarEnabled())
            {
                succeeded = TrackBar_TryDrawThemeBackground(hdc, iPartId, iStateId, ref pRect);
            }
            else if (hTheme == _hThemes[(int)RenderedControl.Progress] && ProgressBarEnabled())
            {
                succeeded = ProgressBar_TryDrawThemeBackground(hdc, iPartId, iStateId, ref pRect, pClipRect);
            }
        }

        return succeeded
            ? success
            : DrawThemeBackground_Original!(hTheme, hdc, iPartId, iStateId, ref pRect, pClipRect);
    }

    private static int GetThemeColor_Hooked(
        nint hTheme,
        int iPartId,
        int iStateId,
        int iPropId,
        out int pColor)
    {
        const int success = 0;
        bool succeeded = false;

        pColor = 0;

        if (!_disableHookedTheming)
        {
            if (hTheme == _hThemes[(int)RenderedControl.ScrollBar] && ScrollBarEnabled())
            {
                succeeded = ScrollBar_TryGetThemeColor(iPartId, iPropId, out pColor);
            }
            else if (hTheme == _hThemes[(int)RenderedControl.ToolTip] && ToolTipEnabled())
            {
                succeeded = ToolTip_TryGetThemeColor(iPropId, out pColor);
            }
            else if (hTheme == _hThemes[(int)RenderedControl.Trackbar] && TrackBarEnabled())
            {
                succeeded = TrackBar_TryGetThemeColor(iPartId, iStateId, out pColor);
            }
        }

        return succeeded
            ? success
            : GetThemeColor_Original!(hTheme, iPartId, iStateId, iPropId, out pColor);
    }

    private static int GetSysColor_Hooked(int nIndex)
    {
        return !_disableHookedTheming && Global.Config.DarkMode
            ? SysColorOverride switch
            {
                Override.Full => nIndex switch
                {
                    COLOR_WINDOW => ColorTranslator.ToWin32(DarkColors.LightBackground),
                    COLOR_WINDOWTEXT => ColorTranslator.ToWin32(DarkColors.LightText),
                    COLOR_HIGHLIGHT => ColorTranslator.ToWin32(DarkColors.BlueSelection),
                    COLOR_HIGHLIGHT_TEXT => ColorTranslator.ToWin32(DarkColors.Fen_HighlightText),
                    COLOR_3DFACE => ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground),
                    COLOR_GRAYTEXT => ColorTranslator.ToWin32(DarkColors.DisabledText),
                    _ => GetSysColor_Original!(nIndex),
                },
                Override.RichText => nIndex switch
                {
                    // Darker background, more desaturated foreground color
                    COLOR_WINDOW => ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground),
                    COLOR_WINDOWTEXT => ColorTranslator.ToWin32(DarkColors.Fen_DarkForeground),
                    COLOR_HIGHLIGHT => ColorTranslator.ToWin32(DarkColors.BlueSelection),
                    COLOR_HIGHLIGHT_TEXT => ColorTranslator.ToWin32(DarkColors.Fen_HighlightText),
                    _ => GetSysColor_Original!(nIndex),
                },
                // This is for scrollbar vert/horz corners on Win7 (and maybe Win8? Haven't tested it).
                // This is the ONLY way that works on those versions.
                // (note: it's really just GetSysColorBrush() that it calls, we technically don't need to do
                // this here in GetSysColor(), but let's do it for robustness because who knows what could change.)
                _ => nIndex == COLOR_3DFACE
                    ? ColorTranslator.ToWin32(DarkColors.DarkBackground)
                    : GetSysColor_Original!(nIndex),
            }
            : GetSysColor_Original!(nIndex);
    }

#if !X64

    private static nint GetSysColorBrush_Hooked(int nIndex)
    {
        return !_disableHookedTheming && Global.Config.DarkMode
            ? SysColorOverride switch
            {
                Override.Full => nIndex switch
                {
                    COLOR_WINDOW => SysColorBrush_LightBackground,
                    COLOR_WINDOWTEXT => SysColorBrush_LightText,
                    COLOR_HIGHLIGHT => SysColorBrush_BlueSelection,
                    COLOR_HIGHLIGHT_TEXT => SysColorBrush_Fen_HighlightText,
                    COLOR_3DFACE => SysColorBrush_Fen_ControlBackground,
                    COLOR_GRAYTEXT => SysColorBrush_DisabledText,
                    _ => GetSysColorBrush_Original!(nIndex),
                },
                Override.RichText => nIndex switch
                {
                    // Darker background, more desaturated foreground color
                    COLOR_WINDOW => SysColorBrush_Fen_DarkBackground,
                    COLOR_WINDOWTEXT => SysColorBrush_Fen_DarkForeground,
                    COLOR_HIGHLIGHT => SysColorBrush_BlueSelection,
                    COLOR_HIGHLIGHT_TEXT => SysColorBrush_Fen_HighlightText,
                    _ => GetSysColorBrush_Original!(nIndex),
                },
                // This is for scrollbar vert/horz corners on Win7 (and maybe Win8? Haven't tested it).
                // This is the ONLY way that works on those versions.
                _ => nIndex == COLOR_3DFACE
                    ? SysColorBrush_DarkBackground
                    : GetSysColorBrush_Original!(nIndex),
            }
            : GetSysColorBrush_Original!(nIndex);
    }

#endif

    [return: MarshalAs(UnmanagedType.Bool)]
    private static bool PatBlt_Hooked(
        nint hdc,
        int x,
        int y,
        int w,
        int h,
        int rop)
    {
        if (WinVersion.IsWine && !_disableHookedTheming && Global.Config.DarkMode && ScrollBarEnabled())
        {
            nint wnd = Native.WindowFromDC(hdc);
            Control? c = Control.FromHandle(wnd);
            if (c is ScrollBar sb && PointIsOnScrollBarTrackArea(sb, x, y))
            {
                nint currentObject = Native.GetCurrentObject(hdc, Native.OBJ_BRUSH);
                if (currentObject != 0)
                {
                    nint prevObject = Native.SelectObject(hdc, NativeBrush_DarkBackground);

                    try
                    {
                        bool result = PatBlt_Original!(hdc, x, y, w, h, Native.PATCOPY);
                        return result;
                    }
                    finally
                    {
                        Native.SelectObject(hdc, prevObject);
                    }
                }
            }
        }

        return PatBlt_Original!(hdc, x, y, w, h, rop);
    }

    private static bool EqualsPhysically(this Color color1, Color color2) =>
        color1.A == color2.A &&
        color1.R == color2.R &&
        color1.G == color2.G &&
        color1.B == color2.B;

    private static int FillRect_Hooked(
        nint hdc,
        ref Native.RECT lprc,
        nint hbr)
    {
        if (WinVersion.IsWine && !_disableHookedTheming && Global.Config.DarkMode)
        {
            nint wnd = Native.WindowFromDC(hdc);
            Control? c = Control.FromHandle(wnd);
            if (c is DateTimePicker dtp)
            {
                nint logBrushPtr = Marshal.AllocHGlobal(Marshal.SizeOf<Native.LOGBRUSH>());
                try
                {
                    Native.GetObject(hbr, Marshal.SizeOf<Native.LOGBRUSH>(), logBrushPtr);
                    Native.LOGBRUSH logBrush = Marshal.PtrToStructure<Native.LOGBRUSH>(logBrushPtr);
                    Color color = ColorTranslator.FromWin32((int)logBrush.lbColor);
                    nint brush;
                    // Wine doesn't render any selection highlight when not focused, so we don't need to either
                    if (color.EqualsPhysically(SystemColors.ActiveCaption))
                    {
                        brush = NativeBrush_BlueSelection;
                    }
                    else
                    {
                        brush = dtp.Enabled ? NativeBrush_LightBackground : NativeBrush_DarkBackground;
                    }
                    int result = FillRect_Original!(hdc, ref lprc, brush);
                    return result;
                }
                finally
                {
                    Marshal.FreeHGlobal(logBrushPtr);
                }
            }
        }

        return FillRect_Original!(hdc, ref lprc, hbr);
    }

    private static int DrawTextW_Hooked(
        nint hdc,
        [MarshalAs(UnmanagedType.LPWStr)]
        string lpszString,
        int nCount,
        ref Native.RECT lpRect,
        int nFormat)
    {
        if (WinVersion.IsWine && !_disableHookedTheming && Global.Config.DarkMode)
        {
            nint wnd = Native.WindowFromDC(hdc);
            Control? c = Control.FromHandle(wnd);
            // If it's disabled, just let it render the default disabled color, as it looks fine.
            // We'd like to use the brighter text color for selected fields, but we have no way to know from this
            // function whether we're drawing text on a selected field or not. So, just leave it as is.
            if (c is DateTimePicker { Enabled: true })
            {
                int prevColor = Native.SetTextColor(hdc, ColorTranslator.ToWin32(DarkColors.LightText));
                try
                {
                    int result = DrawTextW_Original!(hdc, lpszString, nCount, ref lpRect, nFormat);
                    return result;
                }
                finally
                {
                    Native.SetTextColor(hdc, prevColor);
                }
            }
        }

        return DrawTextW_Original!(hdc, lpszString, nCount, ref lpRect, nFormat);
    }

    #endregion

    #endregion

    #region Theming

    private static bool _disableHookedTheming;

    /*
    Hooked themes leak into unthemed dialogs (open file, browse folder etc.), and we can't tell them not to,
    because we can only exclude by thread, and we already know how impossible/focus-fucked threaded dialogs
    are, so we're not even going to go there. The best we can do is to turn off hooked theming _while_ a dialog
    is up. Normally this looks fine, but if our app gets minimized (or Show-Desktop'd) and restored, then the
    hooked themes will be disabled throughout the app (ie. we'll get unthemed scroll bars). This is a fairly
    unlikely scenario though, and even if it happens we refresh the whole app on dialog close anyway so at
    least it's temporary.
    */
    [StructLayout(LayoutKind.Auto)]
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
                List<nint> handles = Native.GetProcessWindowHandles();
                foreach (nint handle in handles)
                {
                    Control? control = Control.FromHandle(handle);
                    if (control is Form form) form.Refresh();
                }
            }
        }
    }

    #region Colors and brushes

    private const int COLOR_HIGHLIGHT = 13;
    private const int COLOR_HIGHLIGHT_TEXT = 14;
    private const int COLOR_WINDOW = 5;
    private const int COLOR_WINDOWTEXT = 8;
    private const int COLOR_3DFACE = 15;
    private const int COLOR_GRAYTEXT = 17;

#if !X64

    private static readonly nint SysColorBrush_LightBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightBackground));
    private static readonly nint SysColorBrush_LightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightText));
    private static readonly nint SysColorBrush_BlueSelection = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.BlueSelection));
    private static readonly nint SysColorBrush_Fen_HighlightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_HighlightText));
    private static readonly nint SysColorBrush_Fen_ControlBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground));
    private static readonly nint SysColorBrush_DisabledText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DisabledText));
    private static readonly nint SysColorBrush_Fen_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground));
    private static readonly nint SysColorBrush_Fen_DarkForeground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkForeground));
    private static readonly nint SysColorBrush_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DarkBackground));

#endif

    private static readonly nint NativeBrush_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DarkBackground));
    private static readonly nint NativeBrush_LightBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightBackground));
    private static readonly nint NativeBrush_BlueSelection = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.BlueSelection));

    #endregion

    #region System color overriding

    internal enum Override
    {
        None,
        Full,
        RichText,
    }

    // We set/unset this while painting specific controls, so other controls aren't affected by the global
    // color change
    private static Override SysColorOverride = Override.None;

    [StructLayout(LayoutKind.Auto)]
    internal readonly ref struct OverrideSysColorScope
    {
        public OverrideSysColorScope(Override @override) => SysColorOverride = @override;

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void Dispose() => SysColorOverride = Override.None;
    }

    #endregion

    #region Theme rendering

    #region Arrays

    private enum RenderedControl
    {
        ScrollBar,
        ToolTip,
        TreeView,
        TabScrollButtons,
        Trackbar,
        Progress,
        Length,
    }

    private static readonly nint[] _hThemes = new nint[(int)RenderedControl.Length];

    // ReSharper disable once RedundantExplicitArraySize
    private static readonly string[] _clSids = new string[(int)RenderedControl.Length]
    {
        "Scrollbar",
        "ToolTip",
        "TreeView",
        "Spin",
        "Trackbar",
        "Progress",
    };

    #endregion

    #region Reloading

    private static void ReloadHThemes()
    {
        for (int i = 0; i < (int)RenderedControl.Length; i++)
        {
            Native.CloseThemeData(_hThemes[i]);
            using Control c = new();
            _hThemes[i] = Native.OpenThemeData(c.Handle, _clSids[i]);
        }
    }

    internal static void ReloadTheme()
    {
        if (!_hooksInstalled || !Native.IsThemeActive()) return;

        // We have to reload the HTheme keys because they may/will(?) have changed
        ReloadHThemes();
    }

    #endregion

    #region Control rendering

    #region Trackbar

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TrackBarEnabled() => Global.Config.DarkMode;

    private static bool TrackBar_TryGetThemeColor(
        int iPartId,
        int iStateId,
        out int pColor)
    {
        if (iPartId == Native.TKP_TICS && iStateId == Native.TSS_NORMAL)
        {
            pColor = ColorTranslator.ToWin32(DarkColors.LightBackground);
            return true;
        }
        else
        {
            pColor = 0;
            return false;
        }
    }

    private static readonly PointF[] _trackBarThumbBottomPoints = new PointF[4];

    private static bool TrackBar_TryDrawThemeBackground(
        nint hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect)
    {
        using Graphics g = Graphics.FromHdc(hdc);

        Rectangle rect = pRect.ToRectangle();

        switch (iPartId)
        {
            case Native.TKP_TRACK:
                g.FillRectangle(DarkColors.DarkBackgroundBrush, rect);
                Rectangle borderRect = rect with { Width = rect.Width - 1, Height = rect.Height - 1 };
                g.DrawRectangle(DarkColors.LightBackgroundPen, borderRect);
                break;
            case Native.TKP_THUMBBOTTOM:
            {
                Brush brush = iStateId switch
                {
                    Native.TUBS_HOT => DarkColors.BlueHighlightBrush,
                    Native.TUBS_PRESSED => DarkColors.BlueBackgroundBrush,
                    Native.TUBS_DISABLED => DarkColors.LightBackgroundBrush,
                    //Native.TUBS_NORMAL => DarkColors.BlueSelectionBrush,
                    //Native.TUBS_FOCUSED => DarkColors.BlueSelectionBrush,
                    _ => DarkColors.BlueSelectionBrush,
                };

                Rectangle squarePartRect = rect with { Height = rect.Height - 5 };
                g.FillRectangle(brush, squarePartRect);

                _trackBarThumbBottomPoints[0] = new PointF(rect.X + -0.5f, rect.Y + 13);
                _trackBarThumbBottomPoints[1] = new PointF(rect.X + 5, rect.Y + 18.5f);
                _trackBarThumbBottomPoints[2] = new PointF(rect.X + 10.5f, rect.Y + 13);
                _trackBarThumbBottomPoints[3] = new PointF(rect.X + -0.5f, rect.Y + 13);

                g.SmoothingMode = SmoothingMode.HighQuality;
                g.FillPolygon(brush, _trackBarThumbBottomPoints);
                break;
            }
        }

        return true;
    }

    #endregion

    #region Tab scroll buttons

    // This is really for all horizontal spinners, as no messages are sent when the tab control's spinner needs
    // to be painted, so we can't override just for that...
    // It's okay because we don't use them anywhere else, but yeah.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TabScrollButtonsEnabled() => Global.Config.DarkMode;

    private static bool TabScrollButtons_TryDrawThemeBackground(
        nint hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect)
    {
        using Graphics g = Graphics.FromHdc(hdc);

        Rectangle rect = pRect.ToRectangle();

        if (iPartId
            is not Native.SPNP_UPHORZ
            and not Native.SPNP_DOWNHORZ)
        {
            return false;
        }

        g.FillRectangle(DarkColors.DarkBackgroundBrush, rect);
        g.DrawRectangle(DarkColors.LighterBackgroundPen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

        Pen pen;
        switch (iStateId)
        {
            case Native.UP_OR_DOWN_HZS_PRESSED:
                pen = DarkColors.ActiveControlPen;
                break;
            case Native.UP_OR_DOWN_HZS_DISABLED:
                pen = DarkColors.GreySelectionPen;
                break;
            case Native.UP_OR_DOWN_HZS_HOT:
                pen = DarkColors.GreyHighlightPen;
                break;
            case Native.UP_OR_DOWN_HZS_NORMAL:
            default:
                pen = DarkColors.GreySelectionPen;
                break;
        }

        Direction direction = iPartId == Native.SPNP_UPHORZ
            ? Direction.Right
            : Direction.Left;

        Images.PaintArrow7x4(
            g: g,
            direction: direction,
            area: rect,
            controlEnabled: iStateId != Native.UP_OR_DOWN_HZS_DISABLED,
            pen: pen);

        return true;
    }

    #endregion

    #region ScrollBar

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ScrollBarEnabled() => Global.Config.DarkMode || (WinVersion.Is11OrAbove && !Native.HighContrastEnabled());

    private static bool ScrollBar_TryDrawThemeBackground(
        nint hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect)
    {
        bool usingLightMode = WinVersion.Is11OrAbove && !Global.Config.DarkMode && !Native.HighContrastEnabled();

        using Graphics g = Graphics.FromHdc(hdc);

        #region Background

        Rectangle rect = pRect.ToRectangle();

        Brush brush;
        switch (iPartId)
        {
            case Native.SBP_ARROWBTN:
            {
                if (usingLightMode)
                {
                    switch (iStateId)
                    {
                        case Native.ABS_UPPRESSED:
                        case Native.ABS_DOWNPRESSED:
                        case Native.ABS_LEFTPRESSED:
                        case Native.ABS_RIGHTPRESSED:
                            brush = DarkColors.LightScrollBarButtonPressedBrush;
                            break;
                        case Native.ABS_UPDISABLED:
                        case Native.ABS_DOWNDISABLED:
                        case Native.ABS_LEFTDISABLED:
                        case Native.ABS_RIGHTDISABLED:
                            brush = SystemBrushes.Control;
                            break;
                        case Native.ABS_UPHOT:
                        case Native.ABS_DOWNHOT:
                        case Native.ABS_LEFTHOT:
                        case Native.ABS_RIGHTHOT:
                            brush = DarkColors.LightScrollBarButtonHotBrush;
                            break;
#if false
                        case Native.ABS_UPNORMAL:
                        case Native.ABS_DOWNNORMAL:
                        case Native.ABS_LEFTNORMAL:
                        case Native.ABS_RIGHTNORMAL:
#endif
                        default:
                            brush = SystemBrushes.Control;
                            break;
                    }
                }
                else
                {
                    brush = DarkColors.DarkBackgroundBrush;
                }
                break;
            }
            case Native.SBP_GRIPPERHORZ:
            case Native.SBP_GRIPPERVERT:
                // The "gripper" is a subset of the thumb, except sometimes it extends outside of it and
                // causes problems with our thumb width correction, so just don't draw it
                return true;
            case Native.SBP_THUMBBTNHORZ:
            case Native.SBP_THUMBBTNVERT:

                #region Correct the thumb width

                Pen thumbPen = usingLightMode
                    ? SystemPens.Control
                    : DarkColors.DarkBackgroundPen;

                // Match Windows behavior - the thumb is 1px in from each side
                // The "gripper" rect gives us the right width, but the wrong length
                switch (iPartId)
                {
                    case Native.SBP_THUMBBTNHORZ:
                        g.DrawLine(thumbPen, rect.X, rect.Y, rect.Right, rect.Y);
                        g.DrawLine(thumbPen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                        rect = rect with { Y = rect.Y + 1, Height = rect.Height - 2 };
                        break;
                    case Native.SBP_THUMBBTNVERT:
                        g.DrawLine(thumbPen, rect.X, rect.Y, rect.X, rect.Bottom);
                        g.DrawLine(thumbPen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
                        rect = rect with { X = rect.X + 1, Width = rect.Width - 2 };
                        break;
                }

                #endregion

                brush = iStateId switch
                {
                    Native.SCRBS_NORMAL =>
                        usingLightMode
                            ? DarkColors.ScrollBarLightBrush
                            : DarkColors.GreySelectionBrush,
                    Native.SCRBS_HOVER =>
                        usingLightMode
                            ? DarkColors.LightScrollBarHoverBrush
                            : DarkColors.Fen_ThumbScrollBarHoverBrush,
                    Native.SCRBS_HOT =>
                        usingLightMode
                            ? DarkColors.DisabledTextBrush
                            : SystemBrushes.ControlDarkDark,
                    Native.SCRBS_PRESSED =>
                        usingLightMode
                            ? DarkColors.LightScrollBarButtonPressedBrush
                            : DarkColors.ActiveControlBrush,
                    _ => DarkColors.DarkGreySelectionBrush,
                };
                break;
            default:
                brush =
                    usingLightMode
                        ? SystemBrushes.Control
                        : DarkColors.DarkBackgroundBrush;
                break;
        }

        g.FillRectangle(brush, rect);

        #endregion

        #region Arrow

        if (iPartId == Native.SBP_ARROWBTN)
        {
            Pen pen;
            switch (iStateId)
            {
                case Native.ABS_UPPRESSED:
                case Native.ABS_DOWNPRESSED:
                case Native.ABS_LEFTPRESSED:
                case Native.ABS_RIGHTPRESSED:
                    pen =
                        usingLightMode
                            ? SystemPens.Control
                            : DarkColors.ActiveControlPen;
                    break;
                case Native.ABS_UPDISABLED:
                case Native.ABS_DOWNDISABLED:
                case Native.ABS_LEFTDISABLED:
                case Native.ABS_RIGHTDISABLED:
                    pen =
                        usingLightMode
                            ? SystemPens.ControlDark
                            : DarkColors.GreySelectionPen;
                    break;
                case Native.ABS_UPHOT:
                case Native.ABS_DOWNHOT:
                case Native.ABS_LEFTHOT:
                case Native.ABS_RIGHTHOT:
                    pen =
                        usingLightMode
                            ? SystemPens.ControlText
                            : DarkColors.GreyHighlightPen;
                    break;
#if false
                case Native.ABS_UPNORMAL:
                case Native.ABS_DOWNNORMAL:
                case Native.ABS_LEFTNORMAL:
                case Native.ABS_RIGHTNORMAL:
#endif
                default:
                    pen =
                        usingLightMode
                            ? SystemPens.ControlDarkDark
                            : DarkColors.GreySelectionPen;
                    break;
            }

            Direction direction;
            switch (iStateId)
            {
                case Native.ABS_LEFTNORMAL:
                case Native.ABS_LEFTHOT:
                case Native.ABS_LEFTPRESSED:
                case Native.ABS_LEFTHOVER:
                case Native.ABS_LEFTDISABLED:
                    direction = Direction.Left;
                    break;
                case Native.ABS_RIGHTNORMAL:
                case Native.ABS_RIGHTHOT:
                case Native.ABS_RIGHTPRESSED:
                case Native.ABS_RIGHTHOVER:
                case Native.ABS_RIGHTDISABLED:
                    direction = Direction.Right;
                    break;
                case Native.ABS_UPNORMAL:
                case Native.ABS_UPHOT:
                case Native.ABS_UPPRESSED:
                case Native.ABS_UPHOVER:
                case Native.ABS_UPDISABLED:
                    direction = Direction.Up;
                    break;
#if false
                case Native.ABS_DOWNNORMAL:
                case Native.ABS_DOWNHOT:
                case Native.ABS_DOWNPRESSED:
                case Native.ABS_DOWNHOVER:
                case Native.ABS_DOWNDISABLED:
#endif
                default:
                    direction = Direction.Down;
                    break;
            }

            Images.PaintArrow7x4(
                g,
                direction,
                rect,
                pen: pen
            );
        }

        #endregion

        return true;
    }

    private static bool ScrollBar_TryGetThemeColor(
        int iPartId,
        int iPropId,
        out int pColor)
    {
        // This is for scrollbar vert/horz corners on Win10 (and maybe Win8? Haven't tested it).
        // This is the ONLY way that works on those versions.
        if (iPartId == Native.SBP_CORNER && iPropId == Native.TMT_FILLCOLOR)
        {
            bool usingLightMode = WinVersion.Is11OrAbove && !Global.Config.DarkMode && !Native.HighContrastEnabled();

            Color color = usingLightMode
                ? SystemColors.Control
                : DarkColors.DarkBackground;

            pColor = ColorTranslator.ToWin32(color);
            return true;
        }
        else
        {
            pColor = 0;
            return false;
        }
    }

    #region Wine-friendly scroll bar interior theming

    /*
    @Wine: Wine's scroll bars don't respond to the usual hook for the track area (or "interior" or whatever it's
    really called), so use a different technique.
    */

    private static bool PointIsOnScrollBarTrackArea(ScrollBar scrollBar, int x, int y)
    {
        Native.SCROLLBARINFO sbi = new() { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
        Native.GetScrollBarInfo(scrollBar.Handle, Native.OBJID_CLIENT, ref sbi);

        Rectangle thumbRect;
        Rectangle firstArrowRect;
        Rectangle secondArrowRect;

        if (scrollBar is VScrollBar)
        {
            int vsbWidth = SystemInformation.VerticalScrollBarWidth;
            int vsbArrowHeight = SystemInformation.VerticalScrollBarArrowHeight;

            thumbRect = new Rectangle(
                x: 0,
                y: sbi.xyThumbTop,
                width: sbi.dxyLineButton,
                height: sbi.xyThumbBottom - sbi.xyThumbTop
            );

            firstArrowRect = new Rectangle(
                x: 0,
                y: 0,
                width: vsbWidth,
                height: vsbArrowHeight);

            secondArrowRect = new Rectangle(
                x: 0,
                y: scrollBar.Height - vsbArrowHeight,
                width: vsbWidth,
                height: vsbArrowHeight);
        }
        else
        {
            int hsbHeight = SystemInformation.HorizontalScrollBarHeight;
            int hsbArrowWidth = SystemInformation.HorizontalScrollBarArrowWidth;

            thumbRect = new Rectangle(
                x: sbi.xyThumbTop,
                y: 0,
                width: sbi.xyThumbBottom - sbi.xyThumbTop,
                height: sbi.dxyLineButton
            );

            firstArrowRect = new Rectangle(
                x: 0,
                y: 0,
                width: hsbArrowWidth,
                height: hsbHeight);

            secondArrowRect = new Rectangle(
                x: scrollBar.Width - hsbArrowWidth,
                y: 0,
                width: hsbArrowWidth,
                height: hsbHeight);
        }

        return !thumbRect.Contains(x, y) && !firstArrowRect.Contains(x, y) && !secondArrowRect.Contains(x, y);
    }

    #endregion

    #endregion

    #region ToolTip

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ToolTipEnabled() => Global.Config.DarkMode && ControlUtils.ToolTipsReflectable;

    private static bool ToolTip_TryDrawThemeBackground(
        nint hdc,
        int iPartId,
        ref Native.RECT pRect)
    {
        using Graphics g = Graphics.FromHdc(hdc);
        if (iPartId is Native.TTP_STANDARD or Native.TTP_STANDARDTITLE)
        {
            Rectangle rect = pRect.ToRectangle();

            g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, rect);
            g.DrawRectangle(
                DarkColors.GreySelectionPen,
                rect with
                {
                    Width = rect.Width - 1,
                    Height = rect.Height - 1,
                });
            return true;
        }
        else
        {
            return false;
        }
    }

    private static bool ToolTip_TryGetThemeColor(
        int iPropId,
        out int pColor)
    {
        if (iPropId == Native.TMT_TEXTCOLOR)
        {
            pColor = ColorTranslator.ToWin32(DarkColors.LightText);
            return true;
        }
        else
        {
            pColor = 0;
            return false;
        }
    }

    #endregion

    #region TreeView

    private static bool TreeView_TryDrawThemeBackground(
        nint hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect)
    {
        if (iPartId is not Native.TVP_GLYPH and not Native.TVP_HOTGLYPH) return false;

        using Graphics g = Graphics.FromHdc(hdc);

        Rectangle rect = pRect.ToRectangle();

        Direction direction = iStateId is Native.GLPS_CLOSED or Native.HGLPS_CLOSED
            ? Direction.Right
            : Direction.Down;

        Images.PaintArrow7x4(
            g,
            direction,
            rect,
            pen: Global.Config.DarkMode ? DarkColors.LightTextPen : SystemPens.WindowText);

        return true;
    }

    #endregion

    #region Progress bar

    private static bool ProgressBarEnabled() => Global.Config.DarkMode && WinVersion.IsWine;

    private static bool ProgressBar_TryDrawThemeBackground(
        nint hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect,
        nint pClipRect)
    {
        using Graphics g = Graphics.FromHdc(hdc);

        Rectangle rect = pRect.ToRectangle();

        /*
        PP_FILL = foreground
        PP_BAR = background
        */

        switch (iPartId)
        {
            case Native.PP_FILL:
            case Native.PP_FILLVERT:
            {
                g.FillRectangle(DarkColors.BlueHighlightBrush, rect);
                return true;
            }
            case Native.PP_BAR:
            case Native.PP_BARVERT:
            {
                if (pClipRect != 0)
                {
                    Native.RECT clipRectNative = Marshal.PtrToStructure<Native.RECT>(pClipRect);
                    Rectangle clipRect = clipRectNative.ToRectangle();
                    g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, clipRect);
                }
                else
                {
                    Rectangle modRect = rect with { Width = rect.Width - 1, Height = rect.Height - 1 };
                    g.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, modRect);
                }
                return true;
            }
        }

        return false;
    }

    #endregion

    #endregion

    #endregion

    #endregion
}

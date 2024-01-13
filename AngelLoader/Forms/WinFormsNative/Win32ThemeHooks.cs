/*
Here's the deal with this thing:

-On Framework, we need EasyHook.
-On .NET modern, we need CoreHook.
-On .NET modern, they've added the [SuppressGCTransition] attribute to the internal GetSysColor() extern function,
 which improves performance but causes an ExecutionEngineException when hooked. Even when we use our own defined
 p/invoke for it without the attribute, it still happens somehow. I don't know how this kind of thing works so
 whatever.
-We include our own custom version of System.Drawing.Primitives.dll without the [SuppressGCTransition] attribute
 on GetSysColor().
*/

using System;
using System.Collections.Generic;
using System.Drawing;
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

    #region Hooks and delegates

    #region GetSysColor

    private static LocalHook? _getSysColorHook;

    private static GetSysColorDelegate? GetSysColor_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate int GetSysColorDelegate(int nIndex);

    #endregion

    #region GetSysColorBrush

#if !NETFRAMEWORK || !X64

    private static LocalHook? _getSysColorBrushHook;

    private static GetSysColorBrushDelegate? GetSysColorBrush_Original;

    [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
    private delegate IntPtr GetSysColorBrushDelegate(int nIndex);

#endif

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

    #endregion

    #region Hook installing

    private static bool _hooksInstalled;

    internal static void InstallHooks()
    {
        if (_hooksInstalled) return;

        ReloadHThemes();

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
            // 2023-11-11: This seems to work with CoreHook.
#if !NETFRAMEWORK || !X64
            (_getSysColorBrushHook, GetSysColorBrush_Original) = InstallHook<GetSysColorBrushDelegate>(
                "user32.dll",
                "GetSysColorBrush",
                GetSysColorBrush_Hooked);
#endif

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
#if !NETFRAMEWORK || !X64
            _getSysColorBrushHook?.Dispose();
#endif
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

    #endregion

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
        }

        return succeeded
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
            }
            : GetSysColor_Original!(nIndex);
    }

#if !NETFRAMEWORK || !X64

    private static IntPtr GetSysColorBrush_Hooked(int nIndex)
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
            }
            : GetSysColorBrush_Original!(nIndex);
    }

#endif

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

    #region Colors and brushes

    private const int COLOR_HIGHLIGHT = 13;
    private const int COLOR_HIGHLIGHT_TEXT = 14;
    private const int COLOR_WINDOW = 5;
    private const int COLOR_WINDOWTEXT = 8;
    private const int COLOR_3DFACE = 15;
    private const int COLOR_GRAYTEXT = 17;

#if !NETFRAMEWORK || !X64

    private static readonly IntPtr SysColorBrush_LightBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightBackground));
    private static readonly IntPtr SysColorBrush_LightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightText));
    private static readonly IntPtr SysColorBrush_BlueSelection = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.BlueSelection));
    private static readonly IntPtr SysColorBrush_Fen_HighlightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_HighlightText));
    private static readonly IntPtr SysColorBrush_Fen_ControlBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground));
    private static readonly IntPtr SysColorBrush_DisabledText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DisabledText));
    private static readonly IntPtr SysColorBrush_Fen_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground));
    private static readonly IntPtr SysColorBrush_Fen_DarkForeground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkForeground));
    private static readonly IntPtr SysColorBrush_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DarkBackground));

#endif

    #endregion

    #region System color overriding

    internal enum Override
    {
        None,
        Full,
        RichText
    }

    // We set/unset this while painting specific controls, so other controls aren't affected by the global
    // color change
    private static Override SysColorOverride = Override.None;

    internal readonly ref struct OverrideSysColorScope
    {
        public OverrideSysColorScope(Override @override) => SysColorOverride = @override;

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public void Dispose() => SysColorOverride = Override.None;
    }

    #endregion

    #region Theme rendering

    #region Arrays

    private const int _renderedControlCount = 4;
    private enum RenderedControl
    {
        ScrollBar,
        ToolTip,
        TreeView,
        TabScrollButtons
    }

    private static readonly IntPtr[] _hThemes = new IntPtr[_renderedControlCount];

    private static readonly string[] _clSids =
    {
        "Scrollbar",
        "ToolTip",
        "TreeView",
        "Spin"
    };

    #endregion

    #region Reloading

    private static void ReloadHThemes()
    {
        for (int i = 0; i < _renderedControlCount; i++)
        {
            Native.CloseThemeData(_hThemes[i]);
            using var c = new Control();
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

    #region Tab scroll buttons

    // This really for all horizontal spinners, as no messages are sent when the tab control's spinner needs to
    // be painted, so we can't override just for that...
    // It's okay because we don't use them anywhere else, but yeah.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TabScrollButtonsEnabled() => Global.Config.DarkMode;

    private static bool TabScrollButtons_TryDrawThemeBackground(
        IntPtr hdc,
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
    private static bool ScrollBarEnabled() => Global.Config.DarkMode;

    private static bool ScrollBar_TryDrawThemeBackground(
        IntPtr hdc,
        int iPartId,
        int iStateId,
        ref Native.RECT pRect)
    {
        using Graphics g = Graphics.FromHdc(hdc);

        #region Background

        Rectangle rect = pRect.ToRectangle();

        Brush brush;
        switch (iPartId)
        {
            case Native.SBP_ARROWBTN:
                brush = DarkColors.DarkBackgroundBrush;
                break;
            case Native.SBP_GRIPPERHORZ:
            case Native.SBP_GRIPPERVERT:
                // The "gripper" is a subset of the thumb, except sometimes it extends outside of it and
                // causes problems with our thumb width correction, so just don't draw it
                return true;
            case Native.SBP_THUMBBTNHORZ:
            case Native.SBP_THUMBBTNVERT:

                #region Correct the thumb width

                // Match Windows behavior - the thumb is 1px in from each side
                // The "gripper" rect gives us the right width, but the wrong length
                switch (iPartId)
                {
                    case Native.SBP_THUMBBTNHORZ:
                        g.DrawLine(DarkColors.DarkBackgroundPen, rect.X, rect.Y, rect.Right, rect.Y);
                        g.DrawLine(DarkColors.DarkBackgroundPen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                        rect = rect with { Y = rect.Y + 1, Height = rect.Height - 2 };
                        break;
                    case Native.SBP_THUMBBTNVERT:
                        g.DrawLine(DarkColors.DarkBackgroundPen, rect.X, rect.Y, rect.X, rect.Bottom);
                        g.DrawLine(DarkColors.DarkBackgroundPen, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom);
                        rect = rect with { X = rect.X + 1, Width = rect.Width - 2 };
                        break;
                }

                #endregion

                brush = iStateId switch
                {
                    Native.SCRBS_NORMAL => DarkColors.GreySelectionBrush,
                    Native.SCRBS_HOVER => DarkColors.Fen_ThumbScrollBarHoverBrush,
                    Native.SCRBS_HOT => DarkColors.GreyHighlightBrush,
                    Native.SCRBS_PRESSED => DarkColors.ActiveControlBrush,
                    _ => DarkColors.GreySelectionBrush
                };
                break;
            default:
                brush = DarkColors.DarkBackgroundBrush;
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
                    pen = DarkColors.ActiveControlPen;
                    break;
                case Native.ABS_UPDISABLED:
                case Native.ABS_DOWNDISABLED:
                case Native.ABS_LEFTDISABLED:
                case Native.ABS_RIGHTDISABLED:
                    pen = DarkColors.GreySelectionPen;
                    break;
                case Native.ABS_UPHOT:
                case Native.ABS_DOWNHOT:
                case Native.ABS_LEFTHOT:
                case Native.ABS_RIGHTHOT:
                    pen = DarkColors.GreyHighlightPen;
                    break;
#if false
                case Native.ABS_UPNORMAL:
                case Native.ABS_DOWNNORMAL:
                case Native.ABS_LEFTNORMAL:
                case Native.ABS_RIGHTNORMAL:
#endif
                default:
                    pen = DarkColors.GreySelectionPen;
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
            pColor = ColorTranslator.ToWin32(DarkColors.DarkBackground);
            return true;
        }
        else
        {
            pColor = 0;
            return false;
        }
    }

    #endregion

    #region ToolTip

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ToolTipEnabled() => Global.Config.DarkMode && ControlUtils.ToolTipsReflectable;

    private static bool ToolTip_TryDrawThemeBackground(
        IntPtr hdc,
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
                    Height = rect.Height - 1
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
        IntPtr hdc,
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

    #endregion

    #endregion

    #endregion
}

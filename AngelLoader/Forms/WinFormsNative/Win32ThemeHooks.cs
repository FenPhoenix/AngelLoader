#region .NET 5+ explanation

/*
tl;dr: For .NET 5+, EasyHook doesn't work and you must replace it with something else (MinHook.NET works).
HOWEVER, the GetSysColor hook crashes with an ExecutionEngineException upon return, always. All other hooks
work.

-2022-11-09: I posted a bug report and they told me it's because GetSysColor() has the SuppressGCTransition
 attribute on it in newer .NETs. Nothing that can be done.

With Reloaded.Hooks, I think we can bring back the GetSysColorBrush() hook for 64-bit. I think, anyway. I don't
remember for certain.

GetSysColor is necessary to theme the DateTimePicker; the selection color for textboxes; and the default text
color for the RichTextBox (though the latter CAN be worked around - clunkily - by making the default color explicit
in the color table and then inserting \cf0 control words after every \pard, \sectd, and \plain (I think that's
all of them...)).

This file is left here as a basic example of how the .NET 5+ friendly way to hook would look like, if GetSysColor
actually worked.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using EasyHook;

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
#if !X64
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

    private static readonly IntPtr SysColorBrush_LightBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightBackground));
    private static readonly IntPtr SysColorBrush_LightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.LightText));
    private static readonly IntPtr SysColorBrush_BlueSelection = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.BlueSelection));
    private static readonly IntPtr SysColorBrush_Fen_HighlightText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_HighlightText));
    private static readonly IntPtr SysColorBrush_Fen_ControlBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_ControlBackground));
    private static readonly IntPtr SysColorBrush_DisabledText = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DisabledText));
    private static readonly IntPtr SysColorBrush_Fen_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkBackground));
    private static readonly IntPtr SysColorBrush_Fen_DarkForeground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.Fen_DarkForeground));
    private static readonly IntPtr SysColorBrush_DarkBackground = Native.CreateSolidBrush(ColorTranslator.ToWin32(DarkColors.DarkBackground));

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

    private const int _renderedControlCount = 3;
    private enum RenderedControl
    {
        ScrollBar,
        ToolTip,
        TreeView
    }

    private static readonly IntPtr[] _hThemes = new IntPtr[_renderedControlCount];

    private static readonly string[] _clSids =
    {
        "Scrollbar",
        "ToolTip",
        "TreeView"
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

            Misc.Direction direction;
            switch (iStateId)
            {
                case Native.ABS_LEFTNORMAL:
                case Native.ABS_LEFTHOT:
                case Native.ABS_LEFTPRESSED:
                case Native.ABS_LEFTHOVER:
                case Native.ABS_LEFTDISABLED:
                    direction = Misc.Direction.Left;
                    break;
                case Native.ABS_RIGHTNORMAL:
                case Native.ABS_RIGHTHOT:
                case Native.ABS_RIGHTPRESSED:
                case Native.ABS_RIGHTHOVER:
                case Native.ABS_RIGHTDISABLED:
                    direction = Misc.Direction.Right;
                    break;
                case Native.ABS_UPNORMAL:
                case Native.ABS_UPHOT:
                case Native.ABS_UPPRESSED:
                case Native.ABS_UPHOVER:
                case Native.ABS_UPDISABLED:
                    direction = Misc.Direction.Up;
                    break;
#if false
                case Native.ABS_DOWNNORMAL:
                case Native.ABS_DOWNHOT:
                case Native.ABS_DOWNPRESSED:
                case Native.ABS_DOWNHOVER:
                case Native.ABS_DOWNDISABLED:
#endif
                default:
                    direction = Misc.Direction.Down;
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

        Misc.Direction direction = iStateId is Native.GLPS_CLOSED or Native.HGLPS_CLOSED
            ? Misc.Direction.Right
            : Misc.Direction.Down;

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

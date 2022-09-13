//#define ENABLE_NET5_PLUS_HOOKS

/*
tl;dr: For .NET 5+, EasyHook doesn't work and you must replace it with something else (MinHook.NET works).
HOWEVER, the GetSysColor hook crashes with an ExecutionEngineException upon return, always. All other hooks
work.

GetSysColor is necessary to theme the DateTimePicker; the selection color for textboxes; and the default text
color for the RichTextBox (though the latter CAN be worked around - clunkily - by making the default color explicit
in the color table and then inserting \cf0 control words after every \pard, \sectd, and \plain (I think that's
all of them...)).

This file is left here as a basic example of how the .NET 5+ friendly way to hook would look like, if GetSysColor
actually worked.

---

Detailed notes:

@NET5: .NET 5+ hooking research:
-MinHook.NET works on .NET 5+ in general, but the GetSysColor hook causes an ExecutionEngineException. Even
 if I have it literally just return 0. It DOES start executing the hooked method (confirmed by putting a
 Trace.WriteLine() in there), but crashes with the exception on return.
 All the other hooks work fine. It's just GetSysColor that throws the dreaded no-stack-trace-and-no-info
 ExecutionEngineException.
-MinSharp (a wrapper, not a port like MinHook.NET) doesn't even get as far as loading its native dll.
 Ever, no matter what fiddling I do. So anyway.
No exceptions for any hook library on Framework, they all work.
God only knows what the god damn hell the .NET version has to do with running A NATIVE WINDOWS PROC but hey.
-Only thing I can think of is using native C++ hook code, shoving it in a dll, and p/invoking it like "hey
 start your hook that has nothing to do with me now". Why do I suspect that wouldn't work either, just to make
 me furious for no reason. Why indeed.
-2022-08-25: Unfortunately, as I'd feared, the above doesn't work. It does indeed result in the same old
 ExecutionEngineException as I snarkily suspected it would. Sigh.
@FenHooks: Try bringing MinHook.NET code directly in, and tracing it
We can compare its .NET 6 state (broken) to the state when it runs on .NET Framework (working), and maybe
find the problem that way...
-2022-09-12: I haven't been able to find the problem that way. Next idea: Get Microsoft Detours and make a quick
 managed wrapper for it, test again, and if GetSysColor still crashes, post a bug report on the .NET repo.
*/

#if ENABLE_NET5_PLUS_HOOKS

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.ThemeRenderers;
using MinHook;
using ScrollBarRenderer = AngelLoader.Forms.ThemeRenderers.ScrollBarRenderer;

namespace AngelLoader.Forms.WinFormsNative
{
    internal static class Win32ThemeHooks_NET5_Plus
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

        private static GetSysColorDelegate? GetSysColor_Original;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int GetSysColorDelegate(int nIndex);

#endregion

#region GetSysColorBrush

        private static GetSysColorBrushDelegate? GetSysColorBrush_Original;

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate IntPtr GetSysColorBrushDelegate(int nIndex);

#endregion

#region DrawThemeBackground

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
        internal static Override SysColorOverride = Override.None;

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

        private static readonly HookEngine _hookEngine = new();

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
                GetSysColor_Original = _hookEngine.CreateHook(
                    "user32.dll",
                    "GetSysColor",
                    new GetSysColorDelegate(GetSysColor_Hooked));

                GetSysColorBrush_Original = _hookEngine.CreateHook(
                    "user32.dll",
                    "GetSysColorBrush",
                    new GetSysColorBrushDelegate(GetSysColorBrush_Hooked));

                DrawThemeBackground_Original = _hookEngine.CreateHook(
                    "uxtheme.dll",
                    "DrawThemeBackground",
                    new DrawThemeBackgroundDelegate(DrawThemeBackground_Hooked));

                GetThemeColor_Original = _hookEngine.CreateHook(
                    "uxtheme.dll",
                    "GetThemeColor",
                    new GetThemeColorDelegate(GetThemeColor_Hooked));

                _hookEngine.EnableHooks();
                _hooksInstalled = true;
            }
            catch
            {
                // If we fail, oh well, just keep the classic-mode colors then... better than nothing
                _hookEngine.DisableHooks();
                _hooksInstalled = false;
            }
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
        internal sealed class DialogScope : IDisposable
        {
            internal DialogScope()
            {
                _disableHookedTheming = true;
                ControlUtils.RecreateAllToolTipHandles();
            }

            public void Dispose()
            {
                _disableHookedTheming = false;
                // Do this AFTER re-enabling hooked theming, otherwise it doesn't take and we end up with
                // dark-on-dark tooltips
                ControlUtils.RecreateAllToolTipHandles();
                var handles = Native.GetProcessWindowHandles();
                foreach (IntPtr handle in handles)
                {
                    Control? control = Control.FromHandle(handle);
                    if (control is Form form) form.Refresh();
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
                   _themeRenderers.TryGetValue(hTheme, out ThemeRenderer? renderer) &&
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
                   _themeRenderers.TryGetValue(hTheme, out ThemeRenderer? renderer) &&
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
}
#endif
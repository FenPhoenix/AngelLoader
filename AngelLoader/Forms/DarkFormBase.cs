using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;
using static AngelLoader.Global;

namespace AngelLoader.Forms;

public class DarkFormBase : Form
{
    private bool _loading = true;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [PublicAPI]
    public new Icon? Icon
    {
        get => base.Icon;
        set => base.Icon = value;
    }

    [Browsable(true)]
    [PublicAPI]
    [DefaultValue(false)]
    public new bool ShowInTaskbar
    {
        get => base.ShowInTaskbar;
        set => base.ShowInTaskbar = value;
    }

    public DarkFormBase()
    {
        base.Icon = Preload.AL_Icon;
        base.ShowInTaskbar = false;

        Win32ThemeHooks.InstallHooks();
    }

    #region Theming

    private readonly List<KeyValuePair<Control, ControlUtils.ControlOriginalColors?>> _controlColors = new();

    /*
    @Import(RespondToSystemThemeChange()):
    This stupid thing can't be abstract because the class can't be abstract because the designer refuses to work
    with it if it is. If it can't be abstract, that means we can't enforce that it be overriden. We can't use an
    interface here either because then it only enforces it be put here, not in derived classes, and if it's here
    then derived classes aren't required to have it. We could put the interface on all derived classes, but then
    it's just as easy to forget that as it is to forget this method. Argh! We might need a custom analyzer to
    enforce this stupid thing.
    */
    public virtual void RespondToSystemThemeChange()
    {
        //throw new NotImplementedException(nameof(RespondToSystemThemeChange) + " must be implemented for all forms!");
    }

    private protected void SetThemeBase(
        VisualTheme theme,
        Func<Component, bool>? excludePredicate = null,
        bool createControlHandles = false,
        Func<Control, bool>? createHandlePredicate = null,
        int capacity = -1)
    {
        if (Utils.WinVersionSupportsDarkMode())
        {
            // Set title bar theme
            int value = theme == VisualTheme.Dark ? 1 : 0;
            int result = Native.DwmSetWindowAttribute(
                Handle,
                Native.DWMWA_USE_IMMERSIVE_DARK_MODE,
                ref value,
                Marshal.SizeOf<int>());
            if (result != 0)
            {
                Native.DwmSetWindowAttribute(
                    Handle,
                    Native.DWMWA_USE_IMMERSIVE_DARK_MODE_OLD,
                    ref value,
                    Marshal.SizeOf<int>());
            }
        }

        ControlUtils.SetTheme(
            baseControl: this,
            controlColors: _controlColors,
            theme: theme,
            excludePredicate: excludePredicate,
            createControlHandles: createControlHandles,
            createHandlePredicate: createHandlePredicate,
            capacity: capacity);
    }

    #endregion

    #region Event handling

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        _loading = false;

        if (!Config.DarkMode) return;

        Refresh();
        // Explicitly refresh non-client area - otherwise on Win7 the non-client area doesn't refresh and we
        // end up with blacked-out title bar and borders etc.
        Native.SendMessageW(Handle, Native.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
    }

    protected override void WndProc(ref Message m)
    {
        // Cover up the flash of bright/half-drawn controls on startup when in dark mode
        if (_loading &&
            Config.DarkMode &&
            IsHandleCreated &&
            (m.Msg
                is Native.WM_PAINT
                or Native.WM_SIZE
                or Native.WM_MOVE
                or Native.WM_WINDOWPOSCHANGED
                or Native.WM_ERASEBKGND
            ))
        {
            using (var gc = new Native.GraphicsContext(Handle))
            {
                gc.G.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, new Rectangle(0, 0, Width, Height));
            }

            if (m.Msg != Native.WM_PAINT)
            {
                base.WndProc(ref m);
            }
        }
        else
        {
            base.WndProc(ref m);
        }
    }

    #endregion
}

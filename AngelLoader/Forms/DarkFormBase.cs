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

/*
We want this one method to be abstract to enforce it be overridden, but then the whole class needs to be abstract,
but then the designer refuses to work with it. So give the designer what it wants in debug mode, but make it
abstract in non-debug so it won't compile if we miss an override.
*/
public
#if !DEBUG
    abstract
#endif
    class DarkFormBase : Form
{
    public
#if DEBUG
        virtual
#else
        abstract
#endif
        void RespondToSystemThemeChange()
#if DEBUG
        { }
#else
        ;
#endif

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

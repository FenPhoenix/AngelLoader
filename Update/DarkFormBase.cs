using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using Update.Properties;

namespace Update;
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
        base.Icon = Resources.AngelLoader;
        base.ShowInTaskbar = false;
    }

    #region Theming

    private readonly List<KeyValuePair<Control, ControlUtils.ControlOriginalColors?>> _controlColors = new();

    private protected void SetThemeBase(
        VisualTheme theme,
        Func<Component, bool>? excludePredicate = null)
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
            excludePredicate: excludePredicate);
    }

    #endregion

    #region Event handling

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        _loading = false;

        if (!Data.DarkMode) return;

        Refresh();
        // Explicitly refresh non-client area - otherwise on Win7 the non-client area doesn't refresh and we
        // end up with blacked-out title bar and borders etc.
        Native.SendMessage(Handle, Native.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
    }

    protected override void WndProc(ref Message m)
    {
        // Cover up the flash of bright/half-drawn controls on startup when in dark mode
        if (_loading &&
            Data.DarkMode &&
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

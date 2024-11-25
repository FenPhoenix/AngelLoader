using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public class DarkTextBox : TextBox, IDarkable
{
    private bool _origValuesStored;
    private Color? _origForeColor;
    private Color? _origBackColor;
    private Padding? _origPadding;
    private BorderStyle? _origBorderStyle;

    [PublicAPI]
    public Color DarkModeBackColor =>
        Enabled
            ? ReadOnly && !DarkModeReadOnlyColorsAreDefault
                ? DarkColors.Fen_ControlBackground
                : DarkColors.LightBackground
            : DarkColors.Fen_ControlBackground;

    [PublicAPI]
    public Color DarkModeForeColor =>
        Enabled
            ? ReadOnly && !DarkModeReadOnlyColorsAreDefault
                ? DarkColors.LightText
                : DarkColors.LightText
            : DarkColors.DisabledText;

    [PublicAPI]
    [DefaultValue(false)]
    public bool DarkModeReadOnlyColorsAreDefault { get; set; }

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public unsafe bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            var sbi_v = new Native.SCROLLBARINFO { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
            int result_v = Native.GetScrollBarInfo(Handle, Native.OBJID_VSCROLL, ref sbi_v);

            bool vertScrollBarNeedsRepositioning =
                result_v != 0 &&
                (sbi_v.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) == 0 &&
                (sbi_v.rgstate[0] & Native.STATE_SYSTEM_UNAVAILABLE) == 0;

            var sbi_h = new Native.SCROLLBARINFO { cbSize = Marshal.SizeOf(typeof(Native.SCROLLBARINFO)) };
            int result_h = Native.GetScrollBarInfo(Handle, Native.OBJID_HSCROLL, ref sbi_h);

            bool horzScrollBarNeedsRepositioning =
                result_h != 0 &&
                (sbi_h.rgstate[0] & Native.STATE_SYSTEM_INVISIBLE) == 0 &&
                (sbi_h.rgstate[0] & Native.STATE_SYSTEM_UNAVAILABLE) == 0;

            Native.SCROLLINFO si_v = new(), si_h = new();
            if (vertScrollBarNeedsRepositioning) si_v = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_VERT);
            if (horzScrollBarNeedsRepositioning) si_h = ControlUtils.GetCurrentScrollInfo(Handle, Native.SB_HORZ);

            if (_darkModeEnabled)
            {
                if (!_origValuesStored)
                {
                    _origForeColor ??= ForeColor;
                    _origBackColor ??= BackColor;
                    _origPadding ??= Padding;
                    _origBorderStyle ??= BorderStyle;
                    _origValuesStored = true;
                }

                BackColor = DarkModeBackColor;
                ForeColor = DarkModeForeColor;
                Padding = new Padding(2, 2, 2, 2);
                BorderStyle = BorderStyle.FixedSingle;

                // Needed for selection backcolor to always be correct
                // Multiline textboxes can't do this because it disables theming on their scrollbars.
                // Fortunately, multiline textboxes inexplicably don't need this to have their selection
                // backcolor work 100%. So hooray, we win by sheer luck or whatever. Moving on.
                if (!Multiline) Native.SetWindowTheme(Handle, "", "");

                FixBackColors();
            }
            else
            {
                if (_origValuesStored)
                {
                    BackColor = (Color)_origBackColor!;
                    ForeColor = (Color)_origForeColor!;
                    Padding = (Padding)_origPadding!;
                    BorderStyle = (BorderStyle)_origBorderStyle!;
                }

                // Reset theme
                if (!Multiline) RecreateHandle();
            }

            if (vertScrollBarNeedsRepositioning)
            {
                ControlUtils.RepositionScroll(Handle, si_v, Native.SB_VERT);
            }
            if (horzScrollBarNeedsRepositioning)
            {
                ControlUtils.RepositionScroll(Handle, si_h, Native.SB_HORZ);
            }
        }
    }

    public DarkTextBox() => base.DoubleBuffered = true;

    protected override void OnEnabledChanged(EventArgs e)
    {
        base.OnEnabledChanged(e);

        if (!_darkModeEnabled) return;

        BackColor = DarkModeBackColor;
        ForeColor = DarkModeForeColor;
    }

    protected override void OnReadOnlyChanged(EventArgs e)
    {
        base.OnReadOnlyChanged(e);

        if (!_darkModeEnabled) return;

        BackColor = DarkModeBackColor;
        ForeColor = DarkModeForeColor;
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!_darkModeEnabled) return;

        FixBackColors();
    }

    private void FixBackColors()
    {
        if (!Enabled)
        {
            // Flip enabled off and on again to fix disabled text color
            Native.SendMessageW(Handle, Native.WM_ENABLE, (IntPtr)1, IntPtr.Zero);
            Native.SendMessageW(Handle, Native.WM_ENABLE, (IntPtr)0, IntPtr.Zero);
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg is Native.WM_CONTEXTMENU &&
            FindForm() is IDarkContextMenuOwner { ViewBlocked: true })
        {
            return;
        }

        if (!_darkModeEnabled)
        {
            base.WndProc(ref m);
            return;
        }

        // We still need this for selected text fore/back color. We can't find the exhaustive set of messages
        // that will make us ALWAYS have the proper selection back color, so just do it for all messages. Meh!
        // (except WM_ENABLE == false, because if we react to that then we get random wrong colors for various
        // textboxes when we're disabled)
        if (m.Msg != Native.WM_ENABLE || m.WParam.ToInt32() != 0)
        {
            using (new Win32ThemeHooks.OverrideSysColorScope(Win32ThemeHooks.Override.Full))
            {
                base.WndProc(ref m);
            }
        }

        if (m.Msg == Native.WM_PAINT)
        {
            using var gc = new Native.GraphicsContext(Handle);
            // ClientSize to draw correctly for multiline textboxes with scroll bar(s)
            gc.G.DrawRectangle(DarkColors.LightBorderPen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }
    }
}

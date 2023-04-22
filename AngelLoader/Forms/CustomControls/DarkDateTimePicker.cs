using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkDateTimePicker : DateTimePicker, IDarkable
{
    private bool _mouseOverButton;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            if (_darkModeEnabled)
            {
                Native.SetWindowTheme(Handle, "", "");
            }
            else
            {
                // I can't get SetWindowTheme() to work for resetting the theme back to normal, but recreating
                // the handle does the job.
                RecreateHandle();
            }

            Invalidate();
        }
    }

    private (Native.DATETIMEPICKERINFO DateTimePickerInfo, Rectangle ButtonRectangle)
    GetDTPInfoAndButtonRect()
    {
        var dtpInfo = new Native.DATETIMEPICKERINFO { cbSize = Marshal.SizeOf(typeof(Native.DATETIMEPICKERINFO)) };
        Native.SendMessage(Handle, Native.DTM_GETDATETIMEPICKERINFO, IntPtr.Zero, ref dtpInfo);

        return (dtpInfo, dtpInfo.rcButton.ToRectangle());
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_darkModeEnabled) return;

        var (_, buttonRect) = GetDTPInfoAndButtonRect();
        bool newMouseOverButton = buttonRect.Contains(this.ClientCursorPos());

        if (newMouseOverButton != _mouseOverButton)
        {
            _mouseOverButton = newMouseOverButton;
            using var gc = new Native.GraphicsContext(Handle);
            DrawButton(gc.G);
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (!_darkModeEnabled) return;

        if (_mouseOverButton)
        {
            _mouseOverButton = false;
            using var gc = new Native.GraphicsContext(Handle);
            DrawButton(gc.G);
        }
    }

    private void DrawButton(Graphics g)
    {
        var (dtpInfo, buttonRect) = GetDTPInfoAndButtonRect();

        SolidBrush buttonBrush =
            (dtpInfo.stateButton & Native.STATE_SYSTEM_PRESSED) != 0
                ? DarkColors.DarkBackgroundBrush
                : _mouseOverButton
                    ? DarkColors.LighterBackgroundBrush
                    : DarkColors.LightBackgroundBrush;

        g.FillRectangle(buttonBrush, buttonRect);

        Images.PaintArrow7x4(g, Direction.Down, buttonRect, Enabled);
    }

    private void PaintCustom()
    {
        using var gc = new Native.GraphicsContext(Handle);

        gc.G.DrawRectangle(DarkColors.LightBorderPen, 0, 0, Width - 1, Height - 1);
        gc.G.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, 1, 1, Width - 3, Height - 3);

        DrawButton(gc.G);
    }

    protected override void WndProc(ref Message m)
    {
        if (!_darkModeEnabled)
        {
            base.WndProc(ref m);
            return;
        }

        switch (m.Msg)
        {
            // @DarkModeNote(DateTimePicker): Still flickers the classic border somewhat on move/resize
            // Not the end of the world, but if we find a quick way to fix it, we should do it. Otherwise,
            // we'll just call it done.
            case Native.WM_PAINT:
                using (new Win32ThemeHooks.OverrideSysColorScope(Win32ThemeHooks.Override.Full))
                {
                    base.WndProc(ref m);
                }
                PaintCustom();
                break;
            case Native.WM_NCPAINT:
                // Attempt to reduce flicker (only reduces the chance very slightly)
                // NOTE: I don't think this does anything at all actually
                PaintCustom();
                break;
            default:
                base.WndProc(ref m);
                break;
        }
    }
}

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
        Native.DATETIMEPICKERINFO dtpInfo = new() { cbSize = Marshal.SizeOf(typeof(Native.DATETIMEPICKERINFO)) };
        Native.SendMessageW(Handle, Native.DTM_GETDATETIMEPICKERINFO, 0, ref dtpInfo);

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
            using Native.GraphicsContext gc = new(Handle);
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
            using Native.GraphicsContext gc = new(Handle);
            DrawButton(gc.G);
        }
    }

    private void DrawButton(Graphics g, Point? offset = null)
    {
        var (dtpInfo, buttonRect) = GetDTPInfoAndButtonRect();

        SolidBrush buttonBrush =
            (dtpInfo.stateButton & Native.STATE_SYSTEM_PRESSED) != 0
                ? DarkColors.DarkBackgroundBrush
                : _mouseOverButton
                    ? DarkColors.LighterBackgroundBrush
                    : DarkColors.LightBackgroundBrush;

        if (offset != null)
        {
            buttonRect = buttonRect with
            {
                X = buttonRect.X + offset.Value.X,
                Y = buttonRect.Y + offset.Value.Y,
            };
        }

        g.FillRectangle(buttonBrush, buttonRect);

        Images.PaintArrow7x4(g, Direction.Down, buttonRect, Enabled);
    }

    // This thing's theme doesn't get fully captured in the control DrawToBitmap() image, so we also use this
    // method to paint the themed visuals directly onto the image after the fact.
    public void PaintCustom(Graphics? g = null, Point? offset = null)
    {
        if (g == null)
        {
            using Native.GraphicsContext gc = new(Handle);
            PaintCustomInternal(gc.G);
        }
        else if (offset != null)
        {
            PaintCustomInternal(g, offset);
        }
    }

    private void PaintCustomInternal(Graphics g, Point? offset = null)
    {
        int x1, y1, x2, y2;
        if (offset != null)
        {
            x1 = offset.Value.X;
            y1 = offset.Value.Y;
            x2 = offset.Value.X + 1;
            y2 = offset.Value.Y + 1;
        }
        else
        {
            x1 = 0;
            y1 = 0;
            x2 = 1;
            y2 = 1;
        }

        g.DrawRectangle(DarkColors.LightBorderPen, x1, y1, Width - 1, Height - 1);
        g.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, x2, y2, Width - 3, Height - 3);

        DrawButton(g, offset);
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
            case Native.WM_PRINT:
            case Native.WM_PRINTCLIENT:
            {
                using (new Win32ThemeHooks.OverrideSysColorScope(Win32ThemeHooks.Override.Full))
                {
                    base.WndProc(ref m);
                }
                PaintCustom();
                break;
            }
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
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

            return (dtpInfo, new Rectangle(
                dtpInfo.rcButton.left,
                dtpInfo.rcButton.top,
                dtpInfo.rcButton.right - dtpInfo.rcButton.left,
                dtpInfo.rcButton.bottom - dtpInfo.rcButton.top
            ));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_darkModeEnabled) return;

            var (_, buttonRect) = GetDTPInfoAndButtonRect();
            bool newMouseOverButton = buttonRect.Contains(this.PointToClient_Fast(Native.GetCursorPosition_Fast()));

            if (newMouseOverButton != _mouseOverButton)
            {
                _mouseOverButton = newMouseOverButton;
                DrawButton();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!_darkModeEnabled) return;

            if (_mouseOverButton)
            {
                _mouseOverButton = false;
                DrawButton();
            }
        }

        private void DrawButton(Graphics? g = null)
        {
            Native.GraphicsContext? gc = null;

            if (g == null)
            {
                gc = new Native.GraphicsContext(Handle);
                g = gc.G;
            }

            try
            {
                var (dtpInfo, buttonRect) = GetDTPInfoAndButtonRect();

                SolidBrush buttonBrush =
                    (dtpInfo.stateButton & Native.STATE_SYSTEM_PRESSED) != 0
                        ? DarkColors.DarkBackgroundBrush
                        : _mouseOverButton
                        ? DarkColors.LighterBackgroundBrush
                        : DarkColors.LightBackgroundBrush;

                g.FillRectangle(buttonBrush, buttonRect);

                Images.PaintArrow7x4(g, Misc.Direction.Down, buttonRect, Enabled);
            }
            finally
            {
                gc?.Dispose();
            }
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
                    // We have to override global colors for this, and we have no proper way to only override them
                    // for this one control specifically, so this is the best we can do. This prevents the colors
                    // from being changed for other controls (stock MessageBoxes, for one).
                    Win32ThemeHooks.SysColorOverride = Win32ThemeHooks.Override.Full;
                    base.WndProc(ref m);
                    Win32ThemeHooks.SysColorOverride = Win32ThemeHooks.Override.None;
                    PaintCustom();
                    break;
                case Native.WM_NCPAINT:
                    // Attempt to reduce flicker (only reduces the chance very slightly)
                    PaintCustom();
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
    }
}

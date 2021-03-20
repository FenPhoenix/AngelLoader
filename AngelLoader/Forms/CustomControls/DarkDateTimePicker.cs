using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkDateTimePicker : DateTimePicker, IDarkable
    {
        private bool _mouseOverButton;

        private bool _darkModeEnabled;
        public bool DarkModeEnabled
        {
            get => _darkModeEnabled;
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
            bool newMouseOverButton = buttonRect.Contains(PointToClient(Cursor.Position));

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
            Native.DeviceContext? dc = null;

            bool disposeGraphics = false;

            if (g == null)
            {
                dc = new Native.DeviceContext(Handle);
                g = Graphics.FromHdc(dc.DC);
                disposeGraphics = true;
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

                ControlPainter.PaintArrow7x4(g, Misc.Direction.Down, buttonRect, Enabled);
            }
            finally
            {
                if (disposeGraphics) g.Dispose();
                dc?.Dispose();
            }
        }

        private void PaintCustom()
        {
            using Native.DeviceContext dc = new Native.DeviceContext(Handle);
            using Graphics g = Graphics.FromHdc(dc.DC);

            g.DrawRectangle(DarkColors.LightBorderPen, 0, 0, Width - 1, Height - 1);
            g.DrawRectangle(DarkColors.Fen_ControlBackgroundPen, 1, 1, Width - 3, Height - 3);

            DrawButton(g);
        }

        protected override void WndProc(ref Message m)
        {
            if (!_darkModeEnabled)
            {
                base.WndProc(ref m);
                return;
            }

            // TODO: @DarkMode(DateTimePicker): Still flickers the classic border somewhat on move/resize
            // Not the end of the world, but if we find a quick way to fix it, we should do it. Otherwise, we'll
            // just call it done.
            if (m.Msg == Native.WM_PAINT)
            {
                // We have to override global colors for this, and we have no proper way to only override them
                // for this one control specifically, so this is the best we can do. This prevents the colors
                // from being changed for other controls (stock MessageBoxes, for one).
                NativeHooks.OverrideColorsForDateTimePicker = true;
                base.WndProc(ref m);
                NativeHooks.OverrideColorsForDateTimePicker = false;
                PaintCustom();
            }
            else if (m.Msg == Native.WM_NCPAINT)
            {
                // Attempt to reduce flicker (only reduces the chance very slightly)
                PaintCustom();
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DTP_Subclass_Test : DateTimePicker
    {
        // TODO: @DarkMode(DateTimePicker): Only redraw the button on mousemove when its state has changed (to prevent flicker)

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Native.SetWindowTheme(Handle, "", "");
        }

        public DTP_Subclass_Test()
        {
            //SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        }

        private bool _mouseOverButton;

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
            var (_, buttonRect) = GetDTPInfoAndButtonRect();
            _mouseOverButton = buttonRect.Contains(PointToClient(Cursor.Position));

            using Native.DeviceContext dc = new Native.DeviceContext(Handle);
            using Graphics g = Graphics.FromHdc(dc.DC);

            DrawButton(g);

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _mouseOverButton = false;

            DrawButton();

            base.OnMouseLeave(e);
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

        // TODO: @DarkMode(DateTimePicker): Paint over the Win98-looking unthemed parts (border, button, etc.)

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Native.WM_PAINT)
            {
                base.WndProc(ref m);
                PaintCustom();
            }
            else if (m.Msg == Native.WM_NCPAINT)
            {
                PaintCustom();
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.WinAPI;

namespace AngelLoader.Forms.CustomControls
{
    public class DarkForm : Form
    {
        private bool _loading = true;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _loading = false;
            Refresh();
        }

        protected override void WndProc(ref Message m)
        {
            // Cover up the flash of bright/half-drawn controls on startup when in dark mode
            if (_loading &&
                Misc.Config.DarkMode &&
                IsHandleCreated &&
                (m.Msg == Native.WM_PAINT
                 || m.Msg == Native.WM_SIZE
                 || m.Msg == Native.WM_MOVE
                 || m.Msg == Native.WM_WINDOWPOSCHANGED
                 || m.Msg == Native.WM_ERASEBKGND
                ))
            {
                using var dc = new Native.DeviceContext(Handle);
                using Graphics g = Graphics.FromHdc(dc.DC);

                g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, new Rectangle(0, 0, Width, Height));

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
    }
}

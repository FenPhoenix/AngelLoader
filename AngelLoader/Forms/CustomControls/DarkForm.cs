using System;
using System.Drawing;
using System.Windows.Forms;
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

            if (!Misc.Config.DarkMode) return;

            Refresh();
            // Explicitly refresh non-client area - otherwise on Win7 the non-client area doesn't refresh and we
            // end up with blacked-out title bar and borders etc.
            Native.SendMessage(Handle, Native.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
        }

        protected override void WndProc(ref Message m)
        {
            // Cover up the flash of bright/half-drawn controls on startup when in dark mode
            if (_loading &&
                Misc.Config.DarkMode &&
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
    }
}

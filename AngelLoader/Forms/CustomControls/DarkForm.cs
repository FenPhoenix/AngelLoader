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
            Refresh();
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
                // On Windows 7 (at least in a VM with Aero Glass disabled), our non-client area (title bar etc.)
                // remains blacked-out even after refresh here, which severely breaks windows visually. So use
                // client area only.
                using var gc = new Native.GraphicsContext(Handle, clientOnly: true);

                gc.G.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, new Rectangle(0, 0, Width, Height));

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

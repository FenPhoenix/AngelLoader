using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    public class TransparentPanel : Panel
    {
        private const int WS_EX_TRANSPARENT = 0x20;

        #region For future use

        [DllImport("user32.dll")]
        private static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

        #endregion

        public TransparentPanel() => SetStyle(ControlStyles.Opaque, true);

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // NOTE: WS_EX_TRANSPARENT will have problems if I ever host the RTFBox in a separate process
                cp.ExStyle |= WS_EX_TRANSPARENT;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(Color.FromArgb(0, BackColor)))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
}

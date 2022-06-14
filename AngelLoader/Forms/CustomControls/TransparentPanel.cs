using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class TransparentPanel : Panel
    {
        private const int WS_EX_TRANSPARENT = 0x20;

        private readonly SolidBrush _transparentBrush;

        public TransparentPanel()
        {
            SetStyle(ControlStyles.Opaque, true);
            _transparentBrush = new SolidBrush(Color.FromArgb(0, BackColor));
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // WS_EX_TRANSPARENT will have problems if I ever host the RTFBox in a separate process
                cp.ExStyle |= WS_EX_TRANSPARENT;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(_transparentBrush, ClientRectangle);
            base.OnPaint(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _transparentBrush.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

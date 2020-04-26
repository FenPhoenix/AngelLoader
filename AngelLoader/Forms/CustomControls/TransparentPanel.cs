using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    public sealed class TransparentPanel : Panel
    {
        private const int WS_EX_TRANSPARENT = 0x20;

        private readonly SolidBrush TransparentBrush;

        public TransparentPanel()
        {
            SetStyle(ControlStyles.Opaque, true);
            TransparentBrush = new SolidBrush(Color.FromArgb(0, BackColor));
        }

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
            e.Graphics.FillRectangle(TransparentBrush, ClientRectangle);
            base.OnPaint(e);
        }
    }
}

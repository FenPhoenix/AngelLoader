using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    public class TransparentPanel : Panel
    {
        private const int WS_EX_TRANSPARENT = 0x20;

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

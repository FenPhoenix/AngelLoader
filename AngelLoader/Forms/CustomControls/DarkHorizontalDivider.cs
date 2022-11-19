using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkHorizontalDivider : Panel, IDarkable
    {
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (DarkModeEnabled)
            {
                g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, ClientRectangle);
                int y = ClientSize.Height / 2;
                g.DrawLine(DarkColors.LighterBorderPen, 0, y, ClientSize.Width, y);
            }
            else
            {
                base.OnPaint(e);

                Pen s1Pen = Images.Sep1Pen;
                Pen s2Pen = Images.Sep2Pen;
                int y = ClientSize.Height / 2;
                g.DrawLine(s1Pen, 0, y, ClientSize.Width - 1, y);
                g.DrawLine(s2Pen, 1, y + 1, ClientSize.Width, y + 1);
            }
        }
    }
}

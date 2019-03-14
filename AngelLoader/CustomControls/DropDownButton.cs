using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.CustomControls
{
    public class DropDownButton : Button
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int arrowX = (ClientRectangle.Width / 2) - 3;
            int arrowY = (ClientRectangle.Height / 2) - 1;

            Point[] arrowPolygon =
            {
                new Point(arrowX, arrowY),
                new Point(arrowX + 7, arrowY),
                new Point(arrowX + 3, arrowY + 4)
            };

            var brush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ControlDark;
            e.Graphics.FillPolygon(brush, arrowPolygon);
        }
    }
}

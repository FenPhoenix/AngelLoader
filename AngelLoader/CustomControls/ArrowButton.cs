using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.CustomControls
{
    public class ArrowButton : Button
    {
        private Direction _arrowDirection;

        // Public for the designer
        [Browsable(true)]
        public Direction ArrowDirection
        {
            get => _arrowDirection;
            set
            {
                _arrowDirection = value;
                Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int arrowX;
            int arrowY;
            Point[] arrowPolygon;

            switch (ArrowDirection)
            {
                case Direction.Left:
                    arrowX = (ClientRectangle.Width / 2) + 2;
                    arrowY = (ClientRectangle.Height / 2) - 4;
                    arrowPolygon = new[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX, arrowY + 7),
                        new Point(arrowX - 4, arrowY + 3)
                    };
                    break;
                case Direction.Right:
                    arrowX = (ClientRectangle.Width / 2) - 2;
                    arrowY = (ClientRectangle.Height / 2) - 4;
                    arrowPolygon = new[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX, arrowY + 7),
                        new Point(arrowX + 4, arrowY + 3)
                    };
                    break;
                case Direction.Up:
                    arrowX = (ClientRectangle.Width / 2) - 3;
                    arrowY = (ClientRectangle.Height / 2) + 1;
                    arrowPolygon = new[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX + 7, arrowY),
                        new Point(arrowX + 3, arrowY - 4)
                    };
                    break;
                case Direction.Down:
                default:
                    arrowX = (ClientRectangle.Width / 2) - 3;
                    arrowY = (ClientRectangle.Height / 2) - 1;
                    arrowPolygon = new[]
                    {
                        new Point(arrowX, arrowY),
                        new Point(arrowX + 7, arrowY),
                        new Point(arrowX + 3, arrowY + 4)
                    };
                    break;
            }

            var brush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ControlDark;
            e.Graphics.FillPolygon(brush, arrowPolygon);
        }
    }
}

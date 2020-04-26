using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static AngelLoader.Misc;

namespace AngelLoader.CustomControls
{
    public class ArrowButton : Button
    {
        private Direction _arrowDirection;
        private readonly Point[] _arrowPolygon = new Point[3];

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

            switch (ArrowDirection)
            {
                case Direction.Left:
                    arrowX = (ClientRectangle.Width / 2) + 2;
                    arrowY = (ClientRectangle.Height / 2) - 4;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY - 1);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX, arrowY + 7);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX - 4, arrowY + 3);

                    break;
                case Direction.Right:
                    arrowX = (ClientRectangle.Width / 2) - 2;
                    arrowY = (ClientRectangle.Height / 2) - 4;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY - 1);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX, arrowY + 7);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX + 4, arrowY + 3);

                    break;
                case Direction.Up:
                    arrowX = (ClientRectangle.Width / 2) - 3;
                    arrowY = (ClientRectangle.Height / 2) + 1;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX + 7, arrowY);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX + 3, arrowY - 4);

                    break;
                case Direction.Down:
                default:
                    arrowX = (ClientRectangle.Width / 2) - 3;
                    arrowY = (ClientRectangle.Height / 2) - 1;

                    (_arrowPolygon[0].X, _arrowPolygon[0].Y) = (arrowX, arrowY);
                    (_arrowPolygon[1].X, _arrowPolygon[1].Y) = (arrowX + 7, arrowY);
                    (_arrowPolygon[2].X, _arrowPolygon[2].Y) = (arrowX + 3, arrowY + 4);

                    break;
            }

            Brush brush = Enabled ? SystemBrushes.ControlText : SystemBrushes.ControlDark;
            e.Graphics.FillPolygon(brush, _arrowPolygon);
        }
    }
}

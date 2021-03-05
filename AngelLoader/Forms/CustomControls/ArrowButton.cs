using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ArrowButton : DarkButton
    {
        private Direction _arrowDirection;
        private readonly Point[] _arrowPolygon = new Point[3];

        // Public for the designer
        [Browsable(true)]
        [PublicAPI]
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
            ControlPainter.PaintArrow(
                g: e.Graphics,
                arrowPolygon: _arrowPolygon,
                direction: _arrowDirection,
                area: ClientRectangle,
                controlEnabled: Enabled
            );
        }
    }
}

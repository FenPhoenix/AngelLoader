using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;
using static AngelLoader.Misc;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class ArrowButton : DarkButton
    {
        private Direction _arrowDirection;

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
            ControlPainter.PaintArrow7x4(
                g: e.Graphics,
                direction: _arrowDirection,
                area: ClientRectangle,
                controlEnabled: Enabled
            );
        }
    }
}

using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkArrowButton : DarkButton
{
    private Direction _arrowDirection;

    [Browsable(true)]
    [PublicAPI]
    public Direction ArrowDirection
    {
        get => _arrowDirection;
        set
        {
            _arrowDirection = value;
            if (Visible) Refresh();
        }
    }

#if DEBUG

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override ImageLayout BackgroundImageLayout { get; set; }

#endif

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (DesignMode) return;
        Images.PaintArrow7x4(
            g: e.Graphics,
            direction: _arrowDirection,
            area: ClientRectangle,
            controlEnabled: Enabled
        );
    }
}

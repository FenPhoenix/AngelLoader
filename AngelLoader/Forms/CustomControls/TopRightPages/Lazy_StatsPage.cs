using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_StatsPage : UserControl
{
    public Lazy_StatsPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Images.DrawHorizDiv(e.Graphics, 6, 24, ClientSize.Width - 8);
    }
}

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

    private void Lazy_StatsPage_Paint(object sender, PaintEventArgs e)
    {
        Images.DrawHorizDiv(e.Graphics, 6, 24, ClientSize.Width - 8);
    }
}

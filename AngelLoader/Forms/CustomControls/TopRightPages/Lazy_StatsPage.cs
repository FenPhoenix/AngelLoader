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
}
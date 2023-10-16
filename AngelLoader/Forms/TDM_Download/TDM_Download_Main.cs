using System.Windows.Forms;

namespace AngelLoader.Forms;

public sealed partial class TDM_Download_Main : UserControl
{
    public TDM_Download_Main()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }
}

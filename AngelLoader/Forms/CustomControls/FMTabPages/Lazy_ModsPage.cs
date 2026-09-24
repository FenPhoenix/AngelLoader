namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ModsPage : UserControlCustom
{
    public Lazy_ModsPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }
}

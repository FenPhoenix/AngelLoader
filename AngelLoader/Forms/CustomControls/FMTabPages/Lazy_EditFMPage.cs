namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_EditFMPage : UserControlCustom
{
    public Lazy_EditFMPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }
}

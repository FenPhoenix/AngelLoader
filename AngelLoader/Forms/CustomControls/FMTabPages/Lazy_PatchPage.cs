namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_PatchPage : UserControlCustom
{
    public Lazy_PatchPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }
}

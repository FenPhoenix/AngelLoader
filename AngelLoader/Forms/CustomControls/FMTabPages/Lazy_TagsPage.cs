namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_TagsPage : UserControlCustom
{
    public Lazy_TagsPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }
}

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_CommentPage : UserControlCustom
{
    public Lazy_CommentPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }
}

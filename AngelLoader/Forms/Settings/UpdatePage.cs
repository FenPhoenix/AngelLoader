using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;

public sealed partial class UpdatePage : UserControlCustom, Interfaces.ISettingsPage
{
    public UpdatePage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;
}

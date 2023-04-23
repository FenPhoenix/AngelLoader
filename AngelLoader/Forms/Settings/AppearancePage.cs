using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;

public sealed partial class AppearancePage : UserControl, Interfaces.ISettingsPage
{
    public AppearancePage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        FMsListGroupBox.PaintCustom += FMsListGroupBox_PaintCustom;
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

    private void FMsListGroupBox_PaintCustom(object sender, PaintEventArgs e)
    {
        Images.DrawHorizDiv(e.Graphics, 8, 136, FMsListGroupBox.Width - 9);
        Images.DrawHorizDiv(e.Graphics, 8, 264, FMsListGroupBox.Width - 9);
        Images.DrawHorizDiv(e.Graphics, 8, 408, FMsListGroupBox.Width - 9);
        Images.DrawHorizDiv(e.Graphics, 8, 584, FMsListGroupBox.Width - 9);
    }
}

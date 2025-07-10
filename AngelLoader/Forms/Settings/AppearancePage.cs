using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;

public sealed partial class AppearancePage : UserControlCustom, Interfaces.ISettingsPage
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
        Images.DrawHorizDiv(e.Graphics, 8, SortingLabel.Top - 20, FMsListGroupBox.Width - 9);
        Images.DrawHorizDiv(e.Graphics, 8, RatingDisplayStyleLabel.Top - 20, FMsListGroupBox.Width - 9);
        Images.DrawHorizDiv(e.Graphics, 8, DateFormatLabel.Top - 20, FMsListGroupBox.Width - 9);
        Images.DrawHorizDiv(e.Graphics, 8, RecentFMsHeaderLabel.Top - 20, FMsListGroupBox.Width - 9);
    }
}

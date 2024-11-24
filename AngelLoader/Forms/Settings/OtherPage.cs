using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;

public sealed partial class OtherPage : UserControl, Interfaces.ISettingsPage
{
    public OtherPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

    private void WebSearchUrlResetButton_Paint(object sender, PaintEventArgs e)
    {
        DarkButton button = (DarkButton)sender;
        Rectangle cr = button.ClientRectangle;
        Images.PaintBitmapButton(
            e: e,
            img: button.Enabled
                ? Images.Refresh
                : Images.Refresh_Disabled,
            scaledRect: new RectangleF(
                cr.X + 2f,
                cr.Y + 2f,
                cr.Width - 4f,
                cr.Height - 4f));
    }
}

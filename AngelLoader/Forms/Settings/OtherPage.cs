using System.Drawing;
using System.Windows.Forms;
using AL_Common;

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
        Rectangle cr = WebSearchUrlResetButton.ClientRectangle;
        Images.PaintBitmapButton(e,
            Images.Refresh,
            scaledRect: new RectangleF(
                cr.X + 2f,
                cr.Y + 2f,
                cr.Width - 4f,
                cr.Height - 4f));
    }
}

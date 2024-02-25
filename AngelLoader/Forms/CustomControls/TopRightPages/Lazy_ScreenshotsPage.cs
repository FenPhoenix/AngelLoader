using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage : UserControl
{
    public Lazy_ScreenshotsPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    private void GammaResetButton_PaintCustom(object sender, PaintEventArgs e)
    {
        Rectangle cr = ((DarkButton)sender).ClientRectangle;
        Images.PaintBitmapButton(e,
            Images.Refresh,
            scaledRect: new RectangleF(
                cr.X + 2f,
                cr.Y + 2f,
                cr.Width - 4f,
                cr.Height - 4f));
    }
}

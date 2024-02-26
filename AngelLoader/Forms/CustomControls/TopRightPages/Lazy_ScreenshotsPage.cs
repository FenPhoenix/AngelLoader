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

    private void RefreshButton_PaintCustom(object sender, PaintEventArgs e)
    {
        Rectangle cr = RefreshButton.ClientRectangle;
        Images.PaintBitmapButton(
            e,
            RefreshButton.Enabled ? Images.Refresh : Images.GetDisabledImage(Images.Refresh),
            scaledRect: new RectangleF(
                cr.X + 2f,
                cr.Y + 2f,
                cr.Width - 4f,
                cr.Height - 4f));
    }

    private void OpenScreenshotsFolderButton_PaintCustom(object sender, PaintEventArgs e)
    {
        Image image = Images.Folder;
        DarkButton button = OpenScreenshotsFolderButton;
        Images.PaintBitmapButton(
            button,
            e,
            button.Enabled ? image : Images.GetDisabledImage(image),
            x: (button.Width - image.Width) / 2);
    }
}

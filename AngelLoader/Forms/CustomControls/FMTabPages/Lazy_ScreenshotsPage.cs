using System.Drawing;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage : UserControlCustom
{
    public Lazy_ScreenshotsPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    private void OpenScreenshotsFolderButton_PaintCustom(object sender, PaintEventArgs e)
    {
        Image image = Images.FolderDim;
        DarkButton button = OpenScreenshotsFolderButton;
        Images.PaintBitmapButton(
            button,
            e,
            button.Enabled ? image : Images.GetDisabledImage(image),
            x: (button.Width - image.Width) / 2);
    }

    private void CopyButton_PaintCustom(object sender, PaintEventArgs e)
    {
        Image image = Images.Copy21;
        DarkButton button = CopyButton;
        Images.PaintBitmapButton(
            button,
            e,
            button.Enabled ? image : Images.GetDisabledImage(image),
            x: (button.Width - image.Width) / 2);
    }
}

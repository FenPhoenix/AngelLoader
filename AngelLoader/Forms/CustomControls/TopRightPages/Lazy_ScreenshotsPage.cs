using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;

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

        DeleteButton.DarkModeBackColor = DarkColors.Fen_ControlBackground;
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

    private void DeleteButton_PaintCustom(object sender, PaintEventArgs e)
    {
        Image image = Images.Trash;
        DarkButton button = DeleteButton;
        Images.PaintBitmapButton(
            button,
            e,
            button.Enabled ? image : Images.GetDisabledImage(image),
            x: (button.Width - image.Width) / 2);
    }
}

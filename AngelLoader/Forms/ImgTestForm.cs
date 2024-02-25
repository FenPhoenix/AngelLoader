/*
@ScreenshotDisplay: This form is a test, remove it later and bring this into the actual screenshot display UI area

@ScreenshotDisplay(Custom image box notes):
-This is slower to draw than PictureBox, but it's faster/trivial to change the gamma. With the PictureBox, we'd
 have to draw the bitmap onto another bitmap with the gamma adjust, then set the PictureBox.Image property again,
 with a full load time and all.
*/

using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using static AngelLoader.Forms.CustomControls.ScreenshotsTabPage;

namespace AngelLoader.Forms;

public sealed partial class ImgTestForm : Form
{
    public ImgTestForm()
    {
        InitializeComponent();
        ImageBox.Image = _currentScreenshotStream.Img;
    }

    private readonly MemoryImage _currentScreenshotStream = new(@"C:\Thief Games\Thief2-ND-T2Fix\FMs\Calendras_Legacy_v1a\screenshots\dump000.png");

    private void GammaTrackBar_Scroll(object sender, System.EventArgs e)
    {
        // @ScreenshotDisplay(Gamma slider): The clamp is a hack to prevent 0 which is invalid, polish it up later
        ImageBox.Gamma = ((GammaTrackBar.Maximum - GammaTrackBar.Value) * 0.10f).ClampToMin(0.01f);
    }

    private void CopyButton_Click(object sender, System.EventArgs e)
    {
        using Bitmap bmp = ImageBox.GetFinalBitmap();
        Clipboard.SetImage(bmp);
    }
}

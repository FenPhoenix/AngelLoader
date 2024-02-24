/*
@ScreenshotDisplay: This form is a test, remove it later and bring this into the actual screenshot display UI area

@ScreenshotDisplay(Custom image box notes):
-This is slower to draw than PictureBox, but it's faster/trivial to change the gamma. With the PictureBox, we'd
 have to draw the bitmap onto another bitmap with the gamma adjust, then set the PictureBox.Image property again,
 with a full load time and all.
*/

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AL_Common;
using static AngelLoader.Forms.CustomControls.ScreenshotsTabPage;

namespace AngelLoader.Forms;

public sealed partial class ImgTestForm : Form
{
    public ImgTestForm()
    {
        InitializeComponent();
    }

    private readonly MemoryImage _currentScreenshotStream = new(@"C:\Thief Games\Thief2-ND-T2Fix\FMs\Calendras_Legacy_v1a\screenshots\dump000.png");

    private readonly ImageAttributes _imageAttributes = new();

    private readonly Size _imageSize = new(2560, 1440);

    private readonly RectangleF _imageRect = new(0, 0, 2560, 1440);

    private float _gamma = 1.0f;

    private void ImageBox_Paint(object sender, PaintEventArgs e)
    {
        Images.FitRectInBounds(e.Graphics, _imageRect, ImageBox.Bounds);

        _imageAttributes.SetGamma(_gamma, ColorAdjustType.Bitmap);

        DrawImageOnGraphics(e.Graphics);
    }

    private void DrawImageOnGraphics(Graphics g)
    {
        g.DrawImage(
            _currentScreenshotStream.Img,
            new Rectangle(0, 0, _imageSize.Width, _imageSize.Height),
            0,
            0,
            _imageSize.Width,
            _imageSize.Height,
            GraphicsUnit.Pixel,
            _imageAttributes
        );
    }

    private Bitmap GetFinalBitmap()
    {
        Bitmap bmp = new(
            _currentScreenshotStream.Img.Width,
            _currentScreenshotStream.Img.Height,
            _currentScreenshotStream.Img.PixelFormat);

        using var g = Graphics.FromImage(bmp);

        DrawImageOnGraphics(g);

        return bmp;
    }

    private void GammaTrackBar_Scroll(object sender, System.EventArgs e)
    {
        // @ScreenshotDisplay(Gamma slider): The clamp is a hack to prevent 0 which is invalid, polish it up later
        _gamma = ((GammaTrackBar.Maximum - GammaTrackBar.Value) * 0.10f).ClampToMin(0.01f);
        ImageBox.Invalidate();
    }

    private void CopyButton_Click(object sender, System.EventArgs e)
    {
        using Bitmap bmp = GetFinalBitmap();
        Clipboard.SetImage(bmp);
    }
}

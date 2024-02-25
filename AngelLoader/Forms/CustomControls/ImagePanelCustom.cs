using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AL_Common;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class ImagePanelCustom : Panel, IDarkable
{
    [PublicAPI]
    public Color DrawnBackColor = SystemColors.Control;

    [PublicAPI]
    public Color DarkModeDrawnBackColor = DarkColors.Fen_ControlBackground;

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        get => _darkModeEnabled;
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            BackColor = _darkModeEnabled ? DarkModeDrawnBackColor : DrawnBackColor;

            if (_showingErrorImage)
            {
                SetImageInternal(Images.BrokenFile);
            }
        }
    }

    private bool _showingErrorImage;

    private readonly ImageAttributes _imageAttributes = new();

    private Image? _image;
    private Size _imageSize = Size.Empty;
    private RectangleF _imageRect = RectangleF.Empty;

    private float _gamma = 1.0f;

    public void SetGamma(float gamma)
    {
        _gamma = gamma.ClampToMin(0.01f);
        Invalidate();
    }

    public void SetImage(Image? image, float? gamma = null)
    {
        _showingErrorImage = false;
        SetImageInternal(image, gamma);
    }

    private void SetImageInternal(Image? image, float? gamma = null)
    {
        _image = image;

        if (gamma != null)
        {
            _gamma = (float)gamma;
        }

        if (_image != null)
        {
            _imageSize = _image.Size;
            _imageRect = new RectangleF(0, 0, _imageSize.Width, _imageSize.Height);
        }
        else
        {
            _imageSize = Size.Empty;
            _imageRect = RectangleF.Empty;
        }
        Invalidate();
    }

    public void SetErrorImage()
    {
        _showingErrorImage = true;
        SetImageInternal(Images.BrokenFile);
    }

    public ImagePanelCustom()
    {
        ResizeRedraw = true;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_image == null) return;

        if (_showingErrorImage)
        {
            e.Graphics.DrawImage(
                _image,
                (ClientSize.Width / 2) - (_image.Width / 2),
                (ClientSize.Height / 2) - (_image.Height / 2)
            );
        }
        else
        {
            Images.FitRectInBounds(e.Graphics, _imageRect, ClientRectangle);
            _imageAttributes.SetGamma(_gamma, ColorAdjustType.Bitmap);
            DrawImageOnGraphics(e.Graphics, _image);
        }
    }

    private void DrawImageOnGraphics(Graphics g, Image image)
    {
        g.DrawImage(
            image,
            new Rectangle(0, 0, _imageSize.Width, _imageSize.Height),
            0,
            0,
            _imageSize.Width,
            _imageSize.Height,
            GraphicsUnit.Pixel,
            _imageAttributes
        );
    }

    /// <summary>
    /// Returns a new bitmap with all effects applied (an exact visual copy) of the displayed image, or
    /// <see langword="null"/> if there is no image displayed.
    /// </summary>
    /// <returns></returns>
    public Bitmap? GetSnapshot()
    {
        if (_image == null || _showingErrorImage)
        {
            return null;
        }

        Bitmap bmp = new(
            _image.Width,
            _image.Height,
            _image.PixelFormat);

        using var g = Graphics.FromImage(bmp);

        DrawImageOnGraphics(g, _image);

        return bmp;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _imageAttributes.Dispose();
            _image = null;
        }

        base.Dispose(disposing);
    }
}

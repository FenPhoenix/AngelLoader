using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
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

    private readonly ImageAttributes _imageAttributes = new();
    private Size _imageSize = Size.Empty;
    private RectangleF _imageRect = RectangleF.Empty;
    private bool _showingErrorImage;

    private float _gamma = 1.0f;
    [PublicAPI]
    [Browsable(false)]
    [DefaultValue(1.0f)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Gamma
    {
        get => _gamma;
        set => _gamma = value;
    }

    private Image? _image;
    [PublicAPI]
    [Browsable(false)]
    [DefaultValue(1.0f)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Image? Image
    {
        get => _image;
        set
        {
            _showingErrorImage = false;
            SetImageInternal(value);
        }
    }

    private void SetImageInternal(Image? image)
    {
        _image = image;
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
            Images.FitRectInBounds(e.Graphics, _imageRect, Bounds);
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

    public Bitmap GetFinalBitmap()
    {
        if (_image == null)
        {
            return new Bitmap(0, 0, PixelFormat.Format32bppPArgb);
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

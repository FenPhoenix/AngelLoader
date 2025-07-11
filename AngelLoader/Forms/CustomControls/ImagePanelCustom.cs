﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class ImagePanelCustom : PanelCustom, IDarkable
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

            if (ShowingErrorImage)
            {
                SetImageInternal(Images.BrokenFile);
            }
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal bool ShowingErrorImage { get; private set; }

    private readonly ImageAttributes _imageAttributes = new();

    private Image? _image;
    private Size _imageSize = Size.Empty;
    private RectangleF _imageRect = RectangleF.Empty;

    private float _gamma = 1.0f;

    public void SetGamma(float gamma)
    {
        SetGammaNoRefresh(gamma);
        Invalidate();
    }

    private void SetGammaNoRefresh(float gamma) => _gamma = gamma.ClampToMin(0.01f);

    public void SetImage(Image? image, float? gamma = null)
    {
        ShowingErrorImage = false;
        SetImageInternal(image, gamma);
    }

    private void SetImageInternal(Image? image, float? gamma = null)
    {
        _image = image;

        if (gamma != null)
        {
            SetGammaNoRefresh((float)gamma);
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
        ShowingErrorImage = true;
        SetImageInternal(Images.BrokenFile);
    }

    public ImagePanelCustom()
    {
        ResizeRedraw = true;
        DoubleBuffered = true;
    }

    protected override void OnClick(EventArgs e)
    {
        // Force focus on click so we can allow Ctrl+C to copy the image only when the image is focused.
        // Clicks don't normally focus panel-type controls so we need this.
        Focus();
        base.OnClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_image == null) return;

        if (ShowingErrorImage)
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
        if (_image == null || ShowingErrorImage)
        {
            return null;
        }

        Bitmap bmp = new(
            _image.Width,
            _image.Height,
            _image.PixelFormat);

        using Graphics g = Graphics.FromImage(bmp);

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

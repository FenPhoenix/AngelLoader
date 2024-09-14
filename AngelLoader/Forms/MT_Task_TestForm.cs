using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using static AngelLoader.Forms.WinFormsNative.Native;

namespace AngelLoader.Forms;
public sealed partial class MT_Task_TestForm : Form
{
    private readonly Bitmap _progressBitmap;
    private readonly Timer _animTimer = new();

    private const int _barLength = 380;
    private const int _barHeight = 13;
    private const int _segmentLength = 26;

    public MT_Task_TestForm()
    {
        InitializeComponent();

        _animTimer.Interval = 16;
        _animTimer.Tick += _animTimer_Tick;
        _animTimer.Enabled = true;

        Bitmap sectionBmp = new(_segmentLength, _barHeight, PixelFormat.Format32bppPArgb);
        _progressBitmap = new Bitmap(_barLength + _segmentLength, _barHeight, PixelFormat.Format32bppPArgb);

        using Graphics sectionBmpG = Graphics.FromImage(sectionBmp);
        using Graphics finalBmpG = Graphics.FromImage(_progressBitmap);

        sectionBmpG.SmoothingMode = SmoothingMode.HighQuality;
        finalBmpG.SmoothingMode = SmoothingMode.HighQuality;

        PointF[] points =
        {
            new(0, 0),
            new(13, 0),
            new(26, 13),
            new(13, 13),
            new(0, 0),
        };

        sectionBmpG.FillPolygon(Brushes.Green, points);

        for (int i = 0; i < _barLength + _segmentLength; i += _segmentLength)
        {
            finalBmpG.DrawImageUnscaled(sectionBmp, i, 0);
        }

        pictureBox1.Image = _progressBitmap;
    }

    private int _animCounter = -_segmentLength;

    private void _animTimer_Tick(object sender, System.EventArgs e)
    {
        try
        {
            Invoke(() =>
            {
                if (_animCounter >= 0)
                {
                    _animCounter = -_segmentLength;
                }
                using var gc = new GraphicsContext_Ref(drawnPanel1.Handle);
                gc.G.Clear(drawnPanel1.BackColor);
                gc.G.DrawImageUnscaled(_progressBitmap, _animCounter, 0);
                _animCounter++;
            });
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _animTimer.Dispose();
        }
        base.Dispose(disposing);
    }
}

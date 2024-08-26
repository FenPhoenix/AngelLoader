using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class ProgressItem : Panel, IDarkable
{
    private readonly DarkLabel _label;
    private readonly DarkProgressBar _progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {

        }
        base.Dispose(disposing);
    }

    [PublicAPI]
    public Color DrawnBackColor = SystemColors.Control;

    [PublicAPI]
    public Color DarkModeDrawnBackColor = DarkColors.LightBackground;

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

            (Color fore, Color back) =
                _darkModeEnabled
                    // Use a lighter background to make it easy to see we're supposed to be in front and modal
                    ? (fore: DarkColors.LightText, back: DarkColors.LightBackground)
                    : (fore: SystemColors.ControlText, back: SystemColors.Control);

            ForeColor = fore;
            BackColor = back;

            _label.DarkModeEnabled = _darkModeEnabled;
            _progressBar.DarkModeEnabled = _darkModeEnabled;
        }
    }

    //protected override void OnPaint(PaintEventArgs e)
    //{
    //    using var brush = new SolidBrush(_darkModeEnabled ? DarkModeDrawnBackColor : DrawnBackColor);
    //    e.Graphics.FillRectangle(brush, ClientRectangle);
    //}

    public ProgressItem()
    {
        Size = new Size(400, 32);
        _label = new DarkLabel
        {
            AutoSize = true,
            Location = new Point(4, 4),
        };
        _progressBar = new DarkProgressBar
        {
            Location = new Point(4, 16),
            Size = new Size(400, 16),
        };
    }
}

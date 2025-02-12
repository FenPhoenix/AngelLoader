using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

/// <summary>
/// Regular Panels don't behave with their BackColors, so...
/// </summary>
public sealed class DrawnPanel : Panel, IDarkable
{
    [PublicAPI]
    public Color DrawnBackColor = SystemColors.Control;

    [PublicAPI]
    public Color DarkModeDrawnBackColor = DarkColors.Fen_ControlBackground;

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled { get; set; }

    // Cache visible state because calling Visible redoes the work even if the value is the same
    private bool _visibleCached = true;

    [PublicAPI]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new bool Visible
    {
        get => base.Visible;
        set
        {
            if (value == _visibleCached) return;
            _visibleCached = value;
            base.Visible = value;
        }
    }

    [PublicAPI]
    public new void Show()
    {
        if (_visibleCached) return;
        _visibleCached = true;
        base.Show();
    }

    [PublicAPI]
    public new void Hide()
    {
        if (!_visibleCached) return;
        _visibleCached = false;
        base.Hide();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using SolidBrush brush = new(DarkModeEnabled ? DarkModeDrawnBackColor : DrawnBackColor);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}

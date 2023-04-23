using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkRadioButtonCustom : DarkButton
{
    private bool _checked;

    public event EventHandler? CheckedChanged;

    public override bool DarkModeEnabled
    {
        set
        {
            base.DarkModeEnabled = value;
            SetCheckedVisualState();
        }
    }

    public DarkRadioButtonCustom()
    {
        DarkModeBackColor = DarkColors.Fen_ControlBackground;
        DarkModeHoverColor = DarkColors.Fen_DarkBackground;
        DarkModePressedColor = DarkColors.Fen_DarkBackground;

        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseDownBackColor = SystemColors.Window;
        FlatAppearance.MouseOverBackColor = SystemColors.Window;
        FlatStyle = FlatStyle.Flat;
    }

    private void SetCheckedVisualState()
    {
        DarkModeBackColor =
            _checked
                ? DarkColors.Fen_DarkBackground
                : DarkColors.Fen_ControlBackground;

        if (!_darkModeEnabled)
        {
            BackColor = _checked
                ? SystemColors.Window
                : SystemColors.Control;
        }

        // Needed to prevent background color sticking when unchecked sometimes
        Refresh();
    }

#if DEBUG

    [Browsable(false)]
    [DefaultValue(FlatStyle.Flat)]
    public new FlatStyle FlatStyle
    {
        get => base.FlatStyle;
        set => base.FlatStyle = value;
    }

#endif

    [Browsable(true)]
    [DefaultValue(false)]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked == value) return;

            _checked = value;
            SetCheckedVisualState();

            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // This is to handle keyboard "clicks"
    protected override void OnClick(EventArgs e)
    {
        Checked = true;
        base.OnClick(e);
    }

    // This is for mouse use, to give a snappier experience, we change on MouseDown
    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) Checked = true;
        base.OnMouseDown(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var rect = new Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
        e.Graphics.DrawRectangle(_darkModeEnabled ? DarkColors.LightTextPen : SystemPens.ControlText, rect);
    }
}

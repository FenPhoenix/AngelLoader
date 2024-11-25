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

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        DarkModeHoverColor = DarkColors.BlueBackground;
        DarkModePressedColor = DarkColors.BlueBackground;

        FlatAppearance.BorderSize = 0;
        FlatAppearance.MouseDownBackColor = DarkColors.SettingsButtonHighlight_Light;
        FlatAppearance.MouseOverBackColor = DarkColors.SettingsButtonHighlight_Light;
        FlatStyle = FlatStyle.Flat;

        BackColor = SystemColors.ButtonFace;
    }

    private void SetCheckedVisualState()
    {
        DarkModeBackColor =
            _checked
                ? DarkColors.BlueBackground
                : DarkColors.Fen_ControlBackground;

        if (!_darkModeEnabled)
        {
            BackColor = _checked
                ? DarkColors.SettingsButtonHighlight_Light
                : SystemColors.ButtonFace;
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

        Rectangle rect = new(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width - 1, ClientRectangle.Height - 1);

        Pen pen;
        if (_darkModeEnabled)
        {
            pen = (Focused && ShowFocusCues) || Checked
                ? DarkColors.BlueHighlightPen
                : _buttonState == DarkControlState.Hover
                    ? DarkColors.BlueHighlightPen
                    : DarkColors.GreySelectionPen;
        }
        else
        {
            if (Checked)
            {
                pen = DarkColors.SettingsButtonHighlightBorder_LightPen;
                e.Graphics.DrawRectangle(pen, Rectangle.Inflate(rect, -1, -1));
            }
            else
            {
                pen = _buttonState == DarkControlState.Hover
                    ? DarkColors.SettingsButtonHighlightBorder_LightPen
                    : SystemPens.ControlDarkDark;
            }
        }

        e.Graphics.DrawRectangle(pen, rect);
    }
}

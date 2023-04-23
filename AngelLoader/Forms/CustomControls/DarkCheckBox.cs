using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

// @DarkModeNote(DarkCheckBox):
// We could add support for putting the checkbox on the right-hand side if we feel like we need it
public sealed class DarkCheckBox : CheckBox, IDarkable
{
    #region Field Region

    private DarkControlState _controlState = DarkControlState.Normal;

    private bool _spacePressed;

    private const int _checkBoxSize = 12;

    #endregion

    [PublicAPI]
    public Color? DarkModeBackColor;

    [PublicAPI]
    public Color? DarkModeForeColor;

#if DEBUG

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseMnemonic
    {
        get => base.UseMnemonic;
        set => base.UseMnemonic = value;
    }

    [PublicAPI]
    [DefaultValue(true)]
    public new bool UseVisualStyleBackColor
    {
        get => base.UseVisualStyleBackColor;
        set => base.UseVisualStyleBackColor = value;
    }

#endif

    private bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;
            Invalidate();
        }
    }

    #region Constructor Region

    public DarkCheckBox()
    {
        // Always true
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        UseMnemonic = false;
        UseVisualStyleBackColor = true;
    }

    #endregion

    private void InvalidateIfDark()
    {
        if (_darkModeEnabled) Invalidate();
    }

    #region Method Region

    private void SetControlState(DarkControlState controlState)
    {
        if (_controlState != controlState)
        {
            _controlState = controlState;
            InvalidateIfDark();
        }
    }

    internal bool? ToNullableBool() => CheckState switch
    {
        CheckState.Checked => true,
        CheckState.Unchecked => false,
        _ => null
    };

    internal void SetFromNullableBool(bool? value) => CheckState = value switch
    {
        true => CheckState.Checked,
        false => CheckState.Unchecked,
        _ => CheckState.Indeterminate
    };

    #endregion

    #region Event Handler Region

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_darkModeEnabled) return;

        if (_spacePressed) return;

        SetControlState(e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)
            ? DarkControlState.Pressed
            : DarkControlState.Hover);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!_darkModeEnabled) return;

        if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
        {
            SetControlState(DarkControlState.Pressed);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!_darkModeEnabled) return;

        if (_spacePressed) return;

        SetControlState(DarkControlState.Normal);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (!_darkModeEnabled) return;

        if (_spacePressed) return;

        SetControlState(DarkControlState.Normal);
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);

        if (!_darkModeEnabled) return;

        if (_spacePressed) return;

        if (!ClientRectangle.Contains(this.ClientCursorPos()))
        {
            SetControlState(DarkControlState.Normal);
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);

        if (!_darkModeEnabled) return;

        InvalidateIfDark();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);

        if (!_darkModeEnabled) return;

        _spacePressed = false;

        SetControlState(ClientRectangle.Contains(this.ClientCursorPos())
            ? DarkControlState.Hover
            : DarkControlState.Normal);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!_darkModeEnabled) return;

        if (e.KeyCode == Keys.Space)
        {
            _spacePressed = true;
            SetControlState(DarkControlState.Pressed);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (!_darkModeEnabled) return;

        if (e.KeyCode == Keys.Space)
        {
            _spacePressed = false;

            SetControlState(ClientRectangle.Contains(this.ClientCursorPos())
                ? DarkControlState.Hover
                : DarkControlState.Normal);
        }
    }

    #endregion

    #region Paint Region

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnPaint(e);
            return;
        }

        Graphics g = e.Graphics;
        var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

        Color textColor = DarkColors.LightText;
        Pen borderPen = DarkColors.LightTextPen;
        SolidBrush fillBrush = DarkColors.LightTextBrush;

        if (Enabled)
        {
            if (AutoCheck && Focused)
            {
                borderPen = DarkColors.BlueHighlightPen;
                fillBrush = DarkColors.BlueHighlightBrush;
            }

            if (_controlState == DarkControlState.Hover)
            {
                borderPen = DarkColors.BlueHighlightPen;
                fillBrush = DarkColors.BlueSelectionBrush;
            }
            else if (_controlState == DarkControlState.Pressed)
            {
                borderPen = DarkColors.GreyHighlightPen;
                fillBrush = DarkColors.GreySelectionBrush;
            }
        }
        else
        {
            textColor = DarkColors.DisabledText;
            borderPen = DarkColors.GreyHighlightPen;
            fillBrush = DarkColors.GreySelectionBrush;
        }

        Color? parentBackColor = Parent?.BackColor;
        if (parentBackColor != null)
        {
            using var b = new SolidBrush(DarkModeBackColor ?? (Color)parentBackColor);
            g.FillRectangle(b, rect);
        }
        else
        {
            g.FillRectangle(DarkColors.GreyBackgroundBrush, rect);
        }

        var outlineBoxRect = new Rectangle(0, (rect.Height / 2) - (_checkBoxSize / 2), _checkBoxSize, _checkBoxSize);
        g.DrawRectangle(borderPen, outlineBoxRect);

        if (CheckState == CheckState.Checked)
        {
            // IMPORTANT! Stop removing this thing, IT NEEDS TO BE SEPARATE BECAUSE IT'S GOT A DIFFERENT WIDTH!
            using var checkMarkPen = new Pen(fillBrush, 1.6f);
            ControlUtils.DrawCheckMark(g, checkMarkPen, outlineBoxRect);
        }
        else if (CheckState == CheckState.Indeterminate)
        {
            var boxRect = new Rectangle(3, ((rect.Height / 2) - ((_checkBoxSize - 4) / 2)) + 1, _checkBoxSize - 5, _checkBoxSize - 5);
            g.FillRectangle(fillBrush, boxRect);
        }

        TextFormatFlags textFormatFlags =
            ControlUtils.GetTextAlignmentFlags(TextAlign) |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoClipping |
            TextFormatFlags.WordBreak;

        var textRect = new Rectangle(_checkBoxSize + 4, 0, rect.Width - _checkBoxSize, rect.Height);
        TextRenderer.DrawText(g, Text, Font, textRect, DarkModeForeColor ?? textColor, textFormatFlags);
    }

    #endregion
}

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

    private bool UsingCustomRendering => _darkModeEnabled || (
        WinVersion.Is11OrAbove &&
        ThreeState &&
        !Native.HighContrastEnabled());

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
        if (UsingCustomRendering) Invalidate();
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
        _ => null,
    };

    internal void SetFromNullableBool(bool? value) => CheckState = value switch
    {
        true => CheckState.Checked,
        false => CheckState.Unchecked,
        _ => CheckState.Indeterminate,
    };

    #endregion

    #region Event Handler Region

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!UsingCustomRendering) return;

        if (_spacePressed) return;

        SetControlState(e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)
            ? DarkControlState.Pressed
            : DarkControlState.Hover);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!UsingCustomRendering) return;

        if (e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location))
        {
            SetControlState(DarkControlState.Pressed);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!UsingCustomRendering) return;

        if (_spacePressed) return;

        SetControlState(DarkControlState.Normal);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (!UsingCustomRendering) return;

        if (_spacePressed) return;

        SetControlState(DarkControlState.Normal);
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);

        if (!UsingCustomRendering) return;

        if (_spacePressed) return;

        if (!ClientRectangle.Contains(this.ClientCursorPos()))
        {
            SetControlState(DarkControlState.Normal);
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);

        if (!UsingCustomRendering) return;

        InvalidateIfDark();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);

        if (!UsingCustomRendering) return;

        _spacePressed = false;

        SetControlState(ClientRectangle.Contains(this.ClientCursorPos())
            ? DarkControlState.Hover
            : DarkControlState.Normal);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!UsingCustomRendering) return;

        if (e.KeyCode == Keys.Space)
        {
            _spacePressed = true;
            SetControlState(DarkControlState.Pressed);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (!UsingCustomRendering) return;

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
        if (!UsingCustomRendering)
        {
            base.OnPaint(e);
            return;
        }

        bool usingLightMode =
            WinVersion.Is11OrAbove &&
            !_darkModeEnabled &&
            ThreeState &&
            !Native.HighContrastEnabled();

        Graphics g = e.Graphics;
        Rectangle rect = ClientRectangle;

        Color textColor;
        Pen borderPen;
        Brush fillBrush;

        if (Enabled)
        {
            textColor =
                usingLightMode
                    ? SystemColors.ControlText
                    : DarkColors.LightText;

            if (CheckState == CheckState.Unchecked)
            {
                switch (_controlState)
                {
                    case DarkControlState.Hover:
                        borderPen =
                            usingLightMode
                                ? SystemPens.Highlight
                                : DarkColors.Fen_HyperlinkPen;
                        fillBrush =
                            usingLightMode
                                ? SystemBrushes.Highlight
                                : DarkColors.Fen_HyperlinkBrush;
                        break;
                    case DarkControlState.Pressed:
                        borderPen =
                            usingLightMode
                                ? DarkColors.CheckBoxPressedBorderPen
                                : DarkColors.GreyHighlightPen;
                        fillBrush =
                            usingLightMode
                                ? DarkColors.CheckBoxPressedFillBrush
                                : DarkColors.GreyHighlightBrush;
                        break;
                    default:
                        borderPen =
                            usingLightMode
                                ? SystemPens.ControlText
                                : DarkColors.DisabledTextPen;
                        fillBrush =
                            usingLightMode
                                ? SystemBrushes.ControlText
                                : DarkColors.DisabledTextBrush;
                        break;
                }
            }
            else
            {
                switch (_controlState)
                {
                    case DarkControlState.Hover:
                        borderPen =
                            usingLightMode
                                ? DarkColors.Win11_LightMode_CheckBox_CheckedBackground_HoverPen
                                : DarkColors.BlueHighlightPen;
                        fillBrush =
                            usingLightMode
                                ? DarkColors.Win11_LightMode_CheckBox_CheckedBackground_HoverBrush
                                : DarkColors.BlueHighlightBrush;
                        break;
                    case DarkControlState.Pressed:
                        borderPen =
                            usingLightMode
                                ? DarkColors.Win11_LightMode_CheckBox_CheckedBackground_PressedPen
                                : DarkColors.BlueSelectionPen;
                        fillBrush =
                            usingLightMode
                                ? DarkColors.Win11_LightMode_CheckBox_CheckedBackground_PressedBrush
                                : DarkColors.BlueSelectionBrush;
                        break;
                    default:
                        borderPen =
                            usingLightMode
                                ? DarkColors.Win11_LightMode_CheckBox_CheckedBackground_NormalPen
                                : DarkColors.Fen_HyperlinkPen;
                        fillBrush =
                            usingLightMode
                                ? DarkColors.Win11_LightMode_CheckBox_CheckedBackground_NormalBrush
                                : DarkColors.Fen_HyperlinkBrush;
                        break;
                }
            }
        }
        else
        {
            textColor = DarkColors.DisabledText;
            borderPen =
                usingLightMode
                    ? DarkColors.DisabledTextPen
                    : DarkColors.GreySelectionPen;
            fillBrush =
                usingLightMode
                    ? DarkColors.DisabledTextBrush
                    : DarkColors.GreySelectionBrush;
        }

        Color? parentBackColor = Parent?.BackColor;
        if (parentBackColor != null)
        {
            using var b =
                usingLightMode
                    ? new SolidBrush((Color)parentBackColor)
                    : new SolidBrush(DarkModeBackColor ?? (Color)parentBackColor);
            g.FillRectangle(b, rect);
        }
        else
        {
            g.FillRectangle(DarkColors.GreyBackgroundBrush, rect);
        }

        var outlineBoxRect = new Rectangle(0, (rect.Height / 2) - (_checkBoxSize / 2), _checkBoxSize, _checkBoxSize);

        if (CheckState == CheckState.Checked)
        {
            g.DrawRectangle(borderPen, outlineBoxRect);
            g.FillRectangle(fillBrush, outlineBoxRect);

            // IMPORTANT! Stop removing this thing, IT NEEDS TO BE SEPARATE BECAUSE IT'S GOT A DIFFERENT WIDTH!
            Color penColor =
                usingLightMode
                    ? SystemColors.Window
                    : DarkColors.Fen_DarkBackground;
            using var checkMarkPen = new Pen(penColor, 1.6f);
            ControlUtils.DrawCheckMark(g, checkMarkPen, outlineBoxRect);
        }
        else if (CheckState == CheckState.Indeterminate)
        {
            var boxRect =
                usingLightMode
                    ? new Rectangle(
                        4,
                        ((rect.Height / 2) - ((_checkBoxSize - 5) / 2)) + 1,
                        _checkBoxSize - 7,
                        _checkBoxSize - 7)
                    : new Rectangle(
                        3,
                        ((rect.Height / 2) - ((_checkBoxSize - 4) / 2)) + 1,
                        _checkBoxSize - 5,
                        _checkBoxSize - 5);

            g.DrawRectangle(borderPen, outlineBoxRect);
            g.FillRectangle(fillBrush, outlineBoxRect);

            Brush brush =
                usingLightMode
                    ? SystemBrushes.Window
                    : DarkColors.Fen_DarkBackgroundBrush;

            g.FillRectangle(brush, boxRect);
        }
        else
        {
            g.DrawRectangle(borderPen, outlineBoxRect);
        }

        TextFormatFlags textFormatFlags =
            ControlUtils.GetTextAlignmentFlags(TextAlign) |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoClipping |
            TextFormatFlags.WordBreak;

        var textRect = new Rectangle(_checkBoxSize + 4, 0, rect.Width - _checkBoxSize, rect.Height);
        TextRenderer.DrawText(g, Text, Font, textRect, DarkModeForeColor ?? textColor, textFormatFlags);

        if (Focused && ShowFocusCues)
        {
            ControlUtils.DrawFocusRectangle(
                this,
                e.Graphics,
                ClientRectangle,
                parentBackColor ?? BackColor);
        }
    }

    #endregion
}

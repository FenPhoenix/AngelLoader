using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkRadioButton : RadioButton, IDarkable
{
    #region Field Region

    private DarkControlState _controlState = DarkControlState.Normal;

    private bool _spacePressed;

    private const int _radioButtonSize = 12;

    #endregion

    #region Property Region

#if DEBUG

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Appearance Appearance => base.Appearance;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool AutoEllipsis => base.AutoEllipsis;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Image? BackgroundImage => base.BackgroundImage;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ImageLayout BackgroundImageLayout => base.BackgroundImageLayout;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool FlatAppearance => false;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new FlatStyle FlatStyle => base.FlatStyle;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Image? Image => base.Image;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ContentAlignment ImageAlign => base.ImageAlign;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new int ImageIndex => base.ImageIndex;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new string ImageKey => base.ImageKey;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ImageList? ImageList => base.ImageList;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ContentAlignment TextAlign => base.TextAlign;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new TextImageRelation TextImageRelation => base.TextImageRelation;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseCompatibleTextRendering => false;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseMnemonic { get => false; set => base.UseMnemonic = false; }

    [PublicAPI]
    [DefaultValue(true)]
    public new bool UseVisualStyleBackColor
    {
        get => base.UseVisualStyleBackColor;
        set => base.UseVisualStyleBackColor = value;
    }

#endif

    #endregion

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

            UseVisualStyleBackColor = !_darkModeEnabled;

            Invalidate();
        }
    }

    #region Constructor Region

    public DarkRadioButton()
    {
        UseMnemonic = false;

        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);

        UseVisualStyleBackColor = true;
    }

    #endregion

    #region Method Region

    private void SetControlState(DarkControlState controlState)
    {
        if (_controlState != controlState)
        {
            _controlState = controlState;
            if (_darkModeEnabled) Invalidate();
        }
    }

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

        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);

        if (!_darkModeEnabled) return;

        _spacePressed = false;

        SetControlState(!ClientRectangle.Contains(this.ClientCursorPos())
            ? DarkControlState.Normal
            : DarkControlState.Hover);
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

        Color textColor;
        Pen borderColorPen;
        SolidBrush fillColorBrush;

        if (Enabled)
        {
            textColor = DarkColors.LightText;

            switch (_controlState)
            {
                case DarkControlState.Hover:
                    borderColorPen = DarkColors.Fen_HyperlinkPen;
                    fillColorBrush = DarkColors.Fen_HyperlinkBrush;
                    break;
                case DarkControlState.Pressed:
                    if (Checked)
                    {
                        borderColorPen = DarkColors.Fen_HyperlinkPen;
                        fillColorBrush = DarkColors.Fen_HyperlinkBrush;
                    }
                    else
                    {
                        borderColorPen = DarkColors.GreyHighlightPen;
                        fillColorBrush = DarkColors.GreyHighlightBrush;
                    }
                    break;
                default:
                    if (Checked)
                    {
                        borderColorPen = DarkColors.Fen_HyperlinkPen;
                        fillColorBrush = DarkColors.Fen_HyperlinkBrush;
                    }
                    else
                    {
                        borderColorPen = DarkColors.LightTextPen;
                        fillColorBrush = DarkColors.LightTextBrush;
                    }
                    break;
            }
        }
        else
        {
            textColor = DarkColors.DisabledText;
            borderColorPen = DarkColors.GreySelectionPen;
            fillColorBrush = DarkColors.GreySelectionBrush;
        }

        Color? parentBackColor = Parent?.BackColor;
        if (parentBackColor != null)
        {
            using var b = new SolidBrush((Color)parentBackColor);
            g.FillRectangle(b, ClientRectangle);
        }

        g.SmoothingMode = SmoothingMode.HighQuality;

        var boxRect = new Rectangle(0, (ClientRectangle.Height / 2) - (_radioButtonSize / 2), _radioButtonSize, _radioButtonSize);

        if (Checked)
        {
            var checkRect = _controlState switch
            {
                DarkControlState.Hover => new Rectangle(
                    2,
                    (ClientRectangle.Height / 2) - ((_radioButtonSize - 6) / 2) - 1,
                    _radioButtonSize - 4,
                    _radioButtonSize - 4),
                DarkControlState.Pressed => new RectangleF(
                    3.5f,
                    // ReSharper disable once RedundantCast
#pragma warning disable IDE0004
                    (int)((ClientRectangle.Height / 2) - ((_radioButtonSize - 8) / 2)) - 0.5f,
#pragma warning restore IDE0004
                    _radioButtonSize - 7,
                    _radioButtonSize - 7),
                _ => new Rectangle(
                    3,
                    (ClientRectangle.Height / 2) - ((_radioButtonSize - 7) / 2) - 1,
                    _radioButtonSize - 6,
                    _radioButtonSize - 6),
            };

            g.DrawEllipse(borderColorPen, boxRect);
            g.FillEllipse(fillColorBrush, boxRect);

            g.FillEllipse(DarkColors.GreyBackgroundBrush, checkRect);
        }
        else
        {
            g.DrawEllipse(borderColorPen, boxRect);
        }

        g.SmoothingMode = SmoothingMode.Default;

        TextFormatFlags textFormatFlags =
            ControlUtils.GetTextAlignmentFlags(TextAlign) |
            TextFormatFlags.NoClipping |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.WordBreak;

        var textRect = new Rectangle(_radioButtonSize + 4, 0, ClientRectangle.Width - _radioButtonSize, ClientRectangle.Height);
        TextRenderer.DrawText(g, Text, Font, textRect, textColor, textFormatFlags);

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

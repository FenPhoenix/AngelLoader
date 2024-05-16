using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

/*
@DarkModeNote(DarkButton): There was an _isDefault thing here where we'd draw differently if we were the default button, but:
It didn't work 100% like it was supposed to, in fact the original works weirdly and inconsistently too, but
ours broke in a worse way than original, so we just removed it completely. The original often doesn't draw
the default button any differently either (sometimes it does, often it doesn't), so we're not really any less
clear than the original. We still draw a selection around the focused button, which ends up being the default
button in dialogs, so we're as clear as the original there, which is the most important place to be clear.
*/

[ToolboxBitmap(typeof(Button))]
[DefaultEvent("Click")]
public class DarkButton : Button, IDarkable
{
    #region Field Region

    private DarkButtonStyle _style = DarkButtonStyle.Normal;

    private DarkControlState _buttonState = DarkControlState.Normal;

    private bool _spacePressed;

    private int _imagePadding = 5;

    private FlatStyle? _originalFlatStyle;
    private int? _originalBorderSize;

    #endregion

    private enum DarkButtonStyle
    {
        Normal,
        Flat,
    }

    #region Designer Property Region

    /// <summary>
    /// If this control represents a game in some way, you can set its <see cref="GameSupport.GameIndex"/> here.
    /// </summary>
    [PublicAPI]
    public GameSupport.GameIndex GameIndex = GameSupport.GameIndex.Thief1;

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
    [Browsable(false)]
    [DefaultValue(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseVisualStyleBackColor
    {
        get => base.UseVisualStyleBackColor;
        set => base.UseVisualStyleBackColor = value;
    }

#endif

    [PublicAPI]
    public Color? DarkModeBackColor;

    [PublicAPI]
    public Color? DarkModeHoverColor;

    [PublicAPI]
    public Color? DarkModePressedColor;

    [PublicAPI]
    public new string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            InvalidateIfDark();
        }
    }

    [PublicAPI]
    public new bool Enabled
    {
        get => base.Enabled;
        set
        {
            base.Enabled = value;
            InvalidateIfDark();
        }
    }

    private DarkButtonStyle ButtonStyle
    {
        get => _style;
        set
        {
            _style = value;
            InvalidateIfDark();
        }
    }

    [Category("Appearance")]
    [Description("Determines the amount of padding between the image and text.")]
    [DefaultValue(5)]
    [PublicAPI]
    public int ImagePadding
    {
        get => _imagePadding;
        set
        {
            _imagePadding = value;
            InvalidateIfDark();
        }
    }

    #endregion

    #region Code Property Region

    [PublicAPI]
    public new FlatStyle FlatStyle
    {
        get => base.FlatStyle;
        set
        {
            base.FlatStyle = value;
            ButtonStyle = value == FlatStyle.Flat
                ? DarkButtonStyle.Flat
                : DarkButtonStyle.Normal;
        }
    }

    #endregion

    private protected bool _darkModeEnabled;
    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public virtual bool DarkModeEnabled
    {
        set
        {
            if (_darkModeEnabled == value) return;
            _darkModeEnabled = value;

            // Everything needs to be just like this, or else there are cases where the appearance is wrong

            if (_darkModeEnabled)
            {
                _originalFlatStyle ??= base.FlatStyle;
                _originalBorderSize ??= FlatAppearance.BorderSize;
                UseVisualStyleBackColor = false;
                SetButtonState(DarkControlState.Normal);

                Invalidate();
            }
            else
            {
                // Need to set these explicitly because in some cases (not all) they don't get set back automatically
                ForeColor = SystemColors.ControlText;
                BackColor = SystemColors.Control;
                UseVisualStyleBackColor = true;
                if (_originalFlatStyle != null) base.FlatStyle = (FlatStyle)_originalFlatStyle;
                if (_originalBorderSize != null) FlatAppearance.BorderSize = (int)_originalBorderSize;
            }
        }
    }

    #region Constructor Region

    public DarkButton()
    {
        UseMnemonic = false;
        UseCompatibleTextRendering = false;
        UseVisualStyleBackColor = true;

        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }

    #endregion

    private void InvalidateIfDark()
    {
        if (_darkModeEnabled) Invalidate();
    }

    #region Method Region

    private void SetButtonState(DarkControlState buttonState)
    {
        if (_buttonState != buttonState)
        {
            _buttonState = buttonState;
            InvalidateIfDark();
        }
    }

    #endregion

    #region Event Handler Region

    // Need our own event because we can't fire base.OnPaint() or it overrides our own painting
    public event EventHandler<PaintEventArgs>? PaintCustom;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_darkModeEnabled) return;

        if (_spacePressed) return;

        SetButtonState(e.Button == MouseButtons.Left && ClientRectangle.Contains(e.Location)
            ? DarkControlState.Pressed
            : DarkControlState.Hover);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (!_darkModeEnabled) return;

        if (e.Button != MouseButtons.Left) return;

        if (!ClientRectangle.Contains(e.Location)) return;

        SetButtonState(DarkControlState.Pressed);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (!_darkModeEnabled) return;

        if (e.Button != MouseButtons.Left) return;

        if (_spacePressed) return;

        SetButtonState(DarkControlState.Normal);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);

        if (!_darkModeEnabled) return;

        if (_spacePressed) return;

        SetButtonState(DarkControlState.Normal);
    }

    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e);

        if (!_darkModeEnabled) return;

        if (_spacePressed) return;

        if (!ClientRectangle.Contains(this.ClientCursorPos()))
        {
            SetButtonState(DarkControlState.Normal);
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

        SetButtonState(!ClientRectangle.Contains(this.ClientCursorPos())
            ? DarkControlState.Normal
            : DarkControlState.Hover);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!_darkModeEnabled) return;

        if (e.KeyCode == Keys.Space)
        {
            _spacePressed = true;
            SetButtonState(DarkControlState.Pressed);
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (!_darkModeEnabled) return;

        if (e.KeyCode == Keys.Space)
        {
            _spacePressed = false;

            SetButtonState(!ClientRectangle.Contains(this.ClientCursorPos())
                ? DarkControlState.Normal
                : DarkControlState.Hover);
        }
    }

    #endregion

    #region Paint Region

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnPaint(e);
            PaintCustom?.Invoke(this, e);
            return;
        }

        Graphics g = e.Graphics;

        Rectangle rect = ButtonStyle == DarkButtonStyle.Normal
            // Slightly modified rectangle to account for Flat style being slightly larger than classic mode,
            // this matches us visually in size and position to classic mode
            ? new Rectangle(1, 1, ClientSize.Width - 2, ClientSize.Height - 3)
            : ClientRectangle;

        SolidBrush textColorBrush = DarkColors.LightTextBrush;
        Pen borderPen = DarkColors.GreySelectionPen;
        Color? fillColor = null;
        if (DarkModeBackColor != null) fillColor = DarkModeBackColor;

        if (Enabled)
        {
            if (ButtonStyle == DarkButtonStyle.Normal)
            {
                if (Focused && TabStop)
                {
                    borderPen = DarkColors.BlueHighlightPen;
                }

                switch (_buttonState)
                {
                    case DarkControlState.Hover:
                        fillColor = DarkModeHoverColor ?? DarkColors.BlueBackground;
                        borderPen = DarkColors.BlueHighlightPen;
                        break;
                    case DarkControlState.Pressed:
                        fillColor = DarkModePressedColor ?? DarkColors.DarkBackground;
                        break;
                }
            }
            else if (ButtonStyle == DarkButtonStyle.Flat)
            {
                switch (_buttonState)
                {
                    case DarkControlState.Normal:
                        fillColor = DarkModeBackColor ?? DarkColors.GreyBackground;
                        break;
                    case DarkControlState.Hover:
                        fillColor = DarkModeHoverColor ?? DarkColors.MediumBackground;
                        break;
                    case DarkControlState.Pressed:
                        fillColor = DarkModePressedColor ?? DarkColors.DarkBackground;
                        break;
                }
            }
        }
        else
        {
            textColorBrush = DarkColors.DisabledTextBrush;
            fillColor = DarkModeBackColor ?? DarkColors.DarkGreySelection;
        }

        if (fillColor != null)
        {
            using var b = new SolidBrush((Color)fillColor);
            g.FillRectangle(b, rect);
        }
        else
        {
            SolidBrush fillBrush = DarkColors.LightBackgroundBrush;
            g.FillRectangle(fillBrush, rect);
        }

        if (ButtonStyle == DarkButtonStyle.Normal)
        {
            // Again, match us visually to size and position of classic mode
            var borderRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height);

            g.DrawRectangle(borderPen, borderRect);
        }

        int textOffsetX = 0;
        int textOffsetY = 0;

        Padding padding = Padding;

        if (Image != null)
        {
            //SizeF stringSize = g.MeasureString(Text, Font, rect.Size);

            int x;
            //int x = (ClientSize.Width / 2) - (Image.Size.Width / 2);
            int y = (ClientSize.Height / 2) - (Image.Size.Height / 2);

            switch (TextImageRelation)
            {
                // @DarkModeNote(DarkButton): These are probably incorrect - fix them if we ever want to use them
#if false
                case TextImageRelation.ImageAboveText:
                    textOffsetY = (Image.Size.Height / 2) + (ImagePadding / 2);
                    y -= (int)(stringSize.Height / 2) + (ImagePadding / 2);
                    break;
                case TextImageRelation.TextAboveImage:
                    textOffsetY = ((Image.Size.Height / 2) + (ImagePadding / 2)) * -1;
                    y += (int)(stringSize.Height / 2) + (ImagePadding / 2);
                    break;
                case TextImageRelation.TextBeforeImage:
                    x += (int)stringSize.Width;
                    break;
#endif
                case TextImageRelation.ImageBeforeText:
                default:
                    textOffsetX = Image.Size.Width + ImagePadding + 1;
                    x = textOffsetX - (ImagePadding * 2);
                    break;
            }

            g.DrawImageUnscaled(Image, x, y);

            padding.Left -= Image.Width * 2;
        }

        // 3 pixel offset on all sides because of fudging with the rectangle, this gets it back to accurate
        // for the text.
        // @DarkModeNote(DarkButton/TextRect):
        // But actually we only know it's accurate for left-alignment, test if it is for all other alignments
        // as well...
        var textRect = new Rectangle(
            rect.Left + textOffsetX + padding.Left + 3,
            rect.Top + textOffsetY + padding.Top + 3,
            (rect.Width - padding.Horizontal) - 6,
            (rect.Height - padding.Vertical) - 6);

        TextFormatFlags textFormat =
            ControlUtils.GetTextAlignmentFlags(TextAlign) |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix |
            TextFormatFlags.NoClipping;

        // Use TextRenderer.DrawText() rather than g.DrawString() to match default text look exactly
        TextRenderer.DrawText(g, Text, Font, textRect, textColorBrush.Color, textFormat);

        #region Draw "transparent" (parent-control-backcolor-matching) border

        // This gets rid of the surrounding garbage from us modifying our draw position slightly to match
        // the visual size and positioning of the classic theme.
        // Draw this AFTER everything else, so that we draw on top so everything looks right.

        if (ButtonStyle == DarkButtonStyle.Normal)
        {
            Control parent = Parent;

            if (parent != null)
            {
                using var pen = new Pen(parent.BackColor);
                var bgRect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
                g.DrawRectangle(pen, bgRect);
            }
        }

        #endregion

        PaintCustom?.Invoke(this, e);
    }

    #endregion
}

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace Update;

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
public sealed class DarkButton : Button, IDarkable
{
    #region Field Region

    private DarkControlState _buttonState = DarkControlState.Normal;

    private bool _spacePressed;

    #endregion

    #region Designer Property Region

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
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool Enabled
    {
        get => base.Enabled;
        set
        {
            base.Enabled = value;
            InvalidateIfDark();
        }
    }

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

            // Everything needs to be just like this, or else there are cases where the appearance is wrong

            if (_darkModeEnabled)
            {
                UseVisualStyleBackColor = false;
                SetButtonState(DarkControlState.Normal);

                Invalidate();
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
            return;
        }

        Graphics g = e.Graphics;

        Rectangle rect = new(1, 1, ClientSize.Width - 2, ClientSize.Height - 3);

        SolidBrush textColorBrush = DarkColors.LightTextBrush;
        Pen borderPen = DarkColors.GreySelectionPen;
        Color? fillColor = null;

        if (Enabled)
        {
            if (Focused && TabStop)
            {
                borderPen = DarkColors.BlueHighlightPen;
            }

            switch (_buttonState)
            {
                case DarkControlState.Hover:
                    fillColor = DarkColors.BlueBackground;
                    borderPen = DarkColors.BlueHighlightPen;
                    break;
                case DarkControlState.Pressed:
                    fillColor = DarkColors.DarkBackground;
                    break;
            }
        }
        else
        {
            textColorBrush = DarkColors.DisabledTextBrush;
            fillColor = DarkColors.DarkGreySelection;
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

        // Again, match us visually to size and position of classic mode
        var borderRect = new Rectangle(rect.Left, rect.Top, rect.Width - 1, rect.Height);

        g.DrawRectangle(borderPen, borderRect);

        Padding padding = Padding;

        // 3 pixel offset on all sides because of fudging with the rectangle, this gets it back to accurate
        // for the text.
        // @DarkModeNote(DarkButton/TextRect):
        // But actually we only know it's accurate for left-alignment, test if it is for all other alignments
        // as well...
        var textRect = new Rectangle(
            rect.Left + padding.Left + 3,
            rect.Top + padding.Top + 3,
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

        Control? parent = Parent;

        if (parent != null)
        {
            using var pen = new Pen(parent.BackColor);
            var bgRect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            g.DrawRectangle(pen, bgRect);
        }

        #endregion
    }

    #endregion
}

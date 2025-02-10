using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkGroupBox : GroupBox, IDarkable
{
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

            if (_darkModeEnabled)
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.UserPaint, true);

                ResizeRedraw = true;
                DoubleBuffered = true;
            }
            else
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
                SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
                ResizeRedraw = true;
                DoubleBuffered = false;
            }
        }
    }

    private const int _padding = 6;

    // Store non-ampersand-doubled text so we can measure it accurately for the purposes of positioning the
    // gap in the border correctly to fit the text.
    private string _rawText = "";

    // Overriding is allowed, but if we do that then it breaks and doesn't show text unless it actually has
    // an ampersand for some goddamn reason we don't know why so just do new. Blah.
    [PublicAPI]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public new string Text
    {
        get => _rawText;
        set
        {
            _rawText = value;
            base.Text = value.EscapeAmpersands();
        }
    }

    public event EventHandler<PaintEventArgs>? PaintCustom;

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!_darkModeEnabled)
        {
            base.OnPaint(e);
            PaintCustom?.Invoke(this, e);
            return;
        }

        Graphics g = e.Graphics;
        Rectangle rect = ClientRectangle;
        Size stringSize = TextRenderer.MeasureText(_rawText, Font);

        g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, rect);

        Rectangle borderRect = new(
            0,
            stringSize.Height / 2,
            rect.Width - 1,
            rect.Height - (int)(Math.Ceiling((double)stringSize.Height / 2)) - 1
        );
        g.DrawRectangle(DarkColors.LighterBorderPen, borderRect);

        Rectangle textRect = new(
            rect.Left + _padding,
            rect.Top,
            rect.Width - (_padding * 2),
            stringSize.Height);

        Rectangle modRect = new(
            textRect.Left + 1,
            textRect.Top,
            Math.Min(textRect.Width, stringSize.Width) - 3, textRect.Height);
        g.FillRectangle(DarkColors.Fen_ControlBackgroundBrush, modRect);

        // No TextAlign property, so leave constant
        const TextFormatFlags textFormatFlags =
            TextFormatFlags.Default |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.NoClipping |
            // Don't remove ampersand garbage, because we can't tell the classic-mode control to not use the
            // feature, so to be consistent between modes we have to just manually escape the ampersands at
            // all times.
            //TextFormatFlags.NoPrefix |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.SingleLine;

        Color textColor = Enabled ? DarkColors.LightText : DarkColors.DisabledText;
        TextRenderer.DrawText(g, base.Text, Font, textRect, textColor, textFormatFlags);

        PaintCustom?.Invoke(this, e);
    }
}

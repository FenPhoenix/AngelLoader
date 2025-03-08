using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkLabel : Label, IDarkable
{
    public DarkLabel() => UseMnemonic = false;

#if DEBUG

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool UseMnemonic { get => base.UseMnemonic; set => base.UseMnemonic = value; }

#endif

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled { get; set; }

    [PublicAPI]
    public Color? DarkModeForeColor;

    [PublicAPI]
    public Color? DarkModeBackColor;

    public event EventHandler<PaintEventArgs>? PaintCustom;

    protected override void OnPaint(PaintEventArgs e)
    {
        if (!DarkModeEnabled)
        {
            base.OnPaint(e);
            PaintCustom?.Invoke(this, e);
            return;
        }

        TextFormatFlags textFormatFlags =
            ControlUtils.GetTextAlignmentFlags(TextAlign)
            | TextFormatFlags.NoPrefix
            | TextFormatFlags.NoClipping
            // This allows long lines with no spaces to still wrap. Matches stock behavior.
            // (actually doesn't quite match - we wrap at a different point, but as long as we still wrap
            // somewhere then whatever.)
            | TextFormatFlags.TextBoxControl
            | TextFormatFlags.WordBreak;

        Color color = Enabled ? DarkModeForeColor ?? DarkColors.LightText : DarkColors.DisabledText;
        if (DarkModeBackColor == null)
        {
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, color, textFormatFlags);
        }
        else
        {
            SolidBrush bgBrush = DarkColors.GetCachedSolidBrush((Color)DarkModeBackColor);
            e.Graphics.FillRectangle(bgBrush, ClientRectangle);
            TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, color, (Color)DarkModeBackColor, textFormatFlags);
        }

        PaintCustom?.Invoke(this, e);
    }
}

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkLabel : Label, IDarkable
    {
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled { get; set; }

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color? DarkModeForeColor { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DarkModeEnabled)
            {
                TextFormatFlags textFormatFlags =
                    ControlUtils.GetTextAlignmentFlags(TextAlign) |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.NoClipping |
                    TextFormatFlags.WordBreak;

                Color color = Enabled ? DarkModeForeColor ?? DarkColors.LightText : DarkColors.DisabledText;
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, color, textFormatFlags);
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}

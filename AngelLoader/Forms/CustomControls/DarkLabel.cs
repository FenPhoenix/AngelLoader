using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class DarkLabel : Label, IDarkable
    {
        [PublicAPI]
        public bool DarkModeEnabled { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DarkModeEnabled)
            {
                // TODO: @DarkMode: Add this to all controls with alignable text
                TextFormatFlags alignmentFlags = TextAlign switch
                {
                    ContentAlignment.TopLeft => TextFormatFlags.Top | TextFormatFlags.Left,
                    ContentAlignment.TopCenter => TextFormatFlags.Top | TextFormatFlags.HorizontalCenter,
                    ContentAlignment.TopRight => TextFormatFlags.Top | TextFormatFlags.Right,
                    ContentAlignment.MiddleLeft => TextFormatFlags.VerticalCenter | TextFormatFlags.Left,
                    ContentAlignment.MiddleCenter => TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter,
                    ContentAlignment.MiddleRight => TextFormatFlags.VerticalCenter | TextFormatFlags.Right,
                    ContentAlignment.BottomLeft => TextFormatFlags.Bottom | TextFormatFlags.Left,
                    ContentAlignment.BottomCenter => TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter,
                    ContentAlignment.BottomRight => TextFormatFlags.Bottom | TextFormatFlags.Right,
                    _ => TextFormatFlags.Top | TextFormatFlags.Left
                };

                TextFormatFlags textFormatFlags =
                    alignmentFlags |
                    TextFormatFlags.NoPrefix |
                    TextFormatFlags.NoClipping |
                    TextFormatFlags.WordBreak;

                Color color = Enabled ? DarkColors.LightText : DarkColors.DisabledText;
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, color, textFormatFlags);
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}

using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class LabelCustom : Label, IDarkable
    {
        [PublicAPI]
        public bool DarkModeEnabled { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DarkModeEnabled)
            {
                Color color = Enabled ? DarkColors.LightText : DarkColors.DisabledText;
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, color);
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}

using System.Drawing;
using System.Windows.Forms;
using DarkUI.Controls;

namespace AngelLoader.Forms.CustomControls
{
    public sealed class LabelCustom : Label, IDarkable
    {
        public bool DarkModeEnabled { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DarkModeEnabled)
            {
                Color color = Enabled ? DarkUI.Config.Colors.Fen_DarkForeground : DarkUI.Config.Colors.DisabledText;
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, color);
            }
            else
            {
                base.OnPaint(e);
            }
        }
    }
}

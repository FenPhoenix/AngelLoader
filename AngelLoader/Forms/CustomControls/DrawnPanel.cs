using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls
{
    /// <summary>
    /// Regular Panels don't behave with their BackColors, so...
    /// </summary>
    public sealed class DrawnPanel : Panel, IDarkable
    {
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color DrawnBackColor { get; set; } = SystemColors.Control;
        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color DarkModeDrawnBackColor { get; set; } = DarkColors.Fen_ControlBackground;

        [PublicAPI]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DarkModeEnabled { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            using var brush = new SolidBrush(DarkModeEnabled ? DarkModeDrawnBackColor : DrawnBackColor);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }
}

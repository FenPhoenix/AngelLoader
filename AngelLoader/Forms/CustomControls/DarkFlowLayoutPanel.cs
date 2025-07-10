using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.DataClasses;
using AngelLoader.Forms.WinFormsNative;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls;

public sealed class DarkFlowLayoutPanel : FlowLayoutPanel, IDarkable
{
    [PublicAPI]
    public Color DrawnBackColor = SystemColors.Control;

    [PublicAPI]
    public Color DarkModeDrawnBackColor = DarkColors.Fen_ControlBackground;

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool DarkModeEnabled { get; set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        SolidBrush brush = DarkColors.GetCachedSolidBrush(DarkModeEnabled ? DarkModeDrawnBackColor : DrawnBackColor);
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == Native.WM_NCPAINT)
        {
            base.WndProc(ref m);
            ControlUtils.Wine_DrawScrollBarCorner(this);
            return;
        }

        base.WndProc(ref m);
    }
}

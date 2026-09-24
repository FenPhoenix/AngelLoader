using System.Windows.Forms;
using AngelLoader.Forms.WinFormsNative;

namespace AngelLoader.Forms.CustomControls;

public class PanelCustom : Panel
{
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

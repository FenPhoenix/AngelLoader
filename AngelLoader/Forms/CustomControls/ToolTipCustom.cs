using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed class ToolTipCustom : ToolTip
{
    public ToolTipCustom() => TrySetMaxDelay();

    public ToolTipCustom(IContainer cont) : base(cont) => TrySetMaxDelay();

    // The docs say the max is 5000, but this is straight-up false. There is no range check for this value in the
    // framework code, and the Win32 max of 32767 works perfectly fine. Weird, but I'll take it!
    private void TrySetMaxDelay()
    {
        // However, let's be careful just in case.
        try
        {
            AutoPopDelay = 32767;
        }
        catch
        {
            try
            {
                AutoPopDelay = 5000;
            }
            catch
            {
                // oh well...
            }
        }
    }
}

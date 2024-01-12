using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed class ToolTipCustom : ToolTip
{
    public ToolTipCustom() => TrySetMaxDelay();

    public ToolTipCustom(IContainer cont) : base(cont) => TrySetMaxDelay();

    /*
    The docs are confusing about this. They mention the value "5000" several times, and then they say this:
    "The maximum time you can delay a popup is 5000 milliseconds." If you're just breezing over that line, you
    might read that as "the maximum time you can display a popup for is 5000 milliseconds". But that's false,
    the maximum display time is 32767 milliseconds. But it doesn't say that anywhere on the page, and the fact
    that this is the help page for AutoPopDelay and NOT InitialDelay means you're expecting any line vaguely
    saying "maximum" and then a number will be the maximum value for the property the page is about. But nope.
    */
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
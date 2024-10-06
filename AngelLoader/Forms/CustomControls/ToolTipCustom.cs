using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed class ToolTipCustom : ToolTip
{
    public ToolTipCustom() => this.SetMaxDelay();

    public ToolTipCustom(IContainer cont) : base(cont) => this.SetMaxDelay();
}

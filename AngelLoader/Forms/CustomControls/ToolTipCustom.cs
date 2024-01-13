using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed class ToolTipCustom : ToolTip
{
    public ToolTipCustom() => this.TrySetMaxDelay();

    public ToolTipCustom(IContainer cont) : base(cont) => this.TrySetMaxDelay();
}
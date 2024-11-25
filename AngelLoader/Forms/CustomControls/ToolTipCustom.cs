using System.ComponentModel;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;

public sealed class ToolTipCustom : ToolTip
{
#if DEBUG
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new int AutoPopDelay
    {
        get => base.AutoPopDelay;
        set => base.AutoPopDelay = value;
    }
#endif

    public ToolTipCustom() => this.SetMaxDelay();

    public ToolTipCustom(IContainer cont) : base(cont) => this.SetMaxDelay();
}

#if DEBUG
using System.ComponentModel;
#endif
using System.Drawing;
using System.Windows.Forms;
#if DEBUG
using JetBrains.Annotations;
#endif

namespace AngelLoader.Forms.CustomControls;

public sealed class StandardButton : DarkButton
{
#if DEBUG

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new bool AutoSize
    {
        get => base.AutoSize;
        set => base.AutoSize = value;
    }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new AutoSizeMode AutoSizeMode
    {
        get => base.AutoSizeMode;
        set => base.AutoSizeMode = value;
    }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Size MinimumSize
    {
        get => base.MinimumSize;
        set => base.MinimumSize = value;
    }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Size Size
    {
        get => base.Size;
        set => base.Size = value;
    }

    [PublicAPI]
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new Padding Padding
    {
        get => base.Padding;
        set => base.Padding = value;
    }
#endif

    public StandardButton()
    {
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        MinimumSize = new Size(75, 23);
        Padding = new Padding(6, 0, 6, 0);
    }
}

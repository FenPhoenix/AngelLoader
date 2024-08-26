using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;

public sealed partial class MainForm
{
    private ProgressBox_MultiItem? ProgressBox_MultiItem;

    [MemberNotNull(nameof(ProgressBox_MultiItem))]
    private void ConstructProgressBox_MultiItem()
    {
        if (ProgressBox_MultiItem != null) return;

        ProgressBox_MultiItem = new ProgressBox_MultiItem(this) { Tag = LoadType.Lazy, Visible = false };
        Controls.Add(ProgressBox_MultiItem);
        ProgressBox_MultiItem.Anchor = AnchorStyles.None;
        ProgressBox_MultiItem.DarkModeEnabled = Global.Config.DarkMode;
    }
}

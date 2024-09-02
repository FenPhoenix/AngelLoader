using System;
using System.Windows.Forms;

namespace AngelLoader.Forms.CustomControls;
public sealed class DGV_ProgressItem : DataGridView
{
    public DGV_ProgressItem()
    {
    }

    protected override void OnSelectionChanged(EventArgs e)
    {
        base.OnSelectionChanged(e);

        foreach (DataGridViewRow row in Rows)
        {
            row.Selected = false;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        e.Handled = true;
        base.OnKeyDown(e);
    }
}

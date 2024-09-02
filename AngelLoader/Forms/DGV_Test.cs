using System.Windows.Forms;

namespace AngelLoader.Forms;

public sealed partial class DGV_Test : Form
{
    public sealed class Blah : DataGridViewRow
    {
        public Blah()
        {
        }
    }

    public DGV_Test()
    {
        InitializeComponent();

        dgV_ProgressItem1.RowCount = 20;
    }
}

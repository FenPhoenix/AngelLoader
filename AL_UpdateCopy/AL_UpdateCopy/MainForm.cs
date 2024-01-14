using System.Windows.Forms;

namespace AL_UpdateCopy;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        CopyingLabel.CenterH(this, clientSize: true);
        CopyingProgressBar.CenterH(this, clientSize: true);
    }
}

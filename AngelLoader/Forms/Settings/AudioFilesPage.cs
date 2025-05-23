using System.Windows.Forms;

namespace AngelLoader.Forms;

public sealed partial class AudioFilesPage : UserControl, Interfaces.ISettingsPage
{
    public AudioFilesPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;
}

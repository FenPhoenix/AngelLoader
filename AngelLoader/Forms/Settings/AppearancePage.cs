using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;

public sealed partial class AppearancePage : UserControl, Interfaces.ISettingsPage
{
    public AppearancePage()
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

using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;

public sealed partial class UpdatePage : UserControl, Interfaces.ISettingsPage
{
    // @Update: We should put an "Update track" option for 32/64, and maybe .NET 9+ in the future

    public UpdatePage()
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

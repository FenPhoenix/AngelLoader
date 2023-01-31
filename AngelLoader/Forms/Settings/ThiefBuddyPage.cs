using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;
public partial class ThiefBuddyPage : UserControl, Interfaces.ISettingsPage
{
    public bool IsVisible => Visible;

    public ThiefBuddyPage()
    {
        InitializeComponent();
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

    public void ShowPage() => Show();

    public void HidePage() => Hide();
}

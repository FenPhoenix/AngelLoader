using System.Windows.Forms;
using AngelLoader.Common.Utility;

namespace AngelLoader.CustomControls.SettingsForm
{
    public partial class OtherPage : UserControl, Interfaces.ISettingsPage
    {
        public OtherPage() => InitializeComponent();

        public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

        public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

        public void ShowPage() => Show();

        public void HidePage() => Hide();
    }
}

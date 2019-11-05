using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.CustomControls.SettingsPages
{
    [PublicAPI]
    public partial class FMDisplayPage : UserControl, Interfaces.ISettingsPage
    {
        public bool IsVisible { get => Visible; set => Visible = value; }

        public FMDisplayPage() => InitializeComponent();

        public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

        public int GetVScrollPos() => PagePanel.VerticalScroll.Value;
        
        public void ShowPage() => Show();

        public void HidePage() => Hide();
    }
}

using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    [PublicAPI]
    public partial class FMDisplayPage : UserControl, Interfaces.ISettingsPage
    {
        public bool IsVisible { get => Visible; set => Visible = value; }

        public FMDisplayPage()
        {
            // TODO: @DarkMode: Redo InitComponentManual()!
#if DEBUG
            InitializeComponent();
#else
            InitComponentManual();
#endif
        }

        public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

        public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

        public void ShowPage() => Show();

        public void HidePage() => Hide();
    }
}

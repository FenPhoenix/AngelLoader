using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;

namespace AngelLoader.Forms
{
    [PublicAPI]
    public partial class FMsListPage : UserControl, Interfaces.ISettingsPage
    {
        public bool IsVisible { get => Visible; set => Visible = value; }

        public FMsListPage()
        {
#if DEBUG
            InitializeComponent();
#else
            InitializeComponentSlim();
#endif
        }

        public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

        public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

        public void ShowPage() => Show();

        public void HidePage() => Hide();
    }
}

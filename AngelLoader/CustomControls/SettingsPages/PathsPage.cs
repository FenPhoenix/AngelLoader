using System.Windows.Forms;
using AngelLoader.Common.Utility;

namespace AngelLoader.CustomControls.SettingsPages
{
    public partial class PathsPage : UserControl, Interfaces.ISettingsPage
    {
        public bool IsVisible { get => Visible; set => Visible = value; }

        public PathsPage() => InitializeComponent();

        public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

        public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

        public void ShowPage() => Show();

        public void HidePage() => Hide();

        private void AddFMArchivePathButton_Paint(object sender, PaintEventArgs e) => ButtonPainter.PaintPlusButton(e);

        private void RemoveFMArchivePathButton_Paint(object sender, PaintEventArgs e) => ButtonPainter.PaintMinusButton(e);
    }
}

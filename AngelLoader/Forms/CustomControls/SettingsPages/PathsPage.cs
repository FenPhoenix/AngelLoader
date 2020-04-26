using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    [PublicAPI]
    public partial class PathsPage : UserControl, Interfaces.ISettingsPage
    {
        public bool IsVisible { get => Visible; set => Visible = value; }

        public PathsPage() => InitializeComponent();

        public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

        public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

        public void ShowPage() => Show();

        public void HidePage() => Hide();

        private void AddFMArchivePathButton_Paint(object sender, PaintEventArgs e) => ButtonPainter.PaintPlusButton(AddFMArchivePathButton, e);

        private void RemoveFMArchivePathButton_Paint(object sender, PaintEventArgs e) => ButtonPainter.PaintMinusButton(RemoveFMArchivePathButton, e);
    }
}

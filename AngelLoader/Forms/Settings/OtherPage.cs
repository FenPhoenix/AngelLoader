using System.Drawing;
using System.Windows.Forms;
using AL_Common;
using JetBrains.Annotations;

namespace AngelLoader.Forms
{
    [PublicAPI]
    public partial class OtherPage : UserControl, Interfaces.ISettingsPage
    {
        public bool IsVisible { get => Visible; set => Visible = value; }

        public OtherPage()
        {
#if DEBUG
            InitializeComponent();
#else
            InitSlim();
#endif
        }

        public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

        public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

        public void ShowPage() => Show();

        public void HidePage() => Hide();

        private void WebSearchUrlResetButton_Paint(object sender, PaintEventArgs e)
        {
            var cr = WebSearchUrlResetButton.ClientRectangle;
            Images.PaintBitmapButton(
                WebSearchUrlResetButton,
                e,
                Images.Refresh,
                scaledRect: new RectangleF(
                    cr.X + 2f,
                    cr.Y + 2f,
                    cr.Width - 4f,
                    cr.Height - 4f));
        }
    }
}

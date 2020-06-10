using System.Drawing;
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

        private void FlowLayoutPanel1_Layout(object sender, LayoutEventArgs e)
        {
            // Manual crap. Yes, it's necessary. All automatic methods are "almost what we need but not quite".
            int flpC = FlowLayoutPanel1.Controls.Count;
            for (int i = 0; i < flpC; i++)
            {
                FlowLayoutPanel1.Controls[i].Width = FlowLayoutPanel1.ClientSize.Width - 16;
            }

            BackupPathHelpLabel.MaximumSize = new Size(
                OtherGroupBox.Width - (BackupPathHelpLabel.Left * 2),
                BackupPathHelpLabel.MaximumSize.Height);

            OtherGroupBox.Height = OtherGroupBox.Padding.Vertical +
                                   BackupPathPanel.Top +
                                   BackupPathPanel.Padding.Vertical +
                                   BackupPathHelpLabel.Top +
                                   BackupPathHelpLabel.Padding.Vertical +
                                   BackupPathHelpLabel.Height +
                                   6;

            // Have to do this separately after, because our heights will have changed above
            int flpHeight = FlowLayoutPanel1.Padding.Vertical;
            for (int i = 0; i < flpC; i++)
            {
                Control c = FlowLayoutPanel1.Controls[i];
                flpHeight += c.Margin.Vertical +
                             c.Padding.Vertical +
                             c.Height;
            }

            FlowLayoutPanel1.Height = flpHeight;

            int greatestTop = 0;
            Control? bottomMostControl = null;
            int appC = ActualPathsPanel.Controls.Count;
            for (int i = 0; i < appC; i++)
            {
                Control c = ActualPathsPanel.Controls[i];
                if (c.Top > greatestTop)
                {
                    greatestTop = c.Top;
                    bottomMostControl = c;
                }
            }

            Misc.AssertR(bottomMostControl != null, nameof(bottomMostControl) + " was null");

            ActualPathsPanel.Height = (bottomMostControl!.Top +
                                       bottomMostControl!.Height) -
                                      ActualPathsPanel.Padding.Vertical;
        }

        private void BackupPathHelpLabel_TextChanged(object sender, System.EventArgs e)
        {
            // The text changing might also require the box to change height, but the flow layout panel may not
            // have fired its layout event in that case, so do it manually here.
            FlowLayoutPanel1.PerformLayout();
        }
    }
}

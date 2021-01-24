using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace AngelLoader.Forms.CustomControls.SettingsPages
{
    [PublicAPI]
    public partial class PathsPage : UserControl, Interfaces.ISettingsPage
    {
        public bool IsVisible { get => Visible; set => Visible = value; }

        /// <summary>
        /// Horrible hack, just set it to true when you want it to start doing the layout crap
        /// </summary>
        public bool DoLayout;

        public PathsPage()
        {
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

        private void AddFMArchivePathButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintPlusButton(AddFMArchivePathButton, e);

        private void RemoveFMArchivePathButton_Paint(object sender, PaintEventArgs e) => ControlPainter.PaintMinusButton(RemoveFMArchivePathButton, e);

        private void FlowLayoutPanel1_Layout(object sender, LayoutEventArgs e)
        {
            // Horrible hack to get everything to look right on first show
            if (!DoLayout) return;
            try
            {
                FlowLayoutPanel1.SuspendLayout();
                ActualPathsPanel.SuspendLayout();

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

                // We have to re-set its width because it isn't docked because apparently it doesn't scroll when
                // it's docked. I'll just trust my hundreds of hours spent on this and move on.
                // If we don't do this, its width can become desynced with the rest of the controls' width under
                // heavy and fast resizing movement (even though it's anchored).
                ActualPathsPanel.Width = PagePanel.VerticalScroll.Visible
                    ? PagePanel.Width - SystemInformation.VerticalScrollBarWidth
                    : PagePanel.Width;
                // Guess we gotta do this one too or something
                FlowLayoutPanel1.Width = ActualPathsPanel.Width;
            }
            finally
            {
                ActualPathsPanel.ResumeLayout();
                FlowLayoutPanel1.ResumeLayout();
            }
        }

        private void BackupPathHelpLabel_TextChanged(object sender, System.EventArgs e)
        {
            // The text changing might also require the box to change height, but the flow layout panel may not
            // have fired its layout event in that case, so do it manually here.
            FlowLayoutPanel1.PerformLayout();
        }
    }
}

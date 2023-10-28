using System.Windows.Forms;
using AL_Common;

namespace AngelLoader.Forms;

public sealed partial class PathsPage : UserControl, Interfaces.ISettingsPage
{
    /// <summary>
    /// Horrible hack, just set it to true when you want it to start doing the layout crap
    /// </summary>
    public bool DoLayout;

    public PathsPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        BackupHelpPictureBox.Image = Images.HelpSmall;
        BackupHelpTDMPictureBox.Image = Images.TDM_16();
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

    private void AddFMArchivePathButton_Paint(object sender, PaintEventArgs e) => Images.PaintPlusButton(AddFMArchivePathButton, e);

    private void RemoveFMArchivePathButton_Paint(object sender, PaintEventArgs e) => Images.PaintMinusButton(RemoveFMArchivePathButton, e);

    private void LayoutFLP_Layout(object sender, LayoutEventArgs e)
    {
        // Horrible hack to get everything to look right on first show
        if (!DoLayout) return;
        try
        {
            LayoutFLP.SuspendLayout();
            ActualPathsPanel.SuspendLayout();

            // Manual crap. Yes, it's necessary. All automatic methods are "almost what we need but not quite".
            ControlCollection layoutFLPControls = LayoutFLP.Controls;
            int layoutFLPControlsCount = layoutFLPControls.Count;
            for (int i = 0; i < layoutFLPControlsCount; i++)
            {
                layoutFLPControls[i].Width = LayoutFLP.ClientSize.Width - 16;
            }

            BackupPathHelpLabel.MaximumSize = BackupPathHelpLabel.MaximumSize with
            {
                Width = BackupGroupBox.Width - (BackupPathHelpLabel.Left * 2)
            };

            BackupPathTDMHelpLabel.Location = BackupPathTDMHelpLabel.Location with { Y = BackupPathHelpLabel.Bottom + 16 };
            BackupHelpTDMPictureBox.Location = BackupHelpTDMPictureBox.Location with { Y = BackupPathTDMHelpLabel.Location.Y };
            BackupPathTDMHelpLabel.MaximumSize = BackupPathTDMHelpLabel.MaximumSize with
            {
                Width = BackupGroupBox.Width - (BackupPathTDMHelpLabel.Left * 2)
            };

            BackupGroupBox.Height = BackupGroupBox.Padding.Vertical +
                                    BackupPathPanel.Top +
                                    BackupPathPanel.Padding.Vertical +
                                    BackupPathTDMHelpLabel.Bottom +
                                    12;

            // Have to do this separately after, because our heights will have changed above
            int flpHeight = LayoutFLP.Padding.Vertical;
            for (int i = 0; i < layoutFLPControlsCount; i++)
            {
                Control c = layoutFLPControls[i];
                flpHeight += c.Margin.Vertical +
                             c.Padding.Vertical +
                             c.Height;
            }

            LayoutFLP.Height = flpHeight;

            int greatestTop = 0;
            Control? bottomMostControl = null;
            ControlCollection actualPathsPanelControls = ActualPathsPanel.Controls;
            int actualPathsPanelControlsCount = actualPathsPanelControls.Count;
            for (int i = 0; i < actualPathsPanelControlsCount; i++)
            {
                Control c = actualPathsPanelControls[i];
                if (c.Top > greatestTop)
                {
                    greatestTop = c.Top;
                    bottomMostControl = c;
                }
            }

            Utils.AssertR(bottomMostControl != null, nameof(bottomMostControl) + " was null");

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
            LayoutFLP.Width = ActualPathsPanel.Width;
        }
        finally
        {
            ActualPathsPanel.ResumeLayout();
            LayoutFLP.ResumeLayout();
        }
    }

    private void BackupPathHelpLabel_TextChanged(object sender, System.EventArgs e)
    {
        // The text changing might also require the box to change height, but the flow layout panel may not
        // have fired its layout event in that case, so do it manually here.
        LayoutFLP.PerformLayout();
    }
}

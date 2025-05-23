using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AngelLoader.Forms.CustomControls;

namespace AngelLoader.Forms;

public sealed partial class IOThreadingPage : UserControl, Interfaces.ISettingsPage
{
    /// <summary>
    /// Horrible hack, just set it to true when you want it to start doing the layout crap
    /// </summary>
    public bool DoLayout;

    public IOThreadingPage()
    {
#if DEBUG
        InitializeComponent();
#else
        InitSlim();
#endif

        HelpPictureBox.Image = Images.HelpSmall;
    }

    public void SetVScrollPos(int value) => PagePanel.VerticalScroll.Value = value.Clamp(PagePanel.VerticalScroll.Minimum, PagePanel.VerticalScroll.Maximum);

    public int GetVScrollPos() => PagePanel.VerticalScroll.Value;

    internal readonly List<int> HorizDivYPositions = new();

    private void IOThreadingLevelGroupBox_PaintCustom(object sender, PaintEventArgs e)
    {
        for (int i = 0; i < HorizDivYPositions.Count; i++)
        {
            int y = HorizDivYPositions[i];
            Images.DrawHorizDiv(e.Graphics, 8, y, IOThreadingLevelGroupBox.Width - 8);
        }
    }

    private void IOThreadsResetButton_PaintCustom(object sender, PaintEventArgs e)
    {
        DarkButton button = (DarkButton)sender;
        Rectangle cr = button.ClientRectangle;
        Images.PaintBitmapButton(
            e: e,
            img: button.Enabled
                ? Images.Refresh
                : Images.Refresh_Disabled,
            scaledRect: new RectangleF(
                cr.X + 2f,
                cr.Y + 2f,
                cr.Width - 4f,
                cr.Height - 4f));
    }

    private void LayoutFLP_Layout(object sender, LayoutEventArgs e)
    {
        // Horrible hack to get everything to look right on first show
        if (!DoLayout) return;
        try
        {
            LayoutFLP.SuspendLayout();
            ActualPagePanel.SuspendLayout();

            // Manual crap. Yes, it's necessary. All automatic methods are "almost what we need but not quite".
            ControlCollection layoutFLPControls = LayoutFLP.Controls;
            int layoutFLPControlsCount = layoutFLPControls.Count;
            for (int i = 0; i < layoutFLPControlsCount; i++)
            {
                layoutFLPControls[i].Width = LayoutFLP.ClientSize.Width - 16;
            }

            HelpLabel.MaximumSize = HelpLabel.MaximumSize with
            {
                Width = HelpPanel.Width - (HelpLabel.Left * 2),
            };

            HelpPanel.Height = HelpPanel.Padding.Vertical +
                                     HelpPanel.Top +
                                     HelpPanel.Padding.Vertical +
                                     HelpLabel.Bottom +
                                     16;

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
            ControlCollection actualPathsPanelControls = ActualPagePanel.Controls;
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

            ActualPagePanel.Height = (bottomMostControl!.Top +
                                       bottomMostControl!.Height) -
                                      ActualPagePanel.Padding.Vertical;

            // We have to re-set its width because it isn't docked because apparently it doesn't scroll when
            // it's docked. I'll just trust my hundreds of hours spent on this and move on.
            // If we don't do this, its width can become desynced with the rest of the controls' width under
            // heavy and fast resizing movement (even though it's anchored).
            ActualPagePanel.Width = PagePanel.VerticalScroll.Visible
                ? PagePanel.Width - SystemInformation.VerticalScrollBarWidth
                : PagePanel.Width;
            // Guess we gotta do this one too or something
            LayoutFLP.Width = ActualPagePanel.Width;
        }
        finally
        {
            ActualPagePanel.ResumeLayout();
            LayoutFLP.ResumeLayout();
        }
    }

    private void HelpLabel_TextChanged(object sender, System.EventArgs e)
    {
        LayoutFLP.PerformLayout();
    }
}

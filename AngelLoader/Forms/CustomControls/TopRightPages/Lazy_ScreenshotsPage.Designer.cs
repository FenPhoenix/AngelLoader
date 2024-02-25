#define FenGen_DesignerSource

namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage
{
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Component Designer generated code

#if DEBUG
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.GammaTrackBar = new AngelLoader.Forms.CustomControls.DarkTrackBar();
            this.OpenScreenshotsFolderButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.NumberLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.NextButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
            this.PrevButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
            this.ScreenshotsPictureBox = new AngelLoader.Forms.CustomControls.ImagePanelCustom();
            ((System.ComponentModel.ISupportInitialize)(this.GammaTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // GammaTrackBar
            // 
            this.GammaTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GammaTrackBar.AutoSize = false;
            this.GammaTrackBar.Location = new System.Drawing.Point(8, 223);
            this.GammaTrackBar.Maximum = 100;
            this.GammaTrackBar.Name = "GammaTrackBar";
            this.GammaTrackBar.Size = new System.Drawing.Size(512, 32);
            this.GammaTrackBar.TabIndex = 5;
            this.GammaTrackBar.TickFrequency = 10;
            this.GammaTrackBar.Value = 50;
            // 
            // OpenScreenshotsFolderButton
            // 
            this.OpenScreenshotsFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OpenScreenshotsFolderButton.Location = new System.Drawing.Point(336, 256);
            this.OpenScreenshotsFolderButton.Name = "OpenScreenshotsFolderButton";
            this.OpenScreenshotsFolderButton.Size = new System.Drawing.Size(35, 23);
            this.OpenScreenshotsFolderButton.TabIndex = 0;
            // 
            // NumberLabel
            // 
            this.NumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NumberLabel.AutoSize = true;
            this.NumberLabel.Location = new System.Drawing.Point(472, 204);
            this.NumberLabel.Name = "NumberLabel";
            this.NumberLabel.Size = new System.Drawing.Size(54, 13);
            this.NumberLabel.TabIndex = 3;
            this.NumberLabel.Text = "999 / 999";
            // 
            // NextButton
            // 
            this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.NextButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
            this.NextButton.Location = new System.Drawing.Point(446, 256);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 2;
            // 
            // PrevButton
            // 
            this.PrevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.PrevButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
            this.PrevButton.Location = new System.Drawing.Point(371, 256);
            this.PrevButton.Name = "PrevButton";
            this.PrevButton.Size = new System.Drawing.Size(75, 23);
            this.PrevButton.TabIndex = 1;
            // 
            // ScreenshotsPictureBox
            // 
            this.ScreenshotsPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ScreenshotsPictureBox.Location = new System.Drawing.Point(8, 8);
            this.ScreenshotsPictureBox.Name = "ScreenshotsPictureBox";
            this.ScreenshotsPictureBox.Size = new System.Drawing.Size(512, 192);
            this.ScreenshotsPictureBox.TabIndex = 4;
            // 
            // Lazy_ScreenshotsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(200, 100);
            this.Controls.Add(this.NumberLabel);
            this.Controls.Add(this.GammaTrackBar);
            this.Controls.Add(this.OpenScreenshotsFolderButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.PrevButton);
            this.Controls.Add(this.ScreenshotsPictureBox);
            this.Name = "Lazy_ScreenshotsPage";
            this.Size = new System.Drawing.Size(527, 284);
            ((System.ComponentModel.ISupportInitialize)(this.GammaTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }
#endif

    #endregion

    internal ImagePanelCustom ScreenshotsPictureBox;
    internal DarkArrowButton PrevButton;
    internal DarkArrowButton NextButton;
    internal DarkLabel NumberLabel;
    internal DarkButton OpenScreenshotsFolderButton;
    internal DarkTrackBar GammaTrackBar;
}

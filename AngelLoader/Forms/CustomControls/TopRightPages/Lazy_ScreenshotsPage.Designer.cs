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
            this.ButtonsFLP = new AngelLoader.Forms.CustomControls.DarkFlowLayoutPanel();
            this.NextButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
            this.PrevButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
            this.CopiedMessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.CopyButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.GammaLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.NumberLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
            this.GammaTrackBar = new AngelLoader.Forms.CustomControls.DarkTrackBar();
            this.OpenScreenshotsFolderButton = new AngelLoader.Forms.CustomControls.DarkButton();
            this.ScreenshotsPictureBox = new AngelLoader.Forms.CustomControls.ImagePanelCustom();
            this.ButtonsFLP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GammaTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonsFLP
            // 
            this.ButtonsFLP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonsFLP.Controls.Add(this.NextButton);
            this.ButtonsFLP.Controls.Add(this.PrevButton);
            this.ButtonsFLP.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.ButtonsFLP.Location = new System.Drawing.Point(56, 256);
            this.ButtonsFLP.Name = "ButtonsFLP";
            this.ButtonsFLP.Size = new System.Drawing.Size(466, 24);
            this.ButtonsFLP.TabIndex = 6;
            // 
            // NextButton
            // 
            this.NextButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
            this.NextButton.Location = new System.Drawing.Point(391, 0);
            this.NextButton.Margin = new System.Windows.Forms.Padding(0);
            this.NextButton.Name = "NextButton";
            this.NextButton.Size = new System.Drawing.Size(75, 23);
            this.NextButton.TabIndex = 2;
            // 
            // PrevButton
            // 
            this.PrevButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
            this.PrevButton.Location = new System.Drawing.Point(316, 0);
            this.PrevButton.Margin = new System.Windows.Forms.Padding(0);
            this.PrevButton.Name = "PrevButton";
            this.PrevButton.Size = new System.Drawing.Size(75, 23);
            this.PrevButton.TabIndex = 1;
            // 
            // CopiedMessageLabel
            // 
            this.CopiedMessageLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.CopiedMessageLabel.AutoSize = true;
            this.CopiedMessageLabel.Location = new System.Drawing.Point(240, 204);
            this.CopiedMessageLabel.Name = "CopiedMessageLabel";
            this.CopiedMessageLabel.Size = new System.Drawing.Size(46, 13);
            this.CopiedMessageLabel.TabIndex = 2;
            this.CopiedMessageLabel.Text = "[Copied]";
            this.CopiedMessageLabel.Visible = false;
            // 
            // CopyButton
            // 
            this.CopyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CopyButton.Location = new System.Drawing.Point(32, 256);
            this.CopyButton.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.CopyButton.Name = "CopyButton";
            this.CopyButton.Size = new System.Drawing.Size(24, 23);
            this.CopyButton.TabIndex = 0;
            this.CopyButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.CopyButton_PaintCustom);
            // 
            // GammaLabel
            // 
            this.GammaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.GammaLabel.AutoSize = true;
            this.GammaLabel.Location = new System.Drawing.Point(8, 204);
            this.GammaLabel.Name = "GammaLabel";
            this.GammaLabel.Size = new System.Drawing.Size(46, 13);
            this.GammaLabel.TabIndex = 1;
            this.GammaLabel.Text = "Gamma:";
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
            // GammaTrackBar
            // 
            this.GammaTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.GammaTrackBar.AutoSize = false;
            this.GammaTrackBar.Location = new System.Drawing.Point(8, 223);
            this.GammaTrackBar.Maximum = 100;
            this.GammaTrackBar.Name = "GammaTrackBar";
            this.GammaTrackBar.Size = new System.Drawing.Size(512, 32);
            this.GammaTrackBar.TabIndex = 4;
            this.GammaTrackBar.TickFrequency = 10;
            this.GammaTrackBar.Value = 50;
            // 
            // OpenScreenshotsFolderButton
            // 
            this.OpenScreenshotsFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.OpenScreenshotsFolderButton.Location = new System.Drawing.Point(8, 256);
            this.OpenScreenshotsFolderButton.Name = "OpenScreenshotsFolderButton";
            this.OpenScreenshotsFolderButton.Size = new System.Drawing.Size(24, 23);
            this.OpenScreenshotsFolderButton.TabIndex = 5;
            this.OpenScreenshotsFolderButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.OpenScreenshotsFolderButton_PaintCustom);
            // 
            // ScreenshotsPictureBox
            // 
            this.ScreenshotsPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ScreenshotsPictureBox.Location = new System.Drawing.Point(8, 8);
            this.ScreenshotsPictureBox.Name = "ScreenshotsPictureBox";
            this.ScreenshotsPictureBox.Size = new System.Drawing.Size(512, 192);
            this.ScreenshotsPictureBox.TabIndex = 0;
            // 
            // Lazy_ScreenshotsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(216, 200);
            this.Controls.Add(this.ButtonsFLP);
            this.Controls.Add(this.CopiedMessageLabel);
            this.Controls.Add(this.CopyButton);
            this.Controls.Add(this.GammaLabel);
            this.Controls.Add(this.NumberLabel);
            this.Controls.Add(this.GammaTrackBar);
            this.Controls.Add(this.OpenScreenshotsFolderButton);
            this.Controls.Add(this.ScreenshotsPictureBox);
            this.Name = "Lazy_ScreenshotsPage";
            this.Size = new System.Drawing.Size(527, 284);
            this.ButtonsFLP.ResumeLayout(false);
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
    internal DarkLabel GammaLabel;
    internal DarkLabel CopiedMessageLabel;
    private DarkFlowLayoutPanel ButtonsFLP;
    internal DarkButton CopyButton;
}

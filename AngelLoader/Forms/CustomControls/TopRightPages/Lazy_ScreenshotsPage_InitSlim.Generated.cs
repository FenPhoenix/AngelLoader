namespace AngelLoader.Forms.CustomControls;

public sealed partial class Lazy_ScreenshotsPage
{
    /// <summary>
    /// Custom generated component initializer with cruft removed.
    /// </summary>
    private void InitSlim()
    {
        this.CopiedMessageLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.GammaLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.NumberLabel = new AngelLoader.Forms.CustomControls.DarkLabel();
        this.GammaTrackBar = new AngelLoader.Forms.CustomControls.DarkTrackBar();
        this.OpenScreenshotsFolderButton = new AngelLoader.Forms.CustomControls.DarkButton();
        this.NextButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.PrevButton = new AngelLoader.Forms.CustomControls.DarkArrowButton();
        this.ScreenshotsPictureBox = new AngelLoader.Forms.CustomControls.ImagePanelCustom();
        this.DeleteButton = new AngelLoader.Forms.CustomControls.DarkButton();
        ((System.ComponentModel.ISupportInitialize)(this.GammaTrackBar)).BeginInit();
        this.SuspendLayout();
        // 
        // CopiedMessageLabel
        // 
        this.CopiedMessageLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
        this.CopiedMessageLabel.AutoSize = true;
        this.CopiedMessageLabel.Location = new System.Drawing.Point(240, 204);
        this.CopiedMessageLabel.Size = new System.Drawing.Size(46, 13);
        this.CopiedMessageLabel.Visible = false;
        // 
        // GammaLabel
        // 
        this.GammaLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.GammaLabel.AutoSize = true;
        this.GammaLabel.Location = new System.Drawing.Point(8, 204);
        this.GammaLabel.Size = new System.Drawing.Size(46, 13);
        // 
        // NumberLabel
        // 
        this.NumberLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.NumberLabel.AutoSize = true;
        this.NumberLabel.Location = new System.Drawing.Point(472, 204);
        this.NumberLabel.Size = new System.Drawing.Size(54, 13);
        // 
        // GammaTrackBar
        // 
        this.GammaTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.GammaTrackBar.AutoSize = false;
        this.GammaTrackBar.Location = new System.Drawing.Point(8, 223);
        this.GammaTrackBar.Maximum = 100;
        this.GammaTrackBar.Size = new System.Drawing.Size(512, 32);
        this.GammaTrackBar.TabIndex = 4;
        this.GammaTrackBar.TickFrequency = 10;
        this.GammaTrackBar.Value = 50;
        // 
        // OpenScreenshotsFolderButton
        // 
        this.OpenScreenshotsFolderButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.OpenScreenshotsFolderButton.Location = new System.Drawing.Point(336, 256);
        this.OpenScreenshotsFolderButton.Size = new System.Drawing.Size(35, 23);
        this.OpenScreenshotsFolderButton.TabIndex = 6;
        this.OpenScreenshotsFolderButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.OpenScreenshotsFolderButton_PaintCustom);
        // 
        // NextButton
        // 
        this.NextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.NextButton.ArrowDirection = AngelLoader.Forms.Direction.Right;
        this.NextButton.Location = new System.Drawing.Point(446, 256);
        this.NextButton.Size = new System.Drawing.Size(75, 23);
        this.NextButton.TabIndex = 8;
        // 
        // PrevButton
        // 
        this.PrevButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.PrevButton.ArrowDirection = AngelLoader.Forms.Direction.Left;
        this.PrevButton.Location = new System.Drawing.Point(371, 256);
        this.PrevButton.Size = new System.Drawing.Size(75, 23);
        this.PrevButton.TabIndex = 7;
        // 
        // ScreenshotsPictureBox
        // 
        this.ScreenshotsPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
        | System.Windows.Forms.AnchorStyles.Left)
        | System.Windows.Forms.AnchorStyles.Right)));
        this.ScreenshotsPictureBox.Location = new System.Drawing.Point(8, 8);
        this.ScreenshotsPictureBox.Size = new System.Drawing.Size(512, 192);
        this.ScreenshotsPictureBox.TabIndex = 0;
        // 
        // DeleteButton
        // 
        this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
        this.DeleteButton.Location = new System.Drawing.Point(8, 256);
        this.DeleteButton.Size = new System.Drawing.Size(23, 23);
        this.DeleteButton.TabIndex = 9;
        this.DeleteButton.PaintCustom += new System.EventHandler<System.Windows.Forms.PaintEventArgs>(this.DeleteButton_PaintCustom);
        // 
        // Lazy_ScreenshotsPage
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
        this.AutoScroll = true;
        this.AutoScrollMinSize = new System.Drawing.Size(200, 100);
        this.Controls.Add(this.DeleteButton);
        this.Controls.Add(this.CopiedMessageLabel);
        this.Controls.Add(this.GammaLabel);
        this.Controls.Add(this.NumberLabel);
        this.Controls.Add(this.GammaTrackBar);
        this.Controls.Add(this.OpenScreenshotsFolderButton);
        this.Controls.Add(this.NextButton);
        this.Controls.Add(this.PrevButton);
        this.Controls.Add(this.ScreenshotsPictureBox);
        this.Size = new System.Drawing.Size(527, 284);
        ((System.ComponentModel.ISupportInitialize)(this.GammaTrackBar)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}
